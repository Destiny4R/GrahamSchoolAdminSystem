# Fees Setup Implementation Guide

## Overview
This document provides comprehensive guidance on the Fees Setup module, which manages termly fees for school classes.

## Architecture Components

### 1. **Model Layer** (`TermlyFeesSetup`)
```csharp
public class TermlyFeesSetup
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public Term Term { get; set; }              // First, Second, Third
    public int SchoolClassId { get; set; }
    public int SessionId { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
    
    // Foreign Keys
    public SessionYear SessionYear { get; set; }
    public SchoolClasses SchoolClass { get; set; }
}
```

### 2. **ViewModel Layer** (`FeesSetupViewModel`)
```csharp
public class FeesSetupViewModel
{
    public int? Id { get; set; }
    [Required]
    public decimal Amount { get; set; }
    [Required]
    public Term Term { get; set; }
    [Required, Display(Name = "Class")]
    public int SchoolClassId { get; set; }
    [Required, Display(Name = "Academic Session")]
    public int SessionId { get; set; }
}
```

### 3. **Service Layer** (`IFinanceServices` / `FinanceServices`)

#### Key Methods:

**GetFeesSetupAsync()**
- Retrieves all fees setups with pagination and filtering
- Returns: (List<dynamic> data, int recordsTotal, int recordsFiltered)
- Used by: DataTable for server-side processing

```csharp
var (data, total, filtered) = await _unitOfWork.FinanceServices
    .GetFeesSetupAsync(skip: 0, pageSize: 10, searchTerm: "");
```

**CreateFeesSetupAsync()**
- Creates new fees setup record
- Validates: No duplicate (class + session + term combination)
- Returns: ServiceResponse<int> with created ID

```csharp
var result = await _unitOfWork.FinanceServices
    .CreateFeesSetupAsync(model);
if (result.Succeeded) {
    var newId = result.Data;
}
```

**UpdateFeesSetupAsync()**
- Updates existing fees setup
- Validates: Duplicate check (excluding current record)
- Returns: ServiceResponse<bool>

**DeleteFeesSetupAsync()**
- Deletes fees setup record
- Returns: ServiceResponse<bool>

**GetFeesSetupSelectionsAsync()**
- Returns dropdown options for form
- Returns: ViewSelections with SchoolClasses, AcademicSession, Terms

### 4. **Page Handler** (`Index.cshtml.cs`)

#### Endpoints:

| Method | Handler | Purpose |
|--------|---------|---------|
| GET | OnGetAsync() | Load page with dropdown data |
| GET | OnGetDataTableAsync() | DataTable server-side processing |
| POST | OnPostAddAsync() | Create new fees setup |
| GET | OnGetEditAsync() | Load fees setup for editing |
| POST | OnPostUpdateAsync() | Update fees setup |
| POST | OnPostDeleteAsync() | Delete fees setup |

#### Example Usage:
```csharp
public async Task<IActionResult> OnGetAsync()
{
    Selections = await _unitOfWork.FinanceServices
        .GetFeesSetupSelectionsAsync();
    return Page();
}
```

### 5. **UI Layer** (`Index.cshtml`)

#### Features:
- **Page Header**: Title, description, add button
- **Search Filter**: Real-time search across class, session, term, amount
- **DataTable**: Server-side processing with pagination
- **Add/Edit Modal**: Form with validation
- **Delete Confirmation**: Modal before deletion
- **Action Buttons**: Edit and Delete for each row

#### Styling:
- Uses consolidated CSS system (shared-components.css, admin-pages.css)
- Bootstrap Icons for consistency
- Responsive design for mobile/tablet

## Data Flow

### Creating Fees Setup:
```
User clicks "Add Fees Setup"
    ↓
Modal opens with empty form
    ↓
User fills: Class, Session, Term, Amount
    ↓
User clicks "Save"
    ↓
JavaScript sends AJAX POST to OnPostAddAsync()
    ↓
Service validates and creates record
    ↓
Returns JSON response (success/error)
    ↓
DataTable reloads with new record
    ↓
Alert shows success/error message
```

### Updating Fees Setup:
```
User clicks Edit button
    ↓
JavaScript sends GET to OnGetEditAsync()
    ↓
Returns fees setup data in JSON
    ↓
Modal opens with pre-filled form
    ↓
User modifies fields
    ↓
User clicks "Update"
    ↓
JavaScript sends AJAX POST to OnPostUpdateAsync()
    ↓
Service validates and updates record
    ↓
DataTable reloads with updated record
```

