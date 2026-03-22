using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Threading.Tasks;
using VetPanelPremium.Data;
using VetPanelPremium.Models;

namespace VetPanelPremium.Controllers
{
    [Authorize] 
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1); // 🚀 DİJİTAL ASİSTAN İÇİN YARININ TARİHİ

            var model = new DashboardViewModel
            {
                // 1. Toplam Kayıtlı Hasta Sayısı
                TotalPets = await _context.Pets.CountAsync(),
                
                // 2. Müşterilerdeki Toplam Alacak (Veresiye) Bakiyesi
                TotalUnpaidDebt = await _context.Owners.SumAsync(o => o.DebtBalance),
                
                // 3. Stoğu 5 ve altına düşen ürün sayısı (Hizmet ve Aşılar ile Silinmişler hariç)
                CriticalStockCount = await _context.Products
                    .CountAsync(p => p.StockQuantity <= 5 && p.Category != "Hizmet" && p.Category != "Aşı" && p.IsActive),
                
                // 4. Sadece BUGÜNE ait ve Bekleyen randevuların sayısı
                TodayAppointmentsCount = await _context.Appointments
                    .CountAsync(a => a.AppointmentDate.Date == today && a.Status == "Bekliyor"),
                
                // 5. Bugünün Randevu Listesi (🎯 İŞTE O KESKİN NİŞANCI FİLTRESİ BURADA!)
                TodayAppointments = await _context.Appointments
                    .Include(a => a.Owner)
                    .Include(a => a.Pet)
                    .Where(a => a.AppointmentDate.Date == today)
                    .OrderBy(a => a.AppointmentDate)
                    .ToListAsync(),
                    
                // 6. Kritik Stok Listesi (En az olanlar en üstte, sadece ilk 5)
                CriticalProducts = await _context.Products
                    .Where(p => p.StockQuantity <= 5 && p.Category != "Hizmet" && p.Category != "Aşı" && p.IsActive)
                    .OrderBy(p => p.StockQuantity)
                    .Take(5) 
                    .ToListAsync()
            };

            // 🚀 7. YARINKİ RANDEVULAR (WhatsApp Asistanı için)
            ViewBag.TomorrowAppointments = await _context.Appointments
                .Include(a => a.Owner)
                .Include(a => a.Pet)
                .Where(a => a.AppointmentDate.Date == tomorrow && a.Status == "Bekliyor")
                .OrderBy(a => a.AppointmentDate)
                .ToListAsync();

            // 🚨 8. RADARIN KÖR NOKTASI: SKT (Son Kullanma Tarihi) Analizi
            ViewBag.ExpiredCount = await _context.Products.CountAsync(p => p.IsActive && p.ExpirationDate.HasValue && p.ExpirationDate.Value.Date < today);
            ViewBag.ExpiringSoonCount = await _context.Products.CountAsync(p => p.IsActive && p.ExpirationDate.HasValue && p.ExpirationDate.Value.Date >= today && p.ExpirationDate.Value.Date <= today.AddDays(30));

            return View(model);
        }
    }
}