using System.Diagnostics;

namespace Molten
{
    internal class AdvancingFront
    {
        internal Node _head;
        internal Node _tail;
        Node _search_node;

        internal AdvancingFront(Node head, Node tail)
        {
            _head = head;
            _tail = tail;
            _search_node = _head;
        }

        public Node LocateNode(double x)
        {
            Node node = _search_node;

            if (x < node.Value)
            {
                while ((node = node.Prev) != null)
                {
                    if (x >= node.Value)
                    {
                        _search_node = node;
                        return node;
                    }
                }
            }
            else
            {
                while ((node = node.Next) != null)
                {
                    if (x < node.Value)
                    {
                        _search_node = node.Prev;
                        return node.Prev;
                    }
                }
            }

            return null;
        }

        public Node LocatePoint(TriPoint point)
        {
            double px = point.X;
            Node node = _search_node;
            double nx = node.Point.X;

            if (px == nx)
            {
                if (point != node.Point)
                {
                    // We might have two nodes with same x value for a short time
                    if (point == node.Prev.Point)
                        node = node.Prev;
                    else if (point == node.Next.Point)
                        node = node.Next;
                    else
                        Debug.Assert(false, "What happened here????");
                }
            }
            else if (px < nx)
            {
                while ((node = node.Prev) != null)
                {
                    if (point == node.Point)
                        break;
                }
            }
            else
            {
                while ((node = node.Next) != null)
                {
                    if (point == node.Point)
                        break;
                }
            }
            if (node != null)
                _search_node = node;

            return node;
        }
    }
}
