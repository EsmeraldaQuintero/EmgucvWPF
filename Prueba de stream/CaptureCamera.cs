using System;
using System.Windows;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV;

namespace Prueba_de_stream
{
    public class CaptureCamera
    {
        public ContextSurf context;
        private ImagePreProcessorAlgorithms processor;

        private Capture _capture;
        private bool _ready;

        public delegate void DisplayResultEventHandler(Image<Gray, byte> resultFrame, long matchTime);
        public event DisplayResultEventHandler DisplayResult;

        public delegate void DisplayImagesEventHandler(Image<Gray,byte> currentFrame, Image<Gray, byte> minFrame, Image<Gray, byte> maxFrame, Image<Gray, byte> subFrame);
        public event DisplayImagesEventHandler DisplayImages;

        public CaptureCamera()
        {
            _capture = null;
            _ready = false;
            CvInvoke.UseOpenCL = false;

            //createCapture("http://192.168.1.99/mjpg/video.mjpg");
            createCapture("");
            context = ContextSurf.Instance;
            processor = new ImagePreProcessorAlgorithms();
        }

        private void createCapture(string path)
        {
            if (_capture == null)   //if camera capture hasn't been created, then created one
            {
                try
                {   //Creating the camera capture
                    _capture = path == string.Empty ? new Capture() : new Capture(path);
                    _capture.ImageGrabbed += ProcessFrame;
                    _ready = true;
                }
                catch (NullReferenceException excpt)
                {   //show errors if there is any
                    MessageBox.Show(excpt.Message);
                }
            }
        }

        public bool IsReady()
        {
            return _ready;
        }

        public void Pause()
        {
            _capture.Pause();
        }
        public void Start()
        {
            _capture.Start();
            _capture.Retrieve(processor.backgroundFrame);
        }

        private void ProcessFrame(object sender, EventArgs e)
        {
            long matchTime = 0;

            Mat currentFrame = new Mat();
            Mat withoutBackgroundMask = new Mat();
            Mat segmentedMask = new Mat();
            Mat maskAnd = new Mat();
            Mat filterMask = new Mat();

            _capture.Retrieve(currentFrame);
            Mat modelFrame = CvInvoke.Imread("C:\\Users\\uabc\\Documents\\EmgucvWPF\\Prueba de stream\\book.png", LoadImageType.Grayscale);

            if( !currentFrame.IsEmpty )
            {
                withoutBackgroundMask = processor.BackgroundRemover(processor.backgroundFrame, currentFrame);
            }

            if (!withoutBackgroundMask.IsEmpty )
            {
                segmentedMask = processor.SegmentationFilter(currentFrame);
            }
            
            if( !segmentedMask.IsEmpty )
            {
                segmentedMask.CopyTo(maskAnd, withoutBackgroundMask);
                filterMask = processor.MorphologyFilter(maskAnd);
            }

            if ( !filterMask.IsEmpty )
            {

                Image<Gray, byte> img1 = null;
                Image<Gray, byte> img2 = null;
                Image<Gray, byte> img3 = null;
                Image<Gray, byte> img4 = null;
                Image<Gray, byte> img5 = null;

                try
                {
                    img1 = withoutBackgroundMask.ToImage<Gray, byte>();
                    img2 = segmentedMask.ToImage<Gray, byte>();
                    img3 = maskAnd.ToImage<Gray, byte>();
                    img4 = filterMask.ToImage<Gray, byte>();

                    DisplayImages?.Invoke(img1, img2, img3, img4);
                }

                finally
                {
                    if (img1 != null)
                        ((IDisposable)img1).Dispose();
                    if (img2 != null)
                        ((IDisposable)img2).Dispose();
                    if (img3 != null)
                        ((IDisposable)img3).Dispose();
                    if (img4 != null)
                        ((IDisposable)img4).Dispose();
                }
            }

            using (Mat modelWeaponImage = w.Mat)                  //Training image
            using (Mat observedCameraImage = currentFrame.Mat)    //Camera image
            {
                if (SurfProcess(modelWeaponImage, observedCameraImage))
                {
                    isWeaponDetected = true;
                }
            }



        }
    }
}
