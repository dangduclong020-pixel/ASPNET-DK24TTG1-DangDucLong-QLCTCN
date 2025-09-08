using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyCTCN.Data;
using QuanLyCTCN.Models;
using QuanLyCTCN.Models.ViewModels;

namespace QuanLyCTCN.Controllers
{
    public class NganSachController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private const string _sessionNguoiDungId = "NguoiDungId";

        public NganSachController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /NganSach
        public async Task<IActionResult> Index(int? thang, int? nam)
        {
            // Kiểm tra đăng nhập
            var redirectResult = RedirectToLoginIfNotAuthenticated();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            var nguoiDungId = HttpContext.Session.GetInt32(_sessionNguoiDungId);

            // Mặc định là tháng và năm hiện tại
            var today = DateTime.Today;
            thang ??= today.Month;
            nam ??= today.Year;

            // Lấy danh sách ngân sách
            var nganSachs = await _context.NganSachs
                .Include(n => n.DanhMuc)
                .Where(n => n.NguoiDungId == nguoiDungId && n.Thang == thang && n.Nam == nam)
                .ToListAsync();

            // Lấy thông tin chi tiêu thực tế theo ngân sách
            var firstDayOfMonth = new DateTime(nam.Value, thang.Value, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            
            var nganSachViewModels = new List<NganSachVuotHanMucViewModel>();

            foreach (var nganSach in nganSachs)
            {
                var tongChiTieu = await _context.ChiTieus
                    .Where(c => c.NguoiDungId == nguoiDungId &&
                           c.DanhMucId == nganSach.DanhMucId &&
                           c.NgayChi >= firstDayOfMonth && c.NgayChi <= lastDayOfMonth)
                    .SumAsync(c => c.SoTien);

                var phanTramDaSuDung = nganSach.HanMuc > 0 ?
                    Math.Round((tongChiTieu / nganSach.HanMuc) * 100, 2) : 0;

                nganSachViewModels.Add(new NganSachVuotHanMucViewModel
                {
                    NganSach = nganSach,
                    TongChiTieu = tongChiTieu,
                    PhanTramDaSuDung = phanTramDaSuDung
                });
            }

            ViewBag.Thang = thang;
            ViewBag.Nam = nam;

            return View(nganSachViewModels);
        }

        // GET: /NganSach/Create
        public async Task<IActionResult> Create()
        {
            // Kiểm tra đăng nhập
            var redirectResult = RedirectToLoginIfNotAuthenticated();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            var nguoiDungId = HttpContext.Session.GetInt32(_sessionNguoiDungId);

            // Lấy danh sách danh mục chi tiêu
            ViewBag.DanhMucList = new SelectList(
                await _context.DanhMucs
                    .Where(d => d.Loai == "ChiTieu")
                    .ToListAsync(),
                "DanhMucId", "TenDanhMuc");

            // Mặc định là tháng và năm hiện tại
            var today = DateTime.Today;
            var model = new NganSach
            {
                NguoiDungId = nguoiDungId,
                Thang = today.Month,
                Nam = today.Year
            };

            return View(model);
        }

        // POST: /NganSach/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NganSach nganSach)
        {
            // Kiểm tra đăng nhập
            var redirectResult = RedirectToLoginIfNotAuthenticated();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            var nguoiDungId = HttpContext.Session.GetInt32(_sessionNguoiDungId);

            // Đảm bảo NguoiDungId được thiết lập
            nganSach.NguoiDungId = nguoiDungId;

            if (ModelState.IsValid)
            {
                // Kiểm tra ngân sách đã tồn tại cho danh mục này trong tháng/năm không
                var exists = await _context.NganSachs
                    .AnyAsync(n => n.NguoiDungId == nguoiDungId &&
                                n.DanhMucId == nganSach.DanhMucId &&
                                n.Thang == nganSach.Thang &&
                                n.Nam == nganSach.Nam);

                if (exists)
                {
                    ModelState.AddModelError("", "Ngân sách cho danh mục này đã tồn tại trong tháng/năm đã chọn");
                    ViewBag.DanhMucList = new SelectList(
                        await _context.DanhMucs
                            .Where(d => d.Loai == "ChiTieu")
                            .ToListAsync(),
                        "DanhMucId", "TenDanhMuc", nganSach.DanhMucId);
                    return View(nganSach);
                }

                _context.Add(nganSach);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm ngân sách thành công!";
                return RedirectToAction(nameof(Index), new { thang = nganSach.Thang, nam = nganSach.Nam });
            }

            // Nếu không thành công, chuẩn bị dữ liệu cho view
            ViewBag.DanhMucList = new SelectList(
                await _context.DanhMucs
                    .Where(d => d.Loai == "ChiTieu")
                    .ToListAsync(),
                "DanhMucId", "TenDanhMuc", nganSach.DanhMucId);

            return View(nganSach);
        }

        // GET: /NganSach/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            // Kiểm tra đăng nhập
            var redirectResult = RedirectToLoginIfNotAuthenticated();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            var nguoiDungId = HttpContext.Session.GetInt32(_sessionNguoiDungId);

            if (id == null)
            {
                return NotFound();
            }

            var nganSach = await _context.NganSachs
                .FirstOrDefaultAsync(n => n.NganSachId == id && n.NguoiDungId == nguoiDungId);

            if (nganSach == null)
            {
                return NotFound();
            }

            // Lấy danh sách danh mục chi tiêu
            ViewBag.DanhMucList = new SelectList(
                await _context.DanhMucs
                    .Where(d => d.Loai == "ChiTieu")
                    .ToListAsync(),
                "DanhMucId", "TenDanhMuc", nganSach.DanhMucId);

            return View(nganSach);
        }

