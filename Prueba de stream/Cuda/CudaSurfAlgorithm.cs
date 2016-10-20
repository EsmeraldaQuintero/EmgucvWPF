using Emgu.CV;
using Emgu.CV.XFeatures2D;
using System.Collections.Generic;
using System.Linq;

namespace Prueba_de_stream.Cuda
{
    public class CudaSurfAlgorithm
    {
        private const float HESSIAN_THRESH = 300;
        CudaSURF cudaSurf;

        public CudaSurfAlgorithm()
        {
            cudaSurf = new CudaSURF(HESSIAN_THRESH);
        }

        public CudaSurfImage GetSurfFeaturesOf(Mat image)
        {
            CudaSurfImage surfModel = new CudaSurfImage(image);
            surfModel.gpuKeyPoints = cudaSurf.DetectKeyPointsRaw(surfModel.gpuMatImage, null);
            surfModel.gpuDescriptors = cudaSurf.ComputeDescriptorsRaw(surfModel.gpuMatImage, null, surfModel.gpuKeyPoints);
            cudaSurf.DownloadKeypoints(surfModel.gpuKeyPoints, surfModel.cpuKeyPoints);
            return surfModel;
        }

        public List<CudaSurfImage> LoadListOfWeaponsTrained(string dirPath, int widthImg, int heightImg)
        {
            IEnumerable<string> imagepaths = GetImagesPathsFromDirectory(dirPath);
            List<CudaSurfImage> weaponsTrained = new List<CudaSurfImage>();
            if (imagepaths != null)
            {
                foreach (var imagepath in imagepaths)
                {
                    using (Mat source = new Mat(imagepath, Emgu.CV.CvEnum.LoadImageType.Grayscale))
                    {
                        Mat img = source.Reshape(widthImg, heightImg);
                        CudaSurfImage cudaSurfImage = GetSurfFeaturesOf(img);
                        weaponsTrained.Add(cudaSurfImage);
                    }
                }
            }
            return weaponsTrained;
        }

        private static IEnumerable<string> GetImagesPathsFromDirectory(string dirPath)
        {
            if (System.IO.Directory.Exists(dirPath))
            {
                return System.IO.Directory.EnumerateFiles(dirPath).Where(f => f.EndsWith(".jpg") || f.EndsWith(".png"));
            }
            return null;
        }
    }
}
