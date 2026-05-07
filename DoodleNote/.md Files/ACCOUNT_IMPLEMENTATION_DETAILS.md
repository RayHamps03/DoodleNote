## Account Implementation Summary

### Overview
This implementation provides a complete account handling system for DoodleNote that integrates AspNetIdentity with application-specific properties for admin and owner management.

### Created Components

#### 1. ApplicationUser Model (`Models/ApplicationUser.cs`)
**Purpose**: Custom IdentityUser class with admin and owner functionality

**Properties**:
- `Id` (inherited from IdentityUser): Primary key
- `UserName`: Username (5-20 characters, unique)
- `Email`: Email address (required, unique, valid email format)
- `PasswordHash` (inherited): Hashed password
- `IsAdmin`: Boolean flag for admin privileges (default: false)
- `IsOwner`: Boolean flag marking the system owner (default: false)

**Methods**:
- `SetAdminStatus(bool isAdmin, IAdmin requestingUser)`: Changes admin status with authorization
  - Throws `UnauthorizedAccessException` if requester is not admin
  - Throws `InvalidOperationException` if trying to demote the owner
  
- `PromoteToOwner(IAdmin requestingUser)`: Promotes user to owner
  - Automatically sets IsAdmin to true
  - Throws `UnauthorizedAccessException` if requester is not admin
  
- `DemoteFromOwner(IAdmin requestingUser)`: Demotes owner to regular admin
  - Keeps IsAdmin as true
  - Throws `UnauthorizedAccessException` if requester is not admin

#### 2. IAdmin Interface (`Models/IAdmin.cs`)
**Purpose**: Defines the contract for objects with admin privileges

**Properties**:
- `IsAdmin` (get-only): Boolean indicating admin status

**Usage**: Used for type-safe privilege checking throughout the application

#### 3. AccountViewModel (`Models/AccountViewModel.cs`)
**Purpose**: View model for account registration and management

**Properties with Validation**:

| Property          | Validation                                     | Purpose                         |
|-------------------|------------------------------------------------|---------------------------------|
| `UserId`          | Read-only                                      | Display-only primary key        |
| `Email`           | Required, EmailAddress, max 256 chars          | User email address              |
| `Username`        | Required, 5-20 chars, alphanumeric+underscores | Unique username                 |
| `Password`        | Required, 6-100 chars, min 1 number + 1 symbol | Account password                |
| `ConfirmPassword` | Must match Password                            | Password confirmation           |
| `IsAdmin`         | Hidden input                                   | Admin status (hidden from user) |

**Password Symbol Pattern**: `!@#$%^&*|_-()+=[]{};\'"<>,.?/\~` ` (backtick)

#### 4. ApplicationDbContext (`Data/ApplicationDbContext.cs`)
**Purpose**: Entity Framework Core context with custom user configuration

**Configuration**:
```csharp
// IsAdmin defaults to false
builder.Entity<ApplicationUser>()
    .Property(u => u.IsAdmin)
    .HasDefaultValue(false);

// IsOwner defaults to false
builder.Entity<ApplicationUser>()
    .Property(u => u.IsOwner)
    .HasDefaultValue(false);
```

**DbSets**:
- `DbSet<ApplicationUser>` (inherited)
- `DbSet<DoodleNote>`

#### 5. AccountService (`Services/AccountService.cs`)
**Purpose**: Business logic layer for account operations

**Methods**:

| Method                       | Parameters              | Returns                            | Purpose                      |
|------------------------------|-------------------------|------------------------------------|------------------------------|
| `ValidateAccountViewModel()` | AccountViewModel        | List<string> errors                | Validates all account fields |
| `CreateAccountAsync()`       | AccountViewModel        | (IdentityResult, ApplicationUser?) | Creates new user account     |
| `FindUserByEmailAsync()`     | email                   | ApplicationUser?                   | Searches by email            |
| `FindUserByUsernameAsync()`  | username                | ApplicationUser?                   | Searches by username         |
| `IsEmailInUseAsync()`        | email                   | bool                               | Checks email uniqueness      |
| `IsUsernameInUseAsync()`     | username                | bool                               | Checks username uniqueness   |
| `FindOwnerAsync()`           | -                       | ApplicationUser?                   | Finds the system owner       |
| `PromoteUserToOwnerAsync()`  | userId, requestingUser  | IdentityResult                     | Promotes user to owner       |
| `DemoteOwnerAsync()`         | ownerId, requestingUser | IdentityResult                     | Demotes owner to admin       |
| `IsUserOwnerAsync()`         | userId                  | bool                               | Checks if user is owner      |

### Database Migration Required

You must create and run a migration to add the new columns:

```powershell
dotnet ef migrations add AddApplicationUserIsAdminAndOwner --output-dir Data/Migrations
dotnet ef database update
```

This migration creates two new columns in the `AspNetUsers` table:
- `IsAdmin` (BIT, default: 0)
- `IsOwner` (BIT, default: 0)

### Security Features

#### 1. Password Validation
- Minimum 6 characters, maximum 100 characters
- Must contain at least one digit (0-9)
- Must contain at least one symbol from: `!@#$%^&*|_-()+=[]{};\'"<>,.?/\~` `
- Example valid passwords: `Password1!`, `Test@123`, `Admin#99`

