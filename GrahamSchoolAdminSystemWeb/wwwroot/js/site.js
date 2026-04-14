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

// Pending payment notifications
function loadPendingNotifications() {
    var notifList = document.getElementById('notifList');
    var notifCount = document.getElementById('notifCount');
    var notifDot = document.getElementById('notifDot');
    if (!notifList) return;

    $.ajax({
        url: '/api/v1/studentpayments/pending-notifications',
        method: 'GET',
        success: function (res) {
            if (!res.success) {
                notifList.innerHTML = '<div class="text-center text-muted py-3" style="font-size:.85rem;">Unable to load</div>';
                return;
            }
            var items = res.data || [];
            var count = res.count || 0;

            // Update badge and dot
            if (notifCount) {
                notifCount.textContent = count;
                notifCount.style.display = count > 0 ? '' : 'none';
            }
            if (notifDot) {
                notifDot.style.display = count > 0 ? '' : 'none';
            }

            if (items.length === 0) {
                notifList.innerHTML = '<div class="text-center text-muted py-3" style="font-size:.85rem;">No pending payments</div>';
                return;
            }

            var html = '';
            for (var i = 0; i < items.length; i++) {
                var n = items[i];
                var amt = Number(n.amount || 0).toLocaleString('en-NG', { minimumFractionDigits: 0 });
                html += '<a href="/admin/student-payments/detail/' + n.paymentId + '" class="notif-item unread" style="text-decoration:none;color:inherit;display:flex;">'
                    + '<div class="notif-icon" style="background:#e8f4ff;color:#3b82f6;"><i class="bi bi-cash-coin"></i></div>'
                    + '<div>'
                    + '<div class="notif-text">Payment of <strong>\u20A6' + amt + '</strong> by ' + (n.studentName || 'Unknown') + ' (' + (n.className || '') + ')</div>'
                    + '<div class="notif-time"><i class="bi bi-clock"></i> ' + (n.timeAgo || '') + ' &middot; ' + (n.reference || '') + '</div>'
                    + '</div></a>';
            }
            notifList.innerHTML = html;
        },
        error: function () {
            notifList.innerHTML = '<div class="text-center text-muted py-3" style="font-size:.85rem;">Unable to load</div>';
        }
    });
}

// Load on page ready, connect SignalR, and refresh every 60 seconds as fallback
$(function () {
    loadPendingNotifications();
    setInterval(loadPendingNotifications, 60000);

    // Only connect SignalR if the notification bell is present (user has REPORT permission)
    if (!document.getElementById('notifWrap')) return;

    // Configure Toastr defaults
    toastr.options = {
        closeButton: true,
        progressBar: true,
        positionClass: 'toast-top-right',
        timeOut: 10000,
        extendedTimeOut: 3000,
        showEasing: 'swing',
        hideEasing: 'linear',
        showMethod: 'fadeIn',
        hideMethod: 'fadeOut'
    };

    // Build SignalR connection
    var connection = new signalR.HubConnectionBuilder()
        .withUrl('/hubs/payment-notifications')
        .withAutomaticReconnect()
        .build();

    // New payment received — show clickable toast + refresh bell
    connection.on('NewPaymentReceived', function (data) {
        var amt = Number(data.amount || 0).toLocaleString('en-NG', { minimumFractionDigits: 0 });
        var msg = '<div style="cursor:pointer"><strong>New Payment</strong><br/>'
            + '\u20A6' + amt + ' submitted by ' + (data.createdBy || 'Unknown')
            + '<br/><small>Click to review</small></div>';

        var toast = toastr.info(msg, '', {
            timeOut: 10000,
            onclick: function () {
                window.location.href = '/admin/student-payments/detail/' + data.paymentId;
            }
        });

        loadPendingNotifications();
    });

    // Payment state changed — refresh the bell list
    connection.on('PaymentStateChanged', function (data) {
        loadPendingNotifications();
    });

    // Start connection with retry
    function startConnection() {
        connection.start()
            .then(function () {
                console.log('SignalR connected to payment notifications.');
            })
            .catch(function (err) {
                console.error('SignalR connection error:', err);
                setTimeout(startConnection, 5000);
            });
    }

    connection.onclose(function () {
        console.log('SignalR disconnected. Reconnecting...');
        setTimeout(startConnection, 5000);
    });

    startConnection();
});

// Mark all read — messages
document.querySelector('#msgPanel .drop-mark')?.addEventListener('click', () => {
    document.querySelectorAll('#msgPanel .msg-item.unread').forEach(i => i.classList.remove('unread'));
    document.querySelectorAll('#msgPanel .msg-unread-dot').forEach(d => d.style.display = 'none');
    document.querySelector('#msgPanel .unread-count').style.display = 'none';
    document.querySelector('#msgBtn .notif-dot').style.display = 'none';
});

/* ── SIDEBAR ACTIVE STATE ── */
$(function () {
    var currentPath = window.location.pathname.replace(/\/+$/, '').toLowerCase();

    // Remove any hardcoded active class
    $('#sidebar .nav-link.active, #sidebar .dropdown-item.active').removeClass('active');

    // Try matching a dropdown-item first (child links)
    var matched = false;
    $('#sidebar .dropdown-menu-list .dropdown-item').each(function () {
        var href = ($(this).attr('href') || '').replace(/\/+$/, '').toLowerCase();
        if (href && href !== '#' && currentPath === href) {
            $(this).addClass('active');
            // Expand the parent collapse and mark the parent toggle active
            var collapseDiv = $(this).closest('.collapse');
            collapseDiv.addClass('show');
            collapseDiv.closest('.nav-dropdown').find('> .nav-link').addClass('active')
                .attr('aria-expanded', 'true');
            matched = true;
            return false; // break
        }
    });

    // If no dropdown-item matched, try top-level nav-links
    if (!matched) {
        $('#sidebar > .nav-link, #sidebar > div > .nav-link').not('[data-bs-toggle="collapse"]').each(function () {
            var href = ($(this).attr('href') || '').replace(/\/+$/, '').toLowerCase();
            if (href && href !== '#' && currentPath === href) {
                $(this).addClass('active');
                matched = true;
                return false; // break
            }
        });
    }

    // Fallback: if nothing matched and we're on the root/dashboard, activate Dashboard
    if (!matched && (currentPath === '' || currentPath === '/')) {
        $('#sidebar .nav-link[href="/"]').first().addClass('active');
    }

    // Click handler: set active on click for nav-links and dropdown-items
    $('#sidebar .nav-link:not([data-bs-toggle="collapse"]), #sidebar .dropdown-item').on('click', function () {
        var href = $(this).attr('href') || '';
        if (href === '#') return;

        // Clear all active states
        $('#sidebar .nav-link, #sidebar .dropdown-item').removeClass('active');

        if ($(this).hasClass('dropdown-item')) {
            $(this).addClass('active');
            $(this).closest('.nav-dropdown').find('> .nav-link').addClass('active');
        } else {
            $(this).addClass('active');
        }
    });
});

/* ── DARK MODE TOGGLE ── */
const darkBtn = document.getElementById('darkToggle');
let darkMode = false;
darkBtn?.addEventListener('click', () => {
    darkMode = !darkMode;
    document.body.classList.toggle('dark-mode', darkMode);
    darkBtn.querySelector('i').className = darkMode ? 'bi bi-sun-fill' : 'bi bi-moon';
});