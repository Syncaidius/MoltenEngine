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
    public struct BezierCurve3D
    {
        /// <summary>
        /// The start point of the curve
        /// </summary>
        public Vector3F Start;

        /// <summary>
        /// The end point of the curve
        /// </summary>
        public Vector3F End;

        /// <summary>
        /// The first control point
        /// </summary>
        public Vector3F ControlPoint1;

        /// <summary>
        /// The second control point.
        /// </summary>
        public Vector3F ControlPoint2;

        /// <summary>
        /// An empty <see cref="BezierCurve3D"/> curve. All values are zero.
        /// </summary>
        public readonly static BezierCurve3D Zero = new BezierCurve3D();

        /// <summary>Creates a new instance of a new bezier curve.</summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="cp"></param>
        public BezierCurve3D(Vector3F start, Vector3F end, Vector3F cp)
        {
            Start = start;
            End = end;
            ControlPoint1 = cp;
            ControlPoint2 = Vector3F.Zero;
        }

        /// <summary>Calculates a point along a 3D cubic bezier curve.</summary>
        /// <param name="t">How far along the curve to calculate the point. 0f is the start. 1f is the end.</param>
        /// <param name="startPoint">Start point.</param>
        /// <param name="cp1">Start control point.</param>
        /// <param name="cp2">End control point.</param>
        /// <param name="endPoint">End point.</param>
        /// <returns></returns>
        public static Vector3F CalculateCubic(float t,
          Vector3F startPoint, Vector3F endPoint, Vector3F cp1, Vector3F cp2)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            Vector3F p = uuu * startPoint; //first term
            p += 3 * uu * t * cp1; //second term
            p += 3 * u * tt * cp2; //third term
            p += ttt * endPoint; //fourth term

            return p;
        }

        /// <summary>Calculates a point along a 3D cubic bezier curve.</summary>
        /// <param name="t">How far along the curve to calculate the point. 0f is the start. 1f is the end.</param>
        /// <param name="start">Start point.</param>
        /// <param name="cp1">First control point.</param>
        /// <param name="cp2">Second control point.</param>
        /// <param name="end">End point.</param>
        /// <returns></returns>
        public static Vector3F CalculateCubic(float t,
            BezierCurve3D curve)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            Vector3F p = uuu * curve.Start; //first term
            p += 3 * uu * t * curve.ControlPoint1; //second term
            p += 3 * u * tt * curve.ControlPoint2; //third term
            p += ttt * curve.End; //fourth term

            return p;
        }

        /// <summary>Calculates a point along a 3D quadratic bezier curve.</summary>
        /// <param name="t">How far along the curve to calculate the point. 0f is the start. 1f is the end.</param>
        /// <param name="start">Start point.</param>
        /// <param name="cp">The control point.</param>
        /// <param name="end">End point.</param>
        /// <returns></returns>
        public static Vector3F CalculateQuadratic(float t, Vector3F start, Vector3F end, Vector3F cp)
        {
            Vector3F cp1 = new Vector3F()
            {
                X = start.X + ((2f / 3f) * (cp.X - start.X)),
                Y = start.Y + ((2f / 3f) * (cp.Y - start.Y)),
            };

            Vector3F cp2 = new Vector3F()
            {
                X = end.X + ((2f / 3f) * (cp.X - end.X)),
                Y = end.Y + ((2f / 3f) * (cp.Y - end.Y)),
            };

            return CalculateCubic(t, start, end, cp1, cp2);
        }

        /// <summary>Calculates a point along a 3D quadratic bezier curve.</summary>
        /// <param name="t">How far along the curve to calculate the point. 0f is the start. 1f is the end.</param>
        /// <param name="startPoint">Start point.</param>
        /// <param name="startCP">Start control point.</param>
        /// <param name="endCP">End control point.</param>
        /// <param name="endPoint">End point.</param>
        /// <returns></returns>
        public static Vector3F CalculateQuadratic(float t,
            BezierCurve3D curve)
        {
            Vector3F cp1 = new Vector3F()
            {
                X = curve.Start.X + ((2f / 3f) * (curve.ControlPoint1.X - curve.Start.X)),
                Y = curve.Start.Y + ((2f / 3f) * (curve.ControlPoint1.Y - curve.Start.Y)),
            };

            Vector3F cp2 = new Vector3F()
            {
                X = curve.End.X + ((2f / 3f) * (curve.ControlPoint1.X - curve.End.X)),
                Y = curve.End.Y + ((2f / 3f) * (curve.ControlPoint1.Y - curve.End.Y)),
            };

            return CalculateCubic(t, curve.Start, cp1, cp2, curve.End);
        }
    }
}
