using System;
using System.ComponentModel;

namespace Lineart.Core.Entities
{
    // YENİ: Bulaşık Makinesi Tipleri Eklendi
    public enum ModuleType { Kapak, Cekmece, AnkastreFirin, AnkastreMikrodalga, AnkastreBulasikTam, AnkastreBulasikYarim, AcikRaf }

    public class CabinetModule
    {
        [DisplayName("1. Modül Tipi")]
        public ModuleType Type { get; set; } = ModuleType.Kapak;

        [DisplayName("2. Yükseklik (mm)")]
        [Description("0 yazarsanız kalan boşluğu otomatik paylaştırır (Auto). Ankastreler için otomatik standart ölçüyü alır.")]
        public double HeightMm { get; set; } = 0; // 0 = Otomatik Paylaştır

        [DisplayName("3. Bölme Sayısı (Adet)")]
        [Description("Örn: Çekmece seçip 3 yazarsanız, bu alanı 3 eşit çekmeceye böler. Kapak seçip 2 yazarsanız çift kapak yapar.")]
        public int SubItemCount { get; set; } = 1;

        [DisplayName("4. İç Raf Sayısı (Seyyar)")]
        [Description("Bu modülün (kapağın) içine eklenecek hareketli seyyar raf sayısı.")]
        public int InnerShelfCount { get; set; } = 0;

        public override string ToString()
        {
            string boy = HeightMm == 0 ? "Oto" : $"{HeightMm}mm";

            if (Type == ModuleType.AnkastreFirin) boy = "Net:600mm (Standart)";
            if (Type == ModuleType.AnkastreMikrodalga) boy = "Net:380mm (Standart)";
            if (Type == ModuleType.AnkastreBulasikTam) boy = "Tam Ankastre (Full Kapak)";
            if (Type == ModuleType.AnkastreBulasikYarim) boy = "Yarım Ankastre (Panel+Kapak)";

            string rafBilgisi = InnerShelfCount > 0 ? $" (+{InnerShelfCount} Raf)" : "";
            return $"{Type} ({SubItemCount} Adet) - {boy}{rafBilgisi}";
        }
    }
}