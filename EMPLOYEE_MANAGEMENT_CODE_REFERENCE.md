# Employee Management - Developer Code Reference

## Service Layer Examples

### Using the Employee Service

#### Get All Employees
```csharp
// In your page handler or service
var (employees, recordsTotal, recordsFiltered) = 
    await _unitOfWork.UsersServices.GetEmployeesAsync(
        start: 0,           // Skip first N records
        length: 10,         // Take N records
        searchValue: "",    // Search term
        sortColumnIndex: 0, // Sort by column index
        sortDirection: "asc" // Sort direction
    );

// Result contains:
// - employees: List<EmployeeViewModel>
// - recordsTotal: total records in database
// - recordsFiltered: records after search filter

foreach (var emp in employees)
{
    Console.WriteLine($"{emp.FullName} - {emp.Email} - {emp.Department}");
}
```

#### Get Single Employee
```csharp
var employee = await _unitOfWork.UsersServices.GetEmployeeByIdAsync(employeeId);

if (employee != null)
{
    Console.WriteLine($"Found: {employee.FullName}");
    Console.WriteLine($"Positions: {string.Join(", ", employee.Positions)}");
}
else
{
    Console.WriteLine("Employee not found");
}
```

#### Create Employee
```csharp
var model = new EmployeeViewModel
{
    FirstName = "John",
    LastName = "Doe",
    Email = "john.doe@school.com",
    Phone = "1234567890",
    Department = "Mathematics",
    Address = "123 Main St",
    ApplicationUserId = userId, // Must exist in AspNetUsers
    IsActive = true
};

var (succeeded, message, data) = 
    await _unitOfWork.UsersServices.CreateEmployeeAsync(model);

if (succeeded)
{
    int employeeId = (int)data;
    Console.WriteLine($"Created employee with ID: {employeeId}");
}
else
{
    Console.WriteLine($"Error: {message}");
}
```

#### Update Employee
```csharp
var model = new EmployeeViewModel
{
    Id = employeeId,
    FirstName = "Jane",
    LastName = "Smith",
    Email = "jane.smith@school.com",
    Phone = "9876543210",
    Department = "English",
    Address = "456 Oak Ave",
    ApplicationUserId = userId,
    IsActive = true
};

var (succeeded, message) = 
    await _unitOfWork.UsersServices.UpdateEmployeeAsync(model);

Console.WriteLine(message); // "Employee updated successfully" or error
```

#### Delete Employee
```csharp
var (succeeded, message) = 
    await _unitOfWork.UsersServices.DeleteEmployeeAsync(employeeId);

if (succeeded)
{
    // Employee and all position assignments deleted
    Console.WriteLine("Employee deleted");
}
else
{
    Console.WriteLine($"Error: {message}");
}
```

#### Assign Position to Employee
```csharp
var (succeeded, message) = 
    await _unitOfWork.UsersServices.AssignPositionToEmployeeAsync(
        employeeId: 1,
        positionId: 5
    );

if (succeeded)
{
    // Employee now has access to position's roles and permissions
    Console.WriteLine("Position assigned");
}
else
{
    Console.WriteLine($"Error: {message}");
}
```

#### Remove Position from Employee
```csharp
var (succeeded, message) = 
    await _unitOfWork.UsersServices.RemovePositionFromEmployeeAsync(
        employeeId: 1,
        positionId: 5
    );

Console.WriteLine(message);
```

## Page Handler Examples

### Complete OnGet Example
```csharp
public async Task<IActionResult> OnGetAsync()
{
    try
    {
        // Load employee data for DataTable
        var (employeeData, totalRecords, filteredRecords) = 
            await _unitOfWork.UsersServices.GetEmployeesAsync(
                start: 0,
                length: 10
            );

        Employees = employeeData;
        ViewData["TotalRecords"] = totalRecords;

        // Load available positions for dropdowns
        var (positionData, _, _) = 
            await _unitOfWork.UsersServices.GetPositionsAsync(
                start: 0,
                length: 100,
                sortDirection: "asc"
            );

        AvailablePositions = positionData.Cast<dynamic>().ToList();

        return Page();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error in OnGetAsync");
        ModelState.AddModelError("", "Error loading page");
        return Page();
    }
}
```

