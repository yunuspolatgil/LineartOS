using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using Lineart.Core.Entities;

namespace Lineart.WinForms.Controls
{

    public partial class IsometricViewControl : XtraUserControl
    {
        private CabinetEntity _cabinet;

        public IsometricViewControl()
        {
            this.DoubleBuffered = true;
            this.BackColor = Color.FromArgb(25, 25, 30); // Siyahımsı şık bir arka plan
        }

        // Dışarıdan seçili dolabı bu panele gönderdiğimiz metot
        public void SetCabinet(CabinetEntity cabinet)
        {
            _cabinet = cabinet;
            this.Invalidate(); // Ekranı yeniden çiz
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (_cabinet == null) return;

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // 1. Z-Order (Derinlik) Sıralaması
            var parts3D = _cabinet.Parts
                .Select(p => Get3DBounds(p, _cabinet))
                .OrderByDescending(p => p.Z)
                .ThenByDescending(p => p.X)
                .ToList();

            if (parts3D.Count == 0) return;

            // --- 2. AUTO-FIT (OTOMATİK SIĞDIRMA VE ORTALAMA) ALGORİTMASI ---
            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;

            // Tüm dolabın 3D uzayda kapladığı en uç noktaları (Bounding Box) bul
            foreach (var p in parts3D)
            {
                PointF[] vertices = GetProjectedVertices(p.X, p.Y, p.Z, p.W, p.H, p.D);
                foreach (var pt in vertices)
                {
                    if (pt.X < minX) minX = pt.X;
                    if (pt.X > maxX) maxX = pt.X;
                    if (pt.Y < minY) minY = pt.Y;
                    if (pt.Y > maxY) maxY = pt.Y;
                }
            }

            float drawWidth = maxX - minX;
            float drawHeight = maxY - minY;

            // Ekrana sığması için %15 boşluk bırakarak ölçekle
            float scaleX = (this.Width * 0.85f) / drawWidth;
            float scaleY = (this.Height * 0.85f) / drawHeight;
            float scale = Math.Min(scaleX, scaleY);

            // Çizimin merkez noktasını bul
            float cx = minX + (drawWidth / 2f);
            float cy = minY + (drawHeight / 2f);

            // Kamerayı ayarla: Ekranın ortasına git, ölçekle, çizimin merkezini ekrana oturt
            g.TranslateTransform(this.Width / 2f, this.Height / 2f);
            g.ScaleTransform(scale, scale);
            g.TranslateTransform(-cx, -cy);
            // -----------------------------------------------------------------

            // 3. Parçaları Çiz
            foreach (var p3d in parts3D)
            {
                Draw3DBox(g, p3d.X, p3d.Y, p3d.Z, p3d.W, p3d.H, p3d.D, p3d.Part.Type);
            }
        }

        // Yardımcı Metot: Bir parçanın 8 köşesinin izometrik izdüşümünü verir
        private PointF[] GetProjectedVertices(float x, float y, float z, float w, float h, float d)
        {
            return new PointF[]
            {
                Project(x, y, z),
                Project(x + w, y, z),
                Project(x + w, y + h, z),
                Project(x, y + h, z),
                Project(x, y, z + d),
                Project(x + w, y, z + d),
                Project(x + w, y + h, z + d),
                Project(x, y + h, z + d)
            };
        }

        // 2D parçalara Z (Derinlik) uydurduğumuz veri yapısı
        private class Part3D
        {
            public PartEntity Part;
            public float X, Y, Z, W, H, D;
        }

