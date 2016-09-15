using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Prueba_de_stream
{
    public class ContextSurf : INotifyPropertyChanged
    {
        public int Hue1
        {
            get
            {
                return _hue1;
            }
            set
            {
                _hue1 = value;
                NotifyPropertyChanged();
            }
        }
        public int Hue2
        {
            get
            {
                return _hue2;
            }
            set
            {
                _hue2 = value;
                NotifyPropertyChanged();
            }
        }
        private int _hue1 = 63;
        private int _hue2 = 26;
        public int MaxHue = 128;

        public int Sat1
        {
            get
            {
                return _sat1;
            }
            set
            {
                _sat1 = value;
                NotifyPropertyChanged();
            }
        }
        public int Sat2
        {
            get
            {
                return _sat2;
            }
            set
            {
                _sat2 = value;
                NotifyPropertyChanged();
            }
        }
        private int _sat1 = 42;
        private int _sat2 = 46;
        public int MaxSat = 128;

        public int Brig1
        {
            get
            {
                return _brig1;
            }
            set
            {
                _brig1 = value;
                NotifyPropertyChanged();
            }
        }
        public int Brig2
        {
            get
            {
                return _brig2;
            }
            set
            {
                _brig2 = value;
                NotifyPropertyChanged();
            }
        }
        private int _brig1 = 45;
        private int _brig2 = 0;
        public int MaxBrig = 255;

        public int Erode1
        {
            get
            {
                return _erode1;
            }
            set
            {
                _erode1 = value;
                NotifyPropertyChanged();
            }
        }
        public int Erode2
        {
            get
            {
                return _erode2;
            }
            set
            {
                _erode2 = value;
                NotifyPropertyChanged();
            }
        }
        private int _erode1 = 4;
        private int _erode2 = 2;

        public int Dilate1
        {
            get
            {
                return _dilate1;
            }
            set
            {
                _dilate1 = value;
                NotifyPropertyChanged();
            }
        }
        public int Dilate2
        {
            get
            {
                return _dilate2;
            }
            set
            {
                _dilate2 = value;
                NotifyPropertyChanged();
            }
        }
        private int _dilate1 = 6;
        private int _dilate2 = 8;

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
