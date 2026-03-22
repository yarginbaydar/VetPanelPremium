using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VetPanelPremium.Migrations
{
    /// <inheritdoc />
    public partial class FinalClean : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
              // LinkedIn test verilerini kalıcı olarak siliyoruz!
    migrationBuilder.Sql("DELETE FROM Invoices;");
    migrationBuilder.Sql("DELETE FROM MedicalRecords;");
    migrationBuilder.Sql("DELETE FROM Appointments;");
    migrationBuilder.Sql("DELETE FROM Pets;");
    migrationBuilder.Sql("DELETE FROM Owners;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
