# Fees Payment Report System Documentation

## Overview
The Fees Payment Report system provides a comprehensive analysis of fee collections across sessions, terms, and classes. It compares expected vs. actual revenue to track payment performance and outstanding balances.

## Data Structure

### Report Line Item ViewModel
`FeesReportLineItemViewModel.cs` - Represents a single report row

**Properties:**
- `SessionId` - Academic session identifier
- `Session` - Session name (e.g., "2024/2025")
- `Term` - Term numeric value (1=First, 2=Second, 3=Third)
- `TermName` - Term name string
- `SchoolClassId` - Class identifier
- `ClassName` - Class name
- `StudentCount` - Number of registered students
- `FeeAmountPerStudent` - Fee per student (₦)
- `ExpectedAmount` - Total expected revenue (FeeAmountPerStudent × StudentCount)
- `ActualAmount` - Total payments received (sum of approved, non-cancelled payments)
- `OutstandingAmount` - Unpaid balance (ExpectedAmount - ActualAmount)
- `CollectionPercentage` - Percentage of expected amount collected (read-only calculated property)

### Report View Model
`FeesReportViewModel.cs` - Main report container

**Properties:**
- `ReportData` - List of `FeesReportLineItemViewModel` items
- `SelectedSessionId`, `SelectedTerm`, `SelectedClassId` - Current filter values
- `Sessions`, `Terms`, `Classes` - Dropdown lists for filtering
- **Summary Totals:**
  - `TotalStudents` - Sum of all student counts
  - `TotalExpected` - Sum of all expected amounts
  - `TotalActual` - Sum of all actual payments
  - `TotalOutstanding` - Sum of all outstanding amounts
  - `OverallCollectionPercentage` - Overall collection rate

## Service Layer

### IFeesPaymentServices Interface
**New Method:**
```csharp
Task<FeesReportViewModel> GetFeesReportAsync(int? sessionId = null, int? term = null, int? classId = null);
```

### FeesPaymentServices Implementation
**Method Logic:**
1. Load all sessions, terms (hardcoded 1-3), and classes from database
2. Filter sessions based on optional `sessionId` parameter
3. Filter terms based on optional `term` parameter
4. Filter classes based on optional `classId` parameter
5. For each combination, calculate:
   - **Expected Amount**: `TermlyFeesSetup.Amount × RegisteredStudentCount`
   - **Actual Amount**: Sum of payments where:
     - `PaymentStatus == Approved`
     - `PaymentState != Cancelled`
   - **Outstanding**: `ExpectedAmount - ActualAmount`
6. Sort results by SessionId (desc), Term (asc), ClassName (asc)
7. Calculate aggregate totals

**Data Sources:**
- **Sessions**: `SessionYear` table
- **Terms**: Enum-based (hardcoded)
- **Classes**: `SchoolClasses` table
- **Fee Setup**: `TermlyFeesSetup` table
- **Student Registration**: `TermRegistration` table
- **Payments**: `FeesPaymentTable` with Status=Approved, PaymentState!=Cancelled

## Page Model

### fees_payment_reportModel

**Properties:**
- `[BindProperty(SupportsGet = true)]` properties:
  - `SessionId` - Selected session filter
  - `Term` - Selected term filter
  - `ClassId` - Selected class filter
- `ReportData` - FeesReportViewModel instance

**Methods:**
1. **OnGetAsync()** - Initial page load
   - Calls `GetFeesReportAsync()` with filter parameters
   - Handles exceptions and displays error messages

2. **OnGetExportAsync()** - Export to CSV
   - Retrieves filtered report data
   - Converts to CSV format
   - Returns file download

3. **ExportToCSV()** - Helper method
   - Creates comma-separated file with headers
   - Includes summary totals
   - Formats currency values with 2 decimal places

## Razor Page UI

### fees-payment-report.cshtml

**Layout:**
1. **Page Header** - Title and back button
2. **Error Alert** - Displays errors from TempData
3. **Filter Section** - Card with dropdown filters:
   - Session dropdown (all sessions)
   - Term dropdown (First, Second, Third)
   - Class dropdown (all classes)
   - Generate Report, Reset, and Export buttons

