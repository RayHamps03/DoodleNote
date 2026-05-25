using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoodleNote.Data.Migrations
{
    /// <inheritdoc />
    public partial class DoodleNoteImageUrlProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "DoodleNotes",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "DoodleNotes");
        }
    }
}
