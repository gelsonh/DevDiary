using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevDiary.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderToBlogPost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Order",
                schema: "blog",
                table: "BlogPosts",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Order",
                schema: "blog",
                table: "BlogPosts");
        }
    }
}
