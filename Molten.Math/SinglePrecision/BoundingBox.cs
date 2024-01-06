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

namespace Molten;

/// <summary>
/// Represents an axis-aligned bounding box in three dimensional space.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 4)]
[Serializable]
public struct BoundingBox : IEquatable<BoundingBox>, IFormattable
{
    /// <summary>
    /// The minimum point of the box.
    /// </summary>
    [DataMember]
    public Vector3F Min;

    /// <summary>
    /// The maximum point of the box.
    /// </summary>
    [DataMember]
    public Vector3F Max;

    /// <summary>
    /// Initializes a new instance of the <see cref="BoundingBox"/> struct.
    /// </summary>
    /// <param name="minimum">The minimum vertex of the bounding box.</param>
    /// <param name="maximum">The maximum vertex of the bounding box.</param>
    public BoundingBox(Vector3F minimum, Vector3F maximum)
    {
        this.Min = minimum;
        this.Max = maximum;
    }

    /// <summary>
    /// Retrieves the eight corners of the bounding box.
    /// </summary>
    /// <returns>An array of points representing the eight corners of the bounding box.</returns>
    public Vector3F[] GetCorners()
    {
        Vector3F[] results = new Vector3F[8];
        GetCorners(results);
        return results;
    }

    /// <summary>
    /// Retrieves the eight corners of the bounding box.
    /// </summary>
    /// <returns>An array of points representing the eight corners of the bounding box.</returns>
    public void GetCorners(Vector3F[] corners)
    {
        corners[0] = new Vector3F(Min.X, Max.Y, Max.Z);
        corners[1] = new Vector3F(Max.X, Max.Y, Max.Z);
        corners[2] = new Vector3F(Max.X, Min.Y, Max.Z);
        corners[3] = new Vector3F(Min.X, Min.Y, Max.Z);
        corners[4] = new Vector3F(Min.X, Max.Y, Min.Z);
        corners[5] = new Vector3F(Max.X, Max.Y, Min.Z);
        corners[6] = new Vector3F(Max.X, Min.Y, Min.Z);
        corners[7] = new Vector3F(Min.X, Min.Y, Min.Z);
    }

    /// <summary>
    /// Computes the bounding box of three points.
    /// </summary>
    /// <param name="a">First vertex of the triangle.</param>
    /// <param name="b">Second vertex of the triangle.</param>
    /// <param name="c">Third vertex of the triangle.</param>
    /// <param name="aabb">Bounding box of the triangle.</param>
    public static void FromTriangle(ref Vector3F a, ref Vector3F b, ref Vector3F c, out BoundingBox aabb)
    {
#if !WINDOWS
        aabb = new BoundingBox();
#endif
        //X axis
        if (a.X > b.X && a.X > c.X)
        {
            //A is max
            aabb.Max.X = a.X;
            aabb.Min.X = b.X > c.X ? c.X : b.X;
        }
        else if (b.X > c.X)
        {
            //B is max
            aabb.Max.X = b.X;
            aabb.Min.X = a.X > c.X ? c.X : a.X;
        }
        else
        {
            //C is max
            aabb.Max.X = c.X;
            aabb.Min.X = a.X > b.X ? b.X : a.X;
        }
        //Y axis
        if (a.Y > b.Y && a.Y > c.Y)
        {
            //A is max
            aabb.Max.Y = a.Y;
            aabb.Min.Y = b.Y > c.Y ? c.Y : b.Y;
        }
        else if (b.Y > c.Y)
        {
            //B is max
            aabb.Max.Y = b.Y;
            aabb.Min.Y = a.Y > c.Y ? c.Y : a.Y;
        }
        else
        {
            //C is max
            aabb.Max.Y = c.Y;
            aabb.Min.Y = a.Y > b.Y ? b.Y : a.Y;
        }
        //Z axis
        if (a.Z > b.Z && a.Z > c.Z)
        {
            //A is max
            aabb.Max.Z = a.Z;
            aabb.Min.Z = b.Z > c.Z ? c.Z : b.Z;
        }
        else if (b.Z > c.Z)
        {
            //B is max
            aabb.Max.Z = b.Z;
            aabb.Min.Z = a.Z > c.Z ? c.Z : a.Z;
        }
        else
        {
            //C is max
            aabb.Max.Z = c.Z;
            aabb.Min.Z = a.Z > b.Z ? b.Z : a.Z;
        }
    }

