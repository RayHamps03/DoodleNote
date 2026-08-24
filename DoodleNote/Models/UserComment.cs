using System.ComponentModel.DataAnnotations;

namespace DoodleNote.Models;

public class UserComment
{
	[Key]
	// Unique identifier for the comment
	public int CommentId { get; set; }
	// Text to be displayed as the comment
	[Required]
	[StringLength(300, ErrorMessage = "Comment cannot exceed 300 characters.")]
	public string CommentText { get; set; } = string.Empty;
	// Foreign key to ApplicationUser
	public required string UserId { get; set; }
	// Foreign key to DoodleNote
	public required int NoteId { get; set; }


	// ApplicationUser navigation property
	public ApplicationUser? User { get; set; }
	// DoodleNote navigation property
	public DoodleNote? Note { get; set; } 


}