        private Part3D Get3DBounds(PartEntity part, CabinetEntity cab)
        {
            float x = part.FrontViewBounds.X;
            float y = part.FrontViewBounds.Y;
            float w = part.FrontViewBounds.Width;
            float h = part.FrontViewBounds.Height;
            float z = 0;
            float d = (float)cab.TotalDepth; // Varsayılan derinlik

            // Parçanın tipine göre 3 boyutlu uzaydaki yerini (Z) ve kalınlığını (D) belirliyoruz
            switch (part.Type)
            {
                case PartType.Arkalik:
                    z = d - 16; d = 8; // Arkalık en arkada durur
                    break;
                case PartType.Raf:
                    z = 20; d -= 30; // Raf biraz içeridedir
                    break;
                case PartType.Kapak:
                case PartType.Cekmece:
                    z = -(float)part.ThicknessMm; // Kapaklar gövdenin dışına/önüne taşar
                    d = (float)part.ThicknessMm;
                    break;
                case PartType.Ankastre:
                    z = 20; d = d - 40; // Ankastre fırın bloğu
                    break;
                case PartType.Gövde:
                    // Üst kayıtların dikey/yatay olmasına göre derinlik hesabı
                    if (part.Name.Contains("Ön Üst Kayıt"))
                    {
                        z = 0;
                        d = h > part.ThicknessMm ? (float)part.ThicknessMm : (float)cab.TopStripWidth;
                    }
                    else if (part.Name.Contains("Arka Üst Kayıt"))
                    {
                        d = h > part.ThicknessMm ? (float)part.ThicknessMm : (float)cab.TopStripWidth;
                        z = (float)cab.TotalDepth - d; // Arka kayıt en arkadadır
                    }
                    break;
            }

            return new Part3D { Part = part, X = x, Y = y, Z = z, W = w, H = h, D = d };
        }

        // --- İZOMETRİK PROJEKSİYON MATEMATİĞİ (SİHİR BURADA) ---
        private PointF Project(float x, float y, float z)
        {
            double angle = Math.PI / 6; // 30 derecelik izometrik açı
            float isoX = (x - z) * (float)Math.Cos(angle);
            float isoY = y + (x + z) * (float)Math.Sin(angle);
            return new PointF(isoX, isoY);
        }

        private void Draw3DBox(Graphics g, float x, float y, float z, float w, float h, float d, PartType type)
        {
            // Kutunun 8 köşesinin 3D'den 2D'ye izdüşümü
            PointF[] v = new PointF[8];
            v[0] = Project(x, y, z);
            v[1] = Project(x + w, y, z);
            v[2] = Project(x + w, y + h, z);
            v[3] = Project(x, y + h, z);
            v[4] = Project(x, y, z + d);
            v[5] = Project(x + w, y, z + d);
            v[6] = Project(x + w, y + h, z + d);
            v[7] = Project(x, y + h, z + d);

            // Tipine göre ahşap/cam renkleri
            Color baseColor = Color.FromArgb(255, 200, 170, 130); // Ahşap gövde rengi
            if (type == PartType.Kapak || type == PartType.Cekmece) baseColor = Color.FromArgb(180, 100, 200, 255); // Şeffaf cam/mavi
            if (type == PartType.Raf) baseColor = Color.FromArgb(255, 150, 255, 150);
            if (type == PartType.Arkalik) baseColor = Color.FromArgb(255, 220, 200, 180);
            if (type == PartType.Ankastre) baseColor = Color.FromArgb(250, 50, 50, 50); // Siyah cihaz

            // Işıklandırma efekti (Üst aydınlık, ön normal, yan karanlık)
            Brush topBrush = new SolidBrush(ControlPaint.Light(baseColor, 0.5f));
            Brush frontBrush = new SolidBrush(baseColor);
            Brush rightBrush = new SolidBrush(ControlPaint.Dark(baseColor, 0.1f));
            Pen edgePen = new Pen(Color.FromArgb(120, 50, 50, 50), 1f);

            // Üst Yüzey (Top Face)
            g.FillPolygon(topBrush, new[] { v[0], v[1], v[5], v[4] });
            g.DrawPolygon(edgePen, new[] { v[0], v[1], v[5], v[4] });

            // Ön Yüzey (Front Face)
            g.FillPolygon(frontBrush, new[] { v[0], v[1], v[2], v[3] });
            g.DrawPolygon(edgePen, new[] { v[0], v[1], v[2], v[3] });

            // Sağ Yan Yüzey (Right Face)
            g.FillPolygon(rightBrush, new[] { v[1], v[5], v[6], v[2] });
            g.DrawPolygon(edgePen, new[] { v[1], v[5], v[6], v[2] });
        }
    }
}