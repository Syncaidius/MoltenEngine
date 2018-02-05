using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten.Utilities
{
    /// <summary>An instanced version of mathametical helper methods, to avoid static calls. Useful in speed-critical blocks of code.</summary>
    public class FastMath
    {
        public Vector3 CalculateNormal(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            // Calculate normals
            Vector3 vec1 = p3 - p1;
            Vector3 vec2 = p2 - p1;
            Vector3 normal = FastCross(vec1, vec2);

            return normal;
        }

        /// <summary>A non-static version of Vector3.Cross.</summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        public Vector3 FastCross(Vector3 a, Vector3 b)
        {
            return new Vector3()
            {
                X = (a.Y * b.Z) - (a.Z * b.Y),
                Y = (a.Z * b.X) - (a.X * b.Z),
                Z = (a.X * b.Y) - (a.Y * b.X),
            };
        }
    }
}
