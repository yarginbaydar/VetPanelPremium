using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VetPanelPremium.Migrations
{
    /// <inheritdoc />
    public partial class AddWeightToMedicalRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Weight",
                table: "MedicalRecords",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Weight",
                table: "MedicalRecords");
        }
    }
}
