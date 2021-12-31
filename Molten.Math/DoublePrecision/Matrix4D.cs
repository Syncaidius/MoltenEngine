﻿// Copyright (c) 2010-2014 SharpDX - Alexandre Mutel
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

using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Molten.DoublePrecision
{
    /// <summary>
    /// Represents a 4x4 mathematical matrix.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Matrix4D : IEquatable<Matrix4D>, IFormattable
    {
        /// <summary>
        /// The size of the <see cref="Matrix4F"/> type, in bytes.
        /// </summary>
        public static readonly int SizeInBytes = 4 * 4 * sizeof(double);

        /// <summary>
        /// A <see cref="Matrix4F"/> with all of its components set to zero.
        /// </summary>
        public static readonly Matrix4D Zero = new Matrix4D();

        /// <summary>
        /// The identity <see cref="Matrix4F"/>.
        /// </summary>
        public static readonly Matrix4D Identity = new Matrix4D() { M11 = 1.0f, M22 = 1.0f, M33 = 1.0f, M44 = 1.0f };

        /// <summary>
        /// Value at row 1 column 1 of the matrix.
        /// </summary>
        public double M11;

        /// <summary>
        /// Value at row 1 column 2 of the matrix.
        /// </summary>
        public double M12;

        /// <summary>
        /// Value at row 1 column 3 of the matrix.
        /// </summary>
        public double M13;

        /// <summary>
        /// Value at row 1 column 4 of the matrix.
        /// </summary>
        public double M14;

        /// <summary>
        /// Value at row 2 column 1 of the matrix.
        /// </summary>
        public double M21;

        /// <summary>
        /// Value at row 2 column 2 of the matrix.
        /// </summary>
        public double M22;

        /// <summary>
        /// Value at row 2 column 3 of the matrix.
        /// </summary>
        public double M23;

        /// <summary>
        /// Value at row 2 column 4 of the matrix.
        /// </summary>
        public double M24;

        /// <summary>
        /// Value at row 3 column 1 of the matrix.
        /// </summary>
        public double M31;

        /// <summary>
        /// Value at row 3 column 2 of the matrix.
        /// </summary>
        public double M32;

        /// <summary>
        /// Value at row 3 column 3 of the matrix.
        /// </summary>
        public double M33;

        /// <summary>
        /// Value at row 3 column 4 of the matrix.
        /// </summary>
        public double M34;

        /// <summary>
        /// Value at row 4 column 1 of the matrix.
        /// </summary>
        public double M41;

        /// <summary>
        /// Value at row 4 column 2 of the matrix.
        /// </summary>
        public double M42;

        /// <summary>
        /// Value at row 4 column 3 of the matrix.
        /// </summary>
        public double M43;

        /// <summary>
        /// Value at row 4 column 4 of the matrix.
        /// </summary>
        public double M44;
     
        /// <summary>
        /// Gets or sets the up <see cref="Vector3D"/> of the matrix; that is M21, M22, and M23.
        /// </summary>
        public Vector3D Up
        {
          get
          {
            Vector3D vector3;
            vector3.X = this.M21;
            vector3.Y = this.M22;
            vector3.Z = this.M23;
            return vector3;
          }
          set
          {
            this.M21 = value.X;
            this.M22 = value.Y;
            this.M23 = value.Z;
          }
        }
    
        /// <summary>
        /// Gets or sets the down <see cref="Vector3D"/> of the matrix; that is -M21, -M22, and -M23.
        /// </summary>
        public Vector3D Down
        {
          get
          {
            Vector3D vector3;
            vector3.X = -this.M21;
            vector3.Y = -this.M22;
            vector3.Z = -this.M23;
            return vector3;
          }
          set
          {
            this.M21 = -value.X;
            this.M22 = -value.Y;
            this.M23 = -value.Z;
          }
        }
    
        /// <summary>
        /// Gets or sets the right <see cref="Vector3D"/> of the matrix; that is M11, M12, and M13.
        /// </summary>
        public Vector3D Right
        {
          get
          {
            Vector3D vector3;
            vector3.X = this.M11;
            vector3.Y = this.M12;
            vector3.Z = this.M13;
            return vector3;
          }
          set
          {
            this.M11 = value.X;
            this.M12 = value.Y;
            this.M13 = value.Z;
          }
        }
    
        /// <summary>
        /// Gets or sets the left <see cref="Vector3D"/> of the matrix; that is -M11, -M12, and -M13.
        /// </summary>
        public Vector3D Left
        {
          get
          {
            Vector3D vector3;
            vector3.X = -this.M11;
            vector3.Y = -this.M12;
            vector3.Z = -this.M13;
            return vector3;
          }
          set
          {
            this.M11 = -value.X;
            this.M12 = -value.Y;
            this.M13 = -value.Z;
          }
        }
        
        /// <summary>
        /// Gets or sets the forward <see cref="Vector3D"/> of the matrix; that is -M31, -M32, and -M33.
        /// </summary>
        public Vector3D Forward
        {
          get
          {
            Vector3D vector3;
            vector3.X = -this.M31;
            vector3.Y = -this.M32;
            vector3.Z = -this.M33;
            return vector3;
          }
          set
          {
            this.M31 = -value.X;
            this.M32 = -value.Y;
            this.M33 = -value.Z;
          }
        }
        
        /// <summary>
        /// Gets or sets the backward <see cref="Vector3D"/> of the matrix; that is M31, M32, and M33.
        /// </summary>
        public Vector3D Backward
        {
          get
          {
            Vector3D vector3;
            vector3.X = this.M31;
            vector3.Y = this.M32;
            vector3.Z = this.M33;
            return vector3;
          }
          set
          {
            this.M31 = value.X;
            this.M32 = value.Y;
            this.M33 = value.Z;
          }
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Matrix4F"/> struct.
        /// </summary>
        /// <param name="value">The value that will be assigned to all components.</param>
        public Matrix4D(double value)
        {
            M11 = M12 = M13 = M14 =
            M21 = M22 = M23 = M24 =
            M31 = M32 = M33 = M34 =
            M41 = M42 = M43 = M44 = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Matrix4F"/> struct.
        /// </summary>
        /// <param name="M11">The value to assign at row 1 column 1 of the matrix.</param>
        /// <param name="M12">The value to assign at row 1 column 2 of the matrix.</param>
        /// <param name="M13">The value to assign at row 1 column 3 of the matrix.</param>
        /// <param name="M14">The value to assign at row 1 column 4 of the matrix.</param>
        /// <param name="M21">The value to assign at row 2 column 1 of the matrix.</param>
        /// <param name="M22">The value to assign at row 2 column 2 of the matrix.</param>
        /// <param name="M23">The value to assign at row 2 column 3 of the matrix.</param>
        /// <param name="M24">The value to assign at row 2 column 4 of the matrix.</param>
        /// <param name="M31">The value to assign at row 3 column 1 of the matrix.</param>
        /// <param name="M32">The value to assign at row 3 column 2 of the matrix.</param>
        /// <param name="M33">The value to assign at row 3 column 3 of the matrix.</param>
        /// <param name="M34">The value to assign at row 3 column 4 of the matrix.</param>
        /// <param name="M41">The value to assign at row 4 column 1 of the matrix.</param>
        /// <param name="M42">The value to assign at row 4 column 2 of the matrix.</param>
        /// <param name="M43">The value to assign at row 4 column 3 of the matrix.</param>
        /// <param name="M44">The value to assign at row 4 column 4 of the matrix.</param>
        public Matrix4D(double M11, double M12, double M13, double M14,
            double M21, double M22, double M23, double M24,
            double M31, double M32, double M33, double M34,
            double M41, double M42, double M43, double M44)
        {
            this.M11 = M11; this.M12 = M12; this.M13 = M13; this.M14 = M14;
            this.M21 = M21; this.M22 = M22; this.M23 = M23; this.M24 = M24;
            this.M31 = M31; this.M32 = M32; this.M33 = M33; this.M34 = M34;
            this.M41 = M41; this.M42 = M42; this.M43 = M43; this.M44 = M44;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Matrix4F"/> struct.
        /// </summary>
        /// <param name="values">The values to assign to the components of the matrix. This must be an array with sixteen elements.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than sixteen elements.</exception>
        public Matrix4D(float[] values)
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
        public Vector4D Row1
        {
            get { return new Vector4D(M11, M12, M13, M14); }
            set { M11 = value.X; M12 = value.Y; M13 = value.Z; M14 = value.W; }
        }

        /// <summary>
        /// Gets or sets the second row in the matrix; that is M21, M22, M23, and M24.
        /// </summary>
        public Vector4D Row2
        {
            get { return new Vector4D(M21, M22, M23, M24); }
            set { M21 = value.X; M22 = value.Y; M23 = value.Z; M24 = value.W; }
        }

        /// <summary>
        /// Gets or sets the third row in the matrix; that is M31, M32, M33, and M34.
        /// </summary>
        public Vector4D Row3
        {
            get { return new Vector4D(M31, M32, M33, M34); }
            set { M31 = value.X; M32 = value.Y; M33 = value.Z; M34 = value.W; }
        }

        /// <summary>
        /// Gets or sets the fourth row in the matrix; that is M41, M42, M43, and M44.
        /// </summary>
        public Vector4D Row4
        {
            get { return new Vector4D(M41, M42, M43, M44); }
            set { M41 = value.X; M42 = value.Y; M43 = value.Z; M44 = value.W; }
        }

        /// <summary>
        /// Gets or sets the first column in the matrix; that is M11, M21, M31, and M41.
        /// </summary>
        public Vector4D Column1
        {
            get { return new Vector4D(M11, M21, M31, M41); }
            set { M11 = value.X; M21 = value.Y; M31 = value.Z; M41 = value.W; }
        }

        /// <summary>
        /// Gets or sets the second column in the matrix; that is M12, M22, M32, and M42.
        /// </summary>
        public Vector4D Column2
        {
            get { return new Vector4D(M12, M22, M32, M42); }
            set { M12 = value.X; M22 = value.Y; M32 = value.Z; M42 = value.W; }
        }

        /// <summary>
        /// Gets or sets the third column in the matrix; that is M13, M23, M33, and M43.
        /// </summary>
        public Vector4D Column3
        {
            get { return new Vector4D(M13, M23, M33, M43); }
            set { M13 = value.X; M23 = value.Y; M33 = value.Z; M43 = value.W; }
        }

        /// <summary>
        /// Gets or sets the fourth column in the matrix; that is M14, M24, M34, and M44.
        /// </summary>
        public Vector4D Column4
        {
            get { return new Vector4D(M14, M24, M34, M44); }
            set { M14 = value.X; M24 = value.Y; M34 = value.Z; M44 = value.W; }
        }

        /// <summary>
        /// Gets or sets the translation of the matrix; that is M41, M42, and M43.
        /// </summary>
        public Vector3D Translation
        {
            get { return new Vector3D(M41, M42, M43); }
            set { M41 = value.X; M42 = value.Y; M43 = value.Z; }
        }

        /// <summary>
        /// Gets or sets the scale of the matrix; that is M11, M22, and M33.
        /// </summary>
        public Vector3D ScaleVector
        {
            get { return new Vector3D(M11, M22, M33); }
            set { M11 = value.X; M22 = value.Y; M33 = value.Z; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is an identity matrix.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is an identity matrix; otherwise, <c>false</c>.
        /// </value>
        public bool IsIdentity
        {
            get { return this.Equals(Identity); }
        }

        /// <summary>
        /// Gets or sets the component at the specified index.
        /// </summary>
        /// <value>The value of the MatrixDouble component, depending on the index.</value>
        /// <param name="index">The zero-based index of the component to access.</param>
        /// <returns>The value of the component at the specified index.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> is out of the range [0, 15].</exception>
        public double this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:  return M11;
                    case 1:  return M12;
                    case 2:  return M13;
                    case 3:  return M14;
                    case 4:  return M21;
                    case 5:  return M22;
                    case 6:  return M23;
                    case 7:  return M24;
                    case 8:  return M31;
                    case 9:  return M32;
                    case 10: return M33;
                    case 11: return M34;
                    case 12: return M41;
                    case 13: return M42;
                    case 14: return M43;
                    case 15: return M44;
                }

                throw new ArgumentOutOfRangeException("index", "Indices for MatrixDouble run from 0 to 15, inclusive.");
            }

            set
            {
                switch (index)
                {
                    case 0: M11 = value; break;
                    case 1: M12 = value; break;
                    case 2: M13 = value; break;
                    case 3: M14 = value; break;
                    case 4: M21 = value; break;
                    case 5: M22 = value; break;
                    case 6: M23 = value; break;
                    case 7: M24 = value; break;
                    case 8: M31 = value; break;
                    case 9: M32 = value; break;
                    case 10: M33 = value; break;
                    case 11: M34 = value; break;
                    case 12: M41 = value; break;
                    case 13: M42 = value; break;
                    case 14: M43 = value; break;
                    case 15: M44 = value; break;
                    default: throw new ArgumentOutOfRangeException("index", "Indices for MatrixDouble run from 0 to 15, inclusive.");
                }
            }
        }

        /// <summary>
        /// Gets or sets the component at the specified index.
        /// </summary>
        /// <value>The value of the MatrixDouble component, depending on the index.</value>
        /// <param name="row">The row of the MatrixDouble to access.</param>
        /// <param name="column">The column of the MatrixDouble to access.</param>
        /// <returns>The value of the component at the specified index.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="row"/> or <paramref name="column"/>is out of the range [0, 3].</exception>
        public double this[int row, int column]
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
        public double Determinant()
        {
            double temp1 = (M33 * M44) - (M34 * M43);
            double temp2 = (M32 * M44) - (M34 * M42);
            double temp3 = (M32 * M43) - (M33 * M42);
            double temp4 = (M31 * M44) - (M34 * M41);
            double temp5 = (M31 * M43) - (M33 * M41);
            double temp6 = (M31 * M42) - (M32 * M41);

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
        /// Transposes the matrix.
        /// </summary>
        public void Transpose()
        {
            Transpose(ref this, out this);
        }

        /// <summary>
        /// Orthogonalizes the specified matrix.
        /// </summary>
        /// <remarks>
        /// <para>Orthogonalization is the process of making all rows orthogonal to each other. This
        /// means that any given row in the MatrixDouble will be orthogonal to any other given row in the
        /// matrix.</para>
        /// <para>Because this method uses the modified Gram-Schmidt process, the resulting matrix
        /// tends to be numerically unstable. The numeric stability decreases according to the rows
        /// so that the first row is the most stable and the last row is the least stable.</para>
        /// <para>This operation is performed on the rows of the MatrixDouble rather than the columns.
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
        /// <para>This operation is performed on the rows of the MatrixDouble rather than the columns.
        /// If you wish for this operation to be performed on the columns, first transpose the
        /// input and than transpose the output.</para>
        /// </remarks>
        public void Orthonormalize()
        {
            Orthonormalize(ref this, out this);
        }

        /// <summary>
        /// Decomposes a MatrixDouble into an orthonormalized MatrixDouble Q and a right triangular MatrixDouble R.
        /// </summary>
        /// <param name="Q">When the method completes, contains the orthonormalized MatrixDouble of the decomposition.</param>
        /// <param name="R">When the method completes, contains the right triangular MatrixDouble of the decomposition.</param>
        public void DecomposeQR(out Matrix4D Q, out Matrix4D R)
        {
            Matrix4D temp = this;
            temp.Transpose();
            Orthonormalize(ref temp, out Q);
            Q.Transpose();

            R = new Matrix4D();
            R.M11 = Vector4D.Dot(Q.Column1, Column1);
            R.M12 = Vector4D.Dot(Q.Column1, Column2);
            R.M13 = Vector4D.Dot(Q.Column1, Column3);
            R.M14 = Vector4D.Dot(Q.Column1, Column4);

            R.M22 = Vector4D.Dot(Q.Column2, Column2);
            R.M23 = Vector4D.Dot(Q.Column2, Column3);
            R.M24 = Vector4D.Dot(Q.Column2, Column4);

            R.M33 = Vector4D.Dot(Q.Column3, Column3);
            R.M34 = Vector4D.Dot(Q.Column3, Column4);

            R.M44 = Vector4D.Dot(Q.Column4, Column4);
        }

        /// <summary>
        /// Decomposes a MatrixDouble into a lower triangular MatrixDouble L and an orthonormalized MatrixDouble Q.
        /// </summary>
        /// <param name="L">When the method completes, contains the lower triangular MatrixDouble of the decomposition.</param>
        /// <param name="Q">When the method completes, contains the orthonormalized MatrixDouble of the decomposition.</param>
        public void DecomposeLQ(out Matrix4D L, out Matrix4D Q)
        {
            Orthonormalize(ref this, out Q);

            L = new Matrix4D();
            L.M11 = Vector4D.Dot(Q.Row1, Row1);
            
            L.M21 = Vector4D.Dot(Q.Row1, Row2);
            L.M22 = Vector4D.Dot(Q.Row2, Row2);
            
            L.M31 = Vector4D.Dot(Q.Row1, Row3);
            L.M32 = Vector4D.Dot(Q.Row2, Row3);
            L.M33 = Vector4D.Dot(Q.Row3, Row3);
            
            L.M41 = Vector4D.Dot(Q.Row1, Row4);
            L.M42 = Vector4D.Dot(Q.Row2, Row4);
            L.M43 = Vector4D.Dot(Q.Row3, Row4);
            L.M44 = Vector4D.Dot(Q.Row4, Row4);
        }

        /// <summary>
        /// Decomposes a MatrixDouble into a scale, rotation, and translation.
        /// </summary>
        /// <param name="scale">When the method completes, contains the scaling component of the decomposed matrix.</param>
        /// <param name="rotation">When the method completes, contains the rotation component of the decomposed matrix.</param>
        /// <param name="translation">When the method completes, contains the translation component of the decomposed matrix.</param>
        /// <remarks>
        /// This method is designed to decompose an SRT transformation MatrixDouble only.
        /// </remarks>
        public bool Decompose(out Vector3D scale, out QuaternionD rotation, out Vector3D translation)
        {
            //Source: Unknown
            //References: http://www.gamedev.net/community/forums/topic.asp?topic_id=441695

            //Get the translation.
            translation.X = this.M41;
            translation.Y = this.M42;
            translation.Z = this.M43;

            //Scaling is the length of the rows.
            scale.X = (float)Math.Sqrt((M11 * M11) + (M12 * M12) + (M13 * M13));
            scale.Y = (float)Math.Sqrt((M21 * M21) + (M22 * M22) + (M23 * M23));
            scale.Z = (float)Math.Sqrt((M31 * M31) + (M32 * M32) + (M33 * M33));

            //If any of the scaling factors are zero, than the rotation MatrixDouble can not exist.
            if (MathHelperDP.IsZero(scale.X) ||
                MathHelperDP.IsZero(scale.Y) ||
                MathHelperDP.IsZero(scale.Z))
            {
                rotation = QuaternionD.Identity;
                return false;
            }

            //The rotation is the left over MatrixDouble after dividing out the scaling.
            Matrix4D rotationmatrix = new Matrix4D();
            rotationmatrix.M11 = M11 / scale.X;
            rotationmatrix.M12 = M12 / scale.X;
            rotationmatrix.M13 = M13 / scale.X;

            rotationmatrix.M21 = M21 / scale.Y;
            rotationmatrix.M22 = M22 / scale.Y;
            rotationmatrix.M23 = M23 / scale.Y;

            rotationmatrix.M31 = M31 / scale.Z;
            rotationmatrix.M32 = M32 / scale.Z;
            rotationmatrix.M33 = M33 / scale.Z;

            rotationmatrix.M44 = 1f;

            QuaternionD.FromRotationMatrix(ref rotationmatrix, out rotation);
            return true;
        }

        /// <summary>
        /// Decomposes a uniform scale MatrixDouble into a scale, rotation, and translation.
        /// A uniform scale MatrixDouble has the same scale in every axis.
        /// </summary>
        /// <param name="scale">When the method completes, contains the scaling component of the decomposed matrix.</param>
        /// <param name="rotation">When the method completes, contains the rotation component of the decomposed matrix.</param>
        /// <param name="translation">When the method completes, contains the translation component of the decomposed matrix.</param>
        /// <remarks>
        /// This method is designed to decompose only an SRT transformation MatrixDouble that has the same scale in every axis.
        /// </remarks>
        public bool DecomposeUniformScale(out double scale, out QuaternionD rotation, out Vector3D translation)
        {
            //Get the translation.
            translation.X = this.M41;
            translation.Y = this.M42;
            translation.Z = this.M43;

            //Scaling is the length of the rows. ( just take one row since this is a uniform matrix)
            scale = (float)Math.Sqrt((M11 * M11) + (M12 * M12) + (M13 * M13));
            var inv_scale = 1f / scale;

            //If any of the scaling factors are zero, then the rotation MatrixDouble can not exist.
            if (Math.Abs(scale) < MathHelperDP.ZeroTolerance)
            {
                rotation = QuaternionD.Identity;
                return false;
            }

            //The rotation is the left over MatrixDouble after dividing out the scaling.
            Matrix4D rotationmatrix = new Matrix4D();
            rotationmatrix.M11 = M11 * inv_scale;
            rotationmatrix.M12 = M12 * inv_scale;
            rotationmatrix.M13 = M13 * inv_scale;

            rotationmatrix.M21 = M21 * inv_scale;
            rotationmatrix.M22 = M22 * inv_scale;
            rotationmatrix.M23 = M23 * inv_scale;

            rotationmatrix.M31 = M31 * inv_scale;
            rotationmatrix.M32 = M32 * inv_scale;
            rotationmatrix.M33 = M33 * inv_scale;

            rotationmatrix.M44 = 1f;

            QuaternionD.FromRotationMatrix(ref rotationmatrix, out rotation);
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

            double temp0 = this[secondRow, 0];
            double temp1 = this[secondRow, 1];
            double temp2 = this[secondRow, 2];
            double temp3 = this[secondRow, 3];

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

            double temp0 = this[0, secondColumn];
            double temp1 = this[1, secondColumn];
            double temp2 = this[2, secondColumn];
            double temp3 = this[3, secondColumn];

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
        public double[] ToArray()
        {
            return new[] { M11, M12, M13, M14, M21, M22, M23, M24, M31, M32, M33, M34, M41, M42, M43, M44 };
        }

        /// <summary>
        /// Determines the sum of two matrices.
        /// </summary>
        /// <param name="left">The first MatrixDouble to add.</param>
        /// <param name="right">The second MatrixDouble to add.</param>
        /// <param name="result">When the method completes, contains the sum of the two matrices.</param>
        public static void Add(ref Matrix4D left, ref Matrix4D right, out Matrix4D result)
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
        /// <param name="left">The first MatrixDouble to add.</param>
        /// <param name="right">The second MatrixDouble to add.</param>
        /// <returns>The sum of the two matrices.</returns>
        public static Matrix4D Add(Matrix4D left, Matrix4D right)
        {
            Matrix4D result;
            Add(ref left, ref right, out result);
            return result;
        }

        /// <summary>
        /// Determines the difference between two matrices.
        /// </summary>
        /// <param name="left">The first MatrixDouble to subtract.</param>
        /// <param name="right">The second MatrixDouble to subtract.</param>
        /// <param name="result">When the method completes, contains the difference between the two matrices.</param>
        public static void Subtract(ref Matrix4D left, ref Matrix4D right, out Matrix4D result)
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
        /// <param name="left">The first MatrixDouble to subtract.</param>
        /// <param name="right">The second MatrixDouble to subtract.</param>
        /// <returns>The difference between the two matrices.</returns>
        public static Matrix4D Subtract(Matrix4D left, Matrix4D right)
        {
            Matrix4D result;
            Subtract(ref left, ref right, out result);
            return result;
        }

        /// <summary>
        /// Scales a MatrixDouble by the given value.
        /// </summary>
        /// <param name="left">The MatrixDouble to scale.</param>
        /// <param name="right">The amount by which to scale.</param>
        /// <param name="result">When the method completes, contains the scaled matrix.</param>
        public static void Multiply(ref Matrix4D left, double right, out Matrix4D result)
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
        /// Scales a MatrixDouble by the given value.
        /// </summary>
        /// <param name="left">The MatrixDouble to scale.</param>
        /// <param name="right">The amount by which to scale.</param>
        /// <returns>The scaled matrix.</returns>
        public static Matrix4D Multiply(Matrix4D left, double right)
        {
            Matrix4D result;
            Multiply(ref left, right, out result);
            return result;
        }

        /// <summary>
        /// Determines the product of two matrices.
        /// </summary>
        /// <param name="left">The first MatrixDouble to multiply.</param>
        /// <param name="right">The second MatrixDouble to multiply.</param>
        /// <param name="result">The product of the two matrices.</param>
        public static void Multiply(ref Matrix4D left, ref Matrix4D right, out Matrix4D result)
        {
            Matrix4D temp = new Matrix4D();
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
        /// <param name="left">The first MatrixDouble to multiply.</param>
        /// <param name="right">The second MatrixDouble to multiply.</param>
        /// <returns>The product of the two matrices.</returns>
        public static Matrix4D Multiply(Matrix4D left, Matrix4D right)
        {
            Matrix4D result;
            Multiply(ref left, ref right, out result);
            return result;
        }

        /// <summary>
        /// Scales a MatrixDouble by the given value.
        /// </summary>
        /// <param name="left">The MatrixDouble to scale.</param>
        /// <param name="right">The amount by which to scale.</param>
        /// <param name="result">When the method completes, contains the scaled matrix.</param>
        public static void Divide(ref Matrix4D left, double right, out Matrix4D result)
        {
            double inv = 1.0f / right;

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
        /// Scales a MatrixDouble by the given value.
        /// </summary>
        /// <param name="left">The MatrixDouble to scale.</param>
        /// <param name="right">The amount by which to scale.</param>
        /// <returns>The scaled matrix.</returns>
        public static Matrix4D Divide(Matrix4D left, double right)
        {
            Matrix4D result;
            Divide(ref left, right, out result);
            return result;
        }

        /// <summary>
        /// Determines the quotient of two matrices.
        /// </summary>
        /// <param name="left">The first MatrixDouble to divide.</param>
        /// <param name="right">The second MatrixDouble to divide.</param>
        /// <param name="result">When the method completes, contains the quotient of the two matrices.</param>
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
        /// Determines the quotient of two matrices.
        /// </summary>
        /// <param name="left">The first MatrixDouble to divide.</param>
        /// <param name="right">The second MatrixDouble to divide.</param>
        /// <returns>The quotient of the two matrices.</returns>
        public static Matrix4D Divide(Matrix4D left, Matrix4D right)
        {
            Matrix4D result;
            Divide(ref left, ref right, out result);
            return result;
        }

        /// <summary>
        /// Performs the exponential operation on a matrix.
        /// </summary>
        /// <param name="value">The MatrixDouble to perform the operation on.</param>
        /// <param name="exponent">The exponent to raise the MatrixDouble to.</param>
        /// <param name="result">When the method completes, contains the exponential matrix.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="exponent"/> is negative.</exception>
        public static void Exponent(ref Matrix4D value, int exponent, out Matrix4D result)
        {
            //Source: http://rosettacode.org
            //Reference: http://rosettacode.org/wiki/Matrix-exponentiation_operator

            if (exponent < 0)
                throw new ArgumentOutOfRangeException("exponent", "The exponent can not be negative.");

            if (exponent == 0)
            {
                result = Matrix4D.Identity;
                return;
            }

            if (exponent == 1)
            {
                result = value;
                return;
            }

            Matrix4D identity = Matrix4D.Identity;
            Matrix4D temp = value;

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
        /// <param name="value">The MatrixDouble to perform the operation on.</param>
        /// <param name="exponent">The exponent to raise the MatrixDouble to.</param>
        /// <returns>The exponential matrix.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="exponent"/> is negative.</exception>
        public static Matrix4D Exponent(Matrix4D value, int exponent)
        {
            Matrix4D result;
            Exponent(ref value, exponent, out result);
            return result;
        }

        /// <summary>
        /// Negates a matrix.
        /// </summary>
        /// <param name="value">The MatrixDouble to be negated.</param>
        /// <param name="result">When the method completes, contains the negated matrix.</param>
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
        /// Negates a matrix.
        /// </summary>
        /// <param name="value">The MatrixDouble to be negated.</param>
        /// <returns>The negated matrix.</returns>
        public static Matrix4D Negate(Matrix4D value)
        {
            Matrix4D result;
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
        public static void Lerp(ref Matrix4D start, ref Matrix4D end, double amount, out Matrix4D result)
        {
            result.M11 = MathHelperDP.Lerp(start.M11, end.M11, amount);
            result.M12 = MathHelperDP.Lerp(start.M12, end.M12, amount);
            result.M13 = MathHelperDP.Lerp(start.M13, end.M13, amount);
            result.M14 = MathHelperDP.Lerp(start.M14, end.M14, amount);
            result.M21 = MathHelperDP.Lerp(start.M21, end.M21, amount);
            result.M22 = MathHelperDP.Lerp(start.M22, end.M22, amount);
            result.M23 = MathHelperDP.Lerp(start.M23, end.M23, amount);
            result.M24 = MathHelperDP.Lerp(start.M24, end.M24, amount);
            result.M31 = MathHelperDP.Lerp(start.M31, end.M31, amount);
            result.M32 = MathHelperDP.Lerp(start.M32, end.M32, amount);
            result.M33 = MathHelperDP.Lerp(start.M33, end.M33, amount);
            result.M34 = MathHelperDP.Lerp(start.M34, end.M34, amount);
            result.M41 = MathHelperDP.Lerp(start.M41, end.M41, amount);
            result.M42 = MathHelperDP.Lerp(start.M42, end.M42, amount);
            result.M43 = MathHelperDP.Lerp(start.M43, end.M43, amount);
            result.M44 = MathHelperDP.Lerp(start.M44, end.M44, amount);
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
            Matrix4D result;
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
        public static void SmoothStep(ref Matrix4D start, ref Matrix4D end, double amount, out Matrix4D result)
        {
            amount = MathHelperDP.SmoothStep(amount);
            Lerp(ref start, ref end, amount, out result);
        }

        /// <summary>
        /// Performs a cubic interpolation between two matrices.
        /// </summary>
        /// <param name="start">Start matrix.</param>
        /// <param name="end">End matrix.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <returns>The cubic interpolation of the two matrices.</returns>
        public static Matrix4D SmoothStep(Matrix4D start, Matrix4D end, double amount)
        {
            Matrix4D result;
            SmoothStep(ref start, ref end, amount, out result);
            return result;
        }

        /// <summary>
        /// Calculates the transpose of the specified matrix.
        /// </summary>
        /// <param name="value">The MatrixDouble whose transpose is to be calculated.</param>
        /// <param name="result">When the method completes, contains the transpose of the specified matrix.</param>
        public static void Transpose(ref Matrix4D value, out Matrix4D result)
        {
            Matrix4D temp = new Matrix4D();
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
        /// <param name="value">The MatrixDouble whose transpose is to be calculated.</param>
        /// <param name="result">When the method completes, contains the transpose of the specified matrix.</param>
        public static void TransposeByRef(ref Matrix4D value, ref Matrix4D result)
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
        /// <param name="value">The MatrixDouble whose transpose is to be calculated.</param>
        /// <returns>The transpose of the specified matrix.</returns>
        public static Matrix4D Transpose(Matrix4D value)
        {
            Matrix4D result;
            Transpose(ref value, out result);
            return result;
        }

        /// <summary>
        /// Calculates the inverse of the specified matrix.
        /// </summary>
        /// <param name="value">The MatrixDouble whose inverse is to be calculated.</param>
        /// <param name="result">When the method completes, contains the inverse of the specified matrix.</param>
        public static void Invert(ref Matrix4D value, out Matrix4D result)
        {
            double b0 = (value.M31 * value.M42) - (value.M32 * value.M41);
            double b1 = (value.M31 * value.M43) - (value.M33 * value.M41);
            double b2 = (value.M34 * value.M41) - (value.M31 * value.M44);
            double b3 = (value.M32 * value.M43) - (value.M33 * value.M42);
            double b4 = (value.M34 * value.M42) - (value.M32 * value.M44);
            double b5 = (value.M33 * value.M44) - (value.M34 * value.M43);

            double d11 = value.M22 * b5 + value.M23 * b4 + value.M24 * b3;
            double d12 = value.M21 * b5 + value.M23 * b2 + value.M24 * b1;
            double d13 = value.M21 * -b4 + value.M22 * b2 + value.M24 * b0;
            double d14 = value.M21 * b3 + value.M22 * -b1 + value.M23 * b0;

            double det = value.M11 * d11 - value.M12 * d12 + value.M13 * d13 - value.M14 * d14;
            if (Math.Abs(det) == 0.0f)
            {
                result = Matrix4D.Zero;
                return;
            }

            det = 1f / det;

            double a0 = (value.M11 * value.M22) - (value.M12 * value.M21);
            double a1 = (value.M11 * value.M23) - (value.M13 * value.M21);
            double a2 = (value.M14 * value.M21) - (value.M11 * value.M24);
            double a3 = (value.M12 * value.M23) - (value.M13 * value.M22);
            double a4 = (value.M14 * value.M22) - (value.M12 * value.M24);
            double a5 = (value.M13 * value.M24) - (value.M14 * value.M23);

            double d21 = value.M12 * b5 + value.M13 * b4 + value.M14 * b3;
            double d22 = value.M11 * b5 + value.M13 * b2 + value.M14 * b1;
            double d23 = value.M11 * -b4 + value.M12 * b2 + value.M14 * b0;
            double d24 = value.M11 * b3 + value.M12 * -b1 + value.M13 * b0;

            double d31 = value.M42 * a5 + value.M43 * a4 + value.M44 * a3;
            double d32 = value.M41 * a5 + value.M43 * a2 + value.M44 * a1;
            double d33 = value.M41 * -a4 + value.M42 * a2 + value.M44 * a0;
            double d34 = value.M41 * a3 + value.M42 * -a1 + value.M43 * a0;

            double d41 = value.M32 * a5 + value.M33 * a4 + value.M34 * a3;
            double d42 = value.M31 * a5 + value.M33 * a2 + value.M34 * a1;
            double d43 = value.M31 * -a4 + value.M32 * a2 + value.M34 * a0;
            double d44 = value.M31 * a3 + value.M32 * -a1 + value.M33 * a0;

            result.M11 = +d11 * det; result.M12 = -d21 * det; result.M13 = +d31 * det; result.M14 = -d41 * det;
            result.M21 = -d12 * det; result.M22 = +d22 * det; result.M23 = -d32 * det; result.M24 = +d42 * det;
            result.M31 = +d13 * det; result.M32 = -d23 * det; result.M33 = +d33 * det; result.M34 = -d43 * det;
            result.M41 = -d14 * det; result.M42 = +d24 * det; result.M43 = -d34 * det; result.M44 = +d44 * det;
        }

        /// <summary>
        /// Calculates the inverse of the specified matrix.
        /// </summary>
        /// <param name="value">The MatrixDouble whose inverse is to be calculated.</param>
        /// <returns>The inverse of the specified matrix.</returns>
        public static Matrix4D Invert(Matrix4D value)
        {
            value.Invert();
            return value;
        }

        /// <summary>
        /// Orthogonalizes the specified matrix.
        /// </summary>
        /// <param name="value">The MatrixDouble to orthogonalize.</param>
        /// <param name="result">When the method completes, contains the orthogonalized matrix.</param>
        /// <remarks>
        /// <para>Orthogonalization is the process of making all rows orthogonal to each other. This
        /// means that any given row in the MatrixDouble will be orthogonal to any other given row in the
        /// matrix.</para>
        /// <para>Because this method uses the modified Gram-Schmidt process, the resulting matrix
        /// tends to be numerically unstable. The numeric stability decreases according to the rows
        /// so that the first row is the most stable and the last row is the least stable.</para>
        /// <para>This operation is performed on the rows of the MatrixDouble rather than the columns.
        /// If you wish for this operation to be performed on the columns, first transpose the
        /// input and than transpose the output.</para>
        /// </remarks>
        public static void Orthogonalize(ref Matrix4D value, out Matrix4D result)
        {
            //Uses the modified Gram-Schmidt process.
            //q1 = m1
            //q2 = m2 - ((q1 ⋅ m2) / (q1 ⋅ q1)) * q1
            //q3 = m3 - ((q1 ⋅ m3) / (q1 ⋅ q1)) * q1 - ((q2 ⋅ m3) / (q2 ⋅ q2)) * q2
            //q4 = m4 - ((q1 ⋅ m4) / (q1 ⋅ q1)) * q1 - ((q2 ⋅ m4) / (q2 ⋅ q2)) * q2 - ((q3 ⋅ m4) / (q3 ⋅ q3)) * q3

            //By separating the above algorithm into multiple lines, we actually increase accuracy.
            result = value;

            result.Row2 = result.Row2 - (Vector4D.Dot(result.Row1, result.Row2) / Vector4D.Dot(result.Row1, result.Row1)) * result.Row1;

            result.Row3 = result.Row3 - (Vector4D.Dot(result.Row1, result.Row3) / Vector4D.Dot(result.Row1, result.Row1)) * result.Row1;
            result.Row3 = result.Row3 - (Vector4D.Dot(result.Row2, result.Row3) / Vector4D.Dot(result.Row2, result.Row2)) * result.Row2;

            result.Row4 = result.Row4 - (Vector4D.Dot(result.Row1, result.Row4) / Vector4D.Dot(result.Row1, result.Row1)) * result.Row1;
            result.Row4 = result.Row4 - (Vector4D.Dot(result.Row2, result.Row4) / Vector4D.Dot(result.Row2, result.Row2)) * result.Row2;
            result.Row4 = result.Row4 - (Vector4D.Dot(result.Row3, result.Row4) / Vector4D.Dot(result.Row3, result.Row3)) * result.Row3;
        }

        /// <summary>
        /// Orthogonalizes the specified matrix.
        /// </summary>
        /// <param name="value">The MatrixDouble to orthogonalize.</param>
        /// <returns>The orthogonalized matrix.</returns>
        /// <remarks>
        /// <para>Orthogonalization is the process of making all rows orthogonal to each other. This
        /// means that any given row in the MatrixDouble will be orthogonal to any other given row in the
        /// matrix.</para>
        /// <para>Because this method uses the modified Gram-Schmidt process, the resulting matrix
        /// tends to be numerically unstable. The numeric stability decreases according to the rows
        /// so that the first row is the most stable and the last row is the least stable.</para>
        /// <para>This operation is performed on the rows of the MatrixDouble rather than the columns.
        /// If you wish for this operation to be performed on the columns, first transpose the
        /// input and than transpose the output.</para>
        /// </remarks>
        public static Matrix4D Orthogonalize(Matrix4D value)
        {
            Matrix4D result;
            Orthogonalize(ref value, out result);
            return result;
        }

        /// <summary>
        /// Orthonormalizes the specified matrix.
        /// </summary>
        /// <param name="value">The MatrixDouble to orthonormalize.</param>
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
        /// <para>This operation is performed on the rows of the MatrixDouble rather than the columns.
        /// If you wish for this operation to be performed on the columns, first transpose the
        /// input and than transpose the output.</para>
        /// </remarks>
        public static void Orthonormalize(ref Matrix4D value, out Matrix4D result)
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

            result.Row1 = Vector4D.Normalize(result.Row1);

            result.Row2 = result.Row2 - Vector4D.Dot(result.Row1, result.Row2) * result.Row1;
            result.Row2 = Vector4D.Normalize(result.Row2);

            result.Row3 = result.Row3 - Vector4D.Dot(result.Row1, result.Row3) * result.Row1;
            result.Row3 = result.Row3 - Vector4D.Dot(result.Row2, result.Row3) * result.Row2;
            result.Row3 = Vector4D.Normalize(result.Row3);

            result.Row4 = result.Row4 - Vector4D.Dot(result.Row1, result.Row4) * result.Row1;
            result.Row4 = result.Row4 - Vector4D.Dot(result.Row2, result.Row4) * result.Row2;
            result.Row4 = result.Row4 - Vector4D.Dot(result.Row3, result.Row4) * result.Row3;
            result.Row4 = Vector4D.Normalize(result.Row4);
        }

        /// <summary>
        /// Orthonormalizes the specified matrix.
        /// </summary>
        /// <param name="value">The MatrixDouble to orthonormalize.</param>
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
        /// <para>This operation is performed on the rows of the MatrixDouble rather than the columns.
        /// If you wish for this operation to be performed on the columns, first transpose the
        /// input and than transpose the output.</para>
        /// </remarks>
        public static Matrix4D Orthonormalize(Matrix4D value)
        {
            Matrix4D result;
            Orthonormalize(ref value, out result);
            return result;
        }

        /// <summary>
        /// Brings the MatrixDouble into upper triangular form using elementary row operations.
        /// </summary>
        /// <param name="value">The MatrixDouble to put into upper triangular form.</param>
        /// <param name="result">When the method completes, contains the upper triangular matrix.</param>
        /// <remarks>
        /// If the MatrixDouble is not invertible (i.e. its determinant is zero) than the result of this
        /// method may produce Single.Nan and Single.Inf values. When the MatrixDouble represents a system
        /// of linear equations, than this often means that either no solution exists or an infinite
        /// number of solutions exist.
        /// </remarks>
        public static void UpperTriangularForm(ref Matrix4D value, out Matrix4D result)
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

                while (MathHelperDP.IsZero(result[i, lead]))
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
                        result[i, 3] -= result[r, 3] * multiplier * result[i, lead];
                    }
                }

                lead++;
            }
        }

        /// <summary>
        /// Brings the MatrixDouble into upper triangular form using elementary row operations.
        /// </summary>
        /// <param name="value">The MatrixDouble to put into upper triangular form.</param>
        /// <returns>The upper triangular matrix.</returns>
        /// <remarks>
        /// If the MatrixDouble is not invertible (i.e. its determinant is zero) than the result of this
        /// method may produce Single.Nan and Single.Inf values. When the MatrixDouble represents a system
        /// of linear equations, than this often means that either no solution exists or an infinite
        /// number of solutions exist.
        /// </remarks>
        public static Matrix4D UpperTriangularForm(Matrix4D value)
        {
            Matrix4D result;
            UpperTriangularForm(ref value, out result);
            return result;
        }

        /// <summary>
        /// Brings the MatrixDouble into lower triangular form using elementary row operations.
        /// </summary>
        /// <param name="value">The MatrixDouble to put into lower triangular form.</param>
        /// <param name="result">When the method completes, contains the lower triangular matrix.</param>
        /// <remarks>
        /// If the MatrixDouble is not invertible (i.e. its determinant is zero) than the result of this
        /// method may produce Single.Nan and Single.Inf values. When the MatrixDouble represents a system
        /// of linear equations, than this often means that either no solution exists or an infinite
        /// number of solutions exist.
        /// </remarks>
        public static void LowerTriangularForm(ref Matrix4D value, out Matrix4D result)
        {
            //Adapted from the row echelon code.
            Matrix4D temp = value;
            Matrix4D.Transpose(ref temp, out result);

            int lead = 0;
            int rowcount = 4;
            int columncount = 4;

            for (int r = 0; r < rowcount; ++r)
            {
                if (columncount <= lead)
                    return;

                int i = r;

                while (MathHelperDP.IsZero(result[i, lead]))
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
                        result[i, 3] -= result[r, 3] * multiplier * result[i, lead];
                    }
                }

                lead++;
            }

            Matrix4D.Transpose(ref result, out result);
        }

        /// <summary>
        /// Brings the MatrixDouble into lower triangular form using elementary row operations.
        /// </summary>
        /// <param name="value">The MatrixDouble to put into lower triangular form.</param>
        /// <returns>The lower triangular matrix.</returns>
        /// <remarks>
        /// If the MatrixDouble is not invertible (i.e. its determinant is zero) than the result of this
        /// method may produce Single.Nan and Single.Inf values. When the MatrixDouble represents a system
        /// of linear equations, than this often means that either no solution exists or an infinite
        /// number of solutions exist.
        /// </remarks>
        public static Matrix4D LowerTriangularForm(Matrix4D value)
        {
            Matrix4D result;
            LowerTriangularForm(ref value, out result);
            return result;
        }

        /// <summary>
        /// Brings the MatrixDouble into row echelon form using elementary row operations;
        /// </summary>
        /// <param name="value">The MatrixDouble to put into row echelon form.</param>
        /// <param name="result">When the method completes, contains the row echelon form of the matrix.</param>
        public static void RowEchelonForm(ref Matrix4D value, out Matrix4D result)
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

                while (MathHelperDP.IsZero(result[i, lead]))
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
        /// Brings the MatrixDouble into row echelon form using elementary row operations;
        /// </summary>
        /// <param name="value">The MatrixDouble to put into row echelon form.</param>
        /// <returns>When the method completes, contains the row echelon form of the matrix.</returns>
        public static Matrix4D RowEchelonForm(Matrix4D value)
        {
            Matrix4D result;
            RowEchelonForm(ref value, out result);
            return result;
        }

        /// <summary>
        /// Brings the MatrixDouble into reduced row echelon form using elementary row operations.
        /// </summary>
        /// <param name="value">The MatrixDouble to put into reduced row echelon form.</param>
        /// <param name="augment">The fifth column of the matrix.</param>
        /// <param name="result">When the method completes, contains the resultant MatrixDouble after the operation.</param>
        /// <param name="augmentResult">When the method completes, contains the resultant fifth column of the matrix.</param>
        /// <remarks>
        /// <para>The fifth column is often called the augmented part of the matrix. This is because the fifth
        /// column is really just an extension of the MatrixDouble so that there is a place to put all of the
        /// non-zero components after the operation is complete.</para>
        /// <para>Often times the resultant MatrixDouble will the identity MatrixDouble or a MatrixDouble similar to the identity
        /// matrix. Sometimes, however, that is not possible and numbers other than zero and one may appear.</para>
        /// <para>This method can be used to solve systems of linear equations. Upon completion of this method,
        /// the <paramref name="augmentResult"/> will contain the solution for the system. It is up to the user
        /// to analyze both the input and the result to determine if a solution really exists.</para>
        /// </remarks>
        public static void ReducedRowEchelonForm(ref Matrix4D value, ref Vector4D augment, out Matrix4D result, out Vector4D augmentResult)
        {
            //Source: http://rosettacode.org
            //Reference: http://rosettacode.org/wiki/Reduced_row_echelon_form

            double[,] matrix = new double[4, 5];

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
                    double temp = matrix[r, j];
                    matrix[r, j] = matrix[i, j];
                    matrix[i, j] = temp;
                }

                double div = matrix[r, lead];

                for (int j = 0; j < columncount; j++)
                {
                    matrix[r, j] /= div;
                }

                for (int j = 0; j < rowcount; j++)
                {
                    if (j != r)
                    {
                        double sub = matrix[j, lead];
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
        public static void BillboardLH(ref Vector3D objectPosition, ref Vector3D cameraPosition, ref Vector3D cameraUpVector, ref Vector3D cameraForwardVector, out Matrix4D result)
        {
            Vector3D crossed;
            Vector3D final;
            Vector3D difference = cameraPosition - objectPosition;

            double lengthSq = difference.LengthSquared();
            if (MathHelperDP.IsZero(lengthSq))
                difference = -cameraForwardVector;
            else
                difference *= (float)(1.0 / Math.Sqrt(lengthSq));

            Vector3D.Cross(ref cameraUpVector, ref difference, out crossed);
            crossed.Normalize();
            Vector3D.Cross(ref difference, ref crossed, out final);

            result.M11 = crossed.X;
            result.M12 = crossed.Y;
            result.M13 = crossed.Z;
            result.M14 = 0.0f;
            result.M21 = final.X;
            result.M22 = final.Y;
            result.M23 = final.Z;
            result.M24 = 0.0f;
            result.M31 = difference.X;
            result.M32 = difference.Y;
            result.M33 = difference.Z;
            result.M34 = 0.0f;
            result.M41 = objectPosition.X;
            result.M42 = objectPosition.Y;
            result.M43 = objectPosition.Z;
            result.M44 = 1.0f;
        }

        /// <summary>
        /// Creates a left-handed spherical billboard that rotates around a specified object position.
        /// </summary>
        /// <param name="objectPosition">The position of the object around which the billboard will rotate.</param>
        /// <param name="cameraPosition">The position of the camera.</param>
        /// <param name="cameraUpVector">The up vector of the camera.</param>
        /// <param name="cameraForwardVector">The forward vector of the camera.</param>
        /// <returns>The created billboard matrix.</returns>
        public static Matrix4D BillboardLH(Vector3D objectPosition, Vector3D cameraPosition, Vector3D cameraUpVector, Vector3D cameraForwardVector)
        {
            Matrix4D result;
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
        public static void BillboardRH(ref Vector3D objectPosition, ref Vector3D cameraPosition, ref Vector3D cameraUpVector, ref Vector3D cameraForwardVector, out Matrix4D result)
        {
            Vector3D crossed;
            Vector3D final;
            Vector3D difference = objectPosition - cameraPosition;

            double lengthSq = difference.LengthSquared();
            if (MathHelperDP.IsZero(lengthSq))
                difference = -cameraForwardVector;
            else
                difference *= (float)(1.0 / Math.Sqrt(lengthSq));

            Vector3D.Cross(ref cameraUpVector, ref difference, out crossed);
            crossed.Normalize();
            Vector3D.Cross(ref difference, ref crossed, out final);

            result.M11 = crossed.X;
            result.M12 = crossed.Y;
            result.M13 = crossed.Z;
            result.M14 = 0.0f;
            result.M21 = final.X;
            result.M22 = final.Y;
            result.M23 = final.Z;
            result.M24 = 0.0f;
            result.M31 = difference.X;
            result.M32 = difference.Y;
            result.M33 = difference.Z;
            result.M34 = 0.0f;
            result.M41 = objectPosition.X;
            result.M42 = objectPosition.Y;
            result.M43 = objectPosition.Z;
            result.M44 = 1.0f;
        }

        /// <summary>
        /// Creates a right-handed spherical billboard that rotates around a specified object position.
        /// </summary>
        /// <param name="objectPosition">The position of the object around which the billboard will rotate.</param>
        /// <param name="cameraPosition">The position of the camera.</param>
        /// <param name="cameraUpVector">The up vector of the camera.</param>
        /// <param name="cameraForwardVector">The forward vector of the camera.</param>
        /// <returns>The created billboard matrix.</returns>
        public static Matrix4D BillboardRH(Vector3D objectPosition, Vector3D cameraPosition, Vector3D cameraUpVector, Vector3D cameraForwardVector) {
            Matrix4D result;
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
        public static void LookAtLH(ref Vector3D eye, ref Vector3D target, ref Vector3D up, out Matrix4D result)
        {
            Vector3D xaxis, yaxis, zaxis;
            Vector3D.Subtract(ref target, ref eye, out zaxis); zaxis.Normalize();
            Vector3D.Cross(ref up, ref zaxis, out xaxis); xaxis.Normalize();
            Vector3D.Cross(ref zaxis, ref xaxis, out yaxis);

            result = Matrix4D.Identity;
            result.M11 = xaxis.X; result.M21 = xaxis.Y; result.M31 = xaxis.Z;
            result.M12 = yaxis.X; result.M22 = yaxis.Y; result.M32 = yaxis.Z;
            result.M13 = zaxis.X; result.M23 = zaxis.Y; result.M33 = zaxis.Z;

            Vector3D.Dot(ref xaxis, ref eye, out result.M41);
            Vector3D.Dot(ref yaxis, ref eye, out result.M42);
            Vector3D.Dot(ref zaxis, ref eye, out result.M43);

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
        public static Matrix4D LookAtLH(Vector3D eye, Vector3D target, Vector3D up)
        {
            Matrix4D result;
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
        public static void LookAtRH(ref Vector3D eye, ref Vector3D target, ref Vector3D up, out Matrix4D result)
        {
            Vector3D xaxis, yaxis, zaxis;
            Vector3D.Subtract(ref eye, ref target, out zaxis); zaxis.Normalize();
            Vector3D.Cross(ref up, ref zaxis, out xaxis); xaxis.Normalize();
            Vector3D.Cross(ref zaxis, ref xaxis, out yaxis);

            result = Matrix4D.Identity;
            result.M11 = xaxis.X; result.M21 = xaxis.Y; result.M31 = xaxis.Z;
            result.M12 = yaxis.X; result.M22 = yaxis.Y; result.M32 = yaxis.Z;
            result.M13 = zaxis.X; result.M23 = zaxis.Y; result.M33 = zaxis.Z;

            Vector3D.Dot(ref xaxis, ref eye, out result.M41);
            Vector3D.Dot(ref yaxis, ref eye, out result.M42);
            Vector3D.Dot(ref zaxis, ref eye, out result.M43);

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
        public static Matrix4D LookAtRH(Vector3D eye, Vector3D target, Vector3D up)
        {
            Matrix4D result;
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
        public static void OrthoLH(double width, double height, double znear, double zfar, out Matrix4D result)
        {
            double halfWidth = width * 0.5f;
            double halfHeight = height * 0.5f;

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
        public static Matrix4D OrthoLH(double width, double height, double znear, double zfar)
        {
            Matrix4D result;
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
        public static void OrthoRH(double width, double height, double znear, double zfar, out Matrix4D result)
        {
            double halfWidth = width * 0.5f;
            double halfHeight = height * 0.5f;

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
        public static Matrix4D OrthoRH(double width, double height, double znear, double zfar)
        {
            Matrix4D result;
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
        public static void OrthoOffCenterLH(double left, double right, double bottom, double top, double znear, double zfar, out Matrix4D result)
        {
            double zRange = 1.0f / (zfar - znear);

            result = Matrix4D.Identity;
            result.M11 = 2.0f / (right - left);
            result.M22 = 2.0f / (top - bottom);
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
        public static Matrix4D OrthoOffCenterLH(double left, double right, double bottom, double top, double znear, double zfar)
        {
            Matrix4D result;
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
        public static void OrthoOffCenterRH(double left, double right, double bottom, double top, double znear, double zfar, out Matrix4D result)
        {
            OrthoOffCenterLH(left, right, bottom, top, znear, zfar, out result);
            result.M33 *= -1.0f;
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
        public static Matrix4D OrthoOffCenterRH(double left, double right, double bottom, double top, double znear, double zfar)
        {
            Matrix4D result;
            OrthoOffCenterRH(left, right, bottom, top, znear, zfar, out result);
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
        public static void PerspectiveLH(double width, double height, double znear, double zfar, out Matrix4D result)
        {
            double halfWidth = width * 0.5f;
            double halfHeight = height * 0.5f;

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
        public static Matrix4D PerspectiveLH(double width, double height, double znear, double zfar)
        {
            Matrix4D result;
            PerspectiveLH(width, height, znear, zfar, out result);
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
        public static void PerspectiveRH(double width, double height, double znear, double zfar, out Matrix4D result)
        {
            double halfWidth = width * 0.5f;
            double halfHeight = height * 0.5f;

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
        public static Matrix4D PerspectiveRH(double width, double height, double znear, double zfar)
        {
            Matrix4D result;
            PerspectiveRH(width, height, znear, zfar, out result);
            return result;
        }

        /// <summary>
        /// Creates a left-handed, perspective projection MatrixDouble based on a field of view.
        /// </summary>
        /// <param name="fov">Field of view in the y direction, in radians.</param>
        /// <param name="aspect">Aspect ratio, defined as view space width divided by height.</param>
        /// <param name="znear">Minimum z-value of the viewing volume.</param>
        /// <param name="zfar">Maximum z-value of the viewing volume.</param>
        /// <param name="result">When the method completes, contains the created projection matrix.</param>
        public static void PerspectiveFovLH(double fov, double aspect, double znear, double zfar, out Matrix4D result)
        {
            double yScale = (float)(1.0 / Math.Tan(fov * 0.5f));
            double xScale = yScale / aspect;

            double halfWidth = znear / xScale;
            double halfHeight = znear / yScale;

            PerspectiveOffCenterLH(-halfWidth, halfWidth, -halfHeight, halfHeight, znear, zfar, out result);
        }

        /// <summary>
        /// Creates a left-handed, perspective projection MatrixDouble based on a field of view.
        /// </summary>
        /// <param name="fov">Field of view in the y direction, in radians.</param>
        /// <param name="aspect">Aspect ratio, defined as view space width divided by height.</param>
        /// <param name="znear">Minimum z-value of the viewing volume.</param>
        /// <param name="zfar">Maximum z-value of the viewing volume.</param>
        /// <returns>The created projection matrix.</returns>
        public static Matrix4D PerspectiveFovLH(double fov, double aspect, double znear, double zfar)
        {
            Matrix4D result;
            PerspectiveFovLH(fov, aspect, znear, zfar, out result);
            return result;
        }

        /// <summary>
        /// Creates a right-handed, perspective projection MatrixDouble based on a field of view.
        /// </summary>
        /// <param name="fov">Field of view in the y direction, in radians.</param>
        /// <param name="aspect">Aspect ratio, defined as view space width divided by height.</param>
        /// <param name="znear">Minimum z-value of the viewing volume.</param>
        /// <param name="zfar">Maximum z-value of the viewing volume.</param>
        /// <param name="result">When the method completes, contains the created projection matrix.</param>
        public static void PerspectiveFovRH(double fov, double aspect, double znear, double zfar, out Matrix4D result)
        {
            double yScale = (float)(1.0 / Math.Tan(fov * 0.5f));
            double xScale = yScale / aspect;

            double halfWidth = znear / xScale;
            double halfHeight = znear / yScale;

            PerspectiveOffCenterRH(-halfWidth, halfWidth, -halfHeight, halfHeight, znear, zfar, out result);
        }

        /// <summary>
        /// Creates a right-handed, perspective projection MatrixDouble based on a field of view.
        /// </summary>
        /// <param name="fov">Field of view in the y direction, in radians.</param>
        /// <param name="aspect">Aspect ratio, defined as view space width divided by height.</param>
        /// <param name="znear">Minimum z-value of the viewing volume.</param>
        /// <param name="zfar">Maximum z-value of the viewing volume.</param>
        /// <returns>The created projection matrix.</returns>
        public static Matrix4D PerspectiveFovRH(double fov, double aspect, double znear, double zfar)
        {
            Matrix4D result;
            PerspectiveFovRH(fov, aspect, znear, zfar, out result);
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
        public static void PerspectiveOffCenterLH(double left, double right, double bottom, double top, double znear, double zfar, out Matrix4D result)
        {
            double zRange = zfar / (zfar - znear);

            result = new Matrix4D();
            result.M11 = 2.0f * znear / (right - left);
            result.M22 = 2.0f * znear / (top - bottom);
            result.M31 = (left + right) / (left - right);
            result.M32 = (top + bottom) / (bottom - top);
            result.M33 = zRange;
            result.M34 = 1.0f;
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
        public static Matrix4D PerspectiveOffCenterLH(double left, double right, double bottom, double top, double znear, double zfar)
        {
            Matrix4D result;
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
        public static void PerspectiveOffCenterRH(double left, double right, double bottom, double top, double znear, double zfar, out Matrix4D result)
        {
            PerspectiveOffCenterLH(left, right, bottom, top, znear, zfar, out result);
            result.M31 *= -1.0f;
            result.M32 *= -1.0f;
            result.M33 *= -1.0f;
            result.M34 *= -1.0f;
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
        public static Matrix4D PerspectiveOffCenterRH(double left, double right, double bottom, double top, double znear, double zfar)
        {
            Matrix4D result;
            PerspectiveOffCenterRH(left, right, bottom, top, znear, zfar, out result);
            return result;
        }


        /// <summary>
        /// Creates a MatrixDouble that scales along the x-axis, y-axis, and y-axis.
        /// </summary>
        /// <param name="scale">Scaling factor for all three axes.</param>
        /// <param name="result">When the method completes, contains the created scaling matrix.</param>
        public static void Scaling(ref Vector3D scale, out Matrix4D result)
        {
            Scaling(scale.X, scale.Y, scale.Z, out result);
        }

        /// <summary>
        /// Creates a MatrixDouble that scales along the x-axis, y-axis, and y-axis.
        /// </summary>
        /// <param name="scale">Scaling factor for all three axes.</param>
        /// <returns>The created scaling matrix.</returns>
        public static Matrix4D Scaling(Vector3D scale)
        {
            Matrix4D result;
            Scaling(ref scale, out result);
            return result;
        }

        /// <summary>
        /// Creates a MatrixDouble that scales along the x-axis, y-axis, and y-axis.
        /// </summary>
        /// <param name="x">Scaling factor that is applied along the x-axis.</param>
        /// <param name="y">Scaling factor that is applied along the y-axis.</param>
        /// <param name="z">Scaling factor that is applied along the z-axis.</param>
        /// <param name="result">When the method completes, contains the created scaling matrix.</param>
        public static void Scaling(double x, double y, double z, out Matrix4D result)
        {
            result = Matrix4D.Identity;
            result.M11 = x;
            result.M22 = y;
            result.M33 = z;
        }

        /// <summary>
        /// Creates a MatrixDouble that scales along the x-axis, y-axis, and y-axis.
        /// </summary>
        /// <param name="x">Scaling factor that is applied along the x-axis.</param>
        /// <param name="y">Scaling factor that is applied along the y-axis.</param>
        /// <param name="z">Scaling factor that is applied along the z-axis.</param>
        /// <returns>The created scaling matrix.</returns>
        public static Matrix4D Scaling(double x, double y, double z)
        {
            Matrix4D result;
            Scaling(x, y, z, out result);
            return result;
        }

        /// <summary>
        /// Creates a MatrixDouble that uniformly scales along all three axis.
        /// </summary>
        /// <param name="scale">The uniform scale that is applied along all axis.</param>
        /// <param name="result">When the method completes, contains the created scaling matrix.</param>
        public static void Scaling(double scale, out Matrix4D result)
        {
            result = Matrix4D.Identity;
            result.M11 = result.M22 = result.M33 = scale;
        }

        /// <summary>
        /// Creates a MatrixDouble that uniformly scales along all three axis.
        /// </summary>
        /// <param name="scale">The uniform scale that is applied along all axis.</param>
        /// <returns>The created scaling matrix.</returns>
        public static Matrix4D Scaling(double scale)
        {
            Matrix4D result;
            Scaling(scale, out result);
            return result;
        }

        /// <summary>
        /// Creates a MatrixDouble that rotates around the x-axis.
        /// </summary>
        /// <param name="angle">Angle of rotation in radians. Angles are measured clockwise when looking along the rotation axis toward the origin.</param>
        /// <param name="result">When the method completes, contains the created rotation matrix.</param>
        public static void RotationX(double angle, out Matrix4D result)
        {
            double cos = (float)Math.Cos(angle);
            double sin = (float)Math.Sin(angle);

            result = Matrix4D.Identity;
            result.M22 = cos;
            result.M23 = sin;
            result.M32 = -sin;
            result.M33 = cos;
        }

        /// <summary>
        /// Creates a MatrixDouble that rotates around the x-axis.
        /// </summary>
        /// <param name="angle">Angle of rotation in radians. Angles are measured clockwise when looking along the rotation axis toward the origin.</param>
        /// <returns>The created rotation matrix.</returns>
        public static Matrix4D RotationX(double angle)
        {
            Matrix4D result;
            RotationX(angle, out result);
            return result;
        }

        /// <summary>
        /// Creates a MatrixDouble that rotates around the y-axis.
        /// </summary>
        /// <param name="angle">Angle of rotation in radians. Angles are measured clockwise when looking along the rotation axis toward the origin.</param>
        /// <param name="result">When the method completes, contains the created rotation matrix.</param>
        public static void RotationY(double angle, out Matrix4D result)
        {
            double cos = (float)Math.Cos(angle);
            double sin = (float)Math.Sin(angle);

            result = Matrix4D.Identity;
            result.M11 = cos;
            result.M13 = -sin;
            result.M31 = sin;
            result.M33 = cos;
        }

        /// <summary>
        /// Creates a MatrixDouble that rotates around the y-axis.
        /// </summary>
        /// <param name="angle">Angle of rotation in radians. Angles are measured clockwise when looking along the rotation axis toward the origin.</param>
        /// <returns>The created rotation matrix.</returns>
        public static Matrix4D RotationY(double angle)
        {
            Matrix4D result;
            RotationY(angle, out result);
            return result;
        }

        /// <summary>
        /// Creates a MatrixDouble that rotates around the z-axis.
        /// </summary>
        /// <param name="angle">Angle of rotation in radians. Angles are measured clockwise when looking along the rotation axis toward the origin.</param>
        /// <param name="result">When the method completes, contains the created rotation matrix.</param>
        public static void RotationZ(double angle, out Matrix4D result)
        {
            double cos = (float)Math.Cos(angle);
            double sin = (float)Math.Sin(angle);

            result = Matrix4D.Identity;
            result.M11 = cos;
            result.M12 = sin;
            result.M21 = -sin;
            result.M22 = cos;
        }

        /// <summary>
        /// Creates a MatrixDouble that rotates around the z-axis.
        /// </summary>
        /// <param name="angle">Angle of rotation in radians. Angles are measured clockwise when looking along the rotation axis toward the origin.</param>
        /// <returns>The created rotation matrix.</returns>
        public static Matrix4D RotationZ(double angle)
        {
            Matrix4D result;
            RotationZ(angle, out result);
            return result;
        }

        /// <summary>
        /// Creates a MatrixDouble that rotates around an arbitrary axis.
        /// </summary>
        /// <param name="axis">The axis around which to rotate. This parameter is assumed to be normalized.</param>
        /// <param name="angle">Angle of rotation in radians. Angles are measured clockwise when looking along the rotation axis toward the origin.</param>
        /// <param name="result">When the method completes, contains the created rotation matrix.</param>
        public static void RotationAxis(ref Vector3D axis, double angle, out Matrix4D result)
        {
            double x = axis.X;
            double y = axis.Y;
            double z = axis.Z;
            double cos = (float)Math.Cos(angle);
            double sin = (float)Math.Sin(angle);
            double xx = x * x;
            double yy = y * y;
            double zz = z * z;
            double xy = x * y;
            double xz = x * z;
            double yz = y * z;

            result = Matrix4D.Identity;
            result.M11 = xx + (cos * (1.0f - xx));
            result.M12 = (xy - (cos * xy)) + (sin * z);
            result.M13 = (xz - (cos * xz)) - (sin * y);
            result.M21 = (xy - (cos * xy)) - (sin * z);
            result.M22 = yy + (cos * (1.0f - yy));
            result.M23 = (yz - (cos * yz)) + (sin * x);
            result.M31 = (xz - (cos * xz)) + (sin * y);
            result.M32 = (yz - (cos * yz)) - (sin * x);
            result.M33 = zz + (cos * (1.0f - zz));
        }

        /// <summary>
        /// Creates a MatrixDouble that rotates around an arbitrary axis.
        /// </summary>
        /// <param name="axis">The axis around which to rotate. This parameter is assumed to be normalized.</param>
        /// <param name="angle">Angle of rotation in radians. Angles are measured clockwise when looking along the rotation axis toward the origin.</param>
        /// <returns>The created rotation matrix.</returns>
        public static Matrix4D RotationAxis(Vector3D axis, double angle)
        {
            Matrix4D result;
            RotationAxis(ref axis, angle, out result);
            return result;
        }

        /// <summary>
        /// Creates a rotation MatrixDouble from a QuaternionDouble .
        /// </summary>
        /// <param name="rotation">The QuaternionDouble to use to build the matrix.</param>
        /// <param name="result">The created rotation matrix.</param>
        public static void FromQuaternion(ref QuaternionD rotation, out Matrix4D result)
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

            result = Identity;
            result.M11 = 1.0f - (2.0f * (yy + zz));
            result.M12 = 2.0f * (xy + zw);
            result.M13 = 2.0f * (zx - yw);
            result.M21 = 2.0f * (xy - zw);
            result.M22 = 1.0f - (2.0f * (zz + xx));
            result.M23 = 2.0f * (yz + xw);
            result.M31 = 2.0f * (zx + yw);
            result.M32 = 2.0f * (yz - xw);
            result.M33 = 1.0f - (2.0f * (yy + xx));
        }

        /// <summary>
        /// Creates a rotation MatrixDouble from a QuaternionDouble .
        /// </summary>
        /// <param name="rotation">The QuaternionDouble to use to build the matrix.</param>
        /// <returns>The created rotation matrix.</returns>
        public static Matrix4D FromQuaternion(QuaternionD rotation)
        {
            Matrix4D result;
            FromQuaternion(ref rotation, out result);
            return result;
        }

        /// <summary>
        /// Creates a rotation MatrixDouble from a QuaternionDouble .
        /// </summary>
        /// <param name="rotation">The QuaternionDouble to use to build the matrix.</param>
        /// <returns>The created rotation matrix.</returns>
        public static Matrix4D FromQuaternion(ref QuaternionD rotation)
        {
            Matrix4D result;
            FromQuaternion(ref rotation, out result);
            return result;
        }

        /// <summary>
        /// Creates a rotation MatrixDouble with a specified yaw, pitch, and roll.
        /// </summary>
        /// <param name="yaw">Yaw around the y-axis, in radians.</param>
        /// <param name="pitch">Pitch around the x-axis, in radians.</param>
        /// <param name="roll">Roll around the z-axis, in radians.</param>
        /// <param name="result">When the method completes, contains the created rotation matrix.</param>
        public static void RotationYawPitchRoll(double yaw, double pitch, double roll, out Matrix4D result)
        {
            QuaternionD quaternion = new QuaternionD();
            QuaternionD.RotationYawPitchRoll(yaw, pitch, roll, out quaternion);
            FromQuaternion(ref quaternion, out result);
        }

        /// <summary>
        /// Creates a rotation MatrixDouble with a specified yaw, pitch, and roll.
        /// </summary>
        /// <param name="yaw">Yaw around the y-axis, in radians.</param>
        /// <param name="pitch">Pitch around the x-axis, in radians.</param>
        /// <param name="roll">Roll around the z-axis, in radians.</param>
        /// <returns>The created rotation matrix.</returns>
        public static Matrix4D RotationYawPitchRoll(double yaw, double pitch, double roll)
        {
            Matrix4D result;
            RotationYawPitchRoll(yaw, pitch, roll, out result);
            return result;
        }

        /// <summary>
        /// Creates a translation MatrixDouble using the specified offsets.
        /// </summary>
        /// <param name="value">The offset for all three coordinate planes.</param>
        /// <param name="result">When the method completes, contains the created translation matrix.</param>
        public static void CreateTranslation(ref Vector3D value, out Matrix4D result)
        {
            CreateTranslation(value.X, value.Y, value.Z, out result);
        }

        /// <summary>
        /// Creates a translation MatrixDouble using the specified offsets.
        /// </summary>
        /// <param name="value">The offset for all three coordinate planes.</param>
        /// <returns>The created translation matrix.</returns>
        public static Matrix4D CreateTranslation(Vector3D value)
        {
            Matrix4D result;
            CreateTranslation(ref value, out result);
            return result;
        }

        /// <summary>
        /// Creates a translation MatrixDouble using the specified offsets.
        /// </summary>
        /// <param name="x">X-coordinate offset.</param>
        /// <param name="y">Y-coordinate offset.</param>
        /// <param name="z">Z-coordinate offset.</param>
        /// <param name="result">When the method completes, contains the created translation matrix.</param>
        public static void CreateTranslation(double x, double y, double z, out Matrix4D result)
        {
            result = Matrix4D.Identity;
            result.M41 = x;
            result.M42 = y;
            result.M43 = z;
        }

        /// <summary>
        /// Creates a translation MatrixDouble using the specified offsets.
        /// </summary>
        /// <param name="x">X-coordinate offset.</param>
        /// <param name="y">Y-coordinate offset.</param>
        /// <param name="z">Z-coordinate offset.</param>
        /// <returns>The created translation matrix.</returns>
        public static Matrix4D CreateTranslation(double x, double y, double z)
        {
            Matrix4D result;
            CreateTranslation(x, y, z, out result);
            return result;
        }

        /// <summary>
        /// Creates a skew/shear MatrixDouble by means of a translation vector, a rotation vector, and a rotation angle.
        /// shearing is performed in the direction of translation vector, where translation vector and rotation vector define the shearing plane.
        /// The effect is such that the skewed rotation vector has the specified angle with rotation itself.
        /// </summary>
        /// <param name="angle">The rotation angle.</param>
        /// <param name="rotationVec">The rotation vector</param>
        /// <param name="transVec">The translation vector</param>
        /// <param name="matrix">Contains the created skew/shear matrix. </param>
        public static void CreateSkew(double angle, ref Vector3D rotationVec, ref Vector3D transVec, out Matrix4D matrix)
        {
            //http://elckerlyc.ewi.utwente.nl/browser/Elckerlyc/Hmi/HmiMath/src/hmi/math/Mat3f.java
            double MINIMAL_SKEW_ANGLE = 0.000001f;

            Vector3D e0 = rotationVec;
            Vector3D e1 = Vector3D.Normalize(transVec);

            double rv1;
            Vector3D.Dot(ref rotationVec, ref  e1, out rv1);
            e0 += rv1 * e1;
            double rv0;
            Vector3D.Dot(ref rotationVec, ref e0, out rv0);
            double cosa = (float)Math.Cos(angle);
            double sina = (float)Math.Sin(angle);
            double rr0 = rv0 * cosa - rv1 * sina;
            double rr1 = rv0 * sina + rv1 * cosa;

            if (rr0 < MINIMAL_SKEW_ANGLE)
                throw new ArgumentException("illegal skew angle");

            double d = (rr1 / rr0) - (rv1 / rv0);

            matrix = Identity;
            matrix.M11 = d * e1[0] * e0[0] + 1.0f;
            matrix.M12 = d * e1[0] * e0[1];
            matrix.M13 = d * e1[0] * e0[2];
            matrix.M21 = d * e1[1] * e0[0];
            matrix.M22 = d * e1[1] * e0[1] + 1.0f;
            matrix.M23 = d * e1[1] * e0[2];
            matrix.M31 = d * e1[2] * e0[0];
            matrix.M32 = d * e1[2] * e0[1];
            matrix.M33 = d * e1[2] * e0[2] + 1.0f;
        }

        /// <summary>
        /// Creates a 3D affine transformation matrix.
        /// </summary>
        /// <param name="scaling">Scaling factor.</param>
        /// <param name="rotation">The rotation of the transformation.</param>
        /// <param name="translation">The translation factor of the transformation.</param>
        /// <param name="result">When the method completes, contains the created affine transformation matrix.</param>
        public static void AffineTransformation(double scaling, ref QuaternionD rotation, ref Vector3D translation, out Matrix4D result)
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
        public static Matrix4D AffineTransformation(double scaling, QuaternionD rotation, Vector3D translation)
        {
            Matrix4D result;
            AffineTransformation(scaling, ref rotation, ref translation, out result);
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
        public static void AffineTransformation(double scaling, ref Vector3D rotationCenter, ref QuaternionD rotation, ref Vector3D translation, out Matrix4D result)
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
        public static Matrix4D AffineTransformation(double scaling, Vector3D rotationCenter, QuaternionD rotation, Vector3D translation)
        {
            Matrix4D result;
            AffineTransformation(scaling, ref rotationCenter, ref rotation, ref translation, out result);
            return result;
        }

        /// <summary>
        /// Creates a 2D affine transformation matrix.
        /// </summary>
        /// <param name="scaling">Scaling factor.</param>
        /// <param name="rotation">The rotation of the transformation.</param>
        /// <param name="translation">The translation factor of the transformation.</param>
        /// <param name="result">When the method completes, contains the created affine transformation matrix.</param>
        public static void AffineTransformation2D(double scaling, double rotation, ref Vector2D translation, out Matrix4D result)
        {
            result = Scaling(scaling, scaling, 1.0f) * RotationZ(rotation) * CreateTranslation((Vector3D)translation);
        }

        /// <summary>
        /// Creates a 2D affine transformation matrix.
        /// </summary>
        /// <param name="scaling">Scaling factor.</param>
        /// <param name="rotation">The rotation of the transformation.</param>
        /// <param name="translation">The translation factor of the transformation.</param>
        /// <returns>The created affine transformation matrix.</returns>
        public static Matrix4D AffineTransformation2D(double scaling, double rotation, Vector2D translation)
        {
            Matrix4D result;
            AffineTransformation2D(scaling, rotation, ref translation, out result);
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
        public static void AffineTransformation2D(double scaling, ref Vector2D rotationCenter, double rotation, ref Vector2D translation, out Matrix4D result)
        {
            result = Scaling(scaling, scaling, 1.0f) * CreateTranslation((Vector3D)(-rotationCenter)) * RotationZ(rotation) *
                CreateTranslation((Vector3D)rotationCenter) * CreateTranslation((Vector3D)translation);
        }

        /// <summary>
        /// Creates a 2D affine transformation matrix.
        /// </summary>
        /// <param name="scaling">Scaling factor.</param>
        /// <param name="rotationCenter">The center of the rotation.</param>
        /// <param name="rotation">The rotation of the transformation.</param>
        /// <param name="translation">The translation factor of the transformation.</param>
        /// <returns>The created affine transformation matrix.</returns>
        public static Matrix4D AffineTransformation2D(double scaling, Vector2D rotationCenter, double rotation, Vector2D translation)
        {
            Matrix4D result;
            AffineTransformation2D(scaling, ref rotationCenter, rotation, ref translation, out result);
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
        public static void Transformation(ref Vector3D scalingCenter, ref QuaternionD scalingRotation, ref Vector3D scaling, ref Vector3D rotationCenter, ref QuaternionD rotation, ref Vector3D translation, out Matrix4D result)
        {
            Matrix4D sr = FromQuaternion(scalingRotation);

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
        public static Matrix4D Transformation(Vector3D scalingCenter, QuaternionD scalingRotation, Vector3D scaling, Vector3D rotationCenter, QuaternionD rotation, Vector3D translation)
        {
            Matrix4D result;
            Transformation(ref scalingCenter, ref scalingRotation, ref scaling, ref rotationCenter, ref rotation, ref translation, out result);
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
        public static void Transformation2D(ref Vector2D scalingCenter, double scalingRotation, ref Vector2D scaling, ref Vector2D rotationCenter, double rotation, ref Vector2D translation, out Matrix4D result)
        {
            result = CreateTranslation((Vector3D)(-scalingCenter)) * RotationZ(-scalingRotation) * Scaling((Vector3D)scaling) * RotationZ(scalingRotation) * CreateTranslation((Vector3D)scalingCenter) * 
                CreateTranslation((Vector3D)(-rotationCenter)) * RotationZ(rotation) * CreateTranslation((Vector3D)rotationCenter) * CreateTranslation((Vector3D)translation);

            result.M33 = 1f;
            result.M44 = 1f;
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
        public static Matrix4D Transformation2D(Vector2D scalingCenter, double scalingRotation, Vector2D scaling, Vector2D rotationCenter, double rotation, Vector2D translation)
        {
            Matrix4D result;
            Transformation2D(ref scalingCenter, scalingRotation, ref scaling, ref rotationCenter, rotation, ref translation, out result);
            return result;
        }

        /// <summary>
        /// Adds two matrices.
        /// </summary>
        /// <param name="left">The first MatrixDouble to add.</param>
        /// <param name="right">The second MatrixDouble to add.</param>
        /// <returns>The sum of the two matrices.</returns>
        public static Matrix4D operator +(Matrix4D left, Matrix4D right)
        {
            Matrix4D result;
            Add(ref left, ref right, out result);
            return result;
        }

        /// <summary>
        /// Assert a MatrixDouble (return it unchanged).
        /// </summary>
        /// <param name="value">The MatrixDouble to assert (unchanged).</param>
        /// <returns>The asserted (unchanged) matrix.</returns>
        public static Matrix4D operator +(Matrix4D value)
        {
            return value;
        }

        /// <summary>
        /// Subtracts two matrices.
        /// </summary>
        /// <param name="left">The first MatrixDouble to subtract.</param>
        /// <param name="right">The second MatrixDouble to subtract.</param>
        /// <returns>The difference between the two matrices.</returns>
        public static Matrix4D operator -(Matrix4D left, Matrix4D right)
        {
            Matrix4D result;
            Subtract(ref left, ref right, out result);
            return result;
        }

        /// <summary>
        /// Negates a matrix.
        /// </summary>
        /// <param name="value">The MatrixDouble to negate.</param>
        /// <returns>The negated matrix.</returns>
        public static Matrix4D operator -(Matrix4D value)
        {
            Matrix4D result;
            Negate(ref value, out result);
            return result;
        }

        /// <summary>
        /// Scales a MatrixDouble by a given value.
        /// </summary>
        /// <param name="right">The MatrixDouble to scale.</param>
        /// <param name="left">The amount by which to scale.</param>
        /// <returns>The scaled matrix.</returns>
        public static Matrix4D operator *(double left, Matrix4D right)
        {
            Matrix4D result;
            Multiply(ref right, left, out result);
            return result;
        }

        /// <summary>
        /// Scales a MatrixDouble by a given value.
        /// </summary>
        /// <param name="left">The MatrixDouble to scale.</param>
        /// <param name="right">The amount by which to scale.</param>
        /// <returns>The scaled matrix.</returns>
        public static Matrix4D operator *(Matrix4D left, double right)
        {
            Matrix4D result;
            Multiply(ref left, right, out result);
            return result;
        }

        /// <summary>
        /// Multiplies two matrices.
        /// </summary>
        /// <param name="left">The first MatrixDouble to multiply.</param>
        /// <param name="right">The second MatrixDouble to multiply.</param>
        /// <returns>The product of the two matrices.</returns>
        public static Matrix4D operator *(Matrix4D left, Matrix4D right)
        {
            Matrix4D result;
            Multiply(ref left, ref right, out result);
            return result;
        }

        /// <summary>
        /// Scales a MatrixDouble by a given value.
        /// </summary>
        /// <param name="left">The MatrixDouble to scale.</param>
        /// <param name="right">The amount by which to scale.</param>
        /// <returns>The scaled matrix.</returns>
        public static Matrix4D operator /(Matrix4D left, double right)
        {
            Matrix4D result;
            Divide(ref left, right, out result);
            return result;
        }

        /// <summary>
        /// Divides two matrices.
        /// </summary>
        /// <param name="left">The first MatrixDouble to divide.</param>
        /// <param name="right">The second MatrixDouble to divide.</param>
        /// <returns>The quotient of the two matrices.</returns>
        public static Matrix4D operator /(Matrix4D left, Matrix4D right)
        {
            Matrix4D result;
            Divide(ref left, ref right, out result);
            return result;
        }

        /// <summary>
        /// Tests for equality between two objects.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> has the same value as <paramref name="right"/>; otherwise, <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Matrix4D left, Matrix4D right)
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
        public static bool operator !=(Matrix4D left, Matrix4D right)
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
        public bool Equals(ref Matrix4D other)
        {
            return (MathHelperDP.NearEqual(other.M11, M11) &&
                MathHelperDP.NearEqual(other.M12, M12) &&
                MathHelperDP.NearEqual(other.M13, M13) &&
                MathHelperDP.NearEqual(other.M14, M14) &&
                MathHelperDP.NearEqual(other.M21, M21) &&
                MathHelperDP.NearEqual(other.M22, M22) &&
                MathHelperDP.NearEqual(other.M23, M23) &&
                MathHelperDP.NearEqual(other.M24, M24) &&
                MathHelperDP.NearEqual(other.M31, M31) &&
                MathHelperDP.NearEqual(other.M32, M32) &&
                MathHelperDP.NearEqual(other.M33, M33) &&
                MathHelperDP.NearEqual(other.M34, M34) &&
                MathHelperDP.NearEqual(other.M41, M41) &&
                MathHelperDP.NearEqual(other.M42, M42) &&
                MathHelperDP.NearEqual(other.M43, M43) &&
                MathHelperDP.NearEqual(other.M44, M44));
        }

        /// <summary>
        /// Determines whether the specified <see cref="Matrix4F"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Matrix4F"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="Matrix4F"/> is equal to this instance; otherwise, <c>false</c>.
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
            if (!(value is Matrix4F))
                return false;

            var strongValue = (Matrix4D)value;
            return Equals(ref strongValue);
        }
    }
}
