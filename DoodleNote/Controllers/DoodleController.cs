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

		string token = Guid.NewGuid().ToString("N");

		string pendingDir = Path.Combine(_env.WebRootPath, "uploads", "pending");
		Directory.CreateDirectory(pendingDir);

		string pendingFilePath = Path.Combine(pendingDir, $"{token}.png");
		await System.IO.File.WriteAllBytesAsync(pendingFilePath, pngBytes);

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

		string pendingFilePath = Path.Combine(_env.WebRootPath, "uploads", "pending", $"{token}.png");
		if (!System.IO.File.Exists(pendingFilePath))
		{
			return NotFound("Pending upload not found (it may have expired).");
		}

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

		string pendingFilePath = Path.Combine(_env.WebRootPath, "uploads", "pending", $"{model.Token}.png");
		if (!System.IO.File.Exists(pendingFilePath))
		{
			return NotFound("Pending upload not found (it may have expired).");
		}

		DoodleNote.Models.DoodleNote note = new()
		{
			NoteTitle = model.NoteTitle,
			Description = model.Description,
			CreatedDate = DateTime.Now
		};

		_context.DoodleNotes.Add(note);
		await _context.SaveChangesAsync();

		string doodlesDir = Path.Combine(_env.WebRootPath, "uploads", "doodles");
		Directory.CreateDirectory(doodlesDir);

		string finalFileName = $"{note.NoteId}-{model.Token}.png";
		string finalFilePath = Path.Combine(doodlesDir, finalFileName);

		System.IO.File.Move(pendingFilePath, finalFilePath, overwrite: true);

		note.ImagePath = $"/uploads/doodles/{finalFileName}";
		await _context.SaveChangesAsync();

		return RedirectToAction("Details", "DoodleNotes", new { id = note.NoteId });
	}

	private static bool TryParsePngDataUrl(string dataUrl, out byte[] pngBytes)
	{
		pngBytes = [];

		const string prefix = "data:image/png;base64,";
		if (!dataUrl.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}

		string base64 = dataUrl[prefix.Length..];
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

