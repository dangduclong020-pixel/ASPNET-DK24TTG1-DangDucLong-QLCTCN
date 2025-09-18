using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyCTCN.Data;
using QuanLyCTCN.Models;
using QuanLyCTCN.Models.ViewModels;

namespace QuanLyCTCN.Controllers
{
    public class ThongKeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string _sessionNguoiDungId = "NguoiDungId";

        public ThongKeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /ThongKe
        public async Task<IActionResult> Index(int? thang = null, int? nam = null)
        {
            // Kiểm tra đăng nhập
            var nguoiDungId = HttpContext.Session.GetInt32(_sessionNguoiDungId);
            if (nguoiDungId == null)
            {
                return RedirectToAction("DangNhap", "NguoiDung");
            }

            // Xử lý logic lọc
            DateTime? tuNgay, denNgay;
            
            if (thang == 0 && nam.HasValue)
            {
                // Trường hợp xem cả năm (thang = 0)
                tuNgay = new DateTime(nam.Value, 1, 1);
                denNgay = new DateTime(nam.Value, 12, 31);
            }
            else
            {
                // Trường hợp xem theo tháng
                thang ??= DateTime.Today.Month;
                nam ??= DateTime.Today.Year;
                tuNgay = new DateTime(nam.Value, thang.Value, 1);
                denNgay = new DateTime(nam.Value, thang.Value, DateTime.DaysInMonth(nam.Value, thang.Value));
            }

            // Lấy dữ liệu thu nhập
            var thuNhaps = await _context.ThuNhaps
                .Include(t => t.DanhMuc)
                .Where(t => t.NguoiDungId == nguoiDungId &&
                           t.ThangThuNhap.HasValue && t.NamThuNhap.HasValue &&
                           t.NamThuNhap == nam &&
                           (thang == 0 || t.ThangThuNhap == thang)) // thang = 0 nghĩa là lấy tất cả tháng
                .ToListAsync();

            // Lấy dữ liệu chi tiêu theo ngày chi
            var chiTieus = await _context.ChiTieus
                .Include(c => c.DanhMuc)
                .Where(c => c.NguoiDungId == nguoiDungId &&
                            c.NgayChi >= tuNgay && c.NgayChi <= denNgay)
                .ToListAsync();

            // Tổng thu nhập và chi tiêu
            var tongThuNhap = thuNhaps.Sum(t => t.SoTien);
            var tongChiTieu = chiTieus.Sum(c => c.SoTien);

            // Thống kê thu nhập theo danh mục
            var thuNhapTheoDanhMuc = thuNhaps
                .GroupBy(t => t.DanhMuc)
                .Select(group => new ThongKeTheoDanhMucViewModel
                {
                    DanhMuc = group.Key!,
                    TongTien = group.Sum(t => t.SoTien)
                })
                .ToList();

            // Tính phần trăm cho thu nhập
            if (tongThuNhap > 0)
            {
                foreach (var item in thuNhapTheoDanhMuc)
                {
                    item.PhanTram = Math.Round((item.TongTien / tongThuNhap) * 100, 2);
                }
            }

            // Thống kê chi tiêu theo danh mục
            var chiTieuTheoDanhMuc = chiTieus
                .GroupBy(c => c.DanhMuc)
                .Select(group => new ThongKeTheoDanhMucViewModel
                {
                    DanhMuc = group.Key!,
                    TongTien = group.Sum(c => c.SoTien)
                })
                .ToList();

            // Tính phần trăm cho chi tiêu
            if (tongChiTieu > 0)
            {
                foreach (var item in chiTieuTheoDanhMuc)
                {
                    item.PhanTram = Math.Round((item.TongTien / tongChiTieu) * 100, 2);
                }
            }

            // Thống kê theo thời gian
            var thuNhapTheoNgay = thuNhaps
                .GroupBy(t => {
                    if (t.ThangThuNhap.HasValue && t.NamThuNhap.HasValue)
                    {
                        // Sử dụng ngày đầu tháng từ thang_thu_nhap
                        return new DateTime(t.NamThuNhap.Value, t.ThangThuNhap.Value, 1);
                    }
                    else
                    {
                        // Sử dụng ngay_nhap
                        return t.NgayNhap.Date;
                    }
                })
                .Select(group => new ThongKeTheoThoiGianViewModel
                {
                    Ngay = group.Key,
                    TongTien = group.Sum(t => t.SoTien)
                })
                .OrderBy(item => item.Ngay)
                .ToList();

            var chiTieuTheoNgay = chiTieus
                .GroupBy(c => c.NgayChi.Date)
                .Select(group => new ThongKeTheoThoiGianViewModel
                {
                    Ngay = group.Key,
                    TongTien = group.Sum(c => c.SoTien)
                })
                .OrderBy(item => item.Ngay)
                .ToList();

            // Tạo view model
            var viewModel = new ThongKeViewModel
            {
                NgayBatDau = tuNgay.GetValueOrDefault(DateTime.Today),
                NgayKetThuc = denNgay.GetValueOrDefault(DateTime.Today),
                TongThuNhap = tongThuNhap,
                TongChiTieu = tongChiTieu,
                ThuNhapTheoDanhMuc = thuNhapTheoDanhMuc,
                ChiTieuTheoDanhMuc = chiTieuTheoDanhMuc,
                ThuNhapTheoThoiGian = thuNhapTheoNgay,
                ChiTieuTheoThoiGian = chiTieuTheoNgay
            };

            return View(viewModel);
        }

