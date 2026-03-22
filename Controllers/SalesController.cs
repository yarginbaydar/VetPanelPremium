using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VetPanelPremium.Data;
using VetPanelPremium.Models;
using Microsoft.AspNetCore.Authorization;
using System.Linq;

namespace VetPanelPremium.Controllers
{
    [Authorize] // 🛡️ 2. ASMA KİLİT: Şifresi olmayan buraya adım atamaz!
    public class SalesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SalesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Kasa Geçmişi
        public async Task<IActionResult> Index()
        {
            var invoices = await _context.Invoices
                .Include(i => i.Owner)
                .OrderByDescending(i => i.InvoiceDate)
                .ToListAsync();
            return View(invoices);
        }

        // 2. GELİŞMİŞ POS EKRANINI AÇ (SEPETLİ)
        public async Task<IActionResult> POS()
        {
            var products = await _context.Products
                .Where(p => p.IsActive && (p.StockQuantity > 0 || p.Category == "Hizmet" || p.Category == "Aşı"))
                .ToListAsync();

            ViewBag.ProductsList = products; 
            ViewBag.Owners = new SelectList(await _context.Owners.ToListAsync(), "Id", "FullName");
            
            return View();
        }

        // 3. SEPETİ TAHSİL ET (STOK KORUMALI)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(int[] productIds, int[] quantities, int? ownerId, string paymentMethod)
        {
            if (productIds == null || productIds.Length == 0)
            {
                TempData["ErrorMessage"] = "Sepet boş olamaz!";
                return RedirectToAction(nameof(POS));
            }

            // 🛡️ 1. GÜVENLİK DUVARI: FATURA KESİLMEDEN ÖNCE STOK KONTROLÜ YAP!
            for (int i = 0; i < productIds.Length; i++)
            {
                var productToCheck = await _context.Products.FindAsync(productIds[i]);
                if (productToCheck != null && productToCheck.Category != "Hizmet" && productToCheck.Category != "Aşı")
                {
                    if (productToCheck.StockQuantity < quantities[i])
                    {
                        // Eğer istenen adet stoktan fazlaysa faturayı İPTAL ET ve uyarı ver!
                        TempData["ErrorMessage"] = $"HATA: Satış İptal Edildi! '{productToCheck.Name}' ürününden stokta sadece {productToCheck.StockQuantity} adet var.";
                        return RedirectToAction(nameof(POS));
                    }
                }
            }

            // 2. STOKLAR ONAYLANDI -> FATURAYI OLUŞTUR
            var invoice = new Invoice
            {
                OwnerId = ownerId,
                InvoiceDate = DateTime.Now,
                PaymentMethod = paymentMethod,
                TotalAmount = 0 
            };
            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync(); 

            decimal grandTotal = 0;

            // 3. ÜRÜNLERİ SEPETE EKLE VE STOKTAN DÜŞ
            for (int i = 0; i < productIds.Length; i++)
            {
                var product = await _context.Products.FindAsync(productIds[i]);
                if (product != null)
                {
                    int qty = quantities[i];
                    decimal lineTotal = product.SalePrice * qty;
                    grandTotal += lineTotal;

                    _context.InvoiceLines.Add(new InvoiceLine
                    {
                        InvoiceId = invoice.Id,
                        ProductId = product.Id,
                        Quantity = qty,
                        UnitPrice = product.SalePrice,
                        LineTotal = lineTotal
                    });

                    // Stoktan Düş
                    if (product.Category != "Hizmet" && product.Category != "Aşı")
                    {
                        product.StockQuantity -= qty;
                        _context.Update(product);
                    }
                }
            }

            // 4. TOPLAMI VE BORCU GÜNCELLE
            invoice.TotalAmount = grandTotal;
            _context.Update(invoice);

            if (paymentMethod == "Veresiye" && ownerId != null)
            {
                var owner = await _context.Owners.FindAsync(ownerId);
                if (owner != null)
                {
                    owner.DebtBalance += grandTotal;
                    _context.Update(owner);
                }
            }

            await _context.SaveChangesAsync(); 
            TempData["SuccessMessage"] = "Tahsilat başarıyla tamamlandı ve stoklar güncellendi!";
            return RedirectToAction(nameof(Index));
        }

        // 4. Veresiye Defteri
        public async Task<IActionResult> Debtors()
        {
            var debtors = await _context.Owners.Where(o => o.DebtBalance > 0).OrderByDescending(o => o.DebtBalance).ToListAsync();
            return View(debtors);
        }

        // 5. Tahsilat İşlemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PayDebt(int ownerId, decimal amount)
        {
            var owner = await _context.Owners.FindAsync(ownerId);
            if (owner != null && amount > 0)
            {
                owner.DebtBalance -= amount;
                if (owner.DebtBalance < 0) owner.DebtBalance = 0; 
                _context.Update(owner);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"{owner.FullName} isimli müşteriden {amount:C} tahsil edildi.";
            }
            return RedirectToAction(nameof(Debtors));
        }

        // 🚀 FİŞ YAZDIRMA MOTORU (PREMİUM VERİLER EKLENDİ)
        public async Task<IActionResult> Print(int id)
        {
            // Faturayı, Müşteriyi ve Faturanın İçindeki Satın Alınan Ürünleri Getirir
            var invoice = await _context.Invoices
                .Include(i => i.Owner)
                .Include(i => i.InvoiceLines!) 
                    .ThenInclude(ii => ii.Product)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
            {
                return NotFound("Fatura bulunamadı!");
            }

            // 1. Fişin tepesine basılacak Klinik Adını ayarlardan çekiyoruz
            var clinicSetting = await _context.ClinicSettings.FirstOrDefaultAsync();
            ViewBag.ClinicName = clinicSetting != null ? clinicSetting.ClinicName : "VetPanel Premium";
            ViewBag.ClinicPhone = clinicSetting?.PhoneNumber ?? "Telefon Kayıtlı Değil"; // Alta yazmak için telefonu da çektik

            // 2. Müşterinin Evcil Hayvanlarını (Pet) buluyoruz
            if (invoice.OwnerId != null)
            {
                ViewBag.Pets = await _context.Pets.Where(p => p.OwnerId == invoice.OwnerId).ToListAsync();
            }

            // 3. Fişi kesen doktorun (O anki kullanıcının) adını alıyoruz
            ViewBag.DoctorName = User.Claims.FirstOrDefault(c => c.Type == "FullName")?.Value ?? "Nöbetçi Hekim";

            return View(invoice);
        }
    }
}