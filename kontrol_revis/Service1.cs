using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace kontrol_revis
{
    public partial class Service1 : ServiceBase
    {
        Timer timer = new Timer();
        List<string> klasorDosyaAdlari = new List<string>();

        List<string> veriTabaniDosyalar = new List<string>();
        List<string> veriTabaniYollar = new List<string>();

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            timer.Interval = 10000; // 300000
            timer.Enabled = true;

            WriteLog("Servis başlatıldı.");
        }

        protected override void OnStop()
        {
            timer.Enabled = false;
            WriteLog("Servis durduruldu.");
        }

        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            try
            {
                // Dosya adlarını ve veritabanı yollarını al
                DosyaBul(); // Klasördeki dosya adlarını alır
                GetirVeriTabaniDosyalar(); // Veritabanındaki dosya yollarını alır

                // Veritabanı bağlantısını oluştur
                string connectionString = "";
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    try
                    {
                        // Bağlantıyı aç
                        connection.Open();
                        Console.WriteLine("Veritabanına başarıyla bağlandı.");

                        // Klasördeki dosyaların isimlerini döngüye al
                        foreach (string dosyaAd in klasorDosyaAdlari)
                        {
                            bool dosyaVarMi = false;

                            // Veritabanındaki dosya yollarını döngüye al
                            foreach (string veriTabaniYolu in veriTabaniYollar)
                            {
                                // Dosya adını ve veritabanı yolunu karşılaştır
                                if (dosyaAd == veriTabaniYolu)
                                {
                                    dosyaVarMi = true;
                                    break;
                                }
                            }

                            // Eğer dosya veritabanında yoksa ekle
                            if (!dosyaVarMi)
                            {
                                // Veritabanına ekleme işlemi
                                string query = "INSERT INTO ";
                                var deger =0;
                                using (MySqlCommand cmd = new MySqlCommand(query, connection))
                                {
                                    cmd.Parameters.AddWithValue("parametre", deger );
                                    
                                    cmd.ExecuteNonQuery();
                                    Console.WriteLine($"Dosya eklendi: {dosyaAd}");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Veritabanı işlemi sırasında bir hata oluştu: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Hata yönetimi
                WriteLog($"101 hata: {ex.Message}");
            }
        }

        private void LogEksikDosyalar(List<string> eksikDosyalar)
        {
            string logDosyasi = @"C:\Logs\EksikDosyalarLog.txt";

            // Log dizini yoksa oluştur
            string logDizin = Path.GetDirectoryName(logDosyasi);
            if (!Directory.Exists(logDizin))
            {
                Directory.CreateDirectory(logDizin);
            }

            try
            {
                using (StreamWriter writer = new StreamWriter(logDosyasi, true))
                {
                    writer.WriteLine($"Eksik dosyalar ({DateTime.Now}):");

                    foreach (string dosya in eksikDosyalar)
                    {
                        writer.WriteLine(dosya);
                    }

                    writer.WriteLine();
                }
            }
            catch (Exception e)
            {
                // Hata yönetimi
                WriteLog($"Log dosyasına yazarken hata: {e.Message}");
            }
        }

        private void DosyaBul()
        {
            string klasorYolu = @"C:\xampp\htdocs\htdocs\videolar";
            

            try
            {
                // Belirtilen klasördeki tüm dosyaların tam yollarını al
                string[] files = Directory.GetFiles(klasorYolu);

                foreach (string filePath in files)
                {
                    // Tam yoldan sadece dosya adını al
                    string fileName = Path.GetFileName(filePath);
                    klasorDosyaAdlari.Add("videolar\\"+fileName);
                }
            }
            catch (Exception e)
            {
                // Hata yönetimi
                WriteLog($"Dosyaları bulurken hata: {e.Message}");
            }

            
        }

        private void GetirVeriTabaniDosyalar()
        {
            string connectionString = "";

            // Veritabanı bağlantısını oluştur
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    // Bağlantıyı aç
                    connection.Open();
                    Console.WriteLine("Veritabanına başarıyla bağlandı.");

                    // SQL sorgusu
                    string query = "SELECT * FROM data"; // Tablo adınızı buraya yazın

                    // Sorguyu çalıştır
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    // Verileri oku
                    while (reader.Read())
                    {
                        string veri = $"ID: {reader["id"]}, Video Yolu: {reader["video_yolu"]}";
                        // Veriyi listeye ekle
                        if (!veriTabaniDosyalar.Contains(veri))
                        {
                            veriTabaniDosyalar.Add(veri);
                            veriTabaniYollar.Add(reader["video_yolu"].ToString());
                        }

                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Bir hata oluştu: {ex.Message}");
                }
            }

          
        }

        private void WriteLog(string message)
        {
            string log = "WindowsServiceLog";

            if (!EventLog.SourceExists(log))
            {
                EventLog.CreateEventSource(log, "Application");
            }
            EventLog.WriteEntry(log, message);
        }
    }
}
