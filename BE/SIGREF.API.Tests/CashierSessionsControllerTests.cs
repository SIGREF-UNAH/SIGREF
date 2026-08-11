using Microsoft.AspNetCore.Mvc;
using Moq;
using SIGREF.API.Controllers.Cashier;
using SIGREF.API.Dtos.Cashier;
using SIGREF.API.Services.Cashier;
using SIGREF.Infrastructure.Keycloak.Interfaces;
using Xunit;

namespace SIGREF.API.Tests;

public sealed class CashierSessionsControllerTests
{
    [Fact]
    public async Task GetActive_uses_authenticated_user_and_returns_session()
    {
        var userId = Guid.NewGuid();
        var expected = new CashierSessionDto { Id = Guid.NewGuid(), UserId = userId };
        var service = new Mock<ICashierSessionService>();
        var userContext = new Mock<IUserContextService>();
        userContext.Setup(x => x.GetUserId()).Returns(userId);
        service.Setup(x => x.GetActiveSessionByUserAsync(userId)).ReturnsAsync(expected);
        var controller = new CashierSessionsController(service.Object, userContext.Object);

        var result = await controller.GetActive();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(expected, ok.Value);
        service.Verify(x => x.GetActiveSessionByUserAsync(userId), Times.Once);
    }
}
