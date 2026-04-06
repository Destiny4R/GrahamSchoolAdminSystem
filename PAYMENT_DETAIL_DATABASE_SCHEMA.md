# Fees Payment Detail Page - Database Schema Reference

## Related Database Tables

### FeesPaymentTable (Primary)
```sql
Column Name          │ Type          │ Notes
─────────────────────┼───────────────┼──────────────────────────────
Id                   │ INT           │ Primary Key, Identity
Amount               │ DECIMAL(18,2) │ Payment amount
Fees                 │ DECIMAL(18,2) │ Total fees amount
TermRegId            │ INT           │ Foreign Key → TermRegistration
TermlyFeesId         │ INT           │ Foreign Key → TermlyFeesSetup
FilePath             │ VARCHAR(450)  │ Path to payment evidence file
Message              │ VARCHAR(500)  │ Optional message/notes
Narration            │ VARCHAR(500)  │ Optional narration
Status               │ INT           │ Enum: Pending(1), Approved(2), Rejected(3)
PaymentState         │ INT           │ Enum: PartPayment(1), Completed(2), Cancelled(3)
InvoiceNumber        │ VARCHAR(100)  │ Unique invoice identifier
CreatedDate          │ DATETIME      │ Record creation timestamp
UpdatedDate          │ DATETIME      │ Last update timestamp
StaffUserId          │ VARCHAR(470)  │ *** NEW *** FK → AspNetUsers.Id
```

### Migration
```csharp
// Added in migration: 20260404173852_InitMigrations3
migrationBuilder.AddColumn<string>(
    name: "StaffUserId",
    table: "FeesPayments",
    type: "varchar(470)",
    maxLength: 470,
    nullable: false,
    defaultValue: "")
    .Annotation("MySql:CharSet", "utf8mb4");
```

## Related Tables (Navigation)

### TermRegistration Table
```sql
Column Name          │ Type      │ Purpose
─────────────────────┼───────────┼────────────────────────────
Id                   │ INT       │ Primary Key
StudentId            │ INT       │ FK → StudentTable
SessionId            │ INT       │ FK → SessionYear
Term                 │ INT       │ Enum: 1st, 2nd, 3rd
SchoolClassId        │ INT       │ FK → SchoolClass
SchoolSubClassId     │ INT       │ FK → SchoolSubClass
CreatedDate          │ DATETIME  │ Creation timestamp
```

### StudentTable
```sql
Column Name          │ Type          │ Purpose
─────────────────────┼───────────────┼────────────────────────────
Id                   │ INT           │ Primary Key
Surname              │ VARCHAR(70)   │ Student surname
Firstname            │ VARCHAR(70)   │ Student first name
Othername            │ VARCHAR(70)   │ Other names
Gender               │ INT           │ Enum: 1=Male, 2=Female
PaspportPath         │ VARCHAR(470)  │ Passport image path
ApplicationUserId    │ VARCHAR(470)  │ FK → AspNetUsers.Id
CreatedDate          │ DATETIME      │ Creation timestamp
```

### AspNetUsers (ApplicationUser)
```sql
Column Name          │ Type          │ Purpose
─────────────────────┼───────────────┼────────────────────────────
Id                   │ VARCHAR(470)  │ Primary Key
UserName             │ VARCHAR(256)  │ Login username
Email                │ VARCHAR(256)  │ Email address
PasswordHash         │ VARCHAR(MAX)  │ Hashed password
SecurityStamp        │ VARCHAR(MAX)  │ Security stamp
ConcurrencyStamp     │ VARCHAR(MAX)  │ Concurrency token
PhoneNumber          │ VARCHAR(MAX)  │ Phone (optional)
CreatedDate          │ DATETIME      │ Account creation date
```

### SessionYear Table
```sql
Column Name          │ Type          │ Purpose
─────────────────────┼───────────────┼────────────────────────────
Id                   │ INT           │ Primary Key
Name                 │ VARCHAR(50)   │ Session name (e.g., 2023/2024)
IsActive             │ BIT           │ Active session flag
CreatedDate          │ DATETIME      │ Creation timestamp
```

### SchoolClass Table
```sql
Column Name          │ Type          │ Purpose
─────────────────────┼───────────────┼────────────────────────────
Id                   │ INT           │ Primary Key
Name                 │ VARCHAR(100)  │ Class name (e.g., JSS 1)
CreatedDate          │ DATETIME      │ Creation timestamp
```

