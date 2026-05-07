# Quick Test Execution Guide

## Running All Tests

```powershell
cd C:\Users\justi\source\repos\DoodleNote
dotnet test DoodleNote.Tests/DoodleNote.Tests.csproj
```

## Running Specific Test Classes

### ApplicationUserTests
```powershell
dotnet test --filter "ClassName=DoodleNote.Tests.ApplicationUserTests"
```

### AccountViewModelTests
```powershell
dotnet test --filter "ClassName=DoodleNote.Tests.AccountViewModelTests"
```

### AccountServiceTests
```powershell
dotnet test --filter "ClassName=DoodleNote.Tests.AccountServiceTests"
```

### IAdminTests
```powershell
dotnet test --filter "ClassName=DoodleNote.Tests.IAdminTests"
```

## Running Specific Tests

### Owner Protection Test
```powershell
dotnet test --filter "Name~SetAdminStatus_ThrowsInvalidOperationException_WhenDemotingOwner"
```

### Password Validation Tests
```powershell
dotnet test --filter "Name~Password_Validation"
```

### Email Validation Tests
```powershell
dotnet test --filter "Name~Email_Validation"
```

## Verbose Output

```powershell
dotnet test --verbosity detailed
```

## Test Summary

After running tests, you should see output similar to:

```
Passed: 70
Failed: 0
Skipped: 0
Time: 2.5 seconds
```

## Test Files & Line Count

| File                     | Tests  | Lines     |
|--------------------------|--------|-----------|
| ApplicationUserTests.cs  | 14     | ~180      |
| AccountViewModelTests.cs | 23     | ~310      |
| AccountServiceTests.cs   | 27     | ~610      |
| IAdminTests.cs           | 6      | ~60       |
| **Total**                | **70** | **1,160** |

## Key Test Scenarios Covered

### ✅ Owner Protection (CRITICAL)
- Owner cannot be demoted via SetAdminStatus() → **THROWS InvalidOperationException**
- Owner can be demoted via DemoteOwnerAsync() → **SUCCEEDS**
- IsAdmin must remain true if IsOwner = true

### ✅ Admin Authorization
- Only admins can grant admin status
- Only admins can revoke admin status
- Only admins can promote to owner
- Non-admins get UnauthorizedAccessException

### ✅ Validation
- Email: Required, valid format, max 256 chars
- Username: Required, 5-20 chars, unique, alphanumeric+_-
- Password: Required, 6-100 chars, 1+ digit, 1+ symbol
- ConfirmPassword: Must match Password

### ✅ Database Operations
- Find user by email
- Find user by username
- Check email uniqueness
- Check username uniqueness
- Find owner account
- Promote to owner
- Demote from owner

## Visual Studio Test Explorer

1. Open Visual Studio
2. Test → Test Explorer (Ctrl+E, T)
3. Double-click test to run
4. View Results pane for details

## Expected Test Results

All 70 tests should **PASS** ✅

If any test FAILS:
1. Check error message in Test Explorer
2. Review the test code
3. Verify implementation in ApplicationUser, AccountViewModel, or AccountService
4. Debug using breakpoints

## Test Dependencies

Required NuGet packages (already in project):
- xunit (2.4.2)
- Moq (4.18.4)
- Microsoft.NET.Test.Sdk (17.8.0)
- Microsoft.EntityFrameworkCore.InMemory (10.0.5)

## Performance Note

First test run may take 2-3 seconds due to:
- xUnit discovery
- Moq setup
- EF Core initialization

Subsequent runs typically 1-2 seconds

## CI/CD Integration

These tests can run in:
- GitHub Actions
- Azure Pipelines
- GitLab CI
- Jenkins
- Local pre-commit hooks

Example GitHub Actions:
```yaml
- name: Run Tests
  run: dotnet test DoodleNote.Tests/DoodleNote.Tests.csproj
```
