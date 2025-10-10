// NhacNho Module JavaScript
document.addEventListener('DOMContentLoaded', function() {
    // Initialize tooltips
    initializeTooltips();

    // Initialize form validations
    initializeFormValidations();

    // Initialize recurring form logic
    initializeRecurringForm();

    // Initialize status updates
    initializeStatusUpdates();

    // Initialize delete confirmations
    initializeDeleteConfirmations();

    // Initialize auto refresh for time displays
    initializeAutoRefresh();
});

function initializeTooltips() {
    // Initialize Bootstrap tooltips if available
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    if (typeof bootstrap !== 'undefined' && bootstrap.Tooltip) {
        tooltipTriggerList.map(function(tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl);
        });
    }

    // Custom tooltips for elements without Bootstrap
    const tooltipElements = document.querySelectorAll('[data-tooltip]');
    tooltipElements.forEach(element => {
        element.addEventListener('mouseenter', showCustomTooltip);
        element.addEventListener('mouseleave', hideCustomTooltip);
    });
}

function showCustomTooltip(event) {
    const element = event.target;
    const tooltipText = element.getAttribute('data-tooltip');

    if (!tooltipText) return;

    // Remove existing tooltip
    hideCustomTooltip();

    const tooltip = document.createElement('div');
    tooltip.className = 'nhacnho-custom-tooltip';
    tooltip.textContent = tooltipText;

    document.body.appendChild(tooltip);

    const rect = element.getBoundingClientRect();
    tooltip.style.left = rect.left + (rect.width / 2) - (tooltip.offsetWidth / 2) + 'px';
    tooltip.style.top = rect.top - tooltip.offsetHeight - 5 + 'px';

    setTimeout(() => tooltip.classList.add('show'), 10);
}

function hideCustomTooltip() {
    const existingTooltip = document.querySelector('.nhacnho-custom-tooltip');
    if (existingTooltip) {
        existingTooltip.remove();
    }
}

function initializeFormValidations() {
    const forms = document.querySelectorAll('.nhacnho-form');

    forms.forEach(form => {
        const inputs = form.querySelectorAll('.nhacnho-form-control, .nhacnho-form-select, .nhacnho-form-textarea');

        inputs.forEach(input => {
            input.addEventListener('blur', function() {
                validateField(this);
            });

            input.addEventListener('input', function() {
                clearFieldError(this);
            });
        });

        form.addEventListener('submit', function(e) {
            let isValid = true;
            inputs.forEach(input => {
                if (!validateField(input)) {
                    isValid = false;
                }
            });

            if (!isValid) {
                e.preventDefault();
                showFormError('Vui lòng kiểm tra lại các trường đã nhập.');
            }
        });
    });
}

function validateField(field) {
    const value = field.value.trim();
    const fieldName = field.getAttribute('asp-for') || field.name;
    let isValid = true;
    let errorMessage = '';

    // Required validation
    if (field.hasAttribute('required') && !value) {
        isValid = false;
        errorMessage = 'Trường này là bắt buộc.';
    }

    // Content validation for NoiDung
    if (field.name === 'NoiDung' && value && value.length < 5) {
        isValid = false;
        errorMessage = 'Nội dung nhắc nhở phải có ít nhất 5 ký tự.';
    }

    // Date validation for ThoiGian
    if (field.name === 'ThoiGian' && value) {
        const selectedDate = new Date(value);
        const now = new Date();
        if (selectedDate < now) {
            isValid = false;
            errorMessage = 'Thời gian nhắc nhở không được ở quá khứ.';
        }
    }

    if (!isValid) {
        showFieldError(field, errorMessage);
    } else {
        clearFieldError(field);
    }

    return isValid;
}

function showFieldError(field, message) {
    clearFieldError(field);

    field.classList.add('nhacnho-field-error');

    const errorElement = document.createElement('div');
    errorElement.className = 'nhacnho-field-error-message';
    errorElement.textContent = message;

    field.parentNode.appendChild(errorElement);
}

