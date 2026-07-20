using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoodleNote.Data.Migrations
{
    /// <inheritdoc />
    public partial class NoteTitleEncapsulate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NoteTitle",
                table: "DoodleNotes",
                type: "nvarchar(25)",
                maxLength: 25,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NoteTitle",
                table: "DoodleNotes");
        }
    }
}
