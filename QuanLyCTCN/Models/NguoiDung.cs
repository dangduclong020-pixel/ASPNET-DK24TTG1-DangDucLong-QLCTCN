using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyCTCN.Models
{
    [Table("Nguoi_Dung")]
    public class NguoiDung
    {
        [Key]
        [Column("nguoidung_id")]
        public int NguoiDungId { get; set; }

        [Required]
        [Column("ten_dang_nhap")]
        [StringLength(50)]
        [Display(Name = "Tên đăng nhập")]
        public string TenDangNhap { get; set; } = string.Empty;

        [Required]
        [Column("mat_khau")]
        [StringLength(255)]
        [Display(Name = "Mật khẩu")]
        [DataType(DataType.Password)]
        public string MatKhau { get; set; } = string.Empty;

        [Column("ho_ten")]
        [StringLength(100)]
        [Display(Name = "Họ tên")]
        public string? HoTen { get; set; }

        [Column("email")]
        [StringLength(100)]
        [EmailAddress]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Column("so_dien_thoai")]
        [StringLength(20)]
        [Phone]
        [Display(Name = "Số điện thoại")]
        public string? SoDienThoai { get; set; }

        [Column("dia_chi")]
        [StringLength(200)]
        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }

        [Column("anh_dai_dien")]
        [StringLength(255)]
        [Display(Name = "Ảnh đại diện")]
        public string? AnhDaiDien { get; set; }

        [Column("lan_dang_nhap_that_bai")]
        [Display(Name = "Số lần đăng nhập thất bại")]
        public int? LanDangNhapThatBai { get; set; }

        [Column("khoa_den")]
        [Display(Name = "Khóa đến")]
        public DateTime? KhoaDen { get; set; }

        // Navigation properties
        public virtual ICollection<ChiTieu> ChiTieus { get; set; } = new List<ChiTieu>();
        public virtual ICollection<ThuNhap> ThuNhaps { get; set; } = new List<ThuNhap>();
        public virtual ICollection<MucTieu> MucTieus { get; set; } = new List<MucTieu>();
        public virtual ICollection<NganSach> NganSaches { get; set; } = new List<NganSach>();
        public virtual ICollection<NhacNho> NhacNhos { get; set; } = new List<NhacNho>();
    }
}
