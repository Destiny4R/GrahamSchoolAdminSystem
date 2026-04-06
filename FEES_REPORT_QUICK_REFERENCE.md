# Fees Payment Report - Quick Reference Guide

## 🎯 What Was Built

A comprehensive **Fees Payment Report System** that shows expected vs. actual fees collected across sessions, terms, and classes.

## 📍 Access Point

**URL**: `/admin/feesmanager/fees-payment-report`

**Navigation**: From any admin page, go to Fees Manager section

## 🎯 Main Features

### 1. Filtering
- **Session Filter**: Select specific academic year or view all
- **Term Filter**: First, Second, or Third term
- **Class Filter**: Specific class or all classes
- **Combined Filters**: Use multiple filters together

### 2. Report Display
Four summary cards showing:
- **Total Students**: Overall student count
- **Expected Amount**: Total fees expected
- **Actual Paid**: Total fees collected
- **Outstanding**: Total unpaid fees

Collection Rate Progress Bar (visual indicator)

### 3. Detailed Table
Columns:
- Session & Term
- Class Name
- Number of Students
- Fee per Student
- Expected Amount (Fee × Students)
- Actual Paid (Approved payments only)
- Outstanding (Expected - Actual)
- Collection % (with color coding)

**Footer Row**: Shows totals for all columns

### 4. Export to CSV
- Download filtered data as CSV
- Filename includes timestamp
- Includes totals row
- Open in Excel or any spreadsheet app

## 📊 Data Sources

| Data | From |
|------|------|
| Expected Fees | TermlyFeesSetup table + Student count |
| Actual Payments | FeesPaymentTable (Approved, not Cancelled) |
| Student Count | TermRegistration records |
| Sessions/Terms/Classes | System lookup tables |

## 🎨 Color Coding

Collection Percentage Badges:
- 🟢 **Green**: ≥ 100% (Complete)
- 🔵 **Blue**: 75-99% (Good)
- 🟡 **Yellow**: 50-74% (Needs Attention)
- 🔴 **Red**: < 50% (Poor)

## 💾 Calculations

```
Expected Amount = Fee per Student × Number of Students
Actual Amount = Sum of Approved, Non-Cancelled Payments
Outstanding = Expected Amount - Actual Amount
Collection % = (Actual Amount / Expected Amount) × 100
```

## 🚀 Usage Steps

### Step 1: Access Report
Go to `/admin/feesmanager/fees-payment-report`

### Step 2: Select Filters (Optional)
- Session dropdown: Select year or leave blank
- Term dropdown: Select term or leave blank
- Class dropdown: Select class or leave blank

### Step 3: Generate Report
Click **"Generate Report"** button
- Report displays with filtered data
- Summary cards and table update
- Collection percentage shows in progress bar

### Step 4: Review Data
- Check Expected vs Actual columns
- Review Collection % badges for quick assessment
- Check Outstanding amount for follow-up

### Step 5: Export (Optional)
Click **"Export to CSV"** to download data
- Save file to computer
- Open in Excel/Google Sheets
- Use for further analysis

### Step 6: Reset Filters
Click **"Reset"** to clear all filters and start over

## 📋 Key Information

### What's Included
- ✅ Approved payments only
- ✅ Non-cancelled transactions only
- ✅ All registered students per session/term/class
- ✅ Current fee setup amounts
- ✅ Calculated totals and percentages

### What's NOT Included
- ❌ Pending payments
- ❌ Rejected payments
- ❌ Cancelled transactions
- ❌ Students with no registration

## ⚙️ Technical Details

### ViewModels
- `FeesReportViewModel` - Main report container
- `FeesReportLineItemViewModel` - Individual report rows

### Service Method
- `GetFeesReportAsync(sessionId?, term?, classId?)` - Returns filtered report

### Page Route
- Route: `/admin/feesmanager/fees-payment-report`
- Handler: `OnGetAsync()` - Display report
- Handler: `OnGetExportAsync()` - Export to CSV

## 🔍 Understanding the Data

### Example Report Row
```
Session: 2024/2025
Term: First
Class: JSS1
Students: 45
Fee/Student: ₦15,000
Expected: ₦675,000
Actual: ₦650,000
Outstanding: ₦25,000
Collection %: 96.30% ✅
```

### Reading Summary
- 45 students expected to pay ₦675,000 total
- ₦650,000 was actually received
- ₦25,000 is still outstanding
- 96.3% collection rate (very good!)

## 🆘 Troubleshooting

### Report Shows No Data
- ✅ Click "Reset" and try again
- ✅ Check if fee setup exists for selected session/term
- ✅ Check if students are registered for selected session/term
- ✅ Check if payments have been made and approved

### Numbers Don't Look Right
- ✅ Verify fee setup amounts in system
- ✅ Check student registration counts
- ✅ Review payment records for correct status/state
- ✅ Ensure payments are marked "Approved" not "Pending"

### Can't Export
- ✅ Generate report first
- ✅ Export button appears when data exists
- ✅ Check browser download settings

## 📈 Interpreting Results

### High Collection Rate (75-100%)
✅ **Good**: School is collecting well
- Follow up on remaining 0-25%
- Maintain current collection practices

### Medium Collection Rate (50-74%)
⚠️ **Needs Attention**: 
- Identify which classes are underperforming
- Check for specific term issues
- Review payment status for pending items

### Low Collection Rate (<50%)
🔴 **Poor**: Urgent action needed
- Review class-by-class breakdown
- Contact students with outstanding balances
- Check for payment system issues

## 🎓 Best Practices

1. **Regular Monitoring**: Check report weekly or monthly
2. **Use Filters**: Analyze by session, term, or class
3. **Export for Analysis**: Use CSV for spreadsheet analysis
4. **Follow Up**: Use outstanding amounts to contact students
5. **Track Trends**: Compare month-over-month collection rates

## 📞 Support

For issues or questions:
- Check data sources (TermlyFeesSetup, TermRegistration, FeesPaymentTable)
- Verify payments are marked as Approved
- Ensure student registrations are complete
- Check system logs for errors

---

**Last Updated**: 2024
**Status**: Production Ready ✅
