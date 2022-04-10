using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.MSDF
{
    public class EdgeHolder
    {
        /// Swaps the edges held by a and b.
        public static void Swap(EdgeHolder a, EdgeHolder b)
        {

        }

        public EdgeSegment Segment { get; set; }

        public EdgeHolder(EdgeSegment segment)
        {
            Segment = segment;
        }

        public EdgeHolder(Vector2D p0, Vector2D p1, EdgeColor edgeColor = EdgeColor.WHITE)
        {
            Segment = new LinearSegment(p0, p1, edgeColor);
        }

        public EdgeHolder(Vector2D p0, Vector2D p1, Vector2D p2, EdgeColor edgeColor = EdgeColor.WHITE)
        {
            Segment = new QuadraticSegment(p0, p1, p2, edgeColor);
        }

        public EdgeHolder(Vector2D p0, Vector2D p1, Vector2D p2, Vector2D p3, EdgeColor edgeColor = EdgeColor.WHITE)
        {
            Segment = new CubicSegment(p0, p1, p2, p3, edgeColor);
        }

        public EdgeHolder(EdgeHolder orig)
        {
            Segment = orig.Segment != null ? orig.Segment.Clone() : null;
        }
    }
}
