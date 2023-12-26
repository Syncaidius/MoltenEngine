using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Molten
{
    /// <summary>Represents a single-precision 2x2 Matrix. Contains only scale and rotation.</summary>
    [StructLayout(LayoutKind.Explicit)]
    [DataContract]
	public partial struct Matrix2F : IEquatable<Matrix2F>, IFormattable, ITransposableMatrix<Matrix2F, Matrix2F>, IMatrix<float>
    {
        /// <summary>
        /// A single-precision <see cref="Matrix2F"/> with values intialized to the identity of a 2 x 2 matrix
        /// </summary>
        public static readonly Matrix2F Identity = new Matrix2F() 
        { 
            M11 = 1F, 
            M22 = 1F, 
        };

        public static readonly int ComponentCount = 4;

        public static readonly int RowCount = 2;

        public static readonly int ColumnCount = 2;

        /// <summary>A <see cref="Matrix2F"/> will all of its components set to 0F.</summary>
        public static readonly Matrix2F Zero = new Matrix2F();

        /// <summary> Gets a value indicating whether this instance is an identity matrix. </summary>
        /// <value>
        /// <c>true</c> if this instance is an identity matrix; otherwise, <c>false</c>.
        /// </value>
        public bool IsIdentity => Equals(Identity);

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


        /// <summary> Row 1 of the current <see cref="Matrix2F"/>.</summary>
        /// <returns>A <see cref="Vector2F"/> containing the row values.</returns>
        public Vector2F Row1
        {
            get => new Vector2F(M11, M12);
            set => (M11, M12) = value;
        }

        /// <summary> Row 2 of the current <see cref="Matrix2F"/>.</summary>
        /// <returns>A <see cref="Vector2F"/> containing the row values.</returns>
        public Vector2F Row2
        {
            get => new Vector2F(M21, M22);
            set => (M21, M22) = value;
        }


        /// <summary> Column 1 of the current <see cref="Matrix2F"/>.</summary>
        /// <returns>A <see cref="Vector2F"/> containing the column values.</returns>
        public Vector2F Column1
        {
            get => new Vector2F(M11, M21);
            set => (M11, M21) = value;
        }

        /// <summary> Column 2 of the current <see cref="Matrix2F"/>.</summary>
        /// <returns>A <see cref="Vector2F"/> containing the column values.</returns>
        public Vector2F Column2
        {
            get => new Vector2F(M12, M22);
            set => (M12, M22) = value;
        }


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
		/// <param name="values">The values to assign to the M11, M12, M21, M22 components of the color. This must be an array with at least 4 elements.</param>
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
		/// <param name="values">The values to assign to the M11, M12, M21, M22 components of the color. This must be an array with at least 4 elements.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than 4 elements.</exception>
		public Matrix2F(Span<float> values)
		{
			if (values == null)
				throw new ArgumentNullException("values");
			if (values.Length < 4)
				throw new ArgumentOutOfRangeException("values", "There must be at least 4 input values for Matrix2F.");

			M11 = values[0];
			M12 = values[1];
			M21 = values[2];
			M22 = values[3];
		}
		/// <summary>Initializes a new instance of <see cref="Matrix2F"/> from a an unsafe pointer.</summary>
		/// <param name="ptrValues">The values to assign to the M11, M12, M21, M22 components of the color.
		/// <para>There must be at least 4 elements available or undefined behaviour will occur.</para></param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="ptrValues"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="ptrValues"/> contains more or less than 4 elements.</exception>
		public unsafe Matrix2F(float* ptrValues)
		{
			if (ptrValues == null)
				throw new ArgumentNullException("ptrValues");

			fixed (float* dst = Values)
				Unsafe.CopyBlock(ptrValues, dst, (sizeof(float) * 4));
		}

        /// <summary> Creates a string representation of the matrix.</summary>
        /// <returns>A string representation of the matrix.</returns>
        public override string ToString()
        {
            return string.Format("[{0}, {1}] [{2}, {3}]", 
            M11, M12, M21, M22);
        }

        /// <summary> Returns a <see cref="String"/> that represents this instance.</summary>
        /// <param name="format">The format.</param>
        /// <returns>
        /// A <see cref="string"/> that represents this instance.
        /// </returns>
        public string ToString(string format)
        {
            if (format == null)
                return ToString();

            CultureInfo cc = CultureInfo.CurrentCulture;
            return string.Format("[{0}, {1}] [{2}, {3}]", cc, 
            M11.ToString(format, cc), M12.ToString(format, cc), M21.ToString(format, cc), M22.ToString(format, cc));
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
            return string.Format("[{0}, {1}] [{2}, {3}]", 
            M11.ToString(formatProvider), M12.ToString(formatProvider), M21.ToString(formatProvider), M22.ToString(formatProvider));
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

            CultureInfo cc = CultureInfo.CurrentCulture;
            return string.Format("[{0}, {1}] [{2}, {3}]", cc, 
            M11.ToString(format, formatProvider), M12.ToString(format, formatProvider), M21.ToString(format, formatProvider), M22.ToString(format, formatProvider));
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
            return MathHelper.NearEqual(other.M11, M11)
            && MathHelper.NearEqual(other.M12, M12)
            && MathHelper.NearEqual(other.M21, M21)
            && MathHelper.NearEqual(other.M22, M22);
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
            if (value is Matrix2F mat)
                return Equals(ref mat);

            return false;
        }

        /// <summary>
        /// Creates an array containing the elements of the <see cref="Matrix2F"/>.
        /// </summary>
        /// <returns>A 4-element array containing the components of the matrix.</returns>
        public unsafe float[] ToArray()
        {
            return [M11, M12, M21, M22];
        }

        /// <summary>
        /// Transposes the current <see cref="Matrix2F"/> and outputs it to <paramref name="result"/>.
        /// </summary>
        /// <param name="result"></param>
        public void Transpose(out Matrix2F result)
        {
            Transpose(ref this, out result);
        }
      
        /// <summary>
        /// Transposes the current <see cref="Matrix2F"/> in-place.
        /// </summary>
        public Matrix2F Transpose()
        {
            Transpose(ref this, out Matrix2F result);
            return result;
        }
        

        /// <summary>
        /// Calculates the transpose of the specified <see cref="Matrix2F"/>.
        /// </summary>
        /// <param name="matrix">The <see cref="Matrix2F"/> whose transpose is to be calculated.</param>
        /// <param name="result">When the method completes, contains the transpose of the specified matrix.</param>
        public static void Transpose(ref Matrix2F matrix, out Matrix2F result)
        {
            Unsafe.SkipInit(out result);
            result.M11 = matrix.M11;
            result.M12 = matrix.M21;
            result.M21 = matrix.M12;
            result.M22 = matrix.M22;
        }

        /// <summary>
        /// Calculates the transpose of the specified <see cref="Matrix2F"/>.
        /// </summary>
        /// <param name="value">The <see cref="Matrix2F"/> whose transpose is to be calculated.</param>
        /// <returns>The transpose of the specified <see cref="Matrix2F"/>.</returns>
        public static Matrix2F Transpose(Matrix2F value)
        {
            Transpose(ref value, out Matrix2F result);
            return result;
        }

        /// <summary>
        /// Performs a linear interpolation between two <see cref="Matrix2F"/>.
        /// </summary>
        /// <param name="start">Start <see cref="Matrix2F"/>.</param>
        /// <param name="end">End <see cref="Matrix2F"/>.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <param name="result">When the method completes, contains the linear interpolation of the two <see cref="Matrix2F"/> matrices.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        public static void Lerp(ref Matrix2F start, ref Matrix2F end, float amount, out Matrix2F result)
        {
            result.M11 = MathHelper.Lerp(start.M11, end.M11, amount);
            result.M12 = MathHelper.Lerp(start.M12, end.M12, amount);
            result.M21 = MathHelper.Lerp(start.M21, end.M21, amount);
            result.M22 = MathHelper.Lerp(start.M22, end.M22, amount);
        }

        /// <summary>
        /// Performs a cubic interpolation between two <see cref="Matrix2F"/>.
        /// </summary>
        /// <param name="start">Start <see cref="Matrix2F"/>.</param>
        /// <param name="end">End <see cref="Matrix2F"/>.</param>
        /// <param name="amount">Value between 0F and 1F indicating the weight of <paramref name="end"/>.</param>
        /// <param name="result">When the method completes, contains the cubic interpolation of the two <see cref="Matrix2F"/> matrices.</param>
        public static void SmoothStep(ref Matrix2F start, ref Matrix2F end, float amount, out Matrix2F result)
        {
            amount = MathHelper.SmoothStep(amount);
            Lerp(ref start, ref end, amount, out result);
        }

        /// <summary>
        /// Performs a cubic interpolation between two matrices.
        /// </summary>
        /// <param name="start">Start <see cref="Matrix2F"/>.</param>
        /// <param name="end">End <see cref="Matrix2F"/>.</param>
        /// <param name="amount">Value between 0F and 1F indicating the weight of <paramref name="end"/>.</param>
        /// <returns>The cubic interpolation of the two matrices.</returns>
        public static Matrix2F SmoothStep(Matrix2F start, Matrix2F end, float amount)
        {
            SmoothStep(ref start, ref end, amount, out Matrix2F result);
            return result;
        }

        /// <summary>
        /// Performs a linear interpolation between two matrices.
        /// </summary>
        /// <param name="start">Start matrix.</param>
        /// <param name="end">End matrix.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <returns>The linear interpolation of the two matrices.</returns>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        public static Matrix2F Lerp(Matrix2F start, Matrix2F end, float amount)
        {
            Lerp(ref start, ref end, amount, out Matrix2F result);
            return result;
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
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="row"/> [0 to 1] or <paramref name="column"/> [0 to 1] is out of the range .</exception>
        public unsafe float this[int row, int column]
        {
            get
            {
                if (row < 0 || row > 1)
                    throw new ArgumentOutOfRangeException("row", "Rolumns for Matrix2F run from 0 to 1, inclusive.");

                if (column < 0 || column > 1)
                    throw new ArgumentOutOfRangeException("column", "Columns for Matrix2F run from 0 to 1, inclusive.");

                return Values[(row * 2) + column];
            }

            set
            {
                if (row < 0 || row > 1)
                    throw new ArgumentOutOfRangeException("row", "Rows for Matrix2F run from 0 to 1, inclusive.");

                if (column < 0 || column > 1)
                    throw new ArgumentOutOfRangeException("column", "Columns for Matrix2F run from 0 to 1, inclusive.");

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
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="row"/> [0 to 1] or <paramref name="column"/> [0 to 1] is out of the range .</exception>
        public unsafe float this[uint row, uint column]
        {
            get
            {
                if (row > 1)
                    throw new ArgumentOutOfRangeException("row", "Row index must be less than 1");

                if (column > 1)
                    throw new ArgumentOutOfRangeException("row", "Column index must be less than 1");

                return Values[(row * 2) + column];
            }

            set
            {
                if (row > 1)
                    throw new ArgumentOutOfRangeException("row", "Row and column index must be less than 1");

                if (column > 1)
                    throw new ArgumentOutOfRangeException("column", "Row and column index must be less than 1");

                Values[(row * 2) + column] = value;
            }
        }

        /// <summary>
        /// Returns a hash code for the current <see cref="Matrix2F"/>.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="Matrix2F"/>, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = M11.GetHashCode();
                 hashCode = (hashCode * 397) ^ M12.GetHashCode();
                 hashCode = (hashCode * 397) ^ M21.GetHashCode();
                 hashCode = (hashCode * 397) ^ M22.GetHashCode();
                 return hashCode;
            }
        }

        public static bool operator ==(Matrix2F matrix1, Matrix2F matrix2)
        {
            return matrix1.Equals(ref matrix2);
        }

        public static bool operator !=(Matrix2F matrix1, Matrix2F matrix2)
        {
            return !matrix1.Equals(ref matrix2);
        }

#region Add operators
		///<summary>Performs a add operation on two <see cref="Matrix2F"/>.</summary>
		///<param name="a">The first <see cref="Matrix2F"/> to add.</param>
		///<param name="b">The second <see cref="Matrix2F"/> to add.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Add(ref Matrix2F a, ref Matrix2F b, out Matrix2F result)
		{
			result.M11 = a.M11 + b.M11;
			result.M12 = a.M12 + b.M12;
			result.M21 = a.M21 + b.M21;
			result.M22 = a.M22 + b.M22;
		}

		///<summary>Performs a add operation on two <see cref="Matrix2F"/>.</summary>
		///<param name="a">The first <see cref="Matrix2F"/> to add.</param>
		///<param name="b">The second <see cref="Matrix2F"/> to add.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Matrix2F operator +(Matrix2F a, Matrix2F b)
		{
			Add(ref a, ref b, out Matrix2F result);
			return result;
		}

		///<summary>Performs a add operation on a <see cref="Matrix2F"/> and a <see cref="float"/>.</summary>
		///<param name="a">The <see cref="Matrix2F"/> to add.</param>
		///<param name="b">The <see cref="float"/> to add.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Add(ref Matrix2F a, float b, out Matrix2F result)
		{
			result.M11 = a.M11 + b;
			result.M12 = a.M12 + b;
			result.M21 = a.M21 + b;
			result.M22 = a.M22 + b;
		}

		///<summary>Performs a add operation on a <see cref="Matrix2F"/> and a <see cref="float"/>.</summary>
		///<param name="a">The <see cref="Matrix2F"/> to add.</param>
		///<param name="b">The <see cref="float"/> to add.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Matrix2F operator +(Matrix2F a, float b)
		{
			Add(ref a, b, out Matrix2F result);
			return result;
		}

		///<summary>Performs a add operation on a <see cref="float"/> and a <see cref="Matrix2F"/>.</summary>
		///<param name="a">The <see cref="float"/> to add.</param>
		///<param name="b">The <see cref="Matrix2F"/> to add.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Matrix2F operator +(float a, Matrix2F b)
		{
			Add(ref b, a, out Matrix2F result);
			return result;
		}


        /// <summary>
        /// Assert a <see cref="Matrix2F"/> (return it unchanged).
        /// </summary>
        /// <param name="value">The <see cref="Matrix2F"/> to assert (unchanged).</param>
        /// <returns>The asserted (unchanged) <see cref="Matrix2F"/>.</returns>
        public static Matrix2F operator +(Matrix2F value)
        {
            return value;
        }
#endregion

#region Subtract operators
		///<summary>Performs a subtract operation on two <see cref="Matrix2F"/>.</summary>
		///<param name="a">The first <see cref="Matrix2F"/> to subtract.</param>
		///<param name="b">The second <see cref="Matrix2F"/> to subtract.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Subtract(ref Matrix2F a, ref Matrix2F b, out Matrix2F result)
		{
			result.M11 = a.M11 - b.M11;
			result.M12 = a.M12 - b.M12;
			result.M21 = a.M21 - b.M21;
			result.M22 = a.M22 - b.M22;
		}

		///<summary>Performs a subtract operation on two <see cref="Matrix2F"/>.</summary>
		///<param name="a">The first <see cref="Matrix2F"/> to subtract.</param>
		///<param name="b">The second <see cref="Matrix2F"/> to subtract.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Matrix2F operator -(Matrix2F a, Matrix2F b)
		{
			Subtract(ref a, ref b, out Matrix2F result);
			return result;
		}

		///<summary>Performs a subtract operation on a <see cref="Matrix2F"/> and a <see cref="float"/>.</summary>
		///<param name="a">The <see cref="Matrix2F"/> to subtract.</param>
		///<param name="b">The <see cref="float"/> to subtract.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Subtract(ref Matrix2F a, float b, out Matrix2F result)
		{
			result.M11 = a.M11 - b;
			result.M12 = a.M12 - b;
			result.M21 = a.M21 - b;
			result.M22 = a.M22 - b;
		}

		///<summary>Performs a subtract operation on a <see cref="Matrix2F"/> and a <see cref="float"/>.</summary>
		///<param name="a">The <see cref="Matrix2F"/> to subtract.</param>
		///<param name="b">The <see cref="float"/> to subtract.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Matrix2F operator -(Matrix2F a, float b)
		{
			Subtract(ref a, b, out Matrix2F result);
			return result;
		}

		///<summary>Performs a subtract operation on a <see cref="float"/> and a <see cref="Matrix2F"/>.</summary>
		///<param name="a">The <see cref="float"/> to subtract.</param>
		///<param name="b">The <see cref="Matrix2F"/> to subtract.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Matrix2F operator -(float a, Matrix2F b)
		{
			Subtract(ref b, a, out Matrix2F result);
			return result;
		}


        /// <summary>
        /// Negates a <see cref="Matrix2F"/>.
        /// </summary>
        /// <param name="value">The matrix to be negated.</param>
        /// <param name="result">When the method completes, contains the negated <see cref="Matrix2F"/>.</param>
        public static void Negate(ref Matrix2F value, out Matrix2F result)
        {
             result.M11 = -value.M11;
             result.M12 = -value.M12;
             result.M21 = -value.M21;
             result.M22 = -value.M22;
         }

        /// <summary>
        /// Negates a <see cref="Matrix2F"/>.
        /// </summary>
        /// <param name="value">The <see cref="Matrix2F"/> to be negated.</param>
        /// <returns>The negated <see cref="Matrix2F"/>.</returns>
        public static Matrix2F Negate(Matrix2F value)
        {
            Matrix2F result;
            Negate(ref value, out result);
            return result;
        }

        /// <summary>
        /// Negates a <see cref="Matrix2F"/>.
        /// </summary>
        /// <param name="value">The <see cref="Matrix2F"/> to negate.</param>
        /// <returns>The negated <see cref="Matrix2F"/>.</returns>
        public static Matrix2F operator -(Matrix2F value)
        {
            Negate(ref value, out Matrix2F result);
            return result;
        }
#endregion

#region Multiply operators
        /// <summary>
        /// Scales a <see cref="Matrix2F"/> by the given scalar value.
        /// </summary>
        /// <param name="matrix">The <see cref="Matrix2F"/> to scale.</param>
        /// <param name="scalar">The scalar value by which to scale.</param>
        /// <param name="result">When the method completes, contains the scaled <see cref="Matrix2F"/>.</param>
        public static void Multiply(ref Matrix2F matrix, float scalar, out Matrix2F result)
        {
			result.M11 = matrix.M11 * scalar;
			result.M12 = matrix.M12 * scalar;
			result.M21 = matrix.M21 * scalar;
			result.M22 = matrix.M22 * scalar;
        }

        /// <summary>
        /// Scales a <see cref="Matrix2F"/> by a given value.
        /// </summary>
        /// <param name="matrix">The <see cref="Matrix2F"/> to scale.</param>
        /// <param name="scalar">The scalar value by which to scale.</param>
        /// <returns>The scaled matrix.</returns>
        public static Matrix2F operator *(Matrix2F matrix, float scalar)
        {
            Multiply(ref matrix, scalar, out Matrix2F result);
            return result;
        }
                
        /// <summary>
        /// Scales a <see cref="Matrix2F"/> by a given value.
        /// </summary>
        /// <param name="scalar">The scalar value by which to scale.</param>
        /// <param name="matrix">The <see cref="Matrix2F"/> to scale.</param>
        /// <returns>The scaled matrix.</returns>

        public static Matrix2F operator *(float scalar, Matrix2F matrix)
        {
            Multiply(ref matrix, scalar, out Matrix2F result);
            return result;
        }
#endregion

#region division operators
/// <summary>
        /// Scales a <see cref="Matrix2F"/> by the given scalar value.
        /// </summary>
        /// <param name="matrix">The <see cref="Matrix2F"/> to scale.</param>
        /// <param name="scalar">The amount by which to scale.</param>
        /// <param name="result">When the method completes, contains the scaled <see cref="Matrix2F"/>.</param>
        public static void Divide(ref Matrix2F matrix, float scalar, out Matrix2F result)
        {
            float inv = 1F / scalar;
			result.M11 = matrix.M11 * inv;
			result.M12 = matrix.M12 * inv;
			result.M21 = matrix.M21 * inv;
			result.M22 = matrix.M22 * inv;
        }

        /// <summary>
        /// Scales a <see cref="Matrix2F"/> by the given scalar value.
        /// </summary>
        /// <param name="matrix">The <see cref="Matrix2F"/> to scale.</param>
        /// <param name="scalar">The amount by which to scale.</param>
        /// <returns>The scaled matrix.</returns>
        public static Matrix2F Divide(Matrix2F matrix, float scalar)
        {
            Divide(ref matrix, scalar, out Matrix2F result);
            return result;
        }

                /// <summary>
        /// Determines the quotient of two matrices.
        /// </summary>
        /// <param name="left">The first <see cref="Matrix2F"/> to divide.</param>
        /// <param name="right">The second <see cref="Matrix2F"/> to divide.</param>
        /// <param name="result">When the method completes, contains the quotient of the two <see cref="Matrix2F"/> matrices.</param>
        public static void Divide(ref Matrix2F left, ref Matrix2F right, out Matrix2F result)
        {
			result.M11 = left.M11 / right.M11;
			result.M12 = left.M12 / right.M12;
			result.M21 = left.M21 / right.M21;
			result.M22 = left.M22 / right.M22;
        }

        /// <summary>
        /// Scales a <see cref="Matrix2F"/> by a given scalar value.
        /// </summary>
        /// <param name="matrix">The matrix to scale.</param>
        /// <param name="scalar">The amount by which to scale.</param>
        /// <returns>The scaled matrix.</returns>
        public static Matrix2F operator /(Matrix2F matrix, float scalar)
        {
            Divide(ref matrix, scalar, out Matrix2F result);
            return result;
        }

        /// <summary>
        /// Divides two <see cref="Matrix2F"/> matrices.
        /// </summary>
        /// <param name="left">The first <see cref="Matrix2F"/> to divide.</param>
        /// <param name="right">The second <see cref="Matrix2F"/> to divide.</param>
        /// <returns>The quotient of the two <see cref="Matrix2F"/> matrices.</returns>
        public static Matrix2F operator /(Matrix2F left, Matrix2F right)
        {
            Divide(ref left, ref right, out Matrix2F result);
            return result;
        }
#endregion
    }
}

