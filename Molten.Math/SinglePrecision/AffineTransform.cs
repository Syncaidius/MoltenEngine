

namespace Molten;

///<summary>
/// A transformation composed of a linear transformation and a translation.
///</summary>
public struct AffineTransform
{
    ///<summary>
    /// Translation in the affine transform.
    ///</summary>
    public Vector3F Translation;
    /// <summary>
    /// Linear transform in the affine transform.
    /// </summary>
    public Matrix3F LinearTransform;

    ///<summary>
    /// Constructs a new affine transform.
    ///</summary>
    ///<param name="translation">Translation to use in the transform.</param>
    public AffineTransform(ref Vector3F translation)
    {
        LinearTransform = Matrix3F.Identity;
        Translation = translation;
    }

    ///<summary>
    /// Constructs a new affine transform.
    ///</summary>
    ///<param name="translation">Translation to use in the transform.</param>
    public AffineTransform(Vector3F translation)
        : this(ref translation)
    {
    }

    ///<summary>
    /// Constructs a new affine tranform.
    ///</summary>
    ///<param name="orientation">Orientation to use as the linear transform.</param>
    ///<param name="translation">Translation to use in the transform.</param>
    public AffineTransform(ref QuaternionF orientation, ref Vector3F translation)
    {
        Matrix3F.FromQuaternion(ref orientation, out LinearTransform);
        Translation = translation;
    }

    ///<summary>
    /// Constructs a new affine tranform.
    ///</summary>
    ///<param name="orientation">Orientation to use as the linear transform.</param>
    ///<param name="translation">Translation to use in the transform.</param>
    public AffineTransform(QuaternionF orientation, Vector3F translation)
        : this(ref orientation, ref translation)
    {
    }

    ///<summary>
    /// Constructs a new affine transform.
    ///</summary>
    ///<param name="scaling">Scaling to apply in the linear transform.</param>
    ///<param name="orientation">Orientation to apply in the linear transform.</param>
    ///<param name="translation">Translation to apply.</param>
    public AffineTransform(ref Vector3F scaling, ref QuaternionF orientation, ref Vector3F translation)
    {
        //Create an SRT transform.
        Matrix3F.CreateScale(ref scaling, out LinearTransform);
        Matrix3F rotation;
        Matrix3F.FromQuaternion(ref orientation, out rotation);
        Matrix3F.Multiply(ref LinearTransform, ref rotation, out LinearTransform);
        Translation = translation;
    }

    ///<summary>
    /// Constructs a new affine transform.
    ///</summary>
    ///<param name="scaling">Scaling to apply in the linear transform.</param>
    ///<param name="orientation">Orientation to apply in the linear transform.</param>
    ///<param name="translation">Translation to apply.</param>
    public AffineTransform(Vector3F scaling, QuaternionF orientation, Vector3F translation)
        : this(ref scaling, ref orientation, ref translation)
    {
    }

    ///<summary>
    /// Constructs a new affine transform.
    ///</summary>
    ///<param name="linearTransform">The linear transform component.</param>
    ///<param name="translation">Translation component of the transform.</param>
    public AffineTransform(ref Matrix3F linearTransform, ref Vector3F translation)
    {
        LinearTransform = linearTransform;
        Translation = translation;

    }

    ///<summary>
    /// Constructs a new affine transform.
    ///</summary>
    ///<param name="linearTransform">The linear transform component.</param>
    ///<param name="translation">Translation component of the transform.</param>
    public AffineTransform(Matrix3F linearTransform, Vector3F translation)
        : this(ref linearTransform, ref translation)
    {
    }

    ///<summary>
    /// Gets or sets the 4x4 matrix representation of the affine transform.
    /// The linear transform is the upper left 3x3 part of the 4x4 matrix.
    /// The translation is included in the matrix's Translation property.
    ///</summary>
    public Matrix4F Matrix
    {
        get
        {
            Matrix4F toReturn;
            Matrix3F.To4x4(ref LinearTransform, out toReturn);
            toReturn.Translation = Translation;
            return toReturn;
        }
        set
        {
            Matrix3F.From4x4(ref value, out LinearTransform);
            Translation = value.Translation;
        }
    }


    ///<summary>
    /// Gets the identity affine transform.
    ///</summary>
    public static AffineTransform Identity
    {
        get
        {
            var t = new AffineTransform { LinearTransform = Matrix3F.Identity, Translation = new Vector3F() };
            return t;
        }
    }

