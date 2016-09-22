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


        public delegate void DisplayResultEventHandler(Image<Gray, Byte> resultFrame, long matchTime);
        public event DisplayResultEventHandler DisplayResult;

        public delegate void DisplayImagesEventHandler(Image<Gray, Byte> currentFrame, Image<Gray, Byte> minFrame, Image<Gray, Byte> maxFrame, Image<Gray, Byte> subFrame);
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
            Mat withoutBackgroundFrame = new Mat();
            Mat segmentedFrame = new Mat();

            _capture.Retrieve(currentFrame);
            Mat modelFrame = CvInvoke.Imread("C:\\Users\\uabc\\Documents\\EmgucvWPF\\Prueba de stream\\Img1.jpg", LoadImageType.Grayscale);

            if( !currentFrame.IsEmpty)
            {
                withoutBackgroundFrame = BackgroundRemover(backgroundFrame, currentFrame);
            }

            if (!withoutBackgroundFrame.IsEmpty)
            {
                segmentedFrame = SegmentationFilter(withoutBackgroundFrame);
            }
            
            if( !segmentedFrame.IsEmpty)
            {
                try
                {
                    //var resultFrame = segmentedFrame.ToImage<Gray, Byte>();
                    //DisplayResult?.Invoke(resultFrame, matchTime);

                }
                catch { }
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
            using (var bgImage = bgFrame.ToImage<Ycc, byte>().Resize(TRAIN_WIDTH, TRAIN_HEIGHT, Emgu.CV.CvEnum.Inter.Cubic) )
            using (var currentImage = currentFrame.ToImage<Ycc, byte>().Resize(TRAIN_WIDTH, TRAIN_HEIGHT, Emgu.CV.CvEnum.Inter.Cubic) )
            using (var originalImage = currentFrame.ToImage<Hsv, byte>().Resize(TRAIN_WIDTH, TRAIN_HEIGHT, Emgu.CV.CvEnum.Inter.Cubic) )
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
                yfilter = yfilter.Not();
                return originalImage.Copy(yfilter).Mat;
            } 
        }

        private Mat SegmentationFilter(Mat withoutBackgroundFrame)
        {
            Mat smoothFrame = new Mat();
            System.Drawing.Size pxDiameter = new System.Drawing.Size(RADIUS_GAUSSIANBLUR, RADIUS_GAUSSIANBLUR);
            CvInvoke.GaussianBlur(withoutBackgroundFrame, smoothFrame, pxDiameter, context.GaussianBlurVal, context.GaussianBlurVal);

            var hsv = new Image<Hsv, byte>(smoothFrame.Size);
            CvInvoke.CvtColor(smoothFrame, hsv, ColorConversion.Bgr2Hsv);   //Convert to HSV Image

            for (var x = 0; x < hsv.Width; x++)
            {
                for (var y = 0; y < hsv.Height; y++)
                {
                    if (!((hsv.Data[y, x, 0] == 0) & (hsv.Data[y, x, 2] == 0))) //i.e. if Black
                    {
                        hsv.Data[y, x, 0] = 100;
                        hsv.Data[y, x, 1] = 100;
                        hsv.Data[y, x, 2] = 100;
                    }
                }
            }
                


            Image<Gray, byte>[] channelsOfBg = hsv.Split();
            var ch0 = channelsOfBg[0];  //Hue (matiz de color)
            var ch2 = channelsOfBg[2];  //Value (brillo)

            
            Bgr min = new Bgr(context.HueForHSV, context.SatForHSV, context.BrigForHSV);
            Bgr max = new Bgr(context.HueForHSV +50, context.SatForHSV + 50, context.BrigForHSV + 50);


            var huefilter = smoothFrame.ToImage<Bgr,byte>().InRange(min, max);
            var brightnessFilter = ch2.InRange(new Gray(context.BrigForHSV), new Gray(context.MaxSatBrigValue));
            




            var img1 = smoothFrame.ToImage<Gray, byte>();
            DisplayImages?.Invoke(
                img1,
                huefilter.Convert<Gray,byte>(),
                hsv.Convert<Gray, byte>(),
                brightnessFilter
                );

            System.Threading.Thread.Sleep(100);
            Mat segmentedFrame = new Mat();

            //GetColorPixelMask(smoothFrame, segmentedFrame, context.MinColorHSV, context.MaxColorHSV);
            //bool useUMat;
            //using (InputOutputArray ia = segmentedFrame.GetInputOutputArray())
            //    useUMat = ia.IsUMat;

            //using (IImage hsv = useUMat ? (IImage)new UMat() : (IImage)new Mat())
            //using (IImage s = useUMat ? (IImage)new UMat() : (IImage)new Mat())
            //{
            //    CvInvoke.CvtColor(smoothFrame, hsv, ColorConversion.Bgr2Hsv);
            //    CvInvoke.ExtractChannel(hsv, segmentedFrame, 0);
            //    CvInvoke.ExtractChannel(hsv, s, 2);

            //    //the mask for hue less than 20 or larger than 160
            //    using (ScalarArray lower = new ScalarArray(context.MinColorHSV))
            //    using (ScalarArray upper = new ScalarArray(context.MaxColorHSV))
            //        CvInvoke.InRange(segmentedFrame, lower, upper, segmentedFrame);
            //    CvInvoke.BitwiseNot(segmentedFrame, segmentedFrame);

            //    //s is the mask for saturation of at least 10, this is mainly used to filter out white pixels
            //    CvInvoke.Threshold(s, s, 10, 255, ThresholdType.Binary);
            //    CvInvoke.BitwiseAnd(segmentedFrame, s, segmentedFrame, null);

                return segmentedFrame;
            //}

        }






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

        private Image<Gray, byte> Morphology(Image<Gray, byte> image, int dilateSize, int erodeSize, bool erode)
        {
            var dilateImage = image.Copy(image);

            Mat rec_Erode = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new System.Drawing.Size(erodeSize, erodeSize), new System.Drawing.Point(erodeSize / 2, erodeSize / 2));
            Mat rec_Dilate = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new System.Drawing.Size(dilateSize, dilateSize), new System.Drawing.Point(dilateSize / 2, dilateSize / 2));

            CvInvoke.Dilate(dilateImage, dilateImage, rec_Dilate, new System.Drawing.Point(1, 1), 2, BorderType.Default, new MCvScalar(0, 0, 0));
            if (erode)
            {
                CvInvoke.Erode(dilateImage, dilateImage, rec_Erode, new System.Drawing.Point(1, 1), 1, BorderType.Default, new MCvScalar(0, 0, 0));
            }
            return dilateImage;
        }

    }
}
