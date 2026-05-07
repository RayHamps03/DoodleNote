namespace DoodleNote.Models;

/// <summary>
/// Interface for objects that can have admin privileges.
/// Used to control access to sensitive operations like setting user admin status.
/// </summary>
public interface IAdmin
{
    /// <summary>
    /// Gets a value indicating whether this entity has admin privileges.
    /// </summary>
    bool IsAdmin { get; }
}
