using Concurrency.Dto;
using Concurrency.Services.Factories;
using Concurrency.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Concurrency.Web.Controllers
{
    public class BankController : Controller
    {
        private readonly ITaskBookingGateway bookingGateway;

        public BankController()
        {
            bookingGateway = BookingGatewayFactory.CreateScoped<ITaskBookingGateway>();
        }

        public IActionResult Index()
        {
            AccountDto account = bookingGateway.GetRandomAccount();
            return View(account);
        }
    }
}
