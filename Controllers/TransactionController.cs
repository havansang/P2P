using Microsoft.AspNetCore.Mvc;
using P2P.Models;
using P2P.Services;
using System.Transactions;

namespace SecureEscrow.Controllers
{
    public class TransactionController : Controller
    {
        private readonly TransactionService _transactionService;

        public TransactionController(TransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateTransactionViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTransactionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var transaction = await _transactionService.CreateTransactionAsync(model);
                TempData["SuccessMessage"] = "Giao dịch đã được tạo thành công!";
                TempData["TransactionKey"] = transaction.TransactionKey;
                return RedirectToAction("Success", new { id = transaction.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra khi tạo giao dịch. Vui lòng thử lại.");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Success(int id)
        {
            var transaction = await _transactionService.GetTransactionByKeyAsync(TempData["TransactionKey"]?.ToString() ?? "");
            if (transaction == null)
            {
                return NotFound();
            }

            ViewBag.TransactionKey = transaction.TransactionKey;
            return View(transaction);
        }

        [HttpGet]
        public IActionResult Claim()
        {
            return View(new ClaimTransactionViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Verify(ClaimTransactionViewModel model)
        {
            if (string.IsNullOrEmpty(model.TransactionKey))
            {
                ModelState.AddModelError("TransactionKey", "Vui lòng nhập mã giao dịch");
                return View("Claim", model);
            }

            var transaction = await _transactionService.GetTransactionByKeyAsync(model.TransactionKey);

            if (transaction == null)
            {
                model.ErrorMessage = "Mã giao dịch không tồn tại. Vui lòng kiểm tra lại.";
                return View("Claim", model);
            }

            if (transaction.Status != TransactionStatus.Paid)
            {
                model.ErrorMessage = transaction.Status switch
                {
                    TransactionStatus.Pending => "Giao dịch chưa được thanh toán.",
                    TransactionStatus.Completed => "Giao dịch này đã được nhận.",
                    _ => "Giao dịch không hợp lệ."
                };
                return View("Claim", model);
            }

            model.VerifiedTransaction = transaction;
            model.IsVerified = true;
            model.RecipientName = transaction.RecipientName;

            return View("Claim", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Claim(ClaimTransactionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Re-verify transaction for display
                var transaction = await _transactionService.GetTransactionByKeyAsync(model.TransactionKey);
                if (transaction != null && transaction.Status == TransactionStatus.Paid)
                {
                    model.VerifiedTransaction = transaction;
                    model.IsVerified = true;
                }
                return View(model);
            }

            try
            {
                var claimedTransaction = await _transactionService.ClaimTransactionAsync(model);

                if (claimedTransaction == null)
                {
                    ModelState.AddModelError("", "Không thể nhận giao dịch. Vui lòng kiểm tra thông tin.");
                    return View(model);
                }

                TempData["SuccessMessage"] = "Giao dịch đã được nhận thành công! Tiền sẽ được chuyển vào tài khoản của bạn trong vòng 24 giờ.";
                return RedirectToAction("ClaimSuccess", new { id = claimedTransaction.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra khi nhận giao dịch. Vui lòng thử lại.");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult ClaimSuccess(int id)
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var transactions = await _transactionService.GetAllTransactionsAsync();
            var transaction = transactions.FirstOrDefault(t => t.Id == id);

            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }
    }
}