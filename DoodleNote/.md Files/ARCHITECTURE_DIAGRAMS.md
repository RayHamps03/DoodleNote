# Account Implementation - Visual Architecture & Diagrams

## System Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                    RAZOR PAGES PRESENTATION LAYER               │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  RegisterModel.cshtml          AdminModel.cshtml                │
│      │                              │                           │
│      ├─ AccountViewModel    ├─ ApplicationUser                  │
│      └─ Validation          └─ IsAdmin, IsOwner                 │
│                                                                 │
└────────────────────────┬────────────────────────────────────────┘
                         │
┌────────────────────────▼────────────────────────────────────────┐
│              BUSINESS LOGIC LAYER (Services)                    │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  AccountService                                                 │
│  ├─ CreateAccountAsync()                                        │
│  ├─ FindUserByEmailAsync()                                      │
│  ├─ PromoteUserToOwnerAsync()  ← OWNER MANAGEMENT               │
│  ├─ DemoteOwnerAsync()         ← OWNER PROTECTION               │
│  ├─ SetAdminStatus()           ← AUTHORIZATION                  │
│  └─ ValidateAccountViewModel() ← VALIDATION                     │
│                                                                 │
└────────────────────────┬────────────────────────────────────────┘
                         │
┌────────────────────────▼────────────────────────────────────────┐
│                    MODEL LAYER (Entities)                       │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ApplicationUser (extends IdentityUser)                         │
│  ├─ Id: string (readonly PK)                                    │
│  ├─ UserName: string (unique)                                   │
│  ├─ Email: string (unique)                                      │
│  ├─ PasswordHash: string                                        │
│  ├─ IsAdmin: bool ────────────────┐                             │
│  ├─ IsOwner: bool ────────────────┤─ Controlled Properties      │
│  ├─ SetAdminStatus(IAdmin)  ──────┤─ Authorization Checks       │
│  ├─ PromoteToOwner(IAdmin)  ──────┤─ Owner Protection           │
│  └─ DemoteFromOwner(IAdmin) ──────┘                             │
│                                                                 │
│  IAdmin (Interface)                                             │
│  └─ IsAdmin { get; }                                            │
│                                                                 │
│  AccountViewModel                                               │
│  ├─ UserId (readonly)                                           │
│  ├─ Email (validated, unique)                                   │
│  ├─ Username (validated, unique)                                │
│  ├─ Password (validated, complex)                               │
│  ├─ ConfirmPassword (validated, matches)                        │
│  └─ IsAdmin (hidden input)                                      │
│                                                                 │
└────────────────────────┬────────────────────────────────────────┘
                         │
┌────────────────────────▼────────────────────────────────────────┐
│               DATA ACCESS LAYER (DbContext)                     │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ApplicationDbContext : IdentityDbContext<ApplicationUser>      │
│  ├─ Users (AspNetUsers table)                                   │
│  │  ├─ Id (string, PK)                                          │
│  │  ├─ UserName (nvarchar, unique)                              │
│  │  ├─ Email (nvarchar, unique)                                 │
│  │  ├─ PasswordHash (nvarchar)                                  │
│  │  ├─ IsAdmin (bit, default: 0)           ← NEW                │
│  │  └─ IsOwner (bit, default: 0)           ← NEW                │
│  └─ DoodleNotes (existing table)                                │
│                                                                 │
└────────────────────────┬────────────────────────────────────────┘
                         │
┌────────────────────────▼────────────────────────────────────────┐
│                   SQL SERVER DATABASE                           │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  AspNetUsers (with new columns)                                 │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │ Id │ UserName │ Email │ PasswordHash │ IsAdmin │ IsOwner  │  │
│  ├───────────────────────────────────────────────────────────┤  │
│  │ 1  │ john     │ j@..  │ hash...      │    0    │    0     │  │
│  │ 2  │ admin    │ a@..  │ hash...      │    1    │    0     │  │
│  │ 3  │ owner    │ o@..  │ hash...      │    1    │    1     │  │  
│  └───────────────────────────────────────────────────────────┘  │
│                                                                 │
│   Owner Protection: When IsOwner=1, SetAdminStatus(false)       │
│                     throws InvalidOperationException            │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘

             ▲
             │
             │ SQL Server Object Explorer
             │ Can directly modify:
             │ • IsAdmin (0 or 1)
             │ • IsOwner (0 or 1)
             │
             │ BUT application logic prevents
             │ owner demotion via SetAdminStatus()
             │
```

## Owner Protection Flow Diagram

```
SCENARIO: Attempt to Demote Owner
┌─────────────────────────────────────────────────────────────────┐
│ USER: Admin tries to remove owner's admin status                │
│ owner.SetAdminStatus(false, adminUser)                          │
└─────────────────────┬───────────────────────────────────────────┘
                      │
                      ▼
        ┌─────────────────────────────┐
        │Check: adminUser.IsAdmin?    │
        └──────┬──────────────────────┘
             /  \
          YES    NO
         /        \
        ▼          ▼
    CONTINUE   THROW: UnauthorizedAccessException
               "Only admin users can..."
        │
        ▼
    ┌─────────────────────────────────────┐
    │Check: this.IsOwner?                 │
    └──────┬──────────────────────────────┘
         /  \
        YES   NO
       /      \
      ▼        ▼
    THROW   CONTINUE
    InvalidOperationException     │
    "The owner account cannot     ▼
     be demoted"        ┌─────────────┐
                        │Set IsAdmin  │
                        │as requested │
                        └─────────────┘
