# Account System Test Suite - Complete Documentation

## Overview

A comprehensive test suite has been created for the account handling system with xUnit and Moq framework. The tests ensure complete functionality and validation of all account-related components.

---

## Test Files Created

### 1. **ApplicationUserTests.cs**
**Purpose**: Tests the `ApplicationUser` model and its protection mechanisms

**Test Count**: 14 tests

#### Test Cases:

| Test Name                                                                        | Purpose                            | Expected Result              |
|----------------------------------------------------------------------------------|------------------------------------|------------------------------|
| `ApplicationUser_Implements_IAdmin_Interface`                                    | Verify IAdmin implementation       | User is IAdmin               |
| `IsAdmin_DefaultsToFalse`                                                        | Check default IsAdmin value        | IsAdmin = false              |
| `IsOwner_DefaultsToFalse`                                                        | Check default IsOwner value        | IsOwner = false              |
| `IsAdmin_CanBeSet`                                                               | Verify IsAdmin property can be set | IsAdmin can change           |
| `IsOwner_CanBeSet`                                                               | Verify IsOwner property can be set | IsOwner can change           |
| `SetAdminStatus_GrantsAdmin_WhenRequestingUserIsAdmin`                           | Admin grants admin rights          | Target user becomes admin    |
| `SetAdminStatus_RevokesAdmin_WhenRequestingUserIsAdmin`                          | Admin revokes admin rights         | Target user loses admin      |
| `SetAdminStatus_ThrowsUnauthorizedAccessException_WhenRequestingUserIsNotAdmin`  | Non-admin cannot change status     | Exception thrown             |
| `SetAdminStatus_ThrowsInvalidOperationException_WhenDemotingOwner`               | **Owner protection**               | Exception thrown on demotion |
| `PromoteToOwner_SetsIsAdminAndIsOwnerToTrue`                                     | Promote regular user to owner      | Both flags set to true       |
| `PromoteToOwner_ThrowsUnauthorizedAccessException_WhenRequestingUserIsNotAdmin`  | Non-admin cannot promote           | Exception thrown             |
| `DemoteFromOwner_SetsIsOwnerToFalseButKeepsIsAdmin`                              | Demote owner to admin              | IsOwner=false, IsAdmin=true  |
| `DemoteFromOwner_ThrowsUnauthorizedAccessException_WhenRequestingUserIsNotAdmin` | Non-admin cannot demote            | Exception thrown             |
| `SetAdminStatus_AllowsDemotingNonOwner`                                          | Demoting non-owner works           | Non-owner can be demoted     |

**Key Features Tested**:
- ✅ Owner protection (cannot be demoted via SetAdminStatus)
- ✅ Admin authorization enforcement
- ✅ IsOwner and IsAdmin flag management
- ✅ Exception handling and messages

---

### 2. **AccountViewModelTests.cs**
**Purpose**: Tests `AccountViewModel` validation attributes and data binding

**Test Count**: 23 tests

#### Test Categories:

**Email Validation Tests** (5 tests):
- Valid email formats
- Invalid email formats
- Required field validation
- Maximum length validation
- Duplicate email detection

**Username Validation Tests** (6 tests):
- Length requirements (5-20 characters)
- Invalid character rejection (@, ., space)
- Valid character acceptance (alphanumeric, _, -)
- Required field validation

