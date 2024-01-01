namespace Molten;

public partial struct Matrix3x2F
{
    /// <summary>
    /// Gets or sets the translation of the matrix; that is M31 and M32.
    /// </summary>
    public Vector2F TranslationVector
    {
        get => new Vector2F(M31, M32);
        set => (M31, M32) = value;
    }

    /// <summary>
    /// Gets or sets the scale of the matrix; that is M11 and M22.
    /// </summary>
    public Vector2F ScaleVector
    {
        get => new Vector2F(M11, M22);
        set => (M11, M22) = value;
    }

    /// <summary>
    /// Scales a matrix by the given value.
    /// </summary>
    /// <param name="left">The matrix to scale.</param>
    /// <param name="right">The amount by which to scale.</param>
    /// <returns>The scaled matrix.</returns>
    public static Matrix3x2F Multiply(Matrix3x2F left, float right)
    {
        Multiply(ref left, right, out Matrix3x2F result);
        return result;
    }

    /// <summary>
    /// Determines the product of two matrices.
    /// </summary>
    /// <param name="left">The first matrix to multiply.</param>
    /// <param name="right">The second matrix to multiply.</param>
    /// <param name="result">The product of the two matrices.</param>
    public static void Multiply(ref Matrix3x2F left, ref Matrix3x2F right, out Matrix3x2F result)
    {
        result.M11 = (left.M11 * right.M11) + (left.M12 * right.M21);
        result.M12 = (left.M11 * right.M12) + (left.M12 * right.M22);
        result.M21 = (left.M21 * right.M11) + (left.M22 * right.M21);
        result.M22 = (left.M21 * right.M12) + (left.M22 * right.M22);
        result.M31 = (left.M31 * right.M11) + (left.M32 * right.M21) + right.M31;
        result.M32 = (left.M31 * right.M12) + (left.M32 * right.M22) + right.M32;
    }

            /// <summary>
    /// Creates a matrix that scales along the x-axis and y-axis.
    /// </summary>
    /// <param name="scale">Scaling factor for both axes.</param>
    /// <param name="result">When the method completes, contains the created scaling matrix.</param>
    public static void Scaling(ref Vector2F scale, out Matrix3x2F result)
    {
        Scaling(scale.X, scale.Y, out result);
    }

    /// <summary>
    /// Creates a matrix that scales along the x-axis and y-axis.
    /// </summary>
    /// <param name="scale">Scaling factor for both axes.</param>
    /// <returns>The created scaling matrix.</returns>
    public static Matrix3x2F Scaling(Vector2F scale)
    {
        Scaling(ref scale, out Matrix3x2F result);
        return result;
    }

    /// <summary>
    /// Creates a matrix that scales along the x-axis and y-axis.
    /// </summary>
    /// <param name="x">Scaling factor that is applied along the x-axis.</param>
    /// <param name="y">Scaling factor that is applied along the y-axis.</param>
    /// <param name="result">When the method completes, contains the created scaling matrix.</param>
    public static void Scaling(float x, float y, out Matrix3x2F result)
    {
        result = Matrix3x2F.Identity;
        result.M11 = x;
        result.M22 = y;
    }

    /// <summary>
    /// Creates a matrix that scales along the x-axis and y-axis.
    /// </summary>
    /// <param name="x">Scaling factor that is applied along the x-axis.</param>
    /// <param name="y">Scaling factor that is applied along the y-axis.</param>
    /// <returns>The created scaling matrix.</returns>
    public static Matrix3x2F Scaling(float x, float y)
    {
        Scaling(x, y, out Matrix3x2F result);
        return result;
    }

    /// <summary>
    /// Creates a matrix that uniformly scales along both axes.
    /// </summary>
    /// <param name="scale">The uniform scale that is applied along both axes.</param>
    /// <param name="result">When the method completes, contains the created scaling matrix.</param>
    public static void Scaling(float scale, out Matrix3x2F result)
    {
        result = Matrix3x2F.Identity;
        result.M11 = result.M22 = scale;
    }

