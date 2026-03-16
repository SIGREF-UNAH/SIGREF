namespace SIGREF.Common.Types;

/// <summary>
/// Estado del job de exportación.
/// </summary>
public enum ReportStatus
{
    Pending    = 0,
    Processing = 1,
    Completed  = 2,
    Failed     = 3
}
