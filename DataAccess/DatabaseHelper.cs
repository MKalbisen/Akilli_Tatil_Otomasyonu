using Npgsql;
using System;
using System.IO;
using System.Windows.Forms;

namespace SmartTour.DataAccess
{
    public static class DatabaseHelper
    {
        private static string _connectionString = "";

        public static string GetConnectionString()
        {
            if (!string.IsNullOrEmpty(_connectionString))
                return _connectionString;

            string dosyaYolu = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "db_config.txt");
            if (File.Exists(dosyaYolu))
            {
                try
                {
                    string okunan = File.ReadAllText(dosyaYolu).Trim();
                    if (!string.IsNullOrEmpty(okunan))
                    {
                        _connectionString = okunan;
                        return _connectionString;
                    }
                }
                catch
                {
                }
            }

            _connectionString = "Host=aws-1-eu-central-1.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.oexvtvmrtudiupnrsuky;Password=gorselprojesifre;";
            return _connectionString;
        }

        public static NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(GetConnectionString());
        }

        public static void InitializeDatabase()
        {
            bool basarili = false;
            int denemeSayisi = 0;

            while (!basarili && denemeSayisi < 3)
            {
                try
                {
                    using (var conn = GetConnection())
                    {
                        conn.Open();
                    }
                    basarili = true;
                }
                catch (Exception ex)
                {
                    denemeSayisi++;
                    MessageBox.Show(
                        $"Veritabanına bağlanılamadı. Lütfen bağlantı ayarlarını kontrol edin.\nHata: {ex.Message}",
                        "Bağlantı Hatası",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    using (var ayarForm = new Forms.BaglantiAyarlariForm())
                    {
                        var result = ayarForm.ShowDialog();
                        if (result != DialogResult.OK)
                        {
                            throw new Exception("Kullanıcı veritabanı ayarlarını yapılandırmayı iptal etti.");
                        }
                    }
                    _connectionString = "";
                }
            }

            string aktifBaglanti = GetConnectionString();
            if (aktifBaglanti.Contains("localhost") || aktifBaglanti.Contains("127.0.0.1"))
            {
                EnsureDatabaseExists();
            }

            using var activeConn = GetConnection();
            activeConn.Open();

            using var cmd = activeConn.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Sehirler (
                    SehirID SERIAL PRIMARY KEY,
                    SehirAdi VARCHAR(100) NOT NULL
                );

                CREATE TABLE IF NOT EXISTS Ulasim (
                    UlasimID SERIAL PRIMARY KEY,
                    SehirID INTEGER NOT NULL REFERENCES Sehirler(SehirID),
                    UlasimTuru VARCHAR(50) NOT NULL,
                    Fiyat NUMERIC(10,2) NOT NULL
                );

                CREATE TABLE IF NOT EXISTS Konaklama (
                    KonaklamaID SERIAL PRIMARY KEY,
                    SehirID INTEGER NOT NULL REFERENCES Sehirler(SehirID),
                    KonaklamaAdi VARCHAR(200) NOT NULL,
                    KonaklamaTuru VARCHAR(50) NOT NULL,
                    GeceFiyat NUMERIC(10,2) NOT NULL
                );

                CREATE TABLE IF NOT EXISTS GezilecekYerler (
                    YerID SERIAL PRIMARY KEY,
                    SehirID INTEGER NOT NULL REFERENCES Sehirler(SehirID),
                    YerAdi VARCHAR(200) NOT NULL,
                    ZiyaretUcreti NUMERIC(10,2) NOT NULL,
                    Aciklama TEXT
                );

                CREATE TABLE IF NOT EXISTS TurPlanlari (
                    PlanID SERIAL PRIMARY KEY,
                    SehirID INTEGER NOT NULL REFERENCES Sehirler(SehirID),
                    UlasimID INTEGER NOT NULL REFERENCES Ulasim(UlasimID),
                    KonaklamaID INTEGER NOT NULL REFERENCES Konaklama(KonaklamaID),
                    GeceSayisi INTEGER NOT NULL DEFAULT 1,
                    Butce NUMERIC(10,2) NOT NULL DEFAULT 0,
                    ToplamMaliyet NUMERIC(10,2) NOT NULL,
                    OlusturmaTarihi TIMESTAMP NOT NULL
                );

                CREATE TABLE IF NOT EXISTS PlanGezilecekYerler (
                    PlanID INTEGER NOT NULL REFERENCES TurPlanlari(PlanID) ON DELETE CASCADE,
                    YerID INTEGER NOT NULL REFERENCES GezilecekYerler(YerID) ON DELETE CASCADE,
                    PRIMARY KEY (PlanID, YerID)
                );
            ";
            cmd.ExecuteNonQuery();

            SeedData(activeConn);

            SeedExtraData(activeConn);

            RunMigrations(activeConn);
        }

        private static void RunMigrations(NpgsqlConnection conn)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                -- GeceSayisi sütunu yoksa ekle
                DO $$ BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns
                                   WHERE table_name='turplanlari' AND column_name='gecesayisi') THEN
                        ALTER TABLE TurPlanlari ADD COLUMN GeceSayisi INTEGER NOT NULL DEFAULT 1;
                    END IF;
                END $$;

                -- Butce sütunu yoksa ekle
                DO $$ BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns
                                   WHERE table_name='turplanlari' AND column_name='butce') THEN
                        ALTER TABLE TurPlanlari ADD COLUMN Butce NUMERIC(10,2) NOT NULL DEFAULT 0;
                    END IF;
                END $$;

                -- Sezon sütunu yoksa ekle
                DO $$ BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns
                                   WHERE table_name='turplanlari' AND column_name='sezon') THEN
                        ALTER TABLE TurPlanlari ADD COLUMN Sezon VARCHAR(50) NOT NULL DEFAULT 'Bahar';
                    END IF;
                END $$;

                -- SehirIciUlasim sütunu yoksa ekle
                DO $$ BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns
                                   WHERE table_name='turplanlari' AND column_name='sehiriciulasim') THEN
                        ALTER TABLE TurPlanlari ADD COLUMN SehirIciUlasim VARCHAR(50) NOT NULL DEFAULT 'Toplu Tasima';
                    END IF;
                END $$;

                -- SehirIciMaliyet sütunu yoksa ekle
                DO $$ BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns
                                   WHERE table_name='turplanlari' AND column_name='sehiricimaliyet') THEN
                        ALTER TABLE TurPlanlari ADD COLUMN SehirIciMaliyet NUMERIC(10,2) NOT NULL DEFAULT 0;
                    END IF;
                END $$;

                -- PlanGezilecekYerler tablosu yoksa oluştur
                CREATE TABLE IF NOT EXISTS PlanGezilecekYerler (
                    PlanID INTEGER NOT NULL REFERENCES TurPlanlari(PlanID) ON DELETE CASCADE,
                    YerID INTEGER NOT NULL REFERENCES GezilecekYerler(YerID) ON DELETE CASCADE,
                    PRIMARY KEY (PlanID, YerID)
                );
            ";
            cmd.ExecuteNonQuery();
        }

        private static void EnsureDatabaseExists()
        {
            var masterConnStr = "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=Mkalbisen234-";
            using var conn = new NpgsqlConnection(masterConnStr);
            conn.Open();

            using var checkCmd = conn.CreateCommand();
            checkCmd.CommandText = "SELECT 1 FROM pg_database WHERE datname = 'SmartTourDB'";
            var result = checkCmd.ExecuteScalar();

            if (result == null)
            {
                try
                {
                    using var createCmd = conn.CreateCommand();
                    createCmd.CommandText = "CREATE DATABASE \"SmartTourDB\"";
                    createCmd.ExecuteNonQuery();
                }
                catch (PostgresException ex) when (ex.SqlState == "42P04")
                {
                }
            }
        }

        private static void SeedData(NpgsqlConnection conn)
        {
            using var checkCmd = conn.CreateCommand();
            checkCmd.CommandText = "SELECT COUNT(*) FROM Sehirler";
            long count = (long)checkCmd.ExecuteScalar()!;
            if (count > 0) return;

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                -- Şehirler
                INSERT INTO Sehirler (SehirAdi) VALUES ('İstanbul');
                INSERT INTO Sehirler (SehirAdi) VALUES ('Antalya');
                INSERT INTO Sehirler (SehirAdi) VALUES ('İzmir');
                INSERT INTO Sehirler (SehirAdi) VALUES ('Kapadokya');
                INSERT INTO Sehirler (SehirAdi) VALUES ('Bodrum');

                -- Ulaşım (İstanbul = 1)
                INSERT INTO Ulasim (SehirID, UlasimTuru, Fiyat) VALUES (1, 'Otobüs', 350);
                INSERT INTO Ulasim (SehirID, UlasimTuru, Fiyat) VALUES (1, 'Uçak', 900);
                INSERT INTO Ulasim (SehirID, UlasimTuru, Fiyat) VALUES (1, 'Tren', 280);
                -- Antalya = 2
                INSERT INTO Ulasim (SehirID, UlasimTuru, Fiyat) VALUES (2, 'Otobüs', 400);
                INSERT INTO Ulasim (SehirID, UlasimTuru, Fiyat) VALUES (2, 'Uçak', 750);
                -- İzmir = 3
                INSERT INTO Ulasim (SehirID, UlasimTuru, Fiyat) VALUES (3, 'Otobüs', 300);
                INSERT INTO Ulasim (SehirID, UlasimTuru, Fiyat) VALUES (3, 'Uçak', 650);
                INSERT INTO Ulasim (SehirID, UlasimTuru, Fiyat) VALUES (3, 'Tren', 250);
                -- Kapadokya = 4
                INSERT INTO Ulasim (SehirID, UlasimTuru, Fiyat) VALUES (4, 'Otobüs', 450);
                INSERT INTO Ulasim (SehirID, UlasimTuru, Fiyat) VALUES (4, 'Uçak', 850);
                -- Bodrum = 5
                INSERT INTO Ulasim (SehirID, UlasimTuru, Fiyat) VALUES (5, 'Otobüs', 500);
                INSERT INTO Ulasim (SehirID, UlasimTuru, Fiyat) VALUES (5, 'Uçak', 800);

                -- Konaklama (İstanbul = 1)
                INSERT INTO Konaklama (SehirID, KonaklamaAdi, KonaklamaTuru, GeceFiyat) VALUES (1, 'Grand Hotel İstanbul', 'Otel', 1200);
                INSERT INTO Konaklama (SehirID, KonaklamaAdi, KonaklamaTuru, GeceFiyat) VALUES (1, 'Sultan Pansiyon', 'Pansiyon', 350);
                INSERT INTO Konaklama (SehirID, KonaklamaAdi, KonaklamaTuru, GeceFiyat) VALUES (1, 'Taksim Apart', 'Apart', 600);
                -- Antalya = 2
                INSERT INTO Konaklama (SehirID, KonaklamaAdi, KonaklamaTuru, GeceFiyat) VALUES (2, 'Lara Resort', 'Otel', 1500);
                INSERT INTO Konaklama (SehirID, KonaklamaAdi, KonaklamaTuru, GeceFiyat) VALUES (2, 'Kaleiçi Pansiyon', 'Pansiyon', 400);
                INSERT INTO Konaklama (SehirID, KonaklamaAdi, KonaklamaTuru, GeceFiyat) VALUES (2, 'Konyaaltı Apart', 'Apart', 700);
                -- İzmir = 3
                INSERT INTO Konaklama (SehirID, KonaklamaAdi, KonaklamaTuru, GeceFiyat) VALUES (3, 'Kordon Otel', 'Otel', 900);
                INSERT INTO Konaklama (SehirID, KonaklamaAdi, KonaklamaTuru, GeceFiyat) VALUES (3, 'Alsancak Pansiyon', 'Pansiyon', 300);
                INSERT INTO Konaklama (SehirID, KonaklamaAdi, KonaklamaTuru, GeceFiyat) VALUES (3, 'Bornova Apart', 'Apart', 500);
                -- Kapadokya = 4
                INSERT INTO Konaklama (SehirID, KonaklamaAdi, KonaklamaTuru, GeceFiyat) VALUES (4, 'Taş Konak Otel', 'Otel', 1800);
                INSERT INTO Konaklama (SehirID, KonaklamaAdi, KonaklamaTuru, GeceFiyat) VALUES (4, 'Göreme Pansiyon', 'Pansiyon', 500);
                INSERT INTO Konaklama (SehirID, KonaklamaAdi, KonaklamaTuru, GeceFiyat) VALUES (4, 'Ürgüp Apart', 'Apart', 800);
                -- Bodrum = 5
                INSERT INTO Konaklama (SehirID, KonaklamaAdi, KonaklamaTuru, GeceFiyat) VALUES (5, 'Bodrum Bay Resort', 'Otel', 2000);
                INSERT INTO Konaklama (SehirID, KonaklamaAdi, KonaklamaTuru, GeceFiyat) VALUES (5, 'Gümbet Pansiyon', 'Pansiyon', 450);
                INSERT INTO Konaklama (SehirID, KonaklamaAdi, KonaklamaTuru, GeceFiyat) VALUES (5, 'Bitez Apart', 'Apart', 750);

                -- Gezilecek Yerler (İstanbul = 1)
                INSERT INTO GezilecekYerler (SehirID, YerAdi, ZiyaretUcreti, Aciklama) VALUES (1, 'Ayasofya', 300, 'Tarihi müze ve cami');
                INSERT INTO GezilecekYerler (SehirID, YerAdi, ZiyaretUcreti, Aciklama) VALUES (1, 'Topkapı Sarayı', 200, 'Osmanlı İmparatorluğu sarayı');
                INSERT INTO GezilecekYerler (SehirID, YerAdi, ZiyaretUcreti, Aciklama) VALUES (1, 'Kapalıçarşı', 0, 'Tarihi alışveriş merkezi');
                INSERT INTO GezilecekYerler (SehirID, YerAdi, ZiyaretUcreti, Aciklama) VALUES (1, 'Boğaz Turu', 150, 'İstanbul Boğazı tekne turu');
                -- Antalya = 2
                INSERT INTO GezilecekYerler (SehirID, YerAdi, ZiyaretUcreti, Aciklama) VALUES (2, 'Düden Şelalesi', 50, 'Doğal şelale');
                INSERT INTO GezilecekYerler (SehirID, YerAdi, ZiyaretUcreti, Aciklama) VALUES (2, 'Aspendos', 100, 'Antik Roma tiyatrosu');
                INSERT INTO GezilecekYerler (SehirID, YerAdi, ZiyaretUcreti, Aciklama) VALUES (2, 'Kaleiçi', 0, 'Tarihi şehir merkezi');
                INSERT INTO GezilecekYerler (SehirID, YerAdi, ZiyaretUcreti, Aciklama) VALUES (2, 'Antalya Müzesi', 100, 'Arkeoloji müzesi');
                -- İzmir = 3
                INSERT INTO GezilecekYerler (SehirID, YerAdi, ZiyaretUcreti, Aciklama) VALUES (3, 'Efes Antik Kenti', 200, 'Antik Yunan şehri');
                INSERT INTO GezilecekYerler (SehirID, YerAdi, ZiyaretUcreti, Aciklama) VALUES (3, 'Saat Kulesi', 0, 'İzmir simgesi');
                INSERT INTO GezilecekYerler (SehirID, YerAdi, ZiyaretUcreti, Aciklama) VALUES (3, 'Kemeraltı Çarşısı', 0, 'Tarihi alışveriş bölgesi');
                INSERT INTO GezilecekYerler (SehirID, YerAdi, ZiyaretUcreti, Aciklama) VALUES (3, 'Şirince Köyü', 50, 'Tarihi köy');
                -- Kapadokya = 4
                INSERT INTO GezilecekYerler (SehirID, YerAdi, ZiyaretUcreti, Aciklama) VALUES (4, 'Balon Turu', 3500, 'Sıcak hava balonu');
                INSERT INTO GezilecekYerler (SehirID, YerAdi, ZiyaretUcreti, Aciklama) VALUES (4, 'Göreme Açık Hava Müzesi', 200, 'UNESCO Dünya Mirası');
                INSERT INTO GezilecekYerler (SehirID, YerAdi, ZiyaretUcreti, Aciklama) VALUES (4, 'Derinkuyu Yeraltı Şehri', 150, 'Antik yeraltı şehri');
                INSERT INTO GezilecekYerler (SehirID, YerAdi, ZiyaretUcreti, Aciklama) VALUES (4, 'Uçhisar Kalesi', 75, 'Doğal kaya kalesi');
                -- Bodrum = 5
                INSERT INTO GezilecekYerler (SehirID, YerAdi, ZiyaretUcreti, Aciklama) VALUES (5, 'Bodrum Kalesi', 100, 'Tarihi kale ve müze');
                INSERT INTO GezilecekYerler (SehirID, YerAdi, ZiyaretUcreti, Aciklama) VALUES (5, 'Sualtı Müzesi', 150, 'Sualtı arkeoloji müzesi');
                INSERT INTO GezilecekYerler (SehirID, YerAdi, ZiyaretUcreti, Aciklama) VALUES (5, 'Gümüşlük', 0, 'Tarihi balıkçı köyü');
                INSERT INTO GezilecekYerler (SehirID, YerAdi, ZiyaretUcreti, Aciklama) VALUES (5, 'Tekne Turu', 250, 'Bodrum koyları turu');
            ";
            cmd.ExecuteNonQuery();
        }

        private static void SeedExtraData(NpgsqlConnection conn)
        {
            using var constraintCmd = conn.CreateCommand();
            constraintCmd.CommandText = @"
                DO $$ BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'uq_konaklama_adi_sehir') THEN
                        ALTER TABLE Konaklama ADD CONSTRAINT uq_konaklama_adi_sehir UNIQUE (SehirID, KonaklamaAdi);
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'uq_yer_adi_sehir') THEN
                        ALTER TABLE GezilecekYerler ADD CONSTRAINT uq_yer_adi_sehir UNIQUE (SehirID, YerAdi);
                    END IF;
                END $$;
            ";
            constraintCmd.ExecuteNonQuery();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                -- Ekstra Konaklama (İstanbul = 1)
                INSERT INTO Konaklama (SehirID, KonaklamaAdi, KonaklamaTuru, GeceFiyat) VALUES (1, 'Pera Palace Hotel', 'Otel', 3500) ON CONFLICT DO NOTHING;
                INSERT INTO Konaklama (SehirID, KonaklamaAdi, KonaklamaTuru, GeceFiyat) VALUES (1, 'Beyoğlu Hostel', 'Hostel', 200) ON CONFLICT DO NOTHING;
                INSERT INTO Konaklama (SehirID, KonaklamaAdi, KonaklamaTuru, GeceFiyat) VALUES (1, 'Sultanahmet Butik Otel', 'Butik Otel', 1800) ON CONFLICT DO NOTHING;
                -- Antalya = 2
                INSERT INTO Konaklama (SehirID, KonaklamaAdi, KonaklamaTuru, GeceFiyat) VALUES (2, 'Belek Premium Resort', 'Otel', 2500) ON CONFLICT DO NOTHING;
                INSERT INTO Konaklama (SehirID, KonaklamaAdi, KonaklamaTuru, GeceFiyat) VALUES (2, 'Side Hostel', 'Hostel', 180) ON CONFLICT DO NOTHING;
                INSERT INTO Konaklama (SehirID, KonaklamaAdi, KonaklamaTuru, GeceFiyat) VALUES (2, 'Olimpos Ağaç Ev', 'Bungalov', 450) ON CONFLICT DO NOTHING;
                -- İzmir = 3
                INSERT INTO Konaklama (SehirID, KonaklamaAdi, KonaklamaTuru, GeceFiyat) VALUES (3, 'Çeşme Boutique Hotel', 'Butik Otel', 1600) ON CONFLICT DO NOTHING;
                INSERT INTO Konaklama (SehirID, KonaklamaAdi, KonaklamaTuru, GeceFiyat) VALUES (3, 'Alaçatı Taş Ev', 'Butik Otel', 2200) ON CONFLICT DO NOTHING;
                INSERT INTO Konaklama (SehirID, KonaklamaAdi, KonaklamaTuru, GeceFiyat) VALUES (3, 'Konak Hostel', 'Hostel', 150) ON CONFLICT DO NOTHING;
                -- Kapadokya = 4
                INSERT INTO Konaklama (SehirID, KonaklamaAdi, KonaklamaTuru, GeceFiyat) VALUES (4, 'Museum Hotel', 'Butik Otel', 4500) ON CONFLICT DO NOTHING;
                INSERT INTO Konaklama (SehirID, KonaklamaAdi, KonaklamaTuru, GeceFiyat) VALUES (4, 'Avanos Cave Hotel', 'Mağara Otel', 2000) ON CONFLICT DO NOTHING;
                INSERT INTO Konaklama (SehirID, KonaklamaAdi, KonaklamaTuru, GeceFiyat) VALUES (4, 'Göreme Hostel', 'Hostel', 250) ON CONFLICT DO NOTHING;
                -- Bodrum = 5
                INSERT INTO Konaklama (SehirID, KonaklamaAdi, KonaklamaTuru, GeceFiyat) VALUES (5, 'Yalıkavak Marina Hotel', 'Otel', 3200) ON CONFLICT DO NOTHING;
                INSERT INTO Konaklama (SehirID, KonaklamaAdi, KonaklamaTuru, GeceFiyat) VALUES (5, 'Türkbükü Butik Otel', 'Butik Otel', 2800) ON CONFLICT DO NOTHING;
                INSERT INTO Konaklama (SehirID, KonaklamaAdi, KonaklamaTuru, GeceFiyat) VALUES (5, 'Bodrum Hostel', 'Hostel', 200) ON CONFLICT DO NOTHING;

                -- Ekstra Gezilecek Yerler (İstanbul = 1)
                INSERT INTO GezilecekYerler (SehirID, YerAdi, ZiyaretUcreti, Aciklama) VALUES (1, 'Yerebatan Sarnıcı', 250, 'Bizans dönemi yeraltı sarnıcı') ON CONFLICT DO NOTHING;
                INSERT INTO GezilecekYerler (SehirID, YerAdi, ZiyaretUcreti, Aciklama) VALUES (1, 'Dolmabahçe Sarayı', 300, 'Son Osmanlı sarayı') ON CONFLICT DO NOTHING;
                INSERT INTO GezilecekYerler (SehirID, YerAdi, ZiyaretUcreti, Aciklama) VALUES (1, 'Galata Kulesi', 200, 'Panoramik İstanbul manzarası') ON CONFLICT DO NOTHING;
                INSERT INTO GezilecekYerler (SehirID, YerAdi, ZiyaretUcreti, Aciklama) VALUES (1, 'Sultanahmet Camii', 0, 'Mavi Cami olarak bilinen tarihi cami') ON CONFLICT DO NOTHING;
                INSERT INTO GezilecekYerler (SehirID, YerAdi, ZiyaretUcreti, Aciklama) VALUES (1, 'Miniatürk', 120, 'Türkiye minyatür parkı') ON CONFLICT DO NOTHING;
                INSERT INTO GezilecekYerler (SehirID, YerAdi, ZiyaretUcreti, Aciklama) VALUES (1, 'Adalar Vapuru', 50, 'Büyükada ve Heybeliada gezisi') ON CONFLICT DO NOTHING;
                -- Antalya = 2
                INSERT INTO GezilecekYerler (SehirID, YerAdi, ZiyaretUcreti, Aciklama) VALUES (2, 'Perge Antik Kenti', 80, 'Roma dönemi antik kent') ON CONFLICT DO NOTHING;
                INSERT INTO GezilecekYerler (SehirID, YerAdi, ZiyaretUcreti, Aciklama) VALUES (2, 'Manavgat Şelalesi', 30, 'Doğal şelale ve piknik alanı') ON CONFLICT DO NOTHING;
                INSERT INTO GezilecekYerler (SehirID, YerAdi, ZiyaretUcreti, Aciklama) VALUES (2, 'Olympos Teleferik', 120, 'Tahtalı Dağı teleferik turu') ON CONFLICT DO NOTHING;
                INSERT INTO GezilecekYerler (SehirID, YerAdi, ZiyaretUcreti, Aciklama) VALUES (2, 'Konyaaltı Plajı', 0, 'Antalyanın ünlü sahili') ON CONFLICT DO NOTHING;
                INSERT INTO GezilecekYerler (SehirID, YerAdi, ZiyaretUcreti, Aciklama) VALUES (2, 'Termessos', 60, 'Dağ tepesindeki antik kent') ON CONFLICT DO NOTHING;
                -- İzmir = 3
                INSERT INTO GezilecekYerler (SehirID, YerAdi, ZiyaretUcreti, Aciklama) VALUES (3, 'Bergama Akropol', 150, 'Antik Pergamon kalıntıları') ON CONFLICT DO NOTHING;
                INSERT INTO GezilecekYerler (SehirID, YerAdi, ZiyaretUcreti, Aciklama) VALUES (3, 'Alaçatı', 0, 'Rüzgar sörfü ve taş sokaklar') ON CONFLICT DO NOTHING;
                INSERT INTO GezilecekYerler (SehirID, YerAdi, ZiyaretUcreti, Aciklama) VALUES (3, 'Çeşme Plajları', 0, 'Ilıca ve Altınkum plajları') ON CONFLICT DO NOTHING;
                INSERT INTO GezilecekYerler (SehirID, YerAdi, ZiyaretUcreti, Aciklama) VALUES (3, 'Agora Açık Hava Müzesi', 50, 'İzmir Agora antik kalıntıları') ON CONFLICT DO NOTHING;
                INSERT INTO GezilecekYerler (SehirID, YerAdi, ZiyaretUcreti, Aciklama) VALUES (3, 'Kordon Yürüyüş Yolu', 0, 'Deniz kenarı yürüyüş parkuru') ON CONFLICT DO NOTHING;
                -- Kapadokya = 4
                INSERT INTO GezilecekYerler (SehirID, YerAdi, ZiyaretUcreti, Aciklama) VALUES (4, 'Ihlara Vadisi', 75, 'Kanyonda yürüyüş parkuru') ON CONFLICT DO NOTHING;
                INSERT INTO GezilecekYerler (SehirID, YerAdi, ZiyaretUcreti, Aciklama) VALUES (4, 'Kaymaklı Yeraltı Şehri', 120, 'Antik yeraltı yaşam alanı') ON CONFLICT DO NOTHING;
                INSERT INTO GezilecekYerler (SehirID, YerAdi, ZiyaretUcreti, Aciklama) VALUES (4, 'Devrent Vadisi', 0, 'Peri bacaları doğal oluşumlar') ON CONFLICT DO NOTHING;
                INSERT INTO GezilecekYerler (SehirID, YerAdi, ZiyaretUcreti, Aciklama) VALUES (4, 'ATV Safari Turu', 400, 'Vadilerde ATV macera turu') ON CONFLICT DO NOTHING;
                INSERT INTO GezilecekYerler (SehirID, YerAdi, ZiyaretUcreti, Aciklama) VALUES (4, 'Çömlek Atölyesi', 100, 'Avanos çömlek yapım deneyimi') ON CONFLICT DO NOTHING;
                -- Bodrum = 5
                INSERT INTO GezilecekYerler (SehirID, YerAdi, ZiyaretUcreti, Aciklama) VALUES (5, 'Kara Ada', 180, 'Termal kaynaklar ve tekne turu') ON CONFLICT DO NOTHING;
                INSERT INTO GezilecekYerler (SehirID, YerAdi, ZiyaretUcreti, Aciklama) VALUES (5, 'Bodrum Amfitiyatro', 0, 'Antik tiyatro kalıntıları') ON CONFLICT DO NOTHING;
                INSERT INTO GezilecekYerler (SehirID, YerAdi, ZiyaretUcreti, Aciklama) VALUES (5, 'Dalyan Tekne Turu', 350, 'Kaunos ve İztuzu sahili') ON CONFLICT DO NOTHING;
                INSERT INTO GezilecekYerler (SehirID, YerAdi, ZiyaretUcreti, Aciklama) VALUES (5, 'Yalıkavak Pazarı', 0, 'Perşembe halk pazarı') ON CONFLICT DO NOTHING;
                INSERT INTO GezilecekYerler (SehirID, YerAdi, ZiyaretUcreti, Aciklama) VALUES (5, 'Akvaryum Koyu', 0, 'Kristal berraklığında deniz') ON CONFLICT DO NOTHING;
            ";
            cmd.ExecuteNonQuery();
        }
    }
}
