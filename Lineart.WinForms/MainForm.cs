using System;
using System.Windows.Forms;
using DevExpress.XtraBars;
using Lineart.Core.Document;
using Lineart.Core.Entities;
using Lineart.WinForms.Controls;

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
        }

        private void SetupWorkspace()
        {
            // Yeni ve boş bir proje belgesi başlat
            _currentDocument = new DrawingDocument();

            _canvas = new DrawingCanvas();
            _canvas.Dock = DockStyle.Fill;
            _canvas.Document = _currentDocument;
            this.Controls.Add(_canvas);
            _canvas.BringToFront();

            // KULLANICI EKRANDA BİR ŞEYE TIKLADIĞINDA NE OLACAK?
            _canvas.SelectionChanged += Canvas_SelectionChanged;

            // PROPERTYGRID'DE ÖLÇÜ DEĞİŞTİĞİNDE NE OLACAK?
            propertyGridControl1.CellValueChanged += PropertyGridControl1_CellValueChanged;
        }

        private void Canvas_SelectionChanged(object sender, CabinetEntity selectedCabinet)
        {
            // Ekranda seçilen dolabı, sağdaki Özellik (PropertyGrid) paneline bağla.
            // Eğer boşluğa tıklanmışsa null gider, panel temizlenir.
            propertyGridControl1.SelectedObject = selectedCabinet;
        }

        private void PropertyGridControl1_CellValueChanged(object sender, DevExpress.XtraVerticalGrid.Events.CellValueChangedEventArgs e)
        {
            if (propertyGridControl1.SelectedObject is CabinetEntity selectedCabinet)
            {
                // Değişen özelliğe göre dolabı yeniden hesapla ve ekranı güncelle
                selectedCabinet.RebuildParts();
                _canvas.Invalidate();
            }
        }

        // --- RİBBON MENÜ BUTON OLAYI ---
        // DevExpress Ribbon'a "Alt Dolap Ekle" adında bir BarButtonItem eklediğinizi ve 
        // ItemClick olayına bunu bağladığınızı varsayıyoruz:
        private void btnAltDolapEkle_ItemClick(object sender, ItemClickEventArgs e)
        {
            // Kullanıcının komutuyla YENİ bir dolap yarat
            var yeniDolap = new CabinetEntity
            {
                Name = "Yeni Alt Dolap",
                TotalWidth = 600,
                TotalHeight = 720,
                TotalDepth = 560
            };

            // X ekseninde biraz sağa kaydırarak ekle (üst üste binmesinler diye)
            int dolapSayisi = _currentDocument.Cabinets.Count;
            yeniDolap.Position = new Core.Geometry.Point2D(dolapSayisi * 700, 0);

            _currentDocument.AddCabinet(yeniDolap);

            // Ekranı güncelle
            _canvas.Invalidate();
        }


    }
}