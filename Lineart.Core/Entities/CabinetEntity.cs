using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text.Json;
using System.Text.Json.Serialization;
using Lineart.Core.Geometry;

namespace Lineart.Core.Entities
{
    public enum JoinType { Icten, Bindirme }
    public enum TopPanelStyle { TamDolu, OnArkaKayit }
    public enum StripOrientation { Yatay, Dikey }
    public enum BackPanelType { Cakma, Kanalli }
    public enum CabinetType { Standart, EvyeDolabi, MakineBoslugu, Kapama, Kose_L }
    public enum HandleType { StandartKulp, BasAc, GolaProfil, DuzProfil }
    public enum FillerType { Dikey_DuvarArasi, Yatay_TavanArasi, YanBitis_Paneli }
    public enum FillerShape { Duz, L_Seklinde }
    public enum FillerMaterialGroup { Govde, Kapak }

    public class CabinetEntity
    {
        [Browsable(false)]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Category("1. Genel Bilgiler")]
        [DisplayName("Dolap Adı")]
        public string Name { get; set; } = "Yeni Dolap";

        [Category("1. Genel Bilgiler")]
        [DisplayName("Dolap Tipi")]
        public CabinetType Type { get; set; } = CabinetType.Standart;

        [Category("2. Dış Ölçüler (mm)")]
        [DisplayName("1. Genişlik (Sol Kol)")]
        public double TotalWidth { get; set; } = 600;

        // YENİ EKLENEN: L Köşe için Sağ Kol Genişliği
        [Category("2. Dış Ölçüler (mm)")]
        [DisplayName("1.1 Genişlik (Sağ Kol - Sadece L Köşe)")]
        public double RightWidth { get; set; } = 900;

        [Category("2. Dış Ölçüler (mm)")]
        [DisplayName("2. Yükseklik")]
        public double TotalHeight { get; set; } = 720;

        [Category("2. Dış Ölçüler (mm)")]
        [DisplayName("3. Derinlik")]
        public double TotalDepth { get; set; } = 560;

        [Category("3. Aksesuarlar (mm)")]
        [DisplayName("Baza Yüksekliği")]
        public double PlinthHeight { get; set; } = 100;

        [Category("3. Aksesuarlar (mm)")]
        [DisplayName("Taç Yüksekliği")]
        public double CrownHeight { get; set; } = 0;

        [Browsable(false)]
        public double OverallHeight => CrownHeight + TotalHeight + PlinthHeight;

        [Category("4. İç Yapı")]
        [DisplayName("Modüller (Kapak/Çekmece)")]
        public BindingList<CabinetModule> Modules { get; set; } = new BindingList<CabinetModule>();

        [Category("4. İç Yapı")]
        [DisplayName("Kulp Tipi")]
        public HandleType Handle { get; set; } = HandleType.StandartKulp;

        // --- İSTİSNAİ İMALAT KURALLARI ---
        [Category("5. İmalat (Bu Dolaba Özel)")][DisplayName("1. Gövde Kalınlığı")] public double MaterialThickness { get; set; } = 18;
        [Category("5. İmalat (Bu Dolaba Özel)")][DisplayName("2. Alt Birleşim Tipi")] public JoinType BottomJoin { get; set; } = JoinType.Icten;
        [Category("5. İmalat (Bu Dolaba Özel)")][DisplayName("3. Üst Birleşim Tipi")] public JoinType TopJoin { get; set; } = JoinType.Icten;
        [Category("5. İmalat (Bu Dolaba Özel)")][DisplayName("4. Set Altı / Üst")] public TopPanelStyle TopStyle { get; set; } = TopPanelStyle.OnArkaKayit;
        [Category("5. İmalat (Bu Dolaba Özel)")][DisplayName("5. Ön Üst Kayıt")] public StripOrientation FrontTopStripOrientation { get; set; } = StripOrientation.Yatay;
        [Category("5. İmalat (Bu Dolaba Özel)")][DisplayName("6. Arka Üst Kayıt")] public StripOrientation BackTopStripOrientation { get; set; } = StripOrientation.Dikey;
        [Category("5. İmalat (Bu Dolaba Özel)")][DisplayName("7. Kayıt Genişliği")] public double TopStripWidth { get; set; } = 100;
        [Category("5. İmalat (Bu Dolaba Özel)")][DisplayName("8. Arkalık Tipi")] public BackPanelType BackType { get; set; } = BackPanelType.Kanalli;

