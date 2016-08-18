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
        Image<Bgr, Byte> currentImage;
        
        public MainWindow()
        {
            InitializeComponent();

            CvInvoke.UseOpenCL = false;

            try
            {
                _capture = new Capture();
                _capture.ImageGrabbed += ProcessFrame;
            }
            catch (NullReferenceException excpt)
            {
                MessageBox.Show(excpt.Message);
            }

            captureButton.Content = "Start Capture";
        }

        private void captureButtonClick(object sender, EventArgs e)
        {
            if (_capture != null)
            {
                if (_captureInProgress)
                {  //stop the capture
                    captureButton.Content = "Start Capture";
                    _capture.Pause();
                    updateImage();
                }
                else
                {   //start the capture
                    captureButton.Content = "Stop";
                    _capture.Start();
                }

                _captureInProgress = !_captureInProgress;
            }
        }

        private void ProcessFrame(object sender, EventArgs arg)
        {
            Mat frame = new Mat();
            _capture.Retrieve(frame, CameraDevice);

            currentImage = frame.ToImage<Bgr,Byte>();

        }

        private void updateImage()
        {
            image1.Source = BitmapSourceConvert.ToBitmapSource(currentImage);
        }

    }
}
