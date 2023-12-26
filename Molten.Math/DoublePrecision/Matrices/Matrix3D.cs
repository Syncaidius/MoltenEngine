using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Molten.DoublePrecision
{
    /// <summary>Represents a double-precision 4x4 Matrix. Contains only scale and rotation.</summary>
    [StructLayout(LayoutKind.Explicit)]
    [DataContract]
	public partial struct Matrix3D : IEquatable<Matrix3D>, IFormattable, ITransposableMatrix<Matrix3D>, IMatrix<double>
    {
        /// <summary>
        /// A single-precision <see cref="Matrix3D"/> with values intialized to the identity of a 2 x 2 matrix
        /// </summary>
        public static readonly Matrix3D Identity = new Matrix3D() 
        { 
            M11 = 1D, 
            M22 = 1D, 
            M33 = 1D, 
            M44 = 1D, 
        };

        public static readonly int ComponentCount = 9;

        public static readonly int RowCount = rowCount;

        public static readonly int ColumnCount = colCount;

        /// <summary>A <see cref="Matrix3D"/> will all of its components set to 0D.</summary>
        public static readonly Matrix3D Zero = new Matrix3D();

        /// <summary> Gets a value indicating whether this instance is an identity matrix. </summary>
        /// <value>
        /// <c>true</c> if this instance is an identity matrix; otherwise, <c>false</c>.
        /// </value>
        public bool IsIdentity => Equals(Identity);

    }
}

