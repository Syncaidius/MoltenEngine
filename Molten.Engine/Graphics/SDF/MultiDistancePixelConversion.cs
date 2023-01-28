using Molten.DoublePrecision;

namespace Molten.Graphics.SDF
{
    public class MultiDistancePixelConversion
    {
        public MultiDistancePixelConversion(double range)
        {
            InvRange = 1.0 / range;
        }

        public unsafe void Convert(Color3* pixels, Color3D distance)
        {
            pixels->R = (float)(InvRange * distance.R + .5);
            pixels->G = (float)(InvRange * distance.G + .5);
            pixels->B = (float)(InvRange * distance.B + .5);
        }
        
        public double InvRange { get; }
    }
}
