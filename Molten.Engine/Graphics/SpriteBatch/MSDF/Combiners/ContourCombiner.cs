using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.SpriteBatch.MSDF
{
    public abstract class ContourCombiner
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

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="ES">EdgeSelector type.</typeparam>
    /// <typeparam name="DT">Distance type. e.g. double, MultiDistance or MultiAndTrueDistance.</typeparam>
    /// <typeparam name="P">Point type, such as double or Vector2D.</typeparam>
    public abstract class ContourCombiner<ES, DT> : ContourCombiner
        where ES : EdgeSelector<DT>
        where DT : unmanaged
    {
        public abstract void reset(in Vector2D p);

        public abstract ES edgeSelector(int i);

        public abstract DT distance();
    }
}
