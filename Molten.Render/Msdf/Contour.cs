//MIT, 2016, Viktor Chlumsky, Multi-channel signed distance field generator, from https://github.com/Chlumsky/msdfgen
//MIT, 2017, WinterDev (C# port)
//MIT, 2018, James Yarwood (Adapted for Molten Engine)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Msdfgen
{
    public class Contour
    {
        public List<EdgeHolder> Edges = new List<EdgeHolder>();

        public void AddEdge(EdgeSegment edge)
        {
            EdgeHolder holder = new EdgeHolder(edge);
            Edges.Add(holder);
        }

        public void AddLine(double x0, double y0, double x1, double y1)
        {
            this.AddEdge(new LinearSegment(new Vector2(x0, y0), new Vector2(x1, y1)));
        }

        public void AddQuadraticSegment(double x0, double y0,
            double ctrl0X, double ctrl0Y,
            double x1, double y1)
        {
            this.AddEdge(new QuadraticSegment(
                new Vector2(x0, y0),
                new Vector2(ctrl0X, ctrl0Y),
                new Vector2(x1, y1)
                ));
        }

        public void AddCubicSegment(double x0, double y0,
            double ctrl0X, double ctrl0Y,
            double ctrl1X, double ctrl1Y,
            double x1, double y1)
        {
            this.AddEdge(new CubicSegment(
               new Vector2(x0, y0),
               new Vector2(ctrl0X, ctrl0Y),
               new Vector2(ctrl1X, ctrl1Y),
               new Vector2(x1, y1)
               ));
        }

        public void FindBounds(ref double left, ref double bottom, ref double right, ref double top)
        {
            int j = Edges.Count;
            for (int i = 0; i < j; ++i)
            {
                Edges[i].edgeSegment.findBounds(ref left, ref bottom, ref right, ref top);
            }
        }

        public int Winding()
        {
            int j = Edges.Count;
            double total = 0;
            switch (j)
            {
                case 0: return 0;
                case 1:
                    {
                        Vector2 a = Edges[0].point(0), b = Edges[0].point(1 / 3.0), c = Edges[0].point(2 / 3.0);
                        total += Vector2.Shoelace(a, b);
                        total += Vector2.Shoelace(b, c);
                        total += Vector2.Shoelace(c, a);

                    }
                    break;
                case 2:
                    {
                        Vector2 a = Edges[0].point(0), b = Edges[0].point(0.5), c = Edges[1].point(0), d = Edges[1].point(0.5);
                        total += Vector2.Shoelace(a, b);
                        total += Vector2.Shoelace(b, c);
                        total += Vector2.Shoelace(c, d);
                        total += Vector2.Shoelace(d, a);
                    }
                    break;
                default:
                    {
                        Vector2 prev = Edges[j - 1].point(0);
                        for (int i = 0; i < j; ++i)
                        {
                            Vector2 cur = Edges[i].point(0);
                            total += Vector2.Shoelace(prev, cur);
                            prev = cur;
                        }
                    }
                    break;
            }

            return SdfMath.Sign(total);
        }
    }
}
