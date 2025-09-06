using System;
using System.Collections.Generic;

namespace QuanLyCTCN.Models.ViewModels
{
    public class ThongKeViewModel
    {
        // Khoảng thời gian
        public DateTime NgayBatDau { get; set; }
        public DateTime NgayKetThuc { get; set; }

        // Tổng thu nhập trong khoảng thời gian
        public decimal TongThuNhap { get; set; }

        // Tổng chi tiêu trong khoảng thời gian
        public decimal TongChiTieu { get; set; }

        // Thu nhập theo danh mục
        public List<ThongKeTheoDanhMucViewModel> ThuNhapTheoDanhMuc { get; set; } = new List<ThongKeTheoDanhMucViewModel>();

        // Chi tiêu theo danh mục
        public List<ThongKeTheoDanhMucViewModel> ChiTieuTheoDanhMuc { get; set; } = new List<ThongKeTheoDanhMucViewModel>();

        // Thu nhập theo thời gian
        public List<ThongKeTheoThoiGianViewModel> ThuNhapTheoThoiGian { get; set; } = new List<ThongKeTheoThoiGianViewModel>();

        // Chi tiêu theo thời gian
        public List<ThongKeTheoThoiGianViewModel> ChiTieuTheoThoiGian { get; set; } = new List<ThongKeTheoThoiGianViewModel>();

        // Tổng số dư (Thu nhập - Chi tiêu)
        public decimal TongSoDu => TongThuNhap - TongChiTieu;
    }

    public class ThongKeTheoDanhMucViewModel
    {
        public DanhMuc DanhMuc { get; set; } = default!;
        public decimal TongTien { get; set; }
        public decimal PhanTram { get; set; }
    }

    public class ThongKeTheoThoiGianViewModel
    {
        public DateTime Ngay { get; set; }
        public decimal TongTien { get; set; }
    }
}
