using System.Diagnostics;

namespace Molten
{
    internal class Triangle
    {
        public TriPoint[] Points;
        public bool[] ConstrainedEdge;
        public bool[] DelaunayEdge;
        Triangle[] _neighbours;

        public Triangle(TriPoint a, TriPoint b, TriPoint c)
        {
            Points = new TriPoint[] { a, b, c };
            ConstrainedEdge = new bool[3];
            DelaunayEdge = new bool[3];
            _neighbours = new Triangle[3];
        }

        public Triangle GetNeighbor(int index)
        {
            return _neighbours[index];
        }

        public bool Contains(TriPoint p)
        {
            return Points[0].Equals(p) || Points[1].Equals(p) || Points[2].Equals(p);
        }

        public bool Contains(TriEdge e)
        {
            return Contains(e.P1) && Contains(e.P2);
        }

        public bool Contains(TriPoint p, TriPoint q)
        {
            return Contains(p) && Contains(q);
        }

        /// <summary>
        /// Update neighbor pointers
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="t"></param>
        public void MarkNeighbor(TriPoint p1, TriPoint p2, Triangle t)
        {
            if ((p1.Equals(Points[2]) && p2.Equals(Points[1])) || (p1.Equals(Points[1]) && p2.Equals(Points[2])))
                _neighbours[0] = t;
            else if ((p1.Equals(Points[0]) && p2.Equals(Points[2])) || (p1.Equals(Points[2]) && p2.Equals(Points[0])))
                _neighbours[1] = t;
            else if ((p1.Equals(Points[0]) && p2.Equals(Points[1])) || (p1.Equals(Points[1]) && p2.Equals(Points[0])))
                _neighbours[2] = t;
            else
                Debug.Assert(false, "Specified trianlge is not a neighbor");
        }

        /// <summary>
        /// Exhaustive search to update neighbor pointers
        /// </summary>
        /// <param name="t"></param>
        public void MarkNeighbor(Triangle t)
        {
            if (t.Contains(Points[1], Points[2]))
            {
                _neighbours[0] = t;
                t.MarkNeighbor(Points[1], Points[2], this);
            }
            else if (t.Contains(Points[0], Points[2]))
            {
                _neighbours[1] = t;
                t.MarkNeighbor(Points[0], Points[2], this);
            }
            else if (t.Contains(Points[0], Points[1]))
            {
                _neighbours[2] = t;
                t.MarkNeighbor(Points[0], Points[1], this);
            }
        }

        public void Clear()
        {
            Triangle t;
            for (int i = 0; i < 3; i++)
            {
                t = _neighbours[i];
                if (t != null)
                    t.ClearNeighbor(this);
            }

            ClearNeighbors();
            Points[0] = Points[1] = Points[2] = null;
        }

        public void ClearNeighbor(Triangle triangle)
        {
            if (_neighbours[0] == triangle)
                _neighbours[0] = null;
            else if (_neighbours[1] == triangle)
                _neighbours[1] = null;
            else
                _neighbours[2] = null;
        }

        public void ClearNeighbors()
        {
            _neighbours[0] = _neighbours[1] = _neighbours[2] = null;
        }

        public void ClearDelaunayEdges()
        {
            DelaunayEdge[0] = DelaunayEdge[1] = DelaunayEdge[2] = false;
        }

        public TriPoint OppositePoint(Triangle t, TriPoint p)
        {
            TriPoint cw = t.PointCW(p);
            return PointCW(cw);
        }

        /// <summary>
        /// Legalized triangle by rotating clockwise around point(0)
        /// </summary>
        /// <param name="point"></param>
        public void Legalize(TriPoint point)
        {
            Points[1] = Points[0];
            Points[0] = Points[2];
            Points[2] = point;
        }

