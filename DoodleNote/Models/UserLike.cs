using DoodleNote.Features.Admin.Models;
namespace DoodleNote.Models;

public class UserLike
{
	
	public string UserId { get; set; } = string.Empty;
	public int NoteId { get; set; }

	public ApplicationUser User { get; set; } = null!;

	public DoodleNote Note { get; set; } = null!;
}
