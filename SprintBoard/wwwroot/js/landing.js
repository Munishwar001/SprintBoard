/* wwwroot/js/landing.js */

document.addEventListener('DOMContentLoaded', function () {
    // 1. Dark/Light Theme Switching
    const themeToggleBtn = document.getElementById('theme-toggle');
    const htmlElement = document.documentElement;

    // Load theme preference
    const savedTheme = localStorage.getItem('theme') || (window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light');
    setTheme(savedTheme);

    if (themeToggleBtn) {
        themeToggleBtn.addEventListener('click', () => {
            const currentTheme = htmlElement.getAttribute('data-bs-theme');
            const newTheme = currentTheme === 'dark' ? 'light' : 'dark';
            setTheme(newTheme);
        });
    }

    function setTheme(theme) {
        htmlElement.setAttribute('data-bs-theme', theme);
        localStorage.setItem('theme', theme);
        
        // Update Icon inside toggle button
        const icon = themeToggleBtn ? themeToggleBtn.querySelector('i') : null;
        if (icon) {
            if (theme === 'dark') {
                icon.className = 'fas fa-sun';
            } else {
                icon.className = 'fas fa-moon';
            }
        }
    }

    // 2. Navbar Scroll Style
    const navbar = document.querySelector('.sb-navbar');
    window.addEventListener('scroll', function () {
        if (window.scrollY > 50) {
            navbar.classList.add('scrolled');
        } else {
            navbar.classList.remove('scrolled');
        }
    });

    // 3. Real-Time Activity / SignalR Simulator (Updates UI components periodically to look premium and alive)
    const notificationBadge = document.getElementById('notification-badge');
    const notificationList = document.getElementById('mock-notifications');
    const activityTimeline = document.getElementById('activity-timeline');
    const pendingTasksCount = document.getElementById('pending-tasks-count');
    const completedTasksCount = document.getElementById('completed-tasks-count');
    const taskProgressBar = document.getElementById('task-progress-bar');
    const taskProgressPct = document.getElementById('task-progress-pct');

    const activityTemplates = [
        { user: "Sarah Connor", action: "completed task", target: "Setup EF Core migrations", time: "Just now", badge: "success" },
        { user: "Marcus Wright", action: "created bug", target: "SignalR reconnect socket timeout", time: "Just now", badge: "danger" },
        { user: "Kyle Reese", action: "moved task", target: "Hangfire job configuration to Active", time: "Just now", badge: "primary" },
        { user: "John Connor", action: "reviewed code", target: "Role-Based authorization handler", time: "Just now", badge: "warning" }
    ];

    let pendingCount = 14;
    let completedCount = 42;

    function simulateRealTimeActivity() {
        if (!activityTimeline) return;

        // Choose a random activity
        const act = activityTemplates[Math.floor(Math.random() * activityTemplates.length)];

        // Update counts
        if (act.action.includes('completed')) {
            completedCount++;
            if (pendingCount > 0) pendingCount--;
        } else if (act.action.includes('created')) {
            pendingCount++;
        }

        // Update UI
        if (pendingTasksCount) pendingTasksCount.textContent = pendingCount;
        if (completedTasksCount) completedTasksCount.textContent = completedCount;

        const totalTasks = pendingCount + completedCount;
        const newPct = Math.round((completedCount / totalTasks) * 100);
        if (taskProgressBar) taskProgressBar.style.width = newPct + '%';
        if (taskProgressPct) taskProgressPct.textContent = newPct + '%';

        // Add to recent activity timeline
        const item = document.createElement('div');
        item.className = 'd-flex align-items-start mb-3 border-bottom pb-2 border-light-subtle fade-in-up';
        item.innerHTML = `
            <div class="bg-primary-subtle text-primary rounded-circle p-1 me-2 d-flex align-items-center justify-content-center" style="width: 28px; height: 28px; font-size: 0.75rem;">
                <i class="fas fa-bolt"></i>
            </div>
            <div class="flex-grow-1">
                <p class="mb-0 text-sm"><strong>${act.user}</strong> ${act.action} <span class="text-primary">${act.target}</span></p>
                <small class="text-muted text-xs">${act.time}</small>
            </div>
        `;
        
        if (activityTimeline.firstChild) {
            activityTimeline.insertBefore(item, activityTimeline.firstChild);
        } else {
            activityTimeline.appendChild(item);
        }

        // Keep timeline limited to 4 items
        while (activityTimeline.children.length > 4) {
            activityTimeline.removeChild(activityTimeline.lastChild);
        }

        // Trigger notification simulation
        if (notificationList) {
            const notif = document.createElement('div');
            notif.className = 'alert alert-info py-2 px-3 mb-2 border-0 shadow-sm d-flex align-items-center justify-content-between fade-in-up';
            notif.style.fontSize = '0.85rem';
            notif.innerHTML = `
                <span><i class="fas fa-bell me-2"></i> ${act.user}: ${act.action}</span>
                <span class="badge bg-secondary-subtle text-secondary-emphasis">New</span>
            `;
            notificationList.insertBefore(notif, notificationList.firstChild);
            
            while (notificationList.children.length > 3) {
                notificationList.removeChild(notificationList.lastChild);
            }

            if (notificationBadge) {
                notificationBadge.textContent = parseInt(notificationBadge.textContent || 0) + 1;
                notificationBadge.classList.add('bg-danger');
                notificationBadge.classList.remove('d-none');
            }
        }
    }

    // Run simulator every 8 seconds
    setInterval(simulateRealTimeActivity, 8000);
});
