using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Prueba_de_stream
{
    public class EmguCvSurfLibrary
    {
        private static List<Image<Gray, byte>> weaponImages;

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
            weaponImages = new List<Image<Gray, byte>>();

            if (imagepaths != null)
            {
                foreach (var imagepath in imagepaths)
                {
                    using (Image<Gray, byte> source = new Image<Gray, byte>(imagepath))
                    {
                        Image<Gray, byte> img = source.Copy().Resize(width, height, Emgu.CV.CvEnum.Inter.Cubic);
                        weaponImages.Add(img);
                    }
                }
            }
        }

        public static List<Image<Gray, Byte>> GetWeaponImages(string dirPath, int widthImg, int heightImg)
        {
            LoadImagesDataSetCPU(dirPath, widthImg, heightImg);
            return weaponImages;
        }
    }
}
