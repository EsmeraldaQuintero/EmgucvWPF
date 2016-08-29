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



namespace Prueba_de_stream
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Capture _capture = null;
        private bool _captureInProgress;
        int CameraDevice = 0;
        long matchTime;

        public MainWindow()
        {
            InitializeComponent();

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
            Mat newFrame = new Mat();
            _capture.Retrieve(newFrame);
            Mat newModelFrame = CvInvoke.Imread("C:\\Users\\uabc\\Documents\\EmgucvWPF\\Prueba de stream\\Img1.jpg", LoadImageType.Grayscale);

            var currentFrame = newFrame.ToImage<Gray, Byte>();
            var processFrame = newModelFrame.ToImage<Gray, Byte>();
            DisplayImage(currentFrame, processFrame);

            using (Mat modelImage = currentFrame.Mat)
            using (Mat observedImage = processFrame.Mat)
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
