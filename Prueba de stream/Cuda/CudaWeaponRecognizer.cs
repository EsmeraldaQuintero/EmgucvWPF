using Emgu.CV;
using System.Collections.Generic;

namespace Prueba_de_stream.Cuda
{
    public class CudaWeaponRecognizer
    {
        private CudaSurfAlgorithm cudaSurfAlgorithm;
        private CudaSURFMatchAlgorithm cudaSURFMatchAlgorithm;
        private TestImagePreProcessorAlgorithm imagePreProcessorAlgorithm;
        private List<CudaSurfImage> weaponsTrained;
        private Mat backgroundFrame;

        public CudaWeaponRecognizer()
        {
            cudaSurfAlgorithm = new CudaSurfAlgorithm();
            cudaSURFMatchAlgorithm = new CudaSURFMatchAlgorithm();
            imagePreProcessorAlgorithm = new TestImagePreProcessorAlgorithm();

            int TRAIN_WIDTH = 640;
            int TRAIN_HEIGHT = 480;
            string path = "C:\\Users\\uabc\\Documents\\EmgucvWPF\\Prueba de stream\\training";
            weaponsTrained  = cudaSurfAlgorithm.LoadListOfWeaponsTrained(path, TRAIN_WIDTH, TRAIN_HEIGHT);
        }

        public bool ProcessFrame(System.Drawing.Bitmap image, CameraCalibration cameraCalibration)
        {
            bool isWeaponDetected = false;
            long matchTime;                                                                 //Debug line
            System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();   //Debug line

            using(Mat sourceFrame = new Image<Emgu.CV.Structure.Bgr, byte>(image).Mat)
            {
                Mat currentFrame = imagePreProcessorAlgorithm.ProcessingImage(sourceFrame, backgroundFrame, cameraCalibration);
                CudaSurfImage observedSurfImage = cudaSurfAlgorithm.GetSurfFeaturesOf(currentFrame);
                weaponsTrained.ForEach(weaponModel =>
                {
                    isWeaponDetected = cudaSURFMatchAlgorithm.Process(weaponModel, observedSurfImage);
                });
            }

            watch.Stop();                                       //Debug line
            matchTime = watch.ElapsedMilliseconds;              //Debug line
            return isWeaponDetected;
        }        
    }
}

