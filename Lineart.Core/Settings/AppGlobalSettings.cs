using System;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using Lineart.Core.Entities;

namespace Lineart.Core.Settings
{
    // EKSİK OLAN ENUM TANIMLAMALARI EKLENDİ
    public enum DoorStyle { Suntalam, Akrilik, Membran, Lake, ProfilCam }
    public enum DrawerSystem { StandartTeleskopik, BlumTandembox, SametSmartSlide }

    public class AppGlobalSettings
    {
        // Klasör Yolları (Belgelerim / MobilyaOS)
        [Browsable(false)]
        public static string AppFolder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MobilyaOS");

        [Browsable(false)]
        public static string ProjectsFolder => Path.Combine(AppFolder, "Projects");

        [Browsable(false)]
        private static string SettingsFilePath => Path.Combine(AppFolder, "settings.json");

        // --- PVC BANT ŞABLONLARI ---
        [Browsable(false)]
        public CabinetBandingScheme BaseCabinetBanding { get; set; } = new CabinetBandingScheme(); // Alt Dolap Şablonu

        [Browsable(false)]
        public CabinetBandingScheme WallCabinetBanding { get; set; } = new CabinetBandingScheme(); // Üst Dolap Şablonu



        // --- SINGLETON ---
        private static AppGlobalSettings _current;
        [Browsable(false)]
        public static AppGlobalSettings Current
        {
            get
            {
                if (_current == null) _current = Load();
                return _current;
            }
        }



        // --- FABRİKA STANDARTLARI ---
        [Category("1. Alt Dolap Standartları")][DisplayName("Standart Baza (mm)")] public double DefaultBasePlinth { get; set; } = 100;
        [Category("1. Alt Dolap Standartları")][DisplayName("Standart Gövde Boyu (mm)")] public double DefaultBaseHeight { get; set; } = 720;
        [Category("1. Alt Dolap Standartları")][DisplayName("Standart Derinlik (mm)")] public double DefaultBaseDepth { get; set; } = 560;

        [Category("2. Üst Dolap Standartları")][DisplayName("Standart Gövde Boyu (mm)")] public double DefaultUpperHeight { get; set; } = 720;
        [Category("2. Üst Dolap Standartları")][DisplayName("Standart Derinlik (mm)")] public double DefaultUpperDepth { get; set; } = 330;
        [Category("2. Üst Dolap Standartları")][DisplayName("Yerden Başlama (mm)")] public double DefaultUpperElevation { get; set; } = 1420;

        [Category("3. Malzeme ve Donanım")][DisplayName("Gövde Kalınlığı (mm)")] public double DefaultThickness { get; set; } = 18;

        // --- EKSİK OLAN KAPAK VE ÇEKMECE AYARLARI EKLENDİ ---
        [Category("3. Malzeme ve Donanım")][DisplayName("Kapak Tipi/Modeli")] public DoorStyle DefaultDoorStyle { get; set; } = DoorStyle.Akrilik;
        [Category("3. Malzeme ve Donanım")][DisplayName("Standart Çekmece Rayı")] public DrawerSystem DefaultDrawerSystem { get; set; } = DrawerSystem.BlumTandembox;

        [Category("3. Malzeme ve Donanım")][DisplayName("Standart Kulp Tipi")] public HandleType DefaultHandleType { get; set; } = HandleType.StandartKulp;

        [Category("4. İmalat Kuralları")][DisplayName("Alt Birleşim Tipi")] public JoinType DefaultBottomJoin { get; set; } = JoinType.Icten;
        [Category("4. İmalat Kuralları")][DisplayName("Üst Birleşim Tipi")] public JoinType DefaultTopJoin { get; set; } = JoinType.Icten;
        [Category("4. İmalat Kuralları")][DisplayName("Arkalık Montaj Tipi")] public BackPanelType DefaultBackType { get; set; } = BackPanelType.Kanalli;
        
        public AppGlobalSettings()
        {
            // Alt Dolap Varsayılanı (Yanlar yere basar, altı bantsız olabilir)
            BaseCabinetBanding.SidePanel = PartBanding.Create(0.4, 0.4, 0.8, 0); // Üst/Alt:0.4, Ön:0.8, Arka:Yok
            BaseCabinetBanding.BottomPanel = PartBanding.Create(0, 0, 0.8, 0);   // Sadece ön bantlı

            // Üst Dolap Varsayılanı (Alt tabla görünür, altı bantlı olmalı)
            WallCabinetBanding.SidePanel = PartBanding.Create(0.4, 0.4, 0.8, 0);
            WallCabinetBanding.BottomPanel = PartBanding.Create(0.4, 0.4, 0.8, 0.4); // Altı komple dönebiliriz veya sadece ön
        }
        // Ayarları bilgisayara kaydet
        public void Save()
        {
            if (!Directory.Exists(AppFolder)) Directory.CreateDirectory(AppFolder);
            string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsFilePath, json);
        }

        // Ayarları bilgisayardan yükle
        public static AppGlobalSettings Load()
        {
            if (File.Exists(SettingsFilePath))
            {
                string json = File.ReadAllText(SettingsFilePath);
                return JsonSerializer.Deserialize<AppGlobalSettings>(json);
            }
            return new AppGlobalSettings();
        }
    }
}