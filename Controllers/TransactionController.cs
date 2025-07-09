using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using P2P.Config;
using P2P.Models;
using P2P.Repository;
using P2P.Service;
using System.Security.Cryptography;


namespace P2P.Controllers
{
    public class TransactionController : Controller
    {
        public IActionResult Details() { return View(); }
        public AccountRepository _accRepo = new AccountRepository();
        public TransactionRepository _tranRepo = new TransactionRepository();
        public FeeRepository _feeRepo = new FeeRepository();

        //Fee list = SQLHelper<Fee>.SqlToList("SELECT * FROM [Transaction]").FirstOrDefault();
        //public Fee feePercent = SQLHelper<Fee>.SqlToList("SELECT * FROM [Fee]").FirstOrDefault();

        private readonly EmailService _emailService;

        public TransactionController(EmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult TransactionHistory()
        {
            Account acc = _accRepo.GetByID(HttpContext.Session.GetInt32("AccountId") ?? 0) ?? new Account();
            if (acc.Id <= 0)
            {
                return Redirect("/Home/Login");
            }
            return View();
        }
        [HttpPost]
        public IActionResult TransactionHistory(DateTime? fromDate, DateTime? toDate)
        {
            int userId = HttpContext.Session.GetInt32("AccountId") ?? 0;

            string sql = $@"
                SELECT * FROM [Transaction]
                WHERE (SenderId = {userId} OR ReceiverId = {userId})
                {(fromDate.HasValue ? $"AND CreatedDate >= '{fromDate:yyyy-MM-dd} 00:00:00'" : "")}
                {(toDate.HasValue ? $"AND CreatedDate <= '{toDate:yyyy-MM-dd} 23:59:59'" : "")}
                ORDER BY CreatedDate DESC";

            var transactions = SQLHelper<Transaction>.SqlToList(sql);

            var result = transactions.Select(tran => new
            {
                tran.TransactionId,
                tran.Amount,
                tran.Description,
                CreatedDate = tran.CreatedDate?.ToString("dd/MM/yyyy HH:mm") ?? "",
                ExpireDate = tran.ExpireDate?.ToString("dd/MM/yyyy HH:mm") ?? "",
                Status = GetStatus(tran),
                IsSender = tran.SenderId == userId
            });

            return Json(new { success = true, data = result });
        }

        private string GetStatus(Transaction tran)
        {
            if (tran.Status == 4) return "❌ Đã hủy";
            if (tran.ExpireDate.HasValue && tran.ExpireDate.Value < DateTime.Now) return "⏰ Hết hạn";
            return tran.Status switch
            {
                1 => "⏳ Đang đợi xác nhận",
                2 => "✅ Đã hoàn thành",
                _ => "Không rõ"
            };
        }




        //===============Nhận tiền=================
        [HttpPost]
        public async Task<IActionResult> ConfirmBanking(string bankCode, string accountNumber, string transactionId)
        {
            if (string.IsNullOrEmpty(bankCode) || string.IsNullOrEmpty(accountNumber) || string.IsNullOrEmpty(transactionId))
            {
                return Json(new { success = false, message = "Thiếu thông tin bắt buộc!"+bankCode+"/"+accountNumber+"/"+transactionId+"/" });
            }

            try
            {
                string sql = $"SELECT * FROM [Transaction] WHERE TransactionID = '{transactionId}'";
                var transaction = SQLHelper<Transaction>.SqlToList(sql).FirstOrDefault();
                int? feePersent = HttpContext.Session.GetInt32("FeePercent");
                if (transaction == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy giao dịch với mã vừa cung cấp!" });
                }

                transaction.Status = 2;
                transaction.ReceiverId = HttpContext.Session.GetInt32("AccountId") ?? 0;
                transaction.ConfirmedDate = DateTime.Now;
                transaction.BankCode = bankCode;
                transaction.AccountNumber = accountNumber;
                transaction.FeeReceive = feePersent;
                _tranRepo.Update(transaction);

                // Gửi email
                var subject = "Thông báo! Có một giao dịch mới được hoàn thành";
                var body = $"Mã giao dịch được hoàn thành là: {transactionId}.\nVui lòng chuyển khoản cho họ.";

                await _emailService.SendEmailAsync("havansang090203@gmail.com", subject, body);

                return Json(new
                {
                    success = true,
                    message = "Hoàn thành giao dịch. Tiền sẽ được chuyển đến tài khoản của bạn."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Lỗi xử lý giao dịch 002: " + ex.Message
                });
            }
        }


        [HttpGet]
        public IActionResult Claim()
        {
            Account acc = _accRepo.GetByID(HttpContext.Session.GetInt32("AccountId") ?? 0) ?? new Account();
            if (acc.Id <= 0)
            {
                return Redirect("/Home/Login");
            }
            var list = SQLHelper<Fee>.SqlToList("SELECT * FROM [Fee]").FirstOrDefault();
            HttpContext.Session.SetInt32("FeePercent", list.Percent);
            return View();
        }

        [HttpPost]
        public IActionResult Claim(string secretKey)
        {
            try
            {
                string sql = $"SELECT * FROM [Transaction] WHERE SecretKey = '{secretKey}'";
                var transaction = SQLHelper<Transaction>.SqlToList(sql).FirstOrDefault();

                if (transaction == null)
                {
                    return Json(new { success = false, message = "❌ Mã giao dịch không tồn tại." });
                }

                if (transaction.Status != 1) // 1 = Chờ nhận
                {
                    return Json(new { success = false, message = "❌ Giao dịch đã được nhận hoặc đã hết hạn." });
                }

                if (transaction.ExpireDate < DateTime.Now)
                {
                    //transaction.Status = 3;
                    //_tranRepo.Update(transaction);
                    return Json(new { success = false, message = "❌ Giao dịch đã hết hạn.",transaction });
                }
                var sender = _accRepo.GetByID(transaction.SenderId);


                return Json(new
                {
                    success = true,
                    message = "Nhận giao dịch thành công!",
                    transaction = new
                    {
                        amount = transaction.Amount,
                        description = transaction.Description,
                        transactionId = transaction.TransactionId
                    },
                    senderName = sender.FullName
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "⚠️ Lỗi: " + ex.Message });
            }
        }




        [HttpGet]
        public IActionResult Create()
        {
            Account acc = _accRepo.GetByID(HttpContext.Session.GetInt32("AccountId") ?? 0) ?? new Account();
            if (acc.Id <= 0)
            {
                return Redirect("/Home/Login");
            }
            var list = SQLHelper<Fee>.SqlToList("SELECT * FROM [Fee]").FirstOrDefault();
            HttpContext.Session.SetInt32("FeePercent", list.Percent);
            return View();
        }

        public IActionResult Index()
        {
            return View( );
        }
        public async Task<IActionResult> CreateSecretkey(decimal amount, int accountId, string paymentMethod, string description, DateTime expired,int feePercent)
        {
            try
            {
                var model = new Transaction
                {
                    Amount = amount,
                    SenderId = accountId,
                    PaymentMethod = paymentMethod,
                    Description = description,
                    ExpireDate = expired,
                    Status = 1,
                    CreatedDate = DateTime.Now,
                    TransactionId = LoadCode(),
                    Secretkey = GenerateUniqueSecretKey(),
                    FeeSend = feePercent
                };
                await _tranRepo.CreateAsync(model);

                return View("SecretKey", model.Secretkey);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return View("Create");
            }
            
        }
        public string LoadCode()
        {
            int currentYear = DateTime.Now.Year;
            List<Transaction> lst = SQLHelper<Transaction>.SqlToList($"SELECT * FROM [Transaction] WHERE YEAR(CreatedDate) = {currentYear}");
            string numberCode = (lst.Count + 1).ToString();
            while (numberCode.Length < 7)
            {
                numberCode = "0" + numberCode;
            }
            string code = $"SE{currentYear}{numberCode}";
            return code;

        }
        private string GenerateUniqueSecretKey()
        {
            string key;
            int count;

            do
            {
                key = GenerateSecretKey(13);
                count = SQLHelper<Transaction>.ExecuteScalarInt(
                    $"SELECT COUNT(*) FROM [Transaction] WHERE SecretKey = '{key}'"
                );
            } while (count > 0);

            return key;
        }


        private string GenerateSecretKey(int length)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var result = new char[length];

            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] data = new byte[length];
                rng.GetBytes(data);

                for (int i = 0; i < length; i++)
                {
                    int index = data[i] % chars.Length;
                    result[i] = chars[index];
                }
            }

            return new string(result);
        }


    }
}