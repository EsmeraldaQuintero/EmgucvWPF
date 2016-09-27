using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV.XFeatures2D;
using Odasoft.Biometrico.WeaponRecognition.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

namespace Odasoft.Biometrico.WeaponRecognition.Algorithms
{
    public class WeaponRecognizer
    {
        private List<Image<Gray, Byte>> weaponsTrained;
        private const int k = 2;
        private const double uniquenessThreshold = 0.8;
        private const double hessianThresh = 300;
        private const double scaleIncrement = 1.5;
        private const int rotationBins = 20;

        public WeaponRecognizer()
        {
            weaponsTrained = EmguCvSurfLibrary.GetWeaponImages();
        }

        public bool ProcessFrame(Bitmap image)
        {
            bool isWeaponDetected = false;
            var currentFrame = new Image<Gray, Byte>(image);
            long matchTime;                                     //Debug line
            Stopwatch watch = Stopwatch.StartNew();             //Debug line
            weaponsTrained.ForEach(w =>
            {
                using (Mat modelWeaponImage = w.Mat)                  //Training image
                using (Mat observedCameraImage = currentFrame.Mat)    //Camera image
                {
                    if( SurfProcess(modelWeaponImage, observedCameraImage))
                    {
                        isWeaponDetected = true;
                    }
                }
            });
            watch.Stop();                                       //Debug line
            matchTime = watch.ElapsedMilliseconds;              //Debug line
            return isWeaponDetected;
        }

        public bool SurfProcess(Mat modelWeaponImage, Mat observedCameraImage)
        {
            SurfImage modelWeapon = new SurfImage(modelWeaponImage);
            SurfImage observedCamera = new SurfImage(observedCameraImage);
            bool isMatched = false;
            using (VectorOfVectorOfDMatch matches = new VectorOfVectorOfDMatch())
            {
                isMatched = FindMatch(modelWeapon, observedCamera, matches);
            }
            return isMatched;
        }

        public bool FindMatch(SurfImage modelWeapon, SurfImage observedCamera, VectorOfVectorOfDMatch matches)
        {
            Mat homography = null;
            Mat mask;

            using (UMat uModelImage = modelWeapon.matImage.ToUMat(AccessType.Read))
            using (UMat uObservedImage = observedCamera.matImage.ToUMat(AccessType.Read))
            {
                SURF surfCPU = new SURF(hessianThresh);

                //extract features from the object image and  observed image
                surfCPU.DetectAndCompute(uModelImage, null, modelWeapon.keyPoints, modelWeapon.descriptors, false);
                surfCPU.DetectAndCompute(uObservedImage, null, observedCamera.keyPoints, observedCamera.descriptors, false);

                //Match
                BFMatcher matcher = new BFMatcher(DistanceType.L2);
                matcher.Add(modelWeapon.descriptors);
                matcher.KnnMatch(observedCamera.descriptors, matches, k, null);

                //Match filter
                mask = new Mat(matches.Size, 1, DepthType.Cv8U, 1);
                mask.SetTo(new MCvScalar(255));
                Features2DToolbox.VoteForUniqueness(matches, uniquenessThreshold, mask);

                int nonZeroCount = CvInvoke.CountNonZero(mask);
                if (nonZeroCount >= 4)
                {
                    nonZeroCount = Features2DToolbox.VoteForSizeAndOrientation(modelWeapon.keyPoints, observedCamera.keyPoints,
                       matches, mask, scaleIncrement, rotationBins);
                    if (nonZeroCount >= 4)
                        homography = Features2DToolbox.GetHomographyMatrixFromMatchedFeatures(modelWeapon.keyPoints,
                           observedCamera.keyPoints, matches, mask, 2);
                }

            }

            return (homography != null);        //Check if the surfaces are matched
        }
    }
}

