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

        public int CompareIntersections(Intersection a, Intersection b)
        {
            return Math.Sign(a.x - b.x);
        }

        public bool InterpretFillRule(int intersections, FillRule fillRule)
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

        public void Preprocess()
        {
            lastIndex = 0;
            if (intersections.Count > 0)
            {
                intersections.Sort(CompareIntersections);
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

        public void SetIntersections(List<Intersection> newIntersections)
        {
            intersections = newIntersections;
            Preprocess();
        }

        public int MoveTo(double x)
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

        public int SumIntersections(double x)
        {
            int index = MoveTo(x);
            if (index >= 0)
                return intersections[index].direction;
            return 0;
        }

        public bool Filled(double x, FillRule fillRule)
        {
            return InterpretFillRule(SumIntersections(x), fillRule);
        }
    }
}