    /// <summary>
    /// Creates a matrix that uniformly scales along both axes.
    /// </summary>
    /// <param name="scale">The uniform scale that is applied along both axes.</param>
    /// <returns>The created scaling matrix.</returns>
    public static Matrix3x2F Scaling(float scale)
    {
        Scaling(scale, out Matrix3x2F result);
        return result;
    }

    /// <summary>
    /// Creates a matrix that is scaling from a specified center.
    /// </summary>
    /// <param name="x">Scaling factor that is applied along the x-axis.</param>
    /// <param name="y">Scaling factor that is applied along the y-axis.</param>
    /// <param name="center">The center of the scaling.</param>
    /// <returns>The created scaling matrix.</returns>
    public static Matrix3x2F Scaling(float x, float y, Vector2F center)
    {
        Matrix3x2F result;
        result.M11 = x;     
        result.M12 = 0F;
        result.M21 = 0F;  
        result.M22 = y;

        result.M31 = center.X - (x * center.X);
        result.M32 = center.Y - (y * center.Y);

        return result;
    }

    /// <summary>
    /// Creates a matrix that is scaling from a specified center.
    /// </summary>
    /// <param name="x">Scaling factor that is applied along the x-axis.</param>
    /// <param name="y">Scaling factor that is applied along the y-axis.</param>
    /// <param name="center">The center of the scaling.</param>
    /// <param name="result">The created scaling matrix.</param>
    public static void Scaling( float x, float y, ref Vector2F center, out Matrix3x2F result)
    {
        Matrix3x2F localResult;
        localResult.M11 = x;     
        localResult.M12 = 0F;
        localResult.M21 = 0F;  
        localResult.M22 = y;

        localResult.M31 = center.X - (x * center.X);
        localResult.M32 = center.Y - (y * center.Y);

        result = localResult;
    }

    /// <summary>
    /// Calculates the determinant of this matrix.
    /// </summary>
    /// <returns>Result of the determinant.</returns>
    public float Determinant()
    {
        return (M11 * M22) - (M12 * M21);
    }

    /// <summary>
    /// Creates a matrix that rotates.
    /// </summary>
    /// <param name="angle">Angle of rotation in radians. Angles are measured clockwise when looking along the rotation axis.</param>
    /// <param name="result">When the method completes, contains the created rotation matrix.</param>
    public static void Rotation(float angle, out Matrix3x2F result)
    {
        float cos = MathF.Cos(angle);
        float sin = MathF.Sin(angle);

        result = Identity;
        result.M11 = cos;
        result.M12 = sin;
        result.M21 = -sin;
        result.M22 = cos;
    }

    /// <summary>
    /// Creates a matrix that rotates.
    /// </summary>
    /// <param name="angle">Angle of rotation in radians. Angles are measured clockwise when looking along the rotation axis.</param>
    /// <returns>The created rotation matrix.</returns>
    public static Matrix3x2F Rotation(float angle)
    {
        Rotation(angle, out Matrix3x2F result);
        return result;
    }

    /// <summary>
    /// Creates a matrix that rotates about a specified center.
    /// </summary>
    /// <param name="angle">Angle of rotation in radians. Angles are measured clockwise when looking along the rotation axis.</param>
    /// <param name="center">The center of the rotation.</param>
    /// <returns>The created rotation matrix.</returns>
    public static Matrix3x2F Rotation(float angle, Vector2F center)
    {
        Rotation(angle, center, out Matrix3x2F result);
        return result;
    }

    /// <summary>
    /// Creates a matrix that rotates about a specified center.
    /// </summary>
    /// <param name="angle">Angle of rotation in radians. Angles are measured clockwise when looking along the rotation axis.</param>
    /// <param name="center">The center of the rotation.</param>
    /// <param name="result">When the method completes, contains the created rotation matrix.</param>
    public static void Rotation(float angle, Vector2F center, out Matrix3x2F result)
    {
        result = Translation(-center) * Rotation(angle) * Translation(center);
    }

