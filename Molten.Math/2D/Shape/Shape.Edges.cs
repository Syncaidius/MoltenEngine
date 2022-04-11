using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public partial class ContourShape
    {
        public abstract class Edge
        {
            public const int INDEX_P0 = 0;
            public const int INDEX_P1 = 1;
            public const int INDEX_CP1 = 2;
            public const int INDEX_CP2 = 3;

            public Vector2D[] Points { get; protected init; }

            public abstract Vector2D Point(double param);
        }

        public class LinearEdge : Edge
        {
            public LinearEdge(Vector2D p0, Vector2D p1)
            {
                Points = new Vector2D[] { p0, p1 };
            }

            public override Vector2D Point(double param)
            {
                return Vector2D.Lerp(ref Points[INDEX_P0], ref Points[INDEX_P1], param);
            }
        }

        public class QuadraticEdge : Edge
        {
            public QuadraticEdge(Vector2D p0, Vector2D p1, Vector2D pControl)
            {
                Points = new Vector2D[] { p0, p1, pControl };
            }

            public override Vector2D Point(double param)
            {
                return Vector2D.Lerp(ref Points[INDEX_P0], ref Points[INDEX_P1], param);
            }
        }

        public class CubicEdge : Edge
        {
            public CubicEdge(Vector2D p0, Vector2D p1, Vector2D pControl1, Vector2D pControl2)
            {
                Points = new Vector2D[] { p0, p1, pControl1, pControl2 };
            }

            public override Vector2D Point(double param)
            {
                return Vector2D.Lerp(ref Points[INDEX_P0], ref Points[INDEX_P1], param);
            }
        }
    }
}
