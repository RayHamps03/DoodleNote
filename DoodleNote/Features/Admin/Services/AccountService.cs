using DoodleNote.Features.Admin.Models;
using DoodleNote.Features.Admin.Constants;
using DoodleNote.Models;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace DoodleNote.Features.Admin.Services;

/// <summary>
/// Service for handling account-related operations with AspNetIdentity.
/// Uses role-based authorization instead of user properties.
/// Part of the self-contained Admin feature module that can be removed without breaking the app.
/// </summary>
public class AccountService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AccountService> _logger;

    public AccountService(UserManager<ApplicationUser> userManager, ILogger<AccountService> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Validates an account view model for correctness.
    /// </summary>
    /// <param name="accountViewModel">The account view model to validate.</param>
    /// <returns>A list of validation errors, empty if valid.</returns>
    public List<string> ValidateAccountViewModel(AccountViewModel accountViewModel)
    {
        List<ValidationResult> validationResults = new();
        ValidationContext validationContext = new(accountViewModel);

        Validator.TryValidateObject(accountViewModel, validationContext, validationResults, validateAllProperties: true);

        return validationResults
            .Select(result => result.ErrorMessage)
            .Where(static errorMessage => !string.IsNullOrWhiteSpace(errorMessage))
            .Cast<string>()
            .ToList();
    }

    /// <summary>
    /// Creates a new user account from an AccountViewModel.
    /// New users are assigned to the User role by default.
    /// </summary>
    /// <param name="accountViewModel">The account view model containing user data.</param>
    /// <returns>A tuple containing the result and the created user (if successful).</returns>
    public async Task<(IdentityResult result, ApplicationUser? user)> CreateAccountAsync(AccountViewModel accountViewModel)
    {
        List<string> validationErrors = ValidateAccountViewModel(accountViewModel);
        if (validationErrors.Count > 0)
        {
            IdentityResult result = IdentityResult.Failed(validationErrors
                .Select(e => new IdentityError { Description = e })
                .ToArray());
            return (result, null);
        }

        ApplicationUser user = new ApplicationUser
        {
            UserName = accountViewModel.Username,
            Email = accountViewModel.Email,
            EmailConfirmed = false
        };

        IdentityResult createResult = await _userManager.CreateAsync(user, accountViewModel.Password);

        if (!createResult.Succeeded)
        {
            _logger.LogWarning("Failed to create user account for username: {Username}", accountViewModel.Username);
            return (createResult, null);
        }

        IdentityResult addToRoleResult = await _userManager.AddToRoleAsync(user, RoleNames.User);
        if (!addToRoleResult.Succeeded)
        {
            _logger.LogWarning("User account created but failed to assign default role for username: {Username}", accountViewModel.Username);
            return (addToRoleResult, null);
        }

        _logger.LogInformation("User account created successfully for username: {Username}", accountViewModel.Username);
        return (addToRoleResult, user);
    }

    /// <summary>
    /// Attempts to find a user by email address.
    /// </summary>
    /// <param name="email">The email address to search for.</param>
    /// <returns>The ApplicationUser if found, null otherwise.</returns>
    public async Task<ApplicationUser?> FindUserByEmailAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    /// <summary>
    /// Attempts to find a user by username.
    /// </summary>
    /// <param name="username">The username to search for.</param>
    /// <returns>The ApplicationUser if found, null otherwise.</returns>
    public async Task<ApplicationUser?> FindUserByUsernameAsync(string username)
    {
        return await _userManager.FindByNameAsync(username);
    }

    /// <summary>
    /// Checks if an email is already in use.
    /// </summary>
    /// <param name="email">The email to check.</param>
    /// <returns>True if the email is in use, false otherwise.</returns>
    public async Task<bool> IsEmailInUseAsync(string email)
    {
        return await FindUserByEmailAsync(email) != null;
    }

    /// <summary>
    /// Checks if a username is already in use.
    /// </summary>
    /// <param name="username">The username to check.</param>
    /// <returns>True if the username is in use, false otherwise.</returns>
    public async Task<bool> IsUsernameInUseAsync(string username)
    {
        return await FindUserByUsernameAsync(username) != null;
    }
}
