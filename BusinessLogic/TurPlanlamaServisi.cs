using System;
using System.Collections.Generic;
using System.Linq;
using SmartTour.Models;

namespace SmartTour.BusinessLogic
{
    public class TurPlanlamaServisi
    {
        public decimal GetSezonCarpan(string seyahatSezonu)
        {
            switch (seyahatSezonu)
            {
                case "Yaz":
                    return 1.4m; // %40 artış
                case "Kış":
                    return 0.8m; // %20 indirim
                default:
                    return 1.0m; // Bahar / Normal sezon
            }
        }

        public decimal GetSehirIciGunlukMaliyet(string ulasimTuru)
        {
            switch (ulasimTuru)
            {
                case "Taksi":
                    return 300.0m;
                case "Araç Kiralama":
                    return 900.0m;
                default:
                    return 50.0m; // Yürüyüş & Toplu Taşıma varsayılan ucuz seçenek
            }
        }

        public decimal ToplamMaliyetHesapla(decimal ulasimFiyat, decimal konaklamaGeceFiyat, int geceSayisi, List<GezilecekYer> secilenYerler, string sezon = "Bahar", string sehirIciUlasim = "Toplu Tasima")
        {
            decimal carpan = GetSezonCarpan(sezon);
            
            decimal ulasimToplam = ulasimFiyat * carpan;

            decimal konaklamaToplam = (konaklamaGeceFiyat * carpan) * geceSayisi;

            decimal geziToplam = secilenYerler.Sum(y => y.ZiyaretUcreti);

            decimal sehirIciToplam = GetSehirIciGunlukMaliyet(sehirIciUlasim) * geceSayisi;

            return ulasimToplam + konaklamaToplam + geziToplam + sehirIciToplam;
        }

        public bool ButceyeUygunMu(decimal toplamMaliyet, decimal butce)
        {
            return toplamMaliyet <= butce;
        }

        public MaliyetDokumu MaliyetDokumuOlustur(decimal ulasimFiyat, decimal konaklamaGeceFiyat, int geceSayisi, List<GezilecekYer> secilenYerler, string sezon = "Bahar", string sehirIciUlasim = "Toplu Tasima")
        {
            decimal carpan = GetSezonCarpan(sezon);
            decimal sehirIciGunluk = GetSehirIciGunlukMaliyet(sehirIciUlasim);
            decimal sehirIciToplam = sehirIciGunluk * geceSayisi;
            decimal geziMaliyeti = secilenYerler.Sum(y => y.ZiyaretUcreti);
            decimal konaklamaFiyatliGeceFiyat = konaklamaGeceFiyat * carpan;
            decimal konaklamaToplam = konaklamaFiyatliGeceFiyat * geceSayisi;
            decimal ulasimMaliyeti = ulasimFiyat * carpan;

            return new MaliyetDokumu
            {
                UlasimMaliyeti = ulasimMaliyeti,
                KonaklamaGeceFiyat = konaklamaFiyatliGeceFiyat,
                GeceSayisi = geceSayisi,
                KonaklamaToplam = konaklamaToplam,
                GeziMaliyeti = geziMaliyeti,
                SehirIciToplam = sehirIciToplam,
                ToplamMaliyet = ToplamMaliyetHesapla(ulasimFiyat, konaklamaGeceFiyat, geceSayisi, secilenYerler, sezon, sehirIciUlasim)
            };
        }

        /// <summary>
        /// Tur planı oluşturur
        /// </summary>
        private (string UlasimTuru, decimal Maliyet) SeciSehirIciUlasim(decimal kalanButce, int geceSayisi)
        {
            // Fiyatları yüksekten düşüğe sırala
            var secenek1 = ("Araç Kiralama", GetSehirIciGunlukMaliyet("Araç Kiralama") * geceSayisi);
            var secenek2 = ("Taksi", GetSehirIciGunlukMaliyet("Taksi") * geceSayisi);
            var secenek3 = ("Toplu Tasima", GetSehirIciGunlukMaliyet("Toplu Tasima") * geceSayisi);

            if (kalanButce >= secenek1.Item2)
                return secenek1;
            if (kalanButce >= secenek2.Item2)
                return secenek2;
            
            return secenek3;
        }

