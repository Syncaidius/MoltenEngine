namespace Molten
{
    public struct Edge
    {
        public TriPoint P;

        public TriPoint Q;

        /// <summary>
        /// Represents a simple polygon's edge
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        public Edge(TriPoint p1, TriPoint p2)
        {
            P = p1;
            Q = p2;

            if (p1.Y > p2.Y)
            {
                Q = p1;
                P = p2;
            }
            else if (p1.Y == p2.Y)
            {
                if (p1.X > p2.X)
                {
                    Q = p1;
                    P = p2;
                }
                else if (p1.X == p2.X)
                {
                    // Repeat points
                    throw new Exception("Edge::Edge: p1 == p2");
                }
            }

            Q.EdgeList = Q.EdgeList ?? new List<Edge>();
            Q.EdgeList.Add(this);
        }
    }
}
