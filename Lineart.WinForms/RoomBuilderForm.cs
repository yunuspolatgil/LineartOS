using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using Lineart.Core.Entities;

namespace Lineart.WinForms
{
    public partial class RoomBuilderForm : XtraForm
    {
        public RoomSettings RoomData { get; private set; }

        private Panel _pnlPreview;
        private TextBox _txtMain, _txtLeft, _txtRight, _txtHeight, _txtThickness;
        private Button _btnTek, _btnL, _btnU;

        public RoomBuilderForm(RoomSettings currentRoom)
        {
            RoomData = currentRoom ?? new RoomSettings();

            this.Text = "Oda ve Duvar Şekilleri";
            this.Size = new Size(800, 550);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 240, 240);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            InitializeUI();
            UpdateUIFromData();
        }

        private void InitializeUI()
        {
            // Sağ Panel (Şekil Seçimi)
            Panel pnlRight = new Panel { Dock = DockStyle.Right, Width = 150, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            this.Controls.Add(pnlRight);

            Label lblShapes = new Label { Text = "Duvar Şekilleri", Dock = DockStyle.Top, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 10, FontStyle.Bold), Height = 40 };
            pnlRight.Controls.Add(lblShapes);

            _btnTek = CreateShapeButton("Tek Duvar", 50, RoomShape.TekDuvar);
            _btnL = CreateShapeButton("L Duvar", 110, RoomShape.L_Duvar);
            _btnU = CreateShapeButton("U Duvar", 170, RoomShape.U_Duvar);
            pnlRight.Controls.Add(_btnTek); pnlRight.Controls.Add(_btnL); pnlRight.Controls.Add(_btnU);

            // Alt Panel (Yükseklik, Kalınlık ve Kaydet)
            Panel pnlBottom = new Panel { Dock = DockStyle.Bottom, Height = 60, BackColor = Color.White };
            this.Controls.Add(pnlBottom);

            pnlBottom.Controls.Add(new Label { Text = "Yükseklik (h):", Location = new Point(20, 20), AutoSize = true });
            _txtHeight = new TextBox { Location = new Point(100, 17), Width = 60, Text = RoomData.Height.ToString() };
            _txtHeight.TextChanged += InputsChanged;
            pnlBottom.Controls.Add(_txtHeight);

            pnlBottom.Controls.Add(new Label { Text = "Kalınlık:", Location = new Point(180, 20), AutoSize = true });
            _txtThickness = new TextBox { Location = new Point(230, 17), Width = 60, Text = RoomData.Thickness.ToString() };
            _txtThickness.TextChanged += InputsChanged;
            pnlBottom.Controls.Add(_txtThickness);

            Button btnSave = new Button { Text = "ÇİZ VE UYGULA", Location = new Point(620, 10), Size = new Size(150, 40), BackColor = Color.FromArgb(37, 99, 235), ForeColor = Color.White, Font = new Font("Segoe UI", 10, FontStyle.Bold), FlatStyle = FlatStyle.Flat };
            btnSave.Click += BtnSave_Click;
            pnlBottom.Controls.Add(btnSave);

            // Orta Panel (Kuşbakışı Çizim Alanı) - Zaten projede var olan DoubleBufferedPanel'i kullanır
            _pnlPreview = new DoubleBufferedPanel { Dock = DockStyle.Fill, BackColor = Color.White, BorderStyle = BorderStyle.Fixed3D };
            _pnlPreview.Paint += PnlPreview_Paint;
            this.Controls.Add(_pnlPreview);

            // Ölçü Kutuçukları (Çizimin üstünde yüzecek)
            _txtLeft = new TextBox { Width = 50, Visible = false };
            _txtMain = new TextBox { Width = 50 };
            _txtRight = new TextBox { Width = 50, Visible = false };

            _txtLeft.TextChanged += InputsChanged;
            _txtMain.TextChanged += InputsChanged;
            _txtRight.TextChanged += InputsChanged;

            _pnlPreview.Controls.Add(_txtLeft);
            _pnlPreview.Controls.Add(_txtMain);
            _pnlPreview.Controls.Add(_txtRight);
        }

        private Button CreateShapeButton(string text, int y, RoomShape shape)
        {
            Button btn = new Button { Text = text, Location = new Point(10, y), Size = new Size(130, 50), Tag = shape, FlatStyle = FlatStyle.Flat };
            btn.Click += ShapeBtn_Click;
            return btn;
        }

