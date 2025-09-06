using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace QuanLyCTCN.Controllers
{
    public abstract class BaseController : Controller
    {
        private const string _sessionNguoiDungId = "NguoiDungId";

        protected int? GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32(_sessionNguoiDungId);
        }

        protected bool IsAuthenticated()
        {
            return GetCurrentUserId().HasValue;
        }

        protected IActionResult? RedirectToLoginIfNotAuthenticated()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("DangNhap", "NguoiDung", new { returnUrl = HttpContext.Request.Path });
            }
            return null;
        }
    }
}
