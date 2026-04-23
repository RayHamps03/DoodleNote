namespace DoodleNote.Models;

public class DoodleNoteListViewModel
{
    public IReadOnlyList<DoodleNote> Notes { get; init; } = Array.Empty<DoodleNote>();
    public int CurrentPage { get; init; }
    public int TotalPages { get; init; }

    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
}
