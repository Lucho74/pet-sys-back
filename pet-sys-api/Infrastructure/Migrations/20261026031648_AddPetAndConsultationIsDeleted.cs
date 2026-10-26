using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPetAndConsultationIsDeleted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Pets",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Consultations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // Clientes ya suspendidos antes de esta migración: marcar sus mascotas y consultas.
            migrationBuilder.Sql("UPDATE Pets SET IsDeleted = 1 WHERE ClientId IN (SELECT Id FROM Users WHERE IsDeleted = 1)");
            migrationBuilder.Sql("UPDATE Consultations SET IsDeleted = 1 WHERE PetId IN (SELECT Id FROM Pets WHERE IsDeleted = 1)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Consultations");
        }
    }
}
