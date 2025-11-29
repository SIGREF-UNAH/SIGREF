using Microsoft.EntityFrameworkCore;
using SIGREF.API.Constants;
using SIGREF.API.Database;
using SIGREF.API.Database.Entity.Cashier;
using SIGREF.API.Dtos.Cashier;
using SIGREF.API.Dtos.Common;
using SIGREF.API.Extensions;
using SIGREF.API.Services.Auth;

namespace SIGREF.API.Services.Cashier;


// TODO : IMPLEMENTAR METODO DE VERIFICACION DE RECIBOS

public class CashierSessionService : ICashierSessionService
{
    private readonly SIGREFContext _db;
    private readonly UserContextService _userContext;

    public CashierSessionService(SIGREFContext db, UserContextService userContext)
    {
        _db = db;
        _userContext = userContext;
    }

    public async Task<ResponseDto<CashierSessionMinimalDto>> OpenSessionAsync(CreateCashierSessionDto dto)
    {
        var userId = _userContext.GetUserId();
        var activeSession = await _db.CashierSessions
            .Where(x => x.UserId == userId && x.IsOpen == true)
            .FirstOrDefaultAsync();

        if (activeSession != null)
        {
            return new ResponseDto<CashierSessionMinimalDto>
            {
                Status = false,
                Message = "Session already opened.",
                StatusCode = 400,
                Data = activeSession.ToMinimalDto()
            };
        }

        var session = new CashierSessionEntity
        {
            UserId = userId,
            ShiftId = dto.ShiftId,
            OpenAt = DateTime.UtcNow,
            IsOpen = true,
            SystemAmount = 0,
            CreatedById = userId
        };

        _db.CashierSessions.Add(session);
        await _db.SaveChangesAsync();

        return new ResponseDto<CashierSessionMinimalDto>
        {
            Status = true,
            Message = "Session opened successfully.",
            StatusCode = 201,
            Data = session.ToMinimalDto()
        };
    }

    public async Task<ResponseDto<CashierSessionDto>> GetActiveSessionByUserAsync(Guid userId)
    {
        var activeSession = await _db.CashierSessions
            .Where(x => x.UserId == userId && x.IsOpen == true)
            .FirstOrDefaultAsync();
        if (activeSession == null)
        {
            return new ResponseDto<CashierSessionDto>
            {
                Status = false,
                Message = "No active cashier session found.",
                StatusCode = 404,
                Data = null
            };
        }

        return new ResponseDto<CashierSessionDto>
        {
            Status = true,
            Message = "Active cashier session found.",
            StatusCode = 200,
            Data = activeSession.ToDto()
        };
    }

    public async Task<ResponseDto<CashierSessionDto>> CloseSessionAsync(Guid sessionId, CloseCashierSessionDto dto)
    {
        var userId = _userContext.GetUserId();
        var session = await _db.CashierSessions
            .FirstOrDefaultAsync(x => x.Id == sessionId);

        if (session == null)
        {
            return new ResponseDto<CashierSessionDto>
            {
                Status = false,
                Message = "Cashier session not found.",
                StatusCode = 404,
                Data = null
            };
        }

        //Validar que esté abierta
        if (!session.IsOpen)
        {
            return new ResponseDto<CashierSessionDto>
            {
                Status = false,
                Message = "The session is already closed.",
                StatusCode = 400,
                Data = session.ToDto()
            };
        }

        // TODO : VERIFICAR ESTO CON OTRO REQUEST PUEDE SER SI ESTA INDEXADO LA SUMA DE INVOICES 
        // cálculo del cierre
        session.DeclaredAmount = dto.DeclaredAmount;

        // SystemAmount ya existe (lo gestionan los recibos)
        var systemAmount = session.SystemAmount ?? 0;

        session.Difference = dto.DeclaredAmount - systemAmount;

        // Marcar estado de la sesión
        session.IsOpen = false;
        session.ClosedAt = DateTime.UtcNow;
        session.UpdatedById = userId;
        session.UpdatedDate = DateTime.UtcNow;

        // Calcular si quedó correcta o incorrecta
        bool isCorrect = session.Difference == 0;
        // Marcar si requiere corrección
        session.RequiresCorrection = session.Difference != 0;

        // Guardar cambios
        await _db.SaveChangesAsync();

        // Retornar DTO COMPLETO
        var resultDto = session.ToDto();
        resultDto.IsClosedCorrectly = isCorrect;

        return new ResponseDto<CashierSessionDto>
        {
            Status = true,
            Message = "Cashier session closed successfully.",
            StatusCode = 200,
            Data = resultDto
        };
    }


    public async Task<ResponseDto<CashierSessionDto>> RequestCorrectionAsync(Guid sessionId, RequestCorrectionDto dto)
    {
        var userId = _userContext.GetUserId();
        // Obtener la sesion de caja
        var session = await _db.CashierSessions
            .FirstOrDefaultAsync(x => x.Id == sessionId);

        if (session == null)
        {
            return new ResponseDto<CashierSessionDto>
            {
                Status = false,
                Message = "Cashier session not found.",
                StatusCode = 404,
                Data = null
            };
        }

        //Validar que este cerrada
        if (session.IsOpen)
        {
            return new ResponseDto<CashierSessionDto>
            {
                Status = false,
                Message = "Cannot request correction while the session is still open.",
                StatusCode = 400,
                Data = session.ToDto()
            };
        }

        // Validar que exista una diferencia
        if (session.Difference == 0 || session.Difference == null)
        {
            return new ResponseDto<CashierSessionDto>
            {
                Status = false,
                Message = "The session does not require correction.",
                StatusCode = 400,
                Data = session.ToDto()
            };
        }

        // Actualizar notas y marcar que requiere correccion
        session.Notes = dto.Notes;
        session.UpdatedById = userId;
        session.UpdatedDate = DateTime.UtcNow;
        session.RequiresCorrection = true; // cuando se cierra, se cambia a true si no coincide pero aseguro de nuevo

        // Guardar cambios
        await _db.SaveChangesAsync();

        // Retornar DTO completo
        return new ResponseDto<CashierSessionDto>
        {
            Status = true,
            Message = "Correction request updated successfully.",
            StatusCode = 200,
            Data = session.ToDto()
        };
    }


