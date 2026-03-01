using System;

namespace Lineart.Core.Settings
{
    // Tek bir parçanın 4 kenarının bant bilgisini tutar
    public class PartBanding
    {
        public double Top { get; set; } = 0;    // Üst Kenar (mm)
        public double Bottom { get; set; } = 0; // Alt Kenar
        public double Left { get; set; } = 0;   // Sol (Uzun)
        public double Right { get; set; } = 0;  // Sağ (Uzun)

        // Hızlı tanımlama için yardımcı metot
        public static PartBanding Create(double t, double b, double l, double r)
        {
            return new PartBanding { Top = t, Bottom = b, Left = l, Right = r };
        }
    }

    // Bir dolabın tüm parçalarının şablonunu tutar
    public class CabinetBandingScheme
    {
        public PartBanding SidePanel { get; set; } = PartBanding.Create(0.4, 0.4, 0.8, 0); // Yan Dikme
        public PartBanding TopPanel { get; set; } = PartBanding.Create(0, 0, 0.8, 0);      // Üst Tabla
        public PartBanding BottomPanel { get; set; } = PartBanding.Create(0, 0, 0.8, 0);   // Alt Tabla
        public PartBanding Shelf { get; set; } = PartBanding.Create(0, 0, 0.8, 0);         // Raf
        public PartBanding Door { get; set; } = PartBanding.Create(2, 2, 2, 2);            // Kapak
    }
}