    /// <summary>
    /// Creates a transformation matrix.
    /// </summary>
    /// <param name="xScale">Scaling factor that is applied along the x-axis.</param>
    /// <param name="yScale">Scaling factor that is applied along the y-axis.</param>
    /// <param name="angle">Angle of rotation in radians. Angles are measured clockwise when looking along the rotation axis.</param>
    /// <param name="xOffset">X-coordinate offset.</param>
    /// <param name="yOffset">Y-coordinate offset.</param>
    /// <param name="result">When the method completes, contains the created transformation matrix.</param>
    public static void Transformation(float xScale, float yScale, float angle, float xOffset, float yOffset, out Matrix3x2F result)
    {
        result = Scaling(xScale, yScale) * Rotation(angle) * Translation(xOffset, yOffset);
    }

    /// <summary>
    /// Creates a transformation matrix.
    /// </summary>
    /// <param name="xScale">Scaling factor that is applied along the x-axis.</param>
    /// <param name="yScale">Scaling factor that is applied along the y-axis.</param>
    /// <param name="angle">Angle of rotation in radians.</param>
    /// <param name="xOffset">X-coordinate offset.</param>
    /// <param name="yOffset">Y-coordinate offset.</param>
    /// <returns>The created transformation matrix.</returns>
    public static Matrix3x2F Transformation(float xScale, float yScale, float angle, float xOffset, float yOffset)
    {
        Matrix3x2F result;
        Transformation(xScale, yScale, angle, xOffset, yOffset, out result);
        return result;
    }

    /// <summary>
    /// Creates a translation matrix using the specified offsets.
    /// </summary>
    /// <param name="value">The offset for both coordinate planes.</param>
    /// <param name="result">When the method completes, contains the created translation matrix.</param>
    public static void Translation(ref Vector2F value, out Matrix3x2F result)
    {
        Translation(value.X, value.Y, out result);
    }

    /// <summary>
    /// Creates a translation matrix using the specified offsets.
    /// </summary>
    /// <param name="value">The offset for both coordinate planes.</param>
    /// <returns>The created translation matrix.</returns>
    public static Matrix3x2F Translation(Vector2F value)
    {
        Matrix3x2F result;
        Translation(ref value, out result);
        return result;
    }

    /// <summary>
    /// Creates a translation matrix using the specified offsets.
    /// </summary>
    /// <param name="x">X-coordinate offset.</param>
    /// <param name="y">Y-coordinate offset.</param>
    /// <param name="result">When the method completes, contains the created translation matrix.</param>
    public static void Translation(float x, float y, out Matrix3x2F result)
    {
        result = Matrix3x2F.Identity;
        result.M31 = x;
        result.M32 = y;
    }

    /// <summary>
    /// Creates a translation matrix using the specified offsets.
    /// </summary>
    /// <param name="x">X-coordinate offset.</param>
    /// <param name="y">Y-coordinate offset.</param>
    /// <returns>The created translation matrix.</returns>
    public static Matrix3x2F Translation(float x, float y)
    {
        Translation(x, y, out Matrix3x2F result);
        return result;
    }

    /// <summary>
    /// Transforms a vector by this matrix.
    /// </summary>
    /// <param name="matrix">The matrix to use as a transformation matrix.</param>
    /// <param name="point">The original vector to apply the transformation.</param>
    /// <returns>The result of the transformation for the input vector.</returns>
    public static Vector2F TransformPoint(Matrix3x2F matrix, Vector2F point)
    {
        Vector2F result;
        result.X = (point.X * matrix.M11) + (point.Y * matrix.M21) + matrix.M31;
        result.Y = (point.X * matrix.M12) + (point.Y * matrix.M22) + matrix.M32;
        return result;
    }

