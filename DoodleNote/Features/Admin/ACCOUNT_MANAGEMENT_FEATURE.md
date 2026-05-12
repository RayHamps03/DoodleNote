## Account Management Feature

### Overview
The Admin Account Management feature allows site owners and administrators to manage user accounts through a paginated, role-based interface. This feature is self-contained within the `Features/Admin` folder and can be removed without breaking the core application.

### Components Created

#### Models
- **AccountManagementViewModel**: Displays individual account information including roles and permission flags
- **PaginatedAccountsViewModel**: Paginated container for displaying multiple accounts with total counts and navigation

#### Controller
- **AdminAccountsController**: Handles all account management operations with authorization
  - `Index(page)`: Displays paginated list of accounts
  - `RemoveAccount(userId)`: Removes a user account from the database
  - `AssignRole(userId, role)`: Assigns a role to a user (owner-only)
  - `RemoveRole(userId, role)`: Removes a role from a user (owner-only)

#### View
- **Views/AdminAccounts/Index.cshtml**: Responsive card-based UI for account management with:
  - Account listings with username, email, and assigned roles
  - Role dropdown for assigning and removing roles (owner-only)
  - Remove account button with appropriate permission checks
  - Pagination controls

### Access Control Rules

1. **Page Access**: Only users with Admin or Owner roles can access the Account Management page
2. **Account Removal**:
   - **Owner**: Can remove any account
   - **Admin**: Can only remove regular users, not other admins or the owner
   - **Prevention**: Users cannot remove their own accounts
3. **Role Management**: Only the Owner can assign or revoke roles

### User Interface Features

- **Paginated Display**: 10 accounts per page with navigation
- **Card Layout**: Clean Bootstrap cards showing account details
- **Role Badges**: Visual badges indicating user roles
- **Conditional Controls**: Buttons only appear if the current user has permission
- **Dropdowns**: Separate dropdowns for assigning and removing roles (owner-only)
- **Confirmation**: Delete actions require confirmation

### Integration

1. **Home Page**: Admin/Owner users see a "Manage Accounts" button on the home page
2. **Role-Based Authorization**: Uses the existing RoleService for all role operations
3. **View Imports**: Added to _ViewImports.cshtml for accessibility across views

### Database Interactions

- Uses existing `ApplicationUser` and Identity tables
- Leverages EF Core `UserManager<ApplicationUser>` for account operations
- Performs paginated queries for efficient data retrieval

### Security Considerations

- All actions require `[Authorize(Roles = "Admin,Owner")]`
- SQL injection prevented through parameterized queries (EF Core)
- CSRF protection via `[ValidateAntiForgeryToken]`
- Role-based authorization prevents unauthorized actions
- Self-deletion prevention

### Files Modified/Created
- Created: `DoodleNote/Features/Admin/Models/AccountManagementViewModel.cs`
- Created: `DoodleNote/Features/Admin/Models/PaginatedAccountsViewModel.cs`
- Created: `DoodleNote/Controllers/AdminAccountsController.cs`
- Created: `DoodleNote/Views/AdminAccounts/Index.cshtml`
- Modified: `DoodleNote/Views/Home/Index.cshtml` (added Manage Accounts button)
- Modified: `DoodleNote/Views/_ViewImports.cshtml` (added namespaces)

### Testing
All existing tests (56) continue to pass. The feature is ready for manual testing and deployment.
