# Term Registration Filters - Quick Reference

## Files Modified

| File | Change | Purpose |
|------|--------|---------|
| `index.cshtml` | Updated DataTable AJAX config | Pass filters to API |
| `index.cshtml` | Updated filter change handler | Reload table when all filters set |
| `TermRegistrationServices.cs` | Added 4 filter parameters | Filter database query |
| `ITermRegistrationServices.cs` | Updated method signature | Reflect new parameters |
| `v1Controller.cs` | Added new endpoint | Handle DataTable requests with filters |
| `DataTableRequest.cs` | Enhanced model structure | Support additional parameters |
| `homeController.cs` | Deprecated old endpoint | Deprecated `/home/gettermregistrations` |

## Code Examples

### Frontend - JavaScript

**Before:**
```javascript
$('#term, #session, #schoolclass, #subclass').on('change', function () {
    if (term && session && schoolclass && subclass) {
        tabledata.ajax.reload();
    }
});
```

**After:**
```javascript
$('#term, #session, #schoolclass, #subclass').on('change', function () {
    const term = $('#term').val();
    const session = $('#session').val();
    const schoolclass = $('#schoolclass').val();
    const subclass = $('#subclass').val();

    if (term && session && schoolclass && subclass) {
        tabledata.page(0).draw('page');
    } else if (!term && !session && !schoolclass && !subclass) {
        tabledata.page(0).draw('page');
    }
});
```

### DataTable AJAX Configuration

**Key Changes:**
```javascript
ajax: {
    url: "/api/v1/termregistration/list",    // Changed URL
    type: "POST",
    contentType: 'application/json',          // JSON format
    dataType: 'json',
    data: function (d) {
        // Include filter parameters
        return JSON.stringify({
            draw: d.draw,
            start: d.start || 0,
            length: d.length || 10,
            search: { value: d.search?.value || '' },
            order: d.order ? [{
                column: d.order[0].column,
                dir: d.order[0].dir
            }] : [],
            additionalParameters: {
                term: $('#term').val(),
                session: $('#session').val(),
                schoolclass: $('#schoolclass').val(),
                subclass: $('#subclass').val()
            }
        });
    }
}
```

### Backend - Service Method Signature

**Before:**
```csharp
public async Task<(List<TermRegDto> data, int recordsTotal, int recordsFiltered)> 
    GetStudentTermRegistrationAsync(
        int skip = 0,
        int pageSize = 10,
        string searchTerm = "",
        int sortColumn = 0,
        string sortDirection = "asc")
```

**After:**
```csharp
public async Task<(List<TermRegDto> data, int recordsTotal, int recordsFiltered)> 
    GetStudentTermRegistrationAsync(
        int skip = 0,
        int pageSize = 10,
        string searchTerm = "",
        int sortColumn = 0,
        string sortDirection = "asc",
        int? termFilter = null,        // NEW - Optional filter
        int? sessionFilter = null,     // NEW - Optional filter
        int? classFilter = null,       // NEW - Optional filter
        int? subclassFilter = null)    // NEW - Optional filter
```

### Backend - Filter Application in Query

```csharp
var query = _context.TermRegistrations
    .Include(tr => tr.Student)
    .Include(tr => tr.SchoolClass)
    .Include(tr => tr.SessionYear)
    .AsNoTracking()
    .AsQueryable();

// Apply filters if provided
if (termFilter.HasValue && termFilter > 0)
{
    query = query.Where(x => (int)x.Term == termFilter.Value);
}

if (sessionFilter.HasValue && sessionFilter > 0)
{
    query = query.Where(x => x.SessionId == sessionFilter.Value);
}

if (classFilter.HasValue && classFilter > 0)
{
    query = query.Where(x => x.SchoolClassId == classFilter.Value);
}

if (subclassFilter.HasValue && subclassFilter > 0)
{
    query = query.Where(x => x.SchoolSubclassId == subclassFilter.Value);
}
```

### Backend - API Endpoint

