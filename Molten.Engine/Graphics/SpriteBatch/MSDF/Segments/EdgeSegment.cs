using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.SpriteBatch.MSDF
{
    public abstract class EdgeSegment
    {
        public EdgeColor Color;

        public EdgeSegment(EdgeColor color)
        {
            Color = color;
        }       

        protected static void pointBounds(Vector2D p, ref double l, ref double b, ref double r, ref double t)
        {
            if (p.X < l) l = p.X;
            if (p.Y < b) b = p.Y;
            if (p.X > r) r = p.X;
            if (p.Y > t) t = p.Y;
        }

        /// Creates a copy of the edge segment.
        public abstract EdgeSegment Clone();

        /// Returns the point on the edge specified by the parameter (between 0 and 1).
        public abstract Vector2D point(double param);

        /// Returns the direction the edge has at the point specified by the parameter.
        public abstract Vector2D direction(double param);

        /// Returns the change of direction (second derivative) at the point specified by the parameter.
        public abstract Vector2D directionChange(double param);

        /// Returns the minimum signed distance between origin and the edge.
        public abstract SignedDistance signedDistance(Vector2D origin, out double param);

        /// Converts a previously retrieved signed distance from origin to pseudo-distance.
        public void distanceToPseudoDistance(ref SignedDistance distance, Vector2D origin, double param)
        {
            if (param < 0)
            {
                Vector2D dir = Vector2D.Normalize(direction(0));
                Vector2D aq = origin - point(0);
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
                Vector2D dir = Vector2D.Normalize(direction(1));
                Vector2D bq = origin - point(1);
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

        /// Outputs a list of (at most three) intersections (their X coordinates) with an infinite horizontal scanline at y and returns how many there are.
        public abstract int scanlineIntersections(X3 x, DY3 dy, double y);

        /// Adjusts the bounding box to fit the edge segment.
        public abstract void bound(ref double l, ref double b, ref double r, ref double t);

        /// Reverses the edge (swaps its start point and end point).
        public abstract void reverse();
        /// Moves the start point of the edge segment.
        public abstract void moveStartPoint(Vector2D to);
        /// Moves the end point of the edge segment.
        public abstract void moveEndPoint(Vector2D to);

        /// Splits the edge segments into thirds which together represent the original edge.
        public abstract void splitInThirds(ref EdgeSegment part1, ref EdgeSegment part2, ref EdgeSegment part3);
    }
}
