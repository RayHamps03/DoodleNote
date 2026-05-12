using Microsoft.AspNetCore.Identity;

namespace DoodleNote.Features.Admin.Models;

/// <summary>
/// Custom user class extending IdentityUser for application-specific user management.
/// Role-based authorization is handled through AspNetRoles instead of user properties.
/// This is a self-contained admin feature that can be removed without breaking core app functionality.
/// </summary>
public class ApplicationUser : IdentityUser
{
}
