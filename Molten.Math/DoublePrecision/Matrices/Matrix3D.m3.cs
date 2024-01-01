
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Molten.DoublePrecision;

public partial struct Matrix3D
{
    /// <summary>
    /// Gets or sets the scale of the Matrix3x3; that is M11, M22, and M33.
    /// </summary>
    public Vector3D ScaleVector
    {
        get => new Vector3D(M11, M22, M33);
        set { M11 = value.X; M22 = value.Y; M33 = value.Z; }
    }

    /// <summary>
    /// Calculates the determinant of the Matrix3x3.
    /// </summary>
    /// <returns>The determinant of the Matrix3x3.</returns>
    public double Determinant()
    {
        return M11 * M22 * M33 + M12 * M23 * M31 + M13 * M21 * M32 - M13 * M22 * M31 - M12 * M21 * M33 - M11 * M23 * M32;
    }

    /// <summary>
    /// <para>Computes the adjugate transpose of a matrix.</para>
    /// <para>The adjugate transpose A of matrix M is: det(M) * transpose(invert(M))</para>
    /// <para>This is necessary when transforming normals (bivectors) with general linear transformations.</para>
    /// </summary>
    /// <param name="matrix">Matrix to compute the adjugate transpose of.</param>
    /// <param name="result">Adjugate transpose of the input matrix.</param>
    public static void AdjugateTranspose(ref Matrix3D matrix, out Matrix3D result)
    {
        // Despite the relative obscurity of the operation, this is a fairly straightforward operation which is actually faster than a true invert (by virtue of cancellation).
        // Conceptually, this is implemented as transpose(det(M) * invert(M)), but that's perfectly acceptable:
        // 1) transpose(invert(M)) == invert(transpose(M)), and
        // 2) det(M) == det(transpose(M))
        // This organization makes it clearer that the invert's usual division by determinant drops out.

        double m11 = (matrix.M22 * matrix.M33 - matrix.M23 * matrix.M32);
        double m12 = (matrix.M13 * matrix.M32 - matrix.M33 * matrix.M12);
        double m13 = (matrix.M12 * matrix.M23 - matrix.M22 * matrix.M13);

        double m21 = (matrix.M23 * matrix.M31 - matrix.M21 * matrix.M33);
        double m22 = (matrix.M11 * matrix.M33 - matrix.M13 * matrix.M31);
        double m23 = (matrix.M13 * matrix.M21 - matrix.M11 * matrix.M23);

        double m31 = (matrix.M21 * matrix.M32 - matrix.M22 * matrix.M31);
        double m32 = (matrix.M12 * matrix.M31 - matrix.M11 * matrix.M32);
        double m33 = (matrix.M11 * matrix.M22 - matrix.M12 * matrix.M21);

        // Note transposition.
        result.M11 = m11;
        result.M12 = m21;
        result.M13 = m31;

        result.M21 = m12;
        result.M22 = m22;
        result.M23 = m32;

        result.M31 = m13;
        result.M32 = m23;
        result.M33 = m33;
    }

    /// <summary>
    /// <para>Computes the adjugate transpose of a matrix.</para>
    /// <para>The adjugate transpose A of matrix M is: det(M) * transpose(invert(M))</para>
    /// <para>This is necessary when transforming normals (bivectors) with general linear transformations.</para>
    /// </summary>
    /// <param name="matrix">Matrix to compute the adjugate transpose of.</param>
    /// <returns>Adjugate transpose of the input matrix.</returns>
    public static Matrix3D AdjugateTranspose(Matrix3D matrix)
    {
        AdjugateTranspose(ref matrix, out Matrix3D result);
        return result;
    }

    /// <summary>
    /// Inverts the Matrix3x3.
    /// </summary>
    public void Invert()
    {
        Invert(ref this, out this);
    }

    /// <summary>
    /// Orthogonalizes the specified Matrix3x3.
    /// </summary>
    /// <remarks>
    /// <para>Orthogonalization is the process of making all rows orthogonal to each other. This
    /// means that any given row in the <see cref="Matrix3D"/> will be orthogonal to any other given row in the
    /// Matrix3x3.</para>
    /// <para>Because this method uses the modified Gram-Schmidt process, the resulting Matrix3x3
    /// tends to be numerically unstable. The numeric stability decreases according to the rows
    /// so that the first row is the most stable and the last row is the least stable.</para>
    /// <para>This operation is performed on the rows of the <see cref="Matrix3D"/> rather than the columns.
    /// If you wish for this operation to be performed on the columns, first transpose the
    /// input and than transpose the output.</para>
    /// </remarks>
    public void Orthogonalize()
    {
        Orthogonalize(ref this, out this);
    }

    /// <summary>
    /// Orthonormalizes the specified Matrix3x3.
    /// </summary>
    /// <remarks>
    /// <para>Orthonormalization is the process of making all rows and columns orthogonal to each
    /// other and making all rows and columns of unit length. This means that any given row will
    /// be orthogonal to any other given row and any given column will be orthogonal to any other
    /// given column. Any given row will not be orthogonal to any given column. Every row and every
    /// column will be of unit length.</para>
    /// <para>Because this method uses the modified Gram-Schmidt process, the resulting Matrix3x3
    /// tends to be numerically unstable. The numeric stability decreases according to the rows
    /// so that the first row is the most stable and the last row is the least stable.</para>
    /// <para>This operation is performed on the rows of the <see cref="Matrix3D"/> rather than the columns.
    /// If you wish for this operation to be performed on the columns, first transpose the
    /// input and than transpose the output.</para>
    /// </remarks>
    public void Orthonormalize()
    {
        Orthonormalize(ref this, out this);
    }

    /// <summary>
    /// Decomposes a <see cref="Matrix3D"/> into an orthonormalized <see cref="Matrix3D"/> Q and a right triangular <see cref="Matrix3D"/> R.
    /// </summary>
    /// <param name="Q">When the method completes, contains the orthonormalized <see cref="Matrix3D"/> of the decomposition.</param>
    /// <param name="R">When the method completes, contains the right triangular <see cref="Matrix3D"/> of the decomposition.</param>
    public void DecomposeQR(out Matrix3D Q, out Matrix3D R)
    {
        Matrix3D temp = this;
        temp.Transpose();
        Orthonormalize(ref temp, out Q);
        Q.Transpose();

        R = new Matrix3D();
        R.M11 = Vector3D.Dot(Q.Column1, Column1);
        R.M12 = Vector3D.Dot(Q.Column1, Column2);
        R.M13 = Vector3D.Dot(Q.Column1, Column3);

        R.M22 = Vector3D.Dot(Q.Column2, Column2);
        R.M23 = Vector3D.Dot(Q.Column2, Column3);

        R.M33 = Vector3D.Dot(Q.Column3, Column3);
    }

    /// <summary>
    /// Decomposes a <see cref="Matrix3D"/> into a lower triangular <see cref="Matrix3D"/> L and an orthonormalized <see cref="Matrix3D"/> Q.
    /// </summary>
    /// <param name="L">When the method completes, contains the lower triangular <see cref="Matrix3D"/> of the decomposition.</param>
    /// <param name="Q">When the method completes, contains the orthonormalized <see cref="Matrix3D"/> of the decomposition.</param>
    public void DecomposeLQ(out Matrix3D L, out Matrix3D Q)
    {
        Orthonormalize(ref this, out Q);

        L = new Matrix3D();
        L.M11 = Vector3D.Dot(Q.Row1, Row1);

        L.M21 = Vector3D.Dot(Q.Row1, Row2);
        L.M22 = Vector3D.Dot(Q.Row2, Row2);

        L.M31 = Vector3D.Dot(Q.Row1, Row3);
        L.M32 = Vector3D.Dot(Q.Row2, Row3);
        L.M33 = Vector3D.Dot(Q.Row3, Row3);
    }

    /// <summary>
    /// Decomposes a <see cref="Matrix3D"/> into a scale, rotation, and translation.
    /// </summary>
    /// <param name="scale">When the method completes, contains the scaling component of the decomposed Matrix3x3.</param>
    /// <param name="rotation">When the method completes, contains the rotation component of the decomposed Matrix3x3.</param>
    /// <remarks>
    /// This method is designed to decompose an SRT transformation <see cref="Matrix3D"/> only.
    /// </remarks>
    public bool Decompose(out Vector3D scale, out QuaternionD rotation)
    {
        //Source: Unknown
        //References: http://www.gamedev.net/community/forums/topic.asp?topic_id=441695

        //Scaling is the length of the rows.
        scale.X =  double.Sqrt((M11 * M11) + (M12 * M12) + (M13 * M13));
        scale.Y =  double.Sqrt((M21 * M21) + (M22 * M22) + (M23 * M23));
        scale.Z =  double.Sqrt((M31 * M31) + (M32 * M32) + (M33 * M33));

        //If any of the scaling factors are zero, than the rotation <see cref="Matrix3D"/> can not exist.
        if (MathHelper.IsZero(scale.X) ||
            MathHelper.IsZero(scale.Y) ||
            MathHelper.IsZero(scale.Z))
        {
            rotation = QuaternionD.Identity;
            return false;
        }

        //The rotation is the left over <see cref="Matrix3D"/> after dividing out the scaling.
        Matrix3D rotationMatrix3x3 = new Matrix3D();
        rotationMatrix3x3.M11 = M11 / scale.X;
        rotationMatrix3x3.M12 = M12 / scale.X;
        rotationMatrix3x3.M13 = M13 / scale.X;

        rotationMatrix3x3.M21 = M21 / scale.Y;
        rotationMatrix3x3.M22 = M22 / scale.Y;
        rotationMatrix3x3.M23 = M23 / scale.Y;

        rotationMatrix3x3.M31 = M31 / scale.Z;
        rotationMatrix3x3.M32 = M32 / scale.Z;
        rotationMatrix3x3.M33 = M33 / scale.Z;

        rotation = QuaternionD.FromRotationMatrix(ref rotationMatrix3x3);
        return true;
    }

