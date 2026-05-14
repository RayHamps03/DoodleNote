#nullable enable
using DoodleNote.Features.Admin.Constants;
using DoodleNote.Features.Admin.Models;
using DoodleNote.Features.Admin.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace DoodleNote.Tests;

public class RoleServiceTests
{
	[Fact]
	public async Task UserHasRoleAsync_ReturnsTrue_WhenUserIsInRole()
	{
		ApplicationUser user = new() { UserName = "test" };
		Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
		userManagerMock.Setup(um => um.IsInRoleAsync(user, RoleNames.Admin)).ReturnsAsync(true);

		RoleService service = new(userManagerMock.Object, CreateRoleManagerMock().Object, Mock.Of<ILogger<RoleService>>());

		bool result = await service.UserHasRoleAsync(user, RoleNames.Admin);

		Assert.True(result);
	}

	[Fact]
	public async Task AddUserToRoleAsync_ReturnsSuccess_WhenAddSucceeds()
	{
		ApplicationUser user = new() { UserName = "test" };
		Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
		userManagerMock.Setup(um => um.AddToRoleAsync(user, RoleNames.User)).ReturnsAsync(IdentityResult.Success);

		RoleService service = new(userManagerMock.Object, CreateRoleManagerMock().Object, Mock.Of<ILogger<RoleService>>());

		IdentityResult result = await service.AddUserToRoleAsync(user, RoleNames.User);

		Assert.True(result.Succeeded);
		userManagerMock.Verify(um => um.AddToRoleAsync(user, RoleNames.User), Times.Once);
	}

	[Fact]
	public async Task PromoteToAdminAsync_ReturnsFailure_WhenRequesterNotAdmin()
	{
		ApplicationUser requester = new() { UserName = "requester" };
		ApplicationUser target = new() { UserName = "target" };
		Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
		userManagerMock.Setup(um => um.IsInRoleAsync(requester, RoleNames.Admin)).ReturnsAsync(false);

		RoleService service = new(userManagerMock.Object, CreateRoleManagerMock().Object, Mock.Of<ILogger<RoleService>>());

		IdentityResult result = await service.PromoteToAdminAsync(target, requester);

		Assert.False(result.Succeeded);
		Assert.Contains(result.Errors, error => error.Description == "Only admins can promote users.");
	}

	[Fact]
	public async Task DemoteFromAdminAsync_ReturnsFailure_WhenUserIsOwner()
	{
		ApplicationUser requester = new() { UserName = "requester" };
		ApplicationUser target = new() { UserName = "target" };
		Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
		userManagerMock.Setup(um => um.IsInRoleAsync(requester, RoleNames.Admin)).ReturnsAsync(true);
		userManagerMock.Setup(um => um.IsInRoleAsync(target, RoleNames.Owner)).ReturnsAsync(true);

		RoleService service = new(userManagerMock.Object, CreateRoleManagerMock().Object, Mock.Of<ILogger<RoleService>>());

		IdentityResult result = await service.DemoteFromAdminAsync(target, requester);

		Assert.False(result.Succeeded);
		Assert.Contains(result.Errors, error => error.Description == "Cannot demote the system owner from admin role.");
	}

	[Fact]
	public async Task PromoteToOwnerAsync_RemovesExistingOwnerAndAddsRoles()
	{
		ApplicationUser requester = new() { UserName = "requester", Id = "requester-id" };
		ApplicationUser currentOwner = new() { UserName = "owner", Id = "owner-id" };
		ApplicationUser target = new() { UserName = "target", Id = "target-id" };
		Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
		userManagerMock.Setup(um => um.IsInRoleAsync(requester, RoleNames.Admin)).ReturnsAsync(true);
		userManagerMock.Setup(um => um.GetUsersInRoleAsync(RoleNames.Owner))
			.ReturnsAsync(new List<ApplicationUser> { currentOwner });
		userManagerMock.Setup(um => um.RemoveFromRoleAsync(currentOwner, RoleNames.Owner))
			.ReturnsAsync(IdentityResult.Success);
		userManagerMock.Setup(um => um.AddToRoleAsync(target, RoleNames.Owner))
			.ReturnsAsync(IdentityResult.Success);
		userManagerMock.Setup(um => um.AddToRoleAsync(target, RoleNames.Admin))
			.ReturnsAsync(IdentityResult.Success);

		RoleService service = new(userManagerMock.Object, CreateRoleManagerMock().Object, Mock.Of<ILogger<RoleService>>());

		IdentityResult result = await service.PromoteToOwnerAsync(target, requester);

		Assert.True(result.Succeeded);
		userManagerMock.Verify(um => um.RemoveFromRoleAsync(currentOwner, RoleNames.Owner), Times.Once);
		userManagerMock.Verify(um => um.AddToRoleAsync(target, RoleNames.Owner), Times.Once);
		userManagerMock.Verify(um => um.AddToRoleAsync(target, RoleNames.Admin), Times.Once);
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

	private static Mock<RoleManager<IdentityRole>> CreateRoleManagerMock()
	{
		Mock<IRoleStore<IdentityRole>> store = new();
		return new Mock<RoleManager<IdentityRole>>(
			store.Object,
			null!,
			null!,
			null!,
			null!);
	}
}
