# Position Management UI - Visual Guide

## 🎨 Page Layout

### Main Page Structure
```
┌──────────────────────────────────────────────────────────────┐
│                    Graham School Admin                       │
├──────────────────────────────────────────────────────────────┤
│                                                               │
│  📋 Position Management                                [+]   │
│  Manage positions, assign roles and permissions              │
│                                                               │
├──────────────────────────────────────────────────────────────┤
│                                                               │
│  ✓ Position created successfully                             │
│  [X]                                                          │
│                                                               │
├──────────────────────────────────────────────────────────────┤
│                                                               │
│  📚 All Positions                                        [10] │
│  ├─ Position Name  │ Description │ Assigned Roles │ Emp│Act│
│  ├─────────────────┼─────────────┼────────────────┼────┼────│
│  │ Principal       │ School head │ [Admin]        │ 2  │ ⚙️ │
│  │ Teacher         │ Classroom   │ [Account]      │ 15 │ ⚙️ │
│  │ Cashier         │ Payments    │ [Cashier]      │ 3  │ ⚙️ │
│  │ Accountant      │ Finance     │ [Account]      │ 2  │ ⚙️ │
│  │ Guard           │ Security    │ No roles       │ 5  │ ⚙️ │
│  │ ...             │ ...         │ ...            │ ...│ ...│
│                                                               │
└──────────────────────────────────────────────────────────────┘
```

---

## 🎯 User Interactions

### 1. Add Position Flow

**Button Click:**
```
User clicks [+ Add Position]
    ↓
Modal Appears
```

**Modal Content:**
```
╔════════════════════════════════════╗
║  ➕ Add New Position              ║  ← Gradient Header
╠════════════════════════════════════╣
║                                    ║
║  Position Name * [____________]    ║  ← Required field
║  ℹ️ Unique name to identify        ║  ← Help text
║     this position                  ║
║                                    ║
║  Description  [____________]       ║  ← Optional
║  ℹ️ Brief description of the       ║
║     position responsibilities     ║
║                                    ║
╠════════════════════════════════════╣
║  [Cancel]  [✓ Create Position]    ║  ← Actions
╚════════════════════════════════════╝
```

**Validation:**
```
Scenario 1: Empty Name
User submits form
    ↓
Client validation triggers
    ↓
Message: "Position name is required"

Scenario 2: Duplicate Name
User enters: "Principal"
    ↓
Server validation occurs
    ↓
Message: "Position already exists"

Scenario 3: Valid Input
User enters: "New Position"
    ↓
CreatePositionAsync() called
    ↓
Database insert succeeds
    ↓
Success message shows
    ↓
Page reloads automatically
```

---

### 2. Edit Position Flow

**Button Click:**
```
User clicks [✏️] next to position
    ↓
AJAX Request to OnGetEditPositionAsync
    ↓
JSON Response received
    ↓
Modal populates with data
```

**Modal Content:**
```
╔════════════════════════════════════╗
║  ✏️ Edit Position                 ║
╠════════════════════════════════════╣
║                                    ║
║  Position Name * [Principal     ]  ║  ← Pre-filled
║                                    ║
║  Description  [School leader    ]  ║  ← Pre-filled
║                                    ║
╠════════════════════════════════════╣
║  [Cancel]  [✓ Update Position]    ║
╚════════════════════════════════════╝
```

**Process:**
```
form.submit()
    ↓
OnPostUpdatePositionAsync(PositionModel)
    ↓
UpdatePositionAsync() validates & saves
    ↓
RedirectToPage()
    ↓
Page reloads with updated data
```

---

### 3. Assign Roles Flow

**Button Click:**
```
User clicks [⚙️] next to position
    ↓
AJAX Request to OnGetAssignRolesAsync
    ↓
Complex JSON response with roles & permissions
    ↓
Modal built dynamically with JavaScript
```

