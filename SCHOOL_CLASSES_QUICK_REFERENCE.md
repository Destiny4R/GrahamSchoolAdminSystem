# School Classes - Quick Reference

## File Locations

```
Models:
  └─ GrahamSchoolAdminSystemModels\Models\SchoolClasses.cs

ViewModels:
  └─ GrahamSchoolAdminSystemModels\ViewModels\SchoolClassViewModel.cs

Services:
  ├─ GrahamSchoolAdminSystemAccess\IServiceRepo\ISchoolClassServices.cs (NEW)
  └─ GrahamSchoolAdminSystemAccess\ServiceRepo\SchoolClassServices.cs (NEW)

Pages:
  ├─ GrahamSchoolAdminSystemWeb\Pages\admin\schoolclass\index.cshtml (UPDATED)
  └─ GrahamSchoolAdminSystemWeb\Pages\admin\schoolclass\index.cshtml.cs (UPDATED)

Configuration:
  ├─ Program.cs (UPDATED)
  └─ UnitOfWork.cs (UPDATED)
```

## Key Classes

### SchoolClasses Model
```csharp
public int Id { get; set; }
public string Name { get; set; }              // Max 50 chars
public DateTime CreatedDate { get; set; }
public DateTime UpdatedDate { get; set; }
```

### SchoolClassViewModel
```csharp
public int? Id { get; set; }
[Required, StringLength(50)]
public string Name { get; set; }
```

## Service Methods

### ISchoolClassServices

```csharp
// Get paginated list with search
GetSchoolClassesAsync(int skip, int pageSize, string searchTerm, int sortColumn, string sortDirection)
  → (List<dynamic>, recordsTotal, recordsFiltered)

// Get single class
GetSchoolClassByIdAsync(int id)
  → SchoolClassViewModel

// Get all classes (no pagination)
GetAllSchoolClassesAsync()
  → List<SchoolClassViewModel>

// Create new
CreateSchoolClassAsync(SchoolClassViewModel model)
  → ServiceResponse<int>

// Update existing
UpdateSchoolClassAsync(SchoolClassViewModel model)
  → ServiceResponse<bool>

// Delete class
DeleteSchoolClassAsync(int id)
  → ServiceResponse<bool>

// Check if name exists
ClassNameExistsAsync(string name, int? excludeId = null)
  → bool
```

## API Endpoints

```
GET  /admin/schoolclass                        → Load page
GET  /admin/schoolclass?handler=DataTable      → DataTable data
POST /admin/schoolclass?handler=Add            → Create class
GET  /admin/schoolclass?handler=Edit&id=X      → Load for edit
POST /admin/schoolclass?handler=Update         → Update class
POST /admin/schoolclass?handler=Delete&id=X    → Delete class
```

## DataTable Configuration

```javascript
{
    processing: true,              // Loading indicator
    serverSide: true,              // Server-side processing
    ajax: { url: '?handler=DataTable' },
    columns: [
        { data: 'id' },            // Row number
        { data: 'name' },          // Class name
        { data: 'createdDateFormatted' },
        { data: 'updatedDateFormatted' },
        { data: 'id', orderable: false }  // Actions
    ],
    pageLength: 10,
    order: [[2, 'desc']]           // Sort by created date
}
```

## Form Fields

| Field | Type | Required | Max Length |
|-------|------|----------|-----------|
| Class Name | text | Yes | 50 |

## HTML Elements

### Page Structure
```html
<div class="page-header">...</div>
<div class="page-content">
    <div class="search-filter-area">...</div>
    <div class="datatable-container">
        <table id="schoolClassesTable">...</table>
    </div>
</div>
<div class="modal" id="schoolClassModal">...</div>
<div class="modal" id="deleteConfirmModal">...</div>
```

## JavaScript Functions

```javascript
// Initialize
table = $('#schoolClassesTable').DataTable({...})

// Form
resetForm()
submitForm()

// Edit/Delete
editSchoolClass(id)
deleteSchoolClass(id)
confirmDelete()

// Utilities
showAlert(type, message)
table.ajax.reload()
```

