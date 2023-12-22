using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Molten
{
    /// <summary>Represents a single-precision 2x2 Matrix. Contains only scale and rotation.</summary>
    [StructLayout(LayoutKind.Explicit)]
    [DataContract]
	public struct Matrix2F : IEquatable<Matrix2F>, IFormattable, ITransposedMatrix<Matrix2F>, IMatrix<float>
    {
        /// <summary>
        /// A single-precision Matrix2x2 with values intialized to the identity of a 2 x 2 matrix
        /// </summary>
        public static readonly Matrix2F Identity = new Matrix2F(1, 0, 0, 1);

        public static readonly int ComponentCount = 4;

        public static readonly int RowCount = 2;

        public static readonly int ColumnCount = 2;

		/// <summary>The value at row 1, column 1 of the matrix.</summary>
		[DataMember]
		[FieldOffset(0)]
		public float M11;

		/// <summary>The value at row 1, column 2 of the matrix.</summary>
		[DataMember]
		[FieldOffset(4)]
		public float M12;

		/// <summary>The value at row 2, column 1 of the matrix.</summary>
		[DataMember]
		[FieldOffset(8)]
		public float M21;

		/// <summary>The value at row 2, column 2 of the matrix.</summary>
		[DataMember]
		[FieldOffset(12)]
		public float M22;

		/// <summary>A fixed array mapped to the same memory space as the individual <see cref="Matrix2F"/> components.</summary>
		[IgnoreDataMember]
		[FieldOffset(0)]
		public unsafe fixed float Values[4];


		/// <summary>
		/// Initializes a new instance of <see cref="Matrix2F"/>.
		/// </summary>
		/// <param name="m11">The value to assign to row 1, column 1 of the matrix.</param>
		/// <param name="m12">The value to assign to row 1, column 2 of the matrix.</param>
		/// <param name="m21">The value to assign to row 2, column 1 of the matrix.</param>
		/// <param name="m22">The value to assign to row 2, column 2 of the matrix.</param>
		public Matrix2F(float m11, float m12, float m21, float m22)
		{
			M11 = m11;
			M12 = m12;
			M21 = m21;
			M22 = m22;
		}
		/// <summary>Initializes a new instance of <see cref="Matrix2F"/>.</summary>
		/// <param name="value">The value that will be assigned to all components.</param>
		public Matrix2F(float value)
		{
			M11 = value;
			M12 = value;
			M21 = value;
			M22 = value;
		}
		/// <summary>Initializes a new instance of <see cref="Matrix2F"/> from an array.</summary>
		/// <param name="values">The values to assign to the M11, M12 components of the color. This must be an array with at least two elements.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than 4 elements.</exception>
		public unsafe Matrix2F(float[] values)
		{
			if (values == null)
				throw new ArgumentNullException("values");
			if (values.Length < 4)
				throw new ArgumentOutOfRangeException("values", "There must be at least 4 input values for Matrix2F.");

			fixed (float* src = values)
			{
				fixed (float* dst = Values)
					Unsafe.CopyBlock(src, dst, (sizeof(float) * 4));
			}
		}
		/// <summary>Initializes a new instance of <see cref="Matrix2F"/> from a span.</summary>
		/// <param name="values">The values to assign to the M11, M12 components of the color. This must be an array with at least two elements.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than 4 elements.</exception>
		public Matrix2F(Span<float> values)
		{
			if (values == null)
				throw new ArgumentNullException("values");
			if (values.Length < 2)
				throw new ArgumentOutOfRangeException("values", "There must be at least two input values for Matrix2F.");

			M11 = values[0];
			M12 = values[1];
		}
		/// <summary>Initializes a new instance of <see cref="Matrix2F"/> from a an unsafe pointer.</summary>
		/// <param name="ptrValues">The values to assign to the M11, M12 components of the color.
		/// <para>There must be at least two elements available or undefined behaviour will occur.</para></param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="ptrValues"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="ptrValues"/> contains more or less than 4 elements.</exception>
		public unsafe Matrix2F(float* ptrValues)
		{
			if (ptrValues == null)
				throw new ArgumentNullException("ptrValues");

			M11 = ptrValues[0];
			M12 = ptrValues[1];
			M21 = ptrValues[2];
			M22 = ptrValues[3];
		}

        /// <summary>
        /// Adds the two matrices together on a per-element basis.
        /// </summary>
        /// <param name="a">First matrix to add.</param>
        /// <param name="b">Second matrix to add.</param>
        /// <param name="result">Sum of the two matrices.</param>
        public static void Add(ref Matrix2F a, ref Matrix2F b, out Matrix2F result)
        {
            result.M11 = a.M11 + b.M11;
            result.M12 = a.M12 + b.M12;
            result.M21 = a.M21 + b.M21;
            result.M22 = a.M22 + b.M22;
        }

        /// <summary>
        /// Adds the two matrices together on a per-element basis.
        /// </summary>
        /// <param name="a">First matrix to add.</param>
        /// <param name="b">Second matrix to add.</param>
        /// <param name="result">Sum of the two matrices.</param>
        public static void Add(ref Matrix4F a, ref Matrix2F b, out Matrix2F result)
        {
            result.M11 = a.M11 + b.M11;
            result.M12 = a.M12 + b.M12;
            result.M21 = a.M21 + b.M21;
            result.M22 = a.M22 + b.M22;
        }

        /// <summary>
        /// Adds the two matrices together on a per-element basis.
        /// </summary>
        /// <param name="a">First matrix to add.</param>
        /// <param name="b">Second matrix to add.</param>
        /// <param name="result">Sum of the two matrices.</param>
        public static void Add(ref Matrix2F a, ref Matrix4F b, out Matrix2F result)
        {
            result.M11 = a.M11 + b.M11;
            result.M12 = a.M12 + b.M12;
            result.M21 = a.M21 + b.M21;
            result.M22 = a.M22 + b.M22;
        }

        /// <summary>
        /// Adds the two matrices together on a per-element basis.
        /// </summary>
        /// <param name="a">First matrix to add.</param>
        /// <param name="b">Second matrix to add.</param>
        /// <param name="result">Sum of the two matrices.</param>
        public static void Add(ref Matrix4F a, ref Matrix4F b, out Matrix2F result)
        {
            result.M11 = a.M11 + b.M11;
            result.M12 = a.M12 + b.M12;
            result.M21 = a.M21 + b.M21;
            result.M22 = a.M22 + b.M22;
        }

        /// <summary>
        /// Constructs a uniform scaling matrix.
        /// </summary>
        /// <param name="scale">Value to use in the diagonal.</param>
        /// <param name="matrix">Scaling matrix.</param>
        public static void CreateScale(float scale, out Matrix2F matrix)
        {
            matrix.M11 = scale;
            matrix.M22 = scale;
            matrix.M12 = 0;
            matrix.M21 = 0;
        }


        /// <summary>
        /// Inverts the given matix.
        /// </summary>
        /// <param name="matrix">Matrix to be inverted.</param>
        /// <param name="result">Inverted matrix.</param>
        public static void Invert(ref Matrix2F matrix, out Matrix2F result)
        {
            float determinantInverse = 1 / (matrix.M11 * matrix.M22 - matrix.M12 * matrix.M21);
            result.M11 = matrix.M22 * determinantInverse;
            result.M12 = -matrix.M12 * determinantInverse;

            result.M21 = -matrix.M21 * determinantInverse;
            result.M22 = matrix.M11 * determinantInverse;
        }

        /// <summary>
        /// Multiplies the two matrices.
        /// </summary>
        /// <param name="a">First matrix to multiply.</param>
        /// <param name="b">Second matrix to multiply.</param>
        /// <param name="result">Product of the multiplication.</param>
        public static void Multiply(ref Matrix2F a, ref Matrix2F b, out Matrix2F result)
        {
            result.M11 = a.M11 * b.M11 + a.M12 * b.M21;
            result.M12 = a.M11 * b.M12 + a.M12 * b.M22;
            result.M21 = a.M21 * b.M11 + a.M22 * b.M21;
            result.M22 = a.M21 * b.M12 + a.M22 * b.M22;
        }

        /// <summary>
        /// Multiplies the two matrices.
        /// </summary>
        /// <param name="a">First matrix to multiply.</param>
        /// <param name="b">Second matrix to multiply.</param>
        /// <param name="result">Product of the multiplication.</param>
        public static void Multiply(ref Matrix2F a, ref Matrix4F b, out Matrix2F result)
        {
            result.M11 = a.M11 * b.M11 + a.M12 * b.M21;
            result.M12 = a.M11 * b.M12 + a.M12 * b.M22;
            result.M21 = a.M21 * b.M11 + a.M22 * b.M21;
            result.M22 = a.M21 * b.M12 + a.M22 * b.M22;
        }

        /// <summary>
        /// Multiplies the two matrices.
        /// </summary>
        /// <param name="a">First matrix to multiply.</param>
        /// <param name="b">Second matrix to multiply.</param>
        /// <param name="result">Product of the multiplication.</param>
        public static void Multiply(ref Matrix4F a, ref Matrix2F b, out Matrix2F result)
        {
            result.M11 = a.M11 * b.M11 + a.M12 * b.M21;
            result.M12 = a.M11 * b.M12 + a.M12 * b.M22;
            result.M21 = a.M21 * b.M11 + a.M22 * b.M21;
            result.M22 = a.M21 * b.M12 + a.M22 * b.M22;
        }

        /// <summary>
        /// Multiplies the two matrices.
        /// </summary>
        /// <param name="a">First matrix to multiply.</param>
        /// <param name="b">Second matrix to multiply.</param>
        /// <param name="result">Product of the multiplication.</param>
        public static void Multiply(ref Matrix2x3F a, ref Matrix3x2F b, out Matrix2F result)
        {
            result.M11 = a.M11 * b.M11 + a.M12 * b.M21 + a.M13 * b.M31;
            result.M12 = a.M11 * b.M12 + a.M12 * b.M22 + a.M13 * b.M32;
            result.M21 = a.M21 * b.M11 + a.M22 * b.M21 + a.M23 * b.M31;
            result.M22 = a.M21 * b.M12 + a.M22 * b.M22 + a.M23 * b.M32;
        }

        /// <summary>
        /// Negates every element in the matrix.
        /// </summary>
        /// <param name="matrix">Matrix to negate.</param>
        /// <param name="result">Negated matrix.</param>
        public static void Negate(ref Matrix2F matrix, out Matrix2F result)
        {
            result.M11 = -matrix.M11;
            result.M12 = -matrix.M12;
            result.M21 = -matrix.M21;
            result.M22 = -matrix.M22;
        }

        /// <summary>
        /// Subtracts the two matrices from each other on a per-element basis.
        /// </summary>
        /// <param name="a">First matrix to subtract.</param>
        /// <param name="b">Second matrix to subtract.</param>
        /// <param name="result">Difference of the two matrices.</param>
        public static void Subtract(ref Matrix2F a, ref Matrix2F b, out Matrix2F result)
        {
            result.M11 = a.M11 - b.M11;
            result.M12 = a.M12 - b.M12;

            result.M21 = a.M21 - b.M21;
            result.M22 = a.M22 - b.M22;
        }

        /// <summary>
        /// Transforms the vector by the matrix.
        /// </summary>
        /// <param name="v">Vector2 to transform.</param>
        /// <param name="matrix">Matrix to use as the transformation.</param>
        /// <param name="result">Product of the transformation.</param>
        public static void Transform(ref Vector2F v, ref Matrix2F matrix, out Vector2F result)
        {
            float vX = v.X;
            float vY = v.Y;

            result.X = vX * matrix.M11 + vY * matrix.M21;
            result.Y = vX * matrix.M12 + vY * matrix.M22;
        }

        public float[] ToArray()
        {
            return [M11, M12, M21, M22];
        }

        public void Transpose(out Matrix2F result)
        {
            result.M21 = M12;
            result.M12 = M21;
            result.M11 = M11;
            result.M22 = M22;
        }

        /// <summary>
        /// Computes the transposed matrix of a matrix.
        /// </summary>
        /// <param name="matrix">Matrix to transpose.</param>
        /// <param name="result">Transposed matrix.</param>
        public static void Transpose(ref Matrix2F matrix, out Matrix2F result)
        {
            matrix.Transpose(out result);
        }

        /// <summary>
        /// Transposes the matrix in-place.
        /// </summary>
        public void Transpose()
        {
            float m21 = M21;
            M21 = M12;
            M12 = m21;
        }

        /// <summary>
        /// Creates a string representation of the matrix.
        /// </summary>
        /// <returns>A string representation of the matrix.</returns>
        public override string ToString()
        {
            return string.Format($"[{M11}, {M12}] [{M21}, {M22}]");
        }

        /// <summary>
        /// Returns a <see cref="String"/> that represents this instance.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns>
        /// A <see cref="string"/> that represents this instance.
        /// </returns>
        public string ToString(string format)
        {
            if (format == null)
                return ToString();

            CultureInfo cc = CultureInfo.CurrentCulture;
            return string.Format(format, cc, "[M11:{0} M12:{1}] [M21:{2} M22:{3}]",
                M11.ToString(format, cc), M12.ToString(format, cc),
                M21.ToString(format, cc), M22.ToString(format, cc));
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents this instance.
        /// </summary>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="string"/> that represents this instance.
        /// </returns>
        public string ToString(IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, "[M11:{0} M12:{1}] [M21:{2} M22:{3}]",
                M11.ToString(formatProvider), M12.ToString(formatProvider),
                M21.ToString(formatProvider), M22.ToString(formatProvider));
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents this instance.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="string"/> that represents this instance.
        /// </returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null)
                return ToString(formatProvider);

            return string.Format(format, formatProvider, "[M11:{0} M12:{1}] [M21:{2} M22:{3}]",
                M11.ToString(format, formatProvider), M12.ToString(format, formatProvider),
                M21.ToString(format, formatProvider), M22.ToString(format, formatProvider));
        }

        /// <summary>
        /// Calculates the determinant of the matrix.
        /// </summary>
        /// <returns>The matrix's determinant.</returns>
        public float Determinant()
        {
            return M11 * M22 - M12 * M21;
        }

        /// <summary>
        /// Determines whether the specified <see cref="Matrix2F"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Matrix2F"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="Matrix2F"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(ref Matrix2F other)
        {
            return MathHelper.NearEqual(other.M11, M11) &&
                MathHelper.NearEqual(other.M12, M12) &&
                MathHelper.NearEqual(other.M21, M21) &&
                MathHelper.NearEqual(other.M22, M22);
        }

        /// <summary>
        /// Determines whether the specified <see cref="Matrix2F"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Matrix2F"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="Matrix2F"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Matrix2F other)
        {
            return Equals(ref other);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="value">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object value)
        {
            if (!(value is Matrix2F))
                return false;

            var strongValue = (Matrix2F)value;
            return Equals(ref strongValue);
        }

        /// <summary>
        /// Gets or sets the component at the specified index.
        /// </summary>
        /// <value>The value of the matrix component, depending on the index.</value>
        /// <param name="index">The zero-based index of the component to access.</param>
        /// <returns>The value of the component at the specified index.</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown when the <paramref name="index"/> is outside the range [0, 3].</exception>  
		public unsafe float this[int index]
		{
			get
            {
                if(index > 3 || index < 0)
                    throw new IndexOutOfRangeException("Index for Matrix2F must be between from 0 to 3, inclusive.");

                return Values[index];
            }
            set
            {
                if (index > 3 || index < 0)
                    throw new IndexOutOfRangeException("Index for Matrix2F must be between from 0 to 3, inclusive.");

                Values[index] = value;
            }
		}

        /// <summary>
        /// Gets or sets the component at the specified index.
        /// </summary>
        /// <value>The value of the matrix component, depending on the index.</value>
        /// <param name="index">The zero-based index of the component to access.</param>
        /// <returns>The value of the component at the specified index.</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown when the <paramref name="index"/> is greater than 3.</exception>
		public unsafe float this[uint index]
		{
			get
            {
                if(index > 3)
                    throw new IndexOutOfRangeException("Index for Matrix2F must less than 4.");

                return Values[index];
            }
            set
            {
                if (index > 3)
                    throw new IndexOutOfRangeException("Index for Matrix2F must less than 4.");

                Values[index] = value;
            }
		}

        /// <summary>
        /// Gets or sets the component at the specified index.
        /// </summary>
        /// <value>The value of the matrix component, depending on the index.</value>
        /// <param name="row">The row of the matrix to access.</param>
        /// <param name="column">The column of the matrix to access.</param>
        /// <returns>The value of the component at the specified index.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="row"/> or <paramref name="column"/>is out of the range [0, 1].</exception>
        public unsafe float this[int row, int column]
        {
            get
            {
                if (row < 0 || row > 1)
                    throw new ArgumentOutOfRangeException("row", "Rows and columns for matrices run from 0 to 1, inclusive.");
                if (column < 0 || column > 1)
                    throw new ArgumentOutOfRangeException("column", "Rows and columns for matrices run from 0 to 1, inclusive.");

                return Values[(row * 2) + column];
            }

            set
            {
                if (row < 0 || row > 1)
                    throw new ArgumentOutOfRangeException("row", "Rows and columns for matrices run from 0 to 1, inclusive.");
                if (column < 0 || column > 1)
                    throw new ArgumentOutOfRangeException("column", "Rows and columns for matrices run from 0 to 1, inclusive.");

                Values[(row * 2) + column] = value;
            }
        }

        /// <summary>
        /// Gets or sets the component at the specified index.
        /// </summary>
        /// <value>The value of the matrix component, depending on the index.</value>
        /// <param name="row">The row of the matrix to access.</param>
        /// <param name="column">The column of the matrix to access.</param>
        /// <returns>The value of the component at the specified index.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="row"/> or <paramref name="column"/>is out of the range [0, 1].</exception>
        public unsafe float this[uint row, uint column]
        {
            get
            {
                if (row > 1)
                    throw new ArgumentOutOfRangeException("row", "Rows and columns for matrices run from 0 to 1, inclusive.");
                if (column > 1)
                    throw new ArgumentOutOfRangeException("column", "Rows and columns for matrices run from 0 to 1, inclusive.");

                return Values[(row * 2) + column];
            }

            set
            {
                if (row > 1)
                    throw new ArgumentOutOfRangeException("row", "Row and column index must be less than 2");

                if (column > 1)
                    throw new ArgumentOutOfRangeException("column", "Row and column index must be less than 2");

                Values[(row * 2) + column] = value;
            }
        }

        public static bool operator ==(Matrix2F matrix1, Matrix2F matrix2)
        {
            return MathHelper.NearEqual(matrix1.M11, matrix2.M11)
                && MathHelper.NearEqual(matrix1.M21, matrix2.M21)
                && MathHelper.NearEqual(matrix1.M12, matrix2.M12)
                && MathHelper.NearEqual(matrix1.M22, matrix2.M22);
        }

        public static bool operator !=(Matrix2F matrix1, Matrix2F matrix2)
        {
            return !MathHelper.NearEqual(matrix1.M11, matrix2.M11)
                || !MathHelper.NearEqual(matrix1.M21, matrix2.M21)
                || !MathHelper.NearEqual(matrix1.M12, matrix2.M12)
                || !MathHelper.NearEqual(matrix1.M22, matrix2.M22);
        }

        public static Matrix2F operator +(Matrix2F matrix1, Matrix2F matrix2)
        {
            return new Matrix2F()
            {
                M11 = matrix1.M11 + matrix2.M11,
                M12 = matrix1.M12 + matrix2.M12,
                M21 = matrix1.M21 + matrix2.M21,
                M22 = matrix1.M22 + matrix2.M22,
            };
        }

        public static Matrix2F operator -(Matrix2F matrix1, Matrix2F matrix2)
        {
            return new Matrix2F()
            {
                M11 = matrix1.M11 - matrix2.M11,
                M12 = matrix1.M12 - matrix2.M12,
                M21 = matrix1.M21 - matrix2.M21,
                M22 = matrix1.M22 - matrix2.M22,
            };
        }

        public static Matrix2F operator *(Matrix2F matrix, float scalar)
        {
            return new Matrix2F()
            {
                M11 = matrix.M11 * scalar,
                M12 = matrix.M12 * scalar,
                M21 = matrix.M21 * scalar,
                M22 = matrix.M22 * scalar,
            };
        }

        public static Matrix2F operator *(float scalar, Matrix2F matrix)
        {
            return new Matrix2F()
            {
                M11 = scalar * matrix.M11,
                M12 = scalar * matrix.M12,
                M21 = scalar * matrix.M21,
                M22 = scalar * matrix.M22,
            };
        }

        public static Matrix2F operator *(Matrix2F matrix1, Matrix2F matrix2)
        {
            return new Matrix2F()
            {
                M11 = matrix1.M11 * matrix2.M11 + matrix1.M21 * matrix2.M12,
                M12 = matrix1.M12 * matrix2.M11 + matrix1.M22 * matrix2.M12,
                M21 = matrix1.M11 * matrix2.M21 + matrix1.M21 * matrix2.M22,
                M22 = matrix1.M12 * matrix2.M21 + matrix1.M22 * matrix2.M22,
            };
        }
        public static Matrix3x2F operator *(Matrix2F matrix1, Matrix3x2F matrix2)
        {
            return new Matrix3x2F()
            {
                M11 = matrix1.M11 * matrix2.M11 + matrix1.M21 * matrix2.M12,
                M21 = matrix1.M11 * matrix2.M21 + matrix1.M21 * matrix2.M22,
                M31 = matrix1.M11 * matrix2.M31 + matrix1.M21 * matrix2.M32,
                M12 = matrix1.M12 * matrix2.M11 + matrix1.M22 * matrix2.M12,
                M22 = matrix1.M12 * matrix2.M21 + matrix1.M22 * matrix2.M22,
                M32 = matrix1.M12 * matrix2.M31 + matrix1.M22 * matrix2.M32,
            };
        }
    }
}

