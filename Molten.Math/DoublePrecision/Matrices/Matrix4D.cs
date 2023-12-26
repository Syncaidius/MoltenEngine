using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Molten.DoublePrecision
{
    /// <summary>Represents a double-precision 4x4 Matrix. Contains only scale and rotation.</summary>
    [StructLayout(LayoutKind.Explicit)]
    [DataContract]
	public partial struct Matrix4D : IEquatable<Matrix4D>, IFormattable, ITransposableMatrix<Matrix4D, Matrix4D>, IMatrix<double>
    {
        /// <summary>
        /// A single-precision <see cref="Matrix4D"/> with values intialized to the identity of a 2 x 2 matrix
        /// </summary>
        public static readonly Matrix4D Identity = new Matrix4D() 
        { 
            M11 = 1D, 
            M22 = 1D, 
            M33 = 1D, 
            M44 = 1D, 
        };

        public static readonly int ComponentCount = 16;

        public static readonly int RowCount = 4;

        public static readonly int ColumnCount = 4;

        /// <summary>A <see cref="Matrix4D"/> will all of its components set to 0D.</summary>
        public static readonly Matrix4D Zero = new Matrix4D();

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

		/// <summary>The value at row 1, column 3 of the matrix.</summary>
		[DataMember]
		[FieldOffset(16)]
		public double M13;

		/// <summary>The value at row 1, column 4 of the matrix.</summary>
		[DataMember]
		[FieldOffset(24)]
		public double M14;

		/// <summary>The value at row 2, column 1 of the matrix.</summary>
		[DataMember]
		[FieldOffset(32)]
		public double M21;

		/// <summary>The value at row 2, column 2 of the matrix.</summary>
		[DataMember]
		[FieldOffset(40)]
		public double M22;

		/// <summary>The value at row 2, column 3 of the matrix.</summary>
		[DataMember]
		[FieldOffset(48)]
		public double M23;

		/// <summary>The value at row 2, column 4 of the matrix.</summary>
		[DataMember]
		[FieldOffset(56)]
		public double M24;

		/// <summary>The value at row 3, column 1 of the matrix.</summary>
		[DataMember]
		[FieldOffset(64)]
		public double M31;

		/// <summary>The value at row 3, column 2 of the matrix.</summary>
		[DataMember]
		[FieldOffset(72)]
		public double M32;

		/// <summary>The value at row 3, column 3 of the matrix.</summary>
		[DataMember]
		[FieldOffset(80)]
		public double M33;

		/// <summary>The value at row 3, column 4 of the matrix.</summary>
		[DataMember]
		[FieldOffset(88)]
		public double M34;

		/// <summary>The value at row 4, column 1 of the matrix.</summary>
		[DataMember]
		[FieldOffset(96)]
		public double M41;

		/// <summary>The value at row 4, column 2 of the matrix.</summary>
		[DataMember]
		[FieldOffset(104)]
		public double M42;

		/// <summary>The value at row 4, column 3 of the matrix.</summary>
		[DataMember]
		[FieldOffset(112)]
		public double M43;

		/// <summary>The value at row 4, column 4 of the matrix.</summary>
		[DataMember]
		[FieldOffset(120)]
		public double M44;

		/// <summary>A fixed array mapped to the same memory space as the individual <see cref="Matrix4D"/> components.</summary>
		[IgnoreDataMember]
		[FieldOffset(0)]
		public unsafe fixed double Values[16];


        /// <summary> Row 1 of the current <see cref="Matrix4D"/>.</summary>
        /// <returns>A <see cref="Vector4D"/> containing the row values.</returns>
        public Vector4D Row1
        {
            get => new Vector4D(M11, M12, M13, M14);
            set => (M11, M12, M13, M14) = value;
        }

        /// <summary> Row 2 of the current <see cref="Matrix4D"/>.</summary>
        /// <returns>A <see cref="Vector4D"/> containing the row values.</returns>
        public Vector4D Row2
        {
            get => new Vector4D(M21, M22, M23, M24);
            set => (M21, M22, M23, M24) = value;
        }

        /// <summary> Row 3 of the current <see cref="Matrix4D"/>.</summary>
        /// <returns>A <see cref="Vector4D"/> containing the row values.</returns>
        public Vector4D Row3
        {
            get => new Vector4D(M31, M32, M33, M34);
            set => (M31, M32, M33, M34) = value;
        }

        /// <summary> Row 4 of the current <see cref="Matrix4D"/>.</summary>
        /// <returns>A <see cref="Vector4D"/> containing the row values.</returns>
        public Vector4D Row4
        {
            get => new Vector4D(M41, M42, M43, M44);
            set => (M41, M42, M43, M44) = value;
        }


        /// <summary> Column 1 of the current <see cref="Matrix4D"/>.</summary>
        /// <returns>A <see cref="Vector4D"/> containing the column values.</returns>
        public Vector4D Column1
        {
            get => new Vector4D(M11, M21, M31, M41);
            set => (M11, M21, M31, M41) = value;
        }

        /// <summary> Column 2 of the current <see cref="Matrix4D"/>.</summary>
        /// <returns>A <see cref="Vector4D"/> containing the column values.</returns>
        public Vector4D Column2
        {
            get => new Vector4D(M12, M22, M32, M42);
            set => (M12, M22, M32, M42) = value;
        }

        /// <summary> Column 3 of the current <see cref="Matrix4D"/>.</summary>
        /// <returns>A <see cref="Vector4D"/> containing the column values.</returns>
        public Vector4D Column3
        {
            get => new Vector4D(M13, M23, M33, M43);
            set => (M13, M23, M33, M43) = value;
        }

        /// <summary> Column 4 of the current <see cref="Matrix4D"/>.</summary>
        /// <returns>A <see cref="Vector4D"/> containing the column values.</returns>
        public Vector4D Column4
        {
            get => new Vector4D(M14, M24, M34, M44);
            set => (M14, M24, M34, M44) = value;
        }


		/// <summary>
		/// Initializes a new instance of <see cref="Matrix4D"/>.
		/// </summary>
		/// <param name="m11">The value to assign to row 1, column 1 of the matrix.</param>
		/// <param name="m12">The value to assign to row 1, column 2 of the matrix.</param>
		/// <param name="m13">The value to assign to row 1, column 3 of the matrix.</param>
		/// <param name="m14">The value to assign to row 1, column 4 of the matrix.</param>
		/// <param name="m21">The value to assign to row 2, column 1 of the matrix.</param>
		/// <param name="m22">The value to assign to row 2, column 2 of the matrix.</param>
		/// <param name="m23">The value to assign to row 2, column 3 of the matrix.</param>
		/// <param name="m24">The value to assign to row 2, column 4 of the matrix.</param>
		/// <param name="m31">The value to assign to row 3, column 1 of the matrix.</param>
		/// <param name="m32">The value to assign to row 3, column 2 of the matrix.</param>
		/// <param name="m33">The value to assign to row 3, column 3 of the matrix.</param>
		/// <param name="m34">The value to assign to row 3, column 4 of the matrix.</param>
		/// <param name="m41">The value to assign to row 4, column 1 of the matrix.</param>
		/// <param name="m42">The value to assign to row 4, column 2 of the matrix.</param>
		/// <param name="m43">The value to assign to row 4, column 3 of the matrix.</param>
		/// <param name="m44">The value to assign to row 4, column 4 of the matrix.</param>
		public Matrix4D(double m11, double m12, double m13, double m14, double m21, double m22, double m23, double m24, double m31, double m32, double m33, double m34, double m41, double m42, double m43, double m44)
		{
			M11 = m11;
			M12 = m12;
			M13 = m13;
			M14 = m14;
			M21 = m21;
			M22 = m22;
			M23 = m23;
			M24 = m24;
			M31 = m31;
			M32 = m32;
			M33 = m33;
			M34 = m34;
			M41 = m41;
			M42 = m42;
			M43 = m43;
			M44 = m44;
		}
		/// <summary>Initializes a new instance of <see cref="Matrix4D"/>.</summary>
		/// <param name="value">The value that will be assigned to all components.</param>
		public Matrix4D(double value)
		{
			M11 = value;
			M12 = value;
			M13 = value;
			M14 = value;
			M21 = value;
			M22 = value;
			M23 = value;
			M24 = value;
			M31 = value;
			M32 = value;
			M33 = value;
			M34 = value;
			M41 = value;
			M42 = value;
			M43 = value;
			M44 = value;
		}
		/// <summary>Initializes a new instance of <see cref="Matrix4D"/> from an array.</summary>
		/// <param name="values">The values to assign to the M11, M12, M13, M14, M21, M22, M23, M24, M31, M32, M33, M34, M41, M42, M43, M44 components of the color. This must be an array with at least 16 elements.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than 16 elements.</exception>
		public unsafe Matrix4D(double[] values)
		{
			if (values == null)
				throw new ArgumentNullException("values");
			if (values.Length < 16)
				throw new ArgumentOutOfRangeException("values", "There must be at least 16 input values for Matrix4D.");

			fixed (double* src = values)
			{
				fixed (double* dst = Values)
					Unsafe.CopyBlock(src, dst, (sizeof(double) * 16));
			}
		}
		/// <summary>Initializes a new instance of <see cref="Matrix4D"/> from a span.</summary>
		/// <param name="values">The values to assign to the M11, M12, M13, M14, M21, M22, M23, M24, M31, M32, M33, M34, M41, M42, M43, M44 components of the color. This must be an array with at least 16 elements.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than 16 elements.</exception>
		public Matrix4D(Span<double> values)
		{
			if (values == null)
				throw new ArgumentNullException("values");
			if (values.Length < 16)
				throw new ArgumentOutOfRangeException("values", "There must be at least 16 input values for Matrix4D.");

			M11 = values[0];
			M12 = values[1];
			M13 = values[2];
			M14 = values[3];
			M21 = values[4];
			M22 = values[5];
			M23 = values[6];
			M24 = values[7];
			M31 = values[8];
			M32 = values[9];
			M33 = values[10];
			M34 = values[11];
			M41 = values[12];
			M42 = values[13];
			M43 = values[14];
			M44 = values[15];
		}
		/// <summary>Initializes a new instance of <see cref="Matrix4D"/> from a an unsafe pointer.</summary>
		/// <param name="ptrValues">The values to assign to the M11, M12, M13, M14, M21, M22, M23, M24, M31, M32, M33, M34, M41, M42, M43, M44 components of the color.
		/// <para>There must be at least 16 elements available or undefined behaviour will occur.</para></param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="ptrValues"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="ptrValues"/> contains more or less than 16 elements.</exception>
		public unsafe Matrix4D(double* ptrValues)
		{
			if (ptrValues == null)
				throw new ArgumentNullException("ptrValues");

			fixed (double* dst = Values)
				Unsafe.CopyBlock(ptrValues, dst, (sizeof(double) * 16));
		}

        /// <summary> Creates a string representation of the matrix.</summary>
        /// <returns>A string representation of the matrix.</returns>
        public override string ToString()
        {
            return string.Format("[{0}, {1}, {2}, {3}] [{4}, {5}, {6}, {7}] [{8}, {9}, {10}, {11}] [{12}, {13}, {14}, {15}]", 
            M11, M12, M13, M14, M21, M22, M23, M24, M31, M32, M33, M34, M41, M42, M43, M44);
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
            return string.Format("[{0}, {1}, {2}, {3}] [{4}, {5}, {6}, {7}] [{8}, {9}, {10}, {11}] [{12}, {13}, {14}, {15}]", cc, 
            M11.ToString(format, cc), M12.ToString(format, cc), M13.ToString(format, cc), M14.ToString(format, cc), M21.ToString(format, cc), M22.ToString(format, cc), M23.ToString(format, cc), M24.ToString(format, cc), M31.ToString(format, cc), M32.ToString(format, cc), M33.ToString(format, cc), M34.ToString(format, cc), M41.ToString(format, cc), M42.ToString(format, cc), M43.ToString(format, cc), M44.ToString(format, cc));
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
            return string.Format("[{0}, {1}, {2}, {3}] [{4}, {5}, {6}, {7}] [{8}, {9}, {10}, {11}] [{12}, {13}, {14}, {15}]", 
            M11.ToString(formatProvider), M12.ToString(formatProvider), M13.ToString(formatProvider), M14.ToString(formatProvider), M21.ToString(formatProvider), M22.ToString(formatProvider), M23.ToString(formatProvider), M24.ToString(formatProvider), M31.ToString(formatProvider), M32.ToString(formatProvider), M33.ToString(formatProvider), M34.ToString(formatProvider), M41.ToString(formatProvider), M42.ToString(formatProvider), M43.ToString(formatProvider), M44.ToString(formatProvider));
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
            return string.Format("[{0}, {1}, {2}, {3}] [{4}, {5}, {6}, {7}] [{8}, {9}, {10}, {11}] [{12}, {13}, {14}, {15}]", cc, 
            M11.ToString(format, formatProvider), M12.ToString(format, formatProvider), M13.ToString(format, formatProvider), M14.ToString(format, formatProvider), M21.ToString(format, formatProvider), M22.ToString(format, formatProvider), M23.ToString(format, formatProvider), M24.ToString(format, formatProvider), M31.ToString(format, formatProvider), M32.ToString(format, formatProvider), M33.ToString(format, formatProvider), M34.ToString(format, formatProvider), M41.ToString(format, formatProvider), M42.ToString(format, formatProvider), M43.ToString(format, formatProvider), M44.ToString(format, formatProvider));
        }

        /// <summary>
        /// Determines whether the specified <see cref="Matrix4D"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Matrix4D"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="Matrix4D"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(ref Matrix4D other)
        {
            return MathHelper.NearEqual(other.M11, M11)
            && MathHelper.NearEqual(other.M12, M12)
            && MathHelper.NearEqual(other.M13, M13)
            && MathHelper.NearEqual(other.M14, M14)
            && MathHelper.NearEqual(other.M21, M21)
            && MathHelper.NearEqual(other.M22, M22)
            && MathHelper.NearEqual(other.M23, M23)
            && MathHelper.NearEqual(other.M24, M24)
            && MathHelper.NearEqual(other.M31, M31)
            && MathHelper.NearEqual(other.M32, M32)
            && MathHelper.NearEqual(other.M33, M33)
            && MathHelper.NearEqual(other.M34, M34)
            && MathHelper.NearEqual(other.M41, M41)
            && MathHelper.NearEqual(other.M42, M42)
            && MathHelper.NearEqual(other.M43, M43)
            && MathHelper.NearEqual(other.M44, M44);
        }

        /// <summary>
        /// Determines whether the specified <see cref="Matrix4D"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Matrix4D"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="Matrix4D"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Matrix4D other)
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
            if (value is Matrix4D mat)
                return Equals(ref mat);

            return false;
        }

        /// <summary>
        /// Creates an array containing the elements of the <see cref="Matrix4D"/>.
        /// </summary>
        /// <returns>A 16-element array containing the components of the matrix.</returns>
        public unsafe double[] ToArray()
        {
            return [M11, M12, M13, M14, M21, M22, M23, M24, M31, M32, M33, M34, M41, M42, M43, M44];
        }

        /// <summary>
        /// Transposes the current <see cref="Matrix4D"/> and outputs it to <paramref name="result"/>.
        /// </summary>
        /// <param name="result"></param>
        public void Transpose(out Matrix4D result)
        {
            Transpose(ref this, out result);
        }
      
        /// <summary>
        /// Transposes the current <see cref="Matrix4D"/> in-place.
        /// </summary>
        public Matrix4D Transpose()
        {
            Transpose(ref this, out Matrix4D result);
            return result;
        }
        

        /// <summary>
        /// Calculates the transpose of the specified <see cref="Matrix4D"/>.
        /// </summary>
        /// <param name="matrix">The <see cref="Matrix4D"/> whose transpose is to be calculated.</param>
        /// <param name="result">When the method completes, contains the transpose of the specified matrix.</param>
        public static void Transpose(ref Matrix4D matrix, out Matrix4D result)
        {
            Unsafe.SkipInit(out result);
            result.M11 = matrix.M11;
            result.M12 = matrix.M21;
            result.M13 = matrix.M31;
            result.M14 = matrix.M41;
            result.M21 = matrix.M12;
            result.M22 = matrix.M22;
            result.M23 = matrix.M32;
            result.M24 = matrix.M42;
            result.M31 = matrix.M13;
            result.M32 = matrix.M23;
            result.M33 = matrix.M33;
            result.M34 = matrix.M43;
            result.M41 = matrix.M14;
            result.M42 = matrix.M24;
            result.M43 = matrix.M34;
            result.M44 = matrix.M44;
        }

        /// <summary>
        /// Calculates the transpose of the specified <see cref="Matrix4D"/>.
        /// </summary>
        /// <param name="value">The <see cref="Matrix4D"/> whose transpose is to be calculated.</param>
        /// <returns>The transpose of the specified <see cref="Matrix4D"/>.</returns>
        public static Matrix4D Transpose(Matrix4D value)
        {
            Transpose(ref value, out Matrix4D result);
            return result;
        }

        /// <summary>
        /// Performs a linear interpolation between two <see cref="Matrix4D"/>.
        /// </summary>
        /// <param name="start">Start <see cref="Matrix4D"/>.</param>
        /// <param name="end">End <see cref="Matrix4D"/>.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <param name="result">When the method completes, contains the linear interpolation of the two <see cref="Matrix4D"/> matrices.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        public static void Lerp(ref Matrix4D start, ref Matrix4D end, double amount, out Matrix4D result)
        {
            result.M11 = MathHelper.Lerp(start.M11, end.M11, amount);
            result.M12 = MathHelper.Lerp(start.M12, end.M12, amount);
            result.M13 = MathHelper.Lerp(start.M13, end.M13, amount);
            result.M14 = MathHelper.Lerp(start.M14, end.M14, amount);
            result.M21 = MathHelper.Lerp(start.M21, end.M21, amount);
            result.M22 = MathHelper.Lerp(start.M22, end.M22, amount);
            result.M23 = MathHelper.Lerp(start.M23, end.M23, amount);
            result.M24 = MathHelper.Lerp(start.M24, end.M24, amount);
            result.M31 = MathHelper.Lerp(start.M31, end.M31, amount);
            result.M32 = MathHelper.Lerp(start.M32, end.M32, amount);
            result.M33 = MathHelper.Lerp(start.M33, end.M33, amount);
            result.M34 = MathHelper.Lerp(start.M34, end.M34, amount);
            result.M41 = MathHelper.Lerp(start.M41, end.M41, amount);
            result.M42 = MathHelper.Lerp(start.M42, end.M42, amount);
            result.M43 = MathHelper.Lerp(start.M43, end.M43, amount);
            result.M44 = MathHelper.Lerp(start.M44, end.M44, amount);
        }

        /// <summary>
        /// Performs a cubic interpolation between two <see cref="Matrix4D"/>.
        /// </summary>
        /// <param name="start">Start <see cref="Matrix4D"/>.</param>
        /// <param name="end">End <see cref="Matrix4D"/>.</param>
        /// <param name="amount">Value between 0D and 1D indicating the weight of <paramref name="end"/>.</param>
        /// <param name="result">When the method completes, contains the cubic interpolation of the two <see cref="Matrix4D"/> matrices.</param>
        public static void SmoothStep(ref Matrix4D start, ref Matrix4D end, double amount, out Matrix4D result)
        {
            amount = MathHelper.SmoothStep(amount);
            Lerp(ref start, ref end, amount, out result);
        }

        /// <summary>
        /// Performs a cubic interpolation between two matrices.
        /// </summary>
        /// <param name="start">Start <see cref="Matrix4D"/>.</param>
        /// <param name="end">End <see cref="Matrix4D"/>.</param>
        /// <param name="amount">Value between 0D and 1D indicating the weight of <paramref name="end"/>.</param>
        /// <returns>The cubic interpolation of the two matrices.</returns>
        public static Matrix4D SmoothStep(Matrix4D start, Matrix4D end, double amount)
        {
            SmoothStep(ref start, ref end, amount, out Matrix4D result);
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
        public static Matrix4D Lerp(Matrix4D start, Matrix4D end, double amount)
        {
            Lerp(ref start, ref end, amount, out Matrix4D result);
            return result;
        }

		/// <summary> Gets or sets the component at the specified index. </summary>
		/// <value>The value of the <see cref="Matrix4D"/> component, depending on the index.</value>
		/// <param name="index">The index of the index component to access, ranging from 0 to 3, inclusive.</param>
		/// <returns>The value of the component at the specified index value provided.</returns>
		/// <exception cref="IndexOutOfRangeException">Thrown if the index is out of range.</exception>
		public unsafe double this[int index]
		{
			get
			{
				if(index < 0 || index > 15)
					throw new IndexOutOfRangeException("index for Matrix4D must be between 0 and 15, inclusive.");

				return Values[index];
			}
			set
			{
				if(index < 0 || index > 15)
					throw new IndexOutOfRangeException("index for Matrix4D must be between 0 and 15, inclusive.");

				Values[index] = value;
			}
		}

		/// <summary> Gets or sets the component at the specified index. </summary>
		/// <value>The value of the <see cref="Matrix4D"/> component, depending on the index.</value>
		/// <param name="index">The index of the index component to access, ranging from 0 to 3, inclusive.</param>
		/// <returns>The value of the component at the specified index value provided.</returns>
		/// <exception cref="IndexOutOfRangeException">Thrown if the index is out of range.</exception>
		public unsafe double this[uint index]
		{
			get
			{
				if(index > 15)
					throw new IndexOutOfRangeException("index for Matrix4D must be between 0 and 15, inclusive.");

				return Values[index];
			}
			set
			{
				if(index > 15)
					throw new IndexOutOfRangeException("index for Matrix4D must be between 0 and 15, inclusive.");

				Values[index] = value;
			}
		}

		/// <summary> Gets or sets the component at the specified index. </summary>
		/// <value>The value of the <see cref="Matrix4D"/> component, depending on the index.</value>
		/// <param name="row">The index of the row component to access, ranging from 0 to 3, inclusive.</param>
		/// <param name="column">The index of the column component to access, ranging from 0 to 3, inclusive.</param>
		/// <returns>The value of the component at the specified index values provided.</returns>
		/// <exception cref="IndexOutOfRangeException">Thrown if the index is out of range.</exception>
		public unsafe double this[int row, int column]
		{
			get
			{
				if(row < 0 || row > 15)
					throw new IndexOutOfRangeException("row for Matrix4D must be between 0 and 15, inclusive.");

				if(column < 0 || column > 15)
					throw new IndexOutOfRangeException("column for Matrix4D must be between 0 and 15, inclusive.");

				return Values[(row * 4) + column];
			}
			set
			{
				if(row < 0 || row > 15)
					throw new IndexOutOfRangeException("row for Matrix4D must be between 0 and 15, inclusive.");

				if(column < 0 || column > 15)
					throw new IndexOutOfRangeException("column for Matrix4D must be between 0 and 15, inclusive.");

				Values[(row * 4) + column] = value;
			}
		}

		/// <summary> Gets or sets the component at the specified index. </summary>
		/// <value>The value of the <see cref="Matrix4D"/> component, depending on the index.</value>
		/// <param name="row">The index of the row component to access, ranging from 0 to 3, inclusive.</param>
		/// <param name="column">The index of the column component to access, ranging from 0 to 3, inclusive.</param>
		/// <returns>The value of the component at the specified index values provided.</returns>
		/// <exception cref="IndexOutOfRangeException">Thrown if the index is out of range.</exception>
		public unsafe double this[uint row, uint column]
		{
			get
			{
				if(row > 15)
					throw new IndexOutOfRangeException("row for Matrix4D must be between 0 and 15, inclusive.");

				if(column > 15)
					throw new IndexOutOfRangeException("column for Matrix4D must be between 0 and 15, inclusive.");

				return Values[(row * 4) + column];
			}
			set
			{
				if(row > 15)
					throw new IndexOutOfRangeException("row for Matrix4D must be between 0 and 15, inclusive.");

				if(column > 15)
					throw new IndexOutOfRangeException("column for Matrix4D must be between 0 and 15, inclusive.");

				Values[(row * 4) + column] = value;
			}
		}


        /// <summary>
        /// Returns a hash code for the current <see cref="Matrix4D"/>.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="Matrix4D"/>, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = M11.GetHashCode();
                    hashCode = (hashCode * 397) ^ M12.GetHashCode();
                    hashCode = (hashCode * 397) ^ M13.GetHashCode();
                    hashCode = (hashCode * 397) ^ M14.GetHashCode();
                    hashCode = (hashCode * 397) ^ M21.GetHashCode();
                    hashCode = (hashCode * 397) ^ M22.GetHashCode();
                    hashCode = (hashCode * 397) ^ M23.GetHashCode();
                    hashCode = (hashCode * 397) ^ M24.GetHashCode();
                    hashCode = (hashCode * 397) ^ M31.GetHashCode();
                    hashCode = (hashCode * 397) ^ M32.GetHashCode();
                    hashCode = (hashCode * 397) ^ M33.GetHashCode();
                    hashCode = (hashCode * 397) ^ M34.GetHashCode();
                    hashCode = (hashCode * 397) ^ M41.GetHashCode();
                    hashCode = (hashCode * 397) ^ M42.GetHashCode();
                    hashCode = (hashCode * 397) ^ M43.GetHashCode();
                    hashCode = (hashCode * 397) ^ M44.GetHashCode();
                    
                return hashCode;
            }
        }

        public static bool operator ==(Matrix4D matrix1, Matrix4D matrix2)
        {
            return matrix1.Equals(ref matrix2);
        }

        public static bool operator !=(Matrix4D matrix1, Matrix4D matrix2)
        {
            return !matrix1.Equals(ref matrix2);
        }