        [Category("6. İleri Seviye (Boşluk/Bant)")][DisplayName("Kenar Çalışma Payı")] public double WorkingGap { get; set; } = 2.0;
        [Category("6. İleri Seviye (Boşluk/Bant)")][DisplayName("Bant Kalınlığı")] public double EdgeBandThickness { get; set; } = 1.0;

        [Category("7. Kapama / Bitiş Ayarları")][DisplayName("1. Kapama Yönü")] public FillerType FillerPanelType { get; set; } = FillerType.Dikey_DuvarArasi;
        [Category("7. Kapama / Bitiş Ayarları")][DisplayName("2. Kapama Şekli")] public FillerShape FillerShapeType { get; set; } = FillerShape.Duz;
        [Category("7. Kapama / Bitiş Ayarları")][DisplayName("3. Malzeme Sınıfı")] public FillerMaterialGroup FillerMaterial { get; set; } = FillerMaterialGroup.Kapak;
        [Category("7. Kapama / Bitiş Ayarları")][DisplayName("4. L Dönüş Derinliği")] public double FillerReturnDepth { get; set; } = 100;

        [Browsable(false)] public double PositionX { get; set; } = 0;
        [Browsable(false)] public double PositionY { get; set; } = 0;

        [Browsable(false)]
        public Point2D Position
        {
            get => new Point2D(PositionX, PositionY);
            set { PositionX = value.X; PositionY = value.Y; }
        }

        [JsonIgnore]
        [Browsable(false)]
        public List<PartEntity> Parts { get; private set; } = new List<PartEntity>();

        [JsonIgnore]
        [Browsable(false)]
        public List<DimensionEntity> Dimensions { get; private set; } = new List<DimensionEntity>();

        public CabinetEntity Clone()
        {
            string json = JsonSerializer.Serialize(this);
            var clone = JsonSerializer.Deserialize<CabinetEntity>(json);
            clone.Id = Guid.NewGuid();
            clone.Name = this.Name + " (Kopya)";
            clone.PositionX += clone.TotalWidth;
            clone.RebuildParts();
            return clone;
        }

