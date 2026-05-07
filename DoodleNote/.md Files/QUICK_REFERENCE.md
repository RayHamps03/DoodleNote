## Quick Reference: Account System

### Database Tables (Post-Migration)

```sql
-- Main table for users
AspNetUsers
├── Id (string, PK)
├── UserName (string, unique)
├── Email (string, unique)
├── PasswordHash (string)
├── IsAdmin (bit, default 0)           ← NEW
└── IsOwner (bit, default 0)           ← NEW
```

### Core Classes

| Class              | Namespace             | Purpose                                  |
|--------------------|-----------------------|------------------------------------------|
| `ApplicationUser`  | `DoodleNote.Models`   | Custom IdentityUser with IsAdmin/IsOwner |
| `IAdmin`           | `DoodleNote.Models`   | Interface for admin privilege contract   |
| `AccountViewModel` | `DoodleNote.Models`   | ViewModel for registration/display       |
| `AccountService`   | `DoodleNote.Services` | Business logic for account operations    |

### Property Validation Rules

```
Email
├── Required: Yes
├── Unique: Yes
├── Format: Valid email address
├── MaxLength: 256 characters
└── Example: user@example.com

Username
├── Required: Yes
├── Unique: Yes
├── MinLength: 5 characters
├── MaxLength: 20 characters
├── Pattern: [a-zA-Z0-9_-]+
└── Example: john_doe, user-123

Password
├── Required: Yes
├── MinLength: 6 characters
├── MaxLength: 100 characters
├── Must contain: At least 1 digit (0-9)
├── Must contain: At least 1 symbol (!@#$%^&*|_-()+=[]{};\'"<>,.?/\~`)
└── Example: Pass123!, Test@456, Admin#99!

IsAdmin
├── Type: Boolean
├── Default: false
├── Set by: Admins only via SetAdminStatus()
└── SQL modification: Allowed

IsOwner
├── Type: Boolean
├── Default: false
├── Set by: Admins via PromoteUserToOwnerAsync()
├── Protection: Cannot set IsAdmin=false while IsOwner=true
└── SQL modification: Allowed but protected by application logic
```

### Common Tasks

#### Create New User
```csharp
var model = new AccountViewModel 
{ 
    Email = "user@example.com",
    Username = "john_doe",
    Password = "Pass123!",
    ConfirmPassword = "Pass123!"
};

var (result, user) = await _accountService.CreateAccountAsync(model);
if (result.Succeeded)
{
    // User created successfully
}
```

#### Find User
```csharp
// By email
var user = await _accountService.FindUserByEmailAsync("user@example.com");

// By username
var user = await _accountService.FindUserByUsernameAsync("john_doe");

// Check uniqueness before registering
bool emailInUse = await _accountService.IsEmailInUseAsync("user@example.com");
bool usernameTaken = await _accountService.IsUsernameInUseAsync("john_doe");
```

#### Manage Admin Status
```csharp
var user = await _userManager.FindByIdAsync(userId);
var admin = await _userManager.FindByIdAsync(adminUserId);

// Grant admin
user.SetAdminStatus(true, admin);
await _userManager.UpdateAsync(user);

// Revoke admin (fails if user is owner)
try 
{
    user.SetAdminStatus(false, admin);
    await _userManager.UpdateAsync(user);
}
catch (InvalidOperationException ex)
{
    // User is owner and cannot be demoted
}
```

#### Manage Owner Status
```csharp
var admin = await _userManager.FindByIdAsync(adminUserId);

// Promote to owner
var result = await _accountService.PromoteUserToOwnerAsync(userId, admin);

// Find current owner
var owner = await _accountService.FindOwnerAsync();

// Demote owner to regular admin
var result = await _accountService.DemoteOwnerAsync(ownerId, admin);

