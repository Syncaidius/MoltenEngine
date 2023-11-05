namespace Molten
{
    internal struct TriEdge
    {
        public TriPoint P1;

        public TriPoint P2;

        /// <summary>
        /// Represents a simple polygon's edge
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        internal TriEdge(TriPoint p1, TriPoint p2)
        {
            P1 = p1;
            P2 = p2;

            if (p1.Y > p2.Y)
            {
                P2 = p1;
                P1 = p2;
            }
            else if (p1.Y == p2.Y)
            {
                if (p1.X > p2.X)
                {
                    P2 = p1;
                    P1 = p2;
                }
                else if (p1.X == p2.X)
                {
                    // Repeat points
                    throw new Exception("Edge::Edge: p1 == p2");
                }
            }

            P2.EdgeList ??= new List<TriEdge>();
            P2.EdgeList.Add(this);
        }
    }
}
