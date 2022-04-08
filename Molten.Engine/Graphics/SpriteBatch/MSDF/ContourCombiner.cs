using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.SpriteBatch.MSDF
{
    public class ContourCombiner<T>
        where T : struct
    {
        public static void initDistance(ref double distance)
        {
            distance = -double.MaxValue;
        }

        public static void initDistance(ref Vector3D distance)
        {
            distance.X = -double.MaxValue;
            distance.Y = -double.MaxValue;
            distance.Z = -double.MaxValue;
        }

        public static double resolveDistance(double distance)
        {
            return distance;
        }

        public static double resolveDistance(Vector3D distance)
        {
            return MsdfMath.median(distance.X, distance.Y, distance.Z);
        }
    }
}
