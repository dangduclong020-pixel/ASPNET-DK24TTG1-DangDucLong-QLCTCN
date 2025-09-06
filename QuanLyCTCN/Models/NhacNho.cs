using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyCTCN.Models
{
    [Table("Nhac_Nho")]
    public class NhacNho
    {
        [Key]
        [Column("nhacnho_id")]
        public int NhacNhoId { get; set; }

        [Column("nguoidung_id")]
        [Display(Name = "Người dùng")]
        public int? NguoiDungId { get; set; }

        [Column("noi_dung")]
        [StringLength(200)]
        [Display(Name = "Nội dung")]
        public string? NoiDung { get; set; }

        [Required]
        [Column("thoi_gian")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Thời gian")]
        public DateTime ThoiGian { get; set; }

        [Column("loai")]
        [StringLength(50)]
        [Display(Name = "Loại nhắc nhở")]
        public string? Loai { get; set; }

        // Navigation properties
        [ForeignKey("NguoiDungId")]
        public virtual NguoiDung? NguoiDung { get; set; }
    }
}
