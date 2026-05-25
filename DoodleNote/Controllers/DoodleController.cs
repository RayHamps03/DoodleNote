using Microsoft.AspNetCore.Mvc;

namespace DoodleNote.Controllers;

public class DoodleController : Controller
{
	public IActionResult Index()
	{
		return View();
	}

	[HttpGet] 
	public IActionResult Upload()
	{
		return View();
	}
}

