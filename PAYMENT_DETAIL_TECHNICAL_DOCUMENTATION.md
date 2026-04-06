# Fees Payment Detail Page - Technical Documentation

## Architecture Overview

### MVC Pattern Structure
```
View (view-payment-detail.cshtml)
  ↓
Page Model (view-payment-detail.cshtml.cs)
  ↓
Service Layer (IFeesPaymentServices implementation)
  ↓
Data Access Layer (EntityFrameworkCore)
  ↓
Database (MySQL)
```

## Service Methods

### 1. GetPaymentDetailAsync(int paymentId)
**Purpose**: Retrieve complete payment details with all related information

**Signature**:
```csharp
public async Task<PaymentDetailViewModel> GetPaymentDetailAsync(int paymentId)
```

**Parameters**:
- `paymentId` (int): The ID of the payment to retrieve

**Returns**: `PaymentDetailViewModel` containing:
- Payment information (invoice, amounts, dates)
- Student details (name, registration, session, term, class)
- Staff information (who processed the payment)
- Action availability flags
- File path for payment evidence

**Logic**:
1. Query database for payment record with related entities
2. Include TermRegistration, Student, and SessionYear navigation
3. Get staff user information from ApplicationUser table
4. Calculate total paid before this payment
5. Determine action availability based on payment status
6. Return complete ViewModel

**Error Handling**: Returns null if payment not found

### 2. ApprovePaymentAsync(int paymentId)
**Purpose**: Approve a pending payment

**Signature**:
```csharp
public async Task<ServiceResponse<bool>> ApprovePaymentAsync(int paymentId)
```

**Parameters**:
- `paymentId` (int): The ID of the payment to approve

**Returns**: `ServiceResponse<bool>`
- Success: `true` with message
- Failure: `false` with error message

**Validation**:
- Payment must exist
- Payment status must be Pending

**Actions**:
- Update Status to Approved
- Update UpdatedDate to UtcNow
- Log action with user details
- Save changes to database

**Error Messages**:
- "Payment not found."
- "Only pending payments can be approved."
- "Failed to approve payment."

### 3. RejectPaymentAsync(int paymentId)
**Purpose**: Reject a pending payment

**Signature**:
```csharp
public async Task<ServiceResponse<bool>> RejectPaymentAsync(int paymentId)
```

**Parameters**:
- `paymentId` (int): The ID of the payment to reject

**Returns**: `ServiceResponse<bool>`

**Validation**:
- Payment must exist
- Payment status must be Pending

**Actions**:
- Update Status to Rejected
- Update UpdatedDate to UtcNow
- Log action with user details
- Save changes to database

**Error Messages**:
- "Payment not found."
- "Only pending payments can be rejected."
- "Failed to reject payment."

### 4. CancelPaymentAsync(int paymentId)
**Purpose**: Cancel a rejected payment

**Signature**:
```csharp
public async Task<ServiceResponse<bool>> CancelPaymentAsync(int paymentId)
```

**Parameters**:
- `paymentId` (int): The ID of the payment to cancel

**Returns**: `ServiceResponse<bool>`

**Validation**:
- Payment must exist
- Payment status must be Rejected (enforces business rule)
- Payment state must not already be Cancelled

**Actions**:
- Update PaymentState to Cancelled
- Update UpdatedDate to UtcNow
- Log action with user details
- Save changes to database

**Business Logic**:
- Only rejected payments can be cancelled
- Cancelled payments excluded from balance calculations
- One-way state change (cannot uncancel)

**Error Messages**:
- "Payment not found."
- "Payment must be rejected before canceling."
- "Payment is already cancelled."
- "Failed to cancel payment."

## Page Model Methods

### OnGetAsync(int id)
**Purpose**: Load and display payment details

**Flow**:
1. Call `GetPaymentDetailAsync(id)`
2. Check if payment found
3. Set `PaymentDetail` property
4. If not found: Set error TempData and redirect to index
5. Return Page()

**Parameters**:
- `id` (int): Payment ID from route

**Return**: IActionResult (Page or Redirect)

### OnPostApproveAsync(int paymentId)
**Purpose**: Handle approve form submission

**Flow**:
1. Call `ApprovePaymentAsync(paymentId)`
2. Check result.Succeeded
3. Set appropriate TempData message
4. Redirect to detail page (forces refresh)

**Parameters**:
- `paymentId` (int): Hidden form input

**Return**: IActionResult (Redirect to view-payment-detail)

### OnPostRejectAsync(int paymentId)
**Purpose**: Handle reject form submission

**Flow**:
1. Call `RejectPaymentAsync(paymentId)`
2. Check result.Succeeded
3. Set appropriate TempData message
4. Redirect to detail page

**Parameters**:
- `paymentId` (int): Hidden form input

**Return**: IActionResult (Redirect to view-payment-detail)

### OnPostCancelAsync(int paymentId)
**Purpose**: Handle cancel form submission

**Flow**:
1. Call `CancelPaymentAsync(paymentId)`
2. Check result.Succeeded
3. Set appropriate TempData message
4. Redirect to detail page

**Parameters**:
- `paymentId` (int): Hidden form input

**Return**: IActionResult (Redirect to view-payment-detail)

