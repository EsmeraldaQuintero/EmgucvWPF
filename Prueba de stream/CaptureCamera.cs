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
            long matchTime;
            int TRAIN_WIDTH = 512;
            int TRAIN_HEIGHT = 384;

            Mat newFrame = new Mat();
            _capture.Retrieve(newFrame);
            Mat newModelFrame = CvInvoke.Imread("C:\\Users\\uabc\\Documents\\EmgucvWPF\\Prueba de stream\\Img1.jpg", LoadImageType.Grayscale);

            //var currentFrame = (newFrame.ToImage<Bgr, byte>().Resize(TRAIN_WIDTH, TRAIN_HEIGHT, Emgu.CV.CvEnum.Inter.Cubic)).Convert<Hsv, byte>();
            //var filterFrame = FilterImage(currentFrame);
            var originalFrame = newFrame.ToImage<Bgr, byte>().Resize(TRAIN_WIDTH, TRAIN_HEIGHT, Emgu.CV.CvEnum.Inter.Cubic);
            var currentFrame = newFrame.ToImage<Ycc, byte>().Resize(TRAIN_WIDTH, TRAIN_HEIGHT, Emgu.CV.CvEnum.Inter.Cubic);
            var bgFrame = backgroundFrame.ToImage<Ycc, byte>().Resize(TRAIN_WIDTH, TRAIN_HEIGHT, Emgu.CV.CvEnum.Inter.Cubic);
            var filterFrame = SubstracBackground(currentFrame, bgFrame);

            DisplayResult?.Invoke(filterFrame, 1000);

            //DisplayImages?.Invoke(
            //    originalFrame,
            //    bgFrame.Convert<Gray, byte>(),
            //    currentFrame.Convert<Gray,byte>(),
            //    filterFrame
            //    );

            var processFrame = newModelFrame.ToImage<Gray, byte>().Resize(TRAIN_WIDTH, TRAIN_HEIGHT, Emgu.CV.CvEnum.Inter.Cubic);

            //using (Mat modelImage = processFrame.Mat)
            //using (Mat observedImage = filterFrame.Mat)
            //{
            //    Mat result = DrawMatches.Draw(modelImage, observedImage, out matchTime);
            //    var resultFrame = result.ToImage<Gray, Byte>();
            //    DisplayResult?.Invoke(resultFrame, matchTime);
            //}
        }

        public Image<Gray, byte> SubstracBackground(Image<Ycc, byte> currentFrame, Image<Ycc, byte> bgFrame)
        {
            Image<Gray, Byte>[] channels1 = currentFrame.Split();
            Image<Gray, Byte> y1 = channels1[0];
            Image<Gray, Byte> cb1 = channels1[1];
            Image<Gray, Byte> cr1 = channels1[2];
            Image<Gray, byte> originFilter = cb1.InRange(new Gray(0), new Gray(68));


            var filterFrame = currentFrame.Convert<Ycc,byte>().AbsDiff(bgFrame);   //subtract background from image


            // applying filters to remove noise
            Image<Gray, Byte>[] channels = filterFrame.Split();
            Image<Gray, Byte> y = channels[0];
            Image<Gray, Byte> cb = channels[1];
            Image<Gray, Byte> cr = channels[2];
            Image<Gray, byte> yfilter = y.InRange(new Gray(context.Hue1), new Gray(context.Hue2));
            Image<Gray, byte> cbfilter = cb.InRange(new Gray(context.Sat1), new Gray(context.Sat2));
            Image<Gray, byte> crfilter = cr.InRange(new Gray(context.Brig1), new Gray(context.Brig2));

            ////apply erosion and dilation to get better results .
            //Image<Gray, byte> res = null;

            ////Eroding the source image using the specified structuring element
            //Mat rect_12 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new System.Drawing.Size(4, 4), new System.Drawing.Point(3, 3));
            //CvInvoke.Erode(yfilter, yfilter, rect_12, new System.Drawing.Point(1, 1), 1, BorderType.Default, new MCvScalar(0, 0, 0));
            //CvInvoke.Erode(crfilter, crfilter, rect_12, new System.Drawing.Point(1, 1), 1, BorderType.Default, new MCvScalar(0, 0, 0));
            //CvInvoke.Erode(cbfilter, cbfilter, rect_12, new System.Drawing.Point(1, 1), 1, BorderType.Default, new MCvScalar(0, 0, 0));

            ////dilating the source image using the specified structuring element
            //Mat rect_6 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new System.Drawing.Size(4, 4), new System.Drawing.Point(3, 3));
            //CvInvoke.Dilate(yfilter, yfilter, rect_6, new System.Drawing.Point(1, 1), 2, BorderType.Default, new MCvScalar(0, 0, 0));
            //CvInvoke.Dilate(crfilter, crfilter, rect_6, new System.Drawing.Point(1, 1), 2, BorderType.Default, new MCvScalar(0, 0, 0));
            //CvInvoke.Dilate(cbfilter, cbfilter, rect_6, new System.Drawing.Point(1, 1), 2, BorderType.Default, new MCvScalar(0, 0, 0));

            //Adding 3 channels
            //res = yfilter.Add(crfilter, cbfilter);
            //Image<Ycc, byte> res2;
            //res2 = res.Convert<Ycc, byte>();

            //adding mask of original ycrcb frame
            //res2 = res2.And(filterFrame);
            //CvInvoke.Erode(res2, res2, rect_12, new System.Drawing.Point(3, 3), 1, BorderType.Default, new MCvScalar(0, 0, 0));



            var mask = cbfilter.Or(yfilter).Or(crfilter);


            DisplayImages?.Invoke(
                originFilter.Convert<Gray, byte>(),
                yfilter.Convert<Gray, byte>(),
                cbfilter.Convert<Gray, byte>(),
                crfilter.Convert<Gray, byte>()
                );


            return mask.Convert<Gray, byte>();

            //return ColorSegmentation(filterFrame.Convert<Hsv, byte>(), context.Hue1, context.Sat1, context.Brig1);
            //return filterFrame.Convert<Gray,byte>();
        }

        public Image<Gray, byte> FilterImage(Image<Hsv, byte> currentFrame)
        {
            var currentFrameWithErode = ColorSegmentation(currentFrame, context.Hue1, context.Sat1, context.Brig1);
            var currentFrameWithDilate = ColorSegmentation(currentFrame, context.Hue2, context.Sat2, context.Brig2);
            //.SmoothGaussian(7, 7, 34.3, 45.3)
            currentFrameWithErode = Morphology(currentFrameWithErode, context.Dilate1, context.Erode1, true);
            currentFrameWithDilate = Morphology(currentFrameWithDilate, context.Dilate2, context.Erode2, false);

            var subFrame = currentFrameWithDilate.Sub(currentFrameWithErode);

            DisplayImages?.Invoke(currentFrame.Convert<Gray,byte>(), currentFrameWithErode, currentFrameWithDilate, subFrame);

            return subFrame;
        }

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

        private Image<Gray, byte> ColorSegmentation(Image<Hsv, byte> currentFrame, int hue, int sat, int brig)
        {
            Image<Gray, byte>[] channels = currentFrame.Split();
            var ch0 = channels[0];  //matiz
            var ch1 = channels[1];  //saturacion
            var ch2 = channels[2];  //brillo
            Image<Gray, byte> huefilter = ch0.InRange(new Gray(hue), new Gray(context.MaxHue));
            Image<Gray, byte> saturfilter = ch1.InRange(new Gray(sat), new Gray(context.MaxSat));
            Image<Gray, byte> brightnessFilter = ch2.InRange(new Gray(brig), new Gray(context.MaxBrig));
            Image<Gray, byte> result2 = huefilter.And(saturfilter);
            Image<Gray, byte> result3 = result2.And(brightnessFilter);

            return result3;
        }

    }
}
