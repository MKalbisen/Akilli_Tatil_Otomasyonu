using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SmartTour.DataAccess;
using SmartTour.Models;
using SmartTour.BusinessLogic;

namespace SmartTour.Forms
{
    public class PlanSecimForm : Form
    {
        private Panel pnlHeader;
        private Label lblBaslik;
        private ListBox lstPlanlar;
        private RichTextBox rtbDetay;

        private Label lblDuzenle;
        private ComboBox cmbUlasim;
        private ComboBox cmbKonaklama;
        private CheckedListBox clbGezilecek;
        private Button btnGuncelle;
        private Button btnKaydet;
        private Button btnGeri;

        private List<TurPlani> _planlar;
        private TurPlani? _secilenPlan;
        private int _sehirId;

        private UlasimRepository _ulasimRepo = new UlasimRepository();
        private KonaklamaRepository _konaklamaRepo = new KonaklamaRepository();
        private GezilecekYerRepository _gezilecekRepo = new GezilecekYerRepository();

        private List<Ulasim> _ulasimlar = new List<Ulasim>();
        private List<Konaklama> _konaklamalar = new List<Konaklama>();
        private List<GezilecekYer> _tumYerler = new List<GezilecekYer>();

        public PlanSecimForm(List<TurPlani> planlar, int sehirId)
        {
            _planlar = planlar;
            _sehirId = sehirId;
            InitializeComponent();
            LoadEditData();
            PopulatePlanList();
        }

        private void InitializeComponent()
        {
            var screen = Screen.PrimaryScreen!.WorkingArea;
            int formW = (int)(screen.Width * 0.65);
            int formH = (int)(screen.Height * 0.7);

            this.Text = "SmartTour - Plan Önerileri";
            this.ClientSize = new Size(formW, formH);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Font = new Font("Segoe UI", 11F);

            pnlHeader = new Panel { Dock = DockStyle.Top, Height = 65, BackColor = Color.FromArgb(0, 120, 215) };
            this.Controls.Add(pnlHeader);

            lblBaslik = new Label
            {
                Text = $"🤖 Bütçenize Uygun Plan Önerileri ({_planlar.Count} öneri)",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false, Size = new Size(formW - 20, 45),
                Location = new Point(10, 10),
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlHeader.Controls.Add(lblBaslik);

            int topH = (int)((formH - 65) * 0.38);
            int listW = (int)(formW * 0.38);
            int detayW = formW - listW - 50;

            this.Controls.Add(new Label
            {
                Text = "📋 Önerilen Planlar:", Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Location = new Point(15, 78), AutoSize = true
            });

            lstPlanlar = new ListBox
            {
                Location = new Point(15, 102),
                Size = new Size(listW, topH),
                Font = new Font("Segoe UI", 11F),
                BorderStyle = BorderStyle.FixedSingle
            };
            lstPlanlar.SelectedIndexChanged += LstPlanlar_SelectedIndexChanged;
            this.Controls.Add(lstPlanlar);

            this.Controls.Add(new Label
            {
                Text = "📄 Plan Detayı:", Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Location = new Point(listW + 30, 78), AutoSize = true
            });

            rtbDetay = new RichTextBox
            {
                Location = new Point(listW + 30, 102),
                Size = new Size(detayW, topH),
                Font = new Font("Consolas", 11F),
                ReadOnly = true, BackColor = Color.FromArgb(250, 250, 255),
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(rtbDetay);

            int editY = 102 + topH + 15;
            lblDuzenle = new Label
            {
                Text = "✏️ Seçili Planı Düzenle:", Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Location = new Point(15, editY), AutoSize = true
            };
            this.Controls.Add(lblDuzenle);
            editY += 30;

            int halfEdit = (formW - 100) / 2;

            this.Controls.Add(new Label
            {
                Text = "🚌 Ulaşım:", Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Location = new Point(15, editY), AutoSize = true
            });
            cmbUlasim = new ComboBox
            {
                Location = new Point(120, editY - 2), Size = new Size(halfEdit - 30, 30),
                DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 11F)
            };
            this.Controls.Add(cmbUlasim);

            this.Controls.Add(new Label
            {
                Text = "🏨 Konaklama:", Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Location = new Point(halfEdit + 110, editY), AutoSize = true
            });
            cmbKonaklama = new ComboBox
            {
                Location = new Point(halfEdit + 250, editY - 2), Size = new Size(halfEdit - 30, 30),
                DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 11F)
            };
            this.Controls.Add(cmbKonaklama);

            btnGuncelle = new Button
            {
                Text = "🔄", Location = new Point(formW - 65, editY - 3), Size = new Size(45, 32),
                Font = new Font("Segoe UI", 13F), BackColor = Color.FromArgb(255, 165, 0),
                ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand
            };
            btnGuncelle.FlatAppearance.BorderSize = 0;
            btnGuncelle.Click += BtnGuncelle_Click;
            var ttGuncelle = new ToolTip();
            ttGuncelle.SetToolTip(btnGuncelle, "Seçimleri uygula");
            this.Controls.Add(btnGuncelle);
            editY += 38;

            this.Controls.Add(new Label
            {
                Text = "📍 Gezilecek Yerler:", Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Location = new Point(15, editY), AutoSize = true
            });
            editY += 26;

            int gezH = formH - editY - 90;
            clbGezilecek = new CheckedListBox
            {
                Location = new Point(15, editY),
                Size = new Size(formW - 40, gezH),
                Font = new Font("Segoe UI", 11F),
                CheckOnClick = true,
                MultiColumn = true, ColumnWidth = (formW - 40) / 3
            };
            this.Controls.Add(clbGezilecek);
            editY += gezH + 8;

            int btnW = (formW - 45) / 2;

            btnKaydet = new Button
            {
                Text = "💾 Planı Kaydet",
                Location = new Point(15, editY), Size = new Size(btnW, 50),
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                BackColor = Color.FromArgb(40, 167, 69), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand
            };
            btnKaydet.FlatAppearance.BorderSize = 0;
            btnKaydet.Click += BtnKaydet_Click;
            btnKaydet.MouseEnter += (s, e) => btnKaydet.BackColor = Color.FromArgb(30, 140, 55);
            btnKaydet.MouseLeave += (s, e) => btnKaydet.BackColor = Color.FromArgb(40, 167, 69);
            this.Controls.Add(btnKaydet);

            btnGeri = new Button
            {
                Text = "← Geri",
                Location = new Point(btnW + 30, editY), Size = new Size(btnW, 50),
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                BackColor = Color.FromArgb(108, 117, 125), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand
            };
            btnGeri.FlatAppearance.BorderSize = 0;
            btnGeri.Click += (s, e) => this.Close();
            btnGeri.MouseEnter += (s, e) => btnGeri.BackColor = Color.FromArgb(80, 90, 100);
            btnGeri.MouseLeave += (s, e) => btnGeri.BackColor = Color.FromArgb(108, 117, 125);
            this.Controls.Add(btnGeri);
        }

