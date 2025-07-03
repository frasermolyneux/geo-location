// Modern GeoLocation Website JavaScript

// Theme Management
class ThemeManager {
    constructor() {
        this.theme = localStorage.getItem('theme') || 'dark'; // Default to dark
        this.init();
    }

    init() {
        this.applyTheme();
        this.setupThemeToggle();
    }

    applyTheme() {
        document.documentElement.setAttribute('data-theme', this.theme);
        localStorage.setItem('theme', this.theme);

        // Update theme toggle icon
        const themeToggle = document.querySelector('.theme-toggle');
        if (themeToggle) {
            const icon = themeToggle.querySelector('.icon');
            if (icon) {
                icon.innerHTML = this.theme === 'dark'
                    ? '<path d="M21.64 13a1 1 0 00-1.05-.68 8.5 8.5 0 01-6.33-6.33 1 1 0 00-1.73-.64 9 9 0 109.71 9.05 1 1 0 00-.6-1.4z"/>'
                    : '<circle cx="12" cy="12" r="5"/><path d="M12 1v2m0 18v2M4.22 4.22l1.42 1.42m12.72 12.72l1.42 1.42M1 12h2m18 0h2M4.22 19.78l1.42-1.42M18.36 5.64l1.42-1.42"/>';
            }
        }
    }

    toggle() {
        this.theme = this.theme === 'dark' ? 'light' : 'dark';
        this.applyTheme();

        // Add a smooth transition effect
        document.body.style.transition = 'background-color 0.3s ease, color 0.3s ease';
        setTimeout(() => {
            document.body.style.transition = '';
        }, 300);
    }

    setupThemeToggle() {
        const themeToggle = document.querySelector('.theme-toggle');
        if (themeToggle) {
            themeToggle.addEventListener('click', (e) => {
                e.preventDefault();
                this.toggle();
            });
        }
    }
}

// Enhanced UI Interactions
class UIEnhancements {
    constructor() {
        this.init();
    }

    init() {
        this.setupSmoothScrolling();
        this.setupFormEnhancements();
        this.setupCardAnimations();
        this.setupTooltips();
        this.setupLoadingStates();
        this.setupCollapseElements();
    }

    setupSmoothScrolling() {
        // Smooth scrolling for anchor links
        document.querySelectorAll('a[href^="#"]').forEach(anchor => {
            anchor.addEventListener('click', function (e) {
                e.preventDefault();
                const target = document.querySelector(this.getAttribute('href'));
                if (target) {
                    target.scrollIntoView({
                        behavior: 'smooth',
                        block: 'start'
                    });
                }
            });
        });
    }

    setupFormEnhancements() {
        // Enhanced form interactions
        document.querySelectorAll('.form-control').forEach(input => {
            // Add floating label effect
            input.addEventListener('focus', function () {
                const parent = this.parentElement;
                if (parent) {
                    parent.classList.add('focused');
                }
            });

            input.addEventListener('blur', function () {
                const parent = this.parentElement;
                if (parent && !this.value) {
                    parent.classList.remove('focused');
                }
            });

            // Initial state check
            if (input.value && input.parentElement) {
                input.parentElement.classList.add('focused');
            }
        });

        // Form validation feedback
        document.querySelectorAll('form').forEach(form => {
            form.addEventListener('submit', function (e) {
                const requiredFields = this.querySelectorAll('[required]');
                let isValid = true;

                requiredFields.forEach(field => {
                    if (!field.value.trim()) {
                        field.classList.add('is-invalid');
                        isValid = false;
                    } else {
                        field.classList.remove('is-invalid');
                    }
                });

                if (!isValid) {
                    e.preventDefault();
                    // Show error message using the notification system
                    if (window.notificationSystem) {
                        window.notificationSystem.show('Please fill in all required fields.', 'error');
                    }

                    // Remove loading state from submit button
                    const submitBtn = this.querySelector('.btn[type="submit"]');
                    if (submitBtn && submitBtn.classList.contains('loading')) {
                        submitBtn.classList.remove('loading');
                        if (submitBtn.dataset.originalContent) {
                            submitBtn.innerHTML = submitBtn.dataset.originalContent;
                        }
                    }
                    return false;
                }
            });
        });
    }

