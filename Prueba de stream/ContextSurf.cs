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

        public int HueForHSV
        {
            get
            {
                return _hueForHSV;
            }
            set
            {
                _hueForHSV = value;
                NotifyPropertyChanged();
            }
        }

        public int SatForHSV
        {
            get
            {
                return _satForHSV;
            }
            set
            {
                _satForHSV = value;
                NotifyPropertyChanged();
            }
        }

        public int BrigForHSV
        {
            get
            {
                return _brigForHSV;
            }
            set
            {
                _brigForHSV = value;
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
        private int _hueForHSV = 0;
        private int _satForHSV = 0;
        private int _brigForHSV = 0;

        private const int MIN_YCC_VALUE = 0;
        private const int MAX_YCC_VALUE = 127;
        private const int MIN_MORPHOLOGY_VALUE = 2;
        private const int MAX_MORPHOLOGY_VALUE = 20;
        private const double MIN_GAUSSIANBLUR_VALUE = 0.0;
        private const double MAX_GAUSSIANBLUR_VALUE = 10.0;
        private const int MIN_HUE_VALUE = 0;
        private const int MAX_HUE_VALUE = 179;
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