        // Method riêng để lấy dữ liệu thu nhập
        private async Task<List<ThuNhap>> GetThuNhapData(int? nguoiDungId, DateTime? tuNgay, DateTime? denNgay, string loaiLoc)
        {
            if (loaiLoc == "ThangThuNhap")
            {
                // Lấy thu nhập theo thang_thu_nhap/nam_thu_nhap
                return await _context.ThuNhaps
                    .Include(t => t.DanhMuc)
                    .Where(t => t.NguoiDungId == nguoiDungId &&
                               t.ThangThuNhap.HasValue && t.NamThuNhap.HasValue &&
                               t.ThangThuNhap == tuNgay.Value.Month &&
                               t.NamThuNhap == tuNgay.Value.Year)
                    .ToListAsync();
            }
            else
            {
                // Lọc theo ngay_nhap (mặc định)
                return await _context.ThuNhaps
                    .Include(t => t.DanhMuc)
                    .Where(t => t.NguoiDungId == nguoiDungId &&
                               t.NgayNhap >= tuNgay && t.NgayNhap <= denNgay)
                    .ToListAsync();
            }
        }

        // GET: /ThongKe/TheoThang
        public async Task<IActionResult> TheoThang(int? nam)
        {
            // Kiểm tra đăng nhập
            var nguoiDungId = HttpContext.Session.GetInt32(_sessionNguoiDungId);
            if (nguoiDungId == null)
            {
                return RedirectToAction("DangNhap", "NguoiDung");
            }

            // Mặc định là năm hiện tại
            nam ??= DateTime.Today.Year;

            // Dữ liệu cho biểu đồ các tháng trong năm
            var thongKeTheoThang = new List<(int Thang, decimal ThuNhap, decimal ChiTieu)>();

            for (int thang = 1; thang <= 12; thang++)
            {
                var firstDayOfMonth = new DateTime(nam.Value, thang, 1);
                var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                // Tính tổng thu nhập trong tháng (sử dụng thang_thu_nhap và nam_thu_nhap nếu có)
                var tongThuNhap = await _context.ThuNhaps
                    .Where(t => t.NguoiDungId == nguoiDungId &&
                           ((t.ThangThuNhap.HasValue && t.NamThuNhap.HasValue &&
                             t.ThangThuNhap == thang && t.NamThuNhap == nam.Value) ||
                            (!t.ThangThuNhap.HasValue && !t.NamThuNhap.HasValue &&
                             t.NgayNhap >= firstDayOfMonth && t.NgayNhap <= lastDayOfMonth)))
                    .SumAsync(t => t.SoTien);

                // Tính tổng chi tiêu trong tháng
                var tongChiTieu = await _context.ChiTieus
                    .Where(c => c.NguoiDungId == nguoiDungId &&
                           c.NgayChi >= firstDayOfMonth && c.NgayChi <= lastDayOfMonth)
                    .SumAsync(c => c.SoTien);

                thongKeTheoThang.Add((thang, tongThuNhap, tongChiTieu));
            }

            ViewBag.Nam = nam ?? DateTime.Today.Year;
            ViewBag.ThongKeTheoThang = thongKeTheoThang;

            return View();
        }

