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
        private const int START_IDX = 0;
        private const int CHANNEL_MONO = 1;
        private const int ROTATION_BINS = 20;
        private const byte VALID_MATCH_VAL = 1;
        private const double HESSIAN_THRESH = 300;
        private const double SCALE_INCREMENT = 1.5;
        private const double UNIQUENESS_THRESHOLD = 0.8;
        private const bool IGNORE_PROVIDED_KEYPOINS = false;

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

            if (matches.Size > modelWeapon.keyPoints.Size / 2)
            {
                byte[] data = VoteForDistanceAndUniqueness(matches, modelWeapon, observedCamera);
                Marshal.Copy(data, START_IDX, matchesMask.DataPointer, matches.Size);
                int nonZeroCountMatches = CvInvoke.CountNonZero(matchesMask);
                if (nonZeroCountMatches >= modelWeapon.keyPoints.Size / 4)
                {
                    nonZeroCountMatches = Features2DToolbox.VoteForSizeAndOrientation(modelWeapon.keyPoints, observedCamera.keyPoints,
                    matches, matchesMask, SCALE_INCREMENT, ROTATION_BINS);
                    return true;
                }
            }
            return false;
        }

        private static VectorOfVectorOfDMatch GetMatches(SurfImage modelWeapon, SurfImage observedCamera)
        {
            VectorOfVectorOfDMatch matches = new VectorOfVectorOfDMatch();
            BFMatcher matcher = new BFMatcher(DistanceType.L2);
            matcher.Add(modelWeapon.descriptors);
            matcher.KnnMatch(observedCamera.descriptors, matches, K, null);
            return matches;
        }

        private static byte[] VoteForDistanceAndUniqueness(VectorOfVectorOfDMatch matches, SurfImage modelWeapon, SurfImage observedCamera)
        {
            byte[] data = new byte[matches.Size];
            double distance_tresh = 0.25 * Math.Sqrt(Math.Pow(modelWeapon.matImage.Height, 2) + Math.Pow(modelWeapon.matImage.Size.Width, 2));
            for (int i = 0; i < matches.Size; i++)
            {
                for (int j = 0; j < matches[i].Size; j++)
                {
                    PointF from = modelWeapon.keyPoints[matches[i][j].TrainIdx].Point;
                    PointF to = observedCamera.keyPoints[matches[i][j].QueryIdx].Point;
                    double dist = CalculateHypotenuseOfDistance(from, to);

                    if (dist < distance_tresh && (matches[i][0].Distance < UNIQUENESS_THRESHOLD * matches[i][1].Distance) )
                    {
                        data[i] = VALID_MATCH_VAL;
                    }
                }
            }
            return data;
        }

        private static double CalculateHypotenuseOfDistance(PointF from, PointF to)
        {
            double ssd_x = Math.Pow(from.X - to.X, 2);
            double ssd_y = Math.Pow(from.Y - to.Y, 2);
            return Math.Sqrt(ssd_x + ssd_y);
        }

    }
}
