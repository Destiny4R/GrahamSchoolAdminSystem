# Sidebar Navigation - Quick Troubleshooting Guide

## ✅ Quick Fixes

### Links Not Clicking?
**Problem**: Nothing happens when clicking sidebar links  
**Solution**:
1. Open browser DevTools (F12)
2. Go to Console tab
3. Look for JavaScript errors
4. If you see errors about `preventDefault`, the fix has been applied
5. Hard refresh: `Ctrl + Shift + R` (Windows) or `Cmd + Shift + R` (Mac)

---

### Active State Not Showing?
**Problem**: Clicked link doesn't highlight in gold  
**Quick Test in Console**:
```javascript
// Check if Navigation module exists
Navigation

// Manually trigger active state
Navigation.setActive()

// Check current path
window.location.pathname
```

**If Navigation is undefined**:
- Make sure you've stopped debugging and restarted the app
- Or use Hot Reload (Ctrl+Alt+F5)

---

### Page Title Not Updating?
**Problem**: Topbar still shows "Dashboard" on all pages  
**Quick Fix**:
```javascript
// In browser console
$('#topbar .page-title').text('Your Page Name')
```

**Permanent Fix**: Navigation module should auto-update on page load. If not, check Console for errors.

---

## 🔧 Developer Console Commands

### Test Navigation Module
```javascript
// Check module loaded
Navigation

// Get current path
Navigation.getCurrentPath()

// Force active state update
Navigation.setActive()

// Check jQuery loaded
$

// Test link selection
$('#sidebar .nav-link[href="/admin/sub-class/index"]').length

// Manually add active class
$('#sidebar .nav-link[href="/admin/sub-class/index"]').addClass('active')
```

### Test Dropdown Expansion
```javascript
// Open Fees dropdown manually
$('#feesMenu').addClass('show')

// Open HR dropdown manually
$('#hrMenu').addClass('show')

// Check active dropdown items
$('#sidebar .dropdown-item.active').length
```

### Debug Active State
```javascript
// See all active elements
$('#sidebar .active').each(function() {
    console.log(this);
});

// Remove all active states
$('#sidebar .nav-link, #sidebar .dropdown-item').removeClass('active')

// Add active to specific link
$('#sidebar .nav-link').eq(3).addClass('active')
```

---

## 📋 Verification Checklist

### After Applying Fix
- [ ] Stop debugging in Visual Studio
- [ ] Press F5 to rebuild and run
- [ ] OR use Hot Reload (Ctrl+Alt+F5)
- [ ] Open browser DevTools (F12)
- [ ] Check Console for errors
- [ ] Test each working link:
  - [ ] School Classes
  - [ ] School Sub-Classes
  - [ ] Academic Sessions
  - [ ] Fees Setup
  - [ ] Employees
  - [ ] Positions

### Visual Indicators
When link is clicked:
- [ ] URL changes in address bar
- [ ] Page content loads
- [ ] Clicked link turns gold color
- [ ] Gold 3px border appears on left edge
- [ ] Page title updates in topbar

For dropdown items:
- [ ] Link turns gold
- [ ] Parent dropdown toggle also turns gold
- [ ] Parent dropdown stays expanded

---

## 🐛 Common Issues

### Issue 1: "Navigation is not defined"
**Cause**: appmain.js not loaded or Navigation module not added  
**Fix**:
1. Check file exists: `GrahamSchoolAdminSystemWeb/wwwroot/js/appmain.js`
2. Check _Layout.cshtml includes it in Scripts section
3. Verify Navigation module was added at end of file
4. Hard refresh browser

### Issue 2: "$ is not defined"
**Cause**: jQuery not loaded before appmain.js  
**Fix**:
1. Check _Layout.cshtml load order:
   ```html
   <script src="~/lib/jquery/dist/jquery.min.js"></script>
   <script src="~/js/appmain.js"></script>
   ```
2. jQuery MUST load before appmain.js

### Issue 3: Links still doing nothing
**Cause**: Old cached JavaScript  
**Fix**:
1. Clear browser cache
2. Hard refresh: `Ctrl + Shift + R`
3. Try different browser
4. Check Network tab in DevTools
5. Verify site.js and appmain.js are loading

### Issue 4: Dropdowns not expanding
**Cause**: Bootstrap JS not loaded or wrong version  
**Fix**:
1. Check _Layout.cshtml includes Bootstrap:
   ```html
   <script src="~/lib/twitter-bootstrap/js/bootstrap.bundle.min.js"></script>
   ```
2. Verify Bootstrap 5 (not Bootstrap 4 or 3)
3. Check Console for Bootstrap errors

### Issue 5: Active state wrong page
**Cause**: URL path doesn't match link href exactly  
**Debug**:
```javascript
// In Console
console.log('Current:', window.location.pathname);
console.log('Link href:', $('#sidebar .nav-link').eq(3).attr('href'));
```
**Fix**: Update link href to match exact page path

