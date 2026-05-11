namespace SmartTour.Models
{
    public class GezilecekYer
    {
        public int YerID { get; set; }
        public int SehirID { get; set; }
        public string YerAdi { get; set; } = string.Empty;
        public decimal ZiyaretUcreti { get; set; }
        public string Aciklama { get; set; } = string.Empty;

        public override string ToString() => $"{YerAdi} - {ZiyaretUcreti:C}";
    }
}