### TermlyFeesSetup Table
```sql
Column Name          │ Type          │ Purpose
─────────────────────┼───────────────┼────────────────────────────
Id                   │ INT           │ Primary Key
Amount               │ DECIMAL(18,2) │ Fees amount for this term
Term                 │ INT           │ Enum: 1st, 2nd, 3rd
SessionId            │ INT           │ FK → SessionYear
SchoolClassId        │ INT           │ FK → SchoolClass
CreatedDate          │ DATETIME      │ Creation timestamp
UpdatedDate          │ DATETIME      │ Last update timestamp
```

### PTAFeesPayments Table (Parallel Structure)
```sql
Column Name          │ Type          │ Purpose
─────────────────────┼───────────────┼────────────────────────────
Id                   │ INT           │ Primary Key
Amount               │ DECIMAL(18,2) │ Payment amount
Balance              │ DECIMAL(18,2) │ Balance
Fees                 │ DECIMAL(18,2) │ Total fees
TermRegId            │ INT           │ FK → TermRegistration
PtaFeesSetupId       │ INT           │ FK → PTAFeesSetup
FilePath             │ VARCHAR(450)  │ Evidence file
InvoiceNumber        │ VARCHAR(100)  │ Invoice ID
IsConfirm            │ BIT           │ Confirmation flag
CreatedDate          │ DATETIME      │ Creation timestamp
UpdatedDate          │ DATETIME      │ Last update timestamp
StaffUserId          │ VARCHAR(470)  │ *** NEW *** FK → AspNetUsers.Id
```

## Database Relationships

```
FeesPaymentTable
    ├─ 1:Many → TermRegistration (TermRegId)
    │   ├─ 1:Many → StudentTable (StudentId)
    │   │   └─ 1:1 → AspNetUsers (ApplicationUserId)
    │   │
    │   ├─ 1:Many → SessionYear (SessionId)
    │   │
    │   └─ 1:Many → SchoolClass (SchoolClassId)
    │
    ├─ 1:Many → TermlyFeesSetup (TermlyFeesId)
    │
    └─ 1:Many → AspNetUsers (StaffUserId) *** NEW ***
```

## Entity Relationships (EF Core)

### FeesPaymentTable Configuration
```csharp
[ForeignKey(nameof(TermRegId))]
public TermRegistration TermRegistration { get; set; }

[ForeignKey(nameof(TermlyFeesId))]
public TermlyFeesSetup TermlyFeesSetup { get; set; }

[StringLength(470)]
public string StaffUserId { get; set; }
```

### Include Chain (Query)
```csharp
var payment = await _context.FeesPayments
    .Include(x => x.TermRegistration)
        .ThenInclude(x => x.Student)
            .ThenInclude(x => x.ApplicationUser)
    .Include(x => x.TermRegistration)
        .ThenInclude(x => x.SchoolClass)
    .Include(x => x.TermRegistration)
        .ThenInclude(x => x.SessionYear)
    .Include(x => x.TermlyFeesSetup)
    .FirstOrDefaultAsync(x => x.Id == paymentId);

var staffUser = await _context.Users
    .FirstOrDefaultAsync(u => u.Id == payment.StaffUserId);
```

## Data Flow - Getting Payment Details

```
User Navigates to Detail Page
            ↓
Route captures paymentId
            ↓
OnGetAsync(id) called
            ↓
GetPaymentDetailAsync(paymentId)
            ↓
Database Query:
SELECT fp.*, tr.*, s.*, sy.*, sc.*, ts.*, au.*
FROM FeesPayments fp
LEFT JOIN TermRegistration tr ON fp.TermRegId = tr.Id
LEFT JOIN StudentTable s ON tr.StudentId = s.Id
LEFT JOIN AspNetUsers au_student ON s.ApplicationUserId = au_student.Id
LEFT JOIN SessionYear sy ON tr.SessionId = sy.Id
LEFT JOIN SchoolClass sc ON tr.SchoolClassId = sc.Id
LEFT JOIN TermlyFeesSetup ts ON fp.TermlyFeesId = ts.Id
WHERE fp.Id = @paymentId
            ↓
Separate Query for Staff:
SELECT au_staff.*
FROM AspNetUsers au_staff
WHERE au_staff.Id = @StaffUserId
            ↓
Build PaymentDetailViewModel
            ↓
Return to View
```

## Indexes and Performance

