using Concurrency.Dto;
using Concurrency.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Concurrency.Web.Controllers
{
    public class BankController : Controller
    {
        private readonly ITaskBookingGateway bookingGateway;

        public BankController(ITaskBookingGateway bookingGateway)
        {
            this.bookingGateway = bookingGateway;
        }

        public IActionResult Index()
        {
            AccountDto account = bookingGateway.GetRandomAccount();
            return View(account);
        }
    }
}
