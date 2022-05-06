using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.SDF
{
    public class MultiDistancePixelConversion
    {
        public MultiDistancePixelConversion(double range)
        {
            InvRange = 1.0 / range;
        }

        public unsafe void Convert(float* pixels, MultiDistance distance)
        {
            pixels[0] = (float)(InvRange * distance.r + .5);
            pixels[1] = (float)(InvRange * distance.g + .5);
            pixels[2] = (float)(InvRange * distance.b + .5);
        }
        
        public double InvRange { get; }
    }
}