// Check if user is owner
bool isOwner = await _accountService.IsUserOwnerAsync(userId);
```

### SQL Server Object Explorer Guide

#### Set User as Admin
```sql
UPDATE AspNetUsers SET IsAdmin = 1 WHERE UserName = 'username'
```

#### Set User as Owner
```sql
UPDATE AspNetUsers SET IsAdmin = 1, IsOwner = 1 WHERE UserName = 'username'
```

#### Remove Admin (if not owner)
```sql
UPDATE AspNetUsers SET IsAdmin = 0 WHERE UserName = 'username' AND IsOwner = 0
```

#### Remove Owner
```sql
UPDATE AspNetUsers SET IsOwner = 0 WHERE IsOwner = 1
-- Note: Must use DemoteOwnerAsync() in code if IsAdmin should remain true
```

#### View All Admins and Owner
```sql
SELECT Id, UserName, Email, IsAdmin, IsOwner 
FROM AspNetUsers 
WHERE IsAdmin = 1 OR IsOwner = 1
ORDER BY IsOwner DESC
```

### Error Messages

| Scenario                    | Exception                   | Message                                                                                   |
|-----------------------------|-----------------------------|-------------------------------------------------------------------------------------------|
| Non-admin sets admin        | UnauthorizedAccessException | "Only admin users can change admin status."                                               |
| Demote owner                | InvalidOperationException   | "The owner account cannot be demoted. The IsAdmin status for the owner must remain true." |
| Non-admin promotes to owner | UnauthorizedAccessException | "Only admin users can promote users to owner."                                            |
| Invalid email               | ValidationException         | "Email must be a valid email address."                                                    |
| Duplicate email             | IdentityResult error        | "[Email] is already taken."                                                               |
| Password too weak           | ValidationException         | "Password must contain at least one number and one symbol."                               |
| Username conflict           | IdentityResult error        | "[Username] is already taken."                                                            |

### Dependency Injection Setup

```csharp
// Program.cs
builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<UserManager<ApplicationUser>>();
builder.Services.AddScoped<SignInManager<ApplicationUser>>();
```

### Razor Page Integration

```csharp
// Page Model
public class RegisterModel : PageModel
{
    private readonly AccountService _accountService;
    
    [BindProperty]
    public AccountViewModel Account { get; set; }
    
    public RegisterModel(AccountService accountService)
    {
        _accountService = accountService;
    }
    
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        
        var (result, user) = await _accountService.CreateAccountAsync(Account);
        
        if (result.Succeeded)
        {
            return RedirectToPage("RegisterConfirmation");
        }
        
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
        
        return Page();
    }
}
```

### Test Data

```sql
-- Create test admin user
INSERT INTO AspNetUsers 
(Id, UserName, NormalizedUserName, Email, NormalizedEmail, IsAdmin, IsOwner)
VALUES 
('admin-id-123', 'admin', 'ADMIN', 'admin@test.com', 'ADMIN@TEST.COM', 1, 0)

-- Create test owner user
INSERT INTO AspNetUsers 
(Id, UserName, NormalizedUserName, Email, NormalizedEmail, IsAdmin, IsOwner)
VALUES 
('owner-id-456', 'owner', 'OWNER', 'owner@test.com', 'OWNER@TEST.COM', 1, 1)

-- Create test regular user
INSERT INTO AspNetUsers 
(Id, UserName, NormalizedUserName, Email, NormalizedEmail, IsAdmin, IsOwner)
VALUES 
('user-id-789', 'user', 'USER', 'user@test.com', 'USER@TEST.COM', 0, 0)
```

### Troubleshooting

| Issue                                     | Cause                   | Solution                                    |
|-------------------------------------------|-------------------------|---------------------------------------------|
| "IsAdmin is not a recognized property"    | Migration not run       | Run `dotnet ef database update`             |
| Cannot create user with validation errors | Invalid data            | Check password complexity, email format     |
| Owner can still be demoted                | Using SetAdminStatus()  | Use DemoteOwnerAsync() instead              |
| Multiple owners exist                     | Direct SQL modification | Run `PromoteUserToOwnerAsync()` to auto-fix |
| "User not found" errors                   | Wrong user ID format    | Verify UserId is GUID string, not numeric   |

### Performance Notes

- `FindOwnerAsync()` uses single DB query with `.FirstOrDefaultAsync()`
- `IsUserOwnerAsync()` requires DB lookup; cache if calling frequently
- Admin status checks use in-memory property (no DB call)
- Email/Username uniqueness checked by UserManager (built-in DB index)

### Security Checklist

- [x] Passwords hashed by AspNetIdentity (PBKDF2)
- [x] Email uniqueness enforced
- [x] Username uniqueness enforced
- [x] Admin operations require authorization
- [x] Owner cannot be demoted via code
- [x] IsOwner flag marks protected account
- [x] All changes logged
- [x] Direct SQL modifications still protected by application logic