    /// <summary>
    /// Decomposes a uniform scale matrix into a scale, rotation, and translation.
    /// A uniform scale matrix has the same scale in every axis.
    /// </summary>
    /// <param name="scale">When the method completes, contains the scaling component of the decomposed matrix.</param>
    /// <param name="rotation">When the method completes, contains the rotation component of the decomposed matrix.</param>
    /// <remarks>
    /// This method is designed to decompose only an SRT transformation matrix that has the same scale in every axis.
    /// </remarks>
    public bool DecomposeUniformScale(out double scale, out QuaternionD rotation)
    {
        //Scaling is the length of the rows. ( just take one row since this is a uniform matrix)
        scale =  double.Sqrt((M11 * M11) + (M12 * M12) + (M13 * M13));
        var inv_scale = 1f / scale;

        //If any of the scaling factors are zero, then the rotation matrix can not exist.
        if (Math.Abs(scale) < MathHelper.Constants<double>.ZeroTolerance)
        {
            rotation = QuaternionD.Identity;
            return false;
        }

        //The rotation is the left over matrix after dividing out the scaling.
        Matrix3D rotationmatrix = new Matrix3D();
        rotationmatrix.M11 = M11 * inv_scale;
        rotationmatrix.M12 = M12 * inv_scale;
        rotationmatrix.M13 = M13 * inv_scale;

        rotationmatrix.M21 = M21 * inv_scale;
        rotationmatrix.M22 = M22 * inv_scale;
        rotationmatrix.M23 = M23 * inv_scale;

        rotationmatrix.M31 = M31 * inv_scale;
        rotationmatrix.M32 = M32 * inv_scale;
        rotationmatrix.M33 = M33 * inv_scale;

        rotation = QuaternionD.FromRotationMatrix(ref rotationmatrix);
        return true;
    }

    /// <summary>
    /// Exchanges two rows in the Matrix3x3.
    /// </summary>
    /// <param name="firstRow">The first row to exchange. This is an index of the row starting at zero.</param>
    /// <param name="secondRow">The second row to exchange. This is an index of the row starting at zero.</param>
    public void ExchangeRows(int firstRow, int secondRow)
    {
        if (firstRow < 0)
            throw new ArgumentOutOfRangeException("firstRow", "The parameter firstRow must be greater than or equal to zero.");
        if (firstRow > 2)
            throw new ArgumentOutOfRangeException("firstRow", "The parameter firstRow must be less than or equal to two.");
        if (secondRow < 0)
            throw new ArgumentOutOfRangeException("secondRow", "The parameter secondRow must be greater than or equal to zero.");
        if (secondRow > 2)
            throw new ArgumentOutOfRangeException("secondRow", "The parameter secondRow must be less than or equal to two.");

        if (firstRow == secondRow)
            return;

        double temp0 = this[secondRow, 0];
        double temp1 = this[secondRow, 1];
        double temp2 = this[secondRow, 2];

        this[secondRow, 0] = this[firstRow, 0];
        this[secondRow, 1] = this[firstRow, 1];
        this[secondRow, 2] = this[firstRow, 2];

        this[firstRow, 0] = temp0;
        this[firstRow, 1] = temp1;
        this[firstRow, 2] = temp2;
    }

    /// <summary>
    /// Exchanges two columns in the Matrix3x3.
    /// </summary>
    /// <param name="firstColumn">The first column to exchange. This is an index of the column starting at zero.</param>
    /// <param name="secondColumn">The second column to exchange. This is an index of the column starting at zero.</param>
    public void ExchangeColumns(int firstColumn, int secondColumn)
    {
        if (firstColumn < 0)
            throw new ArgumentOutOfRangeException("firstColumn", "The parameter firstColumn must be greater than or equal to zero.");
        if (firstColumn > 2)
            throw new ArgumentOutOfRangeException("firstColumn", "The parameter firstColumn must be less than or equal to two.");
        if (secondColumn < 0)
            throw new ArgumentOutOfRangeException("secondColumn", "The parameter secondColumn must be greater than or equal to zero.");
        if (secondColumn > 2)
            throw new ArgumentOutOfRangeException("secondColumn", "The parameter secondColumn must be less than or equal to two.");

        if (firstColumn == secondColumn)
            return;

        double temp0 = this[0, secondColumn];
        double temp1 = this[1, secondColumn];
        double temp2 = this[2, secondColumn];

        this[0, secondColumn] = this[0, firstColumn];
        this[1, secondColumn] = this[1, firstColumn];
        this[2, secondColumn] = this[2, firstColumn];

        this[0, firstColumn] = temp0;
        this[1, firstColumn] = temp1;
        this[2, firstColumn] = temp2;
    }

    /// <summary>
    /// Scales a <see cref="Matrix3D"/> by the given value.
    /// </summary>
    /// <param name="left">The <see cref="Matrix3D"/> to scale.</param>
    /// <param name="right">The amount by which to scale.</param>
    /// <returns>The scaled <see cref="Matrix3D"/>.</returns>
    public static Matrix3D Multiply(Matrix3D left, double right)
    {
        Multiply(ref left, right, out Matrix3D result);
        return result;
    }

    /// <summary>
    /// Determines the product of two matrices.
    /// </summary>
    /// <param name="left">The first <see cref="Matrix3D"/> to multiply.</param>
    /// <param name="right">The second <see cref="Matrix3D"/> to multiply.</param>
    /// <param name="result">The product of the two matrices.</param>
    public static void Multiply(ref Matrix3D left, ref Matrix3D right, out Matrix3D result)
    {
        Matrix3D temp = new Matrix3D();
        temp.M11 = (left.M11 * right.M11) + (left.M12 * right.M21) + (left.M13 * right.M31);
        temp.M12 = (left.M11 * right.M12) + (left.M12 * right.M22) + (left.M13 * right.M32);
        temp.M13 = (left.M11 * right.M13) + (left.M12 * right.M23) + (left.M13 * right.M33);
        temp.M21 = (left.M21 * right.M11) + (left.M22 * right.M21) + (left.M23 * right.M31);
        temp.M22 = (left.M21 * right.M12) + (left.M22 * right.M22) + (left.M23 * right.M32);
        temp.M23 = (left.M21 * right.M13) + (left.M22 * right.M23) + (left.M23 * right.M33);
        temp.M31 = (left.M31 * right.M11) + (left.M32 * right.M21) + (left.M33 * right.M31);
        temp.M32 = (left.M31 * right.M12) + (left.M32 * right.M22) + (left.M33 * right.M32);
        temp.M33 = (left.M31 * right.M13) + (left.M32 * right.M23) + (left.M33 * right.M33);
        result = temp;
    }

    /// <summary>
    /// Determines the product of two matrices.
    /// </summary>
    /// <param name="left">The first <see cref="Matrix3D"/> to multiply.</param>
    /// <param name="right">The second <see cref="Matrix3D"/> to multiply.</param>
    /// <returns>The product of the two matrices.</returns>
    public static Matrix3D Multiply(Matrix3D left, Matrix3D right)
    {
        Multiply(ref left, ref right, out Matrix3D result);
        return result;
    }

    /// <summary>
    /// Creates a skew symmetric matrix M from vector A such that M * B for some other vector B is equivalent to the cross product of A and B.
    /// </summary>
    /// <param name="v">Vector to base the matrix on.</param>
    /// <param name="result">Skew-symmetric matrix result.</param>
    public static void CrossProduct(ref Vector3D v, out Matrix3D result)
    {
        result.M11 = 0;
        result.M12 = -v.Z;
        result.M13 = v.Y;
        result.M21 = v.Z;
        result.M22 = 0;
        result.M23 = -v.X;
        result.M31 = -v.Y;
        result.M32 = v.X;
        result.M33 = 0;
    }

    /// <summary>
    /// Computes the outer product of the given vectors.
    /// </summary>
    /// <param name="a">First vector.</param>
    /// <param name="b">Second vector.</param>
    /// <param name="result">Outer product result.</param>
    public static void CreateOuterProduct(ref Vector3D a, ref Vector3D b, out Matrix3D result)
    {
        result.M11 = a.X * b.X;
        result.M12 = a.X * b.Y;
        result.M13 = a.X * b.Z;

        result.M21 = a.Y * b.X;
        result.M22 = a.Y * b.Y;
        result.M23 = a.Y * b.Z;

        result.M31 = a.Z * b.X;
        result.M32 = a.Z * b.Y;
        result.M33 = a.Z * b.Z;
    }

