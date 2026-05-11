using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SmartTour.DataAccess;
using SmartTour.Models;

namespace SmartTour.Forms
{
    public class KayitliPlanlarForm : Form
    {
        private Panel pnlHeader;
        private Label lblBaslik;
        private DataGridView dgvPlanlar;
        private RichTextBox rtbDetay;
        private Button btnSil;
        private Button btnGuncelle;
        private Button btnKapat;
        private Label lblBilgi;

        private TurPlaniRepository _repo = new TurPlaniRepository();
        private List<TurPlani> _planlar = new List<TurPlani>();

        public KayitliPlanlarForm()
        {
            InitializeComponent();
            LoadPlanlar();
        }

        private void InitializeComponent()
        {
            var screen = Screen.PrimaryScreen!.WorkingArea;
            int cW = (int)(screen.Width * 0.65);
            int cH = (int)(screen.Height * 0.65);

            this.Text = "SmartTour - Kayıtlı Planlar";
            this.ClientSize = new Size(cW, cH);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Font = new Font("Segoe UI", 11F);

            int formW = cW;
            int formH = cH;

            pnlHeader = new Panel
            {
                Dock = DockStyle.Top, Height = 70,
                BackColor = Color.FromArgb(0, 120, 215)
            };
            this.Controls.Add(pnlHeader);

            lblBaslik = new Label
            {
                Text = "📋 Kayıtlı Tur Planları",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false, Size = new Size(formW - 20, 45),
                Location = new Point(10, 12),
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlHeader.Controls.Add(lblBaslik);

            int dgvW = (int)(formW * 0.6);
            int detayW = formW - dgvW - 40;
            int contentH = formH - 175;

            dgvPlanlar = new DataGridView
            {
                Location = new Point(15, 85),
                Size = new Size(dgvW, contentH),
                Font = new Font("Segoe UI", 11F),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(0, 120, 215),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                },
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    SelectionBackColor = Color.FromArgb(200, 220, 255),
                    SelectionForeColor = Color.Black
                },
                EnableHeadersVisualStyles = false
            };
            dgvPlanlar.CellFormatting += DgvPlanlar_CellFormatting;
            dgvPlanlar.SelectionChanged += DgvPlanlar_SelectionChanged;
            this.Controls.Add(dgvPlanlar);