**Modal Content Structure:**
```
╔══════════════════════════════════════════════════════════╗
║  ⚙️ Assign Roles to Position                            ║
╠══════════════════════════════════════════════════════════╣
║                                                          ║
║  ℹ️ Selected Position: [Principal]                      ║
║                                                          ║
║  🛡️ Available Roles & Permissions                      ║
║                                                          ║
│  ┌──────────────────────────────────────────────────┐   │
│  │ ☑ Admin                        [Already checked] │   │
│  │ ─────────────────────────────────────────────    │   │
│  │ 📋 Permissions:                                 │   │
│  │ ┌────────────────────────────────────────────┐  │   │
│  │ │ View Dashboard  │ Manage Positions  │      │  │   │
│  │ │ Manage Employees│ Manage Roles     │      │  │   │
│  │ │ View Reports    │ Manage Students  │      │  │   │
│  │ │ Manage Fees     │ Manage Session   │      │  │   │
│  │ │ Manage Classes  │ User Management  │      │  │   │
│  │ │ System Settings │                  │      │  │   │
│  │ └────────────────────────────────────────────┘  │   │
│  └──────────────────────────────────────────────────┘   │
│                                                          │
│  ┌──────────────────────────────────────────────────┐   │
│  │ ☐ Account                                        │   │
│  │ ─────────────────────────────────────────────    │   │
│  │ 📋 Permissions:                                 │   │
│  │ ┌────────────────────────────────────────────┐  │   │
│  │ │ View Dashboard  │ View Reports     │      │  │   │
│  │ │ Manage Fees     │ Manage Session   │      │  │   │
│  │ │ View Students   │ View Classes     │      │  │   │
│  │ └────────────────────────────────────────────┘  │   │
│  └──────────────────────────────────────────────────┘   │
│                                                          │
│  ┌──────────────────────────────────────────────────┐   │
│  │ ☐ Cashier                                        │   │
│  │ ─────────────────────────────────────────────    │   │
│  │ 📋 Permissions:                                 │   │
│  │ ┌────────────────────────────────────────────┐  │   │
│  │ │ View Dashboard      │ Process Payments  │ │  │   │
│  │ │ View Student Fees   │ Generate Receipts│ │  │   │
│  │ │ View Reports        │                  │ │  │   │
│  │ └────────────────────────────────────────────┘  │   │
│  └──────────────────────────────────────────────────┘   │
║                                                          ║
╠══════════════════════════════════════════════════════════╣
║  [Cancel]  [✓ Save Role Assignments]                    ║
╚══════════════════════════════════════════════════════════╝
```

**Interaction:**
```
User checks/unchecks role checkboxes
    ↓
Form data updated (SelectedRoleIds list)
    ↓
User clicks [Save Role Assignments]
    ↓
form.submit()
    ↓
OnPostAssignRolesAsync()
    ↓
AssignRolesToPositionAsync() removes old, adds new
    ↓
RedirectToPage()
    ↓
Modal closes, page reloads, table updates
```

---

### 4. Delete Position Flow

**Button Click:**
```
User clicks [🗑️]
    ↓
JavaScript confirmation dialog appears
```

**Confirmation Dialog:**
```
┌─────────────────────────────────────┐
│  Confirm Delete                 [x] │
├─────────────────────────────────────┤
│                                     │
│  Are you sure you want to delete    │
│  the position "Principal"?          │
│                                     │
│  This action cannot be undone.      │
│                                     │
├─────────────────────────────────────┤
│  [Cancel]            [Delete]       │
└─────────────────────────────────────┘
```

**Process:**
```
Scenario 1: Position has employees
DeletePositionAsync()
    ↓
Validation fails (employees assigned)
    ↓
Error message shows
    ↓
Position preserved

Scenario 2: Position is empty
DeletePositionAsync()
    ↓
Validation passes
    ↓
Database deletion occurs
    ↓
Success message shows
    ↓
Page reloads
    ↓
Position removed from list
```

---

## 🎨 Color Scheme

### Badge Colors
```
Admin    ▪ Red      (#dc3545)  - Critical/Admin access
Account  ▪ Yellow   (#ffc107)  - Warning/Finance access  
Cashier  ▪ Green    (#28a745)  - Success/Payment access
None     ▪ Light    (#f8f9fa)  - No roles assigned
```

### Status Indicators
```
✓ Success Alert     ▪ Green background   (#d4edda)
✗ Error Alert       ▪ Red background     (#f8d7da)
ℹ️ Info Alert       ▪ Blue background    (#d1ecf1)
⚠️ Warning Alert    ▪ Yellow background  (#fff3cd)
```

### Button Styles
```
Primary Action      ▪ Blue (#0d6efd)    - Create, Save
Secondary Action    ▪ Gray (#6c757d)    - Cancel
Danger Action       ▪ Red (#dc3545)     - Delete
Info Action         ▪ Cyan (#0dcaf0)    - Assign Roles
```

---

## 📊 State Transitions

### Position Card States

