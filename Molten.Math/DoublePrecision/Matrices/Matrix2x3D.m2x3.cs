namespace Molten.DoublePrecision
{
	public partial struct Matrix2x3D
    {
        /// <summary>
        /// Multiplies the two matrices.
        /// </summary>
        /// <param name="a">First matrix to multiply.</param>
        /// <param name="b">Second matrix to multiply.</param>
        /// <param name="result">Product of the multiplication.</param>
        public static void Multiply(ref Matrix2x3D a, ref Matrix4D b, out Matrix2x3D result)
        {
            double resultM11 = a.M11 * b.M11 + a.M12 * b.M21 + a.M13 * b.M31;
            double resultM12 = a.M11 * b.M12 + a.M12 * b.M22 + a.M13 * b.M32;
            double resultM13 = a.M11 * b.M13 + a.M12 * b.M23 + a.M13 * b.M33;

            double resultM21 = a.M21 * b.M11 + a.M22 * b.M21 + a.M23 * b.M31;
            double resultM22 = a.M21 * b.M12 + a.M22 * b.M22 + a.M23 * b.M32;
            double resultM23 = a.M21 * b.M13 + a.M22 * b.M23 + a.M23 * b.M33;

            result.M11 = resultM11;
            result.M12 = resultM12;
            result.M13 = resultM13;

            result.M21 = resultM21;
            result.M22 = resultM22;
            result.M23 = resultM23;
        }

        /// <summary>
        /// Transforms the vector by the matrix.
        /// </summary>
        /// <param name="v">Vector2 to transform. Considered to be a row vector for purposes of multiplication.</param>
        /// <param name="matrix">Matrix to use as the transformation.</param>
        /// <param name="result">Row vector product of the transformation.</param>
        public static void Transform(ref Vector2D v, ref Matrix2x3D matrix, out Vector3D result)
        {
#if !WINDOWS
            result = new Vector3D();
#endif
            result.X = v.X * matrix.M11 + v.Y * matrix.M21;
            result.Y = v.X * matrix.M12 + v.Y * matrix.M22;
            result.Z = v.X * matrix.M13 + v.Y * matrix.M23;
        }

        /// <summary>
        /// Transforms the vector by the matrix.
        /// </summary>
        /// <param name="v">Vector2 to transform. Considered to be a column vector for purposes of multiplication.</param>
        /// <param name="matrix">Matrix to use as the transformation.</param>
        /// <param name="result">Column vector product of the transformation.</param>
        public static void Transform(ref Vector3D v, ref Matrix2x3D matrix, out Vector2D result)
        {
#if !WINDOWS
            result = Vector2D();
#endif
            result.X = matrix.M11 * v.X + matrix.M12 * v.Y + matrix.M13 * v.Z;
            result.Y = matrix.M21 * v.X + matrix.M22 * v.Y + matrix.M23 * v.Z;
        }
    }
}

