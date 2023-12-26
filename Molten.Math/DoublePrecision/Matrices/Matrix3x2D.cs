using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Molten.DoublePrecision
{
    /// <summary>Represents a double-precision 3x2 Matrix. Contains only scale and rotation.</summary>
    [StructLayout(LayoutKind.Explicit)]
    [DataContract]
	public partial struct Matrix3x2D : IEquatable<Matrix3x2D>, IFormattable, ITransposableMatrix<Matrix3x2D, Matrix2x3D>, IMatrix<double>
    {
        /// <summary>
        /// A single-precision <see cref="Matrix3x2D"/> with values intialized to the identity of a 2 x 2 matrix
        /// </summary>
        public static readonly Matrix3x2D Identity = new Matrix3x2D() 
        { 
            M11 = 1D, 
            M22 = 1D, 
        };

        public static readonly int ComponentCount = 6;

        public static readonly int RowCount = 3;

        public static readonly int ColumnCount = 2;

        /// <summary>A <see cref="Matrix3x2D"/> will all of its components set to 0D.</summary>
        public static readonly Matrix3x2D Zero = new Matrix3x2D();

        /// <summary> Gets a value indicating whether this instance is an identity matrix. </summary>
        /// <value>
        /// <c>true</c> if this instance is an identity matrix; otherwise, <c>false</c>.
        /// </value>
        public bool IsIdentity => Equals(Identity);

		/// <summary>The value at row 1, column 1 of the matrix.</summary>
		[DataMember]
		[FieldOffset(0)]
		public double M11;

		/// <summary>The value at row 1, column 2 of the matrix.</summary>
		[DataMember]
		[FieldOffset(8)]
		public double M12;

		/// <summary>The value at row 2, column 1 of the matrix.</summary>
		[DataMember]
		[FieldOffset(16)]
		public double M21;

		/// <summary>The value at row 2, column 2 of the matrix.</summary>
		[DataMember]
		[FieldOffset(24)]
		public double M22;

		/// <summary>The value at row 3, column 1 of the matrix.</summary>
		[DataMember]
		[FieldOffset(32)]
		public double M31;

		/// <summary>The value at row 3, column 2 of the matrix.</summary>
		[DataMember]
		[FieldOffset(40)]
		public double M32;

		/// <summary>A fixed array mapped to the same memory space as the individual <see cref="Matrix3x2D"/> components.</summary>
		[IgnoreDataMember]
		[FieldOffset(0)]
		public unsafe fixed double Values[6];


        /// <summary> Row 1 of the current <see cref="Matrix3x2D"/>.</summary>
        /// <returns>A <see cref="Vector3D"/> containing the row values.</returns>
        public Vector2D Row1
        {
            get => new Vector2D(M11, M12);
            set => (M11, M12) = value;
        }

        /// <summary> Row 2 of the current <see cref="Matrix3x2D"/>.</summary>
        /// <returns>A <see cref="Vector3D"/> containing the row values.</returns>
        public Vector2D Row2
        {
            get => new Vector2D(M21, M22);
            set => (M21, M22) = value;
        }

        /// <summary> Row 3 of the current <see cref="Matrix3x2D"/>.</summary>
        /// <returns>A <see cref="Vector3D"/> containing the row values.</returns>
        public Vector2D Row3
        {
            get => new Vector2D(M31, M32);
            set => (M31, M32) = value;
        }


        /// <summary> Column 1 of the current <see cref="Matrix3x2D"/>.</summary>
        /// <returns>A <see cref="Vector3D"/> containing the column values.</returns>
        public Vector3D Column1
        {
            get => new Vector3D(M11, M21, M31);
            set => (M11, M21, M31) = value;
        }

        /// <summary> Column 2 of the current <see cref="Matrix3x2D"/>.</summary>
        /// <returns>A <see cref="Vector3D"/> containing the column values.</returns>
        public Vector3D Column2
        {
            get => new Vector3D(M12, M22, M32);
            set => (M12, M22, M32) = value;
        }


		/// <summary>
		/// Initializes a new instance of <see cref="Matrix3x2D"/>.
		/// </summary>
		/// <param name="m11">The value to assign to row 1, column 1 of the matrix.</param>
		/// <param name="m12">The value to assign to row 1, column 2 of the matrix.</param>
		/// <param name="m21">The value to assign to row 2, column 1 of the matrix.</param>
		/// <param name="m22">The value to assign to row 2, column 2 of the matrix.</param>
		/// <param name="m31">The value to assign to row 3, column 1 of the matrix.</param>
		/// <param name="m32">The value to assign to row 3, column 2 of the matrix.</param>
		public Matrix3x2D(double m11, double m12, double m21, double m22, double m31, double m32)
		{
			M11 = m11;
			M12 = m12;
			M21 = m21;
			M22 = m22;
			M31 = m31;
			M32 = m32;
		}
		/// <summary>Initializes a new instance of <see cref="Matrix3x2D"/>.</summary>
		/// <param name="value">The value that will be assigned to all components.</param>
		public Matrix3x2D(double value)
		{
			M11 = value;
			M12 = value;
			M21 = value;
			M22 = value;
			M31 = value;
			M32 = value;
		}
		/// <summary>Initializes a new instance of <see cref="Matrix3x2D"/> from an array.</summary>
		/// <param name="values">The values to assign to the M11, M12, M21, M22, M31, M32 components of the color. This must be an array with at least 6 elements.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than 6 elements.</exception>
		public unsafe Matrix3x2D(double[] values)
		{
			if (values == null)
				throw new ArgumentNullException("values");
			if (values.Length < 6)
				throw new ArgumentOutOfRangeException("values", "There must be at least 6 input values for Matrix3x2D.");

			fixed (double* src = values)
			{
				fixed (double* dst = Values)
					Unsafe.CopyBlock(src, dst, (sizeof(double) * 6));
			}
		}
		/// <summary>Initializes a new instance of <see cref="Matrix3x2D"/> from a span.</summary>
		/// <param name="values">The values to assign to the M11, M12, M21, M22, M31, M32 components of the color. This must be an array with at least 6 elements.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than 6 elements.</exception>
		public Matrix3x2D(Span<double> values)
		{
			if (values == null)
				throw new ArgumentNullException("values");
			if (values.Length < 6)
				throw new ArgumentOutOfRangeException("values", "There must be at least 6 input values for Matrix3x2D.");

			M11 = values[0];
			M12 = values[1];
			M21 = values[2];
			M22 = values[3];
			M31 = values[4];
			M32 = values[5];
		}
		/// <summary>Initializes a new instance of <see cref="Matrix3x2D"/> from a an unsafe pointer.</summary>
		/// <param name="ptrValues">The values to assign to the M11, M12, M21, M22, M31, M32 components of the color.
		/// <para>There must be at least 6 elements available or undefined behaviour will occur.</para></param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="ptrValues"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="ptrValues"/> contains more or less than 6 elements.</exception>
		public unsafe Matrix3x2D(double* ptrValues)
		{
			if (ptrValues == null)
				throw new ArgumentNullException("ptrValues");

			fixed (double* dst = Values)
				Unsafe.CopyBlock(ptrValues, dst, (sizeof(double) * 6));
		}

        /// <summary> Creates a string representation of the matrix.</summary>
        /// <returns>A string representation of the matrix.</returns>
        public override string ToString()
        {
            return string.Format("[{0}, {1}] [{2}, {3}] [{4}, {5}]", 
            M11, M12, M21, M22, M31, M32);
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
            return string.Format("[{0}, {1}] [{2}, {3}] [{4}, {5}]", cc, 
            M11.ToString(format, cc), M12.ToString(format, cc), M21.ToString(format, cc), M22.ToString(format, cc), M31.ToString(format, cc), M32.ToString(format, cc));
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
            return string.Format("[{0}, {1}] [{2}, {3}] [{4}, {5}]", 
            M11.ToString(formatProvider), M12.ToString(formatProvider), M21.ToString(formatProvider), M22.ToString(formatProvider), M31.ToString(formatProvider), M32.ToString(formatProvider));
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
            return string.Format("[{0}, {1}] [{2}, {3}] [{4}, {5}]", cc, 
            M11.ToString(format, formatProvider), M12.ToString(format, formatProvider), M21.ToString(format, formatProvider), M22.ToString(format, formatProvider), M31.ToString(format, formatProvider), M32.ToString(format, formatProvider));
        }

        /// <summary>
        /// Determines whether the specified <see cref="Matrix3x2D"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Matrix3x2D"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="Matrix3x2D"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(ref Matrix3x2D other)
        {
            return MathHelper.NearEqual(other.M11, M11)
            && MathHelper.NearEqual(other.M12, M12)
            && MathHelper.NearEqual(other.M21, M21)
            && MathHelper.NearEqual(other.M22, M22)
            && MathHelper.NearEqual(other.M31, M31)
            && MathHelper.NearEqual(other.M32, M32);
        }

        /// <summary>
        /// Determines whether the specified <see cref="Matrix3x2D"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Matrix3x2D"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="Matrix3x2D"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Matrix3x2D other)
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
            if (value is Matrix3x2D mat)
                return Equals(ref mat);

            return false;
        }

        /// <summary>
        /// Creates an array containing the elements of the <see cref="Matrix3x2D"/>.
        /// </summary>
        /// <returns>A 6-element array containing the components of the matrix.</returns>
        public unsafe double[] ToArray()
        {
            return [M11, M12, M21, M22, M31, M32];
        }

        /// <summary>
        /// Transposes the current <see cref="Matrix3x2D"/> and outputs it to <paramref name="result"/>.
        /// </summary>
        /// <param name="result"></param>
        public void Transpose(out Matrix2x3D result)
        {
            Transpose(ref this, out result);
        }
      
        /// <summary>
        /// Transposes the current <see cref="Matrix3x2D"/> in-place.
        /// </summary>
        public Matrix2x3D Transpose()
        {
            Transpose(ref this, out Matrix2x3D result);
            return result;
        }
        
        /// <summary>
        /// Calculates the transposed <see cref="Matrix2x3D"/> of the specified <see cref="Matrix3x2D"/>.
        /// </summary>
        /// <param name="matrix">The <see cref="Matrix3x2D"/> whose transpose is to be calculated.</param>
        /// <param name="result">A <see cref="Matrix2x3D"/> containing the transposed <see cref="Matrix3x2D"/></param>
        public static void Transpose(ref Matrix3x2D matrix, out Matrix2x3D result)
        {
            Unsafe.SkipInit(out result);
            result.M11 = matrix.M11;
            result.M12 = matrix.M12;
            result.M13 = matrix.M21;
            result.M21 = matrix.M22;
            result.M22 = matrix.M31;
            result.M23 = matrix.M32;
        }

        /// <summary>
        /// Calculates the transpose of the specified <see cref="Matrix3x2D"/>.
        /// </summary>
        /// <param name="value">The <see cref="Matrix3x2D"/> whose transpose is to be calculated.</param>
        /// <returns>The transpose of the specified <see cref="Matrix3x2D"/>.</returns>
        public static Matrix2x3D Transpose(Matrix3x2D value)
        {
            Transpose(ref value, out Matrix2x3D result);
            return result;
        }

        /// <summary>
        /// Performs a linear interpolation between two <see cref="Matrix3x2D"/>.
        /// </summary>
        /// <param name="start">Start <see cref="Matrix3x2D"/>.</param>
        /// <param name="end">End <see cref="Matrix3x2D"/>.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <param name="result">When the method completes, contains the linear interpolation of the two <see cref="Matrix3x2D"/> matrices.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        public static void Lerp(ref Matrix3x2D start, ref Matrix3x2D end, double amount, out Matrix3x2D result)
        {
            result.M11 = MathHelper.Lerp(start.M11, end.M11, amount);
            result.M12 = MathHelper.Lerp(start.M12, end.M12, amount);
            result.M21 = MathHelper.Lerp(start.M21, end.M21, amount);
            result.M22 = MathHelper.Lerp(start.M22, end.M22, amount);
            result.M31 = MathHelper.Lerp(start.M31, end.M31, amount);
            result.M32 = MathHelper.Lerp(start.M32, end.M32, amount);
        }

        /// <summary>
        /// Performs a cubic interpolation between two <see cref="Matrix3x2D"/>.
        /// </summary>
        /// <param name="start">Start <see cref="Matrix3x2D"/>.</param>
        /// <param name="end">End <see cref="Matrix3x2D"/>.</param>
        /// <param name="amount">Value between 0D and 1D indicating the weight of <paramref name="end"/>.</param>
        /// <param name="result">When the method completes, contains the cubic interpolation of the two <see cref="Matrix3x2D"/> matrices.</param>
        public static void SmoothStep(ref Matrix3x2D start, ref Matrix3x2D end, double amount, out Matrix3x2D result)
        {
            amount = MathHelper.SmoothStep(amount);
            Lerp(ref start, ref end, amount, out result);
        }

        /// <summary>
        /// Performs a cubic interpolation between two matrices.
        /// </summary>
        /// <param name="start">Start <see cref="Matrix3x2D"/>.</param>
        /// <param name="end">End <see cref="Matrix3x2D"/>.</param>
        /// <param name="amount">Value between 0D and 1D indicating the weight of <paramref name="end"/>.</param>
        /// <returns>The cubic interpolation of the two matrices.</returns>
        public static Matrix3x2D SmoothStep(Matrix3x2D start, Matrix3x2D end, double amount)
        {
            SmoothStep(ref start, ref end, amount, out Matrix3x2D result);
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
        public static Matrix3x2D Lerp(Matrix3x2D start, Matrix3x2D end, double amount)
        {
            Lerp(ref start, ref end, amount, out Matrix3x2D result);
            return result;
        }

		/// <summary> Gets or sets the component at the specified index. </summary>
		/// <value>The value of the <see cref="Matrix3x2D"/> component, depending on the index.</value>
		/// <param name="index">The index of the index component to access, ranging from 0 to 2, inclusive.</param>
		/// <returns>The value of the component at the specified index value provided.</returns>
		/// <exception cref="IndexOutOfRangeException">Thrown if the index is out of range.</exception>
		public unsafe double this[int index]
		{
			get
			{
				if(index < 0 || index > 5)
					throw new IndexOutOfRangeException("index for Matrix3x2D must be between 0 and 5, inclusive.");

				return Values[index];
			}
			set
			{
				if(index < 0 || index > 5)
					throw new IndexOutOfRangeException("index for Matrix3x2D must be between 0 and 5, inclusive.");

				Values[index] = value;
			}
		}

		/// <summary> Gets or sets the component at the specified index. </summary>
		/// <value>The value of the <see cref="Matrix3x2D"/> component, depending on the index.</value>
		/// <param name="index">The index of the index component to access, ranging from 0 to 2, inclusive.</param>
		/// <returns>The value of the component at the specified index value provided.</returns>
		/// <exception cref="IndexOutOfRangeException">Thrown if the index is out of range.</exception>
		public unsafe double this[uint index]
		{
			get
			{
				if(index > 5)
					throw new IndexOutOfRangeException("index for Matrix3x2D must be between 0 and 5, inclusive.");

				return Values[index];
			}
			set
			{
				if(index > 5)
					throw new IndexOutOfRangeException("index for Matrix3x2D must be between 0 and 5, inclusive.");

				Values[index] = value;
			}
		}

		/// <summary> Gets or sets the component at the specified index. </summary>
		/// <value>The value of the <see cref="Matrix3x2D"/> component, depending on the index.</value>
		/// <param name="row">The index of the row component to access, ranging from 0 to 2, inclusive.</param>
		/// <param name="column">The index of the column component to access, ranging from 0 to 1, inclusive.</param>
		/// <returns>The value of the component at the specified index values provided.</returns>
		/// <exception cref="IndexOutOfRangeException">Thrown if the index is out of range.</exception>
		public unsafe double this[int row, int column]
		{
			get
			{
				if(row < 0 || row > 5)
					throw new IndexOutOfRangeException("row for Matrix3x2D must be between 0 and 5, inclusive.");

				if(column < 0 || column > 5)
					throw new IndexOutOfRangeException("column for Matrix3x2D must be between 0 and 5, inclusive.");

				return Values[(row * 2) + column];
			}
			set
			{
				if(row < 0 || row > 5)
					throw new IndexOutOfRangeException("row for Matrix3x2D must be between 0 and 5, inclusive.");

				if(column < 0 || column > 5)
					throw new IndexOutOfRangeException("column for Matrix3x2D must be between 0 and 5, inclusive.");

				Values[(row * 2) + column] = value;
			}
		}

		/// <summary> Gets or sets the component at the specified index. </summary>
		/// <value>The value of the <see cref="Matrix3x2D"/> component, depending on the index.</value>
		/// <param name="row">The index of the row component to access, ranging from 0 to 2, inclusive.</param>
		/// <param name="column">The index of the column component to access, ranging from 0 to 1, inclusive.</param>
		/// <returns>The value of the component at the specified index values provided.</returns>
		/// <exception cref="IndexOutOfRangeException">Thrown if the index is out of range.</exception>
		public unsafe double this[uint row, uint column]
		{
			get
			{
				if(row > 5)
					throw new IndexOutOfRangeException("row for Matrix3x2D must be between 0 and 5, inclusive.");

				if(column > 5)
					throw new IndexOutOfRangeException("column for Matrix3x2D must be between 0 and 5, inclusive.");

				return Values[(row * 2) + column];
			}
			set
			{
				if(row > 5)
					throw new IndexOutOfRangeException("row for Matrix3x2D must be between 0 and 5, inclusive.");

				if(column > 5)
					throw new IndexOutOfRangeException("column for Matrix3x2D must be between 0 and 5, inclusive.");

				Values[(row * 2) + column] = value;
			}
		}


        /// <summary>
        /// Returns a hash code for the current <see cref="Matrix3x2D"/>.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="Matrix3x2D"/>, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = M11.GetHashCode();
                    hashCode = (hashCode * 397) ^ M12.GetHashCode();
                    hashCode = (hashCode * 397) ^ M21.GetHashCode();
                    hashCode = (hashCode * 397) ^ M22.GetHashCode();
                    hashCode = (hashCode * 397) ^ M31.GetHashCode();
                    hashCode = (hashCode * 397) ^ M32.GetHashCode();
                    
                return hashCode;
            }
        }

        public static bool operator ==(Matrix3x2D matrix1, Matrix3x2D matrix2)
        {
            return matrix1.Equals(ref matrix2);
        }

        public static bool operator !=(Matrix3x2D matrix1, Matrix3x2D matrix2)
        {
            return !matrix1.Equals(ref matrix2);
        }

