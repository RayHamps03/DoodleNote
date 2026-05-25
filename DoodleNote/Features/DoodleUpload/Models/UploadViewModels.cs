
using System.ComponentModel.DataAnnotations;

public sealed class StartDoodleUploadRequest
{
	public string? PngDataUrl { get; set; }
}


public sealed class ConfirmDoodleUploadViewModel
{
	[Required]
	public string Token { get; set; } = string.Empty;

	public string PreviewUrl { get; set; } = string.Empty;

	[Required]
	[StringLength(25, MinimumLength = 1)]
	public string NoteTitle { get; set; } = string.Empty;

	[StringLength(300)]
	public string? Description { get; set; }
}