using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class shape : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Border",
                table: "Shapes",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "BorderColor",
                table: "Shapes",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "BorderWidth",
                table: "Shapes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "Fill",
                table: "Shapes",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Shapes",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Border",
                table: "Shapes");

            migrationBuilder.DropColumn(
                name: "BorderColor",
                table: "Shapes");

            migrationBuilder.DropColumn(
                name: "BorderWidth",
                table: "Shapes");

            migrationBuilder.DropColumn(
                name: "Fill",
                table: "Shapes");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Shapes");
        }
    }
}
