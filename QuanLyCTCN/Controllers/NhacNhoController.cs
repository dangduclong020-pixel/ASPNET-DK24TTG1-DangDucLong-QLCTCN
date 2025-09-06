using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyCTCN.Data;
using QuanLyCTCN.Models;

namespace QuanLyCTCN.Controllers
{
    public class NhacNhoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string _sessionNguoiDungId = "NguoiDungId";

        public NhacNhoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /NhacNho
        public async Task<IActionResult> Index()
        {
            // Kiểm tra đăng nhập
            var nguoiDungId = HttpContext.Session.GetInt32(_sessionNguoiDungId);
            if (nguoiDungId == null)
            {
                return RedirectToAction("DangNhap", "NguoiDung");
            }

            // Lấy danh sách nhắc nhở của người dùng
            var nhacNhos = await _context.NhacNhos
                .Where(n => n.NguoiDungId == nguoiDungId)
                .OrderByDescending(n => n.ThoiGian) // Sắp xếp theo thời gian giảm dần
                .ToListAsync();

            return View(nhacNhos);
        }

        // GET: /NhacNho/Details/5
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

            var nhacNho = await _context.NhacNhos
                .FirstOrDefaultAsync(m => m.NhacNhoId == id && m.NguoiDungId == nguoiDungId);

            if (nhacNho == null)
            {
                return NotFound();
            }

