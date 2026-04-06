# Access Denied Logging - Examples & Test Scenarios

## Scenario 1: Student Tries to Access Admin Panel

### User Details
```
Username: Ahmed_Student@school.com
User ID: 550e8400-e29b-41d4-a716-446655440000
Roles: Student
Email: ahmed_student@school.com
```

### Access Attempt
```
Attempted URL: /admin/fees-setup
Required Permission: Admin.Manage
```

### LogsTable Entry
```
┌─ Access Denied Log Entry ────────────────────────────────┐
│                                                           │
│ Subject:        Access Denied                            │
│ Action:         AccessDenied                             │
│ EntityType:     Authorization                            │
│ EntityId:       A1B2C3D4E5F6                             │
│                                                           │
│ UserId:         550e8400-e29b-41d4-a716-446655440000    │
│ UserName:       Ahmed_Student@school.com                 │
│ Email:          ahmed_student@school.com                 │
│                                                           │
│ Message:        Access Denied for user: Ahmed_Student@   │
│                 school.com attempting to access /admin/   │
│                 fees-setup                               │
│                                                           │
│ IpAddress:      192.168.1.45                             │
│ LogLevel:       INFO                                     │
│ CreatedDate:    2026-03-29 10:15:30                      │
│                                                           │
│ Details:                                                 │
│ ───────────────────────────────────────────────────────  │
│ User ID: 550e8400-e29b-41d4-a716-446655440000           │
│ Email: ahmed_student@school.com                          │
│ Roles: Student                                           │
│ Attempted Resource: /admin/fees-setup                    │
│ Required Permission: Admin.Manage                        │
│ HTTP Method: GET                                         │
│ Controller/Action: Unknown/Unknown                       │
│ User Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64)   │
│            AppleWebKit/537.36 (KHTML, like Gecko)       │
│            Chrome/91.0.4472.124 Safari/537.36           │
│ Referrer: http://localhost:5000/dashboard                │
│ Return URL: /admin/fees-setup                            │
│ Incident ID: A1B2C3D4E5F6                               │
│                                                          │
└──────────────────────────────────────────────────────────┘
```

---

## Scenario 2: Teacher with Partial Permissions Denied

### User Details
```
Username: Mrs_Smith@school.com
User ID: 650e8400-e29b-41d4-a716-446655440001
Roles: Teacher, ClassTeacher
Email: mrs.smith@school.com
```

### Access Attempt
```
Attempted URL: /admin/employee-management
Required Permission: Admin.Employee.Edit
```

### LogsTable Entry
```
┌─ Access Denied Log Entry ────────────────────────────────┐
│                                                           │
│ Subject:        Access Denied                            │
│ Action:         AccessDeniedWithRoles ⚠️                │
│ EntityType:     Authorization                            │
│ EntityId:       B2C3D4E5F6A7                             │
│                                                           │
│ UserId:         650e8400-e29b-41d4-a716-446655440001    │
│ UserName:       Mrs_Smith@school.com                     │
│ Email:          mrs.smith@school.com                     │
│                                                           │
│ Message:        User Mrs_Smith@school.com with roles     │
│                 [Teacher, ClassTeacher] was denied       │
│                 access to /admin/employee-management     │
│                                                           │
│ IpAddress:      192.168.1.78                             │
│ LogLevel:       WARNING ⚠️                               │
│ CreatedDate:    2026-03-29 11:42:15                      │
│                                                           │
│ Details:                                                 │
│ ───────────────────────────────────────────────────────  │
│ User ID: 650e8400-e29b-41d4-a716-446655440001           │
│ Email: mrs.smith@school.com                              │
│ Roles: Teacher, ClassTeacher                            │
│ Attempted Resource: /admin/employee-management           │
│ Required Permission: Admin.Employee.Edit                 │
│ HTTP Method: GET                                         │
│ Controller/Action: Admin/EmployeeManagement              │
│ User Agent: Mozilla/5.0 (Macintosh; Intel Mac OS X...)  │
│ Referrer: http://localhost:5000/dashboard/teacher        │
│ Return URL: /admin/employee-management                   │
│ Incident ID: B2C3D4E5F6A7                               │
│                                                          │
│ ⚠️ WARNING: User has roles but was denied access!        │
│    This may indicate a permission configuration issue.   │
│                                                          │
└──────────────────────────────────────────────────────────┘
```

---

## Scenario 3: Anonymous User Access Attempt

### User Details
```
Username: (Not logged in)
User ID: System
Roles: (None)
Email: Unknown
```

### Access Attempt
```
Attempted URL: /admin/dashboard
Required Permission: Admin
```

