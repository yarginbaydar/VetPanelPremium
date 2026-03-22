using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VetPanelPremium.Data;
using VetPanelPremium.Models;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace VetPanelPremium.Controllers
{
    [Authorize] // 🛡️ 2. ASMA KİLİT: Şifresi olmayan buraya adım atamaz!
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AppointmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Takvim Ana Ekranı
        public async Task<IActionResult> Index()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Owner)
                .Include(a => a.Pet)
                .OrderBy(a => a.AppointmentDate)
                .ToListAsync();
                
            return View(appointments);
        }

        // 2. Randevu Formunu Aç (GET)
        public async Task<IActionResult> Create()
        {
            ViewBag.Owners = new SelectList(await _context.Owners.ToListAsync(), "Id", "FullName");
            
            // Sayfa ilk açıldığında Pet listesi boş gelsin (Çünkü henüz Müşteri seçilmedi)
            ViewBag.Pets = new SelectList(Enumerable.Empty<SelectListItem>());

            return View();
        }

        // 3. Formdan Gelen Randevuyu Kaydet (POST) - 🚀 RADAR SİSTEMİ EKLENDİ!
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Appointment appointment)
        {
            // 🚀 RADAR SİSTEMİ: DOUBLE BOOKING KONTROLÜ
            bool isConflict = await _context.Appointments.AnyAsync(a => 
                a.Status != "İptal Edildi" && 
                a.AppointmentDate >= appointment.AppointmentDate.AddMinutes(-29) && 
                a.AppointmentDate <= appointment.AppointmentDate.AddMinutes(29)
            );

            if (isConflict)
            {
                ModelState.AddModelError("AppointmentDate", "⚠️ RADAR UYARISI: Bu saat diliminde (veya 30 dk yakınında) başka bir randevu zaten var! Lütfen saati değiştirin.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(appointment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Randevu başarıyla takvime eklendi!";
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.Owners = new SelectList(await _context.Owners.ToListAsync(), "Id", "FullName", appointment.OwnerId);
            
            // Eğer hata verip sayfaya geri dönerse, seçili müşterinin hayvanlarını tekrar doldur
            var pets = await _context.Pets.Where(p => p.OwnerId == appointment.OwnerId).ToListAsync();
            ViewBag.Pets = new SelectList(pets, "Id", "Name", appointment.PetId);
            
            return View(appointment);
        }

        // 🛠️ YENİ: Randevu Düzenle/Ertele Formunu Aç (GET)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            ViewBag.Owners = new SelectList(await _context.Owners.ToListAsync(), "Id", "FullName", appointment.OwnerId);
            
            // Düzenleme ekranında sadece o müşterinin hayvanları gelsin
            var pets = await _context.Pets.Where(p => p.OwnerId == appointment.OwnerId).ToListAsync();
            ViewBag.Pets = new SelectList(pets, "Id", "Name", appointment.PetId);

            return View(appointment);
        }

        // 🛠️ YENİ: Düzenlenen/Ertelenen Randevuyu Kaydet (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Appointment appointment)
        {
            if (id != appointment.Id) return NotFound();

            // 🚀 RADAR: Kendisi hariç diğer randevularla çakışma var mı?
            bool isConflict = await _context.Appointments.AnyAsync(a => 
                a.Id != appointment.Id && // Kendisini kontrolden çıkar
                a.Status != "İptal Edildi" && 
                a.AppointmentDate >= appointment.AppointmentDate.AddMinutes(-29) && 
                a.AppointmentDate <= appointment.AppointmentDate.AddMinutes(29)
            );

            if (isConflict)
            {
                ModelState.AddModelError("AppointmentDate", "⚠️ RADAR UYARISI: Ertelemek istediğiniz saatte başka bir randevu zaten var!");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(appointment);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Randevu başarıyla güncellendi / ertelendi!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Appointments.Any(e => e.Id == appointment.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.Owners = new SelectList(await _context.Owners.ToListAsync(), "Id", "FullName", appointment.OwnerId);
            var pets = await _context.Pets.Where(p => p.OwnerId == appointment.OwnerId).ToListAsync();
            ViewBag.Pets = new SelectList(pets, "Id", "Name", appointment.PetId);
            
            return View(appointment);
        }

        // 4. Randevuyu Tamamlandı Olarak İşaretle
        [HttpPost]
        public async Task<IActionResult> Complete(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                appointment.Status = "Tamamlandı";
                _context.Update(appointment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Randevu başarıyla tamamlandı!";
            }
            
            // 🚀 AKILLI DÖNÜŞ: Hangi sayfadan basıldıysa (Dashboard veya Takvim) oraya geri dön!
            string referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer)) return Redirect(referer);
            
            return RedirectToAction(nameof(Index));
        }

        // 5. Randevuyu İptal Et
        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                appointment.Status = "İptal Edildi";
                _context.Update(appointment);
                await _context.SaveChangesAsync();
                TempData["ErrorMessage"] = "Randevu iptal edildi.";
            }
            
            // 🚀 AKILLI DÖNÜŞ
            string referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer)) return Redirect(referer);

            return RedirectToAction(nameof(Index));
        }

        // 🚀 YENİ EKLENEN GİZLİ RADAR: AJAX İLE MÜŞTERİNİN HAYVANLARINI GETİRİR
        [HttpGet]
        public async Task<JsonResult> GetPetsByOwner(int ownerId)
        {
            var pets = await _context.Pets
                .Where(p => p.OwnerId == ownerId)
                .Select(p => new { value = p.Id, text = p.Name })
                .ToListAsync();

            return Json(pets);
        }
    }
}