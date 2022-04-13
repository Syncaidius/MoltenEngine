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
        public void Triangulate(IList<Vector2F> output, Vector2F offset, float scale = 1f, int edgeResolution = 3)
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
        public void Triangulate(IList<Triangle> output, Vector2F offset, float scale = 1f, int edgeResolution = 3)
        {
            SweepContext tcx = new SweepContext();
            foreach (Contour contour in Contours)
            {
                List<TriPoint> points = GetContourPoints(contour, edgeResolution);

                int winding = contour.GetWinding();
                switch (winding)
                {
                    case 0: // Unknown
                        continue;

                    case -1: // Outline
                        points.Reverse();
                        tcx.AddPoints(points);
                        break;

                    case 1: // Hole
                        tcx.AddHole(points);
                        break;
                }
            }

            tcx.InitTriangulation();
            Sweep sweep = new Sweep();
            sweep.Triangulate(tcx);

            output = tcx.GetTriangles();

            // Scale and offset triangles
            foreach (Triangle tri in output)
            {
                for (int i = 0; i < 3; i++) {
                    tri.Points[i].X = (tri.Points[i].X * scale) + offset.X;
                    tri.Points[i].Y = (tri.Points[i].Y * scale) + offset.Y;
                }
            }
        }

        private List<TriPoint> GetContourPoints(Contour contour, int edgeResolution = 3)
        {
            if (edgeResolution < 3)
                throw new Exception("Edge resolution must be at least 3");

            List<TriPoint> points = new List<TriPoint>();
            foreach(Edge edge in contour.Edges)
            {
                if(edge is LinearEdge lEdge)
                {
                    if (points.Count > 0)
                        points.Add(new TriPoint((Vector2F)edge.Points[Edge.INDEX_P1]));
                }
                else
                {
                    for(int i = 0; i < edgeResolution; i++)
                    {
                        float dist = i > 0 ? (1.0f / i) : 0;
                    }
                }
            }

            return points;
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
    }
}
