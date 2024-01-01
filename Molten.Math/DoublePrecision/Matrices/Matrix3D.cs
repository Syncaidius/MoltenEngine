using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Molten.DoublePrecision;

/// <summary>Represents a double-precision 3x3 Matrix. Contains only scale and rotation.</summary>
[StructLayout(LayoutKind.Explicit)]
[DataContract]
public partial struct Matrix3D : IEquatable<Matrix3D>, IFormattable, ITransposableMatrix<Matrix3D, Matrix3D>, IUniformMatrix<double>
{
    /// <summary>
    /// A single-precision <see cref="Matrix3D"/> with values intialized to the identity of a 2 x 2 matrix
    /// </summary>
    public static readonly Matrix3D Identity = new Matrix3D() 
    { 
        M11 = 1D, 
        M22 = 1D, 
        M33 = 1D, 
    };

    public static readonly int ComponentCount = 9;

    public static readonly int RowCount = 3;

    public static readonly int ColumnCount = 3;

    /// <summary>A <see cref="Matrix3D"/> will all of its components set to 0D.</summary>
    public static readonly Matrix3D Zero = new Matrix3D();

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

	/// <summary>The value at row 2, column 1 of the matrix.</summary>
	[DataMember]
	[FieldOffset(24)]
	public double M21;

	/// <summary>The value at row 2, column 2 of the matrix.</summary>
	[DataMember]
	[FieldOffset(32)]
	public double M22;

	/// <summary>The value at row 2, column 3 of the matrix.</summary>
	[DataMember]
	[FieldOffset(40)]
	public double M23;

	/// <summary>The value at row 3, column 1 of the matrix.</summary>
	[DataMember]
	[FieldOffset(48)]
	public double M31;

	/// <summary>The value at row 3, column 2 of the matrix.</summary>
	[DataMember]
	[FieldOffset(56)]
	public double M32;

	/// <summary>The value at row 3, column 3 of the matrix.</summary>
	[DataMember]
	[FieldOffset(64)]
	public double M33;

	/// <summary>A fixed array mapped to the same memory space as the individual <see cref="Matrix3D"/> components.</summary>
	[IgnoreDataMember]
	[FieldOffset(0)]
	public unsafe fixed double Values[9];


    /// <summary> Row 1 of the current <see cref="Matrix3D"/>.</summary>
    /// <returns>A <see cref="Vector3D"/> containing the row values.</returns>
    public Vector3D Row1
    {
        get => new Vector3D(M11, M12, M13);
        set => (M11, M12, M13) = value;
    }

    /// <summary> Row 2 of the current <see cref="Matrix3D"/>.</summary>
    /// <returns>A <see cref="Vector3D"/> containing the row values.</returns>
    public Vector3D Row2
    {
        get => new Vector3D(M21, M22, M23);
        set => (M21, M22, M23) = value;
    }

    /// <summary> Row 3 of the current <see cref="Matrix3D"/>.</summary>
    /// <returns>A <see cref="Vector3D"/> containing the row values.</returns>
    public Vector3D Row3
    {
        get => new Vector3D(M31, M32, M33);
        set => (M31, M32, M33) = value;
    }


    /// <summary> Column 1 of the current <see cref="Matrix3D"/>.</summary>
    /// <returns>A <see cref="Vector3D"/> containing the column values.</returns>
    public Vector3D Column1
    {
        get => new Vector3D(M11, M21, M31);
        set => (M11, M21, M31) = value;
    }

    /// <summary> Column 2 of the current <see cref="Matrix3D"/>.</summary>
    /// <returns>A <see cref="Vector3D"/> containing the column values.</returns>
    public Vector3D Column2
    {
        get => new Vector3D(M12, M22, M32);
        set => (M12, M22, M32) = value;
    }

    /// <summary> Column 3 of the current <see cref="Matrix3D"/>.</summary>
    /// <returns>A <see cref="Vector3D"/> containing the column values.</returns>
    public Vector3D Column3
    {
        get => new Vector3D(M13, M23, M33);
        set => (M13, M23, M33) = value;
    }


