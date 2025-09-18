using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyCTCN.Data;
using QuanLyCTCN.Models;

namespace QuanLyCTCN.Controllers
{
    public class DanhMucController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private const string _sessionNguoiDungId = "NguoiDungId";

        public DanhMucController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /DanhMuc
        public async Task<IActionResult> Index(string loai, string nhom)
        {
            // Kiểm tra đăng nhập
            var redirectResult = RedirectToLoginIfNotAuthenticated();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            var nguoiDungId = HttpContext.Session.GetInt32(_sessionNguoiDungId);

            // Lọc danh mục chi tiêu theo người dùng hiện tại
            var danhMucChiTieuQuery = _context.DanhMucs
                .Where(d => d.Loai == "ChiTieu" && d.NguoiDungId == nguoiDungId);
            if (!string.IsNullOrEmpty(nhom))
            {
                danhMucChiTieuQuery = danhMucChiTieuQuery.Where(d => d.Nhom == nhom);
            }
            var danhMucChiTieu = await danhMucChiTieuQuery.ToListAsync();

            // Lọc danh mục thu nhập theo người dùng hiện tại
            var danhMucThuNhapQuery = _context.DanhMucs
                .Where(d => d.Loai == "ThuNhap" && d.NguoiDungId == nguoiDungId);
            var danhMucThuNhap = await danhMucThuNhapQuery.ToListAsync();

            // Áp dụng lọc loại nếu có
            if (!string.IsNullOrEmpty(loai))
            {
                if (loai == "ChiTieu")
                {
                    danhMucThuNhap = new List<DanhMuc>();
                }
                else if (loai == "ThuNhap")
                {
                    danhMucChiTieu = new List<DanhMuc>();
                }
            }

            ViewBag.DanhMucChiTieu = danhMucChiTieu;
            ViewBag.DanhMucThuNhap = danhMucThuNhap;
            ViewBag.LoaiFilter = loai;
            ViewBag.NhomFilter = nhom;

            return View();
        }

        // GET: /DanhMuc/Create
        public IActionResult Create()
        {
            // Kiểm tra đăng nhập
            var redirectResult = RedirectToLoginIfNotAuthenticated();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            return View();
        }

        // POST: /DanhMuc/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DanhMuc danhMuc)
        {
            // Kiểm tra đăng nhập
            var redirectResult = RedirectToLoginIfNotAuthenticated();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            var nguoiDungId = HttpContext.Session.GetInt32(_sessionNguoiDungId);

            if (ModelState.IsValid)
            {
                // Kiểm tra tên danh mục đã tồn tại chưa (trong danh mục của người dùng hiện tại)
                var danhMucExists = await _context.DanhMucs
                    .AnyAsync(d => d.TenDanhMuc == danhMuc.TenDanhMuc && 
                                   d.Loai == danhMuc.Loai && 
                                   d.NguoiDungId == nguoiDungId);

                if (danhMucExists)
                {
                    ModelState.AddModelError("TenDanhMuc", "Danh mục này đã tồn tại");
                    return View(danhMuc);
                }

                // Gán người dùng hiện tại cho danh mục
                danhMuc.NguoiDungId = nguoiDungId;

                _context.Add(danhMuc);
                await _context.SaveChangesAsync();
                TempData["DanhMucSuccessMessage"] = "Thêm danh mục thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(danhMuc);
        }

        // GET: /DanhMuc/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            // Kiểm tra đăng nhập
            var redirectResult = RedirectToLoginIfNotAuthenticated();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            if (id == null)
            {
                return NotFound();
            }

            var nguoiDungId = HttpContext.Session.GetInt32(_sessionNguoiDungId);

            var danhMuc = await _context.DanhMucs
                .FirstOrDefaultAsync(d => d.DanhMucId == id && d.NguoiDungId == nguoiDungId);

            if (danhMuc == null)
            {
                return NotFound();
            }
            return View(danhMuc);
        }

