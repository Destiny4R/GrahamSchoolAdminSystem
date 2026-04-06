# Employee Management - Quick Start Guide

## Accessing Employee Management

1. **Navigate to**: `/admin/employees/`
2. **Required Role**: Admin or HR Manager (controlled by authorization middleware)
3. **Permission**: Requires appropriate role assignments for create/update/delete

## Basic Operations

### Adding an Employee

1. Click **"Add New Employee"** button (top-right of page)
2. Fill in the modal form:
   - **First Name**: Employee's first name (required)
   - **Last Name**: Employee's last name (required)
   - **Email**: Valid email address (required, must be unique)
   - **Phone**: 10+ digit phone number (required, format: digits only)
   - **Department**: Employee's department (optional)
   - **Address**: Physical address (optional)
   - **User Account**: Select corresponding user account (required)
3. Click **"Create Employee"**
4. Success message appears at top of page
5. New employee appears in DataTable

**Behind the Scenes**:
- System creates EmployeesTable record
- Links to ApplicationUser account
- Logs action to LogsTable with your username and IP
- Creates indexed entry for quick search

### Viewing Employee List

**Page loads automatically with**:
- All current employees (latest first)
- Employee name, phone, email, department
- Current positions assigned (as blue badges)
- Active/Inactive status
- Quick action buttons

**Search/Filter**:
1. Type in search box at top-right
2. Results filter in real-time by:
   - Employee name
   - Email address
   - Phone number

**Sort**:
1. Click any column header to sort ascending/descending
2. Default sort: Name A-Z

**Paginate**:
1. Select rows per page: 5, 10, 25, 50, or All
2. Use Previous/Next buttons or jump to specific page
3. View record count at bottom

### Editing an Employee

1. Find employee in DataTable
2. Click **"Edit"** button (pencil icon)
3. Edit modal opens with current information
4. Update any fields:
   - Name, email, phone
   - Department, address
   - Associated user account
5. Click **"Update Employee"**
6. Confirmation message appears
7. DataTable refreshes

**What Gets Logged**:
- "Employee updated successfully"
- All changed fields in details
- Your user ID and IP address
- Exact timestamp

**Validation**:
- Email must be unique across system
- Phone format: 10+ digits minimum
- Cannot change to email already in use

### Assigning Positions to Employees

1. Find employee in DataTable
2. Click **"Assign Position"** button (star icon)
3. Modal opens showing:
   - Employee name
   - Current positions (read-only list)
4. Select **Position** from dropdown
5. Click **"Assign Position"**
6. Success message confirms assignment

**Important**:
- Positions carry roles and permissions
- Assigning position grants those roles
- Employee can have multiple positions
- System prevents duplicate assignments

**Example Workflow**:
```
Employee: John Smith
Current Positions: None

Select Position: "Mathematics Teacher"
↓
Click "Assign Position"
↓
Result: John Smith now has "Mathematics Teacher" position with:
  - Teaching permissions
  - Grade entry permissions
  - Class management permissions
```

### Deleting an Employee

1. Find employee in DataTable
2. Click **"Delete"** button (trash icon)
3. Confirm deletion
4. System automatically:
   - Removes all position assignments
   - Revokes related permissions
   - Deletes employee record
5. Confirmation message appears
6. Employee removed from DataTable

**Warning**: This action is permanent and cannot be undone.

## DataTable Features

### Responsive Design
- **Desktop**: Full table with all columns visible
- **Tablet**: Some columns hidden, actions in dropdown
- **Mobile**: Vertical stacking, essential info visible

### Search Tips
```
Search For          Finds
================    ======
"john"              John Smith, john@school.com
"john.smith"        john.smith@school.com
"9876543210"        Employee with that phone
"english"           Employees in English department
```

### Export Data
Use browser's built-in tools:
- **Print**: Ctrl+P or Cmd+P
- **Export**: DataTable has export button (if enabled)
- **Copy**: Select and copy to spreadsheet

## Common Workflows

### Onboarding New Employee

1. **Create Employee Record**
   - Add employee with basic info
   - Link to their user account
   - Verify email and phone

2. **Assign Initial Position**
   - Click "Assign Position"
   - Select primary role (e.g., "Teacher")
   - Confirm assignment

3. **Verify Permissions**
   - Go to position management
   - Verify position has correct roles
   - Check roles have required permissions

4. **Test Login**
   - Employee logs in
   - Verify they see appropriate permissions
   - Check dashboard shows their role

### Bulk Position Assignment (Manual Process)

1. Open Employee Management page
2. For each employee:
   - Click "Assign Position"
   - Select position
   - Click assign
3. Verify in position detail page

**Future**: Automated bulk assignment coming

### Department Transfer

1. Find employee in list
2. Click "Edit"
3. Change department
4. Click "Update Employee"
5. If position changes:
   - Remove old position (if needed)
   - Assign new position
   - Verify permissions updated

## Error Messages & Solutions

| Message | Cause | Fix |
|---------|-------|-----|
| "Employee with this email already exists" | Email duplicate | Use different email or edit existing employee |
| "Phone must contain at least 10 digits" | Invalid phone format | Enter 10+ digits only (no dashes/spaces) |
| "Email is already in use" | Update conflict | Choose different email |
| "Employee not found" | Employee deleted | Refresh page and try again |
| "Position not found" | Position deleted | Verify position still exists |
| "Employee already assigned to this position" | Duplicate assignment | Check current positions first |

