// Xử lý hiển thị/ẩn sidebar trên thiết bị di động
document.addEventListener('DOMContentLoaded', function () {
    // Thêm chức năng toggle sidebar
    const menuToggle = document.querySelector('.menu-toggle');
    const sidebar = document.querySelector('.sidebar');
    
    if (menuToggle) {
        menuToggle.addEventListener('click', function() {
            sidebar.classList.toggle('show');
        });
    }

    // Xử lý user menu dropdown
    const userIcon = document.querySelector('.user-icon');
    const userDropdown = document.querySelector('.user-dropdown');
    
    if (userIcon && userDropdown) {
        userIcon.addEventListener('click', function() {
            userDropdown.classList.toggle('show');
        });
        
        // Ẩn dropdown khi click ra ngoài
        document.addEventListener('click', function(e) {
            if (!userIcon.contains(e.target) && !userDropdown.contains(e.target)) {
                userDropdown.classList.remove('show');
            }
        });
    }

    // Thêm active class cho menu item hiện tại
    const currentLocation = window.location.pathname;
    const menuItems = document.querySelectorAll('.sidebar-menu a');
    
    menuItems.forEach(item => {
        const href = item.getAttribute('href');
        if (href === currentLocation) {
            item.classList.add('active');
        }
    });
});