        private void ShapeBtn_Click(object sender, EventArgs e)
        {
            RoomData.Shape = (RoomShape)((Button)sender).Tag;

            // Seçime göre gereksiz duvarları sıfırla
            if (RoomData.Shape == RoomShape.TekDuvar) { RoomData.WallLeft = 0; RoomData.WallRight = 0; }
            else if (RoomData.Shape == RoomShape.L_Duvar) { RoomData.WallRight = 0; if (RoomData.WallLeft == 0) RoomData.WallLeft = 2000; }
            else if (RoomData.Shape == RoomShape.U_Duvar) { if (RoomData.WallLeft == 0) RoomData.WallLeft = 2000; if (RoomData.WallRight == 0) RoomData.WallRight = 2000; }

            UpdateUIFromData();
        }

        private void InputsChanged(object sender, EventArgs e)
        {
            double.TryParse(_txtMain.Text, out double m); RoomData.WallMain = m;
            double.TryParse(_txtLeft.Text, out double l); RoomData.WallLeft = l;
            double.TryParse(_txtRight.Text, out double r); RoomData.WallRight = r;
            double.TryParse(_txtHeight.Text, out double h); RoomData.Height = h;
            double.TryParse(_txtThickness.Text, out double t); RoomData.Thickness = t;

            _pnlPreview.Invalidate();
        }

        private void UpdateUIFromData()
        {
            _txtMain.Text = RoomData.WallMain.ToString();
            _txtLeft.Text = RoomData.WallLeft.ToString();
            _txtRight.Text = RoomData.WallRight.ToString();

            _txtLeft.Visible = RoomData.Shape == RoomShape.L_Duvar || RoomData.Shape == RoomShape.U_Duvar;
            _txtRight.Visible = RoomData.Shape == RoomShape.U_Duvar;

            _btnTek.BackColor = RoomData.Shape == RoomShape.TekDuvar ? Color.LightBlue : Color.White;
            _btnL.BackColor = RoomData.Shape == RoomShape.L_Duvar ? Color.LightBlue : Color.White;
            _btnU.BackColor = RoomData.Shape == RoomShape.U_Duvar ? Color.LightBlue : Color.White;

            _pnlPreview.Invalidate();
        }

        // --- KUŞBAKIŞI DİNAMİK DUVAR ÇİZİM MOTORU ---
        private void PnlPreview_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            float w = _pnlPreview.Width; float h = _pnlPreview.Height;
            float thickness = 20f;
            float padding = 80f;

            using (SolidBrush wallBrush = new SolidBrush(Color.FromArgb(210, 200, 180)))
            using (Pen borderPen = new Pen(Color.FromArgb(150, 140, 120), 2f))
            {
                RectangleF mainRect = new RectangleF(padding, padding, w - 2 * padding, thickness);
                g.FillRectangle(wallBrush, mainRect); g.DrawRectangle(borderPen, mainRect.X, mainRect.Y, mainRect.Width, mainRect.Height);

                _txtMain.Location = new Point((int)(mainRect.X + mainRect.Width / 2 - 25), (int)(mainRect.Y + thickness + 10));

                if (RoomData.Shape == RoomShape.L_Duvar || RoomData.Shape == RoomShape.U_Duvar)
                {
                    RectangleF leftRect = new RectangleF(padding, padding, thickness, h - 2 * padding);
                    g.FillRectangle(wallBrush, leftRect); g.DrawRectangle(borderPen, leftRect.X, leftRect.Y, leftRect.Width, leftRect.Height);
                    _txtLeft.Location = new Point((int)(leftRect.X + thickness + 10), (int)(leftRect.Y + leftRect.Height / 2 - 10));
                }

                if (RoomData.Shape == RoomShape.U_Duvar)
                {
                    RectangleF rightRect = new RectangleF(w - padding - thickness, padding, thickness, h - 2 * padding);
                    g.FillRectangle(wallBrush, rightRect); g.DrawRectangle(borderPen, rightRect.X, rightRect.Y, rightRect.Width, rightRect.Height);
                    _txtRight.Location = new Point((int)(rightRect.X - 60), (int)(rightRect.Y + rightRect.Height / 2 - 10));
                }
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }
    }
}