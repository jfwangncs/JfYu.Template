using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JfYu.WebApi.Template.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                 table: "Users",
                 columns: ["UserName", "Password", "RealName", "Status", "Avatar", "LastLoginTime", "CreatedTime", "UpdatedTime"],
                 values: ["admin", "", "小鱼", 1, "", new DateTime(2026, 2, 28, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 2, 28, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 2, 28, 0, 0, 0, DateTimeKind.Utc)]);

            migrationBuilder.InsertData(
                   table: "Roles",
                   columns: ["Name", "Status", "CreatedTime", "UpdatedTime"],
                   values: ["Admin", 1, new DateTime(2026, 2, 28, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 2, 28, 0, 0, 0, DateTimeKind.Utc)]);

            migrationBuilder.InsertData(
                 table: "RoleUser",
                 columns: ["RolesId", "UsersId"],
                 values: [1, 1]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(table: "Roles", "Id", 1);
            migrationBuilder.DeleteData(table: "Users", "Id", 1);
            migrationBuilder.DeleteData(table: "RoleUser", "UsersId", 1);
        }
    }
}
