using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraEditors;
using Lineart.Core.Document;
using Lineart.Core.Settings;

namespace Lineart.WinForms
{
    public partial class ProjectManagerForm : XtraForm
    {
        private GridControl grid;
        private GridView view;
        public DrawingDocument SelectedDocument { get; private set; }

        public ProjectManagerForm()
        {
            this.Text = "MobilyaOS - Proje Yöneticisi";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.IconOptions.ShowIcon = false;

            // --- ÜST BUTONLAR PANELİ ---
            Panel controlPanel = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.FromArgb(40, 40, 45) };

            SimpleButton btnNew = new SimpleButton { Text = "✨ Yeni Proje Oluştur", Bounds = new Rectangle(15, 10, 150, 40) };
            btnNew.Appearance.BackColor = Color.SeaGreen;
            btnNew.Appearance.ForeColor = Color.White;
            btnNew.Appearance.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnNew.Click += (s, e) => { SelectedDocument = new DrawingDocument(); this.DialogResult = DialogResult.OK; };

            SimpleButton btnOpen = new SimpleButton { Text = "📂 Seçili Projeyi Aç", Bounds = new Rectangle(175, 10, 150, 40) };
            btnOpen.Appearance.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnOpen.Click += BtnOpen_Click;

            controlPanel.Controls.AddRange(new Control[] { btnNew, btnOpen });
            this.Controls.Add(controlPanel);

            // --- GRID CONTROL KURULUMU ---
            grid = new GridControl { Dock = DockStyle.Fill };
            view = new GridView(grid);
            grid.MainView = view;

            view.OptionsBehavior.Editable = false;
            view.OptionsSelection.EnableAppearanceFocusedCell = false;
            view.OptionsView.ShowGroupPanel = false;
            view.OptionsView.ShowIndicator = false;
            view.RowHeight = 35;
            view.Appearance.Row.Font = new Font("Segoe UI", 11);
            view.Appearance.HeaderPanel.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            view.DoubleClick += BtnOpen_Click; // Çift tıklayınca da açsın

            this.Controls.Add(grid);
            grid.BringToFront();

            LoadProjects();
        }

        private void LoadProjects()
        {
            List<ProjectSummary> list = new List<ProjectSummary>();
            string dir = AppGlobalSettings.ProjectsFolder;

            if (Directory.Exists(dir))
            {
                var files = Directory.GetFiles(dir, "*.mob");
                foreach (var file in files)
                {
                    try
                    {
                        var doc = DrawingDocument.LoadFromFile(file);
                        list.Add(new ProjectSummary
                        {
                            FilePath = file,
                            ProjeAdi = doc.ProjectName,
                            Musteri = doc.CustomerName,
                            DolapSayisi = doc.Cabinets.Count,
                            SonDegisiklik = doc.LastModified
                        });
                    }
                    catch { /* Bozuk dosyaları atla */ }
                }
            }
            grid.DataSource = list;
            view.Columns["FilePath"].Visible = false; // Yolu gizle
        }

        private void BtnOpen_Click(object sender, EventArgs e)
        {
            if (view.GetFocusedRow() is ProjectSummary summary)
            {
                SelectedDocument = DrawingDocument.LoadFromFile(summary.FilePath);
                this.DialogResult = DialogResult.OK;
            }
        }

        // Grid'de göstermek için özet sınıf
        public class ProjectSummary
        {
            public string FilePath { get; set; }
            public string ProjeAdi { get; set; }
            public string Musteri { get; set; }
            public int DolapSayisi { get; set; }
            public DateTime SonDegisiklik { get; set; }
        }
    }
}