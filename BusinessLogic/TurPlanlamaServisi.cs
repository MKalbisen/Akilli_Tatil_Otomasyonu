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
            decimal sehirIciToplam = GetSehirIciGunlukMaliyet(sehirIciUlasim) * geceSayisi;

            return new MaliyetDokumu
            {
                UlasimMaliyeti = ulasimFiyat * carpan,
                KonaklamaGeceFiyat = konaklamaGeceFiyat * carpan,
                GeceSayisi = geceSayisi,
                KonaklamaToplam = (konaklamaGeceFiyat * carpan) * geceSayisi,
                GeziMaliyeti = secilenYerler.Sum(y => y.ZiyaretUcreti),
                SehirIciToplam = sehirIciToplam,
                ToplamMaliyet = (ulasimFiyat * carpan) + ((konaklamaGeceFiyat * carpan) * geceSayisi) + secilenYerler.Sum(y => y.ZiyaretUcreti) + sehirIciToplam
            };
        }

        public TurPlani? OtomatikPlanOner(Sehir sehir, decimal butce, int geceSayisi,
            List<Ulasim> ulasimlar, List<Konaklama> konaklamalar, List<GezilecekYer> yerler, string sezon = "Bahar")
        {
            decimal carpan = GetSezonCarpan(sezon);

            var uygunUlasimlar = ulasimlar.Where(u => u.Fiyat * carpan <= butce).OrderBy(u => u.Fiyat).ToList();
            if (uygunUlasimlar.Count == 0) return null;
            var secilenUlasim = uygunUlasimlar.First();

            decimal kalanButce = butce - (secilenUlasim.Fiyat * carpan);

            var uygunKonaklamalar = konaklamalar
                .Where(k => (k.GeceFiyat * carpan) * geceSayisi <= kalanButce)
                .OrderByDescending(k => k.GeceFiyat)
                .ToList();
            if (uygunKonaklamalar.Count == 0) return null;
            var secilenKonaklama = uygunKonaklamalar.First();

            kalanButce -= (secilenKonaklama.GeceFiyat * carpan) * geceSayisi;

            string secilenSehirIci = "Toplu Tasima";
            decimal gunlukSehirIci = GetSehirIciGunlukMaliyet(secilenSehirIci);

            if (kalanButce >= 900m * geceSayisi)
            {
                secilenSehirIci = "Araç Kiralama";
                gunlukSehirIci = GetSehirIciGunlukMaliyet(secilenSehirIci);
            }
            else if (kalanButce >= 300m * geceSayisi)
            {
                secilenSehirIci = "Taksi";
                gunlukSehirIci = GetSehirIciGunlukMaliyet(secilenSehirIci);
            }
            kalanButce -= gunlukSehirIci * geceSayisi;

            var secilenYerler = new List<GezilecekYer>();
            var siralanmisYerler = yerler.OrderBy(y => y.ZiyaretUcreti).ToList();
            foreach (var yer in siralanmisYerler)
            {
                if (yer.ZiyaretUcreti <= kalanButce)
                {
                    secilenYerler.Add(yer);
                    kalanButce -= yer.ZiyaretUcreti;
                }
            }

            decimal toplamMaliyet = ToplamMaliyetHesapla(
                secilenUlasim.Fiyat, secilenKonaklama.GeceFiyat, geceSayisi, secilenYerler, sezon, secilenSehirIci);

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
                SehirIciMaliyet = gunlukSehirIci * geceSayisi,
                ToplamMaliyet = toplamMaliyet,
                Butce = butce,
                OlusturmaTarihi = System.DateTime.Now
            };
        }

        public List<TurPlani> CokluPlanOner(Sehir sehir, decimal butce, int geceSayisi,
            List<Ulasim> ulasimlar, List<Konaklama> konaklamalar, List<GezilecekYer> yerler, string sezon = "Bahar", int maxPlan = 5)
        {
            var tumKombinasyonlar = new List<TurPlani>();
            decimal carpan = GetSezonCarpan(sezon);

            foreach (var ulasim in ulasimlar.Where(u => u.Fiyat * carpan <= butce))
            {
                decimal kalanSonraUlasim = butce - (ulasim.Fiyat * carpan);

                foreach (var konaklama in konaklamalar
                    .Where(k => (k.GeceFiyat * carpan) * geceSayisi <= kalanSonraUlasim))
                {
                    decimal kalanButce = kalanSonraUlasim - ((konaklama.GeceFiyat * carpan) * geceSayisi);

                    string secilenSehirIci = "Toplu Tasima";
                    decimal gunlukSehirIci = GetSehirIciGunlukMaliyet(secilenSehirIci);

                    if (kalanButce >= 900m * geceSayisi)
                    {
                        secilenSehirIci = "Araç Kiralama";
                        gunlukSehirIci = GetSehirIciGunlukMaliyet(secilenSehirIci);
                    }
                    else if (kalanButce >= 300m * geceSayisi)
                    {
                        secilenSehirIci = "Taksi";
                        gunlukSehirIci = GetSehirIciGunlukMaliyet(secilenSehirIci);
                    }
                    kalanButce -= gunlukSehirIci * geceSayisi;

                    var secilenYerler = new List<GezilecekYer>();
                    foreach (var yer in yerler.OrderBy(y => y.ZiyaretUcreti))
                    {
                        if (yer.ZiyaretUcreti <= kalanButce)
                        {
                            secilenYerler.Add(yer);
                            kalanButce -= yer.ZiyaretUcreti;
                        }
                    }

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
                        SehirIciMaliyet = gunlukSehirIci * geceSayisi,
                        ToplamMaliyet = toplam,
                        Butce = butce,
                        OlusturmaTarihi = System.DateTime.Now
                    });
                }
            }

            if (tumKombinasyonlar.Count == 0) return new List<TurPlani>();

            var sonuc = new List<TurPlani>();
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

            if (sonuc.Count < maxPlan)
            {
                var eklenenIds = sonuc.Select(p => $"{p.UlasimID}_{p.KonaklamaID}").ToHashSet();
                var kalanlar = tumKombinasyonlar
                    .Where(p => !eklenenIds.Contains($"{p.UlasimID}_{p.KonaklamaID}"))
                    .OrderBy(p => p.ToplamMaliyet)
                    .ToList();

                if (kalanlar.Count > 0 && sonuc.Count < maxPlan)
                    sonuc.Add(kalanlar.First()); // En ucuz alternatif
                if (kalanlar.Count > 1 && sonuc.Count < maxPlan)
                    sonuc.Add(kalanlar.Last());  // En konforlu/pahalı alternatif
                if (kalanlar.Count > 2 && sonuc.Count < maxPlan)
                    sonuc.Add(kalanlar[kalanlar.Count / 2]); // Orta bütçe

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

            for (int gun = 1; gun <= gunSayisi; gun++)
            {
                sb.AppendLine($"  📅 {gun}. GÜN PROGRAMINIZ");
                sb.AppendLine("  ──────────────────────────────────────────");

                var gununYerleri = new List<GezilecekYer>();
                for (int i = 0; i < yerSayisi; i++)
                {
                    if ((i % gunSayisi) == (gun - 1))
                    {
                        gununYerleri.Add(plan.SecilenYerler[i]);
                    }
                }

                if (gununYerleri.Count == 0)
                {
                    sb.AppendLine("    🏖️ Serbest Zaman (Dilediğinizce dinlenebilir ve gezebilirsiniz)");
                }
                else
                {
                    for (int j = 0; j < gununYerleri.Count; j++)
                    {
                        string vakit = "🌅 Sabah";
                        if (j == 1) vakit = "☀️ Öğle ";
                        else if (j == 2) vakit = "🌙 Akşam";
                        else if (j > 2)  vakit = "✨ Ekstra";

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
