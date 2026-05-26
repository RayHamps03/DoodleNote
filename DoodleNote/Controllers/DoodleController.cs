using DoodleNote.Data;
using DoodleNote.Features.DoodleUpload.Models;
using DoodleNote.Models;
using Microsoft.AspNetCore.Mvc;

namespace DoodleNote.Controllers;

public class DoodleController(ApplicationDbContext context, IWebHostEnvironment env) : Controller
{
	private readonly ApplicationDbContext _context = context;
	private readonly IWebHostEnvironment _env = env;

	public IActionResult Index() => View();

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> StartUpload([FromBody] StartDoodleUploadRequest request)
	{
		if (request?.PngDataUrl is null || string.IsNullOrWhiteSpace(request.PngDataUrl))
		{
			return BadRequest("Missing image data.");
		}

		if (!TryParsePngDataUrl(request.PngDataUrl, out byte[] pngBytes))
		{
			return BadRequest("Invalid image format. Expected a PNG data URL.");
		}

		// Generate a unique token for this upload session
		string token = Guid.NewGuid().ToString("N");

		// Save the PNG bytes to a temporary location with the token as the filename
		string pendingDir = Path.Combine(_env.WebRootPath, "uploads", "pending");
		Directory.CreateDirectory(pendingDir);

		// Save the file as {token}.png in the pending directory
		string pendingFilePath = Path.Combine(pendingDir, $"{token}.png");
		await System.IO.File.WriteAllBytesAsync(pendingFilePath, pngBytes);

		// Return the token to the client so they can confirm the upload
		string redirectUrl = Url.Action(nameof(ConfirmUpload), new { token }) ?? $"/Doodle/ConfirmUpload?token={token}";
		return Ok(new { redirectUrl });
	}

	[HttpGet]
	public IActionResult ConfirmUpload(string token)
	{
		if (string.IsNullOrWhiteSpace(token) || !Guid.TryParse(token, out _))
		{
			return BadRequest("Invalid token.");
		}

		// Check if the pending file exists for the given token
		string pendingFilePath = Path.Combine(_env.WebRootPath, "uploads", "pending", $"{token}.png");
		if (!System.IO.File.Exists(pendingFilePath))
		{
			return NotFound("Pending upload not found (it may have expired).");
		}

		// Prepare the view model with the token and a URL to preview the uploaded image
		ConfirmDoodleUploadViewModel vm = new()
		{
			Token = token,
			PreviewUrl = Url.Content($"~/uploads/pending/{token}.png")
		};

		return View(vm);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> ConfirmUpload(ConfirmDoodleUploadViewModel model)
	{
		if (!ModelState.IsValid)
		{
			// Re-hydrate preview URL on validation failure
			model.PreviewUrl = Url.Content($"~/uploads/pending/{model.Token}.png");
			return View(model);
		}


		if (!Guid.TryParse(model.Token, out _))
		{
			return BadRequest("Invalid token.");
		}

		// Check if the pending file exists for the given token
		string pendingFilePath = Path.Combine(_env.WebRootPath, "uploads", "pending", $"{model.Token}.png");
		if (!System.IO.File.Exists(pendingFilePath))
		{
			return NotFound("Pending upload not found (it may have expired).");
		}

		// Create a new Doodle Note record in the database
		DoodleNote.Models.DoodleNote note = new()
		{
			NoteTitle = model.NoteTitle,
			Description = model.Description,
			CreatedDate = DateTime.Now
		};

		_context.DoodleNotes.Add(note);
		await _context.SaveChangesAsync();

		// Redirect user to uploaded Doodle Note page
		string doodlesDir = Path.Combine(_env.WebRootPath, "uploads", "doodles");
		Directory.CreateDirectory(doodlesDir);

		string finalFileName = $"{note.NoteId}-{model.Token}.png";
		string finalFilePath = Path.Combine(doodlesDir, finalFileName);

		System.IO.File.Move(pendingFilePath, finalFilePath, overwrite: true);

		note.ImagePath = $"/uploads/doodles/{finalFileName}";
		await _context.SaveChangesAsync();

		return RedirectToAction("Details", "DoodleNotes", new { id = note.NoteId });
	}

	/// <summary>
	/// Helper method to parse a PNG data URL and extract the byte array. 
	/// </summary>
	/// <param name="dataUrl">The data URL string expected to be in the format "data:image/png;base64,..."</param>
	/// <param name="pngBytes">The output byte array containing the PNG data if parsing is successful; otherwise, an empty array.</param>
	/// <returns>Returns false if the format is invalid or if decoding fails.</returns>
	private static bool TryParsePngDataUrl(string dataUrl, out byte[] pngBytes)
	{
		pngBytes = [];

		// Basic validation for PNG data URL format
		const string prefix = "data:image/png;base64,";
		if (!dataUrl.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}

		// Extract the base64 portion of the data URL
		string base64 = dataUrl[prefix.Length..];
		// Attempt to decode the base64 string into bytes
		try
		{
			pngBytes = Convert.FromBase64String(base64);
			return pngBytes.Length > 0;
		}
		catch
		{
			return false;
		}
	}
}

