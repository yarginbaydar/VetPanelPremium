using Microsoft.AspNetCore.Mvc;
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
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Depo ve Hizmet Listesi (Vitrin)
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Where(p => p.IsActive)
                .OrderBy(p => p.Category)
                .ThenBy(p => p.Name)
                .ToListAsync();
                
            var today = DateTime.Today;
            
            // 🚀 RADAR MOTORU: SKT Analizleri
            // 1. Tarihi çoktan geçmiş ürünlerin sayısı
            ViewBag.ExpiredCount = products.Count(p => p.ExpirationDate.HasValue && p.ExpirationDate.Value.Date < today);
            
            // 2. Tarihinin bitmesine 30 gün veya daha az kalmış ürünlerin sayısı (Sarı Alarm)
            ViewBag.ExpiringSoonCount = products.Count(p => p.ExpirationDate.HasValue && p.ExpirationDate.Value.Date >= today && p.ExpirationDate.Value.Date <= today.AddDays(30));

            return View(products);
        }

        // 2. Yeni Ürün/Hizmet Ekleme Formunu Aç (GET)
        public IActionResult Create()
        {
            return View();
        }

        // 3. Formdan Gelen Ürünü Veritabanına Yaz (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"{product.Name} başarıyla stoka eklendi.";
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // 4. Ürün Düzenleme Ekranını Aç (GET)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            return View(product);
        }

        // 5. Güncellenmiş Ürünü Veritabanına Yaz (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product)
        {
            if (id != product.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Ürün bilgileri başarıyla güncellendi.";
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // 6. HIZLI STOK EKLEME BUTONU İÇİN (POST)
        [HttpPost]
        public async Task<IActionResult> AddStock(int id, int addedStock)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null && addedStock > 0)
            {
                product.StockQuantity += addedStock;
                _context.Update(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"{product.Name} stoğuna {addedStock} adet eklendi! (Yeni Stok: {product.StockQuantity})";
            }
            return RedirectToAction(nameof(Index));
        }

         // 7. Ürünü Sil (Soft Delete - Arşive Kaldır)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                // Veritabanından tamamen silmiyoruz, kasadan ve vitrinden gizliyoruz!
                product.IsActive = false; 
                _context.Update(product);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = $"'{product.Name}' başarıyla listeden kaldırıldı.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}