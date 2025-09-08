// NganSach Module JavaScript
document.addEventListener('DOMContentLoaded', function() {
    // Initialize tooltips
    initializeTooltips();

    // Initialize form validations
    initializeFormValidations();

    // Initialize filter animations
    initializeFilterAnimations();

    // Initialize progress bar animations
    initializeProgressAnimations();

    // Initialize delete confirmations
    initializeDeleteConfirmations();
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
    tooltip.className = 'ngansach-custom-tooltip';
    tooltip.textContent = tooltipText;

    document.body.appendChild(tooltip);

    const rect = element.getBoundingClientRect();
    tooltip.style.left = rect.left + (rect.width / 2) - (tooltip.offsetWidth / 2) + 'px';
    tooltip.style.top = rect.top - tooltip.offsetHeight - 5 + 'px';

    setTimeout(() => tooltip.classList.add('show'), 10);
}

function hideCustomTooltip() {
    const existingTooltip = document.querySelector('.ngansach-custom-tooltip');
    if (existingTooltip) {
        existingTooltip.remove();
    }
}

function initializeFormValidations() {
    const forms = document.querySelectorAll('.ngansach-form');

    forms.forEach(form => {
        const inputs = form.querySelectorAll('.ngansach-form-control, .ngansach-form-select');

        inputs.forEach(input => {
            input.addEventListener('blur', function() {
                validateField(this);
            });

            input.addEventListener('input', function() {
                clearFieldError(this);
            });

            // Format số tiền cho input HanMuc
            if (input.name === 'HanMuc' || input.id === 'HanMuc') {
                formatCurrencyInput(input);
            }
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

    // Number validation for HanMuc
    if (field.name === 'HanMuc' && value) {
        const numValue = parseFloat(value);
        if (isNaN(numValue) || numValue <= 0) {
            isValid = false;
            errorMessage = 'Hạn mức phải là số dương.';
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

    field.classList.add('ngansach-field-error');

    const errorElement = document.createElement('div');
    errorElement.className = 'ngansach-field-error-message';
    errorElement.textContent = message;

    field.parentNode.appendChild(errorElement);
}

function clearFieldError(field) {
    field.classList.remove('ngansach-field-error');

    const existingError = field.parentNode.querySelector('.ngansach-field-error-message');
    if (existingError) {
        existingError.remove();
    }
}

function showFormError(message) {
    // Remove existing form error
    const existingError = document.querySelector('.ngansach-form-error');
    if (existingError) {
        existingError.remove();
    }

    const errorElement = document.createElement('div');
    errorElement.className = 'ngansach-form-error alert alert-danger';
    errorElement.innerHTML = '<i class="fas fa-exclamation-triangle"></i> ' + message;

    const form = document.querySelector('.ngansach-form');
    if (form) {
        form.insertBefore(errorElement, form.firstChild);
    }
}

function initializeFilterAnimations() {
    const filterSection = document.querySelector('.ngansach-filter-section');
    if (filterSection) {
        const selects = filterSection.querySelectorAll('select');

        selects.forEach(select => {
            select.addEventListener('change', function() {
                // Add loading animation
                const button = filterSection.querySelector('button[type="submit"]');
                if (button) {
                    button.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang lọc...';
                    button.disabled = true;

                    // Re-enable after form submission
                    setTimeout(() => {
                        button.innerHTML = '<i class="fas fa-filter"></i> Lọc';
                        button.disabled = false;
                    }, 1000);
                }
            });
        });
    }
}

function initializeProgressAnimations() {
    const progressBars = document.querySelectorAll('.ngansach-progress-bar');

    progressBars.forEach(bar => {
        const percentage = bar.style.width;
        bar.style.width = '0%';

        setTimeout(() => {
            bar.style.width = percentage;
        }, 500);
    });
}

function initializeDeleteConfirmations() {
    const deleteForms = document.querySelectorAll('.ngansach-delete-form');

    deleteForms.forEach(form => {
        form.addEventListener('submit', function(e) {
            const button = form.querySelector('.ngansach-delete-btn');

            // Prevent double submission
            if (button.disabled) {
                e.preventDefault();
                return false;
            }

            const confirmed = confirm('Bạn có chắc chắn muốn xóa ngân sách này? Hành động này không thể hoàn tác.');

            if (!confirmed) {
                e.preventDefault();
                return false;
            }

            // Show loading state
            const originalText = button.innerHTML;
            button.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang xóa...';
            button.disabled = true;

            // Add a small delay to ensure form submission starts
            setTimeout(() => {
                if (button.disabled) {
                    // If still disabled after delay, try to submit form manually
                    try {
                        form.submit();
                    } catch (error) {
                        console.error('Form submission error:', error);
                        // Re-enable button on error
                        button.innerHTML = originalText;
                        button.disabled = false;
                        alert('Có lỗi xảy ra khi xóa ngân sách. Vui lòng thử lại.');
                    }
                }
            }, 100);

            // Fallback: re-enable button after 15 seconds
            setTimeout(() => {
                if (button.disabled) {
                    button.innerHTML = originalText;
                    button.disabled = false;
                }
            }, 15000);
        });
    });
}

// Utility functions
function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND'
    }).format(amount);
}

function animateNumber(element, start, end, duration = 1000) {
    const startTime = performance.now();

    function update(currentTime) {
        const elapsed = currentTime - startTime;
        const progress = Math.min(elapsed / duration, 1);

        const current = start + (end - start) * progress;
        element.textContent = formatCurrency(current);

        if (progress < 1) {
            requestAnimationFrame(update);
        } else {
            element.textContent = formatCurrency(end);
        }
    }

    requestAnimationFrame(update);
}

// Add custom tooltip styles
const tooltipStyles = `
.ngansach-custom-tooltip {
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

.ngansach-custom-tooltip.show {
    opacity: 1;
    transform: translateY(0);
}

.ngansach-field-error {
    border-color: #dc3545 !important;
    box-shadow: 0 0 0 0.2rem rgba(220, 53, 69, 0.25) !important;
}

.ngansach-field-error-message {
    color: #dc3545;
    font-size: 0.85rem;
    margin-top: 5px;
    display: flex;
    align-items: center;
    gap: 5px;
}

.ngansach-field-error-message::before {
    content: '⚠';
    font-size: 0.8rem;
}
`;

// Inject tooltip styles
const styleElement = document.createElement('style');
styleElement.textContent = tooltipStyles;
document.head.appendChild(styleElement);

// Export functions for global use
window.NganSachUtils = {
    formatCurrency,
    animateNumber,
    showCustomTooltip,
    hideCustomTooltip
};

// Format currency input with thousand separators
function formatCurrencyInput(input) {
    let isFormatted = false;
    let originalValue = input.value;

    // Store original value for form submission
    input.addEventListener('focus', function() {
        if (isFormatted && this.value) {
            // Remove formatting when focusing (allow raw number input)
            const rawValue = this.value.replace(/\./g, '');
            this.value = rawValue;
            isFormatted = false;
        }
    });

    input.addEventListener('blur', function() {
        const currentValue = this.value.trim();
        if (currentValue && !isFormatted) {
            // Add formatting when blurring
            const numericValue = parseFloat(currentValue.replace(/\./g, ''));
            if (!isNaN(numericValue) && numericValue >= 0) {
                this.value = formatNumber(numericValue);
                isFormatted = true;
                originalValue = currentValue;
                
                // Đồng bộ hóa với hidden input nếu có
                const hiddenInput = document.getElementById('HanMucHidden');
                if (hiddenInput) {
                    hiddenInput.value = currentValue;
                    // Disable input text để tránh conflict khi submit
                    this.disabled = true;
                }
            } else if (currentValue !== '') {
                // If invalid, restore original value
                this.value = originalValue || '';
            }
        }
    });

    input.addEventListener('input', function() {
        // Allow only numbers during input (remove dots temporarily)
        let cleanValue = this.value.replace(/[^\d]/g, '');
        
        // Prevent leading zeros
        if (cleanValue.length > 1 && cleanValue.startsWith('0')) {
            cleanValue = cleanValue.substring(1);
        }
        
        this.value = cleanValue;
        isFormatted = false;
    });

    // Initialize formatting if input has value
    if (input.value) {
        const numericValue = parseFloat(input.value.replace(/\./g, ''));
        if (!isNaN(numericValue) && numericValue >= 0) {
            input.value = formatNumber(numericValue);
            isFormatted = true;
            originalValue = input.value.replace(/\./g, '');
        }
    }
}

// Format number with Vietnamese thousand separators
function formatNumber(number) {
    return number.toString().replace(/\B(?=(\d{3})+(?!\d))/g, '.');
}
