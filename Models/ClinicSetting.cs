using System.ComponentModel.DataAnnotations;

namespace VetPanelPremium.Models
{
    public class ClinicSetting
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Klinik adı zorunludur.")]
        [StringLength(100)]
        public string ClinicName { get; set; } = "VetPanel Premium Klinik";

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        [StringLength(50)]
        public string? TaxNumber { get; set; } // Fatura için Vergi No

        [StringLength(100)]
        public string? WorkingHours { get; set; } // Örn: 09:00 - 19:00
    }
}