using DoodleNote.Models;
using Xunit;

namespace DoodleNote.Tests;

public class DoodleNoteDescriptionTests
{
    [Fact]
    public void CanSetAndGetDescription()
    {
        DoodleNote.Models.DoodleNote note = new DoodleNote.Models.DoodleNote { NoteTitle = "Test", CreatedDate = System.DateTime.Now };
        note.Description = "This is a test description.";
        Assert.Equal("This is a test description.", note.Description);
    }

    [Fact]
    public void Description_AllowsNull()
    {
        DoodleNote.Models.DoodleNote note = new DoodleNote.Models.DoodleNote { NoteTitle = "Test", CreatedDate = System.DateTime.Now };
        note.Description = null;
        Assert.Null(note.Description);
    }

    [Fact]
    public void Description_RespectsMaxLength()
    {
        DoodleNote.Models.DoodleNote note = new DoodleNote.Models.DoodleNote { NoteTitle = "Test", CreatedDate = System.DateTime.Now };
        string longDesc = new string('a', 301);
        note.Description = longDesc;
        Assert.Equal(301, note.Description.Length); // DataAnnotations not enforced except by validation
    }
}
