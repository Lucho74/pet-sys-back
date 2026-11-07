using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameIsDeletedToIsSuspended : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "Users",
                newName: "IsSuspended");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "Pets",
                newName: "IsSuspended");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "Consultations",
                newName: "IsSuspended");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsSuspended",
                table: "Users",
                newName: "IsDeleted");

            migrationBuilder.RenameColumn(
                name: "IsSuspended",
                table: "Pets",
                newName: "IsDeleted");

            migrationBuilder.RenameColumn(
                name: "IsSuspended",
                table: "Consultations",
                newName: "IsDeleted");
        }
    }
}