            rtbDetay = new RichTextBox
            {
                Location = new Point(dgvW + 25, 85),
                Size = new Size(detayW, contentH),
                Font = new Font("Consolas", 11F),
                ReadOnly = true,
                BackColor = Color.FromArgb(250, 250, 255),
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(rtbDetay);

            lblBilgi = new Label
            {
                Location = new Point(15, contentH + 95),
                Size = new Size(500, 28),
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(100, 100, 100),
                Text = ""
            };
            this.Controls.Add(lblBilgi);

            int btnY = formH - 80;
            int btnW = (formW - 50) / 3;

            btnGuncelle = new Button
            {
                Text = "✏️ Güncelle",
                Location = new Point(15, btnY),
                Size = new Size(btnW, 48),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                BackColor = Color.FromArgb(255, 165, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand
            };
            btnGuncelle.FlatAppearance.BorderSize = 0;
            btnGuncelle.Click += BtnGuncelle_Click;
            btnGuncelle.MouseEnter += (s, e) => btnGuncelle.BackColor = Color.FromArgb(220, 140, 0);
            btnGuncelle.MouseLeave += (s, e) => btnGuncelle.BackColor = Color.FromArgb(255, 165, 0);
            this.Controls.Add(btnGuncelle);

            btnSil = new Button
            {
                Text = "🗑️ Sil",
                Location = new Point(btnW + 25, btnY),
                Size = new Size(btnW, 48),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand
            };
            btnSil.FlatAppearance.BorderSize = 0;
            btnSil.Click += BtnSil_Click;
            btnSil.MouseEnter += (s, e) => btnSil.BackColor = Color.FromArgb(180, 40, 55);
            btnSil.MouseLeave += (s, e) => btnSil.BackColor = Color.FromArgb(220, 53, 69);
            this.Controls.Add(btnSil);

            btnKapat = new Button
            {
                Text = "← Kapat",
                Location = new Point(2 * btnW + 35, btnY),
                Size = new Size(btnW, 48),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand
            };
            btnKapat.FlatAppearance.BorderSize = 0;
            btnKapat.Click += (s, e) => this.Close();
            btnKapat.MouseEnter += (s, e) => btnKapat.BackColor = Color.FromArgb(80, 90, 100);
            btnKapat.MouseLeave += (s, e) => btnKapat.BackColor = Color.FromArgb(108, 117, 125);
            this.Controls.Add(btnKapat);
        }

        private void LoadPlanlar()
        {
            try
            {
                _planlar = _repo.GetAll();
                dgvPlanlar.DataSource = null;
                dgvPlanlar.Columns.Clear();

                dgvPlanlar.Columns.Add(new DataGridViewTextBoxColumn { Name = "PlanID", HeaderText = "ID", Width = 35 });
                dgvPlanlar.Columns.Add(new DataGridViewTextBoxColumn { Name = "SehirAdi", HeaderText = "Şehir", Width = 75 });
                dgvPlanlar.Columns.Add(new DataGridViewTextBoxColumn { Name = "UlasimTuru", HeaderText = "Ulaşım", Width = 60 });
                dgvPlanlar.Columns.Add(new DataGridViewTextBoxColumn { Name = "KonaklamaAdi", HeaderText = "Konaklama", Width = 120 });
                dgvPlanlar.Columns.Add(new DataGridViewTextBoxColumn { Name = "ToplamMaliyet", HeaderText = "Toplam ₺", Width = 80 });
                dgvPlanlar.Columns.Add(new DataGridViewTextBoxColumn { Name = "GezilecekSayisi", HeaderText = "Yer", Width = 35 });
                dgvPlanlar.Columns.Add(new DataGridViewTextBoxColumn { Name = "Tarih", HeaderText = "Tarih", Width = 110 });

                dgvPlanlar.Rows.Clear();
                foreach (var p in _planlar)
                {
                    dgvPlanlar.Rows.Add(
                        p.PlanID, p.SehirAdi, p.UlasimTuru,
                        p.KonaklamaAdi,
                        $"{p.ToplamMaliyet:N2}",
                        p.SecilenYerler.Count,
                        p.OlusturmaTarihi.ToString("dd.MM.yyyy HH:mm")
                    );
                }

                lblBilgi.Text = $"Toplam {_planlar.Count} kayıtlı plan bulundu.";
                rtbDetay.Text = "← Bir plan seçin";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Planlar yüklenirken hata:\n{ex.Message}", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvPlanlar_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvPlanlar.SelectedRows.Count == 0) return;
            int planId = (int)dgvPlanlar.SelectedRows[0].Cells["PlanID"].Value!;
            var plan = _planlar.FirstOrDefault(p => p.PlanID == planId);
            if (plan == null) return;

            string detay = "";
            detay += $"📋 Plan #{plan.PlanID}\n";
            detay += $"━━━━━━━━━━━━━━━━━━━━━\n";
            detay += $"📍 Şehir     : {plan.SehirAdi}\n";
            detay += $"🚌 Ulaşım   : {plan.UlasimTuru}\n";
            detay += $"   Fiyat     : {plan.UlasimFiyat:N2} ₺\n";
            detay += $"🏨 Konaklama : {plan.KonaklamaAdi}\n";
            detay += $"   Gece      : {plan.GeceSayisi}\n";
            detay += $"   Fiyat     : {plan.KonaklamaFiyat:N2} ₺\n";
            detay += $"━━━━━━━━━━━━━━━━━━━━━\n";

            if (plan.SecilenYerler.Count > 0)
            {
                detay += "📍 Gezilecek Yerler:\n";
                foreach (var yer in plan.SecilenYerler)
                {
                    detay += $"  • {yer.YerAdi}\n";
                    detay += $"    {yer.ZiyaretUcreti:N2} ₺\n";
                }
                detay += $"━━━━━━━━━━━━━━━━━━━━━\n";
            }

            detay += $"💰 Toplam    : {plan.ToplamMaliyet:N2} ₺\n";
            if (plan.Butce > 0)
                detay += $"💼 Bütçe     : {plan.Butce:N2} ₺\n";
            detay += $"📅 Tarih     : {plan.OlusturmaTarihi:dd.MM.yyyy HH:mm}\n";

            rtbDetay.Text = detay;
        }

        private void DgvPlanlar_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex % 2 == 0)
                e.CellStyle!.BackColor = Color.FromArgb(248, 249, 252);
            else
                e.CellStyle!.BackColor = Color.White;
        }

