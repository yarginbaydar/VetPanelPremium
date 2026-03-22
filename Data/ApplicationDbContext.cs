using Microsoft.EntityFrameworkCore;
using VetPanelPremium.Models; // Modelleri tanıması için gereken sihirli satır


namespace VetPanelPremium.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Veritabanında oluşacak tablolarımızın listesi
        public DbSet<Owner> Owners { get; set; }
        public DbSet<Pet> Pets { get; set; }
        public DbSet<MedicalRecord> MedicalRecords { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceLine> InvoiceLines { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<ClinicSetting> ClinicSettings { get; set; }
        public DbSet<AdminUser> AdminUsers { get; set; }

    }
}