using System.ComponentModel.DataAnnotations;

namespace QuanLyCTCN.Models.ViewModels
{
    public class CaiDatViewModel
    {
        // Thông tin cá nhân
        [Display(Name = "Họ tên")]
        public string? HoTen { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Display(Name = "Số điện thoại")]
        public string? SoDienThoai { get; set; }

        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }

        // Ảnh đại diện
        [Display(Name = "Ảnh đại diện")]
        public IFormFile? AnhDaiDien { get; set; }

        public string? AnhDaiDienHienTai { get; set; }

        // Đổi mật khẩu
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu hiện tại")]
        public string? MatKhauHienTai { get; set; }

        [StringLength(100, ErrorMessage = "Mật khẩu phải có ít nhất {2} ký tự", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        public string? MatKhauMoi { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu mới")]
        [Compare("MatKhauMoi", ErrorMessage = "Mật khẩu mới và xác nhận mật khẩu không khớp")]
        public string? XacNhanMatKhauMoi { get; set; }
    }
}
