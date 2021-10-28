using Concurrency.Dto;
using Concurrency.Dto.Responses;
using Concurrency.Entities;
using Concurrency.Services.Base;
using Concurrency.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Concurrency.Services
{
    public class BookingService : Mapper, IBookingService
    {

        private readonly ConcurrencyDbContext dbContext;

        public BookingService(ConcurrencyDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<string> ReserveSlot(BookingDto bookingDto)
        {
            Booking booking = await dbContext.Bookings.FirstOrDefaultAsync(b => b.SlotId == bookingDto.SlotId);

            if (booking == null)
            {
                try
                {
                    Slot slotToReserve = await dbContext.Slots.FirstOrDefaultAsync(s => s.Id == bookingDto.SlotId);

                    if (slotToReserve == null) return "slot-has-been-updated-or-removed";

                    booking = new Booking
                    {
                        BookingDate = DateTime.Now,
                        BookingUserId = bookingDto.BookingUserId ?? Guid.NewGuid().ToString(),
                        SlotId = bookingDto.SlotId
                    };

                    //adding a new booking is fine to concurrency
                    await dbContext.AddAsync(booking);

                    dbContext.Entry(slotToReserve).Property(nameof(slotToReserve.RowVersion)).OriginalValue = bookingDto.Slot.RowVersion;

                    slotToReserve.IsAvailable = false;

                    //updating the slot is subject to concurrency failures
                    dbContext.Slots.Update(slotToReserve);
                    await dbContext.SaveChangesAsync();

                    return "success";
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    EntityEntry exEntry = ex.Entries.SingleOrDefault();

                    if(exEntry != null)
                    {
                        Slot clientEntry = exEntry.Entity as Slot;

                        if(clientEntry != null)
                        {
                            PropertyValues dbValues = exEntry.GetDatabaseValues();

                            if (dbValues == null) return "the slot has been deleted";

                            Slot dbEntry = dbValues.ToObject() as Slot;

                            if (dbEntry != null)
                            {
                                string status = string.Empty;

                                if (dbEntry.IsAvailable != clientEntry.IsAvailable)
                                {
                                    status = "slot-has-been-reserved";
                                }

                                bookingDto.Slot.RowVersion = dbEntry.RowVersion;

                                return status;
                            }
                        }
                    }
                }               
            }

            return "slot-has-been-reserved";
        }

        public async Task<BookingDto> GetBooking(int slotId)
        {
            Booking booking = await dbContext.Bookings.Include(b => b.Slot)
                .FirstOrDefaultAsync(b => b.SlotId == slotId);

            if (booking == null) return new BookingDto
            {
                Slot = MapObject<Slot, SlotModel>(await dbContext.Slots.FirstOrDefaultAsync(s => s.Id == slotId)),
                SlotId = slotId
            };

            return MapObject<Booking, BookingDto>(booking);
        }

        public async Task<OperationResponse<BookingDto>> UpdateBooking(BookingDto bookingDto)
        {
            if (bookingDto == null) return new OperationResponse<BookingDto>
            {
                Status = "bad-input"
            };

            Booking bookingToUpdate = await dbContext.Bookings.FirstOrDefaultAsync(b => b.SlotId == bookingDto.SlotId);

            if (bookingToUpdate == null)
            {
                return new OperationResponse<BookingDto>
                {
                    Data = bookingDto,
                    Status = "invalid-operation-outdated-data-to-update"
                };
            }

            dbContext.Entry(bookingToUpdate).Property(nameof(bookingToUpdate.RowVersion)).OriginalValue = bookingDto.RowVersion;

            bookingToUpdate.Notes = bookingDto.Notes;

            try
            {
                dbContext.Bookings.Update(bookingToUpdate);
                await dbContext.SaveChangesAsync();
                return new OperationResponse<BookingDto>
                {
                    Data = bookingDto,
                    Status = "success"
                };
            }
            catch (DbUpdateConcurrencyException ex)
            {
                EntityEntry exEntry = ex.Entries.SingleOrDefault();

                if (exEntry != null)
                {
                    Booking clientEntry = exEntry.Entity as Booking;

                    if (clientEntry != null)
                    {
                        PropertyValues dbValues = exEntry.GetDatabaseValues();

                        if (dbValues == null) return new OperationResponse<BookingDto>
                        {
                            Data = bookingDto,
                            Status = "invalid-operation-deleted-data-to-update"
                        };

                        Booking dbEntry = dbValues.ToObject() as Booking;

                        if (dbEntry != null)
                        {
                            string status = string.Empty;

                            if (dbEntry.Notes != clientEntry.Notes)
                            {
                                status = "invalid-operation-notes-are-updated";
                            }

                            //just in case the same model is used upon form submit
                            //row version should be updated to latest from db
                            //to validate the subsequent updates with the latest
                            //version from db so far
                            bookingDto.RowVersion = dbEntry.RowVersion;
                            bookingDto.Notes = dbEntry.Notes;

                            return new OperationResponse<BookingDto>
                            {
                                Data = bookingDto,
                                Status = status
                            };
                        }
                    }
                }

                return new OperationResponse<BookingDto>
                {
                    Data = bookingDto,
                    Status = "failure"
                };
            }
        }

        public async Task<OperationResponse<bool>> DeleteBooking(long bookingId)
        {
            Booking bookingToDelete = await dbContext.Bookings.Include(b => b.Slot)
                .AsNoTracking().FirstOrDefaultAsync(b => b.Id == bookingId);

            //someone else deleted the booking before this attempt
            //which is OK
            if (bookingToDelete == null)
            {
                return new OperationResponse<bool>
                {
                    Data = true,
                    Status = "success"
                };
            }

            try
            {
                dbContext.Bookings.Remove(bookingToDelete);

                Slot slotToFree = bookingToDelete.Slot;
                slotToFree.IsAvailable = true;
                dbContext.Slots.Update(slotToFree);

                await dbContext.SaveChangesAsync();

                return new OperationResponse<bool>
                {
                    Data = true,
                    Status = "success"
                };
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return new OperationResponse<bool>
                {
                    Data = false,
                    Status = "data-is-updated-before-deletion",
                    Error = ex.Message
                };
            }
        }
    }
}
