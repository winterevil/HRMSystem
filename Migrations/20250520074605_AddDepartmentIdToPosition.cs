using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRMSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddDepartmentIdToPosition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "RecruitmentPositions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_RecruitmentPositions_DepartmentId",
                table: "RecruitmentPositions",
                column: "DepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_RecruitmentPositions_Departments_DepartmentId",
                table: "RecruitmentPositions",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecruitmentPositions_Departments_DepartmentId",
                table: "RecruitmentPositions");

            migrationBuilder.DropIndex(
                name: "IX_RecruitmentPositions_DepartmentId",
                table: "RecruitmentPositions");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "RecruitmentPositions");
        }
    }
}