#region Add operators
		///<summary>Performs a add operation on two <see cref="Matrix3x2D"/>.</summary>
		///<param name="a">The first <see cref="Matrix3x2D"/> to add.</param>
		///<param name="b">The second <see cref="Matrix3x2D"/> to add.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Add(ref Matrix3x2D a, ref Matrix3x2D b, out Matrix3x2D result)
		{
			result.M11 = a.M11 + b.M11;
			result.M12 = a.M12 + b.M12;
			result.M21 = a.M21 + b.M21;
			result.M22 = a.M22 + b.M22;
			result.M31 = a.M31 + b.M31;
			result.M32 = a.M32 + b.M32;
		}

		///<summary>Performs a add operation on two <see cref="Matrix3x2D"/>.</summary>
		///<param name="a">The first <see cref="Matrix3x2D"/> to add.</param>
		///<param name="b">The second <see cref="Matrix3x2D"/> to add.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Matrix3x2D operator +(Matrix3x2D a, Matrix3x2D b)
		{
			Add(ref a, ref b, out Matrix3x2D result);
			return result;
		}

		///<summary>Performs a add operation on a <see cref="Matrix3x2D"/> and a <see cref="double"/>.</summary>
		///<param name="a">The <see cref="Matrix3x2D"/> to add.</param>
		///<param name="b">The <see cref="double"/> to add.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Add(ref Matrix3x2D a, double b, out Matrix3x2D result)
		{
			result.M11 = a.M11 + b;
			result.M12 = a.M12 + b;
			result.M21 = a.M21 + b;
			result.M22 = a.M22 + b;
			result.M31 = a.M31 + b;
			result.M32 = a.M32 + b;
		}

		///<summary>Performs a add operation on a <see cref="Matrix3x2D"/> and a <see cref="double"/>.</summary>
		///<param name="a">The <see cref="Matrix3x2D"/> to add.</param>
		///<param name="b">The <see cref="double"/> to add.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Matrix3x2D operator +(Matrix3x2D a, double b)
		{
			Add(ref a, b, out Matrix3x2D result);
			return result;
		}

		///<summary>Performs a add operation on a <see cref="double"/> and a <see cref="Matrix3x2D"/>.</summary>
		///<param name="a">The <see cref="double"/> to add.</param>
		///<param name="b">The <see cref="Matrix3x2D"/> to add.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Matrix3x2D operator +(double a, Matrix3x2D b)
		{
			Add(ref b, a, out Matrix3x2D result);
			return result;
		}


        /// <summary>
        /// Assert a <see cref="Matrix3x2D"/> (return it unchanged).
        /// </summary>
        /// <param name="value">The <see cref="Matrix3x2D"/> to assert (unchanged).</param>
        /// <returns>The asserted (unchanged) <see cref="Matrix3x2D"/>.</returns>
        public static Matrix3x2D operator +(Matrix3x2D value)
        {
            return value;
        }
