using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyCTCN.Models
{
    [Table("Chi_Tieu")]
    public class ChiTieu
    {
        [Key]
        [Column("chitieu_id")]
        public int ChiTieuId { get; set; }

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
        [Range(0.01, double.MaxValue, ErrorMessage = "Số tiền phải lớn hơn 0")]
        public decimal SoTien { get; set; }

        [Required]
        [Column("ngay_chi")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày chi")]
        public DateTime NgayChi { get; set; }

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