## CSS Classes

| Class | Purpose |
|-------|---------|
| `.page-header` | Page title section |
| `.header-actions` | Button group |
| `.search-filter-area` | Search controls |
| `.datatable-container` | Table wrapper |
| `.action-buttons` | Action button group |
| `.btn-action` | Individual action button |
| `.btn-edit` | Edit button (green) |
| `.btn-delete` | Delete button (red) |

## Icons

| Icon | Usage |
|------|-------|
| `bi-building` | Header icon |
| `bi-plus-circle` | Add button |
| `bi-pencil-square` | Edit |
| `bi-trash` | Delete |
| `bi-check-circle-fill` | Success |
| `bi-exclamation-circle-fill` | Error |
| `bi-exclamation-triangle` | Warning |

## Validation Rules

| Rule | Details |
|------|---------|
| Required | Class name must be provided |
| Max 50 chars | String length limit |
| Unique | Case-insensitive name uniqueness |
| No Related Data | Cannot delete if has registrations or fees |

## Common Workflows

### Add Class
1. Click "Add School Class"
2. Enter name (e.g., "JSS 1")
3. Click "Save"
4. Success alert → DataTable reloads

### Edit Class
1. Click Edit button on row
2. Form pre-fills with current data
3. Modify name
4. Click "Update"
5. Success alert → DataTable reloads

### Delete Class
1. Click Delete button
2. Confirm in modal
3. If no related records → Deleted
4. Otherwise → Error message shown

### Search
1. Type in search box
2. Table filters in real-time
3. Shows matching classes

## Error Messages

| Error | Cause | Solution |
|-------|-------|----------|
| "A class with this name already exists" | Duplicate name | Use different name |
| "Cannot delete class with related records" | Has students/fees | Delete related data first |
| "School class not found" | ID doesn't exist | Refresh page |
| "Error creating school class" | Server error | Check logs |

## Response Formats

### Success Response
```json
{
  "success": true,
  "message": "School class created successfully"
}
```

### Error Response
```json
{
  "success": false,
  "message": "A class with this name already exists"
}
```

### DataTable Response
```json
{
  "draw": 1,
  "recordsTotal": 25,
  "recordsFiltered": 10,
  "data": [
    {
      "id": 1,
      "name": "JSS 1",
      "createdDateFormatted": "15/01/2024 02:30 PM",
      "updatedDateFormatted": "15/01/2024 02:30 PM"
    }
  ]
}
```

## Dependency Injection

### Registration (Program.cs)
```csharp
builder.Services.AddScoped<ISchoolClassServices, SchoolClassServices>();
```

### UnitOfWork Property
```csharp
public ISchoolClassServices SchoolClassServices { get; set; }
```

## Database Relations

```
SchoolClasses (1) ──┬── (N) TermRegistration
                   └── (N) TermlyFeesSetup
```

## Testing Checklist

- [ ] Create class with valid name
- [ ] Prevent duplicate names
- [ ] Edit class name
- [ ] Delete empty class
- [ ] Prevent delete with students
- [ ] Prevent delete with fees
- [ ] Search by name
- [ ] Pagination (10, 25, 50)
- [ ] Sort by date
- [ ] Alerts display correctly
- [ ] Modal opens/closes
- [ ] Form validation
- [ ] Responsive design
- [ ] Audit logging

## Performance Tips

1. **Search:** Uses database-level filtering
2. **Pagination:** Server-side processing
3. **Indexes:** SchoolClasses table indexed on Name
4. **Caching:** Consider caching for dropdown usage

## Browser Support

- Chrome/Edge: ✅ Full support
- Firefox: ✅ Full support
- Safari: ✅ Full support
- IE: ❌ Not supported

## Mobile Responsive

- Desktop (1200px+): Full layout
- Tablet (768px-1199px): Optimized
- Mobile (<768px): Stacked

---

**Quick Start:** Navigate to `/admin/schoolclass` to see it in action!

**Last Updated:** 2025  
**Version:** 1.0
