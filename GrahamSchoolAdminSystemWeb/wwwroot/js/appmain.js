function blockcallback() {
    $.blockUI({
        message: '<div class="bs-spinner mt-4 mt-lg-0"><div class= "spinner-border text-success mr-2 mt-2" style="width: 10rem; height: 10rem;" role="status"><span class="sr-only">Loading...</span></div> ',
        fadeIn: 800,
        overlayCSS: {
            backgroundColor: '#1b2024',
            opacity: 0.8,
            zIndex: 1200,
            cursor: 'wait'
        },
        css: {
            border: 0,
            color: '#fff',
            zIndex: 1201,
            padding: 0,
            backgroundColor: 'transparent'
        },
        onBlock: function () {

        }
    });
}

// ============================================================================
// ACADEMIC SESSION MODULE
// ============================================================================
var AcademicSession = (function() {
    'use strict';

    var academicSessionTable = null;

    // Initialize Academic Session DataTable
    function initDataTable() {
        if ($("#academicSessionTable").length) {
            academicSessionTable = $('#academicSessionTable').DataTable({
                serverSide: true,
                processing: true,
                searching: true,
                paging: true,
                info: true,
                responsive: true,
                language: {
                    processing: '<div class="spinner-border text-primary" role="status"><span class="visually-hidden">Loading...</span></div>'
                },
                ajax: {
                    url: "/home/GetAcademicSessionsDataTable",
                    type: "POST",
                    dataType: 'json',
                    error: function (xhr, error, thrown) {
                        console.error('DataTable error:', error, thrown);
                        Swal.fire({
                            icon: 'error',
                            title: 'Error Loading Data',
                            text: 'Failed to load academic sessions. Please refresh the page.'
                        });
                    }
                },
                columns: [
                    {
                        data: null,
                        orderable: false,
                        searchable: false,
                        width: "5%",
                        render: function (data, type, row, meta) {
                            return meta.settings._iDisplayStart + meta.row + 1;
                        }
                    },
                    {
                        data: "name",
                        width: "40%",
                        render: function (data) {
                            return `<strong>${data}</strong>`;
                        }
                    },
                    {
                        data: "createdate",
                        width: "30%",
                        render: function (data) {
                            if (data) {
                                const date = new Date(data);
                                return date.toLocaleDateString('en-US', {
                                    year: 'numeric',
                                    month: 'short',
                                    day: 'numeric',
                                    hour: '2-digit',
                                    minute: '2-digit'
                                });
                            }
                            return 'N/A';
                        }
                    },
                    {
                        data: null,
                        orderable: false,
                        searchable: false,
                        width: "15%",
                        render: function (data, type, row) {
                            var perms = window.userPermissions || {};
                            var editBtn = perms.canEdit ? `<button class="btn btn-sm btn-edit-session" 
                                            data-id="${row.id}" 
                                            data-name="${row.name}"
                                            title="Edit">
                                        <i class="bi bi-pencil-square"></i>
                                    </button>` : '';
                            var deleteBtn = perms.canDelete ? `<button class="btn btn-sm btn-delete-session" 
                                            data-id="${row.id}" 
                                            data-name="${row.name}"
                                            title="Delete">
                                        <i class="bi bi-trash"></i>
                                    </button>` : '';
                            return `<div class="action-buttons">${editBtn}${deleteBtn}</div>`;
                        }
                    }
                ]
            });

            // Search functionality
            $('#searchInput').on('keyup', function () {
                if (academicSessionTable) {
                    academicSessionTable.search(this.value).draw();
                }
            });
        }
    }

    // Edit button click handler
    function handleEdit() {
        $(document).on('click', '.btn-edit-session', function () {
            const id = $(this).data('id');
            const name = $(this).data('name');

            $('#editId').val(id);
            $('#editName').val(name);

            $('#editModal').modal('show');
        });
    }

    // Delete button click handler
    function handleDelete() {
        $(document).on('click', '.btn-delete-session', function () {
            const id = $(this).data('id');
            const name = $(this).data('name');

            Swal.fire({
                title: 'Delete Academic Session?',
                html: `Are you sure you want to delete <strong>${name}</strong>?<br><br>
                       <span class="text-danger">⚠️ This action cannot be undone.</span>`,
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#d33',
                cancelButtonColor: '#6c757d',
                confirmButtonText: '<i class="bi bi-trash me-1"></i> Yes, Delete',
                cancelButtonText: '<i class="bi bi-x-circle me-1"></i> Cancel'
            }).then((result) => {
                if (result.isConfirmed) {
                    deleteSession(id);
                }
            });
        });
    }

    // Delete academic session via AJAX
    function deleteSession(id) {
        $.ajax({
            url: '/admin/academicsession/index?handler=Delete',
            type: 'POST',
            data: {
                id: id,
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            beforeSend: function () {
                Swal.fire({
                    title: 'Deleting...',
                    text: 'Please wait',
                    allowOutsideClick: false,
                    allowEscapeKey: false,
                    didOpen: () => {
                        Swal.showLoading();
                    }
                });
            },
            success: function (response) {
                Swal.close();
                if (academicSessionTable) {
                    academicSessionTable.ajax.reload(null, false);
                }
                Swal.fire({
                    icon: 'success',
                    title: 'Deleted!',
                    text: 'Academic session has been deleted successfully.',
                    timer: 2000,
                    showConfirmButton: false
                });
            },
            error: function (xhr) {
                Swal.fire({
                    icon: 'error',
                    title: 'Delete Failed',
                    text: xhr.responseJSON?.message || 'An error occurred while deleting the academic session.'
                });
            }
        });
    }

    // Reset form function
    function resetForm() {
        $('#createModal form')[0].reset();
        $('#createModal .invalid-feedback').hide();
        $('#createModal .form-control').removeClass('is-invalid');
    }

    // Initialize all handlers
    function init() {
        initDataTable();
        handleEdit();
        handleDelete();

        // Expose resetForm to global scope for onclick attribute
        window.AcademicSessionResetForm = resetForm;
    }

    // Public API
    return {
        init: init,
        resetForm: resetForm
    };
})();

// Initialize Academic Session module when document is ready
$(document).ready(function() {
    if ($("#academicSessionTable").length) {
        AcademicSession.init();
    }
});

// ============================================================================
// SCHOOL SUB-CLASS MODULE
// ============================================================================
var SchoolSubClass = (function() {
    'use strict';

    var subClassTable = null;

    // Initialize School Sub-Class DataTable
    function initDataTable() {
        if ($("#subClassTable").length) {
            subClassTable = $('#subClassTable').DataTable({
                serverSide: true,
                processing: true,
                searching: true,
                paging: true,
                info: true,
                responsive: true,
                language: {
                    processing: '<div class="spinner-border text-primary" role="status"><span class="visually-hidden">Loading...</span></div>'
                },
                ajax: {
                    url: "/home/GetSchoolSubClassesDataTable",
                    type: "POST",
                    dataType: 'json',
                    error: function (xhr, error, thrown) {
                        console.error('DataTable error:', error, thrown);
                        Swal.fire({
                            icon: 'error',
                            title: 'Error Loading Data',
                            text: 'Failed to load school sub-classes. Please refresh the page.'
                        });
                    }
                },
                columns: [
                    {
                        data: null,
                        orderable: false,
                        searchable: false,
                        width: "5%",
                        render: function (data, type, row, meta) {
                            return meta.settings._iDisplayStart + meta.row + 1;
                        }
                    },
                    {
                        data: "name",
                        width: "40%",
                        render: function (data) {
                            return `<strong>${data}</strong>`;
                        }
                    },
                    {
                        data: "createdate",
                        width: "30%",
                        render: function (data) {
                            if (data) {
                                const date = new Date(data);
                                return date.toLocaleDateString('en-US', {
                                    year: 'numeric',
                                    month: 'short',
                                    day: 'numeric',
                                    hour: '2-digit',
                                    minute: '2-digit'
                                });
                            }
                            return 'N/A';
                        }
                    },
                    {
                        data: null,
                        orderable: false,
                        searchable: false,
                        width: "15%",
                        render: function (data, type, row) {
                            var perms = window.userPermissions || {};
                            var editBtn = perms.canEdit ? `<button class="btn btn-sm btn-edit-subclass" 
                                            data-id="${row.id}" 
                                            data-name="${row.name}"
                                            title="Edit">
                                        <i class="bi bi-pencil-square"></i>
                                    </button>` : '';
                            var deleteBtn = perms.canDelete ? `<button class="btn btn-sm btn-delete-subclass" 
                                            data-id="${row.id}" 
                                            data-name="${row.name}"
                                            title="Delete">
                                        <i class="bi bi-trash"></i>
                                    </button>` : '';
                            return `<div class="action-buttons">${editBtn}${deleteBtn}</div>`;
                        }
                    }
                ]
            });

            // Search functionality
            $('#searchInput').on('keyup', function () {
                if (subClassTable) {
                    subClassTable.search(this.value).draw();
                }
            });
        }
    }

    // Edit button click handler
    function handleEdit() {
        $(document).on('click', '.btn-edit-subclass', function () {
            const id = $(this).data('id');
            const name = $(this).data('name');

            $('#editId').val(id);
            $('#editName').val(name);

            $('#editModal').modal('show');
        });
    }

    // Delete button click handler
    function handleDelete() {
        $(document).on('click', '.btn-delete-subclass', function () {
            const id = $(this).data('id');
            const name = $(this).data('name');

            Swal.fire({
                title: 'Delete Sub-Class?',
                html: `Are you sure you want to delete <strong>${name}</strong>?<br><br>
                       <span class="text-danger">⚠️ This action cannot be undone.</span>`,
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#d33',
                cancelButtonColor: '#6c757d',
                confirmButtonText: '<i class="bi bi-trash me-1"></i> Yes, Delete',
                cancelButtonText: '<i class="bi bi-x-circle me-1"></i> Cancel'
            }).then((result) => {
                if (result.isConfirmed) {
                    deleteSubClass(id);
                }
            });
        });
    }

    // Delete sub-class via AJAX
    function deleteSubClass(id) {
        $.ajax({
            url: '/admin/sub-class/index?handler=Delete',
            type: 'POST',
            data: {
                id: id,
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            beforeSend: function () {
                Swal.fire({
                    title: 'Deleting...',
                    text: 'Please wait',
                    allowOutsideClick: false,
                    allowEscapeKey: false,
                    didOpen: () => {
                        Swal.showLoading();
                    }
                });
            },
            success: function (response) {
                Swal.close();
                if (subClassTable) {
                    subClassTable.ajax.reload(null, false);
                }
                Swal.fire({
                    icon: 'success',
                    title: 'Deleted!',
                    text: 'Sub-class has been deleted successfully.',
                    timer: 2000,
                    showConfirmButton: false
                });
            },
            error: function (xhr) {
                Swal.fire({
                    icon: 'error',
                    title: 'Delete Failed',
                    text: xhr.responseJSON?.message || 'An error occurred while deleting the sub-class.'
                });
            }
        });
    }

    // Reset form function
    function resetForm() {
        $('#createModal form')[0].reset();
        $('#createModal .invalid-feedback').hide();
        $('#createModal .form-control').removeClass('is-invalid');
    }

    // Initialize all handlers
    function init() {
        initDataTable();
        handleEdit();
        handleDelete();

        // Expose resetForm to global scope for onclick attribute
        window.SchoolSubClassResetForm = resetForm;
    }

    // Public API
    return {
        init: init,
        resetForm: resetForm
    };
})();

// Initialize School Sub-Class module when document is ready
$(document).ready(function() {
    if ($("#subClassTable").length) {
        SchoolSubClass.init();
    }
});

// ═══════════════════════════════════════════════════════════════
// NAVIGATION MODULE - Sidebar Active State Management
// ═══════════════════════════════════════════════════════════════
var Navigation = (function () {
    'use strict';

    // Get current page path
    function getCurrentPath() {
        return window.location.pathname;
    }

    // Set active nav link based on current URL
    function setActiveNavLink() {
        var currentPath = getCurrentPath();

        // Remove all existing active states
        $('#sidebar .nav-link').removeClass('active');
        $('#sidebar .dropdown-item').removeClass('active');

        // Find and activate matching link
        var foundMatch = false;

        // First check dropdown items (more specific paths)
        $('#sidebar .dropdown-item').each(function() {
            var href = $(this).attr('href');
            if (href && href !== '#' && currentPath.toLowerCase().indexOf(href.toLowerCase()) === 0) {
                $(this).addClass('active');

                // Also add active state to parent dropdown toggle
                var parentCollapse = $(this).closest('.collapse');
                if (parentCollapse.length) {
                    var parentToggle = $('[data-bs-target="#' + parentCollapse.attr('id') + '"]');
                    parentToggle.addClass('active');
                    // Expand the parent dropdown
                    parentCollapse.addClass('show');
                }

                // Update page title
                updatePageTitle($(this).text().trim());
                foundMatch = true;
                return false; // break loop
            }
        });

        // If no dropdown item matched, check regular nav links
        if (!foundMatch) {
            $('#sidebar .nav-link:not(.dropdown-toggle)').each(function() {
                var href = $(this).attr('href');
                var aspPage = $(this).attr('asp-page');

                // Check both href and asp-page attributes
                var checkPath = href || aspPage;

                if (checkPath && checkPath !== '#' && currentPath.toLowerCase().indexOf(checkPath.toLowerCase()) === 0) {
                    $(this).addClass('active');
                    updatePageTitle($(this).text().trim());
                    foundMatch = true;
                    return false; // break loop
                }
            });
        }

        // Fallback to Dashboard if no match found and we're at root
        if (!foundMatch && (currentPath === '/' || currentPath === '/Index' || currentPath === '/index')) {
            $('#sidebar .nav-link:first').addClass('active');
        }
    }

    // Update the topbar page title
    function updatePageTitle(linkText) {
        if (!linkText) return;

        // Clean up the text (remove icons, badges, extra whitespace)
        var cleanText = linkText.replace(/\s+/g, ' ').trim();

        // Remove any numbers in parentheses or badges
        cleanText = cleanText.replace(/\(\d+\)/g, '').replace(/\d+$/g, '').trim();

        // Update the topbar title
        $('#topbar .page-title').text(cleanText);
    }

    // Initialize navigation
    function init() {
        // Set active state on page load
        setActiveNavLink();

        // Handle dropdown item clicks
        $('#sidebar .dropdown-item').on('click', function(e) {
            var href = $(this).attr('href');
            // Only prevent default if it's a placeholder
            if (!href || href === '#') {
                e.preventDefault();
            }
            // Let real links navigate naturally
        });
    }

    // Public API
    return {
        init: init,
        setActive: setActiveNavLink
    };
})();

// Initialize Navigation module when document is ready
$(document).ready(function() {
    Navigation.init();
});

// ═══════════════════════════════════════════════════════════════
// SCHOOL CLASS MODULE - Manage School Classes
// ═══════════════════════════════════════════════════════════════
var SchoolClass = (function () {
    'use strict';

    // Private variables
    var classTable = null;

    // Initialize DataTable
    function initDataTable() {
        classTable = $('#schoolClassesTable').DataTable({
            serverSide: true,
            processing: true,
            searching: true,
            paging: true,
            info: true,
            responsive: true,
            ajax: {
                url: '/home/GetSchoolClassesDataTable',
                type: 'POST',
                dataType: 'json'
            },
            columns: [
                {
                    data: null,
                    orderable: false,
                    searchable: false,
                    render: function (data, type, row, meta) {
                        return meta.settings._iDisplayStart + meta.row + 1;
                    }
                },
                { data: 'name', autoWidth: true },
                {
                    "data": "createdate", "autoWidth": true,
                    render: function (data, type, row) {
                        if (data != null) {
                            var hours = new Date(data).getHours()
                            let ap = hours >= 12 ? 'pm' : 'am';
                            return data = data.toLocaleString('YYYY-MM-dd').slice(0, 19).replace('T', ' ') + ' ' + ap;
                        } else {
                            return "null";
                        }
                    }
                },
                {
                    data: null,
                    orderable: false,
                    render: function (data, type, row) {
                        var perms = window.userPermissions || {};
                        var editBtn = perms.canEdit ? `<button type="button" class="btn btn-outline-primary btn-edit-class" data-id="${row.id}" data-name="${row.name}" title="Edit">
                                    <i class="bi bi-pencil-fill"></i>
                                </button>` : '';
                        var deleteBtn = perms.canDelete ? `<button type="button" class="btn btn-outline-danger btn-delete-class" data-id="${row.id}" data-name="${row.name}" title="Delete">
                                    <i class="bi bi-trash-fill"></i>
                                </button>` : '';
                        return `<div class="btn-group btn-group-sm" role="group">${editBtn}${deleteBtn}</div>`;
                    }
                }
            ],
            order: [[1, 'asc']],
            language: {
                processing: '<div class="spinner-border text-primary" role="status"><span class="visually-hidden">Loading...</span></div>',
                emptyTable: 'No school classes found',
                zeroRecords: 'No matching school classes found'
            }
        });

        // Bind search input
        $('#searchInput').on('keyup', function () {
            classTable.search(this.value).draw();
        });
    }

    // Handle edit button click
    function handleEdit() {
        $(document).on('click', '.btn-edit-class', function () {
            var id = $(this).data('id');
            var name = $(this).data('name');

            // Populate edit modal
            $('#editClassId').val(id);
            $('#editClassName').val(name);

            // Show edit modal
            var editModal = new bootstrap.Modal(document.getElementById('schoolClassEditModal'));
            editModal.show();
        });
    }

    // Handle delete button click
    function handleDelete() {
        $(document).on('click', '.btn-delete-class', function () {
            var id = $(this).data('id');
            var name = $(this).data('name');

            Swal.fire({
                title: 'Delete School Class?',
                html: `Are you sure you want to delete <strong>${name}</strong>?<br><small class="text-muted">This action cannot be undone.</small>`,
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#d33',
                cancelButtonColor: '#6c757d',
                confirmButtonText: '<i class="bi bi-trash me-1"></i>Yes, delete it',
                cancelButtonText: '<i class="bi bi-x-circle me-1"></i>Cancel'
            }).then((result) => {
                if (result.isConfirmed) {
                    fetch(`/api/v1/schoolclasses/${id}`, {
                        method: 'DELETE',
                        headers: {
                            'Content-Type': 'application/json',
                            'RequestVerificationToken': getAntiForgeryToken()
                        }
                    })
                    .then(response => response.json())
                    .then(data => {
                        if (data.success) {
                            Swal.fire('Deleted!', data.message, 'success');
                            if (schoolClassTable) {
                                schoolClassTable.ajax.reload();
                            }
                        } else {
                            Swal.fire('Error!', data.message || 'Failed to delete', 'error');
                        }
                    })
                    .catch(error => {
                        console.error('Error:', error);
                        Swal.fire('Error!', 'An unexpected error occurred', 'error');
                    });
                }
            });
        });
    }

    // ============================================
    // STUDENT MANAGEMENT SECTION
    // ============================================

    // Initialize Student DataTable
    if ($('#studentsTable').length) {
        var studentsTable = $('#studentsTable').DataTable({
            processing: true,
            serverSide: true,
            responsive: true,
            ajax: {
                url: '/home/GetStudentsDataTable',
                type: 'POST',
                dataType: 'json',
                data: function (d) {
                    d.searchValue = $('#studentSearchInput').val();
                }
            },
            columns: [
                {
                    data: null,
                    orderable: false,
                    searchable: false,
                    render: function (data, type, row, meta) {
                        return meta.settings._iDisplayStart + meta.row + 1;
                    }
                },
                { data: 'fullName' },
                { data: 'email' },
                { data: 'genderDisplay' },
                {
                    data: 'isActive',
                    render: function (data) {
                        return data
                            ? '<span class="badge bg-success"><i class="bi bi-check-circle me-1"></i>Active</span>'
                            : '<span class="badge bg-danger"><i class="bi bi-x-circle me-1"></i>Inactive</span>';
                    }
                },
                {
                    data: 'createdDate',
                    render: function (data) {
                        if (data) {
                            const date = new Date(data);
                            return date.toLocaleDateString('en-US', {
                                year: 'numeric',
                                month: 'short',
                                day: 'numeric',
                                hour: '2-digit',
                                minute: '2-digit'
                            });
                        }
                        return 'N/A';
                    }
                },
                {
                    data: null,
                    orderable: false,
                    searchable: false,
                    render: function (data, type, row) {
                        var perms = window.userPermissions || {};
                        let icon = '', classes = "";
                        if (row.isActive) {
                            icon = "lock";
                            classes = "Deactivate";
                        } else {
                            icon = "unlock";
                            classes = "Activate";
                        }
                        var items = '';
                        if (perms.canEdit) {
                            items += `<li><button class="dropdown-item btn-edit-student" data-id="${row.id}">
                                <i class="bi bi-pencil-square"></i> Edit
                            </button></li>`;
                            items += `<li><button class="dropdown-item btn-student-actionToggle" data-active="${row.isActive}" data-id="${row.id}">
                                <i class="bi bi-${icon} me-2"></i> ${classes}
                            </button></li>`;
                        }
                        if (perms.canDelete) {
                            if (items) items += `<li><hr class="dropdown-divider"></li>`;
                            items += `<li><button class="dropdown-item btn-delete-student" data-id="${row.id}"><i class="bi bi-trash me-2"></i>Delete</button></li>`;
                        }
                        if (!items) return '';
                        return `
                        <div class="">
                          <button type="button" class="btn btn-primary btn-sm" data-bs-toggle="dropdown" aria-expanded="false">
                            <i class="bi bi-three-dots-vertical"></i>
                          </button>
                          <ul class="dropdown-menu">
                            ${items}
                          </ul>
                        </div>
                        `;
                    }
                }
            ],
            order: [[5, 'desc']],
            language: {
                processing: '<div class="spinner-border text-primary" role="status"><span class="visually-hidden">Loading...</span></div>'
            }
        });

        $(document).on('click', '.btn-delete-student', function () {
            const id = $(this).data('id');
            // call API to toggle
            const swalWithBootstrapButtons = Swal.mixin({
                customClass: {
                    confirmButton: "btn btn-success rounded-pill btn-sm",
                    cancelButton: "btn btn-danger  rounded-pill btn-sm me-2"
                },
                buttonsStyling: false
            });
            swalWithBootstrapButtons.fire({
                title: "DELETION",
                text: "Are you sure you want to remove this student, this can not be undone!",
                icon: "warning",
                showCancelButton: true,
                confirmButtonText: "Yes!",
                cancelButtonText: "No!",
                reverseButtons: true
            }).then((result) => {
                if (result.isConfirmed) {
                    $.ajax({
                        type: "DELETE",
                        url: `/api/v1/students/${id}`,
                        data: {
                            id: id
                        },
                        success: function (data) {
                            $.unblockUI();
                            if (data.success) {
                                swalWithBootstrapButtons.fire(
                                    'Information!',
                                    data.message,
                                    'success'
                                );
                                studentsTable.ajax.reload(null, false);
                            } else {
                                Swal.fire({
                                    icon: 'error',
                                    title: 'Oops...',
                                    text: data.message,
                                    footer: 'Message'
                                });
                            }
                        },
                        beforeSend: function () {
                            blockcallback();
                        },
                        error: function () {
                            $.unblockUI();
                        },
                        complete: function () {
                            $.unblockUI();
                        }
                    });

                }
            });
        });

        $(document).on('click', '.btn-student-actionToggle', function () {
            const id = $(this).data('id');
            const isActive = $(this).data('active');
            let msg = isActive ? "You want to deactivate this Student account?" : "You want to activate this Student account?";
            // call API to toggle
            const swalWithBootstrapButtons = Swal.mixin({
                customClass: {
                    confirmButton: "btn btn-success rounded-pill btn-sm",
                    cancelButton: "btn btn-danger  rounded-pill btn-sm me-2"
                },
                buttonsStyling: false
            });
            swalWithBootstrapButtons.fire({
                title: "Are you sure?",
                text: msg,
                icon: "warning",
                showCancelButton: true,
                confirmButtonText: "Yes!",
                cancelButtonText: "No!",
                reverseButtons: true
            }).then((result) => {
                if (result.isConfirmed) {
                    $.ajax({
                        type: "POST",
                        url: `/api/v1/students/${id}/activate`,
                        data: {
                            id: id
                        },
                        success: function (data) {
                            $.unblockUI();
                            if (data.success) {
                                swalWithBootstrapButtons.fire(
                                    'Information!',
                                    data.message,
                                    'success'
                                );
                                studentsTable.ajax.reload(null, false);
                            } else {
                                Swal.fire({
                                    icon: 'error',
                                    title: 'Oops...',
                                    text: data.message,
                                    footer: 'Message'
                                });
                            }
                        },
                        beforeSend: function () {
                            blockcallback();
                        },
                        error: function () {
                            $.unblockUI();
                        },
                        complete: function () {
                            $.unblockUI();
                        }
                    });

                }
            });
        });


        // Search functionality
        $('#studentSearchInput').on('keyup', function () {
            studentsTable.search(this.value).draw();
        });

        // Reset form
        window.resetStudentForm = function () {
            $('#studentForm')[0].reset();
            $('#studentId').val('');
            $('#studentModalTitle').text('Add Student');
            $('#studentModalSubtitle').text('Create new student information');
            $('#studentSubmitBtnText').text('Save');
        };

        // Handle form submission
        $('#studentForm').on('submit', function (e) {
            e.preventDefault();

            const studentId = $('#studentId').val();
            const formData = {
                id: studentId ? parseInt(studentId) : 0,
                surname: $('#studentSurname').val(),
                firstname: $('#studentFirstname').val(),
                othername: $('#studentOthername').val(),
                email: $('#studentEmail').val(),
                genderId: parseInt($('#studentGenderId').val())
            };

            const url = studentId
                ? '/api/v1/students/update'
                : '/api/v1/students/create';

            const method = studentId ? 'PUT' : 'POST';

            fetch(url, {
                method: method,
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': getAntiForgeryToken()
                },
                body: JSON.stringify(formData)
            })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    // Close modal
                    bootstrap.Modal.getInstance(document.getElementById('studentModal')).hide();
                    resetStudentForm();

                    // Show success message
                    showAlert('success', data.message);

                    // Reload table
                    studentsTable.ajax.reload();
                } else {
                    showAlert('danger', data.message || 'An error occurred');
                }
            })
            .catch(error => {
                console.error('Error:', error);
                showAlert('danger', 'An unexpected error occurred');
            });
        });

        // Edit student
        $(document).on('click', '.btn-edit-student', function () {
            const studentId = $(this).data('id');

            fetch(`/api/v1/students/${studentId}`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            })
            .then(response => response.json())
            .then(data => {
                if (data.success && data.data) {
                    const student = data.data;
                    $('#studentId').val(student.id);
                    $('#studentSurname').val(student.surname);
                    $('#studentFirstname').val(student.firstname);
                    $('#studentOthername').val(student.othername);
                    $('#studentEmail').val(student.email);
                    $('#studentGenderId').val(student.genderId);

                    $('#studentModalTitle').text('Edit Student');
                    $('#studentModalSubtitle').text('Update student information');
                    $('#studentSubmitBtnText').text('Update');

                    const modal = new bootstrap.Modal(document.getElementById('studentModal'));
                    modal.show();
                } else {
                    showAlert('danger', 'Failed to load student data');
                }
            })
            .catch(error => {
                console.error('Error:', error);
                showAlert('danger', 'Error loading student data');
            });
        });
    }

    // Helper function to show alerts
    function showAlert(type, message) {
        const alertHtml = `
            <div class="alert alert-${type} alert-dismissible fade show" role="alert">
                <i class="bi bi-${type === 'success' ? 'check-circle-fill' : 'exclamation-circle-fill'} me-2"></i>
                <strong>${type === 'success' ? 'Success!' : 'Error!'}</strong> ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>
        `;
        $('body').prepend(alertHtml);

        // Auto-dismiss after 5 seconds
        setTimeout(() => {
            $('body > .alert').fadeOut(() => {
                $('body > .alert').remove();
            });
        }, 5000);
    }

    // Initialize all handlers
    function init() {
        initDataTable();
        handleEdit();
        handleDelete();
    }

    // Public API
    return {
        init: init,
        resetForm: function() {
            $('#schoolClassModal form')[0].reset();
            $('#schoolClassModal .invalid-feedback').hide();
            $('#schoolClassModal .form-control').removeClass('is-invalid');
        }
    };
})();

