using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public class Sweep
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
            for(int i = 1; i < tcx.PointCount(); i++)
            {
                TriPoint point = tcx.GetPoint(i);
                Node node = PointEvent(tcx, point);
                for (int j = 0; j < point.EdgeList.Count; j++)
                    EdgeEvent(tcx, point.EdgeList[j], node);
            }
        }

        private void FinalizationPolygon(SweepContext tcx)
        {
            // Get an Internal triangle to start with
            Triangle t = tcx.Front._head.next.triangle;
            TriPoint p = tcx.Front._head.next.point;
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
            if (point.X <= node.point.X + TriUtil.EPSILON)
                Fill(tcx, node);

            //tcx.AddNode(new_node);

            FillAdvancingFront(tcx, new_node);
            return new_node;
        }

        private void EdgeEvent(SweepContext tcx, Edge edge, Node node)
        {
            tcx.EdgeEvent.ConstrainedEdge = edge;
            tcx.EdgeEvent.Right = (edge.P.X > edge.Q.X);

            if (IsEdgeSideOfTriangle(node.triangle, edge.P, edge.Q))
                return;

            // For now we will do all needed filling
            // TODO: integrate with flip process might give some better performance
            //       but for now this avoid the issue with cases that needs both flips and fills
            FillEdgeEvent(tcx, edge, node);
            EdgeEvent(tcx, edge.P, edge.Q, node.triangle, edge.Q);
        }

        private void EdgeEvent(SweepContext tcx, TriPoint ep, TriPoint eq, Triangle triangle, TriPoint point)
        {
            if (IsEdgeSideOfTriangle(triangle, ep, eq))
                return;

            TriPoint p1 = triangle.PointCCW(point);
            Winding o1 = TriUtil.Orient2d(eq, p1, ep);
            if(o1 == Winding.Collinear)
            {
                if(triangle.Contains(eq, p1))
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
            if(o2 == Winding.Collinear)
            {
                if(triangle.Contains(eq, p2))
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
                if (o1 == Winding.CW)
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

            if(index != -1)
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
            Triangle triangle = new Triangle(point, node.point, node.next.point);

            triangle.MarkNeighbor(node.triangle);
            tcx.AddToMap(triangle);

            Node new_node = new Node(point);
            _nodes.Add(new_node);

            new_node.next = node.next;
            new_node.prev = node;
            node.next.prev = new_node;
            node.next = new_node;

            if (!Legalize(tcx, triangle))
                tcx.MapTriangleToNodes(triangle);

            return new_node;
        }

        private void Fill(SweepContext tcx, Node node)
        {
            Triangle triangle = new Triangle(node.prev.point, node.point, node.next.point);

            // TODO: should copy the constrained_edge value from neighbor triangles
            //       for now constrained_edge values are copied during the legalize
            triangle.MarkNeighbor(node.prev.triangle);
            triangle.MarkNeighbor(node.triangle);

            tcx.AddToMap(triangle);

            // Update the advancing front
            node.prev.next = node.next;
            node.next.prev = node.prev;

            // If it was legalized the triangle has already been mapped
            if (!Legalize(tcx, triangle))
                tcx.MapTriangleToNodes(triangle);
        }

        private void FillAdvancingFront(SweepContext tcx, Node n)
        {
            // Fill right holes
            Node node = n.next;

            while (node.next != null)
            {
                // if HoleAngle exceeds 90 degrees then break.
                if (LargeHole_DontFill(node)) break;
                Fill(tcx, node);
                node = node.next;
            }

            // Fill left holes
            node = n.prev;

            while (node.prev != null)
            {
                // if HoleAngle exceeds 90 degrees then break.
                if (LargeHole_DontFill(node)) break;
                Fill(tcx, node);
                node = node.prev;
            }

            // Fill right basins
            if (n.next != null && n.next.next != null)
            {
                double angle = BasinAngle(n);
                if (angle < TriUtil.PI_3div4)
                    FillBasin(tcx, n);
            }
        }

        private bool LargeHole_DontFill(Node node)
        {
            Node nextNode = node.next;
            Node prevNode = node.prev;
            if (!AngleExceeds90Degrees(node.point, nextNode.point, prevNode.point))
                return false;

            // Check additional points on front.
            Node next2Node = nextNode.next;
            // "..Plus.." because only want angles on same side as point being added.
            if ((next2Node != null) && !AngleExceedsPlus90DegreesOrIsNegative(node.point, next2Node.point, prevNode.point))
                return false;

            Node prev2Node = prevNode.prev;
            // "..Plus.." because only want angles on same side as point being added.
            if ((prev2Node != null) && !AngleExceedsPlus90DegreesOrIsNegative(node.point, nextNode.point, prev2Node.point))
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
            double ax = node.point.X - node.next.next.point.X;
            double ay = node.point.Y - node.next.next.point.Y;
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
            double ax = node.next.point.X - node.point.X;
            double ay = node.next.point.Y - node.point.Y;
            double bx = node.prev.point.X - node.point.X;
            double by = node.prev.point.Y - node.point.Y;
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
                    TriPoint p = t.GetPoint(i);
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
            if (TriUtil.Orient2d(node.point, node.next.point, node.next.next.point) == Winding.CCW)
                tcx.Basin.left_node = node.next.next;
            else
                tcx.Basin.left_node = node.next;

            // Find the bottom and right node
            tcx.Basin.bottom_node = tcx.Basin.left_node;
            while (tcx.Basin.bottom_node.next != null
                   && tcx.Basin.bottom_node.point.Y >= tcx.Basin.bottom_node.next.point.Y)
            {
                tcx.Basin.bottom_node = tcx.Basin.bottom_node.next;
            }
            if (tcx.Basin.bottom_node == tcx.Basin.left_node)
            {
                // No valid basin
                return;
            }

            tcx.Basin.right_node = tcx.Basin.bottom_node;
            while (tcx.Basin.right_node.next != null
                   && tcx.Basin.right_node.point.Y < tcx.Basin.right_node.next.point.Y)
            {
                tcx.Basin.right_node = tcx.Basin.right_node.next;
            }
            if (tcx.Basin.right_node == tcx.Basin.bottom_node)
            {
                // No valid basins
                return;
            }

            tcx.Basin.width = tcx.Basin.right_node.point.X - tcx.Basin.left_node.point.X;
            tcx.Basin.left_highest = tcx.Basin.left_node.point.Y > tcx.Basin.right_node.point.Y;

            FillBasinReq(tcx, tcx.Basin.bottom_node);
        }

        private void FillBasinReq(SweepContext tcx, Node node)
        {
            // if shallow stop filling
            if (IsShallow(tcx, node))
            {
                return;
            }

            Fill(tcx, node);

            if (node.prev == tcx.Basin.left_node && node.next == tcx.Basin.right_node)
            {
                return;
            }
            else if (node.prev == tcx.Basin.left_node)
            {
                Winding o = TriUtil.Orient2d(node.point, node.next.point, node.next.next.point);
                if (o == Winding.CW)
                    return;

                node = node.next;
            }
            else if (node.next == tcx.Basin.right_node)
            {
                Winding o = TriUtil.Orient2d(node.point, node.prev.point, node.prev.prev.point);
                if (o == Winding.CCW)
                    return;

                node = node.prev;
            }
            else
            {
                // Continue with the neighbor node with lowest Y value
                if (node.prev.point.Y < node.next.point.Y)
                    node = node.prev;
                else
                    node = node.next;
            }

            FillBasinReq(tcx, node);
        }

        private bool IsShallow(SweepContext tcx, Node node)
        {
            double height;

            if (tcx.Basin.left_highest)
                height = tcx.Basin.left_node.point.Y - node.point.Y;
            else
                height = tcx.Basin.right_node.point.Y - node.point.Y;

            // if shallow stop filling
            if (tcx.Basin.width > height)
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
            while (node.next.point.X < edge.P.X)
            {
                // Check if next node is below the edge
                if (TriUtil.Orient2d(edge.Q, node.next.point, edge.P) == Winding.CCW)
                    FillRightBelowEdgeEvent(tcx, edge, node);
                else
                    node = node.next;
            }
        }

        private void FillRightBelowEdgeEvent(SweepContext tcx, Edge edge, Node node)
        {
            if (node.point.X < edge.P.X)
            {
                if (TriUtil.Orient2d(node.point, node.next.point, node.next.next.point) == Winding.CCW)
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
            Fill(tcx, node.next);
            if (node.next.point != edge.P)
            {
                // Next above or below edge?
                if (TriUtil.Orient2d(edge.Q, node.next.point, edge.P) == Winding.CCW)
                {
                    // Below
                    if (TriUtil.Orient2d(node.point, node.next.point, node.next.next.point) == Winding.CCW)
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
            if (TriUtil.Orient2d(node.next.point, node.next.next.point, node.next.next.next.point) == Winding.CCW)
            {
                // Concave
                FillRightConcaveEdgeEvent(tcx, edge, node.next);
            }
            else
            {
                // Convex
                // Next above or below edge?
                if (TriUtil.Orient2d(edge.Q, node.next.next.point, edge.P) == Winding.CCW)
                {
                    // Below
                    FillRightConvexEdgeEvent(tcx, edge, node.next);
                }
                else
                {
                    // Above
                }
            }
        }

        private void FillLeftAboveEdgeEvent(SweepContext tcx, Edge edge, Node node)
        {
            while (node.prev.point.X > edge.P.X)
            {
                // Check if next node is below the edge
                if (TriUtil.Orient2d(edge.Q, node.prev.point, edge.P) == Winding.CW)
                    FillLeftBelowEdgeEvent(tcx, edge, node);
                else
                    node = node.prev;
            }
        }

        private void FillLeftBelowEdgeEvent(SweepContext tcx, Edge edge, Node node)
        {
            if (node.point.X > edge.P.X)
            {
                if (TriUtil.Orient2d(node.point, node.prev.point, node.prev.prev.point) == Winding.CW)
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
            if (TriUtil.Orient2d(node.prev.point, node.prev.prev.point, node.prev.prev.prev.point) == Winding.CW)
            {
                // Concave
                FillLeftConcaveEdgeEvent(tcx, edge, node.prev);
            }
            else
            {
                // Convex
                // Next above or below edge?
                if (TriUtil.Orient2d(edge.Q, node.prev.prev.point, edge.P) == Winding.CW)
                {
                    // Below
                    FillLeftConvexEdgeEvent(tcx, edge, node.prev);
                }
                else
                {
                    // Above
                }
            }
        }

        private void FillLeftConcaveEdgeEvent(SweepContext tcx, Edge edge, Node node)
        {
            Fill(tcx, node.prev);
            if (node.prev.point != edge.P)
            {
                // Next above or below edge?
                if (TriUtil.Orient2d(edge.Q, node.prev.point, edge.P) == Winding.CW)
                {
                    // Below
                    if (TriUtil.Orient2d(node.point, node.prev.point, node.prev.prev.point) == Winding.CW)
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

            if (o == Winding.CCW)
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
            if (o2d == Winding.CW)
            {
                // Right
                return ot.PointCCW(op);
            }
            else if (o2d == Winding.CCW)
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
