using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SmartTour.BusinessLogic;
using SmartTour.DataAccess;
using SmartTour.Models;

namespace SmartTour.Forms
{
    public class TercihlerForm : Form
    {
        private Sehir _secilenSehir;
        private Label lblBaslik;
        private Label lblBudget;
        private NumericUpDown nudButce;

        private Label lblUlasim;
        private TextBox txtUlasimAra;
        private NumericUpDown nudUlasimMinFiyat;
        private NumericUpDown nudUlasimMaxFiyat;
        private ComboBox cmbUlasim;

        private Label lblKonaklama;
        private TextBox txtKonaklamaAra;
        private NumericUpDown nudKonaklamaMinFiyat;
        private NumericUpDown nudKonaklamaMaxFiyat;
        private ComboBox cmbKonaklama;

        private Label lblGeceSayisi;
        private NumericUpDown nudGeceSayisi;

        private Label lblGezilecek;
        private TextBox txtGezilecekAra;
        private NumericUpDown nudGezilecekMinFiyat;
        private NumericUpDown nudGezilecekMaxFiyat;
        private CheckedListBox clbGezilecek;

        private Button btnPlanOlustur;
        private Button btnGeri;
        private Panel pnlHeader;

        private UlasimRepository _ulasimRepo = new UlasimRepository();
        private KonaklamaRepository _konaklamaRepo = new KonaklamaRepository();
        private GezilecekYerRepository _gezilecekRepo = new GezilecekYerRepository();

        private List<Ulasim> _tumUlasimlar = new List<Ulasim>();
        private List<Konaklama> _tumKonaklamalar = new List<Konaklama>();
        private List<GezilecekYer> _tumYerler = new List<GezilecekYer>();

        public TercihlerForm(Sehir sehir)
        {
            _secilenSehir = sehir;
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = $"SmartTour - Tercihler ({_secilenSehir.SehirAdi})";
            this.Size = new Size(650, 850);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Font = new Font("Segoe UI", 10F);
            this.AutoScroll = true;

            pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.FromArgb(0, 120, 215)
            };
            this.Controls.Add(pnlHeader);