---

## 🎯 Manual Testing Steps

### Test Regular Links
1. Click "School Sub-Classes"
2. URL should change to: `/admin/sub-class/index`
3. Link should turn gold
4. Page title should show "School Sub-Classes"
5. 3px gold border on left edge

### Test Dropdown Links
1. Click "Fees & Payments" → Dropdown opens
2. Click "Fees Setup"
3. URL should change to: `/admin/feesmanager/fees-setup`
4. "Fees Setup" should turn gold
5. "Fees & Payments" parent should also turn gold
6. Dropdown should stay expanded
7. Page title should show "Fees Setup"

### Test Mobile
1. Resize browser to mobile width (< 992px)
2. Click hamburger menu → Sidebar opens
3. Click "School Classes"
4. Sidebar should close automatically
5. Page should navigate

---

## 🔍 File Verification

### Check site.js Fixed
Open `GrahamSchoolAdminSystemWeb/wwwroot/js/site.js` around line 15:

**Should see**:
```javascript
const href = this.getAttribute('href') || '';
if (href === '#' || this.classList.contains('dropdown-toggle')) {
    e.preventDefault();
}
```

**Should NOT see**:
```javascript
e.preventDefault(); // ❌ This blocks everything
```

### Check _Layout.cshtml Fixed
Open `GrahamSchoolAdminSystemWeb/Pages/Shared/_Layout.cshtml` around line 49:

**Should see**:
```html
<a href="/admin/feesmanager/fees-setup" class="dropdown-item">
```

**Should NOT see**:
```html
<a asp-page="/admin/feesmanager/fees-setup" class="dropdown-item">
```

### Check appmain.js Has Navigation Module
Open `GrahamSchoolAdminSystemWeb/wwwroot/js/appmain.js` at end of file:

**Should see**:
```javascript
// NAVIGATION MODULE - Sidebar Active State Management
var Navigation = (function () {
    'use strict';
    // ... module code ...
})();

$(document).ready(function() {
    Navigation.init();
});
```

---

## 📞 Still Not Working?

### Check These:
1. **Build Status**: Visual Studio Output window → Build succeeded?
2. **JavaScript Errors**: Browser Console → Any red errors?
3. **File Loading**: Network tab → Are site.js and appmain.js loading (200 OK)?
4. **jQuery Version**: Console → `$.fn.jquery` → Should show 3.x.x
5. **Bootstrap Version**: Console → `bootstrap.Collapse.VERSION` → Should show 5.x.x

### Emergency Reset:
```javascript
// In browser console - manually fix navigation
$('#sidebar .nav-link, #sidebar .dropdown-item').removeClass('active');
var path = window.location.pathname;
$('#sidebar a').each(function() {
    var href = $(this).attr('href');
    if (href && path.indexOf(href) === 0 && href !== '#') {
        $(this).addClass('active');
        var collapse = $(this).closest('.collapse');
        if (collapse.length) {
            collapse.addClass('show');
            $('[data-bs-target="#' + collapse.attr('id') + '"]').addClass('active');
        }
    }
});
```

---

## 🎓 Understanding the Fix

### Before (Broken)
```
User clicks link
   ↓
site.js event: e.preventDefault()  ❌ Blocks navigation
   ↓
Nothing happens
```

### After (Fixed)
```
User clicks link
   ↓
site.js checks: Is href="#"?
   ├─ YES → e.preventDefault() (stay on page)
   └─ NO → Allow navigation ✅
           ↓
       Page loads
           ↓
   Navigation.init() runs
           ↓
   Active state set automatically
```

---

## 📱 Mobile Testing

### iOS Safari
1. Open in iOS device
2. Click hamburger menu
3. Click any working link
4. Should navigate and close sidebar

### Chrome Mobile
1. Open DevTools
2. Toggle device toolbar (Ctrl+Shift+M)
3. Select mobile device
4. Test navigation

### Responsive Testing
```css
/* Mobile sidebar behavior */
@media (max-width: 991px) {
    #sidebar {
        transform: translateX(-100%); /* Hidden by default */
    }
    #sidebar.open {
        transform: translateX(0); /* Visible when open */
    }
}
```

---

## ✨ Success Indicators

### You Know It's Fixed When:
- ✅ Clicking links changes URL
- ✅ Active link shows gold color
- ✅ Gold border appears on left
- ✅ Page title updates
- ✅ Dropdown parents highlight when child is active
- ✅ Dropdowns stay expanded on active child
- ✅ Mobile sidebar closes after navigation
- ✅ No Console errors
- ✅ Smooth, natural navigation experience

---

**Quick Help**: If something's not working, check the Console first. 90% of issues show up there as red errors.
