# Implementation Guide — SprintBoard

Step-by-step reference for building the Identity, role, and authorization layer.

---

## Step 1 — ASP.NET Core Identity Setup

### 1.1 Install NuGet packages

```xml
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="10.0.x" />
<PackageReference Include="Microsoft.AspNetCore.Identity.UI" Version="10.0.x" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.0.x" />
```

### 1.2 Create custom ApplicationUser

```csharp
// Data/ApplicationUser.cs
public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedByAdminId { get; set; }
}
```

### 1.3 Configure DbContext

```csharp
// Data/ApplicationDbContext.cs
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<ApplicationUser>()
            .Property(u => u.CreatedAt)
            .HasDefaultValueSql("now()");
    }
}
```

### 1.4 Register in Program.cs

```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
        options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()           // <-- required for role management
    .AddEntityFrameworkStores<ApplicationDbContext>();
```

### 1.5 Connection string in User Secrets (development only)

```bash
cd SprintBoard/
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=SprintBoard;Username=postgres;Password=YOUR_PASSWORD"
dotnet user-secrets set "SuperAdmin:Email" "superadmin@sprintboard.com"
dotnet user-secrets set "SuperAdmin:Password" "SuperAdmin@123"
dotnet user-secrets set "SuperAdmin:FullName" "Super Admin"
```

---

## Step 2 — Role Seeding

```csharp
// Services/RoleSeeder.cs
public static class RoleSeeder
{
    private static readonly string[] Roles = ["SuperAdmin", "Admin", "User"];

    public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var role in Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}
```

**How it works:** Iterates the three role names and creates any that don't already exist. Fully idempotent.

---

## Step 3 — SuperAdmin Seeding

```csharp
// Services/SuperAdminSeeder.cs
public static class SuperAdminSeeder
{
    public static async Task SeedSuperAdminAsync(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration)
    {
        var email    = configuration["SuperAdmin:Email"]
                       ?? throw new InvalidOperationException("SuperAdmin:Email not configured.");
        var password = configuration["SuperAdmin:Password"]
                       ?? throw new InvalidOperationException("SuperAdmin:Password not configured.");
        var fullName = configuration["SuperAdmin:FullName"] ?? "Super Admin";

        var existing = await userManager.FindByEmailAsync(email);
        if (existing is not null)
        {
            // Fix unconfirmed email if already exists
            if (!existing.EmailConfirmed)
            {
                existing.EmailConfirmed = true;
                await userManager.UpdateAsync(existing);
            }
            return;
        }

        var superAdmin = new ApplicationUser
        {
            FullName       = fullName,
            Email          = email,
            UserName       = email,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(superAdmin, password);
        if (result.Succeeded)
            await userManager.AddToRoleAsync(superAdmin, "SuperAdmin");
    }
}
```

### Invoke both seeders in Program.cs

```csharp
// Program.cs — after var app = builder.Build()
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    await RoleSeeder.SeedRolesAsync(roleManager);
    await SuperAdminSeeder.SeedSuperAdminAsync(userManager, app.Configuration);
}
```

---

## Step 4 — Admin Registration Logic

Modify `Areas/Identity/Pages/Account/Register.cshtml.cs`:

```csharp
// In OnPostAsync(), after var user = CreateUser():
user.FullName = Input.FullName;

// After result.Succeeded:
await _userManager.AddToRoleAsync(user, "Admin");
```

The `InputModel` must include a `FullName` field:
```csharp
[Required]
[Display(Name = "Full Name")]
public string FullName { get; set; } = default!;
```

---

## Step 5 — User Creation Logic (Admin creates User)

```csharp
// Controllers/UsersController.cs
[HttpPost, Authorize(Roles = "Admin,SuperAdmin")]
public async Task<IActionResult> Create(CreateUserViewModel model)
{
    if (!ModelState.IsValid) return View(model);

    var currentUser = await _userManager.GetUserAsync(User);

    var user = new ApplicationUser
    {
        FullName         = model.FullName,
        Email            = model.Email,
        UserName         = model.Email,
        EmailConfirmed   = true,              // bypass email verification
        CreatedAt        = DateTime.UtcNow,
        CreatedByAdminId = currentUser?.Id    // track which admin created this user
    };

    var result = await _userManager.CreateAsync(user, model.Password);
    if (result.Succeeded)
    {
        await _userManager.AddToRoleAsync(user, "User");  // always "User" role
        TempData["Success"] = $"User '{model.FullName}' created.";
        return RedirectToAction(nameof(Index));
    }

    foreach (var error in result.Errors)
        ModelState.AddModelError(string.Empty, error.Description);

    return View(model);
}
```

