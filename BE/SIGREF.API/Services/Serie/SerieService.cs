using Microsoft.EntityFrameworkCore;
using SIGREF.API.Database;
using SIGREF.API.Dtos.Series;
using SIGREF.Common.Constants;
using SIGREF.Common.Dtos;
using SIGREF.Common.Exceptions;
using SIGREF.Core.Entity.Billing;
using SIGREF.Infrastructure.Keycloak.Interfaces;
using SIGREF.Infrastructure.Persistence;

namespace SIGREF.API.Services.Serie;

public class SerieService : ISerieService
{
    private readonly SIGREFContext _context;
    private readonly IUserContextService _userContextService;

    public SerieService(SIGREFContext context, IUserContextService userContextService)
    {
        _context = context;
        _userContextService = userContextService;
    }

    public async Task<SerieDto> CreateSerieAsync(CreateSeriesDto dto)
    {
        var user = _userContextService.GetUserId();

        // Validación StartNumber <= EndNumber
        if (dto.StartNumber > dto.EndNumber)
        {
            throw new ValidationException("START_NUMBER_GREATER_THAN_END_NUMBER", new Dictionary<string, object>
            {
                { "startNumber", dto.StartNumber },
                { "endNumber", dto.EndNumber }
            });
        }

        // No permitir nombres duplicados
        var nameExists = await _context.InvoiceSeries
            .AnyAsync(x => x.Name.ToLower() == dto.Name.ToLower());

        if (nameExists)
        {
            throw new ConflictException("SERIE_NAME_ALREADY_EXISTS", new Dictionary<string, object>
            {
                { "name", dto.Name }
            });
        }

        // Crear entidad
        var entity = new InvoiceSerieEntity
        {
            Name = dto.Name,
            Prefix = dto.Prefix,
            StartNumber = dto.StartNumber,
            EndNumber = dto.EndNumber,
            CurrentNumber = dto.StartNumber,
            CreatedById = user,
            CreatedDate = DateTime.UtcNow,
            IsActive = dto.IsActive ?? true
        };

        _context.InvoiceSeries.Add(entity);
        await _context.SaveChangesAsync();

        return new SerieDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Prefix = entity.Prefix,
            StartNumber = entity.StartNumber,
            EndNumber = entity.EndNumber,
            CurrentNumber = entity.CurrentNumber,
            CreatedDate = entity.CreatedDate,
            IsActive = entity.IsActive,
        };
    }

    public async Task<SerieDto> UpdateSerieAsync(UpdateSeriesDto dto, Guid id)
    {
        var user = _userContextService.GetUserId();

        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _context.Database
                .BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

            var entity = await _context.InvoiceSeries
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                throw new NotFoundException("SERIE_NOT_FOUND", new Dictionary<string, object>
                {
                    { "serieId", id }
                });
            }

            // Validación parcial: StartNumber <= EndNumber si ambos vienen
            if (dto.StartNumber.HasValue && dto.EndNumber.HasValue &&
                dto.StartNumber.Value > dto.EndNumber.Value)
            {
                throw new ValidationException("START_NUMBER_GREATER_THAN_END_NUMBER", new Dictionary<string, object>
                {
                    { "startNumber", dto.StartNumber.Value },
                    { "endNumber", dto.EndNumber.Value }
                });
            }

            // Validar nombre duplicado
            if (!string.IsNullOrWhiteSpace(dto.Name))
            {
                var nameNormalized = dto.Name.Trim().ToLower();

                var exists = await _context.InvoiceSeries
                    .AnyAsync(x => x.Id != id && x.Name.ToLower() == nameNormalized);

                if (exists)
                {
                    throw new ConflictException("SERIE_NAME_ALREADY_EXISTS", new Dictionary<string, object>
                    {
                        { "name", dto.Name }
                    });
                }
            }

            // Determinar valores efectivos
            long newStart = dto.StartNumber ?? entity.StartNumber;
            long newEnd = dto.EndNumber ?? entity.EndNumber;
            bool startChanging = dto.StartNumber.HasValue && dto.StartNumber.Value != entity.StartNumber;
            bool endChanging = dto.EndNumber.HasValue && dto.EndNumber.Value != entity.EndNumber;

            // Regla: StartNumber no puede ser mayor que EndNumber (Rango Coherente)
            if (newStart > newEnd)
            {
                throw new ValidationException("START_NUMBER_GREATER_THAN_END_NUMBER", new Dictionary<string, object>
                {
                    { "startNumber", newStart },
                    { "endNumber", newEnd }
                });
            }

            // Regla: No permitir modificar el inicio si la serie ya tuvo movimiento
            if (startChanging && entity.CurrentNumber != entity.StartNumber)
            {
                throw new BusinessRuleException("CANNOT_CHANGE_START_WHEN_SERIE_IN_USE", new Dictionary<string, object>
                {
                    { "serieId", id },
                    { "currentNumber", entity.CurrentNumber },
                    { "startNumber", entity.StartNumber }
                });
            }

            // Regla: El nuevo número final no puede ser menor al progreso actual de la serie
            long effectiveCurrent = startChanging ? newStart : entity.CurrentNumber;

            if (newEnd < effectiveCurrent)
            {
                throw new BusinessRuleException("END_NUMBER_LESS_THAN_CURRENT", new Dictionary<string, object>
                {
                    { "serieId", id },
                    { "newEnd", newEnd },
                    { "effectiveCurrent", effectiveCurrent }
                });
            }

            // Aplicar cambios a la entidad
            if (startChanging)
            {
                entity.StartNumber = newStart;
                entity.CurrentNumber = newStart;
            }

            if (endChanging)
            {
                entity.EndNumber = newEnd;
            }

            if (!string.IsNullOrWhiteSpace(dto.Name))
                entity.Name = dto.Name.Trim();

            if (!string.IsNullOrWhiteSpace(dto.Prefix))
                entity.Prefix = dto.Prefix.Trim();

            if (dto.IsActive.HasValue)
                entity.IsActive = dto.IsActive.Value;

            entity.UpdatedById = user;
            entity.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return new SerieDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Prefix = entity.Prefix,
                StartNumber = entity.StartNumber,
                EndNumber = entity.EndNumber,
                CurrentNumber = entity.CurrentNumber,
                CreatedDate = entity.CreatedDate,
                ModifiedDate = entity.UpdatedDate,
                IsActive = entity.IsActive
            };
        });
    }

    public async Task<PagedResultDto<SerieDto>> GetSeriesAsync(FilterSerieDto dto)
    {
        var userRole = _userContextService.GetUserRoles();

        int page = dto.PageNumber <= 0 ? 1 : dto.PageNumber;
        int size = dto.PageSize <= 0 ? 10 : dto.PageSize;
        if (size > 50) size = 50;

        var query = _context.InvoiceSeries.AsQueryable().AsNoTracking();

        // Filtros
        if (!string.IsNullOrWhiteSpace(dto.Name))
            query = query.Where(x => EF.Functions.ILike(x.Name, $"%{dto.Name}%"));

        if (!string.IsNullOrWhiteSpace(dto.Prefix))
            query = query.Where(x => EF.Functions.ILike(x.Prefix, $"%{dto.Prefix}%"));

        if (dto.StartNumber.HasValue)
            query = query.Where(x => x.StartNumber == dto.StartNumber.Value);

        if (dto.EndNumber.HasValue)
            query = query.Where(x => x.EndNumber == dto.EndNumber.Value);

        if (dto.IsActive.HasValue)
            query = query.Where(x => x.IsActive == dto.IsActive.Value);

        // Si el usuario es cajero solo ver series activas
        if (userRole.Contains(RolesConstants.cashier))
            query = query.Where(x => x.IsActive);

        // Paginación
        int totalItems = await query.CountAsync();
        int totalPages = (int)Math.Ceiling(totalItems / (double)size);

        var dtoList = await query
            .OrderByDescending(x => x.CreatedDate)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(x => new SerieDto
            {
                Id = x.Id,
                Name = x.Name,
                Prefix = x.Prefix,
                StartNumber = x.StartNumber,
                EndNumber = x.EndNumber,
                CurrentNumber = x.CurrentNumber,
                CreatedDate = x.CreatedDate,
                ModifiedDate = x.UpdatedDate,
                IsActive = x.IsActive
            })
            .ToListAsync();

        return new PagedResultDto<SerieDto>
        {
            Items = dtoList,
            Pagination = new PaginationDto
            {
                CurrentPage = page,
                PageSize = size,
                TotalItems = totalItems,
                TotalPages = totalPages,
                HasPrevious = page > 1,
                HasNext = page < totalPages
            }
        };
    }

    public async Task<SerieDto> GetSerieById(Guid id)
    {
        var dto = await _context.InvoiceSeries
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new SerieDto
            {
                Id = x.Id,
                Name = x.Name,
                Prefix = x.Prefix,
                StartNumber = x.StartNumber,
                EndNumber = x.EndNumber,
                CurrentNumber = x.CurrentNumber,
                CreatedDate = x.CreatedDate,
                ModifiedDate = x.UpdatedDate,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();

        if (dto == null)
        {
            throw new NotFoundException("SERIE_NOT_FOUND", new Dictionary<string, object>
            {
                { "serieId", id }
            });
        }

        return dto;
    }

    public async Task<SerieDto> SoftDeleteSerieAsync(Guid id)
    {
        var userId = _userContextService.GetUserId();

        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _context.Database.BeginTransactionAsync();

            var entity = await _context.InvoiceSeries
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                throw new NotFoundException("SERIE_NOT_FOUND", new Dictionary<string, object>
                {
                    { "serieId", id }
                });
            }

            if (!entity.IsActive)
            {
                // Idempotente: ya estaba desactivada, devolvemos sin error
                return new SerieDto
                {
                    Id = entity.Id,
                    Name = entity.Name,
                    Prefix = entity.Prefix,
                    StartNumber = entity.StartNumber,
                    EndNumber = entity.EndNumber,
                    CurrentNumber = entity.CurrentNumber,
                    CreatedDate = entity.CreatedDate,
                    ModifiedDate = entity.UpdatedDate,
                    IsActive = entity.IsActive
                };
            }

            entity.IsActive = false;
            entity.UpdatedById = userId;
            entity.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return new SerieDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Prefix = entity.Prefix,
                StartNumber = entity.StartNumber,
                EndNumber = entity.EndNumber,
                CurrentNumber = entity.CurrentNumber,
                CreatedDate = entity.CreatedDate,
                ModifiedDate = entity.UpdatedDate,
                IsActive = entity.IsActive
            };
        });
    }
}