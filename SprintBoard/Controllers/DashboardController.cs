using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SprintBoard.Data;

namespace SprintBoard.Controllers;

[Authorize(Roles = "SuperAdmin")]
public class DashboardController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    public DashboardController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.TotalUsers = _userManager.Users.Count();
        ViewBag.TotalAdmins = (await _userManager.GetUsersInRoleAsync("Admin")).Count;
        ViewBag.TotalMembers = (await _userManager.GetUsersInRoleAsync("User")).Count;
        return View();
    }
}
