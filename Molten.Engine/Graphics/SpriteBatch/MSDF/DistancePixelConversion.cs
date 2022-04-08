using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.SpriteBatch.MSDF
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="DT">Distance type</typeparam>
    public abstract class DistancePixelConversion<DT>
        where DT : struct
    {
        protected DistancePixelConversion(double range)
        {
            InvRange = 1.0 / range;
        }

        public unsafe abstract void Convert(float* pixels, DT distance);

        public double InvRange { get; }

        public abstract int NPerPixel { get; }
    }

    public class DoubleDistancePixelConversion : DistancePixelConversion<double>
    {
        public DoubleDistancePixelConversion(double range) : base(range) { }

        public override unsafe void Convert(float* pixels, double distance)
        {
            *pixels = (float)(InvRange * distance + .5);
        }

        public override int NPerPixel => 1;
    }

    public class MultiDistancePixelConversion : DistancePixelConversion<MultiDistance>
    {
        public MultiDistancePixelConversion(double range) : base(range) { }

        public unsafe override void Convert(float* pixels, MultiDistance distance)
        {
            pixels[0] = (float)(InvRange * distance.r + .5);
            pixels[1] = (float)(InvRange * distance.g + .5);
            pixels[2] = (float)(InvRange * distance.b + .5);
        }

        public override int NPerPixel => 3;
    }

    public class MultiTrueDistancePixelConversion : DistancePixelConversion<MultiAndTrueDistance>
    {
        public MultiTrueDistancePixelConversion(double range) : base(range) { }

        public unsafe override void Convert(float* pixels, MultiAndTrueDistance distance)
        {
            pixels[0] = (float)(InvRange * distance.r + .5);
            pixels[1] = (float)(InvRange * distance.g + .5);
            pixels[2] = (float)(InvRange * distance.b + .5);
            pixels[3] = (float)(InvRange * distance.a + .5);
        }

        public override int NPerPixel => 4;
    }
}
