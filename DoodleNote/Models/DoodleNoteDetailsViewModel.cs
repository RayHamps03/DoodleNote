namespace DoodleNote.Models;

public class DoodleNoteDetailsViewModel
{
    public int NoteId { get; set; }
    public string NoteTitle { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}