    /// <summary>
    /// Computes the bounding box of three points (a triangle).
    /// </summary>
    /// <param name="a">First vertex of the triangle.</param>
    /// <param name="b">Second vertex of the triangle.</param>
    /// <param name="c">Third vertex of the triangle.</param>
    public static BoundingBox FromTriangle(ref Vector3F a, ref Vector3F b, ref Vector3F c)
    {
        BoundingBox r;
        FromTriangle(ref a, ref b, ref c, out r);
        return r;
    }

    /// <summary>
    /// Determines if there is an intersection between the current object and a <see cref="Ray"/>.
    /// </summary>
    /// <param name="ray">The ray to test.</param>
    /// <returns>Whether the two objects intersected.</returns>
    public bool Intersects(ref Ray ray)
    {
        float distance;
        return CollisionHelper.RayIntersectsBox(ref ray, ref this, out distance);
    }

    /// <summary>
    /// Determines if there is an intersection between the current object and a <see cref="Ray"/>.
    /// </summary>
    /// <param name="ray">The ray to test.</param>
    /// <param name="distance">When the method completes, contains the distance of the intersection,
    /// or 0 if there was no intersection.</param>
    /// <returns>Whether the two objects intersected.</returns>
    public bool Intersects(ref Ray ray, out float distance)
    {
        return CollisionHelper.RayIntersectsBox(ref ray, ref this, out distance);
    }

    /// <summary>
    /// Determines if there is an intersection between the current object and a <see cref="Ray"/>.
    /// </summary>
    /// <param name="ray">The ray to test.</param>
    /// <param name="point">When the method completes, contains the point of intersection,
    /// or <see cref="Vector3F.Zero"/> if there was no intersection.</param>
    /// <returns>Whether the two objects intersected.</returns>
    public bool Intersects(ref Ray ray, out Vector3F point)
    {
        return CollisionHelper.RayIntersectsBox(ref ray, ref this, out point);
    }

    /// <summary>
    /// Determines if there is an intersection between the current object and a <see cref="Plane"/>.
    /// </summary>
    /// <param name="plane">The plane to test.</param>
    /// <returns>Whether the two objects intersected.</returns>
    public PlaneIntersectionType Intersects(ref Plane plane)
    {
        return CollisionHelper.PlaneIntersectsBox(ref plane, ref this);
    }

    /* This implementation is wrong
    /// <summary>
    /// Determines if there is an intersection between the current object and a triangle.
    /// </summary>
    /// <param name="vertex1">The first vertex of the triangle to test.</param>
    /// <param name="vertex2">The second vertex of the triangle to test.</param>
    /// <param name="vertex3">The third vertex of the triangle to test.</param>
    /// <returns>Whether the two objects intersected.</returns>
    public bool Intersects(ref Vector3 vertex1, ref Vector3 vertex2, ref Vector3 vertex3)
    {
        return Collision.BoxIntersectsTriangle(ref this, ref vertex1, ref vertex2, ref vertex3);
    }
    */

    /// <summary>
    /// Determines if there is an intersection between the current object and a <see cref="BoundingBox"/>.
    /// </summary>
    /// <param name="box">The box to test.</param>
    /// <returns>Whether the two objects intersected.</returns>
    public bool Intersects(ref BoundingBox box)
    {
        return CollisionHelper.BoxIntersectsBox(ref this, ref box);
    }

    /// <summary>
    /// Determines if there is an intersection between the current object and a <see cref="BoundingBox"/>.
    /// </summary>
    /// <param name="box">The box to test.</param>
    /// <returns>Whether the two objects intersected.</returns>
    public void Intersects(ref BoundingBox box, out bool result)
    {
        result = CollisionHelper.BoxIntersectsBox(ref this, ref box);
    }

