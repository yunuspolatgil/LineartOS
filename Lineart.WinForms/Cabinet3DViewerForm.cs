using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Lineart.Core.Entities;

namespace Lineart.WinForms
{
    public partial class Cabinet3DViewerForm : Form
    {
        private CabinetEntity _cabinet;
        private WebView2 _webView;
        private bool _isWebViewReady = false;

        public Cabinet3DViewerForm(CabinetEntity cabinet)
        {
            _cabinet = cabinet;

            this.Text = $"{cabinet.Name} - Profesyonel 3D Görüntüleyici";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            _webView = new WebView2();
            _webView.Dock = DockStyle.Fill;
            this.Controls.Add(_webView);

            InitializeWebView();
        }

        public void Refresh3DView()
        {
            if (_isWebViewReady && _cabinet != null)
            {
                SendCabinetDataToWeb();
            }
        }

        private async void InitializeWebView()
        {
            await _webView.EnsureCoreWebView2Async(null);

            string htmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "3DViewer.html");

            if (File.Exists(htmlPath))
            {
                _webView.CoreWebView2.Navigate(htmlPath);
                _webView.CoreWebView2.NavigationCompleted += (s, e) =>
                {
                    _isWebViewReady = true;
                    SendCabinetDataToWeb();
                };
            }
            else
            {
                MessageBox.Show("3DViewer.html dosyası bulunamadı! Lütfen bin/Debug klasörüne koyun.");
            }
        }

        private void SendCabinetDataToWeb()
        {
            if (!_isWebViewReady || _cabinet == null) return;

            var exportList = new List<object>();

            float t = (float)_cabinet.MaterialThickness;
            float cabDepth = (float)_cabinet.TotalDepth;

            foreach (var part in _cabinet.Parts)
            {
                float px = part.FrontViewBounds.X;
                float py = part.FrontViewBounds.Y;
                float pw = part.FrontViewBounds.Width;
                float ph = part.FrontViewBounds.Height;
                float pz = 0;
                float pd = cabDepth;

                string colorHex = "#C39B73";
                bool isGlass = false;

                // --- YENİ EKLENEN: KAPAMA (FILLER) 3D ALGISI ---
                if (part.Name.Contains("KAPAMA") || part.Name.Contains("BİTİŞ") || part.Name.Contains("DÖNÜŞ"))
                {
                    if (part.Name.Contains("DÖNÜŞ"))
                    {
                        pz = 0;
                        pd = (float)_cabinet.FillerReturnDepth;
                        colorHex = "#E8E8E8"; // Dönüş içte kalır, gövde malzemedir
                    }
                    else if (part.Name == "YAN BİTİŞ PANELİ")
                    {
                        pz = 0;
                        pd = cabDepth;
                        colorHex = _cabinet.FillerMaterial == FillerMaterialGroup.Kapak ? "#3b82f6" : "#C39B73";
                        isGlass = _cabinet.FillerMaterial == FillerMaterialGroup.Kapak;
                    }
                    else
                    { // Düz Ön/Yatay Kapamalar
                        pz = _cabinet.FillerMaterial == FillerMaterialGroup.Kapak ? -t : 0; // Kapak malzemesiyse kapak gibi öne fırla
                        pd = t;
                        colorHex = _cabinet.FillerMaterial == FillerMaterialGroup.Kapak ? "#3b82f6" : "#C39B73";
                        isGlass = _cabinet.FillerMaterial == FillerMaterialGroup.Kapak;
                    }
                }
                else
                {
                    // STANDART DOLAP PARÇALARI
                    switch (part.Type)
                    {
                        case PartType.Gövde:
                            if (part.Name.Contains("Ön Üst Kayıt"))
                            {
                                pz = 0;
                                pd = part.Name.Contains("(Dik)") ? t : (float)_cabinet.TopStripWidth;
                            }
                            else if (part.Name.Contains("Arka Üst Kayıt") || part.Name.Contains("Arka Alt Kayıt"))
                            {
                                pd = part.Name.Contains("(Dik)") ? t : (float)_cabinet.TopStripWidth;
                                pz = cabDepth - (_cabinet.BackType == BackPanelType.Kanalli ? 18f : 0f) - pd;
                            }
                            else if (part.Name.Contains("Çatkı"))
                            {
                                pz = 0;
                                pd = (float)_cabinet.TopStripWidth;
                            }
                            break;
                        case PartType.Arkalik:
                            pd = 8;
                            pz = _cabinet.BackType == BackPanelType.Kanalli ? (cabDepth - 18f - pd) : cabDepth;
                            colorHex = "#E8E8E8";
                            break;
                        case PartType.Baza:
                            pz = 20; pd = t;
                            colorHex = "#826446";
                            break;

                        case PartType.Raf:
                            if (part.Name == "SABİT RAF")
                            {
                                pz = 0;
                                pd = _cabinet.BackType == BackPanelType.Kanalli ? (cabDepth - 18f) : cabDepth;
                                colorHex = "#D2B48C";
                            }
                            else
                            {
                                pz = 20;
                                pd = cabDepth - 40;
                                colorHex = "#90EE90";
                            }
                            break;

                        case PartType.Kapak:
                        case PartType.Cekmece:
                            pz = -t; pd = t;
                            colorHex = "#3b82f6";
                            isGlass = true;
                            break;
                        case PartType.Ankastre:
                            pz = 20; pd = cabDepth - 40;
                            colorHex = "#282828";
                            break;
                    }
                }

                exportList.Add(new
                {
                    Name = part.Name,
                    X = px,
                    Y = py,
                    Z = pz,
                    W = pw,
                    H = ph,
                    D = pd,
                    Color = colorHex,
                    IsGlass = isGlass
                });
            }

            string jsonString = JsonSerializer.Serialize(exportList);
            _webView.CoreWebView2.ExecuteScriptAsync($"window.loadCabinetFromCSharp('{jsonString}')");
        }
    }
}