using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VetPanelPremium.Models
{
    public class Invoice
    {
        [Key]
        public int Id { get; set; }
        public DateTime InvoiceDate { get; set; } = DateTime.Now;
        public int? OwnerId { get; set; }
        [ForeignKey("OwnerId")]
        public Owner? Owner { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; } = 0;
        [Required, StringLength(50)]
        public string PaymentMethod { get; set; } = "Nakit";
        public ICollection<InvoiceLine>? InvoiceLines { get; set; }
    }
}