using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Msdfgen
{
    public class WindingSpanner
    {
        List<ValueTuple<double, int>> crossings = new List<ValueTuple<double, int>>();
        public FillRule fillRule;

        int curW = 0;
        int curSpanID = 0;

        public void Collect(Shape shape, Vector2 p)
        {
            fillRule = shape.FillRule;
            crossings.Clear();
            foreach(Contour contour in shape.contours)
            {
                foreach(EdgeHolder e in contour.Edges)
                    e.crossings(p, this);
            }

            // Make sure we've collected them all in increasing x order.
            crossings.Sort((a, b) =>
            {
                if (a.Item1 < b.Item1)
                    return -1;
                else if (a.Item1 == b.Item1)
                    return 0;
                else
                    return 1;
            });

            if (fillRule == FillRule.EvenOdd)
                curW = 1;
            else
                curW = 0;

            curSpanID = 0;
        }

        public int AdvanceTo(double x)
        {
            while (curSpanID != crossings.Count && x > crossings[curSpanID].Item1)
            {
                curW += crossings[curSpanID].Item2;
                curSpanID++;
            }

            switch (fillRule)
            {
                case FillRule.NonZero:
                    return curW != 0 ? 1 : -1;
                case FillRule.EvenOdd:
                    return curW % 2 == 0 ? 1 : -1;
                case FillRule.None when curSpanID != crossings.Count:
                    return SdfMath.Sign(crossings[curSpanID].Item2);
            }

            return 0;
        }

        public void Intersection(Vector2 p, int winding)
        {
            crossings.Add(new ValueTuple<double, int>(p.X, winding));
        }
    }
}
