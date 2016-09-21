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

        public int NoiseBG
        {
            get
            {
                return _noiseBG;
            }
            set
            {
                _noiseBG = value;
                NotifyPropertyChanged();
            }
        }

        public int ClarifyBG
        {
            get
            {
                return _clarifyBG;
            }
            set
            {
                _clarifyBG = value;
                NotifyPropertyChanged();
            }
        }

        public int ErodeBG
        {
            get
            {
                return _erodeBG;
            }
            set
            {
                _erodeBG = value;
                NotifyPropertyChanged();
            }
        }

        public int DilateBG
        {
            get
            {
                return _dilateBG;
            }
            set
            {
                _dilateBG = value;
                NotifyPropertyChanged();
            }
        }

        public int MinColorHSV
        {
            get
            {
                return _minColorHSV;
            }
            set
            {
                _minColorHSV = value;
                NotifyPropertyChanged();
            }
        }

        public int MaxColorHSV
        {
            get
            {
                return _maxColorHSV;
            }
            set
            {
                _maxColorHSV = value;
                NotifyPropertyChanged();
            }
        }

        public int MinYCCValue => MIN_YCC_VALUE;
        public int MaxYCCValue => MAX_YCC_VALUE;
        public int MinMorphologyValue => MIN_MORPHOLOGY_VALUE;
        public int MaxMorphologyValue => MAX_MORPHOLOGY_VALUE;
        public int MinHSVValue => MIN_HSV_VALUE;
        public int MaxHSVValue => MAX_HSV_VALUE;

        private int _noiseBG = 38;
        private int _clarifyBG = 38;
        private int _erodeBG = 10;
        private int _dilateBG = 7;
        private int _minColorHSV = 20;
        private int _maxColorHSV = 150;

        private const int MIN_YCC_VALUE = 0;
        private const int MAX_YCC_VALUE = 128;
        private const int MIN_MORPHOLOGY_VALUE = 2;
        private const int MAX_MORPHOLOGY_VALUE = 20;
        private const int MIN_HSV_VALUE = 0;
        private const int MAX_HSV_VALUE = 359;


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
