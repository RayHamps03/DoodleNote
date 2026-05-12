using DoodleNote.Features.Admin.Constants;
using DoodleNote.Features.Admin.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DoodleNote.Features.Admin.Services;

/// <summary>
/// Service for managing user roles and role-based access control.
/// Replaces property-based admin handling with AspNetRoles for better scalability.
/// Part of the self-contained Admin feature module.
/// </summary>
public class RoleService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ILogger<RoleService> logger)
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly RoleManager<IdentityRole> _roleManager = roleManager;
    private readonly ILogger<RoleService> _logger = logger;

    /// <summary>
    /// Initializes the required application roles in the database.
    /// Should be called once during application startup.
    /// </summary>
    public async Task InitializeRolesAsync()
    {
        string[] roles = { RoleNames.User, RoleNames.Admin, RoleNames.Owner };

        foreach (string role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                IdentityResult result = await _roleManager.CreateAsync(new IdentityRole(role));

                if (result.Succeeded)
                {
                    _logger.LogInformation("Role '{Role}' created successfully.", role);
                    continue;
                }

                foreach (IdentityError error in result.Errors)
                {
                    _logger.LogError("Failed to create role '{Role}'. Error {Code}: {Description}", role, error.Code, error.Description);
                }

                if (await _roleManager.RoleExistsAsync(role))
                {
                    _logger.LogWarning("Role '{Role}' already exists after a failed create attempt. Continuing initialization.", role);
                    continue;
                }

                throw new InvalidOperationException($"Failed to create required role '{role}'. See logs for Identity errors.");
            }
        }
    }

    /// <summary>
    /// Checks if a user has a specific role.
    /// </summary>
    public async Task<bool> UserHasRoleAsync(ApplicationUser user, string role)
    {
        return await _userManager.IsInRoleAsync(user, role);
    }

    /// <summary>
    /// Gets all roles assigned to a user.
    /// </summary>
    public async Task<IList<string>> GetUserRolesAsync(ApplicationUser user)
    {
        return await _userManager.GetRolesAsync(user);
    }

    /// <summary>
    /// Adds a user to a role.
    /// </summary>
    public async Task<IdentityResult> AddUserToRoleAsync(ApplicationUser user, string role)
    {
        IdentityResult result = await _userManager.AddToRoleAsync(user, role);
        if (result.Succeeded)
        {
            _logger.LogInformation($"User {user.UserName} added to role {role}.");
        }
        return result;
    }

    /// <summary>
    /// Removes a user from a role.
    /// </summary>
    public async Task<IdentityResult> RemoveUserFromRoleAsync(ApplicationUser user, string role)
    {
        IdentityResult result = await _userManager.RemoveFromRoleAsync(user, role);
        if (result.Succeeded)
        {
            _logger.LogInformation($"User {user.UserName} removed from role {role}.");
        }
        return result;
    }

    /// <summary>
    /// Promotes a user to the admin role.
    /// </summary>
    public async Task<IdentityResult> PromoteToAdminAsync(ApplicationUser user, ApplicationUser requestingUser)
    {
        if (!await UserHasRoleAsync(requestingUser, RoleNames.Admin))
        {
            return IdentityResult.Failed(new IdentityError { Description = "Only admins can promote users." });
        }

        return await AddUserToRoleAsync(user, RoleNames.Admin);
    }

    /// <summary>
    /// Demotes a user from admin role. Owner role is removed if user is not the system owner.
    /// </summary>
    public async Task<IdentityResult> DemoteFromAdminAsync(ApplicationUser user, ApplicationUser requestingUser)
    {
        if (!await UserHasRoleAsync(requestingUser, RoleNames.Admin))
        {
            return IdentityResult.Failed(new IdentityError { Description = "Only admins can demote users." });
        }

        if (await UserHasRoleAsync(user, RoleNames.Owner))
        {
            return IdentityResult.Failed(new IdentityError { Description = "Cannot demote the system owner from admin role." });
        }

        return await RemoveUserFromRoleAsync(user, RoleNames.Admin);
    }

    /// <summary>
    /// Promotes a user to system owner. Only one owner should exist.
    /// </summary>
    public async Task<IdentityResult> PromoteToOwnerAsync(ApplicationUser user, ApplicationUser requestingUser)
    {
        if (!await UserHasRoleAsync(requestingUser, RoleNames.Admin))
        {
            return IdentityResult.Failed(new IdentityError { Description = "Only admins can promote to owner." });
        }

        // Remove owner role from current owner
        ApplicationUser? currentOwner = await GetUserByRoleAsync(RoleNames.Owner);
        if (currentOwner != null && currentOwner.Id != user.Id)
        {
            await RemoveUserFromRoleAsync(currentOwner, RoleNames.Owner);
        }

        // Add roles to new owner
        IdentityResult result = await AddUserToRoleAsync(user, RoleNames.Owner);
        if (result.Succeeded)
        {
            result = await AddUserToRoleAsync(user, RoleNames.Admin);
        }

        return result;
    }

    /// <summary>
    /// Demotes a user from owner status. Keeps admin role intact.
    /// </summary>
    public async Task<IdentityResult> DemoteFromOwnerAsync(ApplicationUser user, ApplicationUser requestingUser)
    {
        if (!await UserHasRoleAsync(requestingUser, RoleNames.Admin))
        {
            return IdentityResult.Failed(new IdentityError { Description = "Only admins can demote owner." });
        }

        return await RemoveUserFromRoleAsync(user, RoleNames.Owner);
    }

    /// <summary>
    /// Finds the first user with a specific role.
    /// </summary>
    public async Task<ApplicationUser?> GetUserByRoleAsync(string role)
    {
        var users = await _userManager.GetUsersInRoleAsync(role);
        return users.FirstOrDefault();
    }

    /// <summary>
    /// Gets all users with a specific role.
    /// </summary>
    public async Task<IList<ApplicationUser>> GetUsersByRoleAsync(string role)
    {
        return await _userManager.GetUsersInRoleAsync(role);
    }
}
