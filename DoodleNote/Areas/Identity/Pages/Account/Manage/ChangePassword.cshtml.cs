using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using DoodleNote.Features.Admin.Models;

namespace DoodleNote.Areas.Identity.Pages.Account.Manage;

[Authorize]
public class ChangePasswordModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<ChangePasswordModel> _logger;

    public ChangePasswordModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ILogger<ChangePasswordModel> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "{0} must be between {2} and {1} characters.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToPage("/Account/Login");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToPage("/Account/Login");
        }


        var changeResult = await _userManager.ChangePasswordAsync(user, Input.CurrentPassword, Input.NewPassword);
        if (!changeResult.Succeeded)
        {
            foreach (var error in changeResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return Page();
        }

        // Reload the persisted user to ensure we have up-to-date security fields.
        var persisted = await _userManager.FindByIdAsync(user.Id);
        if (persisted == null)
        {
            _logger.LogWarning("User disappeared after password change: {UserId}", user.Id);
            await _signInManager.SignOutAsync();
            return RedirectToPage("/Account/Login");
        }

        // Ensure the security stamp is updated. Some stores do this automatically
        // during ChangePasswordAsync, but explicitly updating is a safe fallback.
        try
        {
            await _userManager.UpdateSecurityStampAsync(persisted);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "UpdateSecurityStampAsync failed for user {UserId}", persisted.Id);
        }

        // Refresh sign-in so cookie contains current security stamp and claims.
        await _signInManager.RefreshSignInAsync(persisted);
        _logger.LogInformation("User {UserId} changed their password successfully.", user.Id);

        TempData["StatusMessage"] = "Your password has been changed.";
        return RedirectToPage();
    }
}
