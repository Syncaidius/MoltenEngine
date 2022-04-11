using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public partial class ContourShape
    {
        public class Contour
        {
            public List<Edge> Edges { get; } = new List<Edge>();

            public void AddEdge(Edge edge)
            {
                Edges.Add(edge);
            }

            public void AppendLinearPoint(Vector2D p)
            {
                if (Edges.Count == 0)
                    throw new Exception("Cannot append edge point without at least 1 existing edge.");

                Edge last = Edges.Last();
                Edges.Add(new LinearEdge(last.Points[Edge.INDEX_P1], p));
            }

            public void AppendQuadraticPoint(Vector2D p, Vector2D pControl)
            {
                if (Edges.Count == 0)
                    throw new Exception("Cannot append edge point without at least 1 existing edge.");

                Edge last = Edges.Last();
                Edges.Add(new QuadraticEdge(last.Points[Edge.INDEX_P1], p, pControl));
            }

            public void AppendCubicPoint(Vector2D p, Vector2D pControl1, Vector2D pControl2)
            {
                if (Edges.Count == 0)
                    throw new Exception("Cannot append edge point without at least 1 existing edge.");

                Edge last = Edges.Last();
                Edges.Add(new CubicEdge(last.Points[Edge.INDEX_P1], p, pControl1, pControl2));
            }

            private double Shoelace(Vector2D a, Vector2D b)
            {
                return (b.X - a.X) * (a.Y + b.Y);
            }

            public int GetWinding()
            {
                if (Edges.Count == 0)
                    return 0;

                double total = 0;
                if (Edges.Count == 1)
                {
                    Vector2D a = Edges[0].Point(0), b = Edges[0].Point(1 / 3.0), c = Edges[0].Point(2 / 3.0);
                    total += Shoelace(a, b);
                    total += Shoelace(b, c);
                    total += Shoelace(c, a);
                }
                else if (Edges.Count == 2)
                {
                    Vector2D a = Edges[0].Point(0), b = Edges[0].Point(.5), c = Edges[1].Point(0), d = Edges[1].Point(.5);
                    total += Shoelace(a, b);
                    total += Shoelace(b, c);
                    total += Shoelace(c, d);
                    total += Shoelace(d, a);
                }
                else
                {
                    Vector2D prev = Edges.Last().Point(0);
                    foreach (Edge edge in Edges)
                    {
                        Vector2D cur = edge.Point(0);
                        total += Shoelace(prev, cur);
                        prev = cur;
                    }
                }
                return MathHelperDP.Sign(total);
            }
        }
    }
}
