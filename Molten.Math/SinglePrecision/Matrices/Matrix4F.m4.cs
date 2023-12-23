namespace Molten
{
    public partial struct Matrix4F
    {     
        /// <summary>
        /// Gets or sets the up <see cref="Vector3F"/> of the matrix; that is M21, M22, and M23.
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
    
        /// <summary>
        /// Gets or sets the down <see cref="Vector3F"/> of the matrix; that is -M21, -M22, and -M23.
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
        /// Gets or sets the right <see cref="Vector3F"/> of the matrix; that is M11, M12, and M13.
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
        /// Gets or sets the left <see cref="Vector3F"/> of the matrix; that is -M11, -M12, and -M13.
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
        /// Gets or sets the forward <see cref="Vector3F"/> of the matrix; that is -M31, -M32, and -M33.
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
        /// Gets or sets the backward <see cref="Vector3F"/> of the matrix; that is M31, M32, and M33.
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
        /// Gets or sets the first row in the matrix; that is M11, M12, M13, and M14.
        /// </summary>
        public Vector4F Row1
        {
            get => new Vector4F(M11, M12, M13, M14); 
            set { M11 = value.X; M12 = value.Y; M13 = value.Z; M14 = value.W; }
        }

        /// <summary>
        /// Gets or sets the second row in the matrix; that is M21, M22, M23, and M24.
        /// </summary>
        public Vector4F Row2
        {
            get => new Vector4F(M21, M22, M23, M24); 
            set { M21 = value.X; M22 = value.Y; M23 = value.Z; M24 = value.W; }
        }

        /// <summary>
        /// Gets or sets the third row in the matrix; that is M31, M32, M33, and M34.
        /// </summary>
        public Vector4F Row3
        {
            get => new Vector4F(M31, M32, M33, M34);
            set { M31 = value.X; M32 = value.Y; M33 = value.Z; M34 = value.W; }
        }

        /// <summary>
        /// Gets or sets the fourth row in the matrix; that is M41, M42, M43, and M44.
        /// </summary>
        public Vector4F Row4
        {
            get => new Vector4F(M41, M42, M43, M44);
            set { M41 = value.X; M42 = value.Y; M43 = value.Z; M44 = value.W; }
        }

        /// <summary>
        /// Gets or sets the first column in the matrix; that is M11, M21, M31, and M41.
        /// </summary>
        public Vector4F Column1
        {
            get => new Vector4F(M11, M21, M31, M41);
            set { M11 = value.X; M21 = value.Y; M31 = value.Z; M41 = value.W; }
        }

        /// <summary>
        /// Gets or sets the second column in the matrix; that is M12, M22, M32, and M42.
        /// </summary>
        public Vector4F Column2
        {
            get => new Vector4F(M12, M22, M32, M42);
            set { M12 = value.X; M22 = value.Y; M32 = value.Z; M42 = value.W; }
        }

        /// <summary>
        /// Gets or sets the third column in the matrix; that is M13, M23, M33, and M43.
        /// </summary>
        public Vector4F Column3
        {
            get => new Vector4F(M13, M23, M33, M43); 
            set { M13 = value.X; M23 = value.Y; M33 = value.Z; M43 = value.W; }
        }

        /// <summary>
        /// Gets or sets the fourth column in the matrix; that is M14, M24, M34, and M44.
        /// </summary>
        public Vector4F Column4
        {
            get => new Vector4F(M14, M24, M34, M44); 
            set { M14 = value.X; M24 = value.Y; M34 = value.Z; M44 = value.W; }
        }

        /// <summary>
        /// Gets or sets the translation of the matrix; that is M41, M42, and M43.
        /// </summary>
        public Vector3F Translation
        {
            get => new Vector3F(M41, M42, M43); 
            set { M41 = value.X; M42 = value.Y; M43 = value.Z; }
        }

        /// <summary>
        /// Gets or sets the scale of the matrix; that is M11, M22, and M33.
        /// </summary>
        public Vector3F ScaleVector
        {
            get => new Vector3F(M11, M22, M33); 
            set { M11 = value.X; M22 = value.Y; M33 = value.Z; }
        }

        /// <summary>
        /// Calculates the determinant of the matrix.
        /// </summary>
        /// <returns>The determinant of the matrix.</returns>
        public float Determinant()
        {
            float temp1 = (M33 * M44) - (M34 * M43);
            float temp2 = (M32 * M44) - (M34 * M42);
            float temp3 = (M32 * M43) - (M33 * M42);
            float temp4 = (M31 * M44) - (M34 * M41);
            float temp5 = (M31 * M43) - (M33 * M41);
            float temp6 = (M31 * M42) - (M32 * M41);

            return ((((M11 * (((M22 * temp1) - (M23 * temp2)) + (M24 * temp3))) - (M12 * (((M21 * temp1) -
                (M23 * temp4)) + (M24 * temp5)))) + (M13 * (((M21 * temp2) - (M22 * temp4)) + (M24 * temp6)))) -
                (M14 * (((M21 * temp3) - (M22 * temp5)) + (M23 * temp6))));
        }

        /// <summary>
        /// Inverts the matrix.
        /// </summary>
        public void Invert()
        {
            Invert(ref this, out this);
        }

        /// <summary>
        /// Orthogonalizes the specified matrix.
        /// </summary>
        /// <remarks>
        /// <para>Orthogonalization is the process of making all rows orthogonal to each other. This
        /// means that any given row in the matrix will be orthogonal to any other given row in the
        /// matrix.</para>
        /// <para>Because this method uses the modified Gram-Schmidt process, the resulting matrix
        /// tends to be numerically unstable. The numeric stability decreases according to the rows
        /// so that the first row is the most stable and the last row is the least stable.</para>
        /// <para>This operation is performed on the rows of the matrix rather than the columns.
        /// If you wish for this operation to be performed on the columns, first transpose the
        /// input and than transpose the output.</para>
        /// </remarks>
        public void Orthogonalize()
        {
            Orthogonalize(ref this, out this);
        }

        /// <summary>
        /// Orthonormalizes the specified matrix.
        /// </summary>
        /// <remarks>
        /// <para>Orthonormalization is the process of making all rows and columns orthogonal to each
        /// other and making all rows and columns of unit length. This means that any given row will
        /// be orthogonal to any other given row and any given column will be orthogonal to any other
        /// given column. Any given row will not be orthogonal to any given column. Every row and every
        /// column will be of unit length.</para>
        /// <para>Because this method uses the modified Gram-Schmidt process, the resulting matrix
        /// tends to be numerically unstable. The numeric stability decreases according to the rows
        /// so that the first row is the most stable and the last row is the least stable.</para>
        /// <para>This operation is performed on the rows of the matrix rather than the columns.
        /// If you wish for this operation to be performed on the columns, first transpose the
        /// input and than transpose the output.</para>
        /// </remarks>
        public void Orthonormalize()
        {
            Orthonormalize(ref this, out this);
        }

        /// <summary>
        /// Decomposes a matrix into an orthonormalized matrix Q and a right triangular matrix R.
        /// </summary>
        /// <param name="Q">When the method completes, contains the orthonormalized matrix of the decomposition.</param>
        /// <param name="R">When the method completes, contains the right triangular matrix of the decomposition.</param>
        public void DecomposeQR(out Matrix4F Q, out Matrix4F R)
        {
            Matrix4F temp = this;
            temp.Transpose();
            Orthonormalize(ref temp, out Q);
            Q.Transpose();

            R = new Matrix4F();
            R.M11 = Vector4F.Dot(Q.Column1, Column1);
            R.M12 = Vector4F.Dot(Q.Column1, Column2);
            R.M13 = Vector4F.Dot(Q.Column1, Column3);
            R.M14 = Vector4F.Dot(Q.Column1, Column4);

            R.M22 = Vector4F.Dot(Q.Column2, Column2);
            R.M23 = Vector4F.Dot(Q.Column2, Column3);
            R.M24 = Vector4F.Dot(Q.Column2, Column4);

            R.M33 = Vector4F.Dot(Q.Column3, Column3);
            R.M34 = Vector4F.Dot(Q.Column3, Column4);

            R.M44 = Vector4F.Dot(Q.Column4, Column4);
        }

        /// <summary>
        /// Decomposes a matrix into a lower triangular matrix L and an orthonormalized matrix Q.
        /// </summary>
        /// <param name="L">When the method completes, contains the lower triangular matrix of the decomposition.</param>
        /// <param name="Q">When the method completes, contains the orthonormalized matrix of the decomposition.</param>
        public void DecomposeLQ(out Matrix4F L, out Matrix4F Q)
        {
            Orthonormalize(ref this, out Q);

            L = new Matrix4F();
            L.M11 = Vector4F.Dot(Q.Row1, Row1);
            
            L.M21 = Vector4F.Dot(Q.Row1, Row2);
            L.M22 = Vector4F.Dot(Q.Row2, Row2);
            
            L.M31 = Vector4F.Dot(Q.Row1, Row3);
            L.M32 = Vector4F.Dot(Q.Row2, Row3);
            L.M33 = Vector4F.Dot(Q.Row3, Row3);
            
            L.M41 = Vector4F.Dot(Q.Row1, Row4);
            L.M42 = Vector4F.Dot(Q.Row2, Row4);
            L.M43 = Vector4F.Dot(Q.Row3, Row4);
            L.M44 = Vector4F.Dot(Q.Row4, Row4);
        }

        /// <summary>
        /// Decomposes a matrix into a scale, rotation, and translation.
        /// </summary>
        /// <param name="scale">When the method completes, contains the scaling component of the decomposed matrix.</param>
        /// <param name="rotation">When the method completes, contains the rotation component of the decomposed matrix.</param>
        /// <param name="translation">When the method completes, contains the translation component of the decomposed matrix.</param>
        /// <remarks>
        /// This method is designed to decompose an SRT transformation matrix only.
        /// </remarks>
        public bool Decompose(out Vector3F scale, out QuaternionF rotation, out Vector3F translation)
        {
            //Source: Unknown
            //References: http://www.gamedev.net/community/forums/topic.asp?topic_id=441695

            // Get the translation.
            translation.X = M41;
            translation.Y = M42;
            translation.Z = M43;

            // Scaling is the length of the rows.
            scale.X = float.Sqrt((M11 * M11) + (M12 * M12) + (M13 * M13));
            scale.Y = float.Sqrt((M21 * M21) + (M22 * M22) + (M23 * M23));
            scale.Z = float.Sqrt((M31 * M31) + (M32 * M32) + (M33 * M33));

            // If any of the scaling factors are zero, then the rotation matrix can not exist.
            if (MathHelper.IsZero(scale.X) ||
                MathHelper.IsZero(scale.Y) ||
                MathHelper.IsZero(scale.Z))
            {
                rotation = QuaternionF.Identity;
                return false;
            }

            // The rotation is the left over matrix after dividing out the scaling.
            Matrix4F rotMatrix = new Matrix4F();
            rotMatrix.M11 = M11 / scale.X;
            rotMatrix.M12 = M12 / scale.X;
            rotMatrix.M13 = M13 / scale.X;

            rotMatrix.M21 = M21 / scale.Y;
            rotMatrix.M22 = M22 / scale.Y;
            rotMatrix.M23 = M23 / scale.Y;

            rotMatrix.M31 = M31 / scale.Z;
            rotMatrix.M32 = M32 / scale.Z;
            rotMatrix.M33 = M33 / scale.Z;

            rotMatrix.M44 = 1F;

            rotation = QuaternionF.FromRotationMatrix(ref rotMatrix);
            return true;
        }

        /// <summary>
        /// Decomposes a uniform scale matrix into a scale, rotation, and translation.
        /// A uniform scale matrix has the same scale in every axis.
        /// </summary>
        /// <param name="scale">When the method completes, contains the scaling component of the decomposed matrix.</param>
        /// <param name="rotation">When the method completes, contains the rotation component of the decomposed matrix.</param>
        /// <param name="translation">When the method completes, contains the translation component of the decomposed matrix.</param>
        /// <remarks>
        /// This method is designed to decompose only an SRT transformation matrix that has the same scale in every axis.
        /// </remarks>
        public bool DecomposeUniformScale(out float scale, out QuaternionF rotation, out Vector3F translation)
        {
            //Get the translation.
            translation.X = M41;
            translation.Y = M42;
            translation.Z = M43;

            //Scaling is the length of the rows. ( just take one row since this is a uniform matrix)
            scale = float.Sqrt((M11 * M11) + (M12 * M12) + (M13 * M13));
            var inv_scale = 1F / scale;

            //If any of the scaling factors are zero, then the rotation matrix can not exist.
            if (Math.Abs(scale) < MathHelper.Constants<float>.ZeroTolerance)
            {
                rotation = QuaternionF.Identity;
                return false;
            }

            //The rotation is the left over matrix after dividing out the scaling.
            Matrix4F rotMatrix = new Matrix4F();
            rotMatrix.M11 = M11 * inv_scale;
            rotMatrix.M12 = M12 * inv_scale;
            rotMatrix.M13 = M13 * inv_scale;

            rotMatrix.M21 = M21 * inv_scale;
            rotMatrix.M22 = M22 * inv_scale;
            rotMatrix.M23 = M23 * inv_scale;

            rotMatrix.M31 = M31 * inv_scale;
            rotMatrix.M32 = M32 * inv_scale;
            rotMatrix.M33 = M33 * inv_scale;

            rotMatrix.M44 = 1F;

            rotation = QuaternionF.FromRotationMatrix(ref rotMatrix);
            return true;
        }

        /// <summary>
        /// Exchanges two rows in the matrix.
        /// </summary>
        /// <param name="firstRow">The first row to exchange. This is an index of the row starting at zero.</param>
        /// <param name="secondRow">The second row to exchange. This is an index of the row starting at zero.</param>
        public void ExchangeRows(int firstRow, int secondRow)
        {
            if (firstRow < 0)
                throw new ArgumentOutOfRangeException("firstRow", "The parameter firstRow must be greater than or equal to zero.");
            if (firstRow > 3)
                throw new ArgumentOutOfRangeException("firstRow", "The parameter firstRow must be less than or equal to three.");
            if (secondRow < 0)
                throw new ArgumentOutOfRangeException("secondRow", "The parameter secondRow must be greater than or equal to zero.");
            if (secondRow > 3)
                throw new ArgumentOutOfRangeException("secondRow", "The parameter secondRow must be less than or equal to three.");

            if (firstRow == secondRow)
                return;

            float temp0 = this[secondRow, 0];
            float temp1 = this[secondRow, 1];
            float temp2 = this[secondRow, 2];
            float temp3 = this[secondRow, 3];

            this[secondRow, 0] = this[firstRow, 0];
            this[secondRow, 1] = this[firstRow, 1];
            this[secondRow, 2] = this[firstRow, 2];
            this[secondRow, 3] = this[firstRow, 3];

            this[firstRow, 0] = temp0;
            this[firstRow, 1] = temp1;
            this[firstRow, 2] = temp2;
            this[firstRow, 3] = temp3;
        }

        /// <summary>
        /// Exchanges two columns in the matrix.
        /// </summary>
        /// <param name="firstColumn">The first column to exchange. This is an index of the column starting at zero.</param>
        /// <param name="secondColumn">The second column to exchange. This is an index of the column starting at zero.</param>
        public void ExchangeColumns(int firstColumn, int secondColumn)
        {
            if (firstColumn < 0)
                throw new ArgumentOutOfRangeException("firstColumn", "The parameter firstColumn must be greater than or equal to zero.");
            if (firstColumn > 3)
                throw new ArgumentOutOfRangeException("firstColumn", "The parameter firstColumn must be less than or equal to three.");
            if (secondColumn < 0)
                throw new ArgumentOutOfRangeException("secondColumn", "The parameter secondColumn must be greater than or equal to zero.");
            if (secondColumn > 3)
                throw new ArgumentOutOfRangeException("secondColumn", "The parameter secondColumn must be less than or equal to three.");

            if (firstColumn == secondColumn)
                return;

            float temp0 = this[0, secondColumn];
            float temp1 = this[1, secondColumn];
            float temp2 = this[2, secondColumn];
            float temp3 = this[3, secondColumn];

            this[0, secondColumn] = this[0, firstColumn];
            this[1, secondColumn] = this[1, firstColumn];
            this[2, secondColumn] = this[2, firstColumn];
            this[3, secondColumn] = this[3, firstColumn];

            this[0, firstColumn] = temp0;
            this[1, firstColumn] = temp1;
            this[2, firstColumn] = temp2;
            this[3, firstColumn] = temp3;
        }

        /// <summary>
        /// Determines the product of two matrices.
        /// </summary>
        /// <param name="left">The first matrix to multiply.</param>
        /// <param name="right">The second matrix to multiply.</param>
        /// <param name="result">The product of the two matrices.</param>
        public static void Multiply(ref Matrix4F left, ref Matrix4F right, out Matrix4F result)
        {
            Matrix4F temp = new Matrix4F();
            temp.M11 = (left.M11 * right.M11) + (left.M12 * right.M21) + (left.M13 * right.M31) + (left.M14 * right.M41);
            temp.M12 = (left.M11 * right.M12) + (left.M12 * right.M22) + (left.M13 * right.M32) + (left.M14 * right.M42);
            temp.M13 = (left.M11 * right.M13) + (left.M12 * right.M23) + (left.M13 * right.M33) + (left.M14 * right.M43);
            temp.M14 = (left.M11 * right.M14) + (left.M12 * right.M24) + (left.M13 * right.M34) + (left.M14 * right.M44);
            temp.M21 = (left.M21 * right.M11) + (left.M22 * right.M21) + (left.M23 * right.M31) + (left.M24 * right.M41);
            temp.M22 = (left.M21 * right.M12) + (left.M22 * right.M22) + (left.M23 * right.M32) + (left.M24 * right.M42);
            temp.M23 = (left.M21 * right.M13) + (left.M22 * right.M23) + (left.M23 * right.M33) + (left.M24 * right.M43);
            temp.M24 = (left.M21 * right.M14) + (left.M22 * right.M24) + (left.M23 * right.M34) + (left.M24 * right.M44);
            temp.M31 = (left.M31 * right.M11) + (left.M32 * right.M21) + (left.M33 * right.M31) + (left.M34 * right.M41);
            temp.M32 = (left.M31 * right.M12) + (left.M32 * right.M22) + (left.M33 * right.M32) + (left.M34 * right.M42);
            temp.M33 = (left.M31 * right.M13) + (left.M32 * right.M23) + (left.M33 * right.M33) + (left.M34 * right.M43);
            temp.M34 = (left.M31 * right.M14) + (left.M32 * right.M24) + (left.M33 * right.M34) + (left.M34 * right.M44);
            temp.M41 = (left.M41 * right.M11) + (left.M42 * right.M21) + (left.M43 * right.M31) + (left.M44 * right.M41);
            temp.M42 = (left.M41 * right.M12) + (left.M42 * right.M22) + (left.M43 * right.M32) + (left.M44 * right.M42);
            temp.M43 = (left.M41 * right.M13) + (left.M42 * right.M23) + (left.M43 * right.M33) + (left.M44 * right.M43);
            temp.M44 = (left.M41 * right.M14) + (left.M42 * right.M24) + (left.M43 * right.M34) + (left.M44 * right.M44);
            result = temp;
        }

        /// <summary>
        /// Performs the exponential operation on a matrix.
        /// </summary>
        /// <param name="value">The matrix to perform the operation on.</param>
        /// <param name="exponent">The exponent to raise the matrix to.</param>
        /// <param name="result">When the method completes, contains the exponential matrix.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="exponent"/> is negative.</exception>
        public static void Exponent(ref Matrix4F value, int exponent, out Matrix4F result)
        {
            //Source: http://rosettacode.org
            //Reference: http://rosettacode.org/wiki/Matrix-exponentiation_operator

            if (exponent < 0)
                throw new ArgumentOutOfRangeException("exponent", "The exponent can not be negative.");

            if (exponent == 0)
            {
                result = Matrix4F.Identity;
                return;
            }

            if (exponent == 1)
            {
                result = value;
                return;
            }

            Matrix4F identity = Matrix4F.Identity;
            Matrix4F temp = value;

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
        /// Performs the exponential operation on a matrix.
        /// </summary>
        /// <param name="value">The matrix to perform the operation on.</param>
        /// <param name="exponent">The exponent to raise the matrix to.</param>
        /// <returns>The exponential matrix.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="exponent"/> is negative.</exception>
        public static Matrix4F Exponent(Matrix4F value, int exponent)
        {
            Matrix4F result;
            Exponent(ref value, exponent, out result);
            return result;
        }

        /// <summary>
        /// Calculates the inverse of the specified matrix.
        /// </summary>
        /// <param name="value">The matrix whose inverse is to be calculated.</param>
        /// <param name="result">When the method completes, contains the inverse of the specified matrix.</param>
        public static void Invert(ref Matrix4F value, out Matrix4F result)
        {
            float b0 = (value.M31 * value.M42) - (value.M32 * value.M41);
            float b1 = (value.M31 * value.M43) - (value.M33 * value.M41);
            float b2 = (value.M34 * value.M41) - (value.M31 * value.M44);
            float b3 = (value.M32 * value.M43) - (value.M33 * value.M42);
            float b4 = (value.M34 * value.M42) - (value.M32 * value.M44);
            float b5 = (value.M33 * value.M44) - (value.M34 * value.M43);

            float d11 = value.M22 * b5 + value.M23 * b4 + value.M24 * b3;
            float d12 = value.M21 * b5 + value.M23 * b2 + value.M24 * b1;
            float d13 = value.M21 * -b4 + value.M22 * b2 + value.M24 * b0;
            float d14 = value.M21 * b3 + value.M22 * -b1 + value.M23 * b0;

            float det = value.M11 * d11 - value.M12 * d12 + value.M13 * d13 - value.M14 * d14;
            if (Math.Abs(det) == 0.0F)
            {
                result = Matrix4F.Zero;
                return;
            }

            det = 1F / det;

            float a0 = (value.M11 * value.M22) - (value.M12 * value.M21);
            float a1 = (value.M11 * value.M23) - (value.M13 * value.M21);
            float a2 = (value.M14 * value.M21) - (value.M11 * value.M24);
            float a3 = (value.M12 * value.M23) - (value.M13 * value.M22);
            float a4 = (value.M14 * value.M22) - (value.M12 * value.M24);
            float a5 = (value.M13 * value.M24) - (value.M14 * value.M23);

            float d21 = value.M12 * b5 + value.M13 * b4 + value.M14 * b3;
            float d22 = value.M11 * b5 + value.M13 * b2 + value.M14 * b1;
            float d23 = value.M11 * -b4 + value.M12 * b2 + value.M14 * b0;
            float d24 = value.M11 * b3 + value.M12 * -b1 + value.M13 * b0;

            float d31 = value.M42 * a5 + value.M43 * a4 + value.M44 * a3;
            float d32 = value.M41 * a5 + value.M43 * a2 + value.M44 * a1;
            float d33 = value.M41 * -a4 + value.M42 * a2 + value.M44 * a0;
            float d34 = value.M41 * a3 + value.M42 * -a1 + value.M43 * a0;

            float d41 = value.M32 * a5 + value.M33 * a4 + value.M34 * a3;
            float d42 = value.M31 * a5 + value.M33 * a2 + value.M34 * a1;
            float d43 = value.M31 * -a4 + value.M32 * a2 + value.M34 * a0;
            float d44 = value.M31 * a3 + value.M32 * -a1 + value.M33 * a0;

            result.M11 = +d11 * det; result.M12 = -d21 * det; result.M13 = +d31 * det; result.M14 = -d41 * det;
            result.M21 = -d12 * det; result.M22 = +d22 * det; result.M23 = -d32 * det; result.M24 = +d42 * det;
            result.M31 = +d13 * det; result.M32 = -d23 * det; result.M33 = +d33 * det; result.M34 = -d43 * det;
            result.M41 = -d14 * det; result.M42 = +d24 * det; result.M43 = -d34 * det; result.M44 = +d44 * det;
        }

        /// <summary>
        /// Calculates the inverse of the specified matrix.
        /// </summary>
        /// <param name="value">The matrix whose inverse is to be calculated.</param>
        /// <returns>The inverse of the specified matrix.</returns>
        public static Matrix4F Invert(Matrix4F value)
        {
            Invert(ref value, out value);
            return value;
        }

        /// <summary>
        /// Orthogonalizes the specified matrix.
        /// </summary>
        /// <param name="value">The matrix to orthogonalize.</param>
        /// <param name="result">When the method completes, contains the orthogonalized matrix.</param>
        /// <remarks>
        /// <para>Orthogonalization is the process of making all rows orthogonal to each other. This
        /// means that any given row in the matrix will be orthogonal to any other given row in the
        /// matrix.</para>
        /// <para>Because this method uses the modified Gram-Schmidt process, the resulting matrix
        /// tends to be numerically unstable. The numeric stability decreases according to the rows
        /// so that the first row is the most stable and the last row is the least stable.</para>
        /// <para>This operation is performed on the rows of the matrix rather than the columns.
        /// If you wish for this operation to be performed on the columns, first transpose the
        /// input and than transpose the output.</para>
        /// </remarks>
        public static void Orthogonalize(ref Matrix4F value, out Matrix4F result)
        {
            //Uses the modified Gram-Schmidt process.
            //q1 = m1
            //q2 = m2 - ((q1 ⋅ m2) / (q1 ⋅ q1)) * q1
            //q3 = m3 - ((q1 ⋅ m3) / (q1 ⋅ q1)) * q1 - ((q2 ⋅ m3) / (q2 ⋅ q2)) * q2
            //q4 = m4 - ((q1 ⋅ m4) / (q1 ⋅ q1)) * q1 - ((q2 ⋅ m4) / (q2 ⋅ q2)) * q2 - ((q3 ⋅ m4) / (q3 ⋅ q3)) * q3

            //By separating the above algorithm into multiple lines, we actually increase accuracy.
            result = value;

            result.Row2 = result.Row2 - (Vector4F.Dot(result.Row1, result.Row2) / Vector4F.Dot(result.Row1, result.Row1)) * result.Row1;

            result.Row3 = result.Row3 - (Vector4F.Dot(result.Row1, result.Row3) / Vector4F.Dot(result.Row1, result.Row1)) * result.Row1;
            result.Row3 = result.Row3 - (Vector4F.Dot(result.Row2, result.Row3) / Vector4F.Dot(result.Row2, result.Row2)) * result.Row2;

            result.Row4 = result.Row4 - (Vector4F.Dot(result.Row1, result.Row4) / Vector4F.Dot(result.Row1, result.Row1)) * result.Row1;
            result.Row4 = result.Row4 - (Vector4F.Dot(result.Row2, result.Row4) / Vector4F.Dot(result.Row2, result.Row2)) * result.Row2;
            result.Row4 = result.Row4 - (Vector4F.Dot(result.Row3, result.Row4) / Vector4F.Dot(result.Row3, result.Row3)) * result.Row3;
        }

        /// <summary>
        /// Orthogonalizes the specified matrix.
        /// </summary>
        /// <param name="value">The matrix to orthogonalize.</param>
        /// <returns>The orthogonalized matrix.</returns>
        /// <remarks>
        /// <para>Orthogonalization is the process of making all rows orthogonal to each other. This
        /// means that any given row in the matrix will be orthogonal to any other given row in the
        /// matrix.</para>
        /// <para>Because this method uses the modified Gram-Schmidt process, the resulting matrix
        /// tends to be numerically unstable. The numeric stability decreases according to the rows
        /// so that the first row is the most stable and the last row is the least stable.</para>
        /// <para>This operation is performed on the rows of the matrix rather than the columns.
        /// If you wish for this operation to be performed on the columns, first transpose the
        /// input and than transpose the output.</para>
        /// </remarks>
        public static Matrix4F Orthogonalize(Matrix4F value)
        {
            Matrix4F result;
            Orthogonalize(ref value, out result);
            return result;
        }

        /// <summary>
        /// Orthonormalizes the specified matrix.
        /// </summary>
        /// <param name="value">The matrix to orthonormalize.</param>
        /// <param name="result">When the method completes, contains the orthonormalized matrix.</param>
        /// <remarks>
        /// <para>Orthonormalization is the process of making all rows and columns orthogonal to each
        /// other and making all rows and columns of unit length. This means that any given row will
        /// be orthogonal to any other given row and any given column will be orthogonal to any other
        /// given column. Any given row will not be orthogonal to any given column. Every row and every
        /// column will be of unit length.</para>
        /// <para>Because this method uses the modified Gram-Schmidt process, the resulting matrix
        /// tends to be numerically unstable. The numeric stability decreases according to the rows
        /// so that the first row is the most stable and the last row is the least stable.</para>
        /// <para>This operation is performed on the rows of the matrix rather than the columns.
        /// If you wish for this operation to be performed on the columns, first transpose the
        /// input and than transpose the output.</para>
        /// </remarks>
        public static void Orthonormalize(ref Matrix4F value, out Matrix4F result)
        {
            //Uses the modified Gram-Schmidt process.
            //Because we are making unit vectors, we can optimize the math for orthonormalization
            //and simplify the projection operation to remove the division.
            //q1 = m1 / |m1|
            //q2 = (m2 - (q1 ⋅ m2) * q1) / |m2 - (q1 ⋅ m2) * q1|
            //q3 = (m3 - (q1 ⋅ m3) * q1 - (q2 ⋅ m3) * q2) / |m3 - (q1 ⋅ m3) * q1 - (q2 ⋅ m3) * q2|
            //q4 = (m4 - (q1 ⋅ m4) * q1 - (q2 ⋅ m4) * q2 - (q3 ⋅ m4) * q3) / |m4 - (q1 ⋅ m4) * q1 - (q2 ⋅ m4) * q2 - (q3 ⋅ m4) * q3|

            //By separating the above algorithm into multiple lines, we actually increase accuracy.
            result = value;

            result.Row1 = Vector4F.Normalize(result.Row1);

            result.Row2 = result.Row2 - Vector4F.Dot(result.Row1, result.Row2) * result.Row1;
            result.Row2 = Vector4F.Normalize(result.Row2);

            result.Row3 = result.Row3 - Vector4F.Dot(result.Row1, result.Row3) * result.Row1;
            result.Row3 = result.Row3 - Vector4F.Dot(result.Row2, result.Row3) * result.Row2;
            result.Row3 = Vector4F.Normalize(result.Row3);

            result.Row4 = result.Row4 - Vector4F.Dot(result.Row1, result.Row4) * result.Row1;
            result.Row4 = result.Row4 - Vector4F.Dot(result.Row2, result.Row4) * result.Row2;
            result.Row4 = result.Row4 - Vector4F.Dot(result.Row3, result.Row4) * result.Row3;
            result.Row4 = Vector4F.Normalize(result.Row4);
        }

        /// <summary>
        /// Orthonormalizes the specified matrix.
        /// </summary>
        /// <param name="value">The matrix to orthonormalize.</param>
        /// <returns>The orthonormalized matrix.</returns>
        /// <remarks>
        /// <para>Orthonormalization is the process of making all rows and columns orthogonal to each
        /// other and making all rows and columns of unit length. This means that any given row will
        /// be orthogonal to any other given row and any given column will be orthogonal to any other
        /// given column. Any given row will not be orthogonal to any given column. Every row and every
        /// column will be of unit length.</para>
        /// <para>Because this method uses the modified Gram-Schmidt process, the resulting matrix
        /// tends to be numerically unstable. The numeric stability decreases according to the rows
        /// so that the first row is the most stable and the last row is the least stable.</para>
        /// <para>This operation is performed on the rows of the matrix rather than the columns.
        /// If you wish for this operation to be performed on the columns, first transpose the
        /// input and than transpose the output.</para>
        /// </remarks>
        public static Matrix4F Orthonormalize(Matrix4F value)
        {
            Matrix4F result;
            Orthonormalize(ref value, out result);
            return result;
        }

        /// <summary>
        /// Brings the matrix into upper triangular form using elementary row operations.
        /// </summary>
        /// <param name="value">The matrix to put into upper triangular form.</param>
        /// <param name="result">When the method completes, contains the upper triangular matrix.</param>
        /// <remarks>
        /// If the matrix is not invertible (i.e. its determinant is zero) than the result of this
        /// method may produce Single.Nan and Single.Inf values. When the matrix represents a system
        /// of linear equations, than this often means that either no solution exists or an infinite
        /// number of solutions exist.
        /// </remarks>
        public static void UpperTriangularForm(ref Matrix4F value, out Matrix4F result)
        {
            //Adapted from the row echelon code.
            result = value;
            int lead = 0;
            int rowcount = 4;
            int columncount = 4;

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

                float multiplier = 1F / result[r, lead];

                for (; i < rowcount; ++i)
                {
                    if (i != r)
                    {
                        result[i, 0] -= result[r, 0] * multiplier * result[i, lead];
                        result[i, 1] -= result[r, 1] * multiplier * result[i, lead];
                        result[i, 2] -= result[r, 2] * multiplier * result[i, lead];
                        result[i, 3] -= result[r, 3] * multiplier * result[i, lead];
                    }
                }

                lead++;
            }
        }

        /// <summary>
        /// Brings the matrix into upper triangular form using elementary row operations.
        /// </summary>
        /// <param name="value">The matrix to put into upper triangular form.</param>
        /// <returns>The upper triangular matrix.</returns>
        /// <remarks>
        /// If the matrix is not invertible (i.e. its determinant is zero) than the result of this
        /// method may produce Single.Nan and Single.Inf values. When the matrix represents a system
        /// of linear equations, than this often means that either no solution exists or an infinite
        /// number of solutions exist.
        /// </remarks>
        public static Matrix4F UpperTriangularForm(Matrix4F value)
        {
            Matrix4F result;
            UpperTriangularForm(ref value, out result);
            return result;
        }

        /// <summary>
        /// Brings the matrix into lower triangular form using elementary row operations.
        /// </summary>
        /// <param name="value">The matrix to put into lower triangular form.</param>
        /// <param name="result">When the method completes, contains the lower triangular matrix.</param>
        /// <remarks>
        /// If the matrix is not invertible (i.e. its determinant is zero) than the result of this
        /// method may produce Single.Nan and Single.Inf values. When the matrix represents a system
        /// of linear equations, than this often means that either no solution exists or an infinite
        /// number of solutions exist.
        /// </remarks>
        public static void LowerTriangularForm(ref Matrix4F value, out Matrix4F result)
        {
            //Adapted from the row echelon code.
            Matrix4F temp = value;
            Matrix4F.Transpose(ref temp, out result);

            int lead = 0;
            int rowcount = 4;
            int columncount = 4;

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

                float multiplier = 1F / result[r, lead];

                for (; i < rowcount; ++i)
                {
                    if (i != r)
                    {
                        result[i, 0] -= result[r, 0] * multiplier * result[i, lead];
                        result[i, 1] -= result[r, 1] * multiplier * result[i, lead];
                        result[i, 2] -= result[r, 2] * multiplier * result[i, lead];
                        result[i, 3] -= result[r, 3] * multiplier * result[i, lead];
                    }
                }

                lead++;
            }

            Matrix4F.Transpose(ref result, out result);
        }

        /// <summary>
        /// Brings the matrix into lower triangular form using elementary row operations.
        /// </summary>
        /// <param name="value">The matrix to put into lower triangular form.</param>
        /// <returns>The lower triangular matrix.</returns>
        /// <remarks>
        /// If the matrix is not invertible (i.e. its determinant is zero) than the result of this
        /// method may produce Single.Nan and Single.Inf values. When the matrix represents a system
        /// of linear equations, than this often means that either no solution exists or an infinite
        /// number of solutions exist.
        /// </remarks>
        public static Matrix4F LowerTriangularForm(Matrix4F value)
        {
            Matrix4F result;
            LowerTriangularForm(ref value, out result);
            return result;
        }

        /// <summary>
        /// Brings the matrix into row echelon form using elementary row operations;
        /// </summary>
        /// <param name="value">The matrix to put into row echelon form.</param>
        /// <param name="result">When the method completes, contains the row echelon form of the matrix.</param>
        public static void RowEchelonForm(ref Matrix4F value, out Matrix4F result)
        {
            //Source: Wikipedia pseudo code
            //Reference: http://en.wikipedia.org/wiki/Row_echelon_form#Pseudocode

            result = value;
            int lead = 0;
            int rowcount = 4;
            int columncount = 4;

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

                float multiplier = 1F / result[r, lead];
                result[r, 0] *= multiplier;
                result[r, 1] *= multiplier;
                result[r, 2] *= multiplier;
                result[r, 3] *= multiplier;

                for (; i < rowcount; ++i)
                {
                    if (i != r)
                    {
                        result[i, 0] -= result[r, 0] * result[i, lead];
                        result[i, 1] -= result[r, 1] * result[i, lead];
                        result[i, 2] -= result[r, 2] * result[i, lead];
                        result[i, 3] -= result[r, 3] * result[i, lead];
                    }
                }

                lead++;
            }
        }

        /// <summary>
        /// Brings the matrix into row echelon form using elementary row operations;
        /// </summary>
        /// <param name="value">The matrix to put into row echelon form.</param>
        /// <returns>When the method completes, contains the row echelon form of the matrix.</returns>
        public static Matrix4F RowEchelonForm(Matrix4F value)
        {
            Matrix4F result;
            RowEchelonForm(ref value, out result);
            return result;
        }

        /// <summary>
        /// Brings the matrix into reduced row echelon form using elementary row operations.
        /// </summary>
        /// <param name="value">The matrix to put into reduced row echelon form.</param>
        /// <param name="augment">The fifth column of the matrix.</param>
        /// <param name="result">When the method completes, contains the resultant matrix after the operation.</param>
        /// <param name="augmentResult">When the method completes, contains the resultant fifth column of the matrix.</param>
        /// <remarks>
        /// <para>The fifth column is often called the augmented part of the matrix. This is because the fifth
        /// column is really just an extension of the matrix so that there is a place to put all of the
        /// non-zero components after the operation is complete.</para>
        /// <para>Often times the resultant matrix will the identity matrix or a matrix similar to the identity
        /// matrix. Sometimes, however, that is not possible and numbers other than zero and one may appear.</para>
        /// <para>This method can be used to solve systems of linear equations. Upon completion of this method,
        /// the <paramref name="augmentResult"/> will contain the solution for the system. It is up to the user
        /// to analyze both the input and the result to determine if a solution really exists.</para>
        /// </remarks>
        public static void ReducedRowEchelonForm(ref Matrix4F value, ref Vector4F augment, out Matrix4F result, out Vector4F augmentResult)
        {
            //Source: http://rosettacode.org
            //Reference: http://rosettacode.org/wiki/Reduced_row_echelon_form

            float[,] matrix = new float[4, 5];

            matrix[0, 0] = value[0, 0];
            matrix[0, 1] = value[0, 1];
            matrix[0, 2] = value[0, 2];
            matrix[0, 3] = value[0, 3];
            matrix[0, 4] = augment[0];

            matrix[1, 0] = value[1, 0];
            matrix[1, 1] = value[1, 1];
            matrix[1, 2] = value[1, 2];
            matrix[1, 3] = value[1, 3];
            matrix[1, 4] = augment[1];

            matrix[2, 0] = value[2, 0];
            matrix[2, 1] = value[2, 1];
            matrix[2, 2] = value[2, 2];
            matrix[2, 3] = value[2, 3];
            matrix[2, 4] = augment[2];

            matrix[3, 0] = value[3, 0];
            matrix[3, 1] = value[3, 1];
            matrix[3, 2] = value[3, 2];
            matrix[3, 3] = value[3, 3];
            matrix[3, 4] = augment[3];

            int lead = 0;
            int rowcount = 4;
            int columncount = 5;

            for (int r = 0; r < rowcount; r++)
            {
                if (columncount <= lead)
                    break;

                int i = r;

                while (matrix[i, lead] == 0)
                {
                    i++;

                    if (i == rowcount)
                    {
                        i = r;
                        lead++;

                        if (columncount == lead)
                            break;
                    }
                }

                for (int j = 0; j < columncount; j++)
                {
                    float temp = matrix[r, j];
                    matrix[r, j] = matrix[i, j];
                    matrix[i, j] = temp;
                }

                float div = matrix[r, lead];

                for (int j = 0; j < columncount; j++)
                {
                    matrix[r, j] /= div;
                }

                for (int j = 0; j < rowcount; j++)
                {
                    if (j != r)
                    {
                        float sub = matrix[j, lead];
                        for (int k = 0; k < columncount; k++) matrix[j, k] -= (sub * matrix[r, k]);
                    }
                }

                lead++;
            }

            result.M11 = matrix[0, 0];
            result.M12 = matrix[0, 1];
            result.M13 = matrix[0, 2];
            result.M14 = matrix[0, 3];

            result.M21 = matrix[1, 0];
            result.M22 = matrix[1, 1];
            result.M23 = matrix[1, 2];
            result.M24 = matrix[1, 3];

            result.M31 = matrix[2, 0];
            result.M32 = matrix[2, 1];
            result.M33 = matrix[2, 2];
            result.M34 = matrix[2, 3];

            result.M41 = matrix[3, 0];
            result.M42 = matrix[3, 1];
            result.M43 = matrix[3, 2];
            result.M44 = matrix[3, 3];

            augmentResult.X = matrix[0, 4];
            augmentResult.Y = matrix[1, 4];
            augmentResult.Z = matrix[2, 4];
            augmentResult.W = matrix[3, 4];
        }

        /// <summary>
        /// Creates a left-handed spherical billboard that rotates around a specified object position.
        /// </summary>
        /// <param name="objectPosition">The position of the object around which the billboard will rotate.</param>
        /// <param name="cameraPosition">The position of the camera.</param>
        /// <param name="cameraUpVector">The up vector of the camera.</param>
        /// <param name="cameraForwardVector">The forward vector of the camera.</param>
        /// <param name="result">When the method completes, contains the created billboard matrix.</param>
        public static void BillboardLH(ref Vector3F objectPosition, ref Vector3F cameraPosition, ref Vector3F cameraUpVector, ref Vector3F cameraForwardVector, out Matrix4F result)
        {
            Vector3F difference = cameraPosition - objectPosition;

            float lengthSq = difference.LengthSquared();
            if (MathHelper.IsZero(lengthSq))
                difference = -cameraForwardVector;
            else
                difference *= 1F / float.Sqrt(lengthSq);

            Vector3F crossed = Vector3F.Cross(ref cameraUpVector, ref difference);
            crossed.Normalize();
            Vector3F final = Vector3F.Cross(ref difference, ref crossed);

            result.M11 = crossed.X;
            result.M12 = crossed.Y;
            result.M13 = crossed.Z;
            result.M14 = 0.0F;
            result.M21 = final.X;
            result.M22 = final.Y;
            result.M23 = final.Z;
            result.M24 = 0.0F;
            result.M31 = difference.X;
            result.M32 = difference.Y;
            result.M33 = difference.Z;
            result.M34 = 0.0F;
            result.M41 = objectPosition.X;
            result.M42 = objectPosition.Y;
            result.M43 = objectPosition.Z;
            result.M44 = 1F;
        }

        /// <summary>
        /// Creates a left-handed spherical billboard that rotates around a specified object position.
        /// </summary>
        /// <param name="objectPosition">The position of the object around which the billboard will rotate.</param>
        /// <param name="cameraPosition">The position of the camera.</param>
        /// <param name="cameraUpVector">The up vector of the camera.</param>
        /// <param name="cameraForwardVector">The forward vector of the camera.</param>
        /// <returns>The created billboard matrix.</returns>
        public static Matrix4F BillboardLH(Vector3F objectPosition, Vector3F cameraPosition, Vector3F cameraUpVector, Vector3F cameraForwardVector)
        {
            Matrix4F result;
            BillboardLH(ref objectPosition, ref cameraPosition, ref cameraUpVector, ref cameraForwardVector, out result);
            return result;
        }

        /// <summary>
        /// Creates a right-handed spherical billboard that rotates around a specified object position.
        /// </summary>
        /// <param name="objectPosition">The position of the object around which the billboard will rotate.</param>
        /// <param name="cameraPosition">The position of the camera.</param>
        /// <param name="cameraUpVector">The up vector of the camera.</param>
        /// <param name="cameraForwardVector">The forward vector of the camera.</param>
        /// <param name="result">When the method completes, contains the created billboard matrix.</param>
        public static void BillboardRH(ref Vector3F objectPosition, ref Vector3F cameraPosition, ref Vector3F cameraUpVector, ref Vector3F cameraForwardVector, out Matrix4F result)
        {
            Vector3F difference = objectPosition - cameraPosition;

            float lengthSq = difference.LengthSquared();
            if (MathHelper.IsZero(lengthSq))
                difference = -cameraForwardVector;
            else
                difference *= (1F / float.Sqrt(lengthSq));

            Vector3F crossed = Vector3F.Cross(ref cameraUpVector, ref difference);
            crossed.Normalize();
            Vector3F final = Vector3F.Cross(ref difference, ref crossed);

            result.M11 = crossed.X;
            result.M12 = crossed.Y;
            result.M13 = crossed.Z;
            result.M14 = 0.0F;
            result.M21 = final.X;
            result.M22 = final.Y;
            result.M23 = final.Z;
            result.M24 = 0.0F;
            result.M31 = difference.X;
            result.M32 = difference.Y;
            result.M33 = difference.Z;
            result.M34 = 0.0F;
            result.M41 = objectPosition.X;
            result.M42 = objectPosition.Y;
            result.M43 = objectPosition.Z;
            result.M44 = 1F;
        }

        /// <summary>
        /// Creates a right-handed spherical billboard that rotates around a specified object position.
        /// </summary>
        /// <param name="objectPosition">The position of the object around which the billboard will rotate.</param>
        /// <param name="cameraPosition">The position of the camera.</param>
        /// <param name="cameraUpVector">The up vector of the camera.</param>
        /// <param name="cameraForwardVector">The forward vector of the camera.</param>
        /// <returns>The created billboard matrix.</returns>
        public static Matrix4F BillboardRH(Vector3F objectPosition, Vector3F cameraPosition, Vector3F cameraUpVector, Vector3F cameraForwardVector) {
            Matrix4F result;
            BillboardRH(ref objectPosition, ref cameraPosition, ref cameraUpVector, ref cameraForwardVector, out result);
            return result;
        }

        /// <summary>
        /// Creates a left-handed, look-at matrix.
        /// </summary>
        /// <param name="eye">The position of the viewer's eye.</param>
        /// <param name="target">The camera look-at target.</param>
        /// <param name="up">The camera's up vector.</param>
        /// <param name="result">When the method completes, contains the created look-at matrix.</param>
        public static void LookAtLH(ref Vector3F eye, ref Vector3F target, ref Vector3F up, out Matrix4F result)
        {
            Vector3F zaxis = target - eye; 
            zaxis.Normalize();

            Vector3F xaxis = Vector3F.Cross(ref up, ref zaxis); 
            xaxis.Normalize();

            Vector3F yaxis = Vector3F.Cross(ref zaxis, ref xaxis);

            result = Matrix4F.Identity;
            result.M11 = xaxis.X; result.M21 = xaxis.Y; result.M31 = xaxis.Z;
            result.M12 = yaxis.X; result.M22 = yaxis.Y; result.M32 = yaxis.Z;
            result.M13 = zaxis.X; result.M23 = zaxis.Y; result.M33 = zaxis.Z;

            result.M41 = Vector3F.Dot(ref xaxis, ref eye);
            result.M42 = Vector3F.Dot(ref yaxis, ref eye);
            result.M43 = Vector3F.Dot(ref zaxis, ref eye);

            result.M41 = -result.M41;
            result.M42 = -result.M42;
            result.M43 = -result.M43;
        }

        /// <summary>
        /// Creates a left-handed, look-at matrix.
        /// </summary>
        /// <param name="eye">The position of the viewer's eye.</param>
        /// <param name="target">The camera look-at target.</param>
        /// <param name="up">The camera's up vector.</param>
        /// <returns>The created look-at matrix.</returns>
        public static Matrix4F LookAtLH(Vector3F eye, Vector3F target, Vector3F up)
        {
            Matrix4F result;
            LookAtLH(ref eye, ref target, ref up, out result);
            return result;
        }

        /// <summary>
        /// Creates a right-handed, look-at matrix.
        /// </summary>
        /// <param name="eye">The position of the viewer's eye.</param>
        /// <param name="target">The camera look-at target.</param>
        /// <param name="up">The camera's up vector.</param>
        /// <param name="result">When the method completes, contains the created look-at matrix.</param>
        public static void LookAtRH(ref Vector3F eye, ref Vector3F target, ref Vector3F up, out Matrix4F result)
        {
            Vector3F zaxis = eye - target;
            zaxis.Normalize();

            Vector3F xaxis = Vector3F.Cross(ref up, ref zaxis);
            xaxis.Normalize();

            Vector3F yaxis = Vector3F.Cross(ref zaxis, ref xaxis);

            result = Identity;
            result.M11 = xaxis.X; result.M21 = xaxis.Y; result.M31 = xaxis.Z;
            result.M12 = yaxis.X; result.M22 = yaxis.Y; result.M32 = yaxis.Z;
            result.M13 = zaxis.X; result.M23 = zaxis.Y; result.M33 = zaxis.Z;

            result.M41 = Vector3F.Dot(ref xaxis, ref eye);
            result.M42 = Vector3F.Dot(ref yaxis, ref eye);
            result.M43 = Vector3F.Dot(ref zaxis, ref eye);

            result.M41 = -result.M41;
            result.M42 = -result.M42;
            result.M43 = -result.M43;
        }

        /// <summary>
        /// Creates a right-handed, look-at matrix.
        /// </summary>
        /// <param name="eye">The position of the viewer's eye.</param>
        /// <param name="target">The camera look-at target.</param>
        /// <param name="up">The camera's up vector.</param>
        /// <returns>The created look-at matrix.</returns>
        public static Matrix4F LookAtRH(Vector3F eye, Vector3F target, Vector3F up)
        {
            Matrix4F result;
            LookAtRH(ref eye, ref target, ref up, out result);
            return result;
        }

        /// <summary>
        /// Creates a left-handed, orthographic projection matrix.
        /// </summary>
        /// <param name="width">Width of the viewing volume.</param>
        /// <param name="height">Height of the viewing volume.</param>
        /// <param name="znear">Minimum z-value of the viewing volume.</param>
        /// <param name="zfar">Maximum z-value of the viewing volume.</param>
        /// <param name="result">When the method completes, contains the created projection matrix.</param>
        public static void OrthoLH(float width, float height, float znear, float zfar, out Matrix4F result)
        {
            float halfWidth = width * 0.5F;
            float halfHeight = height * 0.5F;

            OrthoOffCenterLH(-halfWidth, halfWidth, -halfHeight, halfHeight, znear, zfar, out result);
        }

        /// <summary>
        /// Creates a left-handed, orthographic projection matrix.
        /// </summary>
        /// <param name="width">Width of the viewing volume.</param>
        /// <param name="height">Height of the viewing volume.</param>
        /// <param name="znear">Minimum z-value of the viewing volume.</param>
        /// <param name="zfar">Maximum z-value of the viewing volume.</param>
        /// <returns>The created projection matrix.</returns>
        public static Matrix4F OrthoLH(float width, float height, float znear, float zfar)
        {
            Matrix4F result;
            OrthoLH(width, height, znear, zfar, out result);
            return result;
        }

        /// <summary>
        /// Creates a right-handed, orthographic projection matrix.
        /// </summary>
        /// <param name="width">Width of the viewing volume.</param>
        /// <param name="height">Height of the viewing volume.</param>
        /// <param name="znear">Minimum z-value of the viewing volume.</param>
        /// <param name="zfar">Maximum z-value of the viewing volume.</param>
        /// <param name="result">When the method completes, contains the created projection matrix.</param>
        public static void OrthoRH(float width, float height, float znear, float zfar, out Matrix4F result)
        {
            float halfWidth = width * 0.5F;
            float halfHeight = height * 0.5F;

            OrthoOffCenterRH(-halfWidth, halfWidth, -halfHeight, halfHeight, znear, zfar, out result);
        }

        /// <summary>
        /// Creates a right-handed, orthographic projection matrix.
        /// </summary>
        /// <param name="width">Width of the viewing volume.</param>
        /// <param name="height">Height of the viewing volume.</param>
        /// <param name="znear">Minimum z-value of the viewing volume.</param>
        /// <param name="zfar">Maximum z-value of the viewing volume.</param>
        /// <returns>The created projection matrix.</returns>
        public static Matrix4F OrthoRH(float width, float height, float znear, float zfar)
        {
            Matrix4F result;
            OrthoRH(width, height, znear, zfar, out result);
            return result;
        }

        /// <summary>
        /// Creates a left-handed, customized orthographic projection matrix.
        /// </summary>
        /// <param name="left">Minimum x-value of the viewing volume.</param>
        /// <param name="right">Maximum x-value of the viewing volume.</param>
        /// <param name="bottom">Minimum y-value of the viewing volume.</param>
        /// <param name="top">Maximum y-value of the viewing volume.</param>
        /// <param name="znear">Minimum z-value of the viewing volume.</param>
        /// <param name="zfar">Maximum z-value of the viewing volume.</param>
        /// <param name="result">When the method completes, contains the created projection matrix.</param>
        public static void OrthoOffCenterLH(float left, float right, float bottom, float top, float znear, float zfar, out Matrix4F result)
        {
            float zRange = 1F / (zfar - znear);

            result = Matrix4F.Identity;
            result.M11 = 2.0F / (right - left);
            result.M22 = 2.0F / (top - bottom);
            result.M33 = zRange;
            result.M41 = (left + right) / (left - right);
            result.M42 = (top + bottom) / (bottom - top);
            result.M43 = -znear * zRange;
        }

        /// <summary>
        /// Creates a left-handed, customized orthographic projection matrix.
        /// </summary>
        /// <param name="left">Minimum x-value of the viewing volume.</param>
        /// <param name="right">Maximum x-value of the viewing volume.</param>
        /// <param name="bottom">Minimum y-value of the viewing volume.</param>
        /// <param name="top">Maximum y-value of the viewing volume.</param>
        /// <param name="znear">Minimum z-value of the viewing volume.</param>
        /// <param name="zfar">Maximum z-value of the viewing volume.</param>
        /// <returns>The created projection matrix.</returns>
        public static Matrix4F OrthoOffCenterLH(float left, float right, float bottom, float top, float znear, float zfar)
        {
            Matrix4F result;
            OrthoOffCenterLH(left, right, bottom, top, znear, zfar, out result);
            return result;
        }

        /// <summary>
        /// Creates a right-handed, customized orthographic projection matrix.
        /// </summary>
        /// <param name="left">Minimum x-value of the viewing volume.</param>
        /// <param name="right">Maximum x-value of the viewing volume.</param>
        /// <param name="bottom">Minimum y-value of the viewing volume.</param>
        /// <param name="top">Maximum y-value of the viewing volume.</param>
        /// <param name="znear">Minimum z-value of the viewing volume.</param>
        /// <param name="zfar">Maximum z-value of the viewing volume.</param>
        /// <param name="result">When the method completes, contains the created projection matrix.</param>
        public static void OrthoOffCenterRH(float left, float right, float bottom, float top, float znear, float zfar, out Matrix4F result)
        {
            OrthoOffCenterLH(left, right, bottom, top, znear, zfar, out result);
            result.M33 *= -1F;
        }

        /// <summary>
        /// Creates a right-handed, customized orthographic projection matrix.
        /// </summary>
        /// <param name="left">Minimum x-value of the viewing volume.</param>
        /// <param name="right">Maximum x-value of the viewing volume.</param>
        /// <param name="bottom">Minimum y-value of the viewing volume.</param>
        /// <param name="top">Maximum y-value of the viewing volume.</param>
        /// <param name="znear">Minimum z-value of the viewing volume.</param>
        /// <param name="zfar">Maximum z-value of the viewing volume.</param>
        /// <returns>The created projection matrix.</returns>
        public static Matrix4F OrthoOffCenterRH(float left, float right, float bottom, float top, float znear, float zfar)
        {
            OrthoOffCenterRH(left, right, bottom, top, znear, zfar, out Matrix4F result);
            return result;
        }

        /// <summary>
        /// Creates a left-handed, perspective projection matrix.
        /// </summary>
        /// <param name="width">Width of the viewing volume.</param>
        /// <param name="height">Height of the viewing volume.</param>
        /// <param name="znear">Minimum z-value of the viewing volume.</param>
        /// <param name="zfar">Maximum z-value of the viewing volume.</param>
        /// <param name="result">When the method completes, contains the created projection matrix.</param>
        public static void PerspectiveLH(float width, float height, float znear, float zfar, out Matrix4F result)
        {
            float halfWidth = width * 0.5F;
            float halfHeight = height * 0.5F;

            PerspectiveOffCenterLH(-halfWidth, halfWidth, -halfHeight, halfHeight, znear, zfar, out result);
        }

        /// <summary>
        /// Creates a left-handed, perspective projection matrix.
        /// </summary>
        /// <param name="width">Width of the viewing volume.</param>
        /// <param name="height">Height of the viewing volume.</param>
        /// <param name="znear">Minimum z-value of the viewing volume.</param>
        /// <param name="zfar">Maximum z-value of the viewing volume.</param>
        /// <returns>The created projection matrix.</returns>
        public static Matrix4F PerspectiveLH(float width, float height, float znear, float zfar)
        {
            PerspectiveLH(width, height, znear, zfar, out Matrix4F result);
            return result;
        }

        /// <summary>
        /// Creates a right-handed, perspective projection matrix.
        /// </summary>
        /// <param name="width">Width of the viewing volume.</param>
        /// <param name="height">Height of the viewing volume.</param>
        /// <param name="znear">Minimum z-value of the viewing volume.</param>
        /// <param name="zfar">Maximum z-value of the viewing volume.</param>
        /// <param name="result">When the method completes, contains the created projection matrix.</param>
        public static void PerspectiveRH(float width, float height, float znear, float zfar, out Matrix4F result)
        {
            float halfWidth = width * 0.5F;
            float halfHeight = height * 0.5F;

            PerspectiveOffCenterRH(-halfWidth, halfWidth, -halfHeight, halfHeight, znear, zfar, out result);
        }

        /// <summary>
        /// Creates a right-handed, perspective projection matrix.
        /// </summary>
        /// <param name="width">Width of the viewing volume.</param>
        /// <param name="height">Height of the viewing volume.</param>
        /// <param name="znear">Minimum z-value of the viewing volume.</param>
        /// <param name="zfar">Maximum z-value of the viewing volume.</param>
        /// <returns>The created projection matrix.</returns>
        public static Matrix4F PerspectiveRH(float width, float height, float znear, float zfar)
        {
            PerspectiveRH(width, height, znear, zfar, out Matrix4F result);
            return result;
        }

        /// <summary>
        /// Creates a left-handed, perspective projection matrix based on a field of view.
        /// </summary>
        /// <param name="fov">Field of view in the y direction, in radians.</param>
        /// <param name="aspect">Aspect ratio, defined as view space width divided by height.</param>
        /// <param name="znear">Minimum z-value of the viewing volume.</param>
        /// <param name="zfar">Maximum z-value of the viewing volume.</param>
        /// <param name="result">When the method completes, contains the created projection matrix.</param>
        public static void PerspectiveFovLH(float fov, float aspect, float znear, float zfar, out Matrix4F result)
        {
            float yScale = 1F / float.Tan(fov * 0.5F);
            float xScale = yScale / aspect;

            float halfWidth = znear / xScale;
            float halfHeight = znear / yScale;

            PerspectiveOffCenterLH(-halfWidth, halfWidth, -halfHeight, halfHeight, znear, zfar, out result);
        }

        /// <summary>
        /// Creates a left-handed, perspective projection matrix based on a field of view.
        /// </summary>
        /// <param name="fov">Field of view in the y direction, in radians.</param>
        /// <param name="aspect">Aspect ratio, defined as view space width divided by height.</param>
        /// <param name="znear">Minimum z-value of the viewing volume.</param>
        /// <param name="zfar">Maximum z-value of the viewing volume.</param>
        /// <returns>The created projection matrix.</returns>
        public static Matrix4F PerspectiveFovLH(float fov, float aspect, float znear, float zfar)
        {
            Matrix4F result;
            PerspectiveFovLH(fov, aspect, znear, zfar, out result);
            return result;
        }

        /// <summary>
        /// Creates a right-handed, perspective projection matrix based on a field of view.
        /// </summary>
        /// <param name="fov">Field of view in the y direction, in radians.</param>
        /// <param name="aspect">Aspect ratio, defined as view space width divided by height.</param>
        /// <param name="znear">Minimum z-value of the viewing volume.</param>
        /// <param name="zfar">Maximum z-value of the viewing volume.</param>
        /// <param name="result">When the method completes, contains the created projection matrix.</param>
        public static void PerspectiveFovRH(float fov, float aspect, float znear, float zfar, out Matrix4F result)
        {
            float yScale = 1F / float.Tan(fov * 0.5F);
            float xScale = yScale / aspect;

            float halfWidth = znear / xScale;
            float halfHeight = znear / yScale;

            PerspectiveOffCenterRH(-halfWidth, halfWidth, -halfHeight, halfHeight, znear, zfar, out result);
        }

        /// <summary>
        /// Creates a right-handed, perspective projection matrix based on a field of view.
        /// </summary>
        /// <param name="fov">Field of view in the y direction, in radians.</param>
        /// <param name="aspect">Aspect ratio, defined as view space width divided by height.</param>
        /// <param name="znear">Minimum z-value of the viewing volume.</param>
        /// <param name="zfar">Maximum z-value of the viewing volume.</param>
        /// <returns>The created projection matrix.</returns>
        public static Matrix4F PerspectiveFovRH(float fov, float aspect, float znear, float zfar)
        {
            PerspectiveFovRH(fov, aspect, znear, zfar, out Matrix4F result);
            return result;
        }

        /// <summary>
        /// Creates a left-handed, customized perspective projection matrix.
        /// </summary>
        /// <param name="left">Minimum x-value of the viewing volume.</param>
        /// <param name="right">Maximum x-value of the viewing volume.</param>
        /// <param name="bottom">Minimum y-value of the viewing volume.</param>
        /// <param name="top">Maximum y-value of the viewing volume.</param>
        /// <param name="znear">Minimum z-value of the viewing volume.</param>
        /// <param name="zfar">Maximum z-value of the viewing volume.</param>
        /// <param name="result">When the method completes, contains the created projection matrix.</param>
        public static void PerspectiveOffCenterLH(float left, float right, float bottom, float top, float znear, float zfar, out Matrix4F result)
        {
            float zRange = zfar / (zfar - znear);

            result = new Matrix4F();
            result.M11 = 2.0F * znear / (right - left);
            result.M22 = 2.0F * znear / (top - bottom);
            result.M31 = (left + right) / (left - right);
            result.M32 = (top + bottom) / (bottom - top);
            result.M33 = zRange;
            result.M34 = 1F;
            result.M43 = -znear * zRange;
        }

        /// <summary>
        /// Creates a left-handed, customized perspective projection matrix.
        /// </summary>
        /// <param name="left">Minimum x-value of the viewing volume.</param>
        /// <param name="right">Maximum x-value of the viewing volume.</param>
        /// <param name="bottom">Minimum y-value of the viewing volume.</param>
        /// <param name="top">Maximum y-value of the viewing volume.</param>
        /// <param name="znear">Minimum z-value of the viewing volume.</param>
        /// <param name="zfar">Maximum z-value of the viewing volume.</param>
        /// <returns>The created projection matrix.</returns>
        public static Matrix4F PerspectiveOffCenterLH(float left, float right, float bottom, float top, float znear, float zfar)
        {
            Matrix4F result;
            PerspectiveOffCenterLH(left, right, bottom, top, znear, zfar, out result);
            return result;
        }

        /// <summary>
        /// Creates a right-handed, customized perspective projection matrix.
        /// </summary>
        /// <param name="left">Minimum x-value of the viewing volume.</param>
        /// <param name="right">Maximum x-value of the viewing volume.</param>
        /// <param name="bottom">Minimum y-value of the viewing volume.</param>
        /// <param name="top">Maximum y-value of the viewing volume.</param>
        /// <param name="znear">Minimum z-value of the viewing volume.</param>
        /// <param name="zfar">Maximum z-value of the viewing volume.</param>
        /// <param name="result">When the method completes, contains the created projection matrix.</param>
        public static void PerspectiveOffCenterRH(float left, float right, float bottom, float top, float znear, float zfar, out Matrix4F result)
        {
            PerspectiveOffCenterLH(left, right, bottom, top, znear, zfar, out result);
            result.M31 *= -1F;
            result.M32 *= -1F;
            result.M33 *= -1F;
            result.M34 *= -1F;
        }

        /// <summary>
        /// Creates a right-handed, customized perspective projection matrix.
        /// </summary>
        /// <param name="left">Minimum x-value of the viewing volume.</param>
        /// <param name="right">Maximum x-value of the viewing volume.</param>
        /// <param name="bottom">Minimum y-value of the viewing volume.</param>
        /// <param name="top">Maximum y-value of the viewing volume.</param>
        /// <param name="znear">Minimum z-value of the viewing volume.</param>
        /// <param name="zfar">Maximum z-value of the viewing volume.</param>
        /// <returns>The created projection matrix.</returns>
        public static Matrix4F PerspectiveOffCenterRH(float left, float right, float bottom, float top, float znear, float zfar)
        {
            PerspectiveOffCenterRH(left, right, bottom, top, znear, zfar, out Matrix4F result);
            return result;
        }


        /// <summary>
        /// Creates a matrix that scales along the x-axis, y-axis, and y-axis.
        /// </summary>
        /// <param name="scale">Scaling factor for all three axes.</param>
        /// <param name="result">When the method completes, contains the created scaling matrix.</param>
        public static void Scaling(ref Vector3F scale, out Matrix4F result)
        {
            Scaling(scale.X, scale.Y, scale.Z, out result);
        }

        /// <summary>
        /// Creates a matrix that scales along the x-axis, y-axis, and y-axis.
        /// </summary>
        /// <param name="scale">Scaling factor for all three axes.</param>
        /// <returns>The created scaling matrix.</returns>
        public static Matrix4F Scaling(Vector3F scale)
        {
            Scaling(ref scale, out Matrix4F result);
            return result;
        }

        /// <summary>
        /// Creates a matrix that scales along the x-axis, y-axis, and y-axis.
        /// </summary>
        /// <param name="x">Scaling factor that is applied along the x-axis.</param>
        /// <param name="y">Scaling factor that is applied along the y-axis.</param>
        /// <param name="z">Scaling factor that is applied along the z-axis.</param>
        /// <param name="result">When the method completes, contains the created scaling matrix.</param>
        public static void Scaling(float x, float y, float z, out Matrix4F result)
        {
            result = Matrix4F.Identity;
            result.M11 = x;
            result.M22 = y;
            result.M33 = z;
        }

        /// <summary>
        /// Creates a matrix that scales along the x-axis, y-axis, and y-axis.
        /// </summary>
        /// <param name="x">Scaling factor that is applied along the x-axis.</param>
        /// <param name="y">Scaling factor that is applied along the y-axis.</param>
        /// <param name="z">Scaling factor that is applied along the z-axis.</param>
        /// <returns>The created scaling matrix.</returns>
        public static Matrix4F Scaling(float x, float y, float z)
        {
            Scaling(x, y, z, out Matrix4F result);
            return result;
        }

        /// <summary>
        /// Creates a matrix that uniformly scales along all three axis.
        /// </summary>
        /// <param name="scale">The uniform scale that is applied along all axis.</param>
        /// <param name="result">When the method completes, contains the created scaling matrix.</param>
        public static void Scaling(float scale, out Matrix4F result)
        {
            result = Matrix4F.Identity;
            result.M11 = result.M22 = result.M33 = scale;
        }

        /// <summary>
        /// Creates a matrix that uniformly scales along all three axis.
        /// </summary>
        /// <param name="scale">The uniform scale that is applied along all axis.</param>
        /// <returns>The created scaling matrix.</returns>
        public static Matrix4F Scaling(float scale)
        {
            Scaling(scale, out Matrix4F result);
            return result;
        }

        /// <summary>
        /// Creates a matrix that rotates around the x-axis.
        /// </summary>
        /// <param name="angle">Angle of rotation in radians. Angles are measured clockwise when looking along the rotation axis toward the origin.</param>
        /// <param name="result">When the method completes, contains the created rotation matrix.</param>
        public static void RotationX(float angle, out Matrix4F result)
        {
            float cos = float.Cos(angle);
            float sin = float.Sin(angle);

            result = Matrix4F.Identity;
            result.M22 = cos;
            result.M23 = sin;
            result.M32 = -sin;
            result.M33 = cos;
        }

        /// <summary>
        /// Creates a matrix that rotates around the x-axis.
        /// </summary>
        /// <param name="angle">Angle of rotation in radians. Angles are measured clockwise when looking along the rotation axis toward the origin.</param>
        /// <returns>The created rotation matrix.</returns>
        public static Matrix4F RotationX(float angle)
        {
            RotationX(angle, out Matrix4F result);
            return result;
        }

        /// <summary>
        /// Creates a matrix that rotates around the y-axis.
        /// </summary>
        /// <param name="angle">Angle of rotation in radians. Angles are measured clockwise when looking along the rotation axis toward the origin.</param>
        /// <param name="result">When the method completes, contains the created rotation matrix.</param>
        public static void RotationY(float angle, out Matrix4F result)
        {
            float cos = float.Cos(angle);
            float sin = float.Sin(angle);

            result = Matrix4F.Identity;
            result.M11 = cos;
            result.M13 = -sin;
            result.M31 = sin;
            result.M33 = cos;
        }

        /// <summary>
        /// Creates a matrix that rotates around the y-axis.
        /// </summary>
        /// <param name="angle">Angle of rotation in radians. Angles are measured clockwise when looking along the rotation axis toward the origin.</param>
        /// <returns>The created rotation matrix.</returns>
        public static Matrix4F RotationY(float angle)
        {
            RotationY(angle, out Matrix4F result);
            return result;
        }

        /// <summary>
        /// Creates a matrix that rotates around the z-axis.
        /// </summary>
        /// <param name="angle">Angle of rotation in radians. Angles are measured clockwise when looking along the rotation axis toward the origin.</param>
        /// <param name="result">When the method completes, contains the created rotation matrix.</param>
        public static void RotationZ(float angle, out Matrix4F result)
        {
            float cos = float.Cos(angle);
            float sin = float.Sin(angle);

            result = Matrix4F.Identity;
            result.M11 = cos;
            result.M12 = sin;
            result.M21 = -sin;
            result.M22 = cos;
        }

        /// <summary>
        /// Creates a matrix that rotates around the z-axis.
        /// </summary>
        /// <param name="angle">Angle of rotation in radians. Angles are measured clockwise when looking along the rotation axis toward the origin.</param>
        /// <returns>The created rotation matrix.</returns>
        public static Matrix4F RotationZ(float angle)
        {
            RotationZ(angle, out Matrix4F result);
            return result;
        }

        /// <summary>
        /// Creates a matrix that rotates around an arbitrary axis.
        /// </summary>
        /// <param name="axis">The axis around which to rotate. This parameter is assumed to be normalized.</param>
        /// <param name="angle">Angle of rotation in radians. Angles are measured clockwise when looking along the rotation axis toward the origin.</param>
        /// <param name="result">When the method completes, contains the created rotation matrix.</param>
        public static void RotationAxis(ref Vector3F axis, float angle, out Matrix4F result)
        {
            float x = axis.X;
            float y = axis.Y;
            float z = axis.Z;
            float cos = float.Cos(angle);
            float sin = float.Sin(angle);
            float xx = x * x;
            float yy = y * y;
            float zz = z * z;
            float xy = x * y;
            float xz = x * z;
            float yz = y * z;

            result = Matrix4F.Identity;
            result.M11 = xx + (cos * (1F - xx));
            result.M12 = (xy - (cos * xy)) + (sin * z);
            result.M13 = (xz - (cos * xz)) - (sin * y);
            result.M21 = (xy - (cos * xy)) - (sin * z);
            result.M22 = yy + (cos * (1F - yy));
            result.M23 = (yz - (cos * yz)) + (sin * x);
            result.M31 = (xz - (cos * xz)) + (sin * y);
            result.M32 = (yz - (cos * yz)) - (sin * x);
            result.M33 = zz + (cos * (1F - zz));
        }

        /// <summary>
        /// Creates a matrix that rotates around an arbitrary axis.
        /// </summary>
        /// <param name="axis">The axis around which to rotate. This parameter is assumed to be normalized.</param>
        /// <param name="angle">Angle of rotation in radians. Angles are measured clockwise when looking along the rotation axis toward the origin.</param>
        /// <returns>The created rotation matrix.</returns>
        public static Matrix4F RotationAxis(Vector3F axis, float angle)
        {
            RotationAxis(ref axis, angle, out Matrix4F result);
            return result;
        }

        /// <summary>
        /// Creates a rotation matrix from a quaternion.
        /// </summary>
        /// <param name="rotation">The quaternion to use to build the matrix.</param>
        /// <param name="result">The created rotation matrix.</param>
        public static void FromQuaternion(ref QuaternionF rotation, out Matrix4F result)
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

            result = Matrix4F.Identity;
            result.M11 = 1F - (2.0F * (yy + zz));
            result.M12 = 2.0F * (xy + zw);
            result.M13 = 2.0F * (zx - yw);
            result.M21 = 2.0F * (xy - zw);
            result.M22 = 1F - (2.0F * (zz + xx));
            result.M23 = 2.0F * (yz + xw);
            result.M31 = 2.0F * (zx + yw);
            result.M32 = 2.0F * (yz - xw);
            result.M33 = 1F - (2.0F * (yy + xx));
        }

        /// <summary>
        /// Creates a rotation matrix from a quaternion.
        /// </summary>
        /// <param name="rotation">The quaternion to use to build the matrix.</param>
        /// <returns>The created rotation matrix.</returns>
        public static Matrix4F FromQuaternion(QuaternionF rotation)
        {
            FromQuaternion(ref rotation, out Matrix4F result);
            return result;
        }

        /// <summary>
        /// Creates a rotation matrix from a quaternion.
        /// </summary>
        /// <param name="rotation">The quaternion to use to build the matrix.</param>
        /// <returns>The created rotation matrix.</returns>
        public static Matrix4F FromQuaternion(ref QuaternionF rotation)
        {
            FromQuaternion(ref rotation, out Matrix4F result);
            return result;
        }

        /// <summary>
        /// Creates a rotation matrix with a specified yaw, pitch, and roll.
        /// </summary>
        /// <param name="yaw">Yaw around the y-axis, in radians.</param>
        /// <param name="pitch">Pitch around the x-axis, in radians.</param>
        /// <param name="roll">Roll around the z-axis, in radians.</param>
        /// <param name="result">When the method completes, contains the created rotation matrix.</param>
        public static void RotationYawPitchRoll(float yaw, float pitch, float roll, out Matrix4F result)
        {
            QuaternionF.RotationYawPitchRoll(yaw, pitch, roll, out QuaternionF quaternion);
            FromQuaternion(ref quaternion, out result);
        }

        /// <summary>
        /// Creates a rotation matrix with a specified yaw, pitch, and roll.
        /// </summary>
        /// <param name="yaw">Yaw around the y-axis, in radians.</param>
        /// <param name="pitch">Pitch around the x-axis, in radians.</param>
        /// <param name="roll">Roll around the z-axis, in radians.</param>
        /// <returns>The created rotation matrix.</returns>
        public static Matrix4F RotationYawPitchRoll(float yaw, float pitch, float roll)
        {
            RotationYawPitchRoll(yaw, pitch, roll, out Matrix4F result);
            return result;
        }

        /// <summary>
        /// Creates a translation matrix using the specified offsets.
        /// </summary>
        /// <param name="value">The offset for all three coordinate planes.</param>
        /// <param name="result">When the method completes, contains the created translation matrix.</param>
        public static void CreateTranslation(ref Vector3F value, out Matrix4F result)
        {
            CreateTranslation(value.X, value.Y, value.Z, out result);
        }

        /// <summary>
        /// Creates a translation matrix using the specified offsets.
        /// </summary>
        /// <param name="value">The offset for all three coordinate planes.</param>
        /// <returns>The created translation matrix.</returns>
        public static Matrix4F CreateTranslation(Vector3F value)
        {
            CreateTranslation(ref value, out Matrix4F result);
            return result;
        }

        /// <summary>
        /// Creates a translation matrix using the specified offsets.
        /// </summary>
        /// <param name="x">X-coordinate offset.</param>
        /// <param name="y">Y-coordinate offset.</param>
        /// <param name="z">Z-coordinate offset.</param>
        /// <param name="result">When the method completes, contains the created translation matrix.</param>
        public static void CreateTranslation(float x, float y, float z, out Matrix4F result)
        {
            result = Matrix4F.Identity;
            result.M41 = x;
            result.M42 = y;
            result.M43 = z;
        }

        /// <summary>
        /// Creates a translation matrix using the specified offsets.
        /// </summary>
        /// <param name="x">X-coordinate offset.</param>
        /// <param name="y">Y-coordinate offset.</param>
        /// <param name="z">Z-coordinate offset.</param>
        /// <returns>The created translation matrix.</returns>
        public static Matrix4F CreateTranslation(float x, float y, float z)
        {
            CreateTranslation(x, y, z, out Matrix4F result);
            return result;
        }

        /// <summary>
        /// Creates a skew/shear matrix by means of a translation vector, a rotation vector, and a rotation angle.
        /// shearing is performed in the direction of translation vector, where translation vector and rotation vector define the shearing plane.
        /// The effect is such that the skewed rotation vector has the specified angle with rotation itself.
        /// </summary>
        /// <param name="angle">The rotation angle.</param>
        /// <param name="rotationVec">The rotation vector</param>
        /// <param name="transVec">The translation vector</param>
        /// <param name="matrix">Contains the created skew/shear matrix. </param>
        public static void CreateSkew(float angle, ref Vector3F rotationVec, ref Vector3F transVec, out Matrix4F matrix)
        {
            //http://elckerlyc.ewi.utwente.nl/browser/Elckerlyc/Hmi/HmiMath/src/hmi/math/Mat3f.java
            float MINIMAL_SKEW_ANGLE = 0.000001F;

            Vector3F e0 = rotationVec;
            Vector3F e1 = Vector3F.Normalize(transVec);

            float rv1 = Vector3F.Dot(ref rotationVec, ref  e1);
            e0 += rv1 * e1;
            float rv0 = Vector3F.Dot(ref rotationVec, ref e0);
            float cosa = float.Cos(angle);
            float sina = float.Sin(angle);
            float rr0 = rv0 * cosa - rv1 * sina;
            float rr1 = rv0 * sina + rv1 * cosa;

            if (rr0 < MINIMAL_SKEW_ANGLE)
                throw new ArgumentException("illegal skew angle");

            float d = (rr1 / rr0) - (rv1 / rv0);

            matrix = Matrix4F.Identity;
            matrix.M11 = d * e1[0] * e0[0] + 1F;
            matrix.M12 = d * e1[0] * e0[1];
            matrix.M13 = d * e1[0] * e0[2];
            matrix.M21 = d * e1[1] * e0[0];
            matrix.M22 = d * e1[1] * e0[1] + 1F;
            matrix.M23 = d * e1[1] * e0[2];
            matrix.M31 = d * e1[2] * e0[0];
            matrix.M32 = d * e1[2] * e0[1];
            matrix.M33 = d * e1[2] * e0[2] + 1F;
        }

        /// <summary>
        /// Creates a 3D affine transformation matrix.
        /// </summary>
        /// <param name="scaling">Scaling factor.</param>
        /// <param name="rotation">The rotation of the transformation.</param>
        /// <param name="translation">The translation factor of the transformation.</param>
        /// <param name="result">When the method completes, contains the created affine transformation matrix.</param>
        public static void AffineTransformation(float scaling, ref QuaternionF rotation, ref Vector3F translation, out Matrix4F result)
        {
            result = Scaling(scaling) * FromQuaternion(rotation) * CreateTranslation(translation);
        }

        /// <summary>
        /// Creates a 3D affine transformation matrix.
        /// </summary>
        /// <param name="scaling">Scaling factor.</param>
        /// <param name="rotation">The rotation of the transformation.</param>
        /// <param name="translation">The translation factor of the transformation.</param>
        /// <returns>The created affine transformation matrix.</returns>
        public static Matrix4F AffineTransformation(float scaling, QuaternionF rotation, Vector3F translation)
        {
            AffineTransformation(scaling, ref rotation, ref translation, out Matrix4F result);
            return result;
        }

        /// <summary>
        /// Creates a 3D affine transformation matrix.
        /// </summary>
        /// <param name="scaling">Scaling factor.</param>
        /// <param name="rotationCenter">The center of the rotation.</param>
        /// <param name="rotation">The rotation of the transformation.</param>
        /// <param name="translation">The translation factor of the transformation.</param>
        /// <param name="result">When the method completes, contains the created affine transformation matrix.</param>
        public static void AffineTransformation(float scaling, ref Vector3F rotationCenter, ref QuaternionF rotation, ref Vector3F translation, out Matrix4F result)
        {
            result = Scaling(scaling) * CreateTranslation(-rotationCenter) * FromQuaternion(rotation) *
                CreateTranslation(rotationCenter) * CreateTranslation(translation);
        }

        /// <summary>
        /// Creates a 3D affine transformation matrix.
        /// </summary>
        /// <param name="scaling">Scaling factor.</param>
        /// <param name="rotationCenter">The center of the rotation.</param>
        /// <param name="rotation">The rotation of the transformation.</param>
        /// <param name="translation">The translation factor of the transformation.</param>
        /// <returns>The created affine transformation matrix.</returns>
        public static Matrix4F AffineTransformation(float scaling, Vector3F rotationCenter, QuaternionF rotation, Vector3F translation)
        {
            AffineTransformation(scaling, ref rotationCenter, ref rotation, ref translation, out Matrix4F result);
            return result;
        }

        /// <summary>
        /// Creates a 2D affine transformation matrix.
        /// </summary>
        /// <param name="scaling">Scaling factor.</param>
        /// <param name="rotation">The rotation of the transformation.</param>
        /// <param name="translation">The translation factor of the transformation.</param>
        /// <param name="result">When the method completes, contains the created affine transformation matrix.</param>
        public static void AffineTransformation2D(float scaling, float rotation, ref Vector2F translation, out Matrix4F result)
        {
            result = Scaling(scaling, scaling, 1F) * RotationZ(rotation) * CreateTranslation((Vector3F)translation);
        }

        /// <summary>
        /// Creates a 2D affine transformation matrix.
        /// </summary>
        /// <param name="scaling">Scaling factor.</param>
        /// <param name="rotation">The rotation of the transformation.</param>
        /// <param name="translation">The translation factor of the transformation.</param>
        /// <returns>The created affine transformation matrix.</returns>
        public static Matrix4F AffineTransformation2D(float scaling, float rotation, Vector2F translation)
        {
            AffineTransformation2D(scaling, rotation, ref translation, out Matrix4F result);
            return result;
        }

        /// <summary>
        /// Creates a 2D affine transformation matrix.
        /// </summary>
        /// <param name="scaling">Scaling factor.</param>
        /// <param name="rotationCenter">The center of the rotation.</param>
        /// <param name="rotation">The rotation of the transformation.</param>
        /// <param name="translation">The translation factor of the transformation.</param>
        /// <param name="result">When the method completes, contains the created affine transformation matrix.</param>
        public static void AffineTransformation2D(float scaling, ref Vector2F rotationCenter, float rotation, ref Vector2F translation, out Matrix4F result)
        {
            result = Scaling(scaling, scaling, 1F) * CreateTranslation((Vector3F)(-rotationCenter)) * RotationZ(rotation) *
                CreateTranslation((Vector3F)rotationCenter) * CreateTranslation((Vector3F)translation);
        }

        /// <summary>
        /// Creates a 2D affine transformation matrix.
        /// </summary>
        /// <param name="scaling">Scaling factor.</param>
        /// <param name="rotationCenter">The center of the rotation.</param>
        /// <param name="rotation">The rotation of the transformation.</param>
        /// <param name="translation">The translation factor of the transformation.</param>
        /// <returns>The created affine transformation matrix.</returns>
        public static Matrix4F AffineTransformation2D(float scaling, Vector2F rotationCenter, float rotation, Vector2F translation)
        {
            AffineTransformation2D(scaling, ref rotationCenter, rotation, ref translation, out Matrix4F result);
            return result;
        }

        /// <summary>
        /// Creates a transformation matrix.
        /// </summary>
        /// <param name="scalingCenter">Center point of the scaling operation.</param>
        /// <param name="scalingRotation">Scaling rotation amount.</param>
        /// <param name="scaling">Scaling factor.</param>
        /// <param name="rotationCenter">The center of the rotation.</param>
        /// <param name="rotation">The rotation of the transformation.</param>
        /// <param name="translation">The translation factor of the transformation.</param>
        /// <param name="result">When the method completes, contains the created transformation matrix.</param>
        public static void Transformation(ref Vector3F scalingCenter, ref QuaternionF scalingRotation, ref Vector3F scaling, ref Vector3F rotationCenter, ref QuaternionF rotation, ref Vector3F translation, out Matrix4F result)
        {
            FromQuaternion(ref scalingRotation, out Matrix4F sr);

            result = CreateTranslation(-scalingCenter) * Transpose(sr) * Scaling(scaling) * sr * CreateTranslation(scalingCenter) * CreateTranslation(-rotationCenter) *
                FromQuaternion(rotation) * CreateTranslation(rotationCenter) * CreateTranslation(translation);       
        }

        /// <summary>
        /// Creates a transformation matrix.
        /// </summary>
        /// <param name="scalingCenter">Center point of the scaling operation.</param>
        /// <param name="scalingRotation">Scaling rotation amount.</param>
        /// <param name="scaling">Scaling factor.</param>
        /// <param name="rotationCenter">The center of the rotation.</param>
        /// <param name="rotation">The rotation of the transformation.</param>
        /// <param name="translation">The translation factor of the transformation.</param>
        /// <returns>The created transformation matrix.</returns>
        public static Matrix4F Transformation(Vector3F scalingCenter, QuaternionF scalingRotation, Vector3F scaling, Vector3F rotationCenter, QuaternionF rotation, Vector3F translation)
        {
            Transformation(ref scalingCenter, 
                ref scalingRotation, 
                ref scaling,
                ref rotationCenter, 
                ref rotation, 
                ref translation, 
                out Matrix4F result);
            return result;
        }

        /// <summary>
        /// Creates a 2D transformation matrix.
        /// </summary>
        /// <param name="scalingCenter">Center point of the scaling operation.</param>
        /// <param name="scalingRotation">Scaling rotation amount.</param>
        /// <param name="scaling">Scaling factor.</param>
        /// <param name="rotationCenter">The center of the rotation.</param>
        /// <param name="rotation">The rotation of the transformation.</param>
        /// <param name="translation">The translation factor of the transformation.</param>
        /// <param name="result">When the method completes, contains the created transformation matrix.</param>
        public static void Transformation2D(ref Vector2F scalingCenter, float scalingRotation, ref Vector2F scaling, ref Vector2F rotationCenter, float rotation, ref Vector2F translation, out Matrix4F result)
        {
            result = CreateTranslation((Vector3F)(-scalingCenter)) * 
                RotationZ(-scalingRotation) * 
                Scaling((Vector3F)scaling) * 
                RotationZ(scalingRotation) * 
                CreateTranslation((Vector3F)scalingCenter) * 
                CreateTranslation((Vector3F)(-rotationCenter)) * 
                RotationZ(rotation) * 
                CreateTranslation((Vector3F)rotationCenter) * 
                CreateTranslation((Vector3F)translation);

            result.M33 = 1F;
            result.M44 = 1F;
        }

        /// <summary>
        /// Creates a 2D transformation matrix.
        /// </summary>
        /// <param name="scalingCenter">Center point of the scaling operation.</param>
        /// <param name="scalingRotation">Scaling rotation amount.</param>
        /// <param name="scaling">Scaling factor.</param>
        /// <param name="rotationCenter">The center of the rotation.</param>
        /// <param name="rotation">The rotation of the transformation.</param>
        /// <param name="translation">The translation factor of the transformation.</param>
        /// <returns>The created transformation matrix.</returns>
        public static Matrix4F Transformation2D(Vector2F scalingCenter, float scalingRotation, Vector2F scaling, Vector2F rotationCenter, float rotation, Vector2F translation)
        {
            Transformation2D(ref scalingCenter, scalingRotation, ref scaling, ref rotationCenter, rotation, ref translation, out Matrix4F result);
            return result;
        }

        /// <summary>
        /// Multiplies two matrices.
        /// </summary>
        /// <param name="left">The first matrix to multiply.</param>
        /// <param name="right">The second matrix to multiply.</param>
        /// <returns>The product of the two matrices.</returns>
        public static Matrix4F operator *(Matrix4F left, Matrix4F right)
        {
            Multiply(ref left, ref right, out Matrix4F result);
            return result;
        }
    }
}

