using DoodleNote.Models;
using Xunit;

namespace DoodleNote.Tests;

public class IAdminTests
{
    [Fact]
    public void IAdmin_IsImplementedByApplicationUser()
    {
        // Arrange & Act
        var user = new ApplicationUser { UserName = "test" };

        // Assert
        Assert.IsAssignableFrom<IAdmin>(user);
    }

    [Fact]
    public void IAdmin_HasIsAdminProperty()
    {
        // Arrange
        var user = new ApplicationUser { UserName = "test", IsAdmin = true };

        // Act
        IAdmin admin = user;
        var isAdmin = admin.IsAdmin;

        // Assert
        Assert.True(isAdmin);
    }

    [Fact]
    public void IAdmin_Interface_DefinesOnlyGetProperty()
    {
        // Arrange
        var interfaceType = typeof(IAdmin);
        var isAdminProperty = interfaceType.GetProperty("IsAdmin");

        // Assert
        Assert.NotNull(isAdminProperty);
        Assert.True(isAdminProperty.CanRead);
    }

    [Fact]
    public void IAdmin_CanBeUsedForTypeChecking()
    {
        // Arrange
        object user = new ApplicationUser { UserName = "test", IsAdmin = true };

        // Act
        var isIAdmin = user is IAdmin;

        // Assert
        Assert.True(isIAdmin);
    }

    [Fact]
    public void IAdmin_CanBeUsedInPolymorphicMethods()
    {
        // Arrange
        IAdmin admin = new ApplicationUser { UserName = "admin", IsAdmin = true };
        IAdmin regularUser = new ApplicationUser { UserName = "user", IsAdmin = false };

        // Act & Assert
        Assert.True(admin.IsAdmin);
        Assert.False(regularUser.IsAdmin);
    }

    [Fact]
    public void IAdmin_CanCheckAdminStatus()
    {
        // Arrange
        var adminUser = new ApplicationUser { UserName = "admin", IsAdmin = true };
        var regularUser = new ApplicationUser { UserName = "user", IsAdmin = false };

        // Act
        void CheckAdminStatus(IAdmin user)
        {
            Assert.NotNull(user);
        }

        // Assert
        CheckAdminStatus(adminUser);
        CheckAdminStatus(regularUser);
    }
}
