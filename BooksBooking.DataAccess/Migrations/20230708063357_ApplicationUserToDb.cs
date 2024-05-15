using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BooksBooking.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ApplicationUserToDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderHeaders_AspNetUsers_Name",
                table: "OrderHeaders");

            migrationBuilder.DropIndex(
                name: "IX_OrderHeaders_Name",
                table: "OrderHeaders");

            migrationBuilder.RenameColumn(
                name: "ApplicationUserID",
                table: "OrderHeaders",
                newName: "ApplicationUserId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "OrderHeaders",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "ApplicationUserId",
                table: "OrderHeaders",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "ApplicationUserId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderHeaders_ApplicationUserId",
                table: "OrderHeaders",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderHeaders_AspNetUsers_ApplicationUserId",
                table: "OrderHeaders",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderHeaders_AspNetUsers_ApplicationUserId",
                table: "OrderHeaders");

            migrationBuilder.DropIndex(
                name: "IX_OrderHeaders_ApplicationUserId",
                table: "OrderHeaders");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "ApplicationUserId",
                table: "OrderHeaders",
                newName: "ApplicationUserID");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "OrderHeaders",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ApplicationUserID",
                table: "OrderHeaders",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_OrderHeaders_Name",
                table: "OrderHeaders",
                column: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderHeaders_AspNetUsers_Name",
                table: "OrderHeaders",
                column: "Name",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
