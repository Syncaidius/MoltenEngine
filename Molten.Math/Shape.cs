using System;
using System.Collections.Generic;
using System.Linq;

namespace Molten
{
    public class Shape
    {
        /// <summary>
        /// A list of shape outline points.
        /// </summary>
        public readonly List<TriPoint> Points = new List<TriPoint>();

        /// <summary>
        /// Extra points inserted within the shape's area to control or increase triangulation.
        /// </summary>
        public readonly List<TriPoint> SteinerPoints;

        /// <summary>
        /// A list of subtraction shapes fully contained inside this shape.<para/>
        /// Shapes added to this list will be used to create holes during triangulation. Any that are outside or intersect the shape outline are invalid.
        /// </summary>
        public readonly List<Shape> Holes = new List<Shape>();

        /// <summary>
        /// Gets or sets the shape's color.
        /// </summary>
        public Color Color { get; set; } = Color.White;

        public RectangleF Bounds { get; private set; }

        List<Triangle> _triangles = new List<Triangle>();


        public Shape() { }

        /// <summary>
        /// Create a polygon from a list of at least 3 points with no duplicates.
        /// </summary>
        /// <param name="points">A list of unique points</param>
        public Shape(IList<TriPoint> points)
        {
            Points.AddRange(points);
        }

        /// <summary>
        /// Create a polygon from a list of at least 3 points with no duplicates.
        /// </summary>
        /// <param name="points">A list of unique points.</param>
        public Shape(IEnumerable<TriPoint> points) : this((points as IList<TriPoint>) ?? points.ToArray()) { }

        /// <summary>
        /// Create a polygon from a list of at least 3 points with no duplicates.
        /// </summary>
        /// <param name="points">A list of unique points.</param>
        public Shape(params TriPoint[] points) : this((IList<TriPoint>)points) { }

        /// <summary>
        /// Creates a polygon from a list of at least 3 Vector3 points, with no duplicates.
        /// </summary>
        /// <param name="points">The input points.</param>
        /// <param name="offset">An offset to apply to all of the provided points.</param>
        /// <param name="scale">The scale of the provided points. 0.5f is half size. 2.0f is 2x the normal size.</param>
        public Shape(IList<Vector2F> points, Vector2F offset, float scale)
        {
            for (int i = 0; i < points.Count; i++)
                Points.Add(new TriPoint(offset + (points[i] * scale)));

            CalculateBounds();
        }

        /// <summary>
        /// Creates a polygon from a list of at least 3 Vector3 points, with no duplicates.
        /// </summary>
        /// <param name="points">The input points.</param>
        public Shape(IList<Vector2F> points) : this(points, Vector2F.Zero, 1.0f) { }

        /// <summary>
        /// Calculates and updates the shape's bounds. Useful after modifying <see cref="Points"/>.
        /// </summary>
        public void CalculateBounds()
        {
            RectangleF b = new RectangleF()
            {
                Left = float.MaxValue,
                Top = float.MaxValue,
                Right = float.MinValue,
                Bottom = float.MinValue,
            };

            foreach(TriPoint p in Points)
            {
                if (p.X < b.Left)
                    b.Left = (float)p.X;
                else if (p.X > b.Right)
                    b.Right = (float)p.Y;

                if (p.Y < b.Top)
                    b.Top = (float)p.Y;
                else if (p.Y > b.Bottom)
                    b.Bottom = (float)p.Y;
            }

            Bounds = b;
        }

        /// <summary>
        /// Triangulates the shape and adds all of the points (in triangle list layout) to the provided output.
        /// </summary>
        /// <param name="output">The output list.</param>
        public void Triangulate(IList<Vector2F> output, Vector2F offset, float scale = 1.0f)
        {
            Points.Reverse();
            SweepContext tcx = new SweepContext();
            tcx.AddPoints(Points);

            // Hole edges
           // foreach (Shape p in Holes)
           //     tcx.AddHole(p.Points);

            tcx.InitTriangulation();
            Sweep sweep = new Sweep();
            sweep.Triangulate(tcx);

            List<Triangle> triangles = tcx.GetTriangles();
            foreach (Triangle tri in triangles)
            {
                //tri.ReversePointFlow();
                output.Add(((Vector2F)tri.GetPoint(0) * scale) + offset);
                output.Add(((Vector2F)tri.GetPoint(2) * scale) + offset);
                output.Add(((Vector2F)tri.GetPoint(1) * scale) + offset);
            }
        }

        /// <summary>
        /// Triangulates the shape and adds all of the triangles to the provided output.
        /// </summary>
        /// <param name="output">The output list.</param>
        public void Triangulate(IList<Triangle> output)
        {
            SweepContext tcx = new SweepContext();
            tcx.AddPoints(Points);

            //// Hole edges
            //if (Holes != null)
            //{
            //    foreach (Shape p in Holes)
            //        tcx.AddHole(p.Points);
            //}

            tcx.InitTriangulation();
            Sweep sweep = new Sweep();
            sweep.Triangulate(tcx);

            List<Triangle> triangles = tcx.GetTriangles();
            foreach (Triangle tri in triangles)
                output.Add(tri);
        }

        public void Scale(float scale)
        {
            for (int i = 0; i < Points.Count; i++)
            {
                Points[i].X *= scale;
                Points[i].Y *= scale;
            }

            foreach (Shape h in Holes)
                h.Scale(scale);

            CalculateBounds();
        }

