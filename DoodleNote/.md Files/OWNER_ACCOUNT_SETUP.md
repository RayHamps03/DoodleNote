## Owner Account Setup Guide

This document explains how to set up and manage the owner account using SQL Server Object Explorer.

### Prerequisites
1. Ensure the database migration has been run:
   ```powershell
   dotnet ef migrations add AddApplicationUserIsAdminAndOwner --output-dir Data/Migrations
   dotnet ef database update
   ```

### Database Schema
The `AspNetUsers` table now includes two new boolean columns:

| Column    | Type | Default   | Purpose                                    |
|-----------|------|-----------|--------------------------------------------|
| `IsAdmin` | bit  | 0 (false) | Indicates if user has admin privileges     |
| `IsOwner` | bit  | 0 (false) | Marks the system owner (cannot be demoted) |

### Setting Up the First Owner Account

#### Step 1: Create a User Account
1. Run the application and create a regular user account through the registration page
2. Or create a test user via code using the `AccountService`

#### Step 2: Promote to Owner via SQL Server Object Explorer

1. **Open SQL Server Object Explorer**
   - In Visual Studio: View → SQL Server Object Explorer
   - Or connect via SQL Server Management Studio (SSMS)

2. **Navigate to the AspNetUsers table**
   - Find your database connection
   - Expand: Databases → [YourDatabaseName] → Tables → dbo.AspNetUsers

3. **Edit the Owner User Record**
   - Right-click on `dbo.AspNetUsers`
   - Select "View Data"
   - Find the user you want to promote to owner
   - Set `IsAdmin = 1` (true)
   - Set `IsOwner = 1` (true)
   - Save changes

#### Alternative: Direct SQL Query
```sql
UPDATE AspNetUsers 
SET IsAdmin = 1, IsOwner = 1 
WHERE UserName = 'your_username_here'
```

### Protection Mechanisms

#### What Cannot Be Done in SQL Server Object Explorer

Although you can directly modify the database, the application includes protective mechanisms:

1. **Owner Account Protection**: The owner's `IsAdmin` status cannot be set to false through the application code
   - If `SetAdminStatus()` method is called to demote the owner, it throws `InvalidOperationException`
   - Direct SQL modifications will succeed, but the application logic will prevent demotion via `SetAdminStatus()`

2. **Enforced via Application Logic**
   - The `SetAdminStatus()` method checks if user is owner
   - If attempt to demote owner: throws exception "The owner account cannot be demoted"
   - If requesting user is not admin: throws exception "Only admin users can change admin status"

#### Direct Database Modification Behavior

If you modify the database directly via SQL Server Object Explorer:

- **Setting `IsOwner = 0` (removing owner status)**: Changes will persist in database
- **Setting `IsAdmin = 0` while `IsOwner = 1`**: Application logic will prevent actions that require admin

**Recommendation**: Always demote the owner using the application code through `AccountService.DemoteOwnerAsync()` to ensure consistency.

### Managing Owner Status via Code

#### Promoting a User to Owner
```csharp
var user = await accountService.FindUserByUsernameAsync("username");
var currentAdmin = // Get current admin user
var result = await accountService.PromoteUserToOwnerAsync(user.Id, currentAdmin);

if (result.Succeeded)
{
    // User is now owner
}
```

#### Demoting the Owner
```csharp
var owner = await accountService.FindOwnerAsync();
var admin = // Get current admin user
var result = await accountService.DemoteOwnerAsync(owner.Id, admin);

if (result.Succeeded)
{
    // Owner demoted to regular admin
}
```

#### Checking If User is Owner
```csharp
var isOwner = await accountService.IsUserOwnerAsync(userId);
```

### Best Practices

1. **Never Set Both IsAdmin and IsOwner to false**: This would break the system
2. **Always Maintain at Least One Admin**: The system requires at least one admin user
3. **Only One Owner**: While not enforced at the database level, the system expects only one owner
4. **Use Application Code When Possible**: Modifying via `AccountService` ensures validation
5. **Backup Before Changes**: Always backup your database before modifying user roles

### Verification Script

To verify your setup:

```sql
SELECT Id, UserName, Email, IsAdmin, IsOwner 
FROM AspNetUsers 
WHERE IsOwner = 1 OR IsAdmin = 1
ORDER BY IsOwner DESC, IsAdmin DESC
```

This query shows all admins and the owner account.

### Troubleshooting

**Issue**: Cannot demote the owner through the application
- **Cause**: Owner protection is active (expected behavior)
- **Solution**: Use `DemoteOwnerAsync()` method or modify directly via SQL if absolutely necessary

**Issue**: Owner account lost (both IsAdmin and IsOwner = 0)
- **Cause**: Direct database modification without application logic
- **Solution**: 
  ```sql
  UPDATE AspNetUsers 
  SET IsAdmin = 1, IsOwner = 1 
  WHERE Id = 'owner_user_id'
  ```

**Issue**: Multiple users have IsOwner = 1
- **Cause**: Direct database modification or application error
- **Solution**: Use `PromoteUserToOwnerAsync()` which automatically demotes previous owner
