using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SprintBoard.Data;
using SprintBoard.Models;

namespace SprintBoard.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    public DashboardController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public IActionResult Index()
    {
        if (User.IsInRole("SuperAdmin")) return RedirectToAction(nameof(SuperAdmin));
        if (User.IsInRole("Admin"))      return RedirectToAction(nameof(Admin));
        return RedirectToAction(nameof(UserDashboard));
    }

    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> SuperAdmin()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        SetViewBagUser(currentUser);

        var allUsers     = _userManager.Users.ToList();
        var admins       = await _userManager.GetUsersInRoleAsync("Admin");
        var regularUsers = await _userManager.GetUsersInRoleAsync("User");
        var superAdmins  = await _userManager.GetUsersInRoleAsync("SuperAdmin");

        var recentUsers = new List<RecentUserViewModel>();
        foreach (var u in allUsers.OrderByDescending(u => u.CreatedAt).Take(10))
        {
            var roles = await _userManager.GetRolesAsync(u);
            recentUsers.Add(new RecentUserViewModel
            {
                FullName  = u.FullName,
                Email     = u.Email!,
                Role      = roles.FirstOrDefault() ?? "—",
                CreatedAt = u.CreatedAt
            });
        }

        return View(new SuperAdminDashboardViewModel
        {
            FullName          = currentUser!.FullName,
            Email             = currentUser.Email!,
            TotalUsers        = allUsers.Count,
            TotalAdmins       = admins.Count,
            TotalRegularUsers = regularUsers.Count,
            TotalSuperAdmins  = superAdmins.Count,
            RecentUsers       = recentUsers
        });
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Admin()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        SetViewBagUser(currentUser);

        var myUsers = _userManager.Users
            .Where(u => u.CreatedByAdminId == currentUser!.Id)
            .OrderByDescending(u => u.CreatedAt)
            .ToList();

        var now = DateTime.UtcNow;

        return View(new AdminDashboardViewModel
        {
            FullName          = currentUser!.FullName,
            Email             = currentUser.Email!,
            TotalUsersCreated = myUsers.Count,
            ActiveUsers       = myUsers.Count(u => u.LockoutEnd == null || u.LockoutEnd <= DateTimeOffset.UtcNow),
            ThisMonthUsers    = myUsers.Count(u => u.CreatedAt.Year == now.Year && u.CreatedAt.Month == now.Month),
            RecentUsers       = myUsers.Take(10).Select(u => new RecentUserViewModel
            {
                FullName  = u.FullName,
                Email     = u.Email!,
                Role      = "User",
                CreatedAt = u.CreatedAt
            }).ToList()
        });
    }

    [Authorize(Roles = "User")]
    public async Task<IActionResult> UserDashboard()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        SetViewBagUser(currentUser);

        var roles = await _userManager.GetRolesAsync(currentUser!);

        return View(new UserDashboardViewModel
        {
            FullName = currentUser.FullName,
            Email    = currentUser.Email!,
            UserName = currentUser.UserName!,
            Role     = roles.FirstOrDefault() ?? "User"
        });
    }

    private void SetViewBagUser(ApplicationUser? user)
    {
        ViewBag.FullName    = user?.FullName ?? User.Identity?.Name;
        ViewBag.UserInitial = (user?.FullName ?? User.Identity?.Name ?? "U").Substring(0, 1).ToUpper();
    }
}
