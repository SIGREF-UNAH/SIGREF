using Microsoft.EntityFrameworkCore;
using SIGREF.API.Dtos.Invoice;
using SIGREF.API.Services.Cashier;
using SIGREF.Common.Constants;
using SIGREF.Common.Dtos;
using SIGREF.Common.Helpers;
using SIGREF.Common.Types;
using SIGREF.Core.Entity.Billing;
using SIGREF.Infrastructure.Keycloak.Interfaces;
using SIGREF.Infrastructure.Persistence;

namespace SIGREF.API.Services.Billing;

// NOTA :
// LOS INVOICES SE ENTIENEN COMO ORDENES DE DONACION

public class InvoiceService : IInvoiceService
{
    private readonly SIGREFContext _dbContext;
    private readonly IUserContextService _userContextService;
    private readonly ICashierSessionService _cashierSessionService;

    public InvoiceService(SIGREFContext dbContext, IUserContextService userContextService , ICashierSessionService  cashierSessionService)
    {
        _dbContext = dbContext;
        _userContextService = userContextService;
        _cashierSessionService = cashierSessionService;
    }

    private void ApplyInvoiceTypeBehavior(InvoiceEntity invoice, InvoiceCreateDto dto)
    {
        // =======================
        // PAGO INICIAL (si existe)
        // =======================
        var initialPayment = dto.InitialPayment ?? 0;

        switch (dto.InvoiceType)
        {
            case InvoiceType.Normal:
                // Normal SIEMPRE se crea pagada
                invoice.AmountPaid = invoice.FinalTotal;
                invoice.AmountDue = 0;
                invoice.Status = InvoiceStatus.Paid;
                break;

            case InvoiceType.Emergency:
                // Puede o no tener pago inicial
                invoice.AmountPaid = initialPayment;
                invoice.AmountDue = invoice.FinalTotal - initialPayment;

                invoice.Status = invoice.AmountDue == 0
                    ? InvoiceStatus.Paid
                    : InvoiceStatus.Created;

                break;

            case InvoiceType.Exempt:
                invoice.FinalTotal = 0;
                invoice.AmountPaid = 0;
                invoice.AmountDue = 0;
                invoice.Status = InvoiceStatus.Paid;
                break;
        }
    }

