using System;
using System.Windows;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV;
using Emgu.CV.XFeatures2D;
using Emgu.CV.Features2D;
using Emgu.CV.Util;

namespace Prueba_de_stream
{
    public class CaptureCamera
    {
        public ContextSurf context;
        Mat backgroundFrame;

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
            long matchTime = 0;

            Mat currentFrame = new Mat();
            Mat withoutBackgroundMask = new Mat();
            Mat segmentedMask = new Mat();
            Mat maskAnd = new Mat();
            Mat filterMask = new Mat();

            _capture.Retrieve(currentFrame);
            if( !currentFrame.IsEmpty )
            {
                withoutBackgroundMask = ImagePreProcessorAlgorithms.BackgroundRemover(backgroundFrame, currentFrame);
            }

            if (!withoutBackgroundMask.IsEmpty )
            {
                segmentedMask = ImagePreProcessorAlgorithms.SegmentationFilter(currentFrame);
            }
            
            if( !segmentedMask.IsEmpty )
            {
                segmentedMask.CopyTo(maskAnd, withoutBackgroundMask);
                filterMask = ImagePreProcessorAlgorithms.MorphologyFilter(maskAnd);
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

            try
            {
                long time;

                //Mat pepe = CvInvoke.Imread("C:\\Users\\uabc\\Documents\\EmgucvWPF\\Prueba de stream\\books2.png", LoadImageType.Grayscale);
                Mat modelFrame = CvInvoke.Imread("C:\\Users\\uabc\\Documents\\EmgucvWPF\\Prueba de stream\\training\\5.png", LoadImageType.Grayscale);

                fakeHarris(filterMask.ToImage<Gray, float>(), modelFrame);
                //Mat result = DrawMatches.Draw(modelFrame, filterMask, out time);
                //string m = SurfAlgorithm.Process(modelFrame, filterMask) ? "Se encontro algo" : string.Empty;
                //if (m != string.Empty)
                //{
                //    MessageBox.Show(m);
                //    System.Threading.Thread.Sleep(1000);
                //}
                //var resultImg = result.ToImage<Gray, byte>();
                //DisplayResult?.Invoke(resultImg,100);
            }
            catch (ArgumentException ae) {}
            catch (Exception exp1) { }
        }

        private void fakeHarris(Image<Gray, float> current, Mat model)
        {
            var m_CornerImage = new Image<Gray, float>(current.Size);
            CvInvoke.CornerHarris(current, m_CornerImage, 3, 3, 0.01);



            Freak freakCPU = new Freak();
            SurfImage modelWeapon = new SurfImage(current.Mat);
            SurfImage observedCamera = new SurfImage(model);


            //Extract features from the object image and  observed image
            freakCPU.DetectAndCompute(modelWeapon.matImage, modelWeapon.matImage, modelWeapon.keyPoints,modelWeapon.descriptors,false);

            VectorOfVectorOfDMatch matches = new VectorOfVectorOfDMatch();
            BFMatcher matcher = new BFMatcher(DistanceType.L2);
            matcher.Add(modelWeapon.descriptors);
            matcher.KnnMatch(observedCamera.descriptors, matches, 2, null);

            //// create and show inverted threshold image
            //var m_ThresholdImage = new Image<Gray, byte>(current.Size);
            //CvInvoke.Threshold(m_CornerImage, m_ThresholdImage, 0.0001, 255.0, ThresholdType.BinaryInv);
            //DisplayResult?.Invoke(m_ThresholdImage, 100);
        }
    }
}
