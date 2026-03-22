using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetPanelPremium.Data;
using VetPanelPremium.Models;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;

namespace VetPanelPremium.Controllers
{
    [Authorize] // 🛡️ 2. ASMA KİLİT: Şifresi olmayan buraya adım atamaz!
    public class OwnersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OwnersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Müşteri Portföyü (Listeleme Ekranı)
        public async Task<IActionResult> Index()
        {
            // En son eklenen müşteri en üstte çıksın diye OrderByDescending kullanıyoruz
            var owners = await _context.Owners.OrderByDescending(o => o.Id).ToListAsync();
            return View(owners);
        }

        // 2. Yeni Müşteri Ekleme Ekranını Aç
        public IActionResult Create()
        {
            return View();
        }

        // 3. Formdan Gelen Müşteriyi Veritabanına Yaz
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Owner owner)
        {
            if (ModelState.IsValid)
            {
                _context.Add(owner);
                await _context.SaveChangesAsync();
                
                // Başarı mesajı
                TempData["SuccessMessage"] = $"{owner.FullName} isimli müşteri başarıyla kaydedildi.";
                return RedirectToAction(nameof(Index));
            }
            return View(owner);
        }

        // 4. Müşteri Detay Sayfası (Profil ve Kayıtlı Petler)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            // SİHİRLİ DOKUNUŞ: Müşteriyi çekerken, Include ile ona ait Pet'leri de çekiyoruz (İlerisi için hazır!)
            var owner = await _context.Owners
                .Include(o => o.Pets)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (owner == null) return NotFound();

            return View(owner);
        }

        // 5. Müşteri Düzenleme Ekranını Aç (GET)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var owner = await _context.Owners.FindAsync(id);
            if (owner == null) return NotFound();

            return View(owner);
        }

        // 6. Düzenlenen Müşteriyi Veritabanında Güncelle (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Owner owner)
        {
            if (id != owner.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(owner);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"{owner.FullName} bilgileri başarıyla güncellendi.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Owners.Any(e => e.Id == owner.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(owner);
        }

        // 7. 🚀 MÜŞTERİYİ SİSTEMDEN TAMAMEN SİL (NÜKLEER TEMİZLİK v2.0)
        public async Task<IActionResult> Delete(int id)
        {
            var owner = await _context.Owners.Include(o => o.Pets).FirstOrDefaultAsync(o => o.Id == id);
            
            if (owner != null)
            {
                var ownerAppointments = _context.Appointments.Where(a => a.OwnerId == id).ToList();
                if (ownerAppointments.Any())
                {
                    _context.Appointments.RemoveRange(ownerAppointments);
                }

                if (owner.Pets != null && owner.Pets.Any())
                {
                    var petIds = owner.Pets.Select(p => p.Id).ToList();

                    var petAppointments = _context.Appointments.Where(a => a.PetId != null && petIds.Contains(a.PetId.Value)).ToList();
                    if (petAppointments.Any())
                    {
                        _context.Appointments.RemoveRange(petAppointments);
                    }

                    foreach (var pet in owner.Pets)
                    {
                        var medicalRecords = _context.MedicalRecords.Where(m => m.PetId == pet.Id).ToList();
                        if (medicalRecords.Any())
                        {
                            _context.MedicalRecords.RemoveRange(medicalRecords);
                        }
                    }
                    
                    _context.Pets.RemoveRange(owner.Pets);
                }

                _context.Owners.Remove(owner);
                
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Müşteri ve ona bağlı TÜM hayvanlar, randevular ve tıbbi geçmiş başarıyla buharlaştırıldı!";
            }
            
            return RedirectToAction(nameof(Index));
        }

        // 8. 💰 KASAYA PARA GİRİŞİ (TAHSİLAT ALMA MOTORU)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReceivePayment(int id, decimal amount)
        {
            if (amount <= 0)
            {
                TempData["ErrorMessage"] = "Tahsilat başarısız: Lütfen sıfırdan büyük geçerli bir tutar girin!";
                return RedirectToAction(nameof(Details), new { id = id });
            }

            var owner = await _context.Owners.FindAsync(id);
            if (owner == null) return NotFound();

            // Müşterinin borcundan girilen parayı düş
            owner.DebtBalance -= amount;

            _context.Update(owner);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{amount:C2} tahsilat başarıyla kasaya eklendi. Müşterinin Güncel Bakiyesi: {owner.DebtBalance:C2}";
            
            // İşlem bitince müşterinin profil sayfasına (Details) geri dön
            return RedirectToAction(nameof(Details), new { id = id });
        }
    }
}