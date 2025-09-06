using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyCTCN.Data;
using QuanLyCTCN.Models;

namespace QuanLyCTCN.Controllers
{
    public class ThuNhapController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string _sessionNguoiDungId = "NguoiDungId";

        public ThuNhapController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /ThuNhap
        public async Task<IActionResult> Index(DateTime? tuNgay, DateTime? denNgay)
        {
            // Kiểm tra đăng nhập
            var nguoiDungId = HttpContext.Session.GetInt32(_sessionNguoiDungId);
            if (nguoiDungId == null)
            {
                return RedirectToAction("DangNhap", "NguoiDung");
            }

            // Mặc định lấy dữ liệu của tháng hiện tại
            if (!tuNgay.HasValue)
            {
                var today = DateTime.Today;
                tuNgay = new DateTime(today.Year, today.Month, 1);
                denNgay = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
            }

            var thuNhaps = await _context.ThuNhaps
                .Include(t => t.DanhMuc)
                .Where(t => t.NguoiDungId == nguoiDungId &&
                            t.NgayNhap >= tuNgay && t.NgayNhap <= denNgay)
                .OrderByDescending(t => t.NgayNhap)
                .ToListAsync();

            // Tính tổng thu nhập
            var tongThuNhap = thuNhaps.Sum(t => t.SoTien);
            ViewBag.TongThuNhap = tongThuNhap;
            ViewBag.TuNgay = (tuNgay ?? DateTime.Today).ToString("yyyy-MM-dd");
            ViewBag.DenNgay = (denNgay ?? DateTime.Today).ToString("yyyy-MM-dd");

            return View(thuNhaps);
        }

        // GET: /ThuNhap/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            // Kiểm tra đăng nhập
            var nguoiDungId = HttpContext.Session.GetInt32(_sessionNguoiDungId);
            if (nguoiDungId == null)
            {
                return RedirectToAction("DangNhap", "NguoiDung");
            }

            if (id == null)
            {
                return NotFound();
            }

            var thuNhap = await _context.ThuNhaps
                .Include(t => t.DanhMuc)
                .FirstOrDefaultAsync(m => m.ThuNhapId == id && m.NguoiDungId == nguoiDungId);

            if (thuNhap == null)
            {
                return NotFound();
            }

