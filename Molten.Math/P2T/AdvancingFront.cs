using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    internal class AdvancingFront
    {
        internal Node _head;
        internal Node _tail;
        internal Node _search_node;

        internal AdvancingFront(Node head, Node tail)
        {
            _head = head;
            _tail = tail;
            _search_node = _head;
        }

        public Node LocateNode(double x)
        {
            Node node = _search_node;

            if (x < node.value)
            {
                while ((node = node.prev) != null)
                {
                    if (x >= node.value)
                    {
                        _search_node = node;
                        return node;
                    }
                }
            }
            else
            {
                while ((node = node.next) != null)
                {
                    if (x < node.value)
                    {
                        _search_node = node.prev;
                        return node.prev;
                    }
                }
            }

            return null;
        }

        private Node FindSearchNode(double x)
        {
            // suppress compiler warnings "unused parameter 'x'"
            // TODO: implement BST index
            return _search_node;
        }

        public Node LocatePoint(TriPoint point)
        {
            double px = point.X;
            Node node = FindSearchNode(px);
            double nx = node.point.X;

            if (px == nx)
            {
                if (!point.Equals(node.point))
                {
                    // We might have two nodes with same x value for a short time
                    if (point.Equals(node.prev.point))
                        node = node.prev;
                    else if (point.Equals(node.next.point))
                        node = node.next;
                    else
                        throw new Exception("What happened here???");
                }
            }
            else if (px < nx)
            {
                while ((node = node.prev) != null)
                {
                    if (point.Equals(node.point))
                        break;
                }
            }
            else
            {
                while ((node = node.next) != null)
                {
                    if (point.Equals(node.point))
                        break;
                }
            }
            if (node != null)
                _search_node = node;

            return node;
        }
    }
}
