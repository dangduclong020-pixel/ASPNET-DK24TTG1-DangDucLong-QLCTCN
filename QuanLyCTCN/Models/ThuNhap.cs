using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyCTCN.Models
{
    [Table("Thu_Nhap")]
    public class ThuNhap
    {
        [Key]
        [Column("thunhap_id")]
        public int ThuNhapId { get; set; }

        [Column("nguoidung_id")]
        [Display(Name = "Người dùng")]
        public int? NguoiDungId { get; set; }

        [Column("danhmuc_id")]
        [Display(Name = "Danh mục")]
        public int? DanhMucId { get; set; }

        [Required]
        [Column("so_tien")]
        [Display(Name = "Số tiền")]
        [DataType(DataType.Currency)]
        public decimal SoTien { get; set; }

        [Required]
        [Column("ngay_nhap")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày nhập")]
        public DateTime NgayNhap { get; set; }

        [Column("ghi_chu")]
        [StringLength(200)]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        // Navigation properties
        [ForeignKey("NguoiDungId")]
        public virtual NguoiDung? NguoiDung { get; set; }

        [ForeignKey("DanhMucId")]
        public virtual DanhMuc? DanhMuc { get; set; }
    }
}
