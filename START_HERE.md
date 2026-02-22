# LineartOS — Sıfırdan Başlangıç Rehberi (.NET 8 + WinForms + DevExpress)

Bu rehber, hiç bilmeyen biri için **adım adım** ilerleyecek şekilde hazırlandı.

## Hemen Başla (Codex'i hiç bilmeyen için)

Eğer "ben şimdi ne yapacağım" diyorsan sadece bunu uygula:

1. Codex'e şu mesajı gönder:

   ```text
   Aşama-1'i birlikte yapalım. Bana tek tek komut ver, ben çalıştırıp sonucu sana yazayım.
   Her adımda sadece 1 komut ver.
   ```

2. Codex'in verdiği ilk komutu terminalde çalıştır.
3. Çıkan sonucu kopyala ve tekrar Codex'e gönder.
4. Bir sonraki komutu iste.

> Kural: **Tek seferde tek komut**. Böylece hata olursa hemen düzeltiriz.

Örnek ilerleme şekli:
- Sen: "Hazırım"
- Codex: "`dotnet --version` çalıştır"
- Sen: "çıktı: 8.0.xx"
- Codex: "harika, şimdi şu komutu çalıştır..."

---

## 0) Hedefimiz

Bu projede aşağıdaki özelliklere giden bir temel kuracağız:
- 2D teknik çizim ekranı
- Snap (köşe/yüzey), zoom/pan, seçim
- Ölçülendirme (mm)
- Cut list (en, boy, adet, bant)
- PDF çıktı

> Not: İlk adımda "mükemmel program" yapmıyoruz. Önce iskelet, sonra özellik.

---

## 1) Proje yaklaşımı (çok önemli)

Tek parça uygulama yerine 3 katman kuracağız:

1. `Lineart.Core`  
   Geometri, ölçü, snap, cut list gibi saf iş kuralları.
2. `Lineart.Application`  
   Araçlar (line tool, rectangle tool), komutlar (undo/redo), use-case akışı.
3. `Lineart.WinForms`  
   DevExpress ekranı, ribbon, paneller, canvas.

Bu sayede ekran değişse bile çekirdek bozulmaz.

---

## 2) Bugün yapacağımız ilk hedef (Aşama-1)

Aşağıdaki minimum çıktıyı alacağız:
- Boş bir çözüm (solution)
- 3 proje (Core/Application/WinForms)
- Core içinde ilk domain sınıfları
- WinForms içinde açılan ana pencere

Bu aşamayı bitirince bir sonraki adımda çizim yüzeyini ekleyeceğiz.

---

## 3) Kurulum Ön Koşulları

- Visual Studio 2022 (17.8+ önerilir)
- .NET 8 SDK (tercih)
- Eğer ortam mecburi ise .NET 7 ile de başlayabiliriz
- DevExpress WinForms bileşenleri kurulu olmalı

Terminalden kontrol:

```bash
dotnet --version
```

7.x, 8.x veya 9.x görebilirsin. Önemli olan projeyi `net7.0` hedefiyle oluşturmak.

---

## 3.1) Senin ortama özel not (ÖNEMLİ)

Senin verdiğin bilgiye göre:
- SDK: `9.0.311`
- Proje hedefi: **mecburen .NET 7**

Bu durumda sorun yok. `dotnet` SDK 9 yüklü olsa da projeyi `net7.0` hedefiyle oluşturabiliriz.

---

## 4) Solution oluşturma komutları

Proje kökünde şu komutları çalıştır:

