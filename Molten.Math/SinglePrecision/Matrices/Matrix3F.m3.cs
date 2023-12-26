
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Molten
{
    public partial struct Matrix3F
    {
        /// <summary>
        /// Gets or sets the scale of the Matrix3x3; that is M11, M22, and M33.
        /// </summary>
        public Vector3F ScaleVector
        {
            get => new Vector3F(M11, M22, M33);
            set { M11 = value.X; M22 = value.Y; M33 = value.Z; }
        }

        /// <summary>
        /// Calculates the determinant of the Matrix3x3.
        /// </summary>
        /// <returns>The determinant of the Matrix3x3.</returns>
        public float Determinant()
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
        public static void AdjugateTranspose(ref Matrix3F matrix, out Matrix3F result)
        {
            // Despite the relative obscurity of the operation, this is a fairly straightforward operation which is actually faster than a true invert (by virtue of cancellation).
            // Conceptually, this is implemented as transpose(det(M) * invert(M)), but that's perfectly acceptable:
            // 1) transpose(invert(M)) == invert(transpose(M)), and
            // 2) det(M) == det(transpose(M))
            // This organization makes it clearer that the invert's usual division by determinant drops out.

            float m11 = (matrix.M22 * matrix.M33 - matrix.M23 * matrix.M32);
            float m12 = (matrix.M13 * matrix.M32 - matrix.M33 * matrix.M12);
            float m13 = (matrix.M12 * matrix.M23 - matrix.M22 * matrix.M13);

            float m21 = (matrix.M23 * matrix.M31 - matrix.M21 * matrix.M33);
            float m22 = (matrix.M11 * matrix.M33 - matrix.M13 * matrix.M31);
            float m23 = (matrix.M13 * matrix.M21 - matrix.M11 * matrix.M23);

            float m31 = (matrix.M21 * matrix.M32 - matrix.M22 * matrix.M31);
            float m32 = (matrix.M12 * matrix.M31 - matrix.M11 * matrix.M32);
            float m33 = (matrix.M11 * matrix.M22 - matrix.M12 * matrix.M21);

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
        public static Matrix3F AdjugateTranspose(Matrix3F matrix)
        {
            AdjugateTranspose(ref matrix, out Matrix3F result);
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
        /// means that any given row in the <see cref="Matrix3F"/> will be orthogonal to any other given row in the
        /// Matrix3x3.</para>
        /// <para>Because this method uses the modified Gram-Schmidt process, the resulting Matrix3x3
        /// tends to be numerically unstable. The numeric stability decreases according to the rows
        /// so that the first row is the most stable and the last row is the least stable.</para>
        /// <para>This operation is performed on the rows of the <see cref="Matrix3F"/> rather than the columns.
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
        /// <para>This operation is performed on the rows of the <see cref="Matrix3F"/> rather than the columns.
        /// If you wish for this operation to be performed on the columns, first transpose the
        /// input and than transpose the output.</para>
        /// </remarks>
        public void Orthonormalize()
        {
            Orthonormalize(ref this, out this);
        }

        /// <summary>
        /// Decomposes a <see cref="Matrix3F"/> into an orthonormalized <see cref="Matrix3F"/> Q and a right triangular <see cref="Matrix3F"/> R.
        /// </summary>
        /// <param name="Q">When the method completes, contains the orthonormalized <see cref="Matrix3F"/> of the decomposition.</param>
        /// <param name="R">When the method completes, contains the right triangular <see cref="Matrix3F"/> of the decomposition.</param>
        public void DecomposeQR(out Matrix3F Q, out Matrix3F R)
        {
            Matrix3F temp = this;
            temp.Transpose();
            Orthonormalize(ref temp, out Q);
            Q.Transpose();

            R = new Matrix3F();
            R.M11 = Vector3F.Dot(Q.Column1, Column1);
            R.M12 = Vector3F.Dot(Q.Column1, Column2);
            R.M13 = Vector3F.Dot(Q.Column1, Column3);

            R.M22 = Vector3F.Dot(Q.Column2, Column2);
            R.M23 = Vector3F.Dot(Q.Column2, Column3);

            R.M33 = Vector3F.Dot(Q.Column3, Column3);
        }

        /// <summary>
        /// Decomposes a <see cref="Matrix3F"/> into a lower triangular <see cref="Matrix3F"/> L and an orthonormalized <see cref="Matrix3F"/> Q.
        /// </summary>
        /// <param name="L">When the method completes, contains the lower triangular <see cref="Matrix3F"/> of the decomposition.</param>
        /// <param name="Q">When the method completes, contains the orthonormalized <see cref="Matrix3F"/> of the decomposition.</param>
        public void DecomposeLQ(out Matrix3F L, out Matrix3F Q)
        {
            Orthonormalize(ref this, out Q);

            L = new Matrix3F();
            L.M11 = Vector3F.Dot(Q.Row1, Row1);

            L.M21 = Vector3F.Dot(Q.Row1, Row2);
            L.M22 = Vector3F.Dot(Q.Row2, Row2);

            L.M31 = Vector3F.Dot(Q.Row1, Row3);
            L.M32 = Vector3F.Dot(Q.Row2, Row3);
            L.M33 = Vector3F.Dot(Q.Row3, Row3);
        }

        /// <summary>
        /// Decomposes a <see cref="Matrix3F"/> into a scale, rotation, and translation.
        /// </summary>
        /// <param name="scale">When the method completes, contains the scaling component of the decomposed Matrix3x3.</param>
        /// <param name="rotation">When the method completes, contains the rotation component of the decomposed Matrix3x3.</param>
        /// <remarks>
        /// This method is designed to decompose an SRT transformation <see cref="Matrix3F"/> only.
        /// </remarks>
        public bool Decompose(out Vector3F scale, out QuaternionF rotation)
        {
            //Source: Unknown
            //References: http://www.gamedev.net/community/forums/topic.asp?topic_id=441695

            //Scaling is the length of the rows.
            scale.X =  float.Sqrt((M11 * M11) + (M12 * M12) + (M13 * M13));
            scale.Y =  float.Sqrt((M21 * M21) + (M22 * M22) + (M23 * M23));
            scale.Z =  float.Sqrt((M31 * M31) + (M32 * M32) + (M33 * M33));

            //If any of the scaling factors are zero, than the rotation <see cref="Matrix3F"/> can not exist.
            if (MathHelper.IsZero(scale.X) ||
                MathHelper.IsZero(scale.Y) ||
                MathHelper.IsZero(scale.Z))
            {
                rotation = QuaternionF.Identity;
                return false;
            }

            //The rotation is the left over <see cref="Matrix3F"/> after dividing out the scaling.
            Matrix3F rotationMatrix3x3 = new Matrix3F();
            rotationMatrix3x3.M11 = M11 / scale.X;
            rotationMatrix3x3.M12 = M12 / scale.X;
            rotationMatrix3x3.M13 = M13 / scale.X;

            rotationMatrix3x3.M21 = M21 / scale.Y;
            rotationMatrix3x3.M22 = M22 / scale.Y;
            rotationMatrix3x3.M23 = M23 / scale.Y;

            rotationMatrix3x3.M31 = M31 / scale.Z;
            rotationMatrix3x3.M32 = M32 / scale.Z;
            rotationMatrix3x3.M33 = M33 / scale.Z;

            rotation = QuaternionF.FromRotationMatrix(ref rotationMatrix3x3);
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
        public bool DecomposeUniformScale(out float scale, out QuaternionF rotation)
        {
            //Scaling is the length of the rows. ( just take one row since this is a uniform matrix)
            scale =  float.Sqrt((M11 * M11) + (M12 * M12) + (M13 * M13));
            var inv_scale = 1f / scale;

            //If any of the scaling factors are zero, then the rotation matrix can not exist.
            if (Math.Abs(scale) < MathHelper.Constants<float>.ZeroTolerance)
            {
                rotation = QuaternionF.Identity;
                return false;
            }

            //The rotation is the left over matrix after dividing out the scaling.
            Matrix3F rotationmatrix = new Matrix3F();
            rotationmatrix.M11 = M11 * inv_scale;
            rotationmatrix.M12 = M12 * inv_scale;
            rotationmatrix.M13 = M13 * inv_scale;

            rotationmatrix.M21 = M21 * inv_scale;
            rotationmatrix.M22 = M22 * inv_scale;
            rotationmatrix.M23 = M23 * inv_scale;

            rotationmatrix.M31 = M31 * inv_scale;
            rotationmatrix.M32 = M32 * inv_scale;
            rotationmatrix.M33 = M33 * inv_scale;

            rotation = QuaternionF.FromRotationMatrix(ref rotationmatrix);
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

            float temp0 = this[secondRow, 0];
            float temp1 = this[secondRow, 1];
            float temp2 = this[secondRow, 2];

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

            float temp0 = this[0, secondColumn];
            float temp1 = this[1, secondColumn];
            float temp2 = this[2, secondColumn];

            this[0, secondColumn] = this[0, firstColumn];
            this[1, secondColumn] = this[1, firstColumn];
            this[2, secondColumn] = this[2, firstColumn];

            this[0, firstColumn] = temp0;
            this[1, firstColumn] = temp1;
            this[2, firstColumn] = temp2;
        }

        /// <summary>
        /// Scales a <see cref="Matrix3F"/> by the given value.
        /// </summary>
        /// <param name="left">The <see cref="Matrix3F"/> to scale.</param>
        /// <param name="right">The amount by which to scale.</param>
        /// <returns>The scaled <see cref="Matrix3F"/>.</returns>
        public static Matrix3F Multiply(Matrix3F left, float right)
        {
            Multiply(ref left, right, out Matrix3F result);
            return result;
        }

        /// <summary>
        /// Determines the product of two matrices.
        /// </summary>
        /// <param name="left">The first <see cref="Matrix3F"/> to multiply.</param>
        /// <param name="right">The second <see cref="Matrix3F"/> to multiply.</param>
        /// <param name="result">The product of the two matrices.</param>
        public static void Multiply(ref Matrix3F left, ref Matrix3F right, out Matrix3F result)
        {
            Matrix3F temp = new Matrix3F();
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
        /// <param name="left">The first <see cref="Matrix3F"/> to multiply.</param>
        /// <param name="right">The second <see cref="Matrix3F"/> to multiply.</param>
        /// <returns>The product of the two matrices.</returns>
        public static Matrix3F Multiply(Matrix3F left, Matrix3F right)
        {
            Multiply(ref left, ref right, out Matrix3F result);
            return result;
        }

        /// <summary>
        /// Creates a skew symmetric matrix M from vector A such that M * B for some other vector B is equivalent to the cross product of A and B.
        /// </summary>
        /// <param name="v">Vector to base the matrix on.</param>
        /// <param name="result">Skew-symmetric matrix result.</param>
        public static void CrossProduct(ref Vector3F v, out Matrix3F result)
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
        public static void CreateOuterProduct(ref Vector3F a, ref Vector3F b, out Matrix3F result)
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
        /// Performs the exponential operation on a <see cref="Matrix3F"/>.
        /// </summary>
        /// <param name="value">The <see cref="Matrix3F"/> to perform the operation on.</param>
        /// <param name="exponent">The exponent to raise the <see cref="Matrix3F"/> to.</param>
        /// <param name="result">When the method completes, contains the exponential <see cref="Matrix3F"/>.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="exponent"/> is negative.</exception>
        public static void Exponent(ref Matrix3F value, int exponent, out Matrix3F result)
        {
            //Source: http://rosettacode.org
            //Reference: http://rosettacode.org/wiki/Matrix3x3-exponentiation_operator

            if (exponent < 0)
                throw new ArgumentOutOfRangeException("exponent", "The exponent can not be negative.");

            if (exponent == 0)
            {
                result = Matrix3F.Identity;
                return;
            }

            if (exponent == 1)
            {
                result = value;
                return;
            }

            Matrix3F identity = Matrix3F.Identity;
            Matrix3F temp = value;

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
        /// Performs the exponential operation on a <see cref="Matrix3F"/>.
        /// </summary>
        /// <param name="value">The <see cref="Matrix3F"/> to perform the operation on.</param>
        /// <param name="exponent">The exponent to raise the <see cref="Matrix3F"/> to.</param>
        /// <returns>The exponential <see cref="Matrix3F"/>.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="exponent"/> is negative.</exception>
        public static Matrix3F Exponent(Matrix3F value, int exponent)
        {
            Exponent(ref value, exponent, out Matrix3F result);
            return result;
        }

        /// <summary>
        /// Transforms the vector by the matrix's transpose.
        /// </summary>
        /// <param name="v">Vector3F to transform.</param>
        /// <param name="matrix">Matrix to use as the transformation transpose.</param>
        /// <param name="result">Product of the transformation.</param>
        public static void TransformTranspose(ref Vector3F v, ref Matrix3F matrix, out Vector3F result)
        {
            result = new Vector3F()
            {
                X = v.X * matrix.M11 + v.Y  * matrix.M12 + v.Z * matrix.M13,
                Y = v.X * matrix.M21 + v.Y  * matrix.M22 + v.Z * matrix.M23,
                Z = v.X * matrix.M31 + v.Y  * matrix.M32 + v.Z * matrix.M33
            };
        }

        /// <summary>
        /// Transforms the vector by the matrix's transpose.
        /// </summary>
        /// <param name="v">Vector3F to transform.</param>
        /// <param name="matrix">Matrix to use as the transformation transpose.</param>
        /// <returns>Product of the transformation.</returns>
        public static Vector3F TransformTranspose(Vector3F v, Matrix3F matrix)
        {
            return new Vector3F()
            {
                X = v.X * matrix.M11 + v.Y  * matrix.M12 + v.Z * matrix.M13,
                Y = v.X * matrix.M21 + v.Y  * matrix.M22 + v.Z * matrix.M23,
                Z = v.X * matrix.M31 + v.Y  * matrix.M32 + v.Z * matrix.M33
            };
        }

        /// <summary>
        /// Calculates the inverse of the specified <see cref="Matrix3F"/>.
        /// </summary>
        /// <param name="value">The <see cref="Matrix3F"/> whose inverse is to be calculated.</param>
        /// <param name="result">When the method completes, contains the inverse of the specified <see cref="Matrix3F"/>.</param>
        public static void Invert(ref Matrix3F value, out Matrix3F result)
        {
            float d11 = value.M22 * value.M33 + value.M23 * -value.M32;
            float d12 = value.M21 * value.M33 + value.M23 * -value.M31;
            float d13 = value.M21 * value.M32 + value.M22 * -value.M31;

            float det = value.M11 * d11 - value.M12 * d12 + value.M13 * d13;
            if (Math.Abs(det) == 0F)
            {
                result = Zero;
                return;
            }

            det = 1F / det;

            float d21 = value.M12 * value.M33 + value.M13 * -value.M32;
            float d22 = value.M11 * value.M33 + value.M13 * -value.M31;
            float d23 = value.M11 * value.M32 + value.M12 * -value.M31;

            float d31 = (value.M12 * value.M23) - (value.M13 * value.M22);
            float d32 = (value.M11 * value.M23) - (value.M13 * value.M21);
            float d33 = (value.M11 * value.M22) - (value.M12 * value.M21);

            result.M11 = +d11 * det; result.M12 = -d21 * det; result.M13 = +d31 * det;
            result.M21 = -d12 * det; result.M22 = +d22 * det; result.M23 = -d32 * det;
            result.M31 = +d13 * det; result.M32 = -d23 * det; result.M33 = +d33 * det;
        }

        /// <summary>
        /// Calculates the inverse of the specified <see cref="Matrix3F"/>.
        /// </summary>
        /// <param name="value">The <see cref="Matrix3F"/> whose inverse is to be calculated.</param>
        /// <returns>The inverse of the specified <see cref="Matrix3F"/>.</returns>
        public static Matrix3F Invert(Matrix3F value)
        {
            value.Invert();
            return value;
        }

        /// <summary>
        /// Orthogonalizes the specified <see cref="Matrix3F"/>.
        /// </summary>
        /// <param name="value">The <see cref="Matrix3F"/> to orthogonalize.</param>
        /// <param name="result">When the method completes, contains the orthogonalized <see cref="Matrix3F"/>.</param>
        /// <remarks>
        /// <para>Orthogonalization is the process of making all rows orthogonal to each other. This
        /// means that any given row in the <see cref="Matrix3F"/> will be orthogonal to any other given row in the
        /// <see cref="Matrix3F"/>.</para>
        /// <para>Because this method uses the modified Gram-Schmidt process, the resulting <see cref="Matrix3F"/>
        /// tends to be numerically unstable. The numeric stability decreases according to the rows
        /// so that the first row is the most stable and the last row is the least stable.</para>
        /// <para>This operation is performed on the rows of the <see cref="Matrix3F"/> rather than the columns.
        /// If you wish for this operation to be performed on the columns, first transpose the
        /// input and than transpose the output.</para>
        /// </remarks>
        public static void Orthogonalize(ref Matrix3F value, out Matrix3F result)
        {
            // Uses the modified Gram-Schmidt process.
            // q1 = m1
            // q2 = m2 - ((q1 ⋅ m2) / (q1 ⋅ q1)) * q1
            // q3 = m3 - ((q1 ⋅ m3) / (q1 ⋅ q1)) * q1 - ((q2 ⋅ m3) / (q2 ⋅ q2)) * q2

            //By separating the above algorithm into multiple lines, we actually increase accuracy.
            result = value;

            result.Row2 = result.Row2 - (Vector3F.Dot(result.Row1, result.Row2) / Vector3F.Dot(result.Row1, result.Row1)) * result.Row1;

            result.Row3 = result.Row3 - (Vector3F.Dot(result.Row1, result.Row3) / Vector3F.Dot(result.Row1, result.Row1)) * result.Row1;
            result.Row3 = result.Row3 - (Vector3F.Dot(result.Row2, result.Row3) / Vector3F.Dot(result.Row2, result.Row2)) * result.Row2;
        }

        /// <summary>
        /// Orthogonalizes the specified Matrix3x3.
        /// </summary>
        /// <param name="value">The <see cref="Matrix3F"/> to orthogonalize.</param>
        /// <returns>The orthogonalized Matrix3x3.</returns>
        /// <remarks>
        /// <para>Orthogonalization is the process of making all rows orthogonal to each other. This
        /// means that any given row in the <see cref="Matrix3F"/> will be orthogonal to any other given row in the
        /// Matrix3x3.</para>
        /// <para>Because this method uses the modified Gram-Schmidt process, the resulting Matrix3x3
        /// tends to be numerically unstable. The numeric stability decreases according to the rows
        /// so that the first row is the most stable and the last row is the least stable.</para>
        /// <para>This operation is performed on the rows of the <see cref="Matrix3F"/> rather than the columns.
        /// If you wish for this operation to be performed on the columns, first transpose the
        /// input and than transpose the output.</para>
        /// </remarks>
        public static Matrix3F Orthogonalize(Matrix3F value)
        {
            Orthogonalize(ref value, out Matrix3F result);
            return result;
        }

        /// <summary>
        /// Orthonormalizes the specified Matrix3x3.
        /// </summary>
        /// <param name="value">The <see cref="Matrix3F"/> to orthonormalize.</param>
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
        /// <para>This operation is performed on the rows of the <see cref="Matrix3F"/> rather than the columns.
        /// If you wish for this operation to be performed on the columns, first transpose the
        /// input and than transpose the output.</para>
        /// </remarks>
        public static void Orthonormalize(ref Matrix3F value, out Matrix3F result)
        {
            //Uses the modified Gram-Schmidt process.
            //Because we are making unit vectors, we can optimize the math for orthonormalization
            //and simplify the projection operation to remove the division.
            //q1 = m1 / |m1|
            //q2 = (m2 - (q1 ⋅ m2) * q1) / |m2 - (q1 ⋅ m2) * q1|
            //q3 = (m3 - (q1 ⋅ m3) * q1 - (q2 ⋅ m3) * q2) / |m3 - (q1 ⋅ m3) * q1 - (q2 ⋅ m3) * q2|

            //By separating the above algorithm into multiple lines, we actually increase accuracy.
            result = value;

            result.Row1 = Vector3F.Normalize(result.Row1);

            result.Row2 = result.Row2 - Vector3F.Dot(result.Row1, result.Row2) * result.Row1;
            result.Row2 = Vector3F.Normalize(result.Row2);

            result.Row3 = result.Row3 - Vector3F.Dot(result.Row1, result.Row3) * result.Row1;
            result.Row3 = result.Row3 - Vector3F.Dot(result.Row2, result.Row3) * result.Row2;
            result.Row3 = Vector3F.Normalize(result.Row3);
        }

        /// <summary>
        /// Orthonormalizes the specified Matrix3x3.
        /// </summary>
        /// <param name="value">The <see cref="Matrix3F"/> to orthonormalize.</param>
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
        /// <para>This operation is performed on the rows of the <see cref="Matrix3F"/> rather than the columns.
        /// If you wish for this operation to be performed on the columns, first transpose the
        /// input and than transpose the output.</para>
        /// </remarks>
        public static Matrix3F Orthonormalize(Matrix3F value)
        {
            Orthonormalize(ref value, out Matrix3F result);
            return result;
        }

        /// <summary>
        /// Brings the <see cref="Matrix3F"/> into upper triangular form using elementary row operations.
        /// </summary>
        /// <param name="value">The <see cref="Matrix3F"/> to put into upper triangular form.</param>
        /// <param name="result">When the method completes, contains the upper triangular Matrix3x3.</param>
        /// <remarks>
        /// If the <see cref="Matrix3F"/> is not invertible (i.e. its determinant is zero) than the result of this
        /// method may produce Single.Nan and Single.Inf values. When the <see cref="Matrix3F"/> represents a system
        /// of linear equations, than this often means that either no solution exists or an infinite
        /// number of solutions exist.
        /// </remarks>
        public static void UpperTriangularForm(ref Matrix3F value, out Matrix3F result)
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

                float multiplier = 1f / result[r, lead];

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
        /// Brings the <see cref="Matrix3F"/> into upper triangular form using elementary row operations.
        /// </summary>
        /// <param name="value">The <see cref="Matrix3F"/> to put into upper triangular form.</param>
        /// <returns>The upper triangular Matrix3x3.</returns>
        /// <remarks>
        /// If the <see cref="Matrix3F"/> is not invertible (i.e. its determinant is zero) than the result of this
        /// method may produce Single.Nan and Single.Inf values. When the <see cref="Matrix3F"/> represents a system
        /// of linear equations, than this often means that either no solution exists or an infinite
        /// number of solutions exist.
        /// </remarks>
        public static Matrix3F UpperTriangularForm(Matrix3F value)
        {
            UpperTriangularForm(ref value, out Matrix3F result);
            return result;
        }

        /// <summary>
        /// Brings the <see cref="Matrix3F"/> into lower triangular form using elementary row operations.
        /// </summary>
        /// <param name="value">The <see cref="Matrix3F"/> to put into lower triangular form.</param>
        /// <param name="result">When the method completes, contains the lower triangular Matrix3x3.</param>
        /// <remarks>
        /// If the <see cref="Matrix3F"/> is not invertible (i.e. its determinant is zero) than the result of this
        /// method may produce Single.Nan and Single.Inf values. When the <see cref="Matrix3F"/> represents a system
        /// of linear equations, than this often means that either no solution exists or an infinite
        /// number of solutions exist.
        /// </remarks>
        public static void LowerTriangularForm(ref Matrix3F value, out Matrix3F result)
        {
            //Adapted from the row echelon code.
            Matrix3F temp = value;
            Matrix3F.Transpose(ref temp, out result);

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

                float multiplier = 1f / result[r, lead];

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

            Matrix3F.Transpose(ref result, out result);
        }

        /// <summary>
        /// Brings the <see cref="Matrix3F"/> into lower triangular form using elementary row operations.
        /// </summary>
        /// <param name="value">The <see cref="Matrix3F"/> to put into lower triangular form.</param>
        /// <returns>The lower triangular Matrix3x3.</returns>
        /// <remarks>
        /// If the <see cref="Matrix3F"/> is not invertible (i.e. its determinant is zero) than the result of this
        /// method may produce Single.Nan and Single.Inf values. When the <see cref="Matrix3F"/> represents a system
        /// of linear equations, than this often means that either no solution exists or an infinite
        /// number of solutions exist.
        /// </remarks>
        public static Matrix3F LowerTriangularForm(Matrix3F value)
        {
            LowerTriangularForm(ref value, out Matrix3F result);
            return result;
        }

        /// <summary>
        /// Brings the <see cref="Matrix3F"/> into row echelon form using elementary row operations;
        /// </summary>
        /// <param name="value">The <see cref="Matrix3F"/> to put into row echelon form.</param>
        /// <param name="result">When the method completes, contains the row echelon form of the Matrix3x3.</param>
        public static void RowEchelonForm(ref Matrix3F value, out Matrix3F result)
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

                float multiplier = 1f / result[r, lead];
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
        /// Brings the <see cref="Matrix3F"/> into row echelon form using elementary row operations;
        /// </summary>
        /// <param name="value">The <see cref="Matrix3F"/> to put into row echelon form.</param>
        /// <returns>When the method completes, contains the row echelon form of the Matrix3x3.</returns>
        public static Matrix3F RowEchelonForm(Matrix3F value)
        {
            Matrix3F result;
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
        public static Matrix3F BillboardLH(ref Vector3F objectPosition, ref Vector3F cameraPosition, ref Vector3F cameraUpVector, ref Vector3F cameraForwardVector)
        {
            Vector3F difference = cameraPosition - objectPosition;

            float lengthSq = difference.LengthSquared();
            if (MathHelper.IsZero(lengthSq))
                difference = -cameraForwardVector;
            else
                difference *= (1.0F / float.Sqrt(lengthSq));

            Vector3F crossed = Vector3F.Cross(ref cameraUpVector, ref difference);
            crossed.Normalize();
            Vector3F final = Vector3F.Cross(ref difference, ref crossed);

            return new Matrix3F()
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
        public static Matrix3F BillboardLH(
            Vector3F objectPosition, 
            Vector3F cameraPosition, 
            Vector3F cameraUpVector, 
            Vector3F cameraForwardVector)
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
        public static Matrix3F BillboardRH(ref Vector3F objectPosition, ref Vector3F cameraPosition, ref Vector3F cameraUpVector, ref Vector3F cameraForwardVector)
        {
            Vector3F difference = objectPosition - cameraPosition;

            float lengthSq = difference.LengthSquared();
            if (MathHelper.IsZero(lengthSq))
                difference = -cameraForwardVector;
            else
                difference *= (1.0F / float.Sqrt(lengthSq));

            Vector3F crossed = Vector3F.Cross(ref cameraUpVector, ref difference);
            crossed.Normalize();
            Vector3F final = Vector3F.Cross(ref difference, ref crossed);

            return new Matrix3F()
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
        public static Matrix3F BillboardRH(Vector3F objectPosition, Vector3F cameraPosition, Vector3F cameraUpVector, Vector3F cameraForwardVector)
        {
            return BillboardRH(ref objectPosition, ref cameraPosition, ref cameraUpVector, ref cameraForwardVector);
        }

        /// <summary>
        /// Creates a left-handed, look-at Matrix3x3.
        /// </summary>
        /// <param name="eye">The position of the viewer's eye.</param>
        /// <param name="target">The camera look-at target.</param>
        /// <param name="up">The camera's up vector.</param>
        public static Matrix3F LookAtLH(ref Vector3F eye, ref Vector3F target, ref Vector3F up)
        {
            Vector3F zaxis = target - eye;
            zaxis.Normalize();

            Vector3F xaxis = Vector3F.Cross(ref up, ref zaxis); 
            xaxis.Normalize();

            Vector3F yaxis = Vector3F.Cross(ref zaxis, ref xaxis);

            Matrix3F result = Identity;
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
        public static Matrix3F LookAtLH(Vector3F eye, Vector3F target, Vector3F up)
        {
            return LookAtLH(ref eye, ref target, ref up);
        }

        /// <summary>
        /// Creates a right-handed, look-at Matrix3x3.
        /// </summary>
        /// <param name="eye">The position of the viewer's eye.</param>
        /// <param name="target">The camera look-at target.</param>
        /// <param name="up">The camera's up vector.</param>
        public static Matrix3F LookAtRH(ref Vector3F eye, ref Vector3F target, ref Vector3F up)
        {
            Vector3F zAxis = eye - target; 
            zAxis.Normalize();

            Vector3F xAxis = Vector3F.Cross(ref up, ref zAxis); 
            xAxis.Normalize();

            Vector3F yAxis = Vector3F.Cross(ref zAxis, ref xAxis);

            return new Matrix3F()
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
        public static Matrix3F LookAtRH(Vector3F eye, Vector3F target, Vector3F up)
        {
            return LookAtRH(ref eye, ref target, ref up);
        }

        /// <summary>
        /// Creates a <see cref="Matrix3F"/> that scales along the x-axis, y-axis, and y-axis.
        /// </summary>
        /// <param name="scale">Scaling factor for all three axes.</param>
        /// <param name="result">When the method completes, contains the created scaling Matrix3x3.</param>
        public static void CreateScale(ref Vector3F scale, out Matrix3F result)
        {
            CreateScale(scale.X, scale.Y, scale.Z, out result);
        }

        /// <summary>
        /// Creates a <see cref="Matrix3F"/> that scales along the x-axis, y-axis, and y-axis.
        /// </summary>
        /// <param name="scale">Scaling factor for all three axes.</param>
        /// <returns>The created scaling Matrix3x3.</returns>
        public static Matrix3F CreateScale(Vector3F scale)
        {
            CreateScale(ref scale, out Matrix3F result);
            return result;
        }

        /// <summary>
        /// Creates a <see cref="Matrix3F"/> that scales along the x-axis, y-axis, and y-axis.
        /// </summary>
        /// <param name="x">Scaling factor that is applied along the x-axis.</param>
        /// <param name="y">Scaling factor that is applied along the y-axis.</param>
        /// <param name="z">Scaling factor that is applied along the z-axis.</param>
        /// <param name="result">When the method completes, contains the created scaling Matrix3x3.</param>
        public static void CreateScale(float x, float y, float z, out Matrix3F result)
        {
            result = Identity;
            result.M11 = x;
            result.M22 = y;
            result.M33 = z;
        }

        /// <summary>
        /// Creates a <see cref="Matrix3F"/> that scales along the x-axis, y-axis, and y-axis.
        /// </summary>
        /// <param name="x">Scaling factor that is applied along the x-axis.</param>
        /// <param name="y">Scaling factor that is applied along the y-axis.</param>
        /// <param name="z">Scaling factor that is applied along the z-axis.</param>
        /// <returns>The created scaling Matrix3x3.</returns>
        public static Matrix3F CreateScale(float x, float y, float z)
        {
            CreateScale(x, y, z, out Matrix3F result);
            return result;
        }

        /// <summary>
        /// Creates a <see cref="Matrix3F"/> that uniformly scales along all three axis.
        /// </summary>
        /// <param name="scale">The uniform scale that is applied along all axis.</param>
        /// <param name="result">When the method completes, contains the created scaling Matrix3x3.</param>
        public static void CreateScale(float scale, out Matrix3F result)
        {
            result = Identity;
            result.M11 = result.M22 = result.M33 = scale;
        }

        /// <summary>
        /// Creates a <see cref="Matrix3F"/> that uniformly scales along all three axis.
        /// </summary>
        /// <param name="scale">The uniform scale that is applied along all axis.</param>
        /// <returns>The created scaling Matrix3x3.</returns>
        public static Matrix3F CreateScale(float scale)
        {
            CreateScale(scale, out Matrix3F result);
            return result;
        }

        /// <summary>
        /// Creates a <see cref="Matrix3F"/> that rotates around the x-axis.
        /// </summary>
        /// <param name="angle">Angle of rotation in radians. Angles are measured clockwise when looking along the rotation axis toward the origin.</param>
        /// <param name="result">When the method completes, contains the created rotation Matrix3x3.</param>
        public static void RotationX(float angle, out Matrix3F result)
        {
            float cos =  float.Cos(angle);
            float sin =  float.Sin(angle);

            result = Identity;
            result.M22 = cos;
            result.M23 = sin;
            result.M32 = -sin;
            result.M33 = cos;
        }

        /// <summary>
        /// Creates a <see cref="Matrix3F"/> that rotates around the x-axis.
        /// </summary>
        /// <param name="angle">Angle of rotation in radians. Angles are measured clockwise when looking along the rotation axis toward the origin.</param>
        /// <returns>The created rotation Matrix3x3.</returns>
        public static Matrix3F RotationX(float angle)
        {
            RotationX(angle, out Matrix3F result);
            return result;
        }

        /// <summary>
        /// Creates a <see cref="Matrix3F"/> that rotates around the y-axis.
        /// </summary>
        /// <param name="angle">Angle of rotation in radians. Angles are measured clockwise when looking along the rotation axis toward the origin.</param>
        /// <param name="result">When the method completes, contains the created rotation Matrix3x3.</param>
        public static void RotationY(float angle, out Matrix3F result)
        {
            float cos =  float.Cos(angle);
            float sin =  float.Sin(angle);

            result = Identity;
            result.M11 = cos;
            result.M13 = -sin;
            result.M31 = sin;
            result.M33 = cos;
        }

        /// <summary>
        /// Creates a <see cref="Matrix3F"/> that rotates around the y-axis.
        /// </summary>
        /// <param name="angle">Angle of rotation in radians. Angles are measured clockwise when looking along the rotation axis toward the origin.</param>
        /// <returns>The created rotation Matrix3x3.</returns>
        public static Matrix3F RotationY(float angle)
        {
            RotationY(angle, out Matrix3F result);
            return result;
        }

        /// <summary>
        /// Creates a <see cref="Matrix3F"/> that rotates around the z-axis.
        /// </summary>
        /// <param name="angle">Angle of rotation in radians. Angles are measured clockwise when looking along the rotation axis toward the origin.</param>
        /// <param name="result">When the method completes, contains the created rotation Matrix3x3.</param>
        public static void RotationZ(float angle, out Matrix3F result)
        {
            float cos =  float.Cos(angle);
            float sin =  float.Sin(angle);

            result = Identity;
            result.M11 = cos;
            result.M12 = sin;
            result.M21 = -sin;
            result.M22 = cos;
        }

        /// <summary>
        /// Creates a <see cref="Matrix3F"/> that rotates around the z-axis.
        /// </summary>
        /// <param name="angle">Angle of rotation in radians. Angles are measured clockwise when looking along the rotation axis toward the origin.</param>
        /// <returns>The created rotation Matrix3x3.</returns>
        public static Matrix3F RotationZ(float angle)
        {
            RotationZ(angle, out Matrix3F result);
            return result;
        }

        /// <summary>
        /// Creates a <see cref="Matrix3F"/> that rotates around an arbitrary axis.
        /// </summary>
        /// <param name="axis">The axis around which to rotate. This parameter is assumed to be normalized.</param>
        /// <param name="angle">Angle of rotation in radians. Angles are measured clockwise when looking along the rotation axis toward the origin.</param>
        /// <param name="result">When the method completes, contains the created rotation Matrix3x3.</param>
        public static void RotationAxis(ref Vector3F axis, float angle, out Matrix3F result)
        {
            float x = axis.X;
            float y = axis.Y;
            float z = axis.Z;
            float cos =  float.Cos(angle);
            float sin =  float.Sin(angle);
            float xx = x * x;
            float yy = y * y;
            float zz = z * z;
            float xy = x * y;
            float xz = x * z;
            float yz = y * z;

            result = Identity;
            result.M11 = xx + (cos * (1.0F - xx));
            result.M12 = (xy - (cos * xy)) + (sin * z);
            result.M13 = (xz - (cos * xz)) - (sin * y);
            result.M21 = (xy - (cos * xy)) - (sin * z);
            result.M22 = yy + (cos * (1.0F - yy));
            result.M23 = (yz - (cos * yz)) + (sin * x);
            result.M31 = (xz - (cos * xz)) + (sin * y);
            result.M32 = (yz - (cos * yz)) - (sin * x);
            result.M33 = zz + (cos * (1.0F - zz));
        }

        /// <summary>
        /// Creates a <see cref="Matrix3F"/> that rotates around an arbitrary axis.
        /// </summary>
        /// <param name="axis">The axis around which to rotate. This parameter is assumed to be normalized.</param>
        /// <param name="angle">Angle of rotation in radians. Angles are measured clockwise when looking along the rotation axis toward the origin.</param>
        /// <returns>The created rotation Matrix3x3.</returns>
        public static Matrix3F RotationAxis(Vector3F axis, float angle)
        {
            RotationAxis(ref axis, angle, out Matrix3F result);
            return result;
        }

        /// <summary>
        /// Creates a rotation <see cref="Matrix3F"/> from a quaternion.
        /// </summary>
        /// <param name="rotation">The quaternion to use to build the Matrix3x3.</param>
        public static void FromQuaternion(ref QuaternionF rotation, out Matrix3F result)
        {
            float xx = rotation.X * rotation.X;
            float yy = rotation.Y * rotation.Y;
            float zz = rotation.Z * rotation.Z;
            float xy = rotation.X * rotation.Y;
            float zw = rotation.Z * rotation.W;
            float zx = rotation.Z * rotation.X;
            float yw = rotation.Y * rotation.W;
            float yz = rotation.Y * rotation.Z;
            float xw = rotation.X * rotation.W;

            result.M11 = 1.0F - (2.0F * (yy + zz));
            result.M12 = 2.0F * (xy + zw);
            result.M13 = 2.0F * (zx - yw);
            result.M21 = 2.0F * (xy - zw);
            result.M22 = 1.0F - (2.0F * (zz + xx));
            result.M23 = 2.0F * (yz + xw);
            result.M31 = 2.0F * (zx + yw);
            result.M32 = 2.0F * (yz - xw);
            result.M33 = 1.0F - (2.0F * (yy + xx));
        }

        /// <summary>
        /// Creates a rotation <see cref="Matrix3F"/> from a quaternion.
        /// </summary>
        /// <param name="rotation">The quaternion to use to build the Matrix3x3.</param>
        /// <returns>The created rotation Matrix3x3.</returns>
        public static Matrix3F FromQuaternion(QuaternionF rotation)
        {
            FromQuaternion(ref rotation, out Matrix3F result);
            return result;
        }

        /// <summary>
        /// Creates a rotation <see cref="Matrix3F"/> with a specified yaw, pitch, and roll.
        /// </summary>
        /// <param name="yaw">Yaw around the y-axis, in radians.</param>
        /// <param name="pitch">Pitch around the x-axis, in radians.</param>
        /// <param name="roll">Roll around the z-axis, in radians.</param>
        /// <param name="result">When the method completes, contains the created rotation Matrix3x3.</param>
        public static void RotationYawPitchRoll(float yaw, float pitch, float roll, out Matrix3F result)
        {
            QuaternionF quaternion = QuaternionF.RotationYawPitchRoll(yaw, pitch, roll);
            FromQuaternion(ref quaternion, out result);
        }

        /// <summary>
        /// Creates a rotation <see cref="Matrix3F"/> with a specified yaw, pitch, and roll.
        /// </summary>
        /// <param name="yaw">Yaw around the y-axis, in radians.</param>
        /// <param name="pitch">Pitch around the x-axis, in radians.</param>
        /// <param name="roll">Roll around the z-axis, in radians.</param>
        /// <param name="result">When the method completes, contains the created rotation Matrix3x3.</param>
        public static Matrix3F RotationYawPitchRoll(float yaw, float pitch, float roll)
        {
            QuaternionF quaternion = QuaternionF.RotationYawPitchRoll(yaw, pitch, roll);
            FromQuaternion(ref quaternion, out Matrix3F result);
            return result;
        }

        /// <summary>
        /// Multiplies a transposed matrix with another matrix.
        /// </summary>
        /// <param name="matrix">Matrix to be multiplied.</param>
        /// <param name="transpose">Matrix to be transposed and multiplied.</param>
        /// <param name="result">Product of the multiplication.</param>
        public static void MultiplyTransposed(ref Matrix3F transpose, ref Matrix3F matrix, out Matrix3F result)
        {
            float resultM11 = transpose.M11 * matrix.M11 + transpose.M21 * matrix.M21 + transpose.M31 * matrix.M31;
            float resultM12 = transpose.M11 * matrix.M12 + transpose.M21 * matrix.M22 + transpose.M31 * matrix.M32;
            float resultM13 = transpose.M11 * matrix.M13 + transpose.M21 * matrix.M23 + transpose.M31 * matrix.M33;

            float resultM21 = transpose.M12 * matrix.M11 + transpose.M22 * matrix.M21 + transpose.M32 * matrix.M31;
            float resultM22 = transpose.M12 * matrix.M12 + transpose.M22 * matrix.M22 + transpose.M32 * matrix.M32;
            float resultM23 = transpose.M12 * matrix.M13 + transpose.M22 * matrix.M23 + transpose.M32 * matrix.M33;

            float resultM31 = transpose.M13 * matrix.M11 + transpose.M23 * matrix.M21 + transpose.M33 * matrix.M31;
            float resultM32 = transpose.M13 * matrix.M12 + transpose.M23 * matrix.M22 + transpose.M33 * matrix.M32;
            float resultM33 = transpose.M13 * matrix.M13 + transpose.M23 * matrix.M23 + transpose.M33 * matrix.M33;

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
        public static void MultiplyByTransposed(ref Matrix3F matrix, ref Matrix3F transpose, out Matrix3F result)
        {
            float resultM11 = matrix.M11 * transpose.M11 + matrix.M12 * transpose.M12 + matrix.M13 * transpose.M13;
            float resultM12 = matrix.M11 * transpose.M21 + matrix.M12 * transpose.M22 + matrix.M13 * transpose.M23;
            float resultM13 = matrix.M11 * transpose.M31 + matrix.M12 * transpose.M32 + matrix.M13 * transpose.M33;

            float resultM21 = matrix.M21 * transpose.M11 + matrix.M22 * transpose.M12 + matrix.M23 * transpose.M13;
            float resultM22 = matrix.M21 * transpose.M21 + matrix.M22 * transpose.M22 + matrix.M23 * transpose.M23;
            float resultM23 = matrix.M21 * transpose.M31 + matrix.M22 * transpose.M32 + matrix.M23 * transpose.M33;

            float resultM31 = matrix.M31 * transpose.M11 + matrix.M32 * transpose.M12 + matrix.M33 * transpose.M13;
            float resultM32 = matrix.M31 * transpose.M21 + matrix.M32 * transpose.M22 + matrix.M33 * transpose.M23;
            float resultM33 = matrix.M31 * transpose.M31 + matrix.M32 * transpose.M32 + matrix.M33 * transpose.M33;

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
        public static void AdaptiveInvert(ref Matrix3F matrix, out Matrix3F result)
        {
            int submatrix;
            float determinantInverse = 1 / matrix.AdaptiveDeterminant(out submatrix);
            float m11, m12, m13, m21, m22, m23, m31, m32, m33;
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
        public float AdaptiveDeterminant(out int subMatrixCode)
        {
            //Try the full matrix first.
            float determinant = M11 * M22 * M33 + M12 * M23 * M31 + M13 * M21 * M32 -
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
        /// <param name="left">The first <see cref="Matrix3F"/> to multiply.</param>
        /// <param name="right">The second <see cref="Matrix3F"/> to multiply.</param>
        /// <returns>The product of the two matrices.</returns>
        public static Matrix3F operator *(Matrix3F left, Matrix3F right)
        {
            Multiply(ref left, ref right, out Matrix3F result);
            return result;
        }

        /// <summary>
        /// Creates a 4x4 matrix from a <see cref="Matrix3F"/>.
        /// </summary>
        /// <param name="a">3x3 matrix.</param>
        /// <param name="b">Created 4x4 matrix.</param>
        public static void To4x4(ref Matrix3F a, out Matrix4F b)
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
        /// Creates a 4x4 matrix from a <see cref="Matrix3F"/>.
        /// </summary>
        /// <param name="a"><see cref="Matrix3F"/>.</param>
        /// <returns>Created 4x4 matrix.</returns>
        public static Matrix4F To4x4(Matrix3F a)
        {
            Matrix4F b;

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
        /// Creates a <see cref="Matrix3F"/> from an 4x4 matrix.
        /// </summary>
        /// <param name="matrix4X4">Matrix to extract a 3x3 matrix from.</param>
        /// <param name="matrix3X3">Upper 3x3 matrix extracted from the matrix.</param>
        public static void From4x4(ref Matrix4F matrix4X4, out Matrix3F matrix3X3)
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
        /// Creates a <see cref="Matrix3F"/> from an 4x4 matrix.
        /// </summary>
        /// <param name="matrix4X4">Matrix to extract a 3x3 matrix from.</param>
        /// <returns>Upper 3x3 matrix extracted from the matrix.</returns>
        public static Matrix3F From4x4(Matrix4F matrix4X4)
        {
            Matrix3F matrix3X3;
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
        /// Convert the <see cref="Matrix3F"/> to a 4x4 Matrix.
        /// </summary>
        /// <returns>A 4x4 Matrix with zero translation and M44=1</returns>
        public static explicit operator Matrix4F(Matrix3F Value)
        {
            return new Matrix4F(
                Value.M11, Value.M12, Value.M13, 0,
                Value.M21, Value.M22, Value.M23, 0,
                Value.M31, Value.M32, Value.M33, 0,
                0, 0, 0, 1
                );
        }

        /// <summary>
        /// Convert the 4x4 Matrix to a <see cref="Matrix3F"/>.
        /// </summary>
        /// <returns>A 3x3 Matrix</returns>
        public static explicit operator Matrix3F(Matrix4F Value)
        {
            return new Matrix3F(
                Value.M11, Value.M12, Value.M13,
                Value.M21, Value.M22, Value.M23,
                Value.M31, Value.M32, Value.M33
                );
        }

        /// <summary>
        /// Gets or sets the backward vector of the <see cref="Matrix3F"/>.
        /// </summary>
        public Vector3F Backward
        {
            get => new Vector3F(M31, M32, M33);
            set
            {
                M31 = value.X;
                M32 = value.Y;
                M33 = value.Z;
            }
        }

        /// <summary>
        /// Gets or sets the down vector of the <see cref="Matrix3F"/>.
        /// </summary>
        public Vector3F Down
        {
            get => new Vector3F(-M21, -M22, -M23);
            set
            {
                M21 = -value.X;
                M22 = -value.Y;
                M23 = -value.Z;
            }
        }

        /// <summary>
        /// Gets or sets the forward vector of the <see cref="Matrix3F"/>.
        /// </summary>
        public Vector3F Forward
        {
            get => new Vector3F(-M31, -M32, -M33);
            set
            {
                M31 = -value.X;
                M32 = -value.Y;
                M33 = -value.Z;
            }
        }

        /// <summary>
        /// Gets or sets the left vector of the <see cref="Matrix3F"/>.
        /// </summary>
        public Vector3F Left
        {
            get => new Vector3F(-M11, -M12, -M13);
            set
            {
                M11 = -value.X;
                M12 = -value.Y;
                M13 = -value.Z;
            }
        }

        /// <summary>
        /// Gets or sets the right vector of the <see cref="Matrix3F"/>.
        /// </summary>
        public Vector3F Right
        {
            get => new Vector3F(M11, M12, M13);
            set
            {
                M11 = value.X;
                M12 = value.Y;
                M13 = value.Z;
            }
        }

        /// <summary>
        /// Gets or sets the up vector of the <see cref="Matrix3F"/>.
        /// </summary>
        public Vector3F Up
        {
            get => new Vector3F(M21, M22, M23);
            set
            {
                M21 = value.X;
                M22 = value.Y;
                M23 = value.Z;
            }
        }
    }
}