### LogsTable Entry
```
┌─ Access Denied Log Entry ────────────────────────────────┐
│                                                           │
│ Subject:        Access Denied                            │
│ Action:         AccessDenied                             │
│ EntityType:     Authorization                            │
│ EntityId:       C3D4E5F6A7B8                             │
│                                                           │
│ UserId:         System                                   │
│ UserName:       Anonymous                                │
│ Email:          Unknown                                  │
│                                                           │
│ Message:        Access Denied for user: Anonymous        │
│                 attempting to access /admin/dashboard    │
│                                                           │
│ IpAddress:      203.45.67.89                             │
│ LogLevel:       INFO                                     │
│ CreatedDate:    2026-03-29 12:05:45                      │
│                                                           │
│ Details:                                                 │
│ ───────────────────────────────────────────────────────  │
│ User ID: System                                          │
│ Email: Unknown                                           │
│ Roles:                                                   │
│ Attempted Resource: /admin/dashboard                     │
│ Required Permission:                                     │
│ HTTP Method: GET                                         │
│ Controller/Action: Admin/Dashboard                       │
│ User Agent: Mozilla/5.0 (Linux; Android 10; SM-G950F)   │
│ Referrer: https://external-site.com                     │
│ Return URL: /admin/dashboard                            │
│ Incident ID: C3D4E5F6A7B8                               │
│                                                          │
└──────────────────────────────────────────────────────────┘
```

---

## Scenario 4: Proxy/VPN Access (Multiple IPs)

### User Details
```
Username: Director@school.com
User ID: 750e8400-e29b-41d4-a716-446655440002
Roles: Director, Admin
Email: director@school.com
```

### Access Attempt
```
Attempted URL: /admin/reports
Actual IP: 203.45.67.89 (forwarded through proxy)
Original IP: 192.168.1.200 (internal network)
```

### LogsTable Entry
```
┌─ Access Denied Log Entry ────────────────────────────────┐
│                                                           │
│ Subject:        Access Denied                            │
│ Action:         AccessDenied                             │
│ EntityType:     Authorization                            │
│ EntityId:       D4E5F6A7B8C9                             │
│                                                           │
│ UserId:         750e8400-e29b-41d4-a716-446655440002    │
│ UserName:       Director@school.com                      │
│ Email:          director@school.com                      │
│                                                           │
│ Message:        Access Denied for user: Director@school. │
│                 com attempting to access /admin/reports  │
│                                                           │
│ IpAddress:      203.45.67.89 (captured from X-Forwarded  │
│                 -For header - proxy detected)            │
│ LogLevel:       INFO                                     │
│ CreatedDate:    2026-03-29 13:20:10                      │
│                                                           │
│ Details:                                                 │
│ ───────────────────────────────────────────────────────  │
│ User ID: 750e8400-e29b-41d4-a716-446655440002           │
│ Email: director@school.com                               │
│ Roles: Director, Admin                                   │
│ Attempted Resource: /admin/reports                       │
│ Required Permission: Admin.Reports                       │
│ HTTP Method: GET                                         │
│ Controller/Action: Admin/Reports                         │
│ User Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64)   │
│            EdgeBrowser/91.0                              │
│ Referrer: http://localhost:5000/admin/dashboard          │
│ Return URL: /admin/reports                               │
│ Incident ID: D4E5F6A7B8C9                               │
│                                                          │
│ ℹ️ Note: IP extracted from X-Forwarded-For header       │
│    Indicates proxy/VPN usage                             │
│                                                          │
└──────────────────────────────────────────────────────────┘
```

---

## Scenario 5: Suspicious Activity - Multiple Denials

### Attack Pattern Detected
```
Time Range: 2026-03-29 14:00:00 to 14:05:00

Attempt 1: 14:00:15 - /admin/users
Attempt 2: 14:01:30 - /admin/fees
Attempt 3: 14:02:45 - /admin/employees
Attempt 4: 14:03:10 - /admin/reports
Attempt 5: 14:04:20 - /admin/settings
```

### Security Logs
```
┌─ Multiple Access Denied Logs ──────────────────────────┐
│                                                         │
│ EntityId    | Resource            | Time      | User    │
│─────────────┼─────────────────────┼───────────┼─────────│
│ E5F6A7B8C9  | /admin/users        | 14:00:15  | Unknown │
│ F6A7B8C9D0  | /admin/fees         | 14:01:30  | Unknown │
│ A7B8C9D0E1  | /admin/employees    | 14:02:45  | Unknown │
│ B8C9D0E1F2  | /admin/reports      | 14:03:10  | Unknown │
│ C9D0E1F2G3  | /admin/settings     | 14:04:20  | Unknown │
│                                                         │
│ Pattern: Systematic admin resource scanning ⚠️         │
│ IpAddress: 203.45.67.100 (consistent)                  │
│ User Agent: curl/7.68.0 (automated tool)              │
│ Action Recommended: 🔴 Block IP / Investigate       │
│                                                         │
└─────────────────────────────────────────────────────────┘
```

---

## Query Examples for Monitoring

### 1. Find All Access Denials by Specific User

