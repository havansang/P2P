using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using P2P.Config;
using P2P.Models;
using P2P.Repository;

namespace P2P.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public AccountRepository _accRepo = new AccountRepository();

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            Account acc = _accRepo.GetByID(HttpContext.Session.GetInt32("AccountId") ?? 0) ?? new Account();
            ViewBag.Account = acc;
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public IActionResult Login()
        {
            HttpContext.Session.Remove("AccountId");
            HttpContext.Session.Remove("AccountRole");
            HttpContext.Session.Remove("FullName");
            HttpContext.Session.Clear();
            return View();
        }
        
        public IActionResult Login(string Email, string PassWord)
        {
            string passwordHash = MaHoaMD5.EncryptPassword(PassWord);
            Account acc = _accRepo.GetAll().FirstOrDefault(x => x.Email.ToLower() == Email.ToLower() && x.Password == passwordHash && x.IsDelete == false) ?? new Account();

            if (acc.Id <=0 )
            {
                return Json(new { success = false, message = "Tài khoản hoặc mật khẩu không chính xác!" });
            }

            if (acc.IsDelete == true)
            {
                return Json(new { success = false, message = "Tài khoản đã bị khóa! Không thể đăng nhập!" });
            }

            HttpContext.Session.SetInt32("AccountId", acc.Id);
            HttpContext.Session.SetInt32("AccountRole", acc.Role);
            HttpContext.Session.SetString("FullName", acc.FullName ?? "");

            string redirectUrl = acc.Role > 1 ? Url.Action("Index", "Admin") : Url.Action("Index", "Home");
            return Json(new { success = true, redirectUrl });
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("AccountId");
            HttpContext.Session.Remove("AccountRole");
            HttpContext.Session.Remove("FullName");
            HttpContext.Session.Clear();
            return RedirectToAction("Index","Home");
        }
    }
}