function clearFieldError(field) {
    field.classList.remove('nhacnho-field-error');

    const existingError = field.parentNode.querySelector('.nhacnho-field-error-message');
    if (existingError) {
        existingError.remove();
    }
}

function showFormError(message) {
    // Remove existing form error
    const existingError = document.querySelector('.nhacnho-form-error');
    if (existingError) {
        existingError.remove();
    }

    const errorElement = document.createElement('div');
    errorElement.className = 'nhacnho-form-error alert alert-danger';
    errorElement.innerHTML = '<i class="fas fa-exclamation-triangle"></i> ' + message;

    const form = document.querySelector('.nhacnho-form');
    if (form) {
        form.insertBefore(errorElement, form.firstChild);
    }
}

function initializeRecurringForm() {
    const loaiLapSelect = document.getElementById('loaiLap');
    const soNgayContainer = document.getElementById('soNgayContainer');
    const soNgayLapInput = document.getElementById('soNgayLap');

    if (loaiLapSelect && soNgayContainer) {
        loaiLapSelect.addEventListener('change', function() {
            const selectedValue = this.value;

            if (selectedValue === 'none') {
                soNgayContainer.style.display = 'none';
                soNgayLapInput.value = '1';
            } else {
                soNgayContainer.style.display = 'block';

                // Set default values based on type
                switch (selectedValue) {
                    case 'daily':
                        soNgayLapInput.value = '1';
                        soNgayLapInput.min = '1';
                        soNgayLapInput.max = '30';
                        break;
                    case 'weekly':
                        soNgayLapInput.value = '1';
                        soNgayLapInput.min = '1';
                        soNgayLapInput.max = '4';
                        break;
                    case 'monthly':
                        soNgayLapInput.value = '1';
                        soNgayLapInput.min = '1';
                        soNgayLapInput.max = '12';
                        break;
                }
            }
        });

        // Trigger initial state
        loaiLapSelect.dispatchEvent(new Event('change'));
    }
}

function initializeStatusUpdates() {
    // Update time remaining displays
    updateTimeDisplays();

    // Update status badges
    updateStatusBadges();
}

function updateTimeDisplays() {
    const timeElements = document.querySelectorAll('[data-time-remaining]');

    timeElements.forEach(element => {
        const targetTime = new Date(element.getAttribute('data-time-remaining'));
        const now = new Date();
        const timeDiff = targetTime - now;

        let timeText = '';
        if (timeDiff < 0) {
            timeText = `Quá hạn ${Math.abs(Math.round(timeDiff / (1000 * 60 * 60)))} giờ`;
        } else if (timeDiff < 60000) { // less than 1 minute
            timeText = 'Dưới 1 phút';
        } else if (timeDiff < 3600000) { // less than 1 hour
            timeText = `${Math.round(timeDiff / (1000 * 60))} phút`;
        } else if (timeDiff < 86400000) { // less than 1 day
            timeText = `${Math.round(timeDiff / (1000 * 60 * 60))} giờ`;
        } else {
            timeText = `${Math.round(timeDiff / (1000 * 60 * 60 * 24))} ngày`;
        }

        element.textContent = timeText;
    });
}

function updateStatusBadges() {
    const statusElements = document.querySelectorAll('[data-status-update]');

    statusElements.forEach(element => {
        const targetTime = new Date(element.getAttribute('data-status-update'));
        const now = new Date();
        const timeDiff = targetTime - now;

        // Remove existing classes
        element.classList.remove('nhacnho-status-upcoming', 'nhacnho-status-overdue', 'nhacnho-status-soon');

        if (timeDiff < 0) {
            element.classList.add('nhacnho-status-overdue');
            element.innerHTML = '<i class="fas fa-exclamation-triangle"></i> Quá hạn';
        } else if (timeDiff <= 3600000) { // 1 hour
            element.classList.add('nhacnho-status-soon');
            element.innerHTML = '<i class="fas fa-clock"></i> Sắp đến';
        } else {
            element.classList.add('nhacnho-status-upcoming');
            element.innerHTML = '<i class="fas fa-calendar-check"></i> Sắp tới';
        }
    });
}

