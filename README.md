# SmartTour – Akıllı Tatil Planlama Otomasyonu
## Proje Raporu

**Geliştirme Ortamı:** Visual Studio / .NET, C# Windows Forms  
**Veritabanı:** PostgreSQL (Supabase Bulut)  
**Tarih:** Haziran 2026

---

# GRUP BİLGİSİ

| # | Ad Soyad | Görev | Öğrenci No |
|---|----------|-------|------------|
| 1 | Sabri Duruk | Veritabanı & DataAccess Katmanı | 032290020 |
| 2 | Mehmet Kalbişen | Form Tasarımları & UI | 032390011 |
| 3 | Efe Tutucu | İş Mantığı & Algoritmalar | 032390034 |


---

# 1. PROJE HAKKINDA DETAYLI BİLGİ (TANITIM)

## 1.1 Projenin Amacı

SmartTour, kullanıcıların belirlediği bütçe, şehir, gece sayısı ve sezon bilgisine göre **otomatik tatil planı öneren** bir masaüstü uygulamasıdır. Ulaşım, konaklama ve gezilecek yer fiyatlarını veritabanında tutarak, bütçeye en uygun kombinasyonları algoritmik olarak hesaplar ve birden fazla alternatif plan sunar.

## 1.2 Temel Özellikler

- **Otomatik Plan Önerme** — Greedy algoritma ile bütçeye uygun 5 farklı plan üretme
- **Manuel Planlama** — Kullanıcının kendi tercihlerini seçerek plan oluşturması
- **Sezon Sistemi** — Yaz (+%40), Bahar (normal), Kış (-%20) fiyat çarpanları
- **Günlük Program** — Gezilecek yerlerin sabah/öğle/akşam olarak günlere dağıtılması
- **Admin Paneli** — Şehir, ulaşım, konaklama ve yer verilerinin CRUD yönetimi
- **Plan CRUD** — Planları kaydetme, görüntüleme, güncelleme ve silme

## 1.3 Kullanılan Teknolojiler

| Teknoloji | Açıklama |
|---|---|
| C# (.NET) | Ana programlama dili |
| Windows Forms | Masaüstü UI framework'ü |
| PostgreSQL / Supabase | Bulut veritabanı |
| Npgsql | .NET PostgreSQL sağlayıcısı |

## 1.4 Proje Katman Mimarisi

```
SmartTour/
├── Models/           → Veri modelleri (Sehir, Ulasim, Konaklama, GezilecekYer, TurPlani)
├── DataAccess/       → Veritabanı erişimi (DatabaseHelper, Repositories)
├── BusinessLogic/    → Hesaplama motoru (TurPlanlamaServisi)
├── Forms/            → 7 kullanıcı arayüzü formu + 1 alt form
└── Program.cs        → Uygulama giriş noktası
```

<img width="371" height="573" alt="Ekran görüntüsü 2026-06-04 173334" src="https://github.com/user-attachments/assets/c86e6dfb-d184-4cc6-82a8-e933c9855030" />


---

# 2. VERİTABANI YAPISI VE PROGRAM İLE İLİŞKİSİ

## 2.1 Veritabanı Şeması (6 Tablo)

| Tablo | Sütunlar | Açıklama |
|---|---|---|
| **Sehirler** | `SehirID` (PK), `SehirAdi` | 5 şehir: İstanbul, Antalya, İzmir, Kapadokya, Bodrum |
| **Ulasim** | `UlasimID` (PK), `SehirID` (FK), `UlasimTuru`, `Fiyat` | Otobüs, Uçak, Tren seçenekleri |
| **Konaklama** | `KonaklamaID` (PK), `SehirID` (FK), `KonaklamaAdi`, `KonaklamaTuru`, `GeceFiyat` | Otel, Pansiyon, Apart, Hostel vb. |
| **GezilecekYerler** | `YerID` (PK), `SehirID` (FK), `YerAdi`, `ZiyaretUcreti`, `Aciklama` | Turistik noktalar ve giriş ücretleri |
| **TurPlanlari** | `PlanID` (PK), `SehirID`/`UlasimID`/`KonaklamaID` (FK), `GeceSayisi`, `Butce`, `ToplamMaliyet`, `Sezon`, `SehirIciUlasim`, `SehirIciMaliyet`, `OlusturmaTarihi` | Kaydedilen planlar |
| **PlanGezilecekYerler** | `PlanID` (FK), `YerID` (FK) — Bileşik PK, CASCADE silme | Çoka-çok ilişki tablosu |

