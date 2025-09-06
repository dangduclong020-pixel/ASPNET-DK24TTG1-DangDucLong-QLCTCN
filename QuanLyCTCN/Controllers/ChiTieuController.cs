using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyCTCN.Data;
using QuanLyCTCN.Models;

namespace QuanLyCTCN.Controllers
{
    public class ChiTieuController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private const string _sessionNguoiDungId = "NguoiDungId";

        public ChiTieuController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /ChiTieu
        public async Task<IActionResult> Index(DateTime? tuNgay, DateTime? denNgay)
        {
            // Kiểm tra đăng nhập
            var redirectResult = RedirectToLoginIfNotAuthenticated();
            if (redirectResult != null)
            {
                return redirectResult;
            }
            
            var nguoiDungId = GetCurrentUserId();

            // Mặc định lấy dữ liệu của tháng hiện tại
            if (!tuNgay.HasValue)
            {
                var today = DateTime.Today;
                tuNgay = new DateTime(today.Year, today.Month, 1);
                denNgay = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
            }

            var chiTieus = await _context.ChiTieus
                .Include(c => c.DanhMuc)
                .Where(c => c.NguoiDungId == nguoiDungId &&
                            c.NgayChi >= tuNgay && c.NgayChi <= denNgay)
                .OrderByDescending(c => c.NgayChi)
                .ToListAsync();

            // Tính tổng chi tiêu
            var tongChiTieu = chiTieus.Sum(c => c.SoTien);
            ViewBag.TongChiTieu = tongChiTieu;
            ViewBag.TuNgay = tuNgay?.ToString("yyyy-MM-dd") ?? "";
            ViewBag.DenNgay = denNgay?.ToString("yyyy-MM-dd") ?? "";

            return View(chiTieus);
        }

        // GET: /ChiTieu/Details/5
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

            var chiTieu = await _context.ChiTieus
                .Include(c => c.DanhMuc)
                .FirstOrDefaultAsync(m => m.ChiTieuId == id && m.NguoiDungId == nguoiDungId);

            if (chiTieu == null)
            {
                return NotFound();
            }

