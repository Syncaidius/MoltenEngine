using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.MSDF
{
    public class Contour
    {
        public List<EdgeSegment> Edges = new List<EdgeSegment>();

        private static double Shoelace(Vector2D a, Vector2D b)
        {
            return (b.X - a.X) * (a.Y + b.Y);
        }

        private static void BoundPoint(ref double l, ref double b, ref double r, ref double t, Vector2D p)
        {
            if (p.X < l) l = p.X;
            if (p.Y < b) b = p.Y;
            if (p.X > r) r = p.X;
            if (p.Y > t) t = p.Y;
        }

        public void AddEdge(EdgeSegment edge)
        {
            Edges.Add(edge);
        }

        public void Bound(ref double l, ref double b, ref double r, ref double t)
        {
            foreach (EdgeSegment edge in Edges)
                edge.Bound(ref l, ref b, ref r, ref t);
        }

        public void BoundMiters(ref double l, ref double b, ref double r, ref double t, double border, double miterLimit, int polarity) {
            if (Edges.Count == 0)
                return;

            Vector2D prevDir = Edges.Last().Direction(1).GetNormalized(true);

            foreach (EdgeSegment edge in Edges)
            {
                Vector2D dir = -edge.Direction(0).GetNormalized(true);
                if (polarity * Vector2D.Cross(prevDir, dir) >= 0) {
                    double miterLength = miterLimit;
                    double q = .5 * (1 - Vector2D.Dot(prevDir, dir));
                    if (q > 0)
                        miterLength = MsdfMath.Min(1 / Math.Sqrt(q), miterLimit);
                    Vector2D miter = edge.Point(0) + border * miterLength * (prevDir + dir).GetNormalized(true);
                    BoundPoint(ref l, ref b, ref r, ref t, miter);
                }
                prevDir = edge.Direction(1).GetNormalized(true);
            }
        }

        public int Winding() {
            if (Edges.Count == 0)
                return 0;

            double total = 0;
            if (Edges.Count == 1) {
                Vector2D a = Edges[0].Point(0), b = Edges[0].Point(1 / 3.0), c = Edges[0].Point(2 / 3.0);
                total += Shoelace(a, b);
                total += Shoelace(b, c);
                total += Shoelace(c, a);
            } else if (Edges.Count == 2) {
                Vector2D a = Edges[0].Point(0), b = Edges[0].Point(.5), c = Edges[1].Point(0), d = Edges[1].Point(.5);
                total += Shoelace(a, b);
                total += Shoelace(b, c);
                total += Shoelace(c, d);
                total += Shoelace(d, a);
            } else
            {
                Vector2D prev = Edges.Last().Point(0);
                foreach (EdgeSegment edge in Edges)
                {
                    Vector2D cur = edge.Point(0);
                    total += Shoelace(prev, cur);
                    prev = cur;
                }
            }
            return MsdfMath.Sign(total);
        }

        public void Reverse()
        {
            for (int i = Edges.Count / 2; i > 0; --i)
                SwapEdges(i - 1, Edges.Count - i);

            foreach (EdgeSegment edge in Edges)
                edge.Reverse();
        }

        /// Swaps two edges at index and and b.
        private void SwapEdges(int indexA, int indexB)
        {
            EdgeSegment tmp = Edges[indexA];
            Edges[indexA] = Edges[indexB];
            Edges[indexB] = tmp;
        }
    }
}

