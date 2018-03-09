// MIT - 2018 - James Yarwood - Modified for Molten Engine - https://github.com/Syncaidius/MoltenEngine

/* Poly2Tri
 * Copyright (c) 2009-2010, Poly2Tri Contributors
 * http://code.google.com/p/poly2tri/
 *
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without modification,
 * are permitted provided that the following conditions are met:
 *
 * * Redistributions of source code must retain the above copyright notice,
 *   this list of conditions and the following disclaimer.
 * * Redistributions in binary form must reproduce the above copyright notice,
 *   this list of conditions and the following disclaimer in the documentation
 *   and/or other materials provided with the distribution.
 * * Neither the name of Poly2Tri nor the names of its contributors may be
 *   used to endorse or promote products derived from this software without specific
 *   prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
 * A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
 * EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 * PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

/// Changes from the Java version
///   Polygon constructors sprused up, checks for 3+ polys
///   Naming of everything
///   getTriangulationMode() -> TriangulationMode { get; }
///   Exceptions replaced
/// Future possibilities
///   We have a lot of Add/Clear methods -- we may prefer to just expose the container
///   Some self-explanitory methods may deserve commenting anyways

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
        public readonly List<ShapePoint> Points = new List<ShapePoint>();

        /// <summary>
        /// Extra points inserted within the shape's area to control or increase triangulation.
        /// </summary>
        public readonly List<ShapePoint> SteinerPoints;

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

        List<ShapeTriangle> _triangles = new List<ShapeTriangle>();


        public Shape() { }

        /// <summary>
        /// Create a polygon from a list of at least 3 points with no duplicates.
        /// </summary>
        /// <param name="points">A list of unique points</param>
        public Shape(IList<ShapePoint> points)
        {
            Points.AddRange(points);
        }

        /// <summary>
        /// Create a polygon from a list of at least 3 points with no duplicates.
        /// </summary>
        /// <param name="points">A list of unique points.</param>
        public Shape(IEnumerable<ShapePoint> points) : this((points as IList<ShapePoint>) ?? points.ToArray()) { }

        /// <summary>
        /// Create a polygon from a list of at least 3 points with no duplicates.
        /// </summary>
        /// <param name="points">A list of unique points.</param>
        public Shape(params ShapePoint[] points) : this((IList<ShapePoint>)points) { }

        /// <summary>
        /// Creates a polygon from a list of at least 3 Vector3 points, with no duplicates.
        /// </summary>
        /// <param name="points">The input points.</param>
        /// <param name="offset">An offset to apply to all of the provided points.</param>
        /// <param name="scale">The scale of the provided points. 0.5f is half size. 2.0f is 2x the normal size.</param>
        public Shape(IList<Vector2> points, Vector2 offset, float scale)
        {
            for (int i = 0; i < points.Count; i++)
                Points.Add(new ShapePoint(offset + (points[i] * scale)));

            CalculateBounds();
        }

        /// <summary>
        /// Creates a polygon from a list of at least 3 Vector3 points, with no duplicates.
        /// </summary>
        /// <param name="points">The input points.</param>
        public Shape(IList<Vector2> points) : this(points, Vector2.Zero, 1.0f) { }

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

            foreach(ShapePoint p in Points)
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

            Bounds = b;
        }

        /// <summary>
        /// Triangulates the shape and adds all of the points (in triangle list layout) to the provided output.
        /// </summary>
        /// <param name="output">The output list.</param>
        public void Triangulate(IList<Vector2> output)
        {
            // Lets sanity check that first and last point haven't got the same position
            // Its something that often happens when importing polygon data from other formats
            if (Points[0].Equals(Points[Points.Count - 1]))
                Points.RemoveAt(Points.Count - 1);

            Triangulation.Triangulate(this);

            foreach (ShapeTriangle tri in _triangles)
            {
                tri.ReversePointFlow();
                output.Add(TriToVector2(tri.Points[0]));
                output.Add(TriToVector2(tri.Points[1]));
                output.Add(TriToVector2(tri.Points[2]));
            }
        }

        /// <summary>
        /// Triangulates the shape and adds all of the triangles to the provided output.
        /// </summary>
        /// <param name="output">The output list.</param>
        public void Triangulate(IList<ShapeTriangle> output)
        {
            Triangulation.Triangulate(this);
            for (int i = 0; i < _triangles.Count; i++)
                output.Add(_triangles[i]);
        }

        private Vector2 TriToVector2(ShapePoint p)
        {
            return new Vector2()
            {
                X = (float)p.X,
                Y = (float)p.Y,
            };
        }

        /// <summary>
        /// Inserts newPoint after point.
        /// </summary>
        /// <param name="point">The point to insert after in the polygon</param>
        /// <param name="newPoint">The point to insert into the polygon</param>
        public void InsertPointAfter(ShapePoint point, ShapePoint newPoint)
        {
            // Validate that 
            int index = Points.IndexOf(point);
            if (index == -1) throw new ArgumentException("Tried to insert a point into a Polygon after a point not belonging to the Polygon", "point");
            Points.Insert(index + 1, newPoint);
        }

        internal void AddTriangle(ShapeTriangle t)
        {
            _triangles.Add(t);
        }

        internal void AddTriangles(IEnumerable<ShapeTriangle> list)
        {
            _triangles.AddRange(list);
        }

        /// <summary>
        /// Creates constraints and populates the context with points
        /// </summary>
        /// <param name="tcx">The context</param>
        internal void Prepare(TriangulationContext tcx)
        {
            _triangles.Clear();

            if (Points.Count < 3)
                throw new InvalidOperationException("Shape has fewer than 3 points");

            // Outer constraints
            for (int i = 0; i < Points.Count - 1; i++)
                tcx.NewConstraint(Points[i], Points[i + 1]);

            tcx.NewConstraint(Points[0], Points[Points.Count - 1]);
            tcx.Points.AddRange(Points);

            // Hole constraints
            if (Holes != null)
            {
                foreach (Shape p in Holes)
                {
                    for (int i = 0; i < p.Points.Count - 1; i++)
                        tcx.NewConstraint(p.Points[i], p.Points[i + 1]);

                    tcx.NewConstraint(p.Points[0], p.Points[p.Points.Count - 1]);
                    tcx.Points.AddRange(p.Points);
                }
            }

            if (SteinerPoints != null)
                tcx.Points.AddRange(SteinerPoints);
        }
    }
}
