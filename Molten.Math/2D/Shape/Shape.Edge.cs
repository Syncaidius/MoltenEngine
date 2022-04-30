using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public partial class Shape
    {
        public abstract class Edge
        {
            public Vector2D[] p { get; protected init; }

            public EdgeColor Color;

            public Edge(EdgeColor color)
            {
                Color = color;
            }

            public void DistanceToPseudoDistance(ref SignedDistance distance, Vector2D origin, double param)
            {
                if (param < 0)
                {
                    Vector2D dir = Vector2D.Normalize(GetDirection(0));
                    Vector2D aq = origin - Point(0);
                    double ts = Vector2D.Dot(aq, dir);
                    if (ts < 0)
                    {
                        double pseudoDistance = Vector2D.Cross(aq, dir);
                        if (Math.Abs(pseudoDistance) <= Math.Abs(distance.Distance))
                        {
                            distance.Distance = pseudoDistance;
                            distance.Dot = 0;
                        }
                    }
                }
                else if (param > 1)
                {
                    Vector2D dir = Vector2D.Normalize(GetDirection(1));
                    Vector2D bq = origin - Point(1);
                    double ts = Vector2D.Dot(bq, dir);
                    if (ts > 0)
                    {
                        double pseudoDistance = Vector2D.Cross(bq, dir);
                        if (Math.Abs(pseudoDistance) <= Math.Abs(distance.Distance))
                        {
                            distance.Distance = pseudoDistance;
                            distance.Dot = 0;
                        }
                    }
                }
            }

            public override string ToString()
            {
                return $"{this.GetType().Name} - P0: {p[0]} -- P1: {p[1]}";
            }

            public abstract Vector2D Point(double param);

            public abstract Vector2D PointAlongEdge(double percentage);

            public abstract Vector2D GetDirection(double param);

            public abstract void SplitInThirds(ref Edge part1, ref Edge part2, ref Edge part3);

            public unsafe abstract int ScanlineIntersections(double* x, int* dy, double y);

            public abstract SignedDistance SignedDistance(Vector2D origin, out double param);
        }
    }
}