**Normal State:**
```
┌────────────────────────────────────────┐
│ Principal          School head          │
│ [Admin]  👥: 2   [✏️] [⚙️] [🗑️]       │
└────────────────────────────────────────┘
```

**Hover State:**
```
┌────────────────────────────────────────┐ ← Shadow grows
│ Principal          School head          │   Elevates up
│ [Admin]  👥: 2   [✏️] [⚙️] [🗑️]       │
└────────────────────────────────────────┘
```

**Selected Role Checkbox:**
```
Normal:
☐ Admin

Selected:
☑ Admin              ← Checkbox filled
│ ✓ Permissions...   ← Permissions visible
│ Layout highlights
```

---

## 📱 Responsive Behavior

### Desktop (1200px+)
```
┌─────────────────────────────────────────┐
│ Position │ Description │ Roles │ Emp │  │
├─────────────────────────────────────────┤
│          │             │       │     │  │  3-column permission grid
│ Teacher  │ Classroom   │ [Acct]│ 15  │✎ │
│ Principal│ School head │ [Admin]│ 2  │✎ │
│          │             │       │     │  │
```

### Tablet (768-1024px)
```
┌──────────────────────────────┐
│ Position │ Roles │ Emp │ Act│
├──────────────────────────────┤
│ Teacher  │ [Acct]│ 15 │ ✎ │  2-column permission grid
│ Principal│ [Admin] │ 2  │ ✎ │  Narrower columns
│          │       │    │   │
```

### Mobile (<768px)
```
┌─────────────────┐
│ Teacher         │
│ [Account]       │
│ Employees: 15   │
│ [Edit][Roles][🗑] │  Single column
│                 │  Stacked buttons
│ Principal       │  Full-width modals
│ [Admin]         │
│ Employees: 2    │
│ [Edit][Roles][🗑] │
```

---

## ✨ Animation & Transitions

### Modal Appearance
```
1. Click button
   ↓
2. Fade in overlay (0.3s)
   ↓
3. Slide down modal (0.3s)
   ↓
4. Ready for interaction
```

### Badge Hover
```
Badge background changes
Border highlights
Shadow appears
(All 0.2s smooth transition)
```

### Button Hover
```
Background color changes
Shadow appears
Slightly scales up
(All 0.2s smooth transition)
```

---

## 🎯 Empty State

When no positions exist:
```
┌─────────────────────────────────┐
│                                 │
│           📂                    │
│                                 │
│   No Positions Found            │
│   Start by creating your first  │
│   position                      │
│                                 │
│   [+ Create Position]           │
│                                 │
└─────────────────────────────────┘
```

---

## 🔍 Key UI Elements

### Table Headers
- **Position Name** - 25% width, sortable
- **Description** - 20% width
- **Assigned Roles** - 20% width, shows badges
- **Employees** - 15% width, shows count badge
- **Actions** - 20% width, button group

### Permission Card Grid
```
Responsive grid:
- Desktop: repeat(auto-fill, minmax(200px, 1fr))
- Tablet: repeat(2, 1fr)
- Mobile: repeat(1, 1fr)
```

### Success/Error Messages
- Position: Top of page
- Duration: 5 seconds (Bootstrap dismissible)
- Icon: ✓ or ✗
- Animation: Fade in/out

---

## 🎓 Component Hierarchy

```
Page Container
├── Page Header
│   ├── Title
│   ├── Subtitle
│   └── Add Button
│
├── Alert Area
│   ├── Success Messages
│   └── Error Messages
│
├── Positions Card
│   └── Data Table
│       ├── Table Header
│       └── Table Body
│           ├── Position Row (repeating)
│           │   ├── Name Column
│           │   ├── Description Column
│           │   ├── Roles Column (with badges)
│           │   ├── Employee Count Column
│           │   └── Actions Column (with buttons)
│           └── Empty State (if no rows)
│
├── Add Position Modal
│   ├── Modal Header
│   ├── Modal Body (Form)
│   └── Modal Footer (Actions)
│
├── Edit Position Modal
│   ├── Modal Header
│   ├── Modal Body (Form)
│   └── Modal Footer (Actions)
│
└── Assign Roles Modal
    ├── Modal Header
    ├── Modal Body
    │   ├── Selected Position Info
    │   ├── Role Checkboxes (repeating)
    │   │   ├── Checkbox + Label
    │   │   └── Permission Grid
    │   └── Modal Footer (Actions)
```

---

This completes the visual guide for the Position Management UI! 🎨
