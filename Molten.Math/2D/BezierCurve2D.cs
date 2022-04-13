using System.Runtime.InteropServices;

namespace Molten
{
    /// <summary>
    /// A bezier curve structure with the byte/struct layout of: StartPoint | EndPoint | ControlPoint1 | ControlPoint2.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct BezierCurve2D
    {
        /// <summary>
        /// The start point of the curve
        /// </summary>
        public Vector2F Start;

        /// <summary>
        /// The end point of the curve
        /// </summary>
        public Vector2F End;

        /// <summary>
        /// The first control point
        /// </summary>
        public Vector2F ControlPoint1;

        /// <summary>
        /// The second control point.
        /// </summary>
        public Vector2F ControlPoint2;

        /// <summary>
        /// An empty <see cref="BezierCurve2D"/> curve. All values are zero.
        /// </summary>
        public readonly static BezierCurve2D Zero = new BezierCurve2D();

        /// <summary>Creates a new instance of a new bezier curve.</summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="cp"></param>
        public BezierCurve2D(Vector2F start, Vector2F end, Vector2F cp)
        {
            Start = start;
            End = end;
            ControlPoint1 = cp;
            ControlPoint2 = Vector2F.Zero;
        }

        /// <summary>Calculates a point along a 2D cubic bezier curve.</summary>
        /// <param name="t">How far along the curve to calculate the point. 0f is the start. 1f is the end.</param>
        /// <param name="start">Start point.</param>
        /// <param name="cp1">First control point.</param>
        /// <param name="cp2">Second control point.</param>
        /// <param name="end">End point.</param>
        /// <returns></returns>
        public static Vector2F CalculateCubic(float t,
            Vector2F start, Vector2F end, Vector2F cp1, Vector2F cp2)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            Vector2F p = uuu * start; //first term
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
        public static Vector2D CalculateCubic(double t,
            Vector2D start, Vector2D end, Vector2D cp1, Vector2D cp2)
        {
            double u = 1 - t;
            double tt = t * t;
            double uu = u * u;
            double uuu = uu * u;
            double ttt = tt * t;

            Vector2D p = uuu * start; //first term
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
        public static Vector2F CalculateCubic(float t,
            BezierCurve2D curve)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            Vector2F p = uuu * curve.Start; //first term
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
        public static Vector2F CalculateQuadratic(float t, Vector2F start, Vector2F end, Vector2F cp)
        {
            Vector2F cp1 = new Vector2F()
            {
                X = start.X + ((2f / 3f) * (cp.X - start.X)),
                Y = start.Y + ((2f / 3f) * (cp.Y - start.Y)),
            };

            Vector2F cp2 = new Vector2F()
            {
                X = end.X + ((2f / 3f) * (cp.X - end.X)),
                Y = end.Y + ((2f / 3f) * (cp.Y - end.Y)),
            };

            return CalculateCubic(t, start, end, cp1, cp2);
        }

        /// <summary>Calculates a point along a 2D quadratic bezier curve.</summary>
        /// <param name="t">How far along the curve to calculate the point. 0f is the start. 1f is the end.</param>
        /// <param name="start">Start point.</param>
        /// <param name="cp">The control point.</param>
        /// <param name="end">End point.</param>
        /// <returns></returns>
        public static Vector2D CalculateQuadratic(double t, Vector2D start, Vector2D end, Vector2D cp)
        {
            Vector2D cp1 = new Vector2D()
            {
                X = start.X + ((2.0 / 3.0) * (cp.X - start.X)),
                Y = start.Y + ((2.0 / 3.0) * (cp.Y - start.Y)),
            };

            Vector2D cp2 = new Vector2D()
            {
                X = end.X + ((2.0 / 3.0) * (cp.X - end.X)),
                Y = end.Y + ((2.0 / 3.0) * (cp.Y - end.Y)),
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
        public static Vector2F CalculateQuadratic(float t,
            BezierCurve2D curve)
        {
            Vector2F cp1 = new Vector2F()
            {
                X = curve.Start.X + ((2f / 3f) * (curve.ControlPoint1.X - curve.Start.X)),
                Y = curve.Start.Y + ((2f / 3f) * (curve.ControlPoint1.Y - curve.Start.Y)),
            };

            Vector2F cp2 = new Vector2F()
            {
                X = curve.End.X + ((2f / 3f) * (curve.ControlPoint1.X - curve.End.X)),
                Y = curve.End.Y + ((2f / 3f) * (curve.ControlPoint1.Y - curve.End.Y)),
            };

            return CalculateCubic(t, curve.Start, cp1, cp2, curve.End);
        }
    }
}
