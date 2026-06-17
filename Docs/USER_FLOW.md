# User Flows — SprintBoard

---

## 1. Registration Flow (Admin Creation)

```
User visits /Identity/Account/Register
        │
        ▼
Fills registration form:
  - Full Name
  - Email
  - Password
  - Confirm Password
        │
        ▼
[POST] Register.cshtml.cs → OnPostAsync()
        │
        ├─── ModelState invalid? → Re-display form with errors
        │
        ▼
ApplicationUser created:
  user.FullName = Input.FullName
  user.Email    = Input.Email
  user.UserName = Input.Email
        │
        ▼
_userManager.CreateAsync(user, password)
        │
        ├─── Failed? → Add Identity errors to ModelState, re-display form
        │
        ▼
_userManager.AddToRoleAsync(user, "Admin")  ← Auto-assigned
        │
        ▼
RequireConfirmedAccount = true?
  YES → Redirect to RegisterConfirmation page
        (user must confirm email before logging in)
  NO  → Sign in immediately, redirect to returnUrl
```

**Key rule:** Every self-registration produces an **Admin** account, never a User or SuperAdmin.

---

## 2. User Creation Flow (by Admin or SuperAdmin)

```
Admin/SuperAdmin navigates to /Users/Create
        │
        ▼
Fills Create User form:
  - Full Name
  - Email
  - Password
  - Confirm Password
        │
        ▼
[POST] UsersController.Create()
        │
        ├─── ModelState invalid? → Re-display form with errors
        │
        ▼
ApplicationUser created:
  user.FullName         = model.FullName
  user.Email            = model.Email
  user.UserName         = model.Email
  user.EmailConfirmed   = true          ← Admin-created users skip email verification
  user.CreatedAt        = DateTime.UtcNow
  user.CreatedByAdminId = currentAdmin.Id
        │
        ▼
_userManager.CreateAsync(user, password)
        │
        ├─── Failed? → Add errors to ModelState, re-display form
        │
        ▼
_userManager.AddToRoleAsync(user, "User")  ← Auto-assigned
        │
        ▼
TempData["Success"] message set
Redirect to /Users/Index
```

**Key rules:**
- Admin-created users get `EmailConfirmed = true` so they can log in immediately.
- `CreatedByAdminId` is stored so the Admin Dashboard can count "My Users".
- Role is always `"User"` — Admin cannot create another Admin.

---

## 3. Login Flow (Role-Based Redirect)

```
User visits /Identity/Account/Login
        │
        ▼
Enters email and password → clicks Sign In
        │
        ▼
[POST] Login.cshtml.cs → OnPostAsync()
        │
        ▼
_signInManager.PasswordSignInAsync(email, password, rememberMe, lockoutOnFailure: false)
        │
        ├─── result.IsLockedOut  → Redirect to /Identity/Account/Lockout
        ├─── result.IsNotAllowed → "Your email is not confirmed." (re-display form)
        ├─── result.RequiresTwoFactor → Redirect to LoginWith2fa
        ├─── Failed              → "Invalid login attempt." (re-display form)
        │
        ▼ result.Succeeded
        │
        ▼
Fetch user: _userManager.FindByEmailAsync(Input.Email)
        │
        ▼
Check role:
  IsInRole("SuperAdmin") → RedirectToAction("SuperAdmin", "Dashboard")
  IsInRole("Admin")      → RedirectToAction("Admin", "Dashboard")
  IsInRole("User")       → RedirectToAction("UserDashboard", "Dashboard")
  No role found          → LocalRedirect(returnUrl)
```

**Key rules:**
- No role selector on the login page — role is determined from the database.
- Each role lands on a completely different dashboard.
- `IsNotAllowed` shows a clear message rather than the misleading "Invalid login attempt."

---

## 4. Logout Flow

```
User clicks "Sign Out" in sidebar or dropdown
        │
        ▼
[POST] /Identity/Account/Logout (anti-forgery token required)
        │
        ▼
_signInManager.SignOutAsync()
        │
        ▼
Redirect to / (Home/Index — public landing page)
```

---

## 5. Access Denied Flow

```
Authenticated user attempts to access a route
beyond their role (e.g., User tries /Dashboard/SuperAdmin)
        │
        ▼
[Authorize(Roles = "SuperAdmin")] fails
        │
        ▼
ASP.NET Core → HTTP 403
        │
        ▼
Redirect to /Identity/Account/AccessDenied
        │
        ▼
User sees: "Access Denied" page with Back button
```

---

## 6. Password Reset Flow

```
User clicks "Forgot Password?" on login page
        │
        ▼
/Identity/Account/ForgotPassword
Enters email address
        │
        ▼
System generates reset token
Sends email via IEmailSender
        │
        ▼
User receives email with reset link
Clicks link → /Identity/Account/ResetPassword?userId=...&code=...
        │
        ▼
Enters new password + confirmation
        │
        ▼
_userManager.ResetPasswordAsync(user, token, newPassword)
        │
        ▼
Success → Redirect to /Identity/Account/ResetPasswordConfirmation
```

---

## 7. Profile Update Flow

```
Authenticated user (any role) navigates to
/Identity/Account/Manage/Index
        │
        ▼
Views current: FullName, Email, Phone
        │
        ▼
Updates fields, clicks Save
        │
        ▼
_userManager.UpdateAsync(user)
        │
        ▼
Success → TempData success message
Redirect back to profile page
```

---

## 8. SuperAdmin Seeding Flow (Startup)

```
Application starts → Program.cs
        │
        ▼
using (var scope = app.Services.CreateScope())
        │
        ▼
RoleSeeder.SeedRolesAsync(roleManager)
  For each role in ["SuperAdmin", "Admin", "User"]:
    if role does not exist → CreateAsync(new IdentityRole(role))
        │
        ▼
SuperAdminSeeder.SeedSuperAdminAsync(userManager, configuration)
  email    = configuration["SuperAdmin:Email"]
  password = configuration["SuperAdmin:Password"]
  fullName = configuration["SuperAdmin:FullName"]
        │
        ├─── User exists with that email AND EmailConfirmed = false?
        │       → Set EmailConfirmed = true, UpdateAsync
        │       → Return (don't re-create)
        │
        ├─── User exists and EmailConfirmed = true?
        │       → Return (idempotent, nothing to do)
        │
        ▼
User does not exist:
  Create ApplicationUser with EmailConfirmed = true
  _userManager.CreateAsync(superAdmin, password)
  _userManager.AddToRoleAsync(superAdmin, "SuperAdmin")
        │
        ▼
Application startup continues → app.Run()
```

**Key rule:** The seeder is fully idempotent — safe to run on every startup.
