using DoodleNote.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Xunit;

namespace DoodleNote.Tests;

public class AccountViewModelTests
{
    [Fact]
    public void AccountViewModel_Properties_CanBeSet()
    {
        // Arrange
        var model = new AccountViewModel
        {
            UserId = "test-id",
            Email = "test@example.com",
            Username = "testuser",
            Password = "Pass123!",
            ConfirmPassword = "Pass123!",
            IsAdmin = false
        };

        // Assert
        Assert.Equal("test-id", model.UserId);
        Assert.Equal("test@example.com", model.Email);
        Assert.Equal("testuser", model.Username);
        Assert.Equal("Pass123!", model.Password);
        Assert.Equal("Pass123!", model.ConfirmPassword);
        Assert.False(model.IsAdmin);
    }

    [Fact]
    public void Email_Validation_Requires_Valid_Email_Format()
    {
        // Arrange
        var model = new AccountViewModel
        {
            Email = "invalid-email",
            Username = "testuser",
            Password = "Pass123!",
            ConfirmPassword = "Pass123!"
        };
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();

        // Act
        bool isValid = Validator.TryValidateObject(model, context, results, true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains("Email"));
    }

    [Fact]
    public void Email_Validation_Requires_Email()
    {
        // Arrange
        var model = new AccountViewModel
        {
            Email = null,
            Username = "testuser",
            Password = "Pass123!",
            ConfirmPassword = "Pass123!"
        };
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();

        // Act
        bool isValid = Validator.TryValidateObject(model, context, results, true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains("Email"));
    }

    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@example.co.uk")]
    [InlineData("user+tag@example.com")]
    public void Email_Validation_Accepts_Valid_Emails(string email)
    {
        // Arrange
        var model = new AccountViewModel
        {
            Email = email,
            Username = "testuser",
            Password = "Pass123!",
            ConfirmPassword = "Pass123!"
        };
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();

        // Act
        bool isValid = Validator.TryValidateObject(model, context, results, true);

        // Assert
        var emailErrors = results.Where(r => r.MemberNames.Contains("Email")).ToList();
        Assert.Empty(emailErrors);
    }

    [Theory]
    [InlineData("john")]
    [InlineData("user123456789012345678")]
    public void Username_Validation_Enforces_Length_Requirements(string username)
    {
        // Arrange
        var model = new AccountViewModel
        {
            Email = "test@example.com",
            Username = username,
            Password = "Pass123!",
            ConfirmPassword = "Pass123!"
        };
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();

        // Act
        bool isValid = Validator.TryValidateObject(model, context, results, true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains("Username"));
    }

    [Theory]
    [InlineData("john_doe")]
    [InlineData("user-123")]
    [InlineData("test_user")]
    [InlineData("admin123")]
    public void Username_Validation_Accepts_Valid_Usernames(string username)
    {
        // Arrange
        var model = new AccountViewModel
        {
            Email = "test@example.com",
            Username = username,
            Password = "Pass123!",
            ConfirmPassword = "Pass123!"
        };
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();

        // Act
        bool isValid = Validator.TryValidateObject(model, context, results, true);

        // Assert
        var usernameErrors = results.Where(r => r.MemberNames.Contains("Username")).ToList();
        Assert.Empty(usernameErrors);
    }

    [Theory]
    [InlineData("user@name")]
    [InlineData("user.name")]
    [InlineData("user name")]
    public void Username_Validation_Rejects_Invalid_Characters(string username)
    {
        // Arrange
        var model = new AccountViewModel
        {
            Email = "test@example.com",
            Username = username,
            Password = "Pass123!",
            ConfirmPassword = "Pass123!"
        };
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();

        // Act
        bool isValid = Validator.TryValidateObject(model, context, results, true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains("Username"));
    }

    [Fact]
    public void Password_Validation_Requires_Password()
    {
        // Arrange
        var model = new AccountViewModel
        {
            Email = "test@example.com",
            Username = "testuser",
            Password = null,
            ConfirmPassword = null
        };
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();

        // Act
        bool isValid = Validator.TryValidateObject(model, context, results, true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains("Password"));
    }

    [Theory]
    [InlineData("Pass1")]
    [InlineData("Password1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789")]
    public void Password_Validation_Enforces_Length_Requirements(string password)
    {
        // Arrange
        var model = new AccountViewModel
        {
            Email = "test@example.com",
            Username = "testuser",
            Password = password,
            ConfirmPassword = password
        };
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();

        // Act
        bool isValid = Validator.TryValidateObject(model, context, results, true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains("Password"));
    }

    [Theory]
    [InlineData("Password123")]
    [InlineData("Pass!")]
    public void Password_Validation_Requires_Digit_And_Symbol(string password)
    {
        // Arrange
        var model = new AccountViewModel
        {
            Email = "test@example.com",
            Username = "testuser",
            Password = password,
            ConfirmPassword = password
        };
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();

        // Act
        bool isValid = Validator.TryValidateObject(model, context, results, true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains("Password"));
    }

    [Theory]
    [InlineData("Pass123!")]
    [InlineData("Test@456")]
    [InlineData("Admin#99!")]
    [InlineData("Welcome1%")]
    [InlineData("Pass_123")]
    [InlineData("Test-456!")]
    public void Password_Validation_Accepts_Valid_Passwords(string password)
    {
        // Arrange
        var model = new AccountViewModel
        {
            Email = "test@example.com",
            Username = "testuser",
            Password = password,
            ConfirmPassword = password
        };
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();

        // Act
        bool isValid = Validator.TryValidateObject(model, context, results, true);

        // Assert
        var passwordErrors = results.Where(r => r.MemberNames.Contains("Password")).ToList();
        Assert.Empty(passwordErrors);
    }

    [Theory]
    [InlineData("Pass123!", "Pass456!")]
    [InlineData("Test@789", "Test@790")]
    public void ConfirmPassword_Validation_Requires_Match(string password, string confirmPassword)
    {
        // Arrange
        var model = new AccountViewModel
        {
            Email = "test@example.com",
            Username = "testuser",
            Password = password,
            ConfirmPassword = confirmPassword
        };
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();

        // Act
        bool isValid = Validator.TryValidateObject(model, context, results, true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains("ConfirmPassword"));
    }

    [Fact]
    public void IsAdmin_DefaultsToFalse()
    {
        // Arrange
        var model = new AccountViewModel
        {
            Email = "test@example.com",
            Username = "testuser",
            Password = "Pass123!",
            ConfirmPassword = "Pass123!"
        };

        // Assert
        Assert.False(model.IsAdmin);
    }

    [Fact]
    public void UserId_CanBeNull()
    {
        // Arrange
        var model = new AccountViewModel
        {
            UserId = null,
            Email = "test@example.com",
            Username = "testuser",
            Password = "Pass123!",
            ConfirmPassword = "Pass123!"
        };

        // Assert
        Assert.Null(model.UserId);
    }

    [Fact]
    public void CompleteValidModel_IsValid()
    {
        // Arrange
        var model = new AccountViewModel
        {
            UserId = "123",
            Email = "john@example.com",
            Username = "john_doe",
            Password = "Pass123!",
            ConfirmPassword = "Pass123!",
            IsAdmin = false
        };
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();

        // Act
        bool isValid = Validator.TryValidateObject(model, context, results, true);

        // Assert
        Assert.True(isValid);
        Assert.Empty(results);
    }
}
