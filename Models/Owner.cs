using System.ComponentModel.DataAnnotations;

namespace VetPanelPremium.Models
{
    public class Owner
    {
        [Key]
        public int Id { get; set; }
        
        [Required, StringLength(100)]
        public string FullName { get; set; } = string.Empty;
        
        [Required, StringLength(15)]
        public string PhoneNumber { get; set; } = string.Empty;
        
        [StringLength(11)]
        public string? IdentityNumber { get; set; } // Premium: Fatura kesimi için TC/VKN
        
        public string? Address { get; set; }
        
        public decimal DebtBalance { get; set; } = 0; // Kasa Modülü İçin Borç Takibi
        
        public DateTime RegisteredDate { get; set; } = DateTime.Now;

        // İLİŞKİ: 1 Müşterinin BİRDEN FAZLA Pet'i olabilir
        public ICollection<Pet>? Pets { get; set; }
    }
}