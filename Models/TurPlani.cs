using System;
using System.Collections.Generic;

namespace SmartTour.Models
{
    public class TurPlani
    {
        public int PlanID { get; set; }
        public int SehirID { get; set; }
        public int UlasimID { get; set; }
        public int KonaklamaID { get; set; }
        public decimal ToplamMaliyet { get; set; }
        public DateTime OlusturmaTarihi { get; set; }

        public string SehirAdi { get; set; } = string.Empty;
        public string UlasimTuru { get; set; } = string.Empty;
        public decimal UlasimFiyat { get; set; }
        public string KonaklamaAdi { get; set; } = string.Empty;
        public decimal KonaklamaFiyat { get; set; }
        public int GeceSayisi { get; set; }
        public List<GezilecekYer> SecilenYerler { get; set; } = new List<GezilecekYer>();
        public decimal Butce { get; set; }
    }
}
