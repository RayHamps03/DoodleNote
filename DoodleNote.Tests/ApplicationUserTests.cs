using DoodleNote.Models;
using System;
using Xunit;

namespace DoodleNote.Tests;

public class ApplicationUserTests
{
    [Fact]
    public void ApplicationUser_Implements_IAdmin_Interface()
    {
        // Arrange & Act
        var user = new ApplicationUser { UserName = "testuser" };

        // Assert
        Assert.IsAssignableFrom<IAdmin>(user);
    }

    [Fact]
    public void IsAdmin_DefaultsToFalse()
    {
        // Arrange & Act
        var user = new ApplicationUser { UserName = "testuser" };

        // Assert
        Assert.False(user.IsAdmin);
    }

    [Fact]
    public void IsOwner_DefaultsToFalse()
    {
        // Arrange & Act
        var user = new ApplicationUser { UserName = "testuser" };

        // Assert
        Assert.False(user.IsOwner);
    }

    [Fact]
    public void IsAdmin_CanBeSet()
    {
        // Arrange
        var user = new ApplicationUser { UserName = "testuser" };

        // Act
        user.IsAdmin = true;

        // Assert
        Assert.True(user.IsAdmin);
    }

    [Fact]
    public void IsOwner_CanBeSet()
    {
        // Arrange
        var user = new ApplicationUser { UserName = "testuser" };

        // Act
        user.IsOwner = true;

        // Assert
        Assert.True(user.IsOwner);
    }

    [Fact]
    public void SetAdminStatus_GrantsAdmin_WhenRequestingUserIsAdmin()
    {
        // Arrange
        var targetUser = new ApplicationUser { UserName = "target", IsAdmin = false };
        var adminUser = new ApplicationUser { UserName = "admin", IsAdmin = true };

        // Act
        targetUser.SetAdminStatus(true, adminUser);

        // Assert
        Assert.True(targetUser.IsAdmin);
    }

    [Fact]
    public void SetAdminStatus_RevokesAdmin_WhenRequestingUserIsAdmin()
    {
        // Arrange
        var targetUser = new ApplicationUser { UserName = "target", IsAdmin = true };
        var adminUser = new ApplicationUser { UserName = "admin", IsAdmin = true };

        // Act
        targetUser.SetAdminStatus(false, adminUser);

        // Assert
        Assert.False(targetUser.IsAdmin);
    }

    [Fact]
    public void SetAdminStatus_ThrowsUnauthorizedAccessException_WhenRequestingUserIsNotAdmin()
    {
        // Arrange
        var targetUser = new ApplicationUser { UserName = "target", IsAdmin = false };
        var regularUser = new ApplicationUser { UserName = "user", IsAdmin = false };

        // Act & Assert
        var exception = Assert.Throws<UnauthorizedAccessException>(
            () => targetUser.SetAdminStatus(true, regularUser)
        );
        Assert.Equal("Only admin users can change admin status.", exception.Message);
    }

    [Fact]
    public void SetAdminStatus_ThrowsInvalidOperationException_WhenDemotingOwner()
    {
        // Arrange
        var ownerUser = new ApplicationUser { UserName = "owner", IsAdmin = true, IsOwner = true };
        var adminUser = new ApplicationUser { UserName = "admin", IsAdmin = true };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(
            () => ownerUser.SetAdminStatus(false, adminUser)
        );
        Assert.Equal("The owner account cannot be demoted. The IsAdmin status for the owner must remain true.", exception.Message);
    }

    [Fact]
    public void PromoteToOwner_SetsIsAdminAndIsOwnerToTrue()
    {
        // Arrange
        var userToPromote = new ApplicationUser { UserName = "user", IsAdmin = false, IsOwner = false };
        var adminUser = new ApplicationUser { UserName = "admin", IsAdmin = true };

        // Act
        userToPromote.PromoteToOwner(adminUser);

        // Assert
        Assert.True(userToPromote.IsAdmin);
        Assert.True(userToPromote.IsOwner);
    }

    [Fact]
    public void PromoteToOwner_ThrowsUnauthorizedAccessException_WhenRequestingUserIsNotAdmin()
    {
        // Arrange
        var userToPromote = new ApplicationUser { UserName = "user", IsAdmin = false };
        var regularUser = new ApplicationUser { UserName = "regular", IsAdmin = false };

        // Act & Assert
        var exception = Assert.Throws<UnauthorizedAccessException>(
            () => userToPromote.PromoteToOwner(regularUser)
        );
        Assert.Equal("Only admin users can promote users to owner.", exception.Message);
    }

    [Fact]
    public void DemoteFromOwner_SetsIsOwnerToFalseButKeepsIsAdmin()
    {
        // Arrange
        var ownerUser = new ApplicationUser { UserName = "owner", IsAdmin = true, IsOwner = true };
        var adminUser = new ApplicationUser { UserName = "admin", IsAdmin = true };

        // Act
        ownerUser.DemoteFromOwner(adminUser);

        // Assert
        Assert.False(ownerUser.IsOwner);
        Assert.True(ownerUser.IsAdmin); // Should remain admin
    }

    [Fact]
    public void DemoteFromOwner_ThrowsUnauthorizedAccessException_WhenRequestingUserIsNotAdmin()
    {
        // Arrange
        var ownerUser = new ApplicationUser { UserName = "owner", IsAdmin = true, IsOwner = true };
        var regularUser = new ApplicationUser { UserName = "regular", IsAdmin = false };

        // Act & Assert
        var exception = Assert.Throws<UnauthorizedAccessException>(
            () => ownerUser.DemoteFromOwner(regularUser)
        );
        Assert.Equal("Only admin users can demote the owner.", exception.Message);
    }

    [Fact]
    public void SetAdminStatus_AllowsPromotingNonOwner()
    {
        // Arrange
        var regularUser = new ApplicationUser { UserName = "user", IsAdmin = false, IsOwner = false };
        var adminUser = new ApplicationUser { UserName = "admin", IsAdmin = true };

        // Act
        regularUser.SetAdminStatus(true, adminUser);

        // Assert
        Assert.True(regularUser.IsAdmin);
        Assert.False(regularUser.IsOwner); // Should still not be owner
    }

    [Fact]
    public void SetAdminStatus_AllowsDemotingNonOwner()
    {
        // Arrange
        var adminUser = new ApplicationUser { UserName = "admin", IsAdmin = true, IsOwner = false };
        var ownerUser = new ApplicationUser { UserName = "owner", IsAdmin = true, IsOwner = true };

        // Act
        adminUser.SetAdminStatus(false, ownerUser);

        // Assert
        Assert.False(adminUser.IsAdmin);
    }
}
