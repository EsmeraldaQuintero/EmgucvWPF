
namespace Prueba_de_stream.Cuda
{
    public class CameraCalibration
    {
        public int CameraId { get; set; }
        public virtual Camera Camera { get; set; }
        public int NoiseBG { get; set; }
        public int ClarifyBG { get; set; }
        public int ErodeBG { get; set; }
        public int DilateBG { get; set; }
        public double GaussianBlurVal { get; set; }
        public int MinHueForHSV { get; set; }
        public int MaxHueForHSV { get; set; }
        public int BeyondDilate { get; set; }
        public int BeyondErode { get; set; }
        public int TopErode { get; set; }
}
}
