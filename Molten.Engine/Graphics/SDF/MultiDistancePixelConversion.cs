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

        public unsafe void Convert(Color3* pixels, MultiDistance distance)
        {
            pixels->R = (float)(InvRange * distance.r + .5);
            pixels->G = (float)(InvRange * distance.g + .5);
            pixels->B = (float)(InvRange * distance.b + .5);
        }
        
        public double InvRange { get; }
    }
}
