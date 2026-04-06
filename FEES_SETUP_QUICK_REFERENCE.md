# Fees Setup - Quick Reference

## File Locations

```
📁 Models:
  └─ GrahamSchoolAdminSystemModels\Models\TermlyFeesSetup.cs

📁 ViewModels:
  ├─ FeesSetupViewModel.cs
  └─ ServiceResponse.cs (NEW)

📁 Services:
  ├─ IServiceRepo\IFinanceServices.cs (UPDATED)
  └─ ServiceRepo\FinanceServices.cs (UPDATED)

📁 UI:
  ├─ Pages\admin\feesmanager\fees-setup\Index.cshtml (UPDATED)
  └─ Pages\admin\feesmanager\fees-setup\Index.cshtml.cs (UPDATED)

📁 Styling:
  ├─ wwwroot\css\shared-components.css (reused)
  └─ wwwroot\css\admin-pages.css (reused)
```

## Key Classes & Interfaces

### ServiceResponse<T>
Generic response wrapper for all service methods
```csharp
// Success
return ServiceResponse<T>.Success(data, "Message");

// Failure
return ServiceResponse<T>.Failure("Error message");
```

### TermlyFeesSetup Model
```csharp
public int Id { get; set; }
public decimal Amount { get; set; }
public Term Term { get; set; }         // 1=First, 2=Second, 3=Third
public int SchoolClassId { get; set; }
public int SessionId { get; set; }
public DateTime CreatedDate { get; set; }
public DateTime UpdatedDate { get; set; }
```

## Database

### Table: TermlyFeesSetups
```sql
Columns:
  - Id (PK)
  - Amount (decimal)
  - Term (int: 1, 2, 3)
  - SchoolClassId (FK)
  - SessionId (FK)
  - CreatedDate
  - UpdatedDate

Constraints:
  - FK: SchoolClasses(Id)
  - FK: SessionYears(Id)
  - Unique: SchoolClassId + SessionId + Term (enforced in code)
```

## API Endpoints

```
GET  /admin/feesmanager/fees-setup             → Load page
GET  /admin/feesmanager/fees-setup?handler=DataTable → DataTable
POST /admin/feesmanager/fees-setup?handler=Add       → Create
GET  /admin/feesmanager/fees-setup?handler=Edit&id=X → Load for edit
POST /admin/feesmanager/fees-setup?handler=Update    → Update
POST /admin/feesmanager/fees-setup?handler=Delete&id=X → Delete
```

## Service Methods

### IFinanceServices Interface

```csharp
// Get paginated list
Task<(List<dynamic> data, int recordsTotal, int recordsFiltered)> GetFeesSetupAsync(
    int skip = 0, 
    int pageSize = 10, 
    string searchTerm = "", 
    int sortColumn = 0, 
    string sortDirection = "asc");

// Get single record
Task<FeesSetupViewModel> GetFeesSetupByIdAsync(int id);

// Create new
Task<ServiceResponse<int>> CreateFeesSetupAsync(FeesSetupViewModel model);

// Update existing
Task<ServiceResponse<bool>> UpdateFeesSetupAsync(FeesSetupViewModel model);

// Delete record
Task<ServiceResponse<bool>> DeleteFeesSetupAsync(int id);

// Get dropdowns
Task<ViewSelections> GetFeesSetupSelectionsAsync();
```

## Frontend JavaScript

### Key Functions

```javascript
// Initialize DataTable
table = $('#feesSetupTable').DataTable({...})

// Reload table
table.ajax.reload()

// Edit record
editFeesSetup(id)

// Delete record
deleteFeesSetup(id)
confirmDelete()

// Submit form
submitForm()

// Reset form
resetForm()

// Show alert
showAlert(type, message)
```

### Form Handling

```javascript
// Auto-submit form on change
$('#feesSetupForm').on('submit', function (e) {
    e.preventDefault();
    submitForm();
});

// AJAX POST
$.ajax({
    type: 'POST',
    url: '?handler=' + handler,
    data: formData,
    processData: false,
    contentType: false,
    success: function (response) {...}
});
```

## HTML Elements

### Page Structure
```html
<div class="page-header">...</div>
<div class="page-content">
    <div class="search-filter-area">...</div>
    <div class="datatable-container">
        <table id="feesSetupTable">...</table>
    </div>
</div>
<div class="modal" id="feesSetupModal">...</div>
<div class="modal" id="deleteConfirmModal">...</div>
```

### Form Fields
- schoolClassId (select dropdown)
- sessionId (select dropdown)
- term (select dropdown)
- amount (number input)

## DataTable Configuration

