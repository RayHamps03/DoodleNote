using DoodleNote.Features.Admin.Constants;
using DoodleNote.Features.Admin.Models;
using Microsoft.AspNetCore.Identity;

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
        List<string> rolesToCreate = new();

        // Batch check which roles exist
        foreach (string role in roles)
        {
            if (await _roleManager.RoleExistsAsync(role))
            {
                rolesToCreate.Add(role);
            }

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

        // Create missing roles
        foreach (string role in rolesToCreate)
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
            _logger.LogInformation("User {UserName} added to role {Role}.", user.UserName, role);
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
            _logger.LogInformation("User {UserName} removed from role {Role}.", user.UserName, role);
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

    /// <summary>
    /// Gets roles for multiple users efficiently with optimized batch querying.
    /// </summary>
    public async Task<Dictionary<string, IList<string>>> GetUserRolesMapAsync(IEnumerable<string> userIds)
    {
        var roleMap = new Dictionary<string, IList<string>>();
        var userIdList = userIds.ToList();

        if (userIdList.Count == 0)
        {
            return roleMap;
        }

        // Batch fetch users in a single query to avoid N+1
        var users = await _userManager.Users
            .Where(u => userIdList.Contains(u.Id))
            .ToListAsync<ApplicationUser>();

        // Load roles for each user
        foreach (var user in users)
        {
            roleMap[user.Id] = await _userManager.GetRolesAsync(user);
        }

        return roleMap;
    }

    /// <summary>
    /// Checks if a role is a protected role (Admin or Owner).
    /// </summary>
    public static bool IsProtectedRole(IList<string> userRoles)
    {
        return userRoles.Contains(RoleNames.Admin) || userRoles.Contains(RoleNames.Owner);
    }

    /// <summary>
    /// Checks if a role is one of the application's defined roles.
    /// </summary>
    public static bool IsValidRole(string role)
    {
        return role == RoleNames.Admin || role == RoleNames.Owner || role == RoleNames.User;
    }

    /// <summary>
    /// Determines if a user can modify target user's roles.
    /// Owner can modify any role; Admin can only modify non-protected roles.
    /// </summary>
    public async Task<bool> CanModifyTargetUserAsync(ApplicationUser requestingUser, IList<string> targetUserRoles)
    {
        bool isOwner = await UserHasRoleAsync(requestingUser, RoleNames.Owner);
        if (isOwner) return true;

        bool isAdmin = await UserHasRoleAsync(requestingUser, RoleNames.Admin);
        return isAdmin && !IsProtectedRole(targetUserRoles);
    }
}
