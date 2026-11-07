using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AssociatePetsWithClientProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pets_Users_ClientId",
                table: "Pets");

            migrationBuilder.AddForeignKey(
                name: "FK_Pets_ClientProfiles_ClientId",
                table: "Pets",
                column: "ClientId",
                principalTable: "ClientProfiles",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pets_ClientProfiles_ClientId",
                table: "Pets");

            migrationBuilder.AddForeignKey(
                name: "FK_Pets_Users_ClientId",
                table: "Pets",
                column: "ClientId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
