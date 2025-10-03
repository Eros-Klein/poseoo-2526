using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CashRegister.Data.Migrations
{
    /// <inheritdoc />
    public partial class new4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Products_ProductId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_ReceiptLines_Orders_ReceiptId",
                table: "ReceiptLines");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Orders",
                table: "Orders");

            migrationBuilder.RenameTable(
                name: "Orders",
                newName: "Receipts");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_ProductId",
                table: "Receipts",
                newName: "IX_Receipts_ProductId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Receipts",
                table: "Receipts",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReceiptLines_Receipts_ReceiptId",
                table: "ReceiptLines",
                column: "ReceiptId",
                principalTable: "Receipts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Receipts_Products_ProductId",
                table: "Receipts",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReceiptLines_Receipts_ReceiptId",
                table: "ReceiptLines");

            migrationBuilder.DropForeignKey(
                name: "FK_Receipts_Products_ProductId",
                table: "Receipts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Receipts",
                table: "Receipts");

            migrationBuilder.RenameTable(
                name: "Receipts",
                newName: "Orders");

            migrationBuilder.RenameIndex(
                name: "IX_Receipts_ProductId",
                table: "Orders",
                newName: "IX_Orders_ProductId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Orders",
                table: "Orders",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Products_ProductId",
                table: "Orders",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReceiptLines_Orders_ReceiptId",
                table: "ReceiptLines",
                column: "ReceiptId",
                principalTable: "Orders",
                principalColumn: "Id");
        }
    }
}
