namespace SmartTour.Models
{
    public class Ulasim
    {
        public int UlasimID { get; set; }
        public int SehirID { get; set; }
        public string UlasimTuru { get; set; } = string.Empty;
        public decimal Fiyat { get; set; }

        public override string ToString() => $"{UlasimTuru} - {Fiyat:C}";
    }
}
