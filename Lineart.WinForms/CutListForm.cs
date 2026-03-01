using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Lineart.Core.Document;
using Lineart.Core.Entities;
using Lineart.Core.Settings;

namespace Lineart.WinForms
{
    public partial class CutListForm : Form
    {
        private TabControl _tabControl;
        private DataGridView _gridGovde;
        private DataGridView _gridKapak;
        private DataGridView _gridArkalik;

        public CutListForm(DrawingDocument document)
        {
            this.Text = $"ÜRETİM RAPORU - {document.ProjectName.ToUpper()}";
            this.Size = new Size(1100, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(15, 23, 42);

            Panel headerPanel = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.FromArgb(15, 23, 42) };
            Label lblTitle = new Label { Text = "📝 ÜRETİM VE KESİM LİSTESİ", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.FromArgb(56, 189, 248), AutoSize = true, Location = new Point(15, 12) };
            headerPanel.Controls.Add(lblTitle);
            this.Controls.Add(headerPanel);

            _tabControl = new TabControl { Dock = DockStyle.Fill, ItemSize = new Size(200, 40), Font = new Font("Segoe UI", 10, FontStyle.Bold) };

            TabPage tabGovde = new TabPage("GÖVDE");
            TabPage tabKapak = new TabPage("KAPAK & ÇEKMECE");
            TabPage tabArkalik = new TabPage("ARKALIK");

            _gridGovde = CreateStyledGrid();
            _gridKapak = CreateStyledGrid();
            _gridArkalik = CreateStyledGrid();

            tabGovde.Controls.Add(_gridGovde);
            tabKapak.Controls.Add(_gridKapak);
            tabArkalik.Controls.Add(_gridArkalik);

            _tabControl.TabPages.Add(tabGovde);
            _tabControl.TabPages.Add(tabKapak);
            _tabControl.TabPages.Add(tabArkalik);

            this.Controls.Add(_tabControl);
            _tabControl.BringToFront();

            LoadProjectData(document);
        }

        private DataGridView CreateStyledGrid()
        {
            var grid = new DataGridView();
            grid.Dock = DockStyle.Fill;
            grid.AllowUserToAddRows = false;
            grid.ReadOnly = true;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.BackgroundColor = Color.FromArgb(30, 41, 59);
            grid.BorderStyle = BorderStyle.None;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.EnableHeadersVisualStyles = false;
            grid.RowHeadersVisible = false;

            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(15, 23, 42);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(148, 163, 184);
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            grid.ColumnHeadersHeight = 40;

            grid.DefaultCellStyle.BackColor = Color.FromArgb(30, 41, 59);
            grid.DefaultCellStyle.ForeColor = Color.FromArgb(203, 213, 225);
            grid.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(51, 65, 85);
            grid.DefaultCellStyle.SelectionForeColor = Color.White;
            grid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            grid.RowTemplate.Height = 40;
            grid.GridColor = Color.FromArgb(51, 65, 85);

            grid.CellPainting += Grid_CellPainting;

            return grid;
        }

        private void Grid_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            var grid = (DataGridView)sender;
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && grid.Columns[e.ColumnIndex].Name == "BANT")
            {
                e.PaintBackground(e.CellBounds, true);
                string rawData = e.Value?.ToString();

                if (string.IsNullOrEmpty(rawData) || rawData == "0" || !rawData.Contains("|"))
                {
                    TextRenderer.DrawText(e.Graphics, "-", e.CellStyle.Font, e.CellBounds, Color.FromArgb(100, 116, 139), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    e.Handled = true;
                    return;
                }

                var parts = rawData.Split('|');
                if (parts.Length != 4) return;

                double.TryParse(parts[0], out double top);
                double.TryParse(parts[1], out double bottom);
                double.TryParse(parts[2], out double left);
                double.TryParse(parts[3], out double right);

                int w = 44;
                int h = 24;
                int x = e.CellBounds.X + (e.CellBounds.Width - w) / 2;
                int y = e.CellBounds.Y + (e.CellBounds.Height - h) / 2;

                Rectangle rect = new Rectangle(x, y, w, h);
                using (SolidBrush boxBrush = new SolidBrush(Color.FromArgb(51, 65, 85)))
                    e.Graphics.FillRectangle(boxBrush, rect);

                DrawMiniEdge(e.Graphics, x, y, w, 2, top);             // Üst
                DrawMiniEdge(e.Graphics, x, y + h - 2, w, 2, bottom);  // Alt
                DrawMiniEdge(e.Graphics, x, y, 2, h, left);            // Sol
                DrawMiniEdge(e.Graphics, x + w - 2, y, 2, h, right);   // Sağ

                e.Handled = true;
            }
        }