        // POST: /NganSach/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, NganSach nganSach)
        {
            // Kiểm tra đăng nhập
            var redirectResult = RedirectToLoginIfNotAuthenticated();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            var nguoiDungId = HttpContext.Session.GetInt32(_sessionNguoiDungId);

            if (id != nganSach.NganSachId)
            {
                return NotFound();
            }

            // Kiểm tra quyền sở hữu ngân sách
            var existingNganSach = await _context.NganSachs
                .FirstOrDefaultAsync(n => n.NganSachId == id && n.NguoiDungId == nguoiDungId);

            if (existingNganSach == null)
            {
                return NotFound();
            }

            // Đảm bảo NguoiDungId được thiết lập
            nganSach.NguoiDungId = nguoiDungId;

            if (ModelState.IsValid)
            {
                // Kiểm tra ngân sách đã tồn tại cho danh mục này trong tháng/năm không (trừ ngân sách hiện tại)
                var exists = await _context.NganSachs
                    .AnyAsync(n => n.NguoiDungId == nguoiDungId &&
                                n.DanhMucId == nganSach.DanhMucId &&
                                n.Thang == nganSach.Thang &&
                                n.Nam == nganSach.Nam &&
                                n.NganSachId != id);

                if (exists)
                {
                    ModelState.AddModelError("", "Ngân sách cho danh mục này đã tồn tại trong tháng/năm đã chọn");
                    ViewBag.DanhMucList = new SelectList(
                        await _context.DanhMucs
                            .Where(d => d.Loai == "ChiTieu")
                            .ToListAsync(),
                        "DanhMucId", "TenDanhMuc", nganSach.DanhMucId);
                    return View(nganSach);
                }

                try
                {
                    // Cập nhật các thuộc tính của entity hiện có
                    existingNganSach.HanMuc = nganSach.HanMuc;
                    existingNganSach.Thang = nganSach.Thang;
                    existingNganSach.Nam = nganSach.Nam;
                    existingNganSach.DanhMucId = nganSach.DanhMucId;
                    // NguoiDungId đã được kiểm tra

                    _context.Update(existingNganSach);
                    await _context.SaveChangesAsync();

                    // Kiểm tra nếu đã vượt ngân sách mới thì tạo nhắc nhở
                    await KiemTraVuotNganSach(existingNganSach);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NganSachExists(existingNganSach.NganSachId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["SuccessMessage"] = "Cập nhật ngân sách thành công!";
                return RedirectToAction(nameof(Index), new { thang = nganSach.Thang, nam = nganSach.Nam });
            }

            // Nếu không thành công, chuẩn bị dữ liệu cho view
            ViewBag.DanhMucList = new SelectList(
                await _context.DanhMucs
                    .Where(d => d.Loai == "ChiTieu")
                    .ToListAsync(),
                "DanhMucId", "TenDanhMuc", nganSach.DanhMucId);

            return View(nganSach);
        }

        // GET: /NganSach/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            // Kiểm tra đăng nhập
            var redirectResult = RedirectToLoginIfNotAuthenticated();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            var nguoiDungId = HttpContext.Session.GetInt32(_sessionNguoiDungId);

            if (id == null)
            {
                return NotFound();
            }

            var nganSach = await _context.NganSachs
                .Include(n => n.DanhMuc)
                .FirstOrDefaultAsync(m => m.NganSachId == id && m.NguoiDungId == nguoiDungId);

            if (nganSach == null)
            {
                return NotFound();
            }

            return View(nganSach);
        }

        // POST: /NganSach/Delete/5
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

            var nganSach = await _context.NganSachs
                .FirstOrDefaultAsync(n => n.NganSachId == id && n.NguoiDungId == nguoiDungId);

            if (nganSach == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy ngân sách cần xóa hoặc bạn không có quyền xóa ngân sách này.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var thang = nganSach.Thang;
                var nam = nganSach.Nam;

                _context.NganSachs.Remove(nganSach);
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    TempData["SuccessMessage"] = "Xóa ngân sách thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể xóa ngân sách. Vui lòng thử lại.";
                }

                return RedirectToAction(nameof(Index), new { thang, nam });
            }
            catch (DbUpdateConcurrencyException)
            {
                // Entity đã bị thay đổi hoặc xóa bởi transaction khác
                TempData["ErrorMessage"] = "Ngân sách đã được thay đổi hoặc xóa bởi người dùng khác. Vui lòng làm mới trang và thử lại.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Log lỗi để debug
                Console.WriteLine($"Lỗi khi xóa ngân sách ID {id}: {ex.Message}");

                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa ngân sách. Vui lòng thử lại.";
                return RedirectToAction(nameof(Index));
            }
        }

        private bool NganSachExists(int id)
        {
            return _context.NganSachs.Any(e => e.NganSachId == id);
        }

        // Kiểm tra nếu đã vượt ngân sách
        private async Task KiemTraVuotNganSach(NganSach nganSach)
        {
            if (nganSach.DanhMucId.HasValue && nganSach.NguoiDungId.HasValue)
            {
                var firstDayOfMonth = new DateTime(nganSach.Nam, nganSach.Thang, 1);
                var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                // Tính tổng chi tiêu trong tháng cho danh mục này
                var tongChiTieu = await _context.ChiTieus
                    .Where(c => c.NguoiDungId == nganSach.NguoiDungId &&
                           c.DanhMucId == nganSach.DanhMucId &&
                           c.NgayChi >= firstDayOfMonth && c.NgayChi <= lastDayOfMonth)
                    .SumAsync(c => c.SoTien);

                // Kiểm tra nếu vượt ngân sách thì tạo nhắc nhở
                if (tongChiTieu > nganSach.HanMuc && nganSach.HanMuc > 0)
                {
                    var danhMuc = await _context.DanhMucs.FindAsync(nganSach.DanhMucId);
                    var tenDanhMuc = danhMuc?.TenDanhMuc ?? "Danh mục";

                    var nhacNho = new NhacNho
                    {
                        NguoiDungId = nganSach.NguoiDungId,
                        NoiDung = $"Bạn đã vượt ngân sách {tenDanhMuc} trong tháng {nganSach.Thang}/{nganSach.Nam}. Đã chi: {tongChiTieu:N0} VND, hạn mức: {nganSach.HanMuc:N0} VND.",
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
