using Emgu.CV.Util;
using Emgu.CV.Cuda;
using Emgu.CV;

namespace Prueba_de_stream.Cuda
{
    public class CudaSURFMatchAlgorithm
    {
        #region constants
        private const int K = 2;
        private const int CHANNEL_MONO = 1;
        private const int MATCHES_COL = 1;
        private const int START_IDX = 0;
        private const int POWER_2 = 2;
        private const double UNIQUENESS_THRESHOLD = 0.8;
        private const byte VALID_MATCH_VAL = 1;
        private const double PERCENTAGE_OF_DISTANCE = 0.25;
        #endregion constants

        CudaBFMatcher matcher;

        public CudaSURFMatchAlgorithm()
        {
            matcher = new CudaBFMatcher(Emgu.CV.Features2D.DistanceType.L2);
        }

        public bool Process(CudaSurfImage modelWeapon, CudaSurfImage observedCamera)
        {
            //Get the matches between the model image and the observed image
            VectorOfVectorOfDMatch matches = new VectorOfVectorOfDMatch();
            matcher.KnnMatch(observedCamera.gpuDescriptors, modelWeapon.gpuDescriptors, matches, K);
            //Validate matches
            if (matches.Size > modelWeapon.cpuKeyPoints.Size / 2)
            {
                Mat matchesMask = new Mat(matches.Size, MATCHES_COL, Emgu.CV.CvEnum.DepthType.Cv8U, CHANNEL_MONO);
                byte[] data = VoteForDistanceAndUniqueness(matches, modelWeapon, observedCamera);
                System.Runtime.InteropServices.Marshal.Copy(data, START_IDX, matchesMask.DataPointer, matches.Size);
                int nonZeroCountMatches = CvInvoke.CountNonZero(matchesMask);
                if (nonZeroCountMatches >= modelWeapon.cpuKeyPoints.Size / 10)
                    return true;
            }
            return false;
        }

        private byte[] VoteForDistanceAndUniqueness(VectorOfVectorOfDMatch matches, CudaSurfImage modelWeapon, CudaSurfImage observedCamera)
        {
            byte[] data = new byte[matches.Size];
            double distance_tresh = CalculateDistanceTresh(modelWeapon.gpuMatImage.Size.Height, modelWeapon.gpuMatImage.Size.Width);
            for (int i = START_IDX; i < matches.Size; i++)
            {
                for (int j = START_IDX; j < matches[i].Size; j++)
                {
                    System.Drawing.PointF from = modelWeapon.cpuKeyPoints[matches[i][j].TrainIdx].Point;
                    System.Drawing.PointF to = observedCamera.cpuKeyPoints[matches[i][j].QueryIdx].Point;
                    double dist = CalculateHypotenuseOfDistance(from, to);
                    if (dist < distance_tresh && (matches[i][0].Distance < UNIQUENESS_THRESHOLD * matches[i][1].Distance))
                    {
                        data[i] = VALID_MATCH_VAL;
                    }
                }
            }
            return data;
        }

        private double CalculateDistanceTresh(int height, int width)
        {
            return PERCENTAGE_OF_DISTANCE * System.Math.Sqrt(System.Math.Pow(height, POWER_2) + System.Math.Pow(width, POWER_2));
        }

        private double CalculateHypotenuseOfDistance(System.Drawing.PointF from, System.Drawing.PointF to)
        {
            double ssd_x = System.Math.Pow(from.X - to.X, POWER_2);
            double ssd_y = System.Math.Pow(from.Y - to.Y, POWER_2);
            return System.Math.Sqrt(ssd_x + ssd_y);
        }
    }
}