// Helper function to get Anti-Forgery Token
function getAntiForgeryToken() {
    return $('input[name="__RequestVerificationToken"]').val();
}

// Initialize School Class module when document is ready
$(document).ready(function() {
    if ($("#schoolClassesTable").length) {
        SchoolClass.init();
    }
});

// ═══════════════════════════════════════════════════════════════
// PAYMENT CATEGORY MODULE
// ═══════════════════════════════════════════════════════════════
var PaymentCategory = (function () {
    'use strict';

    var table = null;

    function initDataTable() {
        table = $('#paymentCategoriesTable').DataTable({
            serverSide: true,
            processing: true,
            searching: true,
            paging: true,
            info: true,
            responsive: true,
            ajax: {
                url: '/home/GetPaymentCategoriesDataTable',
                type: 'POST',
                dataType: 'json'
            },
            columns: [
                {
                    data: null, orderable: false, searchable: false,
                    render: function (data, type, row, meta) {
                        return meta.settings._iDisplayStart + meta.row + 1;
                    }
                },
                { data: 'name' },
                { data: 'description', render: function (d) { return d || '<span class="text-muted">—</span>'; } },
                {
                    data: 'isActive',
                    render: function (data) {
                        return data
                            ? '<span class="badge bg-success"><i class="bi bi-check-circle me-1"></i>Active</span>'
                            : '<span class="badge bg-danger"><i class="bi bi-x-circle me-1"></i>Inactive</span>';
                    }
                },
                {
                    data: 'createdAt',
                    render: function (data) {
                        if (data) {
                            var date = new Date(data);
                            return date.toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' });
                        }
                        return 'N/A';
                    }
                },
                {
                    data: null, orderable: false, searchable: false,
                    render: function (data, type, row) {
                        var perms = window.userPermissions || {};
                        var items = '';
                        if (perms.canEdit) {
                            items += '<li><button class="dropdown-item btn-edit-category" data-id="' + row.id + '"><i class="bi bi-pencil-square me-2"></i>Edit</button></li>';
                            var icon = row.isActive ? 'lock' : 'unlock';
                            var label = row.isActive ? 'Deactivate' : 'Activate';
                            items += '<li><button class="dropdown-item btn-toggle-category" data-id="' + row.id + '" data-active="' + row.isActive + '"><i class="bi bi-' + icon + ' me-2"></i>' + label + '</button></li>';
                        }
                        if (perms.canDelete) {
                            if (items) items += '<li><hr class="dropdown-divider"></li>';
                            items += '<li><button class="dropdown-item btn-delete-category" data-id="' + row.id + '" data-name="' + row.name + '"><i class="bi bi-trash me-2"></i>Delete</button></li>';
                        }
                        if (!items) return '';
                        return '<div><button type="button" class="btn btn-primary btn-sm" data-bs-toggle="dropdown"><i class="bi bi-three-dots-vertical"></i></button><ul class="dropdown-menu">' + items + '</ul></div>';
                    }
                }
            ],
            order: [[1, 'asc']],
            language: {
                processing: '<div class="spinner-border text-primary" role="status"><span class="visually-hidden">Loading...</span></div>',
                emptyTable: 'No payment categories found',
                zeroRecords: 'No matching categories found'
            }
        });

        $('#categorySearchInput').on('keyup', function () {
            table.search(this.value).draw();
        });
    }

    function handleCreate() {
        // Create form now uses native method="post" — no JS submit handler needed
    }

    function handleEdit() {
        $(document).on('click', '.btn-edit-category', function () {
            var id = $(this).data('id');
            fetch('/api/v1/paymentcategories/' + id, { method: 'GET', headers: { 'Content-Type': 'application/json' } })
                .then(function (r) { return r.json(); })
                .then(function (data) {
                    if (data.success && data.data) {
                        var cat = data.data;
                        $('#editCategoryId').val(cat.id);
                        $('#editCategoryName').val(cat.name);
                        $('#editCategoryDescription').val(cat.description);
                        $('#editCategoryIsActive').prop('checked', cat.isActive);
                        var modal = new bootstrap.Modal(document.getElementById('categoryEditModal'));
                        modal.show();
                    } else {
                        Swal.fire('Error!', 'Failed to load category data', 'error');
                    }
                });
        });

        // Edit form now uses native method="post" — no JS submit handler needed
    }

    function handleDelete() {
        $(document).on('click', '.btn-delete-category', function () {
            var id = $(this).data('id');
            var name = $(this).data('name');

            Swal.fire({
                title: 'Delete Category?',
                html: 'Are you sure you want to delete <strong>' + name + '</strong>?<br><small class="text-muted">This action cannot be undone.</small>',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#d33',
                cancelButtonColor: '#6c757d',
                confirmButtonText: '<i class="bi bi-trash me-1"></i>Yes, delete it',
                cancelButtonText: '<i class="bi bi-x-circle me-1"></i>Cancel'
            }).then(function (result) {
                if (result.isConfirmed) {
                    fetch('/api/v1/paymentcategories/' + id, {
                        method: 'DELETE',
                        headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': getAntiForgeryToken() }
                    })
                    .then(function (r) { return r.json(); })
                    .then(function (data) {
                        if (data.success) {
                            Swal.fire('Deleted!', data.message, 'success');
                            table.ajax.reload();
                        } else {
                            Swal.fire('Error!', data.message, 'error');
                        }
                    })
                    .catch(function () { Swal.fire('Error!', 'An unexpected error occurred', 'error'); });
                }
            });
        });
    }

    function handleToggle() {
        $(document).on('click', '.btn-toggle-category', function () {
            var id = $(this).data('id');
            var isActive = $(this).data('active');
            var msg = isActive ? 'Deactivate this category?' : 'Activate this category?';

            Swal.fire({
                title: 'Are you sure?',
                text: msg,
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: 'Yes',
                cancelButtonText: 'No'
            }).then(function (result) {
                if (result.isConfirmed) {
                    fetch('/api/v1/paymentcategories/' + id + '/toggle', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': getAntiForgeryToken() }
                    })
                    .then(function (r) { return r.json(); })
                    .then(function (data) {
                        if (data.success) {
                            Swal.fire('Done!', data.message, 'success');
                            table.ajax.reload();
                        } else {
                            Swal.fire('Error!', data.message, 'error');
                        }
                    });
                }
            });
        });
    }

    function resetForm() {
        if (document.getElementById('categoryForm')) {
            document.getElementById('categoryForm').reset();
        }
    }

    function init() {
        initDataTable();
        handleCreate();
        handleEdit();
        handleDelete();
        handleToggle();
    }

    return { init: init, resetForm: resetForm };
})();

