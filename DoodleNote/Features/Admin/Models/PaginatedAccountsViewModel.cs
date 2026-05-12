namespace DoodleNote.Features.Admin.Models;

/// <summary>
/// Paginated view model for displaying a list of accounts for admin management.
/// </summary>
public class PaginatedAccountsViewModel
{
    /// <summary>
    /// List of account management view models for the current page.
    /// </summary>
    public IReadOnlyList<AccountManagementViewModel> Accounts { get; set; } = new List<AccountManagementViewModel>();

    /// <summary>
    /// Current page number (1-indexed).
    /// </summary>
    public int CurrentPage { get; set; } = 1;

    /// <summary>
    /// Total number of pages available.
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Total number of accounts in the system.
    /// </summary>
    public int TotalAccounts { get; set; }

    /// <summary>
    /// Whether there is a previous page.
    /// </summary>
    public bool HasPreviousPage => CurrentPage > 1;

    /// <summary>
    /// Whether there is a next page.
    /// </summary>
    public bool HasNextPage => CurrentPage < TotalPages;

    /// <summary>
    /// Indicates if the current user is the owner.
    /// </summary>
    public bool IsOwner { get; set; }

    /// <summary>
    /// Indicates if the current user is an admin.
    /// </summary>
    public bool IsAdmin { get; set; }
}
