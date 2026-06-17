namespace SprintBoard.Models;

public class AdminDashboardViewModel
{
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public int TotalUsersCreated { get; set; }
    public int ActiveUsers { get; set; }
    public int ThisMonthUsers { get; set; }
    public List<RecentUserViewModel> RecentUsers { get; set; } = [];
}
