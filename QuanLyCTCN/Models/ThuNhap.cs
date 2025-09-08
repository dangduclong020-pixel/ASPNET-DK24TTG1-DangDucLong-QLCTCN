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
        [Range(0.01, double.MaxValue, ErrorMessage = "Số tiền phải lớn hơn 0")]
        public decimal SoTien { get; set; }

        [Required]
        [Column("ngay_nhap")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày nhận thực tế")]
        public DateTime NgayNhap { get; set; }

        [Column("thang_thu_nhap")]
        [Display(Name = "Tháng thu nhập")]
        [Range(1, 12, ErrorMessage = "Tháng phải từ 1-12")]
        public int? ThangThuNhap { get; set; }

        [Column("nam_thu_nhap")]
        [Display(Name = "Năm thu nhập")]
        [Range(2020, 2030, ErrorMessage = "Năm phải từ 2020-2030")]
        public int? NamThuNhap { get; set; }

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
