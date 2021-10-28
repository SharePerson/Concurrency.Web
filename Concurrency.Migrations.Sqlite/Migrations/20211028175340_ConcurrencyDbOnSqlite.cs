using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Concurrency.Migrations.Sqlite.Migrations
{
    public partial class ConcurrencyDbOnSqlite : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccountHolderName = table.Column<string>(type: "TEXT", nullable: true),
                    Balance = table.Column<double>(type: "REAL", nullable: false),
                    LastTransactionDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                });

            migrationBuilder.Sql(
               @"
                CREATE TRIGGER SetContactTimestampOnUpdate
                AFTER UPDATE ON Accounts
                BEGIN
                    UPDATE Accounts
                    SET RowVersion = randomblob(8)
                    WHERE rowid = NEW.rowid;
                END
            ");
            migrationBuilder.Sql(
                @"
                CREATE TRIGGER SetContactTimestampOnInsert
                AFTER INSERT ON Accounts
                BEGIN
                    UPDATE Accounts
                    SET RowVersion = randomblob(8)
                    WHERE rowid = NEW.rowid;
                END
            ");

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Price = table.Column<double>(type: "REAL", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Slots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    IsAvailable = table.Column<bool>(type: "INTEGER", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Slots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Amount = table.Column<double>(type: "REAL", nullable: false),
                    AccountId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transactions_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BookingDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    BookingUserId = table.Column<string>(type: "TEXT", nullable: true),
                    SlotId = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bookings_Slots_SlotId",
                        column: x => x.SlotId,
                        principalTable: "Slots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Accounts",
                columns: new[] { "Id", "AccountHolderName", "Balance", "LastTransactionDate" },
                values: new object[] { new Guid("1cfbbe9e-d9ad-4512-98c7-1ac32c0949f8"), "User 1", 1000.0, null });

            migrationBuilder.InsertData(
                table: "Accounts",
                columns: new[] { "Id", "AccountHolderName", "Balance", "LastTransactionDate" },
                values: new object[] { new Guid("67675cf8-7518-4551-b775-e89c467d4228"), "User 2", 1000.0, null });

            migrationBuilder.InsertData(
                table: "Accounts",
                columns: new[] { "Id", "AccountHolderName", "Balance", "LastTransactionDate" },
                values: new object[] { new Guid("10d7e635-51f7-4061-a89d-5b62beb361f4"), "User 3", 1000.0, null });

            migrationBuilder.InsertData(
                table: "Accounts",
                columns: new[] { "Id", "AccountHolderName", "Balance", "LastTransactionDate" },
                values: new object[] { new Guid("35e902fa-034f-4e32-89e5-8f8019906fbd"), "User 4", 1000.0, null });

            migrationBuilder.InsertData(
                table: "Accounts",
                columns: new[] { "Id", "AccountHolderName", "Balance", "LastTransactionDate" },
                values: new object[] { new Guid("e7661426-9171-426b-aa63-5ef958830a8e"), "User 5", 1000.0, null });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Description", "Name", "Price" },
                values: new object[] { 1, "This is a description of test product 1", "Test Product 1", 10.0 });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Description", "Name", "Price" },
                values: new object[] { 2, "This is a description of test product 2", "Test Product 2", 20.0 });

            migrationBuilder.InsertData(
                table: "Slots",
                columns: new[] { "Id", "IsAvailable", "Name" },
                values: new object[] { 1, true, "Slot 1" });

            migrationBuilder.InsertData(
                table: "Slots",
                columns: new[] { "Id", "IsAvailable", "Name" },
                values: new object[] { 2, true, "Slot 2" });

            migrationBuilder.InsertData(
                table: "Slots",
                columns: new[] { "Id", "IsAvailable", "Name" },
                values: new object[] { 3, true, "Slot 3" });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_SlotId",
                table: "Bookings",
                column: "SlotId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_AccountId",
                table: "Transactions",
                column: "AccountId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "Slots");

            migrationBuilder.DropTable(
                name: "Accounts");
        }
    }
}
