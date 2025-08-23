using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMS.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswardAndConfirmPassward : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Location",
                table: "Doctors",
                newName: "Address");

            migrationBuilder.AddColumn<string>(
                name: "ConfirmPassward",
                table: "Doctors",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Passward",
                table: "Doctors",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConfirmPassward",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "Passward",
                table: "Doctors");

            migrationBuilder.RenameColumn(
                name: "Address",
                table: "Doctors",
                newName: "Location");
        }
    }
}
