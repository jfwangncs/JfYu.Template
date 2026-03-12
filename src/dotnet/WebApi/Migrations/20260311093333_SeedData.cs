using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApi.Migrations
{
    /// <inheritdoc />
    public partial class SeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                           table: "Users",
                           columns: ["UserName", "Password", "RealName", "Status", "Avatar", "LastLoginTime", "CreatedTime", "UpdatedTime"],
                           values: ["admin", "$2a$11$Q.6dWlH3VtoDGG6LnrqpwumNELWLhSXzVdQlHZcCFuIOoxLQG/HaO", "小鱼", 1, "https://sns-webpic-qc.xhscdn.com/202603021614/3bb03807b63c12fbbee86e53ca3550a6/comment/1040g4b831t6osp58lsbg5pao8fb0ml2f72lihb8!nc_n_webp_mw_1", new DateTime(2026, 2, 28), new DateTime(2026, 2, 28), new DateTime(2026, 2, 28)]);

            migrationBuilder.InsertData(
                   table: "Roles",
                   columns: ["Name", "Status", "CreatedTime", "UpdatedTime"],
                   values: ["Admin", 1, new DateTime(2026, 2, 28), new DateTime(2026, 2, 28)]);

            migrationBuilder.InsertData(
                 table: "RoleUser",
                 columns: ["RolesId", "UsersId"],
                 values: [1, 1]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