        private void LoadEditData()
        {
            _ulasimlar = _ulasimRepo.GetBySehirId(_sehirId);
            _konaklamalar = _konaklamaRepo.GetBySehirId(_sehirId);
            _tumYerler = _gezilecekRepo.GetBySehirId(_sehirId);

            cmbUlasim.Items.Clear();
            foreach (var u in _ulasimlar) cmbUlasim.Items.Add(u);

            cmbKonaklama.Items.Clear();
            foreach (var k in _konaklamalar) cmbKonaklama.Items.Add(k);

            clbGezilecek.Items.Clear();
            foreach (var y in _tumYerler) clbGezilecek.Items.Add(y);
        }

        private void PopulatePlanList()
        {
            lstPlanlar.Items.Clear();
            for (int i = 0; i < _planlar.Count; i++)
            {
                var p = _planlar[i];
                lstPlanlar.Items.Add($"Plan {i + 1}: {p.UlasimTuru} + {p.KonaklamaAdi} = {p.ToplamMaliyet:N0}₺ ({p.SecilenYerler.Count} yer)");
            }
            if (lstPlanlar.Items.Count > 0)
                lstPlanlar.SelectedIndex = 0;
        }

        private void LstPlanlar_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (lstPlanlar.SelectedIndex < 0 || lstPlanlar.SelectedIndex >= _planlar.Count) return;
            _secilenPlan = _planlar[lstPlanlar.SelectedIndex];
            ShowPlanDetail(_secilenPlan);
            SyncEditControls(_secilenPlan);
        }