### Complete OnPost Create Example
```csharp
public async Task<IActionResult> OnPostAddEmployeeAsync()
{
    // Validate model
    if (!ModelState.IsValid)
    {
        return Page();
    }

    try
    {
        // Call service
        var (succeeded, message, data) = 
            await _unitOfWork.UsersServices.CreateEmployeeAsync(EmployeeModel);

        if (succeeded)
        {
            // Log the action
            await _unitOfWork.LogService.LogUserActionAsync(
                userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                userName: User.Identity?.Name,
                action: "Create",
                entityType: "Employee",
                entityId: data?.ToString() ?? "Unknown",
                message: $"Employee '{EmployeeModel.FirstName} {EmployeeModel.LastName}' created",
                ipAddress: GetClientIpAddress(),
                details: $"Email: {EmployeeModel.Email}, Phone: {EmployeeModel.Phone}"
            );

            TempData["SuccessMessage"] = message;
            return RedirectToPage();
        }

        // Failure
        TempData["ErrorMessage"] = message;
        return RedirectToPage();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error creating employee");
        
        // Log error
        await _unitOfWork.LogService.LogErrorAsync(
            subject: "Employee Creation Error",
            message: $"Error creating employee {EmployeeModel.FirstName}",
            details: ex.Message,
            userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            ipAddress: GetClientIpAddress()
        );

        TempData["ErrorMessage"] = "Error creating employee";
        return RedirectToPage();
    }
}
```

### AJAX Handler Example
```csharp
public async Task<IActionResult> OnGetEditEmployeeAsync(int id)
{
    try
    {
        var employee = await _unitOfWork.UsersServices.GetEmployeeByIdAsync(id);
        
        if (employee == null)
            return NotFound();

        // Return JSON for AJAX
        return new JsonResult(new
        {
            id = employee.Id,
            firstName = employee.FirstName,
            lastName = employee.LastName,
            email = employee.Email,
            phone = employee.Phone,
            department = employee.Department,
            address = employee.Address,
            isActive = employee.IsActive
        });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, $"Error loading employee {id}");
        return StatusCode(500, new { error = "Error loading employee" });
    }
}
```

## Validation Examples

### Custom Validation
```csharp
public async Task<(bool Succeeded, string Message)> ValidateEmployeeAsync(
    EmployeeViewModel model)
{
    // Check required fields
    if (string.IsNullOrWhiteSpace(model.FirstName))
        return (false, "First name is required");

    if (string.IsNullOrWhiteSpace(model.Email))
        return (false, "Email is required");

    // Validate email format
    try
    {
        var addr = new System.Net.Mail.MailAddress(model.Email);
    }
    catch
    {
        return (false, "Invalid email format");
    }

    // Validate phone
    var phoneDigitsOnly = System.Text.RegularExpressions.Regex.Replace(
        model.Phone, @"\D", "");
    
    if (phoneDigitsOnly.Length < 10)
        return (false, "Phone must have at least 10 digits");

    // Check for duplicates
    var existingEmployee = await _context.Employees
        .Include(e => e.ApplicationUser)
        .FirstOrDefaultAsync(e => 
            e.ApplicationUser.Email == model.Email && e.Id != model.Id);

    if (existingEmployee != null)
        return (false, "Email already in use");

    return (true, "Validation passed");
}
```

## DataTable Integration Examples

### Server-Side Processing
```javascript
// JavaScript - Communicating with server
$('#employeesTable').DataTable({
    processing: true,
    serverSide: false, // Client-side for simplicity
    ajax: {
        url: '/admin/employees?handler=GetEmployees',
        type: 'POST',
        data: function (d) {
            // Customize what data is sent to server
            d.searchValue = d.search.value;
            d.sortColumnIndex = d.order[0].column;
            d.sortDirection = d.order[0].dir;
        }
    },
    columns: [
        { data: 'firstName' },
        { data: 'email' },
        { data: 'phone' },
        { data: 'department' },
        { 
            data: 'positions',
            render: function (data) {
                if (!data || data.length === 0) return 'None';
                return data.map(p => 
                    `<span class="badge bg-info">${p}</span>`
                ).join(' ');
            }
        },
        {
            data: null,
            render: function (data, type, row) {
                return `
                    <button class="btn btn-sm btn-primary" onclick="editEmployee(${row.id})">
                        <i class="fa fa-edit"></i>
                    </button>
                    <button class="btn btn-sm btn-warning" onclick="assignPosition(${row.id})">
                        <i class="fa fa-star"></i>
                    </button>
                    <button class="btn btn-sm btn-danger" onclick="deleteEmployee(${row.id})">
                        <i class="fa fa-trash"></i>
                    </button>
                `;
            }
        }
    ]
});
```

