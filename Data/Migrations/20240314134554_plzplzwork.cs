using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _521_Project_3.Data.Migrations
{
    /// <inheritdoc />
    public partial class plzplzwork : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IMDBLink",
                table: "Movie",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IMDBLink",
                table: "Movie");
        }
    }
}
