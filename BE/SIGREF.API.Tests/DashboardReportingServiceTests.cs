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
            StartDate = new DateTimeOffset(2026, 8, 10, 0, 0, 0, TimeSpan.Zero),
            EndDate = new DateTimeOffset(2026, 8, 16, 23, 59, 59, TimeSpan.Zero)
        };

        var result = method!.Invoke(
            new DashboardReportingService(null!, null!),
            new object[] { filter });

        Assert.NotNull(result);
        var start = (DateTimeOffset)result!.GetType().GetField("Item1")!.GetValue(result)!;
        var endExclusive = (DateTimeOffset)result.GetType().GetField("Item2")!.GetValue(result)!;

        Assert.Equal(TimeSpan.Zero, start.Offset);
        Assert.Equal(TimeSpan.Zero, endExclusive.Offset);
        Assert.Equal(new DateTimeOffset(2026, 8, 10, 6, 0, 0, TimeSpan.Zero), start);
        Assert.Equal(new DateTimeOffset(2026, 8, 17, 6, 0, 0, TimeSpan.Zero), endExclusive);
    }
}