#endregion

#region Subtract operators
		///<summary>Performs a subtract operation on two <see cref="Matrix3x2D"/>.</summary>
		///<param name="a">The first <see cref="Matrix3x2D"/> to subtract.</param>
		///<param name="b">The second <see cref="Matrix3x2D"/> to subtract.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Subtract(ref Matrix3x2D a, ref Matrix3x2D b, out Matrix3x2D result)
		{
			result.M11 = a.M11 - b.M11;
			result.M12 = a.M12 - b.M12;
			result.M21 = a.M21 - b.M21;
			result.M22 = a.M22 - b.M22;
			result.M31 = a.M31 - b.M31;
			result.M32 = a.M32 - b.M32;
		}

		///<summary>Performs a subtract operation on two <see cref="Matrix3x2D"/>.</summary>
		///<param name="a">The first <see cref="Matrix3x2D"/> to subtract.</param>
		///<param name="b">The second <see cref="Matrix3x2D"/> to subtract.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Matrix3x2D operator -(Matrix3x2D a, Matrix3x2D b)
		{
			Subtract(ref a, ref b, out Matrix3x2D result);
			return result;
		}

		///<summary>Performs a subtract operation on a <see cref="Matrix3x2D"/> and a <see cref="double"/>.</summary>
		///<param name="a">The <see cref="Matrix3x2D"/> to subtract.</param>
		///<param name="b">The <see cref="double"/> to subtract.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Subtract(ref Matrix3x2D a, double b, out Matrix3x2D result)
		{
			result.M11 = a.M11 - b;
			result.M12 = a.M12 - b;
			result.M21 = a.M21 - b;
			result.M22 = a.M22 - b;
			result.M31 = a.M31 - b;
			result.M32 = a.M32 - b;
		}

		///<summary>Performs a subtract operation on a <see cref="Matrix3x2D"/> and a <see cref="double"/>.</summary>
		///<param name="a">The <see cref="Matrix3x2D"/> to subtract.</param>
		///<param name="b">The <see cref="double"/> to subtract.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Matrix3x2D operator -(Matrix3x2D a, double b)
		{
			Subtract(ref a, b, out Matrix3x2D result);
			return result;
		}

		///<summary>Performs a subtract operation on a <see cref="double"/> and a <see cref="Matrix3x2D"/>.</summary>
		///<param name="a">The <see cref="double"/> to subtract.</param>
		///<param name="b">The <see cref="Matrix3x2D"/> to subtract.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Matrix3x2D operator -(double a, Matrix3x2D b)
		{
			Subtract(ref b, a, out Matrix3x2D result);
			return result;
		}


        /// <summary>
        /// Negates a <see cref="Matrix3x2D"/>.
        /// </summary>
        /// <param name="value">The matrix to be negated.</param>
        /// <param name="result">When the method completes, contains the negated <see cref="Matrix3x2D"/>.</param>
        public static void Negate(ref Matrix3x2D value, out Matrix3x2D result)
        {
             result.M11 = -value.M11;
             result.M12 = -value.M12;
             result.M21 = -value.M21;
             result.M22 = -value.M22;
             result.M31 = -value.M31;
             result.M32 = -value.M32;
         }

        /// <summary>
        /// Negates a <see cref="Matrix3x2D"/>.
        /// </summary>
        /// <param name="value">The <see cref="Matrix3x2D"/> to be negated.</param>
        /// <returns>The negated <see cref="Matrix3x2D"/>.</returns>
        public static Matrix3x2D Negate(Matrix3x2D value)
        {
            Matrix3x2D result;
            Negate(ref value, out result);
            return result;
        }

        /// <summary>
        /// Negates a <see cref="Matrix3x2D"/>.
        /// </summary>
        /// <param name="value">The <see cref="Matrix3x2D"/> to negate.</param>
        /// <returns>The negated <see cref="Matrix3x2D"/>.</returns>
        public static Matrix3x2D operator -(Matrix3x2D value)
        {
            Negate(ref value, out Matrix3x2D result);
            return result;
        }
