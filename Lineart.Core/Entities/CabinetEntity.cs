using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using Lineart.Core.Geometry;

namespace Lineart.Core.Entities
{
    public class CabinetEntity
    {
        [Browsable(false)]
        public Guid Id { get; init; } = Guid.NewGuid();

        [Category("1. Genel Bilgiler")]
        [DisplayName("Dolap Adı")]
        public string Name { get; set; } = "Modüler Boş Dolap";

        [Category("2. Dış Ölçüler (mm)")]
        public double TotalWidth { get; set; } = 600;

        [Category("2. Dış Ölçüler (mm)")]
        public double TotalHeight { get; set; } = 720;

        [Category("2. Dış Ölçüler (mm)")]
        public double TotalDepth { get; set; } = 560;

        [Category("3. Aksesuarlar (mm)")]
        public double PlinthHeight { get; set; } = 100; // Baza

        [Category("3. Aksesuarlar (mm)")]
        public double CrownHeight { get; set; } = 0; // Taç

        [Category("5. Malzeme")]
        public double MaterialThickness { get; set; } = 18;

        [Category("6. Konum")]
        [DisplayName("1. Yatay Konum (X)")]
        [Description("Dolabın soldan sağa yerleşimi.")]
        public double PositionX { get; set; } = 0;

        [Category("6. Konum")]
        [DisplayName("2. Dikey Konum (Y) - Yerden Yükseklik")]
        [Description("Üst dolap eklediğinizde bu değeri azaltarak (Örn: -600) dolabı havaya kaldırabilirsiniz.")]
        public double PositionY { get; set; } = 0;

        [Browsable(false)] // Arka plan matematiğinin bozulmaması için eski Position'ı gizlice koruyoruz
        public Point2D Position
        {
            get => new Point2D(PositionX, PositionY);
            set
            {
                PositionX = value.X;
                PositionY = value.Y;
            }
        }

        [Browsable(false)] // Ekranda görünmesine gerek yok, sistem kullanacak
        public double OverallHeight => CrownHeight + TotalHeight + PlinthHeight;

        // YENİ EKLENEN MODÜL LİSTEMİZ (Sihir burada gerçekleşiyor)
        [Category("4. İç Yapı ve Modüller")]
        [DisplayName("Dolap Modülleri")]
        [Description("Dolabın içine eklenecek Kapak, Çekmece ve Rafları buradan yönetin.")]
        public BindingList<CabinetModule> Modules { get; set; } = new BindingList<CabinetModule>();

        [Browsable(false)]
        public List<PartEntity> Parts { get; private set; } = new List<PartEntity>();

        [Browsable(false)]
        public List<DimensionEntity> Dimensions { get; private set; } = new List<DimensionEntity>();