```javascript
{
    processing: true,          // Show loading indicator
    serverSide: true,          // Server-side processing
    ajax: {...},               // AJAX data source
    columns: [...],            // Column definitions
    pageLength: 10,            // Records per page
    lengthMenu: [[10, 25, 50], [10, 25, 50]],
    order: [[0, 'desc']],      // Default sort
    language: {...}            // Localization
}
```

## Validation Rules

| Field | Rule |
|-------|------|
| Amount | Required, > 0, decimal |
| Class | Required, must exist |
| Session | Required, must exist |
| Term | Required, must be 1/2/3 |
| Unique | Class + Session + Term must be unique |

## Column Definitions

| Column | Width | Sortable | Searchable | Render |
|--------|-------|----------|-----------|--------|
| ID | col-1 | Yes | Yes | Row number |
| Class | col-3 | Yes | Yes | Class name |
| Session | col-3 | Yes | Yes | Session name |
| Term | col-2 | Yes | Yes | Term name |
| Amount | col-2 | Yes | Yes | Formatted as ₦X.XX |
| Actions | col-1 | No | No | Edit/Delete buttons |

## Styling & Icons

### CSS Classes
- `.page-header`: Page title section
- `.search-filter-area`: Search/filter controls
- `.datatable-container`: DataTable wrapper
- `.action-buttons`: Action button group
- `.btn-action`: Individual action button
- `.alert-success`, `.alert-danger`: Alert boxes

### Icons (Bootstrap Icons)
- `bi-cash-coin`: Fees header icon
- `bi-plus-circle`: Add button
- `bi-check-circle-fill`: Success alert
- `bi-exclamation-circle-fill`: Error alert
- `bi-pencil-square`: Edit icon
- `bi-trash`: Delete icon
- `bi-building`: Class icon
- `bi-calendar-event`: Session icon
- `bi-hourglass-split`: Term icon
- `bi-currency-dollar`: Amount icon

## Common Workflows

### Add New Fees Setup
1. User clicks "Add Fees Setup" button
2. Modal opens with empty form
3. User selects Class, Session, Term, enters Amount
4. Click "Save" → AJAX POST to OnPostAddAsync()
5. Service validates and creates record
6. DataTable reloads
7. Success alert appears

### Edit Existing Fees Setup
1. User clicks Edit button on row
2. AJAX GET to OnGetEditAsync() returns record data
3. Modal opens with pre-filled form
4. User modifies fields
5. Click "Update" → AJAX POST to OnPostUpdateAsync()
6. Service validates and updates record
7. DataTable reloads
8. Success alert appears

### Delete Fees Setup
1. User clicks Delete button on row
2. Confirmation modal appears
3. User clicks "Delete" button
4. AJAX POST to OnPostDeleteAsync()
5. Service deletes record
6. DataTable reloads
7. Success alert confirms deletion

## Error Handling

### HTTP Status Codes
- 200: Success
- 400: Validation error (duplicate, missing field)
- 404: Record not found
- 500: Server error

### Error Messages
- "Fees setup already exists..."
- "Fees setup not found"
- "Invalid form data"
- "Error creating fees setup"

## Performance Tips

1. **Pagination**: Uses server-side (10, 25, or 50 per page)
2. **Search**: Database-level filtering
3. **Includes**: Uses .Include() for foreign keys
4. **Indexes**: Should exist on SchoolClassId, SessionId

## Debugging

### Check Browser Console
```javascript
// DataTable errors
console.log(table);
console.log($('#feesSetupTable').DataTable());

// AJAX errors
$.ajax({...}).fail(function(jqXHR) {
    console.error('AJAX Error:', jqXHR);
});
```

### Server Logs
```csharp
_logger.LogError(ex, "Error in fees setup: {Error}", ex.Message);
await _unitOfWork.LogService.LogErrorAsync(...);
```

## Audit Logging

All operations are logged:
```csharp
await _unitOfWork.LogService.LogUserActionAsync(
    action: "Create|Update|Delete",
    entityType: "FeesSetup",
    message: "...",
    details: "..."
);
```

## Related Features

- **Students**: Can be linked to fees payments
- **Payments**: FeesPaymentTable references this
- **Reports**: Generate fees reports based on setup
- **Notifications**: Send fee notices to students

## Testing Checklist

- [ ] Create fees setup
- [ ] Edit fees setup  
- [ ] Delete fees setup
- [ ] Search functionality
- [ ] Pagination
- [ ] Validation (empty fields)
- [ ] Duplicate prevention
- [ ] Responsive design (mobile)
- [ ] Error messages
- [ ] Success messages
- [ ] DataTable sorting
- [ ] Modal opening/closing
- [ ] Form reset after add/edit

---

**Quick Start:** Open `/admin/feesmanager/fees-setup` to see the implementation in action!
