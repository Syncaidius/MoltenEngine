using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Msdfgen
{
    public class WindingSpanner
    {
        List<KeyValuePair<double, int>> crossings = new List<KeyValuePair<double, int>>();
        public FillRule fillRule;

        int curW = 0;
        int curSpanID;

        public void Collect(Shape shape, Vector2 p)
        {
            crossings.Clear();
            foreach(Contour contour in shape.contours)
            {
                foreach(EdgeHolder e in contour.Edges)
                    e.crossings(p, this);
            }

            // Make sure we've collected them all in increasing x order.
            crossings.Sort((a, b) =>
            {
                if (a.Key < b.Key)
                    return -1;
                else if (a.Key == b.Key)
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
            while (curSpanID != crossings.Count && x > crossings[curSpanID].Key)
            {
                curW += crossings[curSpanID].Value;
                curSpanID++;
            }

            switch (fillRule)
            {
                case FillRule.NonZero:
                    return curW != 0 ? 1 : -1;
                case FillRule.EvenOdd:
                    return curW % 2 == 0 ? 1 : -1;
                case FillRule.None when curSpanID != crossings.Count:
                    return SdfMath.Sign(crossings[curSpanID].Value);
            }

            return 0;
        }

        public void Intersection(Vector2 p, int winding)
        {
            crossings.Add(new KeyValuePair<double, int>(p.X, winding));
        }
    }

    /// Fill rules compatible with SVG: https://www.w3.org/TR/SVG/painting.html#FillRuleProperty
    public enum FillRule
    {
        None = 0, // Legacy
        NonZero = 1,
        EvenOdd = 2,
    }
}
