using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VetPanelPremium.Models
{
    public class MedicalRecord
    {
        [Key]
        public int Id { get; set; }
        
        public DateTime VisitDate { get; set; } = DateTime.Now;
        
        public string? Complaint { get; set; } // Anamnez / Şikayet
        
        [Required(ErrorMessage = "Teşhis veya yapılan işlem belirtilmelidir.")]
        public string Diagnosis { get; set; } = string.Empty; // Kesin Teşhis / İşlem
        
        public string? Treatment { get; set; } // Uygulanan Tedavi / Reçete
        
        public DateTime? NextControlDate { get; set; } // Randevu Takvimi İçin
        
        public string? AttachmentUrl { get; set; } // Dijital Arşiv (PDF/Röntgen)

        // 🚀 YENİ EKLENEN HAYATİ SÜTUN: KİLO TAKİBİ (Grafik için)
        public double? Weight { get; set; } // Muayene anındaki kilo (kg)

        // İLİŞKİ: Bu muayene hangi pete ait?
        public int PetId { get; set; }
        [ForeignKey("PetId")]
        public Pet? Pet { get; set; }
    }
}