## ViewModel: PaymentDetailViewModel

```csharp
public class PaymentDetailViewModel
{
    // Payment Info
    public int PaymentId { get; set; }
    public string InvoiceNumber { get; set; }
    
    // Student Info
    public string StudentName { get; set; }
    public string StudentRegNumber { get; set; }
    public string AcademicSession { get; set; }
    public string Term { get; set; }
    public string SchoolClass { get; set; }
    
    // Amount Info
    public decimal Amount { get; set; }
    public decimal TotalFees { get; set; }
    public decimal TotalPaidBefore { get; set; }
    
    // Status Info
    public PaymentStatus Status { get; set; }
    public PaymentState PaymentState { get; set; }
    
    // Evidence & Messages
    public string FilePath { get; set; }
    public string Message { get; set; }
    public string Narration { get; set; }
    
    // Timestamps
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
    
    // Staff Info
    public string StaffUserId { get; set; }
    public string StaffName { get; set; }
    public string StaffEmail { get; set; }
    
    // Action Flags
    public bool CanApprove { get; set; }
    public bool CanReject { get; set; }
    public bool CanCancel { get; set; }
}
```

## View Structure

### Sections
1. **Page Header**: Title, navigation button
2. **Alert Section**: Success/Error messages
3. **Invoice Information**: Invoice number, status badges, dates
4. **Student Information**: Name, registration, session, term, class
5. **Payment Information**: Fees breakdown, balance calculation
6. **Staff Information**: Staff details card
7. **Payment Evidence**: Image/PDF viewer
8. **Actions**: Approve/Reject/Cancel buttons with rules

### Responsive Design
- **Desktop**: 2-column grid layout
- **Mobile**: Single column layout
- Bootstrap grid system (grid-template-columns)
- Flexible button layout

### Evidence Display Logic
```
if (file exists)
  if (isImage)
    display inline
  else if (isPdf)
    display embedded iframe
  else
    show download link
else
  show "no evidence" message
```

## Database Entities Involved

### FeesPaymentTable
```csharp
- Id (PK)
- Amount
- Fees
- TermRegId (FK)
- TermlyFeesId (FK)
- FilePath
- Message
- Narration
- Status (PaymentStatus enum)
- PaymentState (PaymentState enum)
- InvoiceNumber
- CreatedDate
- UpdatedDate
- StaffUserId (NEW - added in migration)
```

### Navigation Properties Used
- `TermRegistration` (includes Student, SchoolClass, SessionYear)
- `TermlyFeesSetup`
- `ApplicationUser` (for staff details)

## Enums

### PaymentStatus
```csharp
Pending = 1
Approved = 2
Rejected = 3
```

### PaymentState
```csharp
PartPayment = 1
Completed = 2
Cancelled = 3
```

## Logging

All actions logged via `ILogService.LogUserActionAsync()`:
- **Approve**: Action="Payment Approval"
- **Reject**: Action="Payment Rejection"
- **Cancel**: Action="Payment Cancellation"

Logged Information:
- User ID and name
- Action type
- Entity type and ID
- Message
- IP address
- Additional details (payment ID, amount)

## Security Considerations

1. **Authorization**: Inherited from Razor Pages framework
2. **Audit Trail**: All actions logged with user identity
3. **State Validation**: Business rules enforced (can't approve already approved)
4. **Soft Deletes**: Cancelled status, not physical deletion
5. **User Context**: Current user tracked via HttpContextAccessor

## Performance Optimizations

1. **Eager Loading**: Includes for TermRegistration and related entities
2. **AsNoTracking**: Used where appropriate
3. **Single Database Query**: Payment detail loaded in one query
4. **Indexed Lookups**: By primary key (PaymentId)

## API Route (if using API controller)

```
GET  /api/v1/payment/{id}              - Get payment detail
POST /api/v1/payment/{id}/approve      - Approve payment
POST /api/v1/payment/{id}/reject       - Reject payment
POST /api/v1/payment/{id}/cancel       - Cancel payment
```

## Testing Scenarios

### Test Case 1: Approve Pending Payment
- Create pending payment
- Navigate to detail page
- Click approve
- Verify status changed to Approved
- Verify approve button hidden

### Test Case 2: Reject Pending Payment
- Create pending payment
- Navigate to detail page
- Click reject
- Verify status changed to Rejected
- Verify cancel button now visible

### Test Case 3: Cancel Rejected Payment
- Create rejected payment
- Click cancel
- Verify PaymentState changed to Cancelled
- Verify payment excluded from calculations

### Test Case 4: Verify Business Rules
- Try to approve already approved payment → Fail
- Try to cancel approved payment → Fail
- Try to cancel pending payment → Fail
- Try to approve rejected payment → Fail

## Future Enhancements

1. **Bulk Actions**: Select multiple payments for batch approval
2. **Email Notifications**: Notify student when payment approved/rejected
3. **Payment History**: Show all previous versions/states
4. **Analytics**: Payment trend charts and reports
5. **Webhooks**: Integration with third-party payment systems
6. **Attachment Management**: Upload additional evidence after initial payment
7. **Payment Reversal**: Full refund functionality
8. **Partial Approval**: Accept partial amounts of rejected payments
