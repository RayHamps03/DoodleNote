namespace DoodleNote.Models;

/// <summary>
/// ViewModel for displaying detailed view of a single note.
/// </summary>
public class DoodleNoteDetailsViewModel
{
    public int NoteId { get; set; }
    public string NoteTitle { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}
