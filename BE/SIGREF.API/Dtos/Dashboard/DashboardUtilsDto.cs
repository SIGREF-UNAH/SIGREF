namespace SIGREF.API.Dtos.Dashboard;

public class DashboardSummaryDto
{
    /// <summary>
    /// Ingreso total en el rango filtrado.
    /// </summary>
    public decimal TotalIncome { get; set; }

    /// <summary>
    /// Total de servicios realizados (items facturados).
    /// </summary>
    public int TotalServices { get; set; }

    /// <summary>
    /// Total de pacientes atendidos (distintos).
    /// </summary>
    public int TotalPatients { get; set; }

    /// <summary>
    /// Total de cierres de caja que requirieron corrección
    /// (CashierSession.RequiresCorrection = true).
    /// </summary>
    public int TotalCashierClosuresWithErrors { get; set; }
}

public class ServiceRankingDto
{
    public Guid ServiceId { get; set; }
    public int Count { get; set; }
    public decimal Total { get; set; }
}


public class WeeklyIncomeDto
{
    public string Week { get; set; }
    public decimal Total { get; set; }
}

public class ShiftIncomeDto
{
    public Guid ShiftId { get; set; }
    public string ShiftName { get; set; } = string.Empty;

    public string LocationId { get; set; }
    public string LocationName { get; set; } = string.Empty;

    public decimal TotalIncome { get; set; }
    public int TotalInvoices { get; set; }
}


public class LocationIncomeDto
{
    public string LocationId { get; set; }
    public string LocationName { get; set; } = string.Empty;

    public decimal TotalIncome { get; set; }
    public int TotalInvoices { get; set; }
}



public class ServiceUsageDto
{
    public Guid ServiceId { get; set; }
    public string FhirServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;

    public int Count { get; set; }           // Cantidad de veces utilizado
    public decimal TotalGenerated { get; set; } // Suma total generada

    public decimal Percentage { get; set; }  // Para gráficos tipo donut
}


public class ServiceUsageResultDto
{
    public List<ServiceUsageDto> TopUsed { get; set; } = new();
    public List<ServiceUsageDto> BottomUsed { get; set; } = new();
}

public class PackageUsageDto
{
    public string FhirPackageId { get; set; } = string.Empty;
    public string PackageName { get; set; } = string.Empty;

    public int Count { get; set; }
    public decimal TotalGenerated { get; set; }
    public decimal Percentage { get; set; }
}

public class PackageUsageResultDto
{
    public List<PackageUsageDto> Top5 { get; set; } = new();

    /// <summary>
    /// Si existen más de 5, el sexto elemento representa el total combinado como “Otros”.
    /// </summary>
    public PackageUsageDto? Others { get; set; }
}


