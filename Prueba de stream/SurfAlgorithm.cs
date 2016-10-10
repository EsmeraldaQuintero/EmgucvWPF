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

        private const int   K = 2;
        private const int   START_IDX = 0;
        private const int   CHANNEL_MONO = 1;
        private const int   RANSAC_THRESH = 2;
        private const int   ROTATION_BINS = 20;
        private const int   MIN_VALID_MATCHES = 8;
        private const byte  VALID_MATCH_VAL = 1;
        private const byte  NOT_VALID_MATCH_VAL = 0;
        private const double MATCH_THRESH = 0.6;
        private const double SCALE_INCREMENT = 1.5;
        private const double UNIQUENESS_THRESHOLD = 0.8;
        private const double HESSIAN_THRESH = 300;
        private const bool  IGNORE_PROVIDED_KEYPOINS = false;

        private static SURF surfCPU;
        static SurfAlgorithm()
        {
            surfCPU = new SURF(HESSIAN_THRESH);
        }

        private static SurfImage GetSurfFeaturesOf(Mat image)
        {
            SurfImage surfModel = new SurfImage(image);
            using (UMat uMatImage = surfModel.matImage.ToUMat(AccessType.Read))
            {
                surfCPU.DetectAndCompute(uMatImage, null, surfModel.keyPoints, surfModel.descriptors, IGNORE_PROVIDED_KEYPOINS);
            }
            return surfModel;
        }

        public static bool Process(Mat modelWeaponImage, Mat observedCameraImage)
        {
            SurfImage modelWeapon = GetSurfFeaturesOf(modelWeaponImage);
            SurfImage observedCamera = GetSurfFeaturesOf(observedCameraImage);
            VectorOfVectorOfDMatch matches = GetMatches(modelWeapon, observedCamera);
            Mat matchesMask = new Mat(matches.Size, 1, DepthType.Cv8U, CHANNEL_MONO);

            Mat homography = null;
            bool result = false;
            if (GetMatchesMask(matches, matchesMask))
            {
                homography = GetHomography(modelWeapon, observedCamera, matches, matchesMask);
                if( homography!= null)
                {
                    Point[] pts = GetHomographyPoints(modelWeapon.matImage.Size,homography);
                    result = ValidArea(pts);
                }
            }
            return result;
        }

        private static VectorOfVectorOfDMatch GetMatches(SurfImage modelWeapon, SurfImage observedCamera)
        {
            VectorOfVectorOfDMatch matches = new VectorOfVectorOfDMatch();
            BFMatcher matcher = new BFMatcher(DistanceType.L2);
            matcher.Add(modelWeapon.descriptors);
            matcher.KnnMatch(observedCamera.descriptors, matches, K, null);
            return matches;
        }

        private static bool GetMatchesMask(VectorOfVectorOfDMatch matches, Mat mask)
        {
            MDMatch[][] arrayOfMatches = matches.ToArrayOfArray();
            byte[] data = new byte[matches.Size];
            double SSDacumulator = 0.0;
            for (int i = 0; i < matches.Size; i++)
            {
                SSDacumulator += diffSqr(arrayOfMatches[i][0].Distance, arrayOfMatches[i][1].Distance);
                if (ValidDistance(arrayOfMatches[i][0].Distance, arrayOfMatches[i][1].Distance))
                {
                    data[i] = VALID_MATCH_VAL;
                }
                else
                {
                    data[i] = NOT_VALID_MATCH_VAL;
                }
            }
            Marshal.Copy(data, START_IDX, mask.DataPointer, matches.Size);
            return (SSDacumulator > MATCH_THRESH);
        }

        private static double diffSqr(float distance1, float distance2)
        {
            return (distance1 - distance2) * (distance1 - distance2);
        }

        private static bool ValidDistance(float distance1, float distance2)
        {
            return (distance1 < MATCH_THRESH);
            //return (distance1 > (UNIQUENESS_THRESHOLD * distance2));
            //return ((distance1/distance2) > UNIQUENESS_THRESHOLD);
        }

        private static Mat GetHomography(SurfImage modelWeapon, SurfImage observedCamera, VectorOfVectorOfDMatch matches, Mat mask)
        {
            Mat homography = null;
            int nonZeroCountMatches = CvInvoke.CountNonZero(mask);
            if (nonZeroCountMatches >= matches.Size/2)
            {
                nonZeroCountMatches = Features2DToolbox.VoteForSizeAndOrientation(modelWeapon.keyPoints, observedCamera.keyPoints,
                   matches, mask, SCALE_INCREMENT, ROTATION_BINS);
                if (nonZeroCountMatches >= MIN_VALID_MATCHES)
                {
                    homography = Features2DToolbox.GetHomographyMatrixFromMatchedFeatures(modelWeapon.keyPoints, observedCamera.keyPoints, matches, mask, RANSAC_THRESH);
                    nonZeroCountMatches = CvInvoke.CountNonZero(mask);
                    return (nonZeroCountMatches >= MIN_VALID_MATCHES) ? homography : null;
                }
            }
            return null;
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

        private static bool ValidArea(Point[] pts)
        {
            double ca, co, h2, side1, side2, area1,area2;
            bool result=false;

            if ( (pts[1].X > 0 && pts[2].X > 0) && (pts[2].Y > 0 && pts[3].Y > 0))
            {
                ca = Math.Abs(pts[1].X - pts[0].X);
                co = Math.Abs(pts[1].Y - pts[0].Y);
                h2 = Math.Pow(ca, 2) + Math.Pow(co, 2);
                side1 = Math.Sqrt(h2);
                area2 = ca;

                ca = Math.Abs(pts[2].X - pts[1].X);
                co = Math.Abs(pts[2].Y - pts[1].Y);
                h2 = Math.Pow(ca, 2) + Math.Pow(co, 2);
                side2 = Math.Sqrt(h2);
                area1 = side1 * side2;
                area2 *= co;
                result = (area1 > area2 / 2);
            }
            return result;
        }
    }
}
