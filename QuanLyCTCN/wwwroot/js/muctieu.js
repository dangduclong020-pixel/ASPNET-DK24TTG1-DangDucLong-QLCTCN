/* MucTieu JavaScript - Interactive Components */

/* ===== INITIALIZATION ===== */
document.addEventListener('DOMContentLoaded', function() {
    initializeMucTieu();
});

function initializeMucTieu() {
    // Initialize all MucTieu components
    initializeAnimations();
    initializeTooltips();
    initializeConfirmations();
    initializeProgressBars();
    initializeModalHandlers();
    initializeFormValidation();
    initializeCardInteractions();
}

// ===== ANIMATIONS =====
function initializeAnimations() {
    // Add fade-in animations to cards
    const cards = document.querySelectorAll('.card');
    cards.forEach((card, index) => {
        card.classList.add('muctieu-fade-in-up');
        card.style.animationDelay = `${index * 0.1}s`;
    });

    // Add bounce animation to completed goals
    const completedGoals = document.querySelectorAll('.muctieu-goal-card.completed');
    completedGoals.forEach(goal => {
        goal.classList.add('muctieu-celebration');
    });

    // Add slide animations to progress bars
    const progressBars = document.querySelectorAll('.progress-bar');
    progressBars.forEach(bar => {
        bar.classList.add('muctieu-progress-fill');
        const width = bar.style.width;
        bar.style.width = '0%';
        setTimeout(() => {
            bar.style.width = width;
        }, 500);
    });
}

// ===== TOOLTIPS =====
function initializeTooltips() {
    // Initialize Bootstrap tooltips
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Add custom tooltips for goal cards
    const goalCards = document.querySelectorAll('.muctieu-goal-card');
    goalCards.forEach(card => {
        const progress = card.querySelector('.progress-bar');
        if (progress) {
            const percentage = progress.getAttribute('aria-valuenow');
            card.setAttribute('data-tooltip', `${percentage}% hoàn thành`);
            card.classList.add('muctieu-tooltip');
        }
    });
}

// ===== CONFIRMATIONS =====
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
    const itemName = button.getAttribute('data-item-name') || 'mục tiêu này';
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
            cancelButtonText: 'Hủy',
            customClass: {
                popup: 'muctieu-modal'
            }
        }).then((result) => {
            if (result.isConfirmed) {
                // Get the delete URL from the button's href attribute
                const deleteUrl = button.getAttribute('href') || button.href;

                // Create and submit form for delete
                const form = document.createElement('form');
                form.method = 'post';
                form.action = deleteUrl;

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
        const deleteUrl = button.getAttribute('href') || button.href;
        if (confirm(`Bạn có chắc chắn muốn xóa ${itemName}?`)) {
            window.location.href = deleteUrl;
        }
    }
}

// Form reset confirmation
function showResetConfirmation(button) {
    if (typeof Swal !== 'undefined') {
        Swal.fire({
            title: 'Xác nhận đặt lại',
            text: 'Bạn có chắc chắn muốn đặt lại form? Tất cả dữ liệu chưa lưu sẽ bị mất.',
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
                    const inputs = form.querySelectorAll('.form-control');
                    inputs.forEach(input => {
                        if (!input.value) {
                            input.parentElement.classList.remove('focused');
                        }
                    });
                }
            }
        });
    } else {
        if (confirm('Bạn có chắc chắn muốn đặt lại form? Tất cả dữ liệu chưa lưu sẽ bị mất.')) {
            const form = button.closest('form');
            if (form) {
                form.reset();
            }
        }
    }
}

// ===== PROGRESS BARS =====
function initializeProgressBars() {
    const progressBars = document.querySelectorAll('.progress-bar');
    progressBars.forEach(bar => {
        // Add appropriate color classes based on percentage
        const percentage = parseInt(bar.getAttribute('aria-valuenow') || 0);

        if (percentage >= 100) {
            bar.classList.add('bg-success');
        } else if (percentage >= 75) {
            bar.classList.add('bg-info');
        } else if (percentage >= 50) {
            bar.classList.add('bg-warning');
        } else {
            bar.classList.add('bg-danger');
        }

        // Animate progress bar
        animateProgressBar(bar, percentage);
    });
}

function animateProgressBar(bar, targetPercentage) {
    let currentPercentage = 0;
    const increment = targetPercentage / 100;
    const timer = setInterval(() => {
        currentPercentage += increment;
        if (currentPercentage >= targetPercentage) {
            currentPercentage = targetPercentage;
            clearInterval(timer);
        }
        bar.style.width = currentPercentage + '%';
    }, 10);
}

// ===== MODAL HANDLERS =====
function initializeModalHandlers() {
    // Handle update modal
    const capNhatModal = document.getElementById('capNhatModal');
    if (capNhatModal) {
        capNhatModal.addEventListener('show.bs.modal', function (event) {
            const button = event.relatedTarget;
            const id = button.getAttribute('data-id');
            const ten = button.getAttribute('data-ten');

            document.getElementById('mucTieuId').value = id;
            document.querySelector('.modal-title').textContent = `Cập nhật: ${ten}`;
            document.getElementById('capNhatForm').action = `/MucTieu/CapNhatTienTietKiem/${id}`;

            // Clear form
            document.getElementById('capNhatForm').reset();
        });

        capNhatModal.addEventListener('shown.bs.modal', function () {
            // Focus on amount input
            const amountInput = document.getElementById('soTienThem');
            if (amountInput) {
                amountInput.focus();
            }
        });
    }

    // Handle form submission with loading state
    const updateForm = document.getElementById('capNhatForm');
    if (updateForm) {
        updateForm.addEventListener('submit', function(e) {
            const submitBtn = this.querySelector('button[type="submit"]');
            if (submitBtn) {
                submitBtn.classList.add('muctieu-loading');
                submitBtn.disabled = true;
                submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang cập nhật...';
            }
        });
    }
}

