using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    internal class Sweep
    {
        List<Node> _nodes;

        public void Triangulate(SweepContext tcx)
        {
            _nodes = new List<Node>();

            tcx.InitTriangulation();
            tcx.CreateAdvancingFront(_nodes);

            // Sweep points; build mesh
            SweepPoints(tcx);

            // Clean up
            FinalizationPolygon(tcx);
        }

        public void Reset()
        {
            _nodes.Clear();
        }

        private void SweepPoints(SweepContext tcx)
        {
            for (int i = 1; i < tcx.PointCount(); i++)
            {
                TriPoint point = tcx.GetPoint(i);
                Node node = PointEvent(tcx, point);

                if (point.EdgeList != null)
                {
                    for (int j = 0; j < point.EdgeList.Count; j++)
                        EdgeEvent(tcx, point.EdgeList[j], node);
                }
            }
        }

        private void FinalizationPolygon(SweepContext tcx)
        {
            // Get an Internal triangle to start with
            Triangle t = tcx.Front._head.Next.Triangle;
            TriPoint p = tcx.Front._head.Next.Point;
            while (!t.GetConstrainedEdgeCW(p))
                t = t.NeighborCCW(p);

            tcx.MeshClean(t);
        }

        private Node PointEvent(SweepContext tcx, TriPoint point)
        {
            Node node = tcx.LocateNode(point);
            Node new_node = NewFrontTriangle(tcx, point, node);

            // Only need to check +epsilon since point never have smaller
            // x value than node due to how we fetch nodes from the front
            if (point.X <= node.Point.X + TriUtil.EPSILON)
                Fill(tcx, node);

            //tcx.AddNode(new_node);

            FillAdvancingFront(tcx, new_node);
            return new_node;
        }

        private void EdgeEvent(SweepContext tcx, Edge edge, Node node)
        {
            tcx.EdgeEvent.ConstrainedEdge = edge;
            tcx.EdgeEvent.Right = (edge.P.X > edge.Q.X);

            if (IsEdgeSideOfTriangle(node.Triangle, edge.P, edge.Q))
                return;

            // For now we will do all needed filling
            // TODO: integrate with flip process might give some better performance
            //       but for now this avoid the issue with cases that needs both flips and fills
            FillEdgeEvent(tcx, edge, node);
            EdgeEvent(tcx, edge.P, edge.Q, node.Triangle, edge.Q);
        }

        private void EdgeEvent(SweepContext tcx, TriPoint ep, TriPoint eq, Triangle triangle, TriPoint point)
        {
            if (IsEdgeSideOfTriangle(triangle, ep, eq))
                return;

            TriPoint p1 = triangle.PointCCW(point);
            Winding o1 = TriUtil.Orient2d(eq, p1, ep);
            if (o1 == Winding.Collinear)
            {
                if (triangle.Contains(eq, p1))
                {
                    triangle.MarkConstrainedEdge(eq, p1);
                    // We are modifying the constraint maybe it would be better to
                    // not change the given constraint and just keep a variable for the new constraint
                    tcx.EdgeEvent.ConstrainedEdge.Q = p1;
                    triangle = triangle.NeighborAcross(point);
                    EdgeEvent(tcx, ep, p1, triangle, p1);
                }
                else
                {
                    throw new NotSupportedException("EdgeEvent - collinear points not supported");
                }

                return;
            }

            TriPoint p2 = triangle.PointCW(point);
            Winding o2 = TriUtil.Orient2d(eq, p2, ep);
            if (o2 == Winding.Collinear)
            {
                if (triangle.Contains(eq, p2))
                {
                    triangle.MarkConstrainedEdge(eq, p2);
                    // We are modifying the constraint maybe it would be better to
                    // not change the given constraint and just keep a variable for the new constraint
                    tcx.EdgeEvent.ConstrainedEdge.Q = p2;
                    triangle = triangle.NeighborAcross(point);
                    EdgeEvent(tcx, ep, p2, triangle, p2);
                }
                else
                {
                    throw new NotSupportedException("EdgeEvent - collinear points not supported");
                }

                return;
            }

            if (o1 == o2)
            {
                // Need to decide if we are rotating CW or CCW to get to a triangle
                // that will cross edge
                if (o1 == Winding.Clockwise)
                {
                    triangle = triangle.NeighborCCW(point);
                }
                else
                {
                    triangle = triangle.NeighborCW(point);
                }
                EdgeEvent(tcx, ep, eq, triangle, point);
            }
            else
            {
                // This triangle crosses constraint so lets flippin start!
                FlipEdgeEvent(tcx, ep, eq, triangle, point);
            }
        }

        private bool IsEdgeSideOfTriangle(Triangle triangle, TriPoint ep, TriPoint eq)
        {
            int index = triangle.EdgeIndex(ep, eq);

            if (index != -1)
            {
                triangle.MarkConstrainedEdge(index);
                Triangle t = triangle.GetNeighbor(index);
                if (t != null)
                    t.MarkConstrainedEdge(ep, eq);

                return true;
            }

            return false;
        }

        private Node NewFrontTriangle(SweepContext tcx, TriPoint point, Node node)
        {
            Triangle triangle = new Triangle(point, node.Point, node.Next.Point);

            triangle.MarkNeighbor(node.Triangle);
            tcx.AddToMap(triangle);

            Node new_node = new Node(point);
            _nodes.Add(new_node);

            new_node.Next = node.Next;
            new_node.Prev = node;
            node.Next.Prev = new_node;
            node.Next = new_node;

            if (!Legalize(tcx, triangle))
                tcx.MapTriangleToNodes(triangle);

            return new_node;
        }

        private void Fill(SweepContext tcx, Node node)
        {
            Triangle triangle = new Triangle(node.Prev.Point, node.Point, node.Next.Point);
            triangle.MarkNeighbor(node.Prev.Triangle);
            triangle.MarkNeighbor(node.Triangle);

            tcx.AddToMap(triangle);

            // Update the advancing front
            node.Prev.Next = node.Next;
            node.Next.Prev = node.Prev;

            // If it was legalized the triangle has already been mapped
            if (!Legalize(tcx, triangle))
                tcx.MapTriangleToNodes(triangle);
        }

        private void FillAdvancingFront(SweepContext tcx, Node n)
        {
            // Fill right holes
            Node node = n.Next;

            while (node.Next != null)
            {
                // if HoleAngle exceeds 90 degrees then break.
                if (LargeHole_DontFill(node)) break;
                Fill(tcx, node);
                node = node.Next;
            }

            // Fill left holes
            node = n.Prev;

            while (node.Prev != null)
            {
                // if HoleAngle exceeds 90 degrees then break.
                if (LargeHole_DontFill(node)) break;
                Fill(tcx, node);
                node = node.Prev;
            }

            // Fill right basins
            if (n.Next != null && n.Next.Next != null)
            {
                double angle = BasinAngle(n);
                if (angle < TriUtil.PI_3div4)
                    FillBasin(tcx, n);
            }
        }

        private bool LargeHole_DontFill(Node node)
        {
            Node nextNode = node.Next;
            Node prevNode = node.Prev;
            if (!AngleExceeds90Degrees(node.Point, nextNode.Point, prevNode.Point))
                return false;

            // Check additional points on front.
            Node next2Node = nextNode.Next;
            // "..Plus.." because only want angles on same side as point being added.
            if ((next2Node != null) && !AngleExceedsPlus90DegreesOrIsNegative(node.Point, next2Node.Point, prevNode.Point))
                return false;

            Node prev2Node = prevNode.Prev;
            // "..Plus.." because only want angles on same side as point being added.
            if ((prev2Node != null) && !AngleExceedsPlus90DegreesOrIsNegative(node.Point, nextNode.Point, prev2Node.Point))
                return false;

            return true;
        }

        private bool AngleExceeds90Degrees(TriPoint origin, TriPoint pa, TriPoint pb)
        {
            double angle = Angle(origin, pa, pb);
            return ((angle > TriUtil.PI_div2) || (angle < -TriUtil.PI_div2));
        }

        private bool AngleExceedsPlus90DegreesOrIsNegative(TriPoint origin, TriPoint pa, TriPoint pb)
        {
            double angle = Angle(origin, pa, pb);
            return (angle > TriUtil.PI_div2) || (angle < 0);
        }

        private double Angle(TriPoint origin, TriPoint pa, TriPoint pb)
        {
            /* Complex plane
            * ab = cosA +i*sinA
            * ab = (ax + ay*i)(bx + by*i) = (ax*bx + ay*by) + i(ax*by-ay*bx)
            * atan2(y,x) computes the principal value of the argument function
            * applied to the complex number x+iy
            * Where x = ax*bx + ay*by
            *       y = ax*by - ay*bx
            */
            double px = origin.X;
            double py = origin.Y;
            double ax = pa.X - px;
            double ay = pa.Y - py;
            double bx = pb.X - px;
            double by = pb.Y - py;
            double x = ax * by - ay * bx;
            double y = ax * bx + ay * by;
            return System.Math.Atan2(x, y);
        }

        private double BasinAngle(Node node)
        {
            double ax = node.Point.X - node.Next.Next.Point.X;
            double ay = node.Point.Y - node.Next.Next.Point.Y;
            return Math.Atan2(ay, ax);
        }

        private double HoleAngle(Node node)
        {
            /* Complex plane
            * ab = cosA +i*sinA
            * ab = (ax + ay*i)(bx + by*i) = (ax*bx + ay*by) + i(ax*by-ay*bx)
            * atan2(y,x) computes the principal value of the argument function
            * applied to the complex number x+iy
            * Where x = ax*bx + ay*by
            *       y = ax*by - ay*bx
            */
            double ax = node.Next.Point.X - node.Point.X;
            double ay = node.Next.Point.Y - node.Point.Y;
            double bx = node.Prev.Point.X - node.Point.X;
            double by = node.Prev.Point.Y - node.Point.Y;
            return Math.Atan2(ax * by - ay * bx, ax * bx + ay * by);
        }

        private bool Legalize(SweepContext tcx, Triangle t)
        {
            // To legalize a triangle we start by finding if any of the three edges
            // violate the Delaunay condition
            for (int i = 0; i < 3; i++)
            {
                if (t.DelaunayEdge[i])
                    continue;

                Triangle ot = t.GetNeighbor(i);

                if (ot != null)
                {
                    TriPoint p = t.Points[i];
                    TriPoint op = ot.OppositePoint(t, p);
                    int oi = ot.Index(op);

                    // If this is a Constrained Edge or a Delaunay Edge(only during recursive legalization)
                    // then we should not try to legalize
                    if (ot.ConstrainedEdge[oi] || ot.DelaunayEdge[oi])
                    {
                        t.ConstrainedEdge[i] = ot.ConstrainedEdge[oi];
                        continue;
                    }

                    bool inside = Incircle(p, t.PointCCW(p), t.PointCW(p), op);

                    if (inside)
                    {
                        // Lets mark this shared edge as Delaunay
                        t.DelaunayEdge[i] = true;
                        ot.DelaunayEdge[oi] = true;

                        // Lets rotate shared edge one vertex CW to legalize it
                        RotateTrianglePair(t, p, ot, op);

                        // We now got one valid Delaunay Edge shared by two triangles
                        // This gives us 4 new edges to check for Delaunay

                        // Make sure that triangle to node mapping is done only one time for a specific triangle
                        bool not_legalized = !Legalize(tcx, t);
                        if (not_legalized)
                        {
                            tcx.MapTriangleToNodes(t);
                        }

                        not_legalized = !Legalize(tcx, ot);
                        if (not_legalized)
                            tcx.MapTriangleToNodes(ot);

                        // Reset the Delaunay edges, since they only are valid Delaunay edges
                        // until we add a new triangle or point.
                        // XXX: need to think about this. Can these edges be tried after we
                        //      return to previous recursive level?
                        t.DelaunayEdge[i] = false;
                        ot.DelaunayEdge[oi] = false;

                        // If triangle have been legalized no need to check the other edges since
                        // the recursive legalization will handles those so we can end here.
                        return true;
                    }
                }
            }

            return false;
        }

        /**
        * <b>Requirement</b>:<br>
        * 1. a,b and c form a triangle.<br>
        * 2. a and d is know to be on opposite side of bc<br>
        * <pre>
        *                a
        *                +
        *               / \
        *              /   \
        *            b/     \c
        *            +-------+
        *           /    d    \
        *          /           \
        * </pre>
        * <b>Fact</b>: d has to be in area B to have a chance to be inside the circle formed by
        *  a,b and c<br>
        *  d is outside B if TriUtil.Orient2d(a,b,d) or TriUtil.Orient2d(c,a,d) is CW<br>
        *  This preknowledge gives us a way to optimize the incircle test
        * @param a - triangle point, opposite d
        * @param b - triangle point
        * @param c - triangle point
        * @param d - point opposite a
        * @return true if d is inside circle, false if on circle edge
        */
        private bool Incircle(TriPoint pa, TriPoint pb, TriPoint pc, TriPoint pd)
        {
            double adx = pa.X - pd.X;
            double ady = pa.Y - pd.Y;
            double bdx = pb.X - pd.X;
            double bdy = pb.Y - pd.Y;

            double adxbdy = adx * bdy;
            double bdxady = bdx * ady;
            double oabd = adxbdy - bdxady;

            if (oabd <= 0)
                return false;

            double cdx = pc.X - pd.X;
            double cdy = pc.Y - pd.Y;

            double cdxady = cdx * ady;
            double adxcdy = adx * cdy;
            double ocad = cdxady - adxcdy;

            if (ocad <= 0)
                return false;

            double bdxcdy = bdx * cdy;
            double cdxbdy = cdx * bdy;

            double alift = adx * adx + ady * ady;
            double blift = bdx * bdx + bdy * bdy;
            double clift = cdx * cdx + cdy * cdy;

            double det = alift * (bdxcdy - cdxbdy) + blift * ocad + clift * oabd;

            return det > 0;
        }

        /**
        * Rotates a triangle pair one vertex CW
        *<pre>
        *       n2                    n2
        *  P +-----+             P +-----+
        *    | t  /|               |\  t |
        *    |   / |               | \   |
        *  n1|  /  |n3           n1|  \  |n3
        *    | /   |    after CW   |   \ |
        *    |/ oT |               | oT \|
        *    +-----+ oP            +-----+
        *       n4                    n4
        * </pre>
        */
        private void RotateTrianglePair(Triangle t, TriPoint p, Triangle ot, TriPoint op)
        {
            Triangle n1, n2, n3, n4;
            n1 = t.NeighborCCW(p);
            n2 = t.NeighborCW(p);
            n3 = ot.NeighborCCW(op);
            n4 = ot.NeighborCW(op);

            bool ce1, ce2, ce3, ce4;
            ce1 = t.GetConstrainedEdgeCCW(p);
            ce2 = t.GetConstrainedEdgeCW(p);
            ce3 = ot.GetConstrainedEdgeCCW(op);
            ce4 = ot.GetConstrainedEdgeCW(op);

            bool de1, de2, de3, de4;
            de1 = t.GetDelaunayEdgeCCW(p);
            de2 = t.GetDelaunayEdgeCW(p);
            de3 = ot.GetDelaunayEdgeCCW(op);
            de4 = ot.GetDelaunayEdgeCW(op);

            t.Legalize(p, op);
            ot.Legalize(op, p);

            // Remap delaunay_edge
            ot.SetDelunayEdgeCCW(p, de1);
            t.SetDelunayEdgeCW(p, de2);
            t.SetDelunayEdgeCCW(op, de3);
            ot.SetDelunayEdgeCW(op, de4);

            // Remap constrained_edge
            ot.SetConstrainedEdgeCCW(p, ce1);
            t.SetConstrainedEdgeCW(p, ce2);
            t.SetConstrainedEdgeCCW(op, ce3);
            ot.SetConstrainedEdgeCW(op, ce4);

            // Remap neighbors
            // XXX: might optimize the markNeighbor by keeping track of
            //      what side should be assigned to what neighbor after the
            //      rotation. Now mark neighbor does lots of testing to find
            //      the right side.
            t.ClearNeighbors();
            ot.ClearNeighbors();
            if (n1 != null) ot.MarkNeighbor(n1);
            if (n2 != null) t.MarkNeighbor(n2);
            if (n3 != null) t.MarkNeighbor(n3);
            if (n4 != null) ot.MarkNeighbor(n4);
            t.MarkNeighbor(ot);
        }

        private void FillBasin(SweepContext tcx, Node node)
        {
            if (TriUtil.Orient2d(node.Point, node.Next.Point, node.Next.Next.Point) == Winding.CounterClockwise)
                tcx.Basin.LeftNode = node.Next.Next;
            else
                tcx.Basin.LeftNode = node.Next;

            // Find the bottom and right node
            tcx.Basin.BottomNode = tcx.Basin.LeftNode;
            while (tcx.Basin.BottomNode.Next != null
                   && tcx.Basin.BottomNode.Point.Y >= tcx.Basin.BottomNode.Next.Point.Y)
            {
                tcx.Basin.BottomNode = tcx.Basin.BottomNode.Next;
            }
            if (tcx.Basin.BottomNode == tcx.Basin.LeftNode)
            {
                // No valid basin
                return;
            }

            tcx.Basin.RightNode = tcx.Basin.BottomNode;
            while (tcx.Basin.RightNode.Next != null
                   && tcx.Basin.RightNode.Point.Y < tcx.Basin.RightNode.Next.Point.Y)
            {
                tcx.Basin.RightNode = tcx.Basin.RightNode.Next;
            }
            if (tcx.Basin.RightNode == tcx.Basin.BottomNode)
            {
                // No valid basins
                return;
            }

            tcx.Basin.Width = tcx.Basin.RightNode.Point.X - tcx.Basin.LeftNode.Point.X;
            tcx.Basin.LeftHighest = tcx.Basin.LeftNode.Point.Y > tcx.Basin.RightNode.Point.Y;

            FillBasinReq(tcx, tcx.Basin.BottomNode);
        }

        private void FillBasinReq(SweepContext tcx, Node node)
        {
            // if shallow stop filling
            if (IsShallow(tcx, node))
            {
                return;
            }

            Fill(tcx, node);

            if (node.Prev == tcx.Basin.LeftNode && node.Next == tcx.Basin.RightNode)
            {
                return;
            }
            else if (node.Prev == tcx.Basin.LeftNode)
            {
                Winding o = TriUtil.Orient2d(node.Point, node.Next.Point, node.Next.Next.Point);
                if (o == Winding.Clockwise)
                    return;

                node = node.Next;
            }
            else if (node.Next == tcx.Basin.RightNode)
            {
                Winding o = TriUtil.Orient2d(node.Point, node.Prev.Point, node.Prev.Prev.Point);
                if (o == Winding.CounterClockwise)
                    return;

                node = node.Prev;
            }
            else
            {
                // Continue with the neighbor node with lowest Y value
                if (node.Prev.Point.Y < node.Next.Point.Y)
                    node = node.Prev;
                else
                    node = node.Next;
            }

            FillBasinReq(tcx, node);
        }

        private bool IsShallow(SweepContext tcx, Node node)
        {
            double height;

            if (tcx.Basin.LeftHighest)
                height = tcx.Basin.LeftNode.Point.Y - node.Point.Y;
            else
                height = tcx.Basin.RightNode.Point.Y - node.Point.Y;

            // if shallow stop filling
            if (tcx.Basin.Width > height)
                return true;

            return false;
        }

        private void FillEdgeEvent(SweepContext tcx, Edge edge, Node node)
        {
            if (tcx.EdgeEvent.Right)
                FillRightAboveEdgeEvent(tcx, edge, node);
            else
                FillLeftAboveEdgeEvent(tcx, edge, node);
        }

        private void FillRightAboveEdgeEvent(SweepContext tcx, Edge edge, Node node)
        {
            while (node.Next.Point.X < edge.P.X)
            {
                // Check if next node is below the edge
                if (TriUtil.Orient2d(edge.Q, node.Next.Point, edge.P) == Winding.CounterClockwise)
                    FillRightBelowEdgeEvent(tcx, edge, node);
                else
                    node = node.Next;
            }
        }

        private void FillRightBelowEdgeEvent(SweepContext tcx, Edge edge, Node node)
        {
            if (node.Point.X < edge.P.X)
            {
                if (TriUtil.Orient2d(node.Point, node.Next.Point, node.Next.Next.Point) == Winding.CounterClockwise)
                {
                    // Concave
                    FillRightConcaveEdgeEvent(tcx, edge, node);
                }
                else
                {
                    // Convex
                    FillRightConvexEdgeEvent(tcx, edge, node);
                    // Retry this one
                    FillRightBelowEdgeEvent(tcx, edge, node);
                }
            }
        }

        private void FillRightConcaveEdgeEvent(SweepContext tcx, Edge edge, Node node)
        {
            Fill(tcx, node.Next);
            if (node.Next.Point != edge.P)
            {
                // Next above or below edge?
                if (TriUtil.Orient2d(edge.Q, node.Next.Point, edge.P) == Winding.CounterClockwise)
                {
                    // Below
                    if (TriUtil.Orient2d(node.Point, node.Next.Point, node.Next.Next.Point) == Winding.CounterClockwise)
                    {
                        // Next is concave
                        FillRightConcaveEdgeEvent(tcx, edge, node);
                    }
                    else
                    {
                        // Next is convex
                    }
                }
            }
        }

        private void FillRightConvexEdgeEvent(SweepContext tcx, Edge edge, Node node)
        {
            // Next concave or convex?
            if (TriUtil.Orient2d(node.Next.Point, node.Next.Next.Point, node.Next.Next.Next.Point) == Winding.CounterClockwise)
            {
                // Concave
                FillRightConcaveEdgeEvent(tcx, edge, node.Next);
            }
            else
            {
                // Convex
                // Next above or below edge?
                if (TriUtil.Orient2d(edge.Q, node.Next.Next.Point, edge.P) == Winding.CounterClockwise)
                {
                    // Below
                    FillRightConvexEdgeEvent(tcx, edge, node.Next);
                }
                else
                {
                    // Above
                }
            }
        }

        private void FillLeftAboveEdgeEvent(SweepContext tcx, Edge edge, Node node)
        {
            while (node.Prev.Point.X > edge.P.X)
            {
                // Check if next node is below the edge
                if (TriUtil.Orient2d(edge.Q, node.Prev.Point, edge.P) == Winding.Clockwise)
                    FillLeftBelowEdgeEvent(tcx, edge, node);
                else
                    node = node.Prev;
            }
        }

        private void FillLeftBelowEdgeEvent(SweepContext tcx, Edge edge, Node node)
        {
            if (node.Point.X > edge.P.X)
            {
                if (TriUtil.Orient2d(node.Point, node.Prev.Point, node.Prev.Prev.Point) == Winding.Clockwise)
                {
                    // Concave
                    FillLeftConcaveEdgeEvent(tcx, edge, node);
                }
                else
                {
                    // Convex
                    FillLeftConvexEdgeEvent(tcx, edge, node);
                    // Retry this one
                    FillLeftBelowEdgeEvent(tcx, edge, node);
                }
            }
        }

        private void FillLeftConvexEdgeEvent(SweepContext tcx, Edge edge, Node node)
        {
            // Next concave or convex?
            if (TriUtil.Orient2d(node.Prev.Point, node.Prev.Prev.Point, node.Prev.Prev.Prev.Point) == Winding.Clockwise)
            {
                // Concave
                FillLeftConcaveEdgeEvent(tcx, edge, node.Prev);
            }
            else
            {
                // Convex
                // Next above or below edge?
                if (TriUtil.Orient2d(edge.Q, node.Prev.Prev.Point, edge.P) == Winding.Clockwise)
                {
                    // Below
                    FillLeftConvexEdgeEvent(tcx, edge, node.Prev);
                }
                else
                {
                    // Above
                }
            }
        }

        private void FillLeftConcaveEdgeEvent(SweepContext tcx, Edge edge, Node node)
        {
            Fill(tcx, node.Prev);
            if (node.Prev.Point != edge.P)
            {
                // Next above or below edge?
                if (TriUtil.Orient2d(edge.Q, node.Prev.Point, edge.P) == Winding.Clockwise)
                {
                    // Below
                    if (TriUtil.Orient2d(node.Point, node.Prev.Point, node.Prev.Prev.Point) == Winding.Clockwise)
                    {
                        // Next is concave
                        FillLeftConcaveEdgeEvent(tcx, edge, node);
                    }
                    else
                    {
                        // Next is convex
                    }
                }
            }
        }

        private void FlipEdgeEvent(SweepContext tcx, TriPoint ep, TriPoint eq, Triangle t, TriPoint p)
        {
            Triangle ot = t.NeighborAcross(p);
            TriPoint op = ot.OppositePoint(t, p);

            if (TriUtil.InScanArea(p, t.PointCCW(p), t.PointCW(p), op))
            {
                // Lets rotate shared edge one vertex CW
                RotateTrianglePair(t, p, ot, op);
                tcx.MapTriangleToNodes(t);
                tcx.MapTriangleToNodes(ot);

                if (p == eq && op == ep)
                {
                    if (eq == tcx.EdgeEvent.ConstrainedEdge.Q && ep == tcx.EdgeEvent.ConstrainedEdge.P)
                    {
                        t.MarkConstrainedEdge(ep, eq);
                        ot.MarkConstrainedEdge(ep, eq);
                        Legalize(tcx, t);
                        Legalize(tcx, ot);
                    }
                    else
                    {
                        // XXX: I think one of the triangles should be legalized here?
                    }
                }
                else
                {
                    Winding o = TriUtil.Orient2d(eq, op, ep);
                    t = NextFlipTriangle(tcx, o, t, ot, p, op);
                    FlipEdgeEvent(tcx, ep, eq, t, p);
                }
            }
            else
            {
                TriPoint newP = NextFlipPoint(ep, eq, ot, op);
                FlipScanEdgeEvent(tcx, ep, eq, t, ot, newP);
                EdgeEvent(tcx, ep, eq, t, p);
            }
        }

        private Triangle NextFlipTriangle(SweepContext tcx, Winding o, Triangle t, Triangle ot, TriPoint p, TriPoint op)
        {
            int edge_index;

            if (o == Winding.CounterClockwise)
            {
                // ot is not crossing edge after flip
                edge_index = ot.EdgeIndex(p, op);
                ot.DelaunayEdge[edge_index] = true;
                Legalize(tcx, ot);
                ot.ClearDelaunayEdges();
                return t;
            }

            // t is not crossing edge after flip
            edge_index = t.EdgeIndex(p, op);

            t.DelaunayEdge[edge_index] = true;
            Legalize(tcx, t);
            t.ClearDelaunayEdges();
            return ot;
        }

        private TriPoint NextFlipPoint(TriPoint ep, TriPoint eq, Triangle ot, TriPoint op)
        {
            Winding o2d = TriUtil.Orient2d(eq, op, ep);
            if (o2d == Winding.Clockwise)
            {
                // Right
                return ot.PointCCW(op);
            }
            else if (o2d == Winding.CounterClockwise)
            {
                // Left
                return ot.PointCW(op);
            }

            throw new NotSupportedException("[Unsupported] Opposing point on constrained edge");
        }

        private void FlipScanEdgeEvent(SweepContext tcx, TriPoint ep, TriPoint eq, Triangle flip_triangle, Triangle t, TriPoint p)
        {
            Triangle ot = t.NeighborAcross(p);
            TriPoint op = ot.OppositePoint(t, p);

            if (TriUtil.InScanArea(eq, flip_triangle.PointCCW(eq), flip_triangle.PointCW(eq), op))
            {
                // flip with new edge op->eq
                FlipEdgeEvent(tcx, eq, op, ot, op);
                // TODO: Actually I just figured out that it should be possible to
                //       improve this by getting the next ot and op before the the above
                //       flip and continue the flipScanEdgeEvent here
                // set new ot and op here and loop back to inScanArea test
                // also need to set a new flip_triangle first
                // Turns out at first glance that this is somewhat complicated
                // so it will have to wait.
            }
            else
            {
                TriPoint newP = NextFlipPoint(ep, eq, ot, op);
                FlipScanEdgeEvent(tcx, ep, eq, flip_triangle, ot, newP);
            }
        }
    }
}