// ═══════════════════════════════════════════════════════════════
// PAYMENT ITEM MODULE
// ═══════════════════════════════════════════════════════════════
var PaymentItem = (function () {
    'use strict';

    var table = null;

    function loadCategories(selectId, selectedVal) {
        fetch('/api/v1/paymentcategories/active', { method: 'GET', headers: { 'Content-Type': 'application/json' } })
            .then(function (r) { return r.json(); })
            .then(function (data) {
                if (data.success && data.data) {
                    var sel = $(selectId);
                    sel.find('option:not(:first)').remove();
                    data.data.forEach(function (c) {
                        sel.append('<option value="' + c.id + '"' + (c.id == selectedVal ? ' selected' : '') + '>' + c.name + '</option>');
                    });
                }
            });
    }

    function initDataTable() {
        // Load filter dropdown
        loadCategories('#filterCategory');

        table = $('#paymentItemsTable').DataTable({
            serverSide: true,
            processing: true,
            searching: true,
            paging: true,
            info: true,
            responsive: true,
            ajax: {
                url: '/home/GetPaymentItemsDataTable',
                type: 'POST',
                dataType: 'json',
                data: function (d) {
                    d.category = $('#filterCategory').val();
                }
            },
            columns: [
                {
                    data: null, orderable: false, searchable: false,
                    render: function (data, type, row, meta) {
                        return meta.settings._iDisplayStart + meta.row + 1;
                    }
                },
                { data: 'name' },
                { data: 'categoryName', render: function (d) { return d || '<span class="text-muted">—</span>'; } },
                { data: 'description', render: function (d) { return d || '<span class="text-muted">—</span>'; } },
                {
                    data: 'isActive',
                    render: function (data) {
                        return data
                            ? '<span class="badge bg-success"><i class="bi bi-check-circle me-1"></i>Active</span>'
                            : '<span class="badge bg-danger"><i class="bi bi-x-circle me-1"></i>Inactive</span>';
                    }
                },
                {
                    data: 'createdAt',
                    render: function (data) {
                        if (data) {
                            var date = new Date(data);
                            return date.toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' });
                        }
                        return 'N/A';
                    }
                },
                {
                    data: null, orderable: false, searchable: false,
                    render: function (data, type, row) {
                        var perms = window.userPermissions || {};
                        var items = '';
                        if (perms.canEdit) {
                            items += '<li><button class="dropdown-item btn-edit-item" data-id="' + row.id + '"><i class="bi bi-pencil-square me-2"></i>Edit</button></li>';
                            var icon = row.isActive ? 'lock' : 'unlock';
                            var label = row.isActive ? 'Deactivate' : 'Activate';
                            items += '<li><button class="dropdown-item btn-toggle-item" data-id="' + row.id + '" data-active="' + row.isActive + '"><i class="bi bi-' + icon + ' me-2"></i>' + label + '</button></li>';
                        }
                        if (perms.canDelete) {
                            if (items) items += '<li><hr class="dropdown-divider"></li>';
                            items += '<li><button class="dropdown-item btn-delete-item" data-id="' + row.id + '" data-name="' + row.name + '"><i class="bi bi-trash me-2"></i>Delete</button></li>';
                        }
                        if (!items) return '';
                        return '<div><button type="button" class="btn btn-primary btn-sm" data-bs-toggle="dropdown"><i class="bi bi-three-dots-vertical"></i></button><ul class="dropdown-menu">' + items + '</ul></div>';
                    }
                }
            ],
            order: [[1, 'asc']],
            language: {
                processing: '<div class="spinner-border text-primary" role="status"><span class="visually-hidden">Loading...</span></div>',
                emptyTable: 'No payment items found',
                zeroRecords: 'No matching items found'
            }
        });

        $('#itemSearchInput').on('keyup', function () { table.search(this.value).draw(); });
        $('#filterCategory').on('change', function () { table.ajax.reload(); });
    }

    function handleCreate() {
        // Load categories when modal opens
        $('#itemModal').on('show.bs.modal', function () { loadCategories('#itemCategoryId'); });

        // Create form now uses native method="post" — no JS submit handler needed
    }

    function handleEdit() {
        $(document).on('click', '.btn-edit-item', function () {
            var id = $(this).data('id');
            fetch('/api/v1/paymentitems/' + id, { method: 'GET', headers: { 'Content-Type': 'application/json' } })
                .then(function (r) { return r.json(); })
                .then(function (data) {
                    if (data.success && data.data) {
                        var item = data.data;
                        $('#editItemId').val(item.id);
                        $('#editItemName').val(item.name);
                        $('#editItemDescription').val(item.description);
                        $('#editItemIsActive').prop('checked', item.isActive);
                        loadCategories('#editItemCategoryId', item.categoryId);
                        var modal = new bootstrap.Modal(document.getElementById('itemEditModal'));
                        modal.show();
                    } else {
                        Swal.fire('Error!', 'Failed to load item data', 'error');
                    }
                });
        });

        // Edit form now uses native method="post" — no JS submit handler needed
    }

    function handleDelete() {
        $(document).on('click', '.btn-delete-item', function () {
            var id = $(this).data('id');
            var name = $(this).data('name');

            Swal.fire({
                title: 'Delete Payment Item?',
                html: 'Are you sure you want to delete <strong>' + name + '</strong>?<br><small class="text-muted">This action cannot be undone.</small>',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#d33',
                cancelButtonColor: '#6c757d',
                confirmButtonText: '<i class="bi bi-trash me-1"></i>Yes, delete it',
                cancelButtonText: '<i class="bi bi-x-circle me-1"></i>Cancel'
            }).then(function (result) {
                if (result.isConfirmed) {
                    fetch('/api/v1/paymentitems/' + id, {
                        method: 'DELETE',
                        headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': getAntiForgeryToken() }
                    })
                    .then(function (r) { return r.json(); })
                    .then(function (data) {
                        if (data.success) {
                            Swal.fire('Deleted!', data.message, 'success');
                            table.ajax.reload();
                        } else {
                            Swal.fire('Error!', data.message, 'error');
                        }
                    })
                    .catch(function () { Swal.fire('Error!', 'An unexpected error occurred', 'error'); });
                }
            });
        });
    }

    function handleToggle() {
        $(document).on('click', '.btn-toggle-item', function () {
            var id = $(this).data('id');
            var isActive = $(this).data('active');
            var msg = isActive ? 'Deactivate this item?' : 'Activate this item?';

            Swal.fire({ title: 'Are you sure?', text: msg, icon: 'warning', showCancelButton: true, confirmButtonText: 'Yes', cancelButtonText: 'No' })
            .then(function (result) {
                if (result.isConfirmed) {
                    fetch('/api/v1/paymentitems/' + id + '/toggle', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': getAntiForgeryToken() }
                    })
                    .then(function (r) { return r.json(); })
                    .then(function (data) {
                        if (data.success) { Swal.fire('Done!', data.message, 'success'); table.ajax.reload(); }
                        else { Swal.fire('Error!', data.message, 'error'); }
                    });
                }
            });
        });
    }

    function resetForm() {
        if (document.getElementById('itemForm')) {
            document.getElementById('itemForm').reset();
        }
    }

    function init() {
        initDataTable();
        handleCreate();
        handleEdit();
        handleDelete();
        handleToggle();
    }

    return { init: init, resetForm: resetForm };
})();