        // GET: /ThongKe/SoSanhDanhMuc
        public async Task<IActionResult> SoSanhDanhMuc(string loai, int? thang = null, int? nam = null)
        {
            // Kiểm tra đăng nhập
            var nguoiDungId = HttpContext.Session.GetInt32(_sessionNguoiDungId);
            if (nguoiDungId == null)
            {
                return RedirectToAction("DangNhap", "NguoiDung");
            }

            // Mặc định là chi tiêu và tháng hiện tại
            loai ??= "ChiTieu";
            thang ??= DateTime.Today.Month;
            nam ??= DateTime.Today.Year;

            // Chuyển đổi tháng/năm thành từ ngày/đến ngày
            var tuNgay = new DateTime(nam.Value, thang.Value, 1);
            var denNgay = new DateTime(nam.Value, thang.Value, DateTime.DaysInMonth(nam.Value, thang.Value));

            // Dữ liệu thống kê theo danh mục
            var thongKeTheoDanhMuc = new List<ThongKeTheoDanhMucViewModel>();
            var tongTien = 0m;

            if (loai == "ChiTieu")
            {
                // Thống kê chi tiêu theo danh mục
                var chiTieus = await _context.ChiTieus
                    .Include(c => c.DanhMuc)
                    .Where(c => c.NguoiDungId == nguoiDungId &&
                                c.NgayChi >= tuNgay && c.NgayChi <= denNgay)
                    .ToListAsync();

                tongTien = chiTieus.Sum(c => c.SoTien);

                thongKeTheoDanhMuc = chiTieus
                    .GroupBy(c => c.DanhMuc)
                    .Select(group => new ThongKeTheoDanhMucViewModel
                    {
                        DanhMuc = group.Key!,
                        TongTien = group.Sum(c => c.SoTien)
                    })
                    .ToList();
            }
            else
            {
                // Thống kê thu nhập theo danh mục (sử dụng thang_thu_nhap và nam_thu_nhap)
                var thuNhaps = await _context.ThuNhaps
                    .Include(t => t.DanhMuc)
                    .Where(t => t.NguoiDungId == nguoiDungId &&
                               t.ThangThuNhap.HasValue && t.NamThuNhap.HasValue &&
                               t.ThangThuNhap == thang && t.NamThuNhap == nam)
                    .ToListAsync();

                tongTien = thuNhaps.Sum(t => t.SoTien);

                thongKeTheoDanhMuc = thuNhaps
                    .GroupBy(t => t.DanhMuc)
                    .Select(group => new ThongKeTheoDanhMucViewModel
                    {
                        DanhMuc = group.Key!,
                        TongTien = group.Sum(t => t.SoTien)
                    })
                    .ToList();
            }

            // Tính phần trăm
            if (tongTien > 0)
            {
                foreach (var item in thongKeTheoDanhMuc)
                {
                    item.PhanTram = Math.Round((item.TongTien / tongTien) * 100, 2);
                }
            }

            ViewBag.Loai = loai;
            ViewBag.Thang = thang;
            ViewBag.Nam = nam;
            ViewBag.TongTien = tongTien;

            return View(thongKeTheoDanhMuc);
        }

        // GET: /ThongKe/XuHuongChiTieu
        public async Task<IActionResult> XuHuongChiTieu(int? soThang)
        {
            // Kiểm tra đăng nhập
            var nguoiDungId = HttpContext.Session.GetInt32(_sessionNguoiDungId);
            if (nguoiDungId == null)
            {
                return RedirectToAction("DangNhap", "NguoiDung");
            }

            // Mặc định là 6 tháng gần nhất
            soThang ??= 6;

            // Tính ngày bắt đầu (soThang tháng trước)
            var today = DateTime.Today;
            var startDate = today.AddMonths(-soThang.Value + 1).Date;
            startDate = new DateTime(startDate.Year, startDate.Month, 1);
            
            // Lấy dữ liệu thu nhập và chi tiêu theo tháng
            var result = new List<(string Thang, decimal ThuNhap, decimal ChiTieu)>();

            var currentDate = startDate;
            while (currentDate <= today)
            {
                var firstDayOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
                var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                // Tính tổng thu nhập trong tháng (sử dụng thang_thu_nhap và nam_thu_nhap nếu có)
                var tongThuNhap = await _context.ThuNhaps
                    .Where(t => t.NguoiDungId == nguoiDungId &&
                           ((t.ThangThuNhap.HasValue && t.NamThuNhap.HasValue &&
                             t.ThangThuNhap == currentDate.Month && t.NamThuNhap == currentDate.Year) ||
                            (!t.ThangThuNhap.HasValue && !t.NamThuNhap.HasValue &&
                             t.NgayNhap >= firstDayOfMonth && t.NgayNhap <= lastDayOfMonth)))
                    .SumAsync(t => t.SoTien);

                // Tính tổng chi tiêu trong tháng
                var tongChiTieu = await _context.ChiTieus
                    .Where(c => c.NguoiDungId == nguoiDungId &&
                           c.NgayChi >= firstDayOfMonth && c.NgayChi <= lastDayOfMonth)
                    .SumAsync(c => c.SoTien);

                // Định dạng tháng/năm
                var thangNam = $"{currentDate.Month}/{currentDate.Year}";
                result.Add((thangNam, tongThuNhap, tongChiTieu));

                // Chuyển sang tháng tiếp theo
                currentDate = currentDate.AddMonths(1);
            }

            ViewBag.SoThang = soThang ?? 6;
            ViewBag.ThongKe = result;

            return View();
        }
    }
}
