using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public partial class Shape
    {
        public class Contour
        {
            public List<Edge> Edges { get; } = new List<Edge>();

            public void AddEdge(Edge edge)
            {
                Edges.Add(edge);
            }

            public void AddLinearEdge(Vector2D p1, Vector2D p2, EdgeColor color = EdgeColor.White)
            {
                Edges.Add(new LinearEdge(p1, p2, color));
            }

            public void AppendLinearPoint(Vector2D p, EdgeColor color = EdgeColor.White)
            {
                if (Edges.Count == 0)
                    throw new Exception("Cannot append edge point without at least 1 existing edge.");

                Edge last = Edges.Last();
                Edges.Add(new LinearEdge(last.End, p, color));
            }

            public void AppendQuadraticPoint(Vector2D p, Vector2D pControl, EdgeColor color = EdgeColor.White)
            {
                if (Edges.Count == 0)
                    throw new Exception("Cannot append edge point without at least 1 existing edge.");

                Edge last = Edges.Last();
                Edges.Add(new QuadraticEdge(last.End, p, pControl, color));
            }

            public void AppendCubicPoint(Vector2D p, Vector2D pControl1, Vector2D pControl2, EdgeColor color = EdgeColor.White)
            {
                if (Edges.Count == 0)
                    throw new Exception("Cannot append edge point without at least 1 existing edge.");

                Edge last = Edges.Last();
                Edges.Add(new CubicEdge(last.End, p, pControl1, pControl2, color));
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

            public ContainmentType Contains(Contour other, int edgeResolution = 3)
            {
                bool contains = true;
                bool intersects = false;

                // Curve mid-points
                int mpCount = edgeResolution - 2;
                int incPoints = edgeResolution - 1;
                double distInc = 1.0 / incPoints;

                for (int i = 0; i < other.Edges.Count; i++)
                {
                    Edge e = other.Edges[i];

                    if (i == 0)
                    {
                        bool r = Contains(e.P[0], edgeResolution);
                        contains = r && contains;
                        intersects = r || intersects;
                    }

                    if (e is not LinearEdge)
                    {
                        for (int mp = 1; mp <= mpCount; mp++)
                        {
                            double dist = (distInc * mp);
                            Vector2F ep = (Vector2F)e.PointAlongEdge(dist);

                            bool r = Contains((Vector2D)ep, edgeResolution);
                            contains = r && contains;
                            intersects = r || intersects;
                        }
                    }

                    if (i != Edges.Count - 1)
                    {
                        bool rl = Contains(e.P[0], edgeResolution);
                        contains = rl && contains;
                        intersects = rl || intersects;
                    }

                    // Exit early if containment already failed
                    if (intersects && !contains)
                        return ContainmentType.Intersects;
                }

                if (contains)
                    return ContainmentType.Contains;
                else
                    return ContainmentType.Intersects;
            }

            public bool Contains(Vector2D point, int edgeResolution = 3)
            {
                // Thanks to: https://codereview.stackexchange.com/a/108903
                int eCount = Edges.Count;
                int j = 0;
                bool inside = false;
                double pointX = point.X, pointY = point.Y; // x, y for tested point.

                // start / end point for the current polygon segment.
                double startX, startY, endX, endY;
                Vector2D endPoint = Edges[eCount - 1].End;
                endX = endPoint.X;
                endY = endPoint.Y;

                // Curve mid-points
                int mpCount = edgeResolution - 2;
                int incPoints = edgeResolution - 1;
                double distInc = 1.0 / incPoints;

                while (j < eCount)
                {
                    // Test mid-points of curve, if not linear edge.
                    if(Edges[j] is not LinearEdge)
                    {
                        for (int mp = 1; mp <= mpCount; mp++)
                        {
                            double dist = (distInc * mp);

                            startX = endX; startY = endY;
                            endPoint = Edges[j].PointAlongEdge(dist);
                            endX = endPoint.X; endY = endPoint.Y;
                            inside ^= (endY > pointY ^ startY > pointY) /* ? pointY inside [startY;endY] segment ? */
                                      && /* if so, test if it is under the segment */
                                      ((pointX - endX) < (pointY - endY) * (startX - endX) / (startY - endY));
                        }
                    }

                    // Test edge end-point
                    startX = endX; startY = endY;
                    endPoint = Edges[j++].End;
                    endX = endPoint.X; endY = endPoint.Y;
                    inside ^= (endY > pointY ^ startY > pointY) /* ? pointY inside [startY;endY] segment ? */
                              && /* if so, test if it is under the segment */
                              ((pointX - endX) < (pointY - endY) * (startX - endX) / (startY - endY));
                }

                return inside;
            }

            /// <summary>
            /// Produces a <see cref="RectangleF"/> which contains all of the shape's points.
            /// </summary>
            public RectangleF CalculateBounds()
            {
                throw new NotImplementedException();

                /*RectangleF b = new RectangleF()
                {
                    Left = float.MaxValue,
                    Top = float.MaxValue,
                    Right = float.MinValue,
                    Bottom = float.MinValue,
                };

                foreach (TriPoint p in Points)
                {
                    if (p.X < b.Left)
                        b.Left = p.X;
                    else if (p.X > b.Right)
                        b.Right = p.Y;

                    if (p.Y < b.Top)
                        b.Top = p.Y;
                    else if (p.Y > b.Bottom)
                        b.Bottom = p.Y;
                }

                return b;*/
            }

            public List<TriPoint> GetEdgePoints(int edgeResolution = 3)
            {
                if (edgeResolution < 3)
                    throw new Exception("Edge resolution must be at least 3");

                List<TriPoint> points = new List<TriPoint>();

                // Curve mid-points
                int mpCount = edgeResolution - 2;
                int incPoints = edgeResolution - 1;
                double distInc = 1.0 / incPoints; 

                for (int i = 0; i < Edges.Count; i++)
                {
                    Edge e = Edges[i];

                    if (i == 0)
                        points.Add(new TriPoint((Vector2F)e.Start));
                    
                    if (e is not LinearEdge)
                    {
                        for (int mp = 1; mp <= mpCount; mp++)
                        {
                            double dist = (distInc * mp);
                            Vector2F ep = (Vector2F)e.PointAlongEdge(dist);

                            points.Add(new TriPoint(ep));
                        }
                    }

                    points.Add(new TriPoint((Vector2F)e.End));
                }

                return points;
            }
        }
    }
}