    /// <summary>
    /// Performs the exponential operation on a <see cref="Matrix3D"/>.
    /// </summary>
    /// <param name="value">The <see cref="Matrix3D"/> to perform the operation on.</param>
    /// <param name="exponent">The exponent to raise the <see cref="Matrix3D"/> to.</param>
    /// <param name="result">When the method completes, contains the exponential <see cref="Matrix3D"/>.</param>
    /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="exponent"/> is negative.</exception>
    public static void Exponent(ref Matrix3D value, int exponent, out Matrix3D result)
    {
        //Source: http://rosettacode.org
        //Reference: http://rosettacode.org/wiki/Matrix3x3-exponentiation_operator

        if (exponent < 0)
            throw new ArgumentOutOfRangeException("exponent", "The exponent can not be negative.");

        if (exponent == 0)
        {
            result = Matrix3D.Identity;
            return;
        }

        if (exponent == 1)
        {
            result = value;
            return;
        }

        Matrix3D identity = Matrix3D.Identity;
        Matrix3D temp = value;

        while (true)
        {
            if ((exponent & 1) != 0)
                identity = identity * temp;

            exponent /= 2;

            if (exponent > 0)
                temp *= temp;
            else
                break;
        }

        result = identity;
    }

    /// <summary>
    /// Performs the exponential operation on a <see cref="Matrix3D"/>.
    /// </summary>
    /// <param name="value">The <see cref="Matrix3D"/> to perform the operation on.</param>
    /// <param name="exponent">The exponent to raise the <see cref="Matrix3D"/> to.</param>
    /// <returns>The exponential <see cref="Matrix3D"/>.</returns>
    /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="exponent"/> is negative.</exception>
    public static Matrix3D Exponent(Matrix3D value, int exponent)
    {
        Exponent(ref value, exponent, out Matrix3D result);
        return result;
    }

    /// <summary>
    /// Transforms the vector by the matrix's transpose.
    /// </summary>
    /// <param name="v">Vector3D to transform.</param>
    /// <param name="matrix">Matrix to use as the transformation transpose.</param>
    /// <param name="result">Product of the transformation.</param>
    public static void TransformTranspose(ref Vector3D v, ref Matrix3D matrix, out Vector3D result)
    {
        result = new Vector3D()
        {
            X = v.X * matrix.M11 + v.Y  * matrix.M12 + v.Z * matrix.M13,
            Y = v.X * matrix.M21 + v.Y  * matrix.M22 + v.Z * matrix.M23,
            Z = v.X * matrix.M31 + v.Y  * matrix.M32 + v.Z * matrix.M33
        };
    }

    /// <summary>
    /// Transforms the vector by the matrix's transpose.
    /// </summary>
    /// <param name="v">Vector3D to transform.</param>
    /// <param name="matrix">Matrix to use as the transformation transpose.</param>
    /// <returns>Product of the transformation.</returns>
    public static Vector3D TransformTranspose(Vector3D v, Matrix3D matrix)
    {
        return new Vector3D()
        {
            X = v.X * matrix.M11 + v.Y  * matrix.M12 + v.Z * matrix.M13,
            Y = v.X * matrix.M21 + v.Y  * matrix.M22 + v.Z * matrix.M23,
            Z = v.X * matrix.M31 + v.Y  * matrix.M32 + v.Z * matrix.M33
        };
    }

    /// <summary>
    /// Calculates the inverse of the specified <see cref="Matrix3D"/>.
    /// </summary>
    /// <param name="value">The <see cref="Matrix3D"/> whose inverse is to be calculated.</param>
    /// <param name="result">When the method completes, contains the inverse of the specified <see cref="Matrix3D"/>.</param>
    public static void Invert(ref Matrix3D value, out Matrix3D result)
    {
        double d11 = value.M22 * value.M33 + value.M23 * -value.M32;
        double d12 = value.M21 * value.M33 + value.M23 * -value.M31;
        double d13 = value.M21 * value.M32 + value.M22 * -value.M31;

        double det = value.M11 * d11 - value.M12 * d12 + value.M13 * d13;
        if (Math.Abs(det) == 0D)
        {
            result = Zero;
            return;
        }

        det = 1D / det;

        double d21 = value.M12 * value.M33 + value.M13 * -value.M32;
        double d22 = value.M11 * value.M33 + value.M13 * -value.M31;
        double d23 = value.M11 * value.M32 + value.M12 * -value.M31;

        double d31 = (value.M12 * value.M23) - (value.M13 * value.M22);
        double d32 = (value.M11 * value.M23) - (value.M13 * value.M21);
        double d33 = (value.M11 * value.M22) - (value.M12 * value.M21);

        result.M11 = +d11 * det; result.M12 = -d21 * det; result.M13 = +d31 * det;
        result.M21 = -d12 * det; result.M22 = +d22 * det; result.M23 = -d32 * det;
        result.M31 = +d13 * det; result.M32 = -d23 * det; result.M33 = +d33 * det;
    }

    /// <summary>
    /// Calculates the inverse of the specified <see cref="Matrix3D"/>.
    /// </summary>
    /// <param name="value">The <see cref="Matrix3D"/> whose inverse is to be calculated.</param>
    /// <returns>The inverse of the specified <see cref="Matrix3D"/>.</returns>
    public static Matrix3D Invert(Matrix3D value)
    {
        value.Invert();
        return value;
    }

    /// <summary>
    /// Orthogonalizes the specified <see cref="Matrix3D"/>.
    /// </summary>
    /// <param name="value">The <see cref="Matrix3D"/> to orthogonalize.</param>
    /// <param name="result">When the method completes, contains the orthogonalized <see cref="Matrix3D"/>.</param>
    /// <remarks>
    /// <para>Orthogonalization is the process of making all rows orthogonal to each other. This
    /// means that any given row in the <see cref="Matrix3D"/> will be orthogonal to any other given row in the
    /// <see cref="Matrix3D"/>.</para>
    /// <para>Because this method uses the modified Gram-Schmidt process, the resulting <see cref="Matrix3D"/>
    /// tends to be numerically unstable. The numeric stability decreases according to the rows
    /// so that the first row is the most stable and the last row is the least stable.</para>
    /// <para>This operation is performed on the rows of the <see cref="Matrix3D"/> rather than the columns.
    /// If you wish for this operation to be performed on the columns, first transpose the
    /// input and than transpose the output.</para>
    /// </remarks>
    public static void Orthogonalize(ref Matrix3D value, out Matrix3D result)
    {
        // Uses the modified Gram-Schmidt process.
        // q1 = m1
        // q2 = m2 - ((q1 ⋅ m2) / (q1 ⋅ q1)) * q1
        // q3 = m3 - ((q1 ⋅ m3) / (q1 ⋅ q1)) * q1 - ((q2 ⋅ m3) / (q2 ⋅ q2)) * q2

        //By separating the above algorithm into multiple lines, we actually increase accuracy.
        result = value;

        result.Row2 = result.Row2 - (Vector3D.Dot(result.Row1, result.Row2) / Vector3D.Dot(result.Row1, result.Row1)) * result.Row1;

        result.Row3 = result.Row3 - (Vector3D.Dot(result.Row1, result.Row3) / Vector3D.Dot(result.Row1, result.Row1)) * result.Row1;
        result.Row3 = result.Row3 - (Vector3D.Dot(result.Row2, result.Row3) / Vector3D.Dot(result.Row2, result.Row2)) * result.Row2;
    }

    /// <summary>
    /// Orthogonalizes the specified Matrix3x3.
    /// </summary>
    /// <param name="value">The <see cref="Matrix3D"/> to orthogonalize.</param>
    /// <returns>The orthogonalized Matrix3x3.</returns>
    /// <remarks>
    /// <para>Orthogonalization is the process of making all rows orthogonal to each other. This
    /// means that any given row in the <see cref="Matrix3D"/> will be orthogonal to any other given row in the
    /// Matrix3x3.</para>
    /// <para>Because this method uses the modified Gram-Schmidt process, the resulting Matrix3x3
    /// tends to be numerically unstable. The numeric stability decreases according to the rows
    /// so that the first row is the most stable and the last row is the least stable.</para>
    /// <para>This operation is performed on the rows of the <see cref="Matrix3D"/> rather than the columns.
    /// If you wish for this operation to be performed on the columns, first transpose the
    /// input and than transpose the output.</para>
    /// </remarks>
    public static Matrix3D Orthogonalize(Matrix3D value)
    {
        Orthogonalize(ref value, out Matrix3D result);
        return result;
    }

