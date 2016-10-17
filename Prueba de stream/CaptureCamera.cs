﻿using System;
using System.Windows;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV;
using System.Collections.Generic;
using Microsoft.Win32;
using System.IO;

namespace Prueba_de_stream
{
    public class CaptureCamera
    {
        public ContextSurf context;
        Mat backgroundFrame;

        private Capture _capture;
        private bool _ready;

        public delegate void DisplayResultEventHandler(Image<Bgr, byte> resultFrame, long matchTime);
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

        public Mat saveImg = null;
        public void SaveScreenshot()
        {
            string code = Guid.NewGuid().ToString();
            string path = "C:\\Users\\uabc\\Documents\\EmgucvWPF\\Prueba de stream\\TrainedImg\\" + code + ".png";
            if(saveImg!= null && !saveImg.IsEmpty)
            {
                saveImg.Save(path);
            }

        }

        Mat imgOpenFrame = new Mat();
        public void OpenImage()
        {
            string filterString = "Image Files(*.BMP; *.JPG)| *.BMP; *.JPG";
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = filterString;
            openFileDialog.FilterIndex = 1;
            openFileDialog.InitialDirectory = "C:\\Users\\uabc\\Documents\\EmgucvWPF\\Prueba de stream\\OriginalImg";
            openFileDialog.ShowDialog();

            if ( !string.IsNullOrEmpty(openFileDialog.FileName) )
            {
                imgOpenFrame = CvInvoke.Imread(openFileDialog.FileName, LoadImageType.Color);
            }
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

            if (!currentFrame.IsEmpty)
            {
                withoutBackgroundMask = ImagePreProcessorAlgorithms.BackgroundRemover(backgroundFrame, currentFrame);
            }

            if (!withoutBackgroundMask.IsEmpty)
            {
                segmentedMask = ImagePreProcessorAlgorithms.SegmentationFilter(currentFrame);
            }

            if (!segmentedMask.IsEmpty)
            {
                segmentedMask.CopyTo(maskAnd, withoutBackgroundMask);
                filterMask = ImagePreProcessorAlgorithms.MorphologyFilter(maskAnd);
            }

            if (!filterMask.IsEmpty)
            {
                Image<Gray, byte> img1 = null;
                Image<Gray, byte> img2 = null;
                Image<Gray, byte> img3 = null;
                Image<Gray, byte> img4 = null;

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

                List<Mat> modelList = GetModels();
                List<Mat> blobList = BlobAlgorithm.SplitImageByROI(filterMask);

                Mat model1 = CvInvoke.Imread("C:\\Users\\uabc\\Documents\\EmgucvWPF\\Prueba de stream\\training\\10.png", LoadImageType.Grayscale);

                foreach (var model in modelList)
                {
                    if (blobList.Count > 0 && SurfAlgorithm.Process(model1, blobList[0]))
                    {
                        Image<Bgr, byte> resultImg = new Image<Bgr, byte>(blobList[0].Size);
                        resultImg = blobList[0].ToImage<Bgr, byte>();
                        DisplayResult?.Invoke(resultImg, 100);
                    }
                }

                //foreach (var img in blobList)
                //{
                //    Image<Bgr, byte> resultImg = new Image<Bgr, byte>(img.Size);
                //    resultImg = img.ToImage<Bgr, byte>();
                //    foreach (var model in modelList)
                //    {
                //        if (SurfAlgorithm.Process(model, img))
                //        {
                //            DisplayResult?.Invoke(resultImg, 100);
                //        }
                //    }
                //}

            }
            catch (ArgumentException ae) { }


            //if (!imgOpenFrame.IsEmpty)
            //{
            //    segmentedMask = ImagePreProcessorAlgorithms.SegmentationFilter(imgOpenFrame);
            //}
            //if (!segmentedMask.IsEmpty)
            //{
            //    filterMask = ImagePreProcessorAlgorithms.MorphologyFilter(segmentedMask);
            //}

            //if (!filterMask.IsEmpty)
            //{
            //    Image<Gray, byte> img1 = null;
            //    Image<Gray, byte> img2 = null;
            //    Image<Gray, byte> img3 = null;
            //    Image<Gray, byte> img4 = null;

            //    try
            //    {
            //        img1 = imgOpenFrame.ToImage<Gray, byte>();
            //        img2 = segmentedMask.ToImage<Gray, byte>();
            //        img3 = filterMask.ToImage<Gray, byte>();
            //        img4 = filterMask.ToImage<Gray, byte>();
            //        saveImg = filterMask;


            //        DisplayImages?.Invoke(img1, img2, img3, img4);
            //        DisplayResult?.Invoke(filterMask.ToImage<Bgr, byte>(), 100);
            //    }

            //    finally
            //    {
            //        if (img1 != null)
            //            ((IDisposable)img1).Dispose();
            //        if (img2 != null)
            //            ((IDisposable)img2).Dispose();
            //        if (img3 != null)
            //            ((IDisposable)img3).Dispose();
            //        if (img4 != null)
            //            ((IDisposable)img4).Dispose();
            //    }
            //}
        }

        private List<Mat> GetModels()
        {
            List<Mat> modelList = new List<Mat>();
            for (int i = 1; i < 10; i++)
            {
                string path = "C:\\Users\\uabc\\Documents\\EmgucvWPF\\Prueba de stream\\training\\" + i + ".png";
                Mat modelFrame = CvInvoke.Imread(path, LoadImageType.Grayscale);
                if (modelFrame != null && !modelFrame.IsEmpty)
                    modelList.Add(modelFrame);
            }
            return modelList;
        }
    }
}