        private void DrawMiniEdge(Graphics g, int x, int y, int w, int h, double val)
        {
            if (val == 0) return;

            Color c = Color.Gray;
            if (val == 0.4) c = Color.FromArgb(34, 197, 94);
            else if (val == 0.8) c = Color.FromArgb(59, 130, 246);
            else if (val == 2.0) c = Color.FromArgb(234, 179, 8);

            using (SolidBrush b = new SolidBrush(c))
                g.FillRectangle(b, x, y, w, h);
        }

        public class CutListRow
        {
            public string MALZEME { get; set; }
            public string PARÇA { get; set; }
            public int ADET { get; set; }
            public double BOY { get; set; }
            public double EN { get; set; }
            public string BANT { get; set; }
            public string AÇIKLAMA { get; set; }
        }

        private class PartDto
        {
            public string Category { get; set; }
            public string Material { get; set; }
            public string Name { get; set; }
            public double Length { get; set; }
            public double Width { get; set; }
            public string BandingCode { get; set; }
            public string Desc { get; set; }
        }

        private void LoadProjectData(DrawingDocument document)
        {
            var allParts = new List<PartDto>();
            var s = AppGlobalSettings.Current;

            foreach (var cab in document.Cabinets)
            {
                cab.RebuildParts();

                bool isWallCabinet = cab.Position.Y < (document.RoomHeight / 2);
                var scheme = isWallCabinet ? s.WallCabinetBanding : s.BaseCabinetBanding;

                foreach (var p in cab.Parts)
                {
                    var item = new PartDto();

                    string rawName = p.Name.ToUpper();
                    // YENİ: Kapama ve Dönüş isimlerini bozmadan geçiriyoruz
                    if (rawName.Contains("KAPAMA") || rawName.Contains("BİTİŞ") || rawName.Contains("DÖNÜŞ")) item.Name = rawName;
                    else if (rawName.Contains("YAN")) item.Name = "YAN DİKME";
                    else if (rawName.Contains("KAYIT") || rawName.Contains("ÇATKI")) item.Name = "KAYIT / ÇATKI";
                    else if (rawName.Contains("ALT TABLA")) item.Name = "ALT TABLA";
                    else if (rawName.Contains("ÜST TABLA")) item.Name = "ÜST TABLA";
                    else if (rawName.Contains("BAZA")) item.Name = "BAZA";
                    else if (rawName.Contains("RAF")) item.Name = "RAF";
                    else item.Name = rawName;

                    item.Length = Math.Max(p.WidthMm, p.HeightMm);
                    item.Width = Math.Min(p.WidthMm, p.HeightMm);

                    // MALZEME GRUPLAMASI
                    // Eğer parça doğrudan kapa/çekmece ise VEYA "Kapak Malzemesinden" üretilmiş bir Kapama paneli ise Kapak sekmesine gönder.
                    if (p.Type == PartType.Kapak || p.Type == PartType.Cekmece ||
                       (cab.Type == CabinetType.Kapama && cab.FillerMaterial == FillerMaterialGroup.Kapak && !rawName.Contains("DÖNÜŞ")))
                    {
                        item.Name = (cab.Type == CabinetType.Kapama) ? rawName : "KAPAK / KLAPA";
                        item.Category = "Kapak";
                        item.Material = $"18mm {s.DefaultDoorStyle}";
                        item.BandingCode = $"{scheme.Door.Top}|{scheme.Door.Bottom}|{scheme.Door.Left}|{scheme.Door.Right}";
                        item.Desc = cab.Type == CabinetType.Kapama ? "Kapak Malzemesi Kapama" : "Modül Kapağı";
                    }
                    else if (p.Type == PartType.Arkalik)
                    {
                        item.Category = "Arkalık";
                        item.Material = "8mm Arkalık MDF";
                        item.BandingCode = "0";
                        item.Desc = "Kasa Arkası";
                    }
                    else
                    {
                        item.Category = "Gövde";
                        item.Material = $"{s.DefaultThickness}mm Suntalam/MDF";
                        item.Desc = cab.Type == CabinetType.Kapama ? "Gövde Malzemesi Kapama" : "Karkas Parçası";

                        if (item.Name == "YAN DİKME")
                            item.BandingCode = $"{scheme.SidePanel.Top}|{scheme.SidePanel.Bottom}|{scheme.SidePanel.Left}|{scheme.SidePanel.Right}";
                        else if (item.Name == "ALT TABLA")
                            item.BandingCode = $"{scheme.BottomPanel.Top}|{scheme.BottomPanel.Bottom}|{scheme.BottomPanel.Left}|{scheme.BottomPanel.Right}";
                        else if (item.Name == "ÜST TABLA")
                            item.BandingCode = $"{scheme.TopPanel.Top}|{scheme.TopPanel.Bottom}|{scheme.TopPanel.Left}|{scheme.TopPanel.Right}";
                        else if (item.Name == "RAF")
                            item.BandingCode = $"{scheme.Shelf.Top}|{scheme.Shelf.Bottom}|{scheme.Shelf.Left}|{scheme.Shelf.Right}";
                        else if (item.Name == "BAZA")
                            item.BandingCode = "0.4|0.4|0|0";
                        else if (item.Name.Contains("KAPAMA") || item.Name.Contains("BİTİŞ"))
                            item.BandingCode = $"{scheme.SidePanel.Top}|{scheme.SidePanel.Bottom}|{scheme.SidePanel.Left}|{scheme.SidePanel.Right}"; // Kapamayı yan dikme mantığıyla bantla
                        else if (item.Name.Contains("DÖNÜŞ"))
                            item.BandingCode = "0|0|0|0"; // L Dönüşü içte kalır bantlanmaz
                        else
                            item.BandingCode = "0";
                    }
                    allParts.Add(item);
                }
            }

            var groupedParts = allParts
                .GroupBy(x => new { x.Category, x.Material, x.Name, L = Math.Round(x.Length, 1), W = Math.Round(x.Width, 1), x.BandingCode, x.Desc })
                .Select(g => new CutListRow
                {
                    MALZEME = g.Key.Material,
                    PARÇA = g.Key.Name,
                    ADET = g.Count(),
                    BOY = g.Key.L,
                    EN = g.Key.W,
                    BANT = g.Key.BandingCode,
                    AÇIKLAMA = g.Key.Desc
                }).ToList();

            var govdeList = groupedParts.Where(x => x.AÇIKLAMA.Contains("Karkas") || x.AÇIKLAMA.Contains("Gövde")).OrderByDescending(x => x.BOY).ToList();
            var kapakList = groupedParts.Where(x => x.AÇIKLAMA.Contains("Modül") || x.AÇIKLAMA.Contains("Kapak")).OrderByDescending(x => x.BOY).ToList();
            var arkalikList = groupedParts.Where(x => x.AÇIKLAMA.Contains("Kasa Arkası")).OrderByDescending(x => x.BOY).ToList();

            _gridGovde.DataSource = ToDataTable(govdeList);
            _gridKapak.DataSource = ToDataTable(kapakList);
            _gridArkalik.DataSource = ToDataTable(arkalikList);

            FormatGridColumns(_gridGovde);
            FormatGridColumns(_gridKapak);
            FormatGridColumns(_gridArkalik);

            _tabControl.TabPages[0].Text = $"GÖVDE ({govdeList.Sum(x => x.ADET)})";
            _tabControl.TabPages[1].Text = $"KAPAK & ÇEK. ({kapakList.Sum(x => x.ADET)})";
            _tabControl.TabPages[2].Text = $"ARKALIK ({arkalikList.Sum(x => x.ADET)})";
        }

        private void FormatGridColumns(DataGridView grid)
        {
            if (grid.Columns.Contains("ADET"))
            {
                var col = grid.Columns["ADET"];
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                col.DefaultCellStyle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
                col.DefaultCellStyle.ForeColor = Color.FromArgb(52, 211, 153);
                col.Width = 80;
            }
            if (grid.Columns.Contains("BOY"))
            {
                grid.Columns["BOY"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                grid.Columns["BOY"].Width = 80;
            }
            if (grid.Columns.Contains("EN"))
            {
                grid.Columns["EN"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                grid.Columns["EN"].Width = 80;
            }
            if (grid.Columns.Contains("BANT"))
            {
                grid.Columns["BANT"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                grid.Columns["BANT"].Width = 100;
            }
        }

        private DataTable ToDataTable<T>(List<T> items)
        {
            var dataTable = new DataTable(typeof(T).Name);
            var props = typeof(T).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            foreach (var prop in props) dataTable.Columns.Add(prop.Name, prop.PropertyType);
            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (int i = 0; i < props.Length; i++) values[i] = props[i].GetValue(item, null);
                dataTable.Rows.Add(values);
            }
            return dataTable;
        }
    }
}