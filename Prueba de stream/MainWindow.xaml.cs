using System;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.IO;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Emgu.CV;
using Emgu.CV.Cuda;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Prueba_de_stream
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private Capture _capture = null;
        private bool _captureInProgress;

        public int MinHue
        {
            get
            {
                return _minHue;
            }
            set
            {
                _minHue = value;
                NotifyPropertyChanged();
            }
        }
        public int MaxHue
        {
            get
            {
                return _maxHue;
            }
            set
            {
                _maxHue = value;
                NotifyPropertyChanged();
            }
        }

        public int MinBrig
        {
            get
            {
                return _minBrig;
            }
            set
            {
                _minBrig = value;
                NotifyPropertyChanged();
            }
        }

        public int MaxBrig
        {
            get
            {
                return _maxBrig;
            }
            set
            {
                _maxBrig = value;
                NotifyPropertyChanged();
            }
        }

        public int MaxSat
        {
            get
            {
                return _maxSat;
            }
            set
            {
                _maxSat = value;
                NotifyPropertyChanged();
            }
        }
        public int MinSat
        {
            get
            {
                return _minSat;
            }
            set
            {
                _minSat = value;
                NotifyPropertyChanged();
            }
        }

        private int _minHue = 0;
        public int _maxHue = 0;
        public int _minBrig = 0;
        public int _maxBrig = 0;
        private int _maxSat = 0;
        private int _minSat = 0;

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;
            CvInvoke.UseOpenCL = false;

            if (_capture == null)   //if camera capture hasn't been created, then created one
            {
                try
                {   //Creating the camera capture
                    //_capture = new Capture("http://192.168.1.65/mjpg/video.mjpg");
                    _capture = new Capture();
                    _capture.ImageGrabbed += ProcessFrame;
                }
                catch (NullReferenceException excpt)
                {   //show errors if there is any
                    MessageBox.Show(excpt.Message);
                }
            }

            captureButton.Content = "Start Capture";
        }

        private void captureButtonClick(object sender, EventArgs e)
        {
            if (_capture != null)   //if camera capture has been successfully created
            {
                if (_captureInProgress)
                {  //stop the capture
                    captureButton.Content = "Start Capture";
                    _capture.Pause();
                }
                else
                {   //start the capture
                    captureButton.Content = "Stop";
                    _capture.Start();
                }

                _captureInProgress = !_captureInProgress;
            }
        }


        private void ProcessFrame(object sender, EventArgs e)
        {
            long matchTime;
            int TRAIN_WIDTH = 400;
            int TRAIN_HEIGHT = 300;

            Mat newFrame = new Mat();
            _capture.Retrieve(newFrame);
            //Mat newFrame = CvInvoke.Imread("C:\\Users\\uabc\\Documents\\EmgucvWPF\\Prueba de stream\\Img1.jpg", LoadImageType.Grayscale);
            Mat newModelFrame = CvInvoke.Imread("C:\\Users\\uabc\\Documents\\EmgucvWPF\\Prueba de stream\\Img4.jpg", LoadImageType.Grayscale);

            //var currentFrame = newFrame.ToImage<Gray, Byte>().ThresholdToZero(new Gray(5.0)).Canny(80,150);

            var currentFrame = newFrame.ToImage<Gray, Byte>().Resize(TRAIN_WIDTH, TRAIN_HEIGHT, Emgu.CV.CvEnum.Inter.Cubic);
            var processFrame = newModelFrame.ToImage<Gray, Byte>().Resize(TRAIN_WIDTH, TRAIN_HEIGHT, Emgu.CV.CvEnum.Inter.Cubic);



            var image = (newFrame.ToImage<Bgr, byte>().Resize(TRAIN_WIDTH, TRAIN_HEIGHT, Emgu.CV.CvEnum.Inter.Cubic)).Convert<Hsv, byte>();
            Image<Gray, byte>[] channels = image.Split();
            var ch0 = channels[0];  //matiz

            var ch1 = channels[1];  //saturacion
            var ch2 = channels[2];  //brillo
            Image<Gray, byte> huefilter = ch0.InRange(new Gray(_minHue), new Gray(_maxHue));
            Image<Gray, byte> saturfilter = ch1.InRange(new Gray(_minSat), new Gray(_maxSat));
            Image<Gray, byte> brightnessFilter = ch2.InRange(new Gray(_minBrig), new Gray(_maxBrig));
            Image<Gray, byte> result2 = huefilter.And(saturfilter);
            Image<Gray, byte> result3 = result2.And(brightnessFilter);
            //currentFrame = huefilter;
            currentFrame = result3;



            DisplayImage(currentFrame, processFrame);

            using (Mat modelImage = processFrame.Mat)
            using (Mat observedImage = currentFrame.Mat)
            {
                Mat result = DrawMatches.Draw(modelImage, observedImage, out matchTime);
                var resultFrame = result.ToImage<Bgr, Byte>();
                DisplayResult(resultFrame, matchTime);
            }
        }

        private void DisplayResult(Image<Bgr, Byte> resultFrame, long matchTime)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                resultImage.Source = ToImageSource(resultFrame.Bitmap);
                MatchTimeText.Text = matchTime.ToString();
            }));
        }

        private void DisplayImage(Image<Gray, Byte> currentFrame, Image<Gray, Byte> processFrame)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                sourceImage.Source = ToImageSource(currentFrame.Bitmap);
                modelImage.Source = ToImageSource(processFrame.Bitmap);
            }));
        }

        private BitmapImage ToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }
    }
}
