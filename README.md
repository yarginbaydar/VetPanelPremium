# 🐾 VetPanel Premium - Veteriner Klinik Yönetim Sistemi (ERP/CRM)

Modern, hızlı ve veri güvenliğini ön planda tutan, veteriner klinikleri için sıfırdan .NET Core mimarisiyle geliştirilmiş kapsamlı bir klinik yönetim (SaaS/MVP) sistemidir.

Hantal masaüstü yazılımlarının aksine; **VetPanel Premium**, kliniklerin randevu süreçlerini, hasta tıbbi geçmişini (EMR), stok/satış operasyonlarını ve finansal takibini tek bir çatı altında, kullanıcı dostu bir arayüzle birleştirir.

## 📸 Ekran Görüntüleri

### 1. Kapsamlı Tıbbi Dosya ve Gelişim Takibi (EMR)
*Hastanın tüm tıbbi geçmişi timeline formatında tutulurken, Chart.js entegrasyonu ile kilo gelişimi görselleştirilir. Arşive yüklenen tahlil ve röntgen sonuçlarına tek tıkla ulaşılır.*

### 2. Akıllı POS Ekranı ve Finans/Borç Yönetimi
*Klinik kasası e-ticaret sepeti mantığıyla çalışır. Stokta olmayan ürünün satışı engellenir. "Veresiye" sistemi ile açık hesap çalışan müşterilerin borç takibi dinamik olarak yapılır.*

### 3. Çakışma Engelleyici (Radar) Randevu Sistemi
*Özel zaman hesaplama algoritması sayesinde, aynı saate veya çok yakın saatlere randevu verilmesi engellenerek operasyonel hataların önüne geçilir.*

---

## 🚀 Temel Özellikler (MVP)

* **🛡️ White-Label (Markalaşma) Altyapısı:** Sistem tek bir kliniğe özel değil, ayarlar sayfasından girilen verilerle anında herhangi bir kliniğin kurumsal kimliğine bürünecek şekilde tasarlandı.
* **🩺 Dijital Tıbbi Arşiv (EMR):** Şikayet, teşhis, uygulanan tedavi ve eklenti dosyalarının (PDF/Görsel) tutulduğu güvenli kayıt sistemi.
* **💰 Dinamik Stok & Veresiye Takibi:** Satılan ürünlerin anında stoktan düşmesi ve nakit/kredi kartı haricinde "Açık Hesap" tahsilat yapabilme altyapısı.
* **🖨️ Hayalet Yazıcı Motoru:** Sayfa yenilenmeden, yeni sekme açmadan doğrudan 80mm termal fiş dökümü alabilen entegre JavaScript yazdırma motoru.
* **🔒 Güvenlik Zırhı:** ASP.NET Core Identity ile korunan rotalar, çakışma önleyici algoritmalar ve XSS/CSRF korumaları.

## 💻 Kullanılan Teknolojiler

**Backend:**
* C# 11 & ASP.NET Core MVC (.NET 8/9)
* Entity Framework Core (Code-First)
* ASP.NET Core Identity (Kimlik Doğrulama)
* LINQ (Veri Manipülasyonu)

**Frontend:**
* HTML5, CSS3, Bootstrap 5
* JavaScript & Chart.js (Veri Görselleştirme)
* DataTables (Dinamik Tablolama)





