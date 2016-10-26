﻿using Emgu.CV;
using Emgu.CV.Cvb;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prueba_de_stream.Cuda
{
    public class TestBlobAlgorithm
    {

        private const int MIN_AREA_FOR_BLOB = 1000;
        private const int MAX_AREA_FOR_BLOB = 10000;


        public static List<Mat> SplitImageByROI(Mat contourMask)
        {
            CvBlobDetector _blobDetector;
            _blobDetector = new CvBlobDetector();

            List<Mat> blobList = new List<Mat>();

            using ( Image<Gray, byte> contourImg = contourMask.ToImage<Gray, byte>() )
            using ( CvBlobs blobs = new CvBlobs() )
            {
                _blobDetector.Detect(contourImg, blobs);
                blobs.FilterByArea(MIN_AREA_FOR_BLOB,MAX_AREA_FOR_BLOB);
                foreach (var pair in blobs)
                {
                    Rectangle cropRectangle = pair.Value.BoundingBox;
                    contourImg.ROI = cropRectangle;
                    Mat newImg = new Mat();
                    contourImg.Mat.CopyTo(newImg);
                    blobList.Add(newImg);
                }

            }
            return blobList;
        }
    }
}