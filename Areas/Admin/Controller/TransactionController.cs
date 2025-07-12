using P2P.Models;
using P2P.Config;
using Microsoft.AspNetCore.Mvc;
using P2P.Repository;
using System.Data;

namespace P2P.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class TransactionController : Controller
    {
        public AccountRepository _accRepo = new AccountRepository();
        public IActionResult Index()
        {
            Account acc = _accRepo.GetByID(HttpContext.Session.GetInt32("AccountId") ?? 0) ?? new Account();
            if (acc == null || acc.Role < 2)
            {
                return Redirect("/Home/Index");
            }
            var listt = SQLHelper<Fee>.SqlToList("SELECT * FROM [Fee]").FirstOrDefault();
            HttpContext.Session.SetInt32("FeePercent", listt.Percent ?? 0);
            ViewBag.Account = acc;
            return View();
        }
        [HttpGet]
        public IActionResult GetAll()
        {
            var list = SQLHelper<Transaction>.SqlToList("SELECT * FROM [Transaction]");
            
            return Json(list);
        }

        [HttpPost]
        public IActionResult ConfirmTransaction(int id)
        {
            var sql = $"UPDATE [Transaction] SET Status = 5 WHERE Id = {id} AND Status = 2";
            var rows = SQLHelper<Transaction>.ExecuteScalarInt($"SELECT COUNT(*) FROM [Transaction] WHERE Id = {id} AND Status = 2");

            if (rows == 0)
            {
                return Json(new { success = false, message = "Không tìm thấy giao dịch hợp lệ." });
            }

            try
            {
                SQLHelper<Transaction>.SqlToList(sql); // Hoặc dùng ExecuteNonQuery nếu bạn có sẵn.
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false, message = "Lỗi hệ thống khi xác nhận." });
            }
        }

    }
}
