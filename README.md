# ⚡ .NET 9.0 Identity + JWT Örnek Uygulama

ASP.NET Core 9 üzerinde geliştirilmiş, ASP.NET Identity ve JWT (JSON Web Token) kullanan modern bir kimlik doğrulama ve yetkilendirme demosu. Uygulama; güvenli oturum yönetimi, rol/claim tabanlı yetkilendirme ve içerik moderasyonu senaryolarını göstermeyi hedefler. Zararlı içerikler Yapay Zeka (Hugging Face) ile işaretlenir ve moderatör onayından sonra yayıma alınır.

## 🎯 Öne Çıkanlar
- Kayıt, giriş ve oturum kapatma akışları
- JWT tabanlı kimlik doğrulama ve claim/rol bazlı yetkilendirme
- İsteğe bağlı refresh token altyapısı ile token yenileme
- ASP.NET Core Identity ile kullanıcı/rol yönetimi
- Entity Framework Core ile veri erişimi
- Katmanlı ve bakımı kolay mimari yaklaşımı
- Hugging Face entegrasyonu ile toksik içerik tespiti + moderasyon kuyruğu
- FluentValidation ile model kuralları ve temiz doğrulama

## 🧰 Teknoloji Yığını
- .NET 9.0 / ASP.NET Core
- ASP.NET Core Identity
- JWT (JSON Web Token)
- Entity Framework Core + SQL Server
- FluentValidation
- Hugging Face (NLP/Moderasyon)

## 🚀 Hızlı Başlangıç
1) Bağımlılıkları indir ve derle
```bash
dotnet restore
dotnet build
```
2) Geliştirme ortamında çalıştır
```bash
dotnet run --project NotikaIdentityEmail/NotikaIdentityEmail.csproj
```
3) Yapılandırma (özet)
- Kullanıcı sırlarıyla ayarla (önerilir):
  - `JwtSettingsKey:Key`
  - (Opsiyonel) `Authentication:Google:*`
  - (Opsiyonel) `Smtp:*`

> Not: Projede global CSRF koruması, güvenli cookie ayarları ve HTTPS/HSTS tercihleri etkinleştirilmiştir.
