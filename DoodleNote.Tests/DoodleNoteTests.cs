using DoodleNote.Models;
using Xunit;
using System;

namespace DoodleNote.Tests;

public class DoodleNoteTests
{
    [Fact]
    public void CanSetAndGetProperties()
    {
        var note = new DoodleNote.Models.DoodleNote
        {
            NoteId = 1,
            NoteTitle = "Test Note",
            CreatedDate = new DateTime(2024, 4, 10)
        };
        Assert.Equal(1, note.NoteId);
        Assert.Equal("Test Note", note.NoteTitle);
        Assert.Equal(new DateTime(2024, 4, 10), note.CreatedDate);
    }

    [Fact]
    public void CreatedDate_StoresDateOnly()
    {
        var note = new DoodleNote.Models.DoodleNote { NoteTitle = "Test", CreatedDate = new DateTime(2024, 4, 10, 15, 30, 0) };
        Assert.Equal(new DateTime(2024, 4, 10), note.CreatedDate);
    }
}