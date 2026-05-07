using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace DoodleNote.Models;

/// <summary>
/// Custom user class extending IdentityUser to add application-specific properties.
/// </summary>
public class ApplicationUser : IdentityUser, IAdmin
{
    private bool _isAdmin = false;
    private bool _isOwner = false;

    /// <summary>
    /// Indicates whether the user account has admin privileges.
    /// This property can be modified directly via SQL Server Object Explorer.
    /// The IsOwner flag ensures the owner account cannot be demoted.
    /// 
    /// Public getter allows reading the admin status.
    /// The setter can be used to initialize the value or directly via database.
    /// </summary>
    [Display(Name = "Is Admin", GroupName = "Admin")]
    public bool IsAdmin
    {
        get => _isAdmin;
        set => _isAdmin = value;
    }

    /// <summary>
    /// Indicates whether this user is the system owner.
    /// The owner account cannot have its IsAdmin status changed to false.
    /// Only one user should have this flag set to true.
    /// This can be set directly via SQL Server Object Explorer.
    /// </summary>
    [Display(Name = "Is Owner", GroupName = "Admin")]
    public bool IsOwner
    {
        get => _isOwner;
        set => _isOwner = value;
    }

    /// <summary>
    /// Sets the admin status of this user account.
    /// Only an admin user (implementing IAdmin) can change the admin status of other users.
    /// The owner account cannot be demoted (IsAdmin set to false).
    /// </summary>
    /// <param name="isAdmin">The new admin status.</param>
    /// <param name="requestingUser">The user requesting the change. Must have IsAdmin = true.</param>
    /// <exception cref="UnauthorizedAccessException">Thrown if the requesting user is not an admin.</exception>
    /// <exception cref="InvalidOperationException">Thrown if trying to demote the owner account.</exception>
    public void SetAdminStatus(bool isAdmin, IAdmin requestingUser)
    {
        if (!requestingUser.IsAdmin)
        {
            throw new UnauthorizedAccessException("Only admin users can change admin status.");
        }

        // Prevent demoting the owner account
        if (IsOwner && !isAdmin)
        {
            throw new InvalidOperationException("The owner account cannot be demoted. The IsAdmin status for the owner must remain true.");
        }

        IsAdmin = isAdmin;
    }

    /// <summary>
    /// Promotes a user to owner status.
    /// Only an admin user can promote users to owner.
    /// </summary>
    /// <param name="requestingUser">The user requesting the change. Must have IsAdmin = true.</param>
    /// <exception cref="UnauthorizedAccessException">Thrown if the requesting user is not an admin.</exception>
    public void PromoteToOwner(IAdmin requestingUser)
    {
        if (!requestingUser.IsAdmin)
        {
            throw new UnauthorizedAccessException("Only admin users can promote users to owner.");
        }

        IsOwner = true;
        IsAdmin = true; // Owner must always be admin
    }

    /// <summary>
    /// Demotes a user from owner status to regular admin.
    /// Only an admin user can demote the owner, but the owner must have another admin to replace them.
    /// </summary>
    /// <param name="requestingUser">The user requesting the change. Must have IsAdmin = true.</param>
    /// <exception cref="UnauthorizedAccessException">Thrown if the requesting user is not an admin.</exception>
    public void DemoteFromOwner(IAdmin requestingUser)
    {
        if (!requestingUser.IsAdmin)
        {
            throw new UnauthorizedAccessException("Only admin users can demote the owner.");
        }

        IsOwner = false;
        // IsAdmin remains true after demotion
    }
}
