using DoodleNote.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using DoodleNote.Data;
using Microsoft.EntityFrameworkCore;

namespace DoodleNote.Controllers;

/// <summary>
/// Handles general application pages and error handling.
/// </summary>
public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int page = 1)
    {
        int pageSize = 12;
        var notesQuery = _context.DoodleNotes
            .Include(n => n.User)
            .OrderByDescending(n => n.CreatedDate);
            
        var totalNotes = await notesQuery.CountAsync();
        var notes = await notesQuery.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalNotes / (double)pageSize);

        return View(notes);
    }

    public IActionResult Privacy() => View();

    /// <summary>
    /// Displays error page with request tracking ID for debugging.
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel 
    { 
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier 
    });
}