        private void ShowPlanDetail(TurPlani plan)
        {
            string d = "";
            d += $"📋 Plan Detayı – {plan.SehirAdi}\n";
            d += $"━━━━━━━━━━━━━━━━━━━━━━━━━━━\n";
            d += $"🚌 Ulaşım     : {plan.UlasimTuru,-20} {plan.UlasimFiyat,10:N2} ₺\n";
            d += $"🏨 Konaklama  : {plan.KonaklamaAdi,-20} {plan.KonaklamaFiyat,10:N2} ₺/gece\n";
            d += $"🌙 Gece       : {plan.GeceSayisi} gece = {plan.KonaklamaFiyat * plan.GeceSayisi,10:N2} ₺\n";
            d += $"━━━━━━━━━━━━━━━━━━━━━━━━━━━\n";

            if (plan.SecilenYerler.Count > 0)
            {
                d += "📍 Gezilecek Yerler:\n";
                foreach (var yer in plan.SecilenYerler)
                    d += $"   • {yer.YerAdi,-28} {yer.ZiyaretUcreti,8:N2} ₺\n";
                d += $"   Gezi Toplam: {plan.SecilenYerler.Sum(y => y.ZiyaretUcreti),18:N2} ₺\n";
            }

            d += $"━━━━━━━━━━━━━━━━━━━━━━━━━━━\n";
            d += $"💰 TOPLAM     : {plan.ToplamMaliyet,22:N2} ₺\n";
            d += $"💼 BÜTÇE      : {plan.Butce,22:N2} ₺\n";
            decimal kalan = plan.Butce - plan.ToplamMaliyet;
            d += $"📊 KALAN      : {kalan,22:N2} ₺\n";

            rtbDetay.Text = d;
        }

        private void SyncEditControls(TurPlani plan)
        {
            for (int i = 0; i < cmbUlasim.Items.Count; i++)
                if (((Ulasim)cmbUlasim.Items[i]).UlasimID == plan.UlasimID)
                { cmbUlasim.SelectedIndex = i; break; }

            for (int i = 0; i < cmbKonaklama.Items.Count; i++)
                if (((Konaklama)cmbKonaklama.Items[i]).KonaklamaID == plan.KonaklamaID)
                { cmbKonaklama.SelectedIndex = i; break; }

            var secilenIds = plan.SecilenYerler.Select(y => y.YerID).ToHashSet();
            for (int i = 0; i < clbGezilecek.Items.Count; i++)
            {
                var yer = (GezilecekYer)clbGezilecek.Items[i];
                clbGezilecek.SetItemChecked(i, secilenIds.Contains(yer.YerID));
            }
        }

        private void BtnGuncelle_Click(object? sender, EventArgs e)
        {
            if (_secilenPlan == null || cmbUlasim.SelectedItem == null || cmbKonaklama.SelectedItem == null) return;

            var ulasim = (Ulasim)cmbUlasim.SelectedItem;
            var konaklama = (Konaklama)cmbKonaklama.SelectedItem;

            var secilenYerler = new List<GezilecekYer>();
            foreach (var item in clbGezilecek.CheckedItems)
                secilenYerler.Add((GezilecekYer)item);

            _secilenPlan.UlasimID = ulasim.UlasimID;
            _secilenPlan.UlasimTuru = ulasim.UlasimTuru;
            _secilenPlan.UlasimFiyat = ulasim.Fiyat;
            _secilenPlan.KonaklamaID = konaklama.KonaklamaID;
            _secilenPlan.KonaklamaAdi = konaklama.KonaklamaAdi;
            _secilenPlan.KonaklamaFiyat = konaklama.GeceFiyat;
            _secilenPlan.SecilenYerler = secilenYerler;

            var servis = new TurPlanlamaServisi();
            _secilenPlan.ToplamMaliyet = servis.ToplamMaliyetHesapla(
                ulasim.Fiyat, konaklama.GeceFiyat, _secilenPlan.GeceSayisi, secilenYerler);

            int idx = lstPlanlar.SelectedIndex;
            lstPlanlar.Items[idx] = $"Plan {idx + 1}: {_secilenPlan.UlasimTuru} + {_secilenPlan.KonaklamaAdi} = {_secilenPlan.ToplamMaliyet:N0}₺ ({_secilenPlan.SecilenYerler.Count} yer) ✏️";
            ShowPlanDetail(_secilenPlan);

            MessageBox.Show("Plan güncellendi! Kaydetmek için 'Planı Kaydet' butonunu kullanın.",
                "Güncellendi", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnKaydet_Click(object? sender, EventArgs e)
        {
            if (_secilenPlan == null)
            {
                MessageBox.Show("Lütfen bir plan seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var repo = new TurPlaniRepository();
                repo.Save(_secilenPlan);

                MessageBox.Show("Plan veritabanına başarıyla kaydedildi!",
                    "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);

                btnKaydet.Enabled = false;
                btnKaydet.Text = "✓ Kaydedildi";
                btnKaydet.BackColor = Color.FromArgb(150, 150, 150);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kayıt hatası:\n{ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
