using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Molten
{
    /// <summary>
    /// Structure using the same layout than <see cref="System.Drawing.Point"/> except that it uses unsigned integers instead.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct UIntVector2 : IEquatable<UIntVector2>
    {
        /// <summary>
        /// A point with (0,0) coordinates.
        /// </summary>
        public static readonly UIntVector2 Zero = new UIntVector2(0, 0);

        /// <summary>
        /// Initializes a new instance of the <see cref="UIntVector2"/> struct.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        public UIntVector2(uint x, uint y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Left coordinate.
        /// </summary>
        public uint X;

        /// <summary>
        /// Top coordinate.
        /// </summary>
        public uint Y;

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(ref UIntVector2 other)
        {
            return other.X == X && other.Y == Y;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(UIntVector2 other)
        {
            return Equals(ref other);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if(!(obj is UIntVector2))
                return false;

            var strongValue = (UIntVector2)obj;
            return Equals(ref strongValue);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                return (int)((long)((X * 397) ^ Y) - int.MaxValue);
            }
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(UIntVector2 left, UIntVector2 right)
        {
            return left.Equals(ref right);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(UIntVector2 left, UIntVector2 right)
        {
            return !left.Equals(ref right);
        }

        public override string ToString()
        {
            return string.Format("({0},{1})", X, Y);
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="Vector2"/> to <see cref="UIntVector2"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator UIntVector2(Vector2 value)
        {
            return new UIntVector2((uint)value.X, (uint)value.Y);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="UIntVector2"/> to <see cref="Vector2"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator Vector2(UIntVector2 value)
        {
            return new Vector2(value.X, value.Y);
        }

        public static implicit operator IntVector2(UIntVector2 value)
        {
            return new IntVector2((int)value.X, (int)value.Y);
        }

        public static explicit operator UIntVector2(IntVector2 value)
        {
            return new UIntVector2((uint)value.X, (uint)value.Y);
        }

        public static IntVector2 operator /(UIntVector2 left, UIntVector2 right)
        {
            return new UIntVector2(left.X / right.X, left.Y / right.Y);
        }
    }
}