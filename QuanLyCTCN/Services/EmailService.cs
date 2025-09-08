using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace QuanLyCTCN.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var smtpSettings = _configuration.GetSection("SmtpSettings");

                using var client = new SmtpClient
                {
                    Host = smtpSettings["Host"] ?? "smtp.gmail.com",
                    Port = int.Parse(smtpSettings["Port"] ?? "587"),
                    EnableSsl = bool.Parse(smtpSettings["EnableSsl"] ?? "true"),
                    Credentials = new NetworkCredential(
                        smtpSettings["Username"],
                        smtpSettings["Password"]
                    )
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpSettings["FromEmail"] ?? smtpSettings["Username"]!, "Quản lý Chi tiêu"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                // Log lỗi nhưng không throw exception để không làm dừng ứng dụng
                Console.WriteLine($"Lỗi gửi email: {ex.Message}");
            }
        }

        public async Task SendReminderNotificationAsync(string toEmail, string userName, string reminderContent, DateTime reminderTime, string reminderType)
        {
            var subject = $"Nhắc nhở: {reminderType} - {userName}";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #4285f4;'>Nhắc nhở từ ứng dụng Quản lý Chi tiêu</h2>
                    <div style='background-color: #f8f9fa; padding: 20px; border-radius: 5px; margin: 20px 0;'>
                        <h3>Xin chào {userName}!</h3>
                        <p><strong>Nội dung nhắc nhở:</strong></p>
                        <p style='font-size: 16px; color: #333;'>{reminderContent}</p>
                        <p><strong>Thời gian:</strong> {reminderTime:dd/MM/yyyy HH:mm}</p>
                        <p><strong>Loại:</strong> {GetReminderTypeText(reminderType)}</p>
                    </div>
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='#' style='background-color: #4285f4; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; display: inline-block;'>
                            Truy cập ứng dụng
                        </a>
                    </div>
                    <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;'>
                    <p style='color: #666; font-size: 12px; text-align: center;'>
                        Đây là email tự động từ hệ thống Quản lý Chi tiêu cá nhân.<br>
                        Vui lòng không trả lời email này.
                    </p>
                </div>
            ";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendBudgetWarningAsync(string toEmail, string userName, string categoryName, decimal currentSpending, decimal budgetLimit)
        {
            var subject = $"Cảnh báo ngân sách: {categoryName}";
            var percentage = (currentSpending / budgetLimit) * 100;
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #ff6b6b;'>⚠️ Cảnh báo vượt ngân sách</h2>
                    <div style='background-color: #fff3cd; border: 1px solid #ffeaa7; padding: 20px; border-radius: 5px; margin: 20px 0;'>
                        <h3>Xin chào {userName}!</h3>
                        <p><strong>Cảnh báo:</strong> Chi tiêu của bạn cho danh mục <strong>{categoryName}</strong> đã đạt <strong>{percentage:F1}%</strong> giới hạn ngân sách.</p>
                        <div style='background-color: white; padding: 15px; border-radius: 5px; margin: 15px 0;'>
                            <p><strong>Số tiền đã chi:</strong> {currentSpending:N0} VND</p>
                            <p><strong>Giới hạn ngân sách:</strong> {budgetLimit:N0} VND</p>
                            <p><strong>Còn lại:</strong> {(budgetLimit - currentSpending):N0} VND</p>
                        </div>
                        <p style='color: #856404;'>Vui lòng theo dõi và điều chỉnh chi tiêu để tránh vượt quá ngân sách đã đề ra.</p>
                    </div>
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='#' style='background-color: #ff6b6b; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; display: inline-block;'>
                            Xem chi tiết
                        </a>
                    </div>
                </div>
            ";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendGoalAchievementAsync(string toEmail, string userName, string goalName, decimal targetAmount)
        {
            var subject = $"🎉 Chúc mừng! Đã hoàn thành mục tiêu";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #51cf66;'>🎉 Chúc mừng! Đã hoàn thành mục tiêu</h2>
                    <div style='background-color: #d4edda; border: 1px solid #c3e6cb; padding: 20px; border-radius: 5px; margin: 20px 0;'>
                        <h3>Xin chào {userName}!</h3>
                        <p>Chúc mừng bạn đã hoàn thành mục tiêu <strong>" + goalName + $@"</strong> với số tiền <strong>{targetAmount:N0} VND</strong>!</p>
                        <p>🎊 Bạn đã làm được một việc tuyệt vời! Tiếp tục duy trì thói quen tiết kiệm và đặt ra những mục tiêu mới nhé!</p>
                    </div>
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='#' style='background-color: #51cf66; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; display: inline-block;'>
                            Đặt mục tiêu mới
                        </a>
                    </div>
                </div>
            ";

            await SendEmailAsync(toEmail, subject, body);
        }

        private string GetReminderTypeText(string reminderType)
        {
            return reminderType switch
            {
                "ChiTieu" => "Chi tiêu",
                "ThuNhap" => "Thu nhập",
                "NganSach" => "Ngân sách",
                "MucTieu" => "Mục tiêu",
                _ => "Khác"
            };
        }
    }
}
