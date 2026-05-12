using DoodleNote.Features.Admin.Constants;
using DoodleNote.Features.Admin.Models;
using DoodleNote.Features.Admin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoodleNote.Controllers;

/// <summary>
/// Controller for admin account management operations.
/// All actions require admin or owner role.
/// </summary>
[Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Owner}")]
public class AdminAccountsController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleService _roleService;
    private const int PageSize = 30; // 3 columns × 10 rows

    public AdminAccountsController(UserManager<ApplicationUser> userManager, RoleService roleService)
    {
        _userManager = userManager;
        _roleService = roleService;
    }

    /// <summary>
    /// Displays paginated list of all user accounts with management options (3 columns × 10 rows).
    /// </summary>
    public async Task<IActionResult> Index(int page = 1)
    {
        ApplicationUser? currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Unauthorized();
        }

        bool isOwner = await _roleService.UserHasRoleAsync(currentUser, RoleNames.Owner);
        bool isAdmin = await _roleService.UserHasRoleAsync(currentUser, RoleNames.Admin);

        if (page < 1) page = 1;

        int totalAccounts = _userManager.Users.Count();
        int totalPages = (totalAccounts + PageSize - 1) / PageSize;

        if (page > totalPages && totalPages > 0) page = totalPages;

        List<ApplicationUser> users = await _userManager.Users
            .OrderBy(u => u.UserName)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        List<AccountManagementViewModel> accounts = new();
        foreach (ApplicationUser user in users)
        {
            IList<string> userRoles = await _roleService.GetUserRolesAsync(user);
            bool canRemove = isOwner || (!userRoles.Contains(RoleNames.Admin) && !userRoles.Contains(RoleNames.Owner));

            // Owner can modify all roles. Admin can modify normal users but not admins or owners.
            bool canModifyRoles = isOwner || (isAdmin && !userRoles.Contains(RoleNames.Admin) && !userRoles.Contains(RoleNames.Owner));

            accounts.Add(new AccountManagementViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Roles = userRoles,
                CanRemove = canRemove,
                CanModifyRoles = canModifyRoles
            });
        }

        PaginatedAccountsViewModel viewModel = new()
        {
            Accounts = accounts,
            CurrentPage = page,
            TotalPages = totalPages,
            TotalAccounts = totalAccounts,
            IsOwner = isOwner,
            IsAdmin = isAdmin
        };

        return View(viewModel);
    }

    /// <summary>
    /// Removes a user account from the database.
    /// Owner can remove any account; Admin can only remove non-admin/non-owner accounts.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveAccount(string userId, int page = 1)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest("User ID is required.");
        }

        ApplicationUser? currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Unauthorized();
        }

        ApplicationUser? targetUser = await _userManager.FindByIdAsync(userId);
        if (targetUser == null)
        {
            return NotFound();
        }

        bool isOwner = await _roleService.UserHasRoleAsync(currentUser, RoleNames.Owner);
        IList<string> targetRoles = await _roleService.GetUserRolesAsync(targetUser);

        // Owner can remove any account; Admin cannot remove admin or owner accounts
        if (!isOwner && (targetRoles.Contains(RoleNames.Admin) || targetRoles.Contains(RoleNames.Owner)))
        {
            return Forbid();
        }

        // Prevent self-deletion
        if (currentUser.Id == targetUser.Id)
        {
            return BadRequest("Cannot remove your own account.");
        }

        IdentityResult result = await _userManager.DeleteAsync(targetUser);

        if (result.Succeeded)
        {
            return RedirectToAction(nameof(Index), new { page });
        }

        return BadRequest("Failed to remove account.");
    }

    /// <summary>
    /// Assigns a role to a user. Owner can assign any role; Admin can only assign to normal users.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignRole(string userId, string role, int page = 1)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(role))
        {
            return BadRequest("User ID and role are required.");
        }

        ApplicationUser? currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Unauthorized();
        }

        bool isOwner = await _roleService.UserHasRoleAsync(currentUser, RoleNames.Owner);
        bool isAdmin = await _roleService.UserHasRoleAsync(currentUser, RoleNames.Admin);

        ApplicationUser? targetUser = await _userManager.FindByIdAsync(userId);
        if (targetUser == null)
        {
            return NotFound();
        }

        // Validate role
        if (!IsValidRole(role))
        {
            return BadRequest("Invalid role.");
        }

        IList<string> targetRoles = await _roleService.GetUserRolesAsync(targetUser);

        // Owner can modify all; Admin cannot modify admins or owners
        if (!isOwner && (isAdmin && (targetRoles.Contains(RoleNames.Admin) || targetRoles.Contains(RoleNames.Owner))))
        {
            return Forbid();
        }

        if (!isOwner && !isAdmin)
        {
            return Forbid();
        }

        IdentityResult result = await _roleService.AddUserToRoleAsync(targetUser, role);

        if (result.Succeeded)
        {
            return RedirectToAction(nameof(Index), new { page });
        }

        return BadRequest("Failed to assign role.");
    }

    /// <summary>
    /// Removes a role from a user. Owner can remove any role; Admin can only remove from normal users.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveRole(string userId, string role, int page = 1)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(role))
        {
            return BadRequest("User ID and role are required.");
        }

        ApplicationUser? currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Unauthorized();
        }

        bool isOwner = await _roleService.UserHasRoleAsync(currentUser, RoleNames.Owner);
        bool isAdmin = await _roleService.UserHasRoleAsync(currentUser, RoleNames.Admin);

        ApplicationUser? targetUser = await _userManager.FindByIdAsync(userId);
        if (targetUser == null)
        {
            return NotFound();
        }

        // Validate role
        if (!IsValidRole(role))
        {
            return BadRequest("Invalid role.");
        }

        IList<string> targetRoles = await _roleService.GetUserRolesAsync(targetUser);

        // Owner can modify all; Admin cannot modify admins or owners
        if (!isOwner && (isAdmin && (targetRoles.Contains(RoleNames.Admin) || targetRoles.Contains(RoleNames.Owner))))
        {
            return Forbid();
        }

        if (!isOwner && !isAdmin)
        {
            return Forbid();
        }

        IdentityResult result = await _roleService.RemoveUserFromRoleAsync(targetUser, role);

        if (result.Succeeded)
        {
            return RedirectToAction(nameof(Index), new { page });
        }

        return BadRequest("Failed to remove role.");
    }

    /// <summary>
    /// Validates that a role is one of the application's defined roles.
    /// </summary>
    private static bool IsValidRole(string role)
    {
        return role == RoleNames.Admin || role == RoleNames.Owner || role == RoleNames.User;
    }
}