    setupCardAnimations() {
        // Intersection Observer for fade-in animations
        const observerOptions = {
            threshold: 0.1,
            rootMargin: '0px 0px -50px 0px'
        };

        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('fade-in');
                    observer.unobserve(entry.target);
                }
            });
        }, observerOptions);

        document.querySelectorAll('.card, .table, .hero-section').forEach(el => {
            observer.observe(el);
        });
    }

    setupTooltips() {
        // Simple tooltip system
        document.querySelectorAll('[data-tooltip]').forEach(el => {
            el.addEventListener('mouseenter', function () {
                const tooltip = document.createElement('div');
                tooltip.className = 'tooltip';
                tooltip.textContent = this.getAttribute('data-tooltip');
                document.body.appendChild(tooltip);

                const rect = this.getBoundingClientRect();
                tooltip.style.top = (rect.top - tooltip.offsetHeight - 5) + 'px';
                tooltip.style.left = (rect.left + rect.width / 2 - tooltip.offsetWidth / 2) + 'px';
            });

            el.addEventListener('mouseleave', function () {
                const tooltip = document.querySelector('.tooltip');
                if (tooltip) {
                    tooltip.remove();
                }
            });
        });
    }

    setupLoadingStates() {
        // Enhanced loading states for buttons
        document.querySelectorAll('.btn[type="submit"]').forEach(btn => {
            btn.addEventListener('click', function (e) {
                // Check if form is valid before showing loading state
                const form = this.closest('form');
                if (form && form.checkValidity && !form.checkValidity()) {
                    return; // Don't show loading state for invalid forms
                }

                if (!this.disabled) {
                    this.classList.add('loading');
                    // Store original content
                    this.dataset.originalContent = this.innerHTML;
                    this.innerHTML = '<span class="spinner"></span>' + this.textContent;

                    // Reset loading state after a timeout in case form doesn't submit
                    setTimeout(() => {
                        if (this.classList.contains('loading')) {
                            this.classList.remove('loading');
                            this.innerHTML = this.dataset.originalContent || this.innerHTML;
                        }
                    }, 5000);
                }
            });
        });
    }

    setupCollapseElements() {
        // Handle Bootstrap-style collapse elements manually, but only for non-Bootstrap components
        document.querySelectorAll('[data-bs-toggle="collapse"]:not(.navbar-toggler):not(.dropdown-toggle)').forEach(trigger => {
            console.log('Setting up collapse for:', trigger);
            trigger.addEventListener('click', function (e) {
                e.preventDefault();
                console.log('Collapse button clicked');
                const targetSelector = this.getAttribute('data-bs-target');
                const target = document.querySelector(targetSelector);
                console.log('Target element:', target);

                if (target) {
                    const isExpanded = this.getAttribute('aria-expanded') === 'true';
                    console.log('Is expanded:', isExpanded);

                    if (isExpanded) {
                        // Collapse
                        target.classList.remove('show');
                        target.style.display = 'none';
                        this.setAttribute('aria-expanded', 'false');
                        console.log('Collapsed');

                        // Update icon if present
                        const icon = this.querySelector('.icon');
                        if (icon) {
                            icon.style.transform = 'rotate(0deg)';
                        }
                    } else {
                        // Expand
                        target.style.display = 'block';
                        target.classList.add('show');
                        this.setAttribute('aria-expanded', 'true');
                        console.log('Expanded');

                        // Update icon if present
                        const icon = this.querySelector('.icon');
                        if (icon) {
                            icon.style.transform = 'rotate(180deg)';
                        }
                    }
                }
            });
        });

        // Handle Bootstrap dropdown functionality manually since Bootstrap JS might not be loaded
        document.querySelectorAll('[data-bs-toggle="dropdown"]').forEach(trigger => {
            trigger.addEventListener('click', function (e) {
                e.preventDefault();
                const dropdown = this.nextElementSibling;
                if (dropdown && dropdown.classList.contains('dropdown-menu')) {
                    // Close other dropdowns
                    document.querySelectorAll('.dropdown-menu.show').forEach(menu => {
                        if (menu !== dropdown) {
                            menu.classList.remove('show');
                        }
                    });

                    // Toggle current dropdown
                    dropdown.classList.toggle('show');
                }
            });
        });

        // Close dropdowns when clicking outside
        document.addEventListener('click', function (e) {
            if (!e.target.closest('.dropdown')) {
                document.querySelectorAll('.dropdown-menu.show').forEach(menu => {
                    menu.classList.remove('show');
                });
            }
        });

        // Handle mobile navigation toggle
        document.querySelectorAll('.navbar-toggler').forEach(toggler => {
            toggler.addEventListener('click', function (e) {
                e.preventDefault();
                const targetSelector = this.getAttribute('data-bs-target');
                const target = document.querySelector(targetSelector);

                if (target) {
                    target.classList.toggle('show');
                    const isExpanded = target.classList.contains('show');
                    this.setAttribute('aria-expanded', isExpanded.toString());
                }
            });
        });
    }
}

