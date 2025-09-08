using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyCTCN.Models
{
    [Table("Danh_Muc")]
    public class DanhMuc
    {
        [Key]
        [Column("danhmuc_id")]
        public int DanhMucId { get; set; }

        [Required]
        [Column("ten_danh_muc")]
        [StringLength(100)]
        [Display(Name = "Tên danh mục")]
        public string TenDanhMuc { get; set; } = string.Empty;

        [Column("loai")]
        [StringLength(20)]
        [Display(Name = "Loại")]
        public string Loai { get; set; } = "ChiTieu"; // "ThuNhap" hoặc "ChiTieu"

        [Column("nhom")]
        [StringLength(20)]
        [Display(Name = "Nhóm")]
        public string? Nhom { get; set; } // "CoDinh" hoặc "BienDoi"

        [Column("ghi_chu")]
        [StringLength(200)]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        [Column("nguoidung_id")]
        [Display(Name = "Người dùng")]
        public int? NguoiDungId { get; set; }

        // Navigation properties
        public virtual NguoiDung? NguoiDung { get; set; }
        public virtual ICollection<ChiTieu> ChiTieus { get; set; } = new List<ChiTieu>();
        public virtual ICollection<ThuNhap> ThuNhaps { get; set; } = new List<ThuNhap>();
        public virtual ICollection<NganSach> NganSaches { get; set; } = new List<NganSach>();
    }
}
