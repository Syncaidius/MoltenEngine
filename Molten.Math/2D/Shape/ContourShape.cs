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
        public void Triangulate(IList<Vector2F> output, Vector2F offset, float scale = 1.0f, int edgeResolution = 3)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="output"></param>
        /// <param name="edgeResolution">The maximum number of points that are allowed to represent an edge. For bezier curves, this will affect the curve smoothness.</param>
        public void Triangulate(IList<Triangle> output, int edgeResolution = 3)
        {
            if (edgeResolution < 3)
                throw new Exception("Edge resolution must be at least 3");
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
