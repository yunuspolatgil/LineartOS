using Lineart.Core.Document;
using Lineart.Core.Entities;
using Lineart.Core.Geometry;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Lineart.WinForms.Controls
{
    public partial class DrawingCanvas : UserControl
    {
        public DrawingDocument Document { get; set; }

        public CabinetEntity SelectedCabinet { get; private set; }
        public event EventHandler<CabinetEntity> SelectionChanged;

        // --- KAMERA AYARLARI ---
        private float _zoom = 0.4f; // Sahnemiz artık daha geniş
        private PointF _panOffset = new PointF(100, 100);
        private Point _lastMouseLocation;
        private bool _isPanning = false;

        // --- ADIM 1.1: TAŞIMA (DRAG & DROP) DEĞİŞKENLERİ BURAYA GELDİ ---
        private bool _isDraggingCabinet = false;
        private PointF _dragStartMousePos;
        private Point2D _dragStartCabinetPos;

        public DrawingCanvas()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.Cursor = Cursors.Cross;
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            float zoomDelta = e.Delta > 0 ? 1.1f : 0.9f;
            _zoom *= zoomDelta;
            this.Invalidate();
        }

        // --- ADIM 1.2: YENİ ONMOUSEDOWN VE YENİ HITTEST BURADA ---
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Middle)
            {
                _isPanning = true;
                _lastMouseLocation = e.Location;
            }
            else if (e.Button == MouseButtons.Left)
            {
                var clickedCabinet = PerformHitTest(e.Location);

                if (clickedCabinet != null)
                {
                    _isDraggingCabinet = true;
                    _dragStartMousePos = e.Location;
                    _dragStartCabinetPos = clickedCabinet.Position;
                    this.Cursor = Cursors.SizeAll;
                }
            }
        }

        private CabinetEntity PerformHitTest(Point mousePt)
        {
            if (Document == null) return null;

            float mouseX_InModel = (mousePt.X - _panOffset.X) / _zoom;
            float mouseY_InModel = (mousePt.Y - _panOffset.Y) / _zoom;

            CabinetEntity clicked = null;

            for (int i = Document.Cabinets.Count - 1; i >= 0; i--)
            {
                var cab = Document.Cabinets[i];
                RectangleF cabBounds = new RectangleF(
                    (float)cab.Position.X, (float)cab.Position.Y,
                    (float)cab.TotalWidth, (float)cab.TotalHeight);

                if (cabBounds.Contains(mouseX_InModel, mouseY_InModel))
                {
                    clicked = cab;
                    break;
                }
            }

            if (SelectedCabinet != clicked)
            {
                SelectedCabinet = clicked;
                SelectionChanged?.Invoke(this, SelectedCabinet);
                this.Invalidate();
            }

            return clicked;
        }

        // --- ADIM 1.3: YENİ ONMOUSEMOVE BURADA ---
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_isPanning)
            {
                float dx = e.Location.X - _lastMouseLocation.X;
                float dy = e.Location.Y - _lastMouseLocation.Y;
                _panOffset.X += dx;
                _panOffset.Y += dy;
                _lastMouseLocation = e.Location;
                this.Invalidate();
            }
            else if (_isDraggingCabinet && SelectedCabinet != null)
            {
                float deltaX_Pixel = e.Location.X - _dragStartMousePos.X;
                float deltaY_Pixel = e.Location.Y - _dragStartMousePos.Y;

                double deltaX_Mm = deltaX_Pixel / _zoom;
                double deltaY_Mm = deltaY_Pixel / _zoom;

                // Farenin konumuna göre olası YENİ X ve Y değerleri
                double proposedX = _dragStartCabinetPos.X + deltaX_Mm;
                double proposedY = _dragStartCabinetPos.Y + deltaY_Mm;

                // --- MANYETİK YAPIŞMA (SNAP) MOTORU ---
                double snapTolerance = 20.0; // 20 mm yaklaştığında mıknatıs gibi çek

                foreach (var otherCabinet in Document.Cabinets)
                {
                    if (otherCabinet == SelectedCabinet) continue; // Kendini atla

                    // 1. X EKSENİNDE YAPIŞMA (Yan Yana Dizilim)
                    // Benim SOL kenarım, diğerinin SAĞ kenarına yaklaştı mı?
                    double otherRight = otherCabinet.Position.X + otherCabinet.TotalWidth;
                    if (Math.Abs(proposedX - otherRight) < snapTolerance)
                    {
                        proposedX = otherRight; // Tam yapıştır
                    }
                    // Benim SAĞ kenarım, diğerinin SOL kenarına yaklaştı mı?
                    else if (Math.Abs((proposedX + SelectedCabinet.TotalWidth) - otherCabinet.Position.X) < snapTolerance)
                    {
                        proposedX = otherCabinet.Position.X - SelectedCabinet.TotalWidth; // Tam yapıştır
                    }

                    // 2. Y EKSENİNDE YAPIŞMA (Üst/Alt Hizalama)
                    // Benim ÜST kenarım, diğerinin ÜST kenarıyla aynı hizaya geldi mi?
                    if (Math.Abs(proposedY - otherCabinet.Position.Y) < snapTolerance)
                    {
                        proposedY = otherCabinet.Position.Y; // Aynı çizgiye hizala
                    }
                    // Üst dolaplar için: Benim ALT kenarım, diğerinin ÜSTÜNE oturdu mu? vb. kurallar buraya eklenebilir.
                }

                // Hesaplanmış nihai pozisyonu dolaba uygula
                SelectedCabinet.Position = new Point2D(proposedX, proposedY);
                this.Invalidate();
            }
        }

        // --- ADIM 1.4: YENİ ONMOUSEUP BURADA ---
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button == MouseButtons.Middle)
            {
                _isPanning = false;
            }
            else if (e.Button == MouseButtons.Left)
            {
                if (_isDraggingCabinet)
                {
                    _isDraggingCabinet = false;
                    this.Cursor = Cursors.Cross;
                }
            }
        }

        // --- ÇİZİM İŞLEMİ (RENDER) BURASI DAHA ÖNCEKİYLE AYNI ---
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (Document == null) return;

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            g.TranslateTransform(_panOffset.X, _panOffset.Y);
            g.ScaleTransform(_zoom, _zoom);

            using Pen partPen = new Pen(Color.Cyan, 1.5f / _zoom);
            using Brush partBrush = new SolidBrush(Color.FromArgb(50, 0, 255, 255));
            using Pen selectionPen = new Pen(Color.Red, 2.0f / _zoom) { DashStyle = DashStyle.Dash };
            using Pen dimPen = new Pen(Color.Yellow, 1.5f / _zoom);
            using Font dimFont = new Font("Segoe UI", 12f / _zoom, FontStyle.Regular);
            using Brush dimBrush = new SolidBrush(Color.Yellow);

            foreach (var cabinet in Document.Cabinets)
            {
                var state = g.Save();
                g.TranslateTransform((float)cabinet.Position.X, (float)cabinet.Position.Y);

                foreach (var part in cabinet.Parts)
                {
                    RectangleF rect = part.FrontViewBounds;
                    g.FillRectangle(partBrush, rect);
                    g.DrawRectangle(partPen, rect.X, rect.Y, rect.Width, rect.Height);
                }

                // ÖLÇÜLERİ ÇİZDİRİYORUZ
                foreach (var dim in cabinet.Dimensions)
                {
                    DrawDimension(g, dim, dimPen, dimFont, dimBrush);
                }

                if (cabinet == SelectedCabinet)
                {
                    g.DrawRectangle(selectionPen, 0, 0, (float)cabinet.TotalWidth, (float)cabinet.TotalHeight);
                }

                g.Restore(state);
            }
        }

        // DRAW DIMENSION METODU (Daha önce yazdığımız ölçü çizim metodu)
        private void DrawDimension(Graphics g, DimensionEntity dim, Pen pen, Font font, Brush brush)
        {
            float dx = (float)(dim.EndPoint.X - dim.StartPoint.X);
            float dy = (float)(dim.EndPoint.Y - dim.StartPoint.Y);
            double angle = Math.Atan2(dy, dx);
            float offsetX = (float)(-dim.Offset * Math.Sin(angle));
            float offsetY = (float)(dim.Offset * Math.Cos(angle));

            PointF p1 = new PointF((float)dim.StartPoint.X + offsetX, (float)dim.StartPoint.Y + offsetY);
            PointF p2 = new PointF((float)dim.EndPoint.X + offsetX, (float)dim.EndPoint.Y + offsetY);

            using Pen guidePen = new Pen(Color.FromArgb(100, Color.Yellow), pen.Width) { DashStyle = DashStyle.Dash };
            g.DrawLine(guidePen, (float)dim.StartPoint.X, (float)dim.StartPoint.Y, p1.X, p1.Y);
            g.DrawLine(guidePen, (float)dim.EndPoint.X, (float)dim.EndPoint.Y, p2.X, p2.Y);

            g.DrawLine(pen, p1, p2);

            float tickSize = 8f / _zoom;
            g.DrawLine(pen, p1.X - tickSize, p1.Y - tickSize, p1.X + tickSize, p1.Y + tickSize);
            g.DrawLine(pen, p2.X - tickSize, p2.Y - tickSize, p2.X + tickSize, p2.Y + tickSize);

            PointF textPos = new PointF((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);
            StringFormat sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Far
            };

            var state = g.Save();
            g.TranslateTransform(textPos.X, textPos.Y);
            g.RotateTransform((float)(angle * 180 / Math.PI));
            g.DrawString(dim.Text, font, brush, 0, -2f / _zoom, sf);
            g.Restore(state);
        }
    }
}