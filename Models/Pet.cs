using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http; // 🚀 IFormFile için gerekli

namespace VetPanelPremium.Models
{
    public class Pet
    {
        [Key]
        public int Id { get; set; }
        
        [Required, StringLength(50)]
        public string Name { get; set; } = string.Empty;
        
        [Required, StringLength(50)]
        public string Species { get; set; } = string.Empty; 
        
        public string? Breed { get; set; } 
        public string? Gender { get; set; }
        
        public string? ChipNumber { get; set; } 
        public DateTime? BirthDate { get; set; } 
        public double? Weight { get; set; } 

        // 🚀 YENİ EKLENENLER: RÖNTGEN VE TAHLİL İÇİN
        public string? TahlilDosyaYolu { get; set; } // Veritabanında adres tutacak

        [NotMapped] // Veritabanına sütun açmaz, sadece formdan Controller'a dosya taşır
        public IFormFile? TahlilDosyasi { get; set; } 

        // İLİŞKİ 1:
        public int OwnerId { get; set; }
        [ForeignKey("OwnerId")]
        public Owner? Owner { get; set; }

        // İLİŞKİ 2:
        public ICollection<MedicalRecord>? MedicalRecords { get; set; }

        
    }
}