using Concurrency.Dto;
using Concurrency.Dto.Responses;
using Concurrency.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Concurrency.Web.Controllers
{
    public class BookingController : Controller
    {
        IBookingService bookingService;

        public BookingController(IBookingService bookingService)
        {
            this.bookingService = bookingService;
        }

        public async Task<IActionResult> Index(int slotId)
        {
            return View(await bookingService.GetBooking(slotId));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(BookingDto booking)
        {
            //slot is not booked yet
            if(booking.Id == 0)
            {
                string reservationResponse = 
                    await bookingService.ReserveSlot(booking);
                ViewBag.ReservationResponse = reservationResponse;
            }
            else
            {
                OperationResponse<BookingDto> updateResponse =
                   await bookingService.UpdateBooking(booking);

                booking = updateResponse.Data;

                ViewBag.ReservationResponse = updateResponse.Status;

            }            

            return View("Index", booking);
        } 
    }
}