        /// <summary>
        /// Legalize triagnle by rotating clockwise around oPoint
        /// </summary>
        /// <param name="opoint"></param>
        /// <param name="npoint"></param>
        public void Legalize(TriPoint opoint, TriPoint npoint)
        {
            if (opoint.Equals(Points[0]))
            {
                Points[1] = Points[0];
                Points[0] = Points[2];
                Points[2] = npoint;
            }
            else if (opoint.Equals(Points[1]))
            {
                Points[2] = Points[1];
                Points[1] = Points[0];
                Points[0] = npoint;
            }
            else if (opoint.Equals(Points[2]))
            {
                Points[0] = Points[2];
                Points[2] = Points[1];
                Points[1] = npoint;
            }
            else
            {
                Debug.Assert(false);
            }
        }

        public int Index(TriPoint p)
        {
            if (p.Equals(Points[0]))
                return 0;
            else if (p.Equals(Points[1]))
                return 1;
            else if (p.Equals(Points[2]))
                return 2;

            Debug.Assert(false);
            return -1;
        }

        public int EdgeIndex(TriPoint p1, TriPoint p2)
        {
            if (Points[0].Equals(p1))
            {
                if (Points[1].Equals(p2))
                    return 2;
                else if (Points[2].Equals(p2))
                    return 1;
            }
            else if (Points[1].Equals(p1))
            {
                if (Points[2].Equals(p2))
                    return 0;
                else if (Points[0].Equals(p2))
                    return 2;
            }
            else if (Points[2].Equals(p1))
            {
                if (Points[0].Equals(p2))
                    return 1;
                else if (Points[1].Equals(p2))
                    return 0;
            }

            return -1;
        }

        public void MarkConstrainedEdge(int index)
        {
            ConstrainedEdge[index] = true;
        }

        public void MarkConstrainedEdge(TriEdge edge)
        {
            MarkConstrainedEdge(edge.P1, edge.P2);
        }

        /// <summary>
        /// Mark edge as constrained
        /// </summary>
        /// <param name="p"></param>
        /// <param name="q"></param>
        public void MarkConstrainedEdge(TriPoint p, TriPoint q)
        {
            if ((q.Equals(Points[0]) && p.Equals(Points[1])) || (q.Equals(Points[1]) && p.Equals(Points[0])))
                ConstrainedEdge[2] = true;
            else if ((q.Equals(Points[0]) && p.Equals(Points[2])) || (q.Equals(Points[2]) && p.Equals(Points[0])))
                ConstrainedEdge[1] = true;
            else if ((q.Equals(Points[1]) && p.Equals(Points[2])) || (q.Equals(Points[2]) && p.Equals(Points[1])))
                ConstrainedEdge[0] = true;
        }

        /// <summary>
        /// The point counter-clockwise to given point
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public TriPoint PointCW(TriPoint point)
        {
            if (point.Equals(Points[0]))
                return Points[2];
            else if (point.Equals(Points[1]))
                return Points[0];
            else if (point.Equals(Points[2]))
                return Points[1];

            Debug.Assert(false);
            return null;
        }

        /// <summary>
        /// The point counter-clockwise to given point
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public TriPoint PointCCW(TriPoint point)
        {
            if (point.Equals(Points[0]))
                return Points[1];
            else if (point.Equals(Points[1]))
                return Points[2];
            else if (point.Equals(Points[2]))
                return Points[0];

            Debug.Assert(false);
            return null;
        }

        // The neighbor across to given point
        public Triangle NeighborAcross(TriPoint point)
        {
            if (point.Equals(Points[0]))
                return _neighbours[0];
            else if (point.Equals(Points[1]))
                return _neighbours[1];

            return _neighbours[2];
        }

        /// <summary>
        /// The neighbor clockwise to given point
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public Triangle NeighborCW(TriPoint point)
        {
            if (point.Equals(Points[0]))
                return _neighbours[1];
            else if (point.Equals(Points[1]))
                return _neighbours[2];

            return _neighbours[0];
        }

        public Triangle NeighborCCW(TriPoint point)
        {
            if (point.Equals(Points[0]))
                return _neighbours[2];
            else if (point.Equals(Points[1]))
                return _neighbours[0];

            return _neighbours[1];
        }