// ═══════════════════════════════════════════════════════════════
// PAYMENT SETUP MODULE
// ═══════════════════════════════════════════════════════════════
var PaymentSetup = (function () {
    'use strict';

    var table = null;

    function loadDropdown(url, selectId, selectedVal) {
        fetch(url, { method: 'GET', headers: { 'Content-Type': 'application/json' } })
            .then(function (r) { return r.json(); })
            .then(function (data) {
                if (data.success && data.data) {
                    var sel = $(selectId);
                    sel.find('option:not(:first)').remove();
                    data.data.forEach(function (item) {
                        sel.append('<option value="' + item.id + '"' + (item.id == selectedVal ? ' selected' : '') + '>' + item.name + '</option>');
                    });
                }
            });
    }

    function loadSelectListDropdown(url, selectId, selectedVal) {
        fetch(url, { method: 'GET', headers: { 'Content-Type': 'application/json' } })
            .then(function (r) { return r.json(); })
            .then(function (data) {
                if (data) {
                    var sel = $(selectId);
                    sel.find('option:not(:first)').remove();
                    data.forEach(function (item) {
                        sel.append('<option value="' + item.id + '"' + (item.id == selectedVal ? ' selected' : '') + '>' + item.name + '</option>');
                    });
                }
            });
    }

    function loadItemsByCategory(categoryId, selectId, selectedVal) {
        if (!categoryId) {
            $(selectId).find('option:not(:first)').remove();
            return;
        }
        loadDropdown('/api/v1/paymentitems/active?categoryId=' + categoryId, selectId, selectedVal);
    }

    function initDataTable() {
        // Load filter dropdowns
        loadSelectListDropdown('/api/v1/dropdown/sessions', '#filterSession');
        loadSelectListDropdown('/api/v1/dropdown/classes', '#filterClass');

        table = $('#paymentSetupsTable').DataTable({
            serverSide: true,
            processing: true,
            searching: true,
            paging: true,
            info: true,
            responsive: true,
            ajax: {
                url: '/home/GetPaymentSetupsDataTable',
                type: 'POST',
                dataType: 'json',
                data: function (d) {
                    d.session = $('#filterSession').val();
                    d.term = $('#filterTerm').val();
                    d.schoolclass = $('#filterClass').val();
                }
            },
            columns: [
                {
                    data: null, orderable: false, searchable: false,
                    render: function (data, type, row, meta) {
                        return meta.settings._iDisplayStart + meta.row + 1;
                    }
                },
                { data: 'paymentItemName' },
                { data: 'categoryName', render: function (d) { return d || '<span class="text-muted">—</span>'; } },
                { data: 'sessionName' },
                { data: 'termName' },
                { data: 'className' },
                {
                    data: 'amount'
                    //render: function (data) {
                    //    return '₦' + parseFloat(data).toLocaleString('en-NG', { minimumFractionDigits: 2 });
                    //}
                },
                {
                    data: 'isActive',
                    render: function (data) {
                        return data
                            ? '<span class="badge bg-success"><i class="bi bi-check-circle me-1"></i>Active</span>'
                            : '<span class="badge bg-danger"><i class="bi bi-x-circle me-1"></i>Inactive</span>';
                    }
                },
                {
                    data: null, orderable: false, searchable: false,
                    render: function (data, type, row) {
                        var perms = window.userPermissions || {};
                        var items = '';
                        if (perms.canEdit) {
                            items += '<li><button class="dropdown-item btn-edit-setup" data-id="' + row.id + '"><i class="bi bi-pencil-square me-2"></i>Edit</button></li>';
                            var icon = row.isActive ? 'lock' : 'unlock';
                            var label = row.isActive ? 'Deactivate' : 'Activate';
                            items += '<li><button class="dropdown-item btn-toggle-setup" data-id="' + row.id + '" data-active="' + row.isActive + '"><i class="bi bi-' + icon + ' me-2"></i>' + label + '</button></li>';
                        }
                        if (perms.canDelete) {
                            if (items) items += '<li><hr class="dropdown-divider"></li>';
                            items += '<li><button class="dropdown-item btn-delete-setup" data-id="' + row.id + '"><i class="bi bi-trash me-2"></i>Delete</button></li>';
                        }
                        if (!items) return '';
                        return '<div><button type="button" class="btn btn-primary btn-sm" data-bs-toggle="dropdown"><i class="bi bi-three-dots-vertical"></i></button><ul class="dropdown-menu">' + items + '</ul></div>';
                    }
                }
            ],
            order: [[1, 'asc']],
            language: {
                processing: '<div class="spinner-border text-primary" role="status"><span class="visually-hidden">Loading...</span></div>',
                emptyTable: 'No payment setups found',
                zeroRecords: 'No matching setups found'
            }
        });

        $('#setupSearchInput').on('keyup', function () { table.search(this.value).draw(); });
        $('#filterSession, #filterTerm, #filterClass').on('change', function () { table.ajax.reload(); });
    }

    function handleCreate() {
        $('#setupModal').on('show.bs.modal', function () {
            loadDropdown('/api/v1/paymentcategories/active', '#setupCategoryId');
            loadSelectListDropdown('/api/v1/dropdown/sessions', '#setupSessionId');
            loadSelectListDropdown('/api/v1/dropdown/classes', '#setupClassId');
            $('#setupPaymentItemId').find('option:not(:first)').remove();
        });

        // Cascade: category → items
        $('#setupCategoryId').on('change', function () {
            loadItemsByCategory($(this).val(), '#setupPaymentItemId');
        });

        // Create form now uses native method="post" — no JS submit handler needed
    }

    function handleEdit() {
        $(document).on('click', '.btn-edit-setup', function () {
            var id = $(this).data('id');
            fetch('/api/v1/paymentsetups/' + id, { method: 'GET', headers: { 'Content-Type': 'application/json' } })
                .then(function (r) { return r.json(); })
                .then(function (data) {
                    if (data.success && data.data) {
                        var s = data.data;
                        $('#editSetupId').val(s.id);
                        $('#editSetupAmount').val(s.amount);
                        $('#editSetupTermId').val(s.term);
                        $('#editSetupIsActive').prop('checked', s.isActive);

                        // Load dropdowns with selected values
                        loadDropdown('/api/v1/paymentcategories/active', '#editSetupCategoryId', s.categoryId);
                        loadSelectListDropdown('/api/v1/dropdown/sessions', '#editSetupSessionId', s.sessionId);
                        loadSelectListDropdown('/api/v1/dropdown/classes', '#editSetupClassId', s.classId);

                        // Load items for the category, then select the item
                        setTimeout(function () {
                            loadItemsByCategory(s.categoryId, '#editSetupPaymentItemId', s.paymentItemId);
                        }, 300);

                        var modal = new bootstrap.Modal(document.getElementById('setupEditModal'));
                        modal.show();
                    } else {
                        Swal.fire('Error!', 'Failed to load setup data', 'error');
                    }
                });
        });

        // Cascade: category → items in edit modal
        $('#editSetupCategoryId').on('change', function () {
            loadItemsByCategory($(this).val(), '#editSetupPaymentItemId');
        });

        // Edit form now uses native method="post" — no JS submit handler needed
    }

    function handleDelete() {
        $(document).on('click', '.btn-delete-setup', function () {
            var id = $(this).data('id');

            Swal.fire({
                title: 'Delete Payment Setup?',
                html: 'Are you sure you want to delete this setup?<br><small class="text-muted">This action cannot be undone.</small>',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#d33',
                cancelButtonColor: '#6c757d',
                confirmButtonText: '<i class="bi bi-trash me-1"></i>Yes, delete it',
                cancelButtonText: '<i class="bi bi-x-circle me-1"></i>Cancel'
            }).then(function (result) {
                if (result.isConfirmed) {
                    fetch('/api/v1/paymentsetups/' + id, {
                        method: 'DELETE',
                        headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': getAntiForgeryToken() }
                    })
                    .then(function (r) { return r.json(); })
                    .then(function (data) {
                        if (data.success) { Swal.fire('Deleted!', data.message, 'success'); table.ajax.reload(); }
                        else { Swal.fire('Error!', data.message, 'error'); }
                    })
                    .catch(function () { Swal.fire('Error!', 'An unexpected error occurred', 'error'); });
                }
            });
        });
    }

    function handleToggle() {
        $(document).on('click', '.btn-toggle-setup', function () {
            var id = $(this).data('id');
            var isActive = $(this).data('active');
            var msg = isActive ? 'Deactivate this setup?' : 'Activate this setup?';

            Swal.fire({ title: 'Are you sure?', text: msg, icon: 'warning', showCancelButton: true, confirmButtonText: 'Yes', cancelButtonText: 'No' })
            .then(function (result) {
                if (result.isConfirmed) {
                    fetch('/api/v1/paymentsetups/' + id + '/toggle', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': getAntiForgeryToken() }
                    })
                    .then(function (r) { return r.json(); })
                    .then(function (data) {
                        if (data.success) { Swal.fire('Done!', data.message, 'success'); table.ajax.reload(); }
                        else { Swal.fire('Error!', data.message, 'error'); }
                    });
                }
            });
        });
    }

    function resetForm() {
        if (document.getElementById('setupForm')) {
            document.getElementById('setupForm').reset();
            $('#setupPaymentItemId').find('option:not(:first)').remove();
        }
    }

    function init() {
        initDataTable();
        handleCreate();
        handleEdit();
        handleDelete();
        handleToggle();
    }

    return { init: init, resetForm: resetForm };
})();

