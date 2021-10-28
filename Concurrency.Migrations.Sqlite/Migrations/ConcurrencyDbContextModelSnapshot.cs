﻿// <auto-generated />
using System;
using Concurrency.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Concurrency.Migrations.Sqlite.Migrations
{
    [DbContext(typeof(ConcurrencyDbContext))]
    partial class ConcurrencyDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.11");

            modelBuilder.Entity("Concurrency.Entities.Banking.Account", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("AccountHolderName")
                        .HasColumnType("TEXT");

                    b.Property<double>("Balance")
                        .HasColumnType("REAL");

                    b.Property<DateTime?>("LastTransactionDate")
                        .HasColumnType("TEXT");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("BLOB");

                    b.HasKey("Id");

                    b.ToTable("Accounts");

                    b.HasData(
                        new
                        {
                            Id = new Guid("1cfbbe9e-d9ad-4512-98c7-1ac32c0949f8"),
                            AccountHolderName = "User 1",
                            Balance = 1000.0
                        },
                        new
                        {
                            Id = new Guid("67675cf8-7518-4551-b775-e89c467d4228"),
                            AccountHolderName = "User 2",
                            Balance = 1000.0
                        },
                        new
                        {
                            Id = new Guid("10d7e635-51f7-4061-a89d-5b62beb361f4"),
                            AccountHolderName = "User 3",
                            Balance = 1000.0
                        },
                        new
                        {
                            Id = new Guid("35e902fa-034f-4e32-89e5-8f8019906fbd"),
                            AccountHolderName = "User 4",
                            Balance = 1000.0
                        },
                        new
                        {
                            Id = new Guid("e7661426-9171-426b-aa63-5ef958830a8e"),
                            AccountHolderName = "User 5",
                            Balance = 1000.0
                        });
                });

            modelBuilder.Entity("Concurrency.Entities.Banking.Transaction", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("AccountId")
                        .HasColumnType("TEXT");

                    b.Property<double>("Amount")
                        .HasColumnType("REAL");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("TransactionDate")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("AccountId");

                    b.ToTable("Transactions");
                });

            modelBuilder.Entity("Concurrency.Entities.Booking", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("BookingDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("BookingUserId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Notes")
                        .HasColumnType("TEXT");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("BLOB");

                    b.Property<int>("SlotId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("SlotId");

                    b.ToTable("Bookings");
                });

            modelBuilder.Entity("Concurrency.Entities.Product", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<double>("Price")
                        .HasColumnType("REAL");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("BLOB");

                    b.HasKey("Id");

                    b.ToTable("Products");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Description = "This is a description of test product 1",
                            Name = "Test Product 1",
                            Price = 10.0
                        },
                        new
                        {
                            Id = 2,
                            Description = "This is a description of test product 2",
                            Name = "Test Product 2",
                            Price = 20.0
                        });
                });

            modelBuilder.Entity("Concurrency.Entities.Slot", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsAvailable")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("BLOB");

                    b.HasKey("Id");

                    b.ToTable("Slots");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            IsAvailable = true,
                            Name = "Slot 1"
                        },
                        new
                        {
                            Id = 2,
                            IsAvailable = true,
                            Name = "Slot 2"
                        },
                        new
                        {
                            Id = 3,
                            IsAvailable = true,
                            Name = "Slot 3"
                        });
                });

            modelBuilder.Entity("Concurrency.Entities.Banking.Transaction", b =>
                {
                    b.HasOne("Concurrency.Entities.Banking.Account", "Account")
                        .WithMany()
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Account");
                });

            modelBuilder.Entity("Concurrency.Entities.Booking", b =>
                {
                    b.HasOne("Concurrency.Entities.Slot", "Slot")
                        .WithMany()
                        .HasForeignKey("SlotId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Slot");
                });
#pragma warning restore 612, 618
        }
    }
}
