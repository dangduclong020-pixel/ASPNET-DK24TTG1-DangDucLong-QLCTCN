using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyCTCN.Models
{
    [Table("Muc_Tieu")]
    public class MucTieu
    {
        [Key]
        [Column("muctieu_id")]
        public int MucTieuId { get; set; }

        [Column("nguoidung_id")]
        [Display(Name = "Người dùng")]
        public int? NguoiDungId { get; set; }

        [Required]
        [Column("ten_muc_tieu")]
        [StringLength(100)]
        [Display(Name = "Tên mục tiêu")]
        public string TenMucTieu { get; set; } = string.Empty;

        [Required]
        [Column("so_tien_can")]
        [Display(Name = "Số tiền cần")]
        [DataType(DataType.Currency)]
        public decimal SoTienCan { get; set; }

        [Column("so_tien_da_tiet_kiem")]
        [Display(Name = "Số tiền đã tiết kiệm")]
        [DataType(DataType.Currency)]
        public decimal? SoTienDaTietKiem { get; set; }

        [Column("han")]
        [DataType(DataType.Date)]
        [Display(Name = "Thời hạn")]
        public DateTime? Han { get; set; }

        // Navigation properties
        [ForeignKey("NguoiDungId")]
        public virtual NguoiDung? NguoiDung { get; set; }

        // Tính phần trăm đã đạt được
        [NotMapped]
        [Display(Name = "Tiến độ")]
        public int PhanTramHoanThanh
        {
            get
            {
                if (SoTienCan <= 0)
                    return 0;
                
                decimal tienDaTietKiem = SoTienDaTietKiem ?? 0;
                return (int)Math.Min(100, Math.Round((tienDaTietKiem / SoTienCan) * 100));
            }
        }
    }
}
