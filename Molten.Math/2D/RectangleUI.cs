using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Molten
{
    /// <summary>
    /// Defines an unsigned 32-bit integer (<see cref="uint"/>) rectangle. This structure is slightly different from System.Drawing.
    /// Rectangle as it is internally storing Left,Top,Right,Bottom instead of Left, Top, Width, Height.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    [Serializable]
    public struct RectangleUI : IEquatable<RectangleUI>
    {
        /// <summary>
        /// The left.
        /// </summary>
        [DataMember]
        public uint Left;

        /// <summary>
        /// The top.
        /// </summary>
        [DataMember]
        public uint Top;

        /// <summary>
        /// The right.
        /// </summary>
        [DataMember]
        public uint Right;

        /// <summary>
        /// The bottom.
        /// </summary>
        [DataMember]
        public uint Bottom;

        /// <summary>
        /// An empty rectangle.
        /// </summary>
        public static readonly RectangleUI Empty;

        static RectangleUI()
        {
            Empty = new RectangleUI();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Rectangle"/> struct.
        /// </summary>
        /// <param name="position">The position of the rectangle.</param>
        /// <param name="size">The size of the rectangle.</param>
        public RectangleUI(Vector2UI position, Vector2UI size)
        {
            Left = position.X;
            Top = position.Y;
            Right = position.X + size.X;
            Bottom = position.Y + size.Y;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Rectangle"/> struct.
        /// </summary>
        /// <param name="x">The left.</param>
        /// <param name="y">The top.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        public RectangleUI(uint x, uint y, uint width, uint height)
        {
            Left = x;
            Top = y;
            Right = x + width;
            Bottom = y + height;
        }

        /// <summary>
        /// Gets or sets the X position.
        /// </summary>
        /// <value>The X position.</value>
        public uint X
        {
            get => Left;
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
        public uint Y
        {
            get => Top;
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
        public uint Width
        {
            get => Right - Left;
            set => Right = Left + value;
        }

        /// <summary>
        /// Gets or sets the height.
        /// </summary>
        /// <value>The height.</value>
        public uint Height
        {
            get => Bottom - Top;
            set => Bottom = Top + value;
        }


        /// <summary>
        /// Gets a value that indicates whether the rectangle is empty.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is empty]; otherwise, <c>false</c>.
        /// </value>
        public bool IsEmpty
        {
            get => (Width == 0) && (Height == 0) && (X == 0) && (Y == 0);
        }

        /// <summary>
        /// Gets or sets the Pouint that specifies the center of the rectangle. ,para/>
        /// Setting this will move the rectangle without resizing, so that it is centered at the specified position.
        /// </summary>
        /// <value>
        /// The center.
        /// </value>
        public Vector2UI Center
        {
            get => new Vector2UI(Left + (Width / 2), Top + (Height / 2));
            set
            {
                Vector2UI centerDif = value - Center;
                Left += centerDif.X;
                Right += centerDif.X;
                Top += centerDif.Y;
                Bottom += centerDif.Y;
            }
        }

        /// <summary>
        /// Gets or sets the size of the rectangle.
        /// </summary>
        public Vector2UI Size
        {
            get => new Vector2UI(Right - Left, Bottom - Top);
            set
            {
                Right = Left + value.X;
                Bottom = Top + value.Y;
            }
        }

        /// <summary>
        /// Gets or sets the position of the top-left corner of the rectangle. Setting this will move the rectangle without resizing.
        /// </summary>
        /// <value>The top-left corner of the rectangle.</value>
        public Vector2UI TopLeft
        {
            get => new Vector2UI(Left, Top);
            set
            {
                Bottom = value.Y + Height;
                Right = value.X + Width;

                Top = value.Y;
                Left = value.X;
            }
        }

        /// <summary>
        /// Gets or sets the position of the top-right corner of the rectangle. Setting this will move the rectangle without resizing.
        /// </summary>
        /// <value>The top-right corner of the rectangle.</value>
        public Vector2UI TopRight
        {
            get => new Vector2UI(Right, Top);
            set
            {
                Left = value.X - Width;
                Bottom = value.Y - Height;

                Top = value.Y;
                Right = value.X;
            }
        }

        /// <summary>
        /// Gets or sets the position of the bottom-left corner of the rectangle. Setting this will move the rectangle without resizing.
        /// </summary>
        /// <value>The bottom-left corner of the rectangle.</value>
        public Vector2UI BottomLeft
        {
            get => new Vector2UI(Left, Bottom);
            set
            {
                Top = value.Y - Height;
                Right = value.X + Width;

                Bottom = value.Y;
                Left = value.X;
            }
        }

        /// <summary>
        /// Gets or sets the position of the bottom-right corner of the rectangle. Setting this will move the rectangle without resizing.
        /// </summary>
        /// <value>The bottom-right corner of the rectangle.</value>
        public Vector2UI BottomRight
        {
            get => new Vector2UI(Right, Bottom);
            set
            {
                Top = value.Y - Height;
                Left = value.X - Width;

                Bottom = value.Y;
                Right = value.X;
            }
        }

        /// <summary>
        /// Returns the area of the rectangle based on its width and height.
        /// </summary>
        /// <returns></returns>
        public float Area()
        {
            return Width * Height;
        }

        /// <summary>
        /// Expands the rectangle as needed so that the given pouint falls within it's bounds.
        /// </summary>
        /// <param name="p"></param>
        public void Encapsulate(Vector2F p)
        {
            if (p.X < Left)
                Left = (uint)Math.Floor(p.X);
            else if (p.X > Right)
                Right = (uint)Math.Ceiling(p.X);

            if (p.Y < Top)
                Top = (uint)Math.Floor(p.Y);
            else if (p.Y > Bottom)
                Bottom = (uint)Math.Ceiling(p.Y);
        }

        /// <summary>
        /// Expands the rectangle as needed so that the given pouint falls within it's bounds.
        /// </summary>
        /// <param name="p"></param>
        public void Encapsulate(Vector2UI p)
        {
            if (p.X < Left)
                Left = p.X;
            else if (p.X > Right)
                Right = p.X;

            if (p.Y < Top)
                Top = p.Y;
            else if (p.Y > Bottom)
                Bottom = p.Y;
        }

        /// <summary>
        /// Expands the rectangle as needed so that the given pouint falls within it's bounds.
        /// </summary>
        /// <param name="p"></param>
        public void Encapsulate(RectangleUI p)
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

        /// <summary>Pushes the edges of the rectangle out by the values specified. Negative values can be used to shrink the rectangle.</summary>
        /// <param name="leftAmount">Value to push the left side out by.</param>
        /// <param name="topAmount">Value to push the top side out by.</param>
        /// <param name="rightAmount">Value to push the right side out by.</param>
        /// <param name="bottomAmount">Value to push the bottom side out by.</param>
        public void Inflate(uint leftAmount, uint topAmount, uint rightAmount, uint bottomAmount)
        {
            X -= leftAmount;
            Y -= topAmount;
            Width += leftAmount + rightAmount;
            Height += topAmount + bottomAmount;
        }

        /// <summary>Pushes the edges of the rectangle out by the horizontal and vertical values specified. Negative values can be used to shrink the rectangle.</summary>
        /// <param name="horizontalAmount">Value to push the sides out by.</param>
        /// <param name="verticalAmount">Value to push the top and bottom out by.</param>
        public void Inflate(uint horizontalAmount, uint verticalAmount)
        {
            X -= horizontalAmount;
            Y -= verticalAmount;
            Width += horizontalAmount * 2;
            Height += verticalAmount * 2;
        }

        /// <summary>Pushes the edges of the rectangle out by the horizontal and vertical values specified. Negative values can be used to shrink the rectangle.</summary>
        /// <param name="amount">Value to push all sides out by.</param>
        public void Inflate(uint amount)
        {
            X -= amount;
            Y -= amount;
            Width += amount * 2;
            Height += amount * 2;
        }

        /// <summary>Determines whether this rectangle contains a specified pouint represented by its x- and y-coordinates.</summary>
        /// <param name="x">The x-coordinate of the specified point.</param>
        /// <param name="y">The y-coordinate of the specified point.</param>
        public bool Contains(uint x, uint y)
        {
            return (X <= x) && (x < Right) && (Y <= y) && (y < Bottom);
        }

        /// <summary>Determines whether this rectangle entirely contains a specified rectangle.</summary>
        /// <param name="value">The rectangle to evaluate.</param>
        public bool Contains(RectangleUI value)
        {
            bool result;
            Contains(ref value, out result);
            return result;
        }

        /// <summary>Determines whether this rectangle entirely contains a specified rectangle.</summary>
        /// <param name="value">The rectangle to evaluate.</param>
        /// <param name="result">[OutAttribute] On exit, is true if this rectangle entirely contains the specified rectangle, or false if not.</param>
        public void Contains(ref RectangleUI value, out bool result)
        {
            result = (X <= value.X) && (value.Right <= Right) && (Y <= value.Y) && (value.Bottom <= Bottom);
        }

        /// <summary>
        /// Checks, if specified pouint is inside <see cref="Rectangle"/>.
        /// </summary>
        /// <param name="x">X pouint coordinate.</param>
        /// <param name="y">Y pouint coordinate.</param>
        /// <returns><c>true</c> if pouint is inside <see cref="Rectangle"/>, otherwise <c>false</c>.</returns>
        public bool Contains(float x, float y)
        {
            return (x >= Left && x <= Right && y >= Top && y <= Bottom);
        }

        /// <summary>
        /// Checks, if specified <see cref="Vector2F"/> is inside <see cref="Rectangle"/>.
        /// </summary>
        /// <param name="vector2D">Coordinate <see cref="Vector2F"/>.</param>
        /// <returns><c>true</c> if <see cref="Vector2F"/> is inside <see cref="Rectangle"/>, otherwise <c>false</c>.</returns>
        public bool Contains(Vector2F vector2D)
        {
            return Contains(vector2D.X, vector2D.Y);
        }

        /// <summary>
        /// Checks, if specified <see cref="Vector2UI"/> is inside <see cref="Rectangle"/>.
        /// </summary>
        /// <param name="vector2D">Coordinate <see cref="Vector2UI"/>.</param>
        /// <returns><c>true</c> if <see cref="Vector2UI"/> is inside <see cref="Rectangle"/>, otherwise <c>false</c>.</returns>
        public bool Contains(Vector2UI vector2D)
        {
            return Contains(vector2D.X, vector2D.Y);
        }

        /// <summary>Determines whether a specified rectangle intersects with this rectangle.</summary>
        /// <param name="value">The rectangle to evaluate.</param>
        public bool Intersects(RectangleUI value)
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
        public void Intersects(ref RectangleUI value, out bool result)
        {
            result = (value.X < Right) && (X < value.Right) && (value.Y < Bottom) && (Y < value.Bottom);
        }

        /// <summary>
        /// Creates a rectangle defining the area where one rectangle overlaps with another rectangle.
        /// </summary>
        /// <param name="value1">The first rectangle to compare.</param>
        /// <param name="value2">The second rectangle to compare.</param>
        /// <returns>The intersection rectangle.</returns>
        public static RectangleUI Intersect(RectangleUI value1, RectangleUI value2)
        {
            RectangleUI result;
            Intersect(ref value1, ref value2, out result);
            return result;
        }

        /// <summary>Creates a rectangle defining the area where one rectangle overlaps with another rectangle.</summary>
        /// <param name="value1">The first rectangle to compare.</param>
        /// <param name="value2">The second rectangle to compare.</param>
        /// <param name="result">[OutAttribute] The area where the two first parameters overlap.</param>
        public static void Intersect(ref RectangleUI value1, ref RectangleUI value2, out RectangleUI result)
        {
            uint newLeft = (value1.X > value2.X) ? value1.X : value2.X;
            uint newTop = (value1.Y > value2.Y) ? value1.Y : value2.Y;
            uint newRight = (value1.Right < value2.Right) ? value1.Right : value2.Right;
            uint newBottom = (value1.Bottom < value2.Bottom) ? value1.Bottom : value2.Bottom;
            if ((newRight > newLeft) && (newBottom > newTop))
            {
                result = new RectangleUI(newLeft, newTop, newRight - newLeft, newBottom - newTop);
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
        public static RectangleUI Union(RectangleUI value1, RectangleUI value2)
        {
            RectangleUI result;
            Union(ref value1, ref value2, out result);
            return result;
        }

        /// <summary>
        /// Creates a new rectangle that exactly contains two other rectangles.
        /// </summary>
        /// <param name="value1">The first rectangle to contain.</param>
        /// <param name="value2">The second rectangle to contain.</param>
        /// <param name="result">[OutAttribute] The rectangle that must be the union of the first two rectangles.</param>
        public static void Union(ref RectangleUI value1, ref RectangleUI value2, out RectangleUI result)
        {
            var left = Math.Min(value1.Left, value2.Left);
            var right = Math.Max(value1.Right, value2.Right);
            var top = Math.Min(value1.Top, value2.Top);
            var bottom = Math.Max(value1.Bottom, value2.Bottom);
            result = new RectangleUI(left, top, right - left, bottom - top);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if(!(obj is RectangleUI))
                return false;

            var strongValue = (RectangleUI)obj;
            return Equals(ref strongValue);
        }

        /// <summary>
        /// Determines whether the specified <see cref="Rectangle"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Rectangle"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="Rectangle"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(ref RectangleUI other)
        {
            return other.Left == Left && other.Top == Top && other.Right == Right && other.Bottom == Bottom;
        }

        /// <summary>
        /// Determines whether the specified <see cref="Rectangle"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Rectangle"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="Rectangle"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(RectangleUI other)
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
                uint result = Left;
                result = (result * 397) ^ Top;
                result = (result * 397) ^ Right;
                result = (result * 397) ^ Bottom;
                return (int)result;
            }
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "X:{0} Y:{1} Width:{2} Height:{3}", X, Y, Width, Height);
        }

        /// <summary>
        /// Returns an interpolated <see cref="RectangleUI"/> based on the start and end rectangles given.
        /// </summary>
        /// <param name="start">The end <see cref="RectangleUI"/>.</param>
        /// <param name="end">The end <see cref="RectangleUI"/>.</param>
        /// <param name="percent">The percentage of interpolation, between 0.0 and 1.0f.</param>
        /// <returns></returns>
        public RectangleUI Lerp(RectangleUI start, RectangleUI end, float percent)
        {
            return new RectangleUI()
            {
                Left = MathHelper.Lerp(start.Left, end.Left, percent),
                Right = MathHelper.Lerp(start.Right, end.Right, percent),
                Top = MathHelper.Lerp(start.Top, end.Top, percent),
                Bottom = MathHelper.Lerp(start.Bottom, end.Bottom, percent),
            };
        }

        #region Operators
        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(RectangleUI left, RectangleUI right)
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
        public static bool operator !=(RectangleUI left, RectangleUI right)
        {
            return !left.Equals(ref right);
        }

        /// <summary>
        /// Performs an implicit conversion to a <see cref="RectangleF"/>.
        /// </summary>
        /// <remarks>Performs direct converstion from uint to float.</remarks>
        /// <param name="value">The source <see cref="Rectangle"/> value.</param>
        /// <returns>The converted structure.</returns>
        public static implicit operator RectangleF(RectangleUI value)
        {
            return new RectangleF(value.X, value.Y, value.Width, value.Height);
        }

        /// <summary>
        /// Adds a <see cref="Vector2UI"/> to a <see cref="RectangleUI"/>.
        /// </summary>
        /// <param name="value">The <see cref="RectangleUI"/>.</param>
        /// <param name="vector">The <see cref="Vector2UI"/>.</param>
        /// <returns></returns>
        public static RectangleUI operator +(RectangleUI value, Vector2UI vector)
        {
            return new RectangleUI(value.X + vector.X, value.Y + vector.Y, value.Width, value.Height);
        }
        #endregion
    }
}
