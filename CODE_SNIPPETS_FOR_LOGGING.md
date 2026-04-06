# Code Snippets for Integration

Ready-to-use code snippets for adding logging to your page handlers and actions.

---

## 🔧 Copy-Paste Ready Snippets

### 1. Add to Page Model Class (Top of File)

```csharp
// Add these using statements
using System.Security.Claims;
using GrahamSchoolAdminSystemAccess.IServiceRepo;
```

---

### 2. Inject Required Services in Constructor

```csharp
private readonly IUnitOfWork _unitOfWork;
private readonly ILogger<YourPageModel> _logger;

public YourPageModel(IUnitOfWork unitOfWork, ILogger<YourPageModel> logger)
{
    _unitOfWork = unitOfWork;
    _logger = logger;
}
```

---

### 3. Add Helper Method to Page Model

```csharp
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

---

### 4. Log Create Action

```csharp
public async Task<IActionResult> OnPostAddAsync()
{
    if (!ModelState.IsValid)
        return Page();

    // Your creation logic here
    var result = await YourService.CreateAsync(YourModel);

    if (result.Succeeded)
    {
        // LOG THE ACTION - COPY THIS
        try
        {
            await _unitOfWork.LogService.LogUserActionAsync(
                userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                userName: User.Identity?.Name ?? "Unknown",
                action: "Create",
                entityType: "YourEntity",          // Change this
                entityId: YourModel.Id.ToString(),
                message: $"YourEntity '{YourModel.Name}' created successfully",
                ipAddress: GetClientIpAddress(),
                details: $"Additional info about what was created"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging create action");
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToPage();
    }

    TempData["ErrorMessage"] = result.Message;
    return RedirectToPage();
}
```

---

### 5. Log Update Action

```csharp
public async Task<IActionResult> OnPostUpdateAsync()
{
    if (!ModelState.IsValid)
        return Page();

    // Store old data for comparison if needed
    var oldData = await YourService.GetByIdAsync(YourModel.Id);

    var result = await YourService.UpdateAsync(YourModel);

    if (result.Succeeded)
    {
        // LOG THE ACTION - COPY THIS
        try
        {
            await _unitOfWork.LogService.LogUserActionAsync(
                userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                userName: User.Identity?.Name ?? "Unknown",
                action: "Update",
                entityType: "YourEntity",          // Change this
                entityId: YourModel.Id.ToString(),
                message: $"YourEntity '{YourModel.Name}' updated successfully",
                ipAddress: GetClientIpAddress(),
                details: $"Previous Name: {oldData?.Name}, New Name: {YourModel.Name}"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging update action");
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToPage();
    }

    TempData["ErrorMessage"] = result.Message;
    return RedirectToPage();
}
```

---

### 6. Log Delete Action

```csharp
public async Task<IActionResult> OnPostDeleteAsync(int id)
{
    // Get entity before deletion for logging
    var entity = await YourService.GetByIdAsync(id);

    var result = await YourService.DeleteAsync(id);

    if (result.Succeeded)
    {
        // LOG THE ACTION - COPY THIS
        try
        {
            await _unitOfWork.LogService.LogUserActionAsync(
                userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                userName: User.Identity?.Name ?? "Unknown",
                action: "Delete",
                entityType: "YourEntity",          // Change this
                entityId: id.ToString(),
                message: $"YourEntity deleted successfully",
                ipAddress: GetClientIpAddress(),
                details: $"Deleted: {entity?.Name}, Created by: {entity?.CreatedBy}"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging delete action");
        }

        TempData["SuccessMessage"] = result.Message;
    }
    else
    {
        TempData["ErrorMessage"] = result.Message;
    }

    return RedirectToPage();
}
```

---

### 7. Log Custom Action

```csharp
public async Task<IActionResult> OnPostCustomActionAsync(int id)
{
    try
    {
        var result = await YourService.PerformActionAsync(id);

        if (result.Succeeded)
        {
            // LOG THE ACTION - COPY THIS
            try
            {
                await _unitOfWork.LogService.LogUserActionAsync(
                    userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    userName: User.Identity?.Name ?? "Unknown",
                    action: "CustomAction",     // Change this
                    entityType: "YourEntity",
                    entityId: id.ToString(),
                    message: $"Custom action performed on YourEntity {id}",
                    ipAddress: GetClientIpAddress(),
                    details: $"Action result: {result.Message}"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging custom action");
            }

            return new JsonResult(new { success = true, message = result.Message });
        }

        return new JsonResult(new { success = false, message = result.Message });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error performing custom action");

        // LOG ERROR - COPY THIS
        try
        {
            await _unitOfWork.LogService.LogErrorAsync(
                subject: "Custom Action Error",
                message: $"Error performing action on YourEntity {id}",
                details: ex.Message + " " + ex.StackTrace,
                userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                ipAddress: GetClientIpAddress()
            );
        }
        catch { }

        return new JsonResult(new { success = false, message = "An error occurred" });
    }
}
```

---

### 8. Log in Controller Action

```csharp
[HttpPost]
public async Task<IActionResult> Create(YourViewModel model)
{
    if (!ModelState.IsValid)
        return BadRequest(ModelState);

    try
    {
        var result = await _service.CreateAsync(model);

        if (result.Succeeded)
        {
            // LOG THE ACTION - COPY THIS
            try
            {
                await _unitOfWork.LogService.LogUserActionAsync(
                    userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    userName: User.Identity?.Name ?? "Unknown",
                    action: "Create",
                    entityType: "YourEntity",
                    entityId: model.Id.ToString(),
                    message: $"Created successfully via API",
                    ipAddress: Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                    details: JsonConvert.SerializeObject(model)
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging create action");
            }

            return Ok(new { success = true, message = result.Message, data = result.Data });
        }

        return BadRequest(new { success = false, message = result.Message });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error in Create action");

        // LOG ERROR - COPY THIS
        try
        {
            await _unitOfWork.LogService.LogErrorAsync(
                subject: "Controller Create Error",
                message: $"Error creating entity",
                details: ex.Message,
                userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                ipAddress: Request.HttpContext.Connection.RemoteIpAddress?.ToString()
            );
        }
        catch { }

        return StatusCode(500, new { success = false, message = "An error occurred" });
    }
}
```

---

### 9. Batch Logging Example

```csharp
public async Task<IActionResult> OnPostBulkActionAsync(List<int> ids, string action)
{
    if (!ids.Any())
    {
        TempData["ErrorMessage"] = "No items selected";
        return RedirectToPage();
    }

    var successCount = 0;
    var failureCount = 0;

    foreach (var id in ids)
    {
        try
        {
            var result = await YourService.ActionAsync(id, action);

            if (result.Succeeded)
            {
                successCount++;

                // LOG EACH ACTION - COPY THIS
                try
                {
                    await _unitOfWork.LogService.LogUserActionAsync(
                        userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                        userName: User.Identity?.Name ?? "Unknown",
                        action: action,
                        entityType: "YourEntity",
                        entityId: id.ToString(),
                        message: $"Bulk {action} performed",
                        ipAddress: GetClientIpAddress(),
                        details: $"Bulk operation - Success"
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error logging bulk action");
                }
            }
            else
            {
                failureCount++;
            }
        }
        catch (Exception ex)
        {
            failureCount++;
            _logger.LogError(ex, $"Error in bulk action for id {id}");
        }
    }

    TempData["SuccessMessage"] = $"Processed {successCount} items successfully";
    if (failureCount > 0)
        TempData["ErrorMessage"] = $"{failureCount} items failed";

    return RedirectToPage();
}
```

---

### 10. Conditional Logging

```csharp
public async Task<IActionResult> OnPostUpdateAsync()
{
    if (!ModelState.IsValid)
        return Page();

    var oldEntity = await YourService.GetByIdAsync(YourModel.Id);
    var result = await YourService.UpdateAsync(YourModel);

    if (result.Succeeded)
    {
        // Only log if something actually changed
        if (oldEntity.Name != YourModel.Name || 
            oldEntity.Status != YourModel.Status)
        {
            try
            {
                var changes = new List<string>();
                if (oldEntity.Name != YourModel.Name)
                    changes.Add($"Name: {oldEntity.Name} → {YourModel.Name}");
                if (oldEntity.Status != YourModel.Status)
                    changes.Add($"Status: {oldEntity.Status} → {YourModel.Status}");

                await _unitOfWork.LogService.LogUserActionAsync(
                    userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    userName: User.Identity?.Name ?? "Unknown",
                    action: "Update",
                    entityType: "YourEntity",
                    entityId: YourModel.Id.ToString(),
                    message: "Entity updated with changes",
                    ipAddress: GetClientIpAddress(),
                    details: string.Join("; ", changes)
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging update");
            }
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToPage();
    }

    TempData["ErrorMessage"] = result.Message;
    return RedirectToPage();
}
```

---

### 11. Query Logs Example

```csharp
public async Task<IActionResult> OnGetLogsAsync()
{
    try
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // Get user's recent activities
        var userLogs = await _unitOfWork.LogService.GetUserLogsAsync(
            userId: userId,
            pageNumber: 1,
            pageSize: 50
        );

        // Get all logs with search
        var allLogs = await _unitOfWork.LogService.GetLogsAsync(
            pageNumber: 1,
            pageSize: 50,
            searchTerm: "Create",
            logLevel: "INFO"
        );

        // Get entity history
        var entityLogs = await _unitOfWork.LogService.GetEntityLogsAsync(
            entityType: "Position",
            entityId: "123"
        );

        return new JsonResult(new { userLogs, allLogs, entityLogs });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving logs");
        return new JsonResult(new { error = "Error retrieving logs" });
    }
}
```

---

### 12. Position Management - Complete Example

```csharp
// In Pages/admin/positions/index.cshtml.cs

public async Task<IActionResult> OnPostAddPositionAsync()
{
    if (!ModelState.IsValid)
        return Page();

    var result = await _unitOfWork.UsersServices.CreatePositionAsync(PositionModel);

    if (result.Succeeded)
    {
        // LOG THE ACTION
        try
        {
            await _unitOfWork.LogService.LogUserActionAsync(
                userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                userName: User.Identity?.Name,
                action: "Create",
                entityType: "Position",
                entityId: PositionModel.Id.ToString(),
                message: $"Position '{PositionModel.Name}' created successfully",
                ipAddress: GetClientIpAddress(),
                details: $"Description: {PositionModel.Description}"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging create position");
        }

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
        try
        {
            await _unitOfWork.LogService.LogUserActionAsync(
                userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                userName: User.Identity?.Name,
                action: "Update",
                entityType: "Position",
                entityId: PositionModel.Id.ToString(),
                message: $"Position '{PositionModel.Name}' updated successfully",
                ipAddress: GetClientIpAddress(),
                details: $"Name: {PositionModel.Name}, Description: {PositionModel.Description}"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging update position");
        }

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
        try
        {
            await _unitOfWork.LogService.LogUserActionAsync(
                userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                userName: User.Identity?.Name,
                action: "Delete",
                entityType: "Position",
                entityId: positionId.ToString(),
                message: "Position deleted successfully",
                ipAddress: GetClientIpAddress(),
                details: $"Deleted Position: {position?.Name}, EmployeeCount: {position?.EmployeeCount}"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging delete position");
        }

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
        try
        {
            await _unitOfWork.LogService.LogUserActionAsync(
                userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                userName: User.Identity?.Name,
                action: "Update",
                entityType: "PositionRole",
                entityId: AssignRoleModel.PositionId.ToString(),
                message: "Roles assigned to position",
                ipAddress: GetClientIpAddress(),
                details: $"Assigned {AssignRoleModel.SelectedRoleIds.Count} roles to position"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging assign roles");
        }

        TempData["SuccessMessage"] = result.Message;
    }
    else
    {
        TempData["ErrorMessage"] = result.Message;
    }

    return RedirectToPage();
}

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

---

## 🚀 Quick Integration Steps

1. **Copy the using statements** to the top of your file
2. **Copy the injection code** to your constructor
3. **Copy the helper method** `GetClientIpAddress()`
4. **Choose appropriate snippet** for your action (Create/Update/Delete)
5. **Adjust entity names** (YourEntity, YourModel, etc.)
6. **Test** to verify logging works

---

## 📝 Variables to Replace

When copying snippets, replace these with your values:

| Placeholder | Replace With | Example |
|-------------|--------------|---------|
| `YourEntity` | Entity type | `Position` |
| `YourPageModel` | Your page class | `IndexModel` |
| `YourModel` | Your ViewModel | `PositionModel` |
| `YourService` | Your service | `_unitOfWork.UsersServices` |
| `OnPostAddAsync` | Your handler name | `OnPostAddPositionAsync` |

---

## ✅ Testing Each Snippet

After copying a snippet, test it:

1. Perform the action in the UI
2. Check the database: `SELECT * FROM LogsTables ORDER BY CreatedDate DESC`
3. Verify: UserId, UserName, Action, EntityType are populated
4. Check: IpAddress and details contain expected values

---

*Code Snippets Ready - Copy & Paste Integration*
*Last Updated: 2024*
