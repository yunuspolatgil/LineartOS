using Lineart.Core.Geometry;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace Lineart.Core.Entities
{
    public class CabinetEntity
    {
        [Browsable(false)] // Ekranda ID'yi gizle
        public Guid Id { get; init; } = Guid.NewGuid();

        [Category("1. Genel Bilgiler")]
        [DisplayName("Dolap Adı")]
        public string Name { get; set; } = "Standart Alt Dolap";

        [Category("2. Dış Ölçüler (mm)")]
        [DisplayName("Baza/Yerden Yükseklik")]
        public double BaseHeight { get; set; } = 100; // Varsayılan 10 cm baza

        [Browsable(false)]
        public List<DimensionEntity> Dimensions { get; private set; } = new List<DimensionEntity>();

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
            Dimensions.Clear();

            float t = (float)MaterialThickness;
            float w = (float)TotalWidth;
            float h = (float)TotalHeight;
            float d = (float)TotalDepth;

            // 1. Sol Yan Tabla (Fiziksel Boy: Derinlik x Yükseklik | Görsel Boy: Kalınlık x Yükseklik)
            var solYan = new PartEntity("Sol Yan Tabla", d, h, t) { BandEdgeCount = 1 };
            solYan.FrontViewBounds = new RectangleF(0, 0, t, h);
            Parts.Add(solYan);

            // 2. Sağ Yan Tabla
            var sagYan = new PartEntity("Sağ Yan Tabla", d, h, t) { BandEdgeCount = 1 };
            sagYan.FrontViewBounds = new RectangleF(w - t, 0, t, h);
            Parts.Add(sagYan);

            // 3. Alt Tabla (İki yan arasına girer)
            float innerWidth = w - (2 * t);
            var altTabla = new PartEntity("Alt Tabla", innerWidth, d, t) { BandEdgeCount = 1 };
            altTabla.FrontViewBounds = new RectangleF(t, h - t, innerWidth, t);
            Parts.Add(altTabla);

            // 4. Üst Kayıtlar (Baza/Kayıt)
            var ustKayıt = new PartEntity("Üst Kayıt", innerWidth, 100, t) { BandEdgeCount = 1 };
            ustKayıt.FrontViewBounds = new RectangleF(t, 0, innerWidth, t);
            Parts.Add(ustKayıt);

            // 5. Arkalık (8mm)
            var arkalik = new PartEntity("Arkalık", innerWidth, h - (2 * t), 8) { BandEdgeCount = 0 };
            arkalik.FrontViewBounds = new RectangleF(t, t, innerWidth, h - (2 * t));
            Parts.Add(arkalik);

            // Not: Kapak parçasını eklersek dolabın içini göremeyiz. 
            // Şeffaf çizdiğimiz için eklenebilir ama iskeleti net görmek adına şimdilik yoruma alıyorum.
            /*
            var kapak = new PartEntity("Kapak", w - 4, h - 4, t) { BandEdgeCount = 4 };
            kapak.FrontViewBounds = new RectangleF(2, 2, w - 4, h - 4);
            Parts.Add(kapak);
            */

            // --- ÖLÇÜLENDİRME ÇİZGİLERİ ---
            // X Ekseni Ölçüsü (Genişlik) - Dolabın 50 mm yukarısına
            Dimensions.Add(new DimensionEntity(
                new Point2D(0, 0),
                new Point2D(w, 0),
                $"{w} mm",
                -50
            ));

            // Y Ekseni Ölçüsü (Yükseklik) - Dolabın 50 mm sağına
            Dimensions.Add(new DimensionEntity(
                new Point2D(w, 0),
                new Point2D(w, h),
                $"{h} mm",
                -50
            ));
        }
    }
}