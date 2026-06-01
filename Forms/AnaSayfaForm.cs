using System;
using System.Drawing;
using System.Windows.Forms;
using SmartTour.DataAccess;
using SmartTour.Models;
using SmartTour.BusinessLogic;
using System.Collections.Generic;
using System.Linq;

namespace SmartTour.Forms
{
    public class AnaSayfaForm : Form
    {
        private ComboBox cmbSehir;
        private TextBox txtSehirAra;
        private NumericUpDown nudButce;
        private NumericUpDown nudGeceSayisi;
        private ComboBox cmbSezon; // Yeni Sezon seçimi
        private Button btnAdmin;    // Yeni Admin paneli butonu
        private Button btnPlanOner;
        private Button btnKayitliPlanlar;
        private Button btnManuelPlanlama;
        private Label lblBaslik;
        private Label lblAltBaslik;
        private Panel pnlHeader;
        private Panel pnlContent;

        private SehirRepository _sehirRepo = new SehirRepository();
        private List<Sehir> _tumSehirler = new List<Sehir>();

        public AnaSayfaForm()
        {
            InitializeComponent();
            LoadSehirler();
        }

        private void InitializeComponent()
        {
            var screen = Screen.PrimaryScreen!.WorkingArea;
            int formW = (int)(screen.Width * 0.45);
            int formH = (int)(screen.Height * 0.65);

            this.Text = "SmartTour - Akıllı Tatil Planlama";
            this.ClientSize = new Size(formW, formH);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Font = new Font("Segoe UI", 11F);

            pnlHeader = new Panel
            {
                Dock = DockStyle.Top, Height = 140,
                BackColor = Color.FromArgb(0, 120, 215)
            };
            this.Controls.Add(pnlHeader);

            lblBaslik = new Label
            {
                Text = "✈ SmartTour",
                Font = new Font("Segoe UI", 32F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false, Size = new Size(formW - 20, 60),
                Location = new Point(10, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlHeader.Controls.Add(lblBaslik);

            lblAltBaslik = new Label
            {
                Text = "Bütçenize Uygun Akıllı Tatil Planlama",
                Font = new Font("Segoe UI", 14F),
                ForeColor = Color.FromArgb(200, 220, 255),
                AutoSize = false, Size = new Size(formW - 20, 35),
                Location = new Point(10, 85),
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlHeader.Controls.Add(lblAltBaslik);

            btnAdmin = new Button
            {
                Text = "🛠️ Yönetici",
                Location = new Point(formW - 130, 20),
                Size = new Size(110, 35),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = Color.FromArgb(220, 100, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnAdmin.FlatAppearance.BorderSize = 0;
            btnAdmin.Click += (s, e) =>
            {
                var adminForm = new AdminPanelForm();
                adminForm.ShowDialog();
                LoadSehirler(); // Şehirler güncellenmiş olabilir
            };
            pnlHeader.Controls.Add(btnAdmin);
            btnAdmin.BringToFront();

            int panelW = formW - 160;
            int panelH = formH - 200;
            pnlContent = new Panel
            {
                Location = new Point(80, 165),
                Size = new Size(panelW, panelH),
                BackColor = Color.White
            };
            this.Controls.Add(pnlContent);

            int y = 25;
            int controlX = 30;
            int controlW = panelW - 60;

            pnlContent.Controls.Add(new Label
            {
                Text = "📍 Şehir Seçimi:",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Location = new Point(controlX, y), AutoSize = true
            });
            y += 32;

            txtSehirAra = new TextBox
            {
                Location = new Point(controlX, y),
                Size = new Size(controlW, 32),
                Font = new Font("Segoe UI", 12F),
                ForeColor = Color.Gray, Text = "🔍 Şehir ara..."
            };
            txtSehirAra.GotFocus += (s, e) =>
            {
                if (txtSehirAra.Text == "🔍 Şehir ara...") { txtSehirAra.ForeColor = Color.Black; txtSehirAra.Text = ""; }
            };
            txtSehirAra.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtSehirAra.Text)) { txtSehirAra.ForeColor = Color.Gray; txtSehirAra.Text = "🔍 Şehir ara..."; }
            };
            txtSehirAra.TextChanged += (s, e) => FilterSehirler();
            pnlContent.Controls.Add(txtSehirAra);
            y += 38;

            cmbSehir = new ComboBox
            {
                Location = new Point(controlX, y),
                Size = new Size(controlW, 34),
                Font = new Font("Segoe UI", 12F),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            pnlContent.Controls.Add(cmbSehir);
            y += 50;

            pnlContent.Controls.Add(new Label
            {
                Text = "💰 Bütçe (₺):",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Location = new Point(controlX, y), AutoSize = true
            });
            y += 32;

            nudButce = new NumericUpDown
            {
                Location = new Point(controlX, y),
                Size = new Size(controlW, 34),
                Minimum = 500, Maximum = 100000, Value = 5000,
                Increment = 500,
                Font = new Font("Segoe UI", 12F),
                ThousandsSeparator = true
            };
            pnlContent.Controls.Add(nudButce);
            y += 50;

            int halfW = (controlW - 15) / 2;

            pnlContent.Controls.Add(new Label
            {
                Text = "🌙 Gece Sayısı:",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Location = new Point(controlX, y), AutoSize = true
            });

            pnlContent.Controls.Add(new Label
            {
                Text = "🌸 Sezon Seçimi:",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Location = new Point(controlX + halfW + 15, y), AutoSize = true
            });
            y += 32;

            nudGeceSayisi = new NumericUpDown
            {
                Location = new Point(controlX, y),
                Size = new Size(halfW, 34),
                Minimum = 1, Maximum = 30, Value = 3,
                Font = new Font("Segoe UI", 12F)
            };
            pnlContent.Controls.Add(nudGeceSayisi);

            cmbSezon = new ComboBox
            {
                Location = new Point(controlX + halfW + 15, y),
                Size = new Size(halfW, 34),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 12F)
            };
            cmbSezon.Items.AddRange(new object[] { "🌸 Bahar Sezonu", "☀️ Yaz Sezonu (+%40)", "❄️ Kış Sezonu (-%20)" });
            cmbSezon.SelectedIndex = 0;
            pnlContent.Controls.Add(cmbSezon);
            y += 55;

            btnPlanOner = new Button
            {
                Text = "🤖 Bütçeye Uygun Plan Öner",
                Location = new Point(controlX, y),
                Size = new Size(controlW, 55),
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnPlanOner.FlatAppearance.BorderSize = 0;
            btnPlanOner.Click += BtnPlanOner_Click;
            btnPlanOner.MouseEnter += (s, e) =>
            {
                btnPlanOner.BackColor = Color.FromArgb(30, 140, 55);
                btnPlanOner.Font = new Font("Segoe UI", 14.5F, FontStyle.Bold);
            };
            btnPlanOner.MouseLeave += (s, e) =>
            {
                btnPlanOner.BackColor = Color.FromArgb(40, 167, 69);
                btnPlanOner.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            };
            pnlContent.Controls.Add(btnPlanOner);
            y += 68;

            halfW = (controlW - 15) / 2;

            btnKayitliPlanlar = new Button
            {
                Text = "📋 Kayıtlı Planlar",
                Location = new Point(controlX, y),
                Size = new Size(halfW, 45),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand
            };
            btnKayitliPlanlar.FlatAppearance.BorderSize = 0;
            btnKayitliPlanlar.Click += (s, e) => new KayitliPlanlarForm().Show();
            btnKayitliPlanlar.MouseEnter += (s, e) => btnKayitliPlanlar.BackColor = Color.FromArgb(0, 90, 180);
            btnKayitliPlanlar.MouseLeave += (s, e) => btnKayitliPlanlar.BackColor = Color.FromArgb(0, 120, 215);
            pnlContent.Controls.Add(btnKayitliPlanlar);

            btnManuelPlanlama = new Button
            {
                Text = "Manuel Planlama →",
                Location = new Point(controlX + halfW + 15, y),
                Size = new Size(halfW, 45),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand
            };
            btnManuelPlanlama.FlatAppearance.BorderSize = 0;
            btnManuelPlanlama.Click += BtnManuelPlanlama_Click;
            btnManuelPlanlama.MouseEnter += (s, e) => btnManuelPlanlama.BackColor = Color.FromArgb(80, 90, 100);
            btnManuelPlanlama.MouseLeave += (s, e) => btnManuelPlanlama.BackColor = Color.FromArgb(108, 117, 125);
            pnlContent.Controls.Add(btnManuelPlanlama);

            pnlHeader.MouseMove += PnlHeader_MouseMove;
            lblBaslik.MouseMove += (s, e) =>
            {
                var pt = pnlHeader.PointToClient(lblBaslik.PointToScreen(e.Location));
                PnlHeader_MouseMove(pnlHeader, new MouseEventArgs(e.Button, e.Clicks, pt.X, pt.Y, e.Delta));
            };
        }

        private void PnlHeader_MouseMove(object? sender, MouseEventArgs e)
        {
            float ratio = (float)e.X / Math.Max(pnlHeader.Width, 1);
            int r = (int)(0 + ratio * 30);
            int g = (int)(100 + ratio * 40);
            int b = (int)(200 + ratio * 55);
            b = Math.Min(b, 255);
            pnlHeader.BackColor = Color.FromArgb(r, g, b);
        }

        private void LoadSehirler()
        {
            try
            {
                _tumSehirler = _sehirRepo.GetAll();
                FilterSehirler();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Şehirler yüklenirken hata oluştu:\n{ex.Message}", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FilterSehirler()
        {
            string arama = "";
            if (txtSehirAra.ForeColor != Color.Gray)
                arama = txtSehirAra.Text.Trim().ToLower();

            var filtered = _tumSehirler
                .Where(s => string.IsNullOrEmpty(arama) || s.SehirAdi.ToLower().StartsWith(arama))
                .ToList();

            cmbSehir.Items.Clear();
            cmbSehir.Items.Add("-- Şehir Seçiniz --");
            foreach (var sehir in filtered)
                cmbSehir.Items.Add(sehir);
            cmbSehir.SelectedIndex = 0;
        }

        private void BtnPlanOner_Click(object? sender, EventArgs e)
        {
            if (cmbSehir.SelectedIndex <= 0)
            {
                MessageBox.Show("Lütfen bir şehir seçiniz.", "Uyarı",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var secilenSehir = (Sehir)cmbSehir.SelectedItem!;
            decimal butce = nudButce.Value;
            int geceSayisi = (int)nudGeceSayisi.Value;

            string sezon = "Bahar";
            if (cmbSezon.SelectedIndex == 1) sezon = "Yaz";
            else if (cmbSezon.SelectedIndex == 2) sezon = "Kış";

            var ulasimlar = new UlasimRepository().GetBySehirId(secilenSehir.SehirID);
            var konaklamalar = new KonaklamaRepository().GetBySehirId(secilenSehir.SehirID);
            var yerler = new GezilecekYerRepository().GetBySehirId(secilenSehir.SehirID);

            var servis = new TurPlanlamaServisi();
            var planlar = servis.CokluPlanOner(secilenSehir, butce, geceSayisi, ulasimlar, konaklamalar, yerler, sezon);

            if (planlar.Count == 0)
            {
                MessageBox.Show(
                    "Bu bütçe ve gece sayısı ile uygun bir plan oluşturulamadı.\n" +
                    "Bütçeyi artırmayı veya gece sayısını azaltmayı deneyin.",
                    "Plan Bulunamadı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var secimForm = new PlanSecimForm(planlar, secilenSehir.SehirID);
            secimForm.Show();
            this.Hide();
            secimForm.FormClosed += (s, args) => this.Show();
        }

        private void BtnManuelPlanlama_Click(object? sender, EventArgs e)
        {
            if (cmbSehir.SelectedIndex <= 0)
            {
                MessageBox.Show("Lütfen bir şehir seçiniz.", "Uyarı",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var secilenSehir = (Sehir)cmbSehir.SelectedItem!;
            
            string sezon = "Bahar";
            if (cmbSezon.SelectedIndex == 1) sezon = "Yaz";
            else if (cmbSezon.SelectedIndex == 2) sezon = "Kış";

            var tercihlerForm = new TercihlerForm(secilenSehir, sezon);
            tercihlerForm.Show();
            this.Hide();
            tercihlerForm.FormClosed += (s, args) => this.Show();
        }
    }
}
