# Authorization Rules — SprintBoard

---

## ASP.NET Core Identity Authorization Attributes

### Attribute Reference

| Attribute | Meaning |
|---|---|
| `[Authorize]` | Any authenticated user |
| `[Authorize(Roles = "SuperAdmin")]` | SuperAdmin only |
| `[Authorize(Roles = "Admin")]` | Admin only |
| `[Authorize(Roles = "User")]` | User only |
| `[Authorize(Roles = "SuperAdmin,Admin")]` | SuperAdmin or Admin |
| `[Authorize(Roles = "Admin,User")]` | Admin or User |
| `[AllowAnonymous]` | Anyone, no auth required |

---

## Controller Authorization Map

### HomeController

```csharp
// No [Authorize] — public landing page
public class HomeController : Controller
{
    [AllowAnonymous] public IActionResult Index() { }
    [AllowAnonymous] public IActionResult Privacy() { }
}
```

---

### DashboardController

```csharp
[Authorize]
public class DashboardController : Controller
{
    // Dispatcher — redirects to role-specific action
    public IActionResult Index() { }

    // SuperAdmin only
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> SuperAdmin() { }

    // Admin only
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Admin() { }

    // User only
    [Authorize(Roles = "User")]
    public async Task<IActionResult> UserDashboard() { }
}
```

---

### UsersController

```csharp
// Admin and SuperAdmin can access this controller
[Authorize(Roles = "Admin,SuperAdmin")]
public class UsersController : Controller
{
    // SuperAdmin sees all; Admin sees own users only
    public async Task<IActionResult> Index() { }

    [HttpGet]
    public IActionResult Create() { }

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserViewModel model) { }

    // Admin cannot edit SuperAdmin or other Admin accounts
    [HttpGet]
    public async Task<IActionResult> Edit(string id) { }

    [HttpPost]
    public async Task<IActionResult> Edit(string id, EditUserViewModel model) { }

    // SuperAdmin only
    [Authorize(Roles = "SuperAdmin")]
    [HttpPost]
    public async Task<IActionResult> Delete(string id) { }
}
```

---

### ProjectsController (future)

```csharp
[Authorize]
public class ProjectsController : Controller
{
    // SuperAdmin: all projects. Admin: own projects. User: assigned projects.
    public async Task<IActionResult> Index() { }

    // SuperAdmin and Admin only
    [Authorize(Roles = "SuperAdmin,Admin")]
    public IActionResult Create() { }

    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateProjectViewModel model) { }

    // Any authenticated user — but service layer scopes by role
    public async Task<IActionResult> Details(int id) { }

    // SuperAdmin: any. Admin: own only (service layer enforces)
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Edit(int id) { }

    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Delete(int id) { }
}
```

---

### SprintsController (future)

```csharp
[Authorize(Roles = "SuperAdmin,Admin")]
public class SprintsController : Controller
{
    public async Task<IActionResult> Index(int projectId) { }

    [HttpGet]
    public IActionResult Create(int projectId) { }

    [HttpPost]
    public async Task<IActionResult> Create(CreateSprintViewModel model) { }

    public async Task<IActionResult> Edit(int id) { }

    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Delete(int id) { }
}
```

---

### TasksController (future)

```csharp
[Authorize]
public class TasksController : Controller
{
    // SuperAdmin: all. Admin: own project tasks. User: assigned tasks only.
    public async Task<IActionResult> Index() { }

    // User-only view: tasks assigned to the logged-in user
    [Authorize(Roles = "User")]
    public async Task<IActionResult> MyTasks() { }

    // SuperAdmin and Admin only
    [Authorize(Roles = "SuperAdmin,Admin")]
    public IActionResult Create() { }

    [Authorize(Roles = "SuperAdmin,Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateTaskViewModel model) { }

    // Any authenticated user — but users can only edit their own task's status
    public async Task<IActionResult> Edit(int id) { }

    // SuperAdmin and Admin only
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Delete(int id) { }

    // Any authenticated user (all roles can comment)
    [HttpPost]
    public async Task<IActionResult> AddComment(int taskId, string content) { }
}
```

---

### ReportsController (future)

```csharp
[Authorize(Roles = "SuperAdmin,Admin")]
public class ReportsController : Controller
{
    // SuperAdmin: system-wide. Admin: own team.
    public async Task<IActionResult> Index() { }

    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> SystemReport() { }

    public async Task<IActionResult> TeamReport() { }

    public async Task<IActionResult> ExportCsv(ReportFilterViewModel filter) { }
}
```

---

### SettingsController (future)

```csharp
// SuperAdmin only
[Authorize(Roles = "SuperAdmin")]
public class SettingsController : Controller
{
    public IActionResult Index() { }
    public IActionResult Email() { }
    public IActionResult Audit() { }
}
```

---

## Service-Layer Authorization

Controller attributes guard **access to the route**. Service-layer checks guard **the data** within that route. Always apply both:

```csharp
// Example: Admin cannot access another Admin's project data
public async Task<Project?> GetProjectAsync(int projectId, ClaimsPrincipal currentUser)
{
    var project = await _db.Projects.FindAsync(projectId);
    if (project is null) return null;

    if (currentUser.IsInRole("SuperAdmin")) return project;

    var userId = _userManager.GetUserId(currentUser);
    return project.CreatedById == userId ? project : null; // returns null = Forbid
}
```

---

## Program.cs Policy Registration (optional, for policy-based auth)

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SuperAdminOnly",   p => p.RequireRole("SuperAdmin"));
    options.AddPolicy("AdminOrAbove",     p => p.RequireRole("SuperAdmin", "Admin"));
    options.AddPolicy("AllRoles",         p => p.RequireRole("SuperAdmin", "Admin", "User"));
    options.AddPolicy("CanManageUsers",   p => p.RequireRole("SuperAdmin", "Admin"));
    options.AddPolicy("CanManageTasks",   p => p.RequireRole("SuperAdmin", "Admin"));
});
```

Usage in controllers:
```csharp
[Authorize(Policy = "AdminOrAbove")]
public IActionResult Create() { }
```

---

## Razor View Authorization Guards

```cshtml
@* Only show "Manage Users" link to Admin and SuperAdmin *@
@if (User.IsInRole("SuperAdmin") || User.IsInRole("Admin"))
{
    <a asp-controller="Users" asp-action="Index">Manage Users</a>
}

@* Only show "System Settings" to SuperAdmin *@
@if (User.IsInRole("SuperAdmin"))
{
    <a asp-controller="Settings" asp-action="Index">Settings</a>
}

@* Show "My Tasks" only to Users *@
@if (User.IsInRole("User"))
{
    <a asp-controller="Tasks" asp-action="MyTasks">My Tasks</a>
}
```