    /// <summary>
    /// Orthonormalizes the specified Matrix3x3.
    /// </summary>
    /// <param name="value">The <see cref="Matrix3D"/> to orthonormalize.</param>
    /// <param name="result">When the method completes, contains the orthonormalized Matrix3x3.</param>
    /// <remarks>
    /// <para>Orthonormalization is the process of making all rows and columns orthogonal to each
    /// other and making all rows and columns of unit length. This means that any given row will
    /// be orthogonal to any other given row and any given column will be orthogonal to any other
    /// given column. Any given row will not be orthogonal to any given column. Every row and every
    /// column will be of unit length.</para>
    /// <para>Because this method uses the modified Gram-Schmidt process, the resulting Matrix3x3
    /// tends to be numerically unstable. The numeric stability decreases according to the rows
    /// so that the first row is the most stable and the last row is the least stable.</para>
    /// <para>This operation is performed on the rows of the <see cref="Matrix3D"/> rather than the columns.
    /// If you wish for this operation to be performed on the columns, first transpose the
    /// input and than transpose the output.</para>
    /// </remarks>
    public static void Orthonormalize(ref Matrix3D value, out Matrix3D result)
    {
        //Uses the modified Gram-Schmidt process.
        //Because we are making unit vectors, we can optimize the math for orthonormalization
        //and simplify the projection operation to remove the division.
        //q1 = m1 / |m1|
        //q2 = (m2 - (q1 ⋅ m2) * q1) / |m2 - (q1 ⋅ m2) * q1|
        //q3 = (m3 - (q1 ⋅ m3) * q1 - (q2 ⋅ m3) * q2) / |m3 - (q1 ⋅ m3) * q1 - (q2 ⋅ m3) * q2|

        //By separating the above algorithm into multiple lines, we actually increase accuracy.
        result = value;

        result.Row1 = Vector3D.Normalize(result.Row1);

        result.Row2 = result.Row2 - Vector3D.Dot(result.Row1, result.Row2) * result.Row1;
        result.Row2 = Vector3D.Normalize(result.Row2);

        result.Row3 = result.Row3 - Vector3D.Dot(result.Row1, result.Row3) * result.Row1;
        result.Row3 = result.Row3 - Vector3D.Dot(result.Row2, result.Row3) * result.Row2;
        result.Row3 = Vector3D.Normalize(result.Row3);
    }

    /// <summary>
    /// Orthonormalizes the specified Matrix3x3.
    /// </summary>
    /// <param name="value">The <see cref="Matrix3D"/> to orthonormalize.</param>
    /// <returns>The orthonormalized Matrix3x3.</returns>
    /// <remarks>
    /// <para>Orthonormalization is the process of making all rows and columns orthogonal to each
    /// other and making all rows and columns of unit length. This means that any given row will
    /// be orthogonal to any other given row and any given column will be orthogonal to any other
    /// given column. Any given row will not be orthogonal to any given column. Every row and every
    /// column will be of unit length.</para>
    /// <para>Because this method uses the modified Gram-Schmidt process, the resulting Matrix3x3
    /// tends to be numerically unstable. The numeric stability decreases according to the rows
    /// so that the first row is the most stable and the last row is the least stable.</para>
    /// <para>This operation is performed on the rows of the <see cref="Matrix3D"/> rather than the columns.
    /// If you wish for this operation to be performed on the columns, first transpose the
    /// input and than transpose the output.</para>
    /// </remarks>
    public static Matrix3D Orthonormalize(Matrix3D value)
    {
        Orthonormalize(ref value, out Matrix3D result);
        return result;
    }

    /// <summary>
    /// Brings the <see cref="Matrix3D"/> into upper triangular form using elementary row operations.
    /// </summary>
    /// <param name="value">The <see cref="Matrix3D"/> to put into upper triangular form.</param>
    /// <param name="result">When the method completes, contains the upper triangular Matrix3x3.</param>
    /// <remarks>
    /// If the <see cref="Matrix3D"/> is not invertible (i.e. its determinant is zero) than the result of this
    /// method may produce Single.Nan and Single.Inf values. When the <see cref="Matrix3D"/> represents a system
    /// of linear equations, than this often means that either no solution exists or an infinite
    /// number of solutions exist.
    /// </remarks>
    public static void UpperTriangularForm(ref Matrix3D value, out Matrix3D result)
    {
        //Adapted from the row echelon code.
        result = value;
        int lead = 0;
        int rowcount = 3;
        int columncount = 3;

        for (int r = 0; r < rowcount; ++r)
        {
            if (columncount <= lead)
                return;

            int i = r;

            while (MathHelper.IsZero(result[i, lead]))
            {
                i++;

                if (i == rowcount)
                {
                    i = r;
                    lead++;

                    if (lead == columncount)
                        return;
                }
            }

            if (i != r)
            {
                result.ExchangeRows(i, r);
            }

            double multiplier = 1f / result[r, lead];

            for (; i < rowcount; ++i)
            {
                if (i != r)
                {
                    result[i, 0] -= result[r, 0] * multiplier * result[i, lead];
                    result[i, 1] -= result[r, 1] * multiplier * result[i, lead];
                    result[i, 2] -= result[r, 2] * multiplier * result[i, lead];
                }
            }

            lead++;
        }
    }

    /// <summary>
    /// Brings the <see cref="Matrix3D"/> into upper triangular form using elementary row operations.
    /// </summary>
    /// <param name="value">The <see cref="Matrix3D"/> to put into upper triangular form.</param>
    /// <returns>The upper triangular Matrix3x3.</returns>
    /// <remarks>
    /// If the <see cref="Matrix3D"/> is not invertible (i.e. its determinant is zero) than the result of this
    /// method may produce Single.Nan and Single.Inf values. When the <see cref="Matrix3D"/> represents a system
    /// of linear equations, than this often means that either no solution exists or an infinite
    /// number of solutions exist.
    /// </remarks>
    public static Matrix3D UpperTriangularForm(Matrix3D value)
    {
        UpperTriangularForm(ref value, out Matrix3D result);
        return result;
    }

    /// <summary>
    /// Brings the <see cref="Matrix3D"/> into lower triangular form using elementary row operations.
    /// </summary>
    /// <param name="value">The <see cref="Matrix3D"/> to put into lower triangular form.</param>
    /// <param name="result">When the method completes, contains the lower triangular Matrix3x3.</param>
    /// <remarks>
    /// If the <see cref="Matrix3D"/> is not invertible (i.e. its determinant is zero) than the result of this
    /// method may produce Single.Nan and Single.Inf values. When the <see cref="Matrix3D"/> represents a system
    /// of linear equations, than this often means that either no solution exists or an infinite
    /// number of solutions exist.
    /// </remarks>
    public static void LowerTriangularForm(ref Matrix3D value, out Matrix3D result)
    {
        //Adapted from the row echelon code.
        Matrix3D temp = value;
        Matrix3D.TransposeTo(ref temp, out result);

        int lead = 0;
        int rowcount = 3;
        int columncount = 3;

        for (int r = 0; r < rowcount; ++r)
        {
            if (columncount <= lead)
                return;

            int i = r;

            while (MathHelper.IsZero(result[i, lead]))
            {
                i++;

                if (i == rowcount)
                {
                    i = r;
                    lead++;

                    if (lead == columncount)
                        return;
                }
            }

            if (i != r)
            {
                result.ExchangeRows(i, r);
            }

            double multiplier = 1f / result[r, lead];

            for (; i < rowcount; ++i)
            {
                if (i != r)
                {
                    result[i, 0] -= result[r, 0] * multiplier * result[i, lead];
                    result[i, 1] -= result[r, 1] * multiplier * result[i, lead];
                    result[i, 2] -= result[r, 2] * multiplier * result[i, lead];
                }
            }

            lead++;
        }

        Matrix3D.TransposeTo(ref result, out result);
    }

    /// <summary>
    /// Brings the <see cref="Matrix3D"/> into lower triangular form using elementary row operations.
    /// </summary>
    /// <param name="value">The <see cref="Matrix3D"/> to put into lower triangular form.</param>
    /// <returns>The lower triangular Matrix3x3.</returns>
    /// <remarks>
    /// If the <see cref="Matrix3D"/> is not invertible (i.e. its determinant is zero) than the result of this
    /// method may produce Single.Nan and Single.Inf values. When the <see cref="Matrix3D"/> represents a system
    /// of linear equations, than this often means that either no solution exists or an infinite
    /// number of solutions exist.
    /// </remarks>
    public static Matrix3D LowerTriangularForm(Matrix3D value)
    {
        LowerTriangularForm(ref value, out Matrix3D result);
        return result;
    }

    /// <summary>
    /// Brings the <see cref="Matrix3D"/> into row echelon form using elementary row operations;
    /// </summary>
    /// <param name="value">The <see cref="Matrix3D"/> to put into row echelon form.</param>
    /// <param name="result">When the method completes, contains the row echelon form of the Matrix3x3.</param>
    public static void RowEchelonForm(ref Matrix3D value, out Matrix3D result)
    {
        //Source: Wikipedia pseudo code
        //Reference: http://en.wikipedia.org/wiki/Row_echelon_form#Pseudocode

        result = value;
        int lead = 0;
        int rowcount = 3;
        int columncount = 3;

        for (int r = 0; r < rowcount; ++r)
        {
            if (columncount <= lead)
                return;

            int i = r;

            while (MathHelper.IsZero(result[i, lead]))
            {
                i++;

                if (i == rowcount)
                {
                    i = r;
                    lead++;

                    if (lead == columncount)
                        return;
                }
            }

            if (i != r)
            {
                result.ExchangeRows(i, r);
            }

            double multiplier = 1f / result[r, lead];
            result[r, 0] *= multiplier;
            result[r, 1] *= multiplier;
            result[r, 2] *= multiplier;

            for (; i < rowcount; ++i)
            {
                if (i != r)
                {
                    result[i, 0] -= result[r, 0] * result[i, lead];
                    result[i, 1] -= result[r, 1] * result[i, lead];
                    result[i, 2] -= result[r, 2] * result[i, lead];
                }
            }

            lead++;
        }
    }

