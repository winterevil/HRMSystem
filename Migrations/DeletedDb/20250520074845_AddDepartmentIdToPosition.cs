using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRMSystem.Migrations.DeletedDb
{
    /// <inheritdoc />
    public partial class AddDepartmentIdToPosition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "DeletedRecruitmentPositions",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "DeletedRecruitmentPositions");
        }
    }
}
