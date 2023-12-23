namespace Molten.DoublePrecision
{
	public partial struct Matrix2D
    {
        /// <summary>
        /// Adds the two matrices together on a per-element basis.
        /// </summary>
        /// <param name="a">First matrix to add.</param>
        /// <param name="b">Second matrix to add.</param>
        /// <param name="result">Sum of the two matrices.</param>
        public static void Add(ref Matrix4D a, ref Matrix2D b, out Matrix2D result)
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
        public static void Add(ref Matrix2D a, ref Matrix4D b, out Matrix2D result)
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
        public static void Add(ref Matrix4D a, ref Matrix4D b, out Matrix2D result)
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
        public static void Multiply(ref Matrix2D a, ref Matrix4D b, out Matrix2D result)
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
        public static void Multiply(ref Matrix4D a, ref Matrix2D b, out Matrix2D result)
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
        public static void Multiply(ref Matrix2x3F a, ref Matrix3x2D b, out Matrix2D result)
        {
            result.M11 = a.M11 * b.M11 + a.M12 * b.M21 + a.M13 * b.M31;
            result.M12 = a.M11 * b.M12 + a.M12 * b.M22 + a.M13 * b.M32;
            result.M21 = a.M21 * b.M11 + a.M22 * b.M21 + a.M23 * b.M31;
            result.M22 = a.M21 * b.M12 + a.M22 * b.M22 + a.M23 * b.M32;
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

            result.X = vX * matrix.M11 + vY * matrix.M21;
            result.Y = vX * matrix.M12 + vY * matrix.M22;
        }

        /// <summary>
        /// Calculates the determinant of the matrix.
        /// </summary>
        /// <returns>The matrix's determinant.</returns>
        public double Determinant()
        {
            return M11 * M22 - M12 * M21;
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

