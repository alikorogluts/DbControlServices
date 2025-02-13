using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace kontrol_revis
{
    internal class DataOku
    {
        public class VeritabaniOku
        {
            private static string connectionString = "Data Source=DESKTOP-83AGMDV\\MSSQLSERVER61;Initial Catalog=cafedata;Integrated Security=True";

            public static List<string> SutunuOku(string tabloAdi, string sutunAdi)
            {
                var sonuclar = new List<string>();

                try
                {
                    // MSSQL veritabanı bağlantısı kur
                    using (SqlConnection baglanti = new SqlConnection(connectionString))
                    {
                        baglanti.Open();

                        // SQL sorgusu
                        string sorgu = $"SELECT {sutunAdi} FROM {tabloAdi}";

                        using (var cmd = new SqlCommand(sorgu, baglanti))
                        {
                            using (SqlDataReader okuyucu = cmd.ExecuteReader())
                            {
                                // Verileri oku ve listeye ekle
                                while (okuyucu.Read())
                                {
                                    sonuclar.Add(okuyucu[sutunAdi].ToString());
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Hata yönetimi
                    Console.WriteLine($"Hata: {ex.Message}. Tablo: {tabloAdi}, Sütun: {sutunAdi}");
                }

                return sonuclar;
            }
        }
    }
}
