# Logging & Auditing System Implementation & Login Page

## Overview

This document describes the comprehensive logging and auditing system implemented across the Graham School Admin System, along with the modern, professional login page.

---

## 📊 Logging & Auditing System

### 1. Enhanced LogsTable Model

The `LogsTable` model has been expanded from basic logging to a comprehensive auditing solution with the following properties:

```csharp
public class LogsTable
{
    public int Id { get; set; }
    public string Subject { get; set; }
    public string Message { get; set; }
    public DateTime CreatedDate { get; set; }
    public string LogLevel { get; set; }        // INFO, WARNING, ERROR, CRITICAL, DEBUG
    public string UserId { get; set; }          // Who performed the action
    public string UserName { get; set; }
    public string Action { get; set; }          // Create, Update, Delete, Read, Login
    public string EntityType { get; set; }      // Position, Role, User, etc.
    public string EntityId { get; set; }        // ID of affected entity
    public string IpAddress { get; set; }       // Client IP address
    public string Details { get; set; }         // Additional data in JSON format
    public int? StatusCode { get; set; }        // HTTP response status
}
```

### 2. Enhanced ILogService Interface

The interface now includes multiple logging methods for different scenarios:

#### Basic Logging
```csharp
void Log(string subject, string message);
Task LogAsync(string subject, string message);
```

#### Comprehensive Action Logging
```csharp
Task LogActionAsync(
    string userId,
    string userName,
    string action,
    string entityType,
    string entityId,
    string ipAddress,
    string subject,
    string message,
    string logLevel = "INFO",
    string details = null,
    int? statusCode = null
);
```

#### User Action Logging
```csharp
Task LogUserActionAsync(
    string userId,
    string userName,
    string action,
    string entityType,
    string entityId,
    string message,
    string ipAddress,
    string details = null
);
```

#### Authentication Logging
```csharp
Task LogAuthenticationAsync(
    string userId,
    string userName,
    string action,
    string ipAddress,
    bool success,
    string message = null,
    string details = null
);
```

#### Error Logging
```csharp
Task LogErrorAsync(
    string subject,
    string message,
    string details = null,
    string userId = null,
    string ipAddress = null
);
```

#### Query Methods
```csharp
// Get logs with filtering
Task<List<dynamic>> GetLogsAsync(
    int pageNumber = 1,
    int pageSize = 50,
    string searchTerm = null,
    string logLevel = null,
    DateTime? startDate = null,
    DateTime? endDate = null
);

// Get user activity logs
Task<List<dynamic>> GetUserLogsAsync(string userId, int pageNumber = 1, int pageSize = 50);

// Get entity audit trail
Task<List<dynamic>> GetEntityLogsAsync(string entityType, string entityId);
```

### 3. LogService Implementation

The `LogService` provides implementations for all interface methods:

#### Features:
- **Asynchronous operations**: All methods are async for better performance
- **Error handling**: Catches exceptions to prevent logging errors from breaking the application
- **Filtering & Pagination**: Support for advanced log queries
- **Context capture**: Records user, IP, action type, and entity information
- **Debug logging**: Uses Debug.WriteLine for system diagnostics

### 4. UnitOfWork Integration

The `IUnitOfWork` and `UnitOfWork` classes have been updated to include `ILogService`:

```csharp
public interface IUnitOfWork
{
    IFinanceServices FinanceServices { get; }
    IUsersServices UsersServices { get; }
    ISystemActivitiesServices SystemActivities { get; }
    ILogService LogService { get; }  // NEW
}
```

### 5. Audit Logging Filter

A custom `AuditLoggingFilter` has been created to automatically log all user actions:

**Location**: `GrahamSchoolAdminSystemWeb/Helpers/AuditLoggingFilter.cs`

#### Features:
- **Automatic action logging**: Logs all POST, PUT, DELETE, PATCH requests
- **Excludes**: GET requests and authentication actions (logged separately)
- **IP tracking**: Captures client IP address with proxy support
- **User context**: Records user ID and username
- **Error handling**: Gracefully handles logging errors
- **Action details**: Captures controller, action, method, and parameters

#### Usage in Program.cs:
```csharp
builder.Services.AddScoped<AuditLoggingFilter>();
```

The filter is automatically applied to all actions.

---

## 🔐 Login System Implementation