## 2.2 ER Diyagramı

```mermaid
erDiagram
    Sehirler ||--o{ Ulasim : "1:N"
    Sehirler ||--o{ Konaklama : "1:N"
    Sehirler ||--o{ GezilecekYerler : "1:N"
    Sehirler ||--o{ TurPlanlari : "1:N"
    Ulasim ||--o{ TurPlanlari : "1:N"
    Konaklama ||--o{ TurPlanlari : "1:N"
    TurPlanlari ||--o{ PlanGezilecekYerler : "1:N"
    GezilecekYerler ||--o{ PlanGezilecekYerler : "1:N"
```

<img width="1509" height="848" alt="WhatsApp Image 2026-06-04 at 18 28 21" src="https://github.com/user-attachments/assets/1e9cf042-772e-4e05-a880-6a7d355a7bba" />
<img width="890" height="464" alt="WhatsApp Image 2026-06-04 at 18 27 53" src="https://github.com/user-attachments/assets/81d5a555-dfd7-46d9-b573-0c490640f9a7" />


## 2.3 Program ile Veritabanı İlişkisi

### DatabaseHelper.cs
- **Bağlantı yönetimi:** Önce `db_config.txt` → yoksa Supabase varsayılan → başarısızsa BaglantiAyarlariForm açılır (3 deneme)
- **`InitializeDatabase()`:** Uygulama başlangıcında tabloları oluşturur, seed data ekler ve migration'ları çalıştırır
- **Migration sistemi:** Eski veritabanlarına yeni sütunları (`GeceSayisi`, `Butce`, `Sezon` vb.) otomatik ekler

### Repository Pattern (Repositories.cs)
Her tablo için ayrı Repository sınıfı veritabanı işlemlerini soyutlar:

| Repository | Metotlar |
|---|---|
| `SehirRepository` | `GetAll()` |
| `UlasimRepository` | `GetBySehirId(int)` |
| `KonaklamaRepository` | `GetBySehirId(int)` |
| `GezilecekYerRepository` | `GetBySehirId(int)` |
| `TurPlaniRepository` | `Save()`, `GetAll()`, `Update()`, `Delete()` |

`TurPlaniRepository.GetAll()` 4 tabloyu JOIN ederek plan bilgilerini getirir, ardından her plan için gezilecek yerleri ayrı sorguyla yükler.


<img width="389" height="245" alt="WhatsApp Image 2026-06-04 at 18 33 28" src="https://github.com/user-attachments/assets/0ca1a674-cf0b-4632-80b3-547ce5e93968" />
<img width="359" height="154" alt="WhatsApp Image 2026-06-04 at 18 32 36" src="https://github.com/user-attachments/assets/5d74d58c-d2c5-4b56-b757-d0a3ab643772" />


---

# 3. SİSTEM İLE KULLANICI ARAYÜZLERİNİN AYRI AYRI ANLATIMI

## 3.1 Sistem Tarafı

### Uygulama Başlatma (Program.cs)
1. Windows Forms altyapısı başlatılır
2. `DatabaseHelper.InitializeDatabase()` → veritabanı bağlantısı, tablo oluşturma, seed data
3. Başarılıysa `AnaSayfaForm` açılır; hata olursa mesaj gösterilip uygulama kapanır

### İş Mantığı (TurPlanlamaServisi.cs)

**Sezon Çarpanı:** Yaz → ×1.4 | Bahar → ×1.0 | Kış → ×0.8

**Maliyet Formülü:**
```
Toplam = (Ulaşım × Sezon) + (Konaklama/gece × Sezon × Gece) + Σ(Gezi Ücretleri) + (Şehiriçi × Gece)
```

