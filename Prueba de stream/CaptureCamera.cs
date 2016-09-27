using System;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.IO;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV;
using System.Windows.Threading;

namespace Prueba_de_stream
{
    public class CaptureCamera
    {
        private Capture _capture;
        private bool _ready;

        public ContextSurf context;
        private Mat backgroundFrame;
        private const int TRAIN_WIDTH = 512;
        private const int TRAIN_HEIGHT = 384;
        private const int RADIUS_GAUSSIANBLUR = 5;


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

            backgroundFrame = new Mat();
            context = new ContextSurf();
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
            Mat modelFrame = CvInvoke.Imread("C:\\Users\\uabc\\Documents\\EmgucvWPF\\Prueba de stream\\Img1.jpg", LoadImageType.Grayscale);

            if( !currentFrame.IsEmpty )
            {
                withoutBackgroundMask = BackgroundRemover(backgroundFrame, currentFrame);
            }

            if (!withoutBackgroundMask.IsEmpty )
            {
                segmentedMask = SegmentationFilter(currentFrame);
            }
            
            if( !segmentedMask.IsEmpty )
            {
                segmentedMask.CopyTo(maskAnd, withoutBackgroundMask);
                filterMask = MorphologyFilter(maskAnd);
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


            //var currentFrame = (newFrame.ToImage<Bgr, byte>().Resize(TRAIN_WIDTH, TRAIN_HEIGHT, Emgu.CV.CvEnum.Inter.Cubic)).Convert<Hsv, byte>();
            //var filterFrame = FilterImage(currentFrame);

            //using (Mat modelImage = processFrame.Mat)
            //using (Mat observedImage = filterFrame.Mat)
            //{
            //    Mat result = DrawMatches.Draw(modelImage, observedImage, out matchTime);
            //    var resultFrame = result.ToImage<Gray, Byte>();
            //    DisplayResult?.Invoke(resultFrame, matchTime);
            //}
        }

        private Mat BackgroundRemover(Mat bgFrame, Mat currentFrame)
        {
            using (Mat bgMat = new Mat())
            using (Mat currentMat = new Mat())
            {
                CvInvoke.CvtColor(bgFrame, bgMat, ColorConversion.Bgr2YCrCb);
                CvInvoke.CvtColor(currentFrame, currentMat, ColorConversion.Bgr2YCrCb);
                using (var bgImage = bgMat.ToImage<Ycc, byte>().Resize(TRAIN_WIDTH, TRAIN_HEIGHT, Emgu.CV.CvEnum.Inter.Cubic))
                using (var currentImage = currentMat.ToImage<Ycc, byte>().Resize(TRAIN_WIDTH, TRAIN_HEIGHT, Emgu.CV.CvEnum.Inter.Cubic))
                { 
                    //Creating mask for remove background
                    Ycc thr = new Ycc(context.ClarifyBG, 0, 0);
                    var clarifyBgImage = bgImage.ThresholdToZero(thr);          //clarify background in order to eliminate black zones
                    var maskBgImage = clarifyBgImage.AbsDiff(currentImage);     //subtract background from image

                    //Applying filters to remove noise
                    Image<Gray, Byte>[] channels = maskBgImage.Split();
                    Image<Gray, Byte> y = channels[0];
                    var yfilter = y.InRange(new Gray(0), new Gray(context.NoiseBG));

                    //Eroding the source image using the specified structuring element
                    Mat rect_12 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new System.Drawing.Size(context.ErodeBG, context.ErodeBG), new System.Drawing.Point(context.ErodeBG / 2, context.ErodeBG / 2));
                    CvInvoke.Erode(yfilter, yfilter, rect_12, new System.Drawing.Point(1, 1), 1, BorderType.Default, new MCvScalar(0, 0, 0));

                    //Dilating the source image using the specified structuring element
                    Mat rect_6 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new System.Drawing.Size(context.DilateBG, context.DilateBG), new System.Drawing.Point(context.DilateBG / 2, context.DilateBG / 2));
                    CvInvoke.Dilate(yfilter, yfilter, rect_6, new System.Drawing.Point(1, 1), 2, BorderType.Default, new MCvScalar(0, 0, 0));

                    //Applying mask
                    Mat mask = new Mat();
                    CvInvoke.CvtColor(yfilter.Not().Mat, mask, ColorConversion.Gray2Bgr);
                    CvInvoke.CvtColor(mask, mask, ColorConversion.Bgr2Gray);
                    return mask;
                }
            }
        }

        private Mat SegmentationFilter(Mat withoutBackgroundFrame)
        {
            using( Mat smoothFrame = new Mat() )
            using( Mat hsvFrame = new Mat() )
            {   
                //Appliying Gaussian blur for reduce noise on the image
                System.Drawing.Size pxDiameter = new System.Drawing.Size(RADIUS_GAUSSIANBLUR, RADIUS_GAUSSIANBLUR);
                CvInvoke.GaussianBlur(withoutBackgroundFrame, smoothFrame, pxDiameter, context.GaussianBlurVal, context.GaussianBlurVal);
                
                //Segmenting image in HSV color
                CvInvoke.CvtColor(smoothFrame, hsvFrame, ColorConversion.Bgr2Hsv);
                Image<Gray, byte>[] channels = hsvFrame.ToImage<Hsv, byte>().Split();
                var ch0 = channels[0];  //Hue channel

                //Selecting color range for the mask
                Image<Gray, byte> huefilter = ch0.InRange(new Gray(context.MinHueForHSV), new Gray(context.MaxHueForHSV)).Resize(TRAIN_WIDTH, TRAIN_HEIGHT, Emgu.CV.CvEnum.Inter.Cubic);
                Mat mask = new Mat();
                CvInvoke.CvtColor(huefilter.Mat, mask, ColorConversion.Gray2Bgr);
                CvInvoke.CvtColor(mask, mask, ColorConversion.Bgr2Gray);

                Mat mask2 = new Mat();
                mask2 = DilateImage(mask, context.BeyondDilate);
                mask2 = ErodeImage(mask2, context.BeyondErode);


                return mask2;
            }
        }

        private Mat MorphologyFilter(Mat filterMask)
        {
            Mat beyondMask = new Mat();
            Mat topMask = new Mat();

            topMask = filterMask;
            beyondMask = ErodeImage(filterMask, context.TopErode);

            Mat contoursFrame = new Mat();
            CvInvoke.Subtract(topMask, beyondMask,contoursFrame,null,DepthType.Default);

            return contoursFrame;
        }

        private Mat ErodeImage(Mat frame,int erodeSize)
        {
            Mat erodeFrame = new Mat();
            Mat rect_12 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new System.Drawing.Size(erodeSize, erodeSize), new System.Drawing.Point(erodeSize / 2, erodeSize / 2));
            CvInvoke.Erode(frame, erodeFrame, rect_12, new System.Drawing.Point(1, 1), 1, BorderType.Default, new MCvScalar(0, 0, 0));
            return erodeFrame;
        }

        private Mat DilateImage(Mat frame, int dilateSize)
        {
            Mat dilateFrame = new Mat();
            Mat rect_6 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new System.Drawing.Size(dilateSize, dilateSize), new System.Drawing.Point(dilateSize / 2, dilateSize / 2));
            CvInvoke.Dilate(frame, dilateFrame, rect_6, new System.Drawing.Point(1, 1), 2, BorderType.Default, new MCvScalar(0, 0, 0));
            return dilateFrame;
        }
    }
}
