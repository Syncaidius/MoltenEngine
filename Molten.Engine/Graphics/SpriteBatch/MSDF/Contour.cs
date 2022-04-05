using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.SpriteBatch.MSDF
{
    public class Contour
    {
        public List<EdgeHolder> Edges = new List<EdgeHolder>();

        private static double shoelace(Vector2D a, Vector2D b)
        {
            return (b.X - a.X) * (a.Y + b.Y);
        }

        private static void boundPoint(ref double l, ref double b, ref double r, ref double t, Vector2D p)
        {
            if (p.X < l) l = p.X;
            if (p.Y < b) b = p.Y;
            if (p.X > r) r = p.X;
            if (p.Y > t) t = p.Y;
        }

        public void AddEdge(EdgeHolder edge)
        {
            Edges.Add(edge);
        }

        public EdgeHolder AddEdge()
        {
            EdgeHolder eh = new EdgeHolder();
            Edges.Add(eh);
            return eh;
        }

        public void bound(ref double l, ref double b, ref double r, ref double t)
        {
            foreach (EdgeHolder edge in Edges)
                edge.Segment.bound(ref l, ref b, ref r, ref t);
        }

        public void boundMiters(ref double l, ref double b, ref double r, ref double t, double border, double miterLimit, int polarity) {
            if (Edges.Count == 0)
                return;

            Vector2D prevDir = Edges.Last().Segment.direction(1).GetNormalized(true);

            foreach(EdgeHolder edge in Edges)
            { 
                Vector2D dir = -edge.Segment.direction(0).GetNormalized(true);
                if (polarity * Vector2D.Cross(prevDir, dir) >= 0) {
                    double miterLength = miterLimit;
                    double q = .5 * (1 - Vector2D.Dot(prevDir, dir));
                    if (q > 0)
                        miterLength = MsdfMath.min(1 / Math.Sqrt(q), miterLimit);
                    Vector2D miter = (*edge)->point(0) + border * miterLength * (prevDir + dir).GetNormalized(true);
                    boundPoint(l, b, r, t, miter);
                }
                prevDir = (*edge)->direction(1).normalize(true);
            }
        }
    }
}

