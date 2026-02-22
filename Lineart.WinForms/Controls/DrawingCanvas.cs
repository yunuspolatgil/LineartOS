using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using Lineart.Core.Document;
using Lineart.Core.Entities;

namespace Lineart.WinForms.Controls
{
    public partial class DrawingCanvas : XtraUserControl
    {
        public DrawingDocument Document { get; set; }

        // Kamera (Görünüm) Ayarları
        private float _zoom = 1.0f;
        private PointF _panOffset = new PointF(0, 0);
        private Point _lastMouseLocation;
        private bool _isPanning = false;

        public DrawingCanvas()
        {
            // Titreşimi (Flicker) engellemek için kritik ayarlar
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();

            this.BackColor = Color.FromArgb(30, 30, 30); // CAD tarzı koyu arka plan
            this.Cursor = Cursors.Cross; // Çizim imleci
        }

        // --- MOUSE İŞLEMLERİ (PAN VE ZOOM) ---
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            // Fare tekerleği ile Zoom in / Zoom out
            float zoomDelta = e.Delta > 0 ? 1.1f : 0.9f;
            _zoom *= zoomDelta;
            this.Invalidate(); // Ekranı yeniden çizmeye zorla
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Middle) // Orta tuşa basılı tutarak kaydırma (Pan)
            {
                _isPanning = true;
                _lastMouseLocation = e.Location;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_isPanning)
            {
                // Farenin ne kadar sürüklendiğini hesapla ve Pan değerine ekle
                float dx = e.Location.X - _lastMouseLocation.X;
                float dy = e.Location.Y - _lastMouseLocation.Y;
                _panOffset.X += dx;
                _panOffset.Y += dy;
                _lastMouseLocation = e.Location;
                this.Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button == MouseButtons.Middle) _isPanning = false;
        }

        // --- ÇİZİM İŞLEMİ (RENDER) ---
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (Document == null) return;

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias; // Çizgileri yumuşat

            // Kamera dönüşümlerini uygula (Matrix Transform)
            g.TranslateTransform(_panOffset.X, _panOffset.Y);
            g.ScaleTransform(_zoom, _zoom);

            // Kalem ve Fırça ayarları
            using Pen partPen = new Pen(Color.Cyan, 1.5f / _zoom); // Zoom'a göre çizgi kalınlığını sabit tut
            using Brush partBrush = new SolidBrush(Color.FromArgb(50, 0, 255, 255)); // Şeffaf dolgu

            // 1. Serbest Parçaları Çiz
            foreach (var part in Document.StandaloneParts)
            {
                DrawPart(g, part, partPen, partBrush);
            }

            // 2. Dolapları Çiz
            foreach (var cabinet in Document.Cabinets)
            {
                // İleride dolapların da kendine ait bir yerel koordinatı (Position) olacak,
                // şimdilik parçaları doğrudan çiziyoruz.
                foreach (var part in cabinet.Parts)
                {
                    DrawPart(g, part, partPen, partBrush);
                }
            }
        }

        private void DrawPart(Graphics g, PartEntity part, Pen pen, Brush brush)
        {
            // Modeldeki mm cinsinden verileri GDI+ Rectangle'a çeviriyoruz
            RectangleF rect = new RectangleF(
                (float)part.Location.X,
                (float)part.Location.Y,
                (float)part.WidthMm,
                (float)part.HeightMm);

            g.FillRectangle(brush, rect);
            g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
        }
    }
}