**Şehir İçi Ulaşım:** Toplu Taşıma 50₺/gün | Taksi 300₺/gün | Araç Kiralama 900₺/gün

**Otomatik Plan Önerme (Greedy):** Bütçeye uyan en ucuz ulaşım → en kaliteli konaklama → bütçeye göre şehir içi ulaşım → ucuzdan pahalıya gezilecek yerleri sığdır

**Çoklu Plan Üretme:** Tüm (ulaşım × konaklama) kombinasyonlarını dener, ulaşım türüne göre gruplar, her gruptan medyan fiyatlı planı seçer, 5 plana tamamlar

**Günlük Program:** Gezilecek yerleri günlere dengeli dağıtır, sabah/öğle/akşam zaman dilimi atar

---

## 3.2 Kullanıcı Arayüzü (8 Form)

### Uygulama Akış Şeması

```
AnaSayfaForm (giriş)
    ├─► [Otomatik] PlanSecimForm ──► SonucForm
    ├─► [Manuel]   TercihlerForm ──► SonucForm
    ├─► KayitliPlanlarForm ──► PlanGuncelleForm
    └─► AdminPanelForm
         └── BaglantiAyarlariForm (hata durumunda otomatik açılır)
```

---

### 3.2.1 AnaSayfaForm — Ana Sayfa
Uygulamanın giriş ekranı. Şehir seçimi, bütçe, gece sayısı ve sezon girişi yapılır.
- Mavi gradient header (fare hareketi ile renk değişir)
- Şehir arama/filtreleme, binlik ayraçlı bütçe girişi
- 4 buton: Plan Öner (yeşil), Kayıtlı Planlar (mavi), Manuel Planlama (gri), Yönetici (turuncu)

<img width="858" height="690" alt="Ekran görüntüsü 2026-06-04 184806" src="https://github.com/user-attachments/assets/257d8163-9579-4824-9dbe-b58a4304593e" />

### 3.2.2 TercihlerForm — Manuel Planlama
Kullanıcının tüm tercihleri elle seçtiği form.
- Ulaşım, konaklama ve gezilecek yerler için **isim arama + fiyat aralığı filtresi**
- Gezilecek yerler CheckedListBox ile çoklu seçim; ToolTip ile açıklama gösterimi
- Sezon ve şehir içi ulaşım yan yana seçim kutuları

<img width="626" height="827" alt="Ekran görüntüsü 2026-06-04 184954" src="https://github.com/user-attachments/assets/0f88f25d-d256-4d9b-9b9a-61b7ff208bcb" />

### 3.2.3 PlanSecimForm — Plan Karşılaştırma
Algoritmanın önerdiği 5 planın listelendiği ve düzenlendiği form.
- Sol: Plan listesi | Sağ: Seçili planın maliyet dökümü + günlük ajanda
- Alt bölüm: Ulaşım/konaklama/gezilecek yer değiştirme + 🔄 Güncelle butonu
- 💾 Kaydet ve ← Geri butonları

<img width="1246" height="739" alt="Ekran görüntüsü 2026-06-04 185142" src="https://github.com/user-attachments/assets/1cc712d6-f430-4511-ae97-015cb8ea4f88" />

### 3.2.4 SonucForm — Plan Sonuç Ekranı
Planın maliyet dökümü ve günlük seyahat ajandası.
- ASCII çerçeveli detaylı maliyet raporu
- Bütçe durumu: ✅ Bütçeye Uygun (yeşil) / ⚠️ Bütçe Aşıldı (kırmızı)
- Çift tıklama ile panoya kopyalama özelliği

<img width="626" height="604" alt="Ekran görüntüsü 2026-06-04 210253" src="https://github.com/user-attachments/assets/fc7b68af-11e2-414f-a290-2513e1c77212" />

### 3.2.5 KayitliPlanlarForm + PlanGuncelleForm
Kaydedilmiş planların listelenmesi, silinmesi ve güncellenmesi.
- DataGridView tablosu (çift renk şeritli) + sağ panel detay
- ✏️ Güncelle → PlanGuncelleForm açılır (ulaşım/konaklama/yer değiştirme)
- 🗑️ Sil → onay sonrası CASCADE silme

