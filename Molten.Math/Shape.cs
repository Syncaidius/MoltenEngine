using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Poly2Tri;

namespace Molten
{
    /// <summary>
    /// Represents a shape with a defined outline. 
    /// The last point will automatically be connected to the first point to form a continous shape outline.
    /// </summary>
    public class Shape
    {
        public List<Vector2> Points { get; private set; }

        public Shape(IList<Vector2> outlinePoints)
        {
            Points = new List<Vector2>(outlinePoints);
        }

        public void Triangulate(IList<Vector2> output, Vector2 offset, float scale = 1.0f)
        {
            List<PolygonPoint> pPoints = new List<PolygonPoint>();
            for (int i = 0; i < Points.Count; i++)
            {
                pPoints.Add(new PolygonPoint(offset.X + (Points[i].X * scale), offset.Y + (Points[i].Y * scale)));
            }

            Polygon p = new Polygon(pPoints);
            P2T.Triangulate(p);

            foreach(DelaunayTriangle tri in p.Triangles)
            {
                tri.ReversePointFlow();
                output.Add(TriToVector2(tri.Points[0]));
                output.Add(TriToVector2(tri.Points[1]));
                output.Add(TriToVector2(tri.Points[2]));
            }
        }

        private Vector2 TriToVector2(TriangulationPoint p)
        {
            return new Vector2()
            {
                X = (float)p.X,
                Y = (float)p.Y,
            };
        }

        //private static List<TriangulationPoint> asPointSet(IList<TriangulationPoint> points)
        //{
        //    List<TriangulationPoint> contour = new List<TriangulationPoint>();

        //    for (var n = 0; n < points.Count; n++)
        //    {
        //        var x = points[n].X;
        //        var y = points[n].Y;

        //        TriangulationPoint np = new PolygonPoint(x, y);

        //        if (P2TUtil.indexOfPointInList(np, contour) == -1)
        //        {
        //            if ((n == 0 || n == points.Count - 1) || !P2TUtil.isCollinear(points[n - 1], points[n], points[n + 1]))
        //                contour.Add(np);
        //        }
        //    }
        //    return contour;
        //}

        //private static bool insideHole(poly, point)
        //{
        //    for (var i = 0; i < poly.holes.length; i++)
        //    {
        //        var hole = poly.holes[i];
        //        if (util.pointInPoly(hole, point))
        //            return true;
        //    }
        //    return false;
        //}

        //private static void addSteinerPoints(Polygon poly, IList<TriangulationPoint> points, TriangulationContext sweep)
        //{
        //    var bounds = P2TUtil.getBounds(poly.Points);

        //    //ensure points are unique and not collinear 
        //    points = asPointSet(points);

        //    for (var i = 0; i < points.Count; i++)
        //    {
        //        var p = points[i];

        //        //fugly collinear fix ... gotta revisit this
        //        p.X += 0.5;
        //        p.Y += 0.5;

        //        if (p.X <= bounds.X || p.Y <= bounds.Y || p.X >= bounds.Right || p.Y >= bounds.Bottom)
        //            continue;

        //        if (P2TUtil.pointInPoly(poly.contour, p) && !insideHole(poly, p))
        //        {
        //            //We are in the polygon! Now make sure we're not in a hole..
        //            sweep.addPoint(new poly2tri.Point(p.x, p.y));
        //        }
        //    }
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="shapes"></param>
        ///// <param name="steinerPoints"></param>
        //public static void Triangulate(IList<Polygon> shapes, IList<Vector2> steinerPoints)
        //{
        //    bool windingClockwise = false;
        //    DTSweepContext sweep = new DTSweepContext();

        //    Polygon poly = new Polygon();
        //    var allTris = [];

        //    steinerPoints = (steinerPoints != null && steinerPoints.Count != 0) ? steinerPoints : null;

        //    for (var j=0; j<shapes.Count; j++) {
        //        IList<TriangulationPoint> points = shapes[j].Points;
        //        List<TriangulationPoint> set = asPointSet(points);

        //        //OpenBaskerville-0.0.75.ttf does some strange things
        //        //with the moveTo command, causing the decomposition
        //        //to give us an extra shape with only 1 point. This
        //        //simply skips a path if it can't make up a triangle..
        //        if (set.Count < 3)
        //            continue;

        //        //check the winding order
        //        if (j==0) {
        //            windingClockwise = P2TUtil.isClockwise(set);
        //        }

        //        //if the sweep has already been created, maybe we're on a hole?
        //        if (sweep != null) {
        //            var clock = P2TUtil.isClockwise(set);

        //            //we have a hole...
        //            if (windingClockwise != clock) {
        //                poly.Holes.Add(shapes[j]);
        //            } else {
        //                //no hole, so it must be a new shape.
        //                //add our last shape
        //                if (steinerPoints != null) {
        //                    addSteinerPoints(poly, steinerPoints, sweep);
        //                }

        //                sweep.triangulate();
        //                allTris = allTris.concat(sweep.getTriangles());

        //                //reset the sweep for next shape
        //                sweep = new poly2tri.SweepContext(set);
        //                poly = {holes:[], contour:points};
        //            }
        //        } else {
        //            sweep = new poly2tri.SweepContext(set);
        //            poly = new Polygon();
        //        }
        //    }

        //    //if the sweep is still setup, then triangulate it
        //    if (sweep !== null) {
        //        if (steinerPoints!==null) {
        //            addSteinerPoints(poly, steinerPoints, sweep);
        //        }

        //        sweep.triangulate();
        //        allTris = allTris.concat(sweep.getTriangles());
        //    }
        //    return allTris;
        //}
    }
}