### Deleting Fees Setup:
```
User clicks Delete button
    ↓
Confirmation modal appears
    ↓
User confirms
    ↓
JavaScript sends POST to OnPostDeleteAsync()
    ↓
Service deletes record
    ↓
DataTable reloads
    ↓
Alert confirms deletion
```

## Database Schema

### TermlyFeesSetups Table:
```sql
CREATE TABLE TermlyFeesSetups (
    Id INT PRIMARY KEY IDENTITY,
    Amount DECIMAL(18,2) NOT NULL,
    Term INT NOT NULL,                    -- 1=First, 2=Second, 3=Third
    SchoolClassId INT NOT NULL,
    SessionId INT NOT NULL,
    CreatedDate DATETIME DEFAULT GETUTCDATE(),
    UpdatedDate DATETIME DEFAULT GETUTCDATE(),
    FOREIGN KEY (SchoolClassId) REFERENCES SchoolClasses(Id),
    FOREIGN KEY (SessionId) REFERENCES SessionYears(Id)
)
```

### Relationships:
- **TermlyFeesSetup → SchoolClasses**: Many-to-One (Multiple fee setups per class)
- **TermlyFeesSetup → SessionYears**: Many-to-One (Multiple fee setups per session)
- **TermlyFeesSetup → Term Enum**: One-to-One (First, Second, or Third)

## API Endpoints

### Get Fees Setup List (DataTable):
```
GET /admin/feesmanager/fees-setup?handler=DataTable
Query Parameters:
  - draw: DataTable draw parameter
  - start: Pagination start
  - length: Page size
  - searchValue: Search term

Response:
{
  "draw": 1,
  "recordsTotal": 45,
  "recordsFiltered": 10,
  "data": [
    {
      "id": 1,
      "className": "JSS 1",
      "sessionName": "2023/2024",
      "term": "First",
      "amount": 25000
    }
  ]
}
```

### Create Fees Setup:
```
POST /admin/feesmanager/fees-setup?handler=Add
Content-Type: multipart/form-data

Form Data:
  - id: (empty)
  - schoolClassId: 1
  - sessionId: 1
  - term: 1
  - amount: 25000

Response:
{
  "success": true,
  "message": "Fees setup created successfully"
}
```

### Update Fees Setup:
```
POST /admin/feesmanager/fees-setup?handler=Update
Content-Type: multipart/form-data

Form Data:
  - id: 1
  - schoolClassId: 1
  - sessionId: 1
  - term: 1
  - amount: 30000

Response:
{
  "success": true,
  "message": "Fees setup updated successfully"
}
```

### Delete Fees Setup:
```
POST /admin/feesmanager/fees-setup?handler=Delete&id=1

Response:
{
  "success": true,
  "message": "Fees setup deleted successfully"
}
```

## Validation Rules

### Amount:
- Required
- Must be > 0
- Decimal format (up to 2 decimal places)

### Class:
- Required
- Must exist in SchoolClasses table
- Cannot be changed after creation (best practice)

### Session:
- Required
- Must exist in SessionYears table
- Cannot be changed after creation (best practice)

### Term:
- Required
- Must be: First (1), Second (2), or Third (3)
- Unique combination: Class + Session + Term must be unique

### Business Logic:
- **Duplicate Prevention**: Cannot create two fees setups for same class + session + term
- **Validation on Update**: Similar duplicate check excluding current record
- **Soft Delete**: Records are permanently deleted (no soft delete)

## Usage Examples

### Adding Fees Setup via Form:
```html
<form id="feesSetupForm">
    <select name="schoolClassId" required>
        <option>-- Select Class --</option>
        @foreach(var cls in Model.Selections.SchoolClasses)
        {
            <option value="@cls.Value">@cls.Text</option>
        }
    </select>
    
    <select name="sessionId" required>
        <option>-- Select Session --</option>
        @foreach(var session in Model.Selections.AcademicSession)
        {
            <option value="@session.Value">@session.Text</option>
        }
    </select>
    
    <select name="term" required>
        <option>-- Select Term --</option>
        @foreach(var term in Model.Selections.Terms)
        {
            <option value="@term.Value">@term.Text</option>
        }
    </select>
    
    <input type="number" name="amount" step="0.01" required />
</form>
```

