using System;
using System.Drawing;
using Lineart.Core.Geometry;

namespace Lineart.Core.Entities
{
    public enum PartType
    {
        Gövde,
        Arkalik,
        Raf,
        Kapak,
        Cekmece,
        Baza,
        Tac,
        Ankastre
    }

    public class PartEntity
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string Name { get; set; } = "Yeni Parça";
        public PartType Type { get; set; } = PartType.Gövde;

        public double WidthMm { get; set; }
        public double HeightMm { get; set; }
        public double ThicknessMm { get; set; }
        public int BandEdgeCount { get; set; }

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