#region Add operators
		///<summary>Performs a add operation on two <see cref="Matrix4D"/>.</summary>
		///<param name="a">The first <see cref="Matrix4D"/> to add.</param>
		///<param name="b">The second <see cref="Matrix4D"/> to add.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Add(ref Matrix4D a, ref Matrix4D b, out Matrix4D result)
		{
			result.M11 = a.M11 + b.M11;
			result.M12 = a.M12 + b.M12;
			result.M13 = a.M13 + b.M13;
			result.M14 = a.M14 + b.M14;
			result.M21 = a.M21 + b.M21;
			result.M22 = a.M22 + b.M22;
			result.M23 = a.M23 + b.M23;
			result.M24 = a.M24 + b.M24;
			result.M31 = a.M31 + b.M31;
			result.M32 = a.M32 + b.M32;
			result.M33 = a.M33 + b.M33;
			result.M34 = a.M34 + b.M34;
			result.M41 = a.M41 + b.M41;
			result.M42 = a.M42 + b.M42;
			result.M43 = a.M43 + b.M43;
			result.M44 = a.M44 + b.M44;
		}

		///<summary>Performs a add operation on two <see cref="Matrix4D"/>.</summary>
		///<param name="a">The first <see cref="Matrix4D"/> to add.</param>
		///<param name="b">The second <see cref="Matrix4D"/> to add.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Matrix4D operator +(Matrix4D a, Matrix4D b)
		{
			Add(ref a, ref b, out Matrix4D result);
			return result;
		}

		///<summary>Performs a add operation on a <see cref="Matrix4D"/> and a <see cref="double"/>.</summary>
		///<param name="a">The <see cref="Matrix4D"/> to add.</param>
		///<param name="b">The <see cref="double"/> to add.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Add(ref Matrix4D a, double b, out Matrix4D result)
		{
			result.M11 = a.M11 + b;
			result.M12 = a.M12 + b;
			result.M13 = a.M13 + b;
			result.M14 = a.M14 + b;
			result.M21 = a.M21 + b;
			result.M22 = a.M22 + b;
			result.M23 = a.M23 + b;
			result.M24 = a.M24 + b;
			result.M31 = a.M31 + b;
			result.M32 = a.M32 + b;
			result.M33 = a.M33 + b;
			result.M34 = a.M34 + b;
			result.M41 = a.M41 + b;
			result.M42 = a.M42 + b;
			result.M43 = a.M43 + b;
			result.M44 = a.M44 + b;
		}

		///<summary>Performs a add operation on a <see cref="Matrix4D"/> and a <see cref="double"/>.</summary>
		///<param name="a">The <see cref="Matrix4D"/> to add.</param>
		///<param name="b">The <see cref="double"/> to add.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Matrix4D operator +(Matrix4D a, double b)
		{
			Add(ref a, b, out Matrix4D result);
			return result;
		}

		///<summary>Performs a add operation on a <see cref="double"/> and a <see cref="Matrix4D"/>.</summary>
		///<param name="a">The <see cref="double"/> to add.</param>
		///<param name="b">The <see cref="Matrix4D"/> to add.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Matrix4D operator +(double a, Matrix4D b)
		{
			Add(ref b, a, out Matrix4D result);
			return result;
		}


        /// <summary>
        /// Assert a <see cref="Matrix4D"/> (return it unchanged).
        /// </summary>
        /// <param name="value">The <see cref="Matrix4D"/> to assert (unchanged).</param>
        /// <returns>The asserted (unchanged) <see cref="Matrix4D"/>.</returns>
        public static Matrix4D operator +(Matrix4D value)
        {
            return value;
        }
