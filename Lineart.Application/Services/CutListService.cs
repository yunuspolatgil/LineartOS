using Lineart.Core.Document;

namespace Lineart.Application.Services;

public sealed class CutListRow
{
    public double WidthMm { get; init; }
    public double HeightMm { get; init; }
    public int BandEdgeCount { get; init; }
    public int Quantity { get; init; }
}

public sealed class CutListService
{
    public IReadOnlyList<CutListRow> Build(DrawingDocument doc)
    {
        return doc.Parts
            .GroupBy(p => new { p.WidthMm, p.HeightMm, p.BandEdgeCount })
            .Select(g => new CutListRow
            {
                WidthMm = g.Key.WidthMm,
                HeightMm = g.Key.HeightMm,
                BandEdgeCount = g.Key.BandEdgeCount,
                Quantity = g.Count()
            })
            .OrderByDescending(x => x.Quantity)
            .ToList();
    }
}
