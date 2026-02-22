using Lineart.Core.Geometry;

namespace Lineart.Core.Entities
{
    public class DimensionEntity
    {
        public Point2D StartPoint { get; set; }
        public Point2D EndPoint { get; set; }
        public string Text { get; set; }

        // Ölçü çizgisinin parçadan ne kadar uzaklıkta çizileceğini belirler (mm)
        public double Offset { get; set; }

        public DimensionEntity(Point2D start, Point2D end, string text, double offset = 50)
        {
            StartPoint = start;
            EndPoint = end;
            Text = text;
            Offset = offset;
        }
    }
}