#endregion

#region Subtract operators
		///<summary>Performs a subtract operation on two <see cref="Matrix4D"/>.</summary>
		///<param name="a">The first <see cref="Matrix4D"/> to subtract.</param>
		///<param name="b">The second <see cref="Matrix4D"/> to subtract.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Subtract(ref Matrix4D a, ref Matrix4D b, out Matrix4D result)
		{
			result.M11 = a.M11 - b.M11;
			result.M12 = a.M12 - b.M12;
			result.M13 = a.M13 - b.M13;
			result.M14 = a.M14 - b.M14;
			result.M21 = a.M21 - b.M21;
			result.M22 = a.M22 - b.M22;
			result.M23 = a.M23 - b.M23;
			result.M24 = a.M24 - b.M24;
			result.M31 = a.M31 - b.M31;
			result.M32 = a.M32 - b.M32;
			result.M33 = a.M33 - b.M33;
			result.M34 = a.M34 - b.M34;
			result.M41 = a.M41 - b.M41;
			result.M42 = a.M42 - b.M42;
			result.M43 = a.M43 - b.M43;
			result.M44 = a.M44 - b.M44;
		}

		///<summary>Performs a subtract operation on two <see cref="Matrix4D"/>.</summary>
		///<param name="a">The first <see cref="Matrix4D"/> to subtract.</param>
		///<param name="b">The second <see cref="Matrix4D"/> to subtract.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Matrix4D operator -(Matrix4D a, Matrix4D b)
		{
			Subtract(ref a, ref b, out Matrix4D result);
			return result;
		}

		///<summary>Performs a subtract operation on a <see cref="Matrix4D"/> and a <see cref="double"/>.</summary>
		///<param name="a">The <see cref="Matrix4D"/> to subtract.</param>
		///<param name="b">The <see cref="double"/> to subtract.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Subtract(ref Matrix4D a, double b, out Matrix4D result)
		{
			result.M11 = a.M11 - b;
			result.M12 = a.M12 - b;
			result.M13 = a.M13 - b;
			result.M14 = a.M14 - b;
			result.M21 = a.M21 - b;
			result.M22 = a.M22 - b;
			result.M23 = a.M23 - b;
			result.M24 = a.M24 - b;
			result.M31 = a.M31 - b;
			result.M32 = a.M32 - b;
			result.M33 = a.M33 - b;
			result.M34 = a.M34 - b;
			result.M41 = a.M41 - b;
			result.M42 = a.M42 - b;
			result.M43 = a.M43 - b;
			result.M44 = a.M44 - b;
		}

		///<summary>Performs a subtract operation on a <see cref="Matrix4D"/> and a <see cref="double"/>.</summary>
		///<param name="a">The <see cref="Matrix4D"/> to subtract.</param>
		///<param name="b">The <see cref="double"/> to subtract.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Matrix4D operator -(Matrix4D a, double b)
		{
			Subtract(ref a, b, out Matrix4D result);
			return result;
		}

		///<summary>Performs a subtract operation on a <see cref="double"/> and a <see cref="Matrix4D"/>.</summary>
		///<param name="a">The <see cref="double"/> to subtract.</param>
		///<param name="b">The <see cref="Matrix4D"/> to subtract.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Matrix4D operator -(double a, Matrix4D b)
		{
			Subtract(ref b, a, out Matrix4D result);
			return result;
		}


        /// <summary>
        /// Negates a <see cref="Matrix4D"/>.
        /// </summary>
        /// <param name="value">The matrix to be negated.</param>
        /// <param name="result">When the method completes, contains the negated <see cref="Matrix4D"/>.</param>
        public static void Negate(ref Matrix4D value, out Matrix4D result)
        {
             result.M11 = -value.M11;
             result.M12 = -value.M12;
             result.M13 = -value.M13;
             result.M14 = -value.M14;
             result.M21 = -value.M21;
             result.M22 = -value.M22;
             result.M23 = -value.M23;
             result.M24 = -value.M24;
             result.M31 = -value.M31;
             result.M32 = -value.M32;
             result.M33 = -value.M33;
             result.M34 = -value.M34;
             result.M41 = -value.M41;
             result.M42 = -value.M42;
             result.M43 = -value.M43;
             result.M44 = -value.M44;
         }

        /// <summary>
        /// Negates a <see cref="Matrix4D"/>.
        /// </summary>
        /// <param name="value">The <see cref="Matrix4D"/> to be negated.</param>
        /// <returns>The negated <see cref="Matrix4D"/>.</returns>
        public static Matrix4D Negate(Matrix4D value)
        {
            Matrix4D result;
            Negate(ref value, out result);
            return result;
        }

        /// <summary>
        /// Negates a <see cref="Matrix4D"/>.
        /// </summary>
        /// <param name="value">The <see cref="Matrix4D"/> to negate.</param>
        /// <returns>The negated <see cref="Matrix4D"/>.</returns>
        public static Matrix4D operator -(Matrix4D value)
        {
            Negate(ref value, out Matrix4D result);
            return result;
        }