		/// <summary>
		/// Initializes a new instance of <see cref="Matrix3D"/>.
		/// </summary>
		/// <param name="m11">The value to assign to row 1, column 1 of the matrix.</param>
		/// <param name="m12">The value to assign to row 1, column 2 of the matrix.</param>
		/// <param name="m13">The value to assign to row 1, column 3 of the matrix.</param>
		/// <param name="m21">The value to assign to row 2, column 1 of the matrix.</param>
		/// <param name="m22">The value to assign to row 2, column 2 of the matrix.</param>
		/// <param name="m23">The value to assign to row 2, column 3 of the matrix.</param>
		/// <param name="m31">The value to assign to row 3, column 1 of the matrix.</param>
		/// <param name="m32">The value to assign to row 3, column 2 of the matrix.</param>
		/// <param name="m33">The value to assign to row 3, column 3 of the matrix.</param>
		public Matrix3D(double m11, double m12, double m13, double m21, double m22, double m23, double m31, double m32, double m33)
		{
			M11 = m11;
			M12 = m12;
			M13 = m13;
			M21 = m21;
			M22 = m22;
			M23 = m23;
			M31 = m31;
			M32 = m32;
			M33 = m33;
		}
		/// <summary>Initializes a new instance of <see cref="Matrix3D"/>.</summary>
		/// <param name="value">The value that will be assigned to all components.</param>
		public Matrix3D(double value)
		{
			M11 = value;
			M12 = value;
			M13 = value;
			M21 = value;
			M22 = value;
			M23 = value;
			M31 = value;
			M32 = value;
			M33 = value;
		}
		/// <summary>Initializes a new instance of <see cref="Matrix3D"/> from an array.</summary>
		/// <param name="values">The values to assign to the M11, M12, M13, M21, M22, M23, M31, M32, M33 components of the color. This must be an array with at least 9 elements.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than 9 elements.</exception>
		public unsafe Matrix3D(double[] values)
		{
			if (values == null)
				throw new ArgumentNullException("values");
			if (values.Length < 9)
				throw new ArgumentOutOfRangeException("values", "There must be at least 9 input values for Matrix3D.");

			fixed (double* src = values)
			{
				fixed (double* dst = Values)
					Unsafe.CopyBlock(src, dst, (sizeof(double) * 9));
			}
		}
		/// <summary>Initializes a new instance of <see cref="Matrix3D"/> from a span.</summary>
		/// <param name="values">The values to assign to the M11, M12, M13, M21, M22, M23, M31, M32, M33 components of the color. This must be an array with at least 9 elements.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than 9 elements.</exception>
		public Matrix3D(Span<double> values)
		{
			if (values == null)
				throw new ArgumentNullException("values");
			if (values.Length < 9)
				throw new ArgumentOutOfRangeException("values", "There must be at least 9 input values for Matrix3D.");

			M11 = values[0];
			M12 = values[1];
			M13 = values[2];
			M21 = values[3];
			M22 = values[4];
			M23 = values[5];
			M31 = values[6];
			M32 = values[7];
			M33 = values[8];
		}
		/// <summary>Initializes a new instance of <see cref="Matrix3D"/> from a an unsafe pointer.</summary>
		/// <param name="ptrValues">The values to assign to the M11, M12, M13, M21, M22, M23, M31, M32, M33 components of the color.
		/// <para>There must be at least 9 elements available or undefined behaviour will occur.</para></param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="ptrValues"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="ptrValues"/> contains more or less than 9 elements.</exception>
		public unsafe Matrix3D(double* ptrValues)
		{
			if (ptrValues == null)
				throw new ArgumentNullException("ptrValues");

			fixed (double* dst = Values)
				Unsafe.CopyBlock(ptrValues, dst, (sizeof(double) * 9));
		}

