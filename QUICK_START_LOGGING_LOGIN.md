# Quick Implementation Guide: Logging & Login System

## 🚀 Quick Start

### 1. Database Migration (Required)

Since the `LogsTable` model has been enhanced with new fields, you need to create a migration:

```bash
cd GrahamSchoolAdminSystemAccess
dotnet ef migrations add EnhancedLogging -o Data/Migrations
cd ..
dotnet ef database update
```

This will add the following columns to the `LogsTables` table:
- `LogLevel` (varchar 20)
- `UserId` (varchar max)
- `UserName` (varchar 256)
- `Action` (varchar 50)
- `EntityType` (varchar 100)
- `EntityId` (varchar max)
- `IpAddress` (varchar 45)
- `Details` (varchar 4000)
- `StatusCode` (int nullable)

### 2. Verify Files Are in Place

Ensure these files exist and have been properly updated:

✅ **Modified Files:**
- `GrahamSchoolAdminSystemModels/Models/LogsTable.cs` - Enhanced model
- `GrahamSchoolAdminSystemAccess/IServiceRepo/ILogService.cs` - Enhanced interface
- `GrahamSchoolAdminSystemAccess/ServiceRepo/LogService.cs` - Full implementation
- `GrahamSchoolAdminSystemAccess/ServiceRepo/UnitOfWork.cs` - Now includes LogService
- `GrahamSchoolAdminSystemAccess/IServiceRepo/IUnitOfWork.cs` - Interface updated
- `GrahamSchoolAdminSystemWeb/Program.cs` - Services registered
- `GrahamSchoolAdminSystemWeb/Pages/account/login.cshtml` - New login UI
- `GrahamSchoolAdminSystemWeb/Pages/account/login.cshtml.cs` - Login logic with auditing

✅ **New Files:**
- `GrahamSchoolAdminSystemWeb/Helpers/AuditLoggingFilter.cs` - Auto logging filter
- `LOGGING_AND_LOGIN_IMPLEMENTATION.md` - Full documentation

### 3. Build & Test

```bash
# Build the solution
dotnet build

# Run the application
dotnet run
```

### 4. Test Login

Navigate to `/Account/Login` and test with your admin credentials.

---

## 📋 Implementation Checklist

### Phase 1: Setup (Complete ✅)
- [x] Enhanced LogsTable model
- [x] Updated ILogService interface
- [x] Implemented full LogService
- [x] Updated UnitOfWork
- [x] Created AuditLoggingFilter
- [x] Updated Program.cs

### Phase 2: Login Page (Complete ✅)
- [x] Professional UI design
- [x] Login form with validation
- [x] Password toggle functionality
- [x] Remember me checkbox
- [x] Forgot password modal
- [x] Responsive design
- [x] Authentication logging

### Phase 3: Integration (Manual)

Now you need to add logging calls to your existing page handlers and actions:

#### 3.1 Position Management (`index.cshtml.cs`)

