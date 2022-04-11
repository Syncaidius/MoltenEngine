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
    }
}