```bash
dotnet new sln -n LineartOS

dotnet new classlib -n Lineart.Core -f net7.0
dotnet new classlib -n Lineart.Application -f net7.0
dotnet new winforms -n Lineart.WinForms -f net7.0-windows

dotnet sln LineartOS.sln add Lineart.Core/Lineart.Core.csproj
dotnet sln LineartOS.sln add Lineart.Application/Lineart.Application.csproj
dotnet sln LineartOS.sln add Lineart.WinForms/Lineart.WinForms.csproj

dotnet add Lineart.Application/Lineart.Application.csproj reference Lineart.Core/Lineart.Core.csproj
dotnet add Lineart.WinForms/Lineart.WinForms.csproj reference Lineart.Application/Lineart.Application.csproj
dotnet add Lineart.WinForms/Lineart.WinForms.csproj reference Lineart.Core/Lineart.Core.csproj
```

---

## 5) Core içine ilk sınıflar

`Lineart.Core` içinde şu dosyaları ekle:

### `Geometry/Point2D.cs`

```csharp
namespace Lineart.Core.Geometry;

public readonly record struct Point2D(double X, double Y);
```

### `Entities/PartEntity.cs`

```csharp
using Lineart.Core.Geometry;

namespace Lineart.Core.Entities;

public sealed class PartEntity
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = "Yeni Parça";
    public double WidthMm { get; set; }
    public double HeightMm { get; set; }
    public int BandEdgeCount { get; set; }

    public Point2D Origin { get; set; } = new(0, 0);
}
```

### `Document/DrawingDocument.cs`

```csharp
using Lineart.Core.Entities;

namespace Lineart.Core.Document;

public sealed class DrawingDocument
{
    public string ProjectName { get; set; } = "Yeni Proje";
    public List<PartEntity> Parts { get; } = new();
}
```

> Not (.NET 7/C# 11): `[]` yazımı yerine `new()` kullanıyoruz. `[]` C# 12 özelliğidir ve .NET 7 başlangıcında hata verir.

---

## 6) Application içine temel servis

### `Services/CutListService.cs`

```csharp
using Lineart.Core.Document;

namespace Lineart.Application.Services;

public sealed class CutListRow
{
    public double WidthMm { get; init; }
    public double HeightMm { get; init; }
    public int BandEdgeCount { get; init; }
    public int Quantity { get; init; }
}

public sealed class CutListService
{
    public IReadOnlyList<CutListRow> Build(DrawingDocument doc)
    {
        return doc.Parts
            .GroupBy(p => new { p.WidthMm, p.HeightMm, p.BandEdgeCount })
            .Select(g => new CutListRow
            {
                WidthMm = g.Key.WidthMm,
                HeightMm = g.Key.HeightMm,
                BandEdgeCount = g.Key.BandEdgeCount,
                Quantity = g.Count()
            })
            .OrderByDescending(x => x.Quantity)
            .ToList();
    }
}
```

---

## 7) WinForms tarafı ilk açılış

İlk aşamada sade bir ana form yeterli:
- Üstte Ribbon (şimdilik boş olabilir)
- Ortada panel/canvas alanı (placeholder)
- Sağda özellik paneli için boş alan

> DevExpress kontrollerini ikinci aşamada birlikte bağlayacağız.

---

## 8) İlk doğrulama

Terminalde:

```bash
dotnet build LineartOS.sln
```

Sonra WinForms projesini çalıştır:

```bash
dotnet run --project Lineart.WinForms/Lineart.WinForms.csproj
```

Pencere açılıyorsa Aşama-1 tamam.

---

## 9) Bir sonraki adım (Aşama-2 planı)

> Senin ortam için hedef framework: `net7.0`

Bir sonraki mesajda şunları birlikte yapacağız:
1. Özel `DrawingCanvasControl` (double-buffering)
2. Mouse ile pan/zoom
3. İlk şekil çizimi (dikdörtgen)
4. Seçim ve hit-test başlangıcı

---

## 10) Çalışma kuralımız (senden istediğim)

Her adımda bana şu formatta dön:
1. Ne yaptın?
2. Hangi hata çıktı?
3. Hangi komutu çalıştırdın?
4. Ekranda ne gördün?

Bu şekilde hızlı ve sorunsuz ilerleriz.
