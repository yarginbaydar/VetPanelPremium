using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Identity; // 🚀 YENİ: MİCROSOFT'UN RESMİ KRİPTO KÜTÜPHANESİ
using System.Security.Claims;
using VetPanelPremium.Data;
using VetPanelPremium.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace VetPanelPremium.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<AdminUser> _hasher; // 🚀 ŞİFRE KIRICI MOTOR

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
            _hasher = new PasswordHasher<AdminUser>(); // Motoru çalıştır
        }

        // 1. GİRİŞ EKRANINI GÖSTER
        [HttpGet]
        public IActionResult Login()
        {
            if (!_context.AdminUsers.Any())
            {
                var newAdmin = new AdminUser {
                    Username = "admin",
                    FullName = "Yargın Hocam",
                    Role = "Başhekim"
                };
                // SİHİRLİ DOKUNUŞ: 123 şifresini veritabanına yazmadan önce Kriptoluyoruz!
                newAdmin.Password = _hasher.HashPassword(newAdmin, "123");
                
                _context.AdminUsers.Add(newAdmin);
                _context.SaveChanges();
            }

            if (User.Identity != null && User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");

            return View();
        }

// 2. ŞİFREYİ KONTROL ET VE İÇERİ AL
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = _context.AdminUsers.FirstOrDefault(u => u.Username == username);

            if (user != null)
            {
                bool isPasswordValid = false;

                // 1. ÖNCE KURTARMA OPERASYONU: Şifre eskiden düz metin olarak kaydedilmişse!
                if (user.Password == password)
                {
                    // Adamı kapıda bırakma! Eski şifreyi hemen gizlice kriptola ve DB'yi güncelle!
                    user.Password = _hasher.HashPassword(user, password);
                    _context.SaveChanges();
                    isPasswordValid = true;
                }
                else
                {
                    // 2. MOTORLA KONTROL: Veritabanındaki şifre zaten Hashlenmişse (veya yanlış şifre girildiyse)
                    try
                    {
                        var result = _hasher.VerifyHashedPassword(user, user.Password, password);
                        if (result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded)
                        {
                            isPasswordValid = true;
                        }
                    }
                    catch (System.FormatException)
                    {
                        // DB'deki şifre "123" ama adam "1234" girdiğinde motor patlamasın diye hatayı yutuyoruz.
                        // isPasswordValid zaten "false" kalacağı için sistem hata verip reddedecek.
                    }
                }

                // EĞER ŞİFRE DOĞRUYSA İÇERİ AL
                if (isPasswordValid)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim("FullName", user.FullName), 
                        new Claim(ClaimTypes.Role, user.Role)
                    };

                    var identity = new ClaimsIdentity(claims, "VetPanelAuth");
                    var principal = new ClaimsPrincipal(identity);

                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = false // Tarayıcı kapanınca çöpe at!
                    };

                    await HttpContext.SignInAsync("VetPanelAuth", principal, authProperties);
                    return RedirectToAction("Index", "Home");
                }
            }

            ViewBag.Error = "Kullanıcı adı veya şifre hatalı!";
            return View();
        }

        // 3. GÜVENLİ ÇIKIŞ YAP
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("VetPanelAuth");
            return RedirectToAction("Login");
        }

        // 4. PROFİL DÜZENLEME EKRANINI AÇ
        [Authorize]
        [HttpGet]
        public IActionResult Profile()
        {
            var username = User.Identity?.Name;
            var user = _context.AdminUsers.FirstOrDefault(u => u.Username == username);
            if (user == null) return NotFound();
            
            return View(user);
        }

        // 5. YENİ ŞİFREYİ KAYDET
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(AdminUser updatedUser)
        {
            if (ModelState.IsValid)
            {
                // 🚀 YENİ: Adamın girdiği yeni şifreyi DB'ye yazmadan hemen önce KRİPTOLA!
                updatedUser.Password = _hasher.HashPassword(updatedUser, updatedUser.Password);

                _context.AdminUsers.Update(updatedUser);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Home"); 
            }
            return View(updatedUser);
        }
    }
}