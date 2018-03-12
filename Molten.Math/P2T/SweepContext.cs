using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    internal class SweepContext
    {
        // Inital triangle factor, seed triangle will extend 30% of
        // PointSet width to both left and right.
        const float K_ALPHA = 0.3f;

        List<TriPoint> _points;
        List<Triangle> _triangles;
        List<Triangle> _map;
        List<Edge> _edge_list;

        TriPoint _head;
        TriPoint _tail;

        Node _af_head;
        Node _af_middle;
        Node _af_tail;

        internal AdvancingFront Front;
        TriPoint.Comparer _cmp;

        internal SweepBasin Basin;
        internal EdgeEvent EdgeEvent;

        internal SweepContext()
        {
            _points = new List<TriPoint>();
            _triangles = new List<Triangle>();
            _map = new List<Triangle>();
            _cmp = new TriPoint.Comparer();
            _edge_list = new List<Edge>();
            EdgeEvent = new EdgeEvent();
            Basin = new SweepBasin();
        }

        public void AddHole(List<TriPoint> holePoints)
        {
            int first = _points.Count;
            _points.AddRange(holePoints);
            int last = _points.Count - 1;

            // Sanity check first and last point to make sure they haven't got the same position

            if (_points[first].Equals(_points[last]))
                _points.RemoveAt(last--);

            InitEdges(first, last);
        }

        public void AddPoint(TriPoint point)
        {
            _points.Add(point);
        }

        public void AddPoints(List<TriPoint> points)
        {
            int first = _points.Count;
            _points.AddRange(points);
            int last = _points.Count - 1;

            // Sanity check first and last point to make sure they haven't got the same position
            if (_points[first].Equals(_points[last]))
                _points.RemoveAt(last--);

            InitEdges(first, last);
        }

        public List<Triangle> GetTriangles()
        {
            return _triangles;
        }

        public List<Triangle> GetMap()
        {
            return _map;
        }

        public void InitTriangulation()
        {
            float xmax = _points[0].X;
            float xmin = _points[0].X;
            float ymax = _points[0].Y;
            float ymin = _points[0].Y;

            // Calculate bounds
            for(int i = 0; i < _points.Count; i++)
            {
                TriPoint p = _points[i];
                if (p.X > xmax)
                    xmax = p.X;
                if (p.X < xmin)
                    xmin = p.X;
                if (p.Y > ymax)
                    ymax = p.Y;
                if (p.Y < ymin)
                    ymin = p.Y;
            }

            float dx = K_ALPHA * (xmax - xmin);
            float dy = K_ALPHA * (ymax - ymin);

            _head = new TriPoint(xmax + dx, ymin - dy);
            _tail = new TriPoint(xmin - dx, ymin - dy);

            // Sort points along y-axis
            _points.Sort(_cmp);
        }

        private void InitEdges(int first, int last)
        {
            for(int i = first; i <= last; i++)
            {
                int j = i < last ? i + 1 : first;
                _edge_list.Add(new Edge(_points[i], _points[j]));
            }
        }

        public TriPoint GetPoint(int index)
        {
            return _points[index];
        }

        public void AddToMap(Triangle triangle)
        {
            _map.Add(triangle);
        }

        public Node LocateNode(TriPoint point)
        {
            // TODO implement search tree
            return Front.LocateNode(point.X);
        }

        public void CreateAdvancingFront(List<Node> nodes)
        {
            Triangle triangle = new Triangle(_points[0], _tail, _head);

            _map.Add(triangle);

            _af_head = new Node(triangle.Points[1], triangle);
            _af_middle = new Node(triangle.Points[0], triangle);
            _af_tail = new Node(triangle.Points[2]);
            Front = new AdvancingFront(_af_head, _af_tail);

            // TODO: More intuitive if head is middles next and not previous?
            //       so swap head and tail
            _af_head.Next = _af_middle;
            _af_middle.Next = _af_tail;
            _af_middle.Prev = _af_head;
            _af_tail.Prev = _af_middle;
        }

        public void RemoveNode(Node node)
        {
            // TODO: ???
            // delete node; (C++ code)
        }

        public void MapTriangleToNodes(Triangle t)
        {
            for(int i = 0; i < 3; i++)
            {
                if (t.GetNeighbor(i) == null)
                {
                    Node n = Front.LocatePoint(t.PointCW(t.Points[i]));
                    if (n != null)
                        n.Triangle = t;
                }
            }
        }

        public void RemoveFromMap(Triangle triangle)
        {
            _map.Remove(triangle);
        }

        public void MeshClean(Triangle triangle)
        {
            List<Triangle> triangles = new List<Triangle>();
            triangles.Add(triangle);

            while(triangles.Count != 0)
            {
                int last = triangles.Count - 1;
                Triangle t = triangles[last];
                triangles.RemoveAt(last);

                if(t != null && !t.IsInterior())
                {
                    t.IsInterior(true);
                    _triangles.Add(t);
                    for(int i = 0; i < 3; i++)
                    {
                        if (!t.ConstrainedEdge[i])
                            triangles.Add(t.GetNeighbor(i));
                    }
                }
            }
        }

        public int PointCount()
        {
            return _points.Count;
        }

        public void SetHead(TriPoint p1)
        {
            _head = p1;
        }

        public TriPoint GetHead()
        {
            return _head;
        }
    }
}
