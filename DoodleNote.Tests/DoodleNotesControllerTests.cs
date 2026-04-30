#nullable enable
using System;
using System.Threading.Tasks;
using DoodleNote.Controllers;
using DoodleNote.Data;
using DoodleNote.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DoodleNote.Tests;

public class DoodleNotesControllerTests
{
    private ApplicationDbContext CreateInMemoryContext()
    {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task Index_ReturnsViewWithPaginatedNotes()
    {
        // Arrange
        ApplicationDbContext context = CreateInMemoryContext();
        for (int i = 1; i <= 15; i++)
        {
            context.DoodleNotes.Add(new DoodleNote.Models.DoodleNote
            {
                NoteTitle = $"Note {i}",
                CreatedDate = DateTime.Now
            });
        }
        await context.SaveChangesAsync();

        DoodleNotesController controller = new DoodleNotesController(context);

        // Act
        IActionResult result = await controller.Index(1);

        // Assert
        ViewResult? viewResult = Assert.IsType<ViewResult>(result);
        DoodleNoteListViewModel? model = viewResult.Model as DoodleNoteListViewModel;
        Assert.NotNull(model);
        Assert.Equal(1, model.CurrentPage);
        Assert.Equal(2, model.TotalPages);
        Assert.Equal(10, model.Notes.Count);
    }

    [Fact]
    public async Task Index_WithInvalidPage_ReturnsFirstPage()
    {
        // Arrange
        ApplicationDbContext context = CreateInMemoryContext();
        context.DoodleNotes.Add(new DoodleNote.Models.DoodleNote
        {
            NoteTitle = "Test Note",
            CreatedDate = DateTime.Now
        });
        await context.SaveChangesAsync();

        DoodleNotesController controller = new DoodleNotesController(context);

        // Act
        IActionResult result = await controller.Index(-1);

        // Assert
        ViewResult? viewResult = Assert.IsType<ViewResult>(result);
        DoodleNoteListViewModel? model = viewResult.Model as DoodleNoteListViewModel;
        Assert.NotNull(model);
        Assert.Equal(1, model.CurrentPage);
    }

    [Fact]
    public async Task Index_WithPageBeyondTotal_ReturnsLastPage()
    {
        // Arrange
        ApplicationDbContext context = CreateInMemoryContext();
        for (int i = 1; i <= 5; i++)
        {
            context.DoodleNotes.Add(new DoodleNote.Models.DoodleNote
            {
                NoteTitle = $"Note {i}",
                CreatedDate = DateTime.Now
            });
        }
        await context.SaveChangesAsync();

        DoodleNotesController controller = new DoodleNotesController(context);

        // Act
        IActionResult result = await controller.Index(100);

        // Assert
        ViewResult? viewResult = Assert.IsType<ViewResult>(result);
        DoodleNoteListViewModel? model = viewResult.Model as DoodleNoteListViewModel;
        Assert.NotNull(model);
        Assert.Equal(1, model.CurrentPage);
    }

    [Fact]
    public void Create_Get_ReturnsView()
    {
        // Arrange
        ApplicationDbContext context = CreateInMemoryContext();
        DoodleNotesController controller = new DoodleNotesController(context);

        // Act
        IActionResult result = controller.Create();

        // Assert
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task Create_Post_WithValidModel_SavesAndRedirects()
    {
        // Arrange
        ApplicationDbContext context = CreateInMemoryContext();
        DoodleNotesController controller = new DoodleNotesController(context);
        DoodleNote.Models.DoodleNote newNote = new DoodleNote.Models.DoodleNote
        {
            NoteTitle = "New Note",
            Description = "Test Description"
        };

        // Act
        IActionResult result = await controller.Create(newNote);

        // Assert
        RedirectToActionResult? redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(DoodleNotesController.Index), redirectResult.ActionName);
        DoodleNote.Models.DoodleNote? savedNote = await context.DoodleNotes.FirstOrDefaultAsync();
        Assert.NotNull(savedNote);
        Assert.Equal("New Note", savedNote.NoteTitle);
    }

    [Fact]
    public async Task Create_Post_WithInvalidModel_ReturnsViewWithNote()
    {
        // Arrange
        ApplicationDbContext context = CreateInMemoryContext();
        DoodleNotesController controller = new DoodleNotesController(context);
        controller.ModelState.AddModelError("NoteTitle", "Required");
        DoodleNote.Models.DoodleNote newNote = new DoodleNote.Models.DoodleNote
        {
            NoteTitle = "",
            Description = "Test"
        };

        // Act
        IActionResult result = await controller.Create(newNote);

        // Assert
        ViewResult? viewResult = Assert.IsType<ViewResult>(result);
        DoodleNote.Models.DoodleNote? model = viewResult.Model as DoodleNote.Models.DoodleNote;
        Assert.NotNull(model);
    }

    [Fact]
    public async Task Details_WithValidId_ReturnsViewWithViewModel()
    {
        // Arrange
        ApplicationDbContext context = CreateInMemoryContext();
        DoodleNote.Models.DoodleNote note = new DoodleNote.Models.DoodleNote
        {
            NoteTitle = "Test Note",
            Description = "Test Description",
            CreatedDate = DateTime.Now
        };
        context.DoodleNotes.Add(note);
        await context.SaveChangesAsync();

        DoodleNotesController controller = new DoodleNotesController(context);

        // Act
        IActionResult result = await controller.Details(note.NoteId);

        // Assert
        ViewResult? viewResult = Assert.IsType<ViewResult>(result);
        DoodleNoteDetailsViewModel? model = viewResult.Model as DoodleNoteDetailsViewModel;
        Assert.NotNull(model);
        Assert.Equal(note.NoteId, model.NoteId);
        Assert.Equal("Test Note", model.NoteTitle);
    }

    [Fact]
    public async Task Details_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        ApplicationDbContext context = CreateInMemoryContext();
        DoodleNotesController controller = new DoodleNotesController(context);

        // Act
        IActionResult result = await controller.Details(999);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_Get_WithValidId_ReturnsViewWithNote()
    {
        // Arrange
        ApplicationDbContext context = CreateInMemoryContext();
        DoodleNote.Models.DoodleNote note = new DoodleNote.Models.DoodleNote
        {
            NoteTitle = "Test Note",
            CreatedDate = DateTime.Now
        };
        context.DoodleNotes.Add(note);
        await context.SaveChangesAsync();

        DoodleNotesController controller = new DoodleNotesController(context);

        // Act
        IActionResult result = await controller.Edit(note.NoteId);

        // Assert
        ViewResult? viewResult = Assert.IsType<ViewResult>(result);
        DoodleNote.Models.DoodleNote? model = viewResult.Model as DoodleNote.Models.DoodleNote;
        Assert.NotNull(model);
        Assert.Equal(note.NoteId, model.NoteId);
    }

    [Fact]
    public async Task Edit_Get_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        ApplicationDbContext context = CreateInMemoryContext();
        DoodleNotesController controller = new DoodleNotesController(context);

        // Act
        IActionResult result = await controller.Edit(999);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_Post_WithMatchingIds_UpdatesAndRedirects()
    {
        // Arrange
        ApplicationDbContext context = CreateInMemoryContext();
        DoodleNote.Models.DoodleNote note = new DoodleNote.Models.DoodleNote
        {
            NoteTitle = "Original",
            CreatedDate = DateTime.Now
        };
        context.DoodleNotes.Add(note);
        await context.SaveChangesAsync();

        DoodleNotesController controller = new DoodleNotesController(context);
        note.NoteTitle = "Updated";

        // Act
        IActionResult result = await controller.Edit(note.NoteId, note);

        // Assert
        RedirectToActionResult? redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(DoodleNotesController.Index), redirectResult.ActionName);
    }

    [Fact]
    public async Task Edit_Post_WithMismatchedIds_ReturnsNotFound()
    {
        // Arrange
        ApplicationDbContext context = CreateInMemoryContext();
        DoodleNotesController controller = new DoodleNotesController(context);
        DoodleNote.Models.DoodleNote note = new DoodleNote.Models.DoodleNote
        {
            NoteId = 5,
            NoteTitle = "Test"
        };

        // Act
        IActionResult result = await controller.Edit(1, note);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
}