    /// <summary>
    /// Transforms a vector by this matrix.
    /// </summary>
    /// <param name="matrix">The matrix to use as a transformation matrix.</param>
    /// <param name="point">The original vector to apply the transformation.</param>
    /// <param name="result">The result of the transformation for the input vector.</param>
    /// <returns></returns>
    public static void TransformPoint(ref Matrix3x2F matrix, ref Vector2F point, out Vector2F result)
    {
        Vector2F localResult;
        localResult.X = (point.X * matrix.M11) + (point.Y * matrix.M21) + matrix.M31;
        localResult.Y = (point.X * matrix.M12) + (point.Y * matrix.M22) + matrix.M32;
        result = localResult;
    }

    /// <summary>
    /// Calculates the inverse of this matrix instance.
    /// </summary>
    public void Invert()
    {
        Invert(ref this, out this);
    }

    /// <summary>
    /// Calculates the inverse of the specified matrix.
    /// </summary>
    /// <param name="value">The matrix whose inverse is to be calculated.</param>
    /// <returns>the inverse of the specified matrix.</returns>
    public static Matrix3x2F Invert(Matrix3x2F value)
    {
        Invert(ref value, out Matrix3x2F result);
        return result;
    }

    /// <summary>
    /// Creates a skew matrix.
    /// </summary>
    /// <param name="angleX">Angle of skew along the X-axis in radians.</param>
    /// <param name="angleY">Angle of skew along the Y-axis in radians.</param>
    /// <returns>The created skew matrix.</returns>
    public static Matrix3x2F Skew(float angleX, float angleY)
    {
        Skew(angleX, angleY, out Matrix3x2F result);
        return result;
    }

    /// <summary>
    /// Creates a skew matrix.
    /// </summary>
    /// <param name="angleX">Angle of skew along the X-axis in radians.</param>
    /// <param name="angleY">Angle of skew along the Y-axis in radians.</param>
    /// <param name="result">When the method completes, contains the created skew matrix.</param>
    public static void Skew(float angleX, float angleY, out Matrix3x2F result)
    {
        result = Identity;
        result.M12 = (float) Math.Tan(angleX);
        result.M21 = (float) Math.Tan(angleY);
    }

    /// <summary>
    /// Calculates the inverse of the specified matrix.
    /// </summary>
    /// <param name="value">The matrix whose inverse is to be calculated.</param>
    /// <param name="result">When the method completes, contains the inverse of the specified matrix.</param>
    public static void Invert(ref Matrix3x2F value, out Matrix3x2F result)
    {
        float determinant = value.Determinant();

        if (MathHelper.IsZero(determinant))
        {
            result = Identity;
            return;
        }

        float invdet = 1F / determinant;
        float _offsetX = value.M31;
        float _offsetY = value.M32;

        result = new Matrix3x2F(
            value.M22 * invdet,
            -value.M12 * invdet,
            -value.M21 * invdet,
            value.M11 * invdet,
            (value.M21 * _offsetY - _offsetX * value.M22) * invdet,
            (_offsetX * value.M12 - value.M11 * _offsetY) * invdet);
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="Matrix4F"/> to <see cref="Matrix3x2F"/>.
    /// </summary>
    /// <param name="matrix">The matrix.</param>
    /// <returns>The result of the conversion.</returns>
    public static implicit operator Matrix3x2F(Matrix4F matrix)
    {
        return new Matrix3x2F
        {
            M11 = matrix.M11,
            M12 = matrix.M12,
            M21 = matrix.M21,
            M22 = matrix.M22,
            M31 = matrix.M41,
            M32 = matrix.M42
        };
    }

    /// <summary>
    /// Multiplies two matrices.
    /// </summary>
    /// <param name="left">The first matrix to multiply.</param>
    /// <param name="right">The second matrix to multiply.</param>
    /// <returns>The product of the two matrices.</returns>
    public static Matrix3x2F operator *(Matrix3x2F left, Matrix3x2F right)
    {
        Multiply(ref left, ref right, out Matrix3x2F result);
        return result;
    }
}

