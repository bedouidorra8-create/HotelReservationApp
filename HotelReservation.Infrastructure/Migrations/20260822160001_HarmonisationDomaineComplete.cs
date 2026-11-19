// ====================================================================================================
// MIGRATION EF CORE : HarmonisationDomaineComplete
// 
// Cette migration applique les évolutions majeures du modèle de domaine DDD :
// 1. Module Hotel :
//    - Remplacement de la colonne booléenne 'Actif' par l'énumération 'Statut' (1 = Actif, 2 = Inactif)
//      avec conversion SQL explicite des données existantes (1 -> 1, 0 -> 2).
//    - Retrait de la colonne obsolète 'PrixParNuit' (les tarifs étant désormais gérés unitairement par Chambre).
// 2. Module Reservation :
//    - Remplacement des colonnes 'Prix' et 'Total' (scalaires) par les Value Objects Money
//      'PrixUnitaire' (PrixUnitaireMontant, PrixUnitaireDevise) et 'Total' (TotalMontant, TotalDevise).
//    - Conservation des valeurs financières existantes avec devise par défaut 'EUR'.
// ====================================================================================================

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelReservation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class HarmonisationDomaineComplete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // --- 1. AJOUT DES NOUVELLES COLONNES ---

            migrationBuilder.AddColumn<int>(
                name: "Statut",
                table: "Hotels",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<decimal>(
                name: "PrixUnitaireMontant",
                table: "Reservations",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "PrixUnitaireDevise",
                table: "Reservations",
                type: "varchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "EUR")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalMontant",
                table: "Reservations",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "TotalDevise",
                table: "Reservations",
                type: "varchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "EUR")
                .Annotation("MySql:CharSet", "utf8mb4");

            // --- 2. MIGRATION EXPLICITE DES DONNÉES EXISTANTES ---

            // Migration des statuts d'hôtels : 1 (Actif) -> 1, 0 (Inactif) -> 2
            migrationBuilder.Sql("UPDATE Hotels SET Statut = CASE WHEN Actif = 1 THEN 1 ELSE 2 END;");

            // Migration des montants de réservations vers les Value Objects Money
            migrationBuilder.Sql("UPDATE Reservations SET PrixUnitaireMontant = Prix, PrixUnitaireDevise = 'EUR', TotalMontant = Total, TotalDevise = 'EUR';");

            // --- 3. SUPPRESSION DES ANCIENNES COLONNES ---

            migrationBuilder.DropColumn(
                name: "Actif",
                table: "Hotels");

            migrationBuilder.DropColumn(
                name: "PrixParNuit",
                table: "Hotels");

            migrationBuilder.DropColumn(
                name: "Prix",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "Total",
                table: "Reservations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // --- 1. RÉTABLISSEMENT DES ANCIENNES COLONNES ---

            migrationBuilder.AddColumn<bool>(
                name: "Actif",
                table: "Hotels",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PrixParNuit",
                table: "Hotels",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Prix",
                table: "Reservations",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Total",
                table: "Reservations",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            // --- 2. RÉTRO-MIGRATION DES DONNÉES ---

            migrationBuilder.Sql("UPDATE Hotels SET Actif = CASE WHEN Statut = 1 THEN 1 ELSE 0 END;");
            migrationBuilder.Sql("UPDATE Reservations SET Prix = PrixUnitaireMontant, Total = TotalMontant;");

            // --- 3. SUPPRESSION DES NOUVELLES COLONNES ---

            migrationBuilder.DropColumn(
                name: "Statut",
                table: "Hotels");

            migrationBuilder.DropColumn(
                name: "PrixUnitaireDevise",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "PrixUnitaireMontant",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "TotalDevise",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "TotalMontant",
                table: "Reservations");
        }
    }
}
