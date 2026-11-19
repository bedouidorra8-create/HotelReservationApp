using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelReservation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AjouterEnumGenre : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE Utilisateurs SET Sexe = CASE 
                    WHEN LOWER(Sexe) IN ('m', 'homme', 'masculin', '1') THEN '1'
                    WHEN LOWER(Sexe) IN ('f', 'femme', 'féminin', 'feminin', '2') THEN '2'
                    ELSE NULL
                END WHERE Sexe IS NOT NULL;
            ");

            migrationBuilder.AlterColumn<int>(
                name: "Sexe",
                table: "Utilisateurs",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "Actif",
                table: "Chambres",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Actif",
                table: "Chambres");

            migrationBuilder.UpdateData(
                table: "Utilisateurs",
                keyColumn: "Sexe",
                keyValue: null,
                column: "Sexe",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Sexe",
                table: "Utilisateurs",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