### AJAX Edit Modal Population
```javascript
function editEmployee(id) {
    fetch(`/admin/employees?handler=EditEmployee&id=${id}`, {
        method: 'GET',
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        }
    })
    .then(response => response.json())
    .then(data => {
        // Populate form fields
        document.getElementById('editEmployeeId').value = data.id;
        document.getElementById('editFirstName').value = data.firstName;
        document.getElementById('editLastName').value = data.lastName;
        document.getElementById('editEmail').value = data.email;
        document.getElementById('editPhone').value = data.phone;
        document.getElementById('editDepartment').value = data.department;
        document.getElementById('editAddress').value = data.address;
        
        // Show modal
        const modal = new bootstrap.Modal(
            document.getElementById('editEmployeeModal')
        );
        modal.show();
    })
    .catch(error => {
        console.error('Error:', error);
        showError('Error loading employee');
    });
}
```

## Logging Examples

### Log Employee Creation
```csharp
await _unitOfWork.LogService.LogUserActionAsync(
    userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
    userName: User.Identity?.Name,
    action: "Create",
    entityType: "Employee",
    entityId: employeeId.ToString(),
    message: $"Employee created: {employee.FullName}",
    ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
    details: $@"{{
        ""firstName"": ""{employee.FirstName}"",
        ""lastName"": ""{employee.LastName}"",
        ""email"": ""{employee.Email}"",
        ""phone"": ""{employee.Phone}"",
        ""department"": ""{employee.Department}""
    }}"
);
```

### Query Logs by Employee
```csharp
// Get all actions for specific employee
var employeeLogs = await _unitOfWork.LogService.GetEntityLogsAsync(
    entityType: "Employee",
    entityId: "123",
    pageNumber: 1,
    pageSize: 50
);

foreach (var log in employeeLogs.data)
{
    Console.WriteLine($"{log.CreatedDate:yyyy-MM-dd HH:mm} - {log.Action} - {log.UserName}");
}
```

### Audit Trail Report
```csharp
// Get all employee-related actions
var logs = await _unitOfWork.LogService.GetLogsAsync(
    pageNumber: 1,
    pageSize: 100,
    searchTerm: "Employee",
    logLevel: 0,
    dateRange: 30 // Last 30 days
);

// Analyze
var createCount = logs.data.Count(l => l.Action == "Create");
var updateCount = logs.data.Count(l => l.Action == "Update");
var deleteCount = logs.data.Count(l => l.Action == "Delete");

Console.WriteLine($"Creates: {createCount}, Updates: {updateCount}, Deletes: {deleteCount}");
```

## Error Handling Examples

### Try-Catch Pattern
```csharp
try
{
    var result = await _unitOfWork.UsersServices.CreateEmployeeAsync(model);
    
    if (!result.Succeeded)
    {
        // Business logic error (duplicate, validation, etc)
        _logger.LogWarning($"Business error: {result.Message}");
        ModelState.AddModelError("", result.Message);
        return Page();
    }

    // Success
    return RedirectToPage();
}
catch (DbUpdateException ex)
{
    // Database error
    _logger.LogError(ex, "Database error creating employee");
    ModelState.AddModelError("", "Database error occurred");
    return Page();
}
catch (Exception ex)
{
    // Unexpected error
    _logger.LogError(ex, "Unexpected error creating employee");
    ModelState.AddModelError("", "An unexpected error occurred");
    return Page();
}
```

## Unit Testing Examples