    /// <summary> Creates a string representation of the matrix.</summary>
    /// <returns>A string representation of the matrix.</returns>
    public override string ToString()
    {
        return string.Format("[{0}, {1}, {2}] [{3}, {4}, {5}] [{6}, {7}, {8}]", 
        M11, M12, M13, M21, M22, M23, M31, M32, M33);
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
        return string.Format("[{0}, {1}, {2}] [{3}, {4}, {5}] [{6}, {7}, {8}]", cc, 
        M11.ToString(format, cc), M12.ToString(format, cc), M13.ToString(format, cc), M21.ToString(format, cc), M22.ToString(format, cc), M23.ToString(format, cc), M31.ToString(format, cc), M32.ToString(format, cc), M33.ToString(format, cc));
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
        return string.Format("[{0}, {1}, {2}] [{3}, {4}, {5}] [{6}, {7}, {8}]", 
        M11.ToString(formatProvider), M12.ToString(formatProvider), M13.ToString(formatProvider), M21.ToString(formatProvider), M22.ToString(formatProvider), M23.ToString(formatProvider), M31.ToString(formatProvider), M32.ToString(formatProvider), M33.ToString(formatProvider));
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
        return string.Format("[{0}, {1}, {2}] [{3}, {4}, {5}] [{6}, {7}, {8}]", cc, 
        M11.ToString(format, formatProvider), M12.ToString(format, formatProvider), M13.ToString(format, formatProvider), M21.ToString(format, formatProvider), M22.ToString(format, formatProvider), M23.ToString(format, formatProvider), M31.ToString(format, formatProvider), M32.ToString(format, formatProvider), M33.ToString(format, formatProvider));
    }

    /// <summary>
    /// Determines whether the specified <see cref="Matrix3D"/> is equal to this instance.
    /// </summary>
    /// <param name="other">The <see cref="Matrix3D"/> to compare with this instance.</param>
    /// <returns>
    /// <c>true</c> if the specified <see cref="Matrix3D"/> is equal to this instance; otherwise, <c>false</c>.
    /// </returns>
    public bool Equals(ref Matrix3D other)
    {
        return MathHelper.NearEqual(other.M11, M11)
        && MathHelper.NearEqual(other.M12, M12)
        && MathHelper.NearEqual(other.M13, M13)
        && MathHelper.NearEqual(other.M21, M21)
        && MathHelper.NearEqual(other.M22, M22)
        && MathHelper.NearEqual(other.M23, M23)
        && MathHelper.NearEqual(other.M31, M31)
        && MathHelper.NearEqual(other.M32, M32)
        && MathHelper.NearEqual(other.M33, M33);
    }

    /// <summary>
    /// Determines whether the specified <see cref="Matrix3D"/> is equal to this instance.
    /// </summary>
    /// <param name="other">The <see cref="Matrix3D"/> to compare with this instance.</param>
    /// <returns>
    /// <c>true</c> if the specified <see cref="Matrix3D"/> is equal to this instance; otherwise, <c>false</c>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Matrix3D other)
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
        if (value is Matrix3D mat)
            return Equals(ref mat);