```sql
SELECT 
    CreatedDate,
    Message,
    Details,
    IpAddress,
    IncidentId = EntityId
FROM LogsTable 
WHERE Action = 'AccessDenied' 
AND UserName = 'Ahmed_Student@school.com'
ORDER BY CreatedDate DESC;
```

**Output:**
```
CreatedDate         | Message                           | IpAddress
────────────────────┼──────────────────────────────────┼─────────────
2026-03-29 10:15:30 | Access Denied: /admin/fees-setup | 192.168.1.45
2026-03-29 09:30:20 | Access Denied: /admin/users      | 192.168.1.45
2026-03-29 08:45:10 | Access Denied: /admin/reports    | 192.168.1.45
```

### 2. Find All Access Denials with Roles (Permission Issues)

```sql
SELECT 
    UserName,
    Message,
    CreatedDate,
    Details
FROM LogsTable 
WHERE Action = 'AccessDeniedWithRoles'
ORDER BY CreatedDate DESC;
```

**Indicates:** Users have roles but still denied - check permission configuration!

### 3. Find Suspicious IP Addresses

```sql
SELECT 
    IpAddress,
    COUNT(*) as DenialCount,
    GROUP_CONCAT(DISTINCT UserName) as Users,
    MAX(CreatedDate) as LastAttempt
FROM LogsTable 
WHERE Action = 'AccessDenied'
AND CreatedDate > DATE_SUB(NOW(), INTERVAL 1 HOUR)
GROUP BY IpAddress
HAVING DenialCount > 5
ORDER BY DenialCount DESC;
```

**Output:**
```
IpAddress        | DenialCount | Users                  | LastAttempt
─────────────────┼─────────────┼────────────────────────┼──────────────
203.45.67.100    | 15          | Unknown, Anonymous     | 2026-03-29 14:04:20
192.168.1.50     | 8           | Ahmed_Student, etc     | 2026-03-29 13:45:10
```

### 4. Most Frequently Denied Resources

```sql
SELECT 
    SUBSTRING_INDEX(SUBSTRING_INDEX(Details, 'Attempted Resource: ', -1), '\n', 1) as Resource,
    COUNT(*) as DenialCount,
    COUNT(DISTINCT UserName) as UniqueUsers,
    MAX(CreatedDate) as LastDenial
FROM LogsTable 
WHERE Action = 'AccessDenied'
AND CreatedDate > DATE_SUB(NOW(), INTERVAL 7 DAY)
GROUP BY Resource
ORDER BY DenialCount DESC
LIMIT 10;
```

**Output:**
```
Resource                  | DenialCount | UniqueUsers | LastDenial
──────────────────────────┼─────────────┼─────────────┼───────────
/admin/fees-setup         | 45          | 23          | 2026-03-29 14:20
/admin/employee-mgmt      | 32          | 15          | 2026-03-29 13:50
/admin/reports            | 28          | 12          | 2026-03-29 12:30
```

### 5. Access Denials Timeline

```sql
SELECT 
    DATE(CreatedDate) as Date,
    HOUR(CreatedDate) as Hour,
    COUNT(*) as DenialCount
FROM LogsTable 
WHERE Action IN ('AccessDenied', 'AccessDeniedWithRoles')
AND CreatedDate >= DATE_SUB(NOW(), INTERVAL 7 DAY)
GROUP BY Date, Hour
ORDER BY Date DESC, Hour DESC;
```

---

## Real-World Monitoring Dashboard Queries

### Alert: High Access Denial Rate
```sql
SELECT COUNT(*) as RecentDenials
FROM LogsTable 
WHERE Action = 'AccessDenied'
AND CreatedDate > DATE_SUB(NOW(), INTERVAL 1 MINUTE);

-- Alert if > 10 denials in last minute
```

### Alert: Potential Brute Force Attack
```sql
SELECT UserName, IpAddress, COUNT(*) as Attempts
FROM LogsTable 
WHERE Action = 'AccessDenied'
AND CreatedDate > DATE_SUB(NOW(), INTERVAL 10 MINUTE)
GROUP BY UserName, IpAddress
HAVING Attempts > 3;
```

### Alert: Permission Configuration Issues
```sql
SELECT UserName, COUNT(*) as DenialCount
FROM LogsTable 
WHERE Action = 'AccessDeniedWithRoles'
AND CreatedDate > DATE_SUB(NOW(), INTERVAL 24 HOUR)
GROUP BY UserName
HAVING DenialCount > 1;
```

---

## Summary

| Scenario | Log Level | Action | Details |
|----------|-----------|--------|---------|
| Student accessing admin | INFO | AccessDenied | Standard access denial |
| User with roles denied | WARNING | AccessDeniedWithRoles | Permission config issue |
| Anonymous access | INFO | AccessDenied | Not authenticated |
| Proxy/VPN access | INFO | AccessDenied | IP from X-Forwarded-For |
| Brute force pattern | CRITICAL | Multiple denials | Security threat |

**Status:** ✅ All Scenarios Logged and Trackable