        private void BtnSil_Click(object? sender, EventArgs e)
        {
            if (dgvPlanlar.SelectedRows.Count == 0)
            {
                MessageBox.Show("Lütfen silmek istediğiniz planı seçin.", "Uyarı",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int planId = (int)dgvPlanlar.SelectedRows[0].Cells["PlanID"].Value!;
            var sehir = dgvPlanlar.SelectedRows[0].Cells["SehirAdi"].Value?.ToString();

            var result = MessageBox.Show(
                $"'{sehir}' planını silmek istediğinizden emin misiniz?\n(Gezilecek yerler de silinecektir)",
                "Silme Onayı", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    _repo.Delete(planId);
                    MessageBox.Show("Plan başarıyla silindi.", "Başarılı",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadPlanlar();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Silme hatası:\n{ex.Message}", "Hata",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnGuncelle_Click(object? sender, EventArgs e)
        {
            if (dgvPlanlar.SelectedRows.Count == 0)
            {
                MessageBox.Show("Lütfen güncellemek istediğiniz planı seçin.", "Uyarı",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int planId = (int)dgvPlanlar.SelectedRows[0].Cells["PlanID"].Value!;
            var plan = _planlar.FirstOrDefault(p => p.PlanID == planId);
            if (plan == null) return;

            var guncelleForm = new PlanGuncelleForm(plan);
            guncelleForm.ShowDialog();
            LoadPlanlar();
        }
    }

    public class PlanGuncelleForm : Form
    {
        private TurPlani _plan;
        private Panel pnlHeader;
        private Label lblBaslik;
        private Label lblUlasim;
        private ComboBox cmbUlasim;
        private Label lblKonaklama;
        private ComboBox cmbKonaklama;
        private Label lblGezilecek;
        private CheckedListBox clbGezilecek;
        private Button btnKaydet;
        private Button btnIptal;

        private UlasimRepository _ulasimRepo = new UlasimRepository();
        private KonaklamaRepository _konaklamaRepo = new KonaklamaRepository();
        private GezilecekYerRepository _gezilecekRepo = new GezilecekYerRepository();
        private TurPlaniRepository _planRepo = new TurPlaniRepository();

        public PlanGuncelleForm(TurPlani plan)
        {
            _plan = plan;
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            var screen = Screen.PrimaryScreen!.WorkingArea;
            int formW = (int)(screen.Width * 0.35);
            int formH = (int)(screen.Height * 0.55);

            this.Text = $"Plan Güncelle – {_plan.SehirAdi}";
            this.ClientSize = new Size(formW, formH);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Font = new Font("Segoe UI", 11F);

            pnlHeader = new Panel
            {
                Dock = DockStyle.Top, Height = 60,
                BackColor = Color.FromArgb(255, 165, 0)
            };
            this.Controls.Add(pnlHeader);

            lblBaslik = new Label
            {
                Text = $"✏️ Plan Güncelle – {_plan.SehirAdi}",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false, Size = new Size(formW - 20, 40),
                Location = new Point(10, 10),
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlHeader.Controls.Add(lblBaslik);

            int cW = formW - 60;
            int y = 75;

            lblUlasim = new Label
            {
                Text = "🚌 Ulaşım Türü:", Location = new Point(25, y),
                AutoSize = true, Font = new Font("Segoe UI", 11F, FontStyle.Bold)
            };
            this.Controls.Add(lblUlasim);
            y += 30;

            cmbUlasim = new ComboBox
            {
                Location = new Point(25, y), Size = new Size(cW, 32),
                DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 11F)
            };
            this.Controls.Add(cmbUlasim);
            y += 42;

            lblKonaklama = new Label
            {
                Text = "🏨 Konaklama:", Location = new Point(25, y),
                AutoSize = true, Font = new Font("Segoe UI", 11F, FontStyle.Bold)
            };
            this.Controls.Add(lblKonaklama);
            y += 30;

            cmbKonaklama = new ComboBox
            {
                Location = new Point(25, y), Size = new Size(cW, 32),
                DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 11F)
            };
            this.Controls.Add(cmbKonaklama);
            y += 42;

            lblGezilecek = new Label
            {
                Text = "📍 Gezilecek Yerler:", Location = new Point(25, y),
                AutoSize = true, Font = new Font("Segoe UI", 11F, FontStyle.Bold)
            };
            this.Controls.Add(lblGezilecek);
            y += 28;

            int gezH = formH - y - 95;
            clbGezilecek = new CheckedListBox
            {
                Location = new Point(25, y), Size = new Size(cW, gezH),
                Font = new Font("Segoe UI", 11F), CheckOnClick = true
            };
            this.Controls.Add(clbGezilecek);
            y += gezH + 10;

            int halfBtn = (cW - 10) / 2;
            btnKaydet = new Button
            {
                Text = "💾 Kaydet", Location = new Point(25, y),
                Size = new Size(halfBtn, 45),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                BackColor = Color.FromArgb(40, 167, 69), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand
            };
            btnKaydet.FlatAppearance.BorderSize = 0;
            btnKaydet.Click += BtnKaydet_Click;
            btnKaydet.MouseEnter += (s, e) => btnKaydet.BackColor = Color.FromArgb(30, 140, 55);
            btnKaydet.MouseLeave += (s, e) => btnKaydet.BackColor = Color.FromArgb(40, 167, 69);
            this.Controls.Add(btnKaydet);

            btnIptal = new Button
            {
                Text = "İptal", Location = new Point(25 + halfBtn + 10, y),
                Size = new Size(halfBtn, 45),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                BackColor = Color.FromArgb(108, 117, 125), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand
            };
            btnIptal.FlatAppearance.BorderSize = 0;
            btnIptal.Click += (s, e) => this.Close();
            this.Controls.Add(btnIptal);
        }

        private void LoadData()
        {
            var ulasimlar = _ulasimRepo.GetBySehirId(_plan.SehirID);
            cmbUlasim.Items.Clear();
            foreach (var u in ulasimlar) cmbUlasim.Items.Add(u);
            for (int i = 0; i < cmbUlasim.Items.Count; i++)
                if (((Ulasim)cmbUlasim.Items[i]).UlasimID == _plan.UlasimID)
                { cmbUlasim.SelectedIndex = i; break; }

            var konaklamalar = _konaklamaRepo.GetBySehirId(_plan.SehirID);
            cmbKonaklama.Items.Clear();
            foreach (var k in konaklamalar) cmbKonaklama.Items.Add(k);
            for (int i = 0; i < cmbKonaklama.Items.Count; i++)
                if (((Konaklama)cmbKonaklama.Items[i]).KonaklamaID == _plan.KonaklamaID)
                { cmbKonaklama.SelectedIndex = i; break; }

            var tumYerler = _gezilecekRepo.GetBySehirId(_plan.SehirID);
            var secilenIds = _plan.SecilenYerler.Select(y => y.YerID).ToHashSet();
            clbGezilecek.Items.Clear();
            foreach (var yer in tumYerler)
            {
                int idx = clbGezilecek.Items.Add(yer);
                if (secilenIds.Contains(yer.YerID))
                    clbGezilecek.SetItemChecked(idx, true);
            }
        }

        private void BtnKaydet_Click(object? sender, EventArgs e)
        {
            if (cmbUlasim.SelectedItem == null || cmbKonaklama.SelectedItem == null)
            {
                MessageBox.Show("Lütfen tüm alanları seçin.", "Uyarı",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var ulasim = (Ulasim)cmbUlasim.SelectedItem;
            var konaklama = (Konaklama)cmbKonaklama.SelectedItem;

            var secilenYerler = new List<GezilecekYer>();
            foreach (var item in clbGezilecek.CheckedItems)
                secilenYerler.Add((GezilecekYer)item);

            _plan.UlasimID = ulasim.UlasimID;
            _plan.UlasimFiyat = ulasim.Fiyat;
            _plan.KonaklamaID = konaklama.KonaklamaID;
            _plan.KonaklamaFiyat = konaklama.GeceFiyat;
            _plan.SecilenYerler = secilenYerler;

            decimal geziToplam = secilenYerler.Sum(y => y.ZiyaretUcreti);
            _plan.ToplamMaliyet = ulasim.Fiyat + (konaklama.GeceFiyat * _plan.GeceSayisi) + geziToplam;

            try
            {
                _planRepo.Update(_plan);
                MessageBox.Show("Plan başarıyla güncellendi!", "Başarılı",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Güncelleme hatası:\n{ex.Message}", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
