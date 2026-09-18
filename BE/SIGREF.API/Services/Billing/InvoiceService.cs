using Hl7.Fhir.Rest;
using Microsoft.EntityFrameworkCore;
using SIGREF.API.Dtos.Invoice;
using SIGREF.API.Extensions;
using SIGREF.API.Middleware;
using SIGREF.API.Services.Cashier;
using SIGREF.Common.Constants;
using SIGREF.Common.Dtos;
using SIGREF.Common.Exceptions;
using SIGREF.Common.Types;
using SIGREF.Core.Entity.Billing;
using SIGREF.Infrastructure.Keycloak.Interfaces;
using SIGREF.Infrastructure.Persistence;

namespace SIGREF.API.Services.Billing;

// NOTA: LOS INVOICES SE ENTIENDEN COMO ORDENES DE DONACION
public class InvoiceService : IInvoiceService
{
    private readonly ICashierSessionService _cashierSessionService;
    private readonly SIGREFContext _dbContext;
    private readonly IUserContextService _userContextService;

    public InvoiceService(SIGREFContext dbContext, IUserContextService userContextService,
        ICashierSessionService cashierSessionService)
    {
        _dbContext = dbContext;
        _userContextService = userContextService;
        _cashierSessionService = cashierSessionService;
    }

    // =====================================================================
    // CREAR FACTURA
    // =====================================================================