        /// <summary>
        /// Gezilecek yerleri bütçeye göre seçer
        /// </summary>
        private List<GezilecekYer> SecGezilecekYerler(List<GezilecekYer> yerler, decimal kalanButce)
        {
            if (yerler == null || yerler.Count == 0)
                return new List<GezilecekYer>();

            var secilenYerler = new List<GezilecekYer>();
            foreach (var yer in yerler.OrderBy(y => y.ZiyaretUcreti))
            {
                if (yer.ZiyaretUcreti <= kalanButce)
                {
                    secilenYerler.Add(yer);
                    kalanButce -= yer.ZiyaretUcreti;
                }
            }
            return secilenYerler;
        }

        public TurPlani? OtomatikPlanOner(Sehir sehir, decimal butce, int geceSayisi,
            List<Ulasim> ulasimlar, List<Konaklama> konaklamalar, List<GezilecekYer> yerler, string sezon = "Bahar")
        {
            // Parametreleri doğrula
            if (sehir == null || butce <= 0 || geceSayisi <= 0)
                return null;

            if (ulasimlar == null || ulasimlar.Count == 0 ||
                konaklamalar == null || konaklamalar.Count == 0)
                return null;

            if (yerler == null) 
                yerler = new List<GezilecekYer>(); // Boş liste kullan

            decimal carpan = GetSezonCarpan(sezon);

            // 1. Uygun ulaşım seçeneği bul
            var uygunUlasimlar = ulasimlar
                .Where(u => u.Fiyat * carpan <= butce)
                .OrderBy(u => u.Fiyat)
                .ToList();
            
            if (uygunUlasimlar.Count == 0) 
                return null;

            var secilenUlasim = uygunUlasimlar.First();
            decimal kalanButce = butce - (secilenUlasim.Fiyat * carpan);

            // 2. Uygun konaklama seçeneği bul
            var uygunKonaklamalar = konaklamalar
                .Where(k => (k.GeceFiyat * carpan) * geceSayisi <= kalanButce)
                .OrderByDescending(k => k.GeceFiyat)
                .ToList();
            
            if (uygunKonaklamalar.Count == 0) 
                return null;

            var secilenKonaklama = uygunKonaklamalar.First();
            kalanButce -= (secilenKonaklama.GeceFiyat * carpan) * geceSayisi;

            // 3. Uygun şehir içi ulaşım seç
            var (secilenSehirIci, sehirIciToplam) = SeciSehirIciUlasim(kalanButce, geceSayisi);
            kalanButce -= sehirIciToplam;

            // 4. Gezilecek yerleri seç
            var secilenYerler = SecGezilecekYerler(yerler, kalanButce);

            // 5. Toplam maliyeti hesapla
            decimal toplamMaliyet = ToplamMaliyetHesapla(
                secilenUlasim.Fiyat, secilenKonaklama.GeceFiyat, geceSayisi, secilenYerler, sezon, secilenSehirIci);

            // 6. Tur planı oluştur
            return new TurPlani
            {
                SehirID = sehir.SehirID,
                SehirAdi = sehir.SehirAdi,
                UlasimID = secilenUlasim.UlasimID,
                UlasimTuru = secilenUlasim.UlasimTuru,
                UlasimFiyat = secilenUlasim.Fiyat,
                KonaklamaID = secilenKonaklama.KonaklamaID,
                KonaklamaAdi = secilenKonaklama.KonaklamaAdi,
                KonaklamaFiyat = secilenKonaklama.GeceFiyat,
                GeceSayisi = geceSayisi,
                SecilenYerler = secilenYerler,
                Sezon = sezon,
                SehirIciUlasim = secilenSehirIci,
                SehirIciMaliyet = sehirIciToplam,
                ToplamMaliyet = toplamMaliyet,
                Butce = butce,
                OlusturmaTarihi = System.DateTime.Now
            };
        }

