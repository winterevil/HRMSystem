using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRMSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddJobPostStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "JobPosts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            //migrationBuilder.AddColumn<bool>(
            //    name: "IsSystemOnly",
            //    table: "EmployeeTypes",
            //    type: "bit",
            //    nullable: false,
            //    defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "JobPosts");

            //migrationBuilder.DropColumn(
            //    name: "IsSystemOnly",
            //    table: "EmployeeTypes");
        }
    }
}
