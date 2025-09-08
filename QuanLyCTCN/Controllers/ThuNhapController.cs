using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyCTCN.Data;
using QuanLyCTCN.Models;

namespace QuanLyCTCN.Controllers
{
    public class ThuNhapController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private const string _sessionNguoiDungId = "NguoiDungId";

        public ThuNhapController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /ThuNhap
        public async Task<IActionResult> Index(DateTime? tuNgay, DateTime? denNgay, int? thang, int? nam)
        {
            // Kiểm tra đăng nhập
            var redirectResult = RedirectToLoginIfNotAuthenticated();
            if (redirectResult != null)
            {
                return redirectResult;
            }
            
            var nguoiDungId = GetCurrentUserId();

            // Nếu có tham số tháng và năm, sử dụng chúng
            if (thang.HasValue && nam.HasValue)
            {
                // Không cần tính tuNgay, denNgay nữa vì lọc theo thang_thu_nhap, nam_thu_nhap
            }
            // Mặc định lấy dữ liệu của tháng hiện tại
            else if (!tuNgay.HasValue)
            {
                var today = DateTime.Today;
                thang = today.Month;
                nam = today.Year;
            }

            var thuNhaps = await _context.ThuNhaps
                .Include(t => t.DanhMuc)
                .Where(t => t.NguoiDungId == nguoiDungId &&
                            t.ThangThuNhap == thang && t.NamThuNhap == nam)
                .OrderByDescending(t => t.NgayNhap)
                .ToListAsync();

            // Tính tổng thu nhập
            var tongThuNhap = thuNhaps.Sum(t => t.SoTien);
            ViewBag.TongThuNhap = tongThuNhap;
            ViewBag.Thang = thang ?? DateTime.Today.Month;
            ViewBag.Nam = nam ?? DateTime.Today.Year;

            return View(thuNhaps);
        }

        // GET: /ThuNhap/Details/5
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

            var nguoiDungId = GetCurrentUserId();

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
            var redirectResult = RedirectToLoginIfNotAuthenticated();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            // Lấy danh sách danh mục thu nhập của người dùng hiện tại
            ViewBag.DanhMucList = new SelectList(
                await _context.DanhMucs
                    .Where(d => d.Loai == "ThuNhap" && d.NguoiDungId == GetCurrentUserId())
                    .ToListAsync(),
                "DanhMucId", "TenDanhMuc");

            // Đặt ngày mặc định là ngày hiện tại
            var model = new ThuNhap
            {
                NgayNhap = DateTime.Today,
                ThangThuNhap = DateTime.Today.Month,
                NamThuNhap = DateTime.Today.Year,
                NguoiDungId = GetCurrentUserId()
            };

            return View(model);
        }

        // POST: /ThuNhap/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ThuNhap thuNhap)
        {
            // Kiểm tra đăng nhập
            var redirectResult = RedirectToLoginIfNotAuthenticated();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            // Đảm bảo NguoiDungId được thiết lập
            thuNhap.NguoiDungId = GetCurrentUserId();

            if (ModelState.IsValid)
            {
                _context.Add(thuNhap);
                await _context.SaveChangesAsync();

                // Kiểm tra và cập nhật mục tiêu tiết kiệm nếu cần
                await KiemTraVaCapNhatMucTieu(thuNhap.NguoiDungId!.Value);

                TempData["SuccessMessage"] = "Thêm thu nhập thành công!";
                return RedirectToAction(nameof(Index));
            }

            // Nếu không thành công, chuẩn bị dữ liệu cho view
            ViewBag.DanhMucList = new SelectList(
                await _context.DanhMucs
                    .Where(d => d.Loai == "ThuNhap" && d.NguoiDungId == GetCurrentUserId())
                    .ToListAsync(),
                "DanhMucId", "TenDanhMuc", thuNhap.DanhMucId);

            return View(thuNhap);
        }

        // GET: /ThuNhap/Edit/5
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

            var nguoiDungId = GetCurrentUserId();

            var thuNhap = await _context.ThuNhaps
                .FirstOrDefaultAsync(t => t.ThuNhapId == id && t.NguoiDungId == nguoiDungId);

            if (thuNhap == null)
            {
                return NotFound();
            }

            // Lấy danh sách danh mục thu nhập của người dùng hiện tại
            ViewBag.DanhMucList = new SelectList(
                await _context.DanhMucs
                    .Where(d => d.Loai == "ThuNhap" && d.NguoiDungId == nguoiDungId)
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
            var redirectResult = RedirectToLoginIfNotAuthenticated();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            if (id != thuNhap.ThuNhapId)
            {
                return NotFound();
            }

            // Kiểm tra quyền sở hữu thu nhập
            var existingThuNhap = await _context.ThuNhaps
                .FirstOrDefaultAsync(t => t.ThuNhapId == id && t.NguoiDungId == GetCurrentUserId());

            if (existingThuNhap == null)
            {
                return NotFound();
            }

            // Đảm bảo NguoiDungId được thiết lập
            thuNhap.NguoiDungId = GetCurrentUserId();

            if (ModelState.IsValid)
            {
                try
                {
                    // Cập nhật các thuộc tính của entity hiện có
                    existingThuNhap.SoTien = thuNhap.SoTien;
                    existingThuNhap.NgayNhap = thuNhap.NgayNhap;
                    existingThuNhap.DanhMucId = thuNhap.DanhMucId;
                    existingThuNhap.GhiChu = thuNhap.GhiChu;
                    // NguoiDungId đã được kiểm tra

                    _context.Update(existingThuNhap);
                    await _context.SaveChangesAsync();

                    // Kiểm tra và cập nhật mục tiêu tiết kiệm nếu cần
                    await KiemTraVaCapNhatMucTieu(thuNhap.NguoiDungId!.Value);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ThuNhapExists(existingThuNhap.ThuNhapId))
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
                    .Where(d => d.Loai == "ThuNhap" && d.NguoiDungId == GetCurrentUserId())
                    .ToListAsync(),
                "DanhMucId", "TenDanhMuc", thuNhap.DanhMucId);

            return View(thuNhap);
        }

        // GET: /ThuNhap/Delete/5
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

            var nguoiDungId = GetCurrentUserId();

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
            var redirectResult = RedirectToLoginIfNotAuthenticated();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            var nguoiDungId = GetCurrentUserId();

            var thuNhap = await _context.ThuNhaps
                .FirstOrDefaultAsync(t => t.ThuNhapId == id && t.NguoiDungId == nguoiDungId);

            if (thuNhap == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thu nhập cần xóa hoặc bạn không có quyền xóa thu nhập này.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.ThuNhaps.Remove(thuNhap);
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    // Cập nhật lại mục tiêu sau khi xóa thu nhập
                    await KiemTraVaCapNhatMucTieu(nguoiDungId!.Value);
                    TempData["SuccessMessage"] = "Xóa thu nhập thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể xóa thu nhập. Vui lòng thử lại.";
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                TempData["ErrorMessage"] = "Thu nhập đã được thay đổi hoặc xóa bởi người dùng khác. Vui lòng làm mới trang và thử lại.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi xóa thu nhập ID {id}: {ex.Message}");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa thu nhập. Vui lòng thử lại.";
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
