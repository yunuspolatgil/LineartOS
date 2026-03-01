using System;
using System.Drawing;
using System.Windows.Forms;
using Lineart.Core.Entities;
using Lineart.Core.Document;
using Lineart.Core.Geometry;
using DevExpress.XtraEditors;

namespace Lineart.WinForms.Controls
{
    public partial class QuickMovePad : XtraForm
    {
        private CabinetEntity _cabinet;
        private DrawingDocument _doc;
        private Control _canvasToRefresh;
        private NumericUpDown _numStep;

        public QuickMovePad(CabinetEntity cabinet, DrawingDocument doc, Control canvasToRefresh)
        {
            _cabinet = cabinet;
            _doc = doc;
            _canvasToRefresh = canvasToRefresh;

            this.Text = "Hassas Konum";
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.StartPosition = FormStartPosition.Manual;
            this.Size = new Size(160, 180);
            this.TopMost = true;
            this.BackColor = Color.FromArgb(45, 45, 50);
            this.ForeColor = Color.White;

            Label lbl = new Label { Text = "Adım (mm):", ForeColor = Color.LightGray, AutoSize = true, Location = new Point(10, 15) };
            this.Controls.Add(lbl);

            _numStep = new NumericUpDown { Location = new Point(80, 12), Size = new Size(60, 20), Minimum = 1, Maximum = 1000, Value = 10, BackColor = Color.FromArgb(30, 30, 30), ForeColor = Color.White };
            this.Controls.Add(_numStep);

            // Yön Butonları
            Button btnUp = CreateButton("▲", 60, 50);
            Button btnDown = CreateButton("▼", 60, 110);
            Button btnLeft = CreateButton("◄", 20, 80);
            Button btnRight = CreateButton("►", 100, 80);

            btnUp.Click += (s, e) => MoveCabinet(0, -1);
            btnDown.Click += (s, e) => MoveCabinet(0, 1);
            btnLeft.Click += (s, e) => MoveCabinet(-1, 0);
            btnRight.Click += (s, e) => MoveCabinet(1, 0);

            Button btnClose = new Button { Text = "X", Size = new Size(20, 20), Location = new Point(135, 2), FlatStyle = FlatStyle.Flat, ForeColor = Color.Gray };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => this.Close();
            this.Controls.Add(btnClose);
        }

        private Button CreateButton(string text, int x, int y)
        {
            Button btn = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(40, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(60, 60, 70),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            this.Controls.Add(btn);
            return btn;
        }

        private void MoveCabinet(int dirX, int dirY)
        {
            double step = (double)_numStep.Value;
            double newX = _cabinet.Position.X + (dirX * step);
            double newY = _cabinet.Position.Y + (dirY * step);

            // Duvar Sınırları Kontrolü
            if (newX < 0) newX = 0;
            if (newX + _cabinet.TotalWidth > _doc.RoomWidth) newX = _doc.RoomWidth - _cabinet.TotalWidth;
            if (newY < 0) newY = 0;
            if (newY + _cabinet.OverallHeight > _doc.RoomHeight) newY = _doc.RoomHeight - _cabinet.OverallHeight;

            _cabinet.Position = new Point2D(newX, newY);
            _canvasToRefresh.Invalidate();
        }
    }
}