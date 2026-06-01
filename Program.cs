using System;
using System.Windows.Forms;
using SmartTour.DataAccess;
using SmartTour.Forms;

namespace SmartTour
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            try
            {
                DatabaseHelper.InitializeDatabase();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Veritabanı başlatılırken hata oluştu:\n{ex.Message}",
                    "Veritabanı Hatası",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            Application.Run(new AnaSayfaForm());
        }
    }
}
