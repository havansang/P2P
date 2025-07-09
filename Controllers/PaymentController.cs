using System.Net.Http;
using System.Threading.Tasks;
using System.Transactions;
using DocumentFormat.OpenXml.Drawing.Charts;
using Microsoft.AspNetCore.Mvc;
using P2P.Config;

namespace P2P.Controllers
{
    public class PaymentController : Controller
    {
        private readonly HttpClient _httpClient;

        public PaymentController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpGet]
        public async Task<IActionResult> CheckGoogleSheetTransaction()
        {
            string url = "https://script.google.com/macros/s/AKfycbx8shjiW20CI1jf_ehwGtwA-N14jXtFLW7sPcpW-ssf-7ViM9Lw_WVyvA3Q70mbMTAL/exec";

            try
            {
                var response = await _httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();
                return Content(content, "application/json");
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = true, message = ex.Message });
            }
        }
    }
}
