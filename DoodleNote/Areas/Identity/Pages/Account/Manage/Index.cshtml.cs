using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DoodleNote.Features.Admin.Models;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DoodleNote.Areas.Identity.Pages.Account.Manage;

[Authorize]
public class IndexModel : PageModel
{
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly SignInManager<ApplicationUser> _signInManager;
	private readonly ILogger<IndexModel> _logger;

	public IndexModel(
		UserManager<ApplicationUser> userManager,
		SignInManager<ApplicationUser> signInManager,
		ILogger<IndexModel> logger)
	{
		_userManager = userManager;
		_signInManager = signInManager;
		_logger = logger;
	}

	public ApplicationUser CurrentUser { get; set; } = default!;

	public async Task<IActionResult> OnGetAsync()
	{
		var user = await _userManager.GetUserAsync(User);
		if (user == null)
		{
			_logger.LogWarning("User not found during profile page load");
			return Redirect("/Identity/Account/Logout");
		}

		CurrentUser = user;
		return Page();
	}

	public async Task<IActionResult> OnPostUpdateUsernameAsync()
	{
		Request.Body.Position = 0;
		using var reader = new StreamReader(Request.Body);
		var jsonBody = await reader.ReadToEndAsync();

		if (string.IsNullOrWhiteSpace(jsonBody))
		{
			return new JsonResult(new { message = "Invalid request." }) { StatusCode = 400 };
		}

		try
		{
			if (JsonSerializer.Deserialize<Dictionary<string, string>>(jsonBody) is not { } requestData ||
				!requestData.TryGetValue("newUsername", out var newUsernameValue) ||
				string.IsNullOrWhiteSpace(newUsernameValue))
			{
				return new JsonResult(new { message = "Invalid request format." }) { StatusCode = 400 };
			}

			var newUsername = newUsernameValue.Trim();

			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				return new JsonResult(new { message = "User not found." }) { StatusCode = 404 };
			}

			// Validate username length
			if (newUsername.Length < 3 || newUsername.Length > 256)
			{
				return new JsonResult(new { message = "Username must be between 3 and 256 characters." }) { StatusCode = 400 };
			}

			// Validate username format - alphanumeric and some common characters only
			if (!Regex.IsMatch(newUsername, @"^[a-zA-Z0-9._\-]+$"))
			{
				return new JsonResult(new { message = "Username contains invalid characters." }) { StatusCode = 400 };
			}

			if (newUsername == user.UserName)
			{
				return new JsonResult(new { message = "Username unchanged." });
			}

			var existingUser = await _userManager.FindByNameAsync(newUsername);
			if (existingUser != null)
			{
				return new JsonResult(new { message = "This username is already in use." }) { StatusCode = 400 };
			}

			var result = await _userManager.SetUserNameAsync(user, newUsername);
			if (!result.Succeeded)
			{
				_logger.LogError("Failed to update username for user {UserId}: {ErrorCount} errors", user.Id, result.Errors.Count());
				return new JsonResult(new { message = "Failed to update username. Please try again." }) { StatusCode = 400 };
			}

			_logger.LogInformation("Username updated successfully for user {UserId}", user.Id);
			await _signInManager.RefreshSignInAsync(user);
			return new JsonResult(new { message = "Username updated successfully.", username = user.UserName });
		}
		catch (JsonException)
		{
			return new JsonResult(new { message = "Invalid request format." }) { StatusCode = 400 };
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Exception in OnPostUpdateUsernameAsync for user {UserId}", _userManager.GetUserId(User));
			return new JsonResult(new { message = "An error occurred. Please try again." }) { StatusCode = 500 };
		}
	}

	public async Task<IActionResult> OnPostUpdatePhoneAsync()
	{
		Request.Body.Position = 0;
		using var reader = new StreamReader(Request.Body);
		var jsonBody = await reader.ReadToEndAsync();

		if (string.IsNullOrWhiteSpace(jsonBody))
		{
			return new JsonResult(new { message = "Invalid request." }) { StatusCode = 400 };
		}

		try
		{
			if (JsonSerializer.Deserialize<Dictionary<string, string>>(jsonBody) is not { } requestData ||
				!requestData.TryGetValue("newPhone", out var newPhoneValue))
			{
				return new JsonResult(new { message = "Invalid request format." }) { StatusCode = 400 };
			}

			var newPhone = newPhoneValue?.Trim() ?? "";

			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				return new JsonResult(new { message = "User not found." }) { StatusCode = 404 };
			}

			// Validate phone number format - allow empty or valid format
			if (!string.IsNullOrEmpty(newPhone))
			{
				if (!Regex.IsMatch(newPhone, @"^[\d\s\-\+\(\)\.]+$") || newPhone.Length < 10 || newPhone.Length > 20)
				{
					return new JsonResult(new { message = "Invalid phone number format." }) { StatusCode = 400 };
				}
			}

			if (newPhone == (user.PhoneNumber ?? ""))
			{
				return new JsonResult(new { message = "Phone number unchanged." });
			}

			var result = await _userManager.SetPhoneNumberAsync(user, newPhone);
			if (!result.Succeeded)
			{
				_logger.LogError("Failed to update phone number for user {UserId}: {ErrorCount} errors", user.Id, result.Errors.Count());
				return new JsonResult(new { message = "Failed to update phone number. Please try again." }) { StatusCode = 400 };
			}

			return new JsonResult(new { message = "Phone number updated successfully.", phoneNumber = user.PhoneNumber });
		}
		catch (JsonException)
		{
			return new JsonResult(new { message = "Invalid request format." }) { StatusCode = 400 };
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Exception in OnPostUpdatePhoneAsync for user {UserId}", _userManager.GetUserId(User));
			return new JsonResult(new { message = "An error occurred. Please try again." }) { StatusCode = 500 };
		}
	}
}