        public void Offset(Vector2F offset)
        {
            for (int i = 0; i < Points.Count; i++)
            {
                Points[i].X += offset.X;
                Points[i].Y += offset.Y;
            }

            foreach (Shape h in Holes)
                h.Offset(offset);

            CalculateBounds();
        }

        public void ScaleAndOffset(Vector2F offset, float scale)
        {
            for (int i = 0; i < Points.Count; i++)
            {
                Points[i].X *= scale;
                Points[i].Y *= scale;
                Points[i].X += offset.X;
                Points[i].Y += offset.Y;
            }

            foreach (Shape h in Holes)
                h.ScaleAndOffset(offset, scale);

            CalculateBounds();
        }

        public bool Contains(Shape shape)
        {
            // We only need 1 point to be outside to invalidate a containment.
            for(int i = 0; i < shape.Points.Count; i++)
            {
                if (!Contains((Vector2F)Points[i]))
                    return false;
            }

            return true;
        }

        public bool Contains(Vector2F point)
        {
            const float X_THRESHOLD = 0.5f; // Half a pixel
            List<Shape> holes = Holes;

            // Test holes first
            for (int i = 0; i < holes.Count; i++)
            {
                if (holes[i].Contains(point))
                    return false;
            }

            Vector2F p1;
            Vector2F p2;
            int hitCount = 0;

            Ray originRay = new Ray(new Vector3F(point, 0), Vector3F.Right);
            Vector3F rayHit;
            int end = Points.Count - 1;
            RectangleF inverseRect = new RectangleF()
            {
                Left = int.MaxValue,
                Right = int.MinValue,
                Top = int.MaxValue,
                Bottom = int.MinValue,
            };

            float prevX = float.NaN;
            float prevIntersectX = prevX;
            bool prevRunsLeft = true;

            for (int i = 0; i < end; i++)
            {
                p1 = (Vector2F)Points[i];
                p2 = (Vector2F)Points[i + 1];

                RectangleF contourBounds = inverseRect;
                contourBounds.Encapsulate(p1);
                contourBounds.Encapsulate(p2);

                Vector2F contourDir = -Vector2F.Normalize(p2 - p1);
                Ray contourRay = new Ray(new Vector3F(p1, 0), new Vector3F(contourDir, 0));


                if (CollisionHelper.RayIntersectsRay(ref originRay, ref contourRay, out rayHit))
                {
                    // check if intersection is on the left of the point.
                    if (rayHit.X < point.X)
                        continue;

                    Vector2F intersect = (Vector2F)rayHit;
                    if (contourBounds.Contains(intersect))
                    {
                        //_intersectionLines.Add(contourBounds.TopLeft);
                        //_intersectionLines.Add(contourBounds.TopRight);
                        //_intersectionLines.Add(contourBounds.TopRight);
                        //_intersectionLines.Add(contourBounds.BottomRight);
                        //_intersectionLines.Add(contourBounds.BottomRight);
                        //_intersectionLines.Add(contourBounds.BottomLeft);
                        //_intersectionLines.Add(contourBounds.BottomLeft);
                        //_intersectionLines.Add(contourBounds.TopLeft);

                        // Check if intersect point is the same as the previous one.
                        if (prevX != intersect.X)
                        {
                            float xDist = 0;
                            bool runsLeft = (contourDir.X < 0);

                            if (!float.IsNaN(prevX))
                            {
                                if (prevX == p1.X)
                                {
                                    xDist = (prevIntersectX - intersect.X) / X_THRESHOLD;
                                    if (prevRunsLeft != runsLeft)
                                        xDist = 0;
                                    else if (xDist > -1f && xDist < 1f)
                                        xDist = 1;
                                    else
                                        xDist = 0;
                                }
                            }

                            if (xDist == 0)
                            {
                                hitCount++;
                                prevIntersectX = intersect.X;
                                prevX = p2.X;
                                prevRunsLeft = runsLeft;
                            }
                        }
                    }
                }
            }

            Console.WriteLine($"Hit count: {hitCount}");
            return hitCount % 2 != 0;
        }


        /// <summary>
        /// Creates constraints and populates the context with points
        /// </summary>
        /// <param name="tcx">The context</param>
        //internal void Prepare(TriangulationContext tcx)
        //{
        //    _triangles.Clear();

        //    if (Points.Count < 3)
        //        throw new InvalidOperationException("Shape has fewer than 3 points");

        //    // Outer constraints
        //    for (int i = 0; i < Points.Count - 1; i++)
        //        tcx.NewConstraint(Points[i], Points[i + 1]);

        //    tcx.NewConstraint(Points[0], Points[Points.Count - 1]);
        //    tcx.Points.AddRange(Points);

        //    // Hole constraints
        //    if (Holes != null)
        //    {
        //        foreach (Shape p in Holes)
        //        {
        //            for (int i = 0; i < p.Points.Count - 1; i++)
        //                tcx.NewConstraint(p.Points[i], p.Points[i + 1]);

        //            tcx.NewConstraint(p.Points[0], p.Points[p.Points.Count - 1]);
        //            tcx.Points.AddRange(p.Points);
        //        }
        //    }

        //    if (SteinerPoints != null)
        //        tcx.Points.AddRange(SteinerPoints);
        //}
    }
}
