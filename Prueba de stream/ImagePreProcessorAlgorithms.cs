using System;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV;

namespace Prueba_de_stream
{
    public class ImagePreProcessorAlgorithms
    {
        private static ContextSurf context = ContextSurf.Instance;

        public static Mat ProcessingImage(Mat currentFrame, Mat backgroundFrame)
        {
            Mat withoutBackgroundMask = new Mat();
            Mat segmentedMask = new Mat();
            Mat maskAnd = new Mat();
            Mat contourMask = new Mat();

            if (backgroundFrame != null && !backgroundFrame.IsEmpty)
            {
                if (!currentFrame.IsEmpty)
                {
                    withoutBackgroundMask = BackgroundRemover(backgroundFrame, currentFrame);
                }

                if (!withoutBackgroundMask.IsEmpty)
                {
                    segmentedMask = SegmentationFilter(currentFrame);
                }

                if (!segmentedMask.IsEmpty)
                {
                    segmentedMask.CopyTo(maskAnd, withoutBackgroundMask);
                    contourMask = MorphologyFilter(maskAnd);
                }
            }

            return contourMask.IsEmpty ? currentFrame : contourMask;
        }

        public static Mat BackgroundRemover(Mat bgFrame, Mat currentFrame)
        {
            using (Mat bgMat = new Mat())
            using (Mat currentMat = new Mat())
            {
                CvInvoke.CvtColor(bgFrame, bgMat, ColorConversion.Bgr2YCrCb);
                CvInvoke.CvtColor(currentFrame, currentMat, ColorConversion.Bgr2YCrCb);
                using (var bgImage = bgMat.ToImage<Ycc, byte>().Resize(context.TrainWidth, context.TrainHeight, Emgu.CV.CvEnum.Inter.Cubic))
                using (var currentImage = currentMat.ToImage<Ycc, byte>().Resize(context.TrainWidth, context.TrainHeight, Emgu.CV.CvEnum.Inter.Cubic))
                {
                    //Creating mask for remove background
                    Ycc thr = new Ycc(context.ClarifyBG, 0, 0);
                    var clarifyBgImage = bgImage.ThresholdToZero(thr);          //clarify background in order to eliminate black zones
                    var maskBgImage = clarifyBgImage.AbsDiff(currentImage);     //subtract background from image

                    //Applying filters to remove noise
                    Image<Gray, Byte>[] channels = maskBgImage.Split();
                    Image<Gray, Byte> y = channels[0];
                    var yfilter = y.InRange(new Gray(0), new Gray(context.NoiseBG));

                    //Applying mask
                    Mat mask = new Mat();
                    Mat filter = ErodeImage(yfilter.Not().Mat, context.ErodeBG);
                    CvInvoke.CvtColor(filter, mask, ColorConversion.Gray2Bgr);
                    CvInvoke.CvtColor(mask, mask, ColorConversion.Bgr2Gray);
                    return mask;
                }
            }
        }

        public static Mat SegmentationFilter(Mat withoutBackgroundFrame)
        {
            using (Mat smoothFrame = new Mat())
            using (Mat hsvFrame = new Mat())
            {
                //Appliying Gaussian blur for reduce noise on the image
                System.Drawing.Size pxDiameter = new System.Drawing.Size(context.RadiusGussianBlur, context.RadiusGussianBlur);
                CvInvoke.GaussianBlur(withoutBackgroundFrame, smoothFrame, pxDiameter, context.GaussianBlurVal, context.GaussianBlurVal);

                //Segmenting image in HSV color
                CvInvoke.CvtColor(smoothFrame, hsvFrame, ColorConversion.Bgr2Hsv);
                Image<Gray, byte>[] channels = hsvFrame.ToImage<Hsv, byte>().Split();
                var ch0 = channels[0];  //Hue channel
                var ch2 = channels[2];  //Value channel

                //Selecting color range for the mask
                Image<Gray, byte> huefilter = ch0.InRange(new Gray(context.MinHueForHSV), new Gray(context.MaxHueForHSV)).Resize(context.TrainWidth, context.TrainHeight, Emgu.CV.CvEnum.Inter.Cubic);
                Image<Gray, byte> valfilter = ch2.InRange(new Gray(0), new Gray(55)).Resize(context.TrainWidth, context.TrainHeight, Emgu.CV.CvEnum.Inter.Cubic);
                huefilter = huefilter.Or(valfilter);

                //Creating mask
                Mat mask = new Mat();
                mask = DilateImage(huefilter.Mat, context.BeyondDilate);
                mask = ErodeImage(mask, context.BeyondErode);
                CvInvoke.CvtColor(mask, mask, ColorConversion.Gray2Bgr);
                CvInvoke.CvtColor(mask, mask, ColorConversion.Bgr2Gray);
                return mask;
            }
        }

        public static Mat MorphologyFilter(Mat filterMask)
        {
            Mat beyondMask = new Mat();
            Mat topMask = new Mat();

            topMask = filterMask;
            beyondMask = ErodeImage(filterMask, context.TopErode);

            Mat contoursFrame = new Mat();
            CvInvoke.Subtract(topMask, beyondMask, contoursFrame, null, DepthType.Default);

            return contoursFrame;
        }

        private static Mat ErodeImage(Mat frame, int erodeSize)
        {
            Mat erodeFrame = new Mat();
            Mat rect_12 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new System.Drawing.Size(erodeSize, erodeSize), new System.Drawing.Point(erodeSize / 2, erodeSize / 2));
            CvInvoke.Erode(frame, erodeFrame, rect_12, new System.Drawing.Point(1, 1), 1, BorderType.Default, new MCvScalar(0, 0, 0));
            return erodeFrame;
        }

        private static Mat DilateImage(Mat frame, int dilateSize)
        {
            Mat dilateFrame = new Mat();
            Mat rect_6 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new System.Drawing.Size(dilateSize, dilateSize), new System.Drawing.Point(dilateSize / 2, dilateSize / 2));
            CvInvoke.Dilate(frame, dilateFrame, rect_6, new System.Drawing.Point(1, 1), 2, BorderType.Default, new MCvScalar(0, 0, 0));
            return dilateFrame;
        }
    }
}
