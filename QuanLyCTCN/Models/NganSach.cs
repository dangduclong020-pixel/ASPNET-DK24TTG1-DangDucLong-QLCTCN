using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyCTCN.Models
{
    [Table("Ngan_Sach")]
    public class NganSach
    {
        [Key]
        [Column("ngansach_id")]
        public int NganSachId { get; set; }

        [Column("nguoidung_id")]
        [Display(Name = "Người dùng")]
        public int? NguoiDungId { get; set; }

        [Column("danhmuc_id")]
        [Display(Name = "Danh mục")]
        public int? DanhMucId { get; set; }

        [Required]
        [Column("han_muc")]
        [Display(Name = "Hạn mức")]
        [DataType(DataType.Currency)]
        public decimal HanMuc { get; set; }

        [Required]
        [Column("thang")]
        [Display(Name = "Tháng")]
        [Range(1, 12)]
        public int Thang { get; set; }

        [Required]
        [Column("nam")]
        [Display(Name = "Năm")]
        [Range(2000, 2100)]
        public int Nam { get; set; }

        // Navigation properties
        [ForeignKey("NguoiDungId")]
        public virtual NguoiDung? NguoiDung { get; set; }

        [ForeignKey("DanhMucId")]
        public virtual DanhMuc? DanhMuc { get; set; }
    }
}
