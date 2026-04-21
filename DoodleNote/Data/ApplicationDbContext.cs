using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DoodleNote.Data;

public class ApplicationDbContext : IdentityDbContext
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
}

