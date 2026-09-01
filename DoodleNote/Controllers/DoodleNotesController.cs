using DoodleNote.Data;
using DoodleNote.Features.DoodleNotes.Models;
using DoodleNote.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace DoodleNote.Controllers;

/// <summary>
/// Manages CRUD operations for DoodleNote entities with pagination support.
/// </summary>
public class DoodleNotesController(ApplicationDbContext context) : Controller
{
	private readonly ApplicationDbContext _context = context;
	private const int PageSize = 10; // Number of notes displayed per page

	/// <summary>
	/// Retrieves paginated list of notes ordered by creation date.
	/// </summary>
	public async Task<IActionResult> Index(int page = 1)
	{
		const int pageSize = PageSize;
		if (page < 1) page = 1;

		int totalCount = await _context.DoodleNotes.CountAsync();
		int totalPages = (totalCount + pageSize - 1) / pageSize;

		if (page > totalPages && totalPages > 0) page = totalPages;

		int skip = (page - 1) * PageSize;

		List<DoodleNote.Models.DoodleNote> notes = await _context.DoodleNotes
			.OrderByDescending(n => n.CreatedDate)
			.ThenBy(n => n.NoteId)
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.AsNoTracking()
			.ToListAsync();

		return View(new DoodleNoteListViewModel
		{
			Notes = notes,
			CurrentPage = page,
			TotalPages = totalPages
		});
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Create([Bind("NoteTitle,Description")] DoodleNote.Models.DoodleNote note)
	{
		if (ModelState.IsValid)
		{
			_context.DoodleNotes.Add(note);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}
		return View(note);
	}

	/// <summary>
	/// Displays detailed view of a single note with formatted display model.
	/// </summary>
	public async Task<IActionResult> Details(int id)
	{
		DoodleNote.Models.DoodleNote? note = await _context.DoodleNotes
			.AsNoTracking()
			.FirstOrDefaultAsync(n => n.NoteId == id);
		if (note == null) return NotFound();

		List<CommentViewModel> comments = await _context.UserComments
			.AsNoTracking()
			.Where(c => c.NoteId == note.NoteId)
			.OrderByDescending(c => c.CommentId)
			.Select(c => new CommentViewModel
			{
				CommentId = c.CommentId,
				CommentText = c.CommentText,
				Author = _context.Users
					.Where(u => u.Id == c.UserId)
					.Select(u => u.UserName)
					.FirstOrDefault() ?? "Unknown"
			})
			.ToListAsync();

		DoodleNoteDetailsViewModel viewModel = new()
		{
			NoteId = note.NoteId,
			NoteTitle = note.NoteTitle,
			Author = await _context.Users
				.Where(u => u.Id == note.UserId)
				.Select(u => u.UserName)
				.FirstOrDefaultAsync() ?? "Unknown",
			Description = note.Description ?? string.Empty,
			CreatedDate = note.CreatedDate,
			ImagePath = note.ImagePath,
			LikeCount = await _context.UserLikes.CountAsync(l => l.NoteId == note.NoteId),
			IsLikedByCurrentUser = await _context.UserLikes
				.AnyAsync(l => l.NoteId == note.NoteId && l.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier)),
			Comments = comments
		};
		return View(viewModel);
	}

	/// <summary>
	/// Returns the Edit form pre-populated with existing note data.
	/// </summary>
	public async Task<IActionResult> Edit(int id)
	{
		DoodleNote.Models.DoodleNote? note = await _context.DoodleNotes.FindAsync(id);
		return note == null ? NotFound() : View(note);
	}

	/// <summary>
	/// Updates an existing note and persists changes to database.
	/// </summary>
	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Edit(int id, [Bind("NoteId,NoteTitle,Description")] DoodleNote.Models.DoodleNote note)
	{
		if (id != note.NoteId)
			return NotFound();

		if (!ModelState.IsValid)
			return View(note);

		try
		{
			_context.Update(note);
			await _context.SaveChangesAsync();
		}
		catch (DbUpdateConcurrencyException)
		{
			if (!await DoodleNoteExistsAsync(note.NoteId))
				return NotFound();
			throw;
		}
		return RedirectToAction(nameof(Index));
	}

	[HttpPost]
	[Authorize]
	public async Task<IActionResult> ToggleNoteLike(NoteLikeViewModel model)
	{
		string? UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

		UserLike? like = await _context.UserLikes
			.FirstOrDefaultAsync(l => l.NoteId == model.NoteId && l.UserId == UserId);

		if (like == null)
		{
			_context.UserLikes.Add(new UserLike { NoteId = model.NoteId, UserId = UserId });
		}
		else
		{
			_context.UserLikes.Remove(like);
		}

		await _context.SaveChangesAsync();
		return NoContent();
	}

	[Authorize]
	public async Task<IActionResult> AddComment(int noteId, string commentText)
	{
		if (string.IsNullOrWhiteSpace(commentText))
			return BadRequest("Comment text cannot be empty.");
		string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
		if (userId == null)
			return Unauthorized();
		// Use the CreateComment method to create a new comment instance
		UserComment comment = new UserComment().CreateComment(commentText, userId, noteId);
		_context.UserComments.Add(comment);
		await _context.SaveChangesAsync();
		return RedirectToAction(nameof(Details), new { id = noteId });
	}

	/// <summary>
	/// Checks if a note exists by NoteId.
	/// </summary>
	private Task<bool> DoodleNoteExistsAsync(int id) => _context.DoodleNotes.AnyAsync(e => e.NoteId == id);
}