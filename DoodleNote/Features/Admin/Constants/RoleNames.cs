namespace DoodleNote.Features.Admin.Constants;

/// <summary>
/// Defines the role constants used in the application for role-based access control.
/// Part of the self-contained Admin feature module.
/// </summary>
public static class RoleNames
{
    /// <summary>
    /// Administrator role with full application access.
    /// </summary>
    public const string Admin = "Admin";

    /// <summary>
    /// System owner role - highest privilege level, cannot be removed from admin.
    /// Only one user should have this role.
    /// </summary>
    public const string Owner = "Owner";

    /// <summary>
    /// Regular user role with basic application access.
    /// </summary>
    public const string User = "User";
}
