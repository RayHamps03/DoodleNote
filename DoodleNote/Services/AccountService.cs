using DoodleNote.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace DoodleNote.Services;

/// <summary>
/// Service for handling account-related operations with AspNetIdentity.
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
        var errors = new List<string>();

        // Validate Email
        if (string.IsNullOrWhiteSpace(accountViewModel.Email))
        {
            errors.Add("Email is required.");
        }
        else if (!new EmailAddressAttribute().IsValid(accountViewModel.Email))
        {
            errors.Add("Email format is invalid.");
        }
        else if (accountViewModel.Email.Length > 256)
        {
            errors.Add("Email cannot exceed 256 characters.");
        }

        // Validate Username
        if (string.IsNullOrWhiteSpace(accountViewModel.Username))
        {
            errors.Add("Username is required.");
        }
        else if (accountViewModel.Username.Length < 5 || accountViewModel.Username.Length > 20)
        {
            errors.Add("Username must be between 5 and 20 characters.");
        }

        // Validate Password
        if (string.IsNullOrWhiteSpace(accountViewModel.Password))
        {
            errors.Add("Password is required.");
        }
        else if (accountViewModel.Password.Length < 6 || accountViewModel.Password.Length > 100)
        {
            errors.Add("Password must be between 6 and 100 characters.");
        }
        else if (!HasPasswordRequiredCharacters(accountViewModel.Password))
        {
            errors.Add("Password must contain at least one number and one symbol (e.g., |, !, %).");
        }

        // Validate ConfirmPassword
        if (accountViewModel.Password != accountViewModel.ConfirmPassword)
        {
            errors.Add("Passwords do not match.");
        }

        return errors;
    }

    /// <summary>
    /// Checks if a password contains at least one number and one symbol.
    /// </summary>
    private bool HasPasswordRequiredCharacters(string password)
    {
        bool hasNumber = password.Any(char.IsDigit);
        bool hasSymbol = password.Any(c => "!@#$%^&*|_-()+=[]{};\\'\"<>,.?/\\~`".Contains(c));
        return hasNumber && hasSymbol;
    }

    /// <summary>
    /// Creates a new user account from an AccountViewModel.
    /// </summary>
    /// <param name="accountViewModel">The account view model containing user data.</param>
    /// <returns>A tuple containing the result and the created user (if successful).</returns>
    public async Task<(IdentityResult result, ApplicationUser? user)> CreateAccountAsync(AccountViewModel accountViewModel)
    {
        // Validate the view model
        var validationErrors = ValidateAccountViewModel(accountViewModel);
        if (validationErrors.Count > 0)
        {
            var result = IdentityResult.Failed(validationErrors
                .Select(e => new IdentityError { Description = e })
                .ToArray());
            return (result, null);
        }

        var user = new ApplicationUser
        {
            UserName = accountViewModel.Username,
            Email = accountViewModel.Email,
            EmailConfirmed = false,
            IsAdmin = false // New users cannot be admins
        };

        var createResult = await _userManager.CreateAsync(user, accountViewModel.Password);

        if (createResult.Succeeded)
        {
            _logger.LogInformation($"User account created successfully for username: {accountViewModel.Username}");
        }
        else
        {
            _logger.LogWarning($"Failed to create user account for username: {accountViewModel.Username}");
        }

        return (createResult, createResult.Succeeded ? user : null);
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
        var user = await FindUserByEmailAsync(email);
        return user != null;
    }

    /// <summary>
    /// Checks if a username is already in use.
    /// </summary>
    /// <param name="username">The username to check.</param>
    /// <returns>True if the username is in use, false otherwise.</returns>
    public async Task<bool> IsUsernameInUseAsync(string username)
    {
        var user = await FindUserByUsernameAsync(username);
        return user != null;
    }

    /// <summary>
    /// Finds the current system owner account.
    /// </summary>
    /// <returns>The owner ApplicationUser if found, null otherwise.</returns>
    public async Task<ApplicationUser?> FindOwnerAsync()
    {
        return await _userManager.Users.FirstOrDefaultAsync(u => u.IsOwner);
    }

    /// <summary>
    /// Promotes a user to owner status.
    /// Only one owner should exist in the system at a time.
    /// </summary>
    /// <param name="userId">The ID of the user to promote to owner.</param>
    /// <param name="requestingUser">The user requesting the change. Must be an admin.</param>
    /// <returns>A result indicating success or failure.</returns>
    public async Task<IdentityResult> PromoteUserToOwnerAsync(string userId, ApplicationUser requestingUser)
    {
        if (!requestingUser.IsAdmin)
        {
            return IdentityResult.Failed(new IdentityError 
            { 
                Description = "Only admin users can promote users to owner." 
            });
        }

        var userToPromote = await _userManager.FindByIdAsync(userId);
        if (userToPromote == null)
        {
            return IdentityResult.Failed(new IdentityError 
            { 
                Description = "User not found." 
            });
        }

        // If there's already an owner, demote them first
        var currentOwner = await FindOwnerAsync();
        if (currentOwner != null)
        {
            currentOwner.DemoteFromOwner(requestingUser);
            await _userManager.UpdateAsync(currentOwner);
        }

        userToPromote.PromoteToOwner(requestingUser);
        var result = await _userManager.UpdateAsync(userToPromote);

        if (result.Succeeded)
        {
            _logger.LogInformation($"User {userToPromote.UserName} promoted to owner by {requestingUser.UserName}");
        }
        else
        {
            _logger.LogWarning($"Failed to promote user {userToPromote.UserName} to owner");
        }

        return result;
    }

    /// <summary>
    /// Demotes the owner from owner status to regular admin.
    /// This requires an admin user to request the change.
    /// </summary>
    /// <param name="ownerId">The ID of the owner to demote.</param>
    /// <param name="requestingUser">The user requesting the change. Must be an admin.</param>
    /// <returns>A result indicating success or failure.</returns>
    public async Task<IdentityResult> DemoteOwnerAsync(string ownerId, ApplicationUser requestingUser)
    {
        if (!requestingUser.IsAdmin)
        {
            return IdentityResult.Failed(new IdentityError 
            { 
                Description = "Only admin users can demote the owner." 
            });
        }

        var owner = await _userManager.FindByIdAsync(ownerId);
        if (owner == null)
        {
            return IdentityResult.Failed(new IdentityError 
            { 
                Description = "User not found." 
            });
        }

        if (!owner.IsOwner)
        {
            return IdentityResult.Failed(new IdentityError 
            { 
                Description = "User is not the owner." 
            });
        }

        owner.DemoteFromOwner(requestingUser);
        var result = await _userManager.UpdateAsync(owner);

        if (result.Succeeded)
        {
            _logger.LogInformation($"Owner {owner.UserName} demoted to regular admin by {requestingUser.UserName}");
        }
        else
        {
            _logger.LogWarning($"Failed to demote owner {owner.UserName}");
        }

        return result;
    }

    /// <summary>
    /// Checks if a specific user is the system owner.
    /// </summary>
    /// <param name="userId">The ID of the user to check.</param>
    /// <returns>True if the user is the owner, false otherwise.</returns>
    public async Task<bool> IsUserOwnerAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user?.IsOwner ?? false;
    }
}
