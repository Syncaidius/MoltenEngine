using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public partial class ContourShape
    {
        public List<Contour> Contours { get; } = new List<Contour>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="output"></param>
        /// <param name="offset"></param>
        /// <param name="scale"></param>
        /// <param name="edgeResolution">The maximum number of points that are allowed to represent an edge. For bezier curves, this will affect the curve smoothness.</param>
        public void Triangulate(List<Vector2F> output, Vector2F offset, float scale = 1f, int edgeResolution = 3)
        {
            List<Triangle> triangles = new List<Triangle>();
            Triangulate(triangles, offset, scale, edgeResolution);

            foreach (Triangle tri in triangles)
            {
                output.Add((Vector2F)tri.Points[0]);
                output.Add((Vector2F)tri.Points[2]);
                output.Add((Vector2F)tri.Points[1]);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="output"></param>
        /// <param name="edgeResolution">The maximum number of points that are allowed to represent an edge. For bezier curves, this will affect the curve smoothness.</param>
        public void Triangulate(List<Triangle> output, Vector2F offset, float scale = 1f, int edgeResolution = 3)
        {
            SweepContext tcx = new SweepContext();
            foreach (Contour contour in Contours)
            {
                List<TriPoint> points = contour.GetEdgePoints(edgeResolution);

                int winding = contour.GetWinding();
                switch (winding)
                {
                    case 0: // Unknown
                        continue;

                    case -1: // Outline
                        tcx.AddPoints(points);
                        break;

                    case 1: // Hole
                        points.Reverse();
                        tcx.AddHole(points);
                        break;
                }
            }

            tcx.InitTriangulation();
            Sweep sweep = new Sweep();
            sweep.Triangulate(tcx);

            List<Triangle> result = tcx.GetTriangles();

            // Scale and offset triangles
            foreach (Triangle tri in result)
            {
                for (int i = 0; i < 3; i++)
                {
                    tri.Points[i].X = (tri.Points[i].X * scale) + offset.X;
                    tri.Points[i].Y = (tri.Points[i].Y * scale) + offset.Y;
                    output.Add(tri);
                }
            }
        }

        /// <summary>
        /// Performs basic checks to determine if the object represents a valid shape.
        /// </summary>
        /// <returns></returns>
        public bool Validate()
        {
            foreach (Contour contour in Contours)
            {
                if (contour.Edges.Count > 0)
                {
                    Vector2D corner = contour.Edges.Last().Point(1);
                    foreach (Edge edge in contour.Edges)
                    {
                        if (edge == null)
                            return false;
                        if (edge.Point(0) != corner)
                            return false;

                        corner = edge.Point(1);
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Returns the total number of edges in the current <see cref="ContourShape"/>.
        /// </summary>
        /// <returns></returns>
        public int GetEdgeCount()
        {
            int total = 0;
            foreach (Contour contour in Contours)
                total += contour.Edges.Count;

            return total;
        }

        public void Scale(float scale)
        {
            Scale(new Vector2F(scale));
        }

        public void Scale(Vector2F scale)
        {
            Vector2D dScale = (Vector2D)scale;

            foreach (Contour contour in Contours)
            {
                foreach (Edge e in contour.Edges)
                {
                    for (int i = 0; i < e.p.Length; i++)
                        e.p[i] *= dScale;
                }
            }
        }

        public void Offset(Vector2F offset)
        {
            Vector2D dOffset = (Vector2D)offset;

            foreach (Contour contour in Contours)
            {
                foreach (Edge e in contour.Edges)
                {
                    for (int i = 0; i < e.p.Length; i++)
                        e.p[i] += dOffset;
                }
            }
        }

        public void ScaleAndOffset(Vector2F offset, float scale)
        {
            ScaleAndOffset(offset, new Vector2F(scale));  
        }

        public void ScaleAndOffset(Vector2F offset, Vector2F scale)
        {
            Vector2D dOffset = (Vector2D)offset;
            Vector2D dScale = (Vector2D)scale;

            foreach (Contour contour in Contours)
            {
                foreach (Edge e in contour.Edges)
                {
                    for (int i = 0; i < e.p.Length; i++)
                    {
                        e.p[i] *= dScale;
                        e.p[i] += dOffset;
                    }
                }
            }
        }

        public bool Contains(Shape shape)
        {
            for (int i = 0; i < shape.Points.Count; i++)
            {
                // We only need 1 point to be outside to invalidate a containment.
                if (!Contains((Vector2F)shape.Points[i]))
                    return false;
            }

            return true;
        }

        public bool Contains(Vector2F point, int edgeResolution = 3)
        {
            Vector2D dPoint = (Vector2D)point;

            // Check hole contours first.
            foreach(Contour c in Contours)
            {
                if(c.GetWinding() == -1)
                {
                    if (c.Contains(dPoint, edgeResolution))
                        return true;
                }
            }

            // Now check main contour(s)
            foreach(Contour c in Contours)
            {
                if (c.GetWinding() > -1)
                {
                    if (c.Contains(dPoint, edgeResolution))
                        return false;
                }
            }

            return false;
        }
    }
}
