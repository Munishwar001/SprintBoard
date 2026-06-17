# Database Requirements — SprintBoard

**Database:** PostgreSQL via Npgsql.EntityFrameworkCore.PostgreSQL
**ORM:** Entity Framework Core 10
**DbContext:** `ApplicationDbContext : IdentityDbContext<ApplicationUser>`

---

## Entity Overview

```
ApplicationUser (Identity)
       │
       ├──< Project (CreatedBy)
       │        │
       │        └──< Sprint
       │                │
       │                └──< TaskItem (AssignedTo →) ApplicationUser
       │                          │
       │                          └──< Comment (PostedBy →) ApplicationUser
       │
       └──< Comment (PostedBy)
```

---

## 1. ApplicationUser

**Table:** `AspNetUsers` (managed by ASP.NET Core Identity)

| Column | Type | Notes |
|---|---|---|
| `Id` | `nvarchar(450)` | Primary key, GUID string, auto-generated |
| `FullName` | `nvarchar(max)` | Required. Added as custom property. |
| `Email` | `nvarchar(256)` | Identity standard. Used as login. |
| `NormalizedEmail` | `nvarchar(256)` | Auto-managed by Identity |
| `UserName` | `nvarchar(256)` | Same as Email in this app |
| `NormalizedUserName` | `nvarchar(256)` | Auto-managed by Identity |
| `PasswordHash` | `nvarchar(max)` | Bcrypt hash, auto-managed |
| `EmailConfirmed` | `boolean` | `true` = can log in |
| `CreatedAt` | `timestamp with time zone` | Default: `now()` via `HasDefaultValueSql` |
| `CreatedByAdminId` | `nvarchar(450)` | FK to `AspNetUsers.Id`. Nullable. Set when Admin creates the user. |

```csharp
public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedByAdminId { get; set; }
}
```

**EF Configuration:**
```csharp
builder.Entity<ApplicationUser>()
    .Property(u => u.CreatedAt)
    .HasDefaultValueSql("now()");
```

---

## 2. Project

**Table:** `Projects`

| Column | Type | Nullable | Notes |
|---|---|---|---|
| `Id` | `int` | No | PK, auto-increment |
| `Name` | `nvarchar(200)` | No | Required |
| `Description` | `nvarchar(max)` | Yes | Optional |
| `CreatedById` | `nvarchar(450)` | No | FK → `AspNetUsers.Id` |
| `CreatedAt` | `timestamp` | No | Default `now()` |
| `IsArchived` | `boolean` | No | Default `false` |

```csharp
public class Project
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string CreatedById { get; set; } = null!;
    public ApplicationUser CreatedBy { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsArchived { get; set; }

    public ICollection<Sprint> Sprints { get; set; } = [];
}
```

---

## 3. Sprint

**Table:** `Sprints`

| Column | Type | Nullable | Notes |
|---|---|---|---|
| `Id` | `int` | No | PK, auto-increment |
| `Name` | `nvarchar(200)` | No | Required (e.g., "Sprint 1") |
| `Goal` | `nvarchar(500)` | Yes | Optional sprint goal description |
| `StartDate` | `date` | No | Sprint start |
| `EndDate` | `date` | No | Sprint end |
| `IsActive` | `boolean` | No | Default `false`. Only one active sprint per project. |
| `ProjectId` | `int` | No | FK → `Projects.Id` |

```csharp
public class Sprint
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Goal { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public bool IsActive { get; set; }
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public ICollection<TaskItem> Tasks { get; set; } = [];
}
```

---

## 4. TaskItem

**Table:** `TaskItems`

> Named `TaskItem` (not `Task`) to avoid conflict with `System.Threading.Tasks.Task`.

| Column | Type | Nullable | Notes |
|---|---|---|---|
| `Id` | `int` | No | PK, auto-increment |
| `Title` | `nvarchar(300)` | No | Required |
| `Description` | `nvarchar(max)` | Yes | Optional |
| `Status` | `int` (enum) | No | `0=Todo, 1=InProgress, 2=Done` |
| `Priority` | `int` (enum) | No | `0=Low, 1=Medium, 2=High, 3=Critical` |
| `AssignedUserId` | `nvarchar(450)` | Yes | FK → `AspNetUsers.Id`. Nullable if unassigned. |
| `SprintId` | `int` | No | FK → `Sprints.Id` |
| `CreatedById` | `nvarchar(450)` | No | FK → `AspNetUsers.Id` (Admin who created it) |
| `CreatedAt` | `timestamp` | No | Default `now()` |
| `DueDate` | `date` | Yes | Optional deadline |
| `CompletedAt` | `timestamp` | Yes | Set when status moves to `Done` |

```csharp
public enum TaskStatus { Todo, InProgress, Done }
public enum TaskPriority { Low, Medium, High, Critical }

public class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public TaskStatus Status { get; set; } = TaskStatus.Todo;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    public string? AssignedUserId { get; set; }
    public ApplicationUser? AssignedUser { get; set; }

    public string CreatedById { get; set; } = null!;
    public ApplicationUser CreatedBy { get; set; } = null!;

    public int SprintId { get; set; }
    public Sprint Sprint { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateOnly? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }

    public ICollection<Comment> Comments { get; set; } = [];
}
```

---

## 5. Comment

**Table:** `Comments`

| Column | Type | Nullable | Notes |
|---|---|---|---|
| `Id` | `int` | No | PK, auto-increment |
| `Content` | `nvarchar(max)` | No | Required |
| `TaskId` | `int` | No | FK → `TaskItems.Id` |
| `UserId` | `nvarchar(450)` | No | FK → `AspNetUsers.Id` (author) |
| `CreatedAt` | `timestamp` | No | Default `now()` |
| `EditedAt` | `timestamp` | Yes | Set on edit |

```csharp
public class Comment
{
    public int Id { get; set; }
    public string Content { get; set; } = null!;

    public int TaskId { get; set; }
    public TaskItem Task { get; set; } = null!;

    public string UserId { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EditedAt { get; set; }
}
```

---

## ApplicationDbContext (complete future state)

```csharp
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Sprint> Sprints => Set<Sprint>();
    public DbSet<TaskItem> TaskItems => Set<TaskItem>();
    public DbSet<Comment> Comments => Set<Comment>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>()
            .Property(u => u.CreatedAt)
            .HasDefaultValueSql("now()");

        // Prevent cascade delete cycles on AssignedUser
        builder.Entity<TaskItem>()
            .HasOne(t => t.AssignedUser)
            .WithMany()
            .HasForeignKey(t => t.AssignedUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<TaskItem>()
            .HasOne(t => t.CreatedBy)
            .WithMany()
            .HasForeignKey(t => t.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Comment>()
            .HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
```

---

## Migration Strategy

```bash
# Add migration after entity changes
dotnet ef migrations add <MigrationName> --project SprintBoard/SprintBoard.csproj

# Apply to database
dotnet ef database update --project SprintBoard/SprintBoard.csproj

# Rollback one migration
dotnet ef database update <PreviousMigrationName> --project SprintBoard/SprintBoard.csproj
```

**Existing migrations:**
1. `20260617065925_InitialCreate` — Identity tables + FullName
2. `20260617093407_AddUserCreatedAtAndAdminId` — CreatedAt, CreatedByAdminId
