// James Yarwood - Updated for Molten Engine - MIT license

// Copyright (c) 2010-2014 SharpDX - Alexandre Mutel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// -----------------------------------------------------------------------------
// Original code from SlimMath project. http://code.google.com/p/slimmath/
// Greetings to SlimDX Group. Original code published with the following license:
// -----------------------------------------------------------------------------
/*
* Copyright (c) 2007-2011 SlimDX Group
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/

using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Molten
{
    /// <summary>Represents a single-precision 4x4 Matrix. Contains position, scale, rotation and transform.</summary>
    [StructLayout(LayoutKind.Explicit)]
    [DataContract]
    public struct Matrix4F : IEquatable<Matrix4F>, IFormattable, ITransposedMatrix<Matrix4F>
    {
        /// <summary>
        /// The size of the <see cref="Matrix4F"/> type, in bytes.
        /// </summary>
        public static readonly int SizeInBytes = 4 * 4 * sizeof(float);

        /// <summary>
        /// A <see cref="Matrix4F"/> with all of its components set to zero.
        /// </summary>
        public static readonly Matrix4F Zero = new Matrix4F();

        /// <summary>
        /// The identity <see cref="Matrix4F"/>.
        /// </summary>
        public static readonly Matrix4F Identity = new Matrix4F() { M11 = 1F, M22 = 1F, M33 = 1F, M44 = 1F };

		/// <summary>The value at row 1, column 1 of the matrix.</summary>
		[DataMember]
		[FieldOffset(0)]
		public float M11;

		/// <summary>The value at row 1, column 2 of the matrix.</summary>
		[DataMember]
		[FieldOffset(4)]
		public float M12;

		/// <summary>The value at row 1, column 3 of the matrix.</summary>
		[DataMember]
		[FieldOffset(8)]
		public float M13;

		/// <summary>The value at row 1, column 4 of the matrix.</summary>
		[DataMember]
		[FieldOffset(12)]
		public float M14;

		/// <summary>The value at row 2, column 1 of the matrix.</summary>
		[DataMember]
		[FieldOffset(16)]
		public float M21;

		/// <summary>The value at row 2, column 2 of the matrix.</summary>
		[DataMember]
		[FieldOffset(20)]
		public float M22;

		/// <summary>The value at row 2, column 3 of the matrix.</summary>
		[DataMember]
		[FieldOffset(24)]
		public float M23;

		/// <summary>The value at row 2, column 4 of the matrix.</summary>
		[DataMember]
		[FieldOffset(28)]
		public float M24;

		/// <summary>The value at row 3, column 1 of the matrix.</summary>
		[DataMember]
		[FieldOffset(32)]
		public float M31;

		/// <summary>The value at row 3, column 2 of the matrix.</summary>
		[DataMember]
		[FieldOffset(36)]
		public float M32;

		/// <summary>The value at row 3, column 3 of the matrix.</summary>
		[DataMember]
		[FieldOffset(40)]
		public float M33;

		/// <summary>The value at row 3, column 4 of the matrix.</summary>
		[DataMember]
		[FieldOffset(44)]
		public float M34;

		/// <summary>The value at row 4, column 1 of the matrix.</summary>
		[DataMember]
		[FieldOffset(48)]
		public float M41;

		/// <summary>The value at row 4, column 2 of the matrix.</summary>
		[DataMember]
		[FieldOffset(52)]
		public float M42;

		/// <summary>The value at row 4, column 3 of the matrix.</summary>
		[DataMember]
		[FieldOffset(56)]
		public float M43;

		/// <summary>The value at row 4, column 4 of the matrix.</summary>
		[DataMember]
		[FieldOffset(60)]
		public float M44;

		/// <summary>A fixed array mapped to the same memory space as the individual vector components.</summary>
		[IgnoreDataMember]
		[FieldOffset(0)]
		public unsafe fixed float Values[16];

     
        /// <summary>
        /// Gets or sets the up <see cref="Vector3F"/> of the matrix; that is M21, M22, and M23.
        /// </summary>
        public Vector3F Up
        {
          get
          {
            Vector3F vector3;
            vector3.X = M21;
            vector3.Y = M22;
            vector3.Z = M23;
            return vector3;
          }
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
          get
          {
            Vector3F vector3;
            vector3.X = -M21;
            vector3.Y = -M22;
            vector3.Z = -M23;
            return vector3;
          }
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
          get
          {
            Vector3F vector3;
            vector3.X = M11;
            vector3.Y = M12;
            vector3.Z = M13;
            return vector3;
          }
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
          get
          {
            Vector3F vector3;
            vector3.X = -M11;
            vector3.Y = -M12;
            vector3.Z = -M13;
            return vector3;
          }
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
          get
          {
            Vector3F vector3;
            vector3.X = -M31;
            vector3.Y = -M32;
            vector3.Z = -M33;
            return vector3;
          }
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
          get
          {
            Vector3F vector3;
            vector3.X = M31;
            vector3.Y = M32;
            vector3.Z = M33;
            return vector3;
          }
          set
          {
            M31 = value.X;
            M32 = value.Y;
            M33 = value.Z;
          }
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Matrix4F"/> struct.
        /// </summary>
        /// <param name="value">The value that will be assigned to all components.</param>
        public Matrix4F(float value)
        {
            M11 = M12 = M13 = M14 =
            M21 = M22 = M23 = M24 =
            M31 = M32 = M33 = M34 =
            M41 = M42 = M43 = M44 = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Matrix4F"/> struct.
        /// </summary>
        /// <param name="m11">The value to assign at row 1 column 1 of the matrix.</param>
        /// <param name="m12">The value to assign at row 1 column 2 of the matrix.</param>
        /// <param name="m13">The value to assign at row 1 column 3 of the matrix.</param>
        /// <param name="m14">The value to assign at row 1 column 4 of the matrix.</param>
        /// <param name="m21">The value to assign at row 2 column 1 of the matrix.</param>
        /// <param name="m22">The value to assign at row 2 column 2 of the matrix.</param>
        /// <param name="m23">The value to assign at row 2 column 3 of the matrix.</param>
        /// <param name="m24">The value to assign at row 2 column 4 of the matrix.</param>
        /// <param name="m31">The value to assign at row 3 column 1 of the matrix.</param>
        /// <param name="m32">The value to assign at row 3 column 2 of the matrix.</param>
        /// <param name="m33">The value to assign at row 3 column 3 of the matrix.</param>
        /// <param name="m34">The value to assign at row 3 column 4 of the matrix.</param>
        /// <param name="m41">The value to assign at row 4 column 1 of the matrix.</param>
        /// <param name="m42">The value to assign at row 4 column 2 of the matrix.</param>
        /// <param name="m43">The value to assign at row 4 column 3 of the matrix.</param>
        /// <param name="m44">The value to assign at row 4 column 4 of the matrix.</param>
        public Matrix4F(float m11, float m12, float m13, float m14,
            float m21, float m22, float m23, float m24,
            float m31, float m32, float m33, float m34,
            float m41, float m42, float m43, float m44)
        {
            M11 = m11; M12 = m12; M13 = m13; M14 = m14;
            M21 = m21; M22 = m22; M23 = m23; M24 = m24;
            M31 = m31; M32 = m32; M33 = m33; M34 = m34;
            M41 = m41; M42 = m42; M43 = m43; M44 = m44;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Matrix4F"/> struct.
        /// </summary>
        /// <param name="values">The values to assign to the components of the matrix. This must be an array with sixteen elements.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than sixteen elements.</exception>
        public Matrix4F(float[] values)
        {
            if (values == null)
                throw new ArgumentNullException("values");
            if (values.Length != 16)
                throw new ArgumentOutOfRangeException("values", "There must be sixteen and only sixteen input values for Matrix.");

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
        /// Gets a value indicating whether this instance is an identity matrix.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is an identity matrix; otherwise, <c>false</c>.
        /// </value>
        public bool IsIdentity => Equals(Identity);

        /// <summary>
        /// Gets or sets the component at the specified index.
        /// </summary>
        /// <value>The value of the matrix component, depending on the index.</value>
        /// <param name="index">The zero-based index of the component to access.</param>
        /// <returns>The value of the component at the specified index.</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown when the <paramref name="index"/> is outside the range [0, 15].</exception>  
		public unsafe float this[int index]
		{
			get
            {
                if(index > 15 || index < 0)
                    throw new IndexOutOfRangeException("Index for Matrix4F must be between from 0 to 15, inclusive.");

                return Values[index];
            }
            set
            {
                if (index > 15 || index < 0)
                    throw new IndexOutOfRangeException("Index for Matrix4F must be between from 0 to 15, inclusive.");

                Values[index] = value;
            }
		}

        /// <summary>
        /// Gets or sets the component at the specified index.
        /// </summary>
        /// <value>The value of the matrix component, depending on the index.</value>
        /// <param name="index">The zero-based index of the component to access.</param>
        /// <returns>The value of the component at the specified index.</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown when the <paramref name="index"/> is greater than 15.</exception>
		public unsafe float this[uint index]
		{
			get
            {
                if(index > 15)
                    throw new IndexOutOfRangeException("Index for Matrix4F must less than 16.");

                return Values[index];
            }
            set
            {
                if (index > 15)
                    throw new IndexOutOfRangeException("Index for Matrix4F must less than 16.");

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
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="row"/> or <paramref name="column"/>is out of the range [0, 3].</exception>
        public float this[int row, int column]
        {
            get
            {
                if (row < 0 || row > 3)
                    throw new ArgumentOutOfRangeException("row", "Rows and columns for matrices run from 0 to 3, inclusive.");
                if (column < 0 || column > 3)
                    throw new ArgumentOutOfRangeException("column", "Rows and columns for matrices run from 0 to 3, inclusive.");

                return this[(row * 4) + column];
            }

            set
            {
                if (row < 0 || row > 3)
                    throw new ArgumentOutOfRangeException("row", "Rows and columns for matrices run from 0 to 3, inclusive.");
                if (column < 0 || column > 3)
                    throw new ArgumentOutOfRangeException("column", "Rows and columns for matrices run from 0 to 3, inclusive.");

                this[(row * 4) + column] = value;
            }
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
        /// Creates an array containing the elements of the matrix.
        /// </summary>
        /// <returns>A sixteen-element array containing the components of the matrix.</returns>
        public float[] ToArray()
        {
            return new[] { M11, M12, M13, M14, M21, M22, M23, M24, M31, M32, M33, M34, M41, M42, M43, M44 };
        }

        /// <summary>
        /// Determines the sum of two matrices.
        /// </summary>
        /// <param name="left">The first matrix to add.</param>
        /// <param name="right">The second matrix to add.</param>
        /// <param name="result">When the method completes, contains the sum of the two matrices.</param>
        public static void Add(ref Matrix4F left, ref Matrix4F right, out Matrix4F result)
        {
            result.M11 = left.M11 + right.M11;
            result.M12 = left.M12 + right.M12;
            result.M13 = left.M13 + right.M13;
            result.M14 = left.M14 + right.M14;
            result.M21 = left.M21 + right.M21;
            result.M22 = left.M22 + right.M22;
            result.M23 = left.M23 + right.M23;
            result.M24 = left.M24 + right.M24;
            result.M31 = left.M31 + right.M31;
            result.M32 = left.M32 + right.M32;
            result.M33 = left.M33 + right.M33;
            result.M34 = left.M34 + right.M34;
            result.M41 = left.M41 + right.M41;
            result.M42 = left.M42 + right.M42;
            result.M43 = left.M43 + right.M43;
            result.M44 = left.M44 + right.M44;
        }

        /// <summary>
        /// Determines the sum of two matrices.
        /// </summary>
        /// <param name="left">The first matrix to add.</param>
        /// <param name="right">The second matrix to add.</param>
        /// <returns>The sum of the two matrices.</returns>
        public static Matrix4F Add(Matrix4F left, Matrix4F right)
        {
            Matrix4F result;
            Add(ref left, ref right, out result);
            return result;
        }

        /// <summary>
        /// Determines the difference between two matrices.
        /// </summary>
        /// <param name="left">The first matrix to subtract.</param>
        /// <param name="right">The second matrix to subtract.</param>
        /// <param name="result">When the method completes, contains the difference between the two matrices.</param>
        public static void Subtract(ref Matrix4F left, ref Matrix4F right, out Matrix4F result)
        {
            result.M11 = left.M11 - right.M11;
            result.M12 = left.M12 - right.M12;
            result.M13 = left.M13 - right.M13;
            result.M14 = left.M14 - right.M14;
            result.M21 = left.M21 - right.M21;
            result.M22 = left.M22 - right.M22;
            result.M23 = left.M23 - right.M23;
            result.M24 = left.M24 - right.M24;
            result.M31 = left.M31 - right.M31;
            result.M32 = left.M32 - right.M32;
            result.M33 = left.M33 - right.M33;
            result.M34 = left.M34 - right.M34;
            result.M41 = left.M41 - right.M41;
            result.M42 = left.M42 - right.M42;
            result.M43 = left.M43 - right.M43;
            result.M44 = left.M44 - right.M44;
        }

        /// <summary>
        /// Determines the difference between two matrices.
        /// </summary>
        /// <param name="left">The first matrix to subtract.</param>
        /// <param name="right">The second matrix to subtract.</param>
        /// <returns>The difference between the two matrices.</returns>
        public static Matrix4F Subtract(Matrix4F left, Matrix4F right)
        {
            Matrix4F result;
            Subtract(ref left, ref right, out result);
            return result;
        }

        /// <summary>
        /// Scales a matrix by the given value.
        /// </summary>
        /// <param name="left">The matrix to scale.</param>
        /// <param name="right">The amount by which to scale.</param>
        /// <param name="result">When the method completes, contains the scaled matrix.</param>
        public static void Multiply(ref Matrix4F left, float right, out Matrix4F result)
        {
            result.M11 = left.M11 * right;
            result.M12 = left.M12 * right;
            result.M13 = left.M13 * right;
            result.M14 = left.M14 * right;
            result.M21 = left.M21 * right;
            result.M22 = left.M22 * right;
            result.M23 = left.M23 * right;
            result.M24 = left.M24 * right;
            result.M31 = left.M31 * right;
            result.M32 = left.M32 * right;
            result.M33 = left.M33 * right;
            result.M34 = left.M34 * right;
            result.M41 = left.M41 * right;
            result.M42 = left.M42 * right;
            result.M43 = left.M43 * right;
            result.M44 = left.M44 * right;
        }

        /// <summary>
        /// Scales a matrix by the given value.
        /// </summary>
        /// <param name="left">The matrix to scale.</param>
        /// <param name="right">The amount by which to scale.</param>
        /// <returns>The scaled matrix.</returns>
        public static Matrix4F Multiply(Matrix4F left, float right)
        {
            Matrix4F result;
            Multiply(ref left, right, out result);
            return result;
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
        /// Determines the product of two matrices.
        /// </summary>
        /// <param name="left">The first matrix to multiply.</param>
        /// <param name="right">The second matrix to multiply.</param>
        /// <returns>The product of the two matrices.</returns>
        public static Matrix4F Multiply(Matrix4F left, Matrix4F right)
        {
            Matrix4F result;
            Multiply(ref left, ref right, out result);
            return result;
        }

        /// <summary>
        /// Scales a matrix by the given value.
        /// </summary>
        /// <param name="left">The matrix to scale.</param>
        /// <param name="right">The amount by which to scale.</param>
        /// <param name="result">When the method completes, contains the scaled matrix.</param>
        public static void Divide(ref Matrix4F left, float right, out Matrix4F result)
        {
            float inv = 1F / right;

            result.M11 = left.M11 * inv;
            result.M12 = left.M12 * inv;
            result.M13 = left.M13 * inv;
            result.M14 = left.M14 * inv;
            result.M21 = left.M21 * inv;
            result.M22 = left.M22 * inv;
            result.M23 = left.M23 * inv;
            result.M24 = left.M24 * inv;
            result.M31 = left.M31 * inv;
            result.M32 = left.M32 * inv;
            result.M33 = left.M33 * inv;
            result.M34 = left.M34 * inv;
            result.M41 = left.M41 * inv;
            result.M42 = left.M42 * inv;
            result.M43 = left.M43 * inv;
            result.M44 = left.M44 * inv;
        }

        /// <summary>
        /// Scales a matrix by the given value.
        /// </summary>
        /// <param name="left">The matrix to scale.</param>
        /// <param name="right">The amount by which to scale.</param>
        /// <returns>The scaled matrix.</returns>
        public static Matrix4F Divide(Matrix4F left, float right)
        {
            Matrix4F result;
            Divide(ref left, right, out result);
            return result;
        }

        /// <summary>
        /// Determines the quotient of two matrices.
        /// </summary>
        /// <param name="left">The first matrix to divide.</param>
        /// <param name="right">The second matrix to divide.</param>
        /// <param name="result">When the method completes, contains the quotient of the two matrices.</param>
        public static void Divide(ref Matrix4F left, ref Matrix4F right, out Matrix4F result)
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
        /// Determines the quotient of two matrices.
        /// </summary>
        /// <param name="left">The first matrix to divide.</param>
        /// <param name="right">The second matrix to divide.</param>
        /// <returns>The quotient of the two matrices.</returns>
        public static Matrix4F Divide(Matrix4F left, Matrix4F right)
        {
            Matrix4F result;
            Divide(ref left, ref right, out result);
            return result;
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
        /// Negates a matrix.
        /// </summary>
        /// <param name="value">The matrix to be negated.</param>
        /// <param name="result">When the method completes, contains the negated matrix.</param>
        public static void Negate(ref Matrix4F value, out Matrix4F result)
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
        /// Negates a matrix.
        /// </summary>
        /// <param name="value">The matrix to be negated.</param>
        /// <returns>The negated matrix.</returns>
        public static Matrix4F Negate(Matrix4F value)
        {
            Matrix4F result;
            Negate(ref value, out result);
            return result;
        }

        /// <summary>
        /// Performs a linear interpolation between two matrices.
        /// </summary>
        /// <param name="start">Start matrix.</param>
        /// <param name="end">End matrix.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <param name="result">When the method completes, contains the linear interpolation of the two matrices.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        public static void Lerp(ref Matrix4F start, ref Matrix4F end, float amount, out Matrix4F result)
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
        /// Performs a linear interpolation between two matrices.
        /// </summary>
        /// <param name="start">Start matrix.</param>
        /// <param name="end">End matrix.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <returns>The linear interpolation of the two matrices.</returns>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        public static Matrix4F Lerp(Matrix4F start, Matrix4F end, float amount)
        {
            Matrix4F result;
            Lerp(ref start, ref end, amount, out result);
            return result;
        }

        /// <summary>
        /// Performs a cubic interpolation between two matrices.
        /// </summary>
        /// <param name="start">Start matrix.</param>
        /// <param name="end">End matrix.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <param name="result">When the method completes, contains the cubic interpolation of the two matrices.</param>
        public static void SmoothStep(ref Matrix4F start, ref Matrix4F end, float amount, out Matrix4F result)
        {
            amount = MathHelper.SmoothStep(amount);
            Lerp(ref start, ref end, amount, out result);
        }

        /// <summary>
        /// Performs a cubic interpolation between two matrices.
        /// </summary>
        /// <param name="start">Start matrix.</param>
        /// <param name="end">End matrix.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <returns>The cubic interpolation of the two matrices.</returns>
        public static Matrix4F SmoothStep(Matrix4F start, Matrix4F end, float amount)
        {
            Matrix4F result;
            SmoothStep(ref start, ref end, amount, out result);
            return result;
        }

        /// <summary>
        /// Transposes the matrix in-place.
        /// </summary>
        public void Transpose()
        {
            Transpose(ref this, out this);
        }

        /// <summary>
        /// Transposes the matrix and outputs it to <paramref name="result"/>.
        /// </summary>
        /// <param name="result"></param>
        public void Transpose(out Matrix4F result)
        {
            Transpose(ref this, out result);
        }

        /// <summary>
        /// Calculates the transpose of the specified matrix.
        /// </summary>
        /// <param name="value">The MatrixDouble whose transpose is to be calculated.</param>
        /// <param name="result">When the method completes, contains the transpose of the specified matrix.</param>
        public static void Transpose(ref Matrix4F value, out Matrix4F result)
        {
            Matrix4F temp = new Matrix4F();
            temp.M11 = value.M11;
            temp.M12 = value.M21;
            temp.M13 = value.M31;
            temp.M14 = value.M41;
            temp.M21 = value.M12;
            temp.M22 = value.M22;
            temp.M23 = value.M32;
            temp.M24 = value.M42;
            temp.M31 = value.M13;
            temp.M32 = value.M23;
            temp.M33 = value.M33;
            temp.M34 = value.M43;
            temp.M41 = value.M14;
            temp.M42 = value.M24;
            temp.M43 = value.M34;
            temp.M44 = value.M44;

            result = temp;
        }

        /// <summary>
        /// Calculates the transpose of the specified matrix.
        /// </summary>
        /// <param name="value">The matrix whose transpose is to be calculated.</param>
        /// <param name="result">When the method completes, contains the transpose of the specified matrix.</param>
        public static void TransposeByRef(ref Matrix4F value, ref Matrix4F result)
        {
            result.M11 = value.M11;
            result.M12 = value.M21;
            result.M13 = value.M31;
            result.M14 = value.M41;
            result.M21 = value.M12;
            result.M22 = value.M22;
            result.M23 = value.M32;
            result.M24 = value.M42;
            result.M31 = value.M13;
            result.M32 = value.M23;
            result.M33 = value.M33;
            result.M34 = value.M43;
            result.M41 = value.M14;
            result.M42 = value.M24;
            result.M43 = value.M34;
            result.M44 = value.M44;
        }

        /// <summary>
        /// Calculates the transpose of the specified matrix.
        /// </summary>
        /// <param name="value">The matrix whose transpose is to be calculated.</param>
        /// <returns>The transpose of the specified matrix.</returns>
        public static Matrix4F Transpose(Matrix4F value)
        {
            Matrix4F result;
            Transpose(ref value, out result);
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
            Matrix4F result;
            FromQuaternion(ref rotation, out result);
            return result;
        }

        /// <summary>
        /// Creates a rotation matrix from a quaternion.
        /// </summary>
        /// <param name="rotation">The quaternion to use to build the matrix.</param>
        /// <returns>The created rotation matrix.</returns>
        public static Matrix4F FromQuaternion(ref QuaternionF rotation)
        {
            Matrix4F result;
            FromQuaternion(ref rotation, out result);
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
        /// Adds two matrices.
        /// </summary>
        /// <param name="left">The first matrix to add.</param>
        /// <param name="right">The second matrix to add.</param>
        /// <returns>The sum of the two matrices.</returns>
        public static Matrix4F operator +(Matrix4F left, Matrix4F right)
        {
            Add(ref left, ref right, out Matrix4F result);
            return result;
        }

        /// <summary>
        /// Assert a matrix (return it unchanged).
        /// </summary>
        /// <param name="value">The matrix to assert (unchanged).</param>
        /// <returns>The asserted (unchanged) matrix.</returns>
        public static Matrix4F operator +(Matrix4F value)
        {
            return value;
        }

        /// <summary>
        /// Subtracts two matrices.
        /// </summary>
        /// <param name="left">The first matrix to subtract.</param>
        /// <param name="right">The second matrix to subtract.</param>
        /// <returns>The difference between the two matrices.</returns>
        public static Matrix4F operator -(Matrix4F left, Matrix4F right)
        {
            Subtract(ref left, ref right, out Matrix4F result);
            return result;
        }

        /// <summary>
        /// Negates a matrix.
        /// </summary>
        /// <param name="value">The matrix to negate.</param>
        /// <returns>The negated matrix.</returns>
        public static Matrix4F operator -(Matrix4F value)
        {
            Negate(ref value, out Matrix4F result);
            return result;
        }

        /// <summary>
        /// Scales a matrix by a given value.
        /// </summary>
        /// <param name="right">The matrix to scale.</param>
        /// <param name="left">The amount by which to scale.</param>
        /// <returns>The scaled matrix.</returns>
        public static Matrix4F operator *(float left, Matrix4F right)
        {
            Multiply(ref right, left, out Matrix4F result);
            return result;
        }

        /// <summary>
        /// Scales a matrix by a given value.
        /// </summary>
        /// <param name="left">The matrix to scale.</param>
        /// <param name="right">The amount by which to scale.</param>
        /// <returns>The scaled matrix.</returns>
        public static Matrix4F operator *(Matrix4F left, float right)
        {
            Multiply(ref left, right, out Matrix4F result);
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

        /// <summary>
        /// Scales a matrix by a given value.
        /// </summary>
        /// <param name="left">The matrix to scale.</param>
        /// <param name="right">The amount by which to scale.</param>
        /// <returns>The scaled matrix.</returns>
        public static Matrix4F operator /(Matrix4F left, float right)
        {
            Divide(ref left, right, out Matrix4F result);
            return result;
        }

        /// <summary>
        /// Divides two matrices.
        /// </summary>
        /// <param name="left">The first matrix to divide.</param>
        /// <param name="right">The second matrix to divide.</param>
        /// <returns>The quotient of the two matrices.</returns>
        public static Matrix4F operator /(Matrix4F left, Matrix4F right)
        {
            Divide(ref left, ref right, out Matrix4F result);
            return result;
        }

        /// <summary>
        /// Tests for equality between two objects.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> has the same value as <paramref name="right"/>; otherwise, <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Matrix4F left, Matrix4F right)
        {
            return left.Equals(ref right);
        }

        /// <summary>
        /// Tests for inequality between two objects.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> has a different value than <paramref name="right"/>; otherwise, <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Matrix4F left, Matrix4F right)
        {
            return !left.Equals(ref right);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "[M11:{0} M12:{1} M13:{2} M14:{3}] [M21:{4} M22:{5} M23:{6} M24:{7}] [M31:{8} M32:{9} M33:{10} M34:{11}] [M41:{12} M42:{13} M43:{14} M44:{15}]",
                M11, M12, M13, M14, M21, M22, M23, M24, M31, M32, M33, M34, M41, M42, M43, M44);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public string ToString(string format)
        {
            if (format == null)
                return ToString();

            return string.Format(format, CultureInfo.CurrentCulture, "[M11:{0} M12:{1} M13:{2} M14:{3}] [M21:{4} M22:{5} M23:{6} M24:{7}] [M31:{8} M32:{9} M33:{10} M34:{11}] [M41:{12} M42:{13} M43:{14} M44:{15}]",
                M11.ToString(format, CultureInfo.CurrentCulture), M12.ToString(format, CultureInfo.CurrentCulture), M13.ToString(format, CultureInfo.CurrentCulture), M14.ToString(format, CultureInfo.CurrentCulture),
                M21.ToString(format, CultureInfo.CurrentCulture), M22.ToString(format, CultureInfo.CurrentCulture), M23.ToString(format, CultureInfo.CurrentCulture), M24.ToString(format, CultureInfo.CurrentCulture),
                M31.ToString(format, CultureInfo.CurrentCulture), M32.ToString(format, CultureInfo.CurrentCulture), M33.ToString(format, CultureInfo.CurrentCulture), M34.ToString(format, CultureInfo.CurrentCulture),
                M41.ToString(format, CultureInfo.CurrentCulture), M42.ToString(format, CultureInfo.CurrentCulture), M43.ToString(format, CultureInfo.CurrentCulture), M44.ToString(format, CultureInfo.CurrentCulture));
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public string ToString(IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, "[M11:{0} M12:{1} M13:{2} M14:{3}] [M21:{4} M22:{5} M23:{6} M24:{7}] [M31:{8} M32:{9} M33:{10} M34:{11}] [M41:{12} M42:{13} M43:{14} M44:{15}]",
                M11.ToString(formatProvider), M12.ToString(formatProvider), M13.ToString(formatProvider), M14.ToString(formatProvider),
                M21.ToString(formatProvider), M22.ToString(formatProvider), M23.ToString(formatProvider), M24.ToString(formatProvider),
                M31.ToString(formatProvider), M32.ToString(formatProvider), M33.ToString(formatProvider), M34.ToString(formatProvider),
                M41.ToString(formatProvider), M42.ToString(formatProvider), M43.ToString(formatProvider), M44.ToString(formatProvider));
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null)
                return ToString(formatProvider);

            return string.Format(format, formatProvider, "[M11:{0} M12:{1} M13:{2} M14:{3}] [M21:{4} M22:{5} M23:{6} M24:{7}] [M31:{8} M32:{9} M33:{10} M34:{11}] [M41:{12} M42:{13} M43:{14} M44:{15}]",
                M11.ToString(format, formatProvider), M12.ToString(format, formatProvider), M13.ToString(format, formatProvider), M14.ToString(format, formatProvider),
                M21.ToString(format, formatProvider), M22.ToString(format, formatProvider), M23.ToString(format, formatProvider), M24.ToString(format, formatProvider),
                M31.ToString(format, formatProvider), M32.ToString(format, formatProvider), M33.ToString(format, formatProvider), M34.ToString(format, formatProvider),
                M41.ToString(format, formatProvider), M42.ToString(format, formatProvider), M43.ToString(format, formatProvider), M44.ToString(format, formatProvider));
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
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

        /// <summary>
        /// Determines whether the specified <see cref="Matrix4F"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Matrix4F"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="Matrix4F"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(ref Matrix4F other)
        {
            return (MathHelper.NearEqual(other.M11, M11) &&
                MathHelper.NearEqual(other.M12, M12) &&
                MathHelper.NearEqual(other.M13, M13) &&
                MathHelper.NearEqual(other.M14, M14) &&
                MathHelper.NearEqual(other.M21, M21) &&
                MathHelper.NearEqual(other.M22, M22) &&
                MathHelper.NearEqual(other.M23, M23) &&
                MathHelper.NearEqual(other.M24, M24) &&
                MathHelper.NearEqual(other.M31, M31) &&
                MathHelper.NearEqual(other.M32, M32) &&
                MathHelper.NearEqual(other.M33, M33) &&
                MathHelper.NearEqual(other.M34, M34) &&
                MathHelper.NearEqual(other.M41, M41) &&
                MathHelper.NearEqual(other.M42, M42) &&
                MathHelper.NearEqual(other.M43, M43) &&
                MathHelper.NearEqual(other.M44, M44));
        }

        /// <summary>
        /// Determines whether the specified <see cref="Matrix4F"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Matrix4F"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="Matrix4F"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Matrix4F other)
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
            if (!(value is Matrix4F))
                return false;

            var strongValue = (Matrix4F)value;
            return Equals(ref strongValue);
        }
    }
}

