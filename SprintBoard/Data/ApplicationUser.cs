using Microsoft.AspNetCore.Identity;

namespace SprintBoard.Data;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedByAdminId { get; set; }
}
