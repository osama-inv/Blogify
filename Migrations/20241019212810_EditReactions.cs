using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blogify.Migrations
{
    /// <inheritdoc />
    public partial class EditReactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumOfDisLikes",
                table: "Blogs");

            migrationBuilder.DropColumn(
                name: "NumOfLikes",
                table: "Blogs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NumOfDisLikes",
                table: "Blogs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NumOfLikes",
                table: "Blogs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }
    }
}
