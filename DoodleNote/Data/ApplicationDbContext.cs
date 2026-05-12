using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DoodleNote.Models;

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

	protected override void OnModelCreating(ModelBuilder builder)
	{
		base.OnModelCreating(builder);

		// Set default values for admin flags
		builder.Entity<ApplicationUser>()
			.Property(u => u.IsAdmin)
			.HasDefaultValue(false);

		// IsOwner marks the system owner account (cannot be changed to false once set)
		builder.Entity<ApplicationUser>()
			.Property(u => u.IsOwner)
			.HasDefaultValue(false);
	}
}

