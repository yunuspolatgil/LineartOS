namespace Lineart.WinForms
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            ribbonControl1 = new DevExpress.XtraBars.Ribbon.RibbonControl();
            barButtonItem1 = new DevExpress.XtraBars.BarButtonItem();
            btnProjeAyarlari = new DevExpress.XtraBars.BarButtonItem();
            btnKaydet = new DevExpress.XtraBars.BarButtonItem();
            btnAc = new DevExpress.XtraBars.BarButtonItem();
            btnUstDolapEkle = new DevExpress.XtraBars.BarButtonItem();
            btnYeni = new DevExpress.XtraBars.BarButtonItem();
            btnUretimRaporu = new DevExpress.XtraBars.BarButtonItem();
            btnMezura = new DevExpress.XtraBars.BarButtonItem();
            btnOlcuTemizle = new DevExpress.XtraBars.BarButtonItem();
            btnBantAyarlari = new DevExpress.XtraBars.BarButtonItem();
            btnKapamaEkle = new DevExpress.XtraBars.BarButtonItem();
            ribbonPage1 = new DevExpress.XtraBars.Ribbon.RibbonPage();
            ribbonPageGroup1 = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            ribbonPageGroup2 = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            Raporlar = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            ribbonPageGroup4 = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            ribbonPageGroup3 = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            propertyGridControl1 = new DevExpress.XtraVerticalGrid.PropertyGridControl();
            treeListProject = new DevExpress.XtraTreeList.TreeList();
            btnOdaAyarlari = new DevExpress.XtraBars.BarButtonItem();
            ((System.ComponentModel.ISupportInitialize)ribbonControl1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)propertyGridControl1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)treeListProject).BeginInit();
            SuspendLayout();
            // 
            // ribbonControl1
            // 
            ribbonControl1.Cursor = System.Windows.Forms.Cursors.Hand;
            ribbonControl1.ExpandCollapseItem.Id = 0;
            ribbonControl1.Items.AddRange(new DevExpress.XtraBars.BarItem[] { ribbonControl1.ExpandCollapseItem, ribbonControl1.SearchEditItem, barButtonItem1, btnProjeAyarlari, btnKaydet, btnAc, btnUstDolapEkle, btnYeni, btnUretimRaporu, btnMezura, btnOlcuTemizle, btnBantAyarlari, btnKapamaEkle, btnOdaAyarlari });
            ribbonControl1.Location = new System.Drawing.Point(0, 0);
            ribbonControl1.MaxItemId = 13;
            ribbonControl1.Name = "ribbonControl1";
            ribbonControl1.Pages.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPage[] { ribbonPage1 });
            ribbonControl1.Size = new System.Drawing.Size(1195, 144);
            // 
            // barButtonItem1
            // 
            barButtonItem1.Caption = "Alt Dolap ekle";
            barButtonItem1.Id = 1;
            barButtonItem1.ImageOptions.Image = (System.Drawing.Image)resources.GetObject("barButtonItem1.ImageOptions.Image");
            barButtonItem1.ImageOptions.LargeImage = (System.Drawing.Image)resources.GetObject("barButtonItem1.ImageOptions.LargeImage");
            barButtonItem1.LargeWidth = 80;
            barButtonItem1.Name = "barButtonItem1";
            barButtonItem1.ItemClick += btnAltDolapEkle_ItemClick;
            // 
            // btnProjeAyarlari
            // 
            btnProjeAyarlari.Caption = "Proje Ayarları";
            btnProjeAyarlari.Id = 2;
            btnProjeAyarlari.ImageOptions.Image = (System.Drawing.Image)resources.GetObject("btnProjeAyarlari.ImageOptions.Image");
            btnProjeAyarlari.ImageOptions.LargeImage = (System.Drawing.Image)resources.GetObject("btnProjeAyarlari.ImageOptions.LargeImage");
            btnProjeAyarlari.LargeWidth = 80;
            btnProjeAyarlari.Name = "btnProjeAyarlari";
            btnProjeAyarlari.ItemClick += btnProjeAyarlari_ItemClick;
            // 
            // btnKaydet
            // 
            btnKaydet.Caption = "Ayar Kaydet";
            btnKaydet.Id = 3;
            btnKaydet.ImageOptions.Image = (System.Drawing.Image)resources.GetObject("btnKaydet.ImageOptions.Image");
            btnKaydet.ImageOptions.LargeImage = (System.Drawing.Image)resources.GetObject("btnKaydet.ImageOptions.LargeImage");
            btnKaydet.Name = "btnKaydet";
            btnKaydet.ItemClick += btnKaydet_ItemClick;
            // 
            // btnAc
            // 
            btnAc.Caption = "Proje Aç";
            btnAc.Id = 4;
            btnAc.ImageOptions.Image = (System.Drawing.Image)resources.GetObject("btnAc.ImageOptions.Image");
            btnAc.ImageOptions.LargeImage = (System.Drawing.Image)resources.GetObject("btnAc.ImageOptions.LargeImage");
            btnAc.LargeWidth = 80;
            btnAc.Name = "btnAc";
            btnAc.ItemClick += btnAc_ItemClick;
            // 
            // btnUstDolapEkle
            // 
            btnUstDolapEkle.Caption = "Üst Dolap Ekle";
            btnUstDolapEkle.Id = 5;
            btnUstDolapEkle.ImageOptions.Image = (System.Drawing.Image)resources.GetObject("btnUstDolapEkle.ImageOptions.Image");
            btnUstDolapEkle.ImageOptions.LargeImage = (System.Drawing.Image)resources.GetObject("btnUstDolapEkle.ImageOptions.LargeImage");
            btnUstDolapEkle.LargeWidth = 80;
            btnUstDolapEkle.Name = "btnUstDolapEkle";
            btnUstDolapEkle.ItemClick += btnUstDolapEkle_ItemClick;
            // 
            // btnYeni
            // 
            btnYeni.Caption = "Yeni Proje";
            btnYeni.Id = 6;
            btnYeni.ImageOptions.Image = (System.Drawing.Image)resources.GetObject("btnYeni.ImageOptions.Image");
            btnYeni.ImageOptions.LargeImage = (System.Drawing.Image)resources.GetObject("btnYeni.ImageOptions.LargeImage");
            btnYeni.Name = "btnYeni";
            btnYeni.ItemClick += btnYeni_ItemClick;
            // 
            // btnUretimRaporu
            // 
            btnUretimRaporu.Caption = "Üretim Raporu";
            btnUretimRaporu.Id = 7;
            btnUretimRaporu.ImageOptions.Image = (System.Drawing.Image)resources.GetObject("btnUretimRaporu.ImageOptions.Image");
            btnUretimRaporu.ImageOptions.LargeImage = (System.Drawing.Image)resources.GetObject("btnUretimRaporu.ImageOptions.LargeImage");
            btnUretimRaporu.LargeWidth = 80;
            btnUretimRaporu.Name = "btnUretimRaporu";
            btnUretimRaporu.ItemClick += btnUretimRaporu_ItemClick;
            // 
            // btnMezura
            // 
            btnMezura.ButtonStyle = DevExpress.XtraBars.BarButtonStyle.Check;
            btnMezura.Caption = "Ölçü Al";
            btnMezura.Id = 8;
            btnMezura.ImageOptions.Image = (System.Drawing.Image)resources.GetObject("btnMezura.ImageOptions.Image");
            btnMezura.ImageOptions.LargeImage = (System.Drawing.Image)resources.GetObject("btnMezura.ImageOptions.LargeImage");
            btnMezura.LargeWidth = 80;
            btnMezura.Name = "btnMezura";
            btnMezura.ItemClick += btnMezura_ItemClick;
            // 
            // btnOlcuTemizle
            // 
            btnOlcuTemizle.Caption = "Ölçümleri Temizle";
            btnOlcuTemizle.Id = 9;
            btnOlcuTemizle.ImageOptions.Image = (System.Drawing.Image)resources.GetObject("btnOlcuTemizle.ImageOptions.Image");
            btnOlcuTemizle.ImageOptions.LargeImage = (System.Drawing.Image)resources.GetObject("btnOlcuTemizle.ImageOptions.LargeImage");
            btnOlcuTemizle.LargeWidth = 80;
            btnOlcuTemizle.Name = "btnOlcuTemizle";
            btnOlcuTemizle.ItemClick += btnOlcuTemizle_ItemClick;
            // 
            // btnBantAyarlari
            // 
            btnBantAyarlari.Caption = "PVC Bant Ayarları";
            btnBantAyarlari.Id = 10;
            btnBantAyarlari.ImageOptions.Image = (System.Drawing.Image)resources.GetObject("btnBantAyarlari.ImageOptions.Image");
            btnBantAyarlari.ImageOptions.LargeImage = (System.Drawing.Image)resources.GetObject("btnBantAyarlari.ImageOptions.LargeImage");
            btnBantAyarlari.LargeWidth = 90;
            btnBantAyarlari.Name = "btnBantAyarlari";
            btnBantAyarlari.ItemClick += btnBantAyarlari_ItemClick;
            // 
            // btnKapamaEkle
            // 
            btnKapamaEkle.Caption = "Kapama Ekle";
            btnKapamaEkle.Id = 11;
            btnKapamaEkle.ImageOptions.Image = (System.Drawing.Image)resources.GetObject("btnKapamaEkle.ImageOptions.Image");
            btnKapamaEkle.ImageOptions.LargeImage = (System.Drawing.Image)resources.GetObject("btnKapamaEkle.ImageOptions.LargeImage");
            btnKapamaEkle.LargeWidth = 80;
            btnKapamaEkle.Name = "btnKapamaEkle";
            btnKapamaEkle.ItemClick += btnKapamaEkle_ItemClick;
            // 
            // ribbonPage1
            // 
            ribbonPage1.Groups.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPageGroup[] { ribbonPageGroup1, ribbonPageGroup2, Raporlar, ribbonPageGroup4, ribbonPageGroup3 });
            ribbonPage1.Name = "ribbonPage1";
            ribbonPage1.Text = "ribbonPage1";
            // 
            // ribbonPageGroup1
            // 
            ribbonPageGroup1.ItemLinks.Add(barButtonItem1);
            ribbonPageGroup1.ItemLinks.Add(btnUstDolapEkle);
            ribbonPageGroup1.ItemLinks.Add(btnKapamaEkle);
            ribbonPageGroup1.Name = "ribbonPageGroup1";
            ribbonPageGroup1.Text = "ribbonPageGroup1";
            // 
            // ribbonPageGroup2
            // 
            ribbonPageGroup2.Alignment = DevExpress.XtraBars.Ribbon.RibbonPageGroupAlignment.Far;
            ribbonPageGroup2.ItemLinks.Add(btnYeni);
            ribbonPageGroup2.ItemLinks.Add(btnProjeAyarlari);
            ribbonPageGroup2.ItemLinks.Add(btnKaydet);
            ribbonPageGroup2.ItemLinks.Add(btnAc);
            ribbonPageGroup2.ItemLinks.Add(btnOdaAyarlari);
            ribbonPageGroup2.Name = "ribbonPageGroup2";
            ribbonPageGroup2.Text = "Proje Yönetim";
            // 
            // Raporlar
            // 
            Raporlar.Alignment = DevExpress.XtraBars.Ribbon.RibbonPageGroupAlignment.Far;
            Raporlar.ItemLinks.Add(btnUretimRaporu);
            Raporlar.Name = "Raporlar";
            Raporlar.Text = "Raporlar";
            // 
            // ribbonPageGroup4
            // 
            ribbonPageGroup4.ItemLinks.Add(btnMezura);
            ribbonPageGroup4.ItemLinks.Add(btnOlcuTemizle);
            ribbonPageGroup4.Name = "ribbonPageGroup4";
            ribbonPageGroup4.Text = "ribbonPageGroup4";
            // 
            // ribbonPageGroup3
            // 
            ribbonPageGroup3.ItemLinks.Add(btnBantAyarlari);
            ribbonPageGroup3.Name = "ribbonPageGroup3";
            ribbonPageGroup3.Text = "ribbonPageGroup3";
            // 
            // propertyGridControl1
            // 
            propertyGridControl1.ActiveViewType = DevExpress.XtraVerticalGrid.PropertyGridView.Office;
            propertyGridControl1.AutoGenerateRows = false;
            propertyGridControl1.Cursor = System.Windows.Forms.Cursors.Hand;
            propertyGridControl1.Dock = System.Windows.Forms.DockStyle.Left;
            propertyGridControl1.Location = new System.Drawing.Point(0, 144);
            propertyGridControl1.MenuManager = ribbonControl1;
            propertyGridControl1.Name = "propertyGridControl1";
            propertyGridControl1.OptionsView.AllowReadOnlyRowAppearance = DevExpress.Utils.DefaultBoolean.True;
            propertyGridControl1.Size = new System.Drawing.Size(305, 630);
            propertyGridControl1.TabIndex = 3;
            // 
            // treeListProject
            // 
            treeListProject.Dock = System.Windows.Forms.DockStyle.Right;
            treeListProject.Location = new System.Drawing.Point(889, 144);
            treeListProject.MenuManager = ribbonControl1;
            treeListProject.Name = "treeListProject";
            treeListProject.Size = new System.Drawing.Size(306, 630);
            treeListProject.TabIndex = 4;
            // 
            // btnOdaAyarlari
            // 
            btnOdaAyarlari.Caption = "Oda Oluştur";
            btnOdaAyarlari.Id = 12;
            btnOdaAyarlari.ImageOptions.Image = (System.Drawing.Image)resources.GetObject("btnOdaAyarlari.ImageOptions.Image");
            btnOdaAyarlari.ImageOptions.LargeImage = (System.Drawing.Image)resources.GetObject("btnOdaAyarlari.ImageOptions.LargeImage");
            btnOdaAyarlari.Name = "btnOdaAyarlari";
            btnOdaAyarlari.ItemClick += btnOdaAyarlari_ItemClick;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1195, 774);
            Controls.Add(treeListProject);
            Controls.Add(propertyGridControl1);
            Controls.Add(ribbonControl1);
            Name = "MainForm";
            Ribbon = ribbonControl1;
            Text = "Form1";
            WindowState = System.Windows.Forms.FormWindowState.Maximized;
            Load += MainForm_Load;
            ((System.ComponentModel.ISupportInitialize)ribbonControl1).EndInit();
            ((System.ComponentModel.ISupportInitialize)propertyGridControl1).EndInit();
            ((System.ComponentModel.ISupportInitialize)treeListProject).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private DevExpress.XtraBars.Ribbon.RibbonControl ribbonControl1;
        private DevExpress.XtraBars.Ribbon.RibbonPage ribbonPage1;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup ribbonPageGroup1;
        private DevExpress.XtraVerticalGrid.PropertyGridControl propertyGridControl1;
        private DevExpress.XtraTreeList.TreeList treeListProject;
        private DevExpress.XtraBars.BarButtonItem barButtonItem1;
        private DevExpress.XtraBars.BarButtonItem btnProjeAyarlari;
        private DevExpress.XtraBars.BarButtonItem btnKaydet;
        private DevExpress.XtraBars.BarButtonItem btnAc;
        private DevExpress.XtraBars.BarButtonItem btnUstDolapEkle;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup ribbonPageGroup2;
        private DevExpress.XtraBars.BarButtonItem btnYeni;
        private DevExpress.XtraBars.BarButtonItem btnUretimRaporu;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup Raporlar;
        private DevExpress.XtraBars.BarButtonItem btnMezura;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup ribbonPageGroup4;
        private DevExpress.XtraBars.BarButtonItem btnOlcuTemizle;
        private DevExpress.XtraBars.BarButtonItem btnBantAyarlari;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup ribbonPageGroup3;
        private DevExpress.XtraBars.BarButtonItem btnKapamaEkle;
        private DevExpress.XtraBars.BarButtonItem btnOdaAyarlari;
    }
}

