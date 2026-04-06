# School Sub-Class Quick Reference

## 🚀 Quick Access
**URL:** `/admin/sub-class/index`  
**Navigation:** Sidebar → Academic → School Sub-Classes  
**Icon:** `bi-layers-fill`

---

## 📋 Key Identifiers

### Frontend
```html
Table ID:         #subClassTable
Search Input:     #searchInput
Create Modal:     #createModal
Edit Modal:       #editModal
Edit ID Field:    #editId
Edit Name Field:  #editName
```

### JavaScript Module
```javascript
Module Name:      SchoolSubClass
Global Function:  SchoolSubClassResetForm()
Edit Class:       .btn-edit-subclass
Delete Class:     .btn-delete-subclass
```

### Backend
```csharp
ViewModel:        SchoolSubClassViewModel
DTO:             SchoolSubClassDto
Entity:          SchoolSubClass
Endpoint:        /home/GetSchoolSubClassesDataTable
```

---

## 🎯 CRUD Operations

### CREATE
**Button:** "Add Sub-Class"  
**Handler:** `OnPostAsync()`  
**Service:** `CreateSchoolSubClassAsync(Model)`  
**Validation:** Name required, max 50 chars, no duplicates

### READ
**DataTable:** Server-side processing  
**Service:** `GetSchoolSubClassesAsync()`  
**Columns:** #, Name, Created Date, Actions

### UPDATE
**Button:** Edit icon in table row  
**Handler:** `OnPostUpdateAsync()`  
**Service:** `UpdateSchoolSubClassAsync(Model)`  
**Form:** Pre-populated with existing data

### DELETE
**Button:** Delete icon in table row  
**Handler:** `OnPostDeleteAsync(int id)`  
**Service:** `DeleteSchoolSunClassAsync(id, "Deleted by admin")`  
**Confirmation:** SweetAlert2 warning dialog

---

## 📝 Form Fields

### Add/Edit Modal
| Field | Type | Required | Max Length | Validation |
|-------|------|----------|------------|------------|
| Name  | Text | Yes      | 50         | No duplicates |

### Validation Messages
- **Required:** "Please fill in all required fields correctly."
- **Duplicate:** "Sub class already exists"
- **Not Found:** "Sub class not found"
- **Update Duplicate:** "Another sub class with same name exists"
- **Delete Error:** "Unknown sub class, check and try again"

---

## 🎨 Icons Used

| Element | Icon | Bootstrap Icon Class |
|---------|------|----------------------|
| Page Header | Layers | `bi-layers` |
| Navigation Link | Layers Fill | `bi-layers-fill` |
| Add Button | Plus Circle | `bi-plus-circle` |
| Form Label | Tag | `bi-tag` |
| Edit Button | Pencil Square | `bi-pencil-square` |
| Delete Button | Trash | `bi-trash` |
| Success Alert | Check Circle Fill | `bi-check-circle-fill` |
| Error Alert | Exclamation Circle Fill | `bi-exclamation-circle-fill` |
| Cancel Button | X Circle | `bi-x-circle` |
| Save/Update Button | Check Circle | `bi-check-circle` |

---

## 💻 Code Snippets

### Initialize JavaScript Module
```javascript
$(document).ready(function() {
    if ($("#subClassTable").length) {
        SchoolSubClass.init();
    }
});
```

### Reset Form (Global Function)
```javascript
SchoolSubClassResetForm(); // Called from HTML onclick
```

### DataTable AJAX Call
```javascript
ajax: {
    url: "/home/GetSchoolSubClassesDataTable",
    type: "POST",
    dataType: 'json'
}
```

### Delete AJAX Call
```javascript
$.ajax({
    url: '/admin/sub-class/index?handler=Delete',
    type: 'POST',
    data: {
        id: id,
        __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
    }
});
```

### Create Handler (C#)
```csharp
public async Task<IActionResult> OnPostAsync()
{
    if (!ModelState.IsValid)
    {
        TempData["Error"] = "Please fill in all required fields correctly.";
        return Page();
    }

    var result = await _unitOfWork.SystemActivities.CreateSchoolSubClassAsync(Model);

    if (result.Succeeded)
    {
        TempData["Success"] = result.Message;
        return RedirectToPage();
    }

    TempData["Error"] = result.Message;
    return RedirectToPage();
}
```

---

## 🔍 Troubleshooting

### DataTable Not Loading
```javascript
// Check browser console for errors
// Verify endpoint: /home/GetSchoolSubClassesDataTable
// Check network tab for AJAX response
```

### Create/Update Not Saving
```csharp
// Check ModelState.IsValid
// Verify service call: _unitOfWork.SystemActivities.CreateSchoolSubClassAsync
// Check database connection string
// Review error logs
```

### Delete Not Working
```javascript
// Verify CSRF token exists: $('input[name="__RequestVerificationToken"]')
// Check AJAX URL: /admin/sub-class/index?handler=Delete
// Verify handler method: OnPostDeleteAsync(int id)
```

### Search Not Filtering
```javascript
// Check searchInput ID: #searchInput
// Verify DataTable variable: subClassTable
// Ensure search binding: $('#searchInput').on('keyup', ...)
```

---

## 📊 DataTable Configuration

### Columns
```javascript
columns: [
    { data: null, orderable: false, searchable: false },  // Row number
    { data: "name" },                                      // Name
    { data: "createdate" },                               // Created Date
    { data: null, orderable: false, searchable: false }   // Actions
]
```

### Search Configuration
```javascript
serverSide: true,    // Server-side processing
searching: true,     // Enable search
paging: true,        // Enable pagination
info: true,          // Show info
responsive: true     // Responsive design
```

---

## 🎨 CSS Classes