#### 2. Username Uniqueness
- Validated through AspNetIdentity UserManager
- Must be 5-20 characters
- Allows letters, numbers, underscores, hyphens

#### 3. Email Uniqueness
- Validated through AspNetIdentity UserManager
- Must follow valid email format

#### 4. Admin Privilege Protection
- Only admins can change other users' admin status
- Owner cannot be demoted to non-admin through application code
- All privilege changes are logged

#### 5. Owner Account Protection
- Marked with `IsOwner = 1` flag
- Cannot have `IsAdmin` set to false while `IsOwner = 1`
- Attempts to demote owner throw `InvalidOperationException`
- Only way to remove owner status: use `DemoteOwnerAsync()` method

### Integration with Razor Pages

To use this in your Razor Pages:

#### 1. Register AccountService in Startup
```csharp
// Program.cs or Startup.cs
services.AddScoped<AccountService>();
```

#### 2. Inject Into Page Models
```csharp
public class RegisterModel : PageModel
{
    private readonly AccountService _accountService;
    
    public RegisterModel(AccountService accountService)
    {
        _accountService = accountService;
    }
    
    public AccountViewModel Account { get; set; } = new();
    
    public async Task<IActionResult> OnPostAsync()
    {
        var (result, user) = await _accountService.CreateAccountAsync(Account);
        // Handle result...
    }
}
```

#### 3. Display in Forms
```html
<form method="post">
    <input asp-for="Account.Email" />
    <span asp-validation-for="Account.Email"></span>
    
    <input asp-for="Account.Username" />
    <span asp-validation-for="Account.Username"></span>
    
    <input asp-for="Account.Password" type="password" />
    <span asp-validation-for="Account.Password"></span>
    
    <input asp-for="Account.ConfirmPassword" type="password" />
    <span asp-validation-for="Account.ConfirmPassword"></span>
    
    <button type="submit">Register</button>
</form>
```

### Database Modification via SQL Server Object Explorer

Users can directly modify the `IsAdmin` and `IsOwner` columns in the AspNetUsers table:

1. Open SQL Server Object Explorer
2. Navigate to Databases → [Database] → Tables → dbo.AspNetUsers
3. Right-click and select "View Data"
4. Find the user and update:
   - `IsAdmin = 1` to make admin
   - `IsOwner = 1` to make owner

**Important**: Direct database modifications bypass application logic, but the application will still enforce the owner protection rules.

### Logging

All significant account operations are logged:
- User account creation
- Admin status changes
- Owner promotion/demotion
- Failed operations

Logs can be found in Visual Studio Output window or application logs.

### Error Handling

The implementation includes comprehensive error messages:

| Error                                                       | Cause                       | Solution                      |
|-------------------------------------------------------------|-----------------------------|-------------------------------|
| "Email is required."                                        | Empty email field           | Provide an email              |
| "Email must be a valid email address."                      | Invalid email format        | Use valid email format        |
| "Username is required."                                     | Empty username              | Provide a username            |
| "Username must be between 5 and 20 characters."             | Username length invalid     | Use 5-20 characters           |
| "Password is required."                                     | Empty password              | Provide a password            |
| "Password must contain at least one number and one symbol." | Password lacks complexity   | Add a number and symbol       |
| "Only admin users can change admin status."                 | Non-admin attempting change | Use an admin account          |
| "The owner account cannot be demoted."                      | Trying to demote owner      | Use DemoteOwnerAsync() method |

### Testing Considerations

When testing the account system:

1. **Test Email Validation**
   - Invalid format: "notanemail"
   - Duplicate: Register same email twice
   
2. **Test Password Validation**
   - Too short: "Pass1!"
   - No symbol: "Password1"
   - No number: "Password!"
   
3. **Test Username Validation**
   - Too short: "User"
   - Too long: "ThisIsAVeryLongUsername"
   - Invalid chars: "User@Name"
   - Duplicate: Register same username twice
   
4. **Test Admin Operations**
   - Non-admin trying to set IsAdmin: Should fail
   - Admin setting another user as admin: Should succeed
   
5. **Test Owner Protection**
   - Try to demote owner via SetAdminStatus(): Should throw InvalidOperationException
   - Try to demote owner via DemoteOwnerAsync(): Should succeed