<img width="1246" height="689" alt="Ekran görüntüsü 2026-06-04 210330" src="https://github.com/user-attachments/assets/967f32a5-43e9-42af-b0fe-18d5e0e647a7" />
<img width="668" height="597" alt="Ekran görüntüsü 2026-06-04 210400" src="https://github.com/user-attachments/assets/0e22ac2d-9ea6-4459-9e66-88c294ea8172" />

### 3.2.6 AdminPanelForm — Yönetici Paneli
4 sekmeli CRUD arayüzü (turuncu tema):
- 🌆 Şehirler | 🚌 Ulaşımlar | 🏨 Konaklamalar | 📍 Gezilecek Yerler
- Her sekmede: şehir filtreli DataGridView + sağ tarafta ekleme kontrolleri + Ekle/Sil butonları
- Şehir silme: bağlı tüm veriler kademeli silinir

<img width="777" height="633" alt="Ekran görüntüsü 2026-06-04 210437" src="https://github.com/user-attachments/assets/69b4c6de-e605-4c01-9aad-d24a3e163dfa" />
<img width="775" height="633" alt="Ekran görüntüsü 2026-06-04 210446" src="https://github.com/user-attachments/assets/f791d144-989d-4b63-bb13-5cf9d2bbc687" />
<img width="775" height="636" alt="Ekran görüntüsü 2026-06-04 210452" src="https://github.com/user-attachments/assets/b3900329-d788-4766-a78d-e7146e2b6d5e" />
<img width="775" height="632" alt="Ekran görüntüsü 2026-06-04 210500" src="https://github.com/user-attachments/assets/63c698c2-1948-4f14-906b-12adbc9fa46b" />


---

# 4. USE-CASE MODELLER

## Aktörler

| Aktör | Açıklama |
|---|---|
| **Kullanıcı** | Tatil planlamak isteyen son kullanıcı |
| **Yönetici** | Sistem verilerini yöneten yetkili kişi |
| **Sistem** | Otomatik hesaplama ve veri yönetimi yapan yazılım |

## Use Case Listesi

### UC-01: Otomatik Plan Önerme
**Aktör:** Kullanıcı → **Akış:** Şehir, bütçe, gece, sezon seç → "Plan Öner" tıkla → Sistem 5 plan hesaplar → PlanSecimForm'da listeler → **Alternatif:** Uygun plan yoksa bilgi mesajı

### UC-02: Manuel Plan Oluşturma
**Aktör:** Kullanıcı → **Akış:** Şehir seç → "Manuel Planlama" tıkla → Ulaşım, konaklama, gezilecek yerleri seç → "Plan Oluştur" tıkla → Maliyet hesaplanır → SonucForm'da gösterilir

### UC-03: Plan Kaydetme
**Aktör:** Kullanıcı → **Akış:** Plan oluşturulduktan sonra "Planı Kaydet" tıkla → Plan + gezilecek yerler veritabanına kaydedilir

### UC-04: Kayıtlı Planları Görüntüleme
**Aktör:** Kullanıcı → **Akış:** "Kayıtlı Planlar" tıkla → Tüm planlar DataGridView'de listelenir → Seçilen planın detayı gösterilir

### UC-05: Plan Güncelleme
**Aktör:** Kullanıcı → **Akış:** Kayıtlı plan seç → "Güncelle" tıkla → PlanGuncelleForm'da ulaşım/konaklama/yer değiştir → Maliyet yeniden hesaplanır → Kaydedilir

### UC-06: Plan Silme
**Aktör:** Kullanıcı → **Akış:** Kayıtlı plan seç → "Sil" tıkla → Onay → Plan ve ilişkili kayıtlar silinir (CASCADE)

### UC-07: Veri Yönetimi (Admin)
**Aktör:** Yönetici → **Akış:** "Yönetici" tıkla → AdminPanelForm'da 4 sekmede şehir/ulaşım/konaklama/yer ekleme-silme işlemleri

### UC-08: Veritabanı Bağlantı Ayarlama
**Aktör:** Kullanıcı/Sistem → **Akış:** Bağlantı hatası → BaglantiAyarlariForm açılır → Bilgiler girilir → Test edilir → Başarılıysa kaydedilir

