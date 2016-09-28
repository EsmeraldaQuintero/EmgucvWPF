using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Prueba_de_stream
{
    public class EmguCvSurfLibrary
    {
        private static List<Mat> weaponImages;

        private static IEnumerable<string> GetImagesPathsFromDirectory(string dirPath)
        {
            if ( Directory.Exists(dirPath))
            {
                return Directory.EnumerateFiles(dirPath).Where(f => f.EndsWith(".jpg") || f.EndsWith(".png"));
            }
            return null;
        }

        private static void LoadImagesDataSetCPU(string dirPath, int width, int height)
        {
            IEnumerable<string> imagepaths = GetImagesPathsFromDirectory(dirPath);
            weaponImages = new List<Mat>();

            if (imagepaths != null)
            {
                foreach (var imagepath in imagepaths)
                {
                    using (Mat source = new Mat(imagepath, LoadImageType.Grayscale))
                    {
                        Mat img = source.Reshape(width, height);
                        weaponImages.Add(img);
                    }
                }
            }
        }

        public static List<Mat> GetWeaponImages(string dirPath, int widthImg, int heightImg)
        {
            LoadImagesDataSetCPU(dirPath, widthImg, heightImg);
            return weaponImages;
        }
    }
}
