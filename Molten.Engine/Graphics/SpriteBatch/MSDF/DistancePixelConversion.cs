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
    public abstract class DistancePixelConversion<R, D>
        where R : struct
        where D : struct
    {
        protected DistancePixelConversion(double range)
        {
            InvRange = 1.0 / range;
        }

        public abstract R Convert(D distance);

        public double InvRange { get; }
    }

    public class DoubleDistancePixelConversion : DistancePixelConversion<float, double>
    {
        public DoubleDistancePixelConversion(double range) : base(range) { }

        public override unsafe float Convert(double distance)
        {
            return (float)(InvRange * distance + .5);
        }
    }

    public class MultiDistancePixelConversion : DistancePixelConversion<Color3, Vector3D>
    {
        public MultiDistancePixelConversion(double range) : base(range) { }

        public override  Color3 Convert(Vector3D distance)
        {
            return new Color3()
            {
                R = (float)(InvRange * distance.X + .5),
                G = (float)(InvRange * distance.Y + .5),
                B = (float)(InvRange * distance.Z + .5)
            };
        }
    }

    public class MultiTrueDistancePixelConversion : DistancePixelConversion<Color4, Vector4D>
    {
        public MultiTrueDistancePixelConversion(double range) : base(range) { }

        public override Color4 Convert(Vector4D distance)
        {
            return new Color4()
            {
                R = (float)(InvRange * distance.X + .5),
                G = (float)(InvRange * distance.Y + .5),
                B = (float)(InvRange * distance.Z + .5),
                A = (float)(InvRange * distance.W + .5)
            };            
        }
    }
}