            return View(chiTieu);
        }

        // GET: /ChiTieu/Create
        public async Task<IActionResult> Create()
        {
            // Kiểm tra đăng nhập
            var nguoiDungId = HttpContext.Session.GetInt32(_sessionNguoiDungId);
            if (nguoiDungId == null)
            {
                return RedirectToAction("DangNhap", "NguoiDung");
            }

            // Lấy danh sách danh mục chi tiêu
            ViewBag.DanhMucList = new SelectList(
                await _context.DanhMucs
                    .Where(d => d.Loai == "ChiTieu")
                    .ToListAsync(),
                "DanhMucId", "TenDanhMuc");

            // Đặt ngày mặc định là ngày hiện tại
            var model = new ChiTieu
            {
                NgayChi = DateTime.Today,
                NguoiDungId = nguoiDungId
            };

            return View(model);
        }

        // POST: /ChiTieu/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ChiTieu chiTieu)
        {
            // Kiểm tra đăng nhập
            var nguoiDungId = HttpContext.Session.GetInt32(_sessionNguoiDungId);
            if (nguoiDungId == null)
            {
                return RedirectToAction("DangNhap", "NguoiDung");
            }

            // Đảm bảo NguoiDungId được thiết lập
            chiTieu.NguoiDungId = nguoiDungId;

            if (ModelState.IsValid)
            {
                _context.Add(chiTieu);
                await _context.SaveChangesAsync();

                // Kiểm tra ngân sách và cập nhật nếu cần
                await KiemTraVaCapNhatNganSach(chiTieu);

                TempData["SuccessMessage"] = "Thêm chi tiêu thành công!";
                return RedirectToAction(nameof(Index));
            }

            // Nếu không thành công, chuẩn bị dữ liệu cho view
            ViewBag.DanhMucList = new SelectList(
                await _context.DanhMucs
                    .Where(d => d.Loai == "ChiTieu")
                    .ToListAsync(),
                "DanhMucId", "TenDanhMuc", chiTieu.DanhMucId);

            return View(chiTieu);
        }

        // GET: /ChiTieu/Edit/5
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

            var chiTieu = await _context.ChiTieus
                .FirstOrDefaultAsync(c => c.ChiTieuId == id && c.NguoiDungId == nguoiDungId);

            if (chiTieu == null)
            {
                return NotFound();
            }

            // Lấy danh sách danh mục chi tiêu
            ViewBag.DanhMucList = new SelectList(
                await _context.DanhMucs
                    .Where(d => d.Loai == "ChiTieu")
                    .ToListAsync(),
                "DanhMucId", "TenDanhMuc", chiTieu.DanhMucId);

            return View(chiTieu);
        }

        // POST: /ChiTieu/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ChiTieu chiTieu)
        {
            // Kiểm tra đăng nhập
            var nguoiDungId = HttpContext.Session.GetInt32(_sessionNguoiDungId);
            if (nguoiDungId == null)
            {
                return RedirectToAction("DangNhap", "NguoiDung");
            }

            if (id != chiTieu.ChiTieuId)
            {
                return NotFound();
            }

            // Đảm bảo NguoiDungId được thiết lập
            chiTieu.NguoiDungId = nguoiDungId;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(chiTieu);
                    await _context.SaveChangesAsync();

                    // Kiểm tra ngân sách và cập nhật nếu cần
                    await KiemTraVaCapNhatNganSach(chiTieu);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChiTieuExists(chiTieu.ChiTieuId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["SuccessMessage"] = "Cập nhật chi tiêu thành công!";
                return RedirectToAction(nameof(Index));
            }

            // Nếu không thành công, chuẩn bị dữ liệu cho view
            ViewBag.DanhMucList = new SelectList(
                await _context.DanhMucs
                    .Where(d => d.Loai == "ChiTieu")
                    .ToListAsync(),
                "DanhMucId", "TenDanhMuc", chiTieu.DanhMucId);

            return View(chiTieu);
        }

        // GET: /ChiTieu/Delete/5
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

            var chiTieu = await _context.ChiTieus
                .Include(c => c.DanhMuc)
                .FirstOrDefaultAsync(m => m.ChiTieuId == id && m.NguoiDungId == nguoiDungId);

            if (chiTieu == null)
            {
                return NotFound();
            }

            return View(chiTieu);
        }

        // POST: /ChiTieu/Delete/5
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

            var chiTieu = await _context.ChiTieus
                .FirstOrDefaultAsync(c => c.ChiTieuId == id && c.NguoiDungId == nguoiDungId);

            if (chiTieu != null)
            {
                _context.ChiTieus.Remove(chiTieu);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa chi tiêu thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ChiTieuExists(int id)
        {
            return _context.ChiTieus.Any(e => e.ChiTieuId == id);
        }

        // Kiểm tra và cập nhật ngân sách
        private async Task KiemTraVaCapNhatNganSach(ChiTieu chiTieu)
        {
            if (chiTieu.DanhMucId.HasValue && chiTieu.NguoiDungId.HasValue)
            {
                var ngayChi = chiTieu.NgayChi;
                var nganSach = await _context.NganSachs
                    .FirstOrDefaultAsync(n => n.NguoiDungId == chiTieu.NguoiDungId &&
                                            n.DanhMucId == chiTieu.DanhMucId &&
                                            n.Thang == ngayChi.Month &&
                                            n.Nam == ngayChi.Year);

                if (nganSach != null)
                {
                    // Tính tổng chi tiêu trong tháng cho danh mục này
                    var tongChiTieu = await _context.ChiTieus
                        .Where(c => c.NguoiDungId == chiTieu.NguoiDungId &&
                               c.DanhMucId == chiTieu.DanhMucId &&
                               c.NgayChi.Month == ngayChi.Month &&
                               c.NgayChi.Year == ngayChi.Year)
                        .SumAsync(c => c.SoTien);

                    // Kiểm tra nếu vượt ngân sách thì tạo nhắc nhở
                    if (tongChiTieu > nganSach.HanMuc && nganSach.HanMuc > 0)
                    {
                        var nhacNho = new NhacNho
                        {
                            NguoiDungId = chiTieu.NguoiDungId,
                            NoiDung = $"Bạn đã vượt ngân sách {nganSach.DanhMuc?.TenDanhMuc} trong tháng {ngayChi.Month}/{ngayChi.Year}. Đã chi: {tongChiTieu:N0} VND, hạn mức: {nganSach.HanMuc:N0} VND.",
                            ThoiGian = DateTime.Now,
                            Loai = "VuotNganSach"
                        };

                        _context.NhacNhos.Add(nhacNho);
                        await _context.SaveChangesAsync();
                    }
                }
            }
        }
    }
}
