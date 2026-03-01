using DevExpress.XtraBars;
using DevExpress.XtraTreeList.Nodes;
using Lineart.Core.Document;
using Lineart.Core.Entities;
using Lineart.Core.Settings;
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
        private ContextMenuStrip _treeMenu; // Ağaç için Sağ Tık Menüsü
        private Cabinet3DViewerForm _active3DViewer;
        private QuickMovePad _activeMovePad;

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

            _canvas.SelectionChanged += Canvas_SelectionChanged;
            propertyGridControl1.CellValueChanged += PropertyGridControl1_CellValueChanged;

            treeListProject.FocusedNodeChanged += TreeListProject_FocusedNodeChanged;
            _canvas.CabinetDoubleClicked += Canvas_CabinetDoubleClicked;

            // --- SAĞ TIK MENÜSÜNÜN (CONTEXT MENU) KURULUMU ---
            _treeMenu = new ContextMenuStrip();
            _treeMenu.ItemClicked += TreeMenu_ItemClicked;
            treeListProject.MouseUp += TreeListProject_MouseUp; // Fareyle tıklandığında menüyü açacak

            treeListProject.Columns.Clear();
            treeListProject.Columns.Add(new DevExpress.XtraTreeList.Columns.TreeListColumn
            {
                Caption = "Proje Yöneticisi",
                Visible = true,
                FieldName = "Name"
            });
            treeListProject.OptionsBehavior.Editable = false;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Eski _currentDocument = new DrawingDocument(); satırını SİLİN ve bunu koyun:

            using (var manager = new ProjectManagerForm())
            {
                if (manager.ShowDialog() == DialogResult.OK)
                {
                    _currentDocument = manager.SelectedDocument;
                    _canvas.Document = _currentDocument;
                    RefreshTreeList();
                    propertyGridControl1.SelectedObject = _currentDocument; // Başlangıçta proje ayarlarını göster
                }
                else
                {
                    Application.Exit(); // Pencereyi kapatırsa programdan çık
                }
            }
        }

        private void Canvas_CabinetDoubleClicked(object sender, CabinetEntity clickedCabinet)
        {
            // Eğer daha önce açılmış bir araç kutusu varsa kapat
            if (_activeMovePad != null && !_activeMovePad.IsDisposed)
            {
                _activeMovePad.Close();
            }

            // Yeni araç kutusunu aç
            _activeMovePad = new QuickMovePad(clickedCabinet, _currentDocument, _canvas);

            // Araç kutusunu farenin olduğu yere şık bir şekilde aç
            _activeMovePad.Location = Cursor.Position;
            _activeMovePad.Show();
        }

        // --- TREE LIST SAĞ TIK YÖNETİMİ ---
        private void TreeListProject_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var hitInfo = treeListProject.CalcHitInfo(e.Location);
                if (hitInfo.HitInfoType == DevExpress.XtraTreeList.HitInfoType.Cell)
                {
                    treeListProject.FocusedNode = hitInfo.Node;
                    var tag = hitInfo.Node.Tag;

                    _treeMenu = new ContextMenuStrip();

                    if (tag is CabinetEntity cab)
                    {
                        // Modül Ekleme İşlemleri
                        _treeMenu.Items.Add("Kapak Modülü Ekle").Tag = new Action(() => AddModule(cab, ModuleType.Kapak));
                        _treeMenu.Items.Add("Çekmece Modülü Ekle").Tag = new Action(() => AddModule(cab, ModuleType.Cekmece));

                        // --- HATA BURADA ÇÖZÜLDÜ: ANKASTRE İKİYE AYRILDI ---
                        _treeMenu.Items.Add("Ankastre Fırın (60cm) Ekle").Tag = new Action(() => AddModule(cab, ModuleType.AnkastreFirin));
                        _treeMenu.Items.Add("Mikrodalga (38cm) Ekle").Tag = new Action(() => AddModule(cab, ModuleType.AnkastreMikrodalga));

                        _treeMenu.Items.Add("Açık Raf Modülü Ekle").Tag = new Action(() => AddModule(cab, ModuleType.AcikRaf));
                        _treeMenu.Items.Add(new ToolStripSeparator());

                        _treeMenu.Items.Add("Bulaşık Mak. (Tam Ankastre)").Tag = new Action(() => AddModule(cab, ModuleType.AnkastreBulasikTam));
                        _treeMenu.Items.Add("Bulaşık Mak. (Yarım Ankastre)").Tag = new Action(() => AddModule(cab, ModuleType.AnkastreBulasikYarim));

                        // Araçlar
                        _treeMenu.Items.Add("🌟 3D Görüntüle 🌟").Tag = new Action(() =>
                        {
                            if (_active3DViewer != null && !_active3DViewer.IsDisposed) _active3DViewer.Close();
                            _active3DViewer = new Cabinet3DViewerForm(cab);
                            _active3DViewer.Show();
                        });

                        _treeMenu.Items.Add(new ToolStripSeparator());

                        // Kopyalama ve Silme
                        _treeMenu.Items.Add("🔄 Dolabı Çoğalt (Kopyala)").Tag = new Action(() => DuplicateCabinet(cab));

                        _treeMenu.Items.Add(new ToolStripSeparator());
                        _treeMenu.Items.Add("Dolabı Sil").Tag = new Action(() => DeleteCabinet(cab));
                    }
                    else if (tag is CabinetModule mod)
                    {
                        var parentCab = hitInfo.Node.ParentNode?.Tag as CabinetEntity;
                        _treeMenu.Items.Add("Bu Modülü Sil").Tag = new Action(() => DeleteModule(parentCab, mod));
                    }

                    if (_treeMenu.Items.Count > 0)
                    {
                        _treeMenu.ItemClicked += (s, args) =>
                        {
                            if (args.ClickedItem.Tag is Action action) action.Invoke();
                        };
                        _treeMenu.Show(treeListProject, e.Location);
                    }
                }
            }
        }

        private void DuplicateCabinet(CabinetEntity cab)
        {
            var clone = cab.Clone(); // Dolabın her şeyini kopyala

            // Duvardan taşmasını engelle (Bir önceki adımda yazdığımız GetSmartPlacementX fonksiyonunu kullanıyoruz)
            double startX = GetSmartPlacementX(clone.TotalWidth, clone.Position.Y < (_currentDocument.RoomHeight / 2));
            clone.Position = new Core.Geometry.Point2D(startX, clone.Position.Y);

            _currentDocument.AddCabinet(clone);
            _canvas.Invalidate();
            RefreshTreeList();
        }

        private void TreeMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            _treeMenu.Hide();
            if (e.ClickedItem.Tag is Action action)
            {
                action.Invoke(); // Tıklanan butona ait işlemi çalıştır
            }
        }

        // Kısayol İşlemleri
        private void AddModule(CabinetEntity cab, ModuleType type)
        {
            cab.Modules.Add(new CabinetModule { Type = type, HeightMm = 0 });
            // BindingList otomatik olarak SelectedCabinet_ModulesChanged eventini tetikler!
        }

        private void DeleteModule(CabinetEntity cab, CabinetModule mod)
        {
            if (cab != null) cab.Modules.Remove(mod);
        }

        private void DeleteCabinet(CabinetEntity cab)
        {
            _currentDocument.Cabinets.Remove(cab);
            _canvas.SetSelectedCabinet(null);
            _canvas.Invalidate();
            RefreshTreeList();
        }
        // -------------------------------------

        private void RefreshTreeList()
        {
            treeListProject.BeginUnboundLoad();
            treeListProject.Nodes.Clear();

            foreach (var cabinet in _currentDocument.Cabinets)
            {
                TreeListNode cabinetNode = treeListProject.AppendNode(new object[] { cabinet.Name }, null);
                cabinetNode.Tag = cabinet;

                for (int i = 0; i < cabinet.Modules.Count; i++)
                {
                    var mod = cabinet.Modules[i];
                    TreeListNode modNode = treeListProject.AppendNode(new object[] { $"- {mod.Type} Modülü" }, cabinetNode);
                    modNode.Tag = mod;
                }
            }

            treeListProject.EndUnboundLoad();
            treeListProject.ExpandAll();
        }

        private void TreeListProject_FocusedNodeChanged(object sender, DevExpress.XtraTreeList.FocusedNodeChangedEventArgs e)
        {
            if (e.Node == null || e.Node.Tag == null) return;

            var selectedData = e.Node.Tag;
            propertyGridControl1.SelectedObject = selectedData;

            // KRİTİK EKSİK BURASI: Nesne tipi değiştiğinde DevExpress'in alanları yeniden okumasını sağlar
            propertyGridControl1.RetrieveFields();
            propertyGridControl1.Refresh();

            if (selectedData is CabinetEntity cabinet)
            {
                _canvas.SetSelectedCabinet(cabinet);
            }
            else if (selectedData is CabinetModule)
            {
                var parentNode = e.Node.ParentNode;
                if (parentNode?.Tag is CabinetEntity parentCabinet)
                {
                    _canvas.SetSelectedCabinet(parentCabinet);
                }
            }
        }

        private void Canvas_SelectionChanged(object sender, CabinetEntity selectedCabinet)
        {
            propertyGridControl1.SelectedObject = selectedCabinet;

            // KRİTİK EKSİK BURASI
            propertyGridControl1.RetrieveFields();

            if (selectedCabinet != null)
            {
                selectedCabinet.Modules.ListChanged -= SelectedCabinet_ModulesChanged;
                selectedCabinet.Modules.ListChanged += SelectedCabinet_ModulesChanged;
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
                RefreshTreeList();
            }

            // 3D Pencere açıksa anında güncellenmesini sağla
            if (_active3DViewer != null && !_active3DViewer.IsDisposed)
            {
                _active3DViewer.Refresh3DView();
            }
        }

        private void PropertyGridControl1_CellValueChanged(object sender, DevExpress.XtraVerticalGrid.Events.CellValueChangedEventArgs e)
        {
            if (propertyGridControl1.SelectedObject is CabinetEntity selectedCabinet)
            {
                selectedCabinet.RebuildParts();
                _canvas.Invalidate();

                if (e.Row.Name == "rowName" || e.Row.Properties.FieldName == "Name")
                {
                    RefreshTreeList();
                }
            }
            else if (propertyGridControl1.SelectedObject is CabinetModule)
            {
                var focusedNode = treeListProject.FocusedNode;
                if (focusedNode?.ParentNode?.Tag is CabinetEntity parentCabinet)
                {
                    parentCabinet.RebuildParts();
                    _canvas.Invalidate();
                }
            }

            // 3D Pencere açıksa anında güncellenmesini sağla
            if (_active3DViewer != null && !_active3DViewer.IsDisposed)
            {
                _active3DViewer.Refresh3DView();
            }

        }

        private void btnAltDolapEkle_ItemClick(object sender, ItemClickEventArgs e)
        {
            var s = AppGlobalSettings.Current; // Ayarları çek
            var cab = new CabinetEntity
            {
                Name = $"Alt Dolap {_currentDocument.Cabinets.Count + 1}",
                TotalWidth = 600,
                TotalHeight = s.DefaultBaseHeight,
                TotalDepth = s.DefaultBaseDepth,
                PlinthHeight = s.DefaultBasePlinth,
                CrownHeight = 0,
                MaterialThickness = s.DefaultThickness,
                BottomJoin = s.DefaultBottomJoin,
                TopJoin = s.DefaultTopJoin,
                BackType = s.DefaultBackType,
                TopStyle = TopPanelStyle.OnArkaKayit,
                Handle = s.DefaultHandleType
            };

            cab.RebuildParts();
            double startX = GetSmartPlacementX(cab.TotalWidth, false);
            cab.Position = new Core.Geometry.Point2D(startX, _currentDocument.RoomHeight - cab.OverallHeight);

            _currentDocument.AddCabinet(cab);
            _canvas.Invalidate();
            RefreshTreeList();
        }

        private void btnUstDolapEkle_ItemClick(object sender, ItemClickEventArgs e)
        {
            var s = AppGlobalSettings.Current; // Ayarları çek
            var cab = new CabinetEntity
            {
                Name = $"Üst Dolap {_currentDocument.Cabinets.Count + 1}",
                TotalWidth = 600,
                TotalHeight = s.DefaultUpperHeight,
                TotalDepth = s.DefaultUpperDepth,
                PlinthHeight = 0,
                CrownHeight = 0,
                MaterialThickness = s.DefaultThickness,
                BottomJoin = s.DefaultBottomJoin,
                TopJoin = s.DefaultTopJoin,
                BackType = s.DefaultBackType,
                TopStyle = TopPanelStyle.TamDolu
            };

            cab.RebuildParts();

            // Akıllı X ve Y Koordinatı
            double startX = GetSmartPlacementX(cab.TotalWidth, true);
            double globalY = _currentDocument.RoomHeight - s.DefaultUpperElevation - cab.OverallHeight;

            cab.Position = new Core.Geometry.Point2D(startX, globalY);

            _currentDocument.AddCabinet(cab);
            _canvas.Invalidate();
            RefreshTreeList();
        }

        private void btnAc_ItemClick(object sender, ItemClickEventArgs e)
        {
            // Yaptığımız o harika GridControl'lü Proje Yöneticisi ekranını aç
            using (var manager = new ProjectManagerForm())
            {
                if (manager.ShowDialog() == DialogResult.OK)
                {
                    // Varsa arkada açık kalan 3D penceresini temizle
                    if (_active3DViewer != null && !_active3DViewer.IsDisposed)
                        _active3DViewer.Close();

                    // Yöneticiden seçilen projeyi sahneye aktar
                    _currentDocument = manager.SelectedDocument;
                    _canvas.Document = _currentDocument;

                    // Ekranı, seçimi ve ağacı güncelle
                    _canvas.SetSelectedCabinet(null);
                    _canvas.Invalidate();
                    RefreshTreeList();

                    // Sağ panele yüklenen projenin bilgilerini getir
                    propertyGridControl1.SelectedObject = _currentDocument;
                    propertyGridControl1.RetrieveFields();
                    propertyGridControl1.Refresh();
                }
            }
        }

        private void btnKaydet_ItemClick(object sender, ItemClickEventArgs e)
        {
            _currentDocument.SaveToSystem();
            AppGlobalSettings.Current.Save(); // Projeyi kaydederken genel ayarları da kaydet
            MessageBox.Show("Proje ve Fabrika Ayarları başarıyla kaydedildi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnProjeAyarlari_ItemClick(object sender, ItemClickEventArgs e)
        {
            _canvas.SetSelectedCabinet(null);
            propertyGridControl1.SelectedObject = AppGlobalSettings.Current;
            propertyGridControl1.RetrieveFields();
            propertyGridControl1.Refresh();
        }
        // YARDIMCI METOT: Yeni dolabın X koordinatını bulur ve duvarı aşmasını engeller
        private double GetSmartPlacementX(double cabinetWidth, bool isUpperCabinet)
        {
            double nextX = 0;

            foreach (var cab in _currentDocument.Cabinets)
            {
                // Dolap üst dolap mı alt dolap mı? (Yerden yüksekliğine göre anlıyoruz)
                bool isCabUpper = cab.Position.Y < (_currentDocument.RoomHeight / 2);

                // Sadece kendi türündeki dolapların sağına dizil
                if (isUpperCabinet == isCabUpper)
                {
                    double rightEdge = cab.Position.X + cab.TotalWidth;
                    if (rightEdge > nextX)
                        nextX = rightEdge;
                }
            }

            // DUVAR TAŞMA KONTROLÜ: Eğer yeni eklenecek dolap duvardan çıkıyorsa, onu duvarın tam dibine daya
            if (nextX + cabinetWidth > _currentDocument.RoomWidth)
            {
                nextX = _currentDocument.RoomWidth - cabinetWidth;
            }

            return nextX;
        }

        private void btnYeni_ItemClick(object sender, ItemClickEventArgs e)
        {
            // Kullanıcıya ufak bir teyit sorusu soralım ki yanlışlıkla tıklayıp çizimini kaybetmesin
            var result = MessageBox.Show("Mevcut projeyi kapatıp yepyeni bir projeye başlamak istiyor musunuz? (Kaydetmediğiniz değişiklikler silinir)", "Yeni Proje", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                // Varsa arkada açık kalan 3D penceresini temizle
                if (_active3DViewer != null && !_active3DViewer.IsDisposed)
                    _active3DViewer.Close();

                // Hafızaya yepyeni, sıfır bir belge yükle
                _currentDocument = new DrawingDocument();
                _canvas.Document = _currentDocument;

                // Ekranı, seçimi ve ağacı (TreeList) sıfırla
                _canvas.SetSelectedCabinet(null);
                _canvas.Invalidate();
                RefreshTreeList();

                // Sağ panele yeni projenin bilgilerini getir
                propertyGridControl1.SelectedObject = _currentDocument;
                propertyGridControl1.RetrieveFields();
                propertyGridControl1.Refresh();
            }
        }

        private void btnUretimRaporu_ItemClick(object sender, ItemClickEventArgs e)
        {
            // Projede dolap var mı kontrol et
            if (_currentDocument == null || _currentDocument.Cabinets.Count == 0)
            {
                MessageBox.Show("Rapor alınacak hiçbir dolap yok! Lütfen önce projeye dolap ekleyin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Seçili tek dolabı değil, tüm dokümanı (projeyi) rapora gönder!
            CutListForm reportForm = new CutListForm(_currentDocument);
            reportForm.ShowDialog();
        }

        private void btnMezura_ItemClick(object sender, ItemClickEventArgs e)
        {
            // Eğer buton Check tipindeyse (basılı kalıyorsa) durumunu Canvas'a aktar
            if (e.Item is DevExpress.XtraBars.BarButtonItem btn)
            {
                _canvas.IsMeasuringMode = btn.Down;

                if (_canvas.IsMeasuringMode)
                {
                    _canvas.SetSelectedCabinet(null); // Modu açınca dolap seçimini bırak
                    _canvas.Cursor = Cursors.Cross; // İmleci artı yap
                }
                else
                {
                    _canvas.Cursor = Cursors.Default; // Modu kapatınca imleci düzelt
                }
                _canvas.Invalidate();
            }
        }

        private void btnOlcuTemizle_ItemClick(object sender, ItemClickEventArgs e)
        {
            _canvas.ClearMeasurements();
        }

        private void btnBantAyarlari_ItemClick(object sender, ItemClickEventArgs e)
        {
            // Önce neyi düzenleyeceğini soralım (Alt Dolaplar mı Üst Dolaplar mı?)
            var result = MessageBox.Show("Hangi grup için varsayılan bant ayarlarını düzenlemek istersiniz?\n\nEVET: Alt Dolaplar\nHAYIR: Üst Dolaplar", "Bant Ayarları", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

            if (result == DialogResult.Cancel) return;

            var scheme = (result == DialogResult.Yes) ? AppGlobalSettings.Current.BaseCabinetBanding : AppGlobalSettings.Current.WallCabinetBanding;
            string title = (result == DialogResult.Yes) ? "Alt Dolap Bant Şablonu" : "Üst Dolap Bant Şablonu";

            using (var editor = new BandingEditorForm(scheme, title))
            {
                if (editor.ShowDialog() == DialogResult.OK)
                {
                    AppGlobalSettings.Current.Save(); // Ayarları kalıcı hafızaya kaydet
                    MessageBox.Show("Bant ayarları kaydedildi! Bir sonraki raporda bu ayarlar geçerli olacak.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void btnKapamaEkle_ItemClick(object sender, ItemClickEventArgs e)
        {
            var s = AppGlobalSettings.Current;
            var cab = new CabinetEntity
            {
                Name = "Yeni Kapama / Kör Panel",
                Type = CabinetType.Kapama,
                TotalWidth = 100, // Standart kapama eni
                TotalHeight = s.DefaultBaseHeight + s.DefaultBasePlinth, // Baza dahil tam boy
                TotalDepth = s.DefaultBaseDepth,
                PlinthHeight = 0,
                CrownHeight = 0
            };

            cab.RebuildParts();
            double startX = GetSmartPlacementX(cab.TotalWidth, false);
            cab.Position = new Core.Geometry.Point2D(startX, _currentDocument.RoomHeight - cab.TotalHeight);

            _currentDocument.AddCabinet(cab);
            _canvas.Invalidate();
            RefreshTreeList();
        }

        private void btnOdaAyarlari_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (_currentDocument == null) return;

            using (var roomBuilder = new RoomBuilderForm(_currentDocument.Room))
            {
                if (roomBuilder.ShowDialog() == DialogResult.OK)
                {
                    _currentDocument.Room = roomBuilder.RoomData;
                    // Duvarlar büyüyüp küçülmüş olabilir, ekrana yansıt
                    _canvas.Invalidate();
                    MessageBox.Show("Mekan ölçüleri başarıyla güncellendi. Dolaplarınızı köşe çizgilerine göre hizalayabilirsiniz.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
    }
}