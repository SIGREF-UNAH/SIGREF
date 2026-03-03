namespace SIGREF.API.Database.Entity.common;

/// <summary>
/// Estado del job de exportación.
/// </summary>
public enum ReportExportStatus
{
    Queued = 1,
    Processing = 2,
    Completed = 3,
    Failed = 4
}
