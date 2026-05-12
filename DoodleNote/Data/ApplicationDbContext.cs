using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DoodleNote.Models;
using DoodleNote.Features.Admin.Models;

namespace DoodleNote.Data;

/// <summary>
/// Entity Framework database context for Identity users and DoodleNote entities.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
	public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

	/// <summary>
	/// DbSet for managing DoodleNote entities.
	/// </summary>
	public DbSet<Models.DoodleNote> DoodleNotes { get; set; }
}