#endregion

#region Multiply operators
        /// <summary>
        /// Scales a <see cref="Matrix3x2D"/> by the given scalar value.
        /// </summary>
        /// <param name="matrix">The <see cref="Matrix3x2D"/> to scale.</param>
        /// <param name="scalar">The scalar value by which to scale.</param>
        /// <param name="result">When the method completes, contains the scaled <see cref="Matrix3x2D"/>.</param>
        public static void Multiply(ref Matrix3x2D matrix, double scalar, out Matrix3x2D result)
        {
			result.M11 = matrix.M11 * scalar;
			result.M12 = matrix.M12 * scalar;
			result.M21 = matrix.M21 * scalar;
			result.M22 = matrix.M22 * scalar;
			result.M31 = matrix.M31 * scalar;
			result.M32 = matrix.M32 * scalar;
        }

        /// <summary>
        /// Scales a <see cref="Matrix3x2D"/> by a given value.
        /// </summary>
        /// <param name="matrix">The <see cref="Matrix3x2D"/> to scale.</param>
        /// <param name="scalar">The scalar value by which to scale.</param>
        /// <returns>The scaled matrix.</returns>
        public static Matrix3x2D operator *(Matrix3x2D matrix, double scalar)
        {
            Multiply(ref matrix, scalar, out Matrix3x2D result);
            return result;
        }
                
        /// <summary>
        /// Scales a <see cref="Matrix3x2D"/> by a given value.
        /// </summary>
        /// <param name="scalar">The scalar value by which to scale.</param>
        /// <param name="matrix">The <see cref="Matrix3x2D"/> to scale.</param>
        /// <returns>The scaled matrix.</returns>

        public static Matrix3x2D operator *(double scalar, Matrix3x2D matrix)
        {
            Multiply(ref matrix, scalar, out Matrix3x2D result);
            return result;
        }
