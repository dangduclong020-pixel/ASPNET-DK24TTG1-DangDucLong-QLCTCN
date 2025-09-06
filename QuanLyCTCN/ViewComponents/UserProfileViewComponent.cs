using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyCTCN.Data;
using QuanLyCTCN.Models;

namespace QuanLyCTCN.ViewComponents
{
    public class UserProfileViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private const string _sessionNguoiDungId = "NguoiDungId";

        public UserProfileViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var nguoiDungId = HttpContext.Session.GetInt32(_sessionNguoiDungId);

            if (nguoiDungId == null)
            {
                return View(new NguoiDung());
            }

            var nguoiDung = await _context.NguoiDungs
                .FirstOrDefaultAsync(n => n.NguoiDungId == nguoiDungId);

            if (nguoiDung == null)
            {
                return View(new NguoiDung());
            }

            return View(nguoiDung);
        }
    }
}