        return false;
    }

    /// <summary>
    /// Creates an array containing the elements of the <see cref="Matrix3D"/>.
    /// </summary>
    /// <returns>A 9-element array containing the components of the matrix.</returns>
    public unsafe double[] ToArray()
    {
        return [M11, M12, M13, M21, M22, M23, M31, M32, M33];
    }

    /// <summary>
    /// Transposes the current <see cref="Matrix3D"/> and outputs it to <paramref name="result"/>.
    /// </summary>
    /// <param name="result"></param>
    public void TransposeTo(out Matrix3D result)
    {
        TransposeTo(ref this, out result);
    }
      
    /// <summary>
    /// Transposes the current <see cref="Matrix3D"/> in-place, to a <see cref="Matrix3D"/>.
    /// </summary>
    public Matrix3D TransposeTo()
    {
        TransposeTo(ref this, out Matrix3D result);
        return result;
    }
        
    /// <summary>
    /// Transposes the current <see cref="Matrix3D"/> in-place.
    /// </summary>
    public void Transpose()
    {
        TransposeTo(ref this, out this);
    }

    /// <summary>
    /// Calculates the transpose of the specified <see cref="Matrix3D"/>.
    /// </summary>
    /// <param name="matrix">The <see cref="Matrix3D"/> whose transpose is to be calculated.</param>
    /// <param name="result">When the method completes, contains the transpose of the specified matrix.</param>
    public static void TransposeTo(ref Matrix3D matrix, out Matrix3D result)
    {
        Matrix3D temp = new Matrix3D();
        temp.M11 = matrix.M11;
        temp.M12 = matrix.M21;
        temp.M13 = matrix.M31;
        temp.M21 = matrix.M12;
        temp.M22 = matrix.M22;
        temp.M23 = matrix.M32;
        temp.M31 = matrix.M13;
        temp.M32 = matrix.M23;
        temp.M33 = matrix.M33;
        result = temp;
    }

    /// <summary>
    /// Calculates the transpose of the specified <see cref="Matrix3D"/>.
    /// </summary>
    /// <param name="value">The <see cref="Matrix3D"/> whose transpose is to be calculated.</param>
    /// <returns>The transpose of the specified <see cref="Matrix3D"/>.</returns>
    public static Matrix3D TransposeTo(Matrix3D value)
    {
        TransposeTo(ref value, out Matrix3D result);
        return result;
    }

    /// <summary>
    /// Performs a linear interpolation between two <see cref="Matrix3D"/>.
    /// </summary>
    /// <param name="start">Start <see cref="Matrix3D"/>.</param>
    /// <param name="end">End <see cref="Matrix3D"/>.</param>
    /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
    /// <param name="result">When the method completes, contains the linear interpolation of the two <see cref="Matrix3D"/> matrices.</param>
    /// <remarks>
    /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
    /// </remarks>
    public static void Lerp(ref Matrix3D start, ref Matrix3D end, double amount, out Matrix3D result)
    {
        result.M11 = MathHelper.Lerp(start.M11, end.M11, amount);
        result.M12 = MathHelper.Lerp(start.M12, end.M12, amount);
        result.M13 = MathHelper.Lerp(start.M13, end.M13, amount);
        result.M21 = MathHelper.Lerp(start.M21, end.M21, amount);
        result.M22 = MathHelper.Lerp(start.M22, end.M22, amount);
        result.M23 = MathHelper.Lerp(start.M23, end.M23, amount);
        result.M31 = MathHelper.Lerp(start.M31, end.M31, amount);
        result.M32 = MathHelper.Lerp(start.M32, end.M32, amount);
        result.M33 = MathHelper.Lerp(start.M33, end.M33, amount);
    }

    /// <summary>
    /// Performs a cubic interpolation between two <see cref="Matrix3D"/>.
    /// </summary>
    /// <param name="start">Start <see cref="Matrix3D"/>.</param>
    /// <param name="end">End <see cref="Matrix3D"/>.</param>
    /// <param name="amount">Value between 0D and 1D indicating the weight of <paramref name="end"/>.</param>
    /// <param name="result">When the method completes, contains the cubic interpolation of the two <see cref="Matrix3D"/> matrices.</param>
    public static void SmoothStep(ref Matrix3D start, ref Matrix3D end, double amount, out Matrix3D result)
    {
        amount = MathHelper.SmoothStep(amount);
        Lerp(ref start, ref end, amount, out result);
    }

    /// <summary>
    /// Performs a cubic interpolation between two matrices.
    /// </summary>
    /// <param name="start">Start <see cref="Matrix3D"/>.</param>
    /// <param name="end">End <see cref="Matrix3D"/>.</param>
    /// <param name="amount">Value between 0D and 1D indicating the weight of <paramref name="end"/>.</param>
    /// <returns>The cubic interpolation of the two matrices.</returns>
    public static Matrix3D SmoothStep(Matrix3D start, Matrix3D end, double amount)
    {
        SmoothStep(ref start, ref end, amount, out Matrix3D result);
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
    public static Matrix3D Lerp(Matrix3D start, Matrix3D end, double amount)
    {
        Lerp(ref start, ref end, amount, out Matrix3D result);
        return result;
    }

	/// <summary> Gets or sets the component at the specified index. </summary>
	/// <value>The value of the <see cref="Matrix3D"/> component, depending on the index.</value>
	/// <param name="index">The index of the index component to access, ranging from 0 to 2, inclusive.</param>
	/// <returns>The value of the component at the specified index value provided.</returns>
	/// <exception cref="IndexOutOfRangeException">Thrown if the index is out of range.</exception>
	public unsafe double this[int index]
	{
		get
		{
			if(index < 0 || index > 8)
				throw new IndexOutOfRangeException("index for Matrix3D must be between 0 and 8, inclusive.");

			return Values[index];
		}
		set
		{
			if(index < 0 || index > 8)
				throw new IndexOutOfRangeException("index for Matrix3D must be between 0 and 8, inclusive.");

			Values[index] = value;
		}
	}

	/// <summary> Gets or sets the component at the specified index. </summary>
	/// <value>The value of the <see cref="Matrix3D"/> component, depending on the index.</value>
	/// <param name="index">The index of the index component to access, ranging from 0 to 2, inclusive.</param>
	/// <returns>The value of the component at the specified index value provided.</returns>
	/// <exception cref="IndexOutOfRangeException">Thrown if the index is out of range.</exception>
	public unsafe double this[uint index]
	{
		get
		{
			if(index > 8)
				throw new IndexOutOfRangeException("index for Matrix3D must be between 0 and 8, inclusive.");

			return Values[index];
		}
		set
		{
			if(index > 8)
				throw new IndexOutOfRangeException("index for Matrix3D must be between 0 and 8, inclusive.");

			Values[index] = value;
		}
	}

	/// <summary> Gets or sets the component at the specified index. </summary>
	/// <value>The value of the <see cref="Matrix3D"/> component, depending on the index.</value>
	/// <param name="row">The index of the row component to access, ranging from 0 to 2, inclusive.</param>
	/// <param name="column">The index of the column component to access, ranging from 0 to 2, inclusive.</param>
	/// <returns>The value of the component at the specified index values provided.</returns>
	/// <exception cref="IndexOutOfRangeException">Thrown if the index is out of range.</exception>
	public unsafe double this[int row, int column]
	{
		get
		{
			if(row < 0 || row > 8)
				throw new IndexOutOfRangeException("row for Matrix3D must be between 0 and 8, inclusive.");

			if(column < 0 || column > 8)
				throw new IndexOutOfRangeException("column for Matrix3D must be between 0 and 8, inclusive.");

			return Values[(row * 3) + column];
		}
		set
		{
			if(row < 0 || row > 8)
				throw new IndexOutOfRangeException("row for Matrix3D must be between 0 and 8, inclusive.");

			if(column < 0 || column > 8)
				throw new IndexOutOfRangeException("column for Matrix3D must be between 0 and 8, inclusive.");

			Values[(row * 3) + column] = value;
		}
	}

	/// <summary> Gets or sets the component at the specified index. </summary>
	/// <value>The value of the <see cref="Matrix3D"/> component, depending on the index.</value>
	/// <param name="row">The index of the row component to access, ranging from 0 to 2, inclusive.</param>
	/// <param name="column">The index of the column component to access, ranging from 0 to 2, inclusive.</param>
	/// <returns>The value of the component at the specified index values provided.</returns>
	/// <exception cref="IndexOutOfRangeException">Thrown if the index is out of range.</exception>
	public unsafe double this[uint row, uint column]
	{
		get
		{
			if(row > 8)
				throw new IndexOutOfRangeException("row for Matrix3D must be between 0 and 8, inclusive.");

			if(column > 8)
				throw new IndexOutOfRangeException("column for Matrix3D must be between 0 and 8, inclusive.");

			return Values[(row * 3) + column];
		}
		set
		{
			if(row > 8)
				throw new IndexOutOfRangeException("row for Matrix3D must be between 0 and 8, inclusive.");

			if(column > 8)
				throw new IndexOutOfRangeException("column for Matrix3D must be between 0 and 8, inclusive.");

			Values[(row * 3) + column] = value;
		}
	}


    /// <summary>
    /// Returns a hash code for the current <see cref="Matrix3D"/>.
    /// </summary>
    /// <returns>
    /// A hash code for the current <see cref="Matrix3D"/>, suitable for use in hashing algorithms and data structures like a hash table. 
    /// </returns>
    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = M11.GetHashCode();
            hashCode = (hashCode * 397) ^ M12.GetHashCode();
            hashCode = (hashCode * 397) ^ M13.GetHashCode();
            hashCode = (hashCode * 397) ^ M21.GetHashCode();
            hashCode = (hashCode * 397) ^ M22.GetHashCode();
            hashCode = (hashCode * 397) ^ M23.GetHashCode();
            hashCode = (hashCode * 397) ^ M31.GetHashCode();
            hashCode = (hashCode * 397) ^ M32.GetHashCode();
            hashCode = (hashCode * 397) ^ M33.GetHashCode();
                
            return hashCode;
        }
    }

    public static bool operator ==(Matrix3D matrix1, Matrix3D matrix2)
    {
        return matrix1.Equals(ref matrix2);
    }

    public static bool operator !=(Matrix3D matrix1, Matrix3D matrix2)
    {
        return !matrix1.Equals(ref matrix2);
    }

