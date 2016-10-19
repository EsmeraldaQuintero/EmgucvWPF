namespace Prueba_de_stream.Cuda
{
    public class CudaSurfImage
    {
        public Emgu.CV.Util.VectorOfKeyPoint cpuKeyPoints;
        public Emgu.CV.Cuda.GpuMat gpuMatImage;
        public Emgu.CV.Cuda.GpuMat gpuKeyPoints;
        public Emgu.CV.Cuda.GpuMat gpuDescriptors;

        public CudaSurfImage(Emgu.CV.Mat matImage)
        {
            this.gpuMatImage = new Emgu.CV.Cuda.GpuMat(matImage);
            gpuKeyPoints = new Emgu.CV.Cuda.GpuMat();
            gpuDescriptors = new Emgu.CV.Cuda.GpuMat();
            cpuKeyPoints = new Emgu.CV.Util.VectorOfKeyPoint();
        }
    }
}
