using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoodleNote.Data.Migrations;

    /// <inheritdoc />
    public partial class NoteEncapsulation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "DoodleNotes");

            migrationBuilder.DropColumn(
                name: "NoteTitle",
                table: "DoodleNotes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "DoodleNotes",
                type: "date",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "NoteTitle",
                table: "DoodleNotes",
                type: "nvarchar(25)",
                maxLength: 25,
                nullable: false,
                defaultValue: "");
        }
    }
