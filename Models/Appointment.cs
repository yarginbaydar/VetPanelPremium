using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VetPanelPremium.Models
{
    public class Appointment
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Randevu tarihi ve saati zorunludur.")]
        public DateTime AppointmentDate { get; set; }

        // Randevu hangi müşteriye ait?
        public int OwnerId { get; set; }
        [ForeignKey("OwnerId")]
        public Owner? Owner { get; set; }

        // Randevu hangi hastaya (Pet) ait? (Opsiyonel bıraktık, belki sadece adam kendisi gelip bir şey soracaktır)
        public int? PetId { get; set; }
        [ForeignKey("PetId")]
        public Pet? Pet { get; set; }

        // Geliş Sebebi (Aşı, Genel Muayene, Operasyon vb.)
        [Required(ErrorMessage = "Lütfen randevu sebebini belirtin."), StringLength(100)]
        public string Reason { get; set; } = string.Empty;

        // Randevu Durumu (Bekliyor, Tamamlandı, İptal)
        [Required, StringLength(50)]
        public string Status { get; set; } = "Bekliyor"; 
        
        [StringLength(500)]
        public string? Notes { get; set; } // Doktorun alacağı ufak notlar
    }
}