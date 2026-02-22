using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Lineart.Core.Entities
{
    public enum ModuleType { Kapak, Cekmece, Ankastre, AcikRaf }

    // INotifyPropertyChanged: Özellik değiştiğinde arayüzü anında uyaran sihirli altyapı
    public class CabinetModule : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private ModuleType _type = ModuleType.Kapak;
        [DisplayName("1. Modül Tipi")]
        public ModuleType Type
        {
            get => _type;
            set { _type = value; NotifyChanged(); }
        }

        private double _heightMm = 0;
        [DisplayName("2. Yükseklik (mm)")]
        [Description("Eğer 0 (sıfır) girerseniz, dolabın kalan boşluğunu otomatik doldurur.")]
        public double HeightMm
        {
            get => _heightMm;
            set { _heightMm = value; NotifyChanged(); }
        }

        private int _subItemCount = 1;
        [DisplayName("3. Alt Bölme Sayısı")]
        [Description("Kapak ise yan yana kaç kapak? Çekmece ise üst üste kaç çekmece?")]
        public int SubItemCount
        {
            get => _subItemCount;
            set { _subItemCount = value; NotifyChanged(); }
        }

        // Collection penceresinde güzel görünmesi için
        public override string ToString()
        {
            string h = HeightMm > 0 ? $"{HeightMm} mm" : "Otomatik Boy";
            return $"{Type} Modülü ({SubItemCount} Adet) - [{h}]";
        }
    }
}