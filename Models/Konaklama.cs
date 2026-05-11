namespace SmartTour.Models
{
    public class Konaklama
    {
        public int KonaklamaID { get; set; }
        public int SehirID { get; set; }
        public string KonaklamaAdi { get; set; } = string.Empty;
        public string KonaklamaTuru { get; set; } = string.Empty;
        public decimal GeceFiyat { get; set; }

        public override string ToString() => $"{KonaklamaAdi} ({KonaklamaTuru}) - {GeceFiyat:C}/gece";
    }
}
