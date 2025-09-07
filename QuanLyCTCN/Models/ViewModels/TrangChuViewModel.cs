using System;
using System.Collections.Generic;

namespace QuanLyCTCN.Models.ViewModels
{
    public class TrangChuViewModel
    {
        // Thông tin người dùng
        public NguoiDung NguoiDung { get; set; } = default!;

        // Tổng thu nhập tháng hiện tại
        public decimal TongThuNhapThangHienTai { get; set; }

        // Tổng chi tiêu tháng hiện tại
        public decimal TongChiTieuThangHienTai { get; set; }

        // Số dư hiện tại (Thu nhập - Chi tiêu)
        public decimal SoDuHienTai { get; set; }

        // Tỉ lệ chi tiêu so với thu nhập
        public decimal TiLeChiTieuTrenThuNhap { get; set; }

        // Chi tiêu gần đây
        public IEnumerable<ChiTieu> ChiTieuGanDay { get; set; } = default!;

        // Thu nhập gần đây
        public IEnumerable<ThuNhap> ThuNhapGanDay { get; set; } = default!;

        // Mục tiêu tài chính
        public IEnumerable<MucTieu> MucTieuDangTheoDoiGanNhat { get; set; } = default!;

        // Ngân sách sắp vượt hạn mức
        public IEnumerable<NganSachVuotHanMucViewModel> NganSachGanVuotHanMuc { get; set; } = default!;

        // Nhắc nhở
        public IEnumerable<NhacNho> NhacNhoGanNhat { get; set; } = default!;
        
        // Chi tiêu theo danh mục cho biểu đồ
        public List<BieuDoTheoDanhMucViewModel> ChiTieuTheoDanhMuc { get; set; } = new List<BieuDoTheoDanhMucViewModel>();

        // Thu nhập theo thời gian cho biểu đồ
        public List<BieuDoTheoThoiGianViewModel> ThuNhapTheoThoiGian { get; set; } = new List<BieuDoTheoThoiGianViewModel>();

        // Chi tiêu theo thời gian cho biểu đồ
        public List<BieuDoTheoThoiGianViewModel> ChiTieuTheoThoiGian { get; set; } = new List<BieuDoTheoThoiGianViewModel>();
    }

    public class BieuDoTheoDanhMucViewModel
    {
        public DanhMuc DanhMuc { get; set; } = default!;
        public decimal TongTien { get; set; }
        public decimal PhanTram { get; set; }
    }

    public class BieuDoTheoThoiGianViewModel
    {
        public DateTime Ngay { get; set; }
        public decimal TongTien { get; set; }
    }
}
