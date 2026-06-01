using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SmartTour.BusinessLogic;
using SmartTour.DataAccess;
using SmartTour.Models;

namespace SmartTour.Forms
{
    public class SonucForm : Form
    {
        private TurPlani _plan;
        private Panel pnlHeader;
        private Label lblBaslik;
        private RichTextBox rtbSonuc;
        private Button btnYeniPlan;
        private Button btnKaydet;
        private Label lblDurum;

        public SonucForm(TurPlani plan)
        {
            _plan = plan;
            InitializeComponent();
            SonuclariGoster();
        }

        private void InitializeComponent()
        {
            this.Text = "SmartTour - Tur Planınız";
            this.Size = new Size(650, 620);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Font = new Font("Segoe UI", 10F);

            pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.FromArgb(0, 120, 215)
            };
            this.Controls.Add(pnlHeader);

            lblBaslik = new Label
            {
                Text = "🗺 Tur Planınız Hazır!",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                Size = new Size(630, 40),
                Location = new Point(10, 15),
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlHeader.Controls.Add(lblBaslik);

            rtbSonuc = new RichTextBox
            {
                Location = new Point(20, 90),
                Size = new Size(595, 380),
                Font = new Font("Consolas", 10.5F),
                ReadOnly = true,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            rtbSonuc.MouseDoubleClick += (s, e) =>
            {
                if (!string.IsNullOrEmpty(rtbSonuc.Text))
                {
                    Clipboard.SetText(rtbSonuc.Text);
                    var originalColor = lblDurum.ForeColor;
                    var originalText = lblDurum.Text;
                    lblDurum.Text = "📋 Plan detayları panoya kopyalandı!";
                    lblDurum.ForeColor = Color.FromArgb(0, 120, 215);
                    var timer = new System.Windows.Forms.Timer { Interval = 2000 };
                    timer.Tick += (ts, te) =>
                    {
                        lblDurum.Text = originalText;
                        lblDurum.ForeColor = originalColor;
                        timer.Stop();
                        timer.Dispose();
                    };
                    timer.Start();
                }
            };
            this.Controls.Add(rtbSonuc);

            lblDurum = new Label
            {
                Location = new Point(20, 480),
                Size = new Size(595, 35),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblDurum);

            btnYeniPlan = new Button
            {
                Text = "🔄 Yeni Plan",
                Location = new Point(20, 525),
                Size = new Size(285, 45),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnYeniPlan.FlatAppearance.BorderSize = 0;
            btnYeniPlan.Click += (s, e) => this.Close();
            btnYeniPlan.MouseEnter += (s, e) =>
            {
                btnYeniPlan.BackColor = Color.FromArgb(0, 90, 180);
            };
            btnYeniPlan.MouseLeave += (s, e) =>
            {
                btnYeniPlan.BackColor = Color.FromArgb(0, 120, 215);
            };
            this.Controls.Add(btnYeniPlan);

            btnKaydet = new Button
            {
                Text = "💾 Planı Kaydet",
                Location = new Point(330, 525),
                Size = new Size(285, 45),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnKaydet.FlatAppearance.BorderSize = 0;
            btnKaydet.Click += BtnKaydet_Click;
            btnKaydet.MouseEnter += (s, e) =>
            {
                btnKaydet.BackColor = Color.FromArgb(30, 140, 55);
                btnKaydet.Font = new Font("Segoe UI", 11.5F, FontStyle.Bold);
            };
            btnKaydet.MouseLeave += (s, e) =>
            {
                if (btnKaydet.Enabled)
                {
                    btnKaydet.BackColor = Color.FromArgb(40, 167, 69);
                    btnKaydet.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
                }
            };
            this.Controls.Add(btnKaydet);
        }

        private void SonuclariGoster()
        {
            var servis = new TurPlanlamaServisi();
            var dokum = servis.MaliyetDokumuOlustur(
                _plan.UlasimFiyat,
                _plan.KonaklamaFiyat,
                _plan.GeceSayisi,
                _plan.SecilenYerler,
                _plan.Sezon,
                _plan.SehirIciUlasim
            );

            bool uygun = servis.ButceyeUygunMu(dokum.ToplamMaliyet, _plan.Butce);

            string sonuc = "";
            sonuc += "╔══════════════════════════════════════════════════╗\n";
            sonuc += "║           SMARTTOUR - TUR PLAN DETAYI           ║\n";
            sonuc += "╚══════════════════════════════════════════════════╝\n\n";

            sonuc += $"  📍 Hedef Şehir     : {_plan.SehirAdi}\n";
            sonuc += $"  🌸 Sezon           : {_plan.Sezon}\n";
            sonuc += $"  🚌 Ana Ulaşım      : {_plan.UlasimTuru}\n";
            sonuc += $"  🚗 Şehir İçi Ulaşım: {_plan.SehirIciUlasim}\n";
            sonuc += $"  🏨 Konaklama       : {_plan.KonaklamaAdi}\n";
            sonuc += $"  🌙 Süre            : {_plan.GeceSayisi} Gece / {(_plan.GeceSayisi + 1)} Gün\n\n";

            sonuc += "──────────── MALİYET DÖKÜMÜ ────────────\n\n";
            sonuc += $"  🚌 Ana Ulaşım Maliyeti    : {dokum.UlasimMaliyeti,10:N2} ₺\n";
            sonuc += $"  🏨 Konaklama Toplam       : {dokum.KonaklamaToplam,10:N2} ₺\n";
            sonuc += $"     ({dokum.KonaklamaGeceFiyat:N2} ₺ × {dokum.GeceSayisi} gece)\n";
            sonuc += $"  🚗 Şehir İçi Ulaşım       : {dokum.SehirIciToplam,10:N2} ₺\n";
            sonuc += $"     ({servis.GetSehirIciGunlukMaliyet(_plan.SehirIciUlasim):N2} ₺ × {_plan.GeceSayisi} gün)\n";

            if (_plan.SecilenYerler.Count > 0)
            {
                sonuc += $"  📍 Gezi Giriş Ücretleri   : {dokum.GeziMaliyeti,10:N2} ₺\n";
            }

            sonuc += "\n══════════════════════════════════════════\n";
            sonuc += $"  💰 TOPLAM MALİYET         : {dokum.ToplamMaliyet,10:N2} ₺\n";
            sonuc += $"  💼 BELİRLENEN BÜTÇE       : {_plan.Butce,10:N2} ₺\n";
            sonuc += "══════════════════════════════════════════\n\n";

            if (uygun)
            {
                sonuc += "  ✅ Bütçenize uygun bir plan oluşturuldu!\n";
                decimal kalan = _plan.Butce - dokum.ToplamMaliyet;
                sonuc += $"  💵 Kalan Bütçeniz: {kalan:N2} ₺\n\n";
                lblDurum.Text = "✅ Bütçeye Uygun";
                lblDurum.ForeColor = Color.FromArgb(40, 167, 69);
            }
            else
            {
                decimal fark = dokum.ToplamMaliyet - _plan.Butce;
                sonuc += $"  ⚠️ Bütçe {fark:N2} ₺ aşıldı!\n";
                sonuc += "  Daha uygun seçenekler için bütçenizi düzenleyin.\n\n";
                lblDurum.Text = "⚠️ Bütçe Aşıldı";
                lblDurum.ForeColor = Color.FromArgb(220, 53, 69);
            }

            sonuc += "━━━━━━━━━━ GÜNLÜK SEYAHAT AJANDASI ━━━━━━━━━━\n\n";
            sonuc += TurPlanlamaServisi.GunlukAkisMetniOlustur(_plan);

            rtbSonuc.Text = sonuc;
        }

        private void BtnKaydet_Click(object? sender, EventArgs e)
        {
            try
            {
                var repo = new TurPlaniRepository();
                repo.Save(_plan);

                MessageBox.Show("Tur planınız veritabanına başarıyla kaydedildi!",
                    "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);

                btnKaydet.Enabled = false;
                btnKaydet.Text = "✓ Kaydedildi";
                btnKaydet.BackColor = Color.FromArgb(150, 150, 150);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Plan kaydedilirken hata oluştu:\n{ex.Message}", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string DosyaIcerigiOlustur()
        {
            var sb = new System.Text.StringBuilder();

            sb.AppendLine("═══════════════════════════════════════════════════════");
            sb.AppendLine("           SMARTTOUR - TUR PLANI DETAY RAPORU         ");
            sb.AppendLine("═══════════════════════════════════════════════════════");
            sb.AppendLine();
            sb.AppendLine($"  Oluşturma Tarihi  : {_plan.OlusturmaTarihi:dd.MM.yyyy HH:mm:ss}");
            sb.AppendLine();
            sb.AppendLine("───────────── GENEL BİLGİLER ─────────────");
            sb.AppendLine($"  Şehir             : {_plan.SehirAdi}");
            sb.AppendLine($"  Sezon             : {_plan.Sezon}");
            sb.AppendLine($"  Ulaşım Türü       : {_plan.UlasimTuru}");
            sb.AppendLine($"  Şehir İçi Ulaşım  : {_plan.SehirIciUlasim}");
            sb.AppendLine($"  Konaklama         : {_plan.KonaklamaAdi}");
            sb.AppendLine($"  Gece Sayısı       : {_plan.GeceSayisi}");
            sb.AppendLine($"  Bütçe             : {_plan.Butce:N2} ₺");
            sb.AppendLine();
            sb.AppendLine("───────────── MALİYET DÖKÜMÜ ─────────────");
            sb.AppendLine($"  Ulaşım Maliyeti   : {_plan.UlasimFiyat:N2} ₺");
            sb.AppendLine($"  Konaklama Toplam  : {_plan.KonaklamaFiyat * _plan.GeceSayisi:N2} ₺");
            sb.AppendLine($"    ({_plan.KonaklamaFiyat:N2} ₺ x {_plan.GeceSayisi} gece)");
            sb.AppendLine($"  Şehir İçi Ulaşım  : {_plan.SehirIciMaliyet:N2} ₺");

            if (_plan.SecilenYerler.Count > 0)
            {
                decimal geziToplam = _plan.SecilenYerler.Sum(y => y.ZiyaretUcreti);
                sb.AppendLine();
                sb.AppendLine("───────────── GEZİLECEK YERLER ─────────────");
                foreach (var yer in _plan.SecilenYerler)
                {
                    sb.AppendLine($"  • {yer.YerAdi,-30} {yer.ZiyaretUcreti,10:N2} ₺");
                    if (!string.IsNullOrEmpty(yer.Aciklama))
                        sb.AppendLine($"    {yer.Aciklama}");
                }
                sb.AppendLine($"  Gezi Toplam       : {geziToplam:N2} ₺");
            }

            sb.AppendLine();
            sb.AppendLine("───────────── GÜNLÜK SEYAHAT SEYRİ ─────────────");
            sb.AppendLine(TurPlanlamaServisi.GunlukAkisMetniOlustur(_plan));

            sb.AppendLine();
            sb.AppendLine("═══════════════════════════════════════════════════════");
            sb.AppendLine($"  TOPLAM MALİYET    : {_plan.ToplamMaliyet:N2} ₺");
            sb.AppendLine($"  BÜTÇE             : {_plan.Butce:N2} ₺");

            decimal fark = _plan.Butce - _plan.ToplamMaliyet;
            if (fark >= 0)
            {
                sb.AppendLine($"  KALAN BÜTÇE       : {fark:N2} ₺");
                sb.AppendLine("  DURUM             : Bütçeye Uygun ✓");
            }
            else
            {
                sb.AppendLine($"  BÜTÇE AŞIMI       : {Math.Abs(fark):N2} ₺");
                sb.AppendLine("  DURUM             : Bütçe Aşıldı ✗");
            }
            sb.AppendLine("═══════════════════════════════════════════════════════");
            sb.AppendLine();
            sb.AppendLine("Bu rapor SmartTour uygulaması tarafından oluşturulmuştur.");

            return sb.ToString();
        }
    }
}
