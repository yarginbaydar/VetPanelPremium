using System.Collections.Generic;

namespace VetPanelPremium.Models
{
    public class DashboardViewModel
    {
        // Özet Kartları İçin
        public int TotalPets { get; set; }
        public decimal TotalUnpaidDebt { get; set; } // Müşterilerdeki toplam veresiye/alacak
        public int CriticalStockCount { get; set; }
        public int TodayAppointmentsCount { get; set; }

        // Alt Listeler İçin
        public List<Appointment> TodayAppointments { get; set; } = new List<Appointment>();
        public List<Product> CriticalProducts { get; set; } = new List<Product>();
    }
}