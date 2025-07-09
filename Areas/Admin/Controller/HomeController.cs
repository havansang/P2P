using P2P.Models;
using P2P.Config;
using Microsoft.AspNetCore.Mvc;
using P2P.Repository;
using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace P2P.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
        AccountRepository _accRepo = new AccountRepository();
        public IActionResult Index()
        {
            Account acc = _accRepo.GetByID(HttpContext.Session.GetInt32("AccountId") ?? 0);
            if (acc == null || acc.Role < 2)
            {
                return Redirect("/Home/Index");
            }
            ViewBag.Account = acc;

            DateTime currentDate = DateTime.Now;
            DateTime dateStart = currentDate.AddDays(-30);
            ViewBag.DateStart = dateStart.ToString("yyyy-MM-dd");
            ViewBag.DateEnd = currentDate.ToString("yyyy-MM-dd");

            ViewBag.Month = currentDate.ToString("yyyy-MM");
            ViewBag.Year = currentDate.ToString("yyyy");
            ViewBag.Month1 = currentDate.ToString("MM");
            return View();
        } 
    }

}
