namespace DoodleNote.Models;

/// <summary>
/// ViewModel for displaying error pages with request tracking.
/// </summary>
public class ErrorViewModel
{
    /// <summary>
    /// Unique request ID for error tracking and debugging.
    /// </summary>
    public string? RequestId { get; set; }

    /// <summary>
    /// Determines if RequestId should be displayed on error page.
    /// </summary>
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
