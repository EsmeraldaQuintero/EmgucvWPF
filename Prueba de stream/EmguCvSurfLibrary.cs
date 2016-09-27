using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Odasoft.Biometrico.WeaponRecognition.Algorithms
{
    public class EmguCvSurfLibrary
    {
        private static List<Image<Gray, byte>> weaponImages;
        private const string DirPath = "C:\\odafolders\\WeaponImages";
        private const int TRAIN_WIDTH = 800;
        private const int TRAIN_HEIGHT = 600;

        private static IEnumerable<string> GetImagesPathsFromDirectory(string dirPath)
        {
            if ( Directory.Exists(dirPath))
            {
                return Directory.EnumerateFiles(dirPath).Where(f => f.EndsWith(".jpg") || f.EndsWith(".png"));
            }
            return null;
        }

        private static void LoadImagesDataSetCPU()
        {
            IEnumerable<string> imagepaths = GetImagesPathsFromDirectory(DirPath);
            weaponImages = new List<Image<Gray, byte>>();

            if (imagepaths != null)
            {
                foreach (var imagepath in imagepaths)
                {
                    using (Image<Gray, byte> source = new Image<Gray, byte>(imagepath))
                    {
                        Image<Gray, byte> img = source.Copy().Resize(TRAIN_WIDTH, TRAIN_HEIGHT, Emgu.CV.CvEnum.Inter.Cubic);
                        weaponImages.Add(img);
                    }
                }
            }
        }

        public static List<Image<Gray, Byte>> GetWeaponImages()
        {
            LoadImagesDataSetCPU();
            return weaponImages;
        }
    }
}
