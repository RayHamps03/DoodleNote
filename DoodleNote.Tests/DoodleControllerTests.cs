using DoodleNote.Controllers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace DoodleNote.Tests;

public class DoodleControllerTests
{
	[Fact]
	public void Index_ReturnsViewResult()
	{
		DoodleController controller = new();

		IActionResult result = controller.Index();

		Assert.IsType<ViewResult>(result);
	}
}
