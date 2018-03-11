// Copyright (c) 2010-2014 SharpDX - Alexandre Mutel
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Molten.DoublePrecision
{
    /// <summary>
    /// Double-precision version of <see cref="RectangleD"/>.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct RectangleD : IEquatable<RectangleD>
    {
        /// <summary>
        /// The left.
        /// </summary>
        public double Left;

        /// <summary>
        /// The top.
        /// </summary>
        public double Top;

        /// <summary>
        /// The right.
        /// </summary>
        public double Right;

        /// <summary>
        /// The bottom.
        /// </summary>
        public double Bottom;

        /// <summary>
        /// An empty rectangle.
        /// </summary>
        public static readonly RectangleD Empty;

        /// <summary>
        /// An infinite rectangle. See remarks.
        /// </summary>
        /// <remarks>
        /// http://msdn.microsoft.com/en-us/library/windows/desktop/dd372261%28v=vs.85%29.aspx
        /// Any properties that involve computations, like <see cref="Center"/>, <see cref="Width"/> or <see cref="Height"/>
        /// may return incorrect results - <see cref="float.NaN"/>.
        /// </remarks>
        public static readonly RectangleD Infinite;

        static RectangleD()
        {
            Empty = new RectangleD();
            Infinite = new RectangleD
                       {
                           Left = float.NegativeInfinity,
                           Top = float.NegativeInfinity,
                           Right = float.PositiveInfinity,
                           Bottom = float.PositiveInfinity
                       };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RectangleD"/> struct.
        /// </summary>
        /// <param name="x">The left.</param>
        /// <param name="y">The top.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        public RectangleD(double x, double y, double width, double height)
        {
            Left = x;
            Top = y;
            Right = x + width;
            Bottom = y + height;
        }

        /// <summary>
        /// Returns the area of the rectangle based on its width and height.
        /// </summary>
        /// <returns></returns>
        public double Area()
        {
            return Width * Height;
        }

        /// <summary>
        /// Expands the rectangle as needed so that the given point falls within it's bounds.
        /// </summary>
        /// <param name="p"></param>
        public void Encapsulate(Vector2D p)
        {
            if (p.X < Left)
                Left = Math.Floor(p.X);
            else if (p.X > Right)
                Right = Math.Ceiling(p.X);

            if (p.Y < Top)
                Top = Math.Floor(p.Y);
            else if (p.Y > Bottom)
                Bottom = Math.Ceiling(p.Y);
        }

        /// <summary>
        /// Expands the rectangle as needed so that the given point falls within it's bounds.
        /// </summary>
        /// <param name="p"></param>
        public void Encapsulate(RectangleD p)
        {
            if (p.Left < Left)
                Left = p.Left;
            else if (p.Right > Right)
                Right = p.Right;

            if (p.Top < Top)
                Top = p.Top;
            else if (p.Bottom > Bottom)
                Bottom = p.Bottom;
        }

        /// <summary>
        /// Gets or sets the X position.
        /// </summary>
        /// <value>The X position.</value>
        public double X
        {
            get
            {
                return Left;
            }
            set
            {
                Right = value + Width;
                Left = value;
            }
        }

        /// <summary>
        /// Gets or sets the Y position.
        /// </summary>
        /// <value>The Y position.</value>
        public double Y
        {
            get
            {
                return Top;
            }
            set
            {
                Bottom = value + Height;
                Top = value;
            }
        }

        /// <summary>
        /// Gets or sets the width.
        /// </summary>
        /// <value>The width.</value>
        public double Width
        {
            get { return Right - Left; }
            set { Right = Left + value; }
        }

        /// <summary>
        /// Gets or sets the height.
        /// </summary>
        /// <value>The height.</value>
        public double Height
        {
            get { return Bottom - Top; }
            set { Bottom = Top + value; }
        }

        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        /// <value>
        /// The location.
        /// </value>
        public Vector2D Location
        {
            get
            {
                return new Vector2D(X, Y);
            }
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }

        /// <summary>
        /// Gets the Point that specifies the center of the rectangle.
        /// </summary>
        /// <value>
        /// The center.
        /// </value>
        public Vector2D Center
        {
            get
            {
                return new Vector2D(X + (Width / 2), Y + (Height / 2));
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the rectangle is empty.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is empty]; otherwise, <c>false</c>.
        /// </value>
        public bool IsEmpty
        {
            get
            {
                return (Width == 0.0f) && (Height == 0.0f) && (X == 0.0f) && (Y == 0.0f);
            }
        }

        /// <summary>
        /// Gets or sets the size of the rectangle.
        /// </summary>
        /// <value>The size of the rectangle.</value>
        public Vector2D Size
        {
            get
            {
                return new Vector2D(Width, Height);
            }
            set
            {
                Width = value.X;
                Height = value.Y;
            }
        }

        /// <summary>
        /// Gets the position of the top-left corner of the rectangle.
        /// </summary>
        /// <value>The top-left corner of the rectangle.</value>
        public Vector2D TopLeft { get { return new Vector2D(Left, Top); } }

        /// <summary>
        /// Gets the position of the top-right corner of the rectangle.
        /// </summary>
        /// <value>The top-right corner of the rectangle.</value>
        public Vector2D TopRight { get { return new Vector2D(Right, Top); } }

        /// <summary>
        /// Gets the position of the bottom-left corner of the rectangle.
        /// </summary>
        /// <value>The bottom-left corner of the rectangle.</value>
        public Vector2D BottomLeft { get { return new Vector2D(Left, Bottom); } }

        /// <summary>
        /// Gets the position of the bottom-right corner of the rectangle.
        /// </summary>
        /// <value>The bottom-right corner of the rectangle.</value>
        public Vector2D BottomRight { get { return new Vector2D(Right, Bottom); } }

        /// <summary>Changes the position of the rectangle.</summary>
        /// <param name="amount">The values to adjust the position of the rectangle by.</param>
        public void Offset(Vector2I amount)
        {
            Offset(amount.X, amount.Y);
        }

        /// <summary>Changes the position of the rectangle.</summary>
        /// <param name="amount">The values to adjust the position of the rectangle by.</param>
        public void Offset(Vector2D amount)
        {
            Offset(amount.X, amount.Y);
        }

        /// <summary>Changes the position of the rectangle.</summary>
        /// <param name="offsetX">Change in the x-position.</param>
        /// <param name="offsetY">Change in the y-position.</param>
        public void Offset(double offsetX, double offsetY)
        {
            X += offsetX;
            Y += offsetY;
        }

        /// <summary>Pushes the edges of the rectangle out by the horizontal and vertical values specified.</summary>
        /// <param name="horizontalAmount">Value to push the sides out by.</param>
        /// <param name="verticalAmount">Value to push the top and bottom out by.</param>
        public void Inflate(double horizontalAmount, double verticalAmount)
        {
            X -= horizontalAmount;
            Y -= verticalAmount;
            Width += horizontalAmount * 2;
            Height += verticalAmount * 2;
        }

        /// <summary>Determines whether this rectangle contains a specified Point.</summary>
        /// <param name="value">The Point to evaluate.</param>
        /// <param name="result">[OutAttribute] true if the specified Point is contained within this rectangle; false otherwise.</param>
        public void Contains(ref Vector2D value, out bool result)
        {
            result = (X <= value.X) && (value.X < Right) && (Y <= value.Y) && (value.Y < Bottom);
        }

        /// <summary>Determines whether this rectangle entirely contains a specified rectangle.</summary>
        /// <param name="value">The rectangle to evaluate.</param>
        public bool Contains(Rectangle value)
        {
            return (X <= value.X) && (value.Right <= Right) && (Y <= value.Y) && (value.Bottom <= Bottom);
        }

        /// <summary>Determines whether this rectangle entirely contains a specified rectangle.</summary>
        /// <param name="value">The rectangle to evaluate.</param>
        public bool Contains(RectangleD value)
        {
            return (X <= value.X) && (value.Right <= Right) && (Y <= value.Y) && (value.Bottom <= Bottom);
        }

        /// <summary>Determines whether this rectangle entirely contains a specified rectangle.</summary>
        /// <param name="value">The rectangle to evaluate.</param>
        /// <param name="result">[OutAttribute] On exit, is true if this rectangle entirely contains the specified rectangle, or false if not.</param>
        public void Contains(ref RectangleD value, out bool result)
        {
            result = (X <= value.X) && (value.Right <= Right) && (Y <= value.Y) && (value.Bottom <= Bottom);
        }

        /// <summary>
        /// Checks, if specified point is inside <see cref="RectangleD"/>.
        /// </summary>
        /// <param name="x">X point coordinate.</param>
        /// <param name="y">Y point coordinate.</param>
        /// <returns><c>true</c> if point is inside <see cref="RectangleD"/>, otherwise <c>false</c>.</returns>
        public bool Contains(double x, double y)
        {
            return (x >= Left && x <= Right && y >= Top && y <= Bottom);
        }

        /// <summary>
        /// Checks, if specified <see cref="Vector2D"/> is inside <see cref="RectangleD"/>.
        /// </summary>
        /// <param name="vector2D">Coordinate <see cref="Vector2D"/>.</param>
        /// <returns><c>true</c> if <see cref="Vector2D"/> is inside <see cref="RectangleD"/>, otherwise <c>false</c>.</returns>
        public bool Contains(Vector2D vector2D)
        {
            return Contains(vector2D.X, vector2D.Y);
        }

        /// <summary>
        /// Checks, if specified <see cref="Vector2I"/> is inside <see cref="RectangleD"/>.
        /// </summary>
        /// <param name="point">Coordinate <see cref="Vector2I"/>.</param>
        /// <returns><c>true</c> if <see cref="Vector2I"/> is inside <see cref="RectangleD"/>, otherwise <c>false</c>.</returns>
        public bool Contains(Vector2I point)
        {
            return Contains(point.X, point.Y);
        }

        /// <summary>Determines whether a specified rectangle intersects with this rectangle.</summary>
        /// <param name="value">The rectangle to evaluate.</param>
        public bool Intersects(RectangleD value)
        {
            bool result;
            Intersects(ref value, out result);
            return result;
        }

        /// <summary>
        /// Determines whether a specified rectangle intersects with this rectangle.
        /// </summary>
        /// <param name="value">The rectangle to evaluate</param>
        /// <param name="result">[OutAttribute] true if the specified rectangle intersects with this one; false otherwise.</param>
        public void Intersects(ref RectangleD value, out bool result)
        {
            result = (value.X < Right) && (X < value.Right) && (value.Y < Bottom) && (Y < value.Bottom);
        }

        /// <summary>
        /// Creates a rectangle defining the area where one rectangle overlaps with another rectangle.
        /// </summary>
        /// <param name="value1">The first Rectangle to compare.</param>
        /// <param name="value2">The second Rectangle to compare.</param>
        /// <returns>The intersection rectangle.</returns>
        public static RectangleD Intersect(RectangleD value1, RectangleD value2)
        {
            RectangleD result;
            Intersect(ref value1, ref value2, out result);
            return result;
        }

        /// <summary>Creates a rectangle defining the area where one rectangle overlaps with another rectangle.</summary>
        /// <param name="value1">The first rectangle to compare.</param>
        /// <param name="value2">The second rectangle to compare.</param>
        /// <param name="result">[OutAttribute] The area where the two first parameters overlap.</param>
        public static void Intersect(ref RectangleD value1, ref RectangleD value2, out RectangleD result)
        {
            double newLeft = (value1.X > value2.X) ? value1.X : value2.X;
            double newTop = (value1.Y > value2.Y) ? value1.Y : value2.Y;
            double newRight = (value1.Right < value2.Right) ? value1.Right : value2.Right;
            double newBottom = (value1.Bottom < value2.Bottom) ? value1.Bottom : value2.Bottom;
            if ((newRight > newLeft) && (newBottom > newTop))
            {
                result = new RectangleD(newLeft, newTop, newRight - newLeft, newBottom - newTop);
            }
            else
            {
                result = Empty;
            }
        }

        /// <summary>
        /// Creates a new rectangle that exactly contains two other rectangles.
        /// </summary>
        /// <param name="value1">The first rectangle to contain.</param>
        /// <param name="value2">The second rectangle to contain.</param>
        /// <returns>The union rectangle.</returns>
        public static RectangleD Union(RectangleD value1, RectangleD value2)
        {
            RectangleD result;
            Union(ref value1, ref value2, out result);
            return result;
        }

        /// <summary>
        /// Creates a new rectangle that exactly contains two other rectangles.
        /// </summary>
        /// <param name="value1">The first rectangle to contain.</param>
        /// <param name="value2">The second rectangle to contain.</param>
        /// <param name="result">[OutAttribute] The rectangle that must be the union of the first two rectangles.</param>
        public static void Union(ref RectangleD value1, ref RectangleD value2, out RectangleD result)
        {
            var left = Math.Min(value1.Left, value2.Left);
            var right = Math.Max(value1.Right, value2.Right);
            var top = Math.Min(value1.Top, value2.Top);
            var bottom = Math.Max(value1.Bottom, value2.Bottom);
            result = new RectangleD(left, top, right - left, bottom - top);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if(!(obj is RectangleD))
                return false;

            var strongValue = (RectangleD)obj;
            return Equals(ref strongValue);
        }

        /// <summary>
        /// Determines whether the specified <see cref="RectangleD"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="RectangleD"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="RectangleD"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(ref RectangleD other)
        {
            return DoubleHelper.NearEqual(other.Left, Left) &&
                   DoubleHelper.NearEqual(other.Right, Right) &&
                   DoubleHelper.NearEqual(other.Top, Top) &&
                   DoubleHelper.NearEqual(other.Bottom, Bottom);
        }

        /// <summary>
        /// Determines whether the specified <see cref="RectangleD"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="RectangleD"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="RectangleD"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(RectangleD other)
        {
            return Equals(ref other);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = Left.GetHashCode();
                result = (result * 397) ^ Top.GetHashCode();
                result = (result * 397) ^ Right.GetHashCode();
                result = (result * 397) ^ Bottom.GetHashCode();
                return result;
            }
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "X:{0} Y:{1} Width:{2} Height:{3}", X, Y, Width, Height);
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(RectangleD left, RectangleD right)
        {
            return left.Equals(ref right);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(RectangleD left, RectangleD right)
        {
            return !left.Equals(ref right);
        }

        /// <summary>
        /// Performs an explicit conversion to <see cref="Rectangle"/> structure.
        /// </summary>
        /// <remarks>Performs direct double to int conversion, any fractional data is truncated.</remarks>
        /// <param name="value">The source <see cref="RectangleD"/> value.</param>
        /// <returns>A converted <see cref="Rectangle"/> structure.</returns>
        public static explicit operator Rectangle(RectangleD value)
        {
            return new Rectangle((int)value.X, (int)value.Y, (int)value.Width, (int)value.Height);
        }
    }
}
