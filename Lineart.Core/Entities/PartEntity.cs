using System;
using Lineart.Core.Geometry;

namespace Lineart.Core.Entities
{
    public class PartEntity
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string Name { get; set; } = "Yeni Parça";

        // Ölçüler (mm)
        public double WidthMm { get; set; }
        public double HeightMm { get; set; }
        public double ThicknessMm { get; set; }

        // Üretim detayları
        public int BandEdgeCount { get; set; }

        // Çizim sırasındaki konumu
        public Point2D Location { get; set; } = new Point2D(0, 0);

        public PartEntity(string name, double width, double height, double thickness)
        {
            Name = name;
            WidthMm = width;
            HeightMm = height;
            ThicknessMm = thickness;
        }
    }
}