## Logging & Audit Trail

**Every action is logged including**:
- Who did it (your username)
- What they did (Create/Update/Delete/Assign)
- When (exact timestamp)
- Where from (your IP address)
- Details (what changed)

**View Logs**:
1. Go to Admin > Activity Logs
2. Filter by:
   - Date range
   - Action type
   - Employee name
   - User who made change

**Example Log Entry**:
```
Time: 2025-03-29 10:30:45 UTC
Action: Create
Entity: Employee
User: admin@school.com
IP: 192.168.1.100
Details: Employee 'John Doe' created
  Email: john@school.com
  Phone: 1234567890
  Department: Mathematics
```

## Permissions & Access Control

### By Role

**Admin**: Full access to all operations
- Create, read, update, delete employees
- Assign/remove positions
- View all logs

**HR Manager**: Employee management operations
- Create, read, update employees
- Assign positions
- View logs

**Department Head**: Read-only or limited access
- View employees in department
- Cannot modify records

**Teachers**: View only
- See employee directory
- Cannot modify

## Performance Tips

### For Large Employee Lists (1000+)
1. Use search to narrow results
2. Increase rows per page (50+)
3. Sort by specific column to focus

### Faster Searching
- Start with last name
- Use department if known
- Use partial email

### Position Assignment
- Filter positions by department first
- Multi-assign if role supports it
- Use position templates if available

## Keyboard Shortcuts

**Upcoming Feature**: These shortcuts will be available
- **Ctrl+E**: Quick employee search
- **Ctrl+N**: New employee
- **ESC**: Close modal
- **Tab**: Move between form fields

## Mobile Usage

### On Phone
1. Tap employee name to expand details
2. Tap action buttons (edit/assign/delete)
3. Modals adapt to screen size
4. Scroll horizontally for more columns

### On Tablet
1. Full DataTable visible
2. All features functional
3. Touch-optimized buttons

## FAQ

**Q: Can I undo a deletion?**
A: No, deletions are permanent. Be careful with delete button.

**Q: How do I change an employee's email?**
A: Edit employee record and update email field (must be unique).

**Q: What happens when I delete an employee?**
A: All position assignments are removed, permissions revoked, record deleted.

**Q: Can multiple employees have the same position?**
A: Yes, many employees can hold the same position (e.g., multiple teachers).

**Q: How do permissions work?**
A: Permissions come from positions assigned to employees. Assigning a position grants all its permissions.

**Q: Can I bulk import employees?**
A: Not yet. Future enhancement planned. Currently: manual creation.

**Q: Where are changes logged?**
A: All changes logged to Activity Logs (Admin > Activity Logs) with full audit trail.

**Q: Can I export the employee list?**
A: Yes, use browser print function or copy from DataTable to spreadsheet.

**Q: How do I filter by position?**
A: Position filter coming soon. Currently: search by position name in employee details.

## Getting Help

**For Issues**:
1. Check error message at top of page
2. Look up error message in this guide
3. Check LogsTable for detailed error info
4. Contact system administrator

**For Features**:
1. Document the feature request
2. Submit to system development team
3. Include use case and expected benefit

**For Training**:
1. Refer to this guide
2. Video tutorials available (future)
3. Contact IT support

## Best Practices

### Before Creating Employee
- ✅ Verify user account exists
- ✅ Confirm unique email
- ✅ Get correct phone number
- ✅ Know target department

### After Creating Employee
- ✅ Assign appropriate position
- ✅ Verify permissions in logs
- ✅ Test employee can log in
- ✅ Confirm they see correct menu

### When Deleting Employee
- ✅ Verify employee record is duplicate
- ✅ Backup important data first
- ✅ Notify affected users
- ✅ Revoke any external access
- ✅ Review audit log afterward

### Security
- ✅ Never share passwords
- ✅ Verify email before assignment
- ✅ Use strong position-based roles
- ✅ Review permission changes in logs
- ✅ Audit new employees monthly

## System Integration

### Position Permissions Flow
```
Employee Created
    ↓
Position Assigned
    ↓
Position Roles Loaded
    ↓
Role Permissions Granted
    ↓
Employee Can Access Features
```

### Login After Changes
- New employees can login after:
  - Account created
  - Position assigned
  - Permissions propagated (immediate)

### Position Changes
- Permission changes apply:
  - When position assigned/removed
  - At next login (cached in claims)
  - Immediate for new role checks

## Summary

**Quick Reference**:
- **Create**: Click "Add New Employee" button
- **Edit**: Click "Edit" button next to employee
- **Position**: Click "Assign Position" button
- **Delete**: Click "Delete" button (permanent)
- **Search**: Use search box (real-time filtering)
- **Sort**: Click column header
- **Export**: Browser print/copy functions

**Remember**:
- All actions logged for audit trail
- Changes visible to administrators
- Help documentation always available
- Contact support for issues

The employee management system is ready for production use with full auditing, logging, and security integration.
