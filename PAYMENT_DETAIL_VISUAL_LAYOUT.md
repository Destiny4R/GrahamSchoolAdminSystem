# Fees Payment Detail Page - Visual Layout

## Page Structure

```
┌─────────────────────────────────────────────────────────────────┐
│                         PAGE HEADER                              │
│  Title: Payment Details                                          │
│  Subtitle: View and manage fees payment information              │
│                          [Back Button]                           │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│                    ALERT MESSAGES (if any)                       │
│  ✓ Success: Payment approved successfully.                       │
│  ✗ Error: Payment not found.                                     │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│                  INVOICE INFORMATION CARD                        │
│  ┌──────────────────┬──────────────────┐                        │
│  │ Invoice Number   │ Payment Status   │                        │
│  │ PAY-12345        │ [🟨 PENDING]     │                        │
│  ├──────────────────┼──────────────────┤                        │
│  │ Payment State    │ Payment Date     │                        │
│  │ [🔵 Part Payment]│ 04 April 2024    │                        │
│  └──────────────────┴──────────────────┘                        │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│             STUDENT INFORMATION CARD                             │
│  👤 STUDENT INFORMATION                                          │
│  ┌──────────────────┬──────────────────┐                        │
│  │ Student Name     │ Reg. Number      │                        │
│  │ John Doe         │ STU-2024-001     │                        │
│  ├──────────────────┼──────────────────┤                        │
│  │ Academic Session │ Term             │                        │
│  │ 2023/2024        │ First            │                        │
│  ├──────────────────┼──────────────────┤                        │
│  │ Class            │                  │                        │
│  │ JSS 1 (Alpha)    │                  │                        │
│  └──────────────────┴──────────────────┘                        │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│             PAYMENT INFORMATION CARD                             │
│  💳 PAYMENT INFORMATION                                          │
│  ┌──────────────────┬──────────────────┐                        │
│  │ Total Fees       │ Previously Paid  │                        │
│  │ ₦50,000.00       │ ₦25,000.00       │                        │
│  ├──────────────────┼──────────────────┤                        │
│  │ Current Payment  │ New Balance      │                        │
│  │ ₦15,000.00       │ ₦10,000.00       │                        │
│  ├───────────────────────────────────────┤                      │
│  │ Message: Partial payment received    │                      │
│  └───────────────────────────────────────┘                      │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│             STAFF INFORMATION CARD                               │
│  👤 STAFF INFORMATION                                            │
│  ┌──────────────────────────────────────┐                      │
│  │ Staff Name       │ Staff Email       │                      │
│  │ Mrs. Admin       │ admin@school.com  │                      │
│  ├──────────────────┼──────────────────┤                        │
│  │ Staff ID         │ Last Updated     │                        │
│  │ adm-12345678910  │ 04 April 2024    │                        │
│  └──────────────────┴──────────────────┘                        │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│             PAYMENT EVIDENCE CARD                                │
│  📄 PAYMENT EVIDENCE                                             │
│  ┌──────────────────────────────────────┐                      │
│  │                                      │                      │
│  │         [Payment Receipt Image]      │                      │
│  │         or PDF Viewer                │                      │
│  │         or Download Link             │                      │
│  │                                      │                      │
│  │   [View Full Document]               │                      │
│  └──────────────────────────────────────┘                      │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│             ACTION BUTTONS & RULES                               │
│  ⚡ ACTIONS                                                      │
│  ┌──────────┬──────────┬──────────┐                             │
│  │ Approve  │ Reject   │ Cancel   │                             │
│  │ ✓        │ ✗        │ 🗑️       │                             │
│  └──────────┴──────────┴──────────┘                             │
│                                                                   │
│  ℹ️ PAYMENT STATUS RULES                                         │
│  • Approve: Available for pending payments only                 │
│  • Reject: Available for pending payments only                  │
│  • Cancel: Payment must be rejected first (PaymentState)        │
└─────────────────────────────────────────────────────────────────┘
```

## Responsive Layout - Desktop View

```
Left Column (50%)          │  Right Column (50%)
────────────────────────────────────────────────────
Student Name              │  Reg. Number
Academic Session          │  Term
Class                     │  (empty)
```

## Responsive Layout - Mobile View

```
Column (100%)
──────────────
Student Name
Reg. Number
Academic Session
Term
Class
```

## Component Details

### Status Badge Colors