function initializeDeleteConfirmations() {
    const deleteButtons = document.querySelectorAll('.nhacnho-delete-btn');

    deleteButtons.forEach(button => {
        button.addEventListener('click', function(e) {
            const confirmed = confirm('Bạn có chắc chắn muốn xóa nhắc nhở này? Hành động này không thể hoàn tác.');

            if (!confirmed) {
                e.preventDefault();
                return false;
            }

            // Show loading state
            button.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang xóa...';
            button.disabled = true;
        });
    });

    // Handle delete all modal
    const deleteAllModal = document.getElementById('deleteAllModal');
    if (deleteAllModal) {
        deleteAllModal.addEventListener('show.bs.modal', function(event) {
            const button = event.relatedTarget;
            const form = deleteAllModal.querySelector('form');
            // Form action is already set in the view
        });
    }
}

function initializeAutoRefresh() {
    // Auto refresh time displays every minute
    setInterval(function() {
        updateTimeDisplays();
        updateStatusBadges();
    }, 60000); // 60 seconds

    // Auto refresh page every 5 minutes to update all data
    setTimeout(function() {
        if (!document.hidden) { // Only refresh if page is visible
            location.reload();
        }
    }, 300000); // 5 minutes
}

// Utility functions
function formatDateTime(dateString) {
    const date = new Date(dateString);
    return date.toLocaleString('vi-VN', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit'
    });
}

function getTimeRemaining(targetDate) {
    const now = new Date();
    const target = new Date(targetDate);
    const diff = target - now;

    if (diff < 0) {
        return { text: 'Đã quá hạn', class: 'overdue' };
    } else if (diff < 60000) {
        return { text: 'Dưới 1 phút', class: 'soon' };
    } else if (diff < 3600000) {
        return { text: `${Math.round(diff / (1000 * 60))} phút`, class: 'upcoming' };
    } else if (diff < 86400000) {
        return { text: `${Math.round(diff / (1000 * 60 * 60))} giờ`, class: 'upcoming' };
    } else {
        return { text: `${Math.round(diff / (1000 * 60 * 60 * 24))} ngày`, class: 'upcoming' };
    }
}

// Add custom tooltip styles
const tooltipStyles = `
.nhacnho-custom-tooltip {
    position: absolute;
    background: rgba(0, 0, 0, 0.8);
    color: white;
    padding: 8px 12px;
    border-radius: 6px;
    font-size: 0.85rem;
    white-space: nowrap;
    z-index: 9999;
    opacity: 0;
    transform: translateY(10px);
    transition: all 0.3s ease;
    pointer-events: none;
}

.nhacnho-custom-tooltip.show {
    opacity: 1;
    transform: translateY(0);
}

.nhacnho-field-error {
    border-color: #dc3545 !important;
    box-shadow: 0 0 0 0.2rem rgba(220, 53, 69, 0.25) !important;
}

.nhacnho-field-error-message {
    color: #dc3545;
    font-size: 0.85rem;
    margin-top: 5px;
    display: flex;
    align-items: center;
    gap: 5px;
}

.nhacnho-field-error-message::before {
    content: '⚠';
    font-size: 0.8rem;
}
`;

// Inject tooltip styles
const styleElement = document.createElement('style');
styleElement.textContent = tooltipStyles;
document.head.appendChild(styleElement);

// Export functions for global use
window.NhacNhoUtils = {
    formatDateTime,
    getTimeRemaining,
    updateTimeDisplays,
    updateStatusBadges,
    showCustomTooltip,
    hideCustomTooltip
};
