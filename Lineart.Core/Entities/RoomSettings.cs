using System;
using System.ComponentModel;

namespace Lineart.Core.Entities
{
    public enum RoomShape { TekDuvar, L_Duvar, U_Duvar }

    public class RoomSettings
    {
        public RoomShape Shape { get; set; } = RoomShape.TekDuvar;

        // Duvar Uzunlukları (Açılım mantığıyla yan yana eklenecekler)
        public double WallLeft { get; set; } = 0;      // Sol Duvar
        public double WallMain { get; set; } = 4000;   // Ana (Karşı) Duvar
        public double WallRight { get; set; } = 0;     // Sağ Duvar

        public double Height { get; set; } = 2600;     // Tavan Yüksekliği
        public double Thickness { get; set; } = 200;   // Duvar Kalınlığı

        // 2D Canvas için tüm duvarların açılmış (yan yana) toplam uzunluğu
        [Browsable(false)]
        public double TotalFlattenedWidth => WallLeft + WallMain + WallRight;
    }
}