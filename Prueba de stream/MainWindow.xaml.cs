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
        private int _maxHue = 128;

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
        private int _maxSat = 128;

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
        private int _maxBrig = 255;

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

            //Mat newFrame = CvInvoke.Imread("C:\\Users\\uabc\\Documents\\EmgucvWPF\\Prueba de stream\\Img1.jpg", LoadImageType.Grayscale);
            //originalCurrentFrame._EqualizeHist();      //contraste
            //originalCurrentFrame._GammaCorrect(1.4d);

            Mat newFrame = new Mat();
            _capture.Retrieve(newFrame);
            Mat newModelFrame = CvInvoke.Imread("C:\\Users\\uabc\\Documents\\EmgucvWPF\\Prueba de stream\\Img6.jpg", LoadImageType.Grayscale);

            var currentFrame = (newFrame.ToImage<Bgr, byte>().Resize(TRAIN_WIDTH, TRAIN_HEIGHT, Emgu.CV.CvEnum.Inter.Cubic)).Convert<Hsv, byte>();
            var processFrame = (newModelFrame.ToImage<Bgr, byte>().Resize(TRAIN_WIDTH, TRAIN_HEIGHT, Emgu.CV.CvEnum.Inter.Cubic)).Convert<Hsv, byte>();

            var currentFrameWithErode = ColorSegmentation(currentFrame, _hue1, _sat1, _brig1);
            var currentFrameWithDilate = ColorSegmentation(currentFrame, _hue2, _sat2, _brig2);

            currentFrameWithErode = Morphology(currentFrameWithErode,_dilate1, _erode1, true);
            currentFrameWithDilate = Morphology(currentFrameWithDilate, _dilate2, _erode2, false);

            var subFrame = currentFrameWithDilate.Sub(currentFrameWithErode);

            DisplayImage(currentFrame, currentFrameWithErode, currentFrameWithDilate, subFrame);

            using (Mat modelImage = processFrame.Mat)
            using (Mat observedImage = subFrame.Mat)
            {
                Mat result = DrawMatches.Draw(modelImage, observedImage, out matchTime);
                var resultFrame = result.ToImage<Bgr, Byte>();
                DisplayResult(resultFrame, matchTime);
            }
        }

        private Image<Gray, byte> Morphology(Image<Gray, byte> image, int dilateSize, int erodeSize, bool erode)
        {
            var dilateImage = image.Copy(image);

            Mat rec_Erode = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new System.Drawing.Size(erodeSize, erodeSize), new System.Drawing.Point(erodeSize / 2, erodeSize / 2));
            Mat rec_Dilate = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new System.Drawing.Size(dilateSize, dilateSize), new System.Drawing.Point(dilateSize / 2, dilateSize / 2));

            CvInvoke.Dilate(dilateImage, dilateImage, rec_Dilate, new System.Drawing.Point(1, 1), 2, BorderType.Default, new MCvScalar(0, 0, 0));
            if (erode)
            {
                CvInvoke.Erode(dilateImage, dilateImage, rec_Erode, new System.Drawing.Point(1, 1), 1, BorderType.Default, new MCvScalar(0, 0, 0));
            }
            return dilateImage;
        }

        private Image<Gray,byte> ColorSegmentation(Image<Hsv, byte> currentFrame, int hue, int sat, int brig)
        {
            Image<Gray, byte>[] channels = currentFrame.Split();
            var ch0 = channels[0];  //matiz
            var ch1 = channels[1];  //saturacion
            var ch2 = channels[2];  //brillo
            Image<Gray, byte> huefilter = ch0.InRange(new Gray(hue), new Gray(_maxHue));
            Image<Gray, byte> saturfilter = ch1.InRange(new Gray(sat), new Gray(_maxSat));
            Image<Gray, byte> brightnessFilter = ch2.InRange(new Gray(brig), new Gray(_maxBrig));
            Image<Gray, byte> result2 = huefilter.And(saturfilter);
            Image<Gray, byte> result3 = result2.And(brightnessFilter);

            return result3;
        }

        private void DisplayResult(Image<Bgr, Byte> resultFrame, long matchTime)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                resultImage.Source = ToImageSource(resultFrame.Bitmap);
                MatchTimeText.Text = matchTime.ToString();
            }));
        }

        private void DisplayImage(Image<Hsv, Byte> currentFrame, Image<Gray, Byte> minFrame, Image<Gray, Byte> maxFrame, Image<Gray, Byte> subFrame)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                sourceImage.Source = ToImageSource(currentFrame.Bitmap);
                filterImage.Source = ToImageSource(minFrame.Bitmap);
                erodeImage.Source = ToImageSource(maxFrame.Bitmap);
                dilateImage.Source = ToImageSource(subFrame.Bitmap);
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