    ///<summary>
    /// Transforms a vector by an affine transform.
    ///</summary>
    ///<param name="position">Position to transform.</param>
    ///<param name="transform">Transform to apply.</param>
    ///<param name="transformed">Transformed position.</param>
    public static void Transform(ref Vector3F position, ref AffineTransform transform, out Vector3F transformed)
    {
        Vector3F.Transform(ref position, ref transform.LinearTransform, out transformed);
        Vector3F.Add(ref transformed, ref transform.Translation, out transformed);
    }

    ///<summary>
    /// Transforms a vector by an affine transform's inverse.
    ///</summary>
    ///<param name="position">Position to transform.</param>
    ///<param name="transform">Transform to invert and apply.</param>
    ///<param name="transformed">Transformed position.</param>
    public static void TransformInverse(ref Vector3F position, ref AffineTransform transform, out Vector3F transformed)
    {
        Vector3F.Subtract(ref position, ref transform.Translation, out transformed);
        Matrix3F inverse;
        Matrix3F.Invert(ref transform.LinearTransform, out inverse);
        Matrix3F.TransformTranspose(ref transformed, ref inverse, out transformed);
    }

    ///<summary>
    /// Inverts an affine transform.
    ///</summary>
    ///<param name="transform">Transform to invert.</param>
    /// <param name="inverse">Inverse of the transform.</param>
    public static void Invert(ref AffineTransform transform, out AffineTransform inverse)
    {
        Matrix3F.Invert(ref transform.LinearTransform, out inverse.LinearTransform);
        Vector3F.Transform(ref transform.Translation, ref inverse.LinearTransform, out inverse.Translation);
        Vector3F.Negate(ref inverse.Translation, out inverse.Translation);
    }

    /// <summary>
    /// Multiplies a transform by another transform.
    /// </summary>
    /// <param name="a">First transform.</param>
    /// <param name="b">Second transform.</param>
    /// <param name="transform">Combined transform.</param>
    public static void Multiply(ref AffineTransform a, ref AffineTransform b, out AffineTransform transform)
    {
        Matrix3F linearTransform;//Have to use temporary variable just in case a or b reference is transform.
        Matrix3F.Multiply(ref a.LinearTransform, ref b.LinearTransform, out linearTransform);
        Vector3F translation;
        Vector3F.Transform(ref a.Translation, ref b.LinearTransform, out translation);
        Vector3F.Add(ref translation, ref b.Translation, out transform.Translation);
        transform.LinearTransform = linearTransform;
    }

    ///<summary>
    /// Multiplies a rigid transform by an affine transform.
    ///</summary>
    ///<param name="a">Rigid transform.</param>
    ///<param name="b">Affine transform.</param>
    ///<param name="transform">Combined transform.</param>
    public static void Multiply(ref RigidTransform a, ref AffineTransform b, out AffineTransform transform)
    {
        Matrix3F linearTransform;//Have to use temporary variable just in case b reference is transform.
        Matrix3F.FromQuaternion(ref a.Orientation, out linearTransform);
        Matrix3F.Multiply(ref linearTransform, ref b.LinearTransform, out linearTransform);
        Vector3F translation;
        Vector3F.Transform(ref a.Position, ref b.LinearTransform, out translation);
        Vector3F.Add(ref translation, ref b.Translation, out transform.Translation);
        transform.LinearTransform = linearTransform;
    }


    ///<summary>
    /// Transforms a vector using an affine transform.
    ///</summary>
    ///<param name="position">Position to transform.</param>
    ///<param name="affineTransform">Transform to apply.</param>
    ///<returns>Transformed position.</returns>
    public static Vector3F Transform(Vector3F position, AffineTransform affineTransform)
    {
        Vector3F toReturn;
        Transform(ref position, ref affineTransform, out toReturn);
        return toReturn;
    }

    /// <summary>
    /// Creates an affine transform from a rigid transform.
    /// </summary>
    /// <param name="rigid">Rigid transform to base the affine transform on.</param>
    /// <param name="affine">Affine transform created from the rigid transform.</param>
    public static void CreateFromRigidTransform(ref RigidTransform rigid, out AffineTransform affine)
    {
        affine.Translation = rigid.Position;
        Matrix3F.FromQuaternion(ref rigid.Orientation, out affine.LinearTransform);
    }

    /// <summary>
    /// Creates an affine transform from a rigid transform.
    /// </summary>
    /// <param name="rigid">Rigid transform to base the affine transform on.</param>
    /// <returns>Affine transform created from the rigid transform.</returns>
    public static AffineTransform CreateFromRigidTransform(RigidTransform rigid)
    {
        AffineTransform toReturn;
        toReturn.Translation = rigid.Position;
        Matrix3F.FromQuaternion(ref rigid.Orientation, out toReturn.LinearTransform);
        return toReturn;
    }

}