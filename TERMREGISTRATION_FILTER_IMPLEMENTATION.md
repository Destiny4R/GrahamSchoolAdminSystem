# Term Registration Filter Implementation

## Overview
Updated the Term Registration system to accept and apply filters on the controller and service level. The filter system now properly passes Term, Session, Class, and SubClass filter parameters from the frontend to the backend database query.

## Changes Made

### 1. **Frontend (index.cshtml)**

#### Updated DataTable AJAX Configuration
- Changed URL from `/home/gettermregistrations` to `/api/v1/termregistration/list`
- Updated AJAX data function to send JSON payload with filter parameters
- Filters are sent via `additionalParameters` object:
  ```javascript
  additionalParameters: {
    term: $('#term').val(),
    session: $('#session').val(),
    schoolclass: $('#schoolclass').val(),
    subclass: $('#subclass').val()
  }
  ```

#### Updated Filter Change Handler
- Filters now trigger DataTable reload when all 4 filter fields have values
- Can clear all filters to show all records
- Uses `tabledata.page(0).draw('page')` for proper pagination reset

### 2. **Backend Service Layer (TermRegistrationServices.cs)**

#### Updated Method Signature
```csharp
public async Task<(List<TermRegDto> data, int recordsTotal, int recordsFiltered)> 
GetStudentTermRegistrationAsync(
    int skip = 0,
    int pageSize = 10,
    string searchTerm = "",
    int sortColumn = 0,
    string sortDirection = "asc",
    int? termFilter = null,        // NEW
    int? sessionFilter = null,      // NEW
    int? classFilter = null,        // NEW
    int? subclassFilter = null)     // NEW
```

#### Filter Logic Added
- Checks if filter parameters have values (greater than 0)
- Applies WHERE clauses to the query for each active filter:
  - `termFilter`: Filters by Term enum value
  - `sessionFilter`: Filters by SessionId
  - `classFilter`: Filters by SchoolClassId
  - `subclassFilter`: Filters by SchoolSubclassId

### 3. **Service Interface (ITermRegistrationServices.cs)**
- Updated interface method signature to match implementation with new filter parameters

### 4. **API Controller (v1Controller.cs)**

#### New Endpoint Added
```csharp
[HttpPost("termregistration/list")]
public async Task<IActionResult> GetTermRegistrations([FromBody] DataTableRequest request)
```

**Features:**
- Accepts DataTableRequest with filter parameters in `AdditionalParameters` property
- Extracts filter values (term, session, schoolclass, subclass) from the request
- Validates filter values are integers greater than 0 before applying
- Passes filters to the service method
- Returns DataTable-formatted response with `draw`, `recordsTotal`, `recordsFiltered`, and `data`
- Includes error handling with proper logging

### 5. **Data Model (DataTableRequest.cs)**

#### Enhanced Model Structure
```csharp
public class DataTableRequest
{
    public int? Draw { get; set; }
    public int? Start { get; set; }
    public int? Length { get; set; }
    public int SortColumn { get; set; }
    public string SortDirection { get; set; }
    public string SearchValue { get; set; }
    public SearchInfo Search { get; set; }           // NEW
    public List<OrderInfo> Order { get; set; }       // NEW
    public Dictionary<string, object> AdditionalParameters { get; set; }  // NEW
}

public class SearchInfo
{
    public string Value { get; set; }
}

public class OrderInfo
{
    public int Column { get; set; }
    public string Dir { get; set; }
}
```

### 6. **Home Controller (homeController.cs)**
- Deprecated old `gettermregistrations` endpoint
- Maintained other DataTable endpoints (SchoolClasses, AcademicSession, SubClass, Students)
- Updated `ExecuteDataTableAsync` to handle nullable properties from updated `DataTableRequest`

## Filter Behavior

### How Filters Work
1. **User selects filter values** in Term, Session, Class, and SubClass dropdowns
2. **On any dropdown change**:
   - If all 4 filters have values → Table reloads with filtered data
   - If all 4 filters are empty → Table shows all records
   - If only some filters have values → Table does not reload (validation logic)

### Database Query Filter Applied
```csharp
// Example: When all filters are selected
WHERE Term = 1 (First)
  AND SessionId = 5
  AND SchoolClassId = 3
  AND SchoolSubclassId = 2
```

### Filter Parameter Handling
- Filter values are converted to integers
- Only applied if value is greater than 0
- Null or 0 values are treated as "no filter"
- Multiple filters combine using AND logic

## API Request/Response Format

### Request Format (JSON)
```json
{
  "draw": 1,
  "start": 0,
  "length": 10,
  "search": {
    "value": ""
  },
  "order": [
    {
      "column": 0,
      "dir": "asc"
    }
  ],
  "additionalParameters": {
    "term": "1",
    "session": "5",
    "schoolclass": "3",
    "subclass": "2"
  }
}
```

### Response Format (JSON)
```json
{
  "draw": 1,
  "recordsTotal": 150,
  "recordsFiltered": 5,
  "data": [
    {
      "id": 1,
      "name": "John Doe",
      "term": "First",
      "session": "2024/2025",
      "schoolclass": "JSS 1",
      "createdate": "2025-01-15T10:30:00"
    }
  ]
}
```

## Testing Checklist

- [ ] Select all four filters - table shows filtered records
- [ ] Change one filter - verify data updates with new filter applied
- [ ] Clear all filters - table shows all records
- [ ] Apply filters then search - combined filtering works correctly
- [ ] Pagination works with filters applied
- [ ] Sort columns with filters applied
- [ ] Single delete still works with filters active
- [ ] Bulk delete still works with filters active
- [ ] Filter values persist when deleting records
- [ ] Error handling when API is unavailable

## Browser Console Debugging
To verify filter parameters are being sent correctly, check the Network tab in DevTools:
1. Open DevTools (F12)
2. Go to Network tab
3. Select a filter option
4. Look for POST request to `/api/v1/termregistration/list`
5. In the request payload, verify `additionalParameters` contains correct filter values

## Notes
- All four filters must have values for table to reload (by design for better UX)
- Filters use AND logic (must match all selected criteria)
- Server-side filtering reduces data transfer and improves performance
- Backward compatible with existing search and sort functionality
