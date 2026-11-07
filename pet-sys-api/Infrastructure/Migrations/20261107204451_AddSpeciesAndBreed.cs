using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSpeciesAndBreed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BreedId",
                table: "Pets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Species",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, collation: "Latin1_General_CI_AI")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Species", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Breeds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, collation: "Latin1_General_CI_AI"),
                    SpeciesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Breeds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Breeds_Species_SpeciesId",
                        column: x => x.SpeciesId,
                        principalTable: "Species",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Species",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Perro" },
                    { 2, "Gato" },
                    { 3, "Ave" },
                    { 4, "Conejo" },
                    { 5, "Roedor" },
                    { 6, "Reptil" }
                });

            migrationBuilder.InsertData(
                table: "Breeds",
                columns: new[] { "Id", "Name", "SpeciesId" },
                values: new object[,]
                {
                    { 1, "No especificada", 1 },
                    { 2, "Mestizo", 1 },
                    { 3, "Labrador Retriever", 1 },
                    { 4, "Golden Retriever", 1 },
                    { 5, "Caniche", 1 },
                    { 6, "Bulldog Francés", 1 },
                    { 7, "Pastor Alemán", 1 },
                    { 8, "Beagle", 1 },
                    { 9, "Dachshund", 1 },
                    { 10, "Yorkshire Terrier", 1 },
                    { 11, "Border Collie", 1 },
                    { 12, "Pitbull", 1 },
                    { 13, "Chihuahua", 1 },
                    { 14, "Boxer", 1 },
                    { 15, "Rottweiler", 1 },
                    { 16, "Shih Tzu", 1 },
                    { 17, "No especificada", 2 },
                    { 18, "Mestizo", 2 },
                    { 19, "Europeo", 2 },
                    { 20, "Siamés", 2 },
                    { 21, "Persa", 2 },
                    { 22, "Maine Coon", 2 },
                    { 23, "Bengalí", 2 },
                    { 24, "Angora", 2 },
                    { 25, "Sphynx", 2 },
                    { 26, "British Shorthair", 2 },
                    { 27, "No especificada", 3 },
                    { 28, "Canario", 3 },
                    { 29, "Cacatúa", 3 },
                    { 30, "Periquito", 3 },
                    { 31, "Loro", 3 },
                    { 32, "Agapornis", 3 },
                    { 33, "Ninfa", 3 },
                    { 34, "No especificada", 4 },
                    { 35, "Mestizo", 4 },
                    { 36, "Belier", 4 },
                    { 37, "Rex", 4 },
                    { 38, "Cabeza de León", 4 },
                    { 39, "Enano Holandés", 4 },
                    { 40, "No especificada", 5 },
                    { 41, "Hámster", 5 },
                    { 42, "Cobayo", 5 },
                    { 43, "Chinchilla", 5 },
                    { 44, "Rata", 5 },
                    { 45, "Ratón", 5 },
                    { 46, "Jerbo", 5 },
                    { 47, "No especificada", 6 },
                    { 48, "Tortuga", 6 },
                    { 49, "Iguana", 6 },
                    { 50, "Gecko", 6 },
                    { 51, "Serpiente", 6 }
                });

            // Convierte los strings existentes al catálogo, comparando sin mayúsculas ni tildes.
            // Lo que no está en el catálogo se crea (cada especie nueva con su "No especificada"), así no se pierde nada.
            migrationBuilder.Sql(@"
INSERT INTO Species (Name)
SELECT DISTINCT LTRIM(RTRIM(p.Specie)) COLLATE Latin1_General_CI_AI
FROM Pets p
WHERE NOT EXISTS (SELECT 1 FROM Species s WHERE s.Name = LTRIM(RTRIM(p.Specie)) COLLATE Latin1_General_CI_AI);

INSERT INTO Breeds (Name, SpeciesId)
SELECT N'No especificada', s.Id
FROM Species s
WHERE NOT EXISTS (SELECT 1 FROM Breeds b WHERE b.SpeciesId = s.Id AND b.Name = N'No especificada');

INSERT INTO Breeds (Name, SpeciesId)
SELECT DISTINCT LTRIM(RTRIM(p.Breed)) COLLATE Latin1_General_CI_AI, s.Id
FROM Pets p
JOIN Species s ON s.Name = LTRIM(RTRIM(p.Specie)) COLLATE Latin1_General_CI_AI
WHERE NOT EXISTS (SELECT 1 FROM Breeds b WHERE b.SpeciesId = s.Id AND b.Name = LTRIM(RTRIM(p.Breed)) COLLATE Latin1_General_CI_AI);

UPDATE p SET p.BreedId = b.Id
FROM Pets p
JOIN Species s ON s.Name = LTRIM(RTRIM(p.Specie)) COLLATE Latin1_General_CI_AI
JOIN Breeds b ON b.SpeciesId = s.Id AND b.Name = LTRIM(RTRIM(p.Breed)) COLLATE Latin1_General_CI_AI;
");

            migrationBuilder.DropColumn(
                name: "Breed",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "Specie",
                table: "Pets");

            migrationBuilder.CreateIndex(
                name: "IX_Pets_BreedId",
                table: "Pets",
                column: "BreedId");

            migrationBuilder.CreateIndex(
                name: "IX_Breeds_SpeciesId_Name",
                table: "Breeds",
                columns: new[] { "SpeciesId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Species_Name",
                table: "Species",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Pets_Breeds_BreedId",
                table: "Pets",
                column: "BreedId",
                principalTable: "Breeds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Breed",
                table: "Pets",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Specie",
                table: "Pets",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(@"
UPDATE p SET p.Specie = s.Name, p.Breed = b.Name
FROM Pets p
JOIN Breeds b ON b.Id = p.BreedId
JOIN Species s ON s.Id = b.SpeciesId;
");

            migrationBuilder.DropForeignKey(
                name: "FK_Pets_Breeds_BreedId",
                table: "Pets");

            migrationBuilder.DropTable(
                name: "Breeds");

            migrationBuilder.DropTable(
                name: "Species");

            migrationBuilder.DropIndex(
                name: "IX_Pets_BreedId",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "BreedId",
                table: "Pets");
        }
    }
}