    /// <summary>
    /// Brings the <see cref="Matrix3D"/> into row echelon form using elementary row operations;
    /// </summary>
    /// <param name="value">The <see cref="Matrix3D"/> to put into row echelon form.</param>
    /// <returns>When the method completes, contains the row echelon form of the Matrix3x3.</returns>
    public static Matrix3D RowEchelonForm(Matrix3D value)
    {
        Matrix3D result;
        RowEchelonForm(ref value, out result);
        return result;
    }

    /// <summary>
    /// Creates a left-handed spherical billboard that rotates around a specified object position.
    /// </summary>
    /// <param name="objectPosition">The position of the object around which the billboard will rotate.</param>
    /// <param name="cameraPosition">The position of the camera.</param>
    /// <param name="cameraUpVector">The up vector of the camera.</param>
    /// <param name="cameraForwardVector">The forward vector of the camera.</param>
    public static Matrix3D BillboardLH(ref Vector3D objectPosition, ref Vector3D cameraPosition, ref Vector3D cameraUpVector, ref Vector3D cameraForwardVector)
    {
        Vector3D difference = cameraPosition - objectPosition;

        double lengthSq = difference.LengthSquared();
        if (MathHelper.IsZero(lengthSq))
            difference = -cameraForwardVector;
        else
            difference *= (1.0D / double.Sqrt(lengthSq));

        Vector3D crossed = Vector3D.Cross(ref cameraUpVector, ref difference);
        crossed.Normalize();
        Vector3D final = Vector3D.Cross(ref difference, ref crossed);

        return new Matrix3D()
        {
            M11 = crossed.X,
            M12 = crossed.Y,
            M13 = crossed.Z,
            M21 = final.X,
            M22 = final.Y,
            M23 = final.Z,
            M31 = difference.X,
            M32 = difference.Y,
            M33 = difference.Z,
        };
    }

    /// <summary>
    /// Creates a left-handed spherical billboard that rotates around a specified object position.
    /// </summary>
    /// <param name="objectPosition">The position of the object around which the billboard will rotate.</param>
    /// <param name="cameraPosition">The position of the camera.</param>
    /// <param name="cameraUpVector">The up vector of the camera.</param>
    /// <param name="cameraForwardVector">The forward vector of the camera.</param>
    /// <returns>The created billboard Matrix3x3.</returns>
    public static Matrix3D BillboardLH(
        Vector3D objectPosition, 
        Vector3D cameraPosition, 
        Vector3D cameraUpVector, 
        Vector3D cameraForwardVector)
    {
        return BillboardLH(ref objectPosition, ref cameraPosition, ref cameraUpVector, ref cameraForwardVector);
    }

    /// <summary>
    /// Creates a right-handed spherical billboard that rotates around a specified object position.
    /// </summary>
    /// <param name="objectPosition">The position of the object around which the billboard will rotate.</param>
    /// <param name="cameraPosition">The position of the camera.</param>
    /// <param name="cameraUpVector">The up vector of the camera.</param>
    /// <param name="cameraForwardVector">The forward vector of the camera.</param>
    /// <param name="result">When the method completes, contains the created billboard Matrix3x3.</param>
    public static Matrix3D BillboardRH(ref Vector3D objectPosition, ref Vector3D cameraPosition, ref Vector3D cameraUpVector, ref Vector3D cameraForwardVector)
    {
        Vector3D difference = objectPosition - cameraPosition;

        double lengthSq = difference.LengthSquared();
        if (MathHelper.IsZero(lengthSq))
            difference = -cameraForwardVector;
        else
            difference *= (1.0D / double.Sqrt(lengthSq));

        Vector3D crossed = Vector3D.Cross(ref cameraUpVector, ref difference);
        crossed.Normalize();
        Vector3D final = Vector3D.Cross(ref difference, ref crossed);

        return new Matrix3D()
        {
            M11 = crossed.X,
            M12 = crossed.Y,
            M13 = crossed.Z,
            M21 = final.X,
            M22 = final.Y,
            M23 = final.Z,
            M31 = difference.X,
            M32 = difference.Y,
            M33 = difference.Z,
        };
    }

    /// <summary>
    /// Creates a right-handed spherical billboard that rotates around a specified object position.
    /// </summary>
    /// <param name="objectPosition">The position of the object around which the billboard will rotate.</param>
    /// <param name="cameraPosition">The position of the camera.</param>
    /// <param name="cameraUpVector">The up vector of the camera.</param>
    /// <param name="cameraForwardVector">The forward vector of the camera.</param>
    /// <returns>The created billboard Matrix3x3.</returns>
    public static Matrix3D BillboardRH(Vector3D objectPosition, Vector3D cameraPosition, Vector3D cameraUpVector, Vector3D cameraForwardVector)
    {
        return BillboardRH(ref objectPosition, ref cameraPosition, ref cameraUpVector, ref cameraForwardVector);
    }

    /// <summary>
    /// Creates a left-handed, look-at Matrix3x3.
    /// </summary>
    /// <param name="eye">The position of the viewer's eye.</param>
    /// <param name="target">The camera look-at target.</param>
    /// <param name="up">The camera's up vector.</param>
    public static Matrix3D LookAtLH(ref Vector3D eye, ref Vector3D target, ref Vector3D up)
    {
        Vector3D zaxis = target - eye;
        zaxis.Normalize();

        Vector3D xaxis = Vector3D.Cross(ref up, ref zaxis); 
        xaxis.Normalize();

        Vector3D yaxis = Vector3D.Cross(ref zaxis, ref xaxis);

        Matrix3D result = Identity;
        result.M11 = xaxis.X; result.M21 = xaxis.Y; result.M31 = xaxis.Z;
        result.M12 = yaxis.X; result.M22 = yaxis.Y; result.M32 = yaxis.Z;
        result.M13 = zaxis.X; result.M23 = zaxis.Y; result.M33 = zaxis.Z;
        return result;
    }

    /// <summary>
    /// Creates a left-handed, look-at Matrix3x3.
    /// </summary>
    /// <param name="eye">The position of the viewer's eye.</param>
    /// <param name="target">The camera look-at target.</param>
    /// <param name="up">The camera's up vector.</param>
    /// <returns>The created look-at Matrix3x3.</returns>
    public static Matrix3D LookAtLH(Vector3D eye, Vector3D target, Vector3D up)
    {
        return LookAtLH(ref eye, ref target, ref up);
    }

    /// <summary>
    /// Creates a right-handed, look-at Matrix3x3.
    /// </summary>
    /// <param name="eye">The position of the viewer's eye.</param>
    /// <param name="target">The camera look-at target.</param>
    /// <param name="up">The camera's up vector.</param>
    public static Matrix3D LookAtRH(ref Vector3D eye, ref Vector3D target, ref Vector3D up)
    {
        Vector3D zAxis = eye - target; 
        zAxis.Normalize();

        Vector3D xAxis = Vector3D.Cross(ref up, ref zAxis); 
        xAxis.Normalize();

        Vector3D yAxis = Vector3D.Cross(ref zAxis, ref xAxis);

        return new Matrix3D()
        {
            M11 = xAxis.X,
            M21 = xAxis.Y,
            M31 = xAxis.Z,
            M12 = yAxis.X,
            M22 = yAxis.Y,
            M32 = yAxis.Z,
            M13 = zAxis.X,
            M23 = zAxis.Y,
            M33 = zAxis.Z,
        };
    }

    /// <summary>
    /// Creates a right-handed, look-at Matrix3x3.
    /// </summary>
    /// <param name="eye">The position of the viewer's eye.</param>
    /// <param name="target">The camera look-at target.</param>
    /// <param name="up">The camera's up vector.</param>
    /// <returns>The created look-at Matrix3x3.</returns>
    public static Matrix3D LookAtRH(Vector3D eye, Vector3D target, Vector3D up)
    {
        return LookAtRH(ref eye, ref target, ref up);
    }

    /// <summary>
    /// Creates a <see cref="Matrix3D"/> that scales along the x-axis, y-axis, and y-axis.
    /// </summary>
    /// <param name="scale">Scaling factor for all three axes.</param>
    /// <param name="result">When the method completes, contains the created scaling Matrix3x3.</param>
    public static void CreateScale(ref Vector3D scale, out Matrix3D result)
    {
        CreateScale(scale.X, scale.Y, scale.Z, out result);
    }

    /// <summary>
    /// Creates a <see cref="Matrix3D"/> that scales along the x-axis, y-axis, and y-axis.
    /// </summary>
    /// <param name="scale">Scaling factor for all three axes.</param>
    /// <returns>The created scaling Matrix3x3.</returns>
    public static Matrix3D CreateScale(Vector3D scale)
    {
        CreateScale(ref scale, out Matrix3D result);
        return result;
    }

