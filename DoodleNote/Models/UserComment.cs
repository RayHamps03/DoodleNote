using System.ComponentModel.DataAnnotations;

namespace DoodleNote.Models;

public class UserComment
{
	[Key]
	// Unique identifier for the comment
	public int CommentId { get; private set; }
	// Text to be displayed as the comment
	[Required]
	[StringLength(300, ErrorMessage = "Comment cannot exceed 300 characters.")]
	public string CommentText { get; private set; } = string.Empty;
	// Foreign key to ApplicationUser
	public string? UserId { get; private set; }
	// Foreign key to DoodleNote
	public int NoteId { get; private set; }


	// ApplicationUser navigation property
	public ApplicationUser? User { get; set; }
	// DoodleNote navigation property
	public DoodleNote? Note { get; set; }

	// Creates a new comment with the given comment text to display, user id, and noteid
	public UserComment CreateComment(string cmntTxt, string userId, int noteId) 
	{
		return new UserComment
		{
			CommentText = cmntTxt,
			UserId = userId,
			NoteId = noteId
		};
	}

}