```csharp
[HttpPost("termregistration/list")]
public async Task<IActionResult> GetTermRegistrations([FromBody] DataTableRequest request)
{
    try
    {
        // Extract and validate filter parameters
        int? termFilter = null;
        int? sessionFilter = null;
        int? classFilter = null;
        int? subclassFilter = null;

        if (request.AdditionalParameters != null)
        {
            if (int.TryParse(request.AdditionalParameters.GetValueOrDefault("term")?.ToString(), out int term) && term > 0)
                termFilter = term;
            if (int.TryParse(request.AdditionalParameters.GetValueOrDefault("session")?.ToString(), out int session) && session > 0)
                sessionFilter = session;
            if (int.TryParse(request.AdditionalParameters.GetValueOrDefault("schoolclass")?.ToString(), out int schoolclass) && schoolclass > 0)
                classFilter = schoolclass;
            if (int.TryParse(request.AdditionalParameters.GetValueOrDefault("subclass")?.ToString(), out int subclass) && subclass > 0)
                subclassFilter = subclass;
        }

        // Call service with filters
        var (data, recordsTotal, recordsFiltered) = await _unitOfWork.TermRegistrationServices
            .GetStudentTermRegistrationAsync(
                skip: request.Start ?? 0,
                pageSize: request.Length ?? 10,
                searchTerm: request.Search?.Value ?? "",
                sortColumn: request.Order?.FirstOrDefault()?.Column ?? 0,
                sortDirection: request.Order?.FirstOrDefault()?.Dir ?? "asc",
                termFilter: termFilter,
                sessionFilter: sessionFilter,
                classFilter: classFilter,
                subclassFilter: subclassFilter
            );

        return Json(new
        {
            draw = request.Draw,
            recordsTotal = recordsTotal,
            recordsFiltered = recordsFiltered,
            data = data
        });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error getting term registrations");
        return Json(new
        {
            draw = request?.Draw ?? 0,
            recordsTotal = 0,
            recordsFiltered = 0,
            data = new List<object>(),
            error = "Error retrieving term registrations"
        });
    }
}
```

## How to Test

### Test Case 1: Basic Filtering
1. Open Term Registration page
2. Leave all filters empty → Table shows all records
3. Select Term = "First" → No reload (waiting)
4. Select Session = "2024/2025" → No reload (waiting)
5. Select Class = "JSS 1" → No reload (waiting)
6. Select SubClass = "A" → **Table reloads with filtered data ✓**

### Test Case 2: Clear Filters
1. With all filters set, showing 5 records
2. Click reset or clear one filter at a time
3. When all are cleared → **Table shows all records ✓**

### Test Case 3: Change Filter
1. With all filters set showing 5 records
2. Change Term from "First" to "Second"
3. Table immediately updates with new records

### Test Case 4: Filter + Search
1. All filters set, showing filtered records
2. Type in search box (if applicable)
3. Search works within filtered records

### Test Case 5: Filter + Pagination
1. All filters set
2. Verify pagination shows correct page count
3. Navigate between pages
4. Filters persist across pages

## Debugging

### Browser Developer Tools
1. Open F12 (Developer Tools)
2. Go to Network tab
3. Select any filter
4. Look for POST request to `/api/v1/termregistration/list`
5. Click on request, view Request Payload
6. Verify `additionalParameters` contains filter values

### Visual Verification
- **RecordsTotal**: Total records in database (e.g., 150)
- **RecordsFiltered**: Records after applying filters (e.g., 5)
- If RecordsFiltered < RecordsTotal, filters are working

### Common Issues

| Issue | Solution |
|-------|----------|
| Table doesn't update when filter selected | Ensure all 4 filters have values |
| Filters not sent to API | Check Network tab for `additionalParameters` |
| Wrong records displayed | Verify database values match filter values |
| API returns 500 error | Check controller logs for errors |
| Table shows "No data" | Verify filter values exist in database |

## Performance Considerations

- ✅ Filters applied at database level (efficient)
- ✅ Only required records transferred from server
- ✅ Pagination limits records per page (10 by default)
- ⚠️ No indexes on filter columns could slow large datasets
- 💡 Consider adding database indexes on `Term`, `SessionId`, `SchoolClassId`, `SchoolSubclassId`

## Security Notes

- ✅ Filter values validated as integers
- ✅ No SQL injection risk (using LINQ/EF Core)
- ✅ Server-side validation ensures data integrity
- ✅ Authorization attributes can be added to endpoint if needed
