using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq; 
using System.Threading.Tasks;
using VetPanelPremium.Data;
using VetPanelPremium.Models;
using System;
using Microsoft.AspNetCore.Authorization;

namespace VetPanelPremium.Controllers
{
    [Authorize] // 🛡️ 2. ASMA KİLİT: Şifresi olmayan buraya adım atamaz!
    public class MedicalRecordsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MedicalRecordsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Hastanın Tıbbi Dosyasını (Geçmişini) Aç
        public async Task<IActionResult> Index(int? petId)
        {
            if (petId == null) return NotFound();

            var pet = await _context.Pets
                .Include(p => p.Owner)
                .FirstOrDefaultAsync(p => p.Id == petId);
                
            if (pet == null) return NotFound();
            
            ViewBag.PetInfo = pet;

            // Tıbbi kayıtları getir
            var records = await _context.MedicalRecords
                .Include(m => m.Pet)
                .Where(m => m.PetId == petId)
                .OrderByDescending(m => m.VisitDate)
                .ToListAsync();

            // 🚀 YENİ: KİLO GELİŞİM GRAFİĞİ İÇİN VERİ HAZIRLIĞI (CHART.JS)
            var weightRecords = records.Where(r => r.Weight.HasValue).OrderBy(r => r.VisitDate).ToList();
            
            // 🛠️ DÜZELTME BURADA: .Value yerine .GetValueOrDefault() kullanarak o Null uyarısını tamamen susturduk!
            ViewBag.ChartLabels = weightRecords.Select(r => r.VisitDate.ToString("dd MMM yyyy")).ToArray();
            ViewBag.ChartData = weightRecords.Select(r => r.Weight.GetValueOrDefault()).ToArray();

            return View(records);
        }

        // 2. Yeni Muayene Ekleme Formunu Aç (GET)
        public async Task<IActionResult> Create(int? petId)
        {
            if (petId == null) return NotFound();

            var pet = await _context.Pets.FindAsync(petId);
            if (pet == null) return NotFound();

            ViewBag.PetName = pet.Name;
            
            var record = new MedicalRecord { PetId = pet.Id, VisitDate = DateTime.Now };
            return View(record);
        }

        // 3. Formdan Gelen Muayeneyi Veritabanına Yaz (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MedicalRecord medicalRecord)
        {
            // 🚀 1. ÇÖZÜM: OBEZİTE TUZAĞI (Nokta / Virgül Hatası)
            // Model hata vermesin diye formdaki ham veriyi havada yakalayıp düzeltiyoruz.
            var rawWeight = Request.Form["Weight"].ToString();
            if (!string.IsNullOrEmpty(rawWeight))
            {
                rawWeight = rawWeight.Replace(".", ","); // Noktayı virgüle zorla
                if (double.TryParse(rawWeight, System.Globalization.NumberStyles.Any, new System.Globalization.CultureInfo("tr-TR"), out double parsedWeight))
                {
                    medicalRecord.Weight = parsedWeight;
                    ModelState.Remove("Weight"); // Eğer format yüzünden hata verdiyse o hatayı sil.
                }
            }

            if (ModelState.IsValid)
            {
                var pet = await _context.Pets.Include(p => p.Owner).FirstOrDefaultAsync(p => p.Id == medicalRecord.PetId);
                if (pet == null || pet.Owner == null) return NotFound();

                // 🚀 2. ÇÖZÜM: SİNSİ BUG (Radarı Delen Randevular)
                if (medicalRecord.NextControlDate.HasValue)
                {
                    var targetDate = medicalRecord.NextControlDate.Value;
                    bool isConflict = await _context.Appointments.AnyAsync(a => 
                        a.Status != "İptal Edildi" && 
                        a.AppointmentDate >= targetDate.AddMinutes(-29) && 
                        a.AppointmentDate <= targetDate.AddMinutes(29)
                    );

                    if (isConflict)
                    {
                        // 🛑 Eğer çakışma varsa, veritabanına HİÇBİR ŞEY YAZMADAN motoru durdur ve uyarı ver!
                        ModelState.AddModelError("NextControlDate", "⚠️ RADAR UYARISI: Seçtiğiniz kontrol saatinde (veya 30 dk yakınında) başka bir randevu zaten var! Lütfen saati değiştirin.");
                        ViewBag.PetName = pet.Name;
                        return View(medicalRecord);
                    }
                }

                // --------- RADAR TEMİZ, İŞLEMLERE BAŞLA ---------

                if (medicalRecord.Weight.HasValue)
                {
                    pet.Weight = medicalRecord.Weight;
                    _context.Update(pet);
                }

                decimal muayeneUcreti = 500m; 
                decimal toplamTutar = muayeneUcreti;
                string feedbackMessage = $"Standart Muayene ({muayeneUcreti:C2}) eklendi. ";

                if (!string.IsNullOrEmpty(medicalRecord.Treatment))
                {
                    var treatmentText = medicalRecord.Treatment.ToLower();
                    
                    var activeProducts = await _context.Products
                        .Where(p => p.IsActive && (p.Category == "Aşı" || p.Category == "İlaç"))
                        .ToListAsync();

                    var usedProducts = activeProducts.Where(p => treatmentText.Contains(p.Name.ToLower())).ToList();

                    foreach (var product in usedProducts)
                    {
                        if (product.StockQuantity > 0)
                        {
                            product.StockQuantity -= 1;
                            _context.Update(product);
                            
                            toplamTutar += product.SalePrice;
                            feedbackMessage += $"+ {product.Name} kullanıldı ({product.SalePrice:C2}). [Stok: -1] ";
                        }
                        else
                        {
                            feedbackMessage += $"[DİKKAT: {product.Name} depoda kalmamış, faturaya eklenmedi!] ";
                        }
                    }
                }

                pet.Owner.DebtBalance += toplamTutar;
                _context.Update(pet.Owner);
                feedbackMessage += $"👉 Toplam {toplamTutar:C2} borç kaydedildi. ";

                _context.Add(medicalRecord);
                
                if (medicalRecord.NextControlDate.HasValue)
                {
                    var appointment = new Appointment
                    {
                        OwnerId = pet.OwnerId,
                        PetId = pet.Id,
                        AppointmentDate = medicalRecord.NextControlDate.GetValueOrDefault(),
                        Reason = "Kontrol",
                        Notes = $"{pet.Name} isimli hastanın '{medicalRecord.Diagnosis}' teşhisi sonrası planlanan otomatik kontrolüdür.",
                        Status = "Bekliyor"
                    };
                        
                    _context.Appointments.Add(appointment);
                    feedbackMessage += "(Kontrol randevusu takvime işlendi!)";
                }

                await _context.SaveChangesAsync(); 

                TempData["SuccessMessage"] = feedbackMessage;
                return RedirectToAction(nameof(Index), new { petId = medicalRecord.PetId });
            }
            
            // Eğer bir model hatası varsa sayfa çökmesin diye Pet ismini tekrar dolduruyoruz
            var petForView = await _context.Pets.FindAsync(medicalRecord.PetId);
            ViewBag.PetName = petForView?.Name;
            
            return View(medicalRecord);
        }

        

    }
}