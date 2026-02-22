using System;
using System.Drawing;
using Lineart.Core.Geometry;

namespace Lineart.Core.Entities
{
    // Parçanın üretimdeki/çizimdeki işlevini belirten tipler
    public enum PartType
    {
        Gövde,      // Yan, alt, üst tablalar
        Arkalik,    // Arkalık
        Raf,        // İç raf
        Kapak,      // Menteşeli kapak
        Cekmece,    // Çekmece klapası
        Baza,       // Baza tahtası veya profili
        Tac,        // Taç profili
        Ankastre    // Boş cihaz alanı
    }

    public class PartEntity
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string Name { get; set; } = "Yeni Parça";

        public PartType Type { get; set; } = PartType.Gövde; // Yeni eklenen özellik

        // Üretim Ölçüleri
        public double WidthMm { get; set; }
        public double HeightMm { get; set; }
        public double ThicknessMm { get; set; }
        public int BandEdgeCount { get; set; }

        // 2D Görsel Konum
        public RectangleF FrontViewBounds { get; set; }

        public PartEntity(string name, double width, double height, double thickness, PartType type = PartType.Gövde)
        {
            Name = name;
            WidthMm = width;
            HeightMm = height;
            ThicknessMm = thickness;
            Type = type;
        }
    }
}