    public async Task<ResponseDto<InvoiceDetailDto>> CreateInvoiceAsync(InvoiceCreateDto dto)
    {
        // ============================
        // VALIDACIONES
        // ============================
        
        
        // No permite crear FACTURAS HIJAS usando este método
        if (dto.ParentInvoiceId != null)
            return ResponseHelper.Fail<InvoiceDetailDto>(400,
                "Para notas de crédito o débito debe usar los métodos específicos.");

        var userId = _userContextService.GetUserId();
        var roles = _userContextService.GetUserRoles();
        Guid? activeSessionId = null;

        // Intentar obtener sesión activa para cualquier usuario
        var sessionResponse = await _cashierSessionService.GetActiveSessionByUserAsync(userId);
        
        if (sessionResponse != null && sessionResponse.Status && sessionResponse.Data != null)
        {
            activeSessionId = sessionResponse.Data.Id;
        }

        if (roles.Contains(RolesConstants.cashier))
        {
            if (activeSessionId == null)
            {
                return ResponseHelper.Fail<InvoiceDetailDto>(
                    400,
                    "No se ha aperturado un turno. Registrar la factura fuera del horario es imposible."
                );
            }
        }
        // Realizo las validaciones por la Congelacion Historica de los DATOS
        if (dto.Items == null || dto.Items.Count == 0)
            return ResponseHelper.Fail<InvoiceDetailDto>(400, "La factura debe tener al menos un item.");
        

        var serie = _dbContext.InvoiceSeries.FirstOrDefault(x => x.Id == dto.SerieId && x.IsActive);
        if (serie == null)
            return ResponseHelper.Fail<InvoiceDetailDto>(400, "La serie no existe o no esta activa");
        if (dto.SerieNumber < serie.StartNumber || dto.SerieNumber > serie.EndNumber)
            return ResponseHelper.Fail<InvoiceDetailDto>(400, "La serie no pertenece al rango");


        foreach (var item in dto.Items)
        {
            if (item.Quantity <= 0)
                return ResponseHelper.Fail<InvoiceDetailDto>(400, "La cantidad de un item no puede ser 0 o negativa.");

            if (item.UnitPrice < 0)
                return ResponseHelper.Fail<InvoiceDetailDto>(400, "El precio unitario no puede ser negativo.");
            if (item.Discount < 0)
                return ResponseHelper.Fail<InvoiceDetailDto>(400, "El precio Descuento no puede ser negativo.");


            //if (item.TotalAmount <= 0 && dto.InvoiceType != InvoiceType.Exempt)
            //    return ResponseHelper.Fail<InvoiceDetailDto>(400, "El total de un item debe ser mayor a 0.");
        }

        // ============================
        // CREAR LA FACTURA
        // ============================

        // TODO : EN UN FUTURO VERIFICAR EL SERIE NUMBER QUE NOS DAN
        // = APLICAR SERIE NUMBER AUTOMATICO EN FACTURAS

        var serviceFhirIds = new HashSet<string>();

        if (!string.IsNullOrEmpty(dto.SingleServiceFhirId))
            serviceFhirIds.Add(dto.SingleServiceFhirId);

        foreach (var item in dto.Items)
        {
            serviceFhirIds.Add(item.ServiceId);
        }
        
        var services = await _dbContext.HealthServices
            .AsNoTracking()
            .Where(s => serviceFhirIds.Contains(s.HealthServiceFhirId))
            .ToListAsync();
        
        if (services.Count != serviceFhirIds.Count)
        {
            return ResponseHelper.Fail<InvoiceDetailDto>(
                400, "Uno o más servicios no existen en SIGREF.");
        }
        var serviceMap = services.ToDictionary(
            x => x.HealthServiceFhirId,
            x => x
        );


        var invoice = new InvoiceEntity
        {
            PatientIdFhir = dto.PatientIdFhir,
            PatientDisplay = dto.PatientDisplay,
            PatientSystem = dto.PatientSystem,
            PatientValue = dto.PatientValue,

            ServiceGroupFhirId = dto.ServiceGroupFhirId,
            SingleServiceId = !string.IsNullOrEmpty(dto.SingleServiceFhirId)
                ? serviceMap[dto.SingleServiceFhirId].Id
                : null,

            InvoiceType = dto.InvoiceType,
            PaymentMethod = dto.PaymentMethod,
            SerieId = dto.SerieId,
            ParentInvoiceId = dto.ParentInvoiceId,
            CashierSessionId = activeSessionId,

            CreatedById = _userContextService.GetUserId(),
            CreatedDate = DateTime.UtcNow
        };

        // ============================
        // AGREGAR ITEMS
        // ============================
        foreach (var item in dto.Items)
        {
            var service = serviceMap[item.ServiceId];
            invoice.Items.Add(new InvoiceItemEntity
            {
                ServiceId = service.Id,
                Description = item.NameService,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                Discount = item.Discount,
                TotalAmount = item.TotalAmount,

                CreatedById = invoice.CreatedById,
                CreatedDate = DateTime.UtcNow
            });
        }

        // ============================
        // CALCULAR TOTALES
        // ============================

        invoice.TotalOriginal = invoice.Items.Sum(i => i.TotalAmount);
        invoice.AdjustmentTotal = 0;
        invoice.FinalTotal = invoice.TotalOriginal;

        // ============================
        // APLICAR COMPORTAMIENTO SEGÚN TIPO
        // ============================
        ApplyInvoiceTypeBehavior(invoice, dto);


        // ============================
        // GUARDAR
        // ============================
        _dbContext.Invoices.Add(invoice);
        await _dbContext.SaveChangesAsync();

        // ============================
        // MAPEAR DTO (manual)
        // ============================
        var detail = new InvoiceDetailDto
        {
            Id = invoice.Id,
            PatientIdFhir = invoice.PatientIdFhir,
            PatientDisplay = invoice.PatientDisplay,

            TotalOriginal = invoice.TotalOriginal,
            AdjustmentTotal = invoice.AdjustmentTotal,
            FinalTotal = invoice.FinalTotal,
            AmountPaid = invoice.AmountPaid,
            AmountDue = invoice.AmountDue,

            Status = invoice.Status,
            InvoiceType = invoice.InvoiceType,
            PaymentMethod = invoice.PaymentMethod,

            SerieId = invoice.SerieId,
            Number = invoice.Number,
            CreatedDate = invoice.CreatedDate,
            CreatedById = invoice.CreatedById,

            Items = invoice.Items.Select(x => new InvoiceItemDetailDto
            {
                Id = x.Id,
                Description = x.Description,
                Quantity = x.Quantity,
                UnitPrice = x.UnitPrice,
                Discount = x.Discount,
                TotalAmount = x.TotalAmount
            }).ToList()
        };

        return ResponseHelper.Success(201, "Factura creada correctamente.", detail);
    }

