using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.MSDF
{
    public class Scanline
    {
        public struct Intersection
        {
            /// X coordinate.
            public double x;

            /// Normalized Y direction of the oriented edge at the point of intersection.
            public int direction;

            public Intersection(double xx, int dir)
            {
                x = xx;
                direction = dir;
            }
        }

        public List<Intersection> intersections;
        public int lastIndex;

        public int compareIntersections(Intersection a, Intersection b)
        {
            return MsdfMath.Sign(a.x - b.x);
        }

        public bool interpretFillRule(int intersections, FillRule fillRule)
        {
            switch (fillRule)
            {
                case FillRule.NonZero:
                    return intersections != 0;
                case FillRule.Odd:
                    return (intersections & 1) == 1;
                case FillRule.Positive:
                    return intersections > 0;
                case FillRule.Negative:
                    return intersections < 0;
            }
            return false;
        }

        public double overlap(Scanline a, Scanline b, double xFrom, double xTo, FillRule fillRule)
        {
            double total = 0;
            bool aInside = false, bInside = false;
            int ai = 0, bi = 0;
            double ax = a.intersections.Count > 0 ? a.intersections[ai].x : xTo;
            double bx = b.intersections.Count > 0 ? b.intersections[bi].x : xTo;
            while (ax < xFrom || bx < xFrom)
            {
                double xNext = MsdfMath.Min(ax, bx);
                if (ax == xNext && ai < a.intersections.Count)
                {
                    aInside = interpretFillRule(a.intersections[ai].direction, fillRule);
                    ax = ++ai < a.intersections.Count ? a.intersections[ai].x : xTo;
                }
                if (bx == xNext && bi < b.intersections.Count)
                {
                    bInside = interpretFillRule(b.intersections[bi].direction, fillRule);
                    bx = ++bi < b.intersections.Count ? b.intersections[bi].x : xTo;
                }
            }
            double x = xFrom;
            while (ax < xTo || bx < xTo)
            {
                double xNext = MsdfMath.Min(ax, bx);
                if (aInside == bInside)
                    total += xNext - x;
                if (ax == xNext && ai < a.intersections.Count)
                {
                    aInside = interpretFillRule(a.intersections[ai].direction, fillRule);
                    ax = ++ai < a.intersections.Count ? a.intersections[ai].x : xTo;
                }
                if (bx == xNext && bi < b.intersections.Count)
                {
                    bInside = interpretFillRule(b.intersections[bi].direction, fillRule);
                    bx = ++bi < b.intersections.Count ? b.intersections[bi].x : xTo;
                }
                x = xNext;
            }
            if (aInside == bInside)
                total += xTo - x;
            return total;
        }

        public void preprocess()
        {
            lastIndex = 0;
            if (intersections.Count > 0)
            {
                intersections.Sort(compareIntersections);
                int totalDirection = 0;
                for (int i = 0; i < intersections.Count; i++)
                {
                    Intersection isec = intersections[i];
                    totalDirection += isec.direction;
                    isec.direction = totalDirection;
                    intersections[i] = isec;
                }
            }
        }

        public void setIntersections(List<Intersection> newIntersections)
        {
            intersections = newIntersections;
            preprocess();
        }

        public int moveTo(double x)
        {
            if (intersections.Count == 0)
                return -1;
            int index = lastIndex;
            if (x < intersections[index].x)
            {
                do
                {
                    if (index == 0)
                    {
                        lastIndex = 0;
                        return -1;
                    }
                    --index;
                } while (x < intersections[index].x);
            }
            else
            {
                while (index < intersections.Count - 1 && x >= intersections[index + 1].x)
                    ++index;
            }
            lastIndex = index;
            return index;
        }

        public int countIntersections(double x)
        {
            return moveTo(x) + 1;
        }

        public int sumIntersections(double x)
        {
            int index = moveTo(x);
            if (index >= 0)
                return intersections[index].direction;
            return 0;
        }

        public bool filled(double x, FillRule fillRule)
        {
            return interpretFillRule(sumIntersections(x), fillRule);
        }
    }
}
