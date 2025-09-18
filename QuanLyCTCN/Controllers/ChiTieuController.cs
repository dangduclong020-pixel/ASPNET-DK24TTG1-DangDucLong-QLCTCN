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
        public async Task<IActionResult> Index(DateTime? tuNgay, DateTime? denNgay, int? thang, int? nam)
        {
            // Kiểm tra đăng nhập
            var redirectResult = RedirectToLoginIfNotAuthenticated();
            if (redirectResult != null)
            {
                return redirectResult;
            }
            
            var nguoiDungId = GetCurrentUserId();

            // Ưu tiên sử dụng tháng và năm nếu có (khi người dùng chỉ chọn tháng/năm mà không chọn ngày)
            if (thang.HasValue && nam.HasValue && !tuNgay.HasValue && !denNgay.HasValue)
            {
                tuNgay = new DateTime(nam.Value, thang.Value, 1);
                denNgay = new DateTime(nam.Value, thang.Value, DateTime.DaysInMonth(nam.Value, thang.Value));
            }
            // Nếu có tuNgay, denNgay thì giữ nguyên
            else if (tuNgay.HasValue && denNgay.HasValue)
            {
                thang = tuNgay.Value.Month;
                nam = tuNgay.Value.Year;
            }
            // Nếu chỉ có tuNgay, set denNgay là cuối tháng
            else if (tuNgay.HasValue && !denNgay.HasValue)
            {
                denNgay = new DateTime(tuNgay.Value.Year, tuNgay.Value.Month,
                    DateTime.DaysInMonth(tuNgay.Value.Year, tuNgay.Value.Month));
                thang = tuNgay.Value.Month;
                nam = tuNgay.Value.Year;
            }
            // Nếu chỉ có denNgay, set tuNgay là đầu tháng
            else if (!tuNgay.HasValue && denNgay.HasValue)
            {
                tuNgay = new DateTime(denNgay.Value.Year, denNgay.Value.Month, 1);
                thang = denNgay.Value.Month;
                nam = denNgay.Value.Year;
            }
            // Nếu có tham số tháng và năm, sử dụng chúng
            else if (thang.HasValue && nam.HasValue)
            {
                tuNgay = new DateTime(nam.Value, thang.Value, 1);
                denNgay = new DateTime(nam.Value, thang.Value, DateTime.DaysInMonth(nam.Value, thang.Value));
            }
            // Mặc định lấy dữ liệu của tháng hiện tại
            else
            {
                var today = DateTime.Today;
                tuNgay = new DateTime(today.Year, today.Month, 1);
                denNgay = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
                thang = today.Month;
                nam = today.Year;
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
            ViewBag.Thang = thang ?? DateTime.Today.Month;
            ViewBag.Nam = nam ?? DateTime.Today.Year;

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

            var currentMonth = DateTime.Today.Month;
            var currentYear = DateTime.Today.Year;

            // Lấy tất cả danh mục chi tiêu (sẽ được lọc động qua AJAX theo ngày)
            var danhMucChiTieu = await _context.DanhMucs
                .Where(d => d.Loai == "ChiTieu" && d.NguoiDungId == nguoiDungId)
                .ToListAsync();

            ViewBag.DanhMucList = new SelectList(danhMucChiTieu, "DanhMucId", "TenDanhMuc");

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
                var canhBaoNganSach = await KiemTraNganSachTruocKhiChi(chiTieu);

                _context.Add(chiTieu);
                await _context.SaveChangesAsync();
                await KiemTraVaCapNhatNganSach(chiTieu);

                if (!string.IsNullOrEmpty(canhBaoNganSach))
                {
                    TempData["WarningMessage"] = canhBaoNganSach;
                }
                else
                {
                    TempData["ChiTieuSuccessMessage"] = "Thêm chi tiêu thành công!";
                }

                return RedirectToAction(nameof(Index));
            }

            // Nếu không thành công, chuẩn bị dữ liệu cho view
            var currentMonth = DateTime.Today.Month;
            var currentYear = DateTime.Today.Year;

            var danhMucCoNganSach = await _context.NganSachs
                .Where(n => n.NguoiDungId == nguoiDungId &&
                           n.Thang == currentMonth &&
                           n.Nam == currentYear)
                .Include(n => n.DanhMuc)
                .Select(n => n.DanhMuc)
                .Where(d => d != null && d.Loai == "ChiTieu")
                .Distinct()
                .ToListAsync();

            ViewBag.DanhMucList = new SelectList(danhMucCoNganSach, "DanhMucId", "TenDanhMuc", chiTieu.DanhMucId);

            return View(chiTieu);
        }

        // GET: /ChiTieu/GetNganSachInfo
        [HttpGet]
        public async Task<IActionResult> GetNganSachInfo(int danhMucId, string ngayChi = null)
        {
            Console.WriteLine($"GetNganSachInfo called with danhMucId: {danhMucId}, ngayChi: {ngayChi}");

            // Debug session
            var sessionUserId = HttpContext.Session.GetInt32("NguoiDungId");
            Console.WriteLine($"Session NguoiDungId: {sessionUserId}");
            Console.WriteLine($"Session keys: {string.Join(", ", HttpContext.Session.Keys)}");

            var nguoiDungId = GetCurrentUserId();
            Console.WriteLine($"Current user ID from GetCurrentUserId(): {nguoiDungId}");

            if (nguoiDungId == null)
            {
                Console.WriteLine("User not authenticated - returning error");
                return Json(new { error = "Chưa đăng nhập" });
            }

            // Xác định tháng và năm từ ngày chi, nếu không có thì dùng tháng hiện tại
            int checkMonth, checkYear;
            if (!string.IsNullOrEmpty(ngayChi) && DateTime.TryParse(ngayChi, out DateTime parsedDate))
            {
                checkMonth = parsedDate.Month;
                checkYear = parsedDate.Year;
                Console.WriteLine($"Using parsed date - Month: {checkMonth}, Year: {checkYear}");
            }
            else
            {
                checkMonth = DateTime.Today.Month;
                checkYear = DateTime.Today.Year;
                Console.WriteLine($"Using current date - Month: {checkMonth}, Year: {checkYear}");
            }

            var nganSach = await _context.NganSachs
                .Include(n => n.DanhMuc)
                .FirstOrDefaultAsync(n => n.NguoiDungId == nguoiDungId &&
                                        n.DanhMucId == danhMucId &&
                                        n.Thang == checkMonth &&
                                        n.Nam == checkYear);

            Console.WriteLine($"Budget found: {nganSach != null}");
            if (nganSach != null)
            {
                Console.WriteLine($"Budget details - HanMuc: {nganSach.HanMuc}, DanhMuc: {nganSach.DanhMuc?.TenDanhMuc}");
            }

            if (nganSach == null)
            {
                Console.WriteLine("No budget found for this category and date");
                return Json(new { error = "Không tìm thấy ngân sách cho danh mục này trong tháng đã chọn" });
            }

            // Tính tổng chi tiêu hiện tại trong tháng đã chọn
            var tongChiTieuHienTai = await _context.ChiTieus
                .Where(c => c.NguoiDungId == nguoiDungId &&
                       c.DanhMucId == danhMucId &&
                       c.NgayChi.Month == checkMonth &&
                       c.NgayChi.Year == checkYear)
                .SumAsync(c => c.SoTien);

            Console.WriteLine($"Current spending: {tongChiTieuHienTai}");

            var result = new {
                tenDanhMuc = nganSach.DanhMuc?.TenDanhMuc,
                hanMuc = nganSach.HanMuc,
                daChi = tongChiTieuHienTai,
                conLai = nganSach.HanMuc - tongChiTieuHienTai
            };

            Console.WriteLine($"Returning budget info: {System.Text.Json.JsonSerializer.Serialize(result)}");
            return Json(result);
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

            var currentMonth = DateTime.Today.Month;
            var currentYear = DateTime.Today.Year;

            // Chỉ lấy danh mục chi tiêu có ngân sách trong tháng hiện tại
            var danhMucCoNganSach = await _context.NganSachs
                .Where(n => n.NguoiDungId == nguoiDungId &&
                           n.Thang == currentMonth &&
                           n.Nam == currentYear)
                .Include(n => n.DanhMuc)
                .Select(n => n.DanhMuc)
                .Where(d => d != null && d.Loai == "ChiTieu")
                .Distinct()
                .ToListAsync();

            ViewBag.DanhMucList = new SelectList(danhMucCoNganSach, "DanhMucId", "TenDanhMuc", chiTieu.DanhMucId);

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

            // Kiểm tra quyền sở hữu chi tiêu
            var existingChiTieu = await _context.ChiTieus
                .FirstOrDefaultAsync(c => c.ChiTieuId == id && c.NguoiDungId == nguoiDungId);

            if (existingChiTieu == null)
            {
                return NotFound();
            }

            // Đảm bảo NguoiDungId được thiết lập
            chiTieu.NguoiDungId = nguoiDungId;

            if (ModelState.IsValid)
            {
                try
                {
                    var canhBaoNganSach = await KiemTraNganSachTruocKhiCapNhat(existingChiTieu, chiTieu);

                    // Cập nhật các thuộc tính của entity hiện có
                    existingChiTieu.SoTien = chiTieu.SoTien;
                    existingChiTieu.NgayChi = chiTieu.NgayChi;
                    existingChiTieu.DanhMucId = chiTieu.DanhMucId;
                    existingChiTieu.GhiChu = chiTieu.GhiChu;
                    // NguoiDungId đã được kiểm tra

                    _context.Update(existingChiTieu);
                    await _context.SaveChangesAsync();

                    // Kiểm tra ngân sách và cập nhật nếu cần
                    await KiemTraVaCapNhatNganSach(existingChiTieu);

                    if (!string.IsNullOrEmpty(canhBaoNganSach))
                    {
                        TempData["WarningMessage"] = canhBaoNganSach;
                    }
                    else
                    {
                        TempData["ChiTieuSuccessMessage"] = "Cập nhật chi tiêu thành công!";
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChiTieuExists(existingChiTieu.ChiTieuId))
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

            // Nếu không thành công, chuẩn bị dữ liệu cho view
            var currentMonth = DateTime.Today.Month;
            var currentYear = DateTime.Today.Year;

            var danhMucCoNganSach = await _context.NganSachs
                .Where(n => n.NguoiDungId == nguoiDungId &&
                           n.Thang == currentMonth &&
                           n.Nam == currentYear)
                .Include(n => n.DanhMuc)
                .Select(n => n.DanhMuc)
                .Where(d => d != null && d.Loai == "ChiTieu")
                .Distinct()
                .ToListAsync();

            ViewBag.DanhMucList = new SelectList(danhMucCoNganSach, "DanhMucId", "TenDanhMuc", chiTieu.DanhMucId);

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

            if (chiTieu == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy chi tiêu cần xóa hoặc bạn không có quyền xóa chi tiêu này.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.ChiTieus.Remove(chiTieu);
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    TempData["ChiTieuSuccessMessage"] = "Xóa chi tiêu thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể xóa chi tiêu. Vui lòng thử lại.";
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                TempData["ErrorMessage"] = "Chi tiêu đã được thay đổi hoặc xóa bởi người dùng khác. Vui lòng làm mới trang và thử lại.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi xóa chi tiêu ID {id}: {ex.Message}");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa chi tiêu. Vui lòng thử lại.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ChiTieuExists(int id)
        {
            return _context.ChiTieus.Any(e => e.ChiTieuId == id);
        }

        // Kiểm tra ngân sách trước khi cập nhật chi tiêu
        private async Task<string> KiemTraNganSachTruocKhiCapNhat(ChiTieu existingChiTieu, ChiTieu newChiTieu)
        {
            if (newChiTieu.DanhMucId.HasValue && newChiTieu.NguoiDungId.HasValue)
            {
                var ngayChi = newChiTieu.NgayChi;
                var nganSach = await _context.NganSachs
                    .Include(n => n.DanhMuc)
                    .FirstOrDefaultAsync(n => n.NguoiDungId == newChiTieu.NguoiDungId &&
                                            n.DanhMucId == newChiTieu.DanhMucId &&
                                            n.Thang == ngayChi.Month &&
                                            n.Nam == ngayChi.Year);

                if (nganSach != null)
                {
                    // Tính tổng chi tiêu hiện tại (không bao gồm chi tiêu đang sửa)
                    var tongChiTieuHienTai = await _context.ChiTieus
                        .Where(c => c.NguoiDungId == newChiTieu.NguoiDungId &&
                               c.DanhMucId == newChiTieu.DanhMucId &&
                               c.ChiTieuId != existingChiTieu.ChiTieuId && // Loại trừ chi tiêu đang sửa
                               c.NgayChi.Month == ngayChi.Month &&
                               c.NgayChi.Year == ngayChi.Year)
                        .SumAsync(c => c.SoTien);

                    // Tính tổng sau khi cập nhật
                    var tongChiTieuSauCapNhat = tongChiTieuHienTai + newChiTieu.SoTien;

                    // Kiểm tra nếu sẽ vượt ngân sách
                    if (tongChiTieuSauCapNhat > nganSach.HanMuc && nganSach.HanMuc > 0)
                    {
                        var soTienVuot = tongChiTieuSauCapNhat - nganSach.HanMuc;
                        return $"⚠️ Cảnh báo: Chi tiêu cho danh mục '{nganSach.DanhMuc?.TenDanhMuc}' sau khi cập nhật sẽ vượt ngân sách {soTienVuot:N0} VND!<br>" +
                               $"• Ngân sách: {nganSach.HanMuc:N0} VND<br>" +
                               $"• Chi tiêu khác: {tongChiTieuHienTai:N0} VND<br>" +
                               $"• Sẽ cập nhật thành: {newChiTieu.SoTien:N0} VND<br>" +
                               $"• Tổng sau cập nhật: {tongChiTieuSauCapNhat:N0} VND";
                    }
                }
            }
            return null;
        }

        // Kiểm tra ngân sách trước khi chi và trả về cảnh báo
        private async Task<string> KiemTraNganSachTruocKhiChi(ChiTieu chiTieu)
        {
            if (chiTieu.DanhMucId.HasValue && chiTieu.NguoiDungId.HasValue)
            {
                var ngayChi = chiTieu.NgayChi;
                var nganSach = await _context.NganSachs
                    .Include(n => n.DanhMuc)
                    .FirstOrDefaultAsync(n => n.NguoiDungId == chiTieu.NguoiDungId &&
                                            n.DanhMucId == chiTieu.DanhMucId &&
                                            n.Thang == ngayChi.Month &&
                                            n.Nam == ngayChi.Year);

                if (nganSach != null)
                {
                    // Tính tổng chi tiêu hiện tại trong tháng cho danh mục này (không bao gồm chi tiêu mới)
                    var tongChiTieuHienTai = await _context.ChiTieus
                        .Where(c => c.NguoiDungId == chiTieu.NguoiDungId &&
                               c.DanhMucId == chiTieu.DanhMucId &&
                               c.NgayChi.Month == ngayChi.Month &&
                               c.NgayChi.Year == ngayChi.Year)
                        .SumAsync(c => c.SoTien);

                    // Tính tổng sau khi thêm chi tiêu mới
                    var tongChiTieuSauKhiThem = tongChiTieuHienTai + chiTieu.SoTien;

                    // Kiểm tra nếu sẽ vượt ngân sách
                    if (tongChiTieuSauKhiThem > nganSach.HanMuc && nganSach.HanMuc > 0)
                    {
                        var soTienVuot = tongChiTieuSauKhiThem - nganSach.HanMuc;
                        return $"⚠️ Cảnh báo: Chi tiêu cho danh mục '{nganSach.DanhMuc?.TenDanhMuc}' sẽ vượt ngân sách {soTienVuot:N0} VND!<br>" +
                               $"• Ngân sách: {nganSach.HanMuc:N0} VND<br>" +
                               $"• Đã chi: {tongChiTieuHienTai:N0} VND<br>" +
                               $"• Sẽ chi thêm: {chiTieu.SoTien:N0} VND<br>" +
                               $"• Tổng sau khi chi: {tongChiTieuSauKhiThem:N0} VND";
                    }
                }
            }
            return null;
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

                    // Kiểm tra nếu vượt ngân sách thì tạo nhắc nhở (chỉ tạo một lần trong ngày)
                    if (tongChiTieu > nganSach.HanMuc && nganSach.HanMuc > 0)
                    {
                        var today = DateTime.Today;
                        var tenDanhMuc = nganSach.DanhMuc?.TenDanhMuc ?? "";
                        var daCoNhacNhoHomNay = await _context.NhacNhos
                            .AnyAsync(n => n.NguoiDungId == chiTieu.NguoiDungId &&
                                         n.Loai == "VuotNganSach" &&
                                         n.ThoiGian.Date == today &&
                                         n.NoiDung.Contains(tenDanhMuc));

                        if (!daCoNhacNhoHomNay)
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

        // AJAX: Lấy danh sách danh mục có ngân sách theo tháng/năm
        [HttpGet]
        public async Task<IActionResult> GetDanhMucByThangNam(int thang, int nam)
        {
            var nguoiDungId = HttpContext.Session.GetInt32(_sessionNguoiDungId);
            if (nguoiDungId == null)
            {
                return Json(new { error = "Chưa đăng nhập" });
            }

            var danhMucCoNganSach = await _context.NganSachs
                .Where(n => n.NguoiDungId == nguoiDungId &&
                           n.Thang == thang &&
                           n.Nam == nam)
                .Include(n => n.DanhMuc)
                .Select(n => n.DanhMuc)
                .Where(d => d != null && d.Loai == "ChiTieu")
                .Distinct()
                .Select(d => new { d.DanhMucId, d.TenDanhMuc })
                .ToListAsync();

            return Json(danhMucCoNganSach);
        }
    }
}
