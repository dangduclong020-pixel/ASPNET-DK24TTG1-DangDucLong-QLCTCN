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

            // Lọc danh mục chi tiêu
            var danhMucChiTieuQuery = _context.DanhMucs.Where(d => d.Loai == "ChiTieu");
            if (!string.IsNullOrEmpty(nhom))
            {
                danhMucChiTieuQuery = danhMucChiTieuQuery.Where(d => d.Nhom == nhom);
            }
            var danhMucChiTieu = await danhMucChiTieuQuery.ToListAsync();

            // Lọc danh mục thu nhập
            var danhMucThuNhapQuery = _context.DanhMucs.Where(d => d.Loai == "ThuNhap");
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

            if (ModelState.IsValid)
            {
                // Kiểm tra tên danh mục đã tồn tại chưa
                var danhMucExists = await _context.DanhMucs
                    .AnyAsync(d => d.TenDanhMuc == danhMuc.TenDanhMuc && d.Loai == danhMuc.Loai);

                if (danhMucExists)
                {
                    ModelState.AddModelError("TenDanhMuc", "Danh mục này đã tồn tại");
                    return View(danhMuc);
                }

                _context.Add(danhMuc);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm danh mục thành công!";
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

            var danhMuc = await _context.DanhMucs.FindAsync(id);
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

            if (ModelState.IsValid)
            {
                // Kiểm tra tên danh mục đã tồn tại chưa (trừ danh mục hiện tại)
                var danhMucExists = await _context.DanhMucs
                    .AnyAsync(d => d.TenDanhMuc == danhMuc.TenDanhMuc && d.Loai == danhMuc.Loai && d.DanhMucId != id);

                if (danhMucExists)
                {
                    ModelState.AddModelError("TenDanhMuc", "Danh mục này đã tồn tại");
                    return View(danhMuc);
                }

                try
                {
                    _context.Update(danhMuc);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DanhMucExists(danhMuc.DanhMucId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["SuccessMessage"] = "Cập nhật danh mục thành công!";
                return RedirectToAction(nameof(Index));
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

            var danhMuc = await _context.DanhMucs
                .FirstOrDefaultAsync(m => m.DanhMucId == id);
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

            var danhMuc = await _context.DanhMucs.FindAsync(id);
            if (danhMuc != null)
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
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa danh mục thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool DanhMucExists(int id)
        {
            return _context.DanhMucs.Any(e => e.DanhMucId == id);
        }
    }
}