    /// <summary>
    /// Determines if there is an intersection between the current object and a <see cref="BoundingBox"/>.
    /// </summary>
    /// <param name="box">The box to test.</param>
    /// <returns>Whether the two objects intersected.</returns>
    public bool Intersects(BoundingBox box)
    {
        return Intersects(ref box);
    }

    /// <summary>
    /// Determines if there is an intersection between the current object and a <see cref="BoundingSphere"/>.
    /// </summary>
    /// <param name="sphere">The sphere to test.</param>
    /// <returns>Whether the two objects intersected.</returns>
    public bool Intersects(ref BoundingSphere sphere)
    {
        return CollisionHelper.BoxIntersectsSphere(ref this, ref sphere);
    }

    /// <summary>
    /// Determines if there is an intersection between the current object and a <see cref="BoundingSphere"/>.
    /// </summary>
    /// <param name="sphere">The sphere to test.</param>
    /// <returns>Whether the two objects intersected.</returns>
    public void Intersects(ref BoundingSphere sphere, out bool result)
    {
        result = CollisionHelper.BoxIntersectsSphere(ref this, ref sphere);
    }

    /// <summary>
    /// Determines if there is an intersection between the current object and a <see cref="BoundingSphere"/>.
    /// </summary>
    /// <param name="sphere">The sphere to test.</param>
    /// <returns>Whether the two objects intersected.</returns>
    public bool Intersects(BoundingSphere sphere)
    {
        return Intersects(ref sphere);
    }

    /// <summary>
    /// Determines whether the current objects contains a point.
    /// </summary>
    /// <param name="point">The point to test.</param>
    /// <returns>The type of containment the two objects have.</returns>
    public ContainmentType Contains(ref Vector3F point)
    {
        return CollisionHelper.BoxContainsPoint(ref this, ref point);
    }

    /// <summary>
    /// Determines whether the current objects contains a point.
    /// </summary>
    /// <param name="point">The point to test.</param>
    /// <returns>The type of containment the two objects have.</returns>
    public ContainmentType Contains(Vector3F point)
    {
        return Contains(ref point);
    }

    /* This implementation is wrong
    /// <summary>
    /// Determines whether the current objects contains a triangle.
    /// </summary>
    /// <param name="vertex1">The first vertex of the triangle to test.</param>
    /// <param name="vertex2">The second vertex of the triangle to test.</param>
    /// <param name="vertex3">The third vertex of the triangle to test.</param>
    /// <returns>The type of containment the two objects have.</returns>
    public ContainmentType Contains(ref Vector3 vertex1, ref Vector3 vertex2, ref Vector3 vertex3)
    {
        return Collision.BoxContainsTriangle(ref this, ref vertex1, ref vertex2, ref vertex3);
    }
    */

    /// <summary>
    /// Determines whether the current objects contains a <see cref="BoundingBox"/>.
    /// </summary>
    /// <param name="box">The box to test.</param>
    /// <returns>The type of containment the two objects have.</returns>
    public ContainmentType Contains(ref BoundingBox box)
    {
        return CollisionHelper.BoxContainsBox(ref this, ref box);
    }

    /// <summary>
    /// Determines whether the current objects contains a <see cref="BoundingBox"/>.
    /// </summary>
    /// <param name="box">The box to test.</param>
    /// <returns>The type of containment the two objects have.</returns>
    public ContainmentType Contains(BoundingBox box)
    {
        return Contains(ref box);
    }

    /// <summary>
    /// Determines whether the current objects contains a <see cref="BoundingSphere"/>.
    /// </summary>
    /// <param name="sphere">The sphere to test.</param>
    /// <returns>The type of containment the two objects have.</returns>
    public ContainmentType Contains(ref BoundingSphere sphere)
    {
        return CollisionHelper.BoxContainsSphere(ref this, ref sphere);
    }

    /// <summary>
    /// Determines whether the current objects contains a <see cref="BoundingSphere"/>.
    /// </summary>
    /// <param name="sphere">The sphere to test.</param>
    /// <returns>The type of containment the two objects have.</returns>
    public ContainmentType Contains(BoundingSphere sphere)
    {
        return Contains(ref sphere);
    }

