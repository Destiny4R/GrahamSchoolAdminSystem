# Academic Session Page - Quick Reference Card

## 🎯 What Changed?

### Before (Old Design)
```razor
❌ <div class="container-fluid">
❌ <div class="h3">Academic Session</div>
❌ <button data-bs-target="#addFeeModal">  <!-- Wrong ID! -->
❌ <table id="schoolclasstable">            <!-- Wrong ID! -->
❌ url: "/home/getschoolclasses"            <!-- Wrong endpoint! -->
❌ No CSS styling
❌ No TempData alerts
❌ No search functionality
❌ Incomplete CRUD operations
```

### After (New Design) ✅
```razor
✅ <link href="~/css/shared-components.css" />
✅ <link href="~/css/admin-pages.css" />
✅ <div class="page-header"> with icons
✅ TempData Success/Error alerts
✅ <div class="search-filter-area">
✅ <table id="academicSessionTable">
✅ url: "/home/GetAcademicSessionsDataTable"
✅ Complete CRUD with handlers
✅ SweetAlert2 delete confirmations
✅ Navy/gold color scheme
```

---

## 📂 File Changes Summary

### 1. **homeController.cs** ⭐ NEW ENDPOINT
```csharp
[HttpPost]
public async Task<IActionResult> GetAcademicSessionsDataTable()
{
    return await ExecuteDataTableAsync<SessionYearDto>(
        _unitOfWork.SystemActivities.GetAcademicSessionAsync,
        "Error retrieving academic sessions");
}
```

### 2. **index.cshtml.cs** ⭐ COMPLETE CRUD
```csharp
// Property renamed: model → Model (Pascal case)
[BindProperty]
public SessionYearViewModel Model { get; set; }

// Handlers:
OnPostAsync()         // CREATE
OnPostUpdateAsync()   // UPDATE
OnPostDeleteAsync()   // DELETE
```

### 3. **index.cshtml** ⭐ FULL REDESIGN
```razor
<!-- Page structure matching Fees Setup -->
<div class="page-header">...</div>
<div class="page-content">
    <!-- TempData alerts -->
    <!-- Search input -->
    <!-- DataTable -->
</div>
<!-- Create Modal -->
<!-- Edit Modal -->
<!-- Scripts -->
```

---

## 🚀 Quick Usage Guide

### Add Academic Session
1. Click **"Add Academic Session"** button
2. Enter session name (e.g., "2024/2025")
3. Click **Save**
4. Success alert appears

### Edit Academic Session
1. Click **Edit** button (pencil icon) in table row
2. Modal opens with current data
3. Update the session name
4. Click **Update**
5. Success alert appears

### Delete Academic Session
1. Click **Delete** button (trash icon) in table row
2. SweetAlert confirmation dialog appears
3. Click **"Yes, Delete"**
4. AJAX request sent
5. Page reloads with success message

### Search Sessions
- Type in **"Search Academic Sessions"** input box
- DataTable automatically filters results
- Real-time search as you type

---

## 🎨 Design Features

### Color Scheme (Navy/Gold)
```css
--primary-navy: #0b1437        /* Dark navy */
--primary-navy-light: #1a2b6d  /* Light navy */
--accent-gold: #f5b731         /* Gold accent */
```

### Modal Header Style
```html
<div class="modal-header">
    <!-- Navy gradient background -->
    <!-- White text and close button -->
    <!-- Bootstrap Icon + title -->
    <!-- Subtitle text -->
</div>
```

### Action Buttons
```html
<button class="btn btn-sm btn-edit">
    <i class="bi bi-pencil-square"></i>
</button>
<button class="btn btn-sm btn-delete">
    <i class="bi bi-trash"></i>
</button>
```

---

## 🔧 Technical Details

### DataTable Configuration
```javascript
{
    serverSide: true,
    processing: true,
    searching: true,
    paging: true,
    responsive: true,
    ajax: { url: "/home/GetAcademicSessionsDataTable" }
}
```

### Column Definitions
```javascript
columns: [
    { data: null },              // Row #
    { data: "name" },            // Session Name
    { data: "createdate" },      // Created Date
    { data: null }               // Actions
]
```

### Form Handlers
```razor
<!-- Create -->
<form method="post">
    <!-- Default handler: OnPostAsync() -->
</form>

<!-- Update -->
<form method="post" asp-page-handler="Update">
    <!-- Calls: OnPostUpdateAsync() -->
</form>

<!-- Delete -->
<form method="post" asp-page-handler="Delete">
    <!-- Calls: OnPostDeleteAsync() -->
</form>
```

---

## ✅ Validation Rules

### Client-Side
```html
<input type="text" 
       asp-for="Model.Name" 
       maxlength="15" 
       required>
```

### Server-Side (SystemActivitiesServices)
1. **Create**: Check for duplicate names
2. **Update**: Check for duplicates (excluding current)
3. **Delete**: Check if students are registered

---

## 📊 DataTable Features

| Feature | Status |
|---------|--------|
| Server-side pagination | ✅ |
| Real-time search | ✅ |
| Responsive design | ✅ |
| Loading indicator | ✅ |
| Error handling | ✅ |
| Custom date formatting | ✅ |
| Action buttons | ✅ |

---

## 🎯 Key Icons Used

| Icon | Usage |
|------|-------|
| `bi-calendar3-event` | Page header, modal titles |
| `bi-plus-circle` | Add button |
| `bi-pencil-square` | Edit button |
| `bi-trash` | Delete button |
| `bi-check-circle` | Save/Update buttons |
| `bi-x-circle` | Cancel buttons |
| `bi-check-circle-fill` | Success alert |
| `bi-exclamation-circle-fill` | Error alert |

---

## 🔐 Security

### CSRF Protection
```javascript
__RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
```

### Input Sanitization
- Trimmed in `SystemActivitiesServices`
- MaxLength validation (15 chars)
- Required field validation

---

## 📝 Developer Tips

### Debugging
```javascript
// Console log DataTable errors
error: function (xhr, error, thrown) {
    console.error('DataTable error:', error, thrown);
}
```

### Testing Create/Update
1. Open browser DevTools (F12)
2. Check Network tab for POST requests
3. Verify TempData alerts appear
4. Check DataTable reloads

### Testing Delete
1. Check SweetAlert dialog appears
2. Verify AJAX request in Network tab
3. Check page reloads after success

---

## 🎉 Final Result

**The Academic Session page is now:**
- ✅ Visually consistent with Fees Setup page
- ✅ Uses modern navy/gold color scheme
- ✅ Has complete CRUD functionality
- ✅ Includes search and pagination
- ✅ Has proper validation and error handling
- ✅ Uses SweetAlert2 for confirmations
- ✅ Follows ASP.NET Core best practices
- ✅ Build verified and production-ready

---

## 📞 Support

For questions or issues:
1. Check `ACADEMIC_SESSION_REDESIGN_SUMMARY.md` for detailed documentation
2. Review Fees Setup page (`/admin/feesmanager/fees-setup`) as reference
3. Check browser console for JavaScript errors
4. Verify DataTable endpoint is returning data

---

*Quick Reference Card*  
*Graham School Admin System*  
*D-Best Return Tech - 2026*