### 1. Login Page UI (`login.cshtml`)

#### Design Features:
- **Modern Gradient Background**: Dynamic purple gradient with animated floating elements
- **Responsive Layout**: Optimized for desktop, tablet, and mobile devices
- **Professional Card Design**: Centered login card with smooth animations
- **User-Friendly Form**: Email and password inputs with validation
- **Security Indicators**: Password visibility toggle, remember me option
- **Error Handling**: Clear error message display
- **Modal Support**: Forgot password functionality
- **Loading States**: Visual feedback during login attempt

#### Key Sections:
```html
<!-- Animated header with school logo -->
<div class="login-header">
    <div class="login-logo"><i class="fas fa-graduation-cap"></i></div>
    <h1>Graham School</h1>
    <p>Administration System</p>
</div>

<!-- Login form -->
<form method="post" asp-page-handler="Login">
    <input type="email" class="form-control" ... />
    <input type="password" class="form-control" ... />
    <div class="remember-forgot">
        <input type="checkbox" ... /> Remember me
        <a href="#forgotPasswordModal">Forgot Password?</a>
    </div>
    <button type="submit" class="btn-login">Login</button>
</form>

<!-- Demo info and help section -->
<div class="info-message">
    Demo credentials and help information
</div>
```

#### Styling Highlights:
- **Gradient backgrounds**: Applied to header, buttons, and interactive elements
- **Smooth animations**: Slide-up card entrance, button hover effects, floating background
- **Modern colors**: Purple gradients (#667eea to #764ba2)
- **Responsive breakpoints**: Desktop (>768px), Tablet (≤768px), Mobile (≤480px)
- **Accessibility**: Proper contrast, focus states, and interactive feedback

### 2. Login Page Model (`login.cshtml.cs`)

#### Properties:
```csharp
[BindProperty]
[EmailAddress]
public string Email { get; set; }

[BindProperty]
[DataType(DataType.Password)]
public string Password { get; set; }

[BindProperty]
public bool RememberMe { get; set; }

public string ErrorMessage { get; set; }
public string SuccessMessage { get; set; }
```

#### Handler: OnPostLoginAsync()

This is the main login handler with comprehensive functionality:

##### Workflow:
1. **Validation**: Checks ModelState validity
2. **User lookup**: Finds user by email
3. **Account status**: Checks if account is locked
4. **Password verification**: Validates password
5. **Login attempt**: Calls SignInManager
6. **Logging**: Logs all outcomes (success/failure)
7. **Redirect**: Routes to dashboard on success

##### Logging Points:

**Successful Login:**
```csharp
await _logService.LogAuthenticationAsync(
    userId: user.Id,
    userName: user.UserName,
    action: "Login",
    ipAddress: ipAddress,
    success: true,
    message: $"User {user.UserName} logged in successfully"
);
```

**Failed Login - User Not Found:**
```csharp
await _logService.LogAuthenticationAsync(
    userId: null,
    userName: Email,
    action: "Login",
    ipAddress: ipAddress,
    success: false,
    message: "Login attempt failed - user not found"
);
```

**Account Locked:**
```csharp
await _logService.LogAuthenticationAsync(
    userId: user.Id,
    userName: user.UserName,
    action: "Login",
    ipAddress: ipAddress,
    success: false,
    message: "Account locked due to multiple failed attempts"
);
```

**Invalid Password:**
```csharp
await _logService.LogAuthenticationAsync(
    userId: user.Id,
    userName: user.UserName,
    action: "Login",
    ipAddress: ipAddress,
    success: false,
    message: "Invalid password"
);
```

**Error During Login:**
```csharp
await _logService.LogErrorAsync(
    subject: "Login Error",
    message: $"An error occurred during login attempt",
    details: ex.Message,
    ipAddress: GetClientIpAddress()
);
```

#### Helper Methods:
```csharp
private string GetClientIpAddress()
{
    // Retrieves client IP
    // Checks for X-Forwarded-For header (proxy support)
    // Returns IP address or "Unknown"
}
```

---

## 📝 Logging Usage Examples

### 1. Position Management Actions

When a position is created, updated, or deleted, log it:

```csharp
await _unitOfWork.LogService.LogUserActionAsync(
    userId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
    userName: User.Identity.Name,
    action: "Create",
    entityType: "Position",
    entityId: position.Id.ToString(),
    message: $"Position '{position.Name}' created",
    ipAddress: GetClientIpAddress(),
    details: $"Description: {position.Description}"
);
```

### 2. Role Assignment

When roles are assigned to a position:

```csharp
await _unitOfWork.LogService.LogUserActionAsync(
    userId: userId,
    userName: userName,
    action: "Update",
    entityType: "PositionRole",
    entityId: positionId.ToString(),
    message: $"Roles assigned to position",
    ipAddress: ipAddress,
    details: $"Assigned roles: {string.Join(", ", roleIds)}"
);
```

### 3. User Operations

When managing users (create, update, delete):

```csharp
await _unitOfWork.LogService.LogUserActionAsync(
    userId: performedBy,
    userName: performedByUsername,
    action: "Delete",
    entityType: "User",
    entityId: user.Id,
    message: $"User '{user.UserName}' deleted",
    ipAddress: clientIp,
    details: $"Email: {user.Email}, Positions: {string.Join(", ", userPositions)}"
);
```

### 4. Finance Operations

```csharp
await _unitOfWork.LogService.LogUserActionAsync(
    userId: userId,
    userName: userName,
    action: "Create",
    entityType: "FeeSetup",
    entityId: feeSetup.Id.ToString(),
    message: $"Fee setup created for class '{feeSetup.ClassName}'",
    ipAddress: ipAddress,
    details: $"Amount: {feeSetup.Amount}, Term: {feeSetup.Term}"
);
```

---

## 🔄 Integration with Program.cs

### Service Registration:
```csharp
// Core services
builder.Services.AddScoped<IFinanceServices, FinanceServices>();
builder.Services.AddScoped<IUsersServices, UsersServices>();
builder.Services.AddScoped<ILogService, LogService>();        // NEW
builder.Services.AddScoped<ISystemActivitiesServices, SystemActivitiesServices>();

// Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// HTTP context for IP tracking
builder.Services.AddHttpContextAccessor();                     // NEW

// Audit logging filter
builder.Services.AddScoped<AuditLoggingFilter>();             // NEW
```

### Authentication Configuration:
```csharp
builder.Services.ConfigureApplicationCookie(a =>
{
    a.LoginPath = $"/Account/Login";
    a.LogoutPath = $"/Account/Logout";
    a.AccessDeniedPath = $"/Account/AccessDenied";
    a.ExpireTimeSpan = TimeSpan.FromDays(1);
    a.SlidingExpiration = true;
});
```

---

## 📊 Database Schema

### Logs Table Columns:
| Column | Type | Description |
|--------|------|-------------|
| Id | int | Primary key |
| Subject | varchar(54) | Log subject |
| Message | varchar(2000) | Detailed message |
| CreatedDate | datetime | Timestamp |
| LogLevel | varchar(20) | INFO, WARNING, ERROR, etc. |
| UserId | varchar(max) | User who performed action |
| UserName | varchar(256) | Username |
| Action | varchar(50) | Create, Update, Delete, Read, Login |
| EntityType | varchar(100) | Type of entity |
| EntityId | varchar(max) | ID of entity |
| IpAddress | varchar(45) | Client IP address |
| Details | varchar(4000) | Additional JSON data |
| StatusCode | int | HTTP status code |

---

## 🔐 Security Features

### 1. Account Lockout
- Locks account after 3 failed login attempts
- Prevents brute force attacks
- Clear messaging to users

### 2. IP Tracking
- Logs all IP addresses
- Detects unusual access patterns
- Supports proxy environments (X-Forwarded-For)

### 3. Session Management
- 24-hour session timeout
- Sliding expiration enabled
- Remember-me functionality (optional)

### 4. Comprehensive Audit Trail
- Every action is logged with timestamp
- User identity captured
- Entity changes tracked
- Full context preserved

---

## 📱 Responsive Design

### Desktop (>768px):
- Full-width login form with max-width constraint
- Side-by-side remember/forgot layout
- Full animation effects

### Tablet (≤768px):
- Adjusted padding and margins
- Stack remember/forgot vertically
- Reduced animation complexity

### Mobile (≤480px):
- Compact layout with minimal padding
- Touch-friendly button sizes
- Simplified visual effects

---

## 🎨 Login Page Features

### Visual Elements:
- **Gradient Background**: Purple gradient with animated floating shapes
- **Card-based Design**: Clean, modern card layout
- **Icons**: Font Awesome icons for visual clarity
- **Color Scheme**: 
  - Primary: #667eea (Blue)
  - Secondary: #764ba2 (Purple)
  - Accent: #f5576c (Red)

### Interactive Features:
- **Password Toggle**: Show/hide password functionality
- **Form Validation**: Client and server-side validation
- **Loading State**: Visual feedback during submission
- **Error Messages**: Clear, helpful error messaging
- **Forgot Password**: Modal for password reset flow
- **Demo Info**: Help information for new users

### Animations:
- **Slide-up**: Card entrance animation
- **Bounce**: Logo animation on hover
- **Float**: Background shapes floating effect
- **Fade**: Smooth fade transitions
- **Slide-down**: Error messages slide-down effect

---

## 🚀 Migration & Database Update

To apply the enhanced LogsTable schema to your database, you'll need to create a migration:

```bash
dotnet ef migrations add EnhancedLogging -o Data/Migrations
dotnet ef database update
```

The migration will add the new columns:
- LogLevel
- UserId
- UserName
- Action
- EntityType
- EntityId
- IpAddress
- Details
- StatusCode

---

## 📚 Best Practices

### 1. Always Log User Actions
```csharp
// DO: Log important actions
await _unitOfWork.LogService.LogUserActionAsync(...);

// DON'T: Forget to log
// Missing logging makes audit trails incomplete
```

### 2. Include Context in Logs
```csharp
// DO: Include relevant details
details: $"Position ID: {position.Id}, Previous State: {JsonConvert.SerializeObject(oldPosition)}"

// DON'T: Leave details empty
```

### 3. Use Appropriate Log Levels
```csharp
// INFO: Normal operations
// WARNING: Potential issues (failed login, locked account)
// ERROR: Actual errors (exceptions, invalid states)
// CRITICAL: System failures
```

### 4. Get Client IP Properly
```csharp
private string GetClientIpAddress()
{
    var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
    if (HttpContext.Request.Headers.ContainsKey("X-Forwarded-For"))
    {
        ipAddress = HttpContext.Request.Headers["X-Forwarded-For"]
            .ToString().Split(',').FirstOrDefault()?.Trim();
    }
    return ipAddress ?? "Unknown";
}
```

---

## 🔍 Query Examples

### Get all failed login attempts in last 7 days:
```csharp
var failedLogins = await _logService.GetLogsAsync(
    searchTerm: "Login",
    logLevel: "WARNING",
    startDate: DateTime.UtcNow.AddDays(-7)
);
```

### Get all activities by a specific user:
```csharp
var userActivities = await _logService.GetUserLogsAsync(userId, pageNumber: 1, pageSize: 50);
```

### Get audit trail for a position:
```csharp
var positionHistory = await _logService.GetEntityLogsAsync("Position", positionId.ToString());
```

---

## ✅ Checklist for Implementation

- [x] Enhanced LogsTable model with additional fields
- [x] Comprehensive ILogService interface
- [x] Full LogService implementation
- [x] UnitOfWork updated with LogService
- [x] Professional login page UI
- [x] Login page model with auditing
- [x] Audit logging filter created
- [x] Program.cs updated with service registrations
- [x] Documentation completed

---

## 📞 Support & Troubleshooting

### Issue: Logs not being saved
**Solution**: Check that ILogService is properly injected and the database connection is active

### Issue: IP address showing as Unknown
**Solution**: Verify HttpContext is properly initialized; check proxy headers if behind reverse proxy

### Issue: Login page not loading styles
**Solution**: Ensure Bootstrap and Font Awesome CDN links are accessible; check browser console for errors

### Issue: Authentication always failing
**Solution**: Verify user exists in database; check database connection; review error logs

---

## 🎓 Next Steps

1. **Run Database Migration**: Apply the enhanced LogsTable schema
2. **Integrate Logging**: Add logging calls to all action handlers
3. **Create Admin Dashboard**: Show logs in admin panel for monitoring
4. **Set Up Alerts**: Create alerts for suspicious activities
5. **Regular Audits**: Review logs periodically for security insights
6. **Backup Strategy**: Regularly backup log data for compliance

---

*Document Last Updated: 2024*
*System Version: Graham School Admin System v1.0*
