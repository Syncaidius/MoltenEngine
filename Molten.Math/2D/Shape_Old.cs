namespace Molten
{
    public class Shape_Old
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
        public readonly List<Shape_Old> Holes = new List<Shape_Old>();

        /// <summary>
        /// Gets or sets the shape's color.
        /// </summary>
        public Color Color { get; set; } = Color.White;

        /// <summary>
        /// Creates a new instance of <see cref="Shape_Old"/>.
        /// </summary>
        public Shape_Old() { }

        /// <summary>
        /// Create a polygon from a list of at least 3 points with no duplicates.
        /// </summary>
        /// <param name="points">A list of unique points</param>
        public Shape_Old(IList<TriPoint> points)
        {
            Points.AddRange(points);
        }

        /// <summary>
        /// Create a polygon from a list of at least 3 points with no duplicates.
        /// </summary>
        /// <param name="points">A list of unique points.</param>
        public Shape_Old(IEnumerable<TriPoint> points) : this((points as IList<TriPoint>) ?? points.ToArray()) { }

        /// <summary>
        /// Create a polygon from a list of at least 3 points with no duplicates.
        /// </summary>
        /// <param name="points">A list of unique points.</param>
        public Shape_Old(params TriPoint[] points) : this((IList<TriPoint>)points) { }

        /// <summary>
        /// Creates a polygon from a list of at least 3 Vector3 points, with no duplicates.
        /// </summary>
        /// <param name="points">The input points.</param>
        /// <param name="offset">An offset to apply to all of the provided points.</param>
        /// <param name="scale">The scale of the provided points. 0.5f is half size. 2.0f is 2x the normal size.</param>
        public Shape_Old(IList<Vector2F> points, Vector2F offset, float scale)
        {
            for (int i = 0; i < points.Count; i++)
                Points.Add(new TriPoint(offset + (points[i] * scale)));
        }

        /// <summary>
        /// Creates a polygon from a list of at least 3 Vector3 points, with no duplicates.
        /// </summary>
        /// <param name="points">The input points.</param>
        public Shape_Old(IList<Vector2F> points) : this(points, Vector2F.Zero, 1.0f) { }

        /// <summary>
        /// Produces a <see cref="RectangleF"/> which contains all of the shape's points.
        /// </summary>
        public RectangleF CalculateBounds()
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
                    b.Left = p.X;
                else if (p.X > b.Right)
                    b.Right = p.Y;

                if (p.Y < b.Top)
                    b.Top = p.Y;
                else if (p.Y > b.Bottom)
                    b.Bottom = p.Y;
            }

            return b;
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
            foreach (Shape_Old h in Holes)
                tcx.AddHole(h.Points);

            tcx.InitTriangulation();
            Sweep sweep = new Sweep();
            sweep.Triangulate(tcx);

            List<Triangle> triangles = tcx.GetTriangles();
            foreach (Triangle tri in triangles)
            {
                //tri.ReversePointFlow();
                output.Add(((Vector2F)tri.Points[0] * scale) + offset);
                output.Add(((Vector2F)tri.Points[2] * scale) + offset);
                output.Add(((Vector2F)tri.Points[1] * scale) + offset);
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

            foreach (Shape_Old p in Holes)
                tcx.AddHole(p.Points);

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
                Points[i] *= scale;

            foreach (Shape_Old h in Holes)
                h.Scale(scale);
        }

        public void Scale(Vector2F scale)
        {
            for (int i = 0; i < Points.Count; i++)
                Points[i] *= scale;

            foreach (Shape_Old h in Holes)
                h.Scale(scale);
        }

        public void Offset(Vector2F offset)
        {
            for (int i = 0; i < Points.Count; i++)
                Points[i] += offset;

            foreach (Shape_Old h in Holes)
                h.Offset(offset);
        }

        public void ScaleAndOffset(Vector2F offset, float scale)
        {
            for (int i = 0; i < Points.Count; i++)
            {
                Points[i] *= scale;
                Points[i] += offset;
            }

            foreach (Shape_Old h in Holes)
                h.ScaleAndOffset(offset, scale);
        }

        public void ScaleAndOffset(Vector2F offset, Vector2F scale)
        {
            for (int i = 0; i < Points.Count; i++)
            {
                Points[i] *= scale;
                Points[i] += offset;
            }

            foreach (Shape_Old h in Holes)
                h.ScaleAndOffset(offset, scale);
        }

        public bool Contains(Shape_Old shape)
        {
            for(int i = 0; i < shape.Points.Count; i++)
            {
                // We only need 1 point to be outside to invalidate a containment.
                if (!Contains((Vector2F)shape.Points[i]))
                    return false;
            }

            return true;
        }

        public bool Contains(Vector2F point)
        {
            for (int i = 0; i < Holes.Count; i++)
            {
                if (Holes[i].Contains(point))
                    return false;
            }

            // Thanks to: https://codereview.stackexchange.com/a/108903
            int polygonLength = Points.Count;
            int j = 0;
            bool inside = false;
            float pointX = point.X, pointY = point.Y; // x, y for tested point.

            // start / end point for the current polygon segment.
            float startX, startY, endX, endY;
            Vector2F endPoint = (Vector2F)Points[polygonLength - 1];
            endX = endPoint.X;
            endY = endPoint.Y;

            while (j < polygonLength)
            {
                startX = endX; startY = endY;
                endPoint = (Vector2F)Points[j++];
                endX = endPoint.X; endY = endPoint.Y;
                //
                inside ^= (endY > pointY ^ startY > pointY) /* ? pointY inside [startY;endY] segment ? */
                          && /* if so, test if it is under the segment */
                          ((pointX - endX) < (pointY - endY) * (startX - endX) / (startY - endY));
            }

            return inside;
        }
    }
}
