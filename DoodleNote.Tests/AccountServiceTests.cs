#nullable enable
using DoodleNote.Features.Admin.Constants;
using DoodleNote.Features.Admin.Models;
using DoodleNote.Features.Admin.Services;
using DoodleNote.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace DoodleNote.Tests;

public class AccountServiceTests
{
    [Fact]
    public async Task CreateAccountAsync_WhenCreateSucceeds_AssignsDefaultUserRole()
    {
        // Arrange
        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        userManagerMock
            .Setup(um => um.AddToRoleAsync(It.IsAny<ApplicationUser>(), RoleNames.User))
            .ReturnsAsync(IdentityResult.Success);

        AccountService service = new AccountService(userManagerMock.Object, Mock.Of<ILogger<AccountService>>());

        AccountViewModel model = new AccountViewModel
        {
            Email = "test@example.com",
            Username = "test_user",
            Password = "Pass123!",
            ConfirmPassword = "Pass123!",
            IsAdmin = false
        };

        // Act
        (IdentityResult result, ApplicationUser? user) = await service.CreateAccountAsync(model);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(user);
        userManagerMock.Verify(um => um.AddToRoleAsync(It.IsAny<ApplicationUser>(), RoleNames.User), Times.Once);
    }

    [Fact]
    public async Task CreateAccountAsync_WhenRoleAssignmentFails_ReturnsFailure()
    {
        // Arrange
        Mock<UserManager<ApplicationUser>> userManagerMock = CreateUserManagerMock();
        userManagerMock
            .Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        userManagerMock
            .Setup(um => um.AddToRoleAsync(It.IsAny<ApplicationUser>(), RoleNames.User))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Role assignment failed." }));

        AccountService service = new AccountService(userManagerMock.Object, Mock.Of<ILogger<AccountService>>());

        AccountViewModel model = new AccountViewModel
        {
            Email = "test@example.com",
            Username = "test_user",
            Password = "Pass123!",
            ConfirmPassword = "Pass123!",
            IsAdmin = false
        };

        // Act
        (IdentityResult result, ApplicationUser? user) = await service.CreateAccountAsync(model);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Null(user);
    }

    private static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
    {
        Mock<IUserStore<ApplicationUser>> store = new Mock<IUserStore<ApplicationUser>>();
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
}
