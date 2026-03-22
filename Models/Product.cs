using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VetPanelPremium.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Ürün adı zorunludur.")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Kategori seçimi zorunludur.")]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string? Barcode { get; set; }
        
        // 💰 Kliniğin ürünü alış fiyatı
        [Column(TypeName = "decimal(18,2)")]
        public decimal PurchasePrice { get; set; } = 0;
        
        // 🚀 ADİSYON MOTORUNUN KULLANDIĞI SATIŞ FİYATI!
        [Column(TypeName = "decimal(18,2)")]
        public decimal SalePrice { get; set; } = 0;
        
        // 📦 Otomatik düşülen depo stoğu
        public int StockQuantity { get; set; } = 0;

        // 🚨 YENİ EKLENEN HAYATİ SÜTUN: SON KULLANMA TARİHİ (SKT RADARI İÇİN)
        public DateTime? ExpirationDate { get; set; }
        
        public bool IsActive { get; set; } = true;
    }
}