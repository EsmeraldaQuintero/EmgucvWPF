using Emgu.CV;
using Emgu.CV.Cuda;
using Emgu.CV.Util;
using Emgu.CV.XFeatures2D;

namespace Prueba_de_stream.Cuda
{
    public class CudaSurfAlgorithm
    {
        #region constants
        private const float HESSIAN_THRESH = 300;
        private const int K = 2;
        private const int CHANNEL_MONO = 1;
        private const int START_IDX = 0;
        private const int POWER_2 = 2;
        private const double UNIQUENESS_THRESHOLD = 0.8;
        private const byte VALID_MATCH_VAL = 1;

        #endregion constants
        #region properties
        CudaSURF cudaSurf;
        CudaBFMatcher matcher;
        #endregion properties

        public CudaSurfAlgorithm()
        {
            cudaSurf = new CudaSURF(HESSIAN_THRESH);
            matcher = new CudaBFMatcher(Emgu.CV.Features2D.DistanceType.L2);
        }

        private CudaSurfImage GetSurfFeaturesOf(Mat image)
        {
            CudaSurfImage surfModel = new CudaSurfImage(image);
            surfModel.gpuKeyPoints = cudaSurf.DetectKeyPointsRaw(surfModel.gpuMatImage, null);
            surfModel.gpuDescriptors = cudaSurf.ComputeDescriptorsRaw(surfModel.gpuMatImage, null, surfModel.gpuKeyPoints);
            cudaSurf.DownloadKeypoints(surfModel.gpuKeyPoints, surfModel.cpuKeyPoints);
            return surfModel;
        }

        public bool Process(CudaSurfImage modelWeapon, CudaSurfImage observedCamera)
        {
            //Get the matches between the model image and the observed image
            VectorOfVectorOfDMatch matches = new VectorOfVectorOfDMatch();
            matcher.KnnMatch(observedCamera.gpuDescriptors, modelWeapon.gpuDescriptors, matches, K);

            Mat matchesMask = new Mat(matches.Size, 1, Emgu.CV.CvEnum.DepthType.Cv8U, CHANNEL_MONO);
            if (matches.Size > modelWeapon.cpuKeyPoints.Size / 2)
            {
                byte[] data = VoteForDistanceAndUniqueness(matches, modelWeapon, observedCamera);
                System.Runtime.InteropServices.Marshal.Copy(data, START_IDX, matchesMask.DataPointer, matches.Size);
                int nonZeroCountMatches = CvInvoke.CountNonZero(matchesMask);
                if (nonZeroCountMatches >= modelWeapon.cpuKeyPoints.Size / 4)
                    return true;
            }
            return false;
        }

        private byte[] VoteForDistanceAndUniqueness(VectorOfVectorOfDMatch matches, CudaSurfImage modelWeapon, CudaSurfImage observedCamera)
        {
            byte[] data = new byte[matches.Size];
            double distance_tresh = CalculateDistanceTresh(modelWeapon.gpuMatImage.Size.Height, modelWeapon.gpuMatImage.Size.Width);
            for (int i = 0; i < matches.Size; i++)
            {
                for (int j = 0; j < matches[i].Size; j++)
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
            return 0.25 * System.Math.Sqrt(System.Math.Pow(height, POWER_2) + System.Math.Pow(width, POWER_2));
        }

        private double CalculateHypotenuseOfDistance(System.Drawing.PointF from, System.Drawing.PointF to)
        {
            double ssd_x = System.Math.Pow(from.X - to.X, POWER_2);
            double ssd_y = System.Math.Pow(from.Y - to.Y, POWER_2);
            return System.Math.Sqrt(ssd_x + ssd_y);
        }
    }
}
