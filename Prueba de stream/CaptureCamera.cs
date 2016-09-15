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
        public Capture _capture = null;
        public ContextSurf context;
        public Mat backgroundFrame;

        public delegate void DisplayResultEventHandler(Image<Bgr, Byte> resultFrame, long matchTime);
        public event DisplayResultEventHandler DisplayResult;

        public delegate void DisplayImagesEventHandler(Image<Hsv, Byte> currentFrame, Image<Gray, Byte> minFrame, Image<Gray, Byte> maxFrame, Image<Gray, Byte> subFrame);
        public event DisplayImagesEventHandler DisplayImages;

        public CaptureCamera()
        {
            CvInvoke.UseOpenCL = false;
            backgroundFrame = new Mat();
            context = new ContextSurf();

            if (_capture == null)   //if camera capture hasn't been created, then created one
            {
                try
                {   //Creating the camera capture
                    //_capture = new Capture("http://192.168.1.99/mjpg/video.mjpg");
                    _capture = new Capture();
                    _capture.ImageGrabbed += ProcessFrame;
                }
                catch (NullReferenceException excpt)
                {   //show errors if there is any
                    MessageBox.Show(excpt.Message);
                }
            }
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
            int TRAIN_WIDTH = 400;
            int TRAIN_HEIGHT = 300;

            //originalCurrentFrame._EqualizeHist();      //contraste
            //originalCurrentFrame._GammaCorrect(1.4d);

            Mat newFrame = new Mat();
            _capture.Retrieve(newFrame);
            //Mat newFrame = CvInvoke.Imread("C:\\Users\\uabc\\Documents\\EmgucvWPF\\Prueba de stream\\Img1.jpg", LoadImageType.Grayscale);
            Mat newModelFrame = CvInvoke.Imread("C:\\Users\\uabc\\Documents\\EmgucvWPF\\Prueba de stream\\Img6.jpg", LoadImageType.Grayscale);

            var currentFrame = (newFrame.ToImage<Bgr, byte>().Resize(TRAIN_WIDTH, TRAIN_HEIGHT, Emgu.CV.CvEnum.Inter.Cubic)).Convert<Hsv, byte>();
            var processFrame = (newModelFrame.ToImage<Bgr, byte>().Resize(TRAIN_WIDTH, TRAIN_HEIGHT, Emgu.CV.CvEnum.Inter.Cubic)).Convert<Hsv, byte>();

            //var filterFrame = FilterImage(currentFrame);

            var filterFrame = SubstracBackground(currentFrame);

            using (Mat modelImage = processFrame.Mat)
            using (Mat observedImage = filterFrame.Mat)
            {
                Mat result = DrawMatches.Draw(modelImage, observedImage, out matchTime);
                var resultFrame = result.ToImage<Bgr, Byte>();
                DisplayResult?.Invoke(resultFrame, matchTime);
            }
        }

        public Image<Gray, byte> SubstracBackground(Image<Hsv, byte> currentFrame)
        {

            return new Image<Gray, byte>(currentFrame.Size);
        }

        public Image<Gray, byte> FilterImage(Image<Hsv, byte> currentFrame)
        {
            var currentFrameWithErode = ColorSegmentation(currentFrame, context.Hue1, context.Sat1, context.Brig1);
            var currentFrameWithDilate = ColorSegmentation(currentFrame, context.Hue2, context.Sat2, context.Brig2);
            //.SmoothGaussian(7, 7, 34.3, 45.3)
            currentFrameWithErode = Morphology(currentFrameWithErode, context.Dilate1, context.Erode1, true);
            currentFrameWithDilate = Morphology(currentFrameWithDilate, context.Dilate2, context.Erode2, false);

            var subFrame = currentFrameWithDilate.Sub(currentFrameWithErode);

            DisplayImages?.Invoke(currentFrame, currentFrameWithErode, currentFrameWithDilate, subFrame);

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
