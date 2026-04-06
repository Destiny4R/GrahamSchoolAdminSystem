$(document).ready(function () {
    var tabledata = null;
    if ($("#feesSetupTable").length) {
        tabledata = $('#feesSetupTable').DataTable({
            serverSide: true,
            processing: true,
            "searching": true,
            "paging": true,
            "info": true,
            responsive: true,
            "ajax": {
                "url": "/home/GetFeesSetUplist",
                "type": "POST",
                dataType: 'json',
                //headers: {
                //    'RequestVerificationToken': getAntiForgeryToken()
                //},
                //contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
            },
            "columns": [
                {
                    data: null, orderable: false, searchable: false, autoWidth: true,
                    render: function (data, type, row, meta) {
                        return meta.settings._iDisplayStart + meta.row + 1;
                    }
                },
                { "data": "term1", autoWidth: true },
                { "data": "sessionname", "autoWidth": true },
                { "data": "classname", "autoWidth": true, },
                { "data": "amount1", autoWidth: true },
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
                    "data": null,
                    "orderable": false,
                    "render": function (data, type, row) {

                        return `
                                <div class="dropdown">
                                  <button class="btn btn-dark btn-sm" type="button" role="button" data-bs-toggle="dropdown" aria-expanded="false">
                                    <i class="bi bi-three-dots-vertical me-1"></i>
                                  </button>
                                  <ul class="dropdown-menu">
                                    <li><button class="dropdown-item btn-schoolclassess"
                                    data-id="${row.id}" data-term="${row.term}" data-amount="${row.amount}" data-classid="${data.classid}" data-sessionid="${data.sessionid}"><i class="bi bi-pencil-square"></i> Edit</button></li>
                                    <li><button class="dropdown-item btn-delete-schoolclassfeetsetup" data-id="${row.id}"><i class="bi bi-trash"></i> Delete</button></li>
                                  </ul>
                                </div>
                            `;
                    }
                }
            ]
        });
    }
    //Edit Modal
    $(document).on("click", ".btn-schoolclassess", function () {
        // Get button reference
        const btn = $(this);

        // Extract data attributes
        const id = btn.data("id");
        const term = btn.data("term");
        const amount = btn.data("amount");
        const classId = btn.data("classid");
        const sessionId = btn.data("sessionid");

        // Assign values to modal fields
        $("#feesSetupId").val(id);
        $("#schoolClassId").val(classId);
        $("select[name='FeesSetupModel.SessionId']").val(sessionId);
        $("select[name='FeesSetupModel.Term']").val(term);
        $("input[name='FeesSetupModel.Amount']").val(amount);

        // Optional: update modal title/button
        $("#modalTitle").text("Edit Fees Setup");
        $("#submitBtnText").text("Update");

        // Show modal
        const modal = new bootstrap.Modal(document.getElementById('feesSetupEditModal'));
        modal.show();
    });


    //Delete Fees Set Up if not in use or does not have a payment record
    $(document).on('click', '.btn-delete-schoolclassfeetsetup', function () {
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
                    url: '/api/v1/DeleteFeesSetup',
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
                            tabledata.ajax.reload(null, false);
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
                        Swal.fire({
                            icon: 'error',
                            title: 'Oops...',
                            text: 'Something went wrong!',
                            footer: 'Check internet connectivity'
                        });
                    },
                    complete: function () {
                        $.unblockUI();
                    }
                });

            }
        });
    });


});

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
                            return `
                                <div class="action-buttons">
                                    <button class="btn btn-sm btn-edit-session" 
                                            data-id="${row.id}" 
                                            data-name="${row.name}"
                                            title="Edit">
                                        <i class="bi bi-pencil-square"></i>
                                    </button>
                                    <button class="btn btn-sm btn-delete-session" 
                                            data-id="${row.id}" 
                                            data-name="${row.name}"
                                            title="Delete">
                                        <i class="bi bi-trash"></i>
                                    </button>
                                </div>
                            `;
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
                            return `
                                <div class="action-buttons">
                                    <button class="btn btn-sm btn-edit-subclass" 
                                            data-id="${row.id}" 
                                            data-name="${row.name}"
                                            title="Edit">
                                        <i class="bi bi-pencil-square"></i>
                                    </button>
                                    <button class="btn btn-sm btn-delete-subclass" 
                                            data-id="${row.id}" 
                                            data-name="${row.name}"
                                            title="Delete">
                                        <i class="bi bi-trash"></i>
                                    </button>
                                </div>
                            `;
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
                    "data": "updatedate", "autoWidth": true,
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
                        return `
                            <div class="btn-group btn-group-sm" role="group">
                                <button type="button" class="btn btn-outline-primary btn-edit-class" data-id="${row.id}" data-name="${row.name}" title="Edit">
                                    <i class="bi bi-pencil-fill"></i>
                                </button>
                                <button type="button" class="btn btn-outline-danger btn-delete-class" data-id="${row.id}" data-name="${row.name}" title="Delete">
                                    <i class="bi bi-trash-fill"></i>
                                </button>
                            </div>
                        `;
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
                                day: 'numeric'
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
                        let icon = '', classes = "";
                        if (row.isActive) {
                            icon = "lock";
                            classes = "Deactivate";
                        } else {
                            icon = "unlock";
                            classes = "Activate";
                        }
                        return `
                        <div class="">
                          <button type="button" class="btn btn-primary btn-sm" data-bs-toggle="dropdown" aria-expanded="false">
                            <i class="bi bi-three-dots-vertical"></i>
                          </button>
                          <ul class="dropdown-menu">
                            <li><button class="dropdown-item btn-edit-student" data-id="${row.id}">
                                <i class="bi bi-pencil-square"></i> Edit
                            </button></li>
                            <li><button class="dropdown-item btn-student-actionToggle" data-active="${row.isActive}" data-id="${row.id}">
                                <i class="bi bi-${icon} me-2"></i> ${classes}
                            </button></li>
                            <li><hr class="dropdown-divider"></li>
                            <li><button class="dropdown-item btn-delete-student" data-id="${row.id}"><i class="bi bi-trash me-2"></i>Delete</button></li>
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
    //OTHER PAYMENT ITEM AND ACTIVITIES
    var tabledata = null;
    if ($("#otherItemsTable").length) {
        tabledata = $('#otherItemsTable').DataTable({
            serverSide: true,
            processing: true,
            "searching": true,
            "paging": true,
            "info": true,
            responsive: true,
            "ajax": {
                "url": "/home/GetOtherItemsList",
                "type": "POST",
                dataType: 'json',
            },
            "columns": [
                {
                    data: null, orderable: false, searchable: false, autoWidth: true,
                    render: function (data, type, row, meta) {
                        return meta.settings._iDisplayStart + meta.row + 1;
                    }
                },
                { "data": "name", autoWidth: true },
                { "data": "description", "autoWidth": true },
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
                    "data": null,
                    "orderable": false,
                    "render": function (data, type, row) {

                        return `
                                        <div class="dropdown">
                                            <button class="btn btn-dark btn-sm" type="button" role="button" data-bs-toggle="dropdown" aria-expanded="false">
                                            <i class="bi bi-three-dots-vertical me-1"></i>
                                            </button>
                                            <ul class="dropdown-menu">
                                            <li><button class="dropdown-item btn-edit-otheritem"
                                            data-id="${row.id}" data-name="${row.name}" data-description="${row.description}"><i class="bi bi-pencil-square"></i> Edit</button></li>
                                            <li><button class="dropdown-item btn-delete-otheritem" data-id="${row.id}"><i class="bi bi-trash"></i> Delete</button></li>
                                            </ul>
                                        </div>
                                    `;
                    }
                }
            ]
        });
    }

    // Handle Edit button click
    $(document).on("click", ".btn-edit-otheritem", function () {
        const button = $(this);

        const id = button.data("id");
        const name = button.data("name");
        const description = button.data("description");

        // Assign values to modal inputs
        $("#viewModel_Id").val(id);
        $("#viewModelname").val(name);
        $("#viewModeldescription").val(description);

        // Show modal
        const modal = new bootstrap.Modal(document.getElementById('editModal'));
        modal.show();
    });

    $(document).on('click', '.btn-delete-otheritem', function () {
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
            title: "Warning action?",
            text: 'Do you want to permanently remove this item?',
            icon: "warning",
            showCancelButton: true,
            confirmButtonText: "Yes, delete!",
            cancelButtonText: "No, abort!",
            reverseButtons: true
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    type: "DELETE",
                    url: '/api/v1/otherpaymentitems/'+id,
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
                            tabledata.ajax.reload(null, false);
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
                        Swal.fire({
                            icon: 'error',
                            title: 'Oops...',
                            text: 'Something went wrong!',
                            footer: 'Check internet connectivity'
                        });
                    },
                    complete: function () {
                        $.unblockUI();
                    }
                });

            }
        });
    });

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

