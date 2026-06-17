namespace SprintBoard.Models;

public class SuperAdminDashboardViewModel
{
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public int TotalUsers { get; set; }
    public int TotalAdmins { get; set; }
    public int TotalRegularUsers { get; set; }
    public int TotalSuperAdmins { get; set; }
    public List<RecentUserViewModel> RecentUsers { get; set; } = [];
}