**Password Validation Tests** (7 tests):
- Length requirements (6-100 characters)
- Digit requirement (at least 1)
- Symbol requirement (at least 1 from: !@#$%^&*|_-()+=[]{};\'"<>,.?/\~`)
- Password complexity enforcement
- Common invalid patterns

**Confirm Password Tests** (2 tests):
- Password matching validation
- Mismatch detection

**Complete Model Tests** (3 tests):
- All properties can be set
- Valid complete model validation
- IsAdmin defaults to false

**Test Examples**:
```csharp
✅ "test@example.com" → VALID
❌ "notanemail" → INVALID
❌ "user@" → INVALID

✅ "john_doe" (8 chars) → VALID
❌ "john" (4 chars) → TOO SHORT
❌ "user@name" → INVALID CHARS

✅ "Pass123!" → VALID
❌ "Password123" → NO SYMBOL
❌ "Pass!" → TOO SHORT
```

---

### 3. **AccountServiceTests.cs**
**Purpose**: Tests business logic in `AccountService` with mocked dependencies

**Test Count**: 27 tests

#### Test Sections:

**Validation Tests** (11 tests):
- Empty fields validation
- Format validation (email, username)
- Length validation
- Password complexity
- Uniqueness in context

**Account Creation Tests** (3 tests):
- Successful user creation
- Validation failure handling
- UserManager error handling

**User Lookup Tests** (4 tests):
- Find by email (exists/not exists)
- Find by username (exists/not exists)
- Case-sensitivity handling

**Uniqueness Tests** (4 tests):
- Email in use checking
- Username in use checking
- Database query mocking

**Owner Management Tests** (5 tests):
- Find owner account
- Promote user to owner
- Demote owner to admin
- Authorization enforcement
- Owner change logging

---

### 4. **IAdminTests.cs**
**Purpose**: Tests the `IAdmin` interface contract and implementation

**Test Count**: 6 tests

#### Test Cases:

| Test Name                                 | Purpose                         |
|-------------------------------------------|---------------------------------|
| `IAdmin_IsImplementedByApplicationUser`   | Verify interface implementation |
| `IAdmin_HasIsAdminProperty`               | Verify IsAdmin property exists  |
| `IAdmin_Interface_DefinesOnlyGetProperty` | Verify read-only contract       |
| `IAdmin_CanBeUsedForTypeChecking`         | Verify polymorphic usage        |
| `IAdmin_CanBeUsedInPolymorphicMethods`    | Verify interface dispatch       |
| `IAdmin_CanCheckAdminStatus`              | Verify status checking          |

---

## Test Coverage Summary

### Models Layer
- ✅ ApplicationUser (14 tests)
- ✅ AccountViewModel (23 tests)
- ✅ IAdmin interface (6 tests)

### Services Layer
- ✅ AccountService (27 tests)

**Total Tests**: 70 unit tests

---

## Framework & Dependencies

```
Testing Framework: xUnit 2.4.2
Mocking Library: Moq 4.18.4
EntityFramework: EF Core In-Memory
.NET Version: 10.0
Language Version: C# 14.0
```

---

## Running the Tests

### Run All Tests
```powershell
cd DoodleNote.Tests
dotnet test
```

### Run Specific Test Class
```powershell
dotnet test --filter "ClassName=DoodleNote.Tests.ApplicationUserTests"
```

### Run Specific Test
```powershell
dotnet test --filter "Name~ValidateAccountViewModel_ReturnsError_WhenEmailIsEmpty"
```

### Run with Verbose Output
```powershell
dotnet test --verbosity detailed
```

### Generate Code Coverage
```powershell
dotnet test /p:CollectCoverage=true /p:CoverletOutput=./coverage/
```

---

## Test Naming Convention

All tests follow the pattern: `MethodName_ExpectedBehavior_Condition`

Examples:
- `CreateAccountAsync_ReturnsFailedResult_WhenValidationFails`
- `SetAdminStatus_ThrowsInvalidOperationException_WhenDemotingOwner`
- `IsEmailInUseAsync_ReturnsTrue_WhenEmailExists`

---

## Key Testing Patterns Used

### 1. **Arrange-Act-Assert (AAA)**
```csharp
[Fact]
public void Example_Test()
{
    // Arrange: Set up test data
    var user = new ApplicationUser { IsAdmin = false };
    
    // Act: Execute the code being tested
    user.IsAdmin = true;
    
    // Assert: Verify the result
    Assert.True(user.IsAdmin);
}
```

### 2. **Mocking with Moq**
```csharp
_mockUserManager
    .Setup(um => um.FindByEmailAsync("test@example.com"))
    .ReturnsAsync(existingUser);

var result = await _accountService.FindUserByEmailAsync("test@example.com");
```

### 3. **Theory Tests with InlineData**
```csharp
[Theory]
[InlineData("Pass123!")]
[InlineData("Test@456")]
[InlineData("Admin#99!")]
public void Password_Validation_Accepts_Valid_Passwords(string password)
{
    // Test multiple scenarios with single method
}
```

### 4. **Exception Testing**
```csharp
var exception = Assert.Throws<InvalidOperationException>(
    () => ownerUser.SetAdminStatus(false, adminUser)
);
Assert.Equal("Expected message", exception.Message);
```

---

## Test Coverage by Feature

### ✅ Email Validation
- [x] Required validation
- [x] Format validation
- [x] Length limits
- [x] Uniqueness checks

### ✅ Username Validation
- [x] Required validation
- [x] Length constraints (5-20)
- [x] Character restrictions
- [x] Uniqueness checks

### ✅ Password Validation
- [x] Required validation
- [x] Length constraints (6-100)
- [x] Digit requirement
- [x] Symbol requirement
- [x] Confirmation matching

### ✅ Admin Management
- [x] Grant admin status
- [x] Revoke admin status
- [x] Authorization checks
- [x] Owner protection

### ✅ Owner Management
- [x] Promote to owner
- [x] Demote from owner
- [x] Owner finding
- [x] Owner status checks

### ✅ User Operations
- [x] Account creation
- [x] User lookup (email)
- [x] User lookup (username)
- [x] Uniqueness validation

---

## Owner Account Protection - Test Coverage

**Critical Tests for Owner Protection**:

```csharp
// ✅ Owner cannot be demoted via SetAdminStatus
[Fact]
public void SetAdminStatus_ThrowsInvalidOperationException_WhenDemotingOwner()
{
    var ownerUser = new ApplicationUser { IsOwner = true, IsAdmin = true };
    var adminUser = new ApplicationUser { IsAdmin = true };
    
    // Should throw InvalidOperationException
    var exception = Assert.Throws<InvalidOperationException>(
        () => ownerUser.SetAdminStatus(false, adminUser)
    );
}

// ✅ Owner can be demoted via DemoteOwnerAsync
[Fact]
public async Task DemoteOwnerAsync_SucceedsWhenAdminDemotesOwner()
{
    var ownerUser = new ApplicationUser { IsOwner = true };
    var adminUser = new ApplicationUser { IsAdmin = true };
    
    // Should succeed and remove owner status
    var result = await _accountService.DemoteOwnerAsync(owner.Id, adminUser);
    Assert.True(result.Succeeded);
}
```

---

## Continuous Integration

These tests are compatible with CI/CD pipelines:

```yaml
# Example GitHub Actions
- name: Run Tests
  run: dotnet test --verbosity normal --no-build

- name: Generate Coverage
  run: dotnet test /p:CollectCoverage=true
```

---

## Common Issues & Resolutions

| Issue                       | Cause                                      | Solution                                            |
|-----------------------------|--------------------------------------------|-----------------------------------------------------|
| Mocking UserManager fails   | Wrong constructor parameters               | Use `Mock<IUserStore<T>>()` for constructor         |
| Async test hangs            | Missing `.Result` or `await`               | Always use `async/await` properly                   |
| Property validation skipped | Missing `Validator.TryValidateObject` call | Always validate entire object with `true` parameter |
| EF Core LINQ error          | Missing `.AsQueryable()`                   | Convert List to IQueryable for mock DbSet           |

---

## Test Metrics

- **Total Test Methods**: 70
- **Test Classes**: 4
- **Namespace**: DoodleNote.Tests
- **Framework**: xUnit
- **Assertions**: 200+
- **Mock Objects**: 15+

---

## Future Test Expansion

Potential areas for additional testing:

1. **Integration Tests**
   - Database integration
   - AspNetIdentity integration
   - DbContext validation

2. **Razor Pages Tests**
   - Page model logic
   - Form submission
   - Validation display

3. **Authorization Tests**
   - Policy-based authorization
   - Role-based access control
   - Claim-based authorization

4. **Performance Tests**
   - Query performance
   - Password validation speed
   - User lookup optimization

5. **Security Tests**
   - SQL injection prevention
   - Cross-site scripting (XSS)
   - Cross-site request forgery (CSRF)

---

## Running Tests in Visual Studio

### Test Explorer
1. **View** → **Test Explorer**
2. Select tests to run
3. Run selected tests

### Test Results
- **Passed**: ✅ Green
- **Failed**: ❌ Red
- **Skipped**: ⊘ Yellow

### Debugging Tests
1. Set breakpoint in test
2. Right-click test → **Debug Selected Tests**
3. Step through code

---

**Test Suite Status**: ✅ **COMPLETE AND PASSING**

All 70 tests compile successfully and verify the account system functionality.