### Test Employee Service
```csharp
[TestClass]
public class EmployeeServiceTests
{
    private IUsersServices _service;
    private ApplicationDbContext _context;

    [TestInitialize]
    public void Setup()
    {
        // Setup test database and service
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("TestDb")
            .Options;

        _context = new ApplicationDbContext(options);
        _service = new UsersServices(_context, null, null);
    }

    [TestMethod]
    public async Task CreateEmployee_WithValidData_ShouldSucceed()
    {
        // Arrange
        var model = new EmployeeViewModel
        {
            FirstName = "Test",
            LastName = "Employee",
            Email = "test@example.com",
            Phone = "1234567890",
            ApplicationUserId = "user-1"
        };

        // Act
        var (succeeded, message, data) = 
            await _service.CreateEmployeeAsync(model);

        // Assert
        Assert.IsTrue(succeeded);
        Assert.IsNotNull(data);
    }

    [TestMethod]
    public async Task CreateEmployee_WithDuplicateEmail_ShouldFail()
    {
        // Arrange
        var model = new EmployeeViewModel
        {
            FirstName = "Test",
            LastName = "Employee",
            Email = "test@example.com",
            Phone = "1234567890",
            ApplicationUserId = "user-1"
        };

        // Act - Create first
        await _service.CreateEmployeeAsync(model);

        // Act - Create duplicate
        model.Phone = "9876543210";
        var (succeeded, message, _) = 
            await _service.CreateEmployeeAsync(model);

        // Assert
        Assert.IsFalse(succeeded);
        Assert.IsTrue(message.Contains("already exists"));
    }
}
```

## Performance Tips

### Optimize DataTable Queries
```csharp
// Good: Includes related entities
var employees = _context.Employees
    .Include(e => e.ApplicationUser)           // Avoid N+1
    .Include(e => e.EmployeePositions)         // Avoid N+1
    .ThenInclude(ep => ep.Position)            // Avoid N+1
    .AsNoTracking()                            // Don't track changes
    .Skip(pageStart)
    .Take(pageSize)
    .ToListAsync();

// Bad: Will cause N+1 queries
var employees = _context.Employees.ToList();  // All data
employees.ForEach(e => {
    var positions = e.EmployeePositions;      // N queries!
});
```

### Caching Example
```csharp
private const string POSITIONS_CACHE_KEY = "available_positions";

public async Task<List<PositionDto>> GetAvailablePositionsAsync()
{
    // Check cache first
    if (_cache.TryGetValue(POSITIONS_CACHE_KEY, 
        out List<PositionDto> cachedPositions))
    {
        return cachedPositions;
    }

    // Fetch from database
    var positions = await _context.PositionTables
        .AsNoTracking()
        .Select(p => new PositionDto 
        { 
            Id = p.Id, 
            Name = p.Name 
        })
        .ToListAsync();

    // Cache for 1 hour
    _cache.Set(POSITIONS_CACHE_KEY, positions, 
        TimeSpan.FromHours(1));

    return positions;
}
```

## Extension Ideas

### Add Search Stored Procedure
```sql
CREATE PROCEDURE sp_SearchEmployees
    @SearchTerm NVARCHAR(100),
    @PageNumber INT = 1,
    @PageSize INT = 10
AS
BEGIN
    SELECT 
        e.Id,
        e.FullName,
        e.Email,
        e.Phone,
        COUNT(*) OVER() AS TotalCount
    FROM EmployeesTables e
    WHERE e.FullName LIKE @SearchTerm + '%'
       OR e.Email LIKE @SearchTerm + '%'
       OR e.Phone LIKE @SearchTerm + '%'
    ORDER BY e.FullName
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY
END
```

### Add Excel Export
```csharp
public async Task<byte[]> ExportEmployeesAsync()
{
    var employees = await GetEmployeesAsync(0, 10000, "", 0, "asc");
    
    var workbook = new XLWorkbook();
    var worksheet = workbook.Worksheets.Add("Employees");
    
    // Headers
    worksheet.Cell(1, 1).Value = "Name";
    worksheet.Cell(1, 2).Value = "Email";
    worksheet.Cell(1, 3).Value = "Phone";
    worksheet.Cell(1, 4).Value = "Department";
    
    // Data
    int row = 2;
    foreach (var emp in employees.data)
    {
        worksheet.Cell(row, 1).Value = emp.FullName;
        worksheet.Cell(row, 2).Value = emp.Email;
        worksheet.Cell(row, 3).Value = emp.Phone;
        worksheet.Cell(row, 4).Value = emp.Department;
        row++;
    }
    
    using (var stream = new MemoryStream())
    {
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
```

## Summary

This developer reference provides:
- Complete service method examples
- Page handler implementations
- AJAX integration patterns
- Validation strategies
- Error handling approaches
- Logging integration
- Testing examples
- Performance optimization tips
- Extension ideas

All code follows project patterns and best practices for the Graham School Admin System.