```csharp
public async Task<IActionResult> OnPostAddPositionAsync()
{
    if (!ModelState.IsValid)
        return Page();

    var result = await _unitOfWork.UsersServices.CreatePositionAsync(PositionModel);

    if (result.Succeeded)
    {
        // LOG THE ACTION
        await _unitOfWork.LogService.LogUserActionAsync(
            userId: User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
            userName: User.Identity.Name,
            action: "Create",
            entityType: "Position",
            entityId: PositionModel.Id.ToString(),
            message: $"Position '{PositionModel.Name}' created successfully",
            ipAddress: GetClientIpAddress(),
            details: $"Description: {PositionModel.Description}"
        );

        TempData["SuccessMessage"] = result.Message;
        return RedirectToPage();
    }

    TempData["ErrorMessage"] = result.Message;
    return RedirectToPage();
}

public async Task<IActionResult> OnPostUpdatePositionAsync()
{
    if (!ModelState.IsValid)
        return Page();

    var result = await _unitOfWork.UsersServices.UpdatePositionAsync(PositionModel);

    if (result.Succeeded)
    {
        // LOG THE ACTION
        await _unitOfWork.LogService.LogUserActionAsync(
            userId: User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
            userName: User.Identity.Name,
            action: "Update",
            entityType: "Position",
            entityId: PositionModel.Id.ToString(),
            message: $"Position '{PositionModel.Name}' updated successfully",
            ipAddress: GetClientIpAddress(),
            details: $"New Name: {PositionModel.Name}, Description: {PositionModel.Description}"
        );

        TempData["SuccessMessage"] = result.Message;
        return RedirectToPage();
    }

    TempData["ErrorMessage"] = result.Message;
    return RedirectToPage();
}

public async Task<IActionResult> OnPostDeletePositionAsync(int positionId)
{
    var position = await _unitOfWork.UsersServices.GetPositionByIdAsync(positionId);
    var result = await _unitOfWork.UsersServices.DeletePositionAsync(positionId);

    if (result.Succeeded)
    {
        // LOG THE ACTION
        await _unitOfWork.LogService.LogUserActionAsync(
            userId: User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
            userName: User.Identity.Name,
            action: "Delete",
            entityType: "Position",
            entityId: positionId.ToString(),
            message: $"Position deleted successfully",
            ipAddress: GetClientIpAddress(),
            details: $"Deleted Position: {position?.Name}"
        );

        TempData["SuccessMessage"] = result.Message;
    }
    else
    {
        TempData["ErrorMessage"] = result.Message;
    }

    return RedirectToPage();
}

public async Task<IActionResult> OnPostAssignRolesAsync()
{
    if (!ModelState.IsValid)
        return Page();

    var result = await _unitOfWork.UsersServices.AssignRolesToPositionAsync(
        AssignRoleModel.PositionId,
        AssignRoleModel.SelectedRoleIds
    );

    if (result.Succeeded)
    {
        // LOG THE ACTION
        await _unitOfWork.LogService.LogUserActionAsync(
            userId: User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
            userName: User.Identity.Name,
            action: "Update",
            entityType: "PositionRole",
            entityId: AssignRoleModel.PositionId.ToString(),
            message: $"Roles assigned to position",
            ipAddress: GetClientIpAddress(),
            details: $"Assigned {AssignRoleModel.SelectedRoleIds.Count} roles"
        );

        TempData["SuccessMessage"] = result.Message;
    }
    else
    {
        TempData["ErrorMessage"] = result.Message;
    }

    return RedirectToPage();
}

// Add helper method
private string GetClientIpAddress()
{
    var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
    if (HttpContext.Request.Headers.ContainsKey("X-Forwarded-For"))
    {
        var forwardedIp = HttpContext.Request.Headers["X-Forwarded-For"]
            .ToString().Split(',').FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedIp))
        {
            ipAddress = forwardedIp.Trim();
        }
    }
    return ipAddress ?? "Unknown";
}
```

#### 3.2 Add Using Statements

Add these to the top of your page models:

```csharp
using System.Security.Claims;
using GrahamSchoolAdminSystemAccess.IServiceRepo;
```

#### 3.3 Inject ILogService (if needed)

If you're not using UnitOfWork already, inject ILogService:

```csharp
public class YourPageModel : PageModel
{
    private readonly ILogService _logService;

    public YourPageModel(ILogService logService, ...)
    {
        _logService = logService;
        ...
    }
}
```

---

## 🔑 Key Integration Points

### 1. Logging in Page Handlers

Use this pattern for logging in Razor Page handlers:

```csharp
await _unitOfWork.LogService.LogUserActionAsync(
    userId: User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
    userName: User.Identity.Name,
    action: "Create|Update|Delete|Read",
    entityType: "Position|Role|User|etc",
    entityId: entityId.ToString(),
    message: "User-friendly message",
    ipAddress: GetClientIpAddress(),
    details: "Optional detailed info"
);
```

### 2. Logging in Controllers

```csharp
await _unitOfWork.LogService.LogUserActionAsync(
    userId: User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
    userName: User.Identity.Name,
    action: "Create",
    entityType: "MyEntity",
    entityId: id.ToString(),
    message: "Action completed",
    ipAddress: Request.HttpContext.Connection.RemoteIpAddress?.ToString(),
    details: "Additional context"
);
```

### 3. Logging Errors

