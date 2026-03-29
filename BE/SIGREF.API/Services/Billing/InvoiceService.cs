using Microsoft.EntityFrameworkCore;
using SIGREF.API.Dtos.Invoice;
using SIGREF.API.Extensions;
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

    public InvoiceService(SIGREFContext dbContext, IUserContextService userContextService,
        ICashierSessionService cashierSessionService)
    {
        _dbContext = dbContext;
        _userContextService = userContextService;
        _cashierSessionService = cashierSessionService;
    }

    
    // =====================================================================
    // HELPERS PRIVADOS
    // =====================================================================
 
    /// <summary>
    /// Aplica el comportamiento de pago/estado según el tipo de factura.
    /// Se llama DESPUÉS de haber calculado FinalTotal.
    /// </summary>
    private void ApplyInvoiceTypeBehavior(InvoiceEntity invoice, InvoiceCreateDto dto)
    {
        // =======================
        // PAGO INICIAL (si existe)
        // =======================
        

        switch (dto.InvoiceType)
        {
            case InvoiceType.Normal:
                // Normal siempre se crea pagada al 100%.
                invoice.AmountPaid = invoice.FinalTotal;
                invoice.AmountDue = 0;
                invoice.Status = InvoiceStatus.Paid;
                break;

            case InvoiceType.Emergency:
                // Puede tener pago inicial (parcial o total).
                var initialPayment = dto.InitialPayment ?? 0;
                invoice.AmountPaid = initialPayment;
                invoice.AmountDue = invoice.FinalTotal - initialPayment;

                invoice.Status = invoice.AmountDue == 0
                    ? InvoiceStatus.Paid
                    : InvoiceStatus.Created;

                break;

            case InvoiceType.Exempt:
                // Exento: el descuento cubre todo; FinalTotal queda en 0.
                invoice.FinalTotal = 0;
                invoice.AmountPaid = 0;
                invoice.AmountDue = 0;
                invoice.Status = InvoiceStatus.Paid;
                break;
        }
    }


    // =====================================================================
    // CREAR FACTURA
    // =====================================================================

    public async Task<ResponseDto<InvoiceDetailDto>> CreateInvoiceAsync(InvoiceCreateDto dto)
    {
        // ============================
        // VALIDACIONES GENERALES
        // ============================
 
        if (dto.ParentInvoiceId != null)
            return ResponseHelper.Fail<InvoiceDetailDto>(400,
                "Para notas de crédito o débito use los métodos específicos.");
 
        var userId = _userContextService.GetUserId();
        var roles  = _userContextService.GetUserRoles();
        Guid? activeSessionId = null;
        
        var sessionResponse   = await _cashierSessionService.GetActiveSessionByUserAsync(userId);
        if (sessionResponse?.Status == true && sessionResponse.Data != null)
            activeSessionId = sessionResponse.Data.Id;
 
        if (roles.Contains(RolesConstants.cashier) && activeSessionId == null)
            return ResponseHelper.Fail<InvoiceDetailDto>(400,
                "No se ha aperturado un turno. No es posible registrar la factura fuera de horario.");
 
        if (dto.Items == null || dto.Items.Count == 0)
            return ResponseHelper.Fail<InvoiceDetailDto>(400,
                "La factura debe tener al menos un ítem.");

        // ============================
        // VALIDAR SERIE
        // ============================
 
        var serie = _dbContext.InvoiceSeries.FirstOrDefault(x => x.Id == dto.SerieId && x.IsActive);
        if (serie == null)
            return ResponseHelper.Fail<InvoiceDetailDto>(400, "La serie no existe o no está activa.");
 
        if (dto.SerieNumber < serie.StartNumber || dto.SerieNumber > serie.EndNumber)
            return ResponseHelper.Fail<InvoiceDetailDto>(400, "El número no pertenece al rango de la serie.");


        foreach (var item in dto.Items)
        {
            if (item.Quantity <= 0)
                return ResponseHelper.Fail<InvoiceDetailDto>(400,
                    $"La cantidad del ítem '{item.NameService}' debe ser mayor a 0.");
 
            if (item.UnitPrice < 0)
                return ResponseHelper.Fail<InvoiceDetailDto>(400,
                    $"El precio unitario del ítem '{item.NameService}' no puede ser negativo.");
        }
        
        // ============================
        // VALIDAR DESCUENTO GLOBAL
        // ============================
 
        var invoiceDiscount = dto.InvoiceDiscount ?? Decimal.Zero;
        if (invoiceDiscount < 0)
            return ResponseHelper.Fail<InvoiceDetailDto>(400, "El descuento no puede ser negativo.");
        
        // ============================
        // RESOLVER SERVICIOS FHIR - SIGREF
        // ============================
 
        var fhirIds = new HashSet<string>(dto.Items.Select(i => i.ServiceId));
        if (!string.IsNullOrEmpty(dto.SingleServiceFhirId))
            fhirIds.Add(dto.SingleServiceFhirId);
 
        var services = await _dbContext.HealthServices
            .AsNoTracking()
            .Where(s => fhirIds.Contains(s.HealthServiceFhirId))
            .ToListAsync();
 
        if (services.Count != fhirIds.Count)
            return ResponseHelper.Fail<InvoiceDetailDto>(400,
                "Uno o más servicios no existen en SIGREF.");
 
        var serviceMap = services.ToDictionary(s => s.HealthServiceFhirId, s => s);
        // ============================
        // CONSTRUIR ENTIDAD
        // ============================
 
        var invoice = new InvoiceEntity
        {
            PatientIdFhir  = dto.PatientIdFhir,
            PatientDisplay = dto.PatientDisplay,
            PatientSystem  = dto.PatientSystem,
            PatientValue   = dto.PatientValue,
 
            ServiceGroupFhirId = dto.ServiceGroupFhirId,
            SingleServiceId    = !string.IsNullOrEmpty(dto.SingleServiceFhirId)
                ? serviceMap[dto.SingleServiceFhirId].Id
                : null,
 
            InvoiceType    = dto.InvoiceType,
            PaymentMethod  = dto.PaymentMethod,
            SerieId        = dto.SerieId,
            Number         = dto.SerieNumber,
            CashierSessionId = activeSessionId,
            InvoiceDiscount = dto.InvoiceDiscount?? decimal.Zero,
 
            CreatedById  = userId,
            CreatedDate  = DateTimeOffset.UtcNow
        };

        // TODO : EN UN FUTURO VERIFICAR EL SERIE NUMBER QUE NOS DAN
        // = APLICAR SERIE NUMBER AUTOMATICO EN FACTURAS

        // ============================
        // AGREGAR ITEMS Y CALCULAR TOTALES
        // El servidor calcula TotalAmount; el cliente NO puede manipularlo.
        // ============================
 
        foreach (var item in dto.Items)
        {
            var service   = serviceMap[item.ServiceId];
            var lineTotal = item.Quantity * item.UnitPrice;  // calculado aqui
 
            invoice.Items.Add(new InvoiceItemEntity
            {
                ServiceId   = service.Id,
                Description = item.NameService,
                Quantity    = item.Quantity,
                UnitPrice   = item.UnitPrice,
                TotalAmount = lineTotal,            // servidor calcula
 
                CreatedById = userId,
                CreatedDate = DateTime.UtcNow
            });
        }
        // ============================
        // CALCULAR TOTALES DE LA FACTURA
        //
        //   TotalOriginal   = Σ(Qty × UnitPrice)  — bruto, sin descuento
        //   InvoiceDiscount = descuento global ingresado (se valida abajo)
        //   AdjustmentTotal = 0 al crear (se actualiza cuando llegan notas C/D)
        //   FinalTotal      = TotalOriginal - InvoiceDiscount
        // ============================
 
        invoice.TotalOriginal = invoice.Items.Sum(i => i.TotalAmount);
 
        // Descuento no puede superar el total bruto
        if (dto.InvoiceDiscount > invoice.TotalOriginal)
            return ResponseHelper.Fail<InvoiceDetailDto>(400,
                $"El descuento ({dto.InvoiceDiscount}) no puede superar el total ({invoice.TotalOriginal}).");
 
        invoice.InvoiceDiscount = invoiceDiscount;
        invoice.AdjustmentTotal = 0;
        invoice.FinalTotal      = invoice.TotalOriginal - invoice.InvoiceDiscount;
 
        // ============================
        // COMPORTAMIENTO POR TIPO
        // (puede sobrescribir FinalTotal en el caso Exempt)
        // ============================
 
        ApplyInvoiceTypeBehavior(invoice, dto);
 
        // ============================
        // GUARDAR
        // ============================
 
        _dbContext.Invoices.Add(invoice);
        await _dbContext.SaveChangesAsync();
 
        return ResponseHelper.Success(201, "Factura creada correctamente.", InvoiceExtensions.MapToDetail(invoice));
    }

        
    public async Task<ResponseDto<InvoiceDetailDto?>> GetInvoiceByIdAsync(
        Guid id,
        GetInvoiceParameters parameters)
    {
        if (parameters.NotesPage < 1)     parameters.NotesPage     = 1;
        if (parameters.NotesPageSize < 1) parameters.NotesPageSize = 20;
        if (parameters.NotesPageSize > 100) parameters.NotesPageSize = 100;

        // Query 1: Factura principal
        var invoice = await _dbContext.Invoices
            .AsNoTracking()
            .Where(i => i.Id == id)
            .Select(i => new InvoiceDetailDto
            {
                Id              = i.Id,
                PatientIdFhir   = i.PatientIdFhir,
                PatientDisplay  = i.PatientDisplay,
                TotalOriginal   = i.TotalOriginal,
                InvoiceDiscount = i.InvoiceDiscount,
                AdjustmentTotal = i.AdjustmentTotal,
                FinalTotal      = i.FinalTotal,
                AmountPaid      = i.AmountPaid,
                AmountDue       = i.AmountDue,
                Status          = i.Status,
                InvoiceType     = i.InvoiceType,
                PaymentMethod   = i.PaymentMethod,
                SerieId         = i.SerieId,
                SerieName       = i.Serie!.Name,
                Number          = i.Number,
                CreatedDate     = i.CreatedDate,
                CreatedById     = i.CreatedById,
                ParentInvoiceId = i.ParentInvoiceId,
                ParentInvoiceNumber = i.ParentInvoice != null ? i.ParentInvoice.Number.ToString() : null,
                Items = i.Items.Select(x => new InvoiceItemDetailDto
                {
                    Id          = x.Id,
                    Description = x.Description,
                    Quantity    = x.Quantity,
                    UnitPrice   = x.UnitPrice,
                    TotalAmount = x.TotalAmount
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (invoice == null)
            return ResponseHelper.Fail<InvoiceDetailDto?>(404, "La factura no existe.");
 
        if (!parameters.IncludeNotes || invoice.ParentInvoiceId != null)
            return ResponseHelper.Success(200, "Factura encontrada.", invoice);

        // Query 2: Summary (SIEMPRE completo)
        var summary = await _dbContext.Invoices
            .AsNoTracking()
            .Where(n => n.ParentInvoiceId == id)
            .GroupBy(n => 1)
            .Select(g => new
            {
                TotalCredit  = g.Where(n => n.InvoiceType == InvoiceType.CreditNote).Sum(n => (decimal?)n.FinalTotal) ?? 0,
                TotalDebit   = g.Where(n => n.InvoiceType == InvoiceType.DebitNote).Sum(n => (decimal?)n.FinalTotal) ?? 0,
                CountCredit  = g.Count(n => n.InvoiceType == InvoiceType.CreditNote),
                CountDebit   = g.Count(n => n.InvoiceType == InvoiceType.DebitNote),
                TotalCount   = g.Count()
            })
            .FirstOrDefaultAsync();

        // Si no hay notas, retornar
        if (summary == null || summary.TotalCount == 0)
            return ResponseHelper.Success(200, "Factura encontrada.", invoice);

        // Query 3: Children paginados
        var skip = (parameters.NotesPage - 1) * parameters.NotesPageSize;
        invoice.Children = await _dbContext.Invoices
            .AsNoTracking()
            .Where(n => n.ParentInvoiceId == id)
            .OrderByDescending(n => n.CreatedDate)
            .Skip(skip)
            .Take(parameters.NotesPageSize)
            .Select(n => new InvoiceChildDto
            {
                Id          = n.Id,
                InvoiceType = n.InvoiceType,
                Status      = n.Status,
                FinalTotal  = n.FinalTotal,
                CreatedDate = n.CreatedDate,
                Number      = n.Number,
                SerieId     = n.SerieId
            })
            .ToListAsync();

        // Asignar summary con paginación
        invoice.NotesSummary = new InvoiceNotesSummaryDto
        {
            TotalCreditNotes = summary.TotalCredit,
            TotalDebitNotes  = summary.TotalDebit,
            CountCredit      = summary.CountCredit,
            CountDebit       = summary.CountDebit,
            TotalNotes       = summary.TotalCount,
            CurrentPage      = parameters.NotesPage,
            PageSize         = parameters.NotesPageSize,
            TotalPages       = (int)Math.Ceiling(summary.TotalCount / (double)parameters.NotesPageSize)
        };
 
        return ResponseHelper.Success(200, "Factura encontrada.", invoice);
    }


    // =====================================================================
    // LISTAR FACTURAS
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
 
        if (filter.DateFrom is not null && filter.DateTo is not null && filter.DateFrom > filter.DateTo)
            return ResponseHelper.Fail<PagedResultDto<InvoiceGetDto>>(400,
                "La fecha inicial no puede ser mayor a la fecha final.");
 
        if (filter.MinTotal is not null && filter.MaxTotal is not null && filter.MinTotal > filter.MaxTotal)
            return ResponseHelper.Fail<PagedResultDto<InvoiceGetDto>>(400,
                "El total mínimo no puede ser mayor al total máximo.");

        // ================================
        // QUERY BASE
        // ================================
        var query = _dbContext.Invoices.AsNoTracking().AsQueryable();
 
       

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
    
    // =====================================================================
    // CANCELAR FACTURA
    // =====================================================================


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
            return ResponseHelper.Fail<InvoiceDetailDto>(400,
                "No se puede cancelar: la factura tiene notas de crédito o débito asociadas.");
        // Factura hija (nota) no debe cancelarse aquí
        if (invoice.ParentInvoiceId != null)
            return ResponseHelper.Fail<InvoiceDetailDto>(400,
                "Esta factura es una nota de ajuste. Use el proceso de anulación de notas.");
        
        if (invoice.Status == InvoiceStatus.Paid && invoice.InvoiceType != InvoiceType.Exempt)
            return ResponseHelper.Fail<InvoiceDetailDto>(400,
                "La factura ya fue pagada. Requiere nota de crédito para revertirla.");
        // ================================
        // Aplicar cancelación
        // ================================
        
        invoice.Status        = InvoiceStatus.Cancelled;
        
        // Limpieza operativa 
        invoice.AmountPaid    = 0;
        invoice.AmountDue     = 0;
        invoice.UpdatedById   = _userContextService.GetUserId();
        invoice.UpdatedDate   = DateTime.UtcNow;
 
        await _dbContext.SaveChangesAsync();
        
        return ResponseHelper.Success(200, "Factura cancelada correctamente.", InvoiceExtensions.MapToDetail(invoice));
    }

    // =====================================================================
    // MARCAR COMO PAGADA
    // =====================================================================
    // TODO :  REVISAR SI EXISTE ALGUN PROBLEMA AL INTENTAR PAGAR CON DESCUENTOS APLICADOS DESDE EL INICIO.
    
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
                $"El pago {amountPaid} excede el saldo pendiente. Saldo actual: {invoice.AmountDue}.");
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
        return ResponseHelper.Success(200, "Factura pagada correctamente.",InvoiceExtensions.MapToDetail(invoice));
    }

    
    // =====================================================================
    // CREAR NOTA DE CRÉDITO / DÉBITO
    // =====================================================================
    // TODO :
    // Rrvisar y explicar el funcionamiento de las notas, por que tendria que tener items???
    // El credito o debito, debe tener una razon del por que, pero no necesaria mente exigir items hijos. Esto a menos que se le agregen por que no se agrego el servicio y se cobro
    // Sin embargo se maneja solo 1 factura pro servicio asi que no deveria [por el monento]

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

        // No permitir notas sobre facturas canceladas
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
                return ResponseHelper.Fail<InvoiceDetailDto>(400, "La cantidad de cada ítem debe ser mayor a 0.");
 
            if (item.UnitPrice < 0)
                return ResponseHelper.Fail<InvoiceDetailDto>(400, "El precio unitario no puede ser negativo.");
        }
        // ============================
        // RESOLVER SERVICIOS
        // ============================
 
        var fhirIds = dto.Items.Select(i => i.ServiceId).Distinct().ToList();
        var services = await _dbContext.HealthServices
            .AsNoTracking()
            .Where(s => fhirIds.Contains(s.HealthServiceFhirId))
            .ToListAsync();
 
        if (services.Count != fhirIds.Count)
            return ResponseHelper.Fail<InvoiceDetailDto>(400, "Uno o más servicios no existen en SIGREF.");
 
        var serviceMap = services.ToDictionary(s => s.HealthServiceFhirId, s => s);
        // ============================
        // CONSTRUIR NOTA
        // Las notas NO tienen InvoiceDiscount (descuento global).
        // Su FinalTotal es SIEMPRE POSITIVO — el signo lo determina InvoiceType.
        // El padre luego ajusta su AdjustmentTotal con el signo correcto.
        // ============================
        var userId = _userContextService.GetUserId();
 
        var note = new InvoiceEntity
        {
            ParentInvoiceId = parentInvoiceId,
            PatientIdFhir   = parent.PatientIdFhir,
            PatientDisplay  = parent.PatientDisplay,
 
            InvoiceType    = noteType,
            PaymentMethod  = PaymentMethodType.Cash,
 
            SerieId        = dto.SerieId,
            Number         = dto.SerieNumber,
 
            InvoiceDiscount = 0,    // las notas no tienen descuento global
            AdjustmentTotal = 0,
 
            CreatedById  = userId,
            CreatedDate  = DateTime.UtcNow
        };

        // Items
        // El servidor calcula TotalAmount por ítem
        foreach (var item in dto.Items)
        {
            var service   = serviceMap[item.ServiceId];
            var lineTotal = item.Quantity * item.UnitPrice;
 
            note.Items.Add(new InvoiceItemEntity
            {
                ServiceId   = service.Id,
                Description = item.NameService,
                Quantity    = item.Quantity,
                UnitPrice   = item.UnitPrice,
                TotalAmount = lineTotal,
 
                CreatedById = userId,
                CreatedDate = DateTime.UtcNow
            });
        }

        // Totales de la nota
        // TotalOriginal y FinalTotal son SIEMPRE positivos en la nota.
        // RecalculateInvoiceTotalsAsync aplica el signo en el padre.
        note.TotalOriginal = note.Items.Sum(x => x.TotalAmount);
        note.FinalTotal    = note.TotalOriginal;
 
        // Notas siempre "pagadas" (no tienen saldo pendiente propio)
        note.AmountPaid = 0;
        note.AmountDue  = 0;
        note.Status     = InvoiceStatus.Paid;

        // ============================
        // GUARDAR NOTA Y RECALCULAR PADRE
        // ============================
 
        _dbContext.Invoices.Add(note);
        await _dbContext.SaveChangesAsync();
 
        await RecalculateInvoiceTotalsAsync(parentInvoiceId);
 
        return ResponseHelper.Success(201, "Nota creada correctamente.", InvoiceExtensions.MapToDetail(note));
    }

    // =====================================================================
    // HAS CHILD NOTES
    // =====================================================================
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
                Id          = x.Id,
                Number      = x.Number,
                SerieId     = x.SerieId,
                SerieName   = x.Serie!.Name,
                Status      = x.Status,
                InvoiceType = x.InvoiceType
            })
            .OrderByDescending(x => x.Number)
            .ToListAsync();

        // ================================
        //  Respuestas limpias
        // ================================
        return childNotes.Count == 0
            ? ResponseHelper.Success(200, "La factura no tiene notas asociadas.", new List<MinimalInvoiceDto>())
            : ResponseHelper.Success(200, "La factura tiene notas asociadas.", childNotes);
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