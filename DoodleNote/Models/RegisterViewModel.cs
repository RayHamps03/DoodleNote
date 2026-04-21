using System.ComponentModel.DataAnnotations;

namespace DoodleNote.Models;

public class RegisterViewModel
{
    [Required]
    [StringLength(25, MinimumLength = 1)]
    public required string Username { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(255, MinimumLength = 6)]
    public required string Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [StringLength(35, MinimumLength = 8)]
    public required string Password { get; set; }

    [DataType(DataType.Password)]
    [Compare(nameof(Password))]
    public string? ConfirmPassword { get; set; }
}
