using DoodleNote.Data;
using DoodleNote.Extensions;
using DoodleNote.Features.Admin.Models;
using DoodleNote.Features.Admin.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
        sqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null)
                  .CommandTimeout(30)));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddAntiforgery(options =>
    options.HeaderName = "X-CSRF-TOKEN");

builder.Services.AddScoped<RoleService>();
builder.Services.AddScoped<AccountService>();
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseDbInitialization();
app.UseSecurityHeaders();

string? configuredUrls = builder.Configuration["ASPNETCORE_URLS"];
bool hasHttpsEndpoint = !string.IsNullOrWhiteSpace(configuredUrls)
    && configuredUrls.Contains("https://", StringComparison.OrdinalIgnoreCase);

if (hasHttpsEndpoint)
{
    app.UseHttpsRedirection();
}
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}").WithStaticAssets();
app.MapRazorPages().WithStaticAssets();

app.Run();

// Expose Program for WebApplicationFactory in integration tests
public partial class Program { }
