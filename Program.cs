using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection; // 🚀 1. YENİ KÜTÜPHANEYİ EKLE

var builder = WebApplication.CreateBuilder(args);

// Veritabanı köprüsünü (ApplicationDbContext) ve SQL Server'ı sisteme dahil ediyoruz
builder.Services.AddDbContext<VetPanelPremium.Data.ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllersWithViews();

// 🚀 2. SİHİRLİ DOKUNUŞ: Sunucu her kapandığında eski kimlikleri GECERSİZ KIL!
builder.Services.AddDataProtection()
    .UseEphemeralDataProtectionProvider(); 

// GÜVENLİK DUVARI: Sadece Tarayıcı Açıkken Çalışan Kasa Mantığı
builder.Services.AddAuthentication("VetPanelAuth")
    .AddCookie("VetPanelAuth", options =>
    {
        options.LoginPath = "/Auth/Login"; 
        options.LogoutPath = "/Auth/Logout";
        options.Cookie.Name = "VetPanelPremium.Auth";
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// 🚀 SİHİRLİ ŞALTER BURADA: Önce kimlik sor, sonra yetki ver!
app.UseAuthentication(); 
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();