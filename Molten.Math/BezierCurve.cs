using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    /// <summary>
    /// A bezier curve structure with the byte/struct layout of: StartPoint | EndPoint | ControlPoint1 | ControlPoint2.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct BezierCurve
    {
        /// <summary>
        /// The start point of the curve
        /// </summary>
        public Vector2 Start;

        /// <summary>
        /// The end point of the curve
        /// </summary>
        public Vector2 End;

        /// <summary>
        /// The first control point
        /// </summary>
        public Vector2 ControlPoint1;

        /// <summary>
        /// The second control point.
        /// </summary>
        public Vector2 ControlPoint2;

        /// <summary>Calculates a point along a 3D cubic bezier curve.</summary>
        /// <param name="t">How far along the curve to calculate the point. 0f is the start. 1f is the end.</param>
        /// <param name="startPoint">Start point.</param>
        /// <param name="cp1">Start control point.</param>
        /// <param name="cp2">End control point.</param>
        /// <param name="endPoint">End point.</param>
        /// <returns></returns>
        public static Vector3 CalculateCubic(float t,
          Vector3 startPoint, Vector3 endPoint, Vector3 cp1, Vector3 cp2)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            Vector3 p = uuu * startPoint; //first term
            p += 3 * uu * t * cp1; //second term
            p += 3 * u * tt * cp2; //third term
            p += ttt * endPoint; //fourth term

            return p;
        }

        /// <summary>Calculates a point along a 3D quadratic bezier curve.</summary>
        /// <param name="t">How far along the curve to calculate the point. 0f is the start. 1f is the end.</param>
        /// <param name="start">Start point.</param>
        /// <param name="cp">The control point.</param>
        /// <param name="end">End point.</param>
        /// <returns></returns>
        public static Vector3 CalculateQuadratic(float t, Vector3 start, Vector3 end, Vector3 cp)
        {
            Vector3 cp1 = new Vector3()
            {
                X = start.X + ((2f / 3f) * (cp.X - start.X)),
                Y = start.Y + ((2f / 3f) * (cp.Y - start.Y)),
            };

            Vector3 cp2 = new Vector3()
            {
                X = end.X + ((2f / 3f) * (cp.X - end.X)),
                Y = end.Y + ((2f / 3f) * (cp.Y - end.Y)),
            };

            return CalculateCubic(t, start, end, cp1, cp2);
        }

        /// <summary>Calculates a point along a 2D cubic bezier curve.</summary>
        /// <param name="t">How far along the curve to calculate the point. 0f is the start. 1f is the end.</param>
        /// <param name="start">Start point.</param>
        /// <param name="cp1">First control point.</param>
        /// <param name="cp2">Second control point.</param>
        /// <param name="end">End point.</param>
        /// <returns></returns>
        public static Vector2 CalculateCubic(float t,
            Vector2 start, Vector2 end, Vector2 cp1, Vector2 cp2)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            Vector2 p = uuu * start; //first term
            p += 3 * uu * t * cp1; //second term
            p += 3 * u * tt * cp2; //third term
            p += ttt * end; //fourth term

            return p;
        }

        /// <summary>Calculates a point along a 2D cubic bezier curve.</summary>
        /// <param name="t">How far along the curve to calculate the point. 0f is the start. 1f is the end.</param>
        /// <param name="start">Start point.</param>
        /// <param name="cp1">First control point.</param>
        /// <param name="cp2">Second control point.</param>
        /// <param name="end">End point.</param>
        /// <returns></returns>
        public static Vector2 CalculateCubic(float t,
            BezierCurve curve)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            Vector2 p = uuu * curve.Start; //first term
            p += 3 * uu * t * curve.ControlPoint1; //second term
            p += 3 * u * tt * curve.ControlPoint2; //third term
            p += ttt * curve.End; //fourth term

            return p;
        }

        /// <summary>Calculates a point along a 2D quadratic bezier curve.</summary>
        /// <param name="t">How far along the curve to calculate the point. 0f is the start. 1f is the end.</param>
        /// <param name="start">Start point.</param>
        /// <param name="cp">The control point.</param>
        /// <param name="end">End point.</param>
        /// <returns></returns>
        public static Vector2 CalculateQuadratic(float t, Vector2 start, Vector2 end, Vector2 cp)
        {
            Vector2 cp1 = new Vector2()
            {
                X = start.X + ((2f / 3f) * (cp.X - start.X)),
                Y = start.Y + ((2f / 3f) * (cp.Y - start.Y)),
            };

            Vector2 cp2 = new Vector2()
            {
                X = end.X + ((2f / 3f) * (cp.X - end.X)),
                Y = end.Y + ((2f / 3f) * (cp.Y - end.Y)),
            };

            return CalculateCubic(t, start, end, cp1, cp2);
        }

        /// <summary>Calculates a point along a bezier curve.</summary>
        /// <param name="t">How far along the curve to calculate the point. 0f is the start. 1f is the end.</param>
        /// <param name="startPoint">Start point.</param>
        /// <param name="startCP">Start control point.</param>
        /// <param name="endCP">End control point.</param>
        /// <param name="endPoint">End point.</param>
        /// <returns></returns>
        public static Vector2 CalculateQuadratic(float t,
            BezierCurve curve)
        {
            Vector2 cp1 = new Vector2()
            {
                X = curve.Start.X + ((2f / 3f) * (curve.ControlPoint1.X - curve.Start.X)),
                Y = curve.Start.Y + ((2f / 3f) * (curve.ControlPoint1.Y - curve.Start.Y)),
            };

            Vector2 cp2 = new Vector2()
            {
                X = curve.End.X + ((2f / 3f) * (curve.ControlPoint1.X - curve.End.X)),
                Y = curve.End.Y + ((2f / 3f) * (curve.ControlPoint1.Y - curve.End.Y)),
            };

            return CalculateCubic(t, curve.Start, cp1, cp2, curve.End);
        }
    }
}