// ===== FORM VALIDATION =====
function initializeFormValidation() {
    const forms = document.querySelectorAll('.needs-validation, form');

    forms.forEach(form => {
        const inputs = form.querySelectorAll('input, textarea, select');

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
            const submitBtn = this.querySelector('button[type="submit"]');
            if (submitBtn && !form.checkValidity()) {
                e.preventDefault();
                e.stopPropagation();

                // Show validation errors
                showValidationErrors(form);
            }
            form.classList.add('was-validated');
        });
    });
}

function validateInput(input) {
    const value = input.value.trim();
    const type = input.type;
    let isValid = true;
    let errorMessage = '';

    // Remove existing error messages
    removeErrorMessage(input);

    // Validate based on input type
    switch (type) {
        case 'number':
            if (input.hasAttribute('min') && parseFloat(value) < parseFloat(input.min)) {
                isValid = false;
                errorMessage = `Giá trị phải lớn hơn hoặc bằng ${input.min}`;
            }
            if (input.hasAttribute('max') && parseFloat(value) > parseFloat(input.max)) {
                isValid = false;
                errorMessage = `Giá trị phải nhỏ hơn hoặc bằng ${input.max}`;
            }
            break;
        case 'text':
        case 'textarea':
            if (input.hasAttribute('maxlength') && value.length > parseInt(input.maxlength)) {
                isValid = false;
                errorMessage = `Độ dài tối đa là ${input.maxlength} ký tự`;
            }
            break;
    }

    // Required field validation
    if (input.hasAttribute('required') && !value) {
        isValid = false;
        errorMessage = 'Trường này là bắt buộc';
    }

    // Show/hide error
    if (!isValid) {
        showErrorMessage(input, errorMessage);
        input.classList.add('is-invalid');
        input.classList.remove('is-valid');
    } else if (value) {
        input.classList.add('is-valid');
        input.classList.remove('is-invalid');
    } else {
        input.classList.remove('is-valid', 'is-invalid');
    }

    return isValid;
}

function showErrorMessage(input, message) {
    const errorDiv = document.createElement('div');
    errorDiv.className = 'invalid-feedback';
    errorDiv.textContent = message;
    input.parentElement.appendChild(errorDiv);
}

function removeErrorMessage(input) {
    const errorDiv = input.parentElement.querySelector('.invalid-feedback');
    if (errorDiv) {
        errorDiv.remove();
    }
}

function showValidationErrors(form) {
    const invalidInputs = form.querySelectorAll(':invalid');
    if (invalidInputs.length > 0) {
        invalidInputs[0].scrollIntoView({ behavior: 'smooth', block: 'center' });
        invalidInputs[0].focus();
    }
}

// ===== CARD INTERACTIONS =====
function initializeCardInteractions() {
    const goalCards = document.querySelectorAll('.muctieu-goal-card');

    goalCards.forEach(card => {
        // Add hover effects
        card.addEventListener('mouseenter', function() {
            this.style.transform = 'translateY(-4px) scale(1.02)';
        });

        card.addEventListener('mouseleave', function() {
            this.style.transform = 'translateY(0) scale(1)';
        });

        // Add click effects
        card.addEventListener('click', function(e) {
            // Don't trigger if clicking on buttons/links
            if (e.target.tagName === 'A' || e.target.tagName === 'BUTTON' ||
                e.target.closest('a') || e.target.closest('button')) {
                return;
            }

            // Add click ripple effect
            addRippleEffect(this, e);
        });
    });
}

function addRippleEffect(element, event) {
    const ripple = document.createElement('div');
    const rect = element.getBoundingClientRect();
    const size = Math.max(rect.width, rect.height);
    const x = event.clientX - rect.left - size / 2;
    const y = event.clientY - rect.top - size / 2;

    ripple.style.width = ripple.style.height = size + 'px';
    ripple.style.left = x + 'px';
    ripple.style.top = y + 'px';
    ripple.className = 'ripple-effect';

    element.appendChild(ripple);

    setTimeout(() => {
        ripple.remove();
    }, 600);
}

// ===== UTILITY FUNCTIONS =====
function hasFormChanges() {
    const forms = document.querySelectorAll('form');
    for (let form of forms) {
        const inputs = form.querySelectorAll('input, textarea, select');
        for (let input of inputs) {
            if (input.value !== input.defaultValue) {
                return true;
            }
        }
    }
    return false;
}

// ===== SUCCESS ANIMATIONS =====
function showSuccessAnimation(element, message) {
    // Create success overlay
    const overlay = document.createElement('div');
    overlay.className = 'success-overlay';
    overlay.innerHTML = `
        <div class="success-content">
            <i class="fas fa-check-circle"></i>
            <h4>${message}</h4>
        </div>
    `;

    element.appendChild(overlay);

    // Animate
    setTimeout(() => {
        overlay.classList.add('show');
    }, 100);

    // Remove after animation
    setTimeout(() => {
        overlay.remove();
    }, 3000);
}

// ===== NUMBER FORMATTING =====
function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND'
    }).format(amount);
}

// ===== DATE FORMATTING =====
function formatDate(date) {
    return new Intl.DateTimeFormat('vi-VN', {
        year: 'numeric',
        month: 'long',
        day: 'numeric'
    }).format(new Date(date));
}

// ===== EXPORT FUNCTIONS =====
window.MucTieuUtils = {
    showSuccessAnimation,
    formatCurrency,
    formatDate,
    validateInput
};
