using System;
using System.Windows;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV;
using System.Collections.Generic;
using System.Diagnostics;

namespace Prueba_de_stream.Cuda
{
    public class TestCaptureCamera
    {
        public ContextSurf context;
        Mat backgroundFrame;

        private Capture _capture;
        private bool _ready;
        private TestImagePreProcessorAlgorithm imagePreProcessorAlgorithm;
        private CudaSurfAlgorithm cudaSurfAlgorithm;
        private CudaSURFMatchAlgorithm cudaSURFMatchAlgorithm;
        public delegate void DisplayResultEventHandler(Image<Bgr, byte> resultFrame, long matchTime);
        public event DisplayResultEventHandler DisplayResult;

        public delegate void DisplayImagesEventHandler(Image<Gray,byte> currentFrame, Image<Gray, byte> minFrame, Image<Gray, byte> maxFrame, Image<Gray, byte> subFrame);
        public event DisplayImagesEventHandler DisplayImages;

        public TestCaptureCamera()
        {
            _capture = null;
            _ready = false;
            CvInvoke.UseOpenCL = false;

            imagePreProcessorAlgorithm = new TestImagePreProcessorAlgorithm();
            cudaSurfAlgorithm = new CudaSurfAlgorithm();
            cudaSURFMatchAlgorithm = new CudaSURFMatchAlgorithm();
            //createCapture("http://192.168.1.99/mjpg/video.mjpg");
            createCapture("");
            context = ContextSurf.Instance;
            backgroundFrame = new Mat();
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
            _capture.Retrieve(backgroundFrame);
        }

        private void ProcessFrame(object sender, EventArgs e)
        {
            imagePreProcessorAlgorithm.Context = context.TestContext;
            long matchTime = 0;

            Mat currentFrame = new Mat();
            Mat withoutBackgroundMask = new Mat();
            Mat segmentedMask = new Mat();
            Mat maskAnd = new Mat();
            Mat filterMask = new Mat();

            _capture.Retrieve(currentFrame);

            if ( !currentFrame.IsEmpty )
            {
                withoutBackgroundMask = imagePreProcessorAlgorithm.BackgroundRemover(backgroundFrame, currentFrame);
            }

            if (!withoutBackgroundMask.IsEmpty )
            {
                segmentedMask = imagePreProcessorAlgorithm.SegmentationFilter(currentFrame);
            }
            
            if( !segmentedMask.IsEmpty )
            {
                segmentedMask.CopyTo(maskAnd, withoutBackgroundMask);
                filterMask = imagePreProcessorAlgorithm.MorphologyFilter(maskAnd);
            }

            if ( !filterMask.IsEmpty )
            {
                Image<Gray, byte> img1 = null;
                Image<Gray, byte> img2 = null;
                Image<Gray, byte> img3 = null;
                Image<Gray, byte> img4 = null;

                try
                {
                    img1 = backgroundFrame.ToImage<Gray, byte>();
                    img2 = withoutBackgroundMask.ToImage<Gray, byte>();
                    img3 = segmentedMask.ToImage<Gray, byte>();
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

            try
            {
                int TRAIN_WIDTH = 640;
                int TRAIN_HEIGHT = 480;
                string path = "C:\\Users\\uabc\\Documents\\EmgucvWPF\\Prueba de stream\\training";
                List<CudaSurfImage> weaponsTrained = cudaSurfAlgorithm.LoadListOfWeaponsTrained(path, TRAIN_WIDTH, TRAIN_HEIGHT);
                bool isWeaponDetected = false;

                List<Mat> blobList = BlobAlgorithm.SplitImageByROI(filterMask);

                foreach (var blob in blobList)
                {
                    CudaSurfImage observedSurfImage = cudaSurfAlgorithm.GetSurfFeaturesOf(filterMask);
                    weaponsTrained.ForEach(weaponModel =>
                    {
                        isWeaponDetected = cudaSURFMatchAlgorithm.Process(weaponModel, observedSurfImage);
                    });
                }
            }
            catch (ArgumentException ae) {}
        }
    }
}