#endregion

#region Multiply operators
        /// <summary>
        /// Scales a <see cref="Matrix4D"/> by the given scalar value.
        /// </summary>
        /// <param name="matrix">The <see cref="Matrix4D"/> to scale.</param>
        /// <param name="scalar">The scalar value by which to scale.</param>
        /// <param name="result">When the method completes, contains the scaled <see cref="Matrix4D"/>.</param>
        public static void Multiply(ref Matrix4D matrix, double scalar, out Matrix4D result)
        {
			result.M11 = matrix.M11 * scalar;
			result.M12 = matrix.M12 * scalar;
			result.M13 = matrix.M13 * scalar;
			result.M14 = matrix.M14 * scalar;
			result.M21 = matrix.M21 * scalar;
			result.M22 = matrix.M22 * scalar;
			result.M23 = matrix.M23 * scalar;
			result.M24 = matrix.M24 * scalar;
			result.M31 = matrix.M31 * scalar;
			result.M32 = matrix.M32 * scalar;
			result.M33 = matrix.M33 * scalar;
			result.M34 = matrix.M34 * scalar;
			result.M41 = matrix.M41 * scalar;
			result.M42 = matrix.M42 * scalar;
			result.M43 = matrix.M43 * scalar;
			result.M44 = matrix.M44 * scalar;
        }

        /// <summary>
        /// Scales a <see cref="Matrix4D"/> by a given value.
        /// </summary>
        /// <param name="matrix">The <see cref="Matrix4D"/> to scale.</param>
        /// <param name="scalar">The scalar value by which to scale.</param>
        /// <returns>The scaled matrix.</returns>
        public static Matrix4D operator *(Matrix4D matrix, double scalar)
        {
            Multiply(ref matrix, scalar, out Matrix4D result);
            return result;
        }
                
        /// <summary>
        /// Scales a <see cref="Matrix4D"/> by a given value.
        /// </summary>
        /// <param name="scalar">The scalar value by which to scale.</param>
        /// <param name="matrix">The <see cref="Matrix4D"/> to scale.</param>
        /// <returns>The scaled matrix.</returns>

        public static Matrix4D operator *(double scalar, Matrix4D matrix)
        {
            Multiply(ref matrix, scalar, out Matrix4D result);
            return result;
        }