```
Payment Status:
- 🟨 Yellow (Pending)   - #ffc107
- 🟩 Green (Approved)   - #28a745
- 🔴 Red (Rejected)     - #dc3545

Payment State:
- 🔵 Blue (Part Payment) - #17a2b8
- 🟢 Green (Completed)   - #20c997
- ⚫ Gray (Cancelled)     - #6c757d
```

### Button States

```
PENDING PAYMENT:
┌─────────────┬──────────────┬──────────────┐
│ Approve     │ Reject       │ Cancel       │
│ (visible)   │ (visible)    │ (hidden)     │
└─────────────┴──────────────┴──────────────┘

APPROVED PAYMENT:
┌─────────────────────────────────────────┐
│ ℹ️ No actions available. Payment locked  │
└─────────────────────────────────────────┘

REJECTED PAYMENT:
┌─────────────┬──────────────┬──────────────┐
│ Approve     │ Reject       │ Cancel       │
│ (hidden)    │ (hidden)     │ (visible)    │
└─────────────┴──────────────┴──────────────┘

CANCELLED PAYMENT:
┌─────────────────────────────────────────┐
│ ℹ️ No actions available. Payment void    │
└─────────────────────────────────────────┘
```

## Navigation Flow

```
                    Fees Payment History
                          ↓
                    [Click View Details]
                          ↓
              View Payment Detail Page
                    ↙        ↓        ↘
              Approve      Reject    Back
                ↓            ↓         ↓
           Approved      Rejected   Return to
             Page           ↓       History
                         Cancel
                           ↓
                       Cancelled
```

## Page Elements Location Map

```
┌─ Header Section
│   ├─ Page Title
│   ├─ Breadcrumbs (if needed)
│   └─ Back Button
│
├─ Alert Section
│   ├─ Success Messages
│   └─ Error Messages
│
├─ Content Cards (Scrollable)
│   ├─ Invoice Information
│   ├─ Student Information
│   ├─ Payment Information
│   ├─ Staff Information
│   ├─ Payment Evidence
│   └─ Actions & Rules
│
└─ Footer
    └─ Browser Default
```

## File Type Detection Flow

```
Evidence File Path
        ↓
Check Extension
    ↙   │   ↘
   JPG  PNG  PDF  Other
    │   │     │    │
   IMG  IMG   PDF  DL
```

Where:
- IMG = Display inline image
- PDF = Embedded PDF viewer
- DL = Download link

## Data Flow Diagram

```
User clicks "View Details"
    ↓
Route: /admin/feesmanager/fees-payment/view-payment-detail/{id}
    ↓
OnGetAsync(id) is called
    ↓
Call GetPaymentDetailAsync(id)
    ↓
Query Database:
├─ FeesPayments table
├─ Join TermRegistration
├─ Join Student
├─ Join SessionYear
├─ Join SchoolClass
└─ Join ApplicationUser (Staff)
    ↓
Build PaymentDetailViewModel
    ↓
Return Page with ViewModel
    ↓
Render view-payment-detail.cshtml
    ↓
User sees payment details
```

## Action Processing Flow

```
User clicks Button
    ↓
Form Post Handler
├─ OnPostApproveAsync
├─ OnPostRejectAsync
└─ OnPostCancelAsync
    ↓
Service Method Called
├─ ApprovePaymentAsync
├─ RejectPaymentAsync
└─ CancelPaymentAsync
    ↓
Validation Check
├─ Payment exists?
├─ Status valid?
└─ Rules satisfied?
    ↓
Update Database
    ↓
Log Action
    ↓
Set TempData Message
    ↓
Redirect to Detail Page
    ↓
Page Reloads with Updated Data
```

## Card Section Spacing

```
Each card has:
- 20px padding
- 1px border-bottom (except last)
- Margin-bottom: 20px
- Border-radius: 8px
- Box-shadow: 0 2px 4px rgba(0,0,0,0.1)

Content inside cards:
- Row spacing: 15px
- Column gap: 20px
- Label color: #666
- Value color: #333
```

## Responsive Breakpoints

```
Desktop (≥992px):
- Detail rows: 2 columns
- Grid gap: 20px

Tablet (768px - 991px):
- Detail rows: 1-2 columns (mixed)
- Grid gap: 15px

Mobile (<768px):
- Detail rows: 1 column
- Grid gap: 10px
- Buttons: Full width stacked
```

---

This visual reference helps understand the page layout and component placement.
