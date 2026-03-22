using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VetPanelPremium.Data;
using VetPanelPremium.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting; // 🚀 Sunucu klasörlerini bulmak için
using System.IO; // 🚀 Dosya işlemleri için
using System;
using System.Linq;
using System.Threading.Tasks;

namespace VetPanelPremium.Controllers
{
    [Authorize] 
    public class PetsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env; // 🚀 HARİTA OKUYUCU

        public PetsController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env; // Haritayı sisteme yükle
        }

      // 1. LİSTELEME EKRANI (🚀 ARAMA MOTORU EKLENDİ - UYARILAR SİLİNDİ)
        public async Task<IActionResult> Index(string searchString)
        {
            // Arama kutusunda yazılanı sayfa yenilendiğinde kaybetmemek için ekranda tutuyoruz
            ViewData["CurrentFilter"] = searchString;

            // Veritabanı sorgusunu başlat
            var petsQuery = _context.Pets.Include(p => p.Owner).AsQueryable();

            // Eğer arama kutusuna bir şey yazılmışsa, süzgeci devreye sok!
            if (!string.IsNullOrEmpty(searchString))
            {
                // DİKKAT: Boş olabilecek alanların (Breed, ChipNumber) başına "null değilse" (p.Breed != null) kontrolü ekledik!
                petsQuery = petsQuery.Where(p => 
                    p.Name.Contains(searchString) || 
                    p.Species.Contains(searchString) || 
                    (p.Breed != null && p.Breed.Contains(searchString)) || 
                    (p.ChipNumber != null && p.ChipNumber.Contains(searchString)) ||
                    (p.Owner != null && p.Owner.FullName.Contains(searchString))
                );
            }

            // Süzülmüş listeyi getir
            var pets = await petsQuery.OrderByDescending(p => p.Id).ToListAsync();
                
            return View(pets);
        }

        // 2. YENİ KAYIT EKRANI (GET)
        public async Task<IActionResult> Create(int? ownerId)
        {
            ViewBag.OwnerId = new SelectList(await _context.Owners.ToListAsync(), "Id", "FullName", ownerId);
            return View();
        }

        // 3. YENİ KAYDET (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Pet pet)
        {
            if (ModelState.IsValid)
            {
                // 🚀 DOSYA YÜKLEME MOTORU
                if (pet.TahlilDosyasi != null && pet.TahlilDosyasi.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "tahliller");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                    
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + pet.TahlilDosyasi.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await pet.TahlilDosyasi.CopyToAsync(fileStream);
                    }
                    
                    pet.TahlilDosyaYolu = "/uploads/tahliller/" + uniqueFileName;
                }

                _context.Add(pet);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"{pet.Name} kliniğe başarıyla kaydedildi.";
                
                return RedirectToAction("Details", "Owners", new { id = pet.OwnerId });
            }
            ViewBag.OwnerId = new SelectList(await _context.Owners.ToListAsync(), "Id", "FullName", pet.OwnerId);
            return View(pet);
        }

        // 4. DÜZENLEME EKRANI (GET)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var pet = await _context.Pets.FindAsync(id);
            if (pet == null) return NotFound();
            
            ViewBag.OwnerId = new SelectList(await _context.Owners.ToListAsync(), "Id", "FullName", pet.OwnerId);
            return View(pet);
        }

        // 5. 🚀 DÜZENLEMEYİ KAYDET (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Pet pet)
        {
            if (id != pet.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // 🚀 YENİ DOSYA YÜKLENDİ Mİ KONTROLÜ
                    if (pet.TahlilDosyasi != null && pet.TahlilDosyasi.Length > 0)
                    {
                        string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "tahliller");
                        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + pet.TahlilDosyasi.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await pet.TahlilDosyasi.CopyToAsync(fileStream);
                        }

                        pet.TahlilDosyaYolu = "/uploads/tahliller/" + uniqueFileName;
                    }

                    // VERİTABANINI GÜNCELLE
                    _context.Update(pet);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Hasta bilgileri (ve varsa tahlilleri) başarıyla güncellendi.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Pets.Any(e => e.Id == pet.Id)) return NotFound();
                    else throw;
                }
                
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.OwnerId = new SelectList(await _context.Owners.ToListAsync(), "Id", "FullName", pet.OwnerId);
            return View(pet);
        }

        // 6. HASTAYI SİSTEMDEN TAMAMEN SİL (DERİN TEMİZLİK)
        public async Task<IActionResult> Delete(int id)
        {
            var pet = await _context.Pets.FindAsync(id);
            if (pet != null)
            {
                var appointments = _context.Appointments.Where(a => a.PetId == id).ToList();
                if (appointments.Any())
                {
                    _context.Appointments.RemoveRange(appointments);
                }

                var medicalRecords = _context.MedicalRecords.Where(m => m.PetId == id).ToList();
                if (medicalRecords.Any())
                {
                    _context.MedicalRecords.RemoveRange(medicalRecords);
                }

                _context.Pets.Remove(pet);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Hasta ve ona bağlı tüm randevu/tıbbi geçmiş başarıyla silindi!";
            }
            
            return RedirectToAction(nameof(Index));
        }
    }
}