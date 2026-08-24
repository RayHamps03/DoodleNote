namespace DoodleNote.Models;

public class UserComment
{
	// Primary key
	public int CommentId { get; set; }
	// Foreign key to ApplicationUser
	public required string UserId { get; set; }
	// Foreign key to DoodleNote
	public required int NoteId { get; set; }


	// ApplicationUser navigation property
	public ApplicationUser? User { get; set; }
	// DoodleNote navigation property
	public DoodleNote Note { get; set; } 

	public 

}
