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

        public int MinSqrMin
        {
            get
            {
                return _minSqrMin;
            }
            set
            {
                _minSqrMin = value;
                NotifyPropertyChanged();
            }
        }

        public int MinSqrMax
        {
            get
            {
                return _minSqrMax;
            }
            set
            {
                _minSqrMax = value;
                NotifyPropertyChanged();
            }
        }

        private int _minHue = 79;
        public int _maxHue = 128;
        public int _minBrig = 0;
        public int _maxBrig = 255;
        private int _maxSat = 128;
        private int _minSat = 28;
        private int _minSqrMin = 5;
        private int _minSqrMax = 2;


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
            Mat newModelFrame = CvInvoke.Imread("C:\\Users\\uabc\\Documents\\EmgucvWPF\\Prueba de stream\\Img6.jpg", LoadImageType.Grayscale);

            var currentFrame = (newFrame.ToImage<Bgr, byte>().Resize(TRAIN_WIDTH, TRAIN_HEIGHT, Emgu.CV.CvEnum.Inter.Cubic)).Convert<Hsv, byte>();
            var processFrame = (newModelFrame.ToImage<Bgr, byte>().Resize(TRAIN_WIDTH, TRAIN_HEIGHT, Emgu.CV.CvEnum.Inter.Cubic)).Convert<Hsv, byte>();


            currentFrame._EqualizeHist();
            currentFrame._GammaCorrect(1.4d);
            Image<Gray, byte>[] channels = currentFrame.Split();
            var ch0 = channels[0];  //matiz
            var ch1 = channels[1];  //saturacion
            var ch2 = channels[2];  //brillo
            Image<Gray, byte> huefilter = ch0.InRange(new Gray(_minHue), new Gray(_maxHue));
            Image<Gray, byte> saturfilter = ch1.InRange(new Gray(_minSat), new Gray(_maxSat));
            Image<Gray, byte> brightnessFilter = ch2.InRange(new Gray(_minBrig), new Gray(_maxBrig));
            Image<Gray, byte> result2 = huefilter.And(saturfilter);
            Image<Gray, byte> result3 = result2.And(brightnessFilter);

            var currentFrameWithFilters = result3;



            //Mat rect_12 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new System.Drawing.Size(_minSqrMax, _minSqrMax), new System.Drawing.Point(_minSqrMax / 2, _minSqrMax / 2));
            //Mat rect_6 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new System.Drawing.Size(_minSqrMin, _minSqrMin), new System.Drawing.Point(_minSqrMin / 2, _minSqrMin / 2));
            //CvInvoke.Erode(currentFrame2, currentFrame2, rect_12, new System.Drawing.Point(1, 1), 1, BorderType.Default, new MCvScalar(0, 0, 0));
            //CvInvoke.Dilate(currentFrame2, currentFrame2, rect_6, new System.Drawing.Point(1, 1), 2, BorderType.Default, new MCvScalar(0, 0, 0));


            DisplayImage(currentFrame, currentFrameWithFilters);

            using (Mat modelImage = processFrame.Mat)
            using (Mat observedImage = currentFrame.Mat)
            {
                Mat result = DrawMatches.Draw(modelImage, observedImage, out matchTime);
                var resultFrame = result.ToImage<Hsv, Byte>();
                DisplayResult(resultFrame, matchTime);
            }
        }


        //public Image<Gray, Byte> YCrCbDetectSkinCpu(Mat img, IColor min, IColor max)
        //{
        //    Mat yccMat = new Mat();
        //    var skin = new Image<Gray, Byte>(img.Width, img.Height);
        //    CvInvoke.CvtColor(img, yccMat, ColorConversion.Bgr2YCrCb);

        //    // mejorar esta linea
        //    skin = yccMat.ToImage<Ycc, Byte>().InRange((Ycc)min, (Ycc)max);

        //    Mat rect_12 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new System.Drawing.Size(12, 12), new System.Drawing.Point(6, 6));
        //    Mat rect_6 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new System.Drawing.Size(6, 6), new System.Drawing.Point(3, 3));
        //    CvInvoke.Erode(skin, skin, rect_12, new System.Drawing.Point(6, 6), 1, BorderType.Default, new MCvScalar(0, 0, 0));
        //    CvInvoke.Dilate(skin, skin, rect_6, new System.Drawing.Point(3, 3), 2, BorderType.Default, new MCvScalar(0, 0, 0));

        //    return skin;
        //}
        private void DisplayResult(Image<Hsv, Byte> resultFrame, long matchTime)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                resultImage.Source = ToImageSource(resultFrame.Bitmap);
                MatchTimeText.Text = matchTime.ToString();
            }));
        }

        private void DisplayImage(Image<Hsv, Byte> currentFrame, Image<Gray, Byte> processFrame)
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
