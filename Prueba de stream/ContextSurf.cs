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

        public double GaussianBlurVal
        {
            get
            {
                return _gaussianBlurVal;
            }
            set
            {
                _gaussianBlurVal = value;
                NotifyPropertyChanged();
            }
        }

        public int MinHueForHSV
        {
            get
            {
                return _minHueForHSV;
            }
            set
            {
                _minHueForHSV = value;
                _maxHueForHSV = value + HUE_STEP;
                NotifyPropertyChanged();
            }
        }

        public int MaxHueForHSV
        {
            get
            {
                return _maxHueForHSV;
            }
        }

        public int BeyondDilate
        {
            get
            {
                return _beyondDilate;
            }
            set
            {
                _beyondDilate = value;
                NotifyPropertyChanged();
            }
        }

        public int BeyondErode
        {
            get
            {
                return _beyondErode;
            }
            set
            {
                _beyondErode = value;
                NotifyPropertyChanged();
            }
        }

        public int TopDilate
        {
            get
            {
                return _topDilate;
            }
            set
            {
                _topDilate = value;
                NotifyPropertyChanged();
            }
        }

        public int TopErode
        {
            get
            {
                return _topErode;
            }
            set
            {
                _topErode = value;
                NotifyPropertyChanged();
            }
        }

        public int MinYCCValue => MIN_YCC_VALUE;
        public int MaxYCCValue => MAX_YCC_VALUE;
        public int MinMorphologyValue => MIN_MORPHOLOGY_VALUE;
        public int MaxMorphologyValue => MAX_MORPHOLOGY_VALUE;
        public double MinGaussianValue => MIN_GAUSSIANBLUR_VALUE;
        public double MaxGaussianValue => MAX_GAUSSIANBLUR_VALUE;
        public int MinHueValue => MIN_HUE_VALUE;
        public int MaxHueValue => MAX_HUE_VALUE;
        public int MinSatBrigValue => MIN_SAT_BRIG_VALUE;
        public int MaxSatBrigValue => MAX_SAT_BRIG_VALUE;

        private int _noiseBG = 38;
        private int _clarifyBG = 38;
        private int _erodeBG = 10;
        private int _dilateBG = 7;
        private double _gaussianBlurVal = 1.0;
        private int _minHueForHSV = 0;
        private int _maxHueForHSV = 0;
        private int _beyondDilate = 2;
        private int _beyondErode = 2;
        private int _topDilate = 2;
        private int _topErode = 2;

        private const int MIN_YCC_VALUE = 0;
        private const int MAX_YCC_VALUE = 127;
        private const int MIN_MORPHOLOGY_VALUE = 2;
        private const int MAX_MORPHOLOGY_VALUE = 20;
        private const double MIN_GAUSSIANBLUR_VALUE = 0.0;
        private const double MAX_GAUSSIANBLUR_VALUE = 10.0;
        private const int MIN_HUE_VALUE = 0;
        private const int HUE_STEP = 15;
        private const int MAX_HUE_VALUE = 179 - HUE_STEP; 
        private const int MIN_SAT_BRIG_VALUE = 0;
        private const int MAX_SAT_BRIG_VALUE = 245;

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
