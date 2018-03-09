using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public class Node
    {
        public TriPoint point;

        public Triangle triangle;

        public Node next;

        public Node prev;

        public double value;

        public Node(TriPoint p)
        {
            point = p;
            value = p.X;
        }

        public Node(TriPoint p, Triangle t)
        {
            point = p;
            triangle = t;
            value = p.X;    
        }
    }
}
