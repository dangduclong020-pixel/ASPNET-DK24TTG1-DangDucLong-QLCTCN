using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuanLyCTCN.Data;
using QuanLyCTCN.Services;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QuanLyCTCN.BackgroundServices
{
    public class ReminderBackgroundService : BackgroundService
    {
        private readonly ILogger<ReminderBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5); // Kiểm tra mỗi 5 phút

        public ReminderBackgroundService(
            ILogger<ReminderBackgroundService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Reminder Background Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckAndSendRemindersAsync();
                    await CheckBudgetWarningsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Reminder Background Service");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("Reminder Background Service is stopping.");
        }

        private async Task CheckAndSendRemindersAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();

            // Lấy các nhắc nhở cần gửi (trong vòng 5 phút tới)
            var now = DateTime.Now;
            var upcomingReminders = await context.NhacNhos
                .Include(n => n.NguoiDung)
                .Where(n => n.ThoiGian >= now && n.ThoiGian <= now.AddMinutes(5))
                .ToListAsync();

            foreach (var reminder in upcomingReminders)
            {
                try
                {
                    if (reminder.NguoiDung != null && !string.IsNullOrEmpty(reminder.NguoiDung.Email))
                    {
                        await emailService.SendReminderNotificationAsync(
                            reminder.NguoiDung.Email,
                            reminder.NguoiDung.HoTen ?? reminder.NguoiDung.TenDangNhap,
                            reminder.NoiDung ?? "",
                            reminder.ThoiGian,
                            reminder.Loai ?? "Khac"
                        );

                        _logger.LogInformation($"Sent reminder email for: {reminder.NoiDung}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error sending reminder email for ID: {reminder.NhacNhoId}");
                }
            }
        }

        private async Task CheckBudgetWarningsAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();

            // Lấy tất cả người dùng
            var users = await context.NguoiDungs.ToListAsync();

            foreach (var user in users)
            {
                try
                {
                    if (string.IsNullOrEmpty(user.Email))
                        continue;

                    // Lấy ngân sách của người dùng trong tháng hiện tại
                    var currentMonth = DateTime.Now.Month;
                    var currentYear = DateTime.Now.Year;

                    var budgets = await context.NganSachs
                        .Include(n => n.DanhMuc)
                        .Where(n => n.NguoiDungId == user.NguoiDungId &&
                                   n.Thang == currentMonth &&
                                   n.Nam == currentYear)
                        .ToListAsync();

                    foreach (var budget in budgets)
                    {
                        // Tính tổng chi tiêu trong tháng cho danh mục này
                        var totalSpending = await context.ChiTieus
                            .Where(c => c.NguoiDungId == user.NguoiDungId &&
                                       c.DanhMucId == budget.DanhMucId &&
                                       c.NgayChi.Month == currentMonth &&
                                       c.NgayChi.Year == currentYear)
                            .SumAsync(c => c.SoTien);

                        // Kiểm tra nếu chi tiêu đã đạt 80% ngân sách
                        var percentage = (totalSpending / budget.HanMuc) * 100;
                        if (percentage >= 80 && percentage < 100)
                        {
                            await emailService.SendBudgetWarningAsync(
                                user.Email,
                                user.HoTen ?? user.TenDangNhap,
                                budget.DanhMuc?.TenDanhMuc ?? "Không xác định",
                                totalSpending,
                                budget.HanMuc
                            );

                            _logger.LogInformation($"Sent budget warning for user {user.TenDangNhap}, category {budget.DanhMuc?.TenDanhMuc}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error checking budget for user ID: {user.NguoiDungId}");
                }
            }
        }
    }
}
