using Molten.DoublePrecision;

namespace Molten.Shapes
{
    public class Contour
    {
        List<Edge> _edges = new List<Edge>();

        public void Clear()
        {
            _edges.Clear();
        }

        public void Add(Edge edge)
        {
            _edges.Add(edge);
        }

        public LinearEdge Add(Vector2D pStart, Vector2D pEnd, EdgeColor color = EdgeColor.White)
        {
            LinearEdge edge = new LinearEdge(pStart, pEnd, color);
            _edges.Add(edge);
            return edge;
        }

        public LinearEdge Append(Vector2D pEnd, EdgeColor color = EdgeColor.White)
        {
            if (_edges.Count == 0)
                throw new Exception("Cannot append edge point without at least 1 existing edge.");

            Edge last = _edges.Last();
            LinearEdge edge = new LinearEdge(last.End, pEnd, color);
            _edges.Add(edge);

            return edge;
        }

        public QuadraticEdge Append(Vector2D pEnd, Vector2D pControl, EdgeColor color = EdgeColor.White)
        {
            if (_edges.Count == 0)
                throw new Exception("Cannot append edge point without at least 1 existing edge.");

            Edge last = _edges.Last();
            QuadraticEdge edge = new QuadraticEdge(last.End, pEnd, pControl, color);
            _edges.Add(edge);

            return edge;
        }

        public CubicEdge Append(Vector2D pEnd, Vector2D pControl1, Vector2D pControl2, EdgeColor color = EdgeColor.White)
        {
            if (_edges.Count == 0)
                throw new Exception("Cannot append edge point without at least 1 existing edge.");

            Edge last = _edges.Last();
            CubicEdge edge = new CubicEdge(last.End, pEnd, pControl1, pControl2, color);
            _edges.Add(edge);

            return edge;
        }

        private double Shoelace(ref readonly Vector2D a, ref readonly Vector2D b)
        {
            return (b.X - a.X) * (a.Y + b.Y);
        }

        public int GetWinding()
        {
            if (_edges.Count == 0)
                return 0;

            double total = 0;
            if (_edges.Count == 1)
            {
                Vector2D a = _edges[0].Point(0), b = _edges[0].Point(1 / 3.0), c = _edges[0].Point(2 / 3.0);
                total += Shoelace(ref a, ref b);
                total += Shoelace(ref b, ref c);
                total += Shoelace(ref c, ref a);
            }
            else if (_edges.Count == 2)
            {
                Vector2D a = _edges[0].Point(0), b = _edges[0].Point(.5), c = _edges[1].Point(0), d = _edges[1].Point(.5);
                total += Shoelace(ref a, ref b);
                total += Shoelace(ref b, ref c);
                total += Shoelace(ref c, ref d);
                total += Shoelace(ref d, ref a);
            }
            else
            {
                Vector2D prev = _edges.Last().Point(0); 
                for(int i = 0; i < _edges.Count; i++)
                {
                    Vector2D cur = _edges[i].Point(0);
                    total += Shoelace(ref prev, ref cur);
                    prev = cur;
                }
            }
            return double.Sign(total);
        }

        public ContainmentType Contains(Contour other)
        {
            bool contains = true;
            bool intersects = false;

            // Curve mid-points
            int mpCount = EdgeResolution - 2;
            int incPoints = EdgeResolution - 1;
            double distInc = 1.0 / incPoints;

            for (int i = 0; i < other._edges.Count; i++)
            {
                Edge e = other.Edges[i];

                if (i == 0)
                {
                    bool r = Contains(e.P[0]);
                    contains = r && contains;
                    intersects = r || intersects;
                }

                if (e is not LinearEdge)
                {
                    for (int mp = 1; mp <= mpCount; mp++)
                    {
                        double dist = (distInc * mp);
                        Vector2F ep = (Vector2F)e.PointAlongEdge(dist);

                        bool r = Contains((Vector2D)ep);
                        contains = r && contains;
                        intersects = r || intersects;
                    }
                }

                if (i != _edges.Count - 1)
                {
                    bool rl = Contains(e.P[0]);
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
            int eCount = _edges.Count;
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
                if (Edges[j] is not LinearEdge)
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

        internal List<TriPoint> GetEdgePoints()
        {
            if (EdgeResolution < 3)
                throw new Exception("Edge resolution must be at least 3");

            List<TriPoint> points = new List<TriPoint>();

            // Curve mid-points
            int mpCount = EdgeResolution - 2;
            int incPoints = EdgeResolution - 1;
            double distInc = 1.0 / incPoints;

            for (int i = 0; i < _edges.Count; i++)
            {
                Edge e = Edges[i];

                if (i == 0)
                    points.Add(new TriPoint(e.Start));

                if (e is not LinearEdge)
                {
                    for (int mp = 1; mp <= mpCount; mp++)
                    {
                        double dist = (distInc * mp);
                        Vector2F ep = (Vector2F)e.PointAlongEdge(dist);

                        points.Add(new TriPoint(ep));
                    }
                }

                points.Add(new TriPoint(e.End));
            }

            return points;
        }

        /// <summary>
        /// Gets 9r sets the maximum number of points that are allowed to represent an edge. For bezier curves, this will affect the curve smoothness.
        /// </summary>
        public int EdgeResolution { get; set; } = 3;

        /// <summary>
        /// Gets a read-only list of edges that make up the current <see cref="Contour"/>.
        /// </summary>
        public IReadOnlyList<Edge> Edges => _edges;
    }
}
