using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRMSystem.Migrations
{
    /// <inheritdoc />
    public partial class FixChat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRead",
                table: "Messages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ReceiverId",
                table: "Messages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ChatRequests_FromEmployeeId",
                table: "ChatRequests",
                column: "FromEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRequests_ToEmployeeId",
                table: "ChatRequests",
                column: "ToEmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatRequests_Employees_FromEmployeeId",
                table: "ChatRequests",
                column: "FromEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatRequests_Employees_ToEmployeeId",
                table: "ChatRequests",
                column: "ToEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatRequests_Employees_FromEmployeeId",
                table: "ChatRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatRequests_Employees_ToEmployeeId",
                table: "ChatRequests");

            migrationBuilder.DropIndex(
                name: "IX_ChatRequests_FromEmployeeId",
                table: "ChatRequests");

            migrationBuilder.DropIndex(
                name: "IX_ChatRequests_ToEmployeeId",
                table: "ChatRequests");

            migrationBuilder.DropColumn(
                name: "IsRead",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "ReceiverId",
                table: "Messages");
        }
    }
}
