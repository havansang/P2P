using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using P2P.Config;
using P2P.Models;
using P2P.Repository;
using P2P.Service;

namespace P2P.Controllers
{
    public class AccountController : Controller
    {
        private readonly EmailService _emailService;

        public AccountController(EmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpGet]
        public async Task<JsonResult> SendVerificationCode(string email)
        {
            if (string.IsNullOrEmpty(email))
                return Json(new { success = false, message = "Email không hợp lệ" });

            // Tạo mã ngẫu nhiên 6 chữ số
            var code = new Random().Next(100000, 999999).ToString();

            // Lưu mã vào Session
            HttpContext.Session.SetString("VerificationCode", code);
            HttpContext.Session.SetString("VerificationEmail", email);
            HttpContext.Session.SetString("VerificationTime", DateTime.UtcNow.ToString());

            // Gửi email
            var subject = "Mã xác nhận đăng ký từ SecureEscrow";
            var body = $"Mã xác nhận của bạn là: {code}.\nVui lòng sử dụng trong vòng 10 phút.";

            try
            {
                await _emailService.SendEmailAsync(email, subject, body);
                return Json(new { success = true, message = "Mã xác nhận đã được gửi đến email!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi gửi email: " + ex.Message });
            }
        }



        public AccountRepository _accRepo = new AccountRepository();

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email,string verificationCode)
        {
            if (
                string.IsNullOrWhiteSpace(verificationCode))
            {
                return Json(new { success = false, message = "Vui lòng điền đầy đủ thông tin." });
            }

            // Kiểm tra mã xác nhận
            var code = HttpContext.Session.GetString("VerificationCode");
            var emailSession = HttpContext.Session.GetString("VerificationEmail");
            var timeStr = HttpContext.Session.GetString("VerificationTime");

            if (code == null || emailSession == null || timeStr == null)
                return Json(new { success = false, message = "Mã xác nhận không tồn tại. Vui lòng gửi lại mã." });

            if (verificationCode.Trim() != code)
                return Json(new { success = false, message = "Mã xác nhận không chính xác." });

            if (DateTime.TryParse(timeStr, out var time) && (DateTime.UtcNow - time).TotalMinutes > 10)
                return Json(new { success = false, message = "Mã xác nhận đã hết hạn." });

            HttpContext.Session.Remove("VerificationCode");
            //HttpContext.Session.Remove("VerificationEmail");
            HttpContext.Session.Remove("VerificationTime");

            return Json(new { success = true});
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmChangePassword(string password, string confirmPassword)
        {
            if (password==null || confirmPassword==null)
            {
                return Json(new { success = false, message = "Vui lòng điền đầy đủ thông tin!." });
            }

            if (password != confirmPassword)
                return Json(new { success = false, message = "Mật khẩu xác nhận không khớp." });

            // Tạo mật khẩu mới
            string? email = HttpContext.Session.GetString("VerificationEmail");
            string sql = $"SELECT * FROM [Account] WHERE Email = '{email}'";
            var acc = SQLHelper<Account>.SqlToList(sql).FirstOrDefault();
            if (acc != null)
            {
                acc.Password = MaHoaMD5.EncryptPassword(password);
            }
            else
            {
                return Json(new { success = false, message="Có lỗi xảy ra, vui lòng thử lại!" });
            }

            _accRepo.Update(acc);

            HttpContext.Session.SetInt32("AccountId", acc.Id);
            HttpContext.Session.SetInt32("AccountRole", acc.Role);
            HttpContext.Session.SetString("FullName", acc.FullName ?? "");
            HttpContext.Session.SetString("AvatarUrl", acc.AvatarUrl ?? "");

            HttpContext.Session.Remove("VerificationEmail");

            return Json(new { success = true, redirectUrl = Url.Action("Index", "Home") });
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string email, string password, string confirmPassword, string fullname, string verificationCode)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(confirmPassword) || string.IsNullOrWhiteSpace(fullname) ||
                string.IsNullOrWhiteSpace(verificationCode))
            {
                return Json(new { success = false, message = "Vui lòng điền đầy đủ thông tin." });
            }

            if (password != confirmPassword)
                return Json(new { success = false, message = "Mật khẩu xác nhận không khớp." });

            if (_accRepo.GetAll().Any(x => x.Email == email))
                return Json(new { success = false, message = "Email đã được sử dụng." });

            // Kiểm tra mã xác nhận
            var code = HttpContext.Session.GetString("VerificationCode");
            var emailSession = HttpContext.Session.GetString("VerificationEmail");
            var timeStr = HttpContext.Session.GetString("VerificationTime");

            if (code == null || emailSession == null || timeStr == null)
                return Json(new { success = false, message = "Mã xác nhận không tồn tại. Vui lòng gửi lại mã." });

            if (email != emailSession)
                return Json(new { success = false, message = "Email không khớp với email đã yêu cầu mã." });

            if (verificationCode.Trim() != code)
                return Json(new { success = false, message = "Mã xác nhận không chính xác." });

            if (DateTime.TryParse(timeStr, out var time) && (DateTime.UtcNow - time).TotalMinutes > 10)
                return Json(new { success = false, message = "Mã xác nhận đã hết hạn." });

            // Tạo tài khoản
            var acc = new Account
            {
                Email = email,
                Password = MaHoaMD5.EncryptPassword(password),
                FullName = fullname,
                PhoneNumber = "",
                Role = 1,
                AvatarUrl = "",
                IsDelete = false,
                CreatedDate = DateTime.Now
            };

            await _accRepo.CreateAsync(acc);
            HttpContext.Session.SetInt32("AccountId", acc.Id);
            HttpContext.Session.SetInt32("AccountRole", acc.Role);
            HttpContext.Session.SetString("FullName", acc.FullName ?? "");
            HttpContext.Session.SetString("AvatarUrl", acc.AvatarUrl ?? "");

            HttpContext.Session.Remove("VerificationCode");
            HttpContext.Session.Remove("VerificationEmail");
            HttpContext.Session.Remove("VerificationTime");

            return Json(new { success = true, redirectUrl = Url.Action("Index", "Home") });
        }

    }
}