```

## SQL Server Object Explorer Setup Flow

```
┌──────────────────────────────────────────────────────────────┐
│ SQL SERVER OBJECT EXPLORER                                   │
│ Databases → DoodleNote → Tables → dbo.AspNetUsers            │
│ View Data → Find user row                                    │
└─────────────────────┬────────────────────────────────────────┘
                      │
                      ▼
    ┌────────────────────────────────┐
    │Set IsAdmin = 1                 │
    │Set IsOwner = 1                 │
    │Save Changes                    │
    └────┬───────────────────────────┘
         │
         ▼
    ┌──────────────────────────────────────┐
    │Database Updated                      │
    │ user.IsAdmin = true                  │
    │ user.IsOwner = true                  │
    │   User is now OWNER                  │
    └──────────────────────────────────────┘
         │
         ▼
    ┌──────────────────────────────────────┐
    │Application Logic Activates           │
    │ If SetAdminStatus(false) called:     │
    │  • Check IsOwner = true?             │
    │  • YES: Throw InvalidOperationExc    │
    │  • NO: Allow change                  │
    └──────────────────────────────────────┘
```

## Authorization Matrix

```
┌─────────────────────────────────────────────────────────────────┐
│ WHO CAN DO WHAT?                                                │
├─────────────────────────────────────────────────────────────────┤
│ ACTION                    │ Regular User │ Admin │ Owner        │
├─────────────────────────────────────────────────────────────────┤
│ Create Account            │ ✅ Self      │ ✅    │ ✅          │
│ View Own Profile          │ ✅           │ ✅    │ ✅          │
│ Change Own Password       │ ✅           │ ✅    │ ✅          │
│ Set User as Admin         │ ❌           │ ✅    │ ✅          │
│ Remove User's Admin       │ ❌           │ ✅ *  │ ✅ *        │
│ Promote to Owner          │ ❌           │ ❌    │ ✅          │
│ Demote Owner              │ ❌           │ ❌    │ ✅          │
│ Delete User Account       │ ❌           │ ❌    │ ✅          │
│ View All Users            │ ❌           │ ✅    │ ✅          │
│ Modify Database Directly  │ N/A          │ N/A    │ DBA         │
├─────────────────────────────────────────────────────────────────┤
│ * = Cannot demote owner through SetAdminStatus(), must use      │
│     DemoteOwnerAsync()                                          │
└─────────────────────────────────────────────────────────────────┘
```

## Password Validation Symbols

```
ALLOWED SYMBOLS IN PASSWORD (at least 1 required):
┌────────────────────────────────────────────────────┐
│ ! @ # $ % ^ & * | _ - ( ) + = [ ] { } ; : ' " < >  │
│ , . ? / \ ~ ` (backtick)                           │
└────────────────────────────────────────────────────┘

VALID PASSWORD EXAMPLES:
┌────────────────────────────────────────────────────┐
│ • Pass123!    (letter, digit, symbol: !)           │
│ • Test@456    (letter, digit, symbol: @)           │
│ • Admin#99!   (letter, digit, symbol: #, !)        │
│ • Welcome1%   (letter, digit, symbol: %)           │
│ • P@ss1       (letter, digit, symbol: @)           │
│ • Abc_123def| (letter, digit, symbol: _, |)        │
└────────────────────────────────────────────────────┘

INVALID PASSWORD EXAMPLES:
┌────────────────────────────────────────────────────┐
│ Pass123   (no symbol)                              │
│ Password! (no digit)                               │
│ Pass1     (too short, only 5 chars)                │
│ MyPassword (no digit, no symbol)                   │
│ 123456    (no letter, no symbol)                   │
└────────────────────────────────────────────────────┘
```

## Component Interaction Sequence

```
1. USER REGISTRATION
   ┌──────────────┐
   │ User fills   │
   │ form with    │
   │ credentials  │
   └──────┬───────┘
          │
          ▼
   ┌─────────────────┐
   │AccountViewModel │
   │ Data binding &  │
   │ validation attr │
   └──────┬──────────┘
          │
          ▼
   ┌──────────────────┐
   │AccountService    │
   │ ValidateAccount  │
   │ CheckUniqueness  │
   └──────┬───────────┘
          │
          ├─ Valid?
          │
    ┌─────┴─────┐
    │           │
   YES         NO
   │            │
   ▼            ▼
CREATE       RETURN
USER         ERRORS
   │
   ▼
ApplicationUser
created with
• IsAdmin = false
• IsOwner = false
```

## File Dependencies Graph

```
Razor Pages (UI Layer)
    │
    ├─ Uses: AccountViewModel
    │         │
    │         └─ Defines properties and validations
    │
    ├─ Uses: AccountService
    │         │
    │         ├─ Uses: UserManager<ApplicationUser>
    │         ├─ Uses: ILogger<AccountService>
    │         └─ Uses: DbContext
    │
    └─ Depends on: Authentication
               │
               └─ Uses: ApplicationUser
                        │
                        ├─ Implements: IAdmin
                        ├─ Extends: IdentityUser
                        └─ Persisted in: ApplicationDbContext
                                        │
                                        └─ Configures: IsAdmin, IsOwner
                                                       Default: false
```

---

**Visual Architecture Complete ✅ | Ready for Implementation ✅**
