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

        private int _noiseBG = 38;
        private int _clarifyBG = 38;
        private int _erodeBG = 10;
        private int _dilateBG = 7;

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
