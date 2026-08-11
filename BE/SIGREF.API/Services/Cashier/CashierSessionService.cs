using Microsoft.EntityFrameworkCore;
using SIGREF.API.Dtos.Cashier;
using SIGREF.API.Extensions;
using SIGREF.API.Middleware;
using SIGREF.Common.Constants;
using SIGREF.Common.Exceptions;
using SIGREF.Common.Types;
using SIGREF.Core.Entity.Cashier;
using SIGREF.Infrastructure.Keycloak.Interfaces;
using SIGREF.Infrastructure.Persistence;
using Hl7.Fhir.Rest;
using SIGREF.Common.Dtos;

namespace SIGREF.API.Services.Cashier;

// TODO: IMPLEMENTAR METODO DE VERIFICACION DE RECIBOS
public class CashierSessionService : ICashierSessionService
{
    private readonly SIGREFContext _db;
    private readonly IUserContextService _userContext;
    private readonly IKeycloakAdminService _keycloakClient;

    public CashierSessionService(SIGREFContext db, IUserContextService userContext, IKeycloakAdminService keycloakClient)
    {
        _db = db;
        _userContext = userContext;
        _keycloakClient = keycloakClient;
    }

    public async Task<CashierSessionMinimalDto> OpenSessionAsync(CreateCashierSessionDto dto)
    {
        try
        {
            var userId = _userContext.GetUserId();
            var activeSession = await _db.CashierSessions
                .Where(x => x.UserId == userId && x.IsOpen)
                .FirstOrDefaultAsync();

            if (activeSession != null)
                throw new ConflictException("CASHIER_SESSION_ALREADY_OPEN", new Dictionary<string, object>
                {
                    { "UserId", userId },
                    { "ActiveSessionId", activeSession.Id }
                });

            var session = new CashierSessionEntity
            {
                UserId = userId,
                ShiftId = dto.ShiftId,
                OpenAt = DateTime.UtcNow,
                IsOpen = true,
                SystemAmount = 0,
                CreatedById = userId,
                CreatedDate = DateTime.UtcNow
            };

            _db.CashierSessions.Add(session);
            await _db.SaveChangesAsync();

            return session.ToMinimalDto();
        }
        catch (DbUpdateException dbEx)
        {
            throw new ExternalServiceException("DB_OPEN_SESSION_ERROR", 502, new Dictionary<string, object>
            {
                { "OriginalException", dbEx.Message },
                { "Operation", nameof(OpenSessionAsync) }
            });
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (AppException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("INTERNAL_CASHIER_ERROR", 500, new Dictionary<string, object>
            {
                { "OriginalException", ex.Message },
                { "Operation", nameof(OpenSessionAsync) }
            });
        }
    }

    public async Task<CashierSessionDto> GetActiveSessionByUserAsync(Guid userId)
    {
        try
        {
            var activeSession = await _db.CashierSessions
                .Where(x => x.UserId == userId && x.IsOpen)
                .FirstOrDefaultAsync();

            if (activeSession == null)
                throw new NotFoundException("CASHIER_NO_ACTIVE_SESSION", new Dictionary<string, object>
                {
                    { "UserId", userId }
                });

            return activeSession.ToDto();
        }
        catch (DbUpdateException dbEx)
        {
            throw new ExternalServiceException("DB_GET_SESSION_ERROR", 502, new Dictionary<string, object>
            {
                { "OriginalException", dbEx.Message },
                { "Operation", nameof(GetActiveSessionByUserAsync) },
                { "UserId", userId }
            });
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (AppException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("INTERNAL_CASHIER_ERROR", 500, new Dictionary<string, object>
            {
                { "OriginalException", ex.Message },
                { "Operation", nameof(GetActiveSessionByUserAsync) },
                { "UserId", userId }
            });
        }
    }

    // TODO : Verificar la sesion mandada con la sesion abierta son la misma
    // TODO: Ver si es necesario mandar la session al cerrar , o si se puede hacer de una sola vez
    public async Task<CashierSessionDto> CloseSessionAsync(Guid sessionId, CloseCashierSessionDto dto)
    {
        try
        {
            var userId = _userContext.GetUserId();
            var session = await _db.CashierSessions
                .FirstOrDefaultAsync(x => x.Id == sessionId);

            if (session == null)
                throw new NotFoundException("CASHIER_SESSION_NOT_FOUND", new Dictionary<string, object>
                {
                    { "SessionId", sessionId }
                });

            if (!session.IsOpen)
                throw new BusinessRuleException("CASHIER_SESSION_ALREADY_CLOSED", new Dictionary<string, object>
                {
                    { "SessionId", sessionId }
                });

            if (dto.DeclaredAmount < 0)
                throw new ValidationException("CASHIER_NEGATIVE_DECLARED_AMOUNT", new Dictionary<string, object>
                {
                    { "DeclaredAmount", dto.DeclaredAmount }
                });

            // Calcular el monto real del sistema
            var systemAmount = await _db.Invoices
                .Where(i => i.CashierSessionId == sessionId && i.Status == InvoiceStatus.Paid)
                .SumAsync(i => (decimal?)i.FinalTotal) ?? 0;

            // Validar monto 0 con facturas existentes
            if (systemAmount == 0)
            {
                var hasInvoices = await _db.Invoices
                    .AnyAsync(i => i.CashierSessionId == sessionId);

                if (hasInvoices)
                    throw new BusinessRuleException("CASHIER_ZERO_AMOUNT_WITH_INVOICES", new Dictionary<string, object>
                    {
                        { "SessionId", sessionId }
                    });
            }

            // Cálculo del cierre
            session.DeclaredAmount = dto.DeclaredAmount;
            session.SystemAmount = systemAmount;
            session.Difference = dto.DeclaredAmount - systemAmount;
            session.IsOpen = false;
            session.ClosedAt = DateTime.UtcNow;
            session.UpdatedById = userId;
            session.UpdatedDate = DateTime.UtcNow;

            bool isCorrect = session.Difference == 0;
            session.RequiresCorrection = !isCorrect;

            await _db.SaveChangesAsync();

            var resultDto = session.ToDto();
            resultDto.IsClosedCorrectly = isCorrect;

            return resultDto;
        }
        catch (DbUpdateException dbEx)
        {
            throw new ExternalServiceException("DB_CLOSE_SESSION_ERROR", 502, new Dictionary<string, object>
            {
                { "OriginalException", dbEx.Message },
                { "Operation", nameof(CloseSessionAsync) },
                { "SessionId", sessionId }
            });
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (AppException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("INTERNAL_CASHIER_ERROR", 500, new Dictionary<string, object>
            {
                { "OriginalException", ex.Message },
                { "Operation", nameof(CloseSessionAsync) },
                { "SessionId", sessionId }
            });
        }
    }

    public async Task<CashierSessionDto> RequestCorrectionAsync(Guid sessionId, RequestCorrectionDto dto)
    {
        try
        {
            var userId = _userContext.GetUserId();
            var session = await _db.CashierSessions
                .FirstOrDefaultAsync(x => x.Id == sessionId);

            if (session == null)
                throw new NotFoundException("CASHIER_SESSION_NOT_FOUND", new Dictionary<string, object>
                {
                    { "SessionId", sessionId }
                });

            if (session.IsOpen)
                throw new BusinessRuleException("CASHIER_SESSION_STILL_OPEN", new Dictionary<string, object>
                {
                    { "SessionId", sessionId }
                });

            if (session.Difference == 0 || session.Difference == null)
                throw new BusinessRuleException("CASHIER_NO_CORRECTION_NEEDED", new Dictionary<string, object>
                {
                    { "SessionId", sessionId },
                    { "Difference", session.Difference }
                });

            session.Notes = dto.Notes;
            session.UpdatedById = userId;
            session.UpdatedDate = DateTime.UtcNow;
            session.RequiresCorrection = true;

            await _db.SaveChangesAsync();

            return session.ToDto();
        }
        catch (DbUpdateException dbEx)
        {
            throw new ExternalServiceException("DB_CORRECTION_REQUEST_ERROR", 502, new Dictionary<string, object>
            {
                { "OriginalException", dbEx.Message },
                { "Operation", nameof(RequestCorrectionAsync) },
                { "SessionId", sessionId }
            });
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (AppException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("INTERNAL_CASHIER_ERROR", 500, new Dictionary<string, object>
            {
                { "OriginalException", ex.Message },
                { "Operation", nameof(RequestCorrectionAsync) },
                { "SessionId", sessionId }
            });
        }
    }

    public async Task<CashierSessionDto> ResolveCorrectionAsync(Guid sessionId, ResolveCorrectionDto dto)
    {
        try
        {
            var userId = _userContext.GetUserId();
            var session = await _db.CashierSessions
                .FirstOrDefaultAsync(x => x.Id == sessionId);

            if (session == null)
                throw new NotFoundException("CASHIER_SESSION_NOT_FOUND", new Dictionary<string, object>
                {
                    { "SessionId", sessionId }
                });

            if (session.IsOpen)
                throw new BusinessRuleException("CASHIER_SESSION_STILL_OPEN", new Dictionary<string, object>
                {
                    { "SessionId", sessionId }
                });

            if (!string.IsNullOrWhiteSpace(dto.AdminNotes))
                session.Notes = dto.AdminNotes;

            var systemAmount = session.SystemAmount ?? 0;
            var declared = session.DeclaredAmount ?? 0;

            session.Difference = declared - systemAmount;
            bool isCorrect = session.Difference == 0;
            session.RequiresCorrection = !isCorrect;
            session.UpdatedById = userId;
            session.CorrectionDate = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return session.ToDto();
        }
        catch (DbUpdateException dbEx)
        {
            throw new ExternalServiceException("DB_RESOLVE_CORRECTION_ERROR", 502, new Dictionary<string, object>
            {
                { "OriginalException", dbEx.Message },
                { "Operation", nameof(ResolveCorrectionAsync) },
                { "SessionId", sessionId }
            });
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (AppException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("INTERNAL_CASHIER_ERROR", 500, new Dictionary<string, object>
            {
                { "OriginalException", ex.Message },
                { "Operation", nameof(ResolveCorrectionAsync) },
                { "SessionId", sessionId }
            });
        }
    }

    public async Task<PagedResultDto<CashierSessionDto>> GetFilteredSessionsAsync(CashierSessionFilterDto filter)
    {
        try
        {
            // Paginación
            int pageNumber = filter.PageNumber <= 0 ? 1 : filter.PageNumber;
            int pageSize = filter.PageSize <= 0 ? 10 : Math.Clamp(filter.PageSize, 1, 50);
            pageNumber = Math.Clamp(pageNumber, 1, int.MaxValue);

            // Obtener usuario y roles
            var userId = _userContext.GetUserId();
            var roles = _userContext.GetUserRoles();
            bool isAdmin = roles.Contains(RolesConstants.admin);
            bool isAuditor = roles.Contains(RolesConstants.auditor);
            bool canViewAll = isAdmin || isAuditor;

            // Query base
            var query = _db.CashierSessions.AsNoTracking().AsQueryable();

            // Rol: solo Admin/Auditor pueden ver todo
            if (!canViewAll)
                query = query.Where(x => x.UserId == userId);

            // Filtro: IsOpen
            // Filtro: IsOpen
            if (filter.IsOpen == true)
                query = query.Where(x => x.IsOpen);
            else if (filter.IsOpen == false)
                query = query.Where(x => !x.IsOpen);

            // Filtro: IsClosedCorrectly (Difference == 0)
            if (filter.IsClosedCorrectly == true)
                query = query.Where(x => x.Difference == 0);
            else if (filter.IsClosedCorrectly == false)
                query = query.Where(x => x.Difference != 0);

            // Filtro: fechas UTC (DateTime)
            // Filtro: fechas UTC (DateTime)
            if (filter.FromDate is { } fromDate)
                query = query.Where(x => x.OpenAt >= fromDate);

            if (filter.ToDate is { } toDate)
                query = query.Where(x => x.OpenAt <= toDate);
            

            // Filtro: turno
            if (filter.ShiftId is { } shiftId)
                query = query.Where(x => x.ShiftId == shiftId);
            // Contar total
            int totalItems = await query.CountAsync();
            int totalPages = totalItems > 0
                ? (int)Math.Ceiling(totalItems / (double)pageSize)
                : 0;

            // Paginación
            int skip = (pageNumber - 1) * pageSize;

            // Mapeo directo en la BD
            var sessionDtos = await query
                .OrderByDescending(x => x.OpenAt)
                .Skip(skip)
                .Take(pageSize)
                .Select(x => new CashierSessionDto
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    ShiftId = x.ShiftId,
                    OpenAt = x.OpenAt,
                    ClosedAt = x.ClosedAt,
                    DeclaredAmount = x.DeclaredAmount,
                    SystemAmount = x.SystemAmount,
                    Difference = x.Difference,
                    IsOpen = x.IsOpen,
                    IsClosedCorrectly = (x.Difference == 0 || x.Difference == null),
                    Notes = x.Notes,
                    CorrectionClosure = x.CorrectionDate
                })
                .ToListAsync();

            // ============= Nombres de usuario desde Keycloak ===============
            var uniqueUserIds = sessionDtos
                .Select(s => s.UserId)
                .Distinct()
                .ToList();

            var userNameTasks = uniqueUserIds.ToDictionary(
                id => id,
                id => _keycloakClient.GetUserByIdAsync(id.ToString(), CancellationToken.None)
            );

            await Task.WhenAll(userNameTasks.Values);

            var userNameMap = userNameTasks.ToDictionary(
                kvp => kvp.Key,
                kvp =>
                {
                    var kcUser = kvp.Value.Result;
                    if (kcUser is null) return null;

                    if (!string.IsNullOrWhiteSpace(kcUser.DisplayName))
                        return kcUser.DisplayName;

                    return kcUser.Username;
                }
            );

            foreach (var session in sessionDtos)
                session.UserName = userNameMap.GetValueOrDefault(session.UserId);

            return new PagedResultDto<CashierSessionDto>
            {
                Items = sessionDtos,
                Pagination = new PaginationDto
                {
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages,
                    HasPrevious = pageNumber > 1,
                    HasNext = pageNumber < totalPages
                }
            };
        }
        catch (DbUpdateException dbEx)
        {
            throw new ExternalServiceException("DB_FILTER_SESSIONS_ERROR", 502, new Dictionary<string, object>
            {
                { "OriginalException", dbEx.Message },
                { "Operation", nameof(GetFilteredSessionsAsync) }
            });
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (AppException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("INTERNAL_CASHIER_ERROR", 500, new Dictionary<string, object>
            {
                { "OriginalException", ex.Message },
                { "Operation", nameof(GetFilteredSessionsAsync) }
            });
        }
    }

    public async Task<CashierSessionDto> GetByIdAsync(Guid sessionId)
    {
        try
        {
            var userId = _userContext.GetUserId();
            var roles = _userContext.GetUserRoles();

            bool isAdmin = roles.Contains(RolesConstants.admin);
            bool isAuditor = roles.Contains(RolesConstants.auditor);
            bool canViewAll = isAdmin || isAuditor;

            var sessionDto = await _db.CashierSessions
                .Where(x => x.Id == sessionId)
                .Select(x => new CashierSessionDto
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    ShiftId = x.ShiftId,
                    OpenAt = x.OpenAt,
                    ClosedAt = x.ClosedAt,
                    DeclaredAmount = x.DeclaredAmount,
                    SystemAmount = x.SystemAmount,
                    Difference = x.Difference,
                    IsOpen = x.IsOpen,
                    IsClosedCorrectly = (x.Difference == 0 || x.Difference == null),
                    Notes = x.Notes,
                    CorrectionClosure = x.CorrectionDate
                })
                .FirstOrDefaultAsync();

            if (sessionDto == null)
                throw new NotFoundException("CASHIER_SESSION_NOT_FOUND", new Dictionary<string, object>
                {
                    { "SessionId", sessionId }
                });

            if (!canViewAll && sessionDto.UserId != userId)
                throw new ForbiddenException("CASHIER_SESSION_ACCESS_DENIED", new Dictionary<string, object>
                {
                    { "SessionId", sessionId },
                    { "RequestedBy", userId },
                    { "SessionOwner", sessionDto.UserId }
                });

            return sessionDto;
        }
        catch (DbUpdateException dbEx)
        {
            throw new ExternalServiceException("DB_GET_SESSION_ERROR", 502, new Dictionary<string, object>
            {
                { "OriginalException", dbEx.Message },
                { "Operation", nameof(GetByIdAsync) },
                { "SessionId", sessionId }
            });
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (AppException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException("INTERNAL_CASHIER_ERROR", 500, new Dictionary<string, object>
            {
                { "OriginalException", ex.Message },
                { "Operation", nameof(GetByIdAsync) },
                { "SessionId", sessionId }
            });
        }
    }
}