#region Add operators
	///<summary>Performs a add operation on two <see cref="Matrix3D"/>.</summary>
	///<param name="a">The first <see cref="Matrix3D"/> to add.</param>
	///<param name="b">The second <see cref="Matrix3D"/> to add.</param>
	///<param name="result">Output for the result of the operation.</param>
	public static void Add(ref Matrix3D a, ref Matrix3D b, out Matrix3D result)
	{
		result.M11 = a.M11 + b.M11;
		result.M12 = a.M12 + b.M12;
		result.M13 = a.M13 + b.M13;
		result.M21 = a.M21 + b.M21;
		result.M22 = a.M22 + b.M22;
		result.M23 = a.M23 + b.M23;
		result.M31 = a.M31 + b.M31;
		result.M32 = a.M32 + b.M32;
		result.M33 = a.M33 + b.M33;
	}

	///<summary>Performs a add operation on two <see cref="Matrix3D"/>.</summary>
	///<param name="a">The first <see cref="Matrix3D"/> to add.</param>
	///<param name="b">The second <see cref="Matrix3D"/> to add.</param>
	///<returns>The result of the operation.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Matrix3D operator +(Matrix3D a, Matrix3D b)
	{
		Add(ref a, ref b, out Matrix3D result);
		return result;
	}

	///<summary>Performs a add operation on a <see cref="Matrix3D"/> and a <see cref="double"/>.</summary>
	///<param name="a">The <see cref="Matrix3D"/> to add.</param>
	///<param name="b">The <see cref="double"/> to add.</param>
	///<param name="result">Output for the result of the operation.</param>
	public static void Add(ref Matrix3D a, double b, out Matrix3D result)
	{
		result.M11 = a.M11 + b;
		result.M12 = a.M12 + b;
		result.M13 = a.M13 + b;
		result.M21 = a.M21 + b;
		result.M22 = a.M22 + b;
		result.M23 = a.M23 + b;
		result.M31 = a.M31 + b;
		result.M32 = a.M32 + b;
		result.M33 = a.M33 + b;
	}

	///<summary>Performs a add operation on a <see cref="Matrix3D"/> and a <see cref="double"/>.</summary>
	///<param name="a">The <see cref="Matrix3D"/> to add.</param>
	///<param name="b">The <see cref="double"/> to add.</param>
	///<returns>The result of the operation.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Matrix3D operator +(Matrix3D a, double b)
	{
		Add(ref a, b, out Matrix3D result);
		return result;
	}

	///<summary>Performs a add operation on a <see cref="double"/> and a <see cref="Matrix3D"/>.</summary>
	///<param name="a">The <see cref="double"/> to add.</param>
	///<param name="b">The <see cref="Matrix3D"/> to add.</param>
	///<returns>The result of the operation.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Matrix3D operator +(double a, Matrix3D b)
	{
		Add(ref b, a, out Matrix3D result);
		return result;
	}


    /// <summary>
    /// Assert a <see cref="Matrix3D"/> (return it unchanged).
    /// </summary>
    /// <param name="value">The <see cref="Matrix3D"/> to assert (unchanged).</param>
    /// <returns>The asserted (unchanged) <see cref="Matrix3D"/>.</returns>
    public static Matrix3D operator +(Matrix3D value)
    {
        return value;
    }
