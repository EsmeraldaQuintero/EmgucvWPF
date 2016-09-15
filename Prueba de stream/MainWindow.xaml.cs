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

            captureButton.Content = "Start Capture";
        }

        private void captureButtonClick(object sender, EventArgs e)
        {
            if (captureCamera._capture != null)   //if camera capture has been successfully created
            {
                if (_captureInProgress)
                {  //stop the capture
                    captureButton.Content = "Start Capture";
                    captureCamera.Pause();
                }
                else
                {   //start the capture
                    captureButton.Content = "Stop";
                    captureCamera.Start();
                }

                _captureInProgress = !_captureInProgress;
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
