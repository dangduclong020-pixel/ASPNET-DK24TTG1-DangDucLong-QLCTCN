using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyCTCN.Data;
using QuanLyCTCN.Models;

namespace QuanLyCTCN.Controllers
{
    public class MucTieuController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string _sessionNguoiDungId = "NguoiDungId";

        public MucTieuController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /MucTieu
        public async Task<IActionResult> Index()
        {
            // Kiểm tra đăng nhập
            var nguoiDungId = HttpContext.Session.GetInt32(_sessionNguoiDungId);
            if (nguoiDungId == null)
            {
                return RedirectToAction("DangNhap", "NguoiDung");
            }

            // Lấy danh sách mục tiêu của người dùng
            var mucTieus = await _context.MucTieus
                .Where(m => m.NguoiDungId == nguoiDungId)
                .OrderBy(m => m.Han) // Sắp xếp theo thời hạn
                .ToListAsync();

            return View(mucTieus);
        }

        // GET: /MucTieu/Details/5
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

            var mucTieu = await _context.MucTieus
                .FirstOrDefaultAsync(m => m.MucTieuId == id && m.NguoiDungId == nguoiDungId);

            if (mucTieu == null)
            {
                return NotFound();
            }

            return View(mucTieu);
        }

        // GET: /MucTieu/Create
        public IActionResult Create()
        {
            // Kiểm tra đăng nhập
            var nguoiDungId = HttpContext.Session.GetInt32(_sessionNguoiDungId);
            if (nguoiDungId == null)
            {
                return RedirectToAction("DangNhap", "NguoiDung");
            }

            // Đặt giá trị mặc định
            var model = new MucTieu
            {
                NguoiDungId = nguoiDungId,
                SoTienDaTietKiem = 0,
                Han = DateTime.Today.AddMonths(6) // Mặc định 6 tháng
            };

            return View(model);
        }

        // POST: /MucTieu/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MucTieu mucTieu)
        {
            // Kiểm tra đăng nhập
            var nguoiDungId = HttpContext.Session.GetInt32(_sessionNguoiDungId);
            if (nguoiDungId == null)
            {
                return RedirectToAction("DangNhap", "NguoiDung");
            }

            // Đảm bảo NguoiDungId được thiết lập
            mucTieu.NguoiDungId = nguoiDungId;

            if (ModelState.IsValid)
            {
                _context.Add(mucTieu);
                await _context.SaveChangesAsync();

                // Tạo nhắc nhở cho mục tiêu mới
                var nhacNho = new NhacNho
                {
                    NguoiDungId = nguoiDungId,
                    NoiDung = $"Mục tiêu mới: {mucTieu.TenMucTieu} - Cần {mucTieu.SoTienCan:N0} VND" + 
                              (mucTieu.Han.HasValue ? $" - Thời hạn: {mucTieu.Han.Value:dd/MM/yyyy}" : ""),
                    ThoiGian = DateTime.Now,
                    Loai = "MucTieu"
                };
                _context.NhacNhos.Add(nhacNho);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Thêm mục tiêu thành công!";
                return RedirectToAction(nameof(Index));
            }

            return View(mucTieu);
        }

        // GET: /MucTieu/Edit/5
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

            var mucTieu = await _context.MucTieus
                .FirstOrDefaultAsync(m => m.MucTieuId == id && m.NguoiDungId == nguoiDungId);

            if (mucTieu == null)
            {
                return NotFound();
            }

            return View(mucTieu);
        }

        // POST: /MucTieu/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MucTieu mucTieu)
        {
            // Kiểm tra đăng nhập
            var nguoiDungId = HttpContext.Session.GetInt32(_sessionNguoiDungId);
            if (nguoiDungId == null)
            {
                return RedirectToAction("DangNhap", "NguoiDung");
            }

            if (id != mucTieu.MucTieuId)
            {
                return NotFound();
            }

            // Đảm bảo NguoiDungId được thiết lập
            mucTieu.NguoiDungId = nguoiDungId;

            if (ModelState.IsValid)
            {
                try
                {
                    // Lấy mục tiêu cũ để kiểm tra
                    var oldMucTieu = await _context.MucTieus.AsNoTracking()
                        .FirstOrDefaultAsync(m => m.MucTieuId == id);

                    _context.Update(mucTieu);
                    await _context.SaveChangesAsync();

                    // Kiểm tra nếu đã đạt mục tiêu
                    if (mucTieu.SoTienDaTietKiem >= mucTieu.SoTienCan && 
                        (oldMucTieu == null || oldMucTieu.SoTienDaTietKiem < oldMucTieu.SoTienCan))
                    {
                        var nhacNho = new NhacNho
                        {
                            NguoiDungId = nguoiDungId,
                            NoiDung = $"Chúc mừng! Bạn đã hoàn thành mục tiêu \"{mucTieu.TenMucTieu}\" với số tiền {mucTieu.SoTienCan:N0} VND.",
                            ThoiGian = DateTime.Now,
                            Loai = "MucTieu"
                        };
                        _context.NhacNhos.Add(nhacNho);
                        await _context.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MucTieuExists(mucTieu.MucTieuId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["SuccessMessage"] = "Cập nhật mục tiêu thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(mucTieu);
        }

        // GET: /MucTieu/Delete/5
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

            var mucTieu = await _context.MucTieus
                .FirstOrDefaultAsync(m => m.MucTieuId == id && m.NguoiDungId == nguoiDungId);

            if (mucTieu == null)
            {
                return NotFound();
            }

            return View(mucTieu);
        }

        // POST: /MucTieu/Delete/5
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

            var mucTieu = await _context.MucTieus
                .FirstOrDefaultAsync(m => m.MucTieuId == id && m.NguoiDungId == nguoiDungId);

            if (mucTieu != null)
            {
                _context.MucTieus.Remove(mucTieu);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa mục tiêu thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /MucTieu/CapNhatTienTietKiem/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhatTienTietKiem(int id, decimal soTienThem)
        {
            // Kiểm tra đăng nhập
            var nguoiDungId = HttpContext.Session.GetInt32(_sessionNguoiDungId);
            if (nguoiDungId == null)
            {
                return RedirectToAction("DangNhap", "NguoiDung");
            }

            var mucTieu = await _context.MucTieus
                .FirstOrDefaultAsync(m => m.MucTieuId == id && m.NguoiDungId == nguoiDungId);

            if (mucTieu == null)
            {
                return NotFound();
            }

            var oldTienTietKiem = mucTieu.SoTienDaTietKiem ?? 0;
            mucTieu.SoTienDaTietKiem = oldTienTietKiem + soTienThem;

            _context.Update(mucTieu);
            await _context.SaveChangesAsync();

            // Kiểm tra nếu đã đạt mục tiêu
            if (mucTieu.SoTienDaTietKiem >= mucTieu.SoTienCan && oldTienTietKiem < mucTieu.SoTienCan)
            {
                var nhacNho = new NhacNho
                {
                    NguoiDungId = nguoiDungId,
                    NoiDung = $"Chúc mừng! Bạn đã hoàn thành mục tiêu \"{mucTieu.TenMucTieu}\" với số tiền {mucTieu.SoTienCan:N0} VND.",
                    ThoiGian = DateTime.Now,
                    Loai = "MucTieu"
                };
                _context.NhacNhos.Add(nhacNho);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Chúc mừng! Bạn đã hoàn thành mục tiêu này!";
            }
            else
            {
                TempData["SuccessMessage"] = "Cập nhật tiến độ mục tiêu thành công!";
            }

            return RedirectToAction(nameof(Details), new { id = mucTieu.MucTieuId });
        }

        private bool MucTieuExists(int id)
        {
            return _context.MucTieus.Any(e => e.MucTieuId == id);
        }
    }
}