    /// <summary>
    /// Constructs a <see cref="BoundingBox"/> that fully contains the given points.
    /// </summary>
    /// <param name="points">The points that will be contained by the box.</param>
    /// <param name="result">When the method completes, contains the newly constructed bounding box.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="points"/> is <c>null</c>.</exception>
    public static void FromPoints(Vector3F[] points, out BoundingBox result)
    {
        if (points == null)
            throw new ArgumentNullException("points");

        Vector3F min = new Vector3F(float.MaxValue);
        Vector3F max = new Vector3F(float.MinValue);

        for (int i = 0; i < points.Length; ++i)
        {
            min = Vector3F.Min(ref min, ref points[i]);
            max = Vector3F.Max(ref max, ref points[i]);
        }

        result = new BoundingBox(min, max);
    }

    /// <summary>
    /// Creates the smallest possible bounding box that contains a list of points.
    /// </summary>
    /// <param name="points">Points to enclose with a bounding box.</param>
    /// <returns>Bounding box which contains the list of points.</returns>
    public static BoundingBox FromPoints(IList<Vector3F> points)
    {
        BoundingBox aabb;
        if (points.Count == 0)
            throw new Exception("Cannot construct a bounding box from an empty list.");
        aabb.Min = points[0];
        aabb.Max = aabb.Min;
        for (int i = points.Count - 1; i >= 1; i--)
        {
            Vector3F v = points[i];
            if (v.X < aabb.Min.X)
                aabb.Min.X = v.X;
            else if (v.X > aabb.Max.X)
                aabb.Max.X = v.X;

            if (v.Y < aabb.Min.Y)
                aabb.Min.Y = v.Y;
            else if (v.Y > aabb.Max.Y)
                aabb.Max.Y = v.Y;

            if (v.Z < aabb.Min.Z)
                aabb.Min.Z = v.Z;
            else if (v.Z > aabb.Max.Z)
                aabb.Max.Z = v.Z;
        }
        return aabb;
    }

    /// <summary>Returns a new bounding box which has been expanded in all directions by the provided amount.</summary>
    /// <param name="box">The source bounding box.</param>
    /// <param name="amount">The amount of expand the source box.</param>
    /// <returns></returns>
    public static BoundingBox Expand(BoundingBox box, float amount)
    {
        return new BoundingBox()
        {
            Min = box.Min - amount,
            Max = box.Max + amount,
        };
    }

    /// <summary>
    /// Expands a bounding box by the given sweep. expands either the minimum or maximum values based on the sweep vector.
    /// </summary>
    /// <param name="boundingBox">Bounding box to expand.</param>
    /// <param name="sweep">Sweep to expand the bounding box with.</param>
    public static void SweepExpand(ref BoundingBox boundingBox, ref Vector3F sweep)
    {
        if (sweep.X > 0)
            boundingBox.Max.X += sweep.X;
        else
            boundingBox.Min.X += sweep.X;

        if (sweep.Y > 0)
            boundingBox.Max.Y += sweep.Y;
        else
            boundingBox.Min.Y += sweep.Y;

        if (sweep.Z > 0)
            boundingBox.Max.Z += sweep.Z;
        else
            boundingBox.Min.Z += sweep.Z;
    }

    /// <summary>
    /// Constructs a <see cref="BoundingBox"/> that fully contains the given points.
    /// </summary>
    /// <param name="points">The points that will be contained by the box.</param>
    /// <returns>The newly constructed bounding box.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="points"/> is <c>null</c>.</exception>
    public static BoundingBox FromPoints(Vector3F[] points)
    {
        if (points == null)
            throw new ArgumentNullException("points");

        Vector3F min = new Vector3F(float.MaxValue);
        Vector3F max = new Vector3F(float.MinValue);

        for (int i = 0; i < points.Length; ++i)
        {
            min = Vector3F.Min(ref min, ref points[i]);
            max = Vector3F.Max(ref max, ref points[i]);
        }

        return new BoundingBox(min, max);
    }