---

# 5. USE-CASE DİYAGRAMI

```mermaid
graph LR
    subgraph Aktörler
        K["👤 Kullanıcı"]
        A["🛠️ Yönetici"]
    end

    subgraph SmartTour
        UC1["UC-01: Otomatik Plan Önerme"]
        UC2["UC-02: Manuel Plan Oluşturma"]
        UC3["UC-03: Plan Kaydetme"]
        UC4["UC-04: Kayıtlı Planları Görüntüleme"]
        UC5["UC-05: Plan Güncelleme"]
        UC6["UC-06: Plan Silme"]
        UC7["UC-07: Veri Yönetimi"]
        UC8["UC-08: Bağlantı Ayarlama"]
        UC9["Maliyet Hesaplama"]
    end

    K --> UC1
    K --> UC2
    K --> UC3
    K --> UC4
    K --> UC5
    K --> UC6
    K --> UC8
    A --> UC7

    UC1 -.->|include| UC9
    UC2 -.->|include| UC9
    UC1 -.->|extend| UC3
    UC2 -.->|extend| UC3
```

---

# 6. CLASS DİYAGRAMI

## 6.1 Model Sınıfları

```mermaid
classDiagram
    class Sehir {
        +int SehirID
        +string SehirAdi
    }
    class Ulasim {
        +int UlasimID
        +int SehirID
        +string UlasimTuru
        +decimal Fiyat
    }
    class Konaklama {
        +int KonaklamaID
        +int SehirID
        +string KonaklamaAdi
        +string KonaklamaTuru
        +decimal GeceFiyat
    }
    class GezilecekYer {
        +int YerID
        +int SehirID
        +string YerAdi
        +decimal ZiyaretUcreti
        +string Aciklama
    }
    class TurPlani {
        +int PlanID
        +int SehirID
        +int UlasimID
        +int KonaklamaID
        +decimal ToplamMaliyet
        +int GeceSayisi
        +string Sezon
        +string SehirIciUlasim
        +decimal Butce
        +List~GezilecekYer~ SecilenYerler
    }

    Sehir "1" --> "*" Ulasim
    Sehir "1" --> "*" Konaklama
    Sehir "1" --> "*" GezilecekYer
    TurPlani "*" --> "1" Sehir
    TurPlani "*" --> "1" Ulasim
    TurPlani "*" --> "1" Konaklama
    TurPlani "*" --> "*" GezilecekYer
```

## 6.2 Servis, Repository ve Form Sınıfları

```mermaid
classDiagram
    class DatabaseHelper {
        +GetConnectionString()$ string
        +GetConnection()$ NpgsqlConnection
        +InitializeDatabase()$
    }
    class TurPlanlamaServisi {
        +GetSezonCarpan(string) decimal
        +ToplamMaliyetHesapla() decimal
        +OtomatikPlanOner() TurPlani
        +CokluPlanOner() List~TurPlani~
        +GunlukAkisMetniOlustur()$ string
    }
    class TurPlaniRepository {
        +Save(TurPlani)
        +GetAll() List~TurPlani~
        +Update(TurPlani)
        +Delete(int)
    }
    class AnaSayfaForm {
        -BtnPlanOner_Click()
        -BtnManuelPlanlama_Click()
    }
    class PlanSecimForm {
        -ShowPlanDetail()
        -BtnKaydet_Click()
    }
    class SonucForm {
        -SonuclariGoster()
        -BtnKaydet_Click()
    }
    class AdminPanelForm {
        -SehirleriListele()
        -UlasimleriListele()
    }

    AnaSayfaForm --> PlanSecimForm : Otomatik Mod
    AnaSayfaForm --> TercihlerForm : Manuel Mod
    AnaSayfaForm --> AdminPanelForm : Yönetici
    TurPlaniRepository ..> DatabaseHelper : kullanır
    AnaSayfaForm ..> TurPlanlamaServisi : kullanır
    PlanSecimForm ..> TurPlaniRepository : kaydeder
```
---