### Recommended Indexes
```sql
-- For fast payment lookup
CREATE INDEX IX_FeesPayments_Id ON FeesPayments(Id);

-- For staff tracking
CREATE INDEX IX_FeesPayments_StaffUserId ON FeesPayments(StaffUserId);

-- For term registration queries
CREATE INDEX IX_FeesPayments_TermRegId ON FeesPayments(TermRegId);

-- For status filtering
CREATE INDEX IX_FeesPayments_Status ON FeesPayments(Status);

-- For state filtering
CREATE INDEX IX_FeesPayments_PaymentState ON FeesPayments(PaymentState);

-- For composite queries
CREATE INDEX IX_TermRegistration_Composite ON TermRegistration(SessionId, Term, SchoolClassId);
```

## Enum Mappings

### PaymentStatus Enum
```csharp
public enum PaymentStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3
}
```

### PaymentState Enum
```csharp
[Display(Name = "Part Payment")]
PartPayment = 1,

Completed = 2,

Cancelled = 3
```

## Data Integrity Constraints

### Foreign Key Constraints
```sql
ALTER TABLE FeesPayments
ADD CONSTRAINT FK_FeesPayments_TermRegistration
FOREIGN KEY (TermRegId) REFERENCES TermRegistration(Id);

ALTER TABLE FeesPayments
ADD CONSTRAINT FK_FeesPayments_TermlyFeesSetup
FOREIGN KEY (TermlyFeesId) REFERENCES TermlyFeesSetup(Id);

ALTER TABLE FeesPayments
ADD CONSTRAINT FK_FeesPayments_AspNetUsers
FOREIGN KEY (StaffUserId) REFERENCES AspNetUsers(Id);
```

### Unique Constraints
```sql
-- Assuming InvoiceNumber should be unique
ALTER TABLE FeesPayments
ADD CONSTRAINT UX_FeesPayments_InvoiceNumber
UNIQUE (InvoiceNumber);
```

## Sample Query for Payment Detail Page

### Complete Query
```sql
SELECT 
    fp.Id,
    fp.InvoiceNumber,
    fp.Amount,
    fp.Fees as TotalFees,
    fp.Status,
    fp.PaymentState,
    fp.FilePath,
    fp.Message,
    fp.Narration,
    fp.CreatedDate,
    fp.UpdatedDate,
    fp.StaffUserId,
    -- Student Info
    s.Surname,
    s.Firstname,
    s.Othername,
    au_student.UserName as StudentRegNumber,
    -- Academic Info
    sy.Name as AcademicSession,
    tr.Term,
    sc.Name as SchoolClass,
    -- Staff Info
    au_staff.UserName as StaffName,
    au_staff.Email as StaffEmail
FROM FeesPayments fp
INNER JOIN TermRegistration tr ON fp.TermRegId = tr.Id
INNER JOIN StudentTable s ON tr.StudentId = s.Id
INNER JOIN AspNetUsers au_student ON s.ApplicationUserId = au_student.Id
INNER JOIN SessionYear sy ON tr.SessionId = sy.Id
INNER JOIN SchoolClass sc ON tr.SchoolClassId = sc.Id
INNER JOIN TermlyFeesSetup ts ON fp.TermlyFeesId = ts.Id
LEFT JOIN AspNetUsers au_staff ON fp.StaffUserId = au_staff.Id
WHERE fp.Id = @PaymentId;
```

## Data Size Estimates

```
Table                    │ Typical Records │ Growth
─────────────────────────┼─────────────────┼──────────────
AspNetUsers              │ 100-500         │ Slow (annual)
SessionYear              │ 5-10            │ Slow (1 per year)
SchoolClass              │ 10-20           │ Slow (stable)
StudentTable             │ 500-2000        │ Medium (annual)
TermRegistration         │ 1000-5000       │ Medium (annual)
TermlyFeesSetup          │ 30-60           │ Slow (per term)
FeesPayments             │ 5000-50000      │ High (annual)
PTAFeesPayments          │ 2000-20000      │ High (annual)
```

## Backup Strategy

### Critical Tables for Backup
1. FeesPayments (financial records)
2. TermRegistration (student history)
3. AspNetUsers (staff accountability)

### Recommended Backup Frequency
- Daily for FeesPayments table
- Weekly for others
- Monthly full database backup

---

This schema reference provides developers with complete database understanding for maintenance and optimization.
