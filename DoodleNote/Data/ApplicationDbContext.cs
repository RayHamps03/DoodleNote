using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DoodleNote.Models;

namespace DoodleNote.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
	public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
	{
	}

	/// <summary>
	/// This allows the DbContext to manage a collection
	/// of DoodleNote entities, enabling CRUD operations
	/// on the DoodleNotes table in the database.
	/// </summary>
	public DbSet<Models.DoodleNote> DoodleNotes { get; set; }

	protected override void OnModelCreating(ModelBuilder builder)
	{
		base.OnModelCreating(builder);

		// Configure ApplicationUser properties
		builder.Entity<ApplicationUser>()
			.Property(u => u.IsAdmin)
			.HasDefaultValue(false);

		// Configure IsOwner property
		// IsOwner is a hidden field that marks the system owner account
		// The owner account cannot have IsAdmin set to false once established
		builder.Entity<ApplicationUser>()
			.Property(u => u.IsOwner)
			.HasDefaultValue(false);
	}
}

