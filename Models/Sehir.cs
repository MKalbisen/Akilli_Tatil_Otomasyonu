namespace SmartTour.Models
{
    public class Sehir
    {
        public int SehirID { get; set; }
        public string SehirAdi { get; set; } = string.Empty;

        public override string ToString() => SehirAdi;
    }
}
