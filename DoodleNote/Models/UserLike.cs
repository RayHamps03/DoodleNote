using DoodleNote.Features.Admin.Models;
using System.ComponentModel.DataAnnotations;
namespace DoodleNote.Models;

public class UserLike
{
	[Key]
	[Required]
	public int Id { get; set; } 

	// The UserId associated with the user who liked the note.
	public string UserId { get; set; } = string.Empty;

	// The NoteId associated with the note that was liked.
	public int NoteId { get; set; }

	// Navigation property to the ApplicationUser entity.
	public ApplicationUser User { get; set; } = null!;

	// Navigation property to the DoodleNote entity.
	public DoodleNote Note { get; set; } = null!;
}