    public async Task<ResponseDto<CashierSessionDto>> ResolveCorrectionAsync(Guid sessionId, ResolveCorrectionDto dto)
    {
        var userId = _userContext.GetUserId();
        var session = await _db.CashierSessions
            .FirstOrDefaultAsync(x => x.Id == sessionId);

        if (session == null)
        {
            return new ResponseDto<CashierSessionDto>
            {
                Status = false,
                Message = "Cashier session not found.",
                StatusCode = 404,
                Data = null
            };
        }

        // Validar que este cerrada
        if (session.IsOpen)
        {
            return new ResponseDto<CashierSessionDto>
            {
                Status = false,
                Message = "Session must be closed before resolving correction.",
                StatusCode = 400,
                Data = session.ToDto()
            };
        }

        // Validar que realmente requería corrección
        //if (session.Difference == 0 || session.Difference == null || session.RequiresCorrection == false)
        //{
        //    return new ResponseDto<CashierSessionDto>
        //    {
        //        Status = false,
        //        Message = "This session does not require correction.",
        //        StatusCode = 400,
        //        Data = session.ToDto()
        //    };
        //}

        // Actualizar notas (si vienen)
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

        return new ResponseDto<CashierSessionDto>
        {
            Status = true,
            Message = "Correction resolved successfully.",
            StatusCode = 200,
            Data = session.ToDto()
        };
    }

    public async Task<ResponseDto<PagedResultDto<CashierSessionDto>>> GetFilteredSessionsAsync(
        CashierSessionFilterDto filter)
    {
        // Paginacion
        int pageNumber = filter.PageNumber <= 0 ? 1 : filter.PageNumber;
        int pageSize = filter.PageSize <= 0 ? 10 : Math.Clamp(filter.PageSize, 1, 50);

        pageNumber = Math.Clamp(pageNumber, 1, int.MaxValue);

        // Obtener usuario y roles
        var userId = _userContext.GetUserId();
        var roles = _userContext.GetUserRoles();

        bool isAdmin = roles.Contains(RolesConstants.admin);
        bool isAuditor = roles.Contains(RolesConstants.auditor);

        // Query base
        var query = _db.CashierSessions.AsQueryable().AsNoTracking();

        // Rol: solo Admin/Auditor pueden ver todo
        // si es otro tipo de usuario, solo pueden ver sus cierres 
        if (!isAdmin && !isAuditor)
        {
            query = query.Where(x => x.UserId == userId);
        }

        // Filtro: IsOpen = turno sige corriendo o en pie
        if (filter.IsOpen.HasValue)
            query = query.Where(x => x.IsOpen == filter.IsOpen.Value);

        // Filtro: IsClosedCorrectly (Difference == 0)
        if (filter.IsClosedCorrectly.HasValue)
        {
            if (filter.IsClosedCorrectly.Value)
                query = query.Where(x => x.Difference == 0);
            else
                query = query.Where(x => x.Difference != 0);
        }

        // Filtro: fechas
        if (filter.FromDate.HasValue)
            query = query.Where(x => x.OpenAt >= filter.FromDate.Value);

        if (filter.ToDate.HasValue)
            query = query.Where(x => x.OpenAt <= filter.ToDate.Value);

        // Filtro: turno
        if (filter.ShiftId.HasValue)
            query = query.Where(x => x.ShiftId == filter.ShiftId.Value);

        // Contar total
        int totalItems = await query.CountAsync();

        // Paginacion
        int skip = (pageNumber - 1) * pageSize;

        // Mapeo directo en la BD para no cargar en Memoeeria
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


        // Respuesta final
        return new ResponseDto<PagedResultDto<CashierSessionDto>>
        {
            Status = true,
            Message = "Cashier sessions retrieved successfully.",
            StatusCode = 200,
            Data = new PagedResultDto<CashierSessionDto>
            {
                Items = sessionDtos,
                Pagination = new PaginationDto
                {
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                    HasPrevious = pageNumber > 1,
                    HasNext = pageNumber < (int)Math.Ceiling(totalItems / (double)pageSize)
                }
            }
        };
    }


    public async Task<ResponseDto<CashierSessionDto>> GetByIdAsync(Guid sessionId)
    {
        var userId = _userContext.GetUserId();
        var roles = _userContext.GetUserRoles();

        bool isAdmin = roles.Contains(RolesConstants.admin);
        bool isAuditor = roles.Contains(RolesConstants.auditor);

        // peticion entity a DTO directamente desde la BD
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

        // No existe
        if (sessionDto == null)
        {
            return new ResponseDto<CashierSessionDto>
            {
                Status = false,
                Message = "Cashier session not found.",
                StatusCode = 404,
                Data = null
            };
        }
        
        if (!isAdmin && !isAuditor)
        {
            // Cajero solo sus propias sesiones
            if (sessionDto.UserId != userId)
            {
                return new ResponseDto<CashierSessionDto>
                {
                    Status = false,
                    Message = "You do not have permission to view this session.",
                    StatusCode = 403,
                    Data = null
                };
            }
        }
        
        return new ResponseDto<CashierSessionDto>
        {
            Status = true,
            Message = "Cashier session retrieved successfully.",
            StatusCode = 200,
            Data = sessionDto
        };
    }

}