using Emgu.CV;
using Emgu.CV.Util;

namespace Prueba_de_stream
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
