# Term Registration Filter Flow Diagram

## Complete Data Flow

```
┌─────────────────────────────────────────────────────────────────┐
│                    FRONTEND (Razor View)                        │
│                   index.cshtml                                  │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  Filter Dropdowns:                                             │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐         │
│  │  Term    │ │ Session  │ │ Class    │ │SubClass  │         │
│  └──────────┘ └──────────┘ └──────────┘ └──────────┘         │
│       │              │            │             │              │
│       └──────────────┼────────────┼─────────────┘              │
│                      ↓            ↓                             │
│                 On Change Event                               │
│                      │                                        │
│                      ↓                                        │
│              Check all 4 filters                            │
│              have values?                                   │
│              ┌──────────────┐                                │
│              │  YES  │  NO  │                                │
│              └────┬──┴──────┘                                │
│                   │                                          │
│    ┌──────────────┘                                          │
│    │                                                          │
│    ↓                                                          │
│  tabledata.page(0).draw('page')                             │
│  (Reload with current filters)                             │
│                                                              │
└─────────────────────────────────────────────────────────────────┘
                           │
                           ↓
┌─────────────────────────────────────────────────────────────────┐
│                  AJAX POST REQUEST                              │
│          /api/v1/termregistration/list                         │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  JSON Payload:                                                 │
│  {                                                             │
│    "draw": 1,                                                 │
│    "start": 0,                                                │
│    "length": 10,                                              │
│    "search": { "value": "" },                                 │
│    "order": [{"column": 0, "dir": "asc"}],                  │
│    "additionalParameters": {                                 │
│      "term": "1",        ←── Filter Parameters             │
│      "session": "5",                                          │
│      "schoolclass": "3",                                     │
│      "subclass": "2"                                         │
│    }                                                          │
│  }                                                             │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
                           │
                           ↓
┌─────────────────────────────────────────────────────────────────┐
│              BACKEND API CONTROLLER                             │
│           v1Controller.GetTermRegistrations()                  │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  1. Extract Filter Parameters:                                 │
│     termFilter = int.Parse(request.term)    // 1              │
│     sessionFilter = int.Parse(request.session)  // 5           │
│     classFilter = int.Parse(request.schoolclass)  // 3         │
│     subclassFilter = int.Parse(request.subclass)  // 2         │
│                                                                 │
│  2. Validate Filters:                                          │
│     if (termFilter > 0) include in query ✓                     │
│     if (sessionFilter > 0) include in query ✓                  │
│     if (classFilter > 0) include in query ✓                    │
│     if (subclassFilter > 0) include in query ✓                 │
│                                                                 │
│  3. Call Service:                                              │
│     GetStudentTermRegistrationAsync(                           │
│       skip: 0,                                                 │
│       pageSize: 10,                                            │
│       searchTerm: "",                                          │
│       sortColumn: 0,                                           │
│       sortDirection: "asc",                                    │
│       termFilter: 1,        ←── Filters passed              │
│       sessionFilter: 5,                                        │
│       classFilter: 3,                                         │
│       subclassFilter: 2)                                      │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
                           │
                           ↓
┌─────────────────────────────────────────────────────────────────┐
│          SERVICE LAYER - Business Logic                         │
│      TermRegistrationServices.cs                               │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  1. Build Base Query:                                          │
│     var query = _context.TermRegistrations                    │
│       .Include(tr => tr.Student)                             │
│       .Include(tr => tr.SchoolClass)                         │
│       .Include(tr => tr.SessionYear)                         │
│       .AsNoTracking()                                         │
│       .AsQueryable();                                         │
│                                                                 │
│  2. Get Total Count:                                           │
│     recordsTotal = await query.CountAsync();  // 150          │
│                                                                 │
│  3. Apply Filters (if provided):                              │
│     ┌─ if (termFilter.HasValue && termFilter > 0)            │
│     │  query = query.Where(x => (int)x.Term == termFilter);  │
│     │                                                          │
│     ├─ if (sessionFilter.HasValue && sessionFilter > 0)      │
│     │  query = query.Where(x => x.SessionId == sessionFilter);│
│     │                                                          │
│     ├─ if (classFilter.HasValue && classFilter > 0)          │
│     │  query = query.Where(x => x.SchoolClassId == classFilter);│
│     │                                                          │
│     └─ if (subclassFilter.HasValue && subclassFilter > 0)    │
│        query = query.Where(x => x.SchoolSubclassId == subclassFilter);│
│                                                                 │
│  4. Get Filtered Count:                                        │
│     recordsFiltered = await query.CountAsync();  // 5         │
│                                                                 │
│  5. Apply Search (if provided):                               │
│     query = query.Where(x =>                                  │
│       x.Student.Surname.Contains(searchTerm) ||              │
│       x.Student.Firstname.Contains(searchTerm) ||            │
│       x.SchoolClass.Name.Contains(searchTerm) ||             │
│       x.SessionYear.Name.Contains(searchTerm));              │
│                                                                 │
│  6. Apply Sorting:                                             │
│     query = query.OrderBy/OrderByDescending(...);            │
│                                                                 │
│  7. Apply Pagination:                                          │
│     data = await query.Skip(0).Take(10)                      │
│       .Select(x => new TermRegDto { ... })                  │
│       .ToListAsync();                                         │
│                                                                 │
│  8. Return Results:                                            │
│     return (data, recordsTotal: 150, recordsFiltered: 5);    │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
                           │
                           ↓
┌─────────────────────────────────────────────────────────────────┐
│              DATABASE QUERY EXECUTED                            │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  SELECT * FROM TermRegistrations                              │
│  JOIN Students ON TermRegistrations.StudentId = Students.Id   │
│  JOIN SchoolClasses ON ...                                    │
│  JOIN SessionYears ON ...                                     │
│  WHERE                                                         │
│    Term = 1                           -- Filter 1             │
│    AND SessionId = 5                  -- Filter 2             │
│    AND SchoolClassId = 3              -- Filter 3             │
│    AND SchoolSubclassId = 2           -- Filter 4             │
│  ORDER BY CreatedDate DESC                                    │
│  SKIP 0 TAKE 10;                                              │
│                                                                 │
│  Result: 5 records found (out of 150 total)                  │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
                           │
                           ↓
┌─────────────────────────────────────────────────────────────────┐
│              RESPONSE BACK TO FRONTEND                          │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  {                                                              │
│    "draw": 1,                                                 │
│    "recordsTotal": 150,       ← Total records in DB           │
│    "recordsFiltered": 5,      ← Records after filtering       │
│    "data": [                                                   │
│      {                                                          │
│        "id": 1,                                               │
│        "name": "John Doe",                                    │
│        "term": "First",                                       │
│        "session": "2024/2025",                                │
│        "schoolclass": "JSS 1",                                │
│        "createdate": "2025-01-15T10:30:00"                    │
│      },                                                        │
│      ... 4 more records ...                                   │
│    ]                                                           │
│  }                                                              │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
                           │
                           ↓
┌─────────────────────────────────────────────────────────────────┐
│            DATATABLE PROCESSES RESPONSE                         │
│            Displays 5 Filtered Records                          │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  Table Updates:                                                │
│  ┌──────────────────────────────────────────────────────────┐ │
│  │ # │ Name      │ Term  │ Session    │ Class   │ Actions  │ │
│  ├──────────────────────────────────────────────────────────┤ │
│  │ 1 │ John Doe  │ First │ 2024/2025  │ JSS 1   │ ⋯       │ │
│  │ 2 │ Jane Smith│ First │ 2024/2025  │ JSS 1   │ ⋯       │ │
│  │ 3 │ Bob Wilson│ First │ 2024/2025  │ JSS 1   │ ⋯       │ │
│  │ 4 │ Alice Lee │ First │ 2024/2025  │ JSS 1   │ ⋯       │ │
│  │ 5 │ Charlie Brown│ First │ 2024/2025  │ JSS 1   │ ⋯       │ │
│  └──────────────────────────────────────────────────────────┘ │
│                                                                 │
│  Status Bar: Showing 1 to 5 of 5 entries                     │
│  (filtered from 150 total entries)                            │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

## Filter Validation Rules

```
USER ACTION                          TABLE BEHAVIOR
─────────────────────────────────────────────────────────────
Select Term Only                 → No reload (waiting for others)
Select Term + Session            → No reload (waiting for others)
Select Term + Session + Class     → No reload (waiting for Subclass)
Select All 4 Filters             → RELOAD with filtered data ✓
Clear 1 Filter (back to 3)       → No change
Clear All Filters (all empty)    → RELOAD with all data ✓
Change Any Filter (when 4 set)   → RELOAD immediately ✓
```

## Performance Notes

- **Server-side filtering**: Reduces data transfer - only filtered records sent
- **Database query optimization**: Filters applied at DB level, not in code
- **Indexes recommended**: Add indexes on `Term`, `SessionId`, `SchoolClassId`, `SchoolSubclassId`
- **Large datasets**: Pagination ensures only 10 records per page displayed
- **Search + Filters**: Both work together using AND logic