#endregion

#region Subtract operators
	///<summary>Performs a subtract operation on two <see cref="Matrix3D"/>.</summary>
	///<param name="a">The first <see cref="Matrix3D"/> to subtract.</param>
	///<param name="b">The second <see cref="Matrix3D"/> to subtract.</param>
	///<param name="result">Output for the result of the operation.</param>
	public static void Subtract(ref Matrix3D a, ref Matrix3D b, out Matrix3D result)
	{
		result.M11 = a.M11 - b.M11;
		result.M12 = a.M12 - b.M12;
		result.M13 = a.M13 - b.M13;
		result.M21 = a.M21 - b.M21;
		result.M22 = a.M22 - b.M22;
		result.M23 = a.M23 - b.M23;
		result.M31 = a.M31 - b.M31;
		result.M32 = a.M32 - b.M32;
		result.M33 = a.M33 - b.M33;
	}

	///<summary>Performs a subtract operation on two <see cref="Matrix3D"/>.</summary>
	///<param name="a">The first <see cref="Matrix3D"/> to subtract.</param>
	///<param name="b">The second <see cref="Matrix3D"/> to subtract.</param>
	///<returns>The result of the operation.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Matrix3D operator -(Matrix3D a, Matrix3D b)
	{
		Subtract(ref a, ref b, out Matrix3D result);
		return result;
	}

	///<summary>Performs a subtract operation on a <see cref="Matrix3D"/> and a <see cref="double"/>.</summary>
	///<param name="a">The <see cref="Matrix3D"/> to subtract.</param>
	///<param name="b">The <see cref="double"/> to subtract.</param>
	///<param name="result">Output for the result of the operation.</param>
	public static void Subtract(ref Matrix3D a, double b, out Matrix3D result)
	{
		result.M11 = a.M11 - b;
		result.M12 = a.M12 - b;
		result.M13 = a.M13 - b;
		result.M21 = a.M21 - b;
		result.M22 = a.M22 - b;
		result.M23 = a.M23 - b;
		result.M31 = a.M31 - b;
		result.M32 = a.M32 - b;
		result.M33 = a.M33 - b;
	}

	///<summary>Performs a subtract operation on a <see cref="Matrix3D"/> and a <see cref="double"/>.</summary>
	///<param name="a">The <see cref="Matrix3D"/> to subtract.</param>
	///<param name="b">The <see cref="double"/> to subtract.</param>
	///<returns>The result of the operation.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Matrix3D operator -(Matrix3D a, double b)
	{
		Subtract(ref a, b, out Matrix3D result);
		return result;
	}

	///<summary>Performs a subtract operation on a <see cref="double"/> and a <see cref="Matrix3D"/>.</summary>
	///<param name="a">The <see cref="double"/> to subtract.</param>
	///<param name="b">The <see cref="Matrix3D"/> to subtract.</param>
	///<returns>The result of the operation.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Matrix3D operator -(double a, Matrix3D b)
	{
		Subtract(ref b, a, out Matrix3D result);
		return result;
	}


    /// <summary>
    /// Negates a <see cref="Matrix3D"/>.
    /// </summary>
    /// <param name="value">The matrix to be negated.</param>
    /// <param name="result">When the method completes, contains the negated <see cref="Matrix3D"/>.</param>
    public static void Negate(ref Matrix3D value, out Matrix3D result)
    {
        result.M11 = -value.M11;
        result.M12 = -value.M12;
        result.M13 = -value.M13;
        result.M21 = -value.M21;
        result.M22 = -value.M22;
        result.M23 = -value.M23;
        result.M31 = -value.M31;
        result.M32 = -value.M32;
        result.M33 = -value.M33;
    }

    /// <summary>
    /// Negates a <see cref="Matrix3D"/>.
    /// </summary>
    /// <param name="value">The <see cref="Matrix3D"/> to be negated.</param>
    /// <returns>The negated <see cref="Matrix3D"/>.</returns>
    public static Matrix3D Negate(Matrix3D value)
    {
        Negate(ref value, out Matrix3D result);
        return result;
    }

    /// <summary>
    /// Negates a <see cref="Matrix3D"/>.
    /// </summary>
    /// <param name="value">The <see cref="Matrix3D"/> to negate.</param>
    /// <returns>The negated <see cref="Matrix3D"/>.</returns>
    public static Matrix3D operator -(Matrix3D value)
    {
        Negate(ref value, out Matrix3D result);
        return result;
    }
