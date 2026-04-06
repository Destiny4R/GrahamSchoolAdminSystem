// ============================================================================
// SIDEBAR TOGGLE & RESPONSIVE MENU
// ============================================================================

const menuBtn = document.getElementById('menuBtn');
const sidebar = document.getElementById('sidebar');
const overlay = document.getElementById('overlay');

/**
 * Detect if we're on desktop (≥1200px)
 */
function isDesktop() {
    return window.innerWidth >= 1200;
}

/**
 * Open sidebar (add open class and show overlay) - for mobile/tablet
 */
function openSidebar() { 
    if (!isDesktop()) {
        sidebar.classList.add('open'); 
        overlay.classList.add('show'); 
        document.body.style.overflow = 'hidden'; // Prevent body scroll on mobile
    }
}

/**
 * Close sidebar (remove open class and hide overlay) - for mobile/tablet
 */
function closeSidebar() { 
    sidebar.classList.remove('open'); 
    overlay.classList.remove('show'); 
    document.body.style.overflow = 'auto'; // Re-enable body scroll
}

/**
 * Hide sidebar on desktop (add hidden class)
 */
function hideSidebar() {
    if (isDesktop()) {
        sidebar.classList.add('hidden');
    }
}

/**
 * Show sidebar on desktop (remove hidden class)
 */
function showSidebar() {
    if (isDesktop()) {
        sidebar.classList.remove('hidden');
    }
}

/**
 * Toggle sidebar - handles both mobile (overlay) and desktop (expansion)
 */
function toggleSidebar() {
    if (isDesktop()) {
        // Desktop: toggle hidden class
        sidebar.classList.toggle('hidden');
    } else {
        // Mobile/Tablet: toggle open class with overlay
        if (sidebar.classList.contains('open')) {
            closeSidebar();
        } else {
            openSidebar();
        }
    }
}

// Toggle button click handler - works on ALL screen sizes
if (menuBtn) {
    menuBtn.addEventListener('click', (e) => {
        e.stopPropagation();
        toggleSidebar();
    });
}

// Overlay click handler - close sidebar when overlay is clicked
if (overlay) {
    overlay.addEventListener('click', closeSidebar);
}

// Close sidebar when clicking on a navigation link (especially on mobile)
document.querySelectorAll('#sidebar .nav-link').forEach(link => {
    link.addEventListener('click', function (e) {
        const href = this.getAttribute('href') || '';
        const isDropdownToggle = this.classList.contains('dropdown-toggle') || 
                                 this.getAttribute('data-bs-toggle') === 'collapse';

        // Only prevent default for placeholder links or dropdown toggles
        if (href === '#' || isDropdownToggle) {
            e.preventDefault();
        }

        // Close sidebar on mobile after clicking a real navigation link
        if (!isDesktop() && href !== '#' && !isDropdownToggle) {
            setTimeout(() => closeSidebar(), 150);
        }
    });
});

// Handle window resize - adjust sidebar state based on screen size
let lastWindowWidth = window.innerWidth;
window.addEventListener('resize', () => {
    const currentWidth = window.innerWidth;
    const wasDesktop = lastWindowWidth >= 1200;
    const isNowDesktop = currentWidth >= 1200;

    // If resizing from mobile to desktop, show sidebar and remove overlay classes
    if (!wasDesktop && isNowDesktop) {
        showSidebar();
        closeSidebar(); // Remove overlay classes
    }
    // If resizing from desktop to mobile, hide overlay and remove hidden class
    else if (wasDesktop && !isNowDesktop) {
        showSidebar(); // Remove hidden class
        closeSidebar(); // Hide overlay
    }

    lastWindowWidth = currentWidth;
});

// Prevent sidebar from being scrollable past its content
if (sidebar) {
    sidebar.addEventListener('touchmove', (e) => {
        const isScrollable = sidebar.scrollHeight > sidebar.clientHeight;
        if (!isScrollable) {
            e.preventDefault();
        }
    }, { passive: false });
}


/* ── DROPDOWN HELPER ── */
function setupDropdown(btnId, panelId, wrapId) {
    const btn = document.getElementById(btnId);
    const panel = document.getElementById(panelId);
    if (!btn || !panel) return;
    btn.addEventListener('click', (e) => {
        e.stopPropagation();
        const isOpen = panel.classList.contains('open');
        closeAllDropdowns();
        if (!isOpen) panel.classList.add('open');
    });
}

function closeAllDropdowns() {
    document.querySelectorAll('.notif-panel, .profile-dropdown').forEach(p => p.classList.remove('open'));
}

setupDropdown('notifBtn', 'notifPanel', 'notifWrap');
setupDropdown('msgBtn', 'msgPanel', 'msgWrap');
setupDropdown('profileBtn', 'profileDropdown', 'profileWrap');

// Close on outside click
document.addEventListener('click', (e) => {
    if (!e.target.closest('.notif-wrap') && !e.target.closest('.profile-wrap')) {
        closeAllDropdowns();
    }
});

// Mark all read — notifications
document.querySelector('#notifPanel .drop-mark')?.addEventListener('click', () => {
    document.querySelectorAll('#notifPanel .notif-item.unread').forEach(i => i.classList.remove('unread'));
    document.querySelector('#notifPanel .unread-count').style.display = 'none';
    document.querySelector('#notifBtn .notif-dot').style.display = 'none';
});

// Mark all read — messages
document.querySelector('#msgPanel .drop-mark')?.addEventListener('click', () => {
    document.querySelectorAll('#msgPanel .msg-item.unread').forEach(i => i.classList.remove('unread'));
    document.querySelectorAll('#msgPanel .msg-unread-dot').forEach(d => d.style.display = 'none');
    document.querySelector('#msgPanel .unread-count').style.display = 'none';
    document.querySelector('#msgBtn .notif-dot').style.display = 'none';
});

/* ── DARK MODE TOGGLE ── */
const darkBtn = document.getElementById('darkToggle');
let darkMode = false;
darkBtn?.addEventListener('click', () => {
    darkMode = !darkMode;
    document.body.classList.toggle('dark-mode', darkMode);
    darkBtn.querySelector('i').className = darkMode ? 'bi bi-sun-fill' : 'bi bi-moon';
});