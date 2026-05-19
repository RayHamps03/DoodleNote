using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoodleNote.Data.Migrations;

/// <inheritdoc />
public partial class DoodleNoteProperties : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "DoodleNotes",
            columns: table => new
            {
                NoteId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                NoteTitle = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                CreatedDate = table.Column<DateTime>(type: "date", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DoodleNotes", x => x.NoteId);
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "DoodleNotes");
    }
}
