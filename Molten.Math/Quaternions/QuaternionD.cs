using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>Represents a four dimensional mathematical QuaternionD.</summary>
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	public partial struct QuaternionD
	{
		/// <summary>
        /// The size of the <see cref="QuaternionD"/> type, in bytes.
        /// </summary>
        public static readonly int SizeInBytes = Marshal.SizeOf(typeof(QuaternionD));

		 /// <summary>
        /// A <see cref="QuaternionD"/> with all of its components set to zero.
        /// </summary>
        public static readonly QuaternionD Zero = new QuaternionD();

        /// <summary>
        /// A <see cref="QuaternionD"/> with all of its components set to one.
        /// </summary>
        public static readonly QuaternionD One = new QuaternionD(1D, 1D, 1D, 1D);

        /// <summary>
        /// The identity <see cref="QuaternionD"/> (0, 0, 0, 1).
        /// </summary>
        public static readonly QuaternionD Identity = new QuaternionD(0D, 0D, 0D, 0D);

		///<summary>The X component of the QuaternionD.</summary>
		public double X;

        ///<summary>The Y component of the QuaternionD.</summary>
		public double Y;

        ///<summary>The Z component of the QuaternionD.</summary>
		public double Z;

        ///<summary>The W component of the QuaternionD.</summary>
		public double W;

#region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="QuaternionD"/> struct.
        /// </summary>
        /// <param name="value">A QuaternionD containing the values with which to initialize the components.</param>
        public QuaternionD(Vector4D value)
        {
            X = value.X;
            Y = value.Y;
            Z = value.Z;
            W = value.W;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QuaternionD"/> struct.
        /// </summary>
        /// <param name="value">A vector containing the values with which to initialize the X, Y, and Z components.</param>
        /// <param name="w">Initial value for the W component of the <see cref="QuaternionD"/>.</param>
        public QuaternionD(Vector3D value, double w)
        {
            X = value.X;
            Y = value.Y;
            Z = value.Z;
            W = w;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QuaternionD"/> struct.
        /// </summary>
        /// <param name="value">A vector containing the values with which to initialize the X and Y components.</param>
        /// <param name="z">Initial value for the Z component of the <see cref="QuaternionD"/>.</param>
        /// <param name="w">Initial value for the W component of the <see cref="QuaternionD"/>.</param>
        public QuaternionD(Vector2D value, double z, double w)
        {
            X = value.X;
            Y = value.Y;
            Z = z;
            W = w;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QuaternionD"/> struct.
        /// </summary>
        /// <param name="x">Initial value for the X component of the <see cref="QuaternionD"/>.</param>
        /// <param name="y">Initial value for the Y component of the <see cref="QuaternionD"/>.</param>
        /// <param name="z">Initial value for the Z component of the <see cref="QuaternionD"/>.</param>
        /// <param name="w">Initial value for the W component of the <see cref="QuaternionD"/>.</param>
        public QuaternionD(double x, double y, double z, double w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        
        /// <summary>
        /// Initializes a new instance of the  <see cref="QuaternionD"/> struct.
        /// </summary>
        /// <param name="values">The values to assign to the X, Y, Z, and W components of the quaternion. This must be an array with four elements.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than four elements.</exception>
        public QuaternionD(double[] values)
        {
            if (values == null)
                throw new ArgumentNullException("values");
            if (values.Length != 4)
                throw new ArgumentOutOfRangeException("values", "There must be four and only four input values for QuaternionD.");

            X = values[0];
            Y = values[1];
            Z = values[2];
            W = values[3];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QuaternionD"/> struct from an unsafe pointer. The pointer should point to an array of four elements.
        /// </summary>
        public unsafe QuaternionD(double* ptr)
		{
            X = ptr[0];
            Y = ptr[1];
            Z = ptr[2];
            W = ptr[3];
		}
#endregion

#region Instance Methods
/// <summary>
        /// Gets a value indicating whether this instance is equivalent to the identity QuaternionD.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is an identity QuaternionD; otherwise, <c>false</c>.
        /// </value>
        public bool IsIdentity
        {
            get { return this.Equals(Identity); }
        }

        /// <summary>
        /// Conjugates the <see cref="QuaternionD"/> X,Y and Z components. W component is not changed.
        /// </summary>
        public void Conjugate()
        {
            X = -X;
            Y = -Y;
            Z = -Z;
        }

        /// <summary>
        /// Conjugates the <see cref="QuaternionD"/>.
        /// </summary>
        public void Abs()
        {
            X = Math.Abs(X);
            Y = Math.Abs(Y);
            Z = Math.Abs(Z);
            W = Math.Abs(W);
        }

        /// <summary>
        /// Calculates the squared length of the <see cref="QuaternionD"/>.
        /// </summary>
        /// <returns>The squared length of the <see cref="QuaternionD"/>.</returns>
        /// <remarks>
        /// This method may be preferred to <see cref="QuaternionD.Length"/> when only a relative length is needed
        /// and speed is of the essence.
        /// </remarks>
        public double LengthSquared()
        {
            return (X * X) + (Y * Y) + (Z * Z) + (W * W);
        }

        /// <summary>
        /// Creates an array containing the elements of the <see cref="QuaternionD"/>.
        /// </summary>
        /// <returns>A four-element array containing the components of the <see cref="QuaternionD"/>.</returns>
        public double[] ToArray()
        {
            return new double[] { X, Y, Z, W };
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
                var hashCode = X.GetHashCode();
                hashCode = (hashCode * 397) ^ Y.GetHashCode();
                hashCode = (hashCode * 397) ^ Z.GetHashCode();
                hashCode = (hashCode * 397) ^ W.GetHashCode();
                return hashCode;
            }
        }
#endregion

#region Static Methods
        /// <summary>
        /// Multiplies a <see cref="QuaternionD"/> by another.
        /// </summary>
        /// <param name="left">The first QuaternionD to multiply.</param>
        /// <param name="right">The second QuaternionD to multiply.</param>
        /// <param name="result">When the method completes, contains the multiplied QuaternionD.</param>
        public static QuaternionD Multiply(ref QuaternionD left, ref QuaternionD right)
        {
            double lx = left.X;
            double ly = left.Y;
            double lz = left.Z;
            double lw = left.W;
            double rx = right.X;
            double ry = right.Y;
            double rz = right.Z;
            double rw = right.W;
            double a = (ly * rz - lz * ry);
            double b = (lz * rx - lx * rz);
            double c = (lx * ry - ly * rx);
            double d = (lx * rx + ly * ry + lz * rz);

            return new QuaternionD()
            {
                X = (lx * rw + rx * lw) + a,
                Y = (ly * rw + ry * lw) + b,
                Z = (lz * rw + rz * lw) + c,
                W = lw * rw - d
            };
        }
        /// <summary>
        /// Scales a <see cref="QuaternionD"/> by the given value.
        /// </summary>
        /// <param name="value">The quaternion to scale.</param>
        /// <param name="scale">The amount by which to scale the quaternion.</param>
        public static QuaternionD Multiply(ref QuaternionF value, float scale)
        {
            return new QuaternionD()
            {
                X = value.X * scale,
                Y = value.Y * scale,
                Z = value.Z * scale,
                W = value.W * scale,
            };
        }

        /// <summary>
        /// Multiplies two <see cref="QuaternionD"/> together in opposite order.
        /// </summary>
        /// <param name="a">First <see cref="QuaternionD"/> to multiply.</param>
        /// <param name="b">Second <see cref="QuaternionD"/> to multiply.</param>
        public static QuaternionD Concatenate(ref QuaternionD a, ref QuaternionD b)
        {
            double aX = a.X;
            double aY = a.Y;
            double aZ = a.Z;
            double aW = a.W;
            double bX = b.X;
            double bY = b.Y;
            double bZ = b.Z;
            double bW = b.W;

            return new QuaternionD()
            {
                X = aW * bX + aX * bW + aZ * bY - aY * bZ,
                Y = aW * bY + aY * bW + aX * bZ - aZ * bX,
                Z = aW * bZ + aZ * bW + aY * bX - aX * bY,
                W = aW * bW - aX * bX - aY * bY - aZ * bZ
            };
        }

#endregion

#region Indexers
/// <summary>
        /// Gets or sets the component at the specified index.
        /// </summary>
        /// <value>The value of the X, Y, Z, or W component, depending on the index.</value>
        /// <param name="index">The index of the component to access. Use 0 for the X component, 1 for the Y component, 2 for the Z component, and 3 for the W component.</param>
        /// <returns>The value of the component at the specified index.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> is out of the range [0, 3].</exception>
        public double this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return X;
                    case 1: return Y;
                    case 2: return Z;
                    case 3: return W;
                }

                throw new ArgumentOutOfRangeException("index", "Indices for QuaternionD run from 0 to 3, inclusive.");
            }

            set
            {
                switch (index)
                {
                    case 0: X = value; break;
                    case 1: Y = value; break;
                    case 2: Z = value; break;
                    case 3: W = value; break;
                    default: throw new ArgumentOutOfRangeException("index", "Indices for QuaternionD run from 0 to 3, inclusive.");
                }
            }
        }
#endregion
	}
}

