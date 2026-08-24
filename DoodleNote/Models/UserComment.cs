namespace DoodleNote.Models;

public class UserComment
{
	// Unique identifier for the comment
	public int CommentId { get; set; }
	// Text to be displayed as the comment
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
