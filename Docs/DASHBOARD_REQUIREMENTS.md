# Dashboard Requirements — SprintBoard

---

## SuperAdmin Dashboard

**Route:** `GET /Dashboard/SuperAdmin`
**Authorization:** `[Authorize(Roles = "SuperAdmin")]`
**ViewModel:** `SuperAdminDashboardViewModel`

### Stat Cards (Row 1)
| Card | Data Source | Color |
|---|---|---|
| Total Users | `UserManager.Users.Count()` | Blue |
| Total Admins | `GetUsersInRoleAsync("Admin").Count` | Amber |
| Total Regular Users | `GetUsersInRoleAsync("User").Count` | Green |
| Total SuperAdmins | `GetUsersInRoleAsync("SuperAdmin").Count` | Red |

> **Future additions:** Total Projects, Total Active Tasks, Total Sprints

### Stat Cards (Row 2 — after Project entity is built)
| Card | Data Source | Color |
|---|---|---|
| Total Projects | `DbContext.Projects.Count()` | Indigo |
| Active Sprints | `DbContext.Sprints.Where(s => s.IsActive).Count()` | Cyan |
| Open Tasks | `DbContext.Tasks.Where(t => t.Status != Done).Count()` | Orange |
| Completed Tasks | `DbContext.Tasks.Where(t => t.Status == Done).Count()` | Green |

### Tables / Lists
| Section | Content | Columns |
|---|---|---|
| Recent Users | Last 10 registered users ordered by `CreatedAt DESC` | Name, Email, Role, Joined |
| Recent Activity | Last 20 audit log entries | User, Action, Entity, Timestamp |

### Quick Actions Panel
| Button | Target |
|---|---|
| Manage Users | `GET /Users/Index` |
| Create User | `GET /Users/Create` |
| View All Projects | `GET /Projects/Index` |
| System Reports | `GET /Reports/Index` |

### ViewModel Properties
```csharp
public class SuperAdminDashboardViewModel
{
    public string FullName { get; set; }
    public string Email { get; set; }
    public int TotalUsers { get; set; }
    public int TotalAdmins { get; set; }
    public int TotalRegularUsers { get; set; }
    public int TotalSuperAdmins { get; set; }
    // Future:
    public int TotalProjects { get; set; }
    public int ActiveSprints { get; set; }
    public int OpenTasks { get; set; }
    public int CompletedTasks { get; set; }
    public List<RecentUserViewModel> RecentUsers { get; set; }
    public List<ActivityLogViewModel> RecentActivities { get; set; }
}
```

---

## Admin Dashboard

**Route:** `GET /Dashboard/Admin`
**Authorization:** `[Authorize(Roles = "Admin")]`
**ViewModel:** `AdminDashboardViewModel`

### Stat Cards
| Card | Data Source | Color |
|---|---|---|
| Users Created | Users where `CreatedByAdminId == currentAdmin.Id` | Blue |
| Active Users | Created users not locked out | Green |
| This Month's Users | Created users this calendar month | Purple |

> **Future additions after Projects/Tasks entities:**

| Card | Data Source | Color |
|---|---|---|
| Active Projects | Projects where `CreatedBy == currentAdmin.Id` and not archived | Indigo |
| Pending Tasks | Tasks in admin's projects with status `Todo` or `InProgress` | Amber |
| Completed Tasks | Tasks in admin's projects with status `Done` | Green |
| Sprint Count | Active sprints in admin's projects | Cyan |

### Tables / Lists
| Section | Content | Columns |
|---|---|---|
| My Users | Last 10 users created by this admin | Name, Email, Status, Joined |
| Recent Activities | Recent task/project activity in admin's scope | User, Action, Entity, Time |

### Quick Actions Panel
| Button | Target |
|---|---|
| Create User | `GET /Users/Create` |
| Manage Users | `GET /Users/Index` |
| New Project | `GET /Projects/Create` |
| View Projects | `GET /Projects/Index` |

### ViewModel Properties
```csharp
public class AdminDashboardViewModel
{
    public string FullName { get; set; }
    public string Email { get; set; }
    public int TotalUsersCreated { get; set; }
    public int ActiveUsers { get; set; }
    public int ThisMonthUsers { get; set; }
    // Future:
    public int ActiveProjects { get; set; }
    public int PendingTasks { get; set; }
    public int CompletedTasks { get; set; }
    public List<RecentUserViewModel> RecentUsers { get; set; }
    public List<ActivityLogViewModel> RecentActivities { get; set; }
}
```

---

## User Dashboard

**Route:** `GET /Dashboard/UserDashboard`
**Authorization:** `[Authorize(Roles = "User")]`
**ViewModel:** `UserDashboardViewModel`

### Profile Card
| Field | Data Source |
|---|---|
| Avatar (initial letter) | `FullName[0]` |
| Full Name | `ApplicationUser.FullName` |
| Email | `ApplicationUser.Email` |
| Role badge | `"User"` |

### Account Information Card
| Label | Value |
|---|---|
| Full Name | `ApplicationUser.FullName` |
| Email | `ApplicationUser.Email` |
| Username | `ApplicationUser.UserName` |
| Role | `"User"` |

> **Future additions after Tasks entity:**

### Task Summary Cards
| Card | Data Source | Color |
|---|---|---|
| Assigned Tasks | Tasks where `AssignedUserId == currentUser.Id` | Blue |
| Pending Tasks | Assigned tasks where status `Todo` or `InProgress` | Amber |
| Completed Tasks | Assigned tasks where status `Done` | Green |

### Tables / Lists
| Section | Content | Columns |
|---|---|---|
| My Tasks | Last 10 tasks assigned to this user | Title, Project, Priority, Status, Due |
| Recent Activity | Recent comments / status changes by this user | Action, Task, Time |

### Quick Actions Panel
| Button | Target |
|---|---|
| View Profile | `GET /Identity/Account/Manage/Index` |
| Update Email | `GET /Identity/Account/Manage/Email` |
| Change Password | `GET /Identity/Account/Manage/ChangePassword` |
| Two-Factor Auth | `GET /Identity/Account/Manage/TwoFactorAuthentication` |
| My Tasks | `GET /Tasks/MyTasks` (future) |

### ViewModel Properties
```csharp
public class UserDashboardViewModel
{
    public string FullName { get; set; }
    public string Email { get; set; }
    public string UserName { get; set; }
    public string Role { get; set; }
    // Future:
    public int AssignedTasks { get; set; }
    public int PendingTasks { get; set; }
    public int CompletedTasks { get; set; }
    public List<TaskSummaryViewModel> RecentTasks { get; set; }
}
```

---

## Shared Dashboard Layout Requirements

**File:** `Views/Shared/_DashboardLayout.cshtml`

### Sidebar
- Fixed, 260px wide
- Collapses to 72px icon-only mode on toggle
- Dark background (`#0f172a`)
- Role-specific navigation links
- User avatar (initial letter) with name and role badge
- Sign Out button at bottom

### Top Navbar
- Sticky, 64px height, white background
- Sidebar toggle button (left)
- Page title (left, next to toggle)
- User profile dropdown (right) — shows name, avatar, Profile link, Sign Out

### Mobile
- Sidebar slides in from left as an overlay
- Semi-transparent backdrop overlay (`rgba(0,0,0,0.5)`)
- Clicking backdrop closes sidebar

### Content Area
- `padding: 1.5rem`
- `background: #f1f5f9`
- Fills remaining viewport height
