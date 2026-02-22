using Lineart.Core.Geometry;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Lineart.Core.Entities
{
    public class CabinetEntity
    {
        [Browsable(false)] // Ekranda ID'yi gizle
        public Guid Id { get; init; } = Guid.NewGuid();

        [Category("1. Genel Bilgiler")]
        [DisplayName("Dolap Adı")]
        public string Name { get; set; } = "Standart Alt Dolap";

        // Parametrik ölçüler (mm cinsinden)
        [Category("2. Dış Ölçüler (mm)")]
        [DisplayName("Toplam Genişlik")]
        public double TotalWidth { get; set; } = 600;

        [Category("2. Dış Ölçüler (mm)")]
        [DisplayName("Toplam Yükseklik")]
        public double TotalHeight { get; set; } = 720;

        [Category("2. Dış Ölçüler (mm)")]
        [DisplayName("Toplam Derinlik")]
        public double TotalDepth { get; set; } = 560;

        [Category("3. Malzeme")]
        [DisplayName("Gövde Kalınlığı")]
        public double MaterialThickness { get; set; } = 18;

        [Browsable(false)] // Parça listesini PropertyGrid'de gösterme, TreeList'te göstereceğiz
        public List<PartEntity> Parts { get; private set; } = new List<PartEntity>();

        [Browsable(false)]
        public Point2D Position { get; set; } = new Point2D(0, 0);

        // Parametrik hesaplayıcı metod
        public void RebuildParts()
        {
            Parts.Clear();

            // 1. Yan Tablalar (Sol ve Sağ)
            // Yükseklik: Tam boy, Derinlik: Tam derinlik
            Parts.Add(new PartEntity("Sol Yan Tabla", TotalDepth, TotalHeight, MaterialThickness) { BandEdgeCount = 1 });
            Parts.Add(new PartEntity("Sağ Yan Tabla", TotalDepth, TotalHeight, MaterialThickness) { BandEdgeCount = 1 });

            // 2. Alt ve Üst Tablalar (İki yan tabla arasına girer)
            // Genişlik = Toplam Genişlik - (2 * Malzeme Kalınlığı)
            double innerWidth = TotalWidth - (2 * MaterialThickness);
            Parts.Add(new PartEntity("Alt Tabla", innerWidth, TotalDepth, MaterialThickness) { BandEdgeCount = 1 });
            Parts.Add(new PartEntity("Üst Kayıt Ön", innerWidth, 100, MaterialThickness) { BandEdgeCount = 1 });
            Parts.Add(new PartEntity("Üst Kayıt Arka", innerWidth, 100, MaterialThickness) { BandEdgeCount = 1 });

            // 3. Arkalık (Örn: 8mm sunta, kanallı veya bindirme)
            // Basit bindirme hesabı:
            Parts.Add(new PartEntity("Arkalık", TotalWidth, TotalHeight, 8) { BandEdgeCount = 0 });

            // 4. Kapak (Çalışma boşlukları düşülmüş - örn her yönden 2mm)
            double doorWidth = TotalWidth - 4;
            double doorHeight = TotalHeight - 4;
            Parts.Add(new PartEntity("Kapak", doorWidth, doorHeight, MaterialThickness) { BandEdgeCount = 4 });
        }
    }
}