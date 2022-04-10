﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.MSDF
{
    public struct MultiDistance
    {
        public double r, g, b;
    };

    public class MultiDistanceSelector : EdgeSelector<MultiDistance, PseudoDistanceSelectorBase.EdgeCache>
    {
        Vector2D p;
        PseudoDistanceSelectorBase r = new PseudoDistanceSelector();
        PseudoDistanceSelectorBase g = new PseudoDistanceSelector();
        PseudoDistanceSelectorBase b = new PseudoDistanceSelector();

        public override void reset(in Vector2D p)
        {
            double delta = DISTANCE_DELTA_FACTOR * (p - this.p).Length();
            r.reset(delta);
            g.reset(delta);
            b.reset(delta);
            this.p = p;
        }

        public override void addEdge(ref PseudoDistanceSelectorBase.EdgeCache cache, EdgeSegment prevEdge, EdgeSegment edge, EdgeSegment nextEdge)
        {
            if (
                ((edge.Color & EdgeColor.RED) == EdgeColor.RED && r.isEdgeRelevant(cache, edge, p)) ||
                ((edge.Color & EdgeColor.GREEN) == EdgeColor.GREEN && g.isEdgeRelevant(cache, edge, p)) ||
                ((edge.Color & EdgeColor.BLUE) == EdgeColor.BLUE && b.isEdgeRelevant(cache, edge, p))
            )
            {
                double param;
                SignedDistance distance = edge.signedDistance(p, out param);
                if ((edge.Color & EdgeColor.RED) == EdgeColor.RED)
                    r.addEdgeTrueDistance(edge, distance, param);
                if ((edge.Color & EdgeColor.GREEN) == EdgeColor.GREEN)
                    g.addEdgeTrueDistance(edge, distance, param);
                if ((edge.Color & EdgeColor.BLUE) == EdgeColor.BLUE)
                    b.addEdgeTrueDistance(edge, distance, param);
                cache.point = p;
                cache.absDistance = Math.Abs(distance.Distance);

                Vector2D ap = p - edge.point(0);
                Vector2D bp = p - edge.point(1);
                Vector2D aDir = edge.direction(0).GetNormalized(true);
                Vector2D bDir = edge.direction(1).GetNormalized(true);
                Vector2D prevDir = prevEdge.direction(1).GetNormalized(true);
                Vector2D nextDir = nextEdge.direction(0).GetNormalized(true);
                double add = Vector2D.Dot(ap, (prevDir + aDir).GetNormalized(true));
                double bdd = -Vector2D.Dot(bp, (bDir + nextDir).GetNormalized(true));
                if (add > 0)
                {
                    double pd = distance.Distance;
                    if (PseudoDistanceSelectorBase.getPseudoDistance(pd, ap, -aDir))
                    {
                        pd = -pd;
                        if ((edge.Color & EdgeColor.RED) == EdgeColor.RED)
                            r.addEdgePseudoDistance(pd);
                        if ((edge.Color & EdgeColor.GREEN) == EdgeColor.GREEN)
                            g.addEdgePseudoDistance(pd);
                        if ((edge.Color & EdgeColor.BLUE) == EdgeColor.BLUE)
                            b.addEdgePseudoDistance(pd);
                    }
                    cache.aPseudoDistance = pd;
                }
                if (bdd > 0)
                {
                    double pd = distance.Distance;
                    if (PseudoDistanceSelectorBase.getPseudoDistance(pd, bp, bDir))
                    {
                        if ((edge.Color & EdgeColor.RED) == EdgeColor.RED)
                            r.addEdgePseudoDistance(pd);
                        if ((edge.Color & EdgeColor.GREEN) == EdgeColor.GREEN)
                            g.addEdgePseudoDistance(pd);
                        if ((edge.Color & EdgeColor.BLUE) == EdgeColor.BLUE)
                            b.addEdgePseudoDistance(pd);
                    }
                    cache.bPseudoDistance = pd;
                }
                cache.aDomainDistance = add;
                cache.bDomainDistance = bdd;
            }
        }

        public override void merge(EdgeSelector<MultiDistance, PseudoDistanceSelectorBase.EdgeCache> other)
        {
            MultiDistanceSelector md = other as MultiDistanceSelector;
            r.merge(md.r);
            g.merge(md.g);
            b.merge(md.b);
        }

        public override MultiDistance distance()
        {
            MultiDistance multiDistance;
            multiDistance.r = r.computeDistance(p);
            multiDistance.g = g.computeDistance(p);
            multiDistance.b = b.computeDistance(p);
            return multiDistance;
        }

        public SignedDistance trueDistance()
        {
            SignedDistance distance = r.trueDistance();
            if (g.trueDistance() < distance)
                distance = g.trueDistance();
            if (b.trueDistance() < distance)
                distance = b.trueDistance();
            return distance;
        }

        public override void initDistance(ref MultiDistance distance)
        {
            distance.r = -double.MaxValue;
            distance.g = -double.MaxValue;
            distance.b = -double.MaxValue;
        }

        public override double resolveDistance(MultiDistance distance)
        {
            return MsdfMath.median(distance.r, distance.g, distance.b);
        }

        public override float getRefPSD(in MultiDistance dist, double invRange)
        {
            return (float)(invRange * dist.r + .5);
        }
    }
}