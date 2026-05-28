#nullable enable
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DoodleNote.Areas.Identity.Pages.Account.Manage;
using DoodleNote.Features.Admin.Models;
using DoodleNote.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace DoodleNote.Tests.Areas.Identity.Pages.Account.Manage;

public class IndexModelTests
{
    [Fact]
    public async Task OnPostUpdateUsernameAsync_UpdatesUsername_AndRefreshesSignIn()
    {
        var user = new ApplicationUser
        {
            Id = "user-id",
            UserName = "old_name"
        };

        var userManagerMock = CreateUserManagerMock();
        userManagerMock.Setup(manager => manager.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
        userManagerMock.Setup(manager => manager.FindByNameAsync("new_name")).ReturnsAsync((ApplicationUser?)null);
        userManagerMock
            .Setup(manager => manager.SetUserNameAsync(user, "new_name"))
            .ReturnsAsync(IdentityResult.Success)
            .Callback<ApplicationUser, string>((targetUser, newUserName) => targetUser.UserName = newUserName);
        userManagerMock.Setup(manager => manager.FindByIdAsync(user.Id)).ReturnsAsync(user);

        var signInManagerMock = CreateSignInManagerMock(userManagerMock.Object);
        signInManagerMock.Setup(manager => manager.RefreshSignInAsync(user)).Returns(Task.CompletedTask);

        var pageModel = new IndexModel(userManagerMock.Object, signInManagerMock.Object, Mock.Of<ILogger<IndexModel>>());
        pageModel.PageContext = CreatePageContext("{\"newUsername\":\"new_name\"}");

        IActionResult result = await pageModel.OnPostUpdateUsernameAsync();

        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.Null(jsonResult.StatusCode);
        Assert.Equal("new_name", user.UserName);
        signInManagerMock.Verify(manager => manager.RefreshSignInAsync(user), Times.Once);
        userManagerMock.Verify(manager => manager.SetUserNameAsync(user, "new_name"), Times.Once);
    }

    private static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(
            store.Object,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!);
    }

    private static Mock<SignInManager<ApplicationUser>> CreateSignInManagerMock(UserManager<ApplicationUser> userManager)
    {
        var contextAccessor = new Mock<IHttpContextAccessor>();
        var claimsFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
        return new Mock<SignInManager<ApplicationUser>>(
            userManager,
            contextAccessor.Object,
            claimsFactory.Object,
            null!,
            null!,
            null!,
            null!);
    }

    private static PageContext CreatePageContext(string requestBody)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user-id")
        }, "TestAuth"));
        httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(requestBody));
        httpContext.RequestServices = new ServiceCollection().BuildServiceProvider();

        return new PageContext(new ActionContext(httpContext, new RouteData(), new PageActionDescriptor()));
    }
}
