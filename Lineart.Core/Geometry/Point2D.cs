namespace Lineart.Core.Geometry
{
    // Ekranda ve PDF/DXF çıktılarında kullanılacak temel 2D nokta (mm cinsinden)
    public readonly record struct Point2D(double X, double Y);

    // Genişlik ve yükseklik ölçülerini tutan yapı (mm)
    public readonly record struct Size2D(double Width, double Height);
}