### Programmatic Creation:
```csharp
var model = new FeesSetupViewModel
{
    Amount = 25000,
    Term = Term.First,
    SchoolClassId = 1,
    SessionId = 1
};

var result = await _unitOfWork.FinanceServices.CreateFeesSetupAsync(model);

if (result.Succeeded)
{
    var feesSetupId = result.Data;
    // Handle success
}
else
{
    var errorMessage = result.Message;
    // Handle error
}
```

## Frontend JavaScript Functions

### DataTable Initialization:
```javascript
table = $('#feesSetupTable').DataTable({
    processing: true,
    serverSide: true,
    ajax: {
        url: '?handler=DataTable',
        data: function (d) {
            d.searchValue = $('#searchInput').val();
        }
    }
});
```

### Edit Fees Setup:
```javascript
function editFeesSetup(id) {
    $.get('?handler=Edit&id=' + id, function (data) {
        $('#feesSetupId').val(data.id);
        $('#schoolClassId').val(data.schoolClassId);
        $('#sessionId').val(data.sessionId);
        $('#term').val(data.term);
        $('#amount').val(data.amount);
        // Show modal
    });
}
```

### Delete with Confirmation:
```javascript
function deleteFeesSetup(id) {
    deleteId = id;
    // Show confirmation modal
}

function confirmDelete() {
    $.post('?handler=Delete&id=' + deleteId, function (response) {
        if (response.success) {
            table.ajax.reload();
        }
    });
}
```

## Error Handling

### Common Errors:

**Duplicate Fee Setup:**
```
Message: "Fees setup already exists for this class, session, and term"
Status: 400
Cause: User attempted to create duplicate fee setup
Solution: Check existing records before creating
```

**Class/Session Not Found:**
```
Message: "Invalid school class or session"
Status: 400
Cause: Selected class or session doesn't exist
Solution: Refresh dropdown selections
```

**Database Error:**
```
Message: "Error creating fees setup: [Exception Details]"
Status: 500
Cause: Database connectivity or constraint violation
Solution: Check logs and database connection
```

## Best Practices

1. **Always Validate Inputs**: Server-side validation is performed
2. **Check Duplicates**: Use service method to check before creation
3. **Use Transactions**: For batch operations on multiple records
4. **Log All Operations**: Audit logging is enabled
5. **Handle Errors Gracefully**: Show user-friendly error messages
6. **Refresh After Operations**: Always reload DataTable after CRUD operations
7. **Confirm Deletions**: Always show confirmation modal before deletion

## Performance Considerations

1. **Pagination**: DataTable uses server-side pagination (default: 10 records)
2. **Search**: Uses database-level filtering for performance
3. **Caching**: Consider caching dropdown selections if static
4. **Indexes**: Ensure indexes on SchoolClassId, SessionId for fast queries

### Query Performance:
```csharp
// Good - Uses indexes
var setup = await _context.TermlyFeesSetups
    .Where(x => x.SchoolClassId == classId && x.SessionId == sessionId)
    .ToListAsync();

// Also include related data efficiently
var setup = await _context.TermlyFeesSetups
    .Include(x => x.SchoolClass)
    .Include(x => x.SessionYear)
    .Where(x => x.SchoolClassId == classId)
    .ToListAsync();
```

## Future Enhancements

1. **Bulk Import**: Import fees from CSV/Excel
2. **Fee Schedules**: Planned payment schedules
3. **Discounts**: Apply discounts per student or class
4. **Payment Tracking**: Track student payments against setup
5. **Reports**: Generate fees collection reports
6. **Notifications**: Send fee notifications to students/parents
7. **Payment Plans**: Support installment payment options

## Related Modules

- **Students**: Student fees tracking against this setup
- **Payments**: FeesPaymentTable tracks payments for these setups
- **PTA Fees**: PTAFeesSetup for PTA/Levies
- **Finance Reports**: Generate reports based on fees setup

## Support & Debugging

### Enable Detailed Logging:
```csharp
logger.LogInformation("Loading fees setup: {ClassId}, {SessionId}", classId, sessionId);
logger.LogError(ex, "Error in fees setup: {Error}", ex.Message);
```

### Common Issues:

**DataTable Not Loading:**
- Check browser console for AJAX errors
- Verify handler name matches (DataTable, Add, Update, Delete)
- Check URL format

**Form Validation Failed:**
- Check ModelState.IsValid
- Verify all required fields are populated
- Check data types (e.g., decimal for amount)

**Database Constraint Violation:**
- Check foreign key relationships
- Verify SchoolClassId and SessionId exist
- Check for duplicate term setups

---

**Last Updated:** 2025
**Version:** 1.0
**Status:** Production Ready