### Custom Classes
```css
.btn-edit-subclass    /* Edit button in table */
.btn-delete-subclass  /* Delete button in table */
```

### Shared Classes
```css
.page-header          /* Page title section */
.page-content         /* Main content area */
.search-filter-area   /* Search section */
.datatable-container  /* DataTable wrapper */
.action-buttons       /* Button container in table */
```

---

## 🔐 Security

### CSRF Protection
```javascript
__RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
```

### Validation
- Client-side: HTML5 + Bootstrap validation
- Server-side: ModelState validation + business rules

---

## 📱 Responsive Behavior

### Desktop
- Full DataTable with all columns
- Modal centered on screen
- Search bar full width

### Tablet
- DataTable adapts column width
- Modal adjusts to screen size
- Search bar full width

### Mobile
- DataTable becomes scrollable
- Modal full width
- Search bar stacks vertically

---

## 🚀 Performance Tips

1. **Server-Side Processing** - Only loads visible data
2. **Event Delegation** - Efficient event handling
3. **Debounced Search** - Reduces server calls
4. **Lazy Loading** - DataTable loads on demand
5. **AJAX Reload** - Only refreshes table data, not entire page

---

## 📚 Related Files

### Frontend
- `Pages/admin/sub-class/index.cshtml` - Main page
- `Pages/admin/sub-class/index.cshtml.cs` - Code-behind
- `Pages/Shared/_Layout.cshtml` - Navigation link

### Backend
- `Controllers/homeController.cs` - DataTable endpoint
- `ServiceRepo/SystemActivitiesServices.cs` - CRUD services
- `IServiceRepo/ISystemActivitiesServices.cs` - Service interface

### Models
- `Models/SchoolSubClass.cs` - Entity
- `ViewModels/SchoolSubClassViewModel.cs` - Form model
- `DTOs/SchoolSubClassDto.cs` - DataTable model

### JavaScript
- `wwwroot/js/appmain.js` - SchoolSubClass module

### Styles
- `wwwroot/css/shared-components.css` - Shared styles
- `wwwroot/css/admin-pages.css` - Admin page styles

---

## 🎓 Usage Examples

### Example 1: Add "Class A"
1. Click "Add Sub-Class"
2. Enter "Class A"
3. Click "Save"
4. ✅ "Sub class created successfully"

### Example 2: Edit "Class A" to "Class A1"
1. Click edit icon on "Class A" row
2. Change to "Class A1"
3. Click "Update"
4. ✅ "Sub class updated successfully"

### Example 3: Delete "Class B"
1. Click delete icon on "Class B" row
2. Confirm deletion in SweetAlert
3. ✅ "Sub class successfully removed"

### Example 4: Search for "Class C"
1. Type "Class C" in search box
2. Table filters automatically
3. Shows only matching records

---

## 📞 Quick Commands

### Open Developer Console
```
F12 (Chrome/Edge)
Cmd+Option+I (Mac)
```

### Check DataTable State
```javascript
console.log(SchoolSubClass);
console.log($('#subClassTable').DataTable());
```

### Force Table Reload
```javascript
$('#subClassTable').DataTable().ajax.reload();
```

### Test Form Reset
```javascript
SchoolSubClassResetForm();
```

### Check Module Initialization
```javascript
if ($("#subClassTable").length) {
    console.log("Sub-Class table exists, module should initialize");
}
```

---

## ✅ Testing Checklist

### Manual Testing
- [ ] Navigate to page
- [ ] Add new sub-class
- [ ] Edit existing sub-class
- [ ] Delete sub-class
- [ ] Search for sub-class
- [ ] Test pagination
- [ ] Test form validation
- [ ] Test duplicate detection
- [ ] Test error handling
- [ ] Check responsive design

### Browser Testing
- [ ] Chrome
- [ ] Edge
- [ ] Firefox
- [ ] Safari (if available)

### Responsive Testing
- [ ] Desktop (1920x1080)
- [ ] Laptop (1366x768)
- [ ] Tablet (768x1024)
- [ ] Mobile (375x667)

---

## 🎯 Common Tasks

### Add New Sub-Class Programmatically
```csharp
var model = new SchoolSubClassViewModel { Name = "Class D" };
var result = await _unitOfWork.SystemActivities.CreateSchoolSubClassAsync(model);
```

### Get All Sub-Classes
```csharp
var result = await _unitOfWork.SystemActivities.GetSchoolSubClassesAsync(
    start: 0, 
    length: 100, 
    searchValue: "", 
    sortColumnIndex: 0, 
    sortDirection: "asc"
);
```

### Check if Sub-Class Exists
```csharp
var exists = await _context.SchoolSubClasses.AnyAsync(x => x.Name == "Class A");
```

---

## 📈 Metrics

### Performance Targets
- Page load: < 2 seconds
- DataTable load: < 1 second
- CRUD operations: < 500ms
- Search response: < 300ms

### Capacity
- Max sub-classes: Unlimited
- Recommended per page: 10-50
- Max name length: 50 characters

---

## 🔄 Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | Jan 2026 | Initial implementation following Academic Session pattern |

---

## 📝 Notes

- **Pattern Source:** Academic Session page (completed January 2026)
- **Theme:** Navy (#0b1437) + Gold (#f5b731)
- **Framework:** ASP.NET Core 8, Razor Pages, Bootstrap 5
- **Libraries:** jQuery, DataTables 2.3.7, SweetAlert2, Bootstrap Icons

---

**Quick Start:** Navigate to `/admin/sub-class/index` → Click "Add Sub-Class" → Enter name → Save  
**Documentation:** See `SCHOOL_SUBCLASS_IMPLEMENTATION_SUMMARY.md` for full details