---

## Step 6 — Role-Based Authorization

### Controller level (all actions in controller)
```csharp
[Authorize(Roles = "SuperAdmin")]
public class SettingsController : Controller { }
```

### Action level (specific action only)
```csharp
[Authorize(Roles = "SuperAdmin,Admin")]
public async Task<IActionResult> Create() { }
```

### Combined (controller + action override)
```csharp
[Authorize]  // authenticated users only
public class DashboardController : Controller
{
    [Authorize(Roles = "SuperAdmin")]   // overrides controller attribute
    public IActionResult SuperAdmin() { }
}
```

---

## Step 7 — Dashboard Redirection After Login

```csharp
// Areas/Identity/Pages/Account/Login.cshtml.cs
// In OnPostAsync(), after result.Succeeded:

var user = await _userManager.FindByEmailAsync(Input.Email);
if (user is not null)
{
    if (await _userManager.IsInRoleAsync(user, "SuperAdmin"))
        return RedirectToAction("SuperAdmin", "Dashboard");
    if (await _userManager.IsInRoleAsync(user, "Admin"))
        return RedirectToAction("Admin", "Dashboard");
    if (await _userManager.IsInRoleAsync(user, "User"))
        return RedirectToAction("UserDashboard", "Dashboard");
}
return LocalRedirect(returnUrl);
```

---

## Step 8 — Navigation Menus Based on Roles

In `Views/Shared/_DashboardLayout.cshtml`, use role checks to show/hide nav items:

```cshtml
@if (User.IsInRole("SuperAdmin"))
{
    <a href="/Dashboard/SuperAdmin">Dashboard</a>
    <a href="/Users">All Users</a>
    <a href="/Projects">All Projects</a>
    <a href="/Reports">Reports</a>
    <a href="/Settings">Settings</a>
}
else if (User.IsInRole("Admin"))
{
    <a href="/Dashboard/Admin">Dashboard</a>
    <a href="/Users">My Users</a>
    <a href="/Projects">My Projects</a>
    <a href="/Tasks">Tasks</a>
}
else
{
    <a href="/Dashboard/UserDashboard">Dashboard</a>
    <a href="/Tasks/MyTasks">My Tasks</a>
}
```

---

## Step 9 — Sidebar Visibility Rules

| Sidebar Item | SuperAdmin | Admin | User |
|---|:---:|:---:|:---:|
| Dashboard link | SuperAdmin | Admin | UserDashboard |
| All Users | ✅ | ❌ | ❌ |
| My Users | ❌ | ✅ | ❌ |
| Create User | ✅ | ✅ | ❌ |
| All Projects | ✅ | ❌ | ❌ |
| My Projects | ❌ | ✅ | ❌ |
| My Tasks | ❌ | ❌ | ✅ |
| Reports | ✅ | ✅ | ❌ |
| Settings | ✅ | ❌ | ❌ |
| Profile | ✅ | ✅ | ✅ |
| Sign Out | ✅ | ✅ | ✅ |

---

## Step 10 — Adding Future Entities (Projects, Tasks)

```bash
# 1. Create entity class in Models/
# 2. Add DbSet to ApplicationDbContext
# 3. Add EF configuration in OnModelCreating if needed
# 4. Generate migration
dotnet ef migrations add AddProjectsAndTasks --project SprintBoard/SprintBoard.csproj

# 5. Apply migration
dotnet ef database update --project SprintBoard/SprintBoard.csproj

# 6. Create controller, viewmodels, views
# 7. Add [Authorize] attributes
# 8. Wire up sidebar navigation
```

---

## Security Checklist

- [x] Anti-forgery tokens on all POST forms (`[ValidateAntiForgeryToken]`)
- [x] User secrets for sensitive config (no passwords in `appsettings.json`)
- [x] `RequireConfirmedAccount = true` (email verification enforced)
- [x] Role checks in both controller attributes AND service/data layer
- [x] `CanManageUserAsync()` prevents Admin from touching SuperAdmin data
- [ ] HTTPS enforced in production (`UseHttpsRedirection` + `UseHsts`)
- [ ] Password complexity enforced via Identity options
- [ ] Lockout configured (`lockoutOnFailure: true` in production)
- [ ] Audit logging for sensitive operations
- [ ] Input validation on all ViewModels (DataAnnotations)