// ================================================================
// Student Payments Module
// ================================================================
var StudentPayments = (function () {
    'use strict';

    var dataTable;

    function loadDropdowns() {
        $.get('/api/v1/dropdown/sessions', function (data) {
            var $sel = $('#filterSession');
            $sel.find('option:not(:first)').remove();
            $.each(data, function (i, item) {
                $sel.append($('<option>').val(item.id).text(item.name));
            });
        });
        $.get('/api/v1/dropdown/classes', function (data) {
            var $sel = $('#filterClass');
            $sel.find('option:not(:first)').remove();
            $.each(data, function (i, item) {
                $sel.append($('<option>').val(item.id).text(item.name));
            });
        });
    }

    function initDataTable() {
        dataTable = $('#studentPaymentsTable').DataTable({
            processing: true,
            serverSide: true,
            searching: true,
            responsive: true,
            ajax: {
                url: '/home/GetStudentPaymentsDataTable',
                type: 'POST',
                data: function (d) {
                    d.session = $('#filterSession').val();
                    d.term = $('#filterTerm').val();
                    d.schoolclass = $('#filterClass').val();
                    d.status = $('#filterStatus').val();
                    d.state = $('#filterState').val();
                }
            },
            columns: [
                {
                    data: null,
                    orderable: false,
                    render: function (data, type, row, meta) {
                        return meta.row + meta.settings._iDisplayStart + 1;
                    }
                },
                { data: 'reference' },
                { data: 'studentName' },
                { data: 'admissionNo' },
                { data: 'className' },
                { data: 'sessionName' },
                { data: 'termName' },
                { data: 'totalAmount' },
                {
                    data: 'status',
                    render: function (data) {
                        var cls = data === 'Completed' ? 'bg-success' : data === 'Pending' ? 'bg-warning' : 'bg-danger';
                        return '<span class="badge ' + cls + '">' + data + '</span>';
                    }
                },
                {
                    data: 'state',
                    render: function (data) {
                        var cls = 'bg-secondary';
                        if (data === 'Approved') cls = 'bg-success';
                        else if (data === 'Pending') cls = 'bg-warning text-dark';
                        else if (data === 'Rejected') cls = 'bg-danger';
                        else if (data === 'Cancelled') cls = 'bg-dark';
                        return '<span class="badge ' + cls + '">' + data + '</span>';
                    }
                },
                { data: 'paymentDate' },
                {
                    data: null, orderable: false,
                    render: function (data, type, row) {

                        let actions = `
                            <li>
                                <a class="dropdown-item" href="/admin/student-payments/detail/${row.id}">
                                    <i class="bi bi-eye"></i> View Detail
                                </a>
                            </li>
                        `;

                                        // Conditional actions
                            if (row.state === 'Pending' || row.state === 'Approved') {
                                            actions += `
                                <li>
                                    <a class="dropdown-item" href="/admin/student-payments/receipt/${row.id}">
                                        <i class="bi bi-receipt"></i> View Receipt
                                    </a>
                                </li>
                                <li>
                                    <a class="dropdown-item" href="/admin/student-payments/full-receipt/${row.termRegId}" target="_blank">
                                        <i class="bi bi-printer"></i> Full Receipt
                                    </a>
                                </li>
                            `;
                        }

                        return `
                            <div class="dropdown">
                                <button class="btn btn-primary btn-sm" type="button" data-bs-toggle="dropdown">
                                    <i class="bi bi-three-dots-vertical"></i>
                                </button>
                                <ul class="dropdown-menu">
                                    ${actions}
                                </ul>
                            </div>
                        `;
                    }
                }
            ],
            order: [[10, 'desc']]
        });

        $('#paymentSearchInput').on('keyup', function () {
            dataTable.search(this.value).draw();
        });
        $('#filterSession, #filterTerm, #filterClass, #filterStatus, #filterState').on('change', function () {
            dataTable.ajax.reload();
        });
    }

    function init() {
        loadDropdowns();
        initDataTable();
    }

    return { init: init };
})();