#endregion

#region Multiply operators
    /// <summary>
    /// Scales a <see cref="Matrix3D"/> by the given scalar value.
    /// </summary>
    /// <param name="matrix">The <see cref="Matrix3D"/> to scale.</param>
    /// <param name="scalar">The scalar value by which to scale.</param>
    /// <param name="result">When the method completes, contains the scaled <see cref="Matrix3D"/>.</param>
    public static void Multiply(ref Matrix3D matrix, double scalar, out Matrix3D result)
    {
		result.M11 = matrix.M11 * scalar;
		result.M12 = matrix.M12 * scalar;
		result.M13 = matrix.M13 * scalar;
		result.M21 = matrix.M21 * scalar;
		result.M22 = matrix.M22 * scalar;
		result.M23 = matrix.M23 * scalar;
		result.M31 = matrix.M31 * scalar;
		result.M32 = matrix.M32 * scalar;
		result.M33 = matrix.M33 * scalar;
    }

    /// <summary>
    /// Scales a <see cref="Matrix3D"/> by a given value.
    /// </summary>
    /// <param name="matrix">The <see cref="Matrix3D"/> to scale.</param>
    /// <param name="scalar">The scalar value by which to scale.</param>
    /// <returns>The scaled matrix.</returns>
    public static Matrix3D operator *(Matrix3D matrix, double scalar)
    {
        Multiply(ref matrix, scalar, out Matrix3D result);
        return result;
    }
                
    /// <summary>
    /// Scales a <see cref="Matrix3D"/> by a given value.
    /// </summary>
    /// <param name="scalar">The scalar value by which to scale.</param>
    /// <param name="matrix">The <see cref="Matrix3D"/> to scale.</param>
    /// <returns>The scaled matrix.</returns>

    public static Matrix3D operator *(double scalar, Matrix3D matrix)
    {
        Multiply(ref matrix, scalar, out Matrix3D result);
        return result;
    }
