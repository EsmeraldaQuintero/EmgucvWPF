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
                Mat mask = new Mat();
                segmentedMask.CopyTo(mask, withoutBackgroundMask);
                filterMask = MorphologyFilter(mask);
            }

            if ( !filterMask.IsEmpty )
            {

                Image<Gray, byte> resultFrame = null;
                try
                {
                    resultFrame = filterMask.ToImage<Gray, byte>();
                    DisplayResult?.Invoke(resultFrame, 2000);
                }

                finally
                {
                    if (resultFrame != null)
                        ((IDisposable)resultFrame).Dispose();
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
                return mask;
            }
        }

        public Mat MorphologyFilter(Mat filterMask)
        {
            Mat beyondMask = new Mat();
            Mat topMask = new Mat();

            beyondMask = Morphology(filterMask, context.BeyondDilate, context.BeyondErode, true);
            topMask = Morphology(filterMask, context.TopDilate, context.TopErode, false);

            var img1 = beyondMask.ToImage<Gray, byte>();
            var img2 = topMask.ToImage<Gray, byte>();

            return img2.Sub(img1).Mat;
        }

        private Mat Morphology(Mat image, int dilateSize, int erodeSize, bool erode)
        {
            Mat result = new Mat();
            Mat rec_Erode = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new System.Drawing.Size(erodeSize, erodeSize), new System.Drawing.Point(erodeSize / 2, erodeSize / 2));
            Mat rec_Dilate = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new System.Drawing.Size(dilateSize, dilateSize), new System.Drawing.Point(dilateSize / 2, dilateSize / 2));

            CvInvoke.Dilate(image, result, rec_Dilate, new System.Drawing.Point(1, 1), 2, BorderType.Default, new MCvScalar(0, 0, 0));
            if (erode)
            {
                CvInvoke.Erode(result, result, rec_Erode, new System.Drawing.Point(1, 1), 1, BorderType.Default, new MCvScalar(0, 0, 0));
            }
            return result;
        }




        //Mat smoothFrame2 = new Mat();
        //Mat rainbowFrame = CvInvoke.Imread("C:\\Users\\uabc\\Documents\\EmgucvWPF\\Prueba de stream\\Rainbow.jpg", LoadImageType.Unchanged);
        //CvInvoke.GaussianBlur(rainbowFrame, smoothFrame2, pxDiameter, context.GaussianBlurVal, context.GaussianBlurVal);
        //Mat hsvFrame2 = new Mat();
        //CvInvoke.CvtColor(smoothFrame2, hsvFrame2, ColorConversion.Bgr2Hsv);   //Convert to HSV Image

        //Image<Gray, byte>[] channels2 = hsvFrame2.ToImage<Hsv, byte>().Split();
        //var ch02 = channels2[0];  //Hue
        //Image<Gray, byte> huefilter2 = ch02.InRange(new Gray(context.MinHueForHSV), new Gray(context.MaxHueForHSV));

        //var img1 = smoothFrame.ToImage<Bgr, byte>();
        //DisplayImages?.Invoke(
        //    huefilter.Convert<Gray, byte>(),
        //    brightnessFilter.Convert<Gray, byte>(),
        //    mask,
        //    huefilter2.Convert<Gray, byte>()
        //    );


        //public Image<Gray, byte> SegmentationFilter(Image<Hsv, byte> currentFrame)
        //{
        //    var currentFrameWithErode = ColorSegmentation(currentFrame, context.Hue1, context.Sat1, context.Brig1);
        //    var currentFrameWithDilate = ColorSegmentation(currentFrame, context.Hue2, context.Sat2, context.Brig2);
        //    //.SmoothGaussian(7, 7, 34.3, 45.3)
        //    currentFrameWithErode = Morphology(currentFrameWithErode, context.Dilate1, context.Erode1, true);
        //    currentFrameWithDilate = Morphology(currentFrameWithDilate, context.Dilate2, context.Erode2, false);

        //    var subFrame = currentFrameWithDilate.Sub(currentFrameWithErode);

        //    DisplayImages?.Invoke(currentFrame.Convert<Gray,byte>(), currentFrameWithErode, currentFrameWithDilate, subFrame);

        //    return subFrame;
        //}

        //private Image<Gray, byte> ColorSegmentation(Image<Hsv, byte> currentFrame, int hue, int sat, int brig)
        //{
        //    Image<Gray, byte>[] channels = currentFrame.Split();
        //    var ch0 = channels[0];  //matiz
        //    var ch1 = channels[1];  //saturacion
        //    var ch2 = channels[2];  //brillo
        //    Image<Gray, byte> huefilter = ch0.InRange(new Gray(hue), new Gray(context.MaxHue));
        //    Image<Gray, byte> saturfilter = ch1.InRange(new Gray(sat), new Gray(context.MaxSat));
        //    Image<Gray, byte> brightnessFilter = ch2.InRange(new Gray(brig), new Gray(context.MaxBrig));
        //    Image<Gray, byte> result2 = huefilter.And(saturfilter);
        //    Image<Gray, byte> result3 = result2.And(brightnessFilter);

        //    return result3;
        //}

        //private Image<Gray, byte> Morphology(Image<Gray, byte> image, int dilateSize, int erodeSize, bool erode)
        //{
        //    var dilateImage = image.Copy(image);

        //    Mat rec_Erode = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new System.Drawing.Size(erodeSize, erodeSize), new System.Drawing.Point(erodeSize / 2, erodeSize / 2));
        //    Mat rec_Dilate = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new System.Drawing.Size(dilateSize, dilateSize), new System.Drawing.Point(dilateSize / 2, dilateSize / 2));

        //    CvInvoke.Dilate(dilateImage, dilateImage, rec_Dilate, new System.Drawing.Point(1, 1), 2, BorderType.Default, new MCvScalar(0, 0, 0));
        //    if (erode)
        //    {
        //        CvInvoke.Erode(dilateImage, dilateImage, rec_Erode, new System.Drawing.Point(1, 1), 1, BorderType.Default, new MCvScalar(0, 0, 0));
        //    }
        //    return dilateImage;
        //}

        //private Mat Segmentation1(Mat smoothFrame)
        //{
        //    var hsv = new Image<Hsv, byte>(smoothFrame.Size);
        //    CvInvoke.CvtColor(smoothFrame, hsv, ColorConversion.Bgr2Hsv);   //Convert to HSV Image

        //    for (var x = 0; x < hsv.Width; x++)
        //    {
        //        for (var y = 0; y < hsv.Height; y++)
        //        {
        //            if (!((hsv.Data[y, x, 0] == 0) & (hsv.Data[y, x, 2] == 0))) //i.e. if Black
        //            {
        //                hsv.Data[y, x, 0] = 100;
        //                hsv.Data[y, x, 1] = 100;
        //                hsv.Data[y, x, 2] = 100;
        //            }
        //        }
        //    }

        //    return hsv.Mat;
        //}


    }
}
