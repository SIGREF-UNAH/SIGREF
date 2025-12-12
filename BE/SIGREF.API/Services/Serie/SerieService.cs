using SIGREF.API.Database;
using SIGREF.API.Database.Entity.Billing;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Dtos.Series;
using SIGREF.API.Services.Auth;
using Microsoft.EntityFrameworkCore;
using SIGREF.API.Constants;

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

    public async Task<ResponseDto<SerieDto>> CreateSerieAsync(CreateSeriesDto dto)
    {
        var user = _userContextService.GetUserId();
        // Validación StartNumber <= EndNumber
        if (dto.StartNumber > dto.EndNumber)
        {
            return new ResponseDto<SerieDto>
            {
                Status = false,
                StatusCode = 400,
                Message = "El número de inicio no puede ser mayor al número final.",
            };
        }

        //  No permitir nombres duplicados
        var nameExists = await _context.InvoiceSeries
            .AnyAsync(x => x.Name.ToLower() == dto.Name.ToLower());

        if (nameExists)
        {
            return new ResponseDto<SerieDto>
            {
                Status = false,
                StatusCode = 400,
                Message = "Ya existe una serie con ese nombre.",
            };
        }

        //======================  REVISAR  ===========================
        //  No permitir prefijos duplicados ?? Segun entiendo si se puede pero po si en un futuro resulta que no
        //var prefixExists = await _context.InvoiceSeries
        //    .AnyAsync(x => x.Prefix.ToLower() == dto.Prefix.ToLower());

        //if (prefixExists)
        //{
        //    response.Status = false;
        //    response.StatusCode = 400;
        //    response.Message = "Ya existe una serie con ese prefijo.";
        //    return response;
        //}

        // Crear entidad
        var entity = new InvoiceSerieEntity
        {
            Name = dto.Name,
            Prefix = dto.Prefix,
            StartNumber = dto.StartNumber,
            EndNumber = dto.EndNumber,
            CurrentNumber = dto.StartNumber, // arranca en el inicio del rango
            CreatedById = user,
            CreatedDate = DateTime.UtcNow,
        };

        _context.InvoiceSeries.Add(entity);
        await _context.SaveChangesAsync();

        return new ResponseDto<SerieDto>
        {
            Status = true,
            StatusCode = 201,
            Message = "Serie creada correctamente.",
            Data = new SerieDto
            {
                Id  = entity.Id,
                Name = entity.Name,
                Prefix = entity.Prefix,
                StartNumber = entity.StartNumber,
                EndNumber = entity.EndNumber,
                CurrentNumber = entity.CurrentNumber,
                CreatedDate = entity.CreatedDate,
            }
        };
    }

    public async Task<ResponseDto<SerieDto>> UpdateSerieAsync(UpdateSeriesDto dto, Guid id)
    {
        var user = _userContextService.GetUserId();

        // Bloqueo al registro para actualizacion
        await using var tx = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

        var entity = await _context.InvoiceSeries
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync();

        if (entity == null)
        {
            return new ResponseDto<SerieDto>
            {
                Status = false,
                StatusCode = 404,
                Message = "La serie no existe."
            };
        }

        // ===== VALIDACIONES =====
        if (dto.StartNumber > dto.EndNumber)
        {
            return new ResponseDto<SerieDto>
            {
                Status = false,
                StatusCode = 400,
                Message = "El número de inicio no puede ser mayor al número final."
            };
        }

        // Validación de nombre duplicado
        var exists = await _context.InvoiceSeries
            .AnyAsync(x => x.Name.ToLower() == dto.Name.ToLower() && x.Id != id);

        if (exists)
        {
            return new ResponseDto<SerieDto>
            {
                Status = false,
                StatusCode = 400,
                Message = "Ya existe una serie con ese nombre."
            };
        }

        // ===== CAMBIOS =====
        if (dto.StartNumber != entity.StartNumber)
        {
            if (entity.CurrentNumber != entity.StartNumber)
            {
                return new ResponseDto<SerieDto>
                {
                    Status = false,
                    StatusCode = 400,
                    Message = "No se puede cambiar el inicio porque la serie ya fue usada."
                };
            }

            entity.StartNumber = dto.StartNumber;
            entity.CurrentNumber = dto.StartNumber;
        }

        if (dto.EndNumber < entity.CurrentNumber)
        {
            return new ResponseDto<SerieDto>
            {
                Status = false,
                StatusCode = 400,
                Message = "El número final no puede ser menor al número actual."
            };
        }

        entity.EndNumber = dto.EndNumber;
        entity.Name = dto.Name;
        entity.Prefix = dto.Prefix;

        if (dto.IsActive.HasValue)
            entity.IsActive = dto.IsActive.Value;

        entity.UpdatedById = user;
        entity.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await tx.CommitAsync();

        return new ResponseDto<SerieDto>
        {
            Status = true,
            StatusCode = 200,
            Message = "Serie actualizada correctamente.",
            Data = new SerieDto
            {
                Name = entity.Name,
                Prefix = entity.Prefix,
                StartNumber = entity.StartNumber,
                EndNumber = entity.EndNumber,
                CurrentNumber = entity.CurrentNumber,
                CreatedDate = entity.CreatedDate,
                ModifiedDate = entity.UpdatedDate,
            }
        };
    }


    public async Task<ResponseDto<PagedResultDto<SerieDto>>> GetSeriesAsync(FilterSerieDto dto)
    {
        var userRole = _userContextService.GetUserRoles();

        int page = dto.PageNumber <= 0 ? 1 : dto.PageNumber;
        int size = dto.PageSize <= 0 ? 10 : dto.PageSize;
        if (size > 50) size = 50;

        var query = _context.InvoiceSeries.AsQueryable().AsNoTracking();

        // ==========================
        // FILTROS

        // FILTROS STRING
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

        // Si el usuario es cajero  solo ver series activas
        if (userRole.Contains(RolesConstants.cashier))
            query = query.Where(x => x.IsActive);

        // ==========================
        //        PAGINACION
        // ==========================

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
                ModifiedDate = x.UpdatedDate
            })
            .ToListAsync();

        return new ResponseDto<PagedResultDto<SerieDto>>
        {
            Status = true,
            StatusCode = 200,
            Message = "Listado de series obtenido correctamente.",
            Data = new PagedResultDto<SerieDto>
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
            }
        };
    }


    public async Task<ResponseDto<SerieDto>> GetSerieById(Guid id)
    {
        var dto = await _context.InvoiceSeries
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new SerieDto
            {
                Name = x.Name,
                Prefix = x.Prefix,
                StartNumber = x.StartNumber,
                EndNumber = x.EndNumber,
                CurrentNumber = x.CurrentNumber,
                CreatedDate = x.CreatedDate,
                ModifiedDate = x.UpdatedDate
            })
            .FirstOrDefaultAsync();

        if (dto == null)
        {
            return new ResponseDto<SerieDto>
            {
                Status = false,
                StatusCode = 404,
                Message = "La serie no existe."
            };
        }

        return new ResponseDto<SerieDto>
        {
            Status = true,
            StatusCode = 200,
            Message = "Serie encontrada.",
            Data = dto
        };
    }


    public async Task<ResponseDto<SerieDto>> SoftDeleteSerieAsync(Guid id)
    {
        var userId = _userContextService.GetUserId();

        var entity = await _context.InvoiceSeries
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
        {
            return new ResponseDto<SerieDto>
            {
                Status = false,
                StatusCode = 404,
                Message = "La serie no existe."
            };
        }

        if (!entity.IsActive)
        {
            return new ResponseDto<SerieDto>
            {
                Status = true,
                StatusCode = 200,
                Message = "La serie ya estaba desactivada.",
                Data = new SerieDto
                {
                    Name = entity.Name,
                    Prefix = entity.Prefix,
                    StartNumber = entity.StartNumber,
                    EndNumber = entity.EndNumber,
                    CurrentNumber = entity.CurrentNumber,
                    CreatedDate = entity.CreatedDate,
                    ModifiedDate = entity.UpdatedDate
                }
            };
        }

        // Desactivar
        entity.IsActive = false;
        entity.UpdatedById = userId;
        entity.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new ResponseDto<SerieDto>
        {
            Status = true,
            StatusCode = 200,
            Message = "Serie desactivada correctamente.",
            Data = new SerieDto
            {
                Name = entity.Name,
                Prefix = entity.Prefix,
                StartNumber = entity.StartNumber,
                EndNumber = entity.EndNumber,
                CurrentNumber = entity.CurrentNumber,
                CreatedDate = entity.CreatedDate,
                ModifiedDate = entity.UpdatedDate
            }
        };
    }
}