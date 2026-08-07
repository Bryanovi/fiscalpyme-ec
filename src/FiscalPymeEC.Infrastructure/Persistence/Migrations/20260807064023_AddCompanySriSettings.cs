using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FiscalPymeEC.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanySriSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "emission_point_code",
                table: "companies",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "001");

            migrationBuilder.AddColumn<string>(
                name: "establishment_code",
                table: "companies",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "001");

            migrationBuilder.AddColumn<int>(
                name: "sri_environment",
                table: "companies",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddCheckConstraint(
                name: "ck_companies_emission_point_code",
                table: "companies",
                sql: "emission_point_code ~ '^[0-9]{3}$'");

            migrationBuilder.AddCheckConstraint(
                name: "ck_companies_establishment_code",
                table: "companies",
                sql: "establishment_code ~ '^[0-9]{3}$'");

            migrationBuilder.AddCheckConstraint(
                name: "ck_companies_sri_environment",
                table: "companies",
                sql: "sri_environment IN (1, 2)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_companies_emission_point_code",
                table: "companies");

            migrationBuilder.DropCheckConstraint(
                name: "ck_companies_establishment_code",
                table: "companies");

            migrationBuilder.DropCheckConstraint(
                name: "ck_companies_sri_environment",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "emission_point_code",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "establishment_code",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "sri_environment",
                table: "companies");
        }
    }
}