            return View(nhacNho);
        }

        // GET: /NhacNho/Create
        public IActionResult Create()
        {
            // Kiểm tra đăng nhập
            var nguoiDungId = HttpContext.Session.GetInt32(_sessionNguoiDungId);
            if (nguoiDungId == null)
            {
                return RedirectToAction("DangNhap", "NguoiDung");
            }

            // Đặt giá trị mặc định
            var model = new NhacNho
            {
                NguoiDungId = nguoiDungId,
                ThoiGian = DateTime.Now.AddMinutes(30), // Mặc định 30 phút từ bây giờ
                Loai = "ChiTieu" // Mặc định loại nhắc nhở
            };

            return View(model);
        }

        // POST: /NhacNho/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NhacNho nhacNho)
        {
            // Kiểm tra đăng nhập
            var nguoiDungId = HttpContext.Session.GetInt32(_sessionNguoiDungId);
            if (nguoiDungId == null)
            {
                return RedirectToAction("DangNhap", "NguoiDung");
            }

            // Đảm bảo NguoiDungId được thiết lập
            nhacNho.NguoiDungId = nguoiDungId;

            if (ModelState.IsValid)
            {
                _context.Add(nhacNho);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm nhắc nhở thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(nhacNho);
        }

        // GET: /NhacNho/Edit/5
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

            var nhacNho = await _context.NhacNhos
                .FirstOrDefaultAsync(n => n.NhacNhoId == id && n.NguoiDungId == nguoiDungId);

            if (nhacNho == null)
            {
                return NotFound();
            }

            return View(nhacNho);
        }

        // POST: /NhacNho/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, NhacNho nhacNho)
        {
            // Kiểm tra đăng nhập
            var nguoiDungId = HttpContext.Session.GetInt32(_sessionNguoiDungId);
            if (nguoiDungId == null)
            {
                return RedirectToAction("DangNhap", "NguoiDung");
            }

            if (id != nhacNho.NhacNhoId)
            {
                return NotFound();
            }

            // Đảm bảo NguoiDungId được thiết lập
            nhacNho.NguoiDungId = nguoiDungId;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(nhacNho);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NhacNhoExists(nhacNho.NhacNhoId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["SuccessMessage"] = "Cập nhật nhắc nhở thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(nhacNho);
        }

        // GET: /NhacNho/Delete/5
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

            var nhacNho = await _context.NhacNhos
                .FirstOrDefaultAsync(m => m.NhacNhoId == id && m.NguoiDungId == nguoiDungId);

            if (nhacNho == null)
            {
                return NotFound();
            }

            return View(nhacNho);
        }

        // POST: /NhacNho/Delete/5
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

            var nhacNho = await _context.NhacNhos
                .FirstOrDefaultAsync(n => n.NhacNhoId == id && n.NguoiDungId == nguoiDungId);

            if (nhacNho != null)
            {
                _context.NhacNhos.Remove(nhacNho);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa nhắc nhở thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /NhacNho/DanhDauDaDoc/5
        public async Task<IActionResult> DanhDauDaDoc(int? id)
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

            var nhacNho = await _context.NhacNhos
                .FirstOrDefaultAsync(n => n.NhacNhoId == id && n.NguoiDungId == nguoiDungId);

            if (nhacNho == null)
            {
                return NotFound();
            }

            // Đánh dấu đã đọc bằng cách xóa nhắc nhở
            _context.NhacNhos.Remove(nhacNho);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã đánh dấu nhắc nhở là đã đọc!";

            return RedirectToAction(nameof(Index));
        }

        // GET: /NhacNho/TaoNhacNhoDinhKy
        public IActionResult TaoNhacNhoDinhKy()
        {
            // Kiểm tra đăng nhập
            var nguoiDungId = HttpContext.Session.GetInt32(_sessionNguoiDungId);
            if (nguoiDungId == null)
            {
                return RedirectToAction("DangNhap", "NguoiDung");
            }

            // Đặt giá trị mặc định
            var model = new NhacNho
            {
                NguoiDungId = nguoiDungId,
                ThoiGian = DateTime.Now.AddDays(1).Date.AddHours(8), // Mặc định 8 giờ sáng mai
                Loai = "ChiTieu" // Mặc định loại nhắc nhở
            };

            return View(model);
        }

        // POST: /NhacNho/TaoNhacNhoDinhKy
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TaoNhacNhoDinhKy(NhacNho nhacNho, string loaiLap, int soNgayLap)
        {
            // Kiểm tra đăng nhập
            var nguoiDungId = HttpContext.Session.GetInt32(_sessionNguoiDungId);
            if (nguoiDungId == null)
            {
                return RedirectToAction("DangNhap", "NguoiDung");
            }

            // Đảm bảo NguoiDungId được thiết lập
            nhacNho.NguoiDungId = nguoiDungId;

            if (ModelState.IsValid)
            {
                // Thêm nhắc nhở đầu tiên
                _context.Add(nhacNho);
                await _context.SaveChangesAsync();

                // Tạo nhắc nhở định kỳ
                if (loaiLap != "none" && soNgayLap > 0)
                {
                    // Tạo các nhắc nhở lặp lại
                    var soLanLap = 0;
                    switch (loaiLap)
                    {
                        case "daily":
                            soLanLap = 30; // Lặp lại hàng ngày trong 1 tháng
                            break;
                        case "weekly":
                            soLanLap = 12; // Lặp lại hàng tuần trong 3 tháng
                            break;
                        case "monthly":
                            soLanLap = 12; // Lặp lại hàng tháng trong 1 năm
                            break;
                    }

                    for (int i = 1; i <= soLanLap; i++)
                    {
                        var thoiGianLap = loaiLap switch
                        {
                            "daily" => nhacNho.ThoiGian.AddDays(i * soNgayLap),
                            "weekly" => nhacNho.ThoiGian.AddDays(i * 7 * soNgayLap),
                            "monthly" => nhacNho.ThoiGian.AddMonths(i * soNgayLap),
                            _ => DateTime.MinValue
                        };

                        if (thoiGianLap != DateTime.MinValue)
                        {
                            var nhacNhoLap = new NhacNho
                            {
                                NguoiDungId = nguoiDungId,
                                NoiDung = nhacNho.NoiDung,
                                ThoiGian = thoiGianLap,
                                Loai = nhacNho.Loai
                            };

                            _context.Add(nhacNhoLap);
                        }
                    }

                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = "Tạo nhắc nhở định kỳ thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(nhacNho);
        }

        private bool NhacNhoExists(int id)
        {
            return _context.NhacNhos.Any(e => e.NhacNhoId == id);
        }
    }
}
