using System.Collections.Generic;
using Lineart.Core.Entities;

namespace Lineart.Core.Document
{
    public sealed class DrawingDocument
    {
        public string ProjectName { get; set; } = "Yeni Proje";

        // Sahnede yer alan parametrik dolaplar
        public List<CabinetEntity> Cabinets { get; } = new List<CabinetEntity>();

        // Sahnede yer alan serbest çizim/parçalar
        public List<PartEntity> StandaloneParts { get; } = new List<PartEntity>();
        public object Parts { get; set; }

        public void AddCabinet(CabinetEntity cabinet)
        {
            cabinet.RebuildParts(); // Eklendiği an parçaları hesapla
            Cabinets.Add(cabinet);
        }
    }
}