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

        if (page < 1) page = 1;

        int totalAccounts = _userManager.Users.Count();
        int totalPages = (totalAccounts + PageSize - 1) / PageSize;

        if (page > totalPages && totalPages > 0) page = totalPages;

        bool isOwner = await _roleService.UserHasRoleAsync(currentUser, RoleNames.Owner);
        bool isAdmin = await _roleService.UserHasRoleAsync(currentUser, RoleNames.Admin);

        List<ApplicationUser> users = await _userManager.Users
            .OrderBy(u => u.UserName)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        // Batch load all roles for the users on this page
        var userIds = users.Select(u => u.Id).ToList();
        var userRolesMap = await _roleService.GetUserRolesMapAsync(userIds);

        List<AccountManagementViewModel> accounts = new(users.Count);
        foreach (ApplicationUser user in users)
        {
            IList<string> userRoles = userRolesMap.ContainsKey(user.Id) ? userRolesMap[user.Id] : new List<string>();
            bool isProtectedRole = userRoles.Contains(RoleNames.Admin) || userRoles.Contains(RoleNames.Owner);
            bool canRemove = isOwner || !isProtectedRole;
            bool canModifyRoles = isOwner || (isAdmin && !isProtectedRole);

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

        var (currentUserError, currentUser) = await GetCurrentUserAsync();
        if (currentUserError != null) return currentUserError;

        var (targetUserError, targetUser) = await GetTargetUserAsync(userId);
        if (targetUserError != null) return targetUserError;

        if (currentUser!.Id == targetUser!.Id)
        {
            return BadRequest("Cannot remove your own account.");
        }

        bool isOwner = await _roleService.UserHasRoleAsync(currentUser, RoleNames.Owner);
        IList<string> targetRoles = await _roleService.GetUserRolesAsync(targetUser);

        // Owner can remove any account; Admin cannot remove admin or owner accounts
        if (!isOwner && RoleService.IsProtectedRole(targetRoles))
        {
            return Forbid();
        }

        IdentityResult result = await _userManager.DeleteAsync(targetUser);

        if (result.Succeeded)
        {
            return RedirectToAction(nameof(Index), new { page });
        }

        return BadRequest("Failed to remove account.");
    }

    /// <summary>
    /// Modifies user roles (assign or remove). Owner can assign any role; Admin can only modify non-protected roles.
    /// </summary>
    private async Task<IActionResult> ModifyUserRoleAsync(string userId, string role, bool isAssignment, int page = 1)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(role))
        {
            return BadRequest("User ID and role are required.");
        }

        if (!RoleService.IsValidRole(role))
        {
            return BadRequest("Invalid role.");
        }

        var (currentUserError, currentUser) = await GetCurrentUserAsync();
        if (currentUserError != null) return currentUserError;

        var (targetUserError, targetUser) = await GetTargetUserAsync(userId);
        if (targetUserError != null) return targetUserError;

        IList<string> targetRoles = await _roleService.GetUserRolesAsync(targetUser!);

        if (!await _roleService.CanModifyTargetUserAsync(currentUser!, targetRoles))
        {
            return Forbid();
        }

        IdentityResult result = isAssignment
            ? await _roleService.AddUserToRoleAsync(targetUser, role)
            : await _roleService.RemoveUserFromRoleAsync(targetUser, role);

        if (result.Succeeded)
        {
            return RedirectToAction(nameof(Index), new { page });
        }

        return BadRequest($"Failed to {(isAssignment ? "assign" : "remove")} role.");
    }

    /// <summary>
    /// Assigns a role to a user. Owner can assign any role; Admin can only assign to normal users.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> AssignRole(string userId, string role, int page = 1)
        => ModifyUserRoleAsync(userId, role, isAssignment: true, page);

    /// <summary>
    /// Removes a role from a user. Owner can remove any role; Admin can only remove from normal users.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> RemoveRole(string userId, string role, int page = 1)
        => ModifyUserRoleAsync(userId, role, isAssignment: false, page);

    /// <summary>
    /// Gets the current authenticated user, returning Unauthorized if not found.
    /// </summary>
    private async Task<(IActionResult? error, ApplicationUser? user)> GetCurrentUserAsync()
    {
        ApplicationUser? currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return (Unauthorized(), null);
        }
        return (null, currentUser);
    }

    /// <summary>
    /// Gets a target user by ID, returning appropriate error if not found.
    /// </summary>
    private async Task<(IActionResult? error, ApplicationUser? user)> GetTargetUserAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return (BadRequest("User ID is required."), null);
        }

        ApplicationUser? targetUser = await _userManager.FindByIdAsync(userId);
        if (targetUser == null)
        {
            return (NotFound(), null);
        }
        return (null, targetUser);
    }
}