        public List<TurPlani> CokluPlanOner(Sehir sehir, decimal butce, int geceSayisi,
            List<Ulasim> ulasimlar, List<Konaklama> konaklamalar, List<GezilecekYer> yerler, string sezon = "Bahar", int maxPlan = 5)
        {
            // Parametreleri doğrula
            if (sehir == null || butce <= 0 || geceSayisi <= 0)
                return new List<TurPlani>();

            if (ulasimlar == null || ulasimlar.Count == 0 ||
                konaklamalar == null || konaklamalar.Count == 0)
                return new List<TurPlani>();

            if (yerler == null)
                yerler = new List<GezilecekYer>();

            var tumKombinasyonlar = new List<TurPlani>();
            decimal carpan = GetSezonCarpan(sezon);

            // Uygun ulaşım seçeneklerini filtrele
            var uygunUlasimlar = ulasimlar
                .Where(u => u.Fiyat * carpan <= butce)
                .ToList();

            foreach (var ulasim in uygunUlasimlar)
            {
                decimal kalanSonraUlasim = butce - (ulasim.Fiyat * carpan);

                // Uygun konaklama seçeneklerini filtrele
                var uygunKonaklamalar = konaklamalar
                    .Where(k => (k.GeceFiyat * carpan) * geceSayisi <= kalanSonraUlasim)
                    .ToList();

                foreach (var konaklama in uygunKonaklamalar)
                {
                    decimal kalanButce = kalanSonraUlasim - ((konaklama.GeceFiyat * carpan) * geceSayisi);

                    // Uygun şehir içi ulaşım seç
                    var (secilenSehirIci, sehirIciToplam) = SeciSehirIciUlasim(kalanButce, geceSayisi);
                    decimal kalanGeziButce = kalanButce - sehirIciToplam;

                    // Gezi yerlerini seç
                    var secilenYerler = SecGezilecekYerler(yerler, kalanGeziButce);

                    // Toplam maliyeti hesapla
                    decimal toplam = ToplamMaliyetHesapla(ulasim.Fiyat, konaklama.GeceFiyat, geceSayisi, secilenYerler, sezon, secilenSehirIci);

                    tumKombinasyonlar.Add(new TurPlani
                    {
                        SehirID = sehir.SehirID,
                        SehirAdi = sehir.SehirAdi,
                        UlasimID = ulasim.UlasimID,
                        UlasimTuru = ulasim.UlasimTuru,
                        UlasimFiyat = ulasim.Fiyat,
                        KonaklamaID = konaklama.KonaklamaID,
                        KonaklamaAdi = konaklama.KonaklamaAdi,
                        KonaklamaFiyat = konaklama.GeceFiyat,
                        GeceSayisi = geceSayisi,
                        SecilenYerler = secilenYerler,
                        Sezon = sezon,
                        SehirIciUlasim = secilenSehirIci,
                        SehirIciMaliyet = sehirIciToplam,
                        ToplamMaliyet = toplam,
                        Butce = butce,
                        OlusturmaTarihi = System.DateTime.Now
                    });
                }
            }

            if (tumKombinasyonlar.Count == 0) 
                return new List<TurPlani>();

            return SecBestPlanlar(tumKombinasyonlar, maxPlan);
        }

        /// <summary>
        /// Kombinasyonlar arasından en uygun planları seçer
        /// </summary>
        private List<TurPlani> SecBestPlanlar(List<TurPlani> tumKombinasyonlar, int maxPlan)
        {
            var sonuc = new List<TurPlani>();

            // Ulaşım türlerine göre grupla ve her gruptan seç
            var ulasimGruplari = tumKombinasyonlar
                .GroupBy(p => p.UlasimTuru)
                .OrderBy(g => g.Min(p => p.ToplamMaliyet))
                .ToList();

            foreach (var grup in ulasimGruplari)
            {
                var siralanmis = grup.OrderBy(p => p.ToplamMaliyet).ToList();
                int ortaIdx = siralanmis.Count / 2;
                sonuc.Add(siralanmis[ortaIdx]);
                if (sonuc.Count >= maxPlan) break;
            }

            // Eksik planları tamamla
            if (sonuc.Count < maxPlan)
            {
                var eklenenIds = sonuc.Select(p => $"{p.UlasimID}_{p.KonaklamaID}").ToHashSet();
                var kalanlar = tumKombinasyonlar
                    .Where(p => !eklenenIds.Contains($"{p.UlasimID}_{p.KonaklamaID}"))
                    .OrderBy(p => p.ToplamMaliyet)
                    .ToList();

                if (kalanlar.Count > 0 && sonuc.Count < maxPlan)
                    sonuc.Add(kalanlar.First());
                if (kalanlar.Count > 1 && sonuc.Count < maxPlan)
                    sonuc.Add(kalanlar.Last());
                if (kalanlar.Count > 2 && sonuc.Count < maxPlan)
                    sonuc.Add(kalanlar[kalanlar.Count / 2]);

                foreach (var plan in kalanlar)
                {
                    if (sonuc.Count >= maxPlan) break;
                    if (!sonuc.Any(s => s.UlasimID == plan.UlasimID && s.KonaklamaID == plan.KonaklamaID))
                        sonuc.Add(plan);
                }
            }

            return sonuc.OrderBy(p => p.ToplamMaliyet).Take(maxPlan).ToList();
        }

