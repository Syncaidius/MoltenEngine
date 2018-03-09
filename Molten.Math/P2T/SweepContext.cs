using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public class SweepContext
    {
        // Inital triangle factor, seed triangle will extend 30% of
        // PointSet width to both left and right.
        const double kAlpha = 0.3;

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

        public SweepContext()
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
            double xmax = _points[0].X;
            double xmin = _points[0].X;
            double ymax = _points[0].Y;
            double ymin = _points[0].Y;

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

            double dx = kAlpha * (xmax - xmin);
            double dy = kAlpha * (ymax - ymin);

            _head = new TriPoint(xmax + dx, ymin - dy);
            _tail = new TriPoint(xmin - dx, ymin - dy);

            // Sort points along y-axis
            _points.Sort(_cmp);
        }

        private void InitEdges(int first, int last)
        {
            for(int i = first; i <= last; i++)
            {
                int j = i < last ? i + 1 : 0;
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

            _af_head = new Node(triangle.GetPoint(1), triangle);
            _af_middle = new Node(triangle.GetPoint(0), triangle);
            _af_tail = new Node(triangle.GetPoint(2));
            Front = new AdvancingFront(_af_head, _af_tail);

            // TODO: More intuitive if head is middles next and not previous?
            //       so swap head and tail
            _af_head.next = _af_middle;
            _af_middle.next = _af_tail;
            _af_middle.prev = _af_head;
            _af_tail.prev = _af_middle;
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
                    Node n = Front.LocatePoint(t.PointCW(t.GetPoint(i)));
                    if (n != null)
                        n.triangle = t;
                }
            }
        }

        public void RemoveFromMap(Triangle triangle)
        {
            _map.Remove(triangle);
        }

        public void MeshClean(Triangle triangle)
        {
            Stack<Triangle> triangles = new Stack<Triangle>();
            triangles.Push(triangle);

            while(triangles.Count != 0)
            {
                Triangle t = triangles.Pop();

                if(t != null && !t.IsInterior())
                {
                    t.IsInterior(true);
                    _triangles.Add(t);
                    for(int i = 0; i < 3; i++)
                    {
                        if (!t.ConstrainedEdge[i])
                            triangles.Push(t.GetNeighbor(i));
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
