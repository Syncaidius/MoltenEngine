﻿
namespace Molten.DoublePrecision
{
    /// <summary>
    /// 2 row, 2 column matrix.
    /// </summary>
    public struct Matrix2D : ITransposedMatrix<Matrix2D>
    {
        /// <summary>
        /// A single-precision Matrix2x2 with values intialized to the identity of a 2 x 2 matrix
        /// </summary>
        public static readonly Matrix2D Identity = new Matrix2D(1, 0, 0, 1);

        /// <summary>
        /// Value at row 1, column 1 of the matrix.
        /// </summary>
        public double M11;

        /// <summary>
        /// Value at row 1, column 2 of the matrix.
        /// </summary>
        public double M12;

        /// <summary>
        /// Value at row 2, column 1 of the matrix.
        /// </summary>
        public double M21;

        /// <summary>
        /// Value at row 2, column 2 of the matrix.
        /// </summary>
        public double M22;


        /// <summary>
        /// Constructs a new 2 row, 2 column matrix.
        /// </summary>
        /// <param name="m11">Value at row 1, column 1 of the matrix.</param>
        /// <param name="m12">Value at row 1, column 2 of the matrix.</param>
        /// <param name="m21">Value at row 2, column 1 of the matrix.</param>
        /// <param name="m22">Value at row 2, column 2 of the matrix.</param>
        public Matrix2D(double m11, double m12, double m21, double m22)
        {
            M11 = m11;
            M12 = m12;
            M21 = m21;
            M22 = m22;
        }

        /// <summary>
        /// Adds the two matrices together on a per-element basis.
        /// </summary>
        /// <param name="a">First matrix to add.</param>
        /// <param name="b">Second matrix to add.</param>
        /// <param name="result">Sum of the two matrices.</param>
        public static void Add(ref Matrix2D a, ref Matrix2D b, out Matrix2D result)
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
        public static void Add(ref Matrix4F a, ref Matrix2D b, out Matrix2D result)
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
        public static void Add(ref Matrix2D a, ref Matrix4F b, out Matrix2D result)
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
        public static void Add(ref Matrix4F a, ref Matrix4F b, out Matrix2D result)
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
        public static void CreateScale(double scale, out Matrix2D matrix)
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
        public static void Invert(ref Matrix2D matrix, out Matrix2D result)
        {
            double determinantInverse = 1 / (matrix.M11 * matrix.M22 - matrix.M12 * matrix.M21);
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
        public static void Multiply(ref Matrix2D a, ref Matrix2D b, out Matrix2D result)
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
        public static void Multiply(ref Matrix2D a, ref Matrix4F b, out Matrix2D result)
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
        public static void Multiply(ref Matrix4F a, ref Matrix2D b, out Matrix2D result)
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
        public static void Multiply(ref Matrix2x3F a, ref Matrix3x2F b, out Matrix2D result)
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
        public static void Negate(ref Matrix2D matrix, out Matrix2D result)
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
        public static void Subtract(ref Matrix2D a, ref Matrix2D b, out Matrix2D result)
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
        public static void Transform(ref Vector2D v, ref Matrix2D matrix, out Vector2D result)
        {
            double vX = v.X;
            double vY = v.Y;
#if !WINDOWS
            result = new Vector2D();
#endif
            result.X = vX * matrix.M11 + vY * matrix.M21;
            result.Y = vX * matrix.M12 + vY * matrix.M22;
        }

        public double[] ToArray()
        {
            return new[] { M11, M12, M21, M22 };
        }

        public void Transpose(out Matrix2D result)
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
        public static void Transpose(ref Matrix2D matrix, out Matrix2D result)
        {
            matrix.Transpose(out result);
        }
        
        /// <summary>
        /// Transposes the matrix in-place.
        /// </summary>
        public void Transpose()
        {
            double m21 = M21;
            M21 = M12;
            M12 = m21;
        }      

        /// <summary>
        /// Creates a string representation of the matrix.
        /// </summary>
        /// <returns>A string representation of the matrix.</returns>
        public override string ToString()
        {
            return "{" + M11 + ", " + M12 + "} " +
                   "{" + M21 + ", " + M22 + "}";
        }

        /// <summary>
        /// Calculates the determinant of the matrix.
        /// </summary>
        /// <returns>The matrix's determinant.</returns>
        public double Determinant()
        {
            return M11 * M22 - M12 * M21;
        }

        public override bool Equals(object obj)
        {
            if (obj is Matrix2D m)
                return this == m;
            else
                return false;
        }

        public static bool operator ==(Matrix2D matrix1, Matrix2D matrix2)
        {
            return MathHelper.NearEqual(matrix1.M11, matrix2.M11)
                && MathHelper.NearEqual(matrix1.M21, matrix2.M21)
                && MathHelper.NearEqual(matrix1.M12, matrix2.M12)
                && MathHelper.NearEqual(matrix1.M22, matrix2.M22);
        }

        public static bool operator !=(Matrix2D matrix1, Matrix2D matrix2)
        {
            return !MathHelper.NearEqual(matrix1.M11, matrix2.M11)
                || !MathHelper.NearEqual(matrix1.M21, matrix2.M21)
                || !MathHelper.NearEqual(matrix1.M12, matrix2.M12)
                || !MathHelper.NearEqual(matrix1.M22, matrix2.M22);
        }

        public static Matrix2D operator +(Matrix2D matrix1, Matrix2D matrix2)
        {
            return new Matrix2D()
            {
                M11 = matrix1.M11 + matrix2.M11,
                M12 = matrix1.M12 + matrix2.M12,
                M21 = matrix1.M21 + matrix2.M21,
                M22 = matrix1.M22 + matrix2.M22,
            };
        }

        public static Matrix2D operator -(Matrix2D matrix1, Matrix2D matrix2)
        {
            return new Matrix2D()
            {
                M11 = matrix1.M11 - matrix2.M11,
                M12 = matrix1.M12 - matrix2.M12,
                M21 = matrix1.M21 - matrix2.M21,
                M22 = matrix1.M22 - matrix2.M22,
            };
        }

        public static Matrix2D operator *(Matrix2D matrix, double scalar)
        {
            return new Matrix2D()
            {
                M11 = matrix.M11 * scalar,
                M12 = matrix.M12 * scalar,
                M21 = matrix.M21 * scalar,
                M22 = matrix.M22 * scalar,
            };
        }

        public static Matrix2D operator *(double scalar, Matrix2D matrix)
        {
            return new Matrix2D()
            {
                M11 = scalar * matrix.M11,
                M12 = scalar * matrix.M12,
                M21 = scalar * matrix.M21,
                M22 = scalar * matrix.M22,
            };
        }

        public static Matrix2D operator *(Matrix2D matrix1, Matrix2D matrix2)
        {
            return new Matrix2D()
            {
                M11 = matrix1.M11 * matrix2.M11 + matrix1.M21 * matrix2.M12,
                M12 = matrix1.M12 * matrix2.M11 + matrix1.M22 * matrix2.M12,
                M21 = matrix1.M11 * matrix2.M21 + matrix1.M21 * matrix2.M22,
                M22 = matrix1.M12 * matrix2.M21 + matrix1.M22 * matrix2.M22,
            };
        }
        public static Matrix3x2D operator *(Matrix2D matrix1, Matrix3x2D matrix2)
        {
            return new Matrix3x2D()
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
