using System.Linq;
using System.Threading.Tasks;
using DoodleNote.Data;
using DoodleNote.Features.Admin.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DoodleNote.Tests.Integration
{
    public class ChangePasswordIntegrationTests
    {
        [Fact]
        public async Task ChangePassword_AllowsSigningInWithNewPassword()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("TestDb_ChangePassword"));

            services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddHttpContextAccessor();

            var provider = services.BuildServiceProvider();

            using var scope = provider.CreateScope();
            var sp = scope.ServiceProvider;
            var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
            var signInManager = sp.GetRequiredService<SignInManager<ApplicationUser>>();
            var db = sp.GetRequiredService<ApplicationDbContext>();
            await db.Database.EnsureCreatedAsync();

            var user = new ApplicationUser { UserName = "testuser", Email = "testuser@example.com", EmailConfirmed = true };
            var create = await userManager.CreateAsync(user, "OldP@ssw0rd!");
            Assert.True(create.Succeeded, string.Join(";", create.Errors.Select(e => e.Description)));

            var change = await userManager.ChangePasswordAsync(user, "OldP@ssw0rd!", "NewP@ssw0rd!");
            Assert.True(change.Succeeded, string.Join(";", change.Errors.Select(e => e.Description)));

            var result = await signInManager.CheckPasswordSignInAsync(user, "NewP@ssw0rd!", lockoutOnFailure: false);
            Assert.True(result.Succeeded);
        }
    }
}