    /// <summary>
    /// Creates a <see cref="Matrix3D"/> that scales along the x-axis, y-axis, and y-axis.
    /// </summary>
    /// <param name="x">Scaling factor that is applied along the x-axis.</param>
    /// <param name="y">Scaling factor that is applied along the y-axis.</param>
    /// <param name="z">Scaling factor that is applied along the z-axis.</param>
    /// <param name="result">When the method completes, contains the created scaling Matrix3x3.</param>
    public static void CreateScale(double x, double y, double z, out Matrix3D result)
    {
        result = Identity;
        result.M11 = x;
        result.M22 = y;
        result.M33 = z;
    }

    /// <summary>
    /// Creates a <see cref="Matrix3D"/> that scales along the x-axis, y-axis, and y-axis.
    /// </summary>
    /// <param name="x">Scaling factor that is applied along the x-axis.</param>
    /// <param name="y">Scaling factor that is applied along the y-axis.</param>
    /// <param name="z">Scaling factor that is applied along the z-axis.</param>
    /// <returns>The created scaling Matrix3x3.</returns>
    public static Matrix3D CreateScale(double x, double y, double z)
    {
        CreateScale(x, y, z, out Matrix3D result);
        return result;
    }

    /// <summary>
    /// Creates a <see cref="Matrix3D"/> that uniformly scales along all three axis.
    /// </summary>
    /// <param name="scale">The uniform scale that is applied along all axis.</param>
    /// <param name="result">When the method completes, contains the created scaling Matrix3x3.</param>
    public static void CreateScale(double scale, out Matrix3D result)
    {
        result = Identity;
        result.M11 = result.M22 = result.M33 = scale;
    }

    /// <summary>
    /// Creates a <see cref="Matrix3D"/> that uniformly scales along all three axis.
    /// </summary>
    /// <param name="scale">The uniform scale that is applied along all axis.</param>
    /// <returns>The created scaling Matrix3x3.</returns>
    public static Matrix3D CreateScale(double scale)
    {
        CreateScale(scale, out Matrix3D result);
        return result;
    }

    /// <summary>
    /// Creates a <see cref="Matrix3D"/> that rotates around the x-axis.
    /// </summary>
    /// <param name="angle">Angle of rotation in radians. Angles are measured clockwise when looking along the rotation axis toward the origin.</param>
    /// <param name="result">When the method completes, contains the created rotation Matrix3x3.</param>
    public static void RotationX(double angle, out Matrix3D result)
    {
        double cos =  double.Cos(angle);
        double sin =  double.Sin(angle);

        result = Identity;
        result.M22 = cos;
        result.M23 = sin;
        result.M32 = -sin;
        result.M33 = cos;
    }

    /// <summary>
    /// Creates a <see cref="Matrix3D"/> that rotates around the x-axis.
    /// </summary>
    /// <param name="angle">Angle of rotation in radians. Angles are measured clockwise when looking along the rotation axis toward the origin.</param>
    /// <returns>The created rotation Matrix3x3.</returns>
    public static Matrix3D RotationX(double angle)
    {
        RotationX(angle, out Matrix3D result);
        return result;
    }

    /// <summary>
    /// Creates a <see cref="Matrix3D"/> that rotates around the y-axis.
    /// </summary>
    /// <param name="angle">Angle of rotation in radians. Angles are measured clockwise when looking along the rotation axis toward the origin.</param>
    /// <param name="result">When the method completes, contains the created rotation Matrix3x3.</param>
    public static void RotationY(double angle, out Matrix3D result)
    {
        double cos =  double.Cos(angle);
        double sin =  double.Sin(angle);

        result = Identity;
        result.M11 = cos;
        result.M13 = -sin;
        result.M31 = sin;
        result.M33 = cos;
    }

    /// <summary>
    /// Creates a <see cref="Matrix3D"/> that rotates around the y-axis.
    /// </summary>
    /// <param name="angle">Angle of rotation in radians. Angles are measured clockwise when looking along the rotation axis toward the origin.</param>
    /// <returns>The created rotation Matrix3x3.</returns>
    public static Matrix3D RotationY(double angle)
    {
        RotationY(angle, out Matrix3D result);
        return result;
    }

    /// <summary>
    /// Creates a <see cref="Matrix3D"/> that rotates around the z-axis.
    /// </summary>
    /// <param name="angle">Angle of rotation in radians. Angles are measured clockwise when looking along the rotation axis toward the origin.</param>
    /// <param name="result">When the method completes, contains the created rotation Matrix3x3.</param>
    public static void RotationZ(double angle, out Matrix3D result)
    {
        double cos =  double.Cos(angle);
        double sin =  double.Sin(angle);

        result = Identity;
        result.M11 = cos;
        result.M12 = sin;
        result.M21 = -sin;
        result.M22 = cos;
    }

    /// <summary>
    /// Creates a <see cref="Matrix3D"/> that rotates around the z-axis.
    /// </summary>
    /// <param name="angle">Angle of rotation in radians. Angles are measured clockwise when looking along the rotation axis toward the origin.</param>
    /// <returns>The created rotation Matrix3x3.</returns>
    public static Matrix3D RotationZ(double angle)
    {
        RotationZ(angle, out Matrix3D result);
        return result;
    }

    /// <summary>
    /// Creates a <see cref="Matrix3D"/> that rotates around an arbitrary axis.
    /// </summary>
    /// <param name="axis">The axis around which to rotate. This parameter is assumed to be normalized.</param>
    /// <param name="angle">Angle of rotation in radians. Angles are measured clockwise when looking along the rotation axis toward the origin.</param>
    /// <param name="result">When the method completes, contains the created rotation Matrix3x3.</param>
    public static void RotationAxis(ref Vector3D axis, double angle, out Matrix3D result)
    {
        double x = axis.X;
        double y = axis.Y;
        double z = axis.Z;
        double cos =  double.Cos(angle);
        double sin =  double.Sin(angle);
        double xx = x * x;
        double yy = y * y;
        double zz = z * z;
        double xy = x * y;
        double xz = x * z;
        double yz = y * z;

        result = Identity;
        result.M11 = xx + (cos * (1.0D - xx));
        result.M12 = (xy - (cos * xy)) + (sin * z);
        result.M13 = (xz - (cos * xz)) - (sin * y);
        result.M21 = (xy - (cos * xy)) - (sin * z);
        result.M22 = yy + (cos * (1.0D - yy));
        result.M23 = (yz - (cos * yz)) + (sin * x);
        result.M31 = (xz - (cos * xz)) + (sin * y);
        result.M32 = (yz - (cos * yz)) - (sin * x);
        result.M33 = zz + (cos * (1.0D - zz));
    }

    /// <summary>
    /// Creates a <see cref="Matrix3D"/> that rotates around an arbitrary axis.
    /// </summary>
    /// <param name="axis">The axis around which to rotate. This parameter is assumed to be normalized.</param>
    /// <param name="angle">Angle of rotation in radians. Angles are measured clockwise when looking along the rotation axis toward the origin.</param>
    /// <returns>The created rotation Matrix3x3.</returns>
    public static Matrix3D RotationAxis(Vector3D axis, double angle)
    {
        RotationAxis(ref axis, angle, out Matrix3D result);
        return result;
    }

    /// <summary>
    /// Creates a rotation <see cref="Matrix3D"/> from a quaternion.
    /// </summary>
    /// <param name="rotation">The quaternion to use to build the Matrix3x3.</param>
    public static void FromQuaternion(ref QuaternionD rotation, out Matrix3D result)
    {
        double xx = rotation.X * rotation.X;
        double yy = rotation.Y * rotation.Y;
        double zz = rotation.Z * rotation.Z;
        double xy = rotation.X * rotation.Y;
        double zw = rotation.Z * rotation.W;
        double zx = rotation.Z * rotation.X;
        double yw = rotation.Y * rotation.W;
        double yz = rotation.Y * rotation.Z;
        double xw = rotation.X * rotation.W;

        result.M11 = 1.0D - (2.0D * (yy + zz));
        result.M12 = 2.0D * (xy + zw);
        result.M13 = 2.0D * (zx - yw);
        result.M21 = 2.0D * (xy - zw);
        result.M22 = 1.0D - (2.0D * (zz + xx));
        result.M23 = 2.0D * (yz + xw);
        result.M31 = 2.0D * (zx + yw);
        result.M32 = 2.0D * (yz - xw);
        result.M33 = 1.0D - (2.0D * (yy + xx));
    }

    /// <summary>
    /// Creates a rotation <see cref="Matrix3D"/> from a quaternion.
    /// </summary>
    /// <param name="rotation">The quaternion to use to build the Matrix3x3.</param>
    /// <returns>The created rotation Matrix3x3.</returns>
    public static Matrix3D FromQuaternion(QuaternionD rotation)
    {
        FromQuaternion(ref rotation, out Matrix3D result);
        return result;
    }

