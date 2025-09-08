/* ThuNhap JavaScript - Interactive Components */

// Initialize ThuNhap module when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    initializeThuNhap();
});

function initializeThuNhap() {
    // Initialize all ThuNhap components
    initializeFormValidation();
    initializeAnimations();
    initializeTooltips();
    initializeConfirmations();
    initializeFilters();
    initializeTableInteractions();
}

// Form Validation and Enhancement
function initializeFormValidation() {
    const forms = document.querySelectorAll('.thunhap-form');

    forms.forEach(form => {
        const inputs = form.querySelectorAll('.thunhap-form-control');

        inputs.forEach(input => {
            // Add floating label effect
            input.addEventListener('focus', function() {
                this.parentElement.classList.add('focused');
            });

            input.addEventListener('blur', function() {
                if (!this.value) {
                    this.parentElement.classList.remove('focused');
                }
            });

            // Real-time validation
            input.addEventListener('input', function() {
                validateInput(this);
            });

            // Initialize floating labels
            if (input.value) {
                input.parentElement.classList.add('focused');
            }
        });

        // Form submission enhancement
        form.addEventListener('submit', function(e) {
            const submitBtn = this.querySelector('.thunhap-btn[type="submit"]');
            if (submitBtn) {
                submitBtn.classList.add('thunhap-loading');
                submitBtn.innerHTML = '<div class="thunhap-spinner"></div> Đang xử lý...';
            }
        });
    });
}

// Input validation
function validateInput(input) {
    const value = input.value.trim();
    const type = input.type;
    let isValid = true;
    let message = '';

    // Remove existing validation classes
    input.classList.remove('is-valid', 'is-invalid');

    // Validate based on input type
    switch(type) {
        case 'number':
            if (value && (isNaN(value) || parseFloat(value) < 0)) {
                isValid = false;
                message = 'Vui lòng nhập số dương hợp lệ';
            }
            break;
        case 'date':
            if (value && new Date(value) > new Date()) {
                isValid = false;
                message = 'Ngày không được lớn hơn ngày hiện tại';
            }
            break;
        default:
            if (input.hasAttribute('required') && !value) {
                isValid = false;
                message = 'Trường này là bắt buộc';
            }
    }

    // Apply validation classes and messages
    if (value) {
        if (isValid) {
            input.classList.add('is-valid');
        } else {
            input.classList.add('is-invalid');
            showValidationMessage(input, message);
        }
    }
}

// Show validation message
function showValidationMessage(input, message) {
    let feedback = input.parentElement.querySelector('.invalid-feedback');
    if (!feedback) {
        feedback = document.createElement('div');
        feedback.className = 'invalid-feedback';
        input.parentElement.appendChild(feedback);
    }
    feedback.textContent = message;
}

// Animations
function initializeAnimations() {
    // Animate cards on scroll
    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    };

    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('thunhap-animate-fade-in');
            }
        });
    }, observerOptions);

    // Observe all cards
    document.querySelectorAll('.thunhap-card').forEach(card => {
        observer.observe(card);
    });

    // Add loading animations
    const loadingElements = document.querySelectorAll('.thunhap-loading');
    loadingElements.forEach(el => {
        el.style.animation = 'pulse 1.5s ease-in-out infinite';
    });
}

// Tooltips and Popovers
function initializeTooltips() {
    // Initialize Bootstrap tooltips if available
    if (typeof bootstrap !== 'undefined' && bootstrap.Tooltip) {
        const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
        tooltipTriggerList.map(function (tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl);
        });
    }

    // Custom tooltips for action buttons
    const actionButtons = document.querySelectorAll('.thunhap-action-links a');
    actionButtons.forEach(btn => {
        btn.addEventListener('mouseenter', function(e) {
            showCustomTooltip(e.target, this.getAttribute('title') || this.textContent.trim());
        });

        btn.addEventListener('mouseleave', function() {
            hideCustomTooltip();
        });
    });
}

// Custom tooltip functions
function showCustomTooltip(element, text) {
    const existingTooltip = document.querySelector('.thunhap-custom-tooltip');
    if (existingTooltip) {
        existingTooltip.remove();
    }

    const tooltip = document.createElement('div');
    tooltip.className = 'thunhap-custom-tooltip';
    tooltip.textContent = text;

    document.body.appendChild(tooltip);

    const rect = element.getBoundingClientRect();
    tooltip.style.left = rect.left + (rect.width / 2) - (tooltip.offsetWidth / 2) + 'px';
    tooltip.style.top = rect.top - tooltip.offsetHeight - 5 + 'px';

    setTimeout(() => tooltip.classList.add('visible'), 10);
}

function hideCustomTooltip() {
    const tooltip = document.querySelector('.thunhap-custom-tooltip');
    if (tooltip) {
        tooltip.classList.remove('visible');
        setTimeout(() => tooltip.remove(), 300);
    }
}

