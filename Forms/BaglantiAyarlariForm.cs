using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Npgsql;

namespace SmartTour.Forms
{
    public class BaglantiAyarlariForm : Form
    {
        private Panel pnlHeader;
        private Label lblBaslik;
        private Label lblAltBaslik;

        private Label lblHost;
        private TextBox txtHost;

        private Label lblPort;
        private TextBox txtPort;

        private Label lblDatabase;
        private TextBox txtDatabase;

        private Label lblUser;
        private TextBox txtUser;

        private Label lblPassword;
        private TextBox txtPassword;

        private Button btnKaydet;
        private Button btnIptal;

        public BaglantiAyarlariForm()
        {
            InitializeComponent();
            VarsayilanlariYukle();
        }

        private void InitializeComponent()
        {
            this.Text = "SmartTour - Veritabanı Bağlantı Ayarları";
            this.Size = new Size(480, 520);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Font = new Font("Segoe UI", 10F);

            pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.FromArgb(0, 120, 215)
            };
            this.Controls.Add(pnlHeader);

            lblBaslik = new Label
            {
                Text = "🔌 Veritabanı Bağlantı Ayarı",
                Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(10, 15),
                Size = new Size(460, 25),
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlHeader.Controls.Add(lblBaslik);

            lblAltBaslik = new Label
            {
                Text = "Bağlantı koptuğunda şifre ve host bilgilerini buradan güncelleyin.",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(200, 220, 255),
                Location = new Point(10, 45),
                Size = new Size(460, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlHeader.Controls.Add(lblAltBaslik);

            int y = 100;
            int labelX = 30;
            int inputX = 160;
            int inputW = 270;
            int spacing = 45;

            lblHost = new Label { Text = "Sunucu (Host):", Location = new Point(labelX, y), AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            txtHost = new TextBox { Location = new Point(inputX, y - 2), Size = new Size(inputW, 26) };
            this.Controls.Add(lblHost);
            this.Controls.Add(txtHost);
            y += spacing;

            lblPort = new Label { Text = "Port:", Location = new Point(labelX, y), AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            txtPort = new TextBox { Location = new Point(inputX, y - 2), Size = new Size(inputW, 26), Text = "5432" };
            this.Controls.Add(lblPort);
            this.Controls.Add(txtPort);
            y += spacing;

            lblDatabase = new Label { Text = "Veritabanı:", Location = new Point(labelX, y), AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            txtDatabase = new TextBox { Location = new Point(inputX, y - 2), Size = new Size(inputW, 26), Text = "postgres" };
            this.Controls.Add(lblDatabase);
            this.Controls.Add(txtDatabase);
            y += spacing;

            lblUser = new Label { Text = "Kullanıcı Adı:", Location = new Point(labelX, y), AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            txtUser = new TextBox { Location = new Point(inputX, y - 2), Size = new Size(inputW, 26), Text = "postgres" };
            this.Controls.Add(lblUser);
            this.Controls.Add(txtUser);
            y += spacing;

            lblPassword = new Label { Text = "Şifre (Password):", Location = new Point(labelX, y), AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            txtPassword = new TextBox { Location = new Point(inputX, y - 2), Size = new Size(inputW, 26), PasswordChar = '*' };
            this.Controls.Add(lblPassword);
            this.Controls.Add(txtPassword);
            y += 60;

            btnKaydet = new Button
            {
                Text = "💾 Bağlan ve Kaydet",
                Location = new Point(30, y),
                Size = new Size(240, 45),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnKaydet.FlatAppearance.BorderSize = 0;
            btnKaydet.Click += BtnKaydet_Click;
            this.Controls.Add(btnKaydet);

            btnIptal = new Button
            {
                Text = "İptal",
                Location = new Point(280, y),
                Size = new Size(150, 45),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnIptal.FlatAppearance.BorderSize = 0;
            btnIptal.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            this.Controls.Add(btnIptal);
        }

        private void VarsayilanlariYukle()
        {
            string dosyaYolu = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "db_config.txt");
            if (File.Exists(dosyaYolu))
            {
                try
                {
                    string connStr = File.ReadAllText(dosyaYolu).Trim();
                    var builder = new NpgsqlConnectionStringBuilder(connStr);
                    txtHost.Text = builder.Host;
                    txtPort.Text = builder.Port.ToString();
                    txtDatabase.Text = builder.Database;
                    txtUser.Text = builder.Username;
                    txtPassword.Text = builder.Password;
                }
                catch
                {
                    VarsayilanSupabaseDoldur();
                }
            }
            else
            {
                VarsayilanSupabaseDoldur();
            }
        }

        private void VarsayilanSupabaseDoldur()
        {
            txtHost.Text = "aws-1-eu-central-1.pooler.supabase.com";
            txtPort.Text = "5432";
            txtDatabase.Text = "postgres";
            txtUser.Text = "postgres.oexvtvmrtudiupnrsuky";
            txtPassword.Text = "gorselprojesifresi";
        }

        private void BtnKaydet_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtHost.Text) ||
                string.IsNullOrWhiteSpace(txtPort.Text) ||
                string.IsNullOrWhiteSpace(txtDatabase.Text) ||
                string.IsNullOrWhiteSpace(txtUser.Text) ||
                string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Lütfen tüm alanları doldurun.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string yeniConnStr = $"Host={txtHost.Text.Trim()};Port={txtPort.Text.Trim()};Database={txtDatabase.Text.Trim()};Username={txtUser.Text.Trim()};Password={txtPassword.Text.Trim()};";

            btnKaydet.Enabled = false;
            btnKaydet.Text = "🔄 Test Ediliyor...";

            try
            {
                using (var testConn = new NpgsqlConnection(yeniConnStr))
                {
                    testConn.Open(); // Bağlanmayı dene
                }

                string dosyaYolu = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "db_config.txt");
                File.WriteAllText(dosyaYolu, yeniConnStr);

                MessageBox.Show("Veritabanı bağlantısı başarıyla kuruldu ve ayarlar kaydedildi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Bağlantı başarısız! Lütfen bilgileri kontrol edin.\n\nHata detayı:\n{ex.Message}", "Bağlantı Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnKaydet.Enabled = true;
                btnKaydet.Text = "💾 Bağlan ve Kaydet";
            }
        }
    }
}
