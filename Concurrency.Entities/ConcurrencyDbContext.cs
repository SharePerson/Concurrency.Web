using Concurrency.Entities.Banking;
using Microsoft.EntityFrameworkCore;
using System;

namespace Concurrency.Entities
{
    public class ConcurrencyDbContext: DbContext
    {
        public ConcurrencyDbContext(DbContextOptions options): base(options)
        {

        }

        public DbSet<Product> Products { set; get; }

        public DbSet<Slot> Slots { set; get; }

        public DbSet<Booking> Bookings { set; get; }

        public DbSet<Account> Accounts { set; get; }

        public DbSet<Transaction> Transactions { set; get; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>().HasData(new Product
            {
                Id = 1,
                Name = "Test Product 1",
                Description = "This is a description of test product 1",
                Price = 10
            });

            modelBuilder.Entity<Product>().HasData(new Product
            {
                Id = 2,
                Name = "Test Product 2",
                Description = "This is a description of test product 2",
                Price = 20
            });

            modelBuilder.Entity<Slot>().HasData(new Slot
            {
                Id = 1,
                Name = "Slot 1"
            });

            modelBuilder.Entity<Slot>().HasData(new Slot
            {
                Id = 2,
                Name = "Slot 2"
            });

            modelBuilder.Entity<Slot>().HasData(new Slot
            {
                Id = 3,
                Name = "Slot 3"
            });

            modelBuilder.Entity<Account>().HasData(new Account
            {
                Id = Guid.Parse("1cfbbe9e-d9ad-4512-98c7-1ac32c0949f8"),
                AccountHolderName = "User 1",
                Balance = 1000
            });

            modelBuilder.Entity<Account>().HasData(new Account
            {
                Id = Guid.Parse("67675cf8-7518-4551-b775-e89c467d4228"),
                AccountHolderName = "User 2",
                Balance = 1000
            });

            modelBuilder.Entity<Account>().HasData(new Account
            {
                Id = Guid.Parse("10d7e635-51f7-4061-a89d-5b62beb361f4"),
                AccountHolderName = "User 3",
                Balance = 1000
            });

            modelBuilder.Entity<Account>().HasData(new Account
            {
                Id = Guid.Parse("35e902fa-034f-4e32-89e5-8f8019906fbd"),
                AccountHolderName = "User 4",
                Balance = 1000
            });

            modelBuilder.Entity<Account>().HasData(new Account
            {
                Id = Guid.Parse("e7661426-9171-426b-aa63-5ef958830a8e"),
                AccountHolderName = "User 5",
                Balance = 1000
            });
        }
    }
}
