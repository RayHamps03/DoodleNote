using Microsoft.AspNetCore.Identity;

namespace DoodleNote.Models;

public class ApplicationUser : IdentityUser
{
	public UserAccount? UserAccount { get; set; }
}
