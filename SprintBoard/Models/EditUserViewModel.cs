using System.ComponentModel.DataAnnotations;

namespace SprintBoard.Models;

public class EditUserViewModel
{
    public string Id { get; set; } = default!;

    [Required]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = default!;

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = default!;
}