    /// <summary>
    /// Constructs a <see cref="BoundingBox"/> from a given sphere.
    /// </summary>
    /// <param name="sphere">The sphere that will designate the extents of the box.</param>
    /// <param name="result">When the method completes, contains the newly constructed bounding box.</param>
    public static void FromSphere(ref BoundingSphere sphere, out BoundingBox result)
    {
        result.Min = new Vector3F(sphere.Center.X - sphere.Radius, sphere.Center.Y - sphere.Radius, sphere.Center.Z - sphere.Radius);
        result.Max = new Vector3F(sphere.Center.X + sphere.Radius, sphere.Center.Y + sphere.Radius, sphere.Center.Z + sphere.Radius);
    }

    /// <summary>
    /// Constructs a <see cref="BoundingBox"/> from a given sphere.
    /// </summary>
    /// <param name="sphere">The sphere that will designate the extents of the box.</param>
    /// <returns>The newly constructed bounding box.</returns>
    public static BoundingBox FromSphere(BoundingSphere sphere)
    {
        BoundingBox box;
        box.Min = new Vector3F(sphere.Center.X - sphere.Radius, sphere.Center.Y - sphere.Radius, sphere.Center.Z - sphere.Radius);
        box.Max = new Vector3F(sphere.Center.X + sphere.Radius, sphere.Center.Y + sphere.Radius, sphere.Center.Z + sphere.Radius);
        return box;
    }

    /// <summary>
    /// Constructs a <see cref="BoundingBox"/> that is as large as the total combined area of the two specified boxes.
    /// </summary>
    /// <param name="value1">The first box to merge.</param>
    /// <param name="value2">The second box to merge.</param>
    /// <param name="result">When the method completes, contains the newly constructed bounding box.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Encapsulate(ref BoundingBox value1, ref BoundingBox value2, out BoundingBox result)
    {
        result.Min = Vector3F.Min(ref value1.Min, ref value2.Min);
        result.Max = Vector3F.Max(ref value1.Max, ref value2.Max);
    }

    /// <summary>
    /// Constructs a <see cref="BoundingBox"/> that is as large as the total combined area of the two specified boxes.
    /// </summary>
    /// <param name="value1">The first box to merge.</param>
    /// <param name="value2">The second box to merge.</param>
    /// <returns>The newly constructed bounding box.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BoundingBox Merge(BoundingBox value1, BoundingBox value2)
    {
        BoundingBox box;
        box.Min = Vector3F.Min(ref value1.Min, ref value2.Min);
        box.Max = Vector3F.Max(ref value1.Max, ref value2.Max);
        return box;
    }

    /// <summary>
    /// Creates the smallest bounding box which contains two other bounding boxes.
    /// </summary>
    /// <param name="a">First bounding box to be contained.</param>
    /// <param name="b">Second bounding box to be contained.</param>
    /// <param name="result">Smallest bounding box which contains the two input bounding boxes.</param>
    public static void Merge(ref BoundingBox a, ref BoundingBox b, out BoundingBox result)
    {
        result.Min = Vector3F.Min(ref a.Min, ref b.Min);
        result.Max = Vector3F.Max(ref a.Max, ref b.Max);
    }

    /// <summary>
    /// Tests for equality between two objects.
    /// </summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><c>true</c> if <paramref name="left"/> has the same value as <paramref name="right"/>; otherwise, <c>false</c>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(BoundingBox left, BoundingBox right)
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
    public static bool operator !=(BoundingBox left, BoundingBox right)
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
        return string.Format(CultureInfo.CurrentCulture, "Minimum:{0} Maximum:{1}", Min.ToString(), Max.ToString());
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

        return string.Format(CultureInfo.CurrentCulture, "Minimum:{0} Maximum:{1}", Min.ToString(format, CultureInfo.CurrentCulture),
            Max.ToString(format, CultureInfo.CurrentCulture));
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
        return string.Format(formatProvider, "Minimum:{0} Maximum:{1}", Min.ToString(), Max.ToString());
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