#endregion

#region division operators
/// <summary>
    /// Scales a <see cref="Matrix3D"/> by the given scalar value.
    /// </summary>
    /// <param name="matrix">The <see cref="Matrix3D"/> to scale.</param>
    /// <param name="scalar">The amount by which to scale.</param>
    /// <param name="result">When the method completes, contains the scaled <see cref="Matrix3D"/>.</param>
    public static void Divide(ref Matrix3D matrix, double scalar, out Matrix3D result)
    {
        double inv = 1D / scalar;
		result.M11 = matrix.M11 * inv;
		result.M12 = matrix.M12 * inv;
		result.M13 = matrix.M13 * inv;
		result.M21 = matrix.M21 * inv;
		result.M22 = matrix.M22 * inv;
		result.M23 = matrix.M23 * inv;
		result.M31 = matrix.M31 * inv;
		result.M32 = matrix.M32 * inv;
		result.M33 = matrix.M33 * inv;
    }

    /// <summary>
    /// Scales a <see cref="Matrix3D"/> by the given scalar value.
    /// </summary>
    /// <param name="matrix">The <see cref="Matrix3D"/> to scale.</param>
    /// <param name="scalar">The amount by which to scale.</param>
    /// <returns>The scaled matrix.</returns>
    public static Matrix3D Divide(Matrix3D matrix, double scalar)
    {
        Divide(ref matrix, scalar, out Matrix3D result);
        return result;
    }

            /// <summary>
    /// Determines the quotient of two matrices.
    /// </summary>
    /// <param name="left">The first <see cref="Matrix3D"/> to divide.</param>
    /// <param name="right">The second <see cref="Matrix3D"/> to divide.</param>
    /// <param name="result">When the method completes, contains the quotient of the two <see cref="Matrix3D"/> matrices.</param>
    public static void Divide(ref Matrix3D left, ref Matrix3D right, out Matrix3D result)
    {
		result.M11 = left.M11 / right.M11;
		result.M12 = left.M12 / right.M12;
		result.M13 = left.M13 / right.M13;
		result.M21 = left.M21 / right.M21;
		result.M22 = left.M22 / right.M22;
		result.M23 = left.M23 / right.M23;
		result.M31 = left.M31 / right.M31;
		result.M32 = left.M32 / right.M32;
		result.M33 = left.M33 / right.M33;
    }

    /// <summary>
    /// Scales a <see cref="Matrix3D"/> by a given scalar value.
    /// </summary>
    /// <param name="matrix">The matrix to scale.</param>
    /// <param name="scalar">The amount by which to scale.</param>
    /// <returns>The scaled matrix.</returns>
    public static Matrix3D operator /(Matrix3D matrix, double scalar)
    {
        Divide(ref matrix, scalar, out Matrix3D result);
        return result;
    }

    /// <summary>
    /// Divides two <see cref="Matrix3D"/> matrices.
    /// </summary>
    /// <param name="left">The first <see cref="Matrix3D"/> to divide.</param>
    /// <param name="right">The second <see cref="Matrix3D"/> to divide.</param>
    /// <returns>The quotient of the two <see cref="Matrix3D"/> matrices.</returns>
    public static Matrix3D operator /(Matrix3D left, Matrix3D right)
    {
        Divide(ref left, ref right, out Matrix3D result);
        return result;
    }
#endregion
}