            lblBaslik = new Label
            {
                Text = $"📋 Tercihlerinizi Belirleyin – {_secilenSehir.SehirAdi}",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                Size = new Size(630, 40),
                Location = new Point(10, 15),
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlHeader.Controls.Add(lblBaslik);

            int y = 85;
            int labelX = 20;
            int controlX = 20;
            int controlWidth = 590;
            int halfWidth = 140;

            lblBudget = new Label
            {
                Text = "💰 Maksimum Bütçe (₺):",
                Location = new Point(labelX, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            this.Controls.Add(lblBudget);
            y += 26;

            nudButce = new NumericUpDown
            {
                Location = new Point(controlX, y),
                Size = new Size(controlWidth, 28),
                Minimum = 500, Maximum = 100000, Value = 5000,
                Increment = 500,
                Font = new Font("Segoe UI", 10F),
                ThousandsSeparator = true
            };
            this.Controls.Add(nudButce);
            y += 38;

            lblUlasim = new Label
            {
                Text = "🚌 Ulaşım Türü:",
                Location = new Point(labelX, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            this.Controls.Add(lblUlasim);
            y += 24;

            txtUlasimAra = CreateSearchBox(controlX, y, 280, "İsim ile ara...");
            txtUlasimAra.TextChanged += (s, e) => FilterUlasim();
            this.Controls.Add(txtUlasimAra);

            this.Controls.Add(CreateMiniLabel("Min ₺:", controlX + 290, y + 2));
            nudUlasimMinFiyat = CreatePriceFilter(controlX + 330, y, 0);
            nudUlasimMinFiyat.ValueChanged += (s, e) => FilterUlasim();
            this.Controls.Add(nudUlasimMinFiyat);

            this.Controls.Add(CreateMiniLabel("Max ₺:", controlX + 470, y + 2));
            nudUlasimMaxFiyat = CreatePriceFilter(controlX + 510, y, 50000);
            nudUlasimMaxFiyat.ValueChanged += (s, e) => FilterUlasim();
            this.Controls.Add(nudUlasimMaxFiyat);
            y += 30;

            cmbUlasim = new ComboBox
            {
                Location = new Point(controlX, y),
                Size = new Size(controlWidth, 28),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F)
            };
            this.Controls.Add(cmbUlasim);
            y += 38;

            lblKonaklama = new Label
            {
                Text = "🏨 Konaklama Seçimi:",
                Location = new Point(labelX, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            this.Controls.Add(lblKonaklama);
            y += 24;

            txtKonaklamaAra = CreateSearchBox(controlX, y, 280, "İsim ile ara...");
            txtKonaklamaAra.TextChanged += (s, e) => FilterKonaklama();
            this.Controls.Add(txtKonaklamaAra);

            this.Controls.Add(CreateMiniLabel("Min ₺:", controlX + 290, y + 2));
            nudKonaklamaMinFiyat = CreatePriceFilter(controlX + 330, y, 0);
            nudKonaklamaMinFiyat.ValueChanged += (s, e) => FilterKonaklama();
            this.Controls.Add(nudKonaklamaMinFiyat);

            this.Controls.Add(CreateMiniLabel("Max ₺:", controlX + 470, y + 2));
            nudKonaklamaMaxFiyat = CreatePriceFilter(controlX + 510, y, 50000);
            nudKonaklamaMaxFiyat.ValueChanged += (s, e) => FilterKonaklama();
            this.Controls.Add(nudKonaklamaMaxFiyat);
            y += 30;

            cmbKonaklama = new ComboBox
            {
                Location = new Point(controlX, y),
                Size = new Size(controlWidth, 28),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F)
            };
            this.Controls.Add(cmbKonaklama);
            y += 38;

            lblGeceSayisi = new Label
            {
                Text = "🌙 Konaklama Süresi (Gece):",
                Location = new Point(labelX, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            this.Controls.Add(lblGeceSayisi);
            y += 24;

            nudGeceSayisi = new NumericUpDown
            {
                Location = new Point(controlX, y),
                Size = new Size(controlWidth, 28),
                Minimum = 1, Maximum = 30, Value = 3,
                Font = new Font("Segoe UI", 10F)
            };
            this.Controls.Add(nudGeceSayisi);
            y += 38;

            lblGezilecek = new Label
            {
                Text = "📍 Gezilecek Yerler (Birden fazla seçebilirsiniz):",
                Location = new Point(labelX, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            this.Controls.Add(lblGezilecek);
            y += 24;

            txtGezilecekAra = CreateSearchBox(controlX, y, 280, "İsim ile ara...");
            txtGezilecekAra.TextChanged += (s, e) => FilterGezilecekYerler();
            this.Controls.Add(txtGezilecekAra);

            this.Controls.Add(CreateMiniLabel("Min ₺:", controlX + 290, y + 2));
            nudGezilecekMinFiyat = CreatePriceFilter(controlX + 330, y, 0);
            nudGezilecekMinFiyat.ValueChanged += (s, e) => FilterGezilecekYerler();
            this.Controls.Add(nudGezilecekMinFiyat);

            this.Controls.Add(CreateMiniLabel("Max ₺:", controlX + 470, y + 2));
            nudGezilecekMaxFiyat = CreatePriceFilter(controlX + 510, y, 50000);
            nudGezilecekMaxFiyat.ValueChanged += (s, e) => FilterGezilecekYerler();
            this.Controls.Add(nudGezilecekMaxFiyat);
            y += 30;

            clbGezilecek = new CheckedListBox
            {
                Location = new Point(controlX, y),
                Size = new Size(controlWidth, 120),
                Font = new Font("Segoe UI", 10F),
                CheckOnClick = true
            };
            var toolTip = new ToolTip();
            int lastIndex = -1;
            clbGezilecek.MouseMove += (s, e) =>
            {
                int index = clbGezilecek.IndexFromPoint(e.Location);
                if (index != lastIndex)
                {
                    lastIndex = index;
                    if (index >= 0 && index < clbGezilecek.Items.Count)
                    {
                        var yer = clbGezilecek.Items[index] as GezilecekYer;
                        if (yer != null)
                            toolTip.SetToolTip(clbGezilecek, $"{yer.YerAdi}\n{yer.Aciklama}\nÜcret: {yer.ZiyaretUcreti:C}");
                    }
                    else
                        toolTip.SetToolTip(clbGezilecek, string.Empty);
                }
            };
            this.Controls.Add(clbGezilecek);
            y += 130;

            btnGeri = new Button
            {
                Text = "← Geri",
                Location = new Point(controlX, y),
                Size = new Size(170, 45),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnGeri.FlatAppearance.BorderSize = 0;
            btnGeri.Click += (s, e) => this.Close();
            btnGeri.MouseEnter += (s, e) => btnGeri.BackColor = Color.FromArgb(80, 90, 100);
            btnGeri.MouseLeave += (s, e) => btnGeri.BackColor = Color.FromArgb(108, 117, 125);
            this.Controls.Add(btnGeri);

            btnPlanOlustur = new Button
            {
                Text = "Plan Oluştur 🗺",
                Location = new Point(controlX + 200, y),
                Size = new Size(390, 45),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnPlanOlustur.FlatAppearance.BorderSize = 0;
            btnPlanOlustur.Click += BtnPlanOlustur_Click;
            btnPlanOlustur.MouseEnter += (s, e) =>
            {
                btnPlanOlustur.BackColor = Color.FromArgb(30, 140, 55);
                btnPlanOlustur.Font = new Font("Segoe UI", 12.5F, FontStyle.Bold);
            };
            btnPlanOlustur.MouseLeave += (s, e) =>
            {
                btnPlanOlustur.BackColor = Color.FromArgb(40, 167, 69);
                btnPlanOlustur.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            };
            this.Controls.Add(btnPlanOlustur);
        }

        private TextBox CreateSearchBox(int x, int y, int width, string placeholder)
        {
            var txt = new TextBox
            {
                Location = new Point(x, y),
                Size = new Size(width, 26),
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.Gray,
                Text = placeholder
            };
            string ph = placeholder;
            txt.GotFocus += (s, e) =>
            {
                if (txt.Text == ph) { txt.Text = ""; txt.ForeColor = Color.Black; }
            };
            txt.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txt.Text)) { txt.Text = ph; txt.ForeColor = Color.Gray; }
            };
            return txt;
        }

        private NumericUpDown CreatePriceFilter(int x, int y, decimal defaultVal)
        {
            return new NumericUpDown
            {
                Location = new Point(x, y),
                Size = new Size(100, 26),
                Minimum = 0, Maximum = 50000,
                Value = defaultVal,
                Increment = 100,
                Font = new Font("Segoe UI", 9F),
                ThousandsSeparator = true
            };
        }

        private Label CreateMiniLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(100, 100, 100)
            };
        }

        private void LoadData()
        {
            try
            {
                _tumUlasimlar = _ulasimRepo.GetBySehirId(_secilenSehir.SehirID);
                _tumKonaklamalar = _konaklamaRepo.GetBySehirId(_secilenSehir.SehirID);
                _tumYerler = _gezilecekRepo.GetBySehirId(_secilenSehir.SehirID);

                FilterUlasim();
                FilterKonaklama();
                FilterGezilecekYerler();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Veriler yüklenirken hata oluştu:\n{ex.Message}", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetSearchText(TextBox txt)
        {
            if (txt.ForeColor == Color.Gray) return "";
            return txt.Text.Trim();
        }

        private void FilterUlasim()
        {
            string arama = GetSearchText(txtUlasimAra).ToLower();
            decimal min = nudUlasimMinFiyat.Value;
            decimal max = nudUlasimMaxFiyat.Value;

            var filtered = _tumUlasimlar
                .Where(u => (string.IsNullOrEmpty(arama) || u.UlasimTuru.ToLower().StartsWith(arama)))
                .Where(u => u.Fiyat >= min && u.Fiyat <= max)
                .ToList();

            cmbUlasim.Items.Clear();
            cmbUlasim.Items.Add("-- Ulaşım Seçiniz --");
            foreach (var u in filtered) cmbUlasim.Items.Add(u);
            cmbUlasim.SelectedIndex = 0;
        }

        private void FilterKonaklama()
        {
            string arama = GetSearchText(txtKonaklamaAra).ToLower();
            decimal min = nudKonaklamaMinFiyat.Value;
            decimal max = nudKonaklamaMaxFiyat.Value;

            var filtered = _tumKonaklamalar
                .Where(k => (string.IsNullOrEmpty(arama) ||
                             k.KonaklamaAdi.ToLower().StartsWith(arama) ||
                             k.KonaklamaTuru.ToLower().StartsWith(arama)))
                .Where(k => k.GeceFiyat >= min && k.GeceFiyat <= max)
                .ToList();

            cmbKonaklama.Items.Clear();
            cmbKonaklama.Items.Add("-- Konaklama Seçiniz --");
            foreach (var k in filtered) cmbKonaklama.Items.Add(k);
            cmbKonaklama.SelectedIndex = 0;
        }

        private void FilterGezilecekYerler()
        {
            string arama = GetSearchText(txtGezilecekAra).ToLower();
            decimal min = nudGezilecekMinFiyat.Value;
            decimal max = nudGezilecekMaxFiyat.Value;

            var filtered = _tumYerler
                .Where(y => (string.IsNullOrEmpty(arama) ||
                             y.YerAdi.ToLower().StartsWith(arama) ||
                             y.Aciklama.ToLower().StartsWith(arama)))
                .Where(y => y.ZiyaretUcreti >= min && y.ZiyaretUcreti <= max)
                .ToList();

            clbGezilecek.Items.Clear();
            foreach (var y in filtered) clbGezilecek.Items.Add(y);
        }

        private void BtnPlanOlustur_Click(object? sender, EventArgs e)
        {
            if (cmbUlasim.SelectedIndex <= 0)
            {
                MessageBox.Show("Lütfen bir ulaşım türü seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (cmbKonaklama.SelectedIndex <= 0)
            {
                MessageBox.Show("Lütfen bir konaklama seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var secilenUlasim = (Ulasim)cmbUlasim.SelectedItem!;
            var secilenKonaklama = (Konaklama)cmbKonaklama.SelectedItem!;
            int geceSayisi = (int)nudGeceSayisi.Value;
            decimal butce = nudButce.Value;

            var secilenYerler = new List<GezilecekYer>();
            foreach (var item in clbGezilecek.CheckedItems)
                secilenYerler.Add((GezilecekYer)item);

            var servis = new TurPlanlamaServisi();
            var toplamMaliyet = servis.ToplamMaliyetHesapla(
                secilenUlasim.Fiyat, secilenKonaklama.GeceFiyat, geceSayisi, secilenYerler);

            var plan = new TurPlani
            {
                SehirID = _secilenSehir.SehirID,
                SehirAdi = _secilenSehir.SehirAdi,
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
                OlusturmaTarihi = DateTime.Now
            };

            var sonucForm = new SonucForm(plan);
            sonucForm.Show();
            this.Hide();
            sonucForm.FormClosed += (s, args) => this.Show();
        }
    }
}
