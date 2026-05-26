using DoodleNote.Data;
using DoodleNote.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

	/// <summary>
	/// Returns the Create view form.
	/// </summary>
	public IActionResult Create() => View();

	/// <summary>
	/// Saves a new note to the database and redirects to Index.
	/// </summary>
	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Create([Bind("NoteTitle,Description")] DoodleNote.Models.DoodleNote note)
	{
		if (!ModelState.IsValid)
			return View(note);

		note.CreatedDate = DateTime.Now;
		_context.DoodleNotes.Add(note);
		await _context.SaveChangesAsync();
		return RedirectToAction(nameof(Index));
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

		DoodleNoteDetailsViewModel viewModel = new()
		{
			NoteId = note.NoteId,
			NoteTitle = note.NoteTitle,
			Author = string.Empty, // Not implemented
			Description = note.Description ?? string.Empty,
			CreatedDate = note.CreatedDate,
			ImagePath = note.ImagePath
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

	/// <summary>
	/// Checks if a note exists by NoteId.
	/// </summary>
	private Task<bool> DoodleNoteExistsAsync(int id) => _context.DoodleNotes.AnyAsync(e => e.NoteId == id);
}