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
        private const double MATCH_THRESH = 0.6;
        private const double HESSIAN_THRESH = 300;
        private const bool IGNORE_PROVIDED_KEYPOINS = false;
        private const int K = 2;
        private const int START_IDX = 0;
        private const byte VALID_MATCH_VAL = 1;
        private const byte NOT_VALID_MATCH_VAL = 0;

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
            double ssdAcumulator = CalculateMatch(matches);

            return (ssdAcumulator>MATCH_THRESH);
        }

        private static VectorOfVectorOfDMatch GetMatches(SurfImage modelWeapon, SurfImage observedCamera)
        {
            VectorOfVectorOfDMatch matches = new VectorOfVectorOfDMatch();
            BFMatcher matcher = new BFMatcher(DistanceType.L2);
            matcher.Add(modelWeapon.descriptors);
            matcher.KnnMatch(observedCamera.descriptors, matches, K, null);
            return matches;
        }

        private static Mat CalculateMatch(VectorOfVectorOfDMatch matches)
        {
            MDMatch[][] listOfMatches = matches.ToArrayOfArray();
            byte[] data = new byte[matches.Size];
            for (int i = 0; i < matches.Size; i++)
            {
                if( ValidSumSqrDiff(listOfMatches[i][0].Distance, listOfMatches[i][1].Distance))
                {
                    data[i] = VALID_MATCH_VAL;
                }
                else
                {
                    data[i] = NOT_VALID_MATCH_VAL;
                }

            }
            Mat mask = new Mat();
            Marshal.Copy(data, START_IDX, mask.DataPointer, matches.Size);
            return mask;
        }

        private static bool ValidSumSqrDiff(float distance1, float distance2)
        {
            double result = (distance1 - distance2) * (distance1 - distance2);
            return (result > MATCH_THRESH);
        }
    }
}
