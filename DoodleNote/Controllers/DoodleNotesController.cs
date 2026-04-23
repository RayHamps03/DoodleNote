using DoodleNote.Data;
using DoodleNote.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoodleNote.Controllers;

public class DoodleNotesController : Controller
{
    private readonly ApplicationDbContext _context;
    private const int PageSize = 10;

    public DoodleNotesController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int page = 1)
    {
        if (page < 1)
        {
            page = 1;
        }

        var totalCount = await _context.DoodleNotes.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)PageSize);

        if (totalPages > 0 && page > totalPages)
        {
            page = totalPages;
        }

        var notes = await _context.DoodleNotes
            .OrderByDescending(n => n.CreatedDate)
            .ThenBy(n => n.NoteId)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        var viewModel = new DoodleNoteListViewModel
        {
            Notes = notes,
            CurrentPage = page,
            TotalPages = totalPages
        };

        return View(viewModel);
    }

    public IActionResult Create()
    {
        return View();
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("NoteTitle,Description")] DoodleNote.Models.DoodleNote note)
    {
        if (ModelState.IsValid)
        {
            note.CreatedDate = DateTime.Now;
            _context.DoodleNotes.Add(note);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(note);
    }
    public async Task<IActionResult> Details(int id)
    {
        DoodleNote.Models.DoodleNote? note = await _context.DoodleNotes.FindAsync(id);
        if (note == null)
        {
            return NotFound();
        }
        DoodleNoteDetailsViewModel viewModel = new DoodleNoteDetailsViewModel
        {
            NoteId = note.NoteId,
            NoteTitle = note.NoteTitle,
            Author = string.Empty, // Not implemented
            Description = note.Description,
            CreatedDate = note.CreatedDate
        };
        return View(viewModel);
    }
}