        // POST: /DanhMuc/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DanhMuc danhMuc)
        {
            // Kiểm tra đăng nhập
            var redirectResult = RedirectToLoginIfNotAuthenticated();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            if (id != danhMuc.DanhMucId)
            {
                return NotFound();
            }

            var nguoiDungId = HttpContext.Session.GetInt32(_sessionNguoiDungId);

            // Kiểm tra quyền sở hữu danh mục
            var existingDanhMuc = await _context.DanhMucs
                .FirstOrDefaultAsync(d => d.DanhMucId == id && d.NguoiDungId == nguoiDungId);

            if (existingDanhMuc == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Kiểm tra tên danh mục đã tồn tại chưa (trừ danh mục hiện tại)
                var danhMucExists = await _context.DanhMucs
                    .AnyAsync(d => d.TenDanhMuc == danhMuc.TenDanhMuc && 
                                   d.Loai == danhMuc.Loai && 
                                   d.NguoiDungId == nguoiDungId && 
                                   d.DanhMucId != id);

                if (danhMucExists)
                {
                    ModelState.AddModelError("TenDanhMuc", "Danh mục này đã tồn tại");
                    return View(danhMuc);
                }

                try
                {
                    // Cập nhật các thuộc tính của entity hiện có thay vì update entity mới
                    existingDanhMuc.TenDanhMuc = danhMuc.TenDanhMuc;
                    existingDanhMuc.Loai = danhMuc.Loai;
                    existingDanhMuc.Nhom = danhMuc.Nhom;
                    existingDanhMuc.GhiChu = danhMuc.GhiChu;
                    // NguoiDungId đã được kiểm tra và không thay đổi

                    _context.Update(existingDanhMuc);
                    await _context.SaveChangesAsync();
                    TempData["DanhMucSuccessMessage"] = "Cập nhật danh mục thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DanhMucExists(existingDanhMuc.DanhMucId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(danhMuc);
        }

        // GET: /DanhMuc/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            // Kiểm tra đăng nhập
            var redirectResult = RedirectToLoginIfNotAuthenticated();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            if (id == null)
            {
                return NotFound();
            }

            var nguoiDungId = HttpContext.Session.GetInt32(_sessionNguoiDungId);

            var danhMuc = await _context.DanhMucs
                .FirstOrDefaultAsync(m => m.DanhMucId == id && m.NguoiDungId == nguoiDungId);

            if (danhMuc == null)
            {
                return NotFound();
            }

            return View(danhMuc);
        }

        // GET: /DanhMuc/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            // Kiểm tra đăng nhập
            var redirectResult = RedirectToLoginIfNotAuthenticated();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            if (id == null)
            {
                return NotFound();
            }

            var nguoiDungId = HttpContext.Session.GetInt32(_sessionNguoiDungId);

            var danhMuc = await _context.DanhMucs
                .FirstOrDefaultAsync(m => m.DanhMucId == id && m.NguoiDungId == nguoiDungId);

            if (danhMuc == null)
            {
                return NotFound();
            }

            // Kiểm tra danh mục có đang được sử dụng không
            var chiTieuCount = await _context.ChiTieus.CountAsync(c => c.DanhMucId == id);
            var thuNhapCount = await _context.ThuNhaps.CountAsync(t => t.DanhMucId == id);
            var nganSachCount = await _context.NganSachs.CountAsync(n => n.DanhMucId == id);

            if (chiTieuCount > 0 || thuNhapCount > 0 || nganSachCount > 0)
            {
                TempData["ErrorMessage"] = "Không thể xóa danh mục này vì đang được sử dụng";
                return RedirectToAction(nameof(Index));
            }

            return View(danhMuc);
        }

        // POST: /DanhMuc/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Kiểm tra đăng nhập
            var redirectResult = RedirectToLoginIfNotAuthenticated();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            var nguoiDungId = HttpContext.Session.GetInt32(_sessionNguoiDungId);

            var danhMuc = await _context.DanhMucs
                .FirstOrDefaultAsync(d => d.DanhMucId == id && d.NguoiDungId == nguoiDungId);

            if (danhMuc == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy danh mục cần xóa hoặc bạn không có quyền xóa danh mục này.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Kiểm tra danh mục có đang được sử dụng không
                var chiTieuCount = await _context.ChiTieus.CountAsync(c => c.DanhMucId == id);
                var thuNhapCount = await _context.ThuNhaps.CountAsync(t => t.DanhMucId == id);
                var nganSachCount = await _context.NganSachs.CountAsync(n => n.DanhMucId == id);

                if (chiTieuCount > 0 || thuNhapCount > 0 || nganSachCount > 0)
                {
                    TempData["ErrorMessage"] = "Không thể xóa danh mục này vì đang được sử dụng";
                    return RedirectToAction(nameof(Index));
                }

                _context.DanhMucs.Remove(danhMuc);
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    TempData["DanhMucSuccessMessage"] = "Xóa danh mục thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể xóa danh mục. Vui lòng thử lại.";
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                TempData["ErrorMessage"] = "Danh mục đã được thay đổi hoặc xóa bởi người dùng khác. Vui lòng làm mới trang và thử lại.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi xóa danh mục ID {id}: {ex.Message}");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa danh mục. Vui lòng thử lại.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool DanhMucExists(int id)
        {
            return _context.DanhMucs.Any(e => e.DanhMucId == id);
        }
    }
}
