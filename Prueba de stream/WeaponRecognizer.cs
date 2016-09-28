using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV.XFeatures2D;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

namespace Prueba_de_stream
{
    public class WeaponRecognizer
    {
        private List<Mat> weaponsTrained;
        private Mat backgroundFrame;

        public WeaponRecognizer(string dirPath, int widthImg, int heightImg)
        {
            weaponsTrained = EmguCvSurfLibrary.GetWeaponImages(dirPath, widthImg, heightImg);
        }

        public bool ProcessFrame(Bitmap image)
        {
            bool isWeaponDetected = false;
            long matchTime;                                     //Debug line
            Stopwatch watch = Stopwatch.StartNew();             //Debug line
            Mat sourceFrame = new Image<Gray, Byte>(image).Mat;
            Mat currentFrame = ImagePreProcessorAlgorithms.ProcessingImage(sourceFrame, backgroundFrame);
            weaponsTrained.ForEach(w =>
            {
                using (Mat modelWeaponImage = w)                  //Training image
                using (Mat observedCameraImage = currentFrame)    //Camera image
                {
                    if( SurfAlgorithm.Process(modelWeaponImage, observedCameraImage) )
                    {
                        isWeaponDetected = true;
                    }
                }
            });
            watch.Stop();                                       //Debug line
            matchTime = watch.ElapsedMilliseconds;              //Debug line
            return isWeaponDetected;
        }        
    }
}

