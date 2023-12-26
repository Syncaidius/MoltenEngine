using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using Molten.DoublePrecision;

namespace Molten
{
    ///<summary>Represents a four dimensional mathematical Rectangle.</summary>
    [StructLayout(LayoutKind.Explicit)]
    [DataContract]
	public partial struct Rectangle : IFormattable, IEquatable<Rectangle>
	{
        /// <summary>
        /// An empty rectangle.
        /// </summary>
        public static readonly Rectangle Empty = new Rectangle();

		/// <summary>The Left component.</summary>
		[DataMember]
		[FieldOffset(0)]
		public int Left;

		/// <summary>The Top component.</summary>
		[DataMember]
		[FieldOffset(4)]
		public int Top;

		/// <summary>The Right component.</summary>
		[DataMember]
		[FieldOffset(8)]
		public int Right;

		/// <summary>The Bottom component.</summary>
		[DataMember]
		[FieldOffset(12)]
		public int Bottom;

		/// <summary>A fixed array mapped to the same memory space as the individual <see cref="Rectangle"/> components.</summary>
		[IgnoreDataMember]
		[FieldOffset(0)]
		public unsafe fixed int Values[4];
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Rectangle"/> struct.
        /// </summary>
        /// <param name="position">The position of the rectangle.</param>
        /// <param name="size">The size of the rectangle.</param>
        public Rectangle(Vector2I position, Vector2I size)
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
        public Rectangle(int x, int y, int width, int height)
        {
            Left = x;
            Top = y;
            Right = x + width;
            Bottom = y + height;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Rectangle"/> struct.
        /// </summary>
        /// <param name="values">The values to populate the rectangle with. These should be ordered as X, Y, Width and Height.</param>
        public Rectangle(int[] values)
        {
            if(values == null)
                throw new ArgumentNullException("values");

            if(values.Length < 4)
                throw new Exception("Rectangle expects at least 4 values to populate X, Y, Width and Height.");

            Left = values[0];
            Top = values[1];
            Right = Left + values[2];
            Bottom = Top + values[3];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Rectangle"/> struct.
        /// </summary>
        /// <param name="values">The values to populate the rectangle with. These should be ordered as X, Y, Width and Height.</param>
        public Rectangle(Span<int> values)
        {
            if(values == null)
                throw new ArgumentNullException("values");

            if(values.Length < 4)
                throw new Exception("Rectangle expects at least 4 values to populate X, Y, Width and Height.");

            Left = values[0];
            Top = values[1];
            Right = Left + values[2];
            Bottom = Top + values[3];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Rectangle"/> struct.
        /// </summary>
        /// <param name="values">The <see cref="int"/> values to populate the rectangle with. These should be ordered as X, Y, Width and Height.
        /// <para>If the pointer does not contain at least 4 values of the expected type, undefined behaviour will occur.</para></param>
        public unsafe Rectangle(int* values)
        {
            Left = values[0];
            Top = values[1];
            Right = Left + values[2];
            Bottom = Top + values[3];
        }

        /// <summary>
        /// Gets or sets the X position.
        /// </summary>
        /// <value>The X position.</value>
        public int X
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
        public int Y
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
        public int Width
        {
            get => Right - Left;
            set => Right = Left + value;
        }

        /// <summary>
        /// Gets or sets the height.
        /// </summary>
        /// <value>The height.</value>
        public int Height
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
        /// Gets or sets the Point that specifies the center of the rectangle. ,para/>
        /// Setting this will move the rectangle without resizing, so that it is centered at the specified position.
        /// </summary>
        /// <value>
        /// The center.
        /// </value>
        public Vector2I Center
        {
            get => new Vector2I(Left + (Width / 2), Top + (Height / 2));
            set
            {
                Vector2I centerDif = value - Center;
                Left += centerDif.X;
                Right += centerDif.X;
                Top += centerDif.Y;
                Bottom += centerDif.Y;
            }
        }

        /// <summary>
        /// Gets or sets the size of the rectangle.
        /// </summary>
        public Vector2I Size
        {
            get => new Vector2I(Right - Left, Bottom - Top);
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
        public Vector2I TopLeft
        {
            get => new Vector2I(Left, Top);
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
        public Vector2I TopRight
        {
            get => new Vector2I(Right, Top);
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
        public Vector2I BottomLeft
        {
            get => new Vector2I(Left, Bottom);
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
        public Vector2I BottomRight
        {
            get => new Vector2I(Right, Bottom);
            set
            {
                Top = value.Y - Height;
                Left = value.X - Width;

                Bottom = value.Y;
                Right = value.X;
            }
        }

        /// <summary>Returns the area of the rectangle based on its width and height.</summary>
        /// <returns></returns>
        public float Area()
        {
            return Width * Height;
        }

        /// <summary>Creates and returns an array containing the left, top, right and bottom values of the current <see cref="Rectangle"/>.</summary>
        /// <returns>An array containing the left, top, right and bottom values of the current <see cref="Rectangle"/>.</returns>
        public int[] ToArray()
        {
            return [Left, Top, Right, Bottom];
        }

        /// <summary>
        /// Expands the rectangle as needed so that the given point falls within it's bounds.
        /// </summary>
        /// <param name="p"></param>
        public void Encapsulate(Vector2I p)
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
        /// Expands the rectangle as needed so that the given point falls within it's bounds.
        /// </summary>
        /// <param name="p"></param>
        public void Encapsulate(Rectangle p)
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
        public void Inflate(int leftAmount, int topAmount, int rightAmount, int bottomAmount)
        {
            X -= leftAmount;
            Y -= topAmount;
            Width += leftAmount + rightAmount;
            Height += topAmount + bottomAmount;
        }

        /// <summary>Pushes the edges of the rectangle out by the horizontal and vertical values specified. Negative values can be used to shrink the rectangle.</summary>
        /// <param name="horizontalAmount">Value to push the sides out by.</param>
        /// <param name="verticalAmount">Value to push the top and bottom out by.</param>
        public void Inflate(int horizontalAmount, int verticalAmount)
        {
            X -= horizontalAmount;
            Y -= verticalAmount;
            Width += horizontalAmount * 2;
            Height += verticalAmount * 2;
        }

        /// <summary>Pushes the edges of the rectangle out by the horizontal and vertical values specified. Negative values can be used to shrink the rectangle.</summary>
        /// <param name="amount">Value to push all sides out by.</param>
        public void Inflate(int amount)
        {
            X -= amount;
            Y -= amount;
            Width += amount * 2;
            Height += amount * 2;
        }

        /// <summary>Determines whether this rectangle contains a specified point represented by its x- and y-coordinates.</summary>
        /// <param name="x">The x-coordinate of the specified point.</param>
        /// <param name="y">The y-coordinate of the specified point.</param>
        public bool Contains(int x, int y)
        {
            return (X <= x) && (x < Right) && (Y <= y) && (y < Bottom);
        }

        /// <summary>Determines whether this rectangle entirely contains a specified rectangle.</summary>
        /// <param name="value">The rectangle to evaluate.</param>
        public bool Contains(Rectangle value)
        {
            Contains(ref value, out bool result);
            return result;
        }

        /// <summary>Determines whether this rectangle entirely contains a specified rectangle.</summary>
        /// <param name="value">The rectangle to evaluate.</param>
        /// <param name="result">[OutAttribute] On exit, is true if this rectangle entirely contains the specified rectangle, or false if not.</param>
        public void Contains(ref Rectangle value, out bool result)
        {
            result = (X <= value.X) && (value.Right <= Right) && (Y <= value.Y) && (value.Bottom <= Bottom);
        }

        /// <summary>
        /// Checks, if specified <see cref="Vector2I"/> is inside the current <see cref="Rectangle"/>
        /// </summary>
        /// <param name="vector">Coordinate <see cref="Vector2I"/>.</param>
        /// <returns><c>true</c> if <see cref="Vector2I"/> is inside <see cref="Rectangle"/>, otherwise <c>false</c>.</returns>
        public bool Contains(Vector2I vector)
        {
            return Contains(vector.X, vector.Y);
        }

        /// <summary>Determines whether a specified rectangle intersects with this rectangle.</summary>
        /// <param name="value">The rectangle to evaluate.</param>
        public bool Intersects(Rectangle value)
        {
            Intersects(ref value, out bool result);
            return result;
        }

        /// <summary>
        /// Determines whether a specified rectangle intersects with this rectangle.
        /// </summary>
        /// <param name="value">The rectangle to evaluate</param>
        /// <param name="result">[OutAttribute] true if the specified rectangle intersects with this one; false otherwise.</param>
        public void Intersects(ref Rectangle value, out bool result)
        {
            result = (value.X < Right) && (X < value.Right) && (value.Y < Bottom) && (Y < value.Bottom);
        }

        /// <summary>
        /// Creates a rectangle defining the area where one rectangle overlaps with another rectangle.
        /// </summary>
        /// <param name="value1">The first rectangle to compare.</param>
        /// <param name="value2">The second rectangle to compare.</param>
        /// <returns>The intersection rectangle.</returns>
        public static Rectangle Intersect(Rectangle value1, Rectangle value2)
        {
            Intersect(ref value1, ref value2, out Rectangle result);
            return result;
        }

        /// <summary>Creates a rectangle defining the area where one rectangle overlaps with another rectangle.</summary>
        /// <param name="value1">The first rectangle to compare.</param>
        /// <param name="value2">The second rectangle to compare.</param>
        /// <param name="result">[OutAttribute] The area where the two first parameters overlap.</param>
        public static void Intersect(ref Rectangle value1, ref Rectangle value2, out Rectangle result)
        {
            int newLeft = (value1.X > value2.X) ? value1.X : value2.X;
            int newTop = (value1.Y > value2.Y) ? value1.Y : value2.Y;
            int newRight = (value1.Right < value2.Right) ? value1.Right : value2.Right;
            int newBottom = (value1.Bottom < value2.Bottom) ? value1.Bottom : value2.Bottom;

            if ((newRight > newLeft) && (newBottom > newTop))
                result = new Rectangle(newLeft, newTop, newRight - newLeft, newBottom - newTop);
            else
                result = Empty;
        }

        /// <summary>
        /// Creates a new rectangle that exactly contains two other rectangles.
        /// </summary>
        /// <param name="value1">The first rectangle to contain.</param>
        /// <param name="value2">The second rectangle to contain.</param>
        /// <returns>The union rectangle.</returns>
        public static Rectangle Union(Rectangle value1, Rectangle value2)
        {
            Union(ref value1, ref value2, out Rectangle result);
            return result;
        }

        /// <summary>
        /// Creates a new rectangle that exactly contains two other rectangles.
        /// </summary>
        /// <param name="value1">The first rectangle to contain.</param>
        /// <param name="value2">The second rectangle to contain.</param>
        /// <param name="result">[OutAttribute] The rectangle that must be the union of the first two rectangles.</param>
        public static void Union(ref Rectangle value1, ref Rectangle value2, out Rectangle result)
        {
            int left = Math.Min(value1.Left, value2.Left);
            int right = Math.Max(value1.Right, value2.Right);
            int top = Math.Min(value1.Top, value2.Top);
            int bottom = Math.Max(value1.Bottom, value2.Bottom);
            result = new Rectangle(left, top, right - left, bottom - top);
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
            if(obj is Rectangle rect)
                return Equals(ref rect);

            return false;
        }

        /// <summary>
        /// Determines whether the specified <see cref="Rectangle"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Rectangle"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="Rectangle"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(ref Rectangle other)
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
        public bool Equals(Rectangle other)
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

        string IFormattable.ToString(string? format, IFormatProvider? formatProvider)
        {
            return string.Format(formatProvider, format, X, Y, Width, Height);
        }
        
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "X:{0} Y:{1} Width:{2} Height:{3}", X, Y, Width, Height);
        }

        /// <summary>
        /// Returns an interpolated <see cref="Rectangle"/> based on the start and end rectangles given.
        /// </summary>
        /// <param name="start">The end <see cref="Rectangle"/>.</param>
        /// <param name="end">The end <see cref="Rectangle"/>.</param>
        /// <param name="percent">The percentage of interpolation, between 0.0 and 1.0f.</param>
        /// <returns></returns>
        public static Rectangle Lerp(Rectangle start, Rectangle end, float percent)
        {
            return new Rectangle()
            {
                Left = MathHelper.Lerp(start.Left, end.Left, percent),
                Right = MathHelper.Lerp(start.Right, end.Right, percent),
                Top = MathHelper.Lerp(start.Top, end.Top, percent),
                Bottom = MathHelper.Lerp(start.Bottom, end.Bottom, percent),
            };
        }

		/// <summary> Gets or sets the component at the specified index. </summary>
		/// <value>The value of the <see cref="Rectangle"/> component, depending on the index.</value>
		/// <param name="index">The index of the index component to access, ranging from 0 to 3, inclusive.</param>
		/// <returns>The value of the component at the specified index value provided.</returns>
		/// <exception cref="IndexOutOfRangeException">Thrown if the index is out of range.</exception>
		public unsafe int this[int index]
		{
			get
			{
				if(index < 0 || index > 3)
					throw new IndexOutOfRangeException("index for Rectangle must be between 0 and 3, inclusive.");

				return Values[index];
			}
			set
			{
				if(index < 0 || index > 3)
					throw new IndexOutOfRangeException("index for Rectangle must be between 0 and 3, inclusive.");

				Values[index] = value;
			}
		}

		/// <summary> Gets or sets the component at the specified index. </summary>
		/// <value>The value of the <see cref="Rectangle"/> component, depending on the index.</value>
		/// <param name="index">The index of the index component to access, ranging from 0 to 3, inclusive.</param>
		/// <returns>The value of the component at the specified index value provided.</returns>
		/// <exception cref="IndexOutOfRangeException">Thrown if the index is out of range.</exception>
		public unsafe int this[uint index]
		{
			get
			{
				if(index > 3)
					throw new IndexOutOfRangeException("index for Rectangle must be between 0 and 3, inclusive.");

				return Values[index];
			}
			set
			{
				if(index > 3)
					throw new IndexOutOfRangeException("index for Rectangle must be between 0 and 3, inclusive.");

				Values[index] = value;
			}
		}


        #region Operators
        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Rectangle left, Rectangle right)
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
        public static bool operator !=(Rectangle left, Rectangle right)
        {
            return !left.Equals(ref right);
        }

        /// <summary>
        /// Adds a <see cref="Vector2I"/> to a <see cref="Rectangle"/>.
        /// </summary>
        /// <param name="value">The <see cref="Rectangle"/>.</param>
        /// <param name="vector">The <see cref="Vector2I"/>.</param>
        /// <returns></returns>
        public static Rectangle operator +(Rectangle value, Vector2I vector)
        {
            return new Rectangle(value.X + vector.X, value.Y + vector.Y, value.Width, value.Height);
        }
        #endregion

        #region Rectangle Cast Operators
		///<summary>Casts a <see cref="Rectangle"/> to a <see cref="RectangleUI"/>.</summary>
		public static explicit operator RectangleUI(Rectangle value)
		{
			return new RectangleUI((uint)value.Left, (uint)value.Top, (uint)value.Width, (uint)value.Height);
		}

		///<summary>Casts a <see cref="Rectangle"/> to a <see cref="RectangleL"/>.</summary>
		public static explicit operator RectangleL(Rectangle value)
		{
			return new RectangleL((long)value.Left, (long)value.Top, (long)value.Width, (long)value.Height);
		}

		///<summary>Casts a <see cref="Rectangle"/> to a <see cref="RectangleUL"/>.</summary>
		public static explicit operator RectangleUL(Rectangle value)
		{
			return new RectangleUL((ulong)value.Left, (ulong)value.Top, (ulong)value.Width, (ulong)value.Height);
		}

		///<summary>Casts a <see cref="Rectangle"/> to a <see cref="RectangleF"/>.</summary>
		public static explicit operator RectangleF(Rectangle value)
		{
			return new RectangleF((float)value.Left, (float)value.Top, (float)value.Width, (float)value.Height);
		}

		///<summary>Casts a <see cref="Rectangle"/> to a <see cref="RectangleD"/>.</summary>
		public static explicit operator RectangleD(Rectangle value)
		{
			return new RectangleD((double)value.Left, (double)value.Top, (double)value.Width, (double)value.Height);
		}

        #endregion
	}
}

