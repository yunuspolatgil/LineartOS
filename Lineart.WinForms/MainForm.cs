using DevExpress.XtraBars;
using DevExpress.XtraTreeList.Nodes;
using Lineart.Core.Document;
using Lineart.Core.Entities;
using Lineart.WinForms.Controls;
using System;
using System.ComponentModel;
using System.Windows.Forms;

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
            _currentDocument = new DrawingDocument();

            _canvas = new DrawingCanvas();
            _canvas.Dock = DockStyle.Fill;
            _canvas.Document = _currentDocument;
            this.Controls.Add(_canvas);
            _canvas.SendToBack();

            // Olay Abonelikleri
            _canvas.SelectionChanged += Canvas_SelectionChanged;
            propertyGridControl1.CellValueChanged += PropertyGridControl1_CellValueChanged;

            // YENİ: TreeList Tıklama Olayı
            treeListProject.FocusedNodeChanged += TreeListProject_FocusedNodeChanged;

            // TreeList Görsel Ayarları (Tek sütunlu temiz bir görünüm)
            treeListProject.Columns.Clear();
            treeListProject.Columns.Add(new DevExpress.XtraTreeList.Columns.TreeListColumn
            {
                Caption = "Proje Yöneticisi",
                Visible = true,
                FieldName = "Name" // Kolon adı
            });
            treeListProject.OptionsBehavior.Editable = false; // Ağaç üzerinden isim değiştirmeyi şimdilik kapatıyoruz
        }

        // --- YENİ: AĞACI GÜNCELLEME METODU ---
        private void RefreshTreeList()
        {
            treeListProject.BeginUnboundLoad();
            treeListProject.Nodes.Clear();

            // Sahnede ne kadar dolap varsa ağaca ekle
            foreach (var cabinet in _currentDocument.Cabinets)
            {
                // 1. Ana Dolap Düğümü (Node)
                TreeListNode cabinetNode = treeListProject.AppendNode(
                    new object[] { cabinet.Name }, // Ekranda görünecek isim
                    null); // Kök düğüm olduğu için parent null

                cabinetNode.Tag = cabinet; // Tıkladığımızda hangi dolap olduğunu bilmek için Tag içine objeyi saklıyoruz

                // 2. Alt Modül Düğümleri (Eğer detay göstermek isterseniz)
                for (int i = 0; i < cabinet.Modules.Count; i++)
                {
                    var mod = cabinet.Modules[i];
                    TreeListNode modNode = treeListProject.AppendNode(
                        new object[] { $"- {mod.Type} Modülü" },
                        cabinetNode); // Parent olarak üstteki dolabı veriyoruz

                    modNode.Tag = mod; // Tıklanınca modülün özelliklerini göstermek için
                }
            }

            treeListProject.EndUnboundLoad();
            treeListProject.ExpandAll(); // Tüm klasörleri açık göster
        }

        // TreeList'ten (Soldan) Bir Eleman Seçildiğinde...
        private void TreeListProject_FocusedNodeChanged(object sender, DevExpress.XtraTreeList.FocusedNodeChangedEventArgs e)
        {
            if (e.Node == null || e.Node.Tag == null) return;

            var selectedData = e.Node.Tag;

            // 1. Sağdaki PropertyGrid'e seçilen veriyi yolla (Dolap veya Modül fark etmez)
            propertyGridControl1.SelectedObject = selectedData;

            // 2. Eğer seçilen şey bir Dolap ise, siyah tuvalde de onu kırmızı çerçeveye al (Senkronizasyon)
            if (selectedData is CabinetEntity cabinet)
            {
                _canvas.SetSelectedCabinet(cabinet);
            }
            // Eğer bir Modül seçildiyse, o modülün ait olduğu dolabı bulup seçtirebiliriz
            else if (selectedData is CabinetModule)
            {
                var parentNode = e.Node.ParentNode;
                if (parentNode?.Tag is CabinetEntity parentCabinet)
                {
                    _canvas.SetSelectedCabinet(parentCabinet);
                }
            }
        }

        // Siyah Tuvalden (Ortadan) Bir Dolap Seçildiğinde...
        private void Canvas_SelectionChanged(object sender, CabinetEntity selectedCabinet)
        {
            propertyGridControl1.SelectedObject = selectedCabinet;

            if (selectedCabinet != null)
            {
                selectedCabinet.Modules.ListChanged -= SelectedCabinet_ModulesChanged;
                selectedCabinet.Modules.ListChanged += SelectedCabinet_ModulesChanged;

                // YENİ: Tuvalde dolap seçince, soldaki ağaçta da o dolabı bulup seçili hale getir (Senkronizasyon)
                var nodeToSelect = treeListProject.FindNodeByFieldValue("Name", selectedCabinet.Name);
                if (nodeToSelect != null)
                {
                    treeListProject.SetFocusedNode(nodeToSelect);
                }
            }
        }

        private void SelectedCabinet_ModulesChanged(object sender, ListChangedEventArgs e)
        {
            if (propertyGridControl1.SelectedObject is CabinetEntity selectedCabinet)
            {
                selectedCabinet.RebuildParts();
                _canvas.Invalidate();
                RefreshTreeList(); // Modül değiştiyse/eklendiyse ağacı da yenile
            }
        }

        private void PropertyGridControl1_CellValueChanged(object sender, DevExpress.XtraVerticalGrid.Events.CellValueChangedEventArgs e)
        {
            if (propertyGridControl1.SelectedObject is CabinetEntity selectedCabinet)
            {
                selectedCabinet.RebuildParts();
                _canvas.Invalidate();

                // Eğer dolabın adı (Name) değiştirildiyse ağacı yenile
                if (e.Row.Name == "rowName" || e.Row.Properties.FieldName == "Name")
                {
                    RefreshTreeList();
                }
            }
            // YENİ: Eğer PropertyGrid'de direkt bir Modülün yüksekliğini değiştiriyorsak
            else if (propertyGridControl1.SelectedObject is CabinetModule)
            {
                // Modülün bağlı olduğu dolabı Rebuild yapmamız lazım ama şu an modülden dolaba referansımız yok.
                // Şimdilik TreeList üzerinden parent dolabı buluyoruz:
                var focusedNode = treeListProject.FocusedNode;
                if (focusedNode?.ParentNode?.Tag is CabinetEntity parentCabinet)
                {
                    parentCabinet.RebuildParts();
                    _canvas.Invalidate();
                }
            }
        }

        // Dolap Ekleme Butonu (Mevcut kodunuzun sonuna RefreshTreeList eklenecek)
        private void btnAltDolapEkle_ItemClick(object sender, ItemClickEventArgs e)
        {
            var yeniDolap = new CabinetEntity
            {
                Name = $"Dolap {_currentDocument.Cabinets.Count + 1}",
                TotalWidth = 600,
                TotalHeight = 720,
                TotalDepth = 560
            };

            yeniDolap.Position = new Core.Geometry.Point2D(_currentDocument.Cabinets.Count * 700, 0);
            _currentDocument.AddCabinet(yeniDolap);

            _canvas.Invalidate();

            // YENİ: Dolap eklendiğinde ağacı güncelle
            RefreshTreeList();
        }
    }
}