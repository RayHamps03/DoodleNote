using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoodleNote.Data.Migrations;

    /// <inheritdoc />
    public partial class AddUserNoteRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "DoodleNotes",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DoodleNotes_UserId",
                table: "DoodleNotes",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_DoodleNotes_AspNetUsers_UserId",
                table: "DoodleNotes",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DoodleNotes_AspNetUsers_UserId",
                table: "DoodleNotes");

            migrationBuilder.DropIndex(
                name: "IX_DoodleNotes_UserId",
                table: "DoodleNotes");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "DoodleNotes");
        }
    }
