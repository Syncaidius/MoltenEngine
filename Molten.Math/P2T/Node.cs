using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    internal class Node
    {
        public TriPoint Point;

        public Triangle Triangle;

        public Node Next;

        public Node Prev;

        public double Value;

        public Node(TriPoint p)
        {
            Point = p;
            Value = p.X;
        }

        public Node(TriPoint p, Triangle t)
        {
            Point = p;
            Triangle = t;
            Value = p.X;    
        }
    }
}
