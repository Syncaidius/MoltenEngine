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
    /// <typeparam name="R">BitmapRef type</typeparam>
    /// <typeparam name="D">Distance type</typeparam>
    public abstract class DistancePixelConversion<D>
        where D : struct
    {
        protected DistancePixelConversion(double range)
        {
            InvRange = 1.0 / range;
        }

        public unsafe abstract void Convert(float* pixels, D distance);

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

    public class MultiDistancePixelConversion : DistancePixelConversion<Vector3D>
    {
        public MultiDistancePixelConversion(double range) : base(range) { }

        public unsafe override void Convert(float* pixels, Vector3D distance)
        {
            pixels[0] = (float)(InvRange * distance.X + .5);
            pixels[1] = (float)(InvRange * distance.Y + .5);
            pixels[2] = (float)(InvRange * distance.Z + .5);
        }

        public override int NPerPixel => 3;
    }

    public class MultiTrueDistancePixelConversion : DistancePixelConversion<Vector4D>
    {
        public MultiTrueDistancePixelConversion(double range) : base(range) { }

        public unsafe override void Convert(float* pixels, Vector4D distance)
        {
            pixels[0] = (float)(InvRange * distance.X + .5);
            pixels[1] = (float)(InvRange * distance.Y + .5);
            pixels[2] = (float)(InvRange * distance.Z + .5);
            pixels[3] = (float)(InvRange * distance.W + .5);
        }

        public override int NPerPixel => 4;
    }
}