        public void RebuildParts()
        {
            Parts.Clear();
            Dimensions.Clear();

            float t = (float)MaterialThickness;
            float w = (float)TotalWidth;
            float h = (float)TotalHeight;
            float d = (float)TotalDepth;

            // KAPAMA MOTORU
            if (Type == CabinetType.Kapama)
            {
                PartType pType = (FillerMaterial == FillerMaterialGroup.Kapak) ? PartType.Kapak : PartType.Gövde;

                if (FillerPanelType == FillerType.Dikey_DuvarArasi)
                {
                    var onKapama = new PartEntity("DİKEY KAPAMA", w, h, t, pType);
                    onKapama.FrontViewBounds = new RectangleF(0, 0, w, h);
                    Parts.Add(onKapama);

                    if (FillerShapeType == FillerShape.L_Seklinde)
                    {
                        var donus = new PartEntity("DÖNÜŞ PARÇASI", (float)FillerReturnDepth, h, t, PartType.Gövde);
                        donus.FrontViewBounds = new RectangleF(w - t, 0, t, h);
                        Parts.Add(donus);
                    }
                }
                else if (FillerPanelType == FillerType.Yatay_TavanArasi)
                {
                    var yatayKapama = new PartEntity("YATAY KAPAMA", w, h, t, pType);
                    yatayKapama.FrontViewBounds = new RectangleF(0, 0, w, h);
                    Parts.Add(yatayKapama);

                    if (FillerShapeType == FillerShape.L_Seklinde)
                    {
                        var donus = new PartEntity("DÖNÜŞ PARÇASI", w, (float)FillerReturnDepth, t, PartType.Gövde);
                        donus.FrontViewBounds = new RectangleF(0, h - t, w, t);
                        Parts.Add(donus);
                    }
                }
                else if (FillerPanelType == FillerType.YanBitis_Paneli)
                {
                    var yanPanel = new PartEntity("YAN BİTİŞ PANELİ", d, h, w, pType);
                    yanPanel.FrontViewBounds = new RectangleF(0, 0, w, h);
                    Parts.Add(yanPanel);
                }

                Dimensions.Add(new DimensionEntity(new Point2D(0, 0), new Point2D(w, 0), $"{w} mm", -30));
                Dimensions.Add(new DimensionEntity(new Point2D(w, 0), new Point2D(w, h), $"{h} mm", -50));
                return;
            }



            float baza = (float)PlinthHeight;
            float tac = (float)CrownHeight;
            float gap = (float)WorkingGap;
            float band = (float)EdgeBandThickness;

            // ==========================================
            // YENİ EKLENEN: L KÖŞE DOLAP (90 DERECE) MOTORU
            // ==========================================
            

            float currentGlobalY = 0;

            if (tac > 0)
            {
                var tacPart = new PartEntity("Taç", w, tac, t, PartType.Tac);
                tacPart.FrontViewBounds = new RectangleF(0, currentGlobalY, w, tac);
                Parts.Add(tacPart);
                currentGlobalY += tac;
            }

            float goveyY = currentGlobalY;

            bool hasDoor = false;
            if (Modules != null)
            {
                foreach (var m in Modules)
                    if (m.Type == ModuleType.Kapak || m.Type == ModuleType.AnkastreBulasikTam || m.Type == ModuleType.AnkastreBulasikYarim) hasDoor = true;
            }

            bool isMakine = (Type == CabinetType.MakineBoslugu);
            bool isMakineKapakli = isMakine && hasDoor;
            bool isMakineKapaksiz = isMakine && !hasDoor;
            bool isEvye = (Type == CabinetType.EvyeDolabi);

            float yanBoy = h;
            float altGenislik = w;
            float ustGenislikYatay = w;
            float ustGenislikDikey = w - (2 * t);

            float solYanY = goveyY;
            float altY = goveyY + h - t;
            float ustY = goveyY;

            if (BottomJoin == JoinType.Icten) altGenislik = w - (2 * t);
            else { altGenislik = w; yanBoy -= t; }

            if (TopJoin == JoinType.Icten) ustGenislikYatay = w - (2 * t);
            else { ustGenislikYatay = w; yanBoy -= t; solYanY += t; altY = goveyY + h - t; }

            // 1. YAN TABLALAR
            if (!isMakineKapaksiz)
            {
                float yanDerinlik = isMakineKapakli ? (float)TopStripWidth : d;
                string yanAd = isMakineKapakli ? "Sol Yan (Çatkı)" : "Sol Yan";
                string sagAd = isMakineKapakli ? "Sağ Yan (Çatkı)" : "Sağ Yan";

                var solYan = new PartEntity(yanAd, yanDerinlik, yanBoy, t, PartType.Gövde);
                solYan.FrontViewBounds = new RectangleF(0, solYanY, t, yanBoy);
                Parts.Add(solYan);

                var sagYan = new PartEntity(sagAd, yanDerinlik, yanBoy, t, PartType.Gövde);
                sagYan.FrontViewBounds = new RectangleF(w - t, solYanY, t, yanBoy);
                Parts.Add(sagYan);
            }
            else
            {
                Dimensions.Add(new DimensionEntity(new Point2D(0, goveyY + h / 2), new Point2D(w, goveyY + h / 2), "MAKİNE BOŞLUĞU", 0));
            }

            // 2. ALT TABLA
            if (!isMakine)
            {
                float altX = (BottomJoin == JoinType.Icten) ? t : 0;
                var altTabla = new PartEntity("Alt Tabla", altGenislik, d, t, PartType.Gövde);
                altTabla.FrontViewBounds = new RectangleF(altX, altY, altGenislik, t);
                Parts.Add(altTabla);
            }

            // 3. ÜST TABLA / KAYITLAR
            float ustX = (TopJoin == JoinType.Icten) ? t : 0;

            if (TopStyle == TopPanelStyle.TamDolu && !isMakine && !isEvye)
            {
                var ustTabla = new PartEntity("Üst Tabla", ustGenislikYatay, d, t, PartType.Gövde);
                ustTabla.FrontViewBounds = new RectangleF(ustX, ustY, ustGenislikYatay, t);
                Parts.Add(ustTabla);
            }
            else
            {
                float kayitD = (float)TopStripWidth;
                float kayitW_Yatay = isMakineKapaksiz ? w : ustGenislikYatay;
                float kayitW_Dikey = isMakineKapaksiz ? w : ustGenislikDikey;
                float kayitX = isMakineKapaksiz ? 0 : ustX;
                float kayitX_Dikey = isMakineKapaksiz ? 0 : t;

                bool forceVerticalFront = isEvye;

                if (FrontTopStripOrientation == StripOrientation.Yatay && !forceVerticalFront)
                {
                    var onKayit = new PartEntity("Ön Üst Kayıt", kayitW_Yatay, kayitD, t, PartType.Gövde);
                    onKayit.FrontViewBounds = new RectangleF(kayitX, ustY, kayitW_Yatay, t);
                    Parts.Add(onKayit);
                }
                else
                {
                    var onKayitDikey = new PartEntity("Ön Üst Kayıt (Dik)", kayitW_Dikey, kayitD, t, PartType.Gövde);
                    onKayitDikey.FrontViewBounds = new RectangleF(kayitX_Dikey, ustY, kayitW_Dikey, kayitD);
                    Parts.Add(onKayitDikey);
                }

                if (!isMakineKapakli)
                {
                    float arkaKayitY_Yatay = isEvye ? altY - t : ustY;
                    float arkaKayitY_Dikey = isEvye ? altY - kayitD : ustY;

                    if (BackTopStripOrientation == StripOrientation.Yatay)
                    {
                        var arkaKayit = new PartEntity(isEvye ? "Arka Alt Kayıt" : "Arka Üst Kayıt", kayitW_Yatay, kayitD, t, PartType.Gövde);
                        arkaKayit.FrontViewBounds = new RectangleF(kayitX, arkaKayitY_Yatay, kayitW_Yatay, t);
                        Parts.Add(arkaKayit);
                    }
                    else
                    {
                        var arkaKayitDikey = new PartEntity(isEvye ? "Arka Alt Kayıt (Dik)" : "Arka Üst Kayıt (Dik)", kayitW_Dikey, kayitD, t, PartType.Gövde);
                        arkaKayitDikey.FrontViewBounds = new RectangleF(kayitX_Dikey, arkaKayitY_Dikey, kayitW_Dikey, kayitD);
                        Parts.Add(arkaKayitDikey);
                    }
                }
            }

            // 4. MODÜLLER
            float innerW = w - (2 * t);
            float frontStartY = goveyY;
            float frontTotalH = h;

            float golaPayi = (Handle == HandleType.GolaProfil && !isMakine) ? 35f : 0f;
            frontTotalH -= golaPayi;
            frontStartY += golaPayi;

            float fixedHeightSum = 0;
            int autoCount = 0;

            List<Tuple<float, float>> ankastreExclusions = new List<Tuple<float, float>>();

            if (Modules != null)
            {
                foreach (var mod in Modules)
                {
                    if (mod.Type == ModuleType.AnkastreFirin) mod.HeightMm = 600 + t;
                    else if (mod.Type == ModuleType.AnkastreMikrodalga) mod.HeightMm = 380 + t;

                    if (mod.HeightMm > 0) fixedHeightSum += (float)mod.HeightMm;
                    else autoCount++;
                }

                float autoHeight = autoCount > 0 ? Math.Max(0, (frontTotalH - fixedHeightSum) / autoCount) : 0;
                float currentY = frontStartY;

                for (int m = 0; m < Modules.Count; m++)
                {
                    var mod = Modules[m];
                    float modH = mod.HeightMm > 0 ? (float)mod.HeightMm : autoHeight;

                    // A) MODÜLÜN ÖN YÜZEYİNİ ÇİZ
                    switch (mod.Type)
                    {
                        // --- YENİ EKLENEN BULAŞIK MAKİNESİ MANTIĞI ---
                        case ModuleType.AnkastreBulasikTam:
                            // Tam Ankastre: Kapak tüm modül boyunu kaplar (Normal kapak gibi)
                            var tamAnkKapak = new PartEntity("Bulaşık Makinesi Kapağı (Tam)", w - gap * 2 - band * 2, modH - gap * 2 - band * 2, t, PartType.Kapak);
                            // Not: Makine boşluğu genelde baza hizasından başlar, biz modül neredeyse oraya koyuyoruz
                            // Ancak Bulaşık kapakları genellikle tam kapaktır (width = w, değil innerW)
                            // Çünkü makine dolabın içine girmez, iki dolap arasına girer. 
                            // Eğer bu bir 'Makine Boşluğu' kabiniyse genişlik tam (w) olmalı.
                            float tamKapakW = isMakineKapaksiz || isMakineKapakli ? w - gap * 2 : w - gap * 2;
                            tamAnkKapak.FrontViewBounds = new RectangleF(gap, currentY + gap, tamKapakW, modH - gap * 2);
                            Parts.Add(tamAnkKapak);

                            // Arkalık iptali için bu alanı kaydet (Makine varsa arkada arkalık olmaz)
                            ankastreExclusions.Add(new Tuple<float, float>(currentY, currentY + modH));
                            break;

                        case ModuleType.AnkastreBulasikYarim:
                            // Yarım Ankastre: Üstte Panel, Altta Kapak
                            // Standart Panel Yüksekliği: ~140mm (Ayarlanabilir olması ideal ama standart yapalım)
                            float panelH = 140f;
                            float yarimKapakH = modH - panelH;

                            // 1. Makine Paneli (Ankastre Cihaz Görünümü)
                            var panelPart = new PartEntity("Bulaşık Makinesi Paneli", isMakineKapaksiz ? w : innerW, panelH, 0, PartType.Ankastre);
                            panelPart.FrontViewBounds = new RectangleF(isMakineKapaksiz ? 0 : t, currentY, isMakineKapaksiz ? w : innerW, panelH);
                            Parts.Add(panelPart);

                            // 2. Yarım Kapak
                            var yarimKapak = new PartEntity("Bulaşık Makinesi Kapağı (Yarım)", w - gap * 2 - band * 2, yarimKapakH - gap * 2 - band * 2, t, PartType.Kapak);
                            yarimKapak.FrontViewBounds = new RectangleF(gap, currentY + panelH + gap, w - gap * 2, yarimKapakH - gap * 2);
                            Parts.Add(yarimKapak);

                            ankastreExclusions.Add(new Tuple<float, float>(currentY, currentY + modH));
                            break;

                        case ModuleType.AnkastreFirin:
                        case ModuleType.AnkastreMikrodalga:
                            float ankNetH = modH - t;
                            float ankY = currentY + (t / 2);
                            string ankName = mod.Type == ModuleType.AnkastreFirin ? "Ankastre Fırın" : "Mikrodalga";
                            var ankPart = new PartEntity(ankName, innerW, ankNetH, 0, PartType.Ankastre);
                            ankPart.FrontViewBounds = new RectangleF(t, ankY, innerW, ankNetH);
                            Parts.Add(ankPart);
                            ankastreExclusions.Add(new Tuple<float, float>(currentY, currentY + modH));
                            break;

                        case ModuleType.Cekmece:
                            int dCount = Math.Max(1, mod.SubItemCount);
                            float dHeight = modH / dCount;
                            for (int i = 0; i < dCount; i++)
                            {
                                float drawY = currentY + (i * dHeight);
                                float visualW = w - (gap * 2);
                                float visualH = dHeight - (gap * 2);
                                if (Handle == HandleType.GolaProfil && i > 0)
                                {
                                    drawY += 35f;
                                    visualH -= 35f;
                                }
                                float cutW = visualW - (band * 2);
                                float cutH = visualH - (band * 2);
                                var dPart = new PartEntity("Çekmece", cutW, cutH, t, PartType.Cekmece);
                                dPart.FrontViewBounds = new RectangleF(gap, drawY + gap, visualW, visualH);
                                Parts.Add(dPart);
                            }
                            break;

                        case ModuleType.Kapak:
                            int doorCount = Math.Max(1, mod.SubItemCount);
                            float partW = isMakineKapaksiz ? w / doorCount : w / doorCount;
                            for (int i = 0; i < doorCount; i++)
                            {
                                float visualW = partW - (gap * 2);
                                float visualH = modH - (gap * 2);
                                float cutW = visualW - (band * 2);
                                float cutH = visualH - (band * 2);
                                var doorPart = new PartEntity("Kapak", cutW, cutH, t, PartType.Kapak);
                                float doorX = isMakineKapaksiz ? (i * partW) + gap : (i * partW) + gap;
                                doorPart.FrontViewBounds = new RectangleF(doorX, currentY + gap, visualW, visualH);
                                Parts.Add(doorPart);
                            }
                            break;
                    }

                    // B) SEYYAR RAF (Sadece Kapağın içine, Makineye değil!)
                    if (mod.InnerShelfCount > 0 && !isMakineKapaksiz && mod.Type == ModuleType.Kapak)
                    {
                        float shelfSpacing = modH / (mod.InnerShelfCount + 1);
                        for (int r = 1; r <= mod.InnerShelfCount; r++)
                        {
                            float shelfY = currentY + (r * shelfSpacing) - (t / 2);
                            var seyyarRaf = new PartEntity("SEYYAR RAF", innerW, d - 30, t, PartType.Raf);
                            seyyarRaf.FrontViewBounds = new RectangleF(t, shelfY, innerW, t);
                            Parts.Add(seyyarRaf);
                        }
                    }

                    currentY += modH;

                    // C) SABİT RAF (Makine ve Fırın arasına konur mu? Fırın altına EVET, Makine altına HAYIR)
                    if (m < Modules.Count - 1 && !isMakine)
                    {
                        float sabitRafY = currentY - (t / 2);
                        float sabitRafD = BackType == BackPanelType.Kanalli ? (d - 18) : d;
                        var sabitRaf = new PartEntity("SABİT RAF", innerW, sabitRafD, t, PartType.Raf);
                        sabitRaf.FrontViewBounds = new RectangleF(t, sabitRafY, innerW, t);
                        Parts.Add(sabitRaf);
                    }
                }
            }

            // 5. DİNAMİK ARKALIK (Parçalama Algoritması)
            if (!isMakine && !isEvye)
            {
                float backStripH = (TopStyle == TopPanelStyle.TamDolu) ? t : (BackTopStripOrientation == StripOrientation.Dikey ? (float)TopStripWidth : t);
                float arkalikW = BackType == BackPanelType.Cakma ? w : w - (2 * t) + 16;
                float arkalikY_start = BackType == BackPanelType.Cakma ? goveyY : goveyY + backStripH - 8;
                float arkalikH_total = BackType == BackPanelType.Cakma ? h : h - t - backStripH + 16;
                float arkalikY_end = arkalikY_start + arkalikH_total;

                List<Tuple<float, float>> segments = new List<Tuple<float, float>>
                {
                    new Tuple<float, float>(arkalikY_start, arkalikY_end)
                };

                foreach (var exc in ankastreExclusions)
                {
                    List<Tuple<float, float>> newSegments = new List<Tuple<float, float>>();
                    foreach (var seg in segments)
                    {
                        if (exc.Item2 <= seg.Item1 || exc.Item1 >= seg.Item2)
                        {
                            newSegments.Add(seg);
                        }
                        else
                        {
                            if (seg.Item1 < exc.Item1) newSegments.Add(new Tuple<float, float>(seg.Item1, exc.Item1));
                            if (seg.Item2 > exc.Item2) newSegments.Add(new Tuple<float, float>(exc.Item2, seg.Item2));
                        }
                    }
                    segments = newSegments;
                }

                int aCount = 1;
                foreach (var seg in segments)
                {
                    float segH = seg.Item2 - seg.Item1;
                    if (segH > 10)
                    {
                        string aName = segments.Count > 1 ? $"Arkalık {aCount++}" : "Arkalık";
                        var arkalik = new PartEntity(aName, arkalikW, segH, 8, PartType.Arkalik);
                        arkalik.FrontViewBounds = new RectangleF((w - arkalikW) / 2, seg.Item1, arkalikW, segH);
                        Parts.Add(arkalik);
                    }
                }
            }

            currentGlobalY += h;

            // 6. BAZA
            if (baza > 0 && !isMakine)
            {
                var bazaPart = new PartEntity("Baza", w, baza, t, PartType.Baza);
                bazaPart.FrontViewBounds = new RectangleF(0, currentGlobalY, w, baza);
                Parts.Add(bazaPart);
            }

            Dimensions.Add(new DimensionEntity(new Point2D(0, goveyY), new Point2D(w, goveyY), $"{w} mm", -60));
            Dimensions.Add(new DimensionEntity(new Point2D(w, goveyY), new Point2D(w, goveyY + h), $"{h} mm", -80));
        }
    }
}