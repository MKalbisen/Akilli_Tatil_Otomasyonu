using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Npgsql;
using SmartTour.DataAccess;
using SmartTour.Models;

namespace SmartTour.Forms
{
    public class AdminPanelForm : Form
    {
        private Panel pnlHeader;
        private Label lblBaslik;
        private TabControl tabControl;

        private TabPage tpSehirler;
        private DataGridView dgvSehirler;
        private TextBox txtSehirAdi;
        private Button btnSehirEkle;
        private Button btnSehirSil;

        private TabPage tpUlasim;
        private ComboBox cmbUlasimSehir;
        private DataGridView dgvUlasim;
        private TextBox txtUlasimTuru;
        private NumericUpDown nudUlasimFiyat;
        private Button btnUlasimEkle;
        private Button btnUlasimSil;

        private TabPage tpKonaklama;
        private ComboBox cmbKonaklamaSehir;
        private DataGridView dgvKonaklama;
        private TextBox txtKonaklamaAdi;
        private TextBox txtKonaklamaTuru;
        private NumericUpDown nudKonaklamaFiyat;
        private Button btnKonaklamaEkle;
        private Button btnKonaklamaSil;

        private TabPage tpGezilecek;
        private ComboBox cmbGezilecekSehir;
        private DataGridView dgvGezilecek;
        private TextBox txtYerAdi;
        private NumericUpDown nudYerFiyat;
        private TextBox txtYerAciklama;
        private Button btnGezilecekEkle;
        private Button btnGezilecekSil;

        private Button btnKapat;

        public AdminPanelForm()
        {
            InitializeComponent();
            TumSehirleriYukle();
            SehirleriListele();
        }