        public static string GunlukAkisMetniOlustur(TurPlani plan)
        {
            var sb = new System.Text.StringBuilder();
            
            if (plan.SecilenYerler == null || plan.SecilenYerler.Count == 0)
            {
                sb.AppendLine("  📍 Herhangi bir gezilecek yer seçilmedi.");
                return sb.ToString();
            }

            int yerSayisi = plan.SecilenYerler.Count;
            int gunSayisi = plan.GeceSayisi + 1;
            var vakitEmoji = new[] { "🌅 Sabah", "☀️ Öğle ", "🌙 Akşam", "✨ Ekstra" };

            for (int gun = 1; gun <= gunSayisi; gun++)
            {
                sb.AppendLine($"  📅 {gun}. GÜN PROGRAMINIZ");
                sb.AppendLine("  ──────────────────────────────────────────");

                // Yerler eşit şekilde bölün
                var gununYerleri = GetYerlerGun(plan.SecilenYerler, gun, gunSayisi);

                if (gununYerleri.Count == 0)
                {
                    sb.AppendLine("    🏖️ Serbest Zaman (Dilediğinizce dinlenebilir ve gezebilirsiniz)");
                }
                else
                {
                    for (int j = 0; j < gununYerleri.Count; j++)
                    {
                        string vakit = j < vakitEmoji.Length ? vakitEmoji[j] : "✨ Ekstra";
                        var yer = gununYerleri[j];
                        sb.AppendLine($"    {vakit} : {yer.YerAdi,-22} (Giriş: {yer.ZiyaretUcreti:N0} ₺)");
                        
                        if (!string.IsNullOrWhiteSpace(yer.Aciklama))
                        {
                            sb.AppendLine($"              💡 {yer.Aciklama}");
                        }
                    }
                }
                sb.AppendLine("  ──────────────────────────────────────────");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gezilecek yerleri günlere göre eşit şekilde dağıtır
        /// </summary>
        private static List<GezilecekYer> GetYerlerGun(List<GezilecekYer> tumYerler, int gun, int toplamGun)
        {
            var gununYerleri = new List<GezilecekYer>();
            int yerSayisi = tumYerler.Count;
            int yerBasinaGun = (int)System.Math.Ceiling((decimal)yerSayisi / toplamGun);

            int baslamaIndeksi = (gun - 1) * yerBasinaGun;
            int bitisIndeksi = System.Math.Min(baslamaIndeksi + yerBasinaGun, yerSayisi);

            for (int i = baslamaIndeksi; i < bitisIndeksi; i++)
            {
                gununYerleri.Add(tumYerler[i]);
            }

            return gununYerleri;
        }
    }

    public class MaliyetDokumu
    {
        public decimal UlasimMaliyeti { get; set; }
        public decimal KonaklamaGeceFiyat { get; set; }
        public int GeceSayisi { get; set; }
        public decimal KonaklamaToplam { get; set; }
        public decimal GeziMaliyeti { get; set; }
        public decimal SehirIciToplam { get; set; } // Şehir içi ulaşımın toplam tutarı (Günlük * Gece)
        public decimal ToplamMaliyet { get; set; }
    }
}
