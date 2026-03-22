using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using VetPanelPremium.Data;

namespace VetPanelPremium.ViewComponents
{
    public class NotificationBellViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public NotificationBellViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var today = DateTime.Today;

            // 1. SKT'si Geçen veya Yaklaşanlar
            int expiredCount = await _context.Products.CountAsync(p => p.IsActive && p.ExpirationDate.HasValue && p.ExpirationDate.Value.Date < today);
            int expiringSoonCount = await _context.Products.CountAsync(p => p.IsActive && p.ExpirationDate.HasValue && p.ExpirationDate.Value.Date >= today && p.ExpirationDate.Value.Date <= today.AddDays(30));

            // 2. Stoğu 5'in altına düşen kritik ürünler (Aşı/Hizmet hariç)
            int criticalStockCount = await _context.Products.CountAsync(p => p.IsActive && p.StockQuantity <= 5 && p.Category != "Hizmet" && p.Category != "Aşı");

            // 3. Bugünün Bekleyen Randevuları
            int todayAppointmentsCount = await _context.Appointments.CountAsync(a => a.AppointmentDate.Date == today && a.Status == "Bekliyor");

            // Toplam Bildirim Sayısı
            int totalNotifications = expiredCount + expiringSoonCount + criticalStockCount + todayAppointmentsCount;

            ViewBag.ExpiredCount = expiredCount;
            ViewBag.ExpiringSoonCount = expiringSoonCount;
            ViewBag.CriticalStockCount = criticalStockCount;
            ViewBag.TodayAppointmentsCount = todayAppointmentsCount;
            ViewBag.TotalNotifications = totalNotifications;

            return View();
        }
    }
}