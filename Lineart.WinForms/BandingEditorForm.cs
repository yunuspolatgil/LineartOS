using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Lineart.Core.Settings;

namespace Lineart.WinForms
{
    public partial class BandingEditorForm : Form
    {
        private CabinetBandingScheme _scheme;
        private string _activePart = "Yan Dikme";
        private PartBanding _currentBanding;

        // UI Elemanları
        private DoubleBufferedPanel _canvas; // HATA ÇÖZÜMÜ: Standart Panel yerine kendi panelimizi kullanıyoruz
        private Button[] _tabButtons;

        public BandingEditorForm(CabinetBandingScheme scheme, string title)
        {
            _scheme = scheme;
            _currentBanding = _scheme.SidePanel;

            this.Text = title;
            this.Size = new Size(500, 550);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(15, 23, 42);
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;

            InitializeCustomUI();
        }

        private void InitializeCustomUI()
        {
            Label lblTitle = new Label { Text = "PVC BANT YÖNETİCİSİ", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.White, Dock = DockStyle.Top, Height = 50, TextAlign = ContentAlignment.MiddleCenter };
            this.Controls.Add(lblTitle);

            // Sekmeler
            FlowLayoutPanel pnlTabs = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 40, BackColor = Color.FromArgb(30, 41, 59) };
            string[] tabs = { "Yan Dikme", "Üst Tabla", "Alt Tabla", "Raf", "Kapak" };
            _tabButtons = new Button[tabs.Length];

            for (int i = 0; i < tabs.Length; i++)
            {
                Button btn = new Button
                {
                    Text = tabs[i],
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.Gray,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    Size = new Size(95, 35),
                    Tag = tabs[i]
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.Click += Tab_Click;
                pnlTabs.Controls.Add(btn);
                _tabButtons[i] = btn;
            }
            this.Controls.Add(pnlTabs);

            // Çizim Alanı (Görsel Kutu) - HATA BURADA DÜZELTİLDİ
            _canvas = new DoubleBufferedPanel { Dock = DockStyle.Fill };
            _canvas.Paint += Canvas_Paint;
            _canvas.MouseClick += Canvas_MouseClick;
            this.Controls.Add(_canvas);

            // Kaydet Butonu
            Button btnSave = new Button { Text = "AYARLARI KAYDET", Dock = DockStyle.Bottom, Height = 50, BackColor = Color.FromArgb(37, 99, 235), ForeColor = Color.White, Font = new Font("Segoe UI", 12, FontStyle.Bold), FlatStyle = FlatStyle.Flat };
            btnSave.Click += (s, e) => { this.DialogResult = DialogResult.OK; };
            this.Controls.Add(btnSave);

            UpdateButtonStyles();
        }

        private void Tab_Click(object sender, EventArgs e)
        {
            _activePart = ((Button)sender).Tag.ToString();

            switch (_activePart)
            {
                case "Yan Dikme": _currentBanding = _scheme.SidePanel; break;
                case "Üst Tabla": _currentBanding = _scheme.TopPanel; break;
                case "Alt Tabla": _currentBanding = _scheme.BottomPanel; break;
                case "Raf": _currentBanding = _scheme.Shelf; break;
                case "Kapak": _currentBanding = _scheme.Door; break;
            }
            UpdateButtonStyles();
            _canvas.Invalidate();
        }

        private void UpdateButtonStyles()
        {
            foreach (var btn in _tabButtons)
            {
                if (btn.Tag.ToString() == _activePart) { btn.ForeColor = Color.White; btn.BackColor = Color.FromArgb(51, 65, 85); }
                else { btn.ForeColor = Color.Gray; btn.BackColor = Color.Transparent; }
            }
        }

        // --- GÖRSEL ÇİZİM MOTORU ---
        private void Canvas_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int w = 250; int h = 250;
            int x = (_canvas.Width - w) / 2;
            int y = (_canvas.Height - h) / 2;
            int edgeSize = 30;

            Rectangle rectPart = new Rectangle(x + edgeSize, y + edgeSize, w - 2 * edgeSize, h - 2 * edgeSize);
            using (SolidBrush b = new SolidBrush(Color.FromArgb(30, 41, 59))) g.FillRectangle(b, rectPart);

