#nullable enable
using DoodleNote.Features.Admin.Models;
using Xunit;

namespace DoodleNote.Tests;

/// <summary>
/// Tests for ApplicationUser model basic functionality.
/// </summary>
public class ApplicationUserTests
{
    [Fact]
    public void ApplicationUser_CanBeInstantiated()
    {
        // Arrange & Act
        ApplicationUser user = new ApplicationUser { UserName = "testuser" };

        // Assert
        Assert.NotNull(user);
        Assert.Equal("testuser", user.UserName);
    }

    [Fact]
    public void ApplicationUser_InheritsFromIdentityUser()
    {
        // Arrange & Act
        ApplicationUser user = new ApplicationUser { UserName = "testuser", Email = "test@example.com" };

        // Assert
        Assert.Equal("testuser", user.UserName);
        Assert.Equal("test@example.com", user.Email);
    }

    [Fact]
    public void ApplicationUser_CanSetAndGetProperties()
    {
        // Arrange
        ApplicationUser user = new ApplicationUser();

        // Act
        user.UserName = "johndoe";
        user.Email = "john@example.com";
        user.EmailConfirmed = true;

        // Assert
        Assert.Equal("johndoe", user.UserName);
        Assert.Equal("john@example.com", user.Email);
        Assert.True(user.EmailConfirmed);
    }
}