#endregion

#region division operators
/// <summary>
        /// Scales a <see cref="Matrix4D"/> by the given scalar value.
        /// </summary>
        /// <param name="matrix">The <see cref="Matrix4D"/> to scale.</param>
        /// <param name="scalar">The amount by which to scale.</param>
        /// <param name="result">When the method completes, contains the scaled <see cref="Matrix4D"/>.</param>
        public static void Divide(ref Matrix4D matrix, double scalar, out Matrix4D result)
        {
            double inv = 1D / scalar;
			result.M11 = matrix.M11 * inv;
			result.M12 = matrix.M12 * inv;
			result.M13 = matrix.M13 * inv;
			result.M14 = matrix.M14 * inv;
			result.M21 = matrix.M21 * inv;
			result.M22 = matrix.M22 * inv;
			result.M23 = matrix.M23 * inv;
			result.M24 = matrix.M24 * inv;
			result.M31 = matrix.M31 * inv;
			result.M32 = matrix.M32 * inv;
			result.M33 = matrix.M33 * inv;
			result.M34 = matrix.M34 * inv;
			result.M41 = matrix.M41 * inv;
			result.M42 = matrix.M42 * inv;
			result.M43 = matrix.M43 * inv;
			result.M44 = matrix.M44 * inv;
        }

        /// <summary>
        /// Scales a <see cref="Matrix4D"/> by the given scalar value.
        /// </summary>
        /// <param name="matrix">The <see cref="Matrix4D"/> to scale.</param>
        /// <param name="scalar">The amount by which to scale.</param>
        /// <returns>The scaled matrix.</returns>
        public static Matrix4D Divide(Matrix4D matrix, double scalar)
        {
            Divide(ref matrix, scalar, out Matrix4D result);
            return result;
        }

                /// <summary>
        /// Determines the quotient of two matrices.
        /// </summary>
        /// <param name="left">The first <see cref="Matrix4D"/> to divide.</param>
        /// <param name="right">The second <see cref="Matrix4D"/> to divide.</param>
        /// <param name="result">When the method completes, contains the quotient of the two <see cref="Matrix4D"/> matrices.</param>
        public static void Divide(ref Matrix4D left, ref Matrix4D right, out Matrix4D result)
        {
			result.M11 = left.M11 / right.M11;
			result.M12 = left.M12 / right.M12;
			result.M13 = left.M13 / right.M13;
			result.M14 = left.M14 / right.M14;
			result.M21 = left.M21 / right.M21;
			result.M22 = left.M22 / right.M22;
			result.M23 = left.M23 / right.M23;
			result.M24 = left.M24 / right.M24;
			result.M31 = left.M31 / right.M31;
			result.M32 = left.M32 / right.M32;
			result.M33 = left.M33 / right.M33;
			result.M34 = left.M34 / right.M34;
			result.M41 = left.M41 / right.M41;
			result.M42 = left.M42 / right.M42;
			result.M43 = left.M43 / right.M43;
			result.M44 = left.M44 / right.M44;
        }

        /// <summary>
        /// Scales a <see cref="Matrix4D"/> by a given scalar value.
        /// </summary>
        /// <param name="matrix">The matrix to scale.</param>
        /// <param name="scalar">The amount by which to scale.</param>
        /// <returns>The scaled matrix.</returns>
        public static Matrix4D operator /(Matrix4D matrix, double scalar)
        {
            Divide(ref matrix, scalar, out Matrix4D result);
            return result;
        }

        /// <summary>
        /// Divides two <see cref="Matrix4D"/> matrices.
        /// </summary>
        /// <param name="left">The first <see cref="Matrix4D"/> to divide.</param>
        /// <param name="right">The second <see cref="Matrix4D"/> to divide.</param>
        /// <returns>The quotient of the two <see cref="Matrix4D"/> matrices.</returns>
        public static Matrix4D operator /(Matrix4D left, Matrix4D right)
        {
            Divide(ref left, ref right, out Matrix4D result);
            return result;
        }
#endregion
    }
}