// ================================================================
// Make Payment Module
// ================================================================
var MakePayment = (function () {
    'use strict';

    var termRegistrationId = 0;

    function loadDropdowns() {
        $.get('/api/v1/dropdown/classes', function (data) {
            var $sel = $('#lookupClass');
            $sel.find('option:not(:first)').remove();
            $.each(data, function (i, item) {
                $sel.append($('<option>').val(item.id).text(item.name));
            });
        });
        $.get('/api/v1/paymentcategories/active', function (resp) {
            var $sel = $('#lookupCategory');
            $sel.find('option:not(:first)').remove();
            if (resp.success) {
                $.each(resp.data, function (i, item) {
                    $sel.append($('<option>').val(item.id).text(item.name));
                });
            }
        });
    }

    function handleLookup() {
        $('#btnLookup').on('click', function () {
            var classId = $('#lookupClass').val();
            var categoryId = $('#lookupCategory').val();
            var admissionNo = $('#lookupAdmissionNo').val().trim();

            if (!classId) {
                Swal.fire('Warning', 'Please select a class', 'warning');
                return;
            }
            if (!categoryId) {
                Swal.fire('Warning', 'Please select a payment category', 'warning');
                return;
            }
            if (!admissionNo) {
                Swal.fire('Warning', 'Please enter the student admission number', 'warning');
                return;
            }

            var $btn = $(this);
            $btn.prop('disabled', true).html('<i class="bi bi-hourglass-split me-2"></i>Searching...');

            $.ajax({
                url: '/api/v1/studentpayments/lookup',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({ classId: parseInt(classId), categoryId: parseInt(categoryId), admissionNo: admissionNo }),
                success: function (resp) {
                    $btn.prop('disabled', false).html('<i class="bi bi-search me-2"></i>Search');
                    if (resp.success) {
                        displayPaymentData(resp.data);
                    } else {
                        $('#paymentResultContainer').hide();
                        Swal.fire('Not Found', resp.message, 'warning');
                    }
                },
                error: function () {
                    $btn.prop('disabled', false).html('<i class="bi bi-search me-2"></i>Search');
                    Swal.fire('Error', 'An unexpected error occurred', 'error');
                }
            });
        });
    }

    function formatNaira(amount) {
        return '\u20A6' + parseFloat(amount).toLocaleString('en-NG', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    }

    function displayPaymentData(data) {
        termRegistrationId = data.termRegistrationId;

        $('#infoStudentName').text(data.studentName);
        $('#infoAdmissionNo').text(data.admissionNo);
        $('#infoClassName').text(data.className);
        $('#infoSessionName').text(data.sessionName);
        $('#infoTermName').text(data.termName);

        var $container = $('#paymentItemsContainer');
        $container.empty();

        if (!data.categoryGroups || data.categoryGroups.length === 0) {
            $container.html('<div class="alert alert-warning"><i class="bi bi-exclamation-triangle me-2"></i>No payable items found for this category.</div>');
            $('#paymentResultContainer').show();
            return;
        }

        $.each(data.categoryGroups, function (i, category) {
            var html = '<div class="card mb-3 category-card">';
            html += '<div class="card-header d-flex justify-content-between align-items-center" data-bs-toggle="collapse" data-bs-target="#cat-' + category.categoryId + '" style="cursor:pointer;">';
            html += '<h6 class="mb-0"><i class="bi bi-tag-fill me-2"></i>' + category.categoryName;
            html += ' <span class="badge bg-secondary ms-2">' + category.items.length + ' items</span></h6>';
            html += '<div><button type="button" class="btn btn-sm btn-outline-primary btn-pay-all-category" data-category-id="' + category.categoryId + '">';
            html += '<i class="bi bi-check-all me-1"></i>Pay All</button> <i class="bi bi-chevron-down"></i></div>';
            html += '</div>';
            html += '<div class="collapse show" id="cat-' + category.categoryId + '">';
            html += '<div class="card-body p-0"><table class="table table-hover mb-0"><thead class="table-light"><tr>';
            html += '<th style="width:40px;"><input type="checkbox" class="form-check-input category-check" data-category-id="' + category.categoryId + '" /></th>';
            html += '<th>Item</th><th>Expected</th><th>Paid</th><th>Remaining</th><th style="width:180px;">Amount to Pay</th>';
            html += '</tr></thead><tbody>';

            $.each(category.items, function (j, item) {
                var isFullyPaid = item.isFullyPaid;
                var rowClass = isFullyPaid ? 'table-success' : '';
                html += '<tr class="payment-item-row ' + rowClass + '" data-category-id="' + category.categoryId + '" data-item-id="' + item.paymentItemId + '" data-remaining="' + item.remaining + '">';

                if (!isFullyPaid) {
                    html += '<td><input type="checkbox" class="form-check-input item-check" data-item-id="' + item.paymentItemId + '" /></td>';
                } else {
                    html += '<td><i class="bi bi-check-circle-fill text-success"></i></td>';
                }

                html += '<td>' + item.itemName + '</td>';
                html += '<td>' + formatNaira(item.expectedAmount) + '</td>';
                html += '<td>' + formatNaira(item.alreadyPaid) + '</td>';

                if (isFullyPaid) {
                    html += '<td><span class="badge bg-success">Fully Paid</span></td>';
                    html += '<td><span class="text-muted">\u2014</span></td>';
                } else {
                    html += '<td><span class="text-danger fw-bold">' + formatNaira(item.remaining) + '</span></td>';
                    html += '<td><input type="number" readonly class="form-control form-control-sm amount-input" data-item-id="' + item.paymentItemId + '" min="0" max="' + item.remaining + '" step="0.01" value="0" disabled /></td>';
                }

                html += '</tr>';
            });

            html += '</tbody></table></div></div></div>';
            $container.append(html);
        });

        recalculateTotal();
        $('#paymentResultContainer').show();
    }

    function handleCheckboxes() {
        $(document).on('change', '.item-check', function () {
            var $row = $(this).closest('tr');
            var $input = $row.find('.amount-input');
            if ($(this).is(':checked')) {
                var remaining = parseFloat($row.data('remaining')) || 0;
                $input.prop('disabled', false).val(remaining.toFixed(2));
            } else {
                $input.prop('disabled', true).val('0');
            }
            recalculateTotal();
        });

        $(document).on('change', '.category-check', function () {
            var categoryId = $(this).data('category-id');
            var isChecked = $(this).is(':checked');
            $('tr[data-category-id="' + categoryId + '"]').each(function () {
                var $check = $(this).find('.item-check');
                if ($check.length && $check.prop('checked') !== isChecked) {
                    $check.prop('checked', isChecked).trigger('change');
                }
            });
        });

        $(document).on('click', '.btn-pay-all-category', function (e) {
            e.stopPropagation();
            var categoryId = $(this).data('category-id');
            $('tr[data-category-id="' + categoryId + '"]').each(function () {
                var $check = $(this).find('.item-check');
                if ($check.length && !$check.is(':checked')) {
                    $check.prop('checked', true).trigger('change');
                }
            });
        });

        $(document).on('input', '.amount-input', function () {
            var max = parseFloat($(this).attr('max')) || 0;
            var val = parseFloat($(this).val()) || 0;
            if (val > max) {
                $(this).val(max.toFixed(2));
                Swal.fire('Warning', 'Amount cannot exceed remaining balance', 'warning');
            }
            if (val < 0) $(this).val('0');
            recalculateTotal();
        });
    }

    function recalculateTotal() {
        var total = 0;
        $('.item-check:checked').each(function () {
            var $row = $(this).closest('tr');
            var amount = parseFloat($row.find('.amount-input').val()) || 0;
            total += amount;
        });
        var naira = '\u20A6' + total.toLocaleString('en-NG', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
        $('#totalAmount').text(naira);
        $('#btnSubmitPayment').prop('disabled', total <= 0);
    }

    function handleSubmit() {
        $('#btnSubmitPayment').on('click', function () {
            var items = [];

            $('.item-check:checked').each(function () {
                var $row = $(this).closest('tr');
                var itemId = parseInt($row.data('item-id'));
                var amount = parseFloat($row.find('.amount-input').val()) || 0;
                if (amount > 0) {
                    items.push({ paymentItemId: itemId, amountPaid: amount });
                }
            });

            if (items.length === 0) {
                Swal.fire('Warning', 'No items selected for payment', 'warning');
                return;
            }

            // Validate evidence if required
            var evidenceFile = null;
            if (window.evidenceRequired) {
                var fileInput = document.getElementById('paymentEvidence');
                if (!fileInput || !fileInput.files || fileInput.files.length === 0) {
                    Swal.fire('Warning', 'Payment evidence is required. Please upload a file.', 'warning');
                    return;
                }
                evidenceFile = fileInput.files[0];
                var allowedExts = ['.jpg', '.jpeg', '.png', '.pdf'];
                var fileName = evidenceFile.name.toLowerCase();
                var ext = fileName.substring(fileName.lastIndexOf('.'));
                if (allowedExts.indexOf(ext) === -1) {
                    Swal.fire('Warning', 'Invalid file type. Accepted: JPG, JPEG, PNG, PDF', 'warning');
                    return;
                }
                if (evidenceFile.size > 10 * 1024 * 1024) {
                    Swal.fire('Warning', 'File size must not exceed 10MB', 'warning');
                    return;
                }
            } else {
                var optionalInput = document.getElementById('paymentEvidence');
                if (optionalInput && optionalInput.files && optionalInput.files.length > 0) {
                    evidenceFile = optionalInput.files[0];
                }
            }

            var total = items.reduce(function (sum, i) { return sum + i.amountPaid; }, 0);
            var narration = ($('#paymentNarration').val() || '').trim();

            Swal.fire({
                title: 'Confirm Payment',
                html: 'You are about to process a payment of <strong>\u20A6' + total.toLocaleString('en-NG', { minimumFractionDigits: 2 }) + '</strong> for <strong>' + items.length + '</strong> item(s).<br><br>Proceed?',
                icon: 'question',
                showCancelButton: true,
                confirmButtonText: 'Yes, Submit Payment',
                cancelButtonText: 'Cancel'
            }).then(function (result) {
                if (result.isConfirmed) {
                    var formData = new FormData();
                    formData.append('termRegistrationId', termRegistrationId);
                    formData.append('items', JSON.stringify(items));
                    if (narration) formData.append('narration', narration);
                    if (evidenceFile) formData.append('evidence', evidenceFile);

                    $.ajax({
                        url: '/api/v1/studentpayments/create',
                        type: 'POST',
                        data: formData,
                        processData: false,
                        contentType: false,
                        success: function (resp) {
                            if (resp.success) {
                                Swal.fire({
                                    title: 'Payment Successful!',
                                    text: resp.message,
                                    icon: 'success',
                                    confirmButtonText: 'View Receipt'
                                }).then(function () {
                                    window.location.href = '/admin/student-payments/receipt/' + resp.id;
                                });
                            } else {
                                Swal.fire('Error', resp.message, 'error');
                            }
                        },
                        error: function () {
                            Swal.fire('Error', 'An unexpected error occurred', 'error');
                        }
                    });
                }
            });
        });
    }

    function initEvidenceField() {
        if (window.evidenceRequired) {
            $('#evidenceUploadSection').show();
        }
    }

    function init() {
        loadDropdowns();
        handleLookup();
        handleCheckboxes();
        handleSubmit();
        initEvidenceField();
    }

    return { init: init };
})();

// ================================================================
// Class Payment Report Module
// ================================================================
var ClassReport = (function () {
    'use strict';

    var dataTable;

    function formatNaira(val) {
        return '\u20A6' + Number(val || 0).toLocaleString('en-NG', { minimumFractionDigits: 2 });
    }

    function loadDropdowns() {
        $.get('/api/v1/dropdown/sessions', function (data) {
            var $sel = $('#rptSession');
            $sel.find('option:not(:first)').remove();
            $.each(data, function (i, item) {
                $sel.append($('<option>').val(item.id).text(item.name));
            });
        });
        $.get('/api/v1/dropdown/classes', function (data) {
            var $sel = $('#rptClass');
            $sel.find('option:not(:first)').remove();
            $.each(data, function (i, item) {
                $sel.append($('<option>').val(item.id).text(item.name));
            });
        });
        $.get('/api/v1/dropdown/subclasses', function (data) {
            var $sel = $('#rptSubClass');
            $sel.find('option:not(:first)').remove();
            $.each(data, function (i, item) {
                $sel.append($('<option>').val(item.id).text(item.name));
            });
        });
        $.get('/api/v1/paymentcategories/active', function (resp) {
            var $sel = $('#rptCategory');
            $sel.find('option:not(:first)').remove();
            var items = resp.success ? resp.data : resp;
            $.each(items, function (i, item) {
                $sel.append($('<option>').val(item.id).text(item.name));
            });
        });

        // Category → Payment Item cascade
        $('#rptCategory').on('change', function () {
            var categoryId = $(this).val();
            var $itemSel = $('#rptPaymentItem');
            $itemSel.find('option:not(:first)').remove();
            if (categoryId) {
                $.get('/api/v1/paymentitems/active?categoryId=' + categoryId, function (resp) {
                    if (resp.success && resp.data) {
                        $.each(resp.data, function (i, item) {
                            $itemSel.append($('<option>').val(item.id).text(item.name));
                        });
                    }
                });
            }
        });
    }

    function initDataTable() {
        dataTable = $('#classReportTable').DataTable({
            processing: true,
            serverSide: true,
            searching: false,
            paging: false,
            info: false,
            dom: 'Bfrtip',
            buttons: ['excel', 'csv', 'pdf', 'print'],
            ajax: {
                url: '/home/GetClassReportDataTable',
                type: 'POST',
                data: function (d) {
                    d.session = $('#rptSession').val();
                    d.term = $('#rptTerm').val();
                    d.schoolclass = $('#rptClass').val();
                    d.subclass = $('#rptSubClass').val();
                    d.category = $('#rptCategory').val();
                    d.paymentitem = $('#rptPaymentItem').val();
                },
                dataSrc: function (json) {
                    if (json.summary) {
                        updateSummary(json.summary);
                    }
                    return json.data || [];
                }
            },
            columns: [
                { data: 'studentName' },
                { data: 'admissionNo' },
                { data: 'categoryName' },
                { data: 'paymentItemName', render: function (d) { return d || '—'; } },
                { data: 'expected', render: function (d) { return formatNaira(d); } },
                { data: 'paid', render: function (d) { return formatNaira(d); } },
                { data: 'outstanding', render: function (d) { return formatNaira(d); } },
                {
                    data: 'status',
                    render: function (data) {
                        var cls = data === 'Fully Paid' ? 'bg-success' : data === 'Partial' ? 'bg-warning' : 'bg-danger';
                        return '<span class="badge ' + cls + '">' + data + '</span>';
                    }
                }
            ],
            order: [[0, 'asc']]
        });

        $('#btnLoadClassReport').on('click', function () {
            if (!$('#rptSession').val()) {
                Swal.fire('Required', 'Please select a Session.', 'warning');
                return;
            }
            dataTable.ajax.reload();
        });
    }

    function updateSummary(s) {
        $('#classReportSummary').show();
        $('#sumTotalStudents').text(s.totalStudents);
        $('#sumStudentsPaid').text(s.studentsPaid);
        $('#sumTotalExpected').text(formatNaira(s.totalExpected));
        $('#sumTotalOutstanding').text(formatNaira(s.totalOutstanding));
    }

    function init() {
        loadDropdowns();
        initDataTable();
    }

    return { init: init };
})();

// ================================================================
// School-Wide Payment Report Module
// ================================================================
var SchoolReport = (function () {
    'use strict';

    var dataTable;

    function formatNaira(val) {
        return '\u20A6' + Number(val || 0).toLocaleString('en-NG', { minimumFractionDigits: 2 });
    }

    function loadDropdowns() {
        $.get('/api/v1/dropdown/sessions', function (data) {
            var $sel = $('#schoolRptSession');
            $sel.find('option:not(:first)').remove();
            $.each(data, function (i, item) {
                $sel.append($('<option>').val(item.id).text(item.name));
            });
        });
    }

    function initDataTable() {
        dataTable = $('#schoolReportTable').DataTable({
            processing: true,
            serverSide: true,
            searching: false,
            paging: false,
            info: false,
            dom: 'Bfrtip',
            buttons: ['excel', 'csv', 'pdf', 'print'],
            ajax: {
                url: '/home/GetSchoolReportDataTable',
                type: 'POST',
                data: function (d) {
                    d.session = $('#schoolRptSession').val();
                    d.term = $('#schoolRptTerm').val();
                },
                dataSrc: function (json) {
                    if (json.summary) {
                        updateSummary(json.summary);
                    }
                    return json.data || [];
                }
            },
            columns: [
                { data: 'categoryName' },
                { data: 'itemName' },
                { data: 'totalStudents' },
                { data: 'studentsPaid' },
                { data: 'totalExpected', render: function (d) { return formatNaira(d); } },
                { data: 'totalCollected', render: function (d) { return formatNaira(d); } },
                { data: 'outstanding', render: function (d) { return formatNaira(d); } }
            ],
            order: [[0, 'asc']],
            drawCallback: function () {
                var api = this.api();
                var tbody = $(api.table().body());
                var rows = api.rows({ page: 'current' }).nodes();
                var rowData = api.rows({ page: 'current' }).data();

                // Remove any previously inserted subtotal rows
                tbody.find('tr.category-subtotal').remove();

                if (!rows.length) return;

                // Collect category groups with totals from raw data
                var groups = [];
                var currentGroup = null;

                rowData.each(function (data, index) {
                    var catName = data.categoryName;
                    if (!currentGroup || currentGroup.name !== catName) {
                        currentGroup = {
                            name: catName,
                            endIndex: index,
                            count: 1,
                            totalStudents: data.totalStudents || 0,
                            studentsPaid: data.studentsPaid || 0,
                            totalExpected: data.totalExpected || 0,
                            totalCollected: data.totalCollected || 0,
                            outstanding: data.outstanding || 0
                        };
                        groups.push(currentGroup);
                    } else {
                        currentGroup.endIndex = index;
                        currentGroup.count++;
                        currentGroup.totalStudents += (data.totalStudents || 0);
                        currentGroup.studentsPaid += (data.studentsPaid || 0);
                        currentGroup.totalExpected += (data.totalExpected || 0);
                        currentGroup.totalCollected += (data.totalCollected || 0);
                        currentGroup.outstanding += (data.outstanding || 0);
                    }
                });

                // Insert subtotal rows in reverse order to preserve DOM indices
                for (var g = groups.length - 1; g >= 0; g--) {
                    var grp = groups[g];
                    var subtotalRow = '<tr class="table-warning fw-bold category-subtotal">' +
                        '<td style="display:none;"></td>' +
                        '<td class="text-end fst-italic">Subtotal</td>' +
                        '<td>' + grp.totalStudents + '</td>' +
                        '<td>' + grp.studentsPaid + '</td>' +
                        '<td>' + formatNaira(grp.totalExpected) + '</td>' +
                        '<td>' + formatNaira(grp.totalCollected) + '</td>' +
                        '<td>' + formatNaira(grp.outstanding) + '</td>' +
                        '</tr>';
                    $(rows[grp.endIndex]).after(subtotalRow);
                }

                // Rowspan merging including subtotal rows
                var allRows = tbody.find('tr');
                var lastCategory = null;
                var firstCategoryTd = null;
                var spanCount = 0;

                allRows.each(function () {
                    var $row = $(this);
                    var categoryCell = $('td', this).eq(0);

                    if ($row.hasClass('category-subtotal')) {
                        // Subtotal row: hide category cell, include in span
                        categoryCell.css('display', 'none');
                        spanCount++;
                        if (firstCategoryTd) {
                            firstCategoryTd.attr('rowspan', spanCount);
                        }
                    } else {
                        var currentCategory = categoryCell.text();
                        if (currentCategory === lastCategory) {
                            // Duplicate category cell: hide and increment span
                            categoryCell.css('display', 'none');
                            spanCount++;
                            if (firstCategoryTd) {
                                firstCategoryTd.attr('rowspan', spanCount);
                            }
                        } else {
                            // New category group
                            firstCategoryTd = categoryCell;
                            firstCategoryTd.css('display', '');
                            firstCategoryTd.css('vertical-align', 'middle');
                            firstCategoryTd.css('font-weight', 'bold');
                            spanCount = 1;
                            firstCategoryTd.attr('rowspan', 1);
                            lastCategory = currentCategory;
                        }
                    }
                });
            }
        });

        $('#btnLoadSchoolReport').on('click', function () {
            if (!$('#schoolRptSession').val() || !$('#schoolRptTerm').val()) {
                Swal.fire('Required', 'Please select Session and Term.', 'warning');
                return;
            }
            dataTable.ajax.reload();
        });
    }

    function updateSummary(s) {
        $('#schoolReportSummary').show();
        $('#schoolSumStudents').text(s.totalStudents);
        $('#schoolSumPaid').text(s.studentsPaid);
        $('#schoolSumExpected').text(formatNaira(s.totalExpected));
        $('#schoolSumOutstanding').text(formatNaira(s.totalOutstanding));

        // Category breakdown cards
        if (s.categoryBreakdown && s.categoryBreakdown.length > 0) {
            $('#categoryBreakdownContainer').show();
            var html = '';
            $.each(s.categoryBreakdown, function (i, cat) {
                html += '<div class="col-md-4 col-lg-3">' +
                    '<div class="card border-secondary h-100">' +
                    '<div class="card-body p-2 text-center">' +
                    '<h6 class="card-title text-truncate mb-1">' + cat.categoryName + '</h6>' +
                    '<div class="small text-muted">Expected: ' + formatNaira(cat.totalExpected) + '</div>' +
                    '<div class="small text-success">Collected: ' + formatNaira(cat.totalCollected) + '</div>' +
                    '<div class="small text-danger">Outstanding: ' + formatNaira(cat.outstanding) + '</div>' +
                    '</div></div></div>';
            });
            $('#categoryBreakdownCards').html(html);
        } else {
            $('#categoryBreakdownContainer').hide();
        }
    }

    function init() {
        loadDropdowns();
        initDataTable();
    }

    return { init: init };
})();

// ================================================================
// Category & Item Report Module
// ================================================================
var CategoryReport = (function () {
    'use strict';

    var dataTable;

    function formatNaira(val) {
        return '\u20A6' + Number(val || 0).toLocaleString('en-NG', { minimumFractionDigits: 2 });
    }

    function loadDropdowns() {
        $.get('/api/v1/dropdown/sessions', function (data) {
            var $sel = $('#catRptSession');
            $sel.find('option:not(:first)').remove();
            $.each(data, function (i, item) {
                $sel.append($('<option>').val(item.id).text(item.name));
            });
        });
        $.get('/api/v1/dropdown/classes', function (data) {
            var $sel = $('#catRptClass');
            $sel.find('option:not(:first)').remove();
            $.each(data, function (i, item) {
                $sel.append($('<option>').val(item.id).text(item.name));
            });
        });
        $.get('/api/v1/paymentcategories/active', function (resp) {
            var $sel = $('#catRptCategory');
            $sel.find('option:not(:first)').remove();
            var items = resp.success ? resp.data : resp;
            $.each(items, function (i, item) {
                $sel.append($('<option>').val(item.id).text(item.name));
            });
        });
    }

    function initDataTable() {
        dataTable = $('#categoryReportTable').DataTable({
            processing: true,
            serverSide: true,
            searching: false,
            paging: false,
            info: false,
            dom: 'Bfrtip',
            buttons: ['excel', 'csv', 'pdf', 'print'],
            ajax: {
                url: '/home/GetCategoryItemReportDataTable',
                type: 'POST',
                data: function (d) {
                    d.session = $('#catRptSession').val();
                    d.term = $('#catRptTerm').val();
                    d.category = $('#catRptCategory').val();
                    d.schoolclass = $('#catRptClass').val();
                },
                dataSrc: function (json) {
                    if (json.summary) {
                        updateSummary(json.summary);
                    }
                    return json.data || [];
                }
            },
            columns: [
                { data: 'itemName' },
                { data: 'categoryName' },
                { data: 'className' },
                { data: 'totalStudents' },
                { data: 'studentsPaid' },
                { data: 'expectedAmount', render: function (d) { return formatNaira(d); } },
                { data: 'amountCollected', render: function (d) { return formatNaira(d); } },
                { data: 'outstanding', render: function (d) { return formatNaira(d); } }
            ],
            order: [[0, 'asc']],
            footerCallback: function (row, data, start, end, display) {
                var api = this.api();
                var totalExpected = 0;
                var totalCollected = 0;
                var totalOutstanding = 0;

                api.rows().data().each(function (rowData) {
                    totalExpected += Number(rowData.expectedAmount) || 0;
                    totalCollected += Number(rowData.amountCollected) || 0;
                    totalOutstanding += Number(rowData.outstanding) || 0;
                });

                $(api.column(5).footer()).html(formatNaira(totalExpected));
                $(api.column(6).footer()).html(formatNaira(totalCollected));
                $(api.column(7).footer()).html(formatNaira(totalOutstanding));
            }
        });

        $('#btnLoadCategoryReport').on('click', function () {
            if (!$('#catRptSession').val() || !$('#catRptTerm').val()) {
                Swal.fire('Required', 'Please select Session and Term.', 'warning');
                return;
            }
            dataTable.ajax.reload();
        });
    }

    function updateSummary(s) {
        $('#catReportSummary').show();
        $('#catSumItems').text(s.totalItems);
        $('#catSumStudentsPaid').text(s.totalStudentsPaid);
        $('#catSumCollected').text(formatNaira(s.totalAmountCollected));
    }

    function init() {
        loadDropdowns();
        initDataTable();
    }

    return { init: init };
})();

// Initialize Payment modules when document is ready
$(document).ready(function () {
    if ($("#paymentCategoriesTable").length) { PaymentCategory.init(); }
    if ($("#paymentItemsTable").length) { PaymentItem.init(); }
    if ($("#paymentSetupsTable").length) { PaymentSetup.init(); }
    if ($("#studentPaymentsTable").length) { StudentPayments.init(); }
    if ($("#lookupCard").length) { MakePayment.init(); }
    if ($("#classReportTable").length) { ClassReport.init(); }
    if ($("#schoolReportTable").length) { SchoolReport.init(); }
    if ($("#categoryReportTable").length) { CategoryReport.init(); }
});

