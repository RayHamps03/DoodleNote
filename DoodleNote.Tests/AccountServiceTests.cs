using DoodleNote.Models;
using DoodleNote.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DoodleNote.Tests;

public class AccountServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<ILogger<AccountService>> _mockLogger;
    private readonly AccountService _accountService;

    public AccountServiceTests()
    {
        _mockUserManager = new Mock<UserManager<ApplicationUser>>(
            new Mock<IUserStore<ApplicationUser>>().Object,
            null, null, null, null, null, null, null, null
        );
        _mockLogger = new Mock<ILogger<AccountService>>();
        _accountService = new AccountService(_mockUserManager.Object, _mockLogger.Object);
    }

    #region ValidateAccountViewModel Tests

    [Fact]
    public void ValidateAccountViewModel_ReturnsNoErrors_ForValidModel()
    {
        // Arrange
        var model = new AccountViewModel
        {
            Email = "test@example.com",
            Username = "testuser",
            Password = "Pass123!",
            ConfirmPassword = "Pass123!"
        };

        // Act
        var errors = _accountService.ValidateAccountViewModel(model);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateAccountViewModel_ReturnsError_WhenEmailIsEmpty()
    {
        // Arrange
        var model = new AccountViewModel
        {
            Email = "",
            Username = "testuser",
            Password = "Pass123!",
            ConfirmPassword = "Pass123!"
        };

        // Act
        var errors = _accountService.ValidateAccountViewModel(model);

        // Assert
        Assert.Contains("Email is required.", errors);
    }

    [Fact]
    public void ValidateAccountViewModel_ReturnsError_WhenEmailIsInvalid()
    {
        // Arrange
        var model = new AccountViewModel
        {
            Email = "invalid-email",
            Username = "testuser",
            Password = "Pass123!",
            ConfirmPassword = "Pass123!"
        };

        // Act
        var errors = _accountService.ValidateAccountViewModel(model);

        // Assert
        Assert.Contains("Email format is invalid.", errors);
    }

    [Fact]
    public void ValidateAccountViewModel_ReturnsError_WhenEmailExceedsMaxLength()
    {
        // Arrange
        var longEmail = new string('a', 250) + "@test.com";
        var model = new AccountViewModel
        {
            Email = longEmail,
            Username = "testuser",
            Password = "Pass123!",
            ConfirmPassword = "Pass123!"
        };

        // Act
        var errors = _accountService.ValidateAccountViewModel(model);

        // Assert
        Assert.Contains("Email cannot exceed 256 characters.", errors);
    }

    [Fact]
    public void ValidateAccountViewModel_ReturnsError_WhenUsernameIsEmpty()
    {
        // Arrange
        var model = new AccountViewModel
        {
            Email = "test@example.com",
            Username = "",
            Password = "Pass123!",
            ConfirmPassword = "Pass123!"
        };

        // Act
        var errors = _accountService.ValidateAccountViewModel(model);

        // Assert
        Assert.Contains("Username is required.", errors);
    }

    [Theory]
    [InlineData("john")]
    [InlineData("username123456789012345")]
    public void ValidateAccountViewModel_ReturnsError_WhenUsernameLengthInvalid(string username)
    {
        // Arrange
        var model = new AccountViewModel
        {
            Email = "test@example.com",
            Username = username,
            Password = "Pass123!",
            ConfirmPassword = "Pass123!"
        };

        // Act
        var errors = _accountService.ValidateAccountViewModel(model);

        // Assert
        Assert.Contains("Username must be between 5 and 20 characters.", errors);
    }

    [Fact]
    public void ValidateAccountViewModel_ReturnsError_WhenPasswordIsEmpty()
    {
        // Arrange
        var model = new AccountViewModel
        {
            Email = "test@example.com",
            Username = "testuser",
            Password = "",
            ConfirmPassword = ""
        };

        // Act
        var errors = _accountService.ValidateAccountViewModel(model);

        // Assert
        Assert.Contains("Password is required.", errors);
    }

    [Theory]
    [InlineData("Pass1")]
    [InlineData("Pass123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345")]
    public void ValidateAccountViewModel_ReturnsError_WhenPasswordLengthInvalid(string password)
    {
        // Arrange
        var model = new AccountViewModel
        {
            Email = "test@example.com",
            Username = "testuser",
            Password = password,
            ConfirmPassword = password
        };

        // Act
        var errors = _accountService.ValidateAccountViewModel(model);

        // Assert
        Assert.Contains("Password must be between 6 and 100 characters.", errors);
    }

    [Theory]
    [InlineData("Password123")]
    [InlineData("Pass!")]
    public void ValidateAccountViewModel_ReturnsError_WhenPasswordLacksRequiredCharacters(string password)
    {
        // Arrange
        var model = new AccountViewModel
        {
            Email = "test@example.com",
            Username = "testuser",
            Password = password,
            ConfirmPassword = password
        };

        // Act
        var errors = _accountService.ValidateAccountViewModel(model);

        // Assert
        Assert.Contains("Password must contain at least one number and one symbol (e.g., |, !, %).", errors);
    }

    [Fact]
    public void ValidateAccountViewModel_ReturnsError_WhenPasswordsDoNotMatch()
    {
        // Arrange
        var model = new AccountViewModel
        {
            Email = "test@example.com",
            Username = "testuser",
            Password = "Pass123!",
            ConfirmPassword = "Pass456!"
        };

        // Act
        var errors = _accountService.ValidateAccountViewModel(model);

        // Assert
        Assert.Contains("Passwords do not match.", errors);
    }

    #endregion

    #region CreateAccountAsync Tests

    [Fact]
    public async Task CreateAccountAsync_ReturnsFailedResult_WhenValidationFails()
    {
        // Arrange
        var model = new AccountViewModel
        {
            Email = "invalid",
            Username = "test",
            Password = "Pass123!",
            ConfirmPassword = "Pass123!"
        };

        // Act
        var (result, user) = await _accountService.CreateAccountAsync(model);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Null(user);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task CreateAccountAsync_CreatesUser_WhenValidationPasses()
    {
        // Arrange
        var model = new AccountViewModel
        {
            Email = "test@example.com",
            Username = "testuser",
            Password = "Pass123!",
            ConfirmPassword = "Pass123!"
        };

        var identityResult = IdentityResult.Success;
        _mockUserManager
            .Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(identityResult);

        // Act
        var (result, user) = await _accountService.CreateAccountAsync(model);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(user);
        Assert.Equal("testuser", user.UserName);
        Assert.Equal("test@example.com", user.Email);
        Assert.False(user.IsAdmin);
        Assert.False(user.IsOwner);
    }

    [Fact]
    public async Task CreateAccountAsync_ReturnsNull_WhenUserCreationFails()
    {
        // Arrange
        var model = new AccountViewModel
        {
            Email = "test@example.com",
            Username = "testuser",
            Password = "Pass123!",
            ConfirmPassword = "Pass123!"
        };

        var identityError = new IdentityError { Description = "User creation failed" };
        var failedResult = IdentityResult.Failed(identityError);
        _mockUserManager
            .Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(failedResult);

        // Act
        var (result, user) = await _accountService.CreateAccountAsync(model);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Null(user);
    }

    #endregion

    #region FindUser Tests

    [Fact]
    public async Task FindUserByEmailAsync_ReturnsUser_WhenEmailExists()
    {
        // Arrange
        var existingUser = new ApplicationUser { Id = "1", Email = "test@example.com" };
        _mockUserManager
            .Setup(um => um.FindByEmailAsync("test@example.com"))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _accountService.FindUserByEmailAsync("test@example.com");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test@example.com", result.Email);
    }

    [Fact]
    public async Task FindUserByEmailAsync_ReturnsNull_WhenEmailDoesNotExist()
    {
        // Arrange
        _mockUserManager
            .Setup(um => um.FindByEmailAsync("nonexistent@example.com"))
            .ReturnsAsync((ApplicationUser)null);

        // Act
        var result = await _accountService.FindUserByEmailAsync("nonexistent@example.com");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task FindUserByUsernameAsync_ReturnsUser_WhenUsernameExists()
    {
        // Arrange
        var existingUser = new ApplicationUser { Id = "1", UserName = "testuser" };
        _mockUserManager
            .Setup(um => um.FindByNameAsync("testuser"))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _accountService.FindUserByUsernameAsync("testuser");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("testuser", result.UserName);
    }

    [Fact]
    public async Task FindUserByUsernameAsync_ReturnsNull_WhenUsernameDoesNotExist()
    {
        // Arrange
        _mockUserManager
            .Setup(um => um.FindByNameAsync("nonexistent"))
            .ReturnsAsync((ApplicationUser)null);

        // Act
        var result = await _accountService.FindUserByUsernameAsync("nonexistent");

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region Uniqueness Tests

    [Fact]
    public async Task IsEmailInUseAsync_ReturnsTrue_WhenEmailExists()
    {
        // Arrange
        var existingUser = new ApplicationUser { Email = "test@example.com" };
        _mockUserManager
            .Setup(um => um.FindByEmailAsync("test@example.com"))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _accountService.IsEmailInUseAsync("test@example.com");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsEmailInUseAsync_ReturnsFalse_WhenEmailDoesNotExist()
    {
        // Arrange
        _mockUserManager
            .Setup(um => um.FindByEmailAsync("unused@example.com"))
            .ReturnsAsync((ApplicationUser)null);

        // Act
        var result = await _accountService.IsEmailInUseAsync("unused@example.com");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsUsernameInUseAsync_ReturnsTrue_WhenUsernameExists()
    {
        // Arrange
        var existingUser = new ApplicationUser { UserName = "testuser" };
        _mockUserManager
            .Setup(um => um.FindByNameAsync("testuser"))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _accountService.IsUsernameInUseAsync("testuser");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsUsernameInUseAsync_ReturnsFalse_WhenUsernameDoesNotExist()
    {
        // Arrange
        _mockUserManager
            .Setup(um => um.FindByNameAsync("unused"))
            .ReturnsAsync((ApplicationUser)null);

        // Act
        var result = await _accountService.IsUsernameInUseAsync("unused");

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Owner Management Tests

    [Fact]
    public async Task FindOwnerAsync_ReturnsOwner_WhenOwnerExists()
    {
        // Arrange
        var ownerUser = new ApplicationUser { Id = "1", UserName = "owner", IsOwner = true };
        var users = new List<ApplicationUser> { ownerUser }.AsQueryable();
        var mockDbSet = new Mock<IQueryable<ApplicationUser>>();
        mockDbSet.Setup(m => m.Provider).Returns(users.Provider);
        mockDbSet.Setup(m => m.Expression).Returns(users.Expression);
        mockDbSet.Setup(m => m.ElementType).Returns(users.ElementType);
        mockDbSet.Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());

        _mockUserManager
            .Setup(um => um.Users)
            .Returns(mockDbSet.Object);

        // Act
        var result = await _accountService.FindOwnerAsync();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsOwner);
    }

    [Fact]
    public async Task PromoteUserToOwnerAsync_FailsWhenRequestingUserIsNotAdmin()
    {
        // Arrange
        var targetUser = new ApplicationUser { Id = "1", UserName = "user" };
        var nonAdminUser = new ApplicationUser { Id = "2", IsAdmin = false };

        // Act
        var result = await _accountService.PromoteUserToOwnerAsync("1", nonAdminUser);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, e => e.Description == "Only admin users can promote users to owner.");
    }

    [Fact]
    public async Task PromoteUserToOwnerAsync_FailsWhenUserNotFound()
    {
        // Arrange
        var adminUser = new ApplicationUser { Id = "2", IsAdmin = true };
        _mockUserManager
            .Setup(um => um.FindByIdAsync("99"))
            .ReturnsAsync((ApplicationUser)null);

        // Act
        var result = await _accountService.PromoteUserToOwnerAsync("99", adminUser);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, e => e.Description == "User not found.");
    }

    [Fact]
    public async Task PromoteUserToOwnerAsync_SucceedsWhenAdminPromotesUser()
    {
        // Arrange
        var targetUser = new ApplicationUser { Id = "1", UserName = "user", IsAdmin = false, IsOwner = false };
        var adminUser = new ApplicationUser { Id = "2", IsAdmin = true };

        _mockUserManager
            .Setup(um => um.FindByIdAsync("1"))
            .ReturnsAsync(targetUser);
        _mockUserManager
            .Setup(um => um.FindByIdAsync("2"))
            .ReturnsAsync(adminUser);
        _mockUserManager
            .Setup(um => um.Users)
            .Returns(new List<ApplicationUser>().AsQueryable());
        _mockUserManager
            .Setup(um => um.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _accountService.PromoteUserToOwnerAsync("1", adminUser);

        // Assert
        Assert.True(result.Succeeded);
        Assert.True(targetUser.IsAdmin);
        Assert.True(targetUser.IsOwner);
    }

    [Fact]
    public async Task DemoteOwnerAsync_FailsWhenRequestingUserIsNotAdmin()
    {
        // Arrange
        var ownerUser = new ApplicationUser { Id = "1", IsOwner = true };
        var nonAdminUser = new ApplicationUser { Id = "2", IsAdmin = false };

        // Act
        var result = await _accountService.DemoteOwnerAsync("1", nonAdminUser);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, e => e.Description == "Only admin users can demote the owner.");
    }

    [Fact]
    public async Task DemoteOwnerAsync_FailsWhenUserNotFound()
    {
        // Arrange
        var adminUser = new ApplicationUser { Id = "2", IsAdmin = true };
        _mockUserManager
            .Setup(um => um.FindByIdAsync("99"))
            .ReturnsAsync((ApplicationUser)null);

        // Act
        var result = await _accountService.DemoteOwnerAsync("99", adminUser);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, e => e.Description == "User not found.");
    }

    [Fact]
    public async Task DemoteOwnerAsync_FailsWhenUserIsNotOwner()
    {
        // Arrange
        var regularUser = new ApplicationUser { Id = "1", IsOwner = false };
        var adminUser = new ApplicationUser { Id = "2", IsAdmin = true };

        _mockUserManager
            .Setup(um => um.FindByIdAsync("1"))
            .ReturnsAsync(regularUser);

        // Act
        var result = await _accountService.DemoteOwnerAsync("1", adminUser);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, e => e.Description == "User is not the owner.");
    }

    [Fact]
    public async Task IsUserOwnerAsync_ReturnsTrue_WhenUserIsOwner()
    {
        // Arrange
        var ownerUser = new ApplicationUser { Id = "1", IsOwner = true };
        _mockUserManager
            .Setup(um => um.FindByIdAsync("1"))
            .ReturnsAsync(ownerUser);

        // Act
        var result = await _accountService.IsUserOwnerAsync("1");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsUserOwnerAsync_ReturnsFalse_WhenUserIsNotOwner()
    {
        // Arrange
        var regularUser = new ApplicationUser { Id = "1", IsOwner = false };
        _mockUserManager
            .Setup(um => um.FindByIdAsync("1"))
            .ReturnsAsync(regularUser);

        // Act
        var result = await _accountService.IsUserOwnerAsync("1");

        // Assert
        Assert.False(result);
    }

    #endregion
}
