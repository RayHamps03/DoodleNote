namespace DoodleNote.Models;

/// <summary>
/// ViewModel for displaying paginated list of notes on the Index page.
/// </summary>
public class DoodleNoteListViewModel
{
    /// <summary>
    /// Collection of notes for current page.
    /// </summary>
    public IReadOnlyList<DoodleNote> Notes { get; init; } = Array.Empty<DoodleNote>();

    /// <summary>
    /// Current page number (1-based).
    /// </summary>
    public int CurrentPage { get; init; }

    /// <summary>
    /// Total number of pages.
    /// </summary>
    public int TotalPages { get; init; }

    /// <summary>
    /// Determines if there is a previous page available.
    /// </summary>
    public bool HasPreviousPage => CurrentPage > 1;

    /// <summary>
    /// Determines if there is a next page available.
    /// </summary>
    public bool HasNextPage => CurrentPage < TotalPages;
}