```csharp
try
{
    // Some operation
}
catch (Exception ex)
{
    await _unitOfWork.LogService.LogErrorAsync(
        subject: "Operation Name",
        message: "What went wrong",
        details: ex.Message,
        userId: User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
        ipAddress: GetClientIpAddress()
    );
}
```

---

## 🧪 Testing

### Test 1: Login Page
1. Navigate to `/Account/Login`
2. Try with incorrect credentials
3. Check that error is logged in database
4. Login with correct credentials
5. Verify successful login is logged

### Test 2: Position Management
1. Add a new position
2. Check `LogsTables` for entry with Action="Create"
3. Update the position
4. Check for Action="Update" entry
5. Delete the position
6. Check for Action="Delete" entry

### Test 3: Verify Log Fields
Check that these fields are populated correctly:
- UserId ✓
- UserName ✓
- Action ✓
- EntityType ✓
- EntityId ✓
- IpAddress ✓
- CreatedDate ✓
- LogLevel ✓

---

## 📊 Query Logs

### Via Database

```sql
-- Get all login attempts
SELECT * FROM LogsTables 
WHERE Action = 'Login' 
ORDER BY CreatedDate DESC;

-- Get failed logins
SELECT * FROM LogsTables 
WHERE Action = 'Login' AND LogLevel = 'WARNING' 
ORDER BY CreatedDate DESC;

-- Get user activity
SELECT * FROM LogsTables 
WHERE UserId = 'user-id' 
ORDER BY CreatedDate DESC;

-- Get entity history
SELECT * FROM LogsTables 
WHERE EntityType = 'Position' AND EntityId = 'position-id' 
ORDER BY CreatedDate DESC;
```

### Via Code

```csharp
// Get recent logs
var logs = await _unitOfWork.LogService.GetLogsAsync(
    pageNumber: 1,
    pageSize: 50
);

// Search logs
var searchResults = await _unitOfWork.LogService.GetLogsAsync(
    searchTerm: "Position",
    logLevel: "INFO"
);

// Get user logs
var userLogs = await _unitOfWork.LogService.GetUserLogsAsync(userId);

// Get entity history
var history = await _unitOfWork.LogService.GetEntityLogsAsync("Position", "123");
```

---

## 🎨 Login Page Features

### Auto-Features:
- ✅ Password visibility toggle
- ✅ Remember me functionality
- ✅ Forgot password modal
- ✅ Form validation
- ✅ Error messages
- ✅ Loading state during submission
- ✅ Responsive design
- ✅ Smooth animations

### What Users See:
- Professional gradient background
- Clear form layout
- Password security toggle
- Remember me option
- Forgot password link
- Help information
- Security reassurance

---

## ⚠️ Important Notes

1. **Migration Required**: You MUST run the database migration before using the enhanced logging

2. **IP Address Tracking**: Make sure proxy headers are properly configured if behind a reverse proxy

3. **Performance**: The audit logging filter logs all POST/PUT/DELETE requests - this is intentional for security

4. **Data Retention**: Consider implementing log retention policies to manage database size

5. **Sensitive Data**: Avoid logging passwords or other sensitive information in details field

---

## 📝 Next Steps

1. ✅ Run database migration
2. ⏳ Build and test the application
3. ⏳ Add logging to remaining page handlers
4. ⏳ Create admin dashboard to view logs
5. ⏳ Set up alerts for suspicious activities
6. ⏳ Implement log retention policy
7. ⏳ Regular security audits

---

## 🆘 Troubleshooting

### Issue: "Column not found" error
**Solution**: Run the database migration:
```bash
dotnet ef database update
```

### Issue: Login page not loading
**Solution**: Clear browser cache and reload, check that file exists at `/Account/Login`

### Issue: Logs not being saved
**Solution**: 
- Verify ILogService is injected
- Check database connection string
- Ensure migration was applied

### Issue: IP Address showing as "Unknown"
**Solution**: 
- Check HttpContext initialization
- If behind proxy, verify X-Forwarded-For header is set

---

## 📞 Support

For issues or questions:
1. Check the full documentation: `LOGGING_AND_LOGIN_IMPLEMENTATION.md`
2. Review error logs in database
3. Check Application Insights (if configured)
4. Contact system administrator

---

*Quick Start Guide - Last Updated: 2024*
