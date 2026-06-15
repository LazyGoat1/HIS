using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HIS.Web.Controllers
{
    /// <summary>
    /// 首页
    /// </summary>
    [Authorize]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // 从 Session/Claims 获取用户信息
            ViewBag.UserName = User.FindFirst("RealName")?.Value ?? User.Identity?.Name;
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
