#nullable enable
using DoodleNote.Controllers;
using DoodleNote.Features.Admin.Models;
using DoodleNote.Features.Admin.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace DoodleNote.Tests;

public class AdminAccountsControllerTests
{
	[Fact]
	public async Task Index_ReturnsUnauthorized_WhenCurrentUserMissing()
	{
		Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
		userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync((ApplicationUser?)null);

		AdminAccountsController controller = new(userManagerMock.Object, CreateRoleServiceMock());

		IActionResult result = await controller.Index();

		Assert.IsType<UnauthorizedResult>(result);
	}

	[Fact]
	public async Task RemoveAccount_ReturnsBadRequest_WhenUserIdMissing()
	{
		AdminAccountsController controller = new(CreateUserManagerMock().Object, CreateRoleServiceMock());

		IActionResult result = await controller.RemoveAccount(" ");

		BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(result);
		Assert.Equal("User ID is required.", badRequest.Value);
	}

	[Fact]
	public async Task AssignRole_ReturnsBadRequest_WhenRoleMissing()
	{
		AdminAccountsController controller = new(CreateUserManagerMock().Object, CreateRoleServiceMock());

		IActionResult result = await controller.AssignRole("user-id", " ");

		BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(result);
		Assert.Equal("User ID and role are required.", badRequest.Value);
	}

	[Fact]
	public async Task RemoveRole_ReturnsBadRequest_WhenRoleMissing()
	{
		AdminAccountsController controller = new(CreateUserManagerMock().Object, CreateRoleServiceMock());

		IActionResult result = await controller.RemoveRole("user-id", " ");

		BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(result);
		Assert.Equal("User ID and role are required.", badRequest.Value);
	}

	private static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
	{
		Mock<IUserStore<ApplicationUser>> store = new();
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

	private static RoleService CreateRoleServiceMock()
	{
		var userManagerMock = CreateUserManagerMock();
		var roleManagerMock = new Mock<RoleManager<IdentityRole>>(
			new Mock<IRoleStore<IdentityRole>>().Object,
			null!,
			null!,
			null!,
			null!);
		var loggerMock = new Mock<ILogger<RoleService>>();

		return new RoleService(
			userManagerMock.Object,
			roleManagerMock.Object,
			loggerMock.Object);
	}
}
