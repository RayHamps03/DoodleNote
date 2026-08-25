using System.ComponentModel.DataAnnotations;

namespace DoodleNote.Models;

/// <summary>
/// A single comment rendered on the DoodleNote details view.
/// </summary>
public class CommentViewModel
{
    public int CommentId { get; set; }
    public string Author { get; set; } = string.Empty;
    public string CommentText { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}

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
    public string? ImagePath { get; set; }
    public int LikeCount { get; set; }
    public bool IsLikedByCurrentUser { get; set; }
    public List<CommentViewModel> Comments { get; set; } = new();

    [Required(ErrorMessage = "Comment cannot be empty.")]
    [StringLength(300, ErrorMessage = "Comment cannot exceed 300 characters.")]
    public string NewCommentText { get; set; } = string.Empty;
}
