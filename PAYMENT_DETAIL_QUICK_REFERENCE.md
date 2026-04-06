# Fees Payment Details Page - Quick Reference

## Page URL
- Route: `/admin/feesmanager/fees-payment/view-payment-detail/{paymentId}`
- Example: `/admin/feesmanager/fees-payment/view-payment-detail/5`

## How to Access
1. Go to **Admin** → **Fees Manager** → **Fees Payments History**
2. Click the dropdown menu (⋮) in the Actions column
3. Select **View Details**

## What You'll See

### Payment Information
- Invoice Number (unique identifier)
- Payment Status (Pending, Approved, Rejected)
- Payment State (Part Payment, Completed, Cancelled)
- Payment dates (created and updated)

### Student Information
- Full name and registration number
- Academic session and term
- School class
- Payment history (total fees, previously paid, current payment)

### Staff Information
- Name of staff who processed the payment
- Email address
- Staff ID
- Timestamp of last update

### Payment Evidence
- Automatic display of:
  - **Images** (JPG, PNG, GIF): Displayed inline
  - **PDFs**: Embedded viewer
  - **Other files**: Download link
- Can also open in new tab for full view

## Available Actions

### Approve Button ✓
- **When Available**: Payment status is "Pending"
- **What It Does**: Marks the payment as approved
- **After**: Payment is locked (no further actions available)
- **Logged**: Yes, with user information and timestamp

### Reject Button ✗
- **When Available**: Payment status is "Pending"
- **What It Does**: Marks the payment as rejected
- **After**: Cancel button becomes available
- **Logged**: Yes, with user information and timestamp

### Cancel Button 🗑️
- **When Available**: Payment status is "Rejected"
- **What It Does**: Sets PaymentState to Cancelled
- **Note**: Cannot be used on pending or approved payments
- **After**: Payment is removed from calculations
- **Logged**: Yes, with user information and timestamp

## Status Color Codes

### Payment Status Badges
- 🟨 **Pending** (Yellow) - Awaiting review
- 🟩 **Approved** (Green) - Processed successfully
- 🔴 **Rejected** (Red) - Declined by staff

### Payment State Badges
- 🔵 **Part Payment** (Blue) - Partial payment received
- 🟢 **Completed** (Green) - Full payment received
- ⚫ **Cancelled** (Gray) - Payment cancelled/void

## Important Notes

### Business Rules
1. **Only pending payments** can be approved or rejected
2. **Rejected payments only** can be cancelled
3. **Approved payments** cannot be changed (system locked)
4. **Cancelled payments** are excluded from balance calculations

### Data Displayed
- All timestamps shown in UTC time
- Currency amounts shown in Naira (₦)
- Staff member identity tracked for audit trail
- Payment evidence file path included

### File Support
- **Supported Image Formats**: JPG, JPEG, PNG, GIF
- **Supported Document Format**: PDF
- **Other Formats**: Available for download
- **Max Display Height**: 400px for images, 500px for PDFs

## Troubleshooting

### Issue: "Payment not found"
- Check the payment ID in the URL
- Ensure the payment record exists in the database
- Verify you have permission to view the payment

### Issue: Buttons not showing
- Payment may already be processed (approved)
- Check the current payment status
- Read the status information panel for rules

### Issue: Evidence not displaying
- File may not be uploaded
- Check file format (must be image or PDF for inline display)
- Try downloading the file instead

### Issue: Changes not saved
- Confirm the success message appeared
- Check the "Last Updated" timestamp
- Refresh the page to see latest data

## Workflow Examples

### Scenario 1: Approving a Payment
1. View payment detail page
2. Review student information and amount
3. Check payment evidence
4. Click "Approve" button
5. Confirm in dialog
6. Success message appears
7. Page refreshes showing approved status

### Scenario 2: Rejecting and Cancelling
1. View payment detail page
2. Identify issue with payment
3. Click "Reject" button
4. Confirm rejection
5. Cancel button now available
6. Click "Cancel" to void the payment
7. Payment removed from calculations

### Scenario 3: Viewing Evidence
1. View payment detail page
2. Scroll to "Payment Evidence" section
3. For images: View inline
4. For PDFs: View in embedded viewer or open in new tab
5. Use "View Full Document" link to open in new window

## Permissions

- Requires authentication
- Requires appropriate admin role
- Action history logged with user details
- IP address recorded for all actions
