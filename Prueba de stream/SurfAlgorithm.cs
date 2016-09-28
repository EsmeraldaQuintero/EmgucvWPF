using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV.XFeatures2D;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;

namespace Prueba_de_stream
{
    public class SurfAlgorithm
    {
        private const int K = 2;
        private const double UNIQUENESS_THRESHOLD = 0.8;
        private const double HESSIAN_THRESH = 300;
        private const double SCALE_INCREMENT = 1.5;
        private const int ROTATION_BINS = 20;
        private const int RANSAC_THRESH = 2;

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

        private static bool FindMatch(SurfImage modelWeapon, SurfImage observedCamera, VectorOfVectorOfDMatch matches)
        {
            using (UMat uModelImage = modelWeapon.matImage.ToUMat(AccessType.Read))
            using (UMat uObservedImage = observedCamera.matImage.ToUMat(AccessType.Read))
            {
                SURF surfCPU = new SURF(HESSIAN_THRESH);

                //Extract features from the object image and  observed image
                surfCPU.DetectAndCompute(uModelImage, null, modelWeapon.keyPoints, modelWeapon.descriptors, false);
                surfCPU.DetectAndCompute(uObservedImage, null, observedCamera.keyPoints, observedCamera.descriptors, false);

                //Create the Matcher in order to get the matches for mask
                BFMatcher matcher = new BFMatcher(DistanceType.L2);
                matcher.Add(modelWeapon.descriptors);
                matcher.KnnMatch(observedCamera.descriptors, matches, K, null);
                Mat mask = GetMask(matches);

                //homography
                Mat homography = GetHomography(modelWeapon, observedCamera, matches, mask);
                Point[] homographyPoints = GetHomographyPoints(modelWeapon.matImage.Size,homography);
            }

            return false;        //Check if the surfaces are matched
        }

        private static Mat GetMask(VectorOfVectorOfDMatch matches)
        {
            Mat mask = new Mat(matches.Size, 1, DepthType.Cv8U, 1);
            mask.SetTo(new MCvScalar(255));
            VoteForUniqueness(matches, UNIQUENESS_THRESHOLD, mask);
            return mask;
        }

        private static void VoteForUniqueness(VectorOfVectorOfDMatch matches, double uniquenessThreshold, Mat mask)
        {
            List<MDMatch[]> list = matches.ToArrayOfArray().Cast<MDMatch[]>().ToList();
            byte[] data = new byte[list.Count];
            Marshal.Copy(mask.DataPointer, data, 0, list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                if ((list[i][0].Distance / list[i][1].Distance) <= uniquenessThreshold)
                {
                    data[i] = 0; //if the distance is too similiar, then elimilate the feature by set mask to 0
                }
            }
            Marshal.Copy(data, 0, mask.DataPointer, list.Count);
        }

        private static Mat GetHomography(SurfImage modelWeapon, SurfImage observedCamera, VectorOfVectorOfDMatch matches, Mat mask)
        {
            Mat homography = null;
            int nonZeroCount = CvInvoke.CountNonZero(mask);
            if (nonZeroCount >= 4)
            {
                nonZeroCount = Features2DToolbox.VoteForSizeAndOrientation(modelWeapon.keyPoints, observedCamera.keyPoints,
                   matches, mask, SCALE_INCREMENT, ROTATION_BINS);
                if (nonZeroCount >= 4)
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
