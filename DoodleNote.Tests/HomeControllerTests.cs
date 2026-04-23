using DoodleNote.Controllers;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using Microsoft.AspNetCore.Http;

namespace DoodleNote.Tests
{
    public class HomeControllerTests
    {
        [Fact]
        public void Index_ReturnsViewResult()
        {
            var controller = new HomeController();
            var result = controller.Index();
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Privacy_ReturnsViewResult()
        {
            var controller = new HomeController();
            var result = controller.Privacy();
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Error_ReturnsViewResultWithErrorViewModel()
        {
            var controller = new HomeController();
            var httpContext = new DefaultHttpContext();
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            var result = controller.Error() as ViewResult;
            Assert.NotNull(result);
            Assert.IsType<DoodleNote.Models.ErrorViewModel>(result.Model);
        }
    }
}
