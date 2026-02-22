using System;
using System.Drawing; // RectangleF için eklendi
using Lineart.Core.Geometry;

namespace Lineart.Core.Entities
{
    public class PartEntity
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string Name { get; set; } = "Yeni Parça";

        // ÜRETİM ÖLÇÜLERİ (Kesim listesine gidecek gerçek ölçüler)
        public double WidthMm { get; set; }
        public double HeightMm { get; set; }
        public double ThicknessMm { get; set; }
        public int BandEdgeCount { get; set; }

        // 2D ÇİZİM ÖLÇÜLERİ (Ekranda ön görünüşte nerede ve hangi boyutta çizilecek?)
        public RectangleF FrontViewBounds { get; set; }

        public PartEntity(string name, double width, double height, double thickness)
        {
            Name = name;
            WidthMm = width;
            HeightMm = height;
            ThicknessMm = thickness;
        }
    }
}