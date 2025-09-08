using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyCTCN.Data;
using QuanLyCTCN.Models;
using QuanLyCTCN.Models.ViewModels;

namespace QuanLyCTCN.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Kiểm tra đăng nhập
            var nguoiDungId = GetCurrentUserId();
            
            // Log để kiểm tra giá trị Session
            Console.WriteLine($"HomeController.Index: NguoiDungId = {nguoiDungId}");
            
            if (nguoiDungId == null)
            {
                // Hiển thị trang chủ cho khách (không đăng nhập)
                return View("IndexGuest");
            }

            var nguoiDung = await _context.NguoiDungs
                .FirstOrDefaultAsync(n => n.NguoiDungId == nguoiDungId);

            if (nguoiDung == null)
            {
                return NotFound();
            }

            // Lấy tháng và năm hiện tại
            var today = DateTime.Today;
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            // Tính tổng thu nhập tháng hiện tại (dựa trên thang_thu_nhap và nam_thu_nhap)
            var tongThuNhapThang = await _context.ThuNhaps
                .Where(t => t.NguoiDungId == nguoiDungId &&
                       t.ThangThuNhap.HasValue && t.NamThuNhap.HasValue &&
                       t.ThangThuNhap == today.Month && t.NamThuNhap == today.Year)
                .SumAsync(t => t.SoTien);

            // Tính tổng chi tiêu tháng hiện tại
            var tongChiTieuThang = await _context.ChiTieus
                .Where(c => c.NguoiDungId == nguoiDungId &&
                       c.NgayChi >= firstDayOfMonth && c.NgayChi <= lastDayOfMonth)
                .SumAsync(c => c.SoTien);

            // Số dư hiện tại
            var soDuHienTai = tongThuNhapThang - tongChiTieuThang;

            // Tỉ lệ chi tiêu / thu nhập
            var tiLeChiTieuTrenThuNhap = tongThuNhapThang > 0 ?
                Math.Round((tongChiTieuThang / tongThuNhapThang) * 100, 2) : 0;

            // Lấy 5 giao dịch chi tiêu gần nhất
            var chiTieuGanDay = await _context.ChiTieus
                .Where(c => c.NguoiDungId == nguoiDungId)
                .Include(c => c.DanhMuc)
                .OrderByDescending(c => c.NgayChi)
                .Take(5)
                .ToListAsync();

            // Lấy 5 giao dịch thu nhập gần nhất (sắp xếp theo ngay_nhap)
            var thuNhapGanDay = await _context.ThuNhaps
                .Where(t => t.NguoiDungId == nguoiDungId)
                .Include(t => t.DanhMuc)
                .OrderByDescending(t => t.NgayNhap)
                .Take(5)
                .ToListAsync();

            // Lấy 3 mục tiêu tài chính gần nhất
            var mucTieuGanNhat = await _context.MucTieus
                .Where(m => m.NguoiDungId == nguoiDungId)
                .OrderBy(m => m.Han)
                .Take(3)
                .ToListAsync();

            // Lấy thông tin ngân sách gần vượt hạn mức (sử dụng trên 80%)
            var nganSachThangHienTai = await _context.NganSachs
                .Where(n => n.NguoiDungId == nguoiDungId && n.Thang == today.Month && n.Nam == today.Year)
                .Include(n => n.DanhMuc)
                .ToListAsync();

            var nganSachGanVuotHanMuc = new List<NganSachVuotHanMucViewModel>();

            foreach (var nganSach in nganSachThangHienTai)
            {
                var tongChiTieuTheoDanhMuc = await _context.ChiTieus
                    .Where(c => c.NguoiDungId == nguoiDungId &&
                           c.DanhMucId == nganSach.DanhMucId &&
                           c.NgayChi >= firstDayOfMonth && c.NgayChi <= lastDayOfMonth)
                    .SumAsync(c => c.SoTien);

                var phanTramDaSuDung = nganSach.HanMuc > 0 ?
                    Math.Round((tongChiTieuTheoDanhMuc / nganSach.HanMuc) * 100, 2) : 0;

                if (phanTramDaSuDung >= 80)
                {
                    nganSachGanVuotHanMuc.Add(new NganSachVuotHanMucViewModel
                    {
                        NganSach = nganSach,
                        TongChiTieu = tongChiTieuTheoDanhMuc,
                        PhanTramDaSuDung = phanTramDaSuDung
                    });
                }
            }

            // Lấy các nhắc nhở gần nhất
            var nhacNhoGanNhat = await _context.NhacNhos
                .Where(n => n.NguoiDungId == nguoiDungId && n.ThoiGian >= today)
                .OrderBy(n => n.ThoiGian)
                .Take(5)
                .ToListAsync();

            // Dữ liệu chi tiêu theo danh mục cho biểu đồ
            var chiTieuTheoDanhMuc = await _context.ChiTieus
                .Where(c => c.NguoiDungId == nguoiDungId &&
                       c.NgayChi >= firstDayOfMonth && c.NgayChi <= lastDayOfMonth)
                .GroupBy(c => c.DanhMuc)
                .Select(group => new BieuDoTheoDanhMucViewModel
                {
                    DanhMuc = group.Key!,
                    TongTien = group.Sum(c => c.SoTien)
                })
                .ToListAsync();

            // Tính phần trăm
            var tongChiTieuDanhMuc = chiTieuTheoDanhMuc.Sum(c => c.TongTien);
            if (tongChiTieuDanhMuc > 0)
            {
                foreach (var item in chiTieuTheoDanhMuc)
                {
                    item.PhanTram = Math.Round((item.TongTien / tongChiTieuDanhMuc) * 100, 2);
                }
            }

            // Dữ liệu thu nhập theo thời gian (hiển thị ngày thực tế)
            var thuNhapTheoThoiGian = await _context.ThuNhaps
                .Where(t => t.NguoiDungId == nguoiDungId &&
                       t.ThangThuNhap.HasValue && t.NamThuNhap.HasValue &&
                       t.ThangThuNhap == today.Month && t.NamThuNhap == today.Year)  // Thêm điều kiện tháng hiện tại
                .GroupBy(t => t.NgayNhap.Date)
                .Select(group => new BieuDoTheoThoiGianViewModel
                {
                    Ngay = group.Key,
                    TongTien = group.Sum(t => t.SoTien)
                })
                .OrderBy(item => item.Ngay)
                .ToListAsync();

            // Dữ liệu chi tiêu theo thời gian (đã đúng - hiển thị ngày thực tế)
            var chiTieuTheoThoiGian = await _context.ChiTieus
                .Where(c => c.NguoiDungId == nguoiDungId &&
                       c.NgayChi >= firstDayOfMonth && c.NgayChi <= lastDayOfMonth)
                .GroupBy(c => c.NgayChi.Date)
                .Select(group => new BieuDoTheoThoiGianViewModel
                {
                    Ngay = group.Key,
                    TongTien = group.Sum(c => c.SoTien)
                })
                .OrderBy(item => item.Ngay)
                .ToListAsync();

            var viewModel = new TrangChuViewModel
            {
                NguoiDung = nguoiDung,
                TongThuNhapThangHienTai = tongThuNhapThang,
                TongChiTieuThangHienTai = tongChiTieuThang,
                SoDuHienTai = soDuHienTai,
                TiLeChiTieuTrenThuNhap = tiLeChiTieuTrenThuNhap,
                ChiTieuGanDay = chiTieuGanDay,
                ThuNhapGanDay = thuNhapGanDay,
                MucTieuDangTheoDoiGanNhat = mucTieuGanNhat,
                NganSachGanVuotHanMuc = nganSachGanVuotHanMuc,
                NhacNhoGanNhat = nhacNhoGanNhat,
                ChiTieuTheoDanhMuc = chiTieuTheoDanhMuc,
                ThuNhapTheoThoiGian = thuNhapTheoThoiGian,
                ChiTieuTheoThoiGian = chiTieuTheoThoiGian
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
