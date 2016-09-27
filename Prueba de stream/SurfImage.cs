using Emgu.CV;
using Emgu.CV.Util;

namespace Odasoft.Biometrico.WeaponRecognition.Entities
{
    public class SurfImage
    {
        public VectorOfKeyPoint keyPoints;
        public Mat matImage;
        public UMat descriptors;

        public SurfImage(Mat matImage)
        {
            this.matImage = matImage;
            keyPoints = new VectorOfKeyPoint();
            descriptors = new UMat();
        }
    }
}