// Map Enhancements
class MapEnhancements {
    constructor() {
        this.init();
    }

    init() {
        // Add loading state to maps
        const mapElements = document.querySelectorAll('#map');
        mapElements.forEach(map => {
            map.classList.add('loading');

            // Remove loading state when map is loaded (this is a placeholder)
            setTimeout(() => {
                map.classList.remove('loading');
            }, 2000);
        });
    }
}

// Notification System
class NotificationSystem {
    constructor() {
        this.container = this.createContainer();
    }

    createContainer() {
        const container = document.createElement('div');
        container.id = 'notification-container';
        container.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            z-index: 9999;
            display: flex;
            flex-direction: column;
            gap: 10px;
        `;
        document.body.appendChild(container);
        return container;
    }

    show(message, type = 'info', duration = 5000) {
        const notification = document.createElement('div');
        notification.className = `notification notification-${type}`;
        notification.innerHTML = `
            <div class="notification-content">
                <span class="notification-message">${message}</span>
                <button class="notification-close" aria-label="Close notification">&times;</button>
            </div>
        `;

        // Apply styles directly for better compatibility
        notification.style.cssText = `
            background: var(--bg-card);
            border: 1px solid var(--border-color);
            border-radius: var(--radius-lg);
            padding: 16px;
            box-shadow: var(--shadow-lg);
            max-width: 400px;
            margin-bottom: 8px;
        `;

        this.container.appendChild(notification);

        // Auto remove
        const timer = setTimeout(() => {
            this.remove(notification);
        }, duration);

        // Manual close
        const closeBtn = notification.querySelector('.notification-close');
        if (closeBtn) {
            closeBtn.addEventListener('click', () => {
                clearTimeout(timer);
                this.remove(notification);
            });
        }

        return notification;
    }

    remove(notification) {
        notification.style.animation = 'fadeOut 0.3s ease-out';
        setTimeout(() => {
            if (notification.parentNode) {
                notification.parentNode.removeChild(notification);
            }
        }, 300);
    }
}

// Performance Monitoring
class PerformanceMonitor {
    constructor() {
        this.init();
    }

    init() {
        // Monitor page load performance
        window.addEventListener('load', () => {
            setTimeout(() => {
                const perfData = performance.getEntriesByType('navigation')[0];
                console.log('Page Load Performance:', {
                    domContentLoaded: perfData.domContentLoadedEventEnd - perfData.domContentLoadedEventStart,
                    loadComplete: perfData.loadEventEnd - perfData.loadEventStart,
                    totalTime: perfData.loadEventEnd - perfData.fetchStart
                });
            }, 0);
        });
    }
}

// Initialize everything when DOM is ready
document.addEventListener('DOMContentLoaded', function () {
    try {
        // Initialize all systems
        window.themeManager = new ThemeManager();
        window.uiEnhancements = new UIEnhancements();
        window.mapEnhancements = new MapEnhancements();
        window.notificationSystem = new NotificationSystem();
        window.performanceMonitor = new PerformanceMonitor();

        // Add global error handling (more selective)
        window.addEventListener('error', function (e) {
            console.error('Global error caught:', e.error);
            // Only show notification for actual JavaScript errors, not navigation or resource loading errors
            if (window.notificationSystem && e.error &&
                !e.filename?.includes('bootstrap') &&
                !e.message?.includes('Script error') &&
                !e.message?.includes('Non-Error promise rejection')) {
                // Avoid showing errors for common issues
                console.log('Showing error notification for:', e.error);
            }
        });

        // Handle unhandled promise rejections (more selective)
        window.addEventListener('unhandledrejection', function (e) {
            console.error('Unhandled promise rejection:', e.reason);
            // Only show notifications for actual network/API errors
            if (window.notificationSystem && e.reason?.name === 'NetworkError') {
                window.notificationSystem.show('A network error occurred. Please check your connection and try again.', 'error');
            }
        });

        // Add loading completion class
        document.body.classList.add('loaded');

        // Store references in global object
        window.GeoLocationApp = {
            themeManager: window.themeManager,
            uiEnhancements: window.uiEnhancements,
            mapEnhancements: window.mapEnhancements,
            notificationSystem: window.notificationSystem
        };
    } catch (error) {
        console.error('Failed to initialize application:', error);
    }
});

// Service Worker Registration (for PWA features)
if ('serviceWorker' in navigator) {
    window.addEventListener('load', function () {
        navigator.serviceWorker.register('/sw.js')
            .then(function (registration) {
                console.log('SW registered: ', registration);
            })
            .catch(function (registrationError) {
                console.log('SW registration failed: ', registrationError);
            });
    });
}