// Confirmation dialogs
function initializeConfirmations() {
    // Delete confirmations
    const deleteButtons = document.querySelectorAll('a[href*="Delete"], button[onclick*="delete"]');
    deleteButtons.forEach(btn => {
        btn.addEventListener('click', function(e) {
            e.preventDefault();
            showDeleteConfirmation(this);
        });
    });

    // Form reset confirmations
    const resetButtons = document.querySelectorAll('button[type="reset"]');
    resetButtons.forEach(btn => {
        btn.addEventListener('click', function(e) {
            if (hasFormChanges()) {
                e.preventDefault();
                showResetConfirmation(this);
            }
        });
    });
}

// Delete confirmation dialog
function showDeleteConfirmation(button) {
    const itemName = button.getAttribute('data-item-name') || 'mục này';
    const itemId = button.getAttribute('data-item-id') || '';

    if (typeof Swal !== 'undefined') {
        Swal.fire({
            title: 'Xác nhận xóa',
            text: `Bạn có chắc chắn muốn xóa ${itemName}?`,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#ef4444',
            cancelButtonColor: '#6b7280',
            confirmButtonText: 'Xóa',
            cancelButtonText: 'Hủy'
        }).then((result) => {
            if (result.isConfirmed) {
                // Create and submit form for delete
                const form = document.createElement('form');
                form.method = 'post';
                form.action = button.href || button.getAttribute('data-url');

                const token = document.querySelector('input[name="__RequestVerificationToken"]');
                if (token) {
                    const tokenInput = document.createElement('input');
                    tokenInput.type = 'hidden';
                    tokenInput.name = '__RequestVerificationToken';
                    tokenInput.value = token.value;
                    form.appendChild(tokenInput);
                }

                document.body.appendChild(form);
                form.submit();
            }
        });
    } else {
        // Fallback to browser confirm
        if (confirm(`Bạn có chắc chắn muốn xóa ${itemName}?`)) {
            window.location.href = button.href;
        }
    }
}

// Reset confirmation dialog
function showResetConfirmation(button) {
    if (typeof Swal !== 'undefined') {
        Swal.fire({
            title: 'Xác nhận đặt lại',
            text: 'Bạn có chắc chắn muốn đặt lại tất cả các trường?',
            icon: 'question',
            showCancelButton: true,
            confirmButtonColor: '#f59e0b',
            cancelButtonColor: '#6b7280',
            confirmButtonText: 'Đặt lại',
            cancelButtonText: 'Hủy'
        }).then((result) => {
            if (result.isConfirmed) {
                const form = button.closest('form');
                if (form) {
                    form.reset();
                    // Reset floating labels
                    const inputs = form.querySelectorAll('.thunhap-form-control');
                    inputs.forEach(input => {
                        input.parentElement.classList.remove('focused');
                        input.classList.remove('is-valid', 'is-invalid');
                    });
                }
            }
        });
    } else {
        if (confirm('Bạn có chắc chắn muốn đặt lại tất cả các trường?')) {
            const form = button.closest('form');
            if (form) {
                form.reset();
            }
        }
    }
}

// Check if form has changes
function hasFormChanges() {
    const inputs = document.querySelectorAll('.thunhap-form-control');
    for (let input of inputs) {
        if (input.value !== input.defaultValue) {
            return true;
        }
    }
    return false;
}

// Filter functionality
function initializeFilters() {
    const filterForm = document.querySelector('.thunhap-filter-form');
    if (!filterForm) return;

    const inputs = filterForm.querySelectorAll('input, select');
    let filterTimeout;

    inputs.forEach(input => {
        input.addEventListener('input', function() {
            clearTimeout(filterTimeout);
            filterTimeout = setTimeout(() => {
                applyFilters();
            }, 500);
        });

        input.addEventListener('change', function() {
            applyFilters();
        });
    });

    // Clear filters button
    const clearBtn = document.querySelector('.thunhap-clear-filters');
    if (clearBtn) {
        clearBtn.addEventListener('click', function() {
            inputs.forEach(input => {
                if (input.type === 'checkbox' || input.type === 'radio') {
                    input.checked = false;
                } else {
                    input.value = '';
                }
            });
            applyFilters();
        });
    }
}

// Apply filters
function applyFilters() {
    const formData = new FormData(document.querySelector('.thunhap-filter-form'));
    const params = new URLSearchParams();

    for (let [key, value] of formData.entries()) {
        if (value.trim()) {
            params.append(key, value);
        }
    }

    // Show loading state
    const tableBody = document.querySelector('.thunhap-table tbody');
    if (tableBody) {
        tableBody.innerHTML = '<tr><td colspan="100%" class="text-center"><div class="thunhap-spinner"></div> Đang tải...</td></tr>';
    }

    // Update URL without page reload
    const newUrl = window.location.pathname + (params.toString() ? '?' + params.toString() : '');
    window.history.pushState({}, '', newUrl);

    // In a real implementation, you would make an AJAX request here
    // For now, we'll just reload the page with new filters
    setTimeout(() => {
        window.location.href = newUrl;
    }, 300);
}