            TextRenderer.DrawText(g, _activePart.ToUpper(), new Font("Segoe UI", 12, FontStyle.Bold), rectPart, Color.FromArgb(71, 85, 105), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

            DrawEdge(g, x, y, w, edgeSize, _currentBanding.Top, "Üst", DockStyle.Top);
            DrawEdge(g, x, y + h - edgeSize, w, edgeSize, _currentBanding.Bottom, "Alt", DockStyle.Bottom);
            DrawEdge(g, x, y + edgeSize, edgeSize, h - 2 * edgeSize, _currentBanding.Left, "Sol", DockStyle.Left);
            DrawEdge(g, x + w - edgeSize, y + edgeSize, edgeSize, h - 2 * edgeSize, _currentBanding.Right, "Sağ", DockStyle.Right);

            int ly = y + h + 20;
            DrawLegend(g, x, ly, Color.FromArgb(71, 85, 105), "Yok");
            DrawLegend(g, x + 60, ly, Color.FromArgb(22, 163, 74), "0.40");
            DrawLegend(g, x + 120, ly, Color.FromArgb(37, 99, 235), "0.80");
            DrawLegend(g, x + 180, ly, Color.FromArgb(202, 138, 4), "2mm");
        }

        private void DrawEdge(Graphics g, int x, int y, int w, int h, double val, string label, DockStyle pos)
        {
            Color c = GetBandColor(val);
            using (SolidBrush b = new SolidBrush(c)) g.FillRectangle(b, x, y, w, h);
            using (Pen p = new Pen(Color.FromArgb(15, 23, 42), 2)) g.DrawRectangle(p, x, y, w, h);

            string text = $"{label}: {(val == 0 ? "Yok" : val.ToString())}";
            if (pos == DockStyle.Left)
            {
                g.TranslateTransform(x + h / 2, y + w / 2); g.RotateTransform(-90);
                TextRenderer.DrawText(g, text, new Font("Segoe UI", 9, FontStyle.Bold), new Point(0, 0), Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                g.ResetTransform();
            }
            else if (pos == DockStyle.Right)
            {
                g.TranslateTransform(x + h / 2, y + w / 2); g.RotateTransform(90);
                TextRenderer.DrawText(g, text, new Font("Segoe UI", 9, FontStyle.Bold), new Point(0, 0), Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                g.ResetTransform();
            }
            else
            {
                TextRenderer.DrawText(g, text, new Font("Segoe UI", 9, FontStyle.Bold), new Rectangle(x, y, w, h), Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }

        private void DrawLegend(Graphics g, int x, int y, Color c, string text)
        {
            using (SolidBrush b = new SolidBrush(c)) g.FillRectangle(b, x, y, 15, 15);
            TextRenderer.DrawText(g, text, new Font("Segoe UI", 8), new Point(x + 20, y + 1), Color.Gray);
        }

        private void Canvas_MouseClick(object sender, MouseEventArgs e)
        {
            int w = 250; int h = 250;
            int x = (_canvas.Width - w) / 2;
            int y = (_canvas.Height - h) / 2;
            int edgeSize = 30;

            if (new Rectangle(x, y, w, edgeSize).Contains(e.Location)) _currentBanding.Top = CycleValue(_currentBanding.Top);
            else if (new Rectangle(x, y + h - edgeSize, w, edgeSize).Contains(e.Location)) _currentBanding.Bottom = CycleValue(_currentBanding.Bottom);
            else if (new Rectangle(x, y + edgeSize, edgeSize, h - 2 * edgeSize).Contains(e.Location)) _currentBanding.Left = CycleValue(_currentBanding.Left);
            else if (new Rectangle(x + w - edgeSize, y + edgeSize, edgeSize, h - 2 * edgeSize).Contains(e.Location)) _currentBanding.Right = CycleValue(_currentBanding.Right);

            _canvas.Invalidate();
        }

        private double CycleValue(double current)
        {
            if (current == 0) return 0.4;
            if (current == 0.4) return 0.8;
            if (current == 0.8) return 2.0;
            return 0;
        }

        private Color GetBandColor(double val)
        {
            if (val == 0) return Color.FromArgb(71, 85, 105);
            if (val == 0.4) return Color.FromArgb(22, 163, 74);
            if (val == 0.8) return Color.FromArgb(37, 99, 235);
            if (val == 2.0) return Color.FromArgb(202, 138, 4);
            return Color.Gray;
        }
    }

    // --- EKRAN TİTREMESİNİ (FLICKER) ÖNLEYEN ÖZEL PANEL SINIFI ---
    public class DoubleBufferedPanel : Panel
    {
        public DoubleBufferedPanel()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();
        }
    }
}