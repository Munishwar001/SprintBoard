# Permission Matrix — SprintBoard

## Reading the Matrix

- **✅ Full** — Role can perform this action without restriction.
- **✅ Own** — Role can perform this action only on resources they own or are assigned to.
- **❌** — Role cannot perform this action.

---

## 1. User Management

| Action | SuperAdmin | Admin | User |
|---|:---:|:---:|:---:|
| View all users | ✅ Full | ✅ Own team | ❌ |
| Create Admin accounts | ❌ (public reg only) | ❌ | ❌ |
| Create User accounts | ✅ Full | ✅ Full | ❌ |
| Edit any user | ✅ Full | ✅ Own users | ❌ |
| Delete any user | ✅ Full | ❌ | ❌ |
| Reset user password | ✅ Full | ✅ Own users | ❌ |
| View own profile | ✅ | ✅ | ✅ |
| Edit own profile | ✅ | ✅ | ✅ |
| Change own password | ✅ | ✅ | ✅ |

---

## 2. Role Management

| Action | SuperAdmin | Admin | User |
|---|:---:|:---:|:---:|
| View roles | ✅ | ❌ | ❌ |
| Assign roles | ✅ | ❌ | ❌ |
| Remove roles | ✅ | ❌ | ❌ |
| Create new roles | ✅ | ❌ | ❌ |
| Delete roles | ✅ | ❌ | ❌ |

---

## 3. Project Management

| Action | SuperAdmin | Admin | User |
|---|:---:|:---:|:---:|
| View all projects | ✅ Full | ✅ Own | ✅ Assigned |
| Create project | ✅ | ✅ | ❌ |
| Edit any project | ✅ Full | ✅ Own | ❌ |
| Delete any project | ✅ Full | ✅ Own | ❌ |
| Archive project | ✅ Full | ✅ Own | ❌ |
| Add members to project | ✅ Full | ✅ Own | ❌ |
| Remove members from project | ✅ Full | ✅ Own | ❌ |
| View project reports | ✅ Full | ✅ Own | ❌ |

---

## 4. Sprint Management

| Action | SuperAdmin | Admin | User |
|---|:---:|:---:|:---:|
| View sprints | ✅ Full | ✅ Own | ✅ Assigned |
| Create sprint | ✅ | ✅ | ❌ |
| Edit sprint | ✅ Full | ✅ Own | ❌ |
| Delete sprint | ✅ Full | ✅ Own | ❌ |
| Start sprint | ✅ Full | ✅ Own | ❌ |
| Complete sprint | ✅ Full | ✅ Own | ❌ |
| Move tasks between sprints | ✅ Full | ✅ Own | ❌ |

---

## 5. Task Management

| Action | SuperAdmin | Admin | User |
|---|:---:|:---:|:---:|
| View all tasks | ✅ Full | ✅ Own project | ✅ Assigned only |
| Create task | ✅ | ✅ | ❌ |
| Edit any task | ✅ Full | ✅ Own project | ❌ |
| Delete task | ✅ Full | ✅ Own project | ❌ |
| Assign task to user | ✅ Full | ✅ Own project | ❌ |
| Update task status | ✅ Full | ✅ Full | ✅ Own tasks |
| Set task priority | ✅ Full | ✅ Own project | ❌ |
| Set due date | ✅ Full | ✅ Own project | ❌ |
| Add attachments | ✅ | ✅ | ✅ Own tasks |
| View task history | ✅ Full | ✅ Own project | ✅ Own tasks |

---

## 6. Comment Management

| Action | SuperAdmin | Admin | User |
|---|:---:|:---:|:---:|
| Post comment | ✅ | ✅ | ✅ |
| Edit own comment | ✅ | ✅ | ✅ |
| Delete own comment | ✅ | ✅ | ✅ |
| Delete any comment | ✅ | ✅ Own project | ❌ |

---

## 7. Dashboard & Reports

| Action | SuperAdmin | Admin | User |
|---|:---:|:---:|:---:|
| SuperAdmin Dashboard | ✅ | ❌ | ❌ |
| Admin Dashboard | ❌ | ✅ | ❌ |
| User Dashboard | ❌ | ❌ | ✅ |
| System-wide statistics | ✅ | ❌ | ❌ |
| Team statistics | ✅ | ✅ Own team | ❌ |
| Task completion reports | ✅ Full | ✅ Own project | ✅ Own tasks |
| Burndown charts | ✅ Full | ✅ Own sprints | ❌ |
| User activity reports | ✅ Full | ✅ Own team | ❌ |
| Export reports (CSV/PDF) | ✅ | ✅ Own data | ❌ |

---

## 8. System & Settings

| Action | SuperAdmin | Admin | User |
|---|:---:|:---:|:---:|
| Application settings | ✅ | ❌ | ❌ |
| Email / SMTP settings | ✅ | ❌ | ❌ |
| Audit logs | ✅ | ❌ | ❌ |
| View login history | ✅ Full | ✅ Own | ✅ Own |
| Two-factor authentication | ✅ | ✅ | ✅ |

---

## 9. Navigation Access

| Navigation Item | SuperAdmin | Admin | User |
|---|:---:|:---:|:---:|
| Dashboard | ✅ | ✅ | ✅ |
| Users | ✅ | ✅ | ❌ |
| Roles | ✅ | ❌ | ❌ |
| Projects | ✅ | ✅ | ❌ |
| My Tasks | ❌ | ❌ | ✅ |
| Sprints | ✅ | ✅ | ❌ |
| Reports | ✅ | ✅ | ❌ |
| Settings | ✅ | ❌ | ❌ |
| Profile | ✅ | ✅ | ✅ |
