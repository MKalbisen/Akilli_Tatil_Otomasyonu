using System.Collections.Generic;
using System.Linq;
using SmartTour.Models;

namespace SmartTour.BusinessLogic
{
    public class TurPlanlamaServisi
    {
        public decimal ToplamMaliyetHesapla(decimal ulasimFiyat, decimal konaklamaGeceFiyat, int geceSayisi, List<GezilecekYer> secilenYerler)
        {
            decimal ulasimToplam = ulasimFiyat;
            decimal konaklamaToplam = konaklamaGeceFiyat * geceSayisi;
            decimal geziToplam = secilenYerler.Sum(y => y.ZiyaretUcreti);
            return ulasimToplam + konaklamaToplam + geziToplam;
        }

        public bool ButceyeUygunMu(decimal toplamMaliyet, decimal butce)
        {
            return toplamMaliyet <= butce;
        }

        public MaliyetDokumu MaliyetDokumuOlustur(decimal ulasimFiyat, decimal konaklamaGeceFiyat, int geceSayisi, List<GezilecekYer> secilenYerler)
        {
            return new MaliyetDokumu
            {
                UlasimMaliyeti = ulasimFiyat,
                KonaklamaGeceFiyat = konaklamaGeceFiyat,
                GeceSayisi = geceSayisi,
                KonaklamaToplam = konaklamaGeceFiyat * geceSayisi,
                GeziMaliyeti = secilenYerler.Sum(y => y.ZiyaretUcreti),
                ToplamMaliyet = ulasimFiyat + (konaklamaGeceFiyat * geceSayisi) + secilenYerler.Sum(y => y.ZiyaretUcreti)
            };
        }

        public TurPlani? OtomatikPlanOner(Sehir sehir, decimal butce, int geceSayisi,
            List<Ulasim> ulasimlar, List<Konaklama> konaklamalar, List<GezilecekYer> yerler)
        {
            var uygunUlasimlar = ulasimlar.Where(u => u.Fiyat <= butce).OrderBy(u => u.Fiyat).ToList();
            if (uygunUlasimlar.Count == 0) return null;
            var secilenUlasim = uygunUlasimlar.First();

            decimal kalanButce = butce - secilenUlasim.Fiyat;

            var uygunKonaklamalar = konaklamalar
                .Where(k => k.GeceFiyat * geceSayisi <= kalanButce)
                .OrderByDescending(k => k.GeceFiyat)
                .ToList();
            if (uygunKonaklamalar.Count == 0) return null;
            var secilenKonaklama = uygunKonaklamalar.First();

            kalanButce -= secilenKonaklama.GeceFiyat * geceSayisi;

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
                secilenUlasim.Fiyat, secilenKonaklama.GeceFiyat, geceSayisi, secilenYerler);

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
                ToplamMaliyet = toplamMaliyet,
                Butce = butce,
                OlusturmaTarihi = System.DateTime.Now
            };
        }

        public List<TurPlani> CokluPlanOner(Sehir sehir, decimal butce, int geceSayisi,
            List<Ulasim> ulasimlar, List<Konaklama> konaklamalar, List<GezilecekYer> yerler, int maxPlan = 5)
        {
            var tumKombinasyonlar = new List<TurPlani>();

            foreach (var ulasim in ulasimlar.Where(u => u.Fiyat <= butce))
            {
                decimal kalanSonraUlasim = butce - ulasim.Fiyat;

                foreach (var konaklama in konaklamalar
                    .Where(k => k.GeceFiyat * geceSayisi <= kalanSonraUlasim))
                {
                    decimal kalanButce = kalanSonraUlasim - (konaklama.GeceFiyat * geceSayisi);

                    var secilenYerler = new List<GezilecekYer>();
                    foreach (var yer in yerler.OrderBy(y => y.ZiyaretUcreti))
                    {
                        if (yer.ZiyaretUcreti <= kalanButce)
                        {
                            secilenYerler.Add(yer);
                            kalanButce -= yer.ZiyaretUcreti;
                        }
                    }

                    decimal toplam = ToplamMaliyetHesapla(ulasim.Fiyat, konaklama.GeceFiyat, geceSayisi, secilenYerler);

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
                    sonuc.Add(kalanlar.First()); // En ucuz
                if (kalanlar.Count > 1 && sonuc.Count < maxPlan)
                    sonuc.Add(kalanlar.Last());  // En pahalı
                if (kalanlar.Count > 2 && sonuc.Count < maxPlan)
                    sonuc.Add(kalanlar[kalanlar.Count / 2]); // Orta

                foreach (var plan in kalanlar)
                {
                    if (sonuc.Count >= maxPlan) break;
                    if (!sonuc.Any(s => s.UlasimID == plan.UlasimID && s.KonaklamaID == plan.KonaklamaID))
                        sonuc.Add(plan);
                }
            }

            return sonuc.OrderBy(p => p.ToplamMaliyet).Take(maxPlan).ToList();
        }
    }

    public class MaliyetDokumu
    {
        public decimal UlasimMaliyeti { get; set; }
        public decimal KonaklamaGeceFiyat { get; set; }
        public int GeceSayisi { get; set; }
        public decimal KonaklamaToplam { get; set; }
        public decimal GeziMaliyeti { get; set; }
        public decimal ToplamMaliyet { get; set; }
    }
}
