using System;
using System.Collections.Generic;
using Npgsql;
using SmartTour.Models;

namespace SmartTour.DataAccess
{
    public class SehirRepository
    {
        public List<Sehir> GetAll()
        {
            var list = new List<Sehir>();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT SehirID, SehirAdi FROM Sehirler ORDER BY SehirAdi";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Sehir
                {
                    SehirID = reader.GetInt32(0),
                    SehirAdi = reader.GetString(1)
                });
            }
            return list;
        }
    }

    public class UlasimRepository
    {
        public List<Ulasim> GetBySehirId(int sehirId)
        {
            var list = new List<Ulasim>();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT UlasimID, SehirID, UlasimTuru, Fiyat FROM Ulasim WHERE SehirID = @SehirID ORDER BY Fiyat";
            cmd.Parameters.AddWithValue("@SehirID", sehirId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Ulasim
                {
                    UlasimID = reader.GetInt32(0),
                    SehirID = reader.GetInt32(1),
                    UlasimTuru = reader.GetString(2),
                    Fiyat = reader.GetDecimal(3)
                });
            }
            return list;
        }
    }

    public class KonaklamaRepository
    {
        public List<Konaklama> GetBySehirId(int sehirId)
        {
            var list = new List<Konaklama>();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT KonaklamaID, SehirID, KonaklamaAdi, KonaklamaTuru, GeceFiyat FROM Konaklama WHERE SehirID = @SehirID ORDER BY GeceFiyat";
            cmd.Parameters.AddWithValue("@SehirID", sehirId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Konaklama
                {
                    KonaklamaID = reader.GetInt32(0),
                    SehirID = reader.GetInt32(1),
                    KonaklamaAdi = reader.GetString(2),
                    KonaklamaTuru = reader.GetString(3),
                    GeceFiyat = reader.GetDecimal(4)
                });
            }
            return list;
        }
    }

    public class GezilecekYerRepository
    {
        public List<GezilecekYer> GetBySehirId(int sehirId)
        {
            var list = new List<GezilecekYer>();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT YerID, SehirID, YerAdi, ZiyaretUcreti, Aciklama FROM GezilecekYerler WHERE SehirID = @SehirID ORDER BY YerAdi";
            cmd.Parameters.AddWithValue("@SehirID", sehirId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new GezilecekYer
                {
                    YerID = reader.GetInt32(0),
                    SehirID = reader.GetInt32(1),
                    YerAdi = reader.GetString(2),
                    ZiyaretUcreti = reader.GetDecimal(3),
                    Aciklama = reader.IsDBNull(4) ? string.Empty : reader.GetString(4)
                });
            }
            return list;
        }
    }

    public class TurPlaniRepository
    {
        public void Save(TurPlani plan)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO TurPlanlari (SehirID, UlasimID, KonaklamaID, GeceSayisi, Butce, ToplamMaliyet, OlusturmaTarihi)
                                VALUES (@SehirID, @UlasimID, @KonaklamaID, @GeceSayisi, @Butce, @ToplamMaliyet, @OlusturmaTarihi)
                                RETURNING PlanID";
            cmd.Parameters.AddWithValue("@SehirID", plan.SehirID);
            cmd.Parameters.AddWithValue("@UlasimID", plan.UlasimID);
            cmd.Parameters.AddWithValue("@KonaklamaID", plan.KonaklamaID);
            cmd.Parameters.AddWithValue("@GeceSayisi", plan.GeceSayisi);
            cmd.Parameters.AddWithValue("@Butce", plan.Butce);
            cmd.Parameters.AddWithValue("@ToplamMaliyet", plan.ToplamMaliyet);
            cmd.Parameters.AddWithValue("@OlusturmaTarihi", plan.OlusturmaTarihi);
            int planId = (int)cmd.ExecuteScalar()!;

            foreach (var yer in plan.SecilenYerler)
            {
                using var yerCmd = conn.CreateCommand();
                yerCmd.CommandText = "INSERT INTO PlanGezilecekYerler (PlanID, YerID) VALUES (@PlanID, @YerID) ON CONFLICT DO NOTHING";
                yerCmd.Parameters.AddWithValue("@PlanID", planId);
                yerCmd.Parameters.AddWithValue("@YerID", yer.YerID);
                yerCmd.ExecuteNonQuery();
            }
        }

        public List<TurPlani> GetAll()
        {
            var list = new List<TurPlani>();
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT tp.PlanID, tp.SehirID, tp.UlasimID, tp.KonaklamaID,
                       tp.ToplamMaliyet, tp.OlusturmaTarihi,
                       s.SehirAdi, u.UlasimTuru, u.Fiyat,
                       k.KonaklamaAdi, k.GeceFiyat,
                       tp.GeceSayisi, tp.Butce
                FROM TurPlanlari tp
                JOIN Sehirler s ON tp.SehirID = s.SehirID
                JOIN Ulasim u ON tp.UlasimID = u.UlasimID
                JOIN Konaklama k ON tp.KonaklamaID = k.KonaklamaID
                ORDER BY tp.OlusturmaTarihi DESC";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new TurPlani
                {
                    PlanID = reader.GetInt32(0),
                    SehirID = reader.GetInt32(1),
                    UlasimID = reader.GetInt32(2),
                    KonaklamaID = reader.GetInt32(3),
                    ToplamMaliyet = reader.GetDecimal(4),
                    OlusturmaTarihi = reader.GetDateTime(5),
                    SehirAdi = reader.GetString(6),
                    UlasimTuru = reader.GetString(7),
                    UlasimFiyat = reader.GetDecimal(8),
                    KonaklamaAdi = reader.GetString(9),
                    KonaklamaFiyat = reader.GetDecimal(10),
                    GeceSayisi = reader.GetInt32(11),
                    Butce = reader.GetDecimal(12)
                });
            }
            reader.Close();

            foreach (var plan in list)
            {
                using var yerCmd = conn.CreateCommand();
                yerCmd.CommandText = @"
                    SELECT g.YerID, g.SehirID, g.YerAdi, g.ZiyaretUcreti, g.Aciklama
                    FROM PlanGezilecekYerler pg
                    JOIN GezilecekYerler g ON pg.YerID = g.YerID
                    WHERE pg.PlanID = @PlanID";
                yerCmd.Parameters.AddWithValue("@PlanID", plan.PlanID);
                using var yerReader = yerCmd.ExecuteReader();
                while (yerReader.Read())
                {
                    plan.SecilenYerler.Add(new GezilecekYer
                    {
                        YerID = yerReader.GetInt32(0),
                        SehirID = yerReader.GetInt32(1),
                        YerAdi = yerReader.GetString(2),
                        ZiyaretUcreti = yerReader.GetDecimal(3),
                        Aciklama = yerReader.IsDBNull(4) ? "" : yerReader.GetString(4)
                    });
                }
            }

            return list;
        }

        public void Delete(int planId)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM TurPlanlari WHERE PlanID = @PlanID";
            cmd.Parameters.AddWithValue("@PlanID", planId);
            cmd.ExecuteNonQuery();
        }

        public void Update(TurPlani plan)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"UPDATE TurPlanlari
                                SET SehirID = @SehirID, UlasimID = @UlasimID,
                                    KonaklamaID = @KonaklamaID, GeceSayisi = @GeceSayisi,
                                    Butce = @Butce, ToplamMaliyet = @ToplamMaliyet
                                WHERE PlanID = @PlanID";
            cmd.Parameters.AddWithValue("@PlanID", plan.PlanID);
            cmd.Parameters.AddWithValue("@SehirID", plan.SehirID);
            cmd.Parameters.AddWithValue("@UlasimID", plan.UlasimID);
            cmd.Parameters.AddWithValue("@KonaklamaID", plan.KonaklamaID);
            cmd.Parameters.AddWithValue("@GeceSayisi", plan.GeceSayisi);
            cmd.Parameters.AddWithValue("@Butce", plan.Butce);
            cmd.Parameters.AddWithValue("@ToplamMaliyet", plan.ToplamMaliyet);
            cmd.ExecuteNonQuery();

            using var delCmd = conn.CreateCommand();
            delCmd.CommandText = "DELETE FROM PlanGezilecekYerler WHERE PlanID = @PlanID";
            delCmd.Parameters.AddWithValue("@PlanID", plan.PlanID);
            delCmd.ExecuteNonQuery();

            foreach (var yer in plan.SecilenYerler)
            {
                using var yerCmd = conn.CreateCommand();
                yerCmd.CommandText = "INSERT INTO PlanGezilecekYerler (PlanID, YerID) VALUES (@PlanID, @YerID)";
                yerCmd.Parameters.AddWithValue("@PlanID", plan.PlanID);
                yerCmd.Parameters.AddWithValue("@YerID", yer.YerID);
                yerCmd.ExecuteNonQuery();
            }
        }
    }
}
