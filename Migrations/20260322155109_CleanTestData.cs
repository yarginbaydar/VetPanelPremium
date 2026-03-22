using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VetPanelPremium.Migrations
{
    /// <inheritdoc />
    public partial class CleanTestData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 🚀 SİLME OPERASYONU (Hiyerarşiye tam uygun)
            
            // 0. Önce Kasadaki Fişleri / Faturaları Yırtıyoruz! (HATA ÇÖZÜMÜ)
            migrationBuilder.Sql("DELETE FROM Invoices;");

            // 1. Sonra Pet'e bağlı alt kayıtları siliyoruz
            migrationBuilder.Sql("DELETE FROM MedicalRecords;");
            migrationBuilder.Sql("DELETE FROM Appointments;");
            
            // 2. Sonra Sahibine bağlı Petleri siliyoruz
            migrationBuilder.Sql("DELETE FROM Pets;");
            
            // 3. En son Sahipleri (Müşterileri) siliyoruz
            migrationBuilder.Sql("DELETE FROM Owners;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Bu işlemin geri dönüşü (Geri al tuşu) olmadığı için burayı boş bırakıyoruz.
        }
    }
}