using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shelfwise.Catalog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStockReservationIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_StockReservations",
                schema: "catalog",
                table: "StockReservations");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                schema: "catalog",
                table: "StockReservations",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateOnly>(
                name: "ReservedOn",
                schema: "catalog",
                table: "StockReservations",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddPrimaryKey(
                name: "PK_StockReservations",
                schema: "catalog",
                table: "StockReservations",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_StockReservations_BatchId_OrderId",
                schema: "catalog",
                table: "StockReservations",
                columns: new[] { "BatchId", "OrderId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_StockReservations",
                schema: "catalog",
                table: "StockReservations");

            migrationBuilder.DropIndex(
                name: "IX_StockReservations_BatchId_OrderId",
                schema: "catalog",
                table: "StockReservations");

            migrationBuilder.DropColumn(
                name: "Id",
                schema: "catalog",
                table: "StockReservations");

            migrationBuilder.DropColumn(
                name: "ReservedOn",
                schema: "catalog",
                table: "StockReservations");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StockReservations",
                schema: "catalog",
                table: "StockReservations",
                columns: new[] { "BatchId", "OrderId" });
        }
    }
}
