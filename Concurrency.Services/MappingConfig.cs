using AutoMapper;
using Concurrency.Dto;
using Concurrency.Entities;
using Concurrency.Entities.Banking;

namespace Concurrency.Services
{
    public class MappingConfig
    {
        public static MapperConfiguration RegisterMaps()
        {
            return new MapperConfiguration(config =>
            {
                config.CreateMap<Account, AccountDto>().ReverseMap();
                config.CreateMap<Transaction, TransactionDto>().ReverseMap();
                config.CreateMap<Booking, BookingDto>().ReverseMap();
                config.CreateMap<Slot, SlotModel>().ReverseMap();
                config.CreateMap<Ticket, TicketDto>().ReverseMap();
            });
        }
    }
}
