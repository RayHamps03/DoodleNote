using DoodleNote.Data;
using DoodleNote.Features.Admin.Services;
using Microsoft.EntityFrameworkCore;

namespace DoodleNote.Extensions;

/// <summary>
/// Extension methods for application middleware and initialization.
/// </summary>
public static class MiddlewareExtensions
{
	private static bool _migrationCompleted = false;

	/// <summary>
	/// Runs database migrations and role initialization on first request.
	/// </summary>
	public static WebApplication UseDbInitialization(this WebApplication app)
	{
		app.Use(async (context, next) =>
		{
			if (!_migrationCompleted)
			{
				try
				{
					using var scope = app.Services.CreateScope();
					var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
					var roleService = scope.ServiceProvider.GetRequiredService<RoleService>();
					var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

					await dbContext.Database.MigrateAsync();
					await roleService.InitializeRolesAsync();

					logger.LogInformation("Database migrations and role initialization completed.");
					_migrationCompleted = true;
				}
				catch (Exception ex)
				{
					app.Services.GetRequiredService<ILogger<Program>>()
						.LogError(ex, "Error during database initialization.");
					throw;
				}
			}

			await next();
		});

		return app;
	}

	/// <summary>
	/// Adds security headers to all responses.
	/// </summary>
	public static WebApplication UseSecurityHeaders(this WebApplication app)
	{
		app.Use(async (context, next) =>
		{
			context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
			context.Response.Headers.Append("X-Frame-Options", "DENY");
			context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
			context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
			context.Response.Headers.Append("Permissions-Policy", "geolocation=(), microphone=(), camera=()");
			await next();
		});

		return app;
	}
}
