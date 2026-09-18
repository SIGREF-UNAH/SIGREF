using System.Reflection;
using SIGREF.API.Dtos.Dashboard;
using SIGREF.API.Services.Dashboard;
using Xunit;

namespace SIGREF.API.Tests;

public sealed class DashboardReportingServiceTests
{
    [Fact]
    public void NormalizeDateRange_returns_utc_bounds_for_local_dashboard_dates()
    {
        var method = typeof(DashboardReportingService).GetMethod(
            "NormalizeDateRange",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(method);

        var filter = new DashboardFilterDto
        {
            StartDate = new DateTime(2026, 8, 10),
            EndDate = new DateTime(2026, 8, 16, 23, 59, 59)
        };

        var result = method!.Invoke(
            new DashboardReportingService(null!, null!),
            new object[] { filter });

        Assert.NotNull(result);
        var start = (DateTime)result!.GetType().GetField("Item1")!.GetValue(result)!;
        var endExclusive = (DateTime)result.GetType().GetField("Item2")!.GetValue(result)!;

        Assert.Equal(DateTimeKind.Utc, start.Kind);
        Assert.Equal(DateTimeKind.Utc, endExclusive.Kind);
        Assert.Equal(new DateTime(2026, 8, 10, 6, 0, 0, DateTimeKind.Utc), start);
        Assert.Equal(new DateTime(2026, 8, 17, 6, 0, 0, DateTimeKind.Utc), endExclusive);
    }
}
