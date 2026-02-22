using Lineart.Core.Geometry;

namespace Lineart.Core.Entities;

public sealed class PartEntity
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = "Yeni Parça";
    public double WidthMm { get; set; }
    public double HeightMm { get; set; }
    public int BandEdgeCount { get; set; }

    public Point2D Origin { get; set; } = new(0, 0);
}