        public void RebuildParts()
        {
            Parts.Clear();
            Dimensions.Clear();

            float t = (float)MaterialThickness;
            float w = (float)TotalWidth;
            float h = (float)TotalHeight; // Gövde yüksekliği
            float d = (float)TotalDepth;
            float baza = (float)PlinthHeight;
            float tac = (float)CrownHeight;

            float currentGlobalY = 0; // Çizime en üstten (Y=0) başlıyoruz

            // 1. TAÇ (Crown) - En üstte
            if (tac > 0)
            {
                var tacPart = new PartEntity("Taç", w, tac, t, PartType.Tac);
                tacPart.FrontViewBounds = new RectangleF(0, currentGlobalY, w, tac);
                Parts.Add(tacPart);
                currentGlobalY += tac; // Taç kadar aşağı in
            }

            // Gövdenin başlayacağı asıl Y noktası
            float goveyY = currentGlobalY;

            // 2. ANA KARKAS (Yanlar, Alt, Üst Tablalar)
            var solYan = new PartEntity("Sol Yan", d, h, t, PartType.Gövde);
            solYan.FrontViewBounds = new RectangleF(0, goveyY, t, h);
            Parts.Add(solYan);

            var sagYan = new PartEntity("Sağ Yan", d, h, t, PartType.Gövde);
            sagYan.FrontViewBounds = new RectangleF(w - t, goveyY, t, h);
            Parts.Add(sagYan);

            float innerW = w - (2 * t);

            var ustTabla = new PartEntity("Üst Tabla", innerW, d, t, PartType.Gövde);
            ustTabla.FrontViewBounds = new RectangleF(t, goveyY, innerW, t);
            Parts.Add(ustTabla);

            var altTabla = new PartEntity("Alt Tabla", innerW, d, t, PartType.Gövde);
            altTabla.FrontViewBounds = new RectangleF(t, goveyY + h - t, innerW, t);
            Parts.Add(altTabla);

            // 3. İÇ MODÜLLERİ HESAPLA VE YERLEŞTİR
            float innerH = h - (2 * t);
            float currentY = goveyY + t;

            float fixedHeightSum = 0;
            int autoCount = 0;
            foreach (var mod in Modules)
            {
                if (mod.HeightMm > 0) fixedHeightSum += (float)mod.HeightMm;
                else autoCount++;
            }

            float autoHeight = autoCount > 0 ? Math.Max(0, (innerH - fixedHeightSum) / autoCount) : 0;

            for (int m = 0; m < Modules.Count; m++)
            {
                var mod = Modules[m];
                float modH = mod.HeightMm > 0 ? (float)mod.HeightMm : autoHeight;

                switch (mod.Type)
                {
                    case ModuleType.Ankastre:
                        var ankPart = new PartEntity("Ankastre", innerW, modH, 0, PartType.Ankastre);
                        ankPart.FrontViewBounds = new RectangleF(t, currentY, innerW, modH);
                        Parts.Add(ankPart);
                        break;

                    case ModuleType.Cekmece:
                        int dCount = Math.Max(1, mod.SubItemCount);
                        float dHeight = modH / dCount;
                        for (int i = 0; i < dCount; i++)
                        {
                            var dPart = new PartEntity("Çekmece", w - 4, dHeight - 4, t, PartType.Cekmece);
                            dPart.FrontViewBounds = new RectangleF(2, currentY + (i * dHeight) + 2, w - 4, dHeight - 4);
                            Parts.Add(dPart);
                        }
                        break;

                    case ModuleType.Kapak:
                        int doorCount = Math.Max(1, mod.SubItemCount);
                        float doorW = (w - 4) / doorCount;
                        for (int i = 0; i < doorCount; i++)
                        {
                            var doorPart = new PartEntity("Kapak", doorW - 2, modH - 4, t, PartType.Kapak);
                            doorPart.FrontViewBounds = new RectangleF(2 + (i * doorW), currentY + 2, doorW - 2, modH - 4);
                            Parts.Add(doorPart);
                        }
                        break;

                    case ModuleType.AcikRaf:
                        int shelfCount = Math.Max(1, mod.SubItemCount);
                        float shelfSpacing = modH / (shelfCount + 1);
                        for (int i = 1; i <= shelfCount; i++)
                        {
                            var raf = new PartEntity("Raf", innerW - 2, d - 20, t, PartType.Raf);
                            raf.FrontViewBounds = new RectangleF(t, currentY + (i * shelfSpacing), innerW, t);
                            Parts.Add(raf);

                            Dimensions.Add(new DimensionEntity(
                                new Point2D(w + 10, currentY + (i * shelfSpacing) - shelfSpacing),
                                new Point2D(w + 10, currentY + (i * shelfSpacing)),
                                $"İç: {Math.Round(shelfSpacing)}", 0));
                        }
                        break;
                }

                // Modül dış ölçüsü
                Dimensions.Add(new DimensionEntity(
                    new Point2D(w, currentY),
                    new Point2D(w, currentY + modH),
                    $"{Math.Round(modH)}", -35));

                currentY += modH;

                // Modüller arasına ahşap raf (Ara Tabla)
                if (m < Modules.Count - 1 && innerH > 0)
                {
                    var araTabla = new PartEntity("Ara Tabla", innerW, d, t, PartType.Gövde);
                    araTabla.FrontViewBounds = new RectangleF(t, currentY - (t / 2), innerW, t);
                    Parts.Add(araTabla);
                }
            }

            currentGlobalY += h; // Gövdeyi de geçtik, artık en alt noktadayız

            // 4. BAZA (Plinth) - En Altta
            if (baza > 0)
            {
                var bazaPart = new PartEntity("Baza", w, baza, t, PartType.Baza);
                bazaPart.FrontViewBounds = new RectangleF(0, currentGlobalY, w, baza);
                Parts.Add(bazaPart);
            }

            // --- DIŞ GABARİ ÖLÇÜLENDİRME ---
            Dimensions.Add(new DimensionEntity(new Point2D(0, goveyY), new Point2D(w, goveyY), $"{w} mm", -60));
            Dimensions.Add(new DimensionEntity(new Point2D(w, goveyY), new Point2D(w, goveyY + h), $"{h} mm", -80));
        }
    }
}