    /// <summary>
    /// Creates a rotation <see cref="Matrix3D"/> with a specified yaw, pitch, and roll.
    /// </summary>
    /// <param name="yaw">Yaw around the y-axis, in radians.</param>
    /// <param name="pitch">Pitch around the x-axis, in radians.</param>
    /// <param name="roll">Roll around the z-axis, in radians.</param>
    /// <param name="result">When the method completes, contains the created rotation Matrix3x3.</param>
    public static void RotationYawPitchRoll(double yaw, double pitch, double roll, out Matrix3D result)
    {
        QuaternionD quaternion = QuaternionD.RotationYawPitchRoll(yaw, pitch, roll);
        FromQuaternion(ref quaternion, out result);
    }

    /// <summary>
    /// Creates a rotation <see cref="Matrix3D"/> with a specified yaw, pitch, and roll.
    /// </summary>
    /// <param name="yaw">Yaw around the y-axis, in radians.</param>
    /// <param name="pitch">Pitch around the x-axis, in radians.</param>
    /// <param name="roll">Roll around the z-axis, in radians.</param>
    /// <param name="result">When the method completes, contains the created rotation Matrix3x3.</param>
    public static Matrix3D RotationYawPitchRoll(double yaw, double pitch, double roll)
    {
        QuaternionD quaternion = QuaternionD.RotationYawPitchRoll(yaw, pitch, roll);
        FromQuaternion(ref quaternion, out Matrix3D result);
        return result;
    }

    /// <summary>
    /// Multiplies a transposed matrix with another matrix.
    /// </summary>
    /// <param name="matrix">Matrix to be multiplied.</param>
    /// <param name="transpose">Matrix to be transposed and multiplied.</param>
    /// <param name="result">Product of the multiplication.</param>
    public static void MultiplyTransposed(ref Matrix3D transpose, ref Matrix3D matrix, out Matrix3D result)
    {
        double resultM11 = transpose.M11 * matrix.M11 + transpose.M21 * matrix.M21 + transpose.M31 * matrix.M31;
        double resultM12 = transpose.M11 * matrix.M12 + transpose.M21 * matrix.M22 + transpose.M31 * matrix.M32;
        double resultM13 = transpose.M11 * matrix.M13 + transpose.M21 * matrix.M23 + transpose.M31 * matrix.M33;

        double resultM21 = transpose.M12 * matrix.M11 + transpose.M22 * matrix.M21 + transpose.M32 * matrix.M31;
        double resultM22 = transpose.M12 * matrix.M12 + transpose.M22 * matrix.M22 + transpose.M32 * matrix.M32;
        double resultM23 = transpose.M12 * matrix.M13 + transpose.M22 * matrix.M23 + transpose.M32 * matrix.M33;

        double resultM31 = transpose.M13 * matrix.M11 + transpose.M23 * matrix.M21 + transpose.M33 * matrix.M31;
        double resultM32 = transpose.M13 * matrix.M12 + transpose.M23 * matrix.M22 + transpose.M33 * matrix.M32;
        double resultM33 = transpose.M13 * matrix.M13 + transpose.M23 * matrix.M23 + transpose.M33 * matrix.M33;

        result.M11 = resultM11;
        result.M12 = resultM12;
        result.M13 = resultM13;

        result.M21 = resultM21;
        result.M22 = resultM22;
        result.M23 = resultM23;

        result.M31 = resultM31;
        result.M32 = resultM32;
        result.M33 = resultM33;
    }

    /// <summary>
    /// Multiplies a matrix with a transposed matrix.
    /// </summary>
    /// <param name="matrix">Matrix to be multiplied.</param>
    /// <param name="transpose">Matrix to be transposed and multiplied.</param>
    /// <param name="result">Product of the multiplication.</param>
    public static void MultiplyByTransposed(ref Matrix3D matrix, ref Matrix3D transpose, out Matrix3D result)
    {
        double resultM11 = matrix.M11 * transpose.M11 + matrix.M12 * transpose.M12 + matrix.M13 * transpose.M13;
        double resultM12 = matrix.M11 * transpose.M21 + matrix.M12 * transpose.M22 + matrix.M13 * transpose.M23;
        double resultM13 = matrix.M11 * transpose.M31 + matrix.M12 * transpose.M32 + matrix.M13 * transpose.M33;

        double resultM21 = matrix.M21 * transpose.M11 + matrix.M22 * transpose.M12 + matrix.M23 * transpose.M13;
        double resultM22 = matrix.M21 * transpose.M21 + matrix.M22 * transpose.M22 + matrix.M23 * transpose.M23;
        double resultM23 = matrix.M21 * transpose.M31 + matrix.M22 * transpose.M32 + matrix.M23 * transpose.M33;

        double resultM31 = matrix.M31 * transpose.M11 + matrix.M32 * transpose.M12 + matrix.M33 * transpose.M13;
        double resultM32 = matrix.M31 * transpose.M21 + matrix.M32 * transpose.M22 + matrix.M33 * transpose.M23;
        double resultM33 = matrix.M31 * transpose.M31 + matrix.M32 * transpose.M32 + matrix.M33 * transpose.M33;

        result.M11 = resultM11;
        result.M12 = resultM12;
        result.M13 = resultM13;

        result.M21 = resultM21;
        result.M22 = resultM22;
        result.M23 = resultM23;

        result.M31 = resultM31;
        result.M32 = resultM32;
        result.M33 = resultM33;
    }

    /// <summary>
    /// Inverts the largest nonsingular submatrix in the matrix, excluding 2x2's that involve M13 or M31, and excluding 1x1's that include nondiagonal elements.
    /// </summary>
    /// <param name="matrix">Matrix to be inverted.</param>
    /// <param name="result">Inverted matrix.</param>
    public static void AdaptiveInvert(ref Matrix3D matrix, out Matrix3D result)
    {
        int submatrix;
        double determinantInverse = 1 / matrix.AdaptiveDeterminant(out submatrix);
        double m11, m12, m13, m21, m22, m23, m31, m32, m33;
        switch (submatrix)
        {
            case 0: //Full matrix.
                m11 = (matrix.M22 * matrix.M33 - matrix.M23 * matrix.M32) * determinantInverse;
                m12 = (matrix.M13 * matrix.M32 - matrix.M33 * matrix.M12) * determinantInverse;
                m13 = (matrix.M12 * matrix.M23 - matrix.M22 * matrix.M13) * determinantInverse;

                m21 = (matrix.M23 * matrix.M31 - matrix.M21 * matrix.M33) * determinantInverse;
                m22 = (matrix.M11 * matrix.M33 - matrix.M13 * matrix.M31) * determinantInverse;
                m23 = (matrix.M13 * matrix.M21 - matrix.M11 * matrix.M23) * determinantInverse;

                m31 = (matrix.M21 * matrix.M32 - matrix.M22 * matrix.M31) * determinantInverse;
                m32 = (matrix.M12 * matrix.M31 - matrix.M11 * matrix.M32) * determinantInverse;
                m33 = (matrix.M11 * matrix.M22 - matrix.M12 * matrix.M21) * determinantInverse;
                break;
            case 1: //Upper left matrix, m11, m12, m21, m22.
                m11 = matrix.M22 * determinantInverse;
                m12 = -matrix.M12 * determinantInverse;
                m13 = 0;

                m21 = -matrix.M21 * determinantInverse;
                m22 = matrix.M11 * determinantInverse;
                m23 = 0;

                m31 = 0;
                m32 = 0;
                m33 = 0;
                break;
            case 2: //Lower right matrix, m22, m23, m32, m33.
                m11 = 0;
                m12 = 0;
                m13 = 0;

                m21 = 0;
                m22 = matrix.M33 * determinantInverse;
                m23 = -matrix.M23 * determinantInverse;

                m31 = 0;
                m32 = -matrix.M32 * determinantInverse;
                m33 = matrix.M22 * determinantInverse;
                break;
            case 3: //Corners, m11, m31, m13, m33.
                m11 = matrix.M33 * determinantInverse;
                m12 = 0;
                m13 = -matrix.M13 * determinantInverse;

                m21 = 0;
                m22 = 0;
                m23 = 0;

                m31 = -matrix.M31 * determinantInverse;
                m32 = 0;
                m33 = matrix.M11 * determinantInverse;
                break;
            case 4: //M11
                m11 = 1 / matrix.M11;
                m12 = 0;
                m13 = 0;

                m21 = 0;
                m22 = 0;
                m23 = 0;

                m31 = 0;
                m32 = 0;
                m33 = 0;
                break;
            case 5: //M22
                m11 = 0;
                m12 = 0;
                m13 = 0;

                m21 = 0;
                m22 = 1 / matrix.M22;
                m23 = 0;

                m31 = 0;
                m32 = 0;
                m33 = 0;
                break;
            case 6: //M33
                m11 = 0;
                m12 = 0;
                m13 = 0;

                m21 = 0;
                m22 = 0;
                m23 = 0;

                m31 = 0;
                m32 = 0;
                m33 = 1 / matrix.M33;
                break;
            default: //Completely singular.
                m11 = 0; m12 = 0; m13 = 0; m21 = 0; m22 = 0; m23 = 0; m31 = 0; m32 = 0; m33 = 0;
                break;
        }

        result.M11 = m11;
        result.M12 = m12;
        result.M13 = m13;

        result.M21 = m21;
        result.M22 = m22;
        result.M23 = m23;

        result.M31 = m31;
        result.M32 = m32;
        result.M33 = m33;
    }

