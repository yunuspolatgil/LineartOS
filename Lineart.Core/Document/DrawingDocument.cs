using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Lineart.Core.Entities;
using Lineart.Core.Settings;

namespace Lineart.Core.Document
{
    public class DrawingDocument
    {
        [Browsable(false)] public Guid Id { get; set; } = Guid.NewGuid();

        [Category("Proje Bilgileri")]
        [DisplayName("1. Proje Adı")]
        public string ProjectName { get; set; } = "Yeni Mutfak Projesi";

        [Category("Proje Bilgileri")]
        [DisplayName("2. Müşteri Adı")]
        public string CustomerName { get; set; } = "Belirtilmedi";

        [Browsable(false)] public DateTime CreatedDate { get; set; } = DateTime.Now;
        [Browsable(false)] public DateTime LastModified { get; set; } = DateTime.Now;

        [Category("Oda Ayarları")][DisplayName("Duvar Genişliği (mm)")]
        public double RoomWidth
        {
            get => Room.TotalFlattenedWidth > 0 ? Room.TotalFlattenedWidth : 4000;
            set { /* Geriye dönük uyumluluk için boş bırakılabilir veya WallMain'e eşitlenebilir */ }
        }

        [Category("Oda Ayarları")][DisplayName("Tavan Yüksekliği (mm)")]
        public double RoomHeight
        {
            get => Room.Height > 0 ? Room.Height : 2500;
            set { Room.Height = value; }
        }

        [Browsable(false)]
        public List<CabinetEntity> Cabinets { get; set; } = new List<CabinetEntity>();

        public void AddCabinet(CabinetEntity cabinet) => Cabinets.Add(cabinet);

        public RoomSettings Room { get; set; } = new RoomSettings();

        // OTOMATİK DİZİN KAYDI (Belgelerim/MobilyaOS/Projects/...)
        public void SaveToSystem()
        {
            this.LastModified = DateTime.Now;
            string dir = AppGlobalSettings.ProjectsFolder;
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            // Proje adına göre dosya ismi oluştur (Geçersiz karakterleri temizle)
            string safeName = string.Join("_", ProjectName.Split(Path.GetInvalidFileNameChars()));
            string filePath = Path.Combine(dir, $"{safeName}_{Id.ToString().Substring(0, 4)}.mob");

            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(filePath, JsonSerializer.Serialize(this, options));
        }

        public static DrawingDocument LoadFromFile(string filePath)
        {
            var doc = JsonSerializer.Deserialize<DrawingDocument>(File.ReadAllText(filePath));
            foreach (var cab in doc.Cabinets) cab.RebuildParts();
            return doc;
        }

        

      
    }
}