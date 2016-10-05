using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV.XFeatures2D;
using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Prueba_de_stream
{
    public class SurfAlgorithm
    {
        private const int K = 2;
        private const int ROTATION_BINS = 20;
        private const int RANSAC_THRESH = 2;
        private const int MIN_VALID_MATCHES = 4;
        private const int START_IDX = 0;
        private const int CHANNEL_MONO = 1;
        private const byte NOT_VALID_MATCH_VAL = 0;
        private const double VALID_MATCH_VAL = 255;
        private const double UNIQUENESS_THRESHOLD = 0.8;
        private const double HESSIAN_THRESH = 300;
        private const double SCALE_INCREMENT = 1.5;
        private const bool IGNORE_PROVIDED_KEYPOINS = false;

        public static bool Process(Mat modelWeaponImage, Mat observedCameraImage)
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

        private static bool FakeDetector(Mat homography, Size modelWeaponSize, Mat mask, Mat img)
        {
            bool isValid = false;
            if (homography != null)
            {
                bool isValidPoint = true;
                Point[] homographyPoints = GetHomographyPoints(modelWeaponSize, homography);
                foreach (var point in homographyPoints)
                    if (point.X < 0 || point.Y < 0)
                        isValidPoint = false;

                if (isValidPoint)
                {
                    using (VectorOfPoint vp = new VectorOfPoint(homographyPoints))
                    {
                        CvInvoke.Polylines(img, vp, true, new MCvScalar(255, 0, 0, 255), 5);
                    }

                    int minSizeX = img.Width / 2;
                    int minSizeY = img.Height / 2;
                    int nonZeroCountMatches = CvInvoke.CountNonZero(mask);
                    if ((homographyPoints[1].X - homographyPoints[0].X) > minSizeX && (homographyPoints[2].X - homographyPoints[3].X) > minSizeX)
                    {
                        if ((homographyPoints[3].Y - homographyPoints[0].Y) > minSizeY && (homographyPoints[2].Y - homographyPoints[1].Y) > minSizeY)
                        {
                            isValid = true;
                        }
                    }    
                }

            }
            return isValid;
        }

        private static bool FindMatch(SurfImage modelWeapon, SurfImage observedCamera, VectorOfVectorOfDMatch matches)
        {
            Mat homography;
            Mat mask;
            using (UMat uModelImage = modelWeapon.matImage.ToUMat(AccessType.Read))
            using (UMat uObservedImage = observedCamera.matImage.ToUMat(AccessType.Read))
            {
                SURF surfCPU = new SURF(HESSIAN_THRESH);

                //Extract features from the object image and  observed image
                surfCPU.DetectAndCompute(uModelImage, null, modelWeapon.keyPoints, modelWeapon.descriptors, IGNORE_PROVIDED_KEYPOINS);
                surfCPU.DetectAndCompute(uObservedImage, null, observedCamera.keyPoints, observedCamera.descriptors, IGNORE_PROVIDED_KEYPOINS);

                //Create the Matcher in order to get the matches
                BFMatcher matcher = new BFMatcher(DistanceType.L2);
                matcher.Add(modelWeapon.descriptors);
                matcher.KnnMatch(observedCamera.descriptors, matches, K, null);

                //Filtering matches
                mask = GetMask(matches);
                homography = GetHomography(modelWeapon, observedCamera, matches, mask);
            }
            return FakeDetector(homography, modelWeapon.matImage.Size, mask,observedCamera.matImage);      //Check if the surfaces are matched
        }

        private static Mat GetMask(VectorOfVectorOfDMatch matches)
        {
            Mat mask = new Mat(matches.Size, 1, DepthType.Cv8U, CHANNEL_MONO);
            mask.SetTo(new MCvScalar(VALID_MATCH_VAL));
            VoteForUniqueness(matches, UNIQUENESS_THRESHOLD, mask);
            return mask;
        }

        private static void VoteForUniqueness(VectorOfVectorOfDMatch matches, double uniquenessThreshold, Mat mask)
        {
            MDMatch[][] listOfMatches = matches.ToArrayOfArray();
            byte[] data = new byte[matches.Size];
            Marshal.Copy(mask.DataPointer, data, START_IDX, matches.Size);
            for (int i = 0; i < matches.Size ; i++)
            {
                if(listOfMatches[i][0].Distance < (uniquenessThreshold * listOfMatches[i][1].Distance) )
                {
                    data[i] = NOT_VALID_MATCH_VAL; //if the distance is too similiar, then elimilate the feature by set mask to 0
                }
            }
            Marshal.Copy(data, START_IDX, mask.DataPointer, matches.Size);
        }

        private static Mat GetHomography(SurfImage modelWeapon, SurfImage observedCamera, VectorOfVectorOfDMatch matches, Mat mask)
        {
            Mat homography = null;
            int nonZeroCountMatches = CvInvoke.CountNonZero(mask);
            if (nonZeroCountMatches >= (matches.Size - matches.Size/4))
            {
                int nonZeroCountValid = Features2DToolbox.VoteForSizeAndOrientation(modelWeapon.keyPoints, observedCamera.keyPoints,
                   matches, mask, SCALE_INCREMENT, ROTATION_BINS);
                if (nonZeroCountValid >= MIN_VALID_MATCHES)
                    homography = Features2DToolbox.GetHomographyMatrixFromMatchedFeatures(modelWeapon.keyPoints, observedCamera.keyPoints, matches, mask, RANSAC_THRESH);
            }
            return homography;
        }

        private static Point[] GetHomographyPoints(Size modelWeaponSize, Mat homography)
        {
            Rectangle rect = new Rectangle(Point.Empty, modelWeaponSize);
            PointF[] pts = new PointF[]
            {
                  new PointF(rect.Left, rect.Bottom),
                  new PointF(rect.Right, rect.Bottom),
                  new PointF(rect.Right, rect.Top),
                  new PointF(rect.Left, rect.Top)
            };
            pts = CvInvoke.PerspectiveTransform(pts, homography);
            return Array.ConvertAll<PointF, Point>(pts, Point.Round);
        }
    }
}