    /// <summary>
    /// Calculates the determinant of largest nonsingular submatrix, excluding 2x2's that involve M13 or M31, and excluding all 1x1's that involve nondiagonal elements.
    /// </summary>
    /// <param name="subMatrixCode">Represents the submatrix that was used to compute the determinant.
    /// 0 is the full 3x3.  1 is the upper left 2x2.  2 is the lower right 2x2.  3 is the four corners.
    /// 4 is M11.  5 is M22.  6 is M33.</param>
    /// <returns>The matrix's determinant.</returns>
    public double AdaptiveDeterminant(out int subMatrixCode)
    {
        //Try the full matrix first.
        double determinant = M11 * M22 * M33 + M12 * M23 * M31 + M13 * M21 * M32 -
                            M31 * M22 * M13 - M32 * M23 * M11 - M33 * M21 * M12;
        if (determinant != 0) //This could be a little numerically flimsy.  Fortunately, the way this method is used, that doesn't matter!
        {
            subMatrixCode = 0;
            return determinant;
        }

        //Try m11, m12, m21, m22.
        determinant = M11 * M22 - M12 * M21;
        if (determinant != 0)
        {
            subMatrixCode = 1;
            return determinant;
        }

        //Try m22, m23, m32, m33.
        determinant = M22 * M33 - M23 * M32;
        if (determinant != 0)
        {
            subMatrixCode = 2;
            return determinant;
        }

        //Try m11, m13, m31, m33.
        determinant = M11 * M33 - M13 * M12;
        if (determinant != 0)
        {
            subMatrixCode = 3;
            return determinant;
        }

        //Try m11.
        if (M11 != 0)
        {
            subMatrixCode = 4;
            return M11;
        }

        //Try m22.
        if (M22 != 0)
        {
            subMatrixCode = 5;
            return M22;
        }

        //Try m33.
        if (M33 != 0)
        {
            subMatrixCode = 6;
            return M33;
        }

        // Matrix is completely singular.
        subMatrixCode = -1;
        return 0;
    }

    /// <summary>
    /// Multiplies two matrices.
    /// </summary>
    /// <param name="left">The first <see cref="Matrix3D"/> to multiply.</param>
    /// <param name="right">The second <see cref="Matrix3D"/> to multiply.</param>
    /// <returns>The product of the two matrices.</returns>
    public static Matrix3D operator *(Matrix3D left, Matrix3D right)
    {
        Multiply(ref left, ref right, out Matrix3D result);
        return result;
    }

    /// <summary>
    /// Creates a 4x4 matrix from a <see cref="Matrix3D"/>.
    /// </summary>
    /// <param name="a">3x3 matrix.</param>
    /// <param name="b">Created 4x4 matrix.</param>
    public static void To4x4(ref Matrix3D a, out Matrix4D b)
    {
        b.M11 = a.M11;
        b.M12 = a.M12;
        b.M13 = a.M13;

        b.M21 = a.M21;
        b.M22 = a.M22;
        b.M23 = a.M23;

        b.M31 = a.M31;
        b.M32 = a.M32;
        b.M33 = a.M33;

        b.M14 = 0;
        b.M24 = 0;
        b.M34 = 0;
        b.M41 = 0;
        b.M42 = 0;
        b.M43 = 0;
        b.M44 = 1;
    }

    /// <summary>
    /// Creates a 4x4 matrix from a <see cref="Matrix3D"/>.
    /// </summary>
    /// <param name="a"><see cref="Matrix3D"/>.</param>
    /// <returns>Created 4x4 matrix.</returns>
    public static Matrix4D To4x4(Matrix3D a)
    {
        Matrix4D b;

        b.M11 = a.M11;
        b.M12 = a.M12;
        b.M13 = a.M13;

        b.M21 = a.M21;
        b.M22 = a.M22;
        b.M23 = a.M23;

        b.M31 = a.M31;
        b.M32 = a.M32;
        b.M33 = a.M33;

        b.M14 = 0;
        b.M24 = 0;
        b.M34 = 0;
        b.M41 = 0;
        b.M42 = 0;
        b.M43 = 0;
        b.M44 = 1;
        return b;
    }

    /// <summary>
    /// Creates a <see cref="Matrix3D"/> from an 4x4 matrix.
    /// </summary>
    /// <param name="matrix4X4">Matrix to extract a 3x3 matrix from.</param>
    /// <param name="matrix3X3">Upper 3x3 matrix extracted from the matrix.</param>
    public static void From4x4(ref Matrix4D matrix4X4, out Matrix3D matrix3X3)
    {
        matrix3X3.M11 = matrix4X4.M11;
        matrix3X3.M12 = matrix4X4.M12;
        matrix3X3.M13 = matrix4X4.M13;

        matrix3X3.M21 = matrix4X4.M21;
        matrix3X3.M22 = matrix4X4.M22;
        matrix3X3.M23 = matrix4X4.M23;

        matrix3X3.M31 = matrix4X4.M31;
        matrix3X3.M32 = matrix4X4.M32;
        matrix3X3.M33 = matrix4X4.M33;
    }

    /// <summary>
    /// Creates a <see cref="Matrix3D"/> from an 4x4 matrix.
    /// </summary>
    /// <param name="matrix4X4">Matrix to extract a 3x3 matrix from.</param>
    /// <returns>Upper 3x3 matrix extracted from the matrix.</returns>
    public static Matrix3D From4x4(Matrix4D matrix4X4)
    {
        Matrix3D matrix3X3;
        matrix3X3.M11 = matrix4X4.M11;
        matrix3X3.M12 = matrix4X4.M12;
        matrix3X3.M13 = matrix4X4.M13;

        matrix3X3.M21 = matrix4X4.M21;
        matrix3X3.M22 = matrix4X4.M22;
        matrix3X3.M23 = matrix4X4.M23;

        matrix3X3.M31 = matrix4X4.M31;
        matrix3X3.M32 = matrix4X4.M32;
        matrix3X3.M33 = matrix4X4.M33;
        return matrix3X3;
    }

    /// <summary>
    /// Convert the <see cref="Matrix3D"/> to a 4x4 Matrix.
    /// </summary>
    /// <returns>A 4x4 Matrix with zero translation and M44=1</returns>
    public static explicit operator Matrix4D(Matrix3D Value)
    {
        return new Matrix4D(
            Value.M11, Value.M12, Value.M13, 0,
            Value.M21, Value.M22, Value.M23, 0,
            Value.M31, Value.M32, Value.M33, 0,
            0, 0, 0, 1
            );
    }

    /// <summary>
    /// Convert the 4x4 Matrix to a <see cref="Matrix3D"/>.
    /// </summary>
    /// <returns>A 3x3 Matrix</returns>
    public static explicit operator Matrix3D(Matrix4D Value)
    {
        return new Matrix3D(
            Value.M11, Value.M12, Value.M13,
            Value.M21, Value.M22, Value.M23,
            Value.M31, Value.M32, Value.M33
            );
    }

    /// <summary>
    /// Gets or sets the backward vector of the <see cref="Matrix3D"/>.
    /// </summary>
    public Vector3D Backward
    {
        get => new Vector3D(M31, M32, M33);
        set
        {
            M31 = value.X;
            M32 = value.Y;
            M33 = value.Z;
        }
    }

    /// <summary>
    /// Gets or sets the down vector of the <see cref="Matrix3D"/>.
    /// </summary>
    public Vector3D Down
    {
        get => new Vector3D(-M21, -M22, -M23);
        set
        {
            M21 = -value.X;
            M22 = -value.Y;
            M23 = -value.Z;
        }
    }

    /// <summary>
    /// Gets or sets the forward vector of the <see cref="Matrix3D"/>.
    /// </summary>
    public Vector3D Forward
    {
        get => new Vector3D(-M31, -M32, -M33);
        set
        {
            M31 = -value.X;
            M32 = -value.Y;
            M33 = -value.Z;
        }
    }

    /// <summary>
    /// Gets or sets the left vector of the <see cref="Matrix3D"/>.
    /// </summary>
    public Vector3D Left
    {
        get => new Vector3D(-M11, -M12, -M13);
        set
        {
            M11 = -value.X;
            M12 = -value.Y;
            M13 = -value.Z;
        }
    }

    /// <summary>
    /// Gets or sets the right vector of the <see cref="Matrix3D"/>.
    /// </summary>
    public Vector3D Right
    {
        get => new Vector3D(M11, M12, M13);
        set
        {
            M11 = value.X;
            M12 = value.Y;
            M13 = value.Z;
        }
    }

    /// <summary>
    /// Gets or sets the up vector of the <see cref="Matrix3D"/>.
    /// </summary>
    public Vector3D Up
    {
        get => new Vector3D(M21, M22, M23);
        set
        {
            M21 = value.X;
            M22 = value.Y;
            M23 = value.Z;
        }
    }
}