#endregion

#region division operators
/// <summary>
        /// Scales a <see cref="Matrix3x2D"/> by the given scalar value.
        /// </summary>
        /// <param name="matrix">The <see cref="Matrix3x2D"/> to scale.</param>
        /// <param name="scalar">The amount by which to scale.</param>
        /// <param name="result">When the method completes, contains the scaled <see cref="Matrix3x2D"/>.</param>
        public static void Divide(ref Matrix3x2D matrix, double scalar, out Matrix3x2D result)
        {
            double inv = 1D / scalar;
			result.M11 = matrix.M11 * inv;
			result.M12 = matrix.M12 * inv;
			result.M21 = matrix.M21 * inv;
			result.M22 = matrix.M22 * inv;
			result.M31 = matrix.M31 * inv;
			result.M32 = matrix.M32 * inv;
        }

        /// <summary>
        /// Scales a <see cref="Matrix3x2D"/> by the given scalar value.
        /// </summary>
        /// <param name="matrix">The <see cref="Matrix3x2D"/> to scale.</param>
        /// <param name="scalar">The amount by which to scale.</param>
        /// <returns>The scaled matrix.</returns>
        public static Matrix3x2D Divide(Matrix3x2D matrix, double scalar)
        {
            Divide(ref matrix, scalar, out Matrix3x2D result);
            return result;
        }

                /// <summary>
        /// Determines the quotient of two matrices.
        /// </summary>
        /// <param name="left">The first <see cref="Matrix3x2D"/> to divide.</param>
        /// <param name="right">The second <see cref="Matrix3x2D"/> to divide.</param>
        /// <param name="result">When the method completes, contains the quotient of the two <see cref="Matrix3x2D"/> matrices.</param>
        public static void Divide(ref Matrix3x2D left, ref Matrix3x2D right, out Matrix3x2D result)
        {
			result.M11 = left.M11 / right.M11;
			result.M12 = left.M12 / right.M12;
			result.M21 = left.M21 / right.M21;
			result.M22 = left.M22 / right.M22;
			result.M31 = left.M31 / right.M31;
			result.M32 = left.M32 / right.M32;
        }

        /// <summary>
        /// Scales a <see cref="Matrix3x2D"/> by a given scalar value.
        /// </summary>
        /// <param name="matrix">The matrix to scale.</param>
        /// <param name="scalar">The amount by which to scale.</param>
        /// <returns>The scaled matrix.</returns>
        public static Matrix3x2D operator /(Matrix3x2D matrix, double scalar)
        {
            Divide(ref matrix, scalar, out Matrix3x2D result);
            return result;
        }

        /// <summary>
        /// Divides two <see cref="Matrix3x2D"/> matrices.
        /// </summary>
        /// <param name="left">The first <see cref="Matrix3x2D"/> to divide.</param>
        /// <param name="right">The second <see cref="Matrix3x2D"/> to divide.</param>
        /// <returns>The quotient of the two <see cref="Matrix3x2D"/> matrices.</returns>
        public static Matrix3x2D operator /(Matrix3x2D left, Matrix3x2D right)
        {
            Divide(ref left, ref right, out Matrix3x2D result);
            return result;
        }
#endregion
    }
}

