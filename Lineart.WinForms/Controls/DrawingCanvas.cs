using Lineart.Core.Document;
using Lineart.Core.Entities;
using Lineart.Core.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace Lineart.WinForms.Controls
{
    public partial class DrawingCanvas : UserControl
    {
        public DrawingDocument Document { get; set; }

        public CabinetEntity SelectedCabinet { get; private set; }
        public event EventHandler<CabinetEntity> SelectionChanged;
        public event EventHandler<CabinetEntity> CabinetDoubleClicked;

        // --- YENİ EKLENEN: MEZURA (ÖLÇÜM) SİSTEMİ ---
        public bool IsMeasuringMode { get; set; } = false;
        private bool _isDrawingMeasurement = false;
        private PointF _measureStart;
        private PointF _measureEnd;
        public List<Tuple<PointF, PointF>> UserMeasurements { get; } = new List<Tuple<PointF, PointF>>();

        public void ClearMeasurements()
        {
            UserMeasurements.Clear();
            this.Invalidate();
        }

        public void SetSelectedCabinet(CabinetEntity cabinet)
        {
            if (SelectedCabinet != cabinet)
            {
                SelectedCabinet = cabinet;
                SelectionChanged?.Invoke(this, SelectedCabinet);
                this.Invalidate();
            }
        }

        private float _zoom = 0.4f;
        private PointF _panOffset = new PointF(100, 100);
        private Point _lastMouseLocation;
        private bool _isPanning = false;

        private bool _isDraggingCabinet = false;
        private PointF _dragStartMousePos;
        private Point2D _dragStartCabinetPos;

        public DrawingCanvas()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();
            this.BackColor = Color.Transparent;
            this.Cursor = Cursors.Cross;
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            float zoomDelta = e.Delta > 0 ? 1.1f : 0.9f;
            _zoom *= zoomDelta;
            this.Invalidate();
        }

        // AKILLI MIKNATIS (SNAP) FONKSİYONU: Farenin dolap kenarlarına yapışmasını sağlar
        private PointF GetSnappedPoint(PointF mousePosInModel)
        {
            float snapDist = 20f / _zoom;
            PointF snapped = mousePosInModel;

            if (Document == null) return snapped;

            // 1. Duvarlara Yapışma
            if (Math.Abs(snapped.X - 0) < snapDist) snapped.X = 0;
            if (Math.Abs(snapped.X - Document.RoomWidth) < snapDist) snapped.X = (float)Document.RoomWidth;
            if (Math.Abs(snapped.Y - 0) < snapDist) snapped.Y = 0;
            if (Math.Abs(snapped.Y - Document.RoomHeight) < snapDist) snapped.Y = (float)Document.RoomHeight;

            // 2. Dolap Kenarlarına Yapışma
            foreach (var cab in Document.Cabinets)
            {
                float cL = (float)cab.Position.X;
                float cR = (float)(cab.Position.X + cab.TotalWidth);
                float cT = (float)cab.Position.Y;
                float cB = (float)(cab.Position.Y + cab.OverallHeight);

                if (Math.Abs(snapped.X - cL) < snapDist) snapped.X = cL;
                if (Math.Abs(snapped.X - cR) < snapDist) snapped.X = cR;
                if (Math.Abs(snapped.Y - cT) < snapDist) snapped.Y = cT;
                if (Math.Abs(snapped.Y - cB) < snapDist) snapped.Y = cB;
            }
            return snapped;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            float mouseX_InModel = (e.Location.X - _panOffset.X) / _zoom;
            float mouseY_InModel = (e.Location.Y - _panOffset.Y) / _zoom;
            PointF modelPos = new PointF(mouseX_InModel, mouseY_InModel);

            if (e.Button == MouseButtons.Middle)
            {
                _isPanning = true;
                _lastMouseLocation = e.Location;
            }
            else if (e.Button == MouseButtons.Left)
            {
                // MEZURA MODUNDAYSAK DOLAP SEÇME, ÖLÇÜM ÇİZ
                if (IsMeasuringMode)
                {
                    _isDrawingMeasurement = true;
                    _measureStart = GetSnappedPoint(modelPos);
                    _measureEnd = _measureStart;
                }
                else
                {
                    var clickedCabinet = PerformHitTest(e.Location);

                    // --- ÇÖZÜM BURADA: Tıklanan dolabı seçili hale getir ve Sol Panele haber ver! ---
                    SetSelectedCabinet(clickedCabinet);

                    if (clickedCabinet != null)
                    {
                        _isDraggingCabinet = true;
                        _dragStartMousePos = e.Location;
                        _dragStartCabinetPos = clickedCabinet.Position;
                        this.Cursor = Cursors.SizeAll;
                    }
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            float mouseX_InModel = (e.Location.X - _panOffset.X) / _zoom;
            float mouseY_InModel = (e.Location.Y - _panOffset.Y) / _zoom;
            PointF modelPos = new PointF(mouseX_InModel, mouseY_InModel);

            // YENİ: MEZURA İLE ÖLÇÜM ÇİZİLİYORSA
            if (_isDrawingMeasurement)
            {
                _measureEnd = GetSnappedPoint(modelPos);

                // Shift tuşuna basılıysa çizgiyi dümdüz (Ortogonal) yap
                if (Control.ModifierKeys == Keys.Shift)
                {
                    if (Math.Abs(_measureEnd.X - _measureStart.X) > Math.Abs(_measureEnd.Y - _measureStart.Y))
                        _measureEnd.Y = _measureStart.Y; // Yatay kilitle
                    else
                        _measureEnd.X = _measureStart.X; // Dikey kilitle
                }

                this.Invalidate();
            }
            else if (_isPanning)
            {
                float dx = e.Location.X - _lastMouseLocation.X;
                float dy = e.Location.Y - _lastMouseLocation.Y;
                _panOffset.X += dx;
                _panOffset.Y += dy;
                _lastMouseLocation = e.Location;
                this.Invalidate();
            }
            else if (_isDraggingCabinet && SelectedCabinet != null && !IsMeasuringMode)
            {
                float deltaX_Pixel = e.Location.X - _dragStartMousePos.X;
                float deltaY_Pixel = e.Location.Y - _dragStartMousePos.Y;
                double proposedX = _dragStartCabinetPos.X + (deltaX_Pixel / _zoom);
                double proposedY = _dragStartCabinetPos.Y + (deltaY_Pixel / _zoom);
                double snapTolerance = 20.0;

                double myBottom = proposedY + SelectedCabinet.OverallHeight;
                if (Math.Abs(myBottom - Document.RoomHeight) < snapTolerance) proposedY = Document.RoomHeight - SelectedCabinet.OverallHeight;
                if (Math.Abs(proposedX - 0) < snapTolerance) proposedX = 0;

                foreach (var otherCabinet in Document.Cabinets)
                {
                    if (otherCabinet == SelectedCabinet) continue;
                    double otherRight = otherCabinet.Position.X + otherCabinet.TotalWidth;
                    if (Math.Abs(proposedX - otherRight) < snapTolerance) proposedX = otherRight;
                    else if (Math.Abs((proposedX + SelectedCabinet.TotalWidth) - otherCabinet.Position.X) < snapTolerance) proposedX = otherCabinet.Position.X - SelectedCabinet.TotalWidth;

                    double myTop = proposedY;
                    myBottom = proposedY + SelectedCabinet.OverallHeight;
                    double otherTop = otherCabinet.Position.Y;
                    double otherBottom = otherCabinet.Position.Y + otherCabinet.OverallHeight;

                    if (Math.Abs(myBottom - otherBottom) < snapTolerance) proposedY = otherBottom - SelectedCabinet.OverallHeight;
                    else if (Math.Abs(myTop - otherTop) < snapTolerance) proposedY = otherTop;
                    else if (Math.Abs(myBottom - otherTop) < snapTolerance) proposedY = otherTop - SelectedCabinet.OverallHeight;
                    else if (Math.Abs(myTop - otherBottom) < snapTolerance) proposedY = otherBottom;
                }

                if (proposedX < 0) proposedX = 0;
                if (proposedX + SelectedCabinet.TotalWidth > Document.RoomWidth) proposedX = Document.RoomWidth - SelectedCabinet.TotalWidth;
                if (proposedY < 0) proposedY = 0;
                if (proposedY + SelectedCabinet.OverallHeight > Document.RoomHeight) proposedY = Document.RoomHeight - SelectedCabinet.OverallHeight;

                SelectedCabinet.Position = new Point2D(proposedX, proposedY);
                this.Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button == MouseButtons.Middle) _isPanning = false;
            else if (e.Button == MouseButtons.Left)
            {
                // YENİ: ÖLÇÜMÜ BİTİR VE LİSTEYE EKLE
                if (_isDrawingMeasurement)
                {
                    _isDrawingMeasurement = false;
                    double dist = Math.Sqrt(Math.Pow(_measureEnd.X - _measureStart.X, 2) + Math.Pow(_measureEnd.Y - _measureStart.Y, 2));
                    if (dist > 1) UserMeasurements.Add(new Tuple<PointF, PointF>(_measureStart, _measureEnd));
                    this.Invalidate();
                }
                else if (_isDraggingCabinet)
                {
                    _isDraggingCabinet = false;
                    this.Cursor = Cursors.Cross;
                }
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            if (!IsMeasuringMode && e.Button == MouseButtons.Left)
            {
                var clickedCabinet = PerformHitTest(e.Location);
                if (clickedCabinet != null) CabinetDoubleClicked?.Invoke(this, clickedCabinet);
            }
        }

        private CabinetEntity PerformHitTest(Point mousePt)
        {
            if (Document == null) return null;
            float mouseX_InModel = (mousePt.X - _panOffset.X) / _zoom;
            float mouseY_InModel = (mousePt.Y - _panOffset.Y) / _zoom;

            for (int i = Document.Cabinets.Count - 1; i >= 0; i--)
            {
                var cab = Document.Cabinets[i];
                RectangleF cabBounds = new RectangleF((float)cab.Position.X, (float)cab.Position.Y, (float)cab.TotalWidth, (float)cab.OverallHeight);
                if (cabBounds.Contains(mouseX_InModel, mouseY_InModel)) return cab;
            }
            return null;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (Document == null) return;

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TranslateTransform(_panOffset.X, _panOffset.Y);
            g.ScaleTransform(_zoom, _zoom);

            float roomW = (float)Document.RoomWidth;
            float roomH = (float)Document.RoomHeight;
            Color contrastColor = this.Parent?.ForeColor ?? Color.Gray;
            Color bgColor = this.BackColor == Color.Transparent ? (this.Parent?.BackColor ?? Color.White) : this.BackColor;

            using (Pen wallPen = new Pen(contrastColor, 2f / _zoom))
                g.DrawRectangle(wallPen, 0, 0, roomW, roomH);

            // ==========================================
            // YENİ: DUVAR DÖNÜŞ (KÖŞE) ÇİZGİLERİNİ ÇİZ (AÇILIM MANTIĞI)
            // ==========================================
            if (Document.Room != null)
            {
                using (Pen foldPen = new Pen(Color.FromArgb(100, Color.Gray), 3f / _zoom) { DashStyle = DashStyle.Dash })
                using (Font foldFont = new Font("Segoe UI", 12f / _zoom, FontStyle.Italic | FontStyle.Bold))
                using (Brush foldBrush = new SolidBrush(Color.Gray))
                {
                    // Eğer L veya U Duvar ise, Sol Duvarın bittiği yer 1. Köşedir.
                    if (Document.Room.Shape == RoomShape.L_Duvar || Document.Room.Shape == RoomShape.U_Duvar)
                    {
                        float fold1 = (float)Document.Room.WallLeft;
                        g.DrawLine(foldPen, fold1, 0, fold1, roomH);
                        DrawTextWithBackground(g, "◀ 90° KÖŞE ▶", foldFont, foldBrush, bgColor, fold1, 40 / _zoom);
                    }

                    // Eğer U Duvar ise, Ana Duvarın bittiği yer 2. Köşedir.
                    if (Document.Room.Shape == RoomShape.U_Duvar)
                    {
                        float fold2 = (float)(Document.Room.WallLeft + Document.Room.WallMain);
                        g.DrawLine(foldPen, fold2, 0, fold2, roomH);
                        DrawTextWithBackground(g, "◀ 90° KÖŞE ▶", foldFont, foldBrush, bgColor, fold2, 40 / _zoom);
                    }
                }
            }

            using Pen partPen = new Pen(contrastColor == Color.White ? Color.Cyan : Color.Blue, 1.5f / _zoom);
            using Brush partBrush = new SolidBrush(Color.FromArgb(40, 0, 150, 255));
            using Pen selectionPen = new Pen(Color.Red, 2.0f / _zoom) { DashStyle = DashStyle.Dash };
            using Pen dimPen = new Pen(Color.Orange, 1.5f / _zoom);
            using Font dimFont = new Font("Segoe UI", 12f / _zoom, FontStyle.Regular);
            using Brush dimBrush = new SolidBrush(Color.Orange);

            foreach (var cabinet in Document.Cabinets)
            {
                var state = g.Save();
                g.TranslateTransform((float)cabinet.Position.X, (float)cabinet.Position.Y);
                foreach (var part in cabinet.Parts) DrawPart(g, part, partPen, partBrush);
                foreach (var dim in cabinet.Dimensions) DrawDimension(g, dim, dimPen, dimFont, dimBrush, bgColor);
                if (cabinet == SelectedCabinet && !IsMeasuringMode) g.DrawRectangle(selectionPen, 0, 0, (float)cabinet.TotalWidth, (float)cabinet.OverallHeight);
                g.Restore(state);
            }

            var sortedCabinets = Document.Cabinets.OrderBy(c => c.Position.X).ToList();
            using (Font gapFont = new Font("Segoe UI", 12f / _zoom, FontStyle.Bold))
            using (Brush gapBrush = new SolidBrush(Color.OrangeRed))
            using (Pen gapPen = new Pen(Color.OrangeRed, 1.5f / _zoom) { CustomEndCap = new AdjustableArrowCap(4, 4), CustomStartCap = new AdjustableArrowCap(4, 4) })
            {
                double currentX = 0;
                foreach (var cab in sortedCabinets)
                {
                    double gap = cab.Position.X - currentX;
                    if (gap > 1)
                    {
                        float midX = (float)(currentX + gap / 2);
                        float lineY = roomH - 50;
                        g.DrawLine(gapPen, (float)currentX, lineY, (float)cab.Position.X, lineY);
                        DrawTextWithBackground(g, $"Boşluk: {Math.Round(gap)} mm", gapFont, gapBrush, bgColor, midX, lineY - (25 / _zoom));
                    }
                    currentX = cab.Position.X + cab.TotalWidth;
                }

                double finalGap = Document.RoomWidth - currentX;
                if (finalGap > 1)
                {
                    float midX = (float)(currentX + finalGap / 2);
                    float lineY = roomH - 50;
                    g.DrawLine(gapPen, (float)currentX, lineY, roomW, lineY);
                    DrawTextWithBackground(g, $"Boşluk: {Math.Round(finalGap)} mm", gapFont, gapBrush, bgColor, midX, lineY - (25 / _zoom));
                }
            }

            if (SelectedCabinet != null && !IsMeasuringMode)
            {
                using (Pen posPen = new Pen(Color.MediumSpringGreen, 1.5f / _zoom) { DashStyle = DashStyle.DashDot, CustomEndCap = new AdjustableArrowCap(4, 4) })
                using (Font posFont = new Font("Segoe UI", 13f / _zoom, FontStyle.Bold))
                using (Brush posBrush = new SolidBrush(Color.MediumSpringGreen))
                {
                    float cabLeft = (float)SelectedCabinet.Position.X;
                    float cabRight = (float)(SelectedCabinet.Position.X + SelectedCabinet.TotalWidth);
                    float cabTop = (float)SelectedCabinet.Position.Y;
                    float cabBottom = (float)(SelectedCabinet.Position.Y + SelectedCabinet.OverallHeight);
                    float midX = cabLeft + (cabRight - cabLeft) / 2f;
                    float midY = cabTop + (cabBottom - cabTop) / 2f;

                    if (cabLeft > 0) { g.DrawLine(posPen, 0, midY, cabLeft, midY); DrawTextWithBackground(g, $"Soldan: {Math.Round(cabLeft)}", posFont, posBrush, bgColor, cabLeft / 2f, midY); }
                    if (cabRight < roomW) { g.DrawLine(posPen, roomW, midY, cabRight, midY); DrawTextWithBackground(g, $"Sağdan: {Math.Round(roomW - cabRight)}", posFont, posBrush, bgColor, cabRight + (roomW - cabRight) / 2f, midY); }
                    if (cabTop > 0) { g.DrawLine(posPen, midX, 0, midX, cabTop); DrawTextWithBackground(g, $"Tavandan: {Math.Round(cabTop)}", posFont, posBrush, bgColor, midX, cabTop / 2f); }
                    if (cabBottom < roomH) { g.DrawLine(posPen, midX, roomH, midX, cabBottom); DrawTextWithBackground(g, $"Yerden: {Math.Round(roomH - cabBottom)}", posFont, posBrush, bgColor, midX, cabBottom + (roomH - cabBottom) / 2f); }
                }
            }

            // --- YENİ EKLENEN: KULLANICI ÖLÇÜMLERİNİ ÇİZME (MEZURA) ---
            using (Pen userDimPen = new Pen(Color.Magenta, 2.5f / _zoom) { CustomEndCap = new AdjustableArrowCap(4, 4), CustomStartCap = new AdjustableArrowCap(4, 4) })
            using (Font userDimFont = new Font("Segoe UI", 14f / _zoom, FontStyle.Bold))
            using (Brush userDimBrush = new SolidBrush(Color.Magenta))
            {
                foreach (var m in UserMeasurements)
                {
                    g.DrawLine(userDimPen, m.Item1, m.Item2);
                    double dist = Math.Sqrt(Math.Pow(m.Item2.X - m.Item1.X, 2) + Math.Pow(m.Item2.Y - m.Item1.Y, 2));
                    DrawTextWithBackground(g, $"{Math.Round(dist)} mm", userDimFont, userDimBrush, bgColor, (m.Item1.X + m.Item2.X) / 2f, (m.Item1.Y + m.Item2.Y) / 2f);
                }

                if (_isDrawingMeasurement)
                {
                    g.DrawLine(userDimPen, _measureStart, _measureEnd);
                    double dist = Math.Sqrt(Math.Pow(_measureEnd.X - _measureStart.X, 2) + Math.Pow(_measureEnd.Y - _measureStart.Y, 2));
                    DrawTextWithBackground(g, $"{Math.Round(dist)} mm", userDimFont, userDimBrush, bgColor, (_measureStart.X + _measureEnd.X) / 2f, (_measureStart.Y + _measureEnd.Y) / 2f);
                }
            }
        }

        private void DrawTextWithBackground(Graphics g, string text, Font font, Brush textBrush, Color bgColor, float x, float y)
        {
            SizeF size = g.MeasureString(text, font);
            RectangleF bgRect = new RectangleF(x - size.Width / 2f, y - size.Height / 2f, size.Width, size.Height);
            using (Brush bgBrush = new SolidBrush(Color.FromArgb(220, bgColor))) g.FillRectangle(bgBrush, bgRect);
            StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString(text, font, textBrush, x, y, sf);
        }

        private void DrawPart(Graphics g, PartEntity part, Pen pen, Brush brush)
        {
            RectangleF rect = part.FrontViewBounds;
            switch (part.Type)
            {
                case PartType.Gövde:
                case PartType.Arkalik:
                    g.FillRectangle(brush, rect); g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height); break;
                case PartType.Raf:
                    using (Brush rafBrush = new SolidBrush(Color.FromArgb(150, 0, 255, 0))) { g.FillRectangle(rafBrush, rect); g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height); }
                    break;
                case PartType.Baza:
                    using (Brush bazaBrush = new HatchBrush(HatchStyle.Horizontal, Color.Gray, Color.Transparent)) { g.FillRectangle(bazaBrush, rect); g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height); }
                    break;
                case PartType.Tac:
                    using (Brush tacBrush = new SolidBrush(Color.FromArgb(100, 255, 150, 0))) { g.FillRectangle(tacBrush, rect); g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height); }
                    break;
                case PartType.Ankastre:
                    using (Brush ankBrush = new SolidBrush(Color.FromArgb(150, 50, 50, 50))) { g.FillRectangle(ankBrush, rect); using (Font f = new Font("Arial", 16f / _zoom, FontStyle.Bold)) using (Brush tb = new SolidBrush(Color.White)) g.DrawString("ANKASTRE", f, tb, rect, new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center }); }
                    break;
                case PartType.Cekmece:
                case PartType.Kapak:
                    using (Brush glassBrush = new SolidBrush(Color.FromArgb(40, 150, 200, 255))) g.FillRectangle(glassBrush, rect);
                    using (Pen frontPen = new Pen(Color.FromArgb(180, 100, 150, 255), 1.2f / _zoom)) g.DrawRectangle(frontPen, rect.X, rect.Y, rect.Width, rect.Height);
                    using (Pen dashPen = new Pen(Color.FromArgb(100, 150, 150, 150), 1.0f / _zoom) { DashStyle = DashStyle.Dash })
                    {
                        if (part.Type == PartType.Cekmece) { g.DrawLine(dashPen, rect.Left, rect.Top, rect.Right, rect.Bottom); g.DrawLine(dashPen, rect.Right, rect.Top, rect.Left, rect.Bottom); }
                        else { float midY = rect.Top + (rect.Height / 2); g.DrawLine(dashPen, rect.Right, rect.Top, rect.Left, midY); g.DrawLine(dashPen, rect.Right, rect.Bottom, rect.Left, midY); }
                    }
                    break;
            }
        }

        private void DrawDimension(Graphics g, DimensionEntity dim, Pen pen, Font font, Brush brush, Color bgColor)
        {
            float dx = (float)(dim.EndPoint.X - dim.StartPoint.X); float dy = (float)(dim.EndPoint.Y - dim.StartPoint.Y);
            double angle = Math.Atan2(dy, dx);
            float offsetX = (float)(-dim.Offset * Math.Sin(angle)); float offsetY = (float)(dim.Offset * Math.Cos(angle));
            PointF p1 = new PointF((float)dim.StartPoint.X + offsetX, (float)dim.StartPoint.Y + offsetY);
            PointF p2 = new PointF((float)dim.EndPoint.X + offsetX, (float)dim.EndPoint.Y + offsetY);

            using Pen guidePen = new Pen(Color.FromArgb(100, Color.Gray), pen.Width) { DashStyle = DashStyle.Dash };
            g.DrawLine(guidePen, (float)dim.StartPoint.X, (float)dim.StartPoint.Y, p1.X, p1.Y);
            g.DrawLine(guidePen, (float)dim.EndPoint.X, (float)dim.EndPoint.Y, p2.X, p2.Y);
            g.DrawLine(pen, p1, p2);

            float tickSize = 8f / _zoom;
            g.DrawLine(pen, p1.X - tickSize, p1.Y - tickSize, p1.X + tickSize, p1.Y + tickSize);
            g.DrawLine(pen, p2.X - tickSize, p2.Y - tickSize, p2.X + tickSize, p2.Y + tickSize);
            PointF textPos = new PointF((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);
            DrawTextWithBackground(g, dim.Text, font, brush, bgColor, textPos.X, textPos.Y);
        }
    }
}