// Table interactions
function initializeTableInteractions() {
    const table = document.querySelector('.thunhap-table');
    if (!table) return;

    // Row hover effects
    const rows = table.querySelectorAll('tbody tr');
    rows.forEach(row => {
        row.addEventListener('mouseenter', function() {
            this.style.transform = 'scale(1.01)';
        });

        row.addEventListener('mouseleave', function() {
            this.style.transform = 'scale(1)';
        });
    });

    // Sortable columns
    const headers = table.querySelectorAll('thead th[data-sort]');
    headers.forEach(header => {
        header.style.cursor = 'pointer';
        header.addEventListener('click', function() {
            sortTable(this);
        });
    });

    // Select all checkbox
    const selectAll = document.querySelector('.thunhap-select-all');
    if (selectAll) {
        selectAll.addEventListener('change', function() {
            const checkboxes = document.querySelectorAll('.thunhap-row-checkbox');
            checkboxes.forEach(cb => cb.checked = this.checked);
            updateBulkActions();
        });
    }

    // Individual checkboxes
    const rowCheckboxes = document.querySelectorAll('.thunhap-row-checkbox');
    rowCheckboxes.forEach(cb => {
        cb.addEventListener('change', updateBulkActions);
    });
}

// Sort table
function sortTable(header) {
    const column = header.getAttribute('data-sort');
    const table = header.closest('table');
    const tbody = table.querySelector('tbody');
    const rows = Array.from(tbody.querySelectorAll('tr'));

    // Toggle sort direction
    const currentSort = header.getAttribute('data-sort-dir') || 'asc';
    const newSort = currentSort === 'asc' ? 'desc' : 'asc';
    header.setAttribute('data-sort-dir', newSort);

    // Update sort indicators
    table.querySelectorAll('thead th').forEach(th => {
        th.classList.remove('sort-asc', 'sort-desc');
    });
    header.classList.add(`sort-${newSort}`);

    // Sort rows
    rows.sort((a, b) => {
        const aVal = a.querySelector(`[data-value="${column}"]`)?.textContent || '';
        const bVal = b.querySelector(`[data-value="${column}"]`)?.textContent || '';

        let comparison = 0;
        if (aVal < bVal) comparison = -1;
        if (aVal > bVal) comparison = 1;

        return newSort === 'asc' ? comparison : -comparison;
    });

    // Re-append sorted rows
    rows.forEach(row => tbody.appendChild(row));
}

// Update bulk actions
function updateBulkActions() {
    const checkedBoxes = document.querySelectorAll('.thunhap-row-checkbox:checked');
    const bulkActions = document.querySelector('.thunhap-bulk-actions');

    if (bulkActions) {
        if (checkedBoxes.length > 0) {
            bulkActions.style.display = 'block';
            bulkActions.querySelector('.selected-count').textContent = checkedBoxes.length;
        } else {
            bulkActions.style.display = 'none';
        }
    }
}

// Utility functions
function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND'
    }).format(amount);
}

function formatDate(dateString) {
    const date = new Date(dateString);
    return date.toLocaleDateString('vi-VN', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit'
    });
}

// Export functions for global use
window.ThuNhapUtils = {
    formatCurrency: formatCurrency,
    formatDate: formatDate,
    showDeleteConfirmation: showDeleteConfirmation,
    showResetConfirmation: showResetConfirmation
};

// Auto-submit forms with auto-submit class
document.addEventListener('change', function(e) {
    if (e.target.classList.contains('thunhap-auto-submit')) {
        e.target.closest('form').submit();
    }
});

// Keyboard shortcuts
document.addEventListener('keydown', function(e) {
    // Ctrl/Cmd + Enter to submit forms
    if ((e.ctrlKey || e.metaKey) && e.key === 'Enter') {
        const activeForm = document.activeElement.closest('form');
        if (activeForm) {
            activeForm.querySelector('button[type="submit"]')?.click();
        }
    }

    // Escape to close modals or go back
    if (e.key === 'Escape') {
        const modal = document.querySelector('.modal.show');
        if (modal) {
            const closeBtn = modal.querySelector('[data-bs-dismiss="modal"]');
            if (closeBtn) closeBtn.click();
        }
    }
});

// Print functionality
function initializePrint() {
    const printButtons = document.querySelectorAll('.thunhap-print-btn');
    printButtons.forEach(btn => {
        btn.addEventListener('click', function() {
            window.print();
        });
    });
}

// Initialize print on load
initializePrint();