4. **Summary Cards** (displayed when data exists)
   - Total Students
   - Total Expected Amount
   - Total Actual Paid
   - Total Outstanding

5. **Collection Rate Progress Bar**
   - Visual representation of overall collection percentage
   - Bootstrap progress bar with percentage label

6. **Report Table**
   - Detailed report data with columns:
     - Session, Term (badge), Class
     - No. of Students (badge)
     - Fee/Student, Expected Amount, Actual Paid, Outstanding
     - Collection % (color-coded badge)
   - Footer row with totals
   - Responsive table design

7. **Empty State** - Message when no data available

### Color Coding
Collection percentage badges:
- **Green** (≥100%) - Full or over-collection
- **Blue** (75-99%) - Good collection
- **Yellow** (50-74%) - Needs attention
- **Red** (<50%) - Poor collection

## Filter Logic

**Filter Behavior:**
- **No Filters**: Shows all sessions, terms, and classes
- **Session Selected**: Shows all terms and classes for that session
- **Term Selected**: Shows that term across all sessions and classes
- **Class Selected**: Shows that class across all sessions and terms
- **Multiple Filters**: Combines filters with AND logic

## Export Functionality

**CSV Export Format:**
```
Session,Term,Class,No. of Students,Fee per Student,Expected Amount,Actual Paid,Outstanding,Collection %
2024/2025,First,JSS1,45,15000.00,675000.00,650000.00,25000.00,96.30%
...
TOTALS,,,1250,,18750000.00,17500000.00,1250000.00,93.33%
```

## Key Constraints

1. **Only Approved Payments**: Filter `PaymentStatus == Approved`
2. **No Cancelled Payments**: Filter `PaymentState != Cancelled`
3. **No Duplicate Payments**: Aggregates by TermRegistration level
4. **Student Count**: Based on `TermRegistration` records
5. **Fee Amount**: Retrieved from `TermlyFeesSetup`

## Integration Points

**Dependencies:**
- `IFeesPaymentServices` - Service interface
- `FeesReportViewModel`, `FeesReportLineItemViewModel` - View models
- `TermlyFeesSetup`, `TermRegistration`, `FeesPaymentTable` - Data models
- `PaymentStatus`, `PaymentState` - Enums

**Related Pages:**
- `/admin/feesmanager/index` - Main fees management page
- `/admin/feesmanager/fees-payment/make-payment` - Payment recording
- `/admin/feesmanager/fees-payment/payment-invoice` - Invoice view

## Performance Considerations

1. **Database Queries**: Report generation involves multiple database calls
   - Consider adding indexes on:
     - `FeesPaymentTable.Status, PaymentState`
     - `TermRegistration.SessionId, Term, SchoolClassId`
     - `TermlyFeesSetup.SessionId, Term, SchoolClassId`

2. **Large Datasets**: For schools with many students/sessions:
   - Implement pagination if table grows very large
   - Consider caching dropdown lists
   - Optimize aggregate calculations

3. **Export Performance**: CSV export includes all filtered data
   - Large exports may require timeout adjustment
   - Consider implementing async export with file download link

## Future Enhancements

1. **Additional Export Formats**:
   - PDF export with formatted styling
   - Excel export with charts
   - JSON export for API consumption

2. **Advanced Reporting**:
   - Historical comparison (session-over-session)
   - Collection trend analysis
   - Student-level payment status drill-down
   - Payment method breakdown

3. **Notifications**:
   - Alert for classes with low collection rates
   - Overdue payment reminders
   - Monthly collection summary emails

4. **Performance Optimization**:
   - Implement report caching
   - Background report generation
   - Pagination for large datasets
   - Advanced filtering options

## Testing Checklist

- [ ] Report loads without filters (all data)
- [ ] Session filter works correctly
- [ ] Term filter works correctly
- [ ] Class filter works correctly
- [ ] Multiple filters combined correctly
- [ ] Totals calculated accurately
- [ ] Collection percentage badge colors display correctly
- [ ] CSV export contains correct data
- [ ] Empty state displays when no data
- [ ] Error handling and display works
- [ ] Performance acceptable with large dataset
- [ ] Responsive design on mobile devices