    public async Task<ResponseDto<InvoiceDetailDto>> GetInvoiceByIdAsync(
        Guid id,
        bool includeNotes = true,
        int notesPage = 1,
        int notesPageSize = 10)
    {
        // Validaciones
        if (notesPage < 1) notesPage = 1;
        if (notesPageSize < 1) notesPageSize = 20;
        if (notesPageSize > 100) notesPageSize = 100;

        // Query 1: Factura principal
        var invoice = await _dbContext.Invoices
            .AsNoTracking()
            .Where(i => i.Id == id)
            .Select(i => new InvoiceDetailDto
            {
                Id = i.Id,
                PatientIdFhir = i.PatientIdFhir,
                PatientDisplay = i.PatientDisplay,
                TotalOriginal = i.TotalOriginal,
                AdjustmentTotal = i.AdjustmentTotal,
                FinalTotal = i.FinalTotal,
                AmountPaid = i.AmountPaid,
                AmountDue = i.AmountDue,
                Status = i.Status,
                InvoiceType = i.InvoiceType,
                PaymentMethod = i.PaymentMethod,
                SerieId = i.SerieId,
                SerieName = i.Serie.Name,
                Number = i.Number,
                CreatedDate = i.CreatedDate,
                CreatedById = i.CreatedById,
                ParentInvoiceId = i.ParentInvoiceId,
                ParentInvoiceNumber = i.ParentInvoice != null ? i.ParentInvoice.Number.ToString() : null,
                Items = i.Items.Select(x => new InvoiceItemDetailDto
                {
                    Id = x.Id,
                    Description = x.Description,
                    Quantity = x.Quantity,
                    UnitPrice = x.UnitPrice,
                    Discount = x.Discount,
                    TotalAmount = x.TotalAmount
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (invoice == null)
            return ResponseHelper.Fail<InvoiceDetailDto>(404, "La factura no existe.");

        if (!includeNotes || invoice.ParentInvoiceId != null)
            return ResponseHelper.Success(200, "Factura encontrada.", invoice);

        // Query 2: Summary (SIEMPRE completo)
        var summary = await _dbContext.Invoices
            .AsNoTracking()
            .Where(n => n.ParentInvoiceId == id)
            .GroupBy(n => 1)
            .Select(g => new
            {
                TotalCredit = g
                    .Where(n => n.InvoiceType == InvoiceType.CreditNote)
                    .Sum(n => (decimal?)n.FinalTotal) ?? 0,
                TotalDebit = g
                    .Where(n => n.InvoiceType == InvoiceType.DebitNote)
                    .Sum(n => (decimal?)n.FinalTotal) ?? 0,
                CountCredit = g.Count(n => n.InvoiceType == InvoiceType.CreditNote),
                CountDebit = g.Count(n => n.InvoiceType == InvoiceType.DebitNote),
                TotalCount = g.Count()
            })
            .FirstOrDefaultAsync();

        // Si no hay notas, retornar
        if (summary == null || summary.TotalCount == 0)
            return ResponseHelper.Success(200, "Factura encontrada.", invoice);

        // Query 3: Children paginados
        var skip = (notesPage - 1) * notesPageSize;
        invoice.Children = await _dbContext.Invoices
            .AsNoTracking()
            .Where(n => n.ParentInvoiceId == id)
            .OrderByDescending(n => n.CreatedDate)
            .Skip(skip)
            .Take(notesPageSize)
            .Select(n => new InvoiceChildDto
            {
                Id = n.Id,
                InvoiceType = n.InvoiceType,
                Status = n.Status,
                FinalTotal = n.FinalTotal,
                CreatedDate = n.CreatedDate,
                Number = n.Number,
                SerieId = n.SerieId
            })
            .ToListAsync();

        // Asignar summary con paginación
        invoice.NotesSummary = new InvoiceNotesSummaryDto
        {
            TotalCreditNotes = summary.TotalCredit,
            TotalDebitNotes = summary.TotalDebit,
            CountCredit = summary.CountCredit,
            CountDebit = summary.CountDebit,
            TotalNotes = summary.TotalCount,
            CurrentPage = notesPage,
            PageSize = notesPageSize,
            TotalPages = (int)Math.Ceiling(summary.TotalCount / (double)notesPageSize)
        };

        return ResponseHelper.Success(200, "Factura encontrada.", invoice);
    }


    // =====================================================================
    // InvoiceService.cs - Método GetInvoicesAsync
    // =====================================================================
    public async Task<ResponseDto<PagedResultDto<InvoiceGetDto>>> GetInvoicesAsync(InvoiceFilterDto filter)
    {
        // ================================
        // PAGINACIÓN (con defaults)
        // ================================
        filter.PageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
        filter.PageSize = filter.PageSize < 1 ? 10 : filter.PageSize;
        filter.PageSize = filter.PageSize > 100 ? 100 : filter.PageSize;

        // ================================
        // VALIDACIONES LÓGICAS
        // ================================
        if (filter.OnlyInvoices == true && filter.OnlyNotes == true)
            return ResponseHelper.Fail<PagedResultDto<InvoiceGetDto>>(400,
                "No se puede filtrar por facturas y notas al mismo tiempo.");

        if (filter.DateFrom is not null && filter.DateTo is not null &&
            filter.DateFrom > filter.DateTo)
            return ResponseHelper.Fail<PagedResultDto<InvoiceGetDto>>(400,
                "La fecha inicial no puede ser mayor a la fecha final.");

        if (filter.MinTotal is not null && filter.MaxTotal is not null &&
            filter.MinTotal > filter.MaxTotal)
            return ResponseHelper.Fail<PagedResultDto<InvoiceGetDto>>(400,
                "El total mínimo no puede ser mayor al total máximo.");

        // ================================
        // QUERY BASE
        // ================================
        var query = _dbContext.Invoices
            .AsNoTracking()
            .AsQueryable();

        // ================================
        // APLICAR FILTROS
        // ================================
        // Facturas reales
        if (filter.OnlyInvoices == true)
            query = query.Where(i =>
                i.InvoiceType != InvoiceType.CreditNote &&
                i.InvoiceType != InvoiceType.DebitNote);

        // Notas
        if (filter.OnlyNotes == true)
            query = query.Where(i =>
                i.InvoiceType == InvoiceType.CreditNote ||
                i.InvoiceType == InvoiceType.DebitNote);

        // Tipo
        if (filter.InvoiceType is not null)
            query = query.Where(i => i.InvoiceType == filter.InvoiceType);

        // Estado
        if (filter.Status is not null)
            query = query.Where(i => i.Status == filter.Status);

        // Serie
        if (filter.SerieId is not null)
            query = query.Where(i => i.SerieId == filter.SerieId);

        // Número
        if (filter.Number is not null)
            query = query.Where(i => i.Number == filter.Number);

        // Paciente por ID FHIR
        if (!string.IsNullOrWhiteSpace(filter.PatientIdFhir))
            query = query.Where(i => i.PatientIdFhir == filter.PatientIdFhir);

        // Paciente por nombre
        if (!string.IsNullOrWhiteSpace(filter.PatientDisplay))
        {
            var searchP = filter.PatientDisplay.ToLower();
            query = query.Where(i =>
                i.PatientDisplay != null &&
                i.PatientDisplay.ToLower().Contains(searchP));
        }

        // Sesión de caja
        if (filter.CashierSessionId is not null)
            query = query.Where(i => i.CashierSessionId == filter.CashierSessionId);

        // Fecha desde
        if (filter.DateFrom is not null)
        {
            var from = filter.DateFrom.Value.Date;
            query = query.Where(i => i.CreatedDate >= from);
        }

        // Fecha hasta (incluyente)
        if (filter.DateTo is not null)
        {
            var to = filter.DateTo.Value.Date.AddDays(1);
            query = query.Where(i => i.CreatedDate < to);
        }

        // Total mínimo
        if (filter.MinTotal is not null)
            query = query.Where(i => i.FinalTotal >= filter.MinTotal);

        // Total máximo
        if (filter.MaxTotal is not null)
            query = query.Where(i => i.FinalTotal <= filter.MaxTotal);

        // Búsqueda global
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.ToLower();
            query = query.Where(i =>
                i.Number.ToString().Contains(search) ||
                (i.PatientDisplay != null && i.PatientDisplay.ToLower().Contains(search)) ||
                (i.Serie != null && i.Serie.Name.ToLower().Contains(search))
            );
        }

        // ================================
        // CONTAR RESULTADOS
        // ================================
        var totalItems = await query.CountAsync();
        if (totalItems == 0)
        {
            return ResponseHelper.Success(200, "No se encontraron facturas.",
                new PagedResultDto<InvoiceGetDto>
                {
                    Items = Enumerable.Empty<InvoiceGetDto>(),
                    Pagination = new PaginationDto
                    {
                        CurrentPage = filter.PageNumber,
                        PageSize = filter.PageSize,
                        TotalItems = 0,
                        TotalPages = 0,
                        HasPrevious = false,
                        HasNext = false
                    }
                });
        }

        // ================================
        // ORDENAMIENTO CON ENUM
        // ================================
        var sortBy = filter.SortBy ?? InvoiceSortField.CreatedDate;
        var desc = filter.SortDescending;

        query = query.ApplyInvoiceSorting(filter.SortBy, filter.SortDescending);


        var skip = (filter.PageNumber - 1) * filter.PageSize;

        var items = await query
            .Skip(skip)
            .Take(filter.PageSize)
            .Select(i => new InvoiceGetDto
            {
                Id = i.Id,
                PatientDisplay = i.PatientDisplay,
                Status = i.Status,
                InvoiceType = i.InvoiceType,
                TotalOriginal = i.TotalOriginal,
                AdjustmentTotal = i.AdjustmentTotal,
                FinalTotal = i.FinalTotal,
                AmountPaid = i.AmountPaid,
                AmountDue = i.AmountDue,
                Number = i.Number,
                CreatedDate = i.CreatedDate
            })
            .ToListAsync();

        // ================================
        // RESPUESTA FINAL
        // ================================
        var totalPages = (int)Math.Ceiling(totalItems / (double)filter.PageSize);

        return ResponseHelper.Success(200, "Facturas obtenidas correctamente.",
            new PagedResultDto<InvoiceGetDto>
            {
                Items = items,
                Pagination = new PaginationDto
                {
                    CurrentPage = filter.PageNumber,
                    PageSize = filter.PageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages,
                    HasPrevious = filter.PageNumber > 1,
                    HasNext = filter.PageNumber < totalPages
                }
            });
    }


    public async Task<ResponseDto<InvoiceDetailDto>> CancelInvoiceAsync(Guid id)
    {
        // ================================
        // Obtener factura
        // ================================
        var invoice = await _dbContext.Invoices
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (invoice == null)
            return ResponseHelper.Fail<InvoiceDetailDto>(404, "La factura no existe.");

        // ================================
        //  Validaciones de negocio
        // ================================

        // Ya está cancelada
        if (invoice.Status == InvoiceStatus.Cancelled)
            return ResponseHelper.Fail<InvoiceDetailDto>(400, "La factura ya está cancelada.");

        // No se cancela si tiene notas hijas
        var hasNotes = await _dbContext.Invoices
            .AsNoTracking()
            .AnyAsync(x => x.ParentInvoiceId == id);

        if (hasNotes)
        {
            return ResponseHelper.Fail<InvoiceDetailDto>(400,
                "No se puede cancelar esta factura, tiene notas de crédito o débito asociadas.");
        }

        // Factura hija (nota) no debe cancelarse aquí
        if (invoice.ParentInvoiceId != null)
        {
            return ResponseHelper.Fail<InvoiceDetailDto>(400,
                "Esta factura es una nota de ajuste. Use el proceso correspondiente para anular notas.");
        }

        // Si está pagada no puede cancelarse directamente
        if (invoice.Status == InvoiceStatus.Paid && invoice.InvoiceType != InvoiceType.Exempt)
        {
            return ResponseHelper.Fail<InvoiceDetailDto>(400,
                "La factura ya fue pagada. Requiere nota de crédito para revertirla.");
        }

        // ================================
        // Aplicar cancelación
        // ================================
        invoice.Status = InvoiceStatus.Cancelled;

        // Limpieza operativa (no histórica)
        invoice.AmountPaid = 0;
        invoice.AmountDue = 0;

        invoice.UpdatedById = _userContextService.GetUserId();
        invoice.UpdatedDate = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        var detail = new InvoiceDetailDto
        {
            Id = invoice.Id,
            PatientIdFhir = invoice.PatientIdFhir,
            PatientDisplay = invoice.PatientDisplay,
            TotalOriginal = invoice.TotalOriginal,
            AdjustmentTotal = invoice.AdjustmentTotal,
            FinalTotal = invoice.FinalTotal,
            AmountPaid = invoice.AmountPaid,
            AmountDue = invoice.AmountDue,
            Status = invoice.Status,
            InvoiceType = invoice.InvoiceType,
            PaymentMethod = invoice.PaymentMethod,
            SerieId = invoice.SerieId,
            Number = invoice.Number,
            CreatedDate = invoice.CreatedDate,
            CreatedById = invoice.CreatedById,
            Items = invoice.Items.Select(x => new InvoiceItemDetailDto
            {
                Id = x.Id,
                Description = x.Description,
                Quantity = x.Quantity,
                UnitPrice = x.UnitPrice,
                Discount = x.Discount,
                TotalAmount = x.TotalAmount
            }).ToList()
        };

        return ResponseHelper.Success(200, "Factura cancelada correctamente.", detail);
    }


    public async Task<ResponseDto<InvoiceDetailDto>> MarkAsPaidAsync(Guid id, decimal amountPaid)
    {
        // ================================
        // Validación del monto
        // ================================
        if (amountPaid <= 0)
            return ResponseHelper.Fail<InvoiceDetailDto>(400, "El monto pagado debe ser mayor a 0.");

        // ================================
        // Obtener factura
        // ================================
        var invoice = await _dbContext.Invoices
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (invoice == null)
            return ResponseHelper.Fail<InvoiceDetailDto>(404, "La factura no existe.");


        // No pagar notas de crédito ni débito
        if (invoice.InvoiceType == InvoiceType.CreditNote ||
            invoice.InvoiceType == InvoiceType.DebitNote)
        {
            return ResponseHelper.Fail<InvoiceDetailDto>(400,
                "Las notas de crédito o débito no se pagan.");
        }

        // No pagar si ya está cancelada
        if (invoice.Status == InvoiceStatus.Cancelled)
            return ResponseHelper.Fail<InvoiceDetailDto>(400, "La factura está cancelada.");

        // No pagar si ya está completamente pagada
        if (invoice.Status == InvoiceStatus.Paid && invoice.AmountDue == 0)
            return ResponseHelper.Fail<InvoiceDetailDto>(400, "La factura ya está pagada.");

        // Pago mayor al saldo error
        if (amountPaid > invoice.AmountDue)
        {
            return ResponseHelper.Fail<InvoiceDetailDto>(400,
                $"El pago excede el saldo pendiente. Saldo actual: {invoice.AmountDue}.");
        }

        // ================================
        // Aplicar pago
        // ================================
        invoice.AmountPaid += amountPaid;
        invoice.AmountDue = invoice.FinalTotal - invoice.AmountPaid;

        invoice.Status = invoice.AmountDue == 0
            ? InvoiceStatus.Paid
            : InvoiceStatus.Created;

        invoice.UpdatedById = _userContextService.GetUserId();
        invoice.UpdatedDate = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        var detail = new InvoiceDetailDto
        {
            Id = invoice.Id,
            PatientIdFhir = invoice.PatientIdFhir,
            PatientDisplay = invoice.PatientDisplay,

            TotalOriginal = invoice.TotalOriginal,
            AdjustmentTotal = invoice.AdjustmentTotal,
            FinalTotal = invoice.FinalTotal,
            AmountPaid = invoice.AmountPaid,
            AmountDue = invoice.AmountDue,

            Status = invoice.Status,
            InvoiceType = invoice.InvoiceType,
            PaymentMethod = invoice.PaymentMethod,

            SerieId = invoice.SerieId,
            Number = invoice.Number,

            CreatedDate = invoice.CreatedDate,
            CreatedById = invoice.CreatedById,

            ParentInvoiceId = invoice.ParentInvoiceId,

            Items = invoice.Items.Select(x => new InvoiceItemDetailDto
            {
                Id = x.Id,
                Description = x.Description,
                Quantity = x.Quantity,
                UnitPrice = x.UnitPrice,
                Discount = x.Discount,
                TotalAmount = x.TotalAmount
            }).ToList()
        };

        return ResponseHelper.Success(200, "Factura pagada correctamente.", detail);
    }


    public async Task<ResponseDto<InvoiceDetailDto>> CreateNoteAsync(
        Guid parentInvoiceId,
        InvoiceCreateDto dto,
        InvoiceType noteType)
    {
        // ============================
        // VALIDACIONES
        // ============================

        //  Validar tipo de nota
        if (noteType != InvoiceType.CreditNote &&
            noteType != InvoiceType.DebitNote)
        {
            return ResponseHelper.Fail<InvoiceDetailDto>(400,
                "Solo se permiten notas de crédito o débito.");
        }

        // Obtener factura padre
        var parent = await _dbContext.Invoices
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == parentInvoiceId);

        if (parent == null)
            return ResponseHelper.Fail<InvoiceDetailDto>(404,
                "La factura origen no existe.");

        // No permitir notas sobre notas
        if (parent.ParentInvoiceId != null)
            return ResponseHelper.Fail<InvoiceDetailDto>(400,
                "No se pueden generar notas sobre una nota.");

        // No permitir notas sobre fcaturas canceladas
        if (parent.Status == InvoiceStatus.Cancelled)
            return ResponseHelper.Fail<InvoiceDetailDto>(400,
                "No se pueden generar notas de una factura cancelada.");
        var serie = _dbContext.InvoiceSeries.FirstOrDefault(x => x.Id == dto.SerieId && x.IsActive);
        if (serie == null)
            return ResponseHelper.Fail<InvoiceDetailDto>(400, "La serie no existe o no esta activa");
        if (dto.SerieNumber < serie.StartNumber || dto.SerieNumber > serie.EndNumber)
            return ResponseHelper.Fail<InvoiceDetailDto>(400, "La serie no pertenece al rango");
        // Validar items congelados (nota debe especificar montos exactos)
        if (dto.Items == null || dto.Items.Count == 0)
            return ResponseHelper.Fail<InvoiceDetailDto>(400,
                "La nota debe tener items.");

        foreach (var item in dto.Items)
        {
            if (item.Quantity <= 0)
                return ResponseHelper.Fail<InvoiceDetailDto>(400, "Cantidad inválida.");

            if (item.UnitPrice < 0)
                return ResponseHelper.Fail<InvoiceDetailDto>(400, "Precio inválido.");

            if (item.TotalAmount <= 0)
                return ResponseHelper.Fail<InvoiceDetailDto>(400, "El total debe ser mayor que 0.");
        }
        // ============================
        // RESOLVER SERVICIOS (FHIR → SIGREF)
        // ============================

        var serviceFhirIds = dto.Items
            .Select(i => i.ServiceId)
            .Distinct()
            .ToList();

        var services = await _dbContext.HealthServices
            .AsNoTracking()
            .Where(s => serviceFhirIds.Contains(s.HealthServiceFhirId))
            .ToListAsync();

        if (services.Count != serviceFhirIds.Count)
            return ResponseHelper.Fail<InvoiceDetailDto>(
                400, "Uno o más servicios no existen en SIGREF.");

        var serviceMap = services.ToDictionary(
            s => s.HealthServiceFhirId,
            s => s
        );
        // ============================
        // CREAR NOTA (factura hija)
        // ============================
        var note = new InvoiceEntity
        {
            ParentInvoiceId = parentInvoiceId,
            PatientIdFhir = parent.PatientIdFhir,
            PatientDisplay = parent.PatientDisplay,

            InvoiceType = noteType, // CREDITO o DEBITO
            PaymentMethod = PaymentMethodType.Cash,

            SerieId = dto.SerieId,
            CreatedById = _userContextService.GetUserId(),
            CreatedDate = DateTime.UtcNow
        };

        // Items
        note.Items = dto.Items.Select(i =>
        {
            var service = serviceMap[i.ServiceId];

            return new InvoiceItemEntity
            {
                ServiceId = service.Id, // ID INTERNO SIGREF
                Description = i.NameService,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Discount = i.Discount,
                TotalAmount = i.TotalAmount,
                CreatedById = note.CreatedById,
                CreatedDate = DateTime.UtcNow
            };
        }).ToList();

        // Totales de la nota
        note.TotalOriginal = note.Items.Sum(x => x.TotalAmount);

        // NOTA DE CREDITO: monto negativo
        if (noteType == InvoiceType.CreditNote)
            note.TotalOriginal *= -1;

        // NOTA DE DÉBITO: monto positivo por defecto

        note.AdjustmentTotal = 0;
        note.FinalTotal = note.TotalOriginal;
        note.AmountPaid = 0;
        note.AmountDue = 0;
        note.Status = InvoiceStatus.Paid; // Notas siempre pagadas 

        // TODO : 
        // VERIFICAR METODO Y FUNCIONAMIENTO

        // ============================
        // AUMENTAR EL NUMBERO DE LA SERIE
        // ============================
        //note.Number = await _dbContext.Invoices
        //    .Where(x => x.SerieId == note.SerieId)
        //    .Select(x => x.Number)
        //    .DefaultIfEmpty(0)
        //    .MaxAsync() + 1;

        // ============================
        // GUARDAR NOTA
        // ============================
        _dbContext.Invoices.Add(note);
        await _dbContext.SaveChangesAsync();

        // ============================
        // RECALCULAR PADRE
        // ============================
        await RecalculateInvoiceTotalsAsync(parentInvoiceId);

        // ============================
        // MAPEAR DTO
        // ============================
        var detail = new InvoiceDetailDto
        {
            Id = note.Id,
            PatientIdFhir = note.PatientIdFhir,
            PatientDisplay = note.PatientDisplay,
            TotalOriginal = note.TotalOriginal,
            AdjustmentTotal = note.AdjustmentTotal,
            FinalTotal = note.FinalTotal,
            AmountPaid = note.AmountPaid,
            AmountDue = note.AmountDue,
            Status = note.Status,
            InvoiceType = note.InvoiceType,
            PaymentMethod = note.PaymentMethod,
            SerieId = note.SerieId,
            Number = note.Number,
            CreatedDate = note.CreatedDate,
            CreatedById = note.CreatedById,
            ParentInvoiceId = parentInvoiceId
        };

        return ResponseHelper.Success(201, "Nota creada correctamente.", detail);
    }


    public async Task<ResponseDto<List<MinimalInvoiceDto>>> HasChildNotesAsync(Guid invoiceId)
    {
        // ================================
        // Validar si existe y traer SOLO lo que necesitamos
        // ================================
        var invoiceData = await _dbContext.Invoices
            .AsNoTracking()
            .Where(i => i.Id == invoiceId)
            .Select(i => new
            {
                i.Id,
                i.InvoiceType
            })
            .FirstOrDefaultAsync();

        if (invoiceData == null)
            return ResponseHelper.Fail<List<MinimalInvoiceDto>>(404, "La factura no existe.");

        // ================================
        // 2. Verificar que NO sea una nota
        // ================================
        if (invoiceData.InvoiceType == InvoiceType.CreditNote ||
            invoiceData.InvoiceType == InvoiceType.DebitNote)
        {
            return ResponseHelper.Fail<List<MinimalInvoiceDto>>(400,
                "Este documento es una nota, no puede tener notas asociadas.");
        }

        // ================================
        // Buscar notas hijas (SOLO GUID)
        // ================================
        var childNotes = await _dbContext.Invoices
            .AsNoTracking()
            .Where(x => x.ParentInvoiceId == invoiceId)
            .Select(x => new MinimalInvoiceDto
            {
                Id = x.Id,
                Number = x.Number,
                SerieId = x.SerieId,
                SerieName = x.Serie!.Name,
                Status = x.Status,
                InvoiceType = x.InvoiceType
            })
            .OrderByDescending(x => x.Number)
            .ToListAsync();

        // ================================
        //  Respuestas limpias
        // ================================
        if (childNotes.Count == 0)
            return ResponseHelper.Success(200, "La factura no tiene notas asociadas.", new List<MinimalInvoiceDto>());

        return ResponseHelper.Success(200, "La factura tiene notas asociadas.", childNotes);
    }


    public async Task<ResponseDto<bool>> RecalculateInvoiceTotalsAsync(Guid invoiceId)
    {
        // ================================
        // Validar factura padre
        // ================================
        var invoice = await _dbContext.Invoices
            .FirstOrDefaultAsync(i => i.Id == invoiceId);

        if (invoice == null)
            return ResponseHelper.Fail<bool>(404, "La factura no existe.");

        if (invoice.InvoiceType == InvoiceType.CreditNote ||
            invoice.InvoiceType == InvoiceType.DebitNote)
        {
            return ResponseHelper.Fail<bool>(400,
                "No se pueden recalcular notas. Solo facturas reales.");
        }

        // ================================
        // Traer notas hijas (credit + debit)
        // ================================
        var childNotes = await _dbContext.Invoices
            .AsNoTracking()
            .Where(n => n.ParentInvoiceId == invoiceId)
            .Select(n => new
            {
                n.FinalTotal,
                n.InvoiceType
            })
            .ToListAsync();

        // Si no hay notas no cambios
        if (childNotes.Count == 0)
        {
            // Ajuste total a 0
            invoice.AdjustmentTotal = 0;
            invoice.FinalTotal = invoice.TotalOriginal;

            invoice.AmountDue = invoice.FinalTotal - invoice.AmountPaid;

            // Estado si varía
            invoice.Status = invoice.AmountDue == 0
                ? InvoiceStatus.Paid
                : InvoiceStatus.Created;

            invoice.UpdatedById = _userContextService.GetUserId();
            invoice.UpdatedDate = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
            return ResponseHelper.Success(200, "Totales recalculados correctamente.", true);
        }

        // ================================
        // Calcular ajustes
        // ================================
        decimal totalCredits = childNotes
            .Where(x => x.InvoiceType == InvoiceType.CreditNote)
            .Sum(x => x.FinalTotal); // valores negativos

        decimal totalDebits = childNotes
            .Where(x => x.InvoiceType == InvoiceType.DebitNote)
            .Sum(x => x.FinalTotal); // valores positivos

        invoice.AdjustmentTotal = totalCredits + totalDebits;

        // ================================
        // Calcular total final
        // ================================
        invoice.FinalTotal = invoice.TotalOriginal + invoice.AdjustmentTotal;

        // ================================
        // Calcular saldo pendiente
        // ================================
        invoice.AmountDue = invoice.FinalTotal - invoice.AmountPaid;

        // ================================
        // Actualizar estado automático
        // ================================
        if (invoice.AmountDue == 0)
            invoice.Status = InvoiceStatus.Paid;
        else if (invoice.AmountDue > 0)
            invoice.Status = InvoiceStatus.Created;
        else
            invoice.Status = InvoiceStatus.Refunded; // cuando queda en negativo

        // ================================
        // Auditoría
        // ================================
        invoice.UpdatedById = _userContextService.GetUserId();
        invoice.UpdatedDate = DateTime.UtcNow;

        // ================================
        // Guardar cambios
        // ================================
        await _dbContext.SaveChangesAsync();

        return ResponseHelper.Success(200, "Totales recalculados correctamente.", true);
    }


    public async Task<ResponseDto<InvoiceNotesSummaryDto>> GetNotesSummaryAsync(Guid invoiceId)
    {
        // ================================
        // Validar factura padre
        // ================================
        var invoice = await _dbContext.Invoices
            .AsNoTracking()
            .Where(i => i.Id == invoiceId)
            .Select(i => new { i.Id, i.InvoiceType })
            .FirstOrDefaultAsync();

        if (invoice == null)
            return ResponseHelper.Fail<InvoiceNotesSummaryDto>(404, "La factura no existe.");
        // Validar que no sea una nota , si no una Factura
        if (invoice.InvoiceType == InvoiceType.CreditNote ||
            invoice.InvoiceType == InvoiceType.DebitNote)
        {
            return ResponseHelper.Fail<InvoiceNotesSummaryDto>(400,
                "Este documento es una nota, no posee notas asociadas.");
        }

        // ================================
        // Obtener notas hijas (solo campos necesarios)
        // ================================
        var notes = await _dbContext.Invoices
            .AsNoTracking()
            .Where(n => n.ParentInvoiceId == invoiceId)
            .Select(n => new
            {
                n.FinalTotal, // negativo en crédito, positivo en débito
                n.InvoiceType
            })
            .ToListAsync();

        // Si no tiene notas → retornar summary vacío
        if (notes.Count == 0)
        {
            var emptySummary = new InvoiceNotesSummaryDto
            {
                TotalCreditNotes = 0,
                TotalDebitNotes = 0,
                CountCredit = 0,
                CountDebit = 0
            };

            return ResponseHelper.Success(200, "La factura no posee notas.", emptySummary);
        }

        // ================================
        // Calcular summary
        // ================================
        var summary = new InvoiceNotesSummaryDto
        {
            TotalCreditNotes = notes
                .Where(x => x.InvoiceType == InvoiceType.CreditNote)
                .Sum(x => x.FinalTotal), // valores negativos

            TotalDebitNotes = notes
                .Where(x => x.InvoiceType == InvoiceType.DebitNote)
                .Sum(x => x.FinalTotal), // valores positivos

            CountCredit = notes.Count(x => x.InvoiceType == InvoiceType.CreditNote),
            CountDebit = notes.Count(x => x.InvoiceType == InvoiceType.DebitNote)
        };

        return ResponseHelper.Success(200, "Resumen de notas obtenido correctamente.", summary);
    }
}