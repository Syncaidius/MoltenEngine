using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public class Triangle
    {
        TriPoint[] _points;
        public bool[] ConstrainedEdge;
        public bool[] DelaunayEdge;
        Triangle[] _neighbours;
        bool _interior;

        public Triangle(TriPoint a, TriPoint b, TriPoint c)
        {
            _points = new TriPoint[] { a, b, c };
            ConstrainedEdge = new bool[3];
            DelaunayEdge = new bool[3];
            _neighbours = new Triangle[3];
        }

        public TriPoint GetPoint(int index)
        {
            return _points[index];
        }

        public Triangle GetNeighbor(int index)
        {
            return _neighbours[index];
        }

        public bool Contains(TriPoint p)
        {
            return _points[0].Equals(p) || _points[1].Equals(p) || _points[2].Equals(p);
        }

        public bool Contains(Edge e)
        {
            return Contains(e.P) && Contains(e.Q);
        }

        public bool Contains(TriPoint p, TriPoint q)
        {
            return Contains(p) && Contains(q);
        }

        public bool IsInterior()
        {
            return _interior;
        }

        public void IsInterior(bool b)
        {
            _interior = b;
        }

        /// <summary>
        /// Update neighbor pointers
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="t"></param>
        public void MarkNeighbor(TriPoint p1, TriPoint p2, Triangle t)
        {
            if ((p1.Equals(_points[2]) && p2.Equals(_points[1])) || (p1.Equals(_points[1]) && p2.Equals(_points[2])))
                _neighbours[0] = t;
            else if ((p1.Equals(_points[0]) && p2.Equals(_points[2])) || (p1.Equals(_points[2]) && p2.Equals(_points[0])))
                _neighbours[1] = t;
            else if ((p1.Equals(_points[0]) && p2.Equals(_points[1])) || (p1.Equals(_points[1]) && p2.Equals(_points[0])))
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
            if (t.Contains(_points[1], _points[2]))
            {
                _neighbours[0] = t;
                t.MarkNeighbor(_points[1], _points[2], this);
            }
            else if (t.Contains(_points[0], _points[2]))
            {
                _neighbours[1] = t;
                t.MarkNeighbor(_points[0], _points[2], this);
            }
            else if (t.Contains(_points[0], _points[1]))
            {
                _neighbours[2] = t;
                t.MarkNeighbor(_points[0], _points[1], this);
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
            _points[0] = _points[1] = _points[2] = null;
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
            _points[1] = _points[0];
            _points[0] = _points[2];
            _points[2] = point;
        }

        /// <summary>
        /// Legalize triagnle by rotating clockwise around oPoint
        /// </summary>
        /// <param name="opoint"></param>
        /// <param name="npoint"></param>
        public void Legalize(TriPoint opoint, TriPoint npoint)
        {
            if (opoint.Equals(_points[0]))
            {
                _points[1] = _points[0];
                _points[0] = _points[2];
                _points[2] = npoint;
            }
            else if (opoint.Equals(_points[1]))
            {
                _points[2] = _points[1];
                _points[1] = _points[0];
                _points[0] = npoint;
            }
            else if (opoint.Equals(_points[2]))
            {
                _points[0] = _points[2];
                _points[2] = _points[1];
                _points[1] = npoint;
            }
            else
            {
                Debug.Assert(false, "What happened here????");
            }
        }

        public int Index(TriPoint p)
        {
            if (p.Equals(_points[0]))
                return 0;
            else if (p.Equals(_points[1]))
                return 1;
            else if (p.Equals(_points[2]))
                return 2;

            Debug.Assert(false, "What happened here????");
            return -1;
        }

        public int EdgeIndex(TriPoint p1, TriPoint p2)
        {
            if (_points[0].Equals(p1))
            {
                if (_points[1].Equals(p2))
                    return 2;
                else if (_points[2].Equals(p2))
                    return 1;
            }
            else if (_points[1].Equals(p1))
            {
                if (_points[2].Equals(p2))
                    return 0;
                else if (_points[0].Equals(p2))
                    return 2;
            }
            else if (_points[2].Equals(p1))
            {
                if (_points[0].Equals(p2))
                    return 1;
                else if (_points[1].Equals(p2))
                    return 0;
            }

            return -1;
        }

        public void MarkConstrainedEdge(int index)
        {
            ConstrainedEdge[index] = true;
        }

        public void MarkConstrainedEdge(Edge edge)
        {
            MarkConstrainedEdge(edge.P, edge.Q);
        }

        /// <summary>
        /// Mark edge as constrained
        /// </summary>
        /// <param name="p"></param>
        /// <param name="q"></param>
        public void MarkConstrainedEdge(TriPoint p, TriPoint q)
        {
            if ((q.Equals(_points[0]) && p.Equals(_points[1])) || (q.Equals(_points[1]) && p.Equals(_points[0])))
                ConstrainedEdge[2] = true;
            else if ((q.Equals(_points[0]) && p.Equals(_points[2])) || (q.Equals(_points[2]) && p.Equals(_points[0])))
                ConstrainedEdge[1] = true;
            else if ((q.Equals(_points[1]) && p.Equals(_points[2])) || (q.Equals(_points[2]) && p.Equals(_points[1])))
                ConstrainedEdge[0] = true;
        }

        /// <summary>
        /// The point counter-clockwise to given point
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public TriPoint PointCW(TriPoint point)
        {
            if (point.Equals(_points[0]))
                return _points[2];
            else if (point.Equals(_points[1]))
                return _points[0];
            else if (point.Equals(_points[2]))
                return _points[1];

            Debug.Assert(false, "What happened here????");
            return TriPoint.Empty;
        }

        /// <summary>
        /// The point counter-clockwise to given point
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public TriPoint PointCCW(TriPoint point)
        {
            if (point.Equals(_points[0]))
                return _points[1];
            else if (point.Equals(_points[1]))
                return _points[2];
            else if (point.Equals(_points[2]))
                return _points[0];

            Debug.Assert(false, "What happened here????");
            return TriPoint.Empty;
        }

        /// <summary>
        /// The neighbor clockwise to given point
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public Triangle NeighborCW(TriPoint point)
        {
            if (point.Equals(_points[0]))
                return _neighbours[1];
            else if (point.Equals(_points[1]))
                return _neighbours[2];

            return _neighbours[0];
        }

        public Triangle NeighborCCW(TriPoint point)
        {
            if (point.Equals(_points[0]))
                return _neighbours[2];
            else if (point.Equals(_points[1]))
                return _neighbours[0];

            return _neighbours[1];
        }

        public bool GetConstrainedEdgeCCW(TriPoint p)
        {
            if (p.Equals(_points[0]))
                return ConstrainedEdge[2];
            else if (p.Equals( _points[1]))
                return ConstrainedEdge[0];

            return ConstrainedEdge[1];
        }

        public bool GetConstrainedEdgeCW(TriPoint p)
        {
            if (p.Equals(_points[0]))
                return ConstrainedEdge[1];
            else if (p.Equals(_points[1]))
                return ConstrainedEdge[2];

            return ConstrainedEdge[0];
        }

        public void SetConstrainedEdgeCCW(TriPoint p, bool ce)
        {
            if (p.Equals(_points[0]))
                ConstrainedEdge[2] = ce;
            else if (p.Equals(_points[1]))
                ConstrainedEdge[0] = ce;
            else
                ConstrainedEdge[1] = ce;
        }

        public void SetConstrainedEdgeCW(TriPoint p, bool ce)
        {
            if (p.Equals(_points[0]))
                ConstrainedEdge[1] = ce;
            else if (p.Equals(_points[1]))
                ConstrainedEdge[2] = ce;
            else
                ConstrainedEdge[0] = ce;
        }

        public bool GetDelaunayEdgeCCW(TriPoint p)
        {
            if (p.Equals(_points[0]))
                return DelaunayEdge[2];
            else if (p.Equals(_points[1]))
                return DelaunayEdge[0];

            return DelaunayEdge[1];
        }

        public bool GetDelaunayEdgeCW(TriPoint p)
        {
            if (p.Equals(_points[0]))
                return DelaunayEdge[1];
            else if (p.Equals(_points[1]))
                return DelaunayEdge[2];

            return DelaunayEdge[0];
        }

        public void SetDelunayEdgeCCW(TriPoint p, bool e)
        {
            if (p.Equals(_points[0]))
                DelaunayEdge[2] = e;
            else if (p.Equals(_points[1]))
                DelaunayEdge[0] = e;
            else
                DelaunayEdge[1] = e;
        }

        public void SetDelunayEdgeCW(TriPoint p, bool e)
        {
            if (p.Equals(_points[0]))
                DelaunayEdge[1] = e;
            else if (p.Equals(_points[1]))
                DelaunayEdge[2] = e;
            else
                DelaunayEdge[0] = e;
        }

        public Triangle NeighborAcross(TriPoint opoint)
        {
            if (opoint.Equals(_points[0]))
                return _neighbours[0];
            else if (opoint.Equals(_points[1]))
                return _neighbours[1];

            return _neighbours[2];
        }

        public override string ToString()
        {
            return $"{_points[0].X},{_points[0].Y} | {_points[1].X},{_points[1].Y} | {_points[2].X},{_points[2].Y}";
        }
    }
}