        return string.Format(formatProvider, "Minimum:{0} Maximum:{1}", Min.ToString(format, formatProvider),
            Max.ToString(format, formatProvider));
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
            return (Min.GetHashCode() * 397) ^ Max.GetHashCode();
        }
    }

    /// <summary>
    /// Determines whether the specified <see cref="Vector4F"/> is equal to this instance.
    /// </summary>
    /// <param name="value">The <see cref="Vector4F"/> to compare with this instance.</param>
    /// <returns>
    /// <c>true</c> if the specified <see cref="Vector4F"/> is equal to this instance; otherwise, <c>false</c>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(ref BoundingBox value)
    {
        return Min == value.Min && Max == value.Max;
    }

    /// <summary>
    /// Determines whether the specified <see cref="Vector4F"/> is equal to this instance.
    /// </summary>
    /// <param name="value">The <see cref="Vector4F"/> to compare with this instance.</param>
    /// <returns>
    /// <c>true</c> if the specified <see cref="Vector4F"/> is equal to this instance; otherwise, <c>false</c>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(BoundingBox value)
    {
        return Equals(ref value);
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
        if (!(value is BoundingBox))
            return false;

        var strongValue = (BoundingBox)value;
        return Equals(ref strongValue);
    }

    /// <summary>Expands the bounding box in all directions by the provided amount.</summary>
    /// <param name="amount"></param>
    public void Expand(float amount)
    {
        Min -= amount;
        Max += amount;
    }

    /// <summary>
    /// Constructs a <see cref="BoundingBox"/> that is as large as the total combined area of the two specified boxes.
    /// </summary>
    /// <param name="value1">The first box to merge.</param>
    /// <param name="other">The second box to merge.</param>
    /// <param name="result">When the method completes, contains the newly constructed bounding box.</param>
    public void Encapsulate(ref BoundingBox other)
    {
        Min = Vector3F.Min(Min, other.Min);
        Max = Vector3F.Max(Max, other.Max);
    }

    /// <summary>
    /// Constructs a <see cref="BoundingBox"/> that is as large as the total combined area of the two specified boxes.
    /// </summary>
    /// <param name="value1">The first box to merge.</param>
    /// <param name="other">The second box to merge.</param>
    /// <param name="result">When the method completes, contains the newly constructed bounding box.</param>
    public void Encapsulate(BoundingBox other)
    {
        Min = Vector3F.Min(Min, other.Min);
        Max = Vector3F.Max(Max, other.Max);
    }

    /// <summary>
    /// Constructs a <see cref="BoundingBox"/> that is as large as the bounding box and specified point.
    /// </summary>
    /// <param name="value1">The first box to merge.</param>
    /// <param name="other">The point to encapsulate.</param>
    /// <param name="result">When the method completes, contains the newly constructed bounding box.</param>
    public void Encapsulate(Vector3F other)
    {
        Min = Vector3F.Min(Min, other);
        Max = Vector3F.Max(Max, other);
    }

    /// <summary>
    /// Constructs a <see cref="BoundingBox"/> that is as large as the bounding box and specified point.
    /// </summary>
    /// <param name="value1">The first box to merge.</param>
    /// <param name="other">The point to encapsulate.</param>
    /// <param name="result">When the method completes, contains the newly constructed bounding box.</param>
    public void Encapsulate(ref Vector3F other)
    {
        Min = Vector3F.Min(Min, other);
        Max = Vector3F.Max(Max, other);
    }

    /// <summary>
    /// Constructs a <see cref="BoundingBox"/> that is as large as the total combined area of the bounding box and the specified sphere.
    /// </summary>
    /// <param name="other">The other.</param>
    public void Encapsulate(ref BoundingSphere other)
    {
        Min = Vector3F.Min(Min, other.Center - other.Radius);
        Max = Vector3F.Max(Max, other.Center + other.Radius);
    }

    /// <summary>
    /// Constructs a <see cref="BoundingBox"/> that is as large as the total combined area of the bounding box and the specified sphere.
    /// </summary>
    /// <param name="other">THe other.</param>
    public void Encapsulate(BoundingSphere other)
    {
        Min = Vector3F.Min(Min, other.Center - other.Radius);
        Max = Vector3F.Max(Max, other.Center + other.Radius);
    }
}
