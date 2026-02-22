using Lineart.Core.Entities;

namespace Lineart.Core.Document;

public sealed class DrawingDocument
{
    public string ProjectName { get; set; } = "Yeni Proje";
    public List<PartEntity> Parts { get; } = new();
}
