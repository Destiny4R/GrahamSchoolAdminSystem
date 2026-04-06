/**
 * API Client with SweetAlert2 and jQuery BlockUI Integration
 * Handles all AJAX calls to v1Controller with loading states and delete confirmations
 */

const ApiClient = {
    baseUrl: '/api/v1',

    /**
     * Show loading state with jQuery BlockUI
     */
    showLoading: function (message = 'Loading...') {
        $.blockUI({
            message: `<div class="spinner-border text-primary" role="status"><span class="visually-hidden">${message}</span></div><p class="mt-2">${message}</p>`,
            css: {
                backgroundColor: '#ffffff',
                border: '1px solid #ddd',
                borderRadius: '8px',
                padding: '20px'
            },
            overlayCSS: {
                backgroundColor: 'rgba(0,0,0,0.5)'
            }
        });
    },

    /**
     * Hide loading state
     */
    hideLoading: function () {
        $.unblockUI();
    },

    /**
     * Show success alert with SweetAlert2
     */
    showSuccess: function (message = 'Operation completed successfully!', title = 'Success') {
        return Swal.fire({
            icon: 'success',
            title: title,
            text: message,
            confirmButtonClass: 'btn btn-success',
            confirmButtonText: 'OK'
        });
    },

    /**
     * Show error alert with SweetAlert2
     */
    showError: function (message = 'An error occurred', title = 'Error') {
        return Swal.fire({
            icon: 'error',
            title: title,
            text: message,
            confirmButtonClass: 'btn btn-danger',
            confirmButtonText: 'OK'
        });
    },

    /**
     * Show warning alert with SweetAlert2
     */
    showWarning: function (message = 'Please check your input', title = 'Warning') {
        return Swal.fire({
            icon: 'warning',
            title: title,
            text: message,
            confirmButtonClass: 'btn btn-warning',
            confirmButtonText: 'OK'
        });
    },

    /**
     * Show delete confirmation with SweetAlert2
     */
    showDeleteConfirmation: function (entityName = 'item') {
        return Swal.fire({
            title: 'Delete ' + entityName + '?',
            text: `Are you sure you want to delete this ${entityName}? This action cannot be undone.`,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonClass: 'btn btn-danger',
            cancelButtonClass: 'btn btn-secondary',
            confirmButtonText: 'Yes, Delete',
            cancelButtonText: 'Cancel'
        });
    },

    /**
     * Generic AJAX call wrapper
     */
    call: function (method, endpoint, data = null, options = {}) {
        this.showLoading(options.loadingMessage || 'Processing...');

        const settings = {
            type: method,
            url: this.baseUrl + endpoint,
            contentType: 'application/json',
            dataType: 'json',
            success: (response) => {
                this.hideLoading();
                if (options.onSuccess) options.onSuccess(response);
            },
            error: (xhr) => {
                this.hideLoading();
                const errorMsg = xhr.responseJSON?.message || 'An error occurred';
                this.showError(errorMsg, 'Error');
                if (options.onError) options.onError(xhr);
            },
            complete: () => {
                if (options.onComplete) options.onComplete();
            }
        };

        if (data) settings.data = JSON.stringify(data);

        return $.ajax(settings);
    },

    // ============ School Classes API ============

    /**
     * Get school classes DataTable
     */
    getSchoolClassesDataTable: function (draw, start, length, searchValue = '') {
        return $.ajax({
            type: 'GET',
            url: `${this.baseUrl}/schoolclasses/datatable?draw=${draw}&start=${start}&length=${length}&search[value]=${searchValue}`,
            contentType: 'application/json',
            dataType: 'json',
            error: function (xhr) {
                console.error('DataTable error:', xhr);
            }
        });
    },

    /**
     * Create school class
     */
    createSchoolClass: function (model) {
        return this.call('POST', '/schoolclasses/create', model, {
            loadingMessage: 'Creating school class...',
            onSuccess: (response) => {
                if (response.success) {
                    this.showSuccess(response.message, 'School Class Created');
                } else {
                    this.showError(response.message, 'Failed to Create');
                }
            }
        });
    },

    /**
     * Get school class for editing
     */
    getSchoolClass: function (id) {
        return this.call('GET', `/schoolclasses/${id}`, null, {
            loadingMessage: 'Loading school class...'
        });
    },

    /**
     * Update school class
     */
    updateSchoolClass: function (model) {
        return this.call('PUT', '/schoolclasses/update', model, {
            loadingMessage: 'Updating school class...',
            onSuccess: (response) => {
                if (response.success) {
                    this.showSuccess(response.message, 'School Class Updated');
                } else {
                    this.showError(response.message, 'Failed to Update');
                }
            }
        });
    },

    /**
     * Delete school class with SweetAlert2 confirmation
     */
    deleteSchoolClass: async function (id, onSuccess = null) {
        const result = await this.showDeleteConfirmation('School Class');
        
        if (result.isConfirmed) {
            return this.call('DELETE', `/schoolclasses/${id}`, null, {
                loadingMessage: 'Deleting school class...',
                onSuccess: (response) => {
                    if (response.success) {
                        this.showSuccess(response.message, 'Deleted');
                        if (onSuccess) onSuccess(response);
                    } else {
                        this.showError(response.message, 'Delete Failed');
                    }
                }
            });
        }
    },

    // ============ Fees Setup API ============

    /**
     * Get fees setup DataTable
     */
    getFeesSetupDataTable: function (draw, start, length, searchValue = '') {
        return $.ajax({
            type: 'GET',
            url: `${this.baseUrl}/feessetup/datatable?draw=${draw}&start=${start}&length=${length}&search[value]=${searchValue}`,
            contentType: 'application/json',
            dataType: 'json',
            error: function (xhr) {
                console.error('DataTable error:', xhr);
            }
        });
    },

    /**
     * Get fees setup selections (dropdowns)
     */
    getFeesSetupSelections: function () {
        return $.ajax({
            type: 'GET',
            url: `${this.baseUrl}/feessetup/selections`,
            contentType: 'application/json',
            dataType: 'json'
        });
    },

    /**
     * Create fees setup
     */
    createFeesSetup: function (model) {
        return this.call('POST', '/feessetup/create', model, {
            loadingMessage: 'Creating fees setup...',
            onSuccess: (response) => {
                if (response.success) {
                    this.showSuccess(response.message, 'Fees Setup Created');
                } else {
                    this.showError(response.message, 'Failed to Create');
                }
            }
        });
    },

    /**
     * Get fees setup for editing
     */
    getFeesSetup: function (id) {
        return this.call('GET', `/feessetup/${id}`, null, {
            loadingMessage: 'Loading fees setup...'
        });
    },

    /**
     * Update fees setup
     */
    updateFeesSetup: function (model) {
        return this.call('PUT', '/feessetup/update', model, {
            loadingMessage: 'Updating fees setup...',
            onSuccess: (response) => {
                if (response.success) {
                    this.showSuccess(response.message, 'Fees Setup Updated');
                } else {
                    this.showError(response.message, 'Failed to Update');
                }
            }
        });
    },

    /**
     * Delete fees setup with SweetAlert2 confirmation
     */
    deleteFeesSetup: async function (id, onSuccess = null) {
        const result = await this.showDeleteConfirmation('Fees Setup');
        
        if (result.isConfirmed) {
            return this.call('DELETE', `/feessetup/${id}`, null, {
                loadingMessage: 'Deleting fees setup...',
                onSuccess: (response) => {
                    if (response.success) {
                        this.showSuccess(response.message, 'Deleted');
                        if (onSuccess) onSuccess(response);
                    } else {
                        this.showError(response.message, 'Delete Failed');
                    }
                }
            });
        }
    }
};
