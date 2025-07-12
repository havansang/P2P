using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using P2P.Config;
using P2P.Models;
using P2P.Models.DTOs;
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
        public DisputeRepository _disputeRepo = new DisputeRepository();
        public DisputeFileRepository _disputeFileRepo = new DisputeFileRepository();

        //Fee list = SQLHelper<Fee>.SqlToList("SELECT * FROM [Transaction]").FirstOrDefault();
        //public Fee feePercent = SQLHelper<Fee>.SqlToList("SELECT * FROM [Fee]").FirstOrDefault();

        private readonly EmailService _emailService;

        public TransactionController(EmailService emailService)
        {
            _emailService = emailService;
        }

        //Hủy
        [HttpGet]
        public IActionResult Huy(string transactionId)
        {
            Account acc = _accRepo.GetByID(HttpContext.Session.GetInt32("AccountId") ?? 0) ?? new Account();
            if (acc.Id <= 0)
            {
                return Redirect("/Home/Login");
            }
            var list = SQLHelper<Fee>.SqlToList("SELECT * FROM [Fee]").FirstOrDefault();
            HttpContext.Session.SetInt32("FeePercent",0);

            Transaction transaction = _tranRepo.Find(x => x.TransactionId == transactionId).FirstOrDefault();
            var sender = _accRepo.GetByID(transaction.SenderId);
            var model = new ClaimViewModel
            {
                Amount = transaction.Amount,
                Description = transaction.Description,
                TransactionId = transaction.TransactionId,
                SenderName = sender.FullName
            };
            return View("Claim",model);
        }

        //B nhấn nhận tiền
        [HttpGet]
        public IActionResult Claim(string transactionId)
        {
            Account acc = _accRepo.GetByID(HttpContext.Session.GetInt32("AccountId") ?? 0) ?? new Account();
            if (acc.Id <= 0)
            {
                return Redirect("/Home/Login");
            }
            var list = SQLHelper<Fee>.SqlToList("SELECT * FROM [Fee]").FirstOrDefault();
            HttpContext.Session.SetInt32("FeePercent", list.Percent ?? 0);

            Transaction transaction = _tranRepo.Find(x => x.TransactionId == transactionId).FirstOrDefault();
            var sender = _accRepo.GetByID(transaction.SenderId);
            var model=new ClaimViewModel
            {
                Amount = transaction.Amount,
                Description = transaction.Description,
                TransactionId = transaction.TransactionId,
                SenderName = sender.FullName
            };
            return View(model);
        }


        //===============Nhận tiền=================
        [HttpPost]
        public async Task<IActionResult> ConfirmBanking(string bankCode, string accountNumber, string transactionId)
        {
            if (string.IsNullOrEmpty(bankCode) || string.IsNullOrEmpty(accountNumber) || string.IsNullOrEmpty(transactionId))
            {
                return Json(new { success = false, message = "Thiếu thông tin bắt buộc!" + bankCode + "/" + accountNumber + "/" + transactionId + "/" });
            }

            try
            {
                Transaction transaction = _tranRepo.Find(x => x.TransactionId == transactionId).FirstOrDefault();
                int? feePersent = HttpContext.Session.GetInt32("FeePercent");
                if (transaction == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy giao dịch với mã vừa cung cấp!" });
                }
                int st = transaction.Status;
                if (st == 3)
                {
                    transaction.Status = 5;
                    var subjectClaim = "Thông báo! Bạn vừa hoàn thành 1 giao dịch";
                    var bodyClaim = $"Mã giao dịch được hoàn thành là: {transactionId}.\nTiền sẽ được chuyển khoản vào tài khoản của bạn.";
                    Account acc = _accRepo.Find(x => x.Id == transaction.ReceiverId).FirstOrDefault();
                    await _emailService.SendEmailAsync(acc.Email, subjectClaim, bodyClaim);
                }
                if(st == 1)
                {
                    transaction.Status = 4;
                    var subjectClaim = "Thông báo! Bạn vừa hủy 1 giao dịch";
                    var bodyClaim = $"Mã giao dịch được hoàn thành là: {transactionId}.\nTiền sẽ được chuyển khoản vào tài khoản của bạn.";
                    Account acc = _accRepo.Find(x => x.Id == transaction.SenderId).FirstOrDefault();
                    await _emailService.SendEmailAsync(acc.Email, subjectClaim, bodyClaim);
                }
                //transaction.ReceiverId = HttpContext.Session.GetInt32("AccountId") ?? 0;
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

        //B nhấn Đã hoàn thành dịch vụ
        [HttpPost]
        public JsonResult PerformTransaction(string transactionId)
        {
            try
            {
                Transaction transaction = _tranRepo.Find(x => x.TransactionId == transactionId).FirstOrDefault();
                if (transaction == null)
                    return Json(new { success = false, message = "Không tìm thấy giao dịch." });

                if (transaction.Status != 1) // Chỉ cho phép hủy nếu đang ở trạng thái 1 (đã nhận ký quỹ)
                    return Json(new { success = false, message = "Không thể hủy giao dịch ở trạng thái hiện tại." });

                transaction.Status = 2; // Đặt lại trạng thái: Đã hủy
                transaction.UpdateDate = DateTime.Now;

                _tranRepo.Update(transaction);

                return Json(new { success = true, message = "Đã xác nhận hoàn thành dịch vụ." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // A nhấn Hoàn thành giao dịch
        [HttpPost]
        public JsonResult CompleteTransaction(string transactionId)
        {
            Transaction transaction = _tranRepo.Find(x => x.TransactionId == transactionId).FirstOrDefault();
            if (transaction == null || transaction.Status != 2)
                return Json(new { success = false, message = "Không thể hoàn thành giao dịch." });

            transaction.Status = 3; // Hoàn thành
            transaction.UpdateDate = DateTime.Now;
            _tranRepo.Update(transaction);

            return Json(new { success = true, message = "✅ Giao dịch đã hoàn thành." });
        }

        //Nhấn Khiếu nại
        [HttpPost]
        public async Task<JsonResult> SendComplaint(string transactionId, string content, List<IFormFile> attachments)
        {
            try
            {
                int accountId = HttpContext.Session.GetInt32("AccountId") ?? 0;
                Transaction tran = _tranRepo.Find(x => x.TransactionId == transactionId).FirstOrDefault();
                if (tran == null)
                    return Json(new { success = false, message = "Không tìm thấy giao dịch." });

                if (attachments.Count > 5)
                    return Json(new { success = false, message = "Chỉ được phép gửi tối đa 5 tệp." });


                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/complaints");
                if (!Directory.Exists(uploadDir))
                    Directory.CreateDirectory(uploadDir);

                var fileLinks = new List<string>();

                foreach (var file in attachments)
                {
                    if (file.Length > 100 * 1024 * 1024) // > 100MB
                    {
                        return Json(new { success = false, message = $"Tệp {file.FileName} vượt quá giới hạn 100MB." });
                    }
                }
                //Tạo dispute
                Dispute dispute = new Dispute()
                {
                    TransactionId = transactionId,
                    DisputeBy= accountId,
                    DisputeTo=(accountId==tran.SenderId)?tran.ReceiverId:tran.SenderId,
                    Description= content,
                    Status=1,
                    CreatedDate= DateTime.Now
                };
                await _disputeRepo.CreateAsync(dispute);
                Dispute disp = _disputeRepo.Find(x => x.TransactionId == transactionId).FirstOrDefault();

                foreach (var file in attachments)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                    var fullPath = Path.Combine(uploadDir, fileName);

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    fileLinks.Add($"/uploads/complaints/{fileName}");
                    //Lưu bảng DisputeFile
                    DisputeFile dpf = new DisputeFile()
                    {
                        DisputeId=disp.Id,
                        FilePath=fullPath,
                        FileName=fileName,
                        CreatedBy=accountId,
                    };
                    await _disputeFileRepo.CreateAsync(dpf);
                }

                // Cập nhật trạng thái status=6 bảng transaction
                tran.Status = 6;
                tran.UpdateDate = DateTime.Now;
                _tranRepo.Update(tran);

                //Gủi email cho bên còn lại
                var subjectClaim = "Thông báo! Bạn vừa bị khiếu nại 1 giao dịch";
                var bodyClaim = $"Mã giao dịch bị khiếu nại là: {transactionId}.\nVui lòng đăng nhập và phản hồi cho chúng tôi biết ở mục Lịch sử giao dịch";
                int idAccountSendMail = (accountId == tran.SenderId) ? tran.ReceiverId : tran.SenderId;
                Account accc = _accRepo.Find(x => x.Id == idAccountSendMail).FirstOrDefault();
                await _emailService.SendEmailAsync(accc.Email, subjectClaim, bodyClaim);

                return Json(new { success = true, message = "📩 Khiếu nại đã được gửi thành công." });
            }
            catch (Exception ex) {
                Console.WriteLine(ex.InnerException?.Message);
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        //Nhấn hủy giao dịch
        [HttpPost]
        public JsonResult CancelTransaction(string transactionId)
        {
            try
            {
                Transaction transaction = _tranRepo.Find(x => x.TransactionId == transactionId).FirstOrDefault();
                if (transaction == null)
                    return Json(new { success = false, message = "Không tìm thấy giao dịch." });

                if (transaction.Status != 1) // Chỉ cho phép hủy nếu đang ở trạng thái 1 (đã nhận ký quỹ)
                    return Json(new { success = false, message = "Không thể hủy giao dịch ở trạng thái hiện tại." });

                return Json(new { success = true, message = "." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
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
                tran.SenderId,
                tran.ReceiverId,
                tran.CreatedDate,
                tran.Status,
                tran.Description,
                emailSender= _accRepo.Find(x => x.Id == tran.SenderId).FirstOrDefault().Email,
                emailReceiver = _accRepo.Find(x => x.Id == tran.ReceiverId).FirstOrDefault().Email
            });

            return Json(new { success = true, data = result });
        }

        private string GetStatus(Transaction tran)
        {
            if (tran.Status == 4) return "❌ Đã hủy";
            //if (tran.ExpireDate.HasValue && tran.ExpireDate.Value < DateTime.Now) return "⏰ Hết hạn";
            return tran.Status switch
            {
                1 => "⏳ Đang đợi xác nhận",
                2 => "✅ Đã hoàn thành",
                _ => "Không rõ"
            };
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
            HttpContext.Session.SetInt32("FeePercent", list.Percent ?? 0);
            HttpContext.Session.SetString("Mail", acc.Email);
            var test = HttpContext.Session.GetString("Mail");
            Console.WriteLine($"Session Mail: {test}");
            return View();
        }

        public IActionResult Index()
        {
            return View( );
        }
        public async Task<IActionResult> CreateSecretkey(decimal amount, int accountId, string paymentMethod, string description, int feePercent, string receiverEmail)
        {
            try
            {
                var account = _accRepo.Find(x => x.Email == receiverEmail).FirstOrDefault();
                var model = new Transaction
                {
                    Amount = amount,
                    SenderId = accountId,
                    ReceiverId=account.Id,
                    PaymentMethod = paymentMethod,
                    Description = description,
                    Status = 1,
                    CreatedDate = DateTime.Now,
                    TransactionId = LoadCode(),
                    //Secretkey = GenerateUniqueSecretKey(),
                    FeeSend = feePercent
                };
                await _tranRepo.CreateAsync(model);

                var subjectClaim = "Thông báo! Bạn vừa tạo 1 giao dịch";
                var bodyClaim = $"Mã giao dịch được tạo là: {model.TransactionId}.";
                Account acc = _accRepo.Find(x => x.Id == accountId).FirstOrDefault();
                await _emailService.SendEmailAsync(acc.Email, subjectClaim, bodyClaim);

                
                var subjectCreate = "Thông báo! Bạn vừa tham gia 1 giao dịch";
                var bodyCreate = $"Mã giao dịch được tạo là: {model.TransactionId}.";
                await _emailService.SendEmailAsync(receiverEmail, subjectClaim, bodyClaim);

                return View("SecretKey",model.TransactionId);
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

        [HttpPost]
        public JsonResult CheckReceiverEmail(string email)
        {
            bool exists = _accRepo.GetAll().Any(x => x.Email == email);
            return Json(new { exists });
        }
    }
}