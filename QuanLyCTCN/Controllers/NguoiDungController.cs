using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyCTCN.Data;
using QuanLyCTCN.Models;
using QuanLyCTCN.Models.ViewModels;
using System.Security.Cryptography;
using System.Text;

namespace QuanLyCTCN.Controllers
{
    public class NguoiDungController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;
        private const string _sessionNguoiDungId = "NguoiDungId"; // Giữ lại để tương thích với code hiện có

        public NguoiDungController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // GET: /NguoiDung/DangNhap
        public IActionResult DangNhap(string? returnUrl = null)
        {
            if (HttpContext.Session.GetInt32(_sessionNguoiDungId) != null)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: /NguoiDung/DangNhap
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangNhap(DangNhapViewModel model, string? returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                var nguoiDung = await _context.NguoiDungs
                    .FirstOrDefaultAsync(u => u.TenDangNhap == model.TenDangNhap);

                if (nguoiDung == null)
                {
                    ModelState.AddModelError("", "Tên đăng nhập không tồn tại");
                    return View(model);
                }

                // Kiểm tra tài khoản bị khóa
                if (nguoiDung.KhoaDen.HasValue && nguoiDung.KhoaDen > DateTime.Now)
                {
                    ModelState.AddModelError("", $"Tài khoản của bạn đã bị khóa đến {nguoiDung.KhoaDen.Value.ToString("dd/MM/yyyy HH:mm")}");
                    return View(model);
                }

                if (nguoiDung.MatKhau != model.MatKhau) // Trong thực tế nên sử dụng mã hóa
                {
                    // Tăng số lần đăng nhập thất bại
                    nguoiDung.LanDangNhapThatBai = (nguoiDung.LanDangNhapThatBai ?? 0) + 1;

                    // Khóa tài khoản nếu đăng nhập thất bại quá 5 lần
                    if (nguoiDung.LanDangNhapThatBai >= 5)
                    {
                        nguoiDung.KhoaDen = DateTime.Now.AddMinutes(15); // Khóa 15 phút
                    }

                    await _context.SaveChangesAsync();

                    ModelState.AddModelError("", "Mật khẩu không đúng");
                    return View(model);
                }

                // Reset số lần đăng nhập thất bại khi đăng nhập thành công
                nguoiDung.LanDangNhapThatBai = 0;
                nguoiDung.KhoaDen = null;
                await _context.SaveChangesAsync();

                // Lưu ID người dùng vào Session và đảm bảo nó được lưu trữ
                HttpContext.Session.SetInt32(_sessionNguoiDungId, nguoiDung.NguoiDungId);
                
                // Log để kiểm tra xem giá trị Session đã được đặt hay chưa
                Console.WriteLine($"Session đã được đặt: NguoiDungId = {HttpContext.Session.GetInt32(_sessionNguoiDungId)}");

                // Chuyển hướng về trang được yêu cầu trước đó hoặc trang chủ
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }

        // GET: /NguoiDung/DangKy
        public IActionResult DangKy()
        {
            return View();
        }

        // POST: /NguoiDung/DangKy
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangKy(DangKyViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra tên đăng nhập đã tồn tại chưa
                var userExists = await _context.NguoiDungs
                    .AnyAsync(u => u.TenDangNhap == model.TenDangNhap);

                if (userExists)
                {
                    ModelState.AddModelError("TenDangNhap", "Tên đăng nhập đã tồn tại");
                    return View(model);
                }

                // Kiểm tra email đã tồn tại chưa
                var emailExists = await _context.NguoiDungs
                    .AnyAsync(u => u.Email == model.Email);

                if (emailExists)
                {
                    ModelState.AddModelError("Email", "Email đã tồn tại");
                    return View(model);
                }

                var nguoiDung = new NguoiDung
                {
                    TenDangNhap = model.TenDangNhap,
                    MatKhau = model.MatKhau, // Trong thực tế nên mã hóa mật khẩu
                    HoTen = model.HoTen,
                    Email = model.Email,
                    SoDienThoai = model.SoDienThoai,
                    LanDangNhapThatBai = 0
                };

                _context.Add(nguoiDung);
                await _context.SaveChangesAsync();

                // Tự động đăng nhập sau khi đăng ký
                HttpContext.Session.SetInt32(_sessionNguoiDungId, nguoiDung.NguoiDungId);

                TempData["SuccessMessage"] = "Đăng ký tài khoản thành công!";
                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }

        // GET: /NguoiDung/DoiMatKhau
        public IActionResult DoiMatKhau()
        {
            // Kiểm tra đăng nhập
            if (HttpContext.Session.GetInt32(_sessionNguoiDungId) == null)
            {
                return RedirectToAction("DangNhap");
            }

            return View();
        }

        // POST: /NguoiDung/DoiMatKhau
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoiMatKhau(DoiMatKhauViewModel model)
        {
            // Kiểm tra đăng nhập
            var nguoiDungId = HttpContext.Session.GetInt32(_sessionNguoiDungId);
            if (nguoiDungId == null)
            {
                return RedirectToAction("DangNhap");
            }

            if (ModelState.IsValid)
            {
                var nguoiDung = await _context.NguoiDungs.FindAsync(nguoiDungId);
                if (nguoiDung == null)
                {
                    return NotFound();
                }

                // Kiểm tra mật khẩu hiện tại
                if (nguoiDung.MatKhau != model.MatKhauHienTai) // Trong thực tế nên sử dụng mã hóa
                {
                    ModelState.AddModelError("MatKhauHienTai", "Mật khẩu hiện tại không đúng");
                    return View(model);
                }

                // Cập nhật mật khẩu mới
                nguoiDung.MatKhau = model.MatKhauMoi; // Trong thực tế nên mã hóa
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
                return RedirectToAction("HoSo");
            }

            return View(model);
        }

        // GET: /NguoiDung/HoSo
        public async Task<IActionResult> HoSo()
        {
            // Kiểm tra đăng nhập
            var nguoiDungId = HttpContext.Session.GetInt32(_sessionNguoiDungId);
            if (nguoiDungId == null)
            {
                return RedirectToAction("DangNhap");
            }

            var nguoiDung = await _context.NguoiDungs.FindAsync(nguoiDungId);
            if (nguoiDung == null)
            {
                return NotFound();
            }

            return View(nguoiDung);
        }

        // POST: /NguoiDung/CapNhatHoSo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhatHoSo(NguoiDung model, IFormFile? anhDaiDien)
        {
            // Kiểm tra đăng nhập
            var nguoiDungId = HttpContext.Session.GetInt32(_sessionNguoiDungId);
            if (nguoiDungId == null)
            {
                return RedirectToAction("DangNhap");
            }

            if (ModelState.IsValid)
            {
                var nguoiDung = await _context.NguoiDungs.FindAsync(nguoiDungId);
                if (nguoiDung == null)
                {
                    return NotFound();
                }

                // Cập nhật thông tin cá nhân
                nguoiDung.HoTen = model.HoTen;
                nguoiDung.Email = model.Email;
                nguoiDung.SoDienThoai = model.SoDienThoai;
                nguoiDung.DiaChi = model.DiaChi;

                // Xử lý upload ảnh đại diện nếu có
                if (anhDaiDien != null && anhDaiDien.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images", "avatars");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Xóa ảnh cũ nếu có
                    if (!string.IsNullOrEmpty(nguoiDung.AnhDaiDien))
                    {
                        var oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, nguoiDung.AnhDaiDien.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    // Lưu ảnh mới
                    var uniqueFileName = $"{Guid.NewGuid()}_{anhDaiDien.FileName}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await anhDaiDien.CopyToAsync(fileStream);
                    }

                    nguoiDung.AnhDaiDien = $"/images/avatars/{uniqueFileName}";
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Cập nhật thông tin cá nhân thành công!";
                return RedirectToAction("HoSo");
            }

            return View("HoSo", model);
        }

        // GET: /NguoiDung/CaiDat
        public async Task<IActionResult> CaiDat()
        {
            // Kiểm tra đăng nhập
            var nguoiDungId = HttpContext.Session.GetInt32(_sessionNguoiDungId);
            if (nguoiDungId == null)
            {
                return RedirectToAction("DangNhap");
            }

            var nguoiDung = await _context.NguoiDungs.FindAsync(nguoiDungId);
            if (nguoiDung == null)
            {
                return NotFound();
            }

            var model = new CaiDatViewModel
            {
                HoTen = nguoiDung.HoTen,
                Email = nguoiDung.Email,
                SoDienThoai = nguoiDung.SoDienThoai,
                DiaChi = nguoiDung.DiaChi
            };

            return View(model);
        }

        // POST: /NguoiDung/CaiDat
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CaiDat(CaiDatViewModel model)
        {
            var nguoiDungId = HttpContext.Session.GetInt32(_sessionNguoiDungId);
            if (nguoiDungId == null)
            {
                return RedirectToAction("DangNhap");
            }

            var nguoiDung = await _context.NguoiDungs.FindAsync(nguoiDungId);
            if (nguoiDung == null)
            {
                return NotFound();
            }

            // Cập nhật thông tin cá nhân
            nguoiDung.HoTen = model.HoTen;
            nguoiDung.Email = model.Email;
            nguoiDung.SoDienThoai = model.SoDienThoai;
            nguoiDung.DiaChi = model.DiaChi;

            // Đổi mật khẩu nếu có
            if (!string.IsNullOrEmpty(model.MatKhauMoi))
            {
                if (string.IsNullOrEmpty(model.MatKhauHienTai))
                {
                    ModelState.AddModelError("MatKhauHienTai", "Vui lòng nhập mật khẩu hiện tại");
                }
                else if (model.MatKhauHienTai != nguoiDung.MatKhau)
                {
                    ModelState.AddModelError("MatKhauHienTai", "Mật khẩu hiện tại không đúng");
                }
                else
                {
                    nguoiDung.MatKhau = model.MatKhauMoi;
                }
            }

            if (ModelState.IsValid)
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
                return RedirectToAction("CaiDat");
            }

            return View(model);
        }

        // GET: /NguoiDung/DangXuat
        public IActionResult DangXuat()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("DangNhap");
        }
    }
}
