namespace DoodleNote.Features.Admin.Models;

/// <summary>
/// View model for displaying account information in the admin management page.
/// </summary>
public class AccountManagementViewModel
{
    /// <summary>
    /// The unique identifier for the user account.
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// The username of the account.
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// The email address of the account.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// List of roles assigned to the account.
    /// </summary>
    public IList<string> Roles { get; set; } = new List<string>();

    /// <summary>
    /// Indicates whether the requesting user can remove this account.
    /// </summary>
    public bool CanRemove { get; set; }

    /// <summary>
    /// Indicates whether the requesting user can modify this account's roles.
    /// </summary>
    public bool CanModifyRoles { get; set; }
}
