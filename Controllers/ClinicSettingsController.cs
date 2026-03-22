using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using VetPanelPremium.Data;
using VetPanelPremium.Models;
using Microsoft.AspNetCore.Authorization;

namespace VetPanelPremium.Controllers
{
    [Authorize] // 🛡️ 2. ASMA KİLİT: Şifresi olmayan buraya adım atamaz!
    public class ClinicSettingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClinicSettingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Ayarlar Sayfasını Aç (Varsa getir, yoksa boş form ver)
        public async Task<IActionResult> Index()
        {
            var settings = await _context.ClinicSettings.FirstOrDefaultAsync();
            
            // Eğer veritabanında henüz hiç ayar yoksa, boş bir tane oluşturup ekrana yolla
            if (settings == null)
            {
                settings = new ClinicSetting(); 
            }
            
            return View(settings);
        }

        // POST: Ayarları Kaydet veya Güncelle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ClinicSetting settings)
        {
            if (ModelState.IsValid)
            {
                if (settings.Id == 0)
                {
                    // ID 0 ise demek ki ilk defa kaydediliyor
                    _context.ClinicSettings.Add(settings);
                }
                else
                {
                    // ID varsa mevcut olanı güncelliyoruz
                    _context.ClinicSettings.Update(settings);
                }
                
                await _context.SaveChangesAsync();
                
                // 🚀 BU MESAJI EKRANDA GÖSTERECEK KODU HTML'E EKLEDİK
                TempData["SuccessMessage"] = "Klinik ayarları başarıyla güncellendi!";
                
                return RedirectToAction(nameof(Index));
            }
            
            return View(settings);
        }
    }
}