        public bool GetConstrainedEdgeCCW(TriPoint p)
        {
            if (p.Equals(Points[0]))
                return ConstrainedEdge[2];
            else if (p.Equals(Points[1]))
                return ConstrainedEdge[0];

            return ConstrainedEdge[1];
        }

        public bool GetConstrainedEdgeCW(TriPoint p)
        {
            if (p.Equals(Points[0]))
                return ConstrainedEdge[1];
            else if (p.Equals(Points[1]))
                return ConstrainedEdge[2];

            return ConstrainedEdge[0];
        }

        public void SetConstrainedEdgeCCW(TriPoint p, bool ce)
        {
            if (p.Equals(Points[0]))
                ConstrainedEdge[2] = ce;
            else if (p.Equals(Points[1]))
                ConstrainedEdge[0] = ce;
            else
                ConstrainedEdge[1] = ce;
        }

        public void SetConstrainedEdgeCW(TriPoint p, bool ce)
        {
            if (p.Equals(Points[0]))
                ConstrainedEdge[1] = ce;
            else if (p.Equals(Points[1]))
                ConstrainedEdge[2] = ce;
            else
                ConstrainedEdge[0] = ce;
        }

        public bool GetDelaunayEdgeCCW(TriPoint p)
        {
            if (p.Equals(Points[0]))
                return DelaunayEdge[2];
            else if (p.Equals(Points[1]))
                return DelaunayEdge[0];

            return DelaunayEdge[1];
        }

        public bool GetDelaunayEdgeCW(TriPoint p)
        {
            if (p.Equals(Points[0]))
                return DelaunayEdge[1];
            else if (p.Equals(Points[1]))
                return DelaunayEdge[2];

            return DelaunayEdge[0];
        }

        public void SetDelunayEdgeCCW(TriPoint p, bool e)
        {
            if (p.Equals(Points[0]))
                DelaunayEdge[2] = e;
            else if (p.Equals(Points[1]))
                DelaunayEdge[0] = e;
            else
                DelaunayEdge[1] = e;
        }

        public void SetDelunayEdgeCW(TriPoint p, bool e)
        {
            if (p.Equals(Points[0]))
                DelaunayEdge[1] = e;
            else if (p.Equals(Points[1]))
                DelaunayEdge[2] = e;
            else
                DelaunayEdge[0] = e;
        }

        public override string ToString()
        {
            return $"{Points[0].X},{Points[0].Y} | {Points[1].X},{Points[1].Y} | {Points[2].X},{Points[2].Y}";
        }

        public bool CircumcicleContains(TriPoint point)
        {
            Debug.Assert(IsCounterClockwise());
            double dx = Points[0].X - point.X;
            double dy = Points[0].Y - point.Y;
            double ex = Points[1].X - point.X;
            double ey = Points[1].Y - point.Y;
            double fx = Points[2].X - point.X;
            double fy = Points[2].Y - point.Y;

            double ap = dx * dx + dy * dy;
            double bp = ex * ex + ey * ey;
            double cp = fx * fx + fy * fy;

            return (dx * (fy * bp - cp * ey) - dy * (fx * bp - cp * ex) + ap * (fx * ey - fy * ex)) < 0;
        }

        public bool IsCounterClockwise()
        {
            return (Points[1].X - Points[0].X) * (Points[2].Y - Points[0].Y) -
                       (Points[2].X - Points[0].X) * (Points[1].Y - Points[0].Y) > 0;
        }

        public bool IsDelaunay(List<Triangle> triangles)
        {
            foreach (Triangle triangle in triangles)
            {
                foreach (Triangle other in triangles)
                {
                    if (triangle == other)
                    {
                        continue;
                    }
                    for (int i = 0; i < 3; ++i)
                    {
                        if (triangle.CircumcicleContains(other.Points[i]))
                            return false;
                    }
                }
            }
            return true;
        }

        public bool IsInterior { get; set; }
    }
}
