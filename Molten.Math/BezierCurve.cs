using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public static class BezierCurve
    {
        /// <summary>Calculates a point along a bezier curve.</summary>
        /// <param name="t">How far along the curve to calculate the point. 0f is the start. 1f is the end.</param>
        /// <param name="startPoint">Start point.</param>
        /// <param name="startCP">Start control point.</param>
        /// <param name="endCP">End control point.</param>
        /// <param name="endPoint">End point.</param>
        /// <returns></returns>
        public static Vector3 CalculateBezierPoint(float t,
          Vector3 startPoint, Vector3 endPoint, Vector3 startCP, Vector3 endCP)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            Vector3 p = uuu * startPoint; //first term
            p += 3 * uu * t * startCP; //second term
            p += 3 * u * tt * endCP; //third term
            p += ttt * endPoint; //fourth term

            return p;
        }

        /// <summary>Calculates a point along a bezier curve.</summary>
        /// <param name="t">How far along the curve to calculate the point. 0f is the start. 1f is the end.</param>
        /// <param name="startPoint">Start point.</param>
        /// <param name="startCP">Start control point.</param>
        /// <param name="endCP">End control point.</param>
        /// <param name="endPoint">End point.</param>
        /// <returns></returns>
        public static Vector2 CalculateBezierPoint(float t,
            Vector2 startPoint, Vector2 endPoint, Vector2 startCP, Vector2 endCP)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            Vector2 p = uuu * startPoint; //first term
            p += 3 * uu * t * startCP; //second term
            p += 3 * u * tt * endCP; //third term
            p += ttt * endPoint; //fourth term

            return p;
        }
    }
}
