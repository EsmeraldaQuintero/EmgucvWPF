using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.IO;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Emgu.CV;



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
        Image<Bgr, Byte> currentFrame;
        
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
            currentFrame = newFrame.ToImage<Bgr, Byte>();


            var processFrame = newFrame.ToImage<Gray, Byte>();
            //var processFrame = skinDetection.YCrCbDetectSkin(newFrame, YCrCb_min, YCrCb_max);
            //DetectGesture(processFrame);
            DisplayImage(currentFrame, processFrame);
        }


        private void DisplayImage(Image<Bgr, Byte> currentFrame, Image<Gray, Byte> processFrame)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                sourceImage.Source = ToImageSource(currentFrame.Bitmap);

               // motionImage.Source = ToImageSource(processFrame.Bitmap);
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
