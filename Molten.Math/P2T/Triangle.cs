using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Math.P2T
{
    public class Triangle
    {
        TriPoint[] _points;
        public bool[] ConstrainedEdge;
        public bool[] DelauneyEdge;
        Triangle[] _neighbours;
        bool _interior;

        public Triangle(TriPoint a, TriPoint b, TriPoint c)
        {
            _points = new TriPoint[] { a, b, c };
            ConstrainedEdge = new bool[3];
            DelauneyEdge = new bool[3];
            _neighbours = new Triangle[3];
        }

        public TriPoint GetPoint(int index)
        {
            return _points[index];
        }

        public bool Contains(TriPoint p)
        {
            return _points[0] == p || _points[1] == p || _points[2] == p;
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
    }
}
