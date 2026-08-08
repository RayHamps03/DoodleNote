using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DoodleNote.Models;
using DoodleNote.Features.Admin.Models;

namespace DoodleNote.Data;

/// <summary>
/// Entity Framework database context for Identity users and DoodleNote entities.
/// </summary>
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{

	/// <summary>
	/// DbSet for managing DoodleNote entities.
	/// </summary>
	public DbSet<Models.DoodleNote> DoodleNotes { get; set; }

	protected override void OnModelCreating(ModelBuilder builder)
	{
		base.OnModelCreating(builder);

		// Configure the relationship between DoodleNote and ApplicationUser
		builder.Entity<DoodleNote.Models.DoodleNote>()
			.HasOne(p => p.User)
			.WithMany(u => u.Notes)
			.HasForeignKey(p => p.UserId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}

