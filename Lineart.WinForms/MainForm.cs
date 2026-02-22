using System;
using System.Windows.Forms;
using Lineart.Core.Document;
using Lineart.Core.Entities;
using Lineart.WinForms.Controls;
// DevExpress using'leri eklenecektir...

namespace Lineart.WinForms
{
    public partial class MainForm : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        private DrawingDocument _currentDocument;
        private DrawingCanvas _canvas;

        public MainForm()
        {
            InitializeComponent();
            SetupWorkspace();
            CreateMockData();
        }

        private void SetupWorkspace()
        {
            // 1. Çizim Tahtasını oluştur ve orta alana yerleştir
            _canvas = new DrawingCanvas();
            _canvas.Dock = DockStyle.Fill;
            this.Controls.Add(_canvas); // Veya varsa DocumentManager alanına eklenir
            _canvas.BringToFront();

            // 2. PropertyGrid Olayını Bağla (Ölçü değiştiğinde çizimi güncelle)
            propertyGridControl1.CellValueChanged += PropertyGridControl1_CellValueChanged;

            // 3. TreeList Olayını Bağla (Ağaçtan dolap seçildiğinde özelliklerini göster)
            treeListProject.FocusedNodeChanged += TreeListProject_FocusedNodeChanged;
        }

        private void CreateMockData()
        {
            _currentDocument = new DrawingDocument();
            var myCabinet = new CabinetEntity { Name = "Evye Alt Dolabı", TotalWidth = 900 };

            _currentDocument.AddCabinet(myCabinet); // İçeride RebuildParts çalışır

            _canvas.Document = _currentDocument;
            _canvas.Invalidate(); // İlk çizimi tetikle

            // Geçici olarak TreeList'e doldurmak yerine doğrudan PropertyGrid'e atıyoruz
            propertyGridControl1.SelectedObject = myCabinet;
        }

        // --- KONTROL OLAYLARI (EVENTS) ---

        // Sağdaki panelde bir ölçü (örn: Genişlik 600'den 800'e) değiştirildiğinde tetiklenir
        private void PropertyGridControl1_CellValueChanged(object sender, DevExpress.XtraVerticalGrid.Events.CellValueChangedEventArgs e)
        {
            if (propertyGridControl1.SelectedObject is CabinetEntity selectedCabinet)
            {
                // 1. Parametrik motoru çalıştır: Parçaları yeni milimetre değerlerine göre baştan hesapla
                selectedCabinet.RebuildParts();

                // 2. Ekranı yenile
                _canvas.Invalidate();
            }
        }

        // Soldaki ağaçtan (TreeList) başka bir dolap seçildiğinde tetiklenir
        private void TreeListProject_FocusedNodeChanged(object sender, DevExpress.XtraTreeList.FocusedNodeChangedEventArgs e)
        {
            // Seçilen düğümdeki objeyi alıp PropertyGrid'e gönderiyoruz
            var selectedObj = treeListProject.GetDataRecordByNode(e.Node);
            if (selectedObj != null)
            {
                propertyGridControl1.SelectedObject = selectedObj;
            }
        }
    }
}