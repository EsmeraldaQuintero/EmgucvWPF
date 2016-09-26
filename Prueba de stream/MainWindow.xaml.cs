using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Prueba_de_stream
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _captureInProgress;
        private CaptureCamera captureCamera;

        public MainWindow()
        {
            InitializeComponent();

            captureCamera = new CaptureCamera();
            captureCamera.DisplayImages += DisplayImage;
            captureCamera.DisplayResult += DisplayResult;
            DataContext = captureCamera.context;

            CaptureButton.Content = "Start Capture";
        }

        private void captureButtonClick(object sender, EventArgs e)
        {
            if ( captureCamera.IsReady() )   //if camera capture has been successfully created
            {
                if (_captureInProgress)
                {  //stop the capture
                    CaptureButton.Content = "Start Capture";
                    captureCamera.Pause();
                }
                else
                {   //start the capture
                    CaptureButton.Content = "Stop";
                    captureCamera.Start();
                }

                _captureInProgress = !_captureInProgress;
            }
        }

        private void DisplayResult(Image<Gray, byte> resultImg, long matchTime)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                ResultImage.Source = ToImageSource(resultImg.Bitmap);
                MatchTimeText.Text = matchTime.ToString();
            }));
        }

        private void DisplayImage(Image<Bgr, byte> bgImg, Image<Gray, byte> bgRemoveImg, Image<Gray, byte> segmentedImg, Image<Gray, byte> contourImg)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                BackgroundImage.Source = ToImageSource(bgImg.Bitmap);
                WithoutBackgroundImage.Source = ToImageSource(bgRemoveImg.Bitmap);
                SegmentedImage.Source = ToImageSource(segmentedImg.Bitmap);
                ContoursImage.Source = ToImageSource(contourImg.Bitmap);
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