        private void InitializeComponent()
        {
            this.Text = "SmartTour - Yönetici Arayüzü 🛠️";
            this.Size = new Size(800, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Font = new Font("Segoe UI", 10F);

            pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.FromArgb(220, 100, 0) // Turuncu yönetici teması
            };
            this.Controls.Add(pnlHeader);

            lblBaslik = new Label
            {
                Text = "🛠️ SmartTour Yönetici Arayüzü",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(10, 15),
                Size = new Size(760, 40),
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlHeader.Controls.Add(lblBaslik);

            tabControl = new TabControl
            {
                Location = new Point(15, 85),
                Size = new Size(755, 450),
                Font = new Font("Segoe UI", 10F)
            };
            this.Controls.Add(tabControl);

            SekmeSehirlerOlustur();
            SekmeUlasimOlustur();
            SekmeKonaklamaOlustur();
            SekmeGezilecekOlustur();

            btnKapat = new Button
            {
                Text = "← Paneli Kapat",
                Location = new Point(15, 545),
                Size = new Size(180, 45),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnKapat.FlatAppearance.BorderSize = 0;
            btnKapat.Click += (s, e) => this.Close();
            this.Controls.Add(btnKapat);
        }

        #region Sekme Tasarımları

        private void SekmeSehirlerOlustur()
        {
            tpSehirler = new TabPage("🌆 Şehirler");
            tpSehirler.BackColor = Color.White;

            dgvSehirler = CreateGridView(15, 15, 420, 380);
            tpSehirler.Controls.Add(dgvSehirler);

            int x = 455;
            int y = 20;

            tpSehirler.Controls.Add(new Label { Text = "Şehir Adı:", Location = new Point(x, y), Font = new Font("Segoe UI", 10F, FontStyle.Bold), AutoSize = true });
            txtSehirAdi = new TextBox { Location = new Point(x, y + 25), Size = new Size(260, 26) };
            tpSehirler.Controls.Add(txtSehirAdi);

            btnSehirEkle = CreateButton("➕ Şehir Ekle", x, y + 70, 260, 40, Color.FromArgb(40, 167, 69), BtnSehirEkle_Click);
            tpSehirler.Controls.Add(btnSehirEkle);

            btnSehirSil = CreateButton("🗑️ Seçili Şehri Sil", x, y + 120, 260, 40, Color.FromArgb(220, 53, 69), BtnSehirSil_Click);
            tpSehirler.Controls.Add(btnSehirSil);

            tabControl.TabPages.Add(tpSehirler);
        }

        private void SekmeUlasimOlustur()
        {
            tpUlasim = new TabPage("🚌 Ulaşımlar");
            tpUlasim.BackColor = Color.White;

            tpUlasim.Controls.Add(new Label { Text = "📍 Şehir Filtresi:", Location = new Point(15, 18), Font = new Font("Segoe UI", 10F, FontStyle.Bold), AutoSize = true });
            cmbUlasimSehir = new ComboBox { Location = new Point(125, 15), Size = new Size(200, 26), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbUlasimSehir.SelectedIndexChanged += (s, e) => UlasimleriListele();
            tpUlasim.Controls.Add(cmbUlasimSehir);

            dgvUlasim = CreateGridView(15, 55, 420, 340);
            tpUlasim.Controls.Add(dgvUlasim);

            int x = 455;
            int y = 55;

            tpUlasim.Controls.Add(new Label { Text = "Ulaşım Türü (Örn: Uçak):", Location = new Point(x, y), Font = new Font("Segoe UI", 10F, FontStyle.Bold), AutoSize = true });
            txtUlasimTuru = new TextBox { Location = new Point(x, y + 25), Size = new Size(260, 26) };
            tpUlasim.Controls.Add(txtUlasimTuru);

            tpUlasim.Controls.Add(new Label { Text = "Bilet Fiyatı (₺):", Location = new Point(x, y + 65), Font = new Font("Segoe UI", 10F, FontStyle.Bold), AutoSize = true });
            nudUlasimFiyat = new NumericUpDown { Location = new Point(x, y + 90), Size = new Size(260, 26), Maximum = 50000, Minimum = 50, Value = 500 };
            tpUlasim.Controls.Add(nudUlasimFiyat);

            btnUlasimEkle = CreateButton("➕ Ulaşım Ekle", x, y + 140, 260, 40, Color.FromArgb(40, 167, 69), BtnUlasimEkle_Click);
            tpUlasim.Controls.Add(btnUlasimEkle);

            btnUlasimSil = CreateButton("🗑️ Seçili Ulaşımı Sil", x, y + 190, 260, 40, Color.FromArgb(220, 53, 69), BtnUlasimSil_Click);
            tpUlasim.Controls.Add(btnUlasimSil);

            tabControl.TabPages.Add(tpUlasim);
        }

        private void SekmeKonaklamaOlustur()
        {
            tpKonaklama = new TabPage("🏨 Konaklamalar");
            tpKonaklama.BackColor = Color.White;

            tpKonaklama.Controls.Add(new Label { Text = "📍 Şehir Filtresi:", Location = new Point(15, 18), Font = new Font("Segoe UI", 10F, FontStyle.Bold), AutoSize = true });
            cmbKonaklamaSehir = new ComboBox { Location = new Point(125, 15), Size = new Size(200, 26), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbKonaklamaSehir.SelectedIndexChanged += (s, e) => KonaklamalariListele();
            tpKonaklama.Controls.Add(cmbKonaklamaSehir);

            dgvKonaklama = CreateGridView(15, 55, 420, 340);
            tpKonaklama.Controls.Add(dgvKonaklama);

            int x = 455;
            int y = 55;

            tpKonaklama.Controls.Add(new Label { Text = "Otel/Pansiyon Adı:", Location = new Point(x, y), Font = new Font("Segoe UI", 10F, FontStyle.Bold), AutoSize = true });
            txtKonaklamaAdi = new TextBox { Location = new Point(x, y + 25), Size = new Size(260, 26) };
            tpKonaklama.Controls.Add(txtKonaklamaAdi);

            tpKonaklama.Controls.Add(new Label { Text = "Tesis Türü (Ötel/Pansiyon):", Location = new Point(x, y + 65), Font = new Font("Segoe UI", 10F, FontStyle.Bold), AutoSize = true });
            txtKonaklamaTuru = new TextBox { Location = new Point(x, y + 90), Size = new Size(260, 26) };
            tpKonaklama.Controls.Add(txtKonaklamaTuru);

            tpKonaklama.Controls.Add(new Label { Text = "Gecelik Fiyat (₺):", Location = new Point(x, y + 130), Font = new Font("Segoe UI", 10F, FontStyle.Bold), AutoSize = true });
            nudKonaklamaFiyat = new NumericUpDown { Location = new Point(x, y + 155), Size = new Size(260, 26), Maximum = 100000, Minimum = 50, Value = 1000 };
            tpKonaklama.Controls.Add(nudKonaklamaFiyat);

            btnKonaklamaEkle = CreateButton("➕ Konaklama Ekle", x, y + 200, 260, 40, Color.FromArgb(40, 167, 69), BtnKonaklamaEkle_Click);
            tpKonaklama.Controls.Add(btnKonaklamaEkle);

            btnKonaklamaSil = CreateButton("🗑️ Seçili Konaklamayı Sil", x, y + 250, 260, 40, Color.FromArgb(220, 53, 69), BtnKonaklamaSil_Click);
            tpKonaklama.Controls.Add(btnKonaklamaSil);

            tabControl.TabPages.Add(tpKonaklama);
        }

        private void SekmeGezilecekOlustur()
        {
            tpGezilecek = new TabPage("📍 Gezilecek Yerler");
            tpGezilecek.BackColor = Color.White;

            tpGezilecek.Controls.Add(new Label { Text = "📍 Şehir Filtresi:", Location = new Point(15, 18), Font = new Font("Segoe UI", 10F, FontStyle.Bold), AutoSize = true });
            cmbGezilecekSehir = new ComboBox { Location = new Point(125, 15), Size = new Size(200, 26), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbGezilecekSehir.SelectedIndexChanged += (s, e) => GezilecekYerleriListele();
            tpGezilecek.Controls.Add(cmbGezilecekSehir);

            dgvGezilecek = CreateGridView(15, 55, 420, 340);
            tpGezilecek.Controls.Add(dgvGezilecek);

            int x = 455;
            int y = 55;

            tpGezilecek.Controls.Add(new Label { Text = "Gezilecek Nokta Adı:", Location = new Point(x, y), Font = new Font("Segoe UI", 10F, FontStyle.Bold), AutoSize = true });
            txtYerAdi = new TextBox { Location = new Point(x, y + 25), Size = new Size(260, 26) };
            tpGezilecek.Controls.Add(txtYerAdi);

            tpGezilecek.Controls.Add(new Label { Text = "Giriş/Ziyaret Ücreti (₺):", Location = new Point(x, y + 65), Font = new Font("Segoe UI", 10F, FontStyle.Bold), AutoSize = true });
            nudYerFiyat = new NumericUpDown { Location = new Point(x, y + 90), Size = new Size(260, 26), Maximum = 50000, Minimum = 0, Value = 150 };
            tpGezilecek.Controls.Add(nudYerFiyat);

            tpGezilecek.Controls.Add(new Label { Text = "Açıklama / Detay:", Location = new Point(x, y + 130), Font = new Font("Segoe UI", 10F, FontStyle.Bold), AutoSize = true });
            txtYerAciklama = new TextBox { Location = new Point(x, y + 155), Size = new Size(260, 26) };
            tpGezilecek.Controls.Add(txtYerAciklama);

            btnGezilecekEkle = CreateButton("➕ Nokta Ekle", x, y + 200, 260, 40, Color.FromArgb(40, 167, 69), BtnGezilecekEkle_Click);
            tpGezilecek.Controls.Add(btnGezilecekEkle);

            btnGezilecekSil = CreateButton("🗑️ Seçili Noktayı Sil", x, y + 250, 260, 40, Color.FromArgb(220, 53, 69), BtnGezilecekSil_Click);
            tpGezilecek.Controls.Add(btnGezilecekSil);

            tabControl.TabPages.Add(tpGezilecek);
        }

        #endregion

        #region Yardımcı UI Metotları

        private DataGridView CreateGridView(int x, int y, int w, int h)
        {
            var dgv = new DataGridView
            {
                Location = new Point(x, y),
                Size = new Size(w, h),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                EnableHeadersVisualStyles = false
            };

            dgv.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter
            };

            dgv.CellFormatting += (s, e) =>
            {
                if (e.RowIndex % 2 == 0)
                    e.CellStyle!.BackColor = Color.FromArgb(248, 249, 252);
                else
                    e.CellStyle!.BackColor = Color.White;
            };

            return dgv;
        }

        private Button CreateButton(string text, int x, int y, int w, int h, Color color, EventHandler onClick)
        {
            var btn = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(w, h),
                BackColor = color,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += onClick;
            return btn;
        }

        #endregion

        #region Veritabanı Listeleme Metotları

        private void TumSehirleriYukle()
        {
            try
            {
                var sehirler = new SehirRepository().GetAll();

                cmbUlasimSehir.Items.Clear();
                cmbKonaklamaSehir.Items.Clear();
                cmbGezilecekSehir.Items.Clear();

                foreach (var s in sehirler)
                {
                    cmbUlasimSehir.Items.Add(s);
                    cmbKonaklamaSehir.Items.Add(s);
                    cmbGezilecekSehir.Items.Add(s);
                }

                if (cmbUlasimSehir.Items.Count > 0)
                {
                    cmbUlasimSehir.SelectedIndex = 0;
                    cmbKonaklamaSehir.SelectedIndex = 0;
                    cmbGezilecekSehir.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Şehirler yüklenirken hata oluştu:\n{ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SehirleriListele()
        {
            try
            {
                dgvSehirler.DataSource = null;
                dgvSehirler.Columns.Clear();

                var list = new SehirRepository().GetAll();
                dgvSehirler.Columns.Add("SehirID", "Şehir ID");
                dgvSehirler.Columns.Add("SehirAdi", "Şehir Adı");

                foreach (var s in list)
                {
                    dgvSehirler.Rows.Add(s.SehirID, s.SehirAdi);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UlasimleriListele()
        {
            if (cmbUlasimSehir.SelectedItem == null) return;
            var sehir = (Sehir)cmbUlasimSehir.SelectedItem;

            try
            {
                dgvUlasim.DataSource = null;
                dgvUlasim.Columns.Clear();

                var list = new UlasimRepository().GetBySehirId(sehir.SehirID);
                dgvUlasim.Columns.Add("UlasimID", "Ulaşım ID");
                dgvUlasim.Columns.Add("UlasimTuru", "Ulaşım Türü");
                dgvUlasim.Columns.Add("Fiyat", "Fiyat (₺)");

                foreach (var u in list)
                {
                    dgvUlasim.Rows.Add(u.UlasimID, u.UlasimTuru, $"{u.Fiyat:N2}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void KonaklamalariListele()
        {
            if (cmbKonaklamaSehir.SelectedItem == null) return;
            var sehir = (Sehir)cmbKonaklamaSehir.SelectedItem;

            try
            {
                dgvKonaklama.DataSource = null;
                dgvKonaklama.Columns.Clear();

                var list = new KonaklamaRepository().GetBySehirId(sehir.SehirID);
                dgvKonaklama.Columns.Add("KonaklamaID", "ID");
                dgvKonaklama.Columns.Add("KonaklamaAdi", "Tesis Adı");
                dgvKonaklama.Columns.Add("KonaklamaTuru", "Türü");
                dgvKonaklama.Columns.Add("GeceFiyat", "Gecelik (₺)");

                foreach (var k in list)
                {
                    dgvKonaklama.Rows.Add(k.KonaklamaID, k.KonaklamaAdi, k.KonaklamaTuru, $"{k.GeceFiyat:N2}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GezilecekYerleriListele()
        {
            if (cmbGezilecekSehir.SelectedItem == null) return;
            var sehir = (Sehir)cmbGezilecekSehir.SelectedItem;

            try
            {
                dgvGezilecek.DataSource = null;
                dgvGezilecek.Columns.Clear();

                var list = new GezilecekYerRepository().GetBySehirId(sehir.SehirID);
                dgvGezilecek.Columns.Add("YerID", "ID");
                dgvGezilecek.Columns.Add("YerAdi", "Ziyaret Noktası");
                dgvGezilecek.Columns.Add("ZiyaretUcreti", "Giriş Ücreti (₺)");
                dgvGezilecek.Columns.Add("Aciklama", "Açıklama");

                foreach (var y in list)
                {
                    dgvGezilecek.Rows.Add(y.YerID, y.YerAdi, $"{y.ZiyaretUcreti:N2}", y.Aciklama);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Veritabanı Ekle / Sil (CRUD) Eventleri

        private void BtnSehirEkle_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSehirAdi.Text))
            {
                MessageBox.Show("Lütfen geçerli bir şehir adı girin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using var conn = DatabaseHelper.GetConnection();
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "INSERT INTO Sehirler (SehirAdi) VALUES (@SehirAdi)";
                cmd.Parameters.AddWithValue("@SehirAdi", txtSehirAdi.Text.Trim());
                cmd.ExecuteNonQuery();

                MessageBox.Show("Şehir başarıyla eklendi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtSehirAdi.Clear();
                SehirleriListele();
                TumSehirleriYukle();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSehirSil_Click(object? sender, EventArgs e)
        {
            if (dgvSehirler.SelectedRows.Count == 0)
            {
                MessageBox.Show("Silinecek şehri tablodan seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int sehirId = Convert.ToInt32(dgvSehirler.SelectedRows[0].Cells["SehirID"].Value);
            string sehirAdi = Convert.ToString(dgvSehirler.SelectedRows[0].Cells["SehirAdi"].Value) ?? "";

            var res = MessageBox.Show($"'{sehirAdi}' şehrini silmek istediğinize emin misiniz?\nUyarı: Bu şehre bağlı tüm konaklama, ulaşım ve gezilecek yer kayıtları da silinecektir!", 
                "Silme Onayı", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (res == DialogResult.No) return;

            try
            {
                using var conn = DatabaseHelper.GetConnection();
                conn.Open();

                using (var cmdDel = conn.CreateCommand())
                {
                    cmdDel.CommandText = @"
                        DELETE FROM PlanGezilecekYerler WHERE YerID IN (SELECT YerID FROM GezilecekYerler WHERE SehirID = @SehirID);
                        DELETE FROM GezilecekYerler WHERE SehirID = @SehirID;
                        DELETE FROM Konaklama WHERE SehirID = @SehirID;
                        DELETE FROM Ulasim WHERE SehirID = @SehirID;
                        DELETE FROM TurPlanlari WHERE SehirID = @SehirID;
                        DELETE FROM Sehirler WHERE SehirID = @SehirID;
                    ";
                    cmdDel.Parameters.AddWithValue("@SehirID", sehirId);
                    cmdDel.ExecuteNonQuery();
                }

                MessageBox.Show("Şehir ve ilişkili tüm kayıtlar silindi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                SehirleriListele();
                TumSehirleriYukle();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnUlasimEkle_Click(object? sender, EventArgs e)
        {
            if (cmbUlasimSehir.SelectedItem == null || string.IsNullOrWhiteSpace(txtUlasimTuru.Text)) return;
            var sehir = (Sehir)cmbUlasimSehir.SelectedItem;

            try
            {
                using var conn = DatabaseHelper.GetConnection();
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "INSERT INTO Ulasim (SehirID, UlasimTuru, Fiyat) VALUES (@SehirID, @UlasimTuru, @Fiyat)";
                cmd.Parameters.AddWithValue("@SehirID", sehir.SehirID);
                cmd.Parameters.AddWithValue("@UlasimTuru", txtUlasimTuru.Text.Trim());
                cmd.Parameters.AddWithValue("@Fiyat", nudUlasimFiyat.Value);
                cmd.ExecuteNonQuery();

                MessageBox.Show("Ulaşım seçeneği eklendi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtUlasimTuru.Clear();
                UlasimleriListele();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnUlasimSil_Click(object? sender, EventArgs e)
        {
            if (dgvUlasim.SelectedRows.Count == 0) return;
            int ulasimId = Convert.ToInt32(dgvUlasim.SelectedRows[0].Cells["UlasimID"].Value);

            try
            {
                using var conn = DatabaseHelper.GetConnection();
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM Ulasim WHERE UlasimID = @UlasimID";
                cmd.Parameters.AddWithValue("@UlasimID", ulasimId);
                cmd.ExecuteNonQuery();

                MessageBox.Show("Ulaşım kaydı silindi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                UlasimleriListele();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnKonaklamaEkle_Click(object? sender, EventArgs e)
        {
            if (cmbKonaklamaSehir.SelectedItem == null || string.IsNullOrWhiteSpace(txtKonaklamaAdi.Text) || string.IsNullOrWhiteSpace(txtKonaklamaTuru.Text)) return;
            var sehir = (Sehir)cmbKonaklamaSehir.SelectedItem;

            try
            {
                using var conn = DatabaseHelper.GetConnection();
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "INSERT INTO Konaklama (SehirID, KonaklamaAdi, KonaklamaTuru, GeceFiyat) VALUES (@SehirID, @KonaklamaAdi, @KonaklamaTuru, @GeceFiyat)";
                cmd.Parameters.AddWithValue("@SehirID", sehir.SehirID);
                cmd.Parameters.AddWithValue("@KonaklamaAdi", txtKonaklamaAdi.Text.Trim());
                cmd.Parameters.AddWithValue("@KonaklamaTuru", txtKonaklamaTuru.Text.Trim());
                cmd.Parameters.AddWithValue("@GeceFiyat", nudKonaklamaFiyat.Value);
                cmd.ExecuteNonQuery();

                MessageBox.Show("Konaklama seçeneği eklendi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtKonaklamaAdi.Clear();
                txtKonaklamaTuru.Clear();
                KonaklamalariListele();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnKonaklamaSil_Click(object? sender, EventArgs e)
        {
            if (dgvKonaklama.SelectedRows.Count == 0) return;
            int konaklamaId = Convert.ToInt32(dgvKonaklama.SelectedRows[0].Cells["KonaklamaID"].Value);

            try
            {
                using var conn = DatabaseHelper.GetConnection();
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM Konaklama WHERE KonaklamaID = @KonaklamaID";
                cmd.Parameters.AddWithValue("@KonaklamaID", konaklamaId);
                cmd.ExecuteNonQuery();

                MessageBox.Show("Konaklama seçeneği silindi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                KonaklamalariListele();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnGezilecekEkle_Click(object? sender, EventArgs e)
        {
            if (cmbGezilecekSehir.SelectedItem == null || string.IsNullOrWhiteSpace(txtYerAdi.Text)) return;
            var sehir = (Sehir)cmbGezilecekSehir.SelectedItem;

            try
            {
                using var conn = DatabaseHelper.GetConnection();
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "INSERT INTO GezilecekYerler (SehirID, YerAdi, ZiyaretUcreti, Aciklama) VALUES (@SehirID, @YerAdi, @ZiyaretUcreti, @Aciklama)";
                cmd.Parameters.AddWithValue("@SehirID", sehir.SehirID);
                cmd.Parameters.AddWithValue("@YerAdi", txtYerAdi.Text.Trim());
                cmd.Parameters.AddWithValue("@ZiyaretUcreti", nudYerFiyat.Value);
                cmd.Parameters.AddWithValue("@Aciklama", txtYerAciklama.Text.Trim());
                cmd.ExecuteNonQuery();

                MessageBox.Show("Gezilecek nokta başarıyla eklendi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtYerAdi.Clear();
                txtYerAciklama.Clear();
                GezilecekYerleriListele();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnGezilecekSil_Click(object? sender, EventArgs e)
        {
            if (dgvGezilecek.SelectedRows.Count == 0) return;
            int yerId = Convert.ToInt32(dgvGezilecek.SelectedRows[0].Cells["YerID"].Value);

            try
            {
                using var conn = DatabaseHelper.GetConnection();
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM GezilecekYerler WHERE YerID = @YerID";
                cmd.Parameters.AddWithValue("@YerID", yerId);
                cmd.ExecuteNonQuery();

                MessageBox.Show("Gezilecek yer silindi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                GezilecekYerleriListele();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion
    }
}
