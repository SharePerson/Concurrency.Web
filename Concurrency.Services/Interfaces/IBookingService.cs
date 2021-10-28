using Concurrency.Dto;
using Concurrency.Dto.Responses;
using System.Threading.Tasks;

namespace Concurrency.Services.Interfaces
{
    public interface IBookingService
    {
        Task<string> ReserveSlot(BookingDto bookingDto);
        Task<OperationResponse<BookingDto>> UpdateBooking(BookingDto bookingDto);
        Task<OperationResponse<bool>> DeleteBooking(long bookingId);
        Task<BookingDto> GetBooking(int slotId);
    }
}
