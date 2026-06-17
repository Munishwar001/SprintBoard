# Role Documentation — SprintBoard

## Overview

SprintBoard uses a three-tier role hierarchy built on ASP.NET Core Identity. Every user is assigned exactly one role at account creation. Roles are non-additive — each role is a distinct identity with a defined scope of authority.

---

## Role Hierarchy

```
┌─────────────────────────────────────┐
│           SUPERADMIN                │
│  Full system authority              │
│  Manages Admins + Users + System    │
└──────────────┬──────────────────────┘
               │ delegates to
┌──────────────▼──────────────────────┐
│              ADMIN                  │
│  Workspace authority                │
│  Manages Users + Projects + Tasks   │
└──────────────┬──────────────────────┘
               │ assigns work to
┌──────────────▼──────────────────────┐
│              USER                   │
│  Task authority                     │
│  Executes assigned work             │
└─────────────────────────────────────┘
```

---

## Role 1 — SuperAdmin

### Purpose
The SuperAdmin is the system owner. There is exactly **one** SuperAdmin account, seeded at application startup. This role has unrestricted access to every feature, entity, and configuration in the system.

### How It Is Assigned
- Seeded automatically by `SuperAdminSeeder` on first startup.
- Cannot be self-registered.
- Cannot be assigned through the UI by any other role.

### Responsibilities
| # | Responsibility |
|---|---|
| 1 | Manage the entire user base (Admins and Users) |
| 2 | Oversee all projects and sprints across all Admins |
| 3 | Manage roles and system-wide permissions |
| 4 | View system-wide reports and statistics |
| 5 | Audit all activity across the platform |
| 6 | Configure application-level settings |

### Allowed Actions
- Create, read, update, delete **any** user (Admin or User)
- Create, read, update, delete **any** project
- Create, read, update, delete **any** sprint
- Create, read, update, delete **any** task
- Assign tasks to any user
- Manage role assignments
- View all dashboards
- Access audit logs and system reports
- Change system settings

### Restricted Actions
- Cannot register through the public registration page
- Cannot be created by any Admin

---

## Role 2 — Admin

### Purpose
Admins are workspace managers. Each Admin is created via the **public registration page** and is the primary operator of day-to-day project and user management. An Admin manages a team of Users and organises them into projects and sprints.

### How It Is Assigned
- Automatically assigned when a user self-registers via `/Identity/Account/Register`.
- No manual step required — the `Register.cshtml.cs` calls `AddToRoleAsync(user, "Admin")` on success.

### Responsibilities
| # | Responsibility |
|---|---|
| 1 | Create and manage their team of Users |
| 2 | Create and manage projects |
| 3 | Define sprints and milestones |
| 4 | Create and assign tasks to Users |
| 5 | Track project progress and team activity |
| 6 | Comment on and moderate task discussions |

### Allowed Actions
- Create Users (Users are auto-assigned the `User` role)
- Read, update Users they created
- Create, read, update projects
- Create, read, update sprints within their projects
- Create, assign, update, delete tasks within their projects
- Comment on any task in their projects
- Access Admin Dashboard
- View reports for their own team

### Restricted Actions
- Cannot create or modify SuperAdmin accounts
- Cannot create other Admin accounts
- Cannot delete another Admin's projects or Users
- Cannot access system-level settings
- Cannot manage role assignments

---

## Role 3 — User

### Purpose
Users are the task executors. They are created by Admins and are assigned work items (tasks) within sprints. Their authority is limited to their own assigned work and profile.

### How It Is Assigned
- Created by an Admin or SuperAdmin via `UsersController.Create`.
- `AddToRoleAsync(user, "User")` is called automatically on creation.
- Cannot self-register (public registration creates Admins, not Users).

### Responsibilities
| # | Responsibility |
|---|---|
| 1 | Complete assigned tasks |
| 2 | Update the status of their own tasks |
| 3 | Participate in task discussions via comments |
| 4 | Maintain their own profile |

### Allowed Actions
- View their own assigned tasks
- Update status of their own tasks (`Todo → In Progress → Done`)
- Comment on tasks they are assigned to or participating in
- View their own User Dashboard
- Edit their own profile (name, email, password)

### Restricted Actions
- Cannot create or manage any other user
- Cannot create projects or sprints
- Cannot assign tasks to others
- Cannot view other users' data
- Cannot access Admin or SuperAdmin dashboards
- Cannot manage roles

---

## Permission Summary Matrix

| Capability | SuperAdmin | Admin | User |
|---|:---:|:---:|:---:|
| Manage Admins | ✅ | ❌ | ❌ |
| Manage Users | ✅ | ✅ (own) | ❌ |
| Manage Roles | ✅ | ❌ | ❌ |
| Create Projects | ✅ | ✅ | ❌ |
| Manage All Projects | ✅ | ❌ | ❌ |
| Manage Own Projects | ✅ | ✅ | ❌ |
| Create Sprints | ✅ | ✅ | ❌ |
| Create Tasks | ✅ | ✅ | ❌ |
| Assign Tasks | ✅ | ✅ | ❌ |
| Update Task Status | ✅ | ✅ | ✅ (own) |
| Comment on Tasks | ✅ | ✅ | ✅ |
| View System Reports | ✅ | ❌ | ❌ |
| View Team Reports | ✅ | ✅ | ❌ |
| System Settings | ✅ | ❌ | ❌ |

---

## Security Notes

1. **Role escalation is impossible** — no action in the UI allows a lower role to elevate itself.
2. **SuperAdmin is not self-registrable** — it is seeded only at startup via `SuperAdminSeeder`.
3. **Admin cannot touch SuperAdmin** — `CanManageUserAsync()` in `UsersController` blocks this explicitly.
4. **All dashboard routes are role-guarded** with `[Authorize(Roles = "...")]`.
