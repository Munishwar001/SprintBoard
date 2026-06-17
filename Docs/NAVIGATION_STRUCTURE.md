# Navigation Structure — SprintBoard

---

## Sidebar Navigation by Role

### SuperAdmin Sidebar

```
┌─────────────────────────┐
│  ⚡ SprintBoard          │
├─────────────────────────┤
│  [Avatar]  Super Admin  │
│            [SuperAdmin] │
├─────────────────────────┤
│  MAIN MENU              │
│  ⊞  Dashboard           │  → /Dashboard/SuperAdmin
├─────────────────────────┤
│  MANAGEMENT             │
│  👥 All Users           │  → /Users/Index
│  ➕ Create User         │  → /Users/Create
│  📁 All Projects        │  → /Projects/Index  (future)
│  📋 All Tasks           │  → /Tasks/Index     (future)
│  📊 Reports             │  → /Reports/Index   (future)
├─────────────────────────┤
│  SYSTEM                 │
│  ⚙️  Settings           │  → /Settings/Index  (future)
│  🔍 Audit Logs          │  → /Settings/Audit  (future)
├─────────────────────────┤
│  ACCOUNT                │
│  👤 My Profile          │  → /Identity/Account/Manage
├─────────────────────────┤
│  🚪 Sign Out            │  → POST /Identity/Account/Logout
└─────────────────────────┘
```

---

### Admin Sidebar

```
┌─────────────────────────┐
│  ⚡ SprintBoard          │
├─────────────────────────┤
│  [Avatar]  John Smith   │
│            [Admin]      │
├─────────────────────────┤
│  MAIN MENU              │
│  ⊞  Dashboard           │  → /Dashboard/Admin
├─────────────────────────┤
│  USERS                  │
│  👥 My Users            │  → /Users/Index
│  ➕ Create User         │  → /Users/Create
├─────────────────────────┤
│  PROJECTS               │
│  📁 My Projects         │  → /Projects/Index  (future)
│  ➕ New Project         │  → /Projects/Create (future)
├─────────────────────────┤
│  WORK                   │
│  🏃 Sprints             │  → /Sprints/Index   (future)
│  📋 Tasks               │  → /Tasks/Index     (future)
│  📊 Reports             │  → /Reports/Index   (future)
├─────────────────────────┤
│  ACCOUNT                │
│  👤 My Profile          │  → /Identity/Account/Manage
├─────────────────────────┤
│  🚪 Sign Out            │  → POST /Identity/Account/Logout
└─────────────────────────┘
```

---

### User Sidebar

```
┌─────────────────────────┐
│  ⚡ SprintBoard          │
├─────────────────────────┤
│  [Avatar]  Jane Doe     │
│            [User]       │
├─────────────────────────┤
│  MAIN MENU              │
│  ⊞  Dashboard           │  → /Dashboard/UserDashboard
├─────────────────────────┤
│  MY WORK                │
│  ✅ My Tasks            │  → /Tasks/MyTasks   (future)
│  🏃 My Sprints          │  → /Sprints/My      (future)
├─────────────────────────┤
│  ACCOUNT                │
│  👤 My Profile          │  → /Identity/Account/Manage
│  🔑 Change Password     │  → /Identity/Account/Manage/ChangePassword
│  🛡️  Two-Factor Auth    │  → /Identity/Account/Manage/TwoFactorAuthentication
├─────────────────────────┤
│  🚪 Sign Out            │  → POST /Identity/Account/Logout
└─────────────────────────┘
```

---

## Top Navbar (all roles)

```
┌─────────────────────────────────────────────────────────┐
│  ☰ (toggle)   Dashboard Title              [Avatar ▼]   │
└─────────────────────────────────────────────────────────┘
```

### Profile Dropdown (all roles)

```
┌──────────────────────┐
│  John Smith          │
│  john@example.com    │
├──────────────────────┤
│  👤 My Profile       │
│  🔑 Change Password  │
├──────────────────────┤
│  🚪 Sign Out         │
└──────────────────────┘
```

---

## Active State Rules

The sidebar highlights the active link based on the current controller and action:

```cshtml
class="sb-nav-link @(ViewContext.RouteData.Values["action"]?.ToString() == "SuperAdmin" ? "active" : "")"
```

| Current Page | Active Item |
|---|---|
| `/Dashboard/SuperAdmin` | Dashboard |
| `/Dashboard/Admin` | Dashboard |
| `/Dashboard/UserDashboard` | Dashboard |
| `/Users/Index` | All Users / My Users |
| `/Users/Create` | Create User |
| `/Projects/Index` | My Projects / All Projects |
| `/Tasks/MyTasks` | My Tasks |

---

## Public Navigation (unauthenticated)

The landing page (`Views/Home/Index.cshtml`) uses `_LandingLayout.cshtml`:

```
SprintBoard [logo]    Features   How It Works   Architecture    [Register]  [Login]
```

The `_LoginPartial.cshtml` switches between authenticated and unauthenticated states:

```cshtml
@if (SignInManager.IsSignedIn(User))
{
    <a asp-area="Identity" asp-page="/Account/Manage/Index">Hello @User.Identity?.Name</a>
    <form asp-area="Identity" asp-page="/Account/Logout">
        <button type="submit">Logout</button>
    </form>
}
else
{
    <a asp-area="Identity" asp-page="/Account/Register">Register</a>
    <a asp-area="Identity" asp-page="/Account/Login">Login</a>
}
```

---

## Route Map

| URL Pattern | Controller | Action | Auth |
|---|---|---|---|
| `/` | Home | Index | Anonymous |
| `/Identity/Account/Register` | (Razor Page) | Register | Anonymous |
| `/Identity/Account/Login` | (Razor Page) | Login | Anonymous |
| `/Identity/Account/Logout` | (Razor Page) | Logout | Authenticated |
| `/Dashboard` | Dashboard | Index | Authenticated |
| `/Dashboard/SuperAdmin` | Dashboard | SuperAdmin | SuperAdmin |
| `/Dashboard/Admin` | Dashboard | Admin | Admin |
| `/Dashboard/UserDashboard` | Dashboard | UserDashboard | User |
| `/Users` | Users | Index | Admin, SuperAdmin |
| `/Users/Create` | Users | Create | Admin, SuperAdmin |
| `/Users/Edit/{id}` | Users | Edit | Admin, SuperAdmin |
| `/Projects` | Projects | Index | Admin, SuperAdmin |
| `/Projects/Create` | Projects | Create | Admin, SuperAdmin |
| `/Tasks` | Tasks | Index | Admin, SuperAdmin |
| `/Tasks/MyTasks` | Tasks | MyTasks | User |
| `/Sprints` | Sprints | Index | Admin, SuperAdmin |
| `/Reports` | Reports | Index | Admin, SuperAdmin |
| `/Settings` | Settings | Index | SuperAdmin |