    public async Task<InvoiceDetailDto> CreateInvoiceAsync(InvoiceCreateDto dto)
    {
        try
        {
            // ============================
            // VALIDACIONES GENERALES
            // ============================
            if (dto.ParentInvoiceId != null)
                throw new ValidationException("INVOICE_USE_NOTE_METHOD", new Dictionary<string, object>
                {
                    { "ParentInvoiceId", dto.ParentInvoiceId }
                });

            if (dto.InvoiceType != InvoiceType.Emergency && string.IsNullOrWhiteSpace(dto.PatientIdFhir))
                throw new ValidationException("INVOICE_PATIENT_REQUIRED", new Dictionary<string, object>
                {
                    { "InvoiceType", dto.InvoiceType }
                });

            var userId = _userContextService.GetUserId();
            var roles = _userContextService.GetUserRoles();
            Guid? activeSessionId = null;

            try
            {
                var session = await _cashierSessionService.GetActiveSessionByUserAsync(userId);
                activeSessionId = session.Id;
            }
            catch (NotFoundException)
            {
                // No hay sesión activa
            }

            if (roles.Contains(RolesConstants.cashier) && activeSessionId == null)
                throw new BusinessRuleException("INVOICE_NO_ACTIVE_SESSION", new Dictionary<string, object>
                {
                    { "UserId", userId }
                });

            if (dto.Items == null || dto.Items.Count == 0)
                throw new ValidationException("INVOICE_ITEMS_REQUIRED");

            // ============================
            // VALIDAR ITEMS (sincrono en memoria)
            // ============================
            foreach (var item in dto.Items)
            {
                if (item.Quantity <= 0)
                    throw new ValidationException("INVOICE_ITEM_INVALID_QUANTITY", new Dictionary<string, object>
                    {
                        { "ItemName", item.NameService },
                        { "Quantity", item.Quantity }
                    });

                if (item.UnitPrice < 0)
                    throw new ValidationException("INVOICE_ITEM_INVALID_PRICE", new Dictionary<string, object>
                    {
                        { "ItemName", item.NameService },
                        { "UnitPrice", item.UnitPrice }
                    });
            }

            // ============================
            // VALIDAR DESCUENTO GLOBAL
            // ============================
            var invoiceDiscount = dto.InvoiceDiscount ?? decimal.Zero;
            if (invoiceDiscount < 0)
                throw new ValidationException("INVOICE_NEGATIVE_DISCOUNT", new Dictionary<string, object>
                {
                    { "Discount", invoiceDiscount }
                });

            // ============================
            // PARALELIZAR: SERIE + SERVICIOS (Recomendación #1)
            // ============================
            var fhirIds = new HashSet<string>(dto.Items.Select(i => i.ServiceId));
            if (!string.IsNullOrEmpty(dto.SingleServiceFhirId))
                fhirIds.Add(dto.SingleServiceFhirId);

            var serieTask = _dbContext.InvoiceSeries
                .FirstOrDefaultAsync(x => x.Id == dto.SerieId && x.IsActive);

            var servicesTask = _dbContext.HealthServices
                .AsNoTracking()
                .Where(s => fhirIds.Contains(s.HealthServiceFhirId))
                .ToListAsync();

            await Task.WhenAll(serieTask, servicesTask);

            var serie = await serieTask;
            var services = await servicesTask;

            // ============================
            // VALIDAR SERIE
            // ============================
            if (serie == null)
                throw new NotFoundException("INVOICE_SERIE_NOT_FOUND", new Dictionary<string, object>
                {
                    { "SerieId", dto.SerieId }
                });

            if (dto.SerieNumber < serie.StartNumber || dto.SerieNumber > serie.EndNumber)
                throw new ValidationException("INVOICE_SERIE_NUMBER_OUT_OF_RANGE", new Dictionary<string, object>
                {
                    { "SerieNumber", dto.SerieNumber },
                    { "StartNumber", serie.StartNumber },
                    { "EndNumber", serie.EndNumber }
                });

            // ============================
            // VALIDAR SERVICIOS
            // ============================
            if (services.Count != fhirIds.Count)
                throw new NotFoundException("INVOICE_SERVICES_NOT_FOUND", new Dictionary<string, object>
                {
                    { "RequestedServices", fhirIds },
                    { "FoundServices", services.Select(s => s.HealthServiceFhirId) }
                });

            var serviceMap = services.ToDictionary(s => s.HealthServiceFhirId, s => s);

            // ============================
            // CONSTRUIR ENTIDAD
            // ============================
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
                Number = dto.SerieNumber,
                CashierSessionId = activeSessionId,
                InvoiceDiscount = dto.InvoiceDiscount ?? decimal.Zero,
                CreatedById = userId,
                CreatedDate = DateTime.UtcNow
            };

            // ============================
            // AGREGAR ITEMS Y CALCULAR TOTALES
            // ============================
            foreach (var item in dto.Items)
            {
                var service = serviceMap[item.ServiceId];
                var lineTotal = item.Quantity * item.UnitPrice;

                invoice.Items.Add(new InvoiceItemEntity
                {
                    ServiceId = service.Id,
                    Description = item.NameService,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalAmount = lineTotal,
                    CreatedById = userId,
                    CreatedDate = DateTime.UtcNow
                });
            }

            // ============================
            // CALCULAR TOTALES DE LA FACTURA
            // ============================
            invoice.TotalOriginal = invoice.Items.Sum(i => i.TotalAmount);

            if (dto.InvoiceDiscount > invoice.TotalOriginal)
                throw new ValidationException("INVOICE_DISCOUNT_EXCEEDS_TOTAL", new Dictionary<string, object>
                {
                    { "Discount", dto.InvoiceDiscount },
                    { "TotalOriginal", invoice.TotalOriginal }
                });

            invoice.InvoiceDiscount = invoiceDiscount;
            invoice.AdjustmentTotal = 0;
            invoice.FinalTotal = invoice.TotalOriginal - invoice.InvoiceDiscount;

            // ============================
            // COMPORTAMIENTO POR TIPO
            // ============================
            ApplyInvoiceTypeBehavior(invoice, dto);

            // ============================
            // GUARDAR
            // ============================
            _dbContext.Invoices.Add(invoice);
            await _dbContext.SaveChangesAsync();

            return InvoiceExtensions.MapToDetail(invoice);
        }
        catch (FhirOperationException fhirEx)
        {
            throw FhirExceptionMapper.Map(fhirEx, "InvoiceCreate", nameof(CreateInvoiceAsync));
        }
        catch (DbUpdateException dbEx)
        {
            throw new ExternalServiceException("DB_CREATE_INVOICE_ERROR", 502, new Dictionary<string, object>
            {
                { "OriginalException", dbEx.Message },
                { "Operation", nameof(CreateInvoiceAsync) }
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
            throw new ExternalServiceException("INTERNAL_INVOICE_ERROR", 500, new Dictionary<string, object>
            {
                { "OriginalException", ex.Message },
                { "Operation", nameof(CreateInvoiceAsync) }
            });
        }
    }

    // =====================================================================
    // OBTENER FACTURA POR ID
    // =====================================================================

    public async Task<InvoiceDetailDto> GetInvoiceByIdAsync(Guid id, GetInvoiceParameters parameters)
    {
        try
        {
            if (parameters.NotesPage < 1) parameters.NotesPage = 1;
            if (parameters.NotesPageSize < 1) parameters.NotesPageSize = 20;
            if (parameters.NotesPageSize > 100) parameters.NotesPageSize = 100;

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
                    InvoiceDiscount = i.InvoiceDiscount,
                    AdjustmentTotal = i.AdjustmentTotal,
                    FinalTotal = i.FinalTotal,
                    AmountPaid = i.AmountPaid,
                    AmountDue = i.AmountDue,
                    Status = i.Status,
                    InvoiceType = i.InvoiceType,
                    PaymentMethod = i.PaymentMethod,
                    SerieId = i.SerieId,
                    SerieName = i.Serie!.Name,
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
                        TotalAmount = x.TotalAmount
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (invoice == null)
                throw new NotFoundException("INVOICE_NOT_FOUND", new Dictionary<string, object>
                {
                    { "InvoiceId", id }
                });

            if (!parameters.IncludeNotes || invoice.ParentInvoiceId != null)
                return invoice;

            // Query 2: Summary
            var summary = await _dbContext.Invoices
                .AsNoTracking()
                .Where(n => n.ParentInvoiceId == id)
                .GroupBy(n => 1)
                .Select(g => new
                {
                    TotalCredit =
                        g.Where(n => n.InvoiceType == InvoiceType.CreditNote).Sum(n => (decimal?)n.FinalTotal) ?? 0,
                    TotalDebit =
                        g.Where(n => n.InvoiceType == InvoiceType.DebitNote).Sum(n => (decimal?)n.FinalTotal) ?? 0,
                    CountCredit = g.Count(n => n.InvoiceType == InvoiceType.CreditNote),
                    CountDebit = g.Count(n => n.InvoiceType == InvoiceType.DebitNote),
                    TotalCount = g.Count()
                })
                .FirstOrDefaultAsync();

            if (summary == null || summary.TotalCount == 0)
                return invoice;

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
                    Id = n.Id,
                    InvoiceType = n.InvoiceType,
                    Status = n.Status,
                    FinalTotal = n.FinalTotal,
                    CreatedDate = n.CreatedDate,
                    Number = n.Number,
                    SerieId = n.SerieId
                })
                .ToListAsync();

            invoice.NotesSummary = new InvoiceNotesSummaryDto
            {
                TotalCreditNotes = summary.TotalCredit,
                TotalDebitNotes = summary.TotalDebit,
                CountCredit = summary.CountCredit,
                CountDebit = summary.CountDebit,
                TotalNotes = summary.TotalCount,
                CurrentPage = parameters.NotesPage,
                PageSize = parameters.NotesPageSize,
                TotalPages = (int)Math.Ceiling(summary.TotalCount / (double)parameters.NotesPageSize)
            };

            return invoice;
        }
        catch (DbUpdateException dbEx)
        {
            throw new ExternalServiceException("DB_GET_INVOICE_ERROR", 502, new Dictionary<string, object>
            {
                { "OriginalException", dbEx.Message },
                { "Operation", nameof(GetInvoiceByIdAsync) },
                { "InvoiceId", id }
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
            throw new ExternalServiceException("INTERNAL_INVOICE_ERROR", 500, new Dictionary<string, object>
            {
                { "OriginalException", ex.Message },
                { "Operation", nameof(GetInvoiceByIdAsync) },
                { "InvoiceId", id }
            });
        }
    }

    // =====================================================================
    // LISTAR FACTURAS
    // =====================================================================

    public async Task<PagedResultDto<InvoiceGetDto>> GetInvoicesAsync(InvoiceFilterDto filter)
    {
        try
        {
            // ================================
            // PAGINACIÓN
            // ================================
            filter.PageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
            filter.PageSize = filter.PageSize < 1 ? 10 : filter.PageSize;
            filter.PageSize = filter.PageSize > 100 ? 100 : filter.PageSize;

            // ================================
            // VALIDACIONES LÓGICAS
            // ================================
            if (filter.OnlyInvoices == true && filter.OnlyNotes == true)
                throw new ValidationException("INVOICE_INVALID_FILTER_COMBINATION");

            if (filter.DateFrom is not null && filter.DateTo is not null && filter.DateFrom > filter.DateTo)
                throw new ValidationException("INVOICE_INVALID_DATE_RANGE", new Dictionary<string, object>
                {
                    { "DateFrom", filter.DateFrom.Value },
                    { "DateTo", filter.DateTo.Value }
                });

            if (filter.MinTotal is not null && filter.MaxTotal is not null && filter.MinTotal > filter.MaxTotal)
                throw new ValidationException("INVOICE_INVALID_TOTAL_RANGE", new Dictionary<string, object>
                {
                    { "MinTotal", filter.MinTotal.Value },
                    { "MaxTotal", filter.MaxTotal.Value }
                });

            // ================================
            // QUERY BASE
            // ================================
            var query = _dbContext.Invoices.AsNoTracking().AsQueryable();

            // ================================
            // APLICAR FILTROS
            // ================================
            if (filter.OnlyInvoices == true)
                query = query.Where(i =>
                    i.InvoiceType != InvoiceType.CreditNote &&
                    i.InvoiceType != InvoiceType.DebitNote);

            if (filter.OnlyNotes == true)
                query = query.Where(i =>
                    i.InvoiceType == InvoiceType.CreditNote ||
                    i.InvoiceType == InvoiceType.DebitNote);

            if (filter.InvoiceType is not null)
                query = query.Where(i => i.InvoiceType == filter.InvoiceType);

            if (filter.Status is not null)
                query = query.Where(i => i.Status == filter.Status);

            if (filter.SerieId is not null)
                query = query.Where(i => i.SerieId == filter.SerieId);

            if (filter.Number is not null)
                query = query.Where(i => i.Number == filter.Number);

            if (!string.IsNullOrWhiteSpace(filter.PatientIdFhir))
                query = query.Where(i => i.PatientIdFhir == filter.PatientIdFhir);

            // Recomendación #2: Usar EF.Functions.ILike para búsquedas case-insensitive en PostgreSQL
            if (!string.IsNullOrWhiteSpace(filter.PatientDisplay))
            {
                var searchP = filter.PatientDisplay.Trim();
                query = query.Where(i =>
                    i.PatientDisplay != null &&
                    EF.Functions.ILike(i.PatientDisplay, $"%{searchP}%"));
            }

            if (filter.CashierSessionId is not null)
                query = query.Where(i => i.CashierSessionId == filter.CashierSessionId);

            if (filter.DateFrom is not null)
            {
                var from = filter.DateFrom.Value.Date;
                query = query.Where(i => i.CreatedDate >= from);
            }

            if (filter.DateTo is not null)
            {
                var to = filter.DateTo.Value.Date.AddDays(1);
                query = query.Where(i => i.CreatedDate < to);
            }

            if (filter.MinTotal is not null)
                query = query.Where(i => i.FinalTotal >= filter.MinTotal);

            if (filter.MaxTotal is not null)
                query = query.Where(i => i.FinalTotal <= filter.MaxTotal);

            // Recomendación #2: EF.Functions.ILike para búsqueda global
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.Trim();
                query = query.Where(i =>
                    i.Number.ToString().Contains(search) ||
                    (i.PatientDisplay != null && EF.Functions.ILike(i.PatientDisplay, $"%{search}%")) ||
                    (i.Serie != null && EF.Functions.ILike(i.Serie.Name, $"%{search}%"))
                );
            }

            // ================================
            // CONTAR RESULTADOS
            // ================================
            var totalItems = await query.CountAsync();
            var totalPages = totalItems > 0
                ? (int)Math.Ceiling(totalItems / (double)filter.PageSize)
                : 0;

            // ================================
            // ORDENAMIENTO
            // ================================
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

            return new PagedResultDto<InvoiceGetDto>
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
            };
        }
        catch (DbUpdateException dbEx)
        {
            throw new ExternalServiceException("DB_FILTER_INVOICES_ERROR", 502, new Dictionary<string, object>
            {
                { "OriginalException", dbEx.Message },
                { "Operation", nameof(GetInvoicesAsync) }
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
            throw new ExternalServiceException("INTERNAL_INVOICE_ERROR", 500, new Dictionary<string, object>
            {
                { "OriginalException", ex.Message },
                { "Operation", nameof(GetInvoicesAsync) }
            });
        }
    }

    // =====================================================================
    // CANCELAR FACTURA
    // =====================================================================

    public async Task<InvoiceDetailDto> CancelInvoiceAsync(Guid id)
    {
        try
        {
            var invoice = await _dbContext.Invoices
                .Include(i => i.Items)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
                throw new NotFoundException("INVOICE_NOT_FOUND", new Dictionary<string, object>
                {
                    { "InvoiceId", id }
                });

            if (invoice.Status == InvoiceStatus.Cancelled)
                throw new BusinessRuleException("INVOICE_ALREADY_CANCELLED", new Dictionary<string, object>
                {
                    { "InvoiceId", id }
                });

            var hasNotes = await _dbContext.Invoices
                .AsNoTracking()
                .AnyAsync(x => x.ParentInvoiceId == id);

            if (hasNotes)
                throw new BusinessRuleException("INVOICE_HAS_CHILD_NOTES", new Dictionary<string, object>
                {
                    { "InvoiceId", id }
                });

            if (invoice.ParentInvoiceId != null)
                throw new BusinessRuleException("INVOICE_IS_NOTE", new Dictionary<string, object>
                {
                    { "InvoiceId", id }
                });

            if (invoice.Status == InvoiceStatus.Paid && invoice.InvoiceType != InvoiceType.Exempt)
                throw new BusinessRuleException("INVOICE_PAID_REQUIRES_CREDIT_NOTE", new Dictionary<string, object>
                {
                    { "InvoiceId", id },
                    { "Status", invoice.Status },
                    { "InvoiceType", invoice.InvoiceType }
                });

            invoice.Status = InvoiceStatus.Cancelled;
            invoice.AmountPaid = 0;
            invoice.AmountDue = 0;
            invoice.UpdatedById = _userContextService.GetUserId();
            invoice.UpdatedDate = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return InvoiceExtensions.MapToDetail(invoice);
        }
        catch (DbUpdateException dbEx)
        {
            throw new ExternalServiceException("DB_CANCEL_INVOICE_ERROR", 502, new Dictionary<string, object>
            {
                { "OriginalException", dbEx.Message },
                { "Operation", nameof(CancelInvoiceAsync) },
                { "InvoiceId", id }
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
            throw new ExternalServiceException("INTERNAL_INVOICE_ERROR", 500, new Dictionary<string, object>
            {
                { "OriginalException", ex.Message },
                { "Operation", nameof(CancelInvoiceAsync) },
                { "InvoiceId", id }
            });
        }
    }

    // =====================================================================
    // MARCAR COMO PAGADA
    // =====================================================================

    public async Task<InvoiceDetailDto> MarkAsPaidAsync(Guid id, decimal amountPaid)
    {
        try
        {
            if (amountPaid <= 0)
                throw new ValidationException("INVOICE_INVALID_PAYMENT_AMOUNT", new Dictionary<string, object>
                {
                    { "AmountPaid", amountPaid }
                });

            var invoice = await _dbContext.Invoices
                .Include(i => i.Items)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
                throw new NotFoundException("INVOICE_NOT_FOUND", new Dictionary<string, object>
                {
                    { "InvoiceId", id }
                });

            if (invoice.InvoiceType == InvoiceType.CreditNote ||
                invoice.InvoiceType == InvoiceType.DebitNote)
                throw new BusinessRuleException("INVOICE_NOTES_NOT_PAYABLE", new Dictionary<string, object>
                {
                    { "InvoiceId", id },
                    { "InvoiceType", invoice.InvoiceType }
                });

            if (invoice.Status == InvoiceStatus.Cancelled)
                throw new BusinessRuleException("INVOICE_CANCELLED_NOT_PAYABLE", new Dictionary<string, object>
                {
                    { "InvoiceId", id }
                });

            if (invoice.Status == InvoiceStatus.Paid && invoice.AmountDue == 0)
                throw new BusinessRuleException("INVOICE_ALREADY_PAID", new Dictionary<string, object>
                {
                    { "InvoiceId", id }
                });

            if (amountPaid > invoice.AmountDue)
                throw new ValidationException("INVOICE_PAYMENT_EXCEEDS_DUE", new Dictionary<string, object>
                {
                    { "AmountPaid", amountPaid },
                    { "AmountDue", invoice.AmountDue }
                });

            invoice.AmountPaid += amountPaid;
            invoice.AmountDue = invoice.FinalTotal - invoice.AmountPaid;
            invoice.Status = invoice.AmountDue == 0
                ? InvoiceStatus.Paid
                : InvoiceStatus.Created;
            invoice.UpdatedById = _userContextService.GetUserId();
            invoice.UpdatedDate = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return InvoiceExtensions.MapToDetail(invoice);
        }
        catch (DbUpdateException dbEx)
        {
            throw new ExternalServiceException("DB_PAY_INVOICE_ERROR", 502, new Dictionary<string, object>
            {
                { "OriginalException", dbEx.Message },
                { "Operation", nameof(MarkAsPaidAsync) },
                { "InvoiceId", id }
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
            throw new ExternalServiceException("INTERNAL_INVOICE_ERROR", 500, new Dictionary<string, object>
            {
                { "OriginalException", ex.Message },
                { "Operation", nameof(MarkAsPaidAsync) },
                { "InvoiceId", id }
            });
        }
    }

    // =====================================================================
    // CREAR NOTA DE CRÉDITO / DÉBITO
    // =====================================================================

    public async Task<InvoiceDetailDto> CreateNoteAsync(Guid parentInvoiceId, InvoiceCreateDto dto,
        InvoiceType noteType)
    {
        // Recomendación #3: Transacción para garantizar consistencia entre nota y recálculo
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            if (noteType != InvoiceType.CreditNote && noteType != InvoiceType.DebitNote)
                throw new ValidationException("INVOICE_INVALID_NOTE_TYPE", new Dictionary<string, object>
                {
                    { "NoteType", noteType }
                });

            var parent = await _dbContext.Invoices
                .Include(i => i.Items)
                .FirstOrDefaultAsync(i => i.Id == parentInvoiceId);

            if (parent == null)
                throw new NotFoundException("INVOICE_PARENT_NOT_FOUND", new Dictionary<string, object>
                {
                    { "ParentInvoiceId", parentInvoiceId }
                });

            if (parent.ParentInvoiceId != null)
                throw new BusinessRuleException("INVOICE_NOTES_ON_NOTES_NOT_ALLOWED", new Dictionary<string, object>
                {
                    { "ParentInvoiceId", parentInvoiceId }
                });

            if (parent.Status == InvoiceStatus.Cancelled)
                throw new BusinessRuleException("INVOICE_CANCELLED_NO_NOTES", new Dictionary<string, object>
                {
                    { "ParentInvoiceId", parentInvoiceId }
                });

            if (dto.Items == null || dto.Items.Count == 0)
                throw new ValidationException("INVOICE_NOTE_ITEMS_REQUIRED");
            // Paralelizar: Serie + Servicios (Recomendación #1)
            var fhirIds = dto.Items.Select(i => i.ServiceId).Distinct().ToList();

            var serieTask = _dbContext.InvoiceSeries
                .FirstOrDefaultAsync(x => x.Id == dto.SerieId && x.IsActive);

            var servicesTask = _dbContext.HealthServices
                .AsNoTracking()
                .Where(s => fhirIds.Contains(s.HealthServiceFhirId))
                .ToListAsync();

            await Task.WhenAll(serieTask, servicesTask);

            var serie = await serieTask;
            var services = await servicesTask;

            if (serie == null)
                throw new NotFoundException("INVOICE_SERIE_NOT_FOUND", new Dictionary<string, object>
                {
                    { "SerieId", dto.SerieId }
                });

            if (dto.SerieNumber < serie.StartNumber || dto.SerieNumber > serie.EndNumber)
                throw new ValidationException("INVOICE_SERIE_NUMBER_OUT_OF_RANGE", new Dictionary<string, object>
                {
                    { "SerieNumber", dto.SerieNumber },
                    { "StartNumber", serie.StartNumber },
                    { "EndNumber", serie.EndNumber }
                });

            foreach (var item in dto.Items)
            {
                if (item.Quantity <= 0)
                    throw new ValidationException("INVOICE_ITEM_INVALID_QUANTITY", new Dictionary<string, object>
                    {
                        { "ItemName", item.NameService },
                        { "Quantity", item.Quantity }
                    });

                if (item.UnitPrice < 0)
                    throw new ValidationException("INVOICE_ITEM_INVALID_PRICE", new Dictionary<string, object>
                    {
                        { "ItemName", item.NameService },
                        { "UnitPrice", item.UnitPrice }
                    });
            }

            if (services.Count != fhirIds.Count)
                throw new NotFoundException("INVOICE_SERVICES_NOT_FOUND", new Dictionary<string, object>
                {
                    { "RequestedServices", fhirIds },
                    { "FoundServices", services.Select(s => s.HealthServiceFhirId) }
                });

            var serviceMap = services.ToDictionary(s => s.HealthServiceFhirId, s => s);

            // ============================
            // CONSTRUIR NOTA
            // ============================
            var userId = _userContextService.GetUserId();

            var note = new InvoiceEntity
            {
                ParentInvoiceId = parentInvoiceId,
                PatientIdFhir = parent.PatientIdFhir,
                PatientDisplay = parent.PatientDisplay,
                InvoiceType = noteType,
                PaymentMethod = PaymentMethodType.Cash,
                SerieId = dto.SerieId,
                Number = dto.SerieNumber,
                InvoiceDiscount = 0,
                AdjustmentTotal = 0,
                CreatedById = userId,
                CreatedDate = DateTime.UtcNow
            };

            foreach (var item in dto.Items)
            {
                var service = serviceMap[item.ServiceId];
                var lineTotal = item.Quantity * item.UnitPrice;

                note.Items.Add(new InvoiceItemEntity
                {
                    ServiceId = service.Id,
                    Description = item.NameService,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalAmount = lineTotal,
                    CreatedById = userId,
                    CreatedDate = DateTime.UtcNow
                });
            }

            note.TotalOriginal = note.Items.Sum(x => x.TotalAmount);
            note.FinalTotal = note.TotalOriginal;
            note.AmountPaid = 0;
            note.AmountDue = 0;
            note.Status = InvoiceStatus.Paid;

            _dbContext.Invoices.Add(note);
            await _dbContext.SaveChangesAsync();

            // Recalcular padre dentro de la misma transacción
            await RecalculateInvoiceTotalsAsync(parentInvoiceId);

            // Commit de la transacción
            await transaction.CommitAsync();

            return InvoiceExtensions.MapToDetail(note);
        }
        catch (DbUpdateException dbEx)
        {
            await transaction.RollbackAsync();
            throw new ExternalServiceException("DB_CREATE_NOTE_ERROR", 502, new Dictionary<string, object>
            {
                { "OriginalException", dbEx.Message },
                { "Operation", nameof(CreateNoteAsync) },
                { "ParentInvoiceId", parentInvoiceId }
            });
        }
        catch (OperationCanceledException)
        {
            await transaction.RollbackAsync();
            throw;
        }
        catch (AppException)
        {
            await transaction.RollbackAsync();
            throw;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new ExternalServiceException("INTERNAL_INVOICE_ERROR", 500, new Dictionary<string, object>
            {
                { "OriginalException", ex.Message },
                { "Operation", nameof(CreateNoteAsync) },
                { "ParentInvoiceId", parentInvoiceId }
            });
        }
    }

    // =====================================================================
    // HAS CHILD NOTES
    // =====================================================================

    public async Task<List<MinimalInvoiceDto>> HasChildNotesAsync(Guid invoiceId)
    {
        try
        {
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
                throw new NotFoundException("INVOICE_NOT_FOUND", new Dictionary<string, object>
                {
                    { "InvoiceId", invoiceId }
                });

            if (invoiceData.InvoiceType == InvoiceType.CreditNote ||
                invoiceData.InvoiceType == InvoiceType.DebitNote)
                throw new BusinessRuleException("INVOICE_NOTES_HAVE_NO_CHILDREN", new Dictionary<string, object>
                {
                    { "InvoiceId", invoiceId }
                });

            return await _dbContext.Invoices
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
        }
        catch (DbUpdateException dbEx)
        {
            throw new ExternalServiceException("DB_GET_CHILD_NOTES_ERROR", 502, new Dictionary<string, object>
            {
                { "OriginalException", dbEx.Message },
                { "Operation", nameof(HasChildNotesAsync) },
                { "InvoiceId", invoiceId }
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
            throw new ExternalServiceException("INTERNAL_INVOICE_ERROR", 500, new Dictionary<string, object>
            {
                { "OriginalException", ex.Message },
                { "Operation", nameof(HasChildNotesAsync) },
                { "InvoiceId", invoiceId }
            });
        }
    }

    // =====================================================================
    // RECALCULAR TOTALES
    // =====================================================================

    public async Task RecalculateInvoiceTotalsAsync(Guid invoiceId)
    {
        try
        {
            var invoice = await _dbContext.Invoices
                .FirstOrDefaultAsync(i => i.Id == invoiceId);

            if (invoice == null)
                throw new NotFoundException("INVOICE_NOT_FOUND", new Dictionary<string, object>
                {
                    { "InvoiceId", invoiceId }
                });

            if (invoice.InvoiceType == InvoiceType.CreditNote ||
                invoice.InvoiceType == InvoiceType.DebitNote)
                throw new BusinessRuleException("INVOICE_NOTES_NOT_RECALCULABLE", new Dictionary<string, object>
                {
                    { "InvoiceId", invoiceId }
                });

            var childNotes = await _dbContext.Invoices
                .AsNoTracking()
                .Where(n => n.ParentInvoiceId == invoiceId)
                .Select(n => new
                {
                    n.FinalTotal,
                    n.InvoiceType
                })
                .ToListAsync();

            if (childNotes.Count == 0)
            {
                invoice.AdjustmentTotal = 0;
                invoice.FinalTotal = invoice.TotalOriginal;
                invoice.AmountDue = invoice.FinalTotal - invoice.AmountPaid;
                invoice.Status = invoice.AmountDue == 0
                    ? InvoiceStatus.Paid
                    : InvoiceStatus.Created;
            }
            else
            {
                var totalCredits = childNotes
                    .Where(x => x.InvoiceType == InvoiceType.CreditNote)
                    .Sum(x => x.FinalTotal);

                var totalDebits = childNotes
                    .Where(x => x.InvoiceType == InvoiceType.DebitNote)
                    .Sum(x => x.FinalTotal);

                invoice.AdjustmentTotal = totalCredits + totalDebits;
                invoice.FinalTotal = invoice.TotalOriginal + invoice.AdjustmentTotal;
                invoice.AmountDue = invoice.FinalTotal - invoice.AmountPaid;

                if (invoice.AmountDue == 0)
                    invoice.Status = InvoiceStatus.Paid;
                else if (invoice.AmountDue > 0)
                    invoice.Status = InvoiceStatus.Created;
                else
                    invoice.Status = InvoiceStatus.Refunded;
            }

            invoice.UpdatedById = _userContextService.GetUserId();
            invoice.UpdatedDate = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException dbEx)
        {
            throw new ExternalServiceException("DB_RECALCULATE_ERROR", 502, new Dictionary<string, object>
            {
                { "OriginalException", dbEx.Message },
                { "Operation", nameof(RecalculateInvoiceTotalsAsync) },
                { "InvoiceId", invoiceId }
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
            throw new ExternalServiceException("INTERNAL_INVOICE_ERROR", 500, new Dictionary<string, object>
            {
                { "OriginalException", ex.Message },
                { "Operation", nameof(RecalculateInvoiceTotalsAsync) },
                { "InvoiceId", invoiceId }
            });
        }
    }

    // =====================================================================
    // GET NOTES SUMMARY
    // =====================================================================

    public async Task<InvoiceNotesSummaryDto> GetNotesSummaryAsync(Guid invoiceId)
    {
        try
        {
            var invoice = await _dbContext.Invoices
                .AsNoTracking()
                .Where(i => i.Id == invoiceId)
                .Select(i => new { i.Id, i.InvoiceType })
                .FirstOrDefaultAsync();

            if (invoice == null)
                throw new NotFoundException("INVOICE_NOT_FOUND", new Dictionary<string, object>
                {
                    { "InvoiceId", invoiceId }
                });

            if (invoice.InvoiceType == InvoiceType.CreditNote ||
                invoice.InvoiceType == InvoiceType.DebitNote)
                throw new BusinessRuleException("INVOICE_NOTES_HAVE_NO_SUMMARY", new Dictionary<string, object>
                {
                    { "InvoiceId", invoiceId }
                });

            var notes = await _dbContext.Invoices
                .AsNoTracking()
                .Where(n => n.ParentInvoiceId == invoiceId)
                .Select(n => new
                {
                    n.FinalTotal,
                    n.InvoiceType
                })
                .ToListAsync();

            if (notes.Count == 0)
                return new InvoiceNotesSummaryDto
                {
                    TotalCreditNotes = 0,
                    TotalDebitNotes = 0,
                    CountCredit = 0,
                    CountDebit = 0
                };

            return new InvoiceNotesSummaryDto
            {
                TotalCreditNotes = notes
                    .Where(x => x.InvoiceType == InvoiceType.CreditNote)
                    .Sum(x => x.FinalTotal),
                TotalDebitNotes = notes
                    .Where(x => x.InvoiceType == InvoiceType.DebitNote)
                    .Sum(x => x.FinalTotal),
                CountCredit = notes.Count(x => x.InvoiceType == InvoiceType.CreditNote),
                CountDebit = notes.Count(x => x.InvoiceType == InvoiceType.DebitNote)
            };
        }
        catch (DbUpdateException dbEx)
        {
            throw new ExternalServiceException("DB_NOTES_SUMMARY_ERROR", 502, new Dictionary<string, object>
            {
                { "OriginalException", dbEx.Message },
                { "Operation", nameof(GetNotesSummaryAsync) },
                { "InvoiceId", invoiceId }
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
            throw new ExternalServiceException("INTERNAL_INVOICE_ERROR", 500, new Dictionary<string, object>
            {
                { "OriginalException", ex.Message },
                { "Operation", nameof(GetNotesSummaryAsync) },
                { "InvoiceId", invoiceId }
            });
        }
    }

    // =====================================================================
    // HELPERS PRIVADOS
    // =====================================================================

    private void ApplyInvoiceTypeBehavior(InvoiceEntity invoice, InvoiceCreateDto dto)
    {
        switch (dto.InvoiceType)
        {
            case InvoiceType.Normal:
                invoice.AmountPaid = invoice.FinalTotal;
                invoice.AmountDue = 0;
                invoice.Status = InvoiceStatus.Paid;
                break;

            case InvoiceType.Emergency:
                var initialPayment = dto.InitialPayment ?? 0;
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
}