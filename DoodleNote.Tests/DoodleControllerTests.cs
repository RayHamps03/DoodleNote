using System;
using System.IO;
using DoodleNote.Controllers;
using DoodleNote.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Xunit;

namespace DoodleNote.Tests;

public class DoodleControllerTests
{
	private sealed class TestWebHostEnvironment : Microsoft.AspNetCore.Hosting.IWebHostEnvironment
	{
		public string ApplicationName { get; set; } = "DoodleNote.Tests";
		public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
		public string WebRootPath { get; set; } = Path.Combine(Path.GetTempPath(), "DoodleNote.Tests", Guid.NewGuid().ToString("N"));
		public string EnvironmentName { get; set; } = "Development";
		public string ContentRootPath { get; set; } = Path.GetTempPath();
		public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
	}

	private static ApplicationDbContext CreateInMemoryContext()
	{
		DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
			.Options;
		return new ApplicationDbContext(options);
	}

	[Fact]
	public void Index_ReturnsViewResult()
	{
		ApplicationDbContext context = CreateInMemoryContext();
		TestWebHostEnvironment env = new();
		Directory.CreateDirectory(env.WebRootPath);
		DoodleController controller = new(context, env);

		IActionResult result = controller.Index();

		Assert.IsType<ViewResult>(result);
	}
}
