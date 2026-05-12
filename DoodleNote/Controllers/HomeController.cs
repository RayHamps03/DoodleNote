using DoodleNote.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace DoodleNote.Controllers;

/// <summary>
/// Handles general application pages and error handling.
/// </summary>
public class HomeController : Controller
{
    public IActionResult Index() => View();

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