            return View(thuNhap);
        }

        // GET: /ThuNhap/Create
        public async Task<IActionResult> Create()
        {
            // Kiểm tra đăng nhập
            var nguoiDungId = HttpContext.Session.GetInt32(_sessionNguoiDungId);
            if (nguoiDungId == null)
            {
                return RedirectToAction("DangNhap", "NguoiDung");
            }

            // Lấy danh sách danh mục thu nhập
            ViewBag.DanhMucList = new SelectList(
                await _context.DanhMucs
                    .Where(d => d.Loai == "ThuNhap")
                    .ToListAsync(),
                "DanhMucId", "TenDanhMuc");

            // Đặt ngày mặc định là ngày hiện tại
            var model = new ThuNhap
            {
                NgayNhap = DateTime.Today,
                NguoiDungId = nguoiDungId.Value
            };

            return View(model);
        }

        // POST: /ThuNhap/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ThuNhap thuNhap)
        {
            // Kiểm tra đăng nhập
            var nguoiDungId = HttpContext.Session.GetInt32(_sessionNguoiDungId);
            if (nguoiDungId == null)
            {
                return RedirectToAction("DangNhap", "NguoiDung");
            }

            // Đảm bảo NguoiDungId được thiết lập
            thuNhap.NguoiDungId = nguoiDungId.Value;

            if (ModelState.IsValid)
            {
                _context.Add(thuNhap);
                await _context.SaveChangesAsync();

                // Kiểm tra và cập nhật mục tiêu tiết kiệm nếu cần
                await KiemTraVaCapNhatMucTieu(nguoiDungId.Value);

                TempData["SuccessMessage"] = "Thêm thu nhập thành công!";
                return RedirectToAction(nameof(Index));
            }

            // Nếu không thành công, chuẩn bị dữ liệu cho view
            ViewBag.DanhMucList = new SelectList(
                await _context.DanhMucs
                    .Where(d => d.Loai == "ThuNhap")
                    .ToListAsync(),
                "DanhMucId", "TenDanhMuc", thuNhap.DanhMucId);

            return View(thuNhap);
        }

        // GET: /ThuNhap/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            // Kiểm tra đăng nhập
            var nguoiDungId = HttpContext.Session.GetInt32(_sessionNguoiDungId);
            if (nguoiDungId == null)
            {
                return RedirectToAction("DangNhap", "NguoiDung");
            }

            if (id == null)
            {
                return NotFound();
            }

            var thuNhap = await _context.ThuNhaps
                .FirstOrDefaultAsync(t => t.ThuNhapId == id && t.NguoiDungId == nguoiDungId);

            if (thuNhap == null)
            {
                return NotFound();
            }

            // Lấy danh sách danh mục thu nhập
            ViewBag.DanhMucList = new SelectList(
                await _context.DanhMucs
                    .Where(d => d.Loai == "ThuNhap")
                    .ToListAsync(),
                "DanhMucId", "TenDanhMuc", thuNhap.DanhMucId);

            return View(thuNhap);
        }

        // POST: /ThuNhap/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ThuNhap thuNhap)
        {
            // Kiểm tra đăng nhập
            var nguoiDungId = HttpContext.Session.GetInt32(_sessionNguoiDungId);
            if (nguoiDungId == null)
            {
                return RedirectToAction("DangNhap", "NguoiDung");
            }

            if (id != thuNhap.ThuNhapId)
            {
                return NotFound();
            }

            // Đảm bảo NguoiDungId được thiết lập
            thuNhap.NguoiDungId = nguoiDungId.Value;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(thuNhap);
                    await _context.SaveChangesAsync();

                    // Kiểm tra và cập nhật mục tiêu tiết kiệm nếu cần
                    await KiemTraVaCapNhatMucTieu(nguoiDungId.Value);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ThuNhapExists(thuNhap.ThuNhapId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["SuccessMessage"] = "Cập nhật thu nhập thành công!";
                return RedirectToAction(nameof(Index));
            }

            // Nếu không thành công, chuẩn bị dữ liệu cho view
            ViewBag.DanhMucList = new SelectList(
                await _context.DanhMucs
                    .Where(d => d.Loai == "ThuNhap")
                    .ToListAsync(),
                "DanhMucId", "TenDanhMuc", thuNhap.DanhMucId);

            return View(thuNhap);
        }

        // GET: /ThuNhap/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            // Kiểm tra đăng nhập
            var nguoiDungId = HttpContext.Session.GetInt32(_sessionNguoiDungId);
            if (nguoiDungId == null)
            {
                return RedirectToAction("DangNhap", "NguoiDung");
            }

            if (id == null)
            {
                return NotFound();
            }

            var thuNhap = await _context.ThuNhaps
                .Include(t => t.DanhMuc)
                .FirstOrDefaultAsync(m => m.ThuNhapId == id && m.NguoiDungId == nguoiDungId);

            if (thuNhap == null)
            {
                return NotFound();
            }

            return View(thuNhap);
        }

        // POST: /ThuNhap/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Kiểm tra đăng nhập
            var nguoiDungId = HttpContext.Session.GetInt32(_sessionNguoiDungId);
            if (nguoiDungId == null)
            {
                return RedirectToAction("DangNhap", "NguoiDung");
            }

            var thuNhap = await _context.ThuNhaps
                .FirstOrDefaultAsync(t => t.ThuNhapId == id && t.NguoiDungId == nguoiDungId);

            if (thuNhap != null)
            {
                _context.ThuNhaps.Remove(thuNhap);
                await _context.SaveChangesAsync();

                // Cập nhật lại mục tiêu sau khi xóa thu nhập
                await KiemTraVaCapNhatMucTieu(nguoiDungId.Value);

                TempData["SuccessMessage"] = "Xóa thu nhập thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ThuNhapExists(int id)
        {
            return _context.ThuNhaps.Any(e => e.ThuNhapId == id);
        }

        // Kiểm tra và cập nhật mục tiêu tài chính
        private async Task KiemTraVaCapNhatMucTieu(int nguoiDungId)
        {
            // Lấy danh sách mục tiêu đang theo dõi
            var mucTieus = await _context.MucTieus
                .Where(m => m.NguoiDungId == nguoiDungId && (!m.Han.HasValue || m.Han >= DateTime.Today))
                .ToListAsync();

            if (mucTieus.Any())
            {
                // Tính tổng thu nhập
                var tongThuNhap = await _context.ThuNhaps
                    .Where(t => t.NguoiDungId == nguoiDungId)
                    .SumAsync(t => t.SoTien);

                // Tính tổng chi tiêu
                var tongChiTieu = await _context.ChiTieus
                    .Where(c => c.NguoiDungId == nguoiDungId)
                    .SumAsync(c => c.SoTien);

                // Tính số tiền có thể tiết kiệm (thu nhập - chi tiêu)
                var tienTietKiem = tongThuNhap - tongChiTieu;

                if (tienTietKiem > 0)
                {
                    // Phân bổ tiền tiết kiệm vào các mục tiêu (ưu tiên theo thời gian hết hạn)
                    foreach (var mucTieu in mucTieus.OrderBy(m => m.Han))
                    {
                        var tienCanThemChoMucTieu = mucTieu.SoTienCan - (mucTieu.SoTienDaTietKiem ?? 0);
                        if (tienCanThemChoMucTieu > 0 && tienTietKiem > 0)
                        {
                            var tienThemVaoMucTieu = Math.Min(tienCanThemChoMucTieu, tienTietKiem);
                            mucTieu.SoTienDaTietKiem = (mucTieu.SoTienDaTietKiem ?? 0) + tienThemVaoMucTieu;
                            tienTietKiem -= tienThemVaoMucTieu;
                        }

                        // Nếu mục tiêu đã đạt 100%
                        if (mucTieu.SoTienDaTietKiem >= mucTieu.SoTienCan)
                        {
                            // Tạo nhắc nhở mục tiêu hoàn thành
                            var nhacNho = new NhacNho
                            {
                                NguoiDungId = nguoiDungId,
                                NoiDung = $"Chúc mừng! Bạn đã hoàn thành mục tiêu \"{mucTieu.TenMucTieu}\" với số tiền {mucTieu.SoTienCan:N0} VND.",
                                ThoiGian = DateTime.Now,
                                Loai = "MucTieu"
                            };
                            _context.NhacNhos.Add(nhacNho);
                        }
                    }

                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}
