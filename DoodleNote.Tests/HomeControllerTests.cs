#nullable enable
using DoodleNote.Controllers;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using Microsoft.AspNetCore.Http;

namespace DoodleNote.Tests;

public class HomeControllerTests
{
    [Fact]
    public void Index_ReturnsViewResult()
    {
        HomeController controller = new HomeController();
        IActionResult result = controller.Index();
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void Privacy_ReturnsViewResult()
    {
        HomeController controller = new HomeController();
        IActionResult result = controller.Privacy();
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void Error_ReturnsViewResultWithErrorViewModel()
    {
        HomeController controller = new HomeController();
        DefaultHttpContext httpContext = new DefaultHttpContext();
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        ViewResult? result = controller.Error() as ViewResult;
        Assert.NotNull(result);
        Assert.IsType<DoodleNote.Models.ErrorViewModel>(result.Model);
    }
}