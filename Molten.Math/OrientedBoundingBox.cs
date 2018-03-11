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

using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Molten
{
    /// <summary>
    /// OrientedBoundingBox (OBB) is a rectangular block, much like an AABB (BoundingBox) but with an arbitrary orientation.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct OrientedBoundingBox : IEquatable<OrientedBoundingBox>, IFormattable
    {
        /// <summary>
        /// Half lengths of the box along each axis.
        /// </summary>
        public Vector3F Extents;

        /// <summary>
        /// The matrix which aligns and scales the box, and its translation vector represents the center of the box.
        /// </summary>
        public Matrix4F Transformation;

        /// <summary>
        /// Creates an <see cref="OrientedBoundingBox"/> from a BoundingBox.
        /// </summary>
        /// <param name="bb">The BoundingBox to create from.</param>
        /// <remarks>
        /// Initially, the OBB is axis-aligned box, but it can be rotated and transformed later.
        /// </remarks>
        public OrientedBoundingBox(BoundingBox bb)
        {
            var Center = bb.Min + (bb.Max - bb.Min) / 2f;
            Extents = bb.Max - Center;
            Transformation = Matrix4F.CreateTranslation(Center);
        }

        /// <summary>
        /// Creates an <see cref="OrientedBoundingBox"/> which contained between two minimum and maximum points.
        /// </summary>
        /// <param name="minimum">The minimum vertex of the bounding box.</param>
        /// <param name="maximum">The maximum vertex of the bounding box.</param>
        /// <remarks>
        /// Initially, the OrientedBoundingBox is axis-aligned box, but it can be rotated and transformed later.
        /// </remarks>
        public OrientedBoundingBox(Vector3F minimum, Vector3F maximum)
        {
            var Center = minimum + (maximum - minimum) / 2f;
            Extents = maximum - Center;
            Transformation = Matrix4F.CreateTranslation(Center);
        }

        /// <summary>
        /// Creates an <see cref="OrientedBoundingBox"/> that fully contains the given points.
        /// </summary>
        /// <param name="points">The points that will be contained by the box.</param>
        /// <remarks>
        /// This method is not for computing the best tight-fitting OrientedBoundingBox.
        /// And initially, the OrientedBoundingBox is axis-aligned box, but it can be rotated and transformed later.
        /// </remarks>
        public OrientedBoundingBox(Vector3F[] points)
        {
            if (points == null || points.Length == 0)
                throw new ArgumentNullException("points");

            Vector3F minimum = new Vector3F(float.MaxValue);
            Vector3F maximum = new Vector3F(float.MinValue);

            for (int i = 0; i < points.Length; ++i)
            {
                Vector3F.Min(ref minimum, ref points[i], out minimum);
                Vector3F.Max(ref maximum, ref points[i], out maximum);
            }

            var Center = minimum + (maximum - minimum) / 2f;
            Extents = maximum - Center;
            Transformation = Matrix4F.CreateTranslation(Center);
        }

        /// <summary>
        /// Retrieves the eight corners of the bounding box.
        /// </summary>
        /// <returns>An array of points representing the eight corners of the bounding box.</returns>
        public Vector3F[] GetCorners()
        {
            var xv = new Vector3F(Extents.X, 0, 0);
            var yv = new Vector3F(0, Extents.Y, 0);
            var zv = new Vector3F(0, 0, Extents.Z);
            Vector3F.TransformNormal(ref xv, ref Transformation, out xv);
            Vector3F.TransformNormal(ref yv, ref Transformation, out yv);
            Vector3F.TransformNormal(ref zv, ref Transformation, out zv);

            var center = Transformation.Translation;

            var corners = new Vector3F[8];
            corners[0] = center + xv + yv + zv;
            corners[1] = center + xv + yv - zv;
            corners[2] = center - xv + yv - zv;
            corners[3] = center - xv + yv + zv;
            corners[4] = center + xv - yv + zv;
            corners[5] = center + xv - yv - zv;
            corners[6] = center - xv - yv - zv;
            corners[7] = center - xv - yv + zv;

            return corners;
        }

        /// <summary>
        /// Transforms this box using a transformation matrix.
        /// </summary>
        /// <param name="mat">The transformation matrix.</param>
        /// <remarks>
        /// While any kind of transformation can be applied, it is recommended to apply scaling using scale method instead, which
        /// scales the Extents and keeps the Transformation matrix for rotation only, and that preserves collision detection accuracy.
        /// </remarks>
        public void Transform(ref Matrix4F mat)
        {
            Transformation *= mat;
        }

        /// <summary>
        /// Transforms this box using a transformation matrix.
        /// </summary>
        /// <param name="mat">The transformation matrix.</param>
        /// <remarks>
        /// While any kind of transformation can be applied, it is recommended to apply scaling using scale method instead, which
        /// scales the Extents and keeps the Transformation matrix for rotation only, and that preserves collision detection accuracy.
        /// </remarks>
        public void Transform(Matrix4F mat)
        {
            Transformation *= mat;
        }

        /// <summary>
        /// Scales the <see cref="OrientedBoundingBox"/> by scaling its Extents without affecting the Transformation matrix,
        /// By keeping Transformation matrix scaling-free, the collision detection methods will be more accurate.
        /// </summary>
        /// <param name="scaling"></param>
        public void Scale(ref Vector3F scaling)
        {
            Extents *= scaling;
        }

        /// <summary>
        /// Scales the <see cref="OrientedBoundingBox"/> by scaling its Extents without affecting the Transformation matrix,
        /// By keeping Transformation matrix scaling-free, the collision detection methods will be more accurate.
        /// </summary>
        /// <param name="scaling"></param>
        public void Scale(Vector3F scaling)
        {
            Extents *= scaling;
        }

        /// <summary>
        /// Scales the <see cref="OrientedBoundingBox"/> by scaling its Extents without affecting the Transformation matrix,
        /// By keeping Transformation matrix scaling-free, the collision detection methods will be more accurate.
        /// </summary>
        /// <param name="scaling"></param>
        public void Scale(float scaling)
        {
            Extents *= scaling;
        }

        /// <summary>
        /// Translates the <see cref="OrientedBoundingBox"/> to a new position using a translation vector;
        /// </summary>
        /// <param name="translation">the translation vector.</param>
        public void Translate(ref Vector3F translation)
        {
            Transformation.Translation += translation;
        }

        /// <summary>
        /// Translates the <see cref="OrientedBoundingBox"/> to a new position using a translation vector;
        /// </summary>
        /// <param name="translation">the translation vector.</param>
        public void Translate(Vector3F translation)
        {
            Transformation.Translation += translation;
        }

        /// <summary>
        /// The size of the <see cref="OrientedBoundingBox"/> if no scaling is applied to the transformation matrix.
        /// </summary>
        /// <remarks>
        /// The property will return the actual size even if the scaling is applied using Scale method, 
        /// but if the scaling is applied to transformation matrix, use GetSize Function instead.
        /// </remarks>
        public Vector3F Size
        {
            get
            {
                return Extents * 2;
            }
        }

        /// <summary>
        /// Returns the size of the <see cref="OrientedBoundingBox"/> taking into consideration the scaling applied to the transformation matrix.
        /// </summary>
        /// <returns>The size of the consideration</returns>
        /// <remarks>
        /// This method is computationally expensive, so if no scale is applied to the transformation matrix
        /// use <see cref="OrientedBoundingBox.Size"/> property instead.
        /// </remarks>
        public Vector3F GetSize()
        {
            var xv = new Vector3F(Extents.X * 2, 0, 0);
            var yv = new Vector3F(0, Extents.Y * 2, 0);
            var zv = new Vector3F(0, 0, Extents.Z * 2);
            Vector3F.TransformNormal(ref xv, ref Transformation, out xv);
            Vector3F.TransformNormal(ref yv, ref Transformation, out yv);
            Vector3F.TransformNormal(ref zv, ref Transformation, out zv);

            return new Vector3F(xv.Length(), yv.Length(), zv.Length());
        }

        /// <summary>
        /// Returns the square size of the <see cref="OrientedBoundingBox"/> taking into consideration the scaling applied to the transformation matrix.
        /// </summary>
        /// <returns>The size of the consideration</returns>
        public Vector3F GetSizeSquared()
        {
            var xv = new Vector3F(Extents.X * 2, 0, 0);
            var yv = new Vector3F(0, Extents.Y * 2, 0);
            var zv = new Vector3F(0, 0, Extents.Z * 2);
            Vector3F.TransformNormal(ref xv, ref Transformation, out xv);
            Vector3F.TransformNormal(ref yv, ref Transformation, out yv);
            Vector3F.TransformNormal(ref zv, ref Transformation, out zv);

            return new Vector3F(xv.LengthSquared(), yv.LengthSquared(), zv.LengthSquared());
        }

        /// <summary>
        /// Returns the center of the <see cref="OrientedBoundingBox"/>.
        /// </summary>
        public Vector3F Center
        {
            get
            {
                return Transformation.Translation;
            }
        }

        /// <summary>
        /// Determines whether a <see cref="OrientedBoundingBox"/> contains a point. 
        /// </summary>
        /// <param name="point">The point to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        public ContainmentType Contains(ref Vector3F point)
        {
            // Transform the point into the obb coordinates
            Matrix4F invTrans;
            Matrix4F.Invert(ref Transformation, out invTrans);

            Vector3F locPoint;
            Vector3F.TransformCoordinate(ref point, ref invTrans, out locPoint);

            locPoint.X = Math.Abs(locPoint.X);
            locPoint.Y = Math.Abs(locPoint.Y);
            locPoint.Z = Math.Abs(locPoint.Z);

            //Simple axes-aligned BB check
            if (MathHelper.NearEqual(locPoint.X, Extents.X) && MathHelper.NearEqual(locPoint.Y, Extents.Y) && MathHelper.NearEqual(locPoint.Z, Extents.Z))
                return ContainmentType.Intersects;
            if (locPoint.X < Extents.X && locPoint.Y < Extents.Y && locPoint.Z < Extents.Z)
                return ContainmentType.Contains;
            else
                return ContainmentType.Disjoint;
        }

        /// <summary>
        /// Determines whether a <see cref="OrientedBoundingBox"/> contains a point. 
        /// </summary>
        /// <param name="point">The point to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        public ContainmentType Contains(Vector3F point)
        {
            return Contains(ref point);
        }

        /// <summary>
        /// Determines whether a <see cref="OrientedBoundingBox"/> contains an array of points>.
        /// </summary>
        /// <param name="points">The points array to test.</param>
        /// <returns>The type of containment.</returns>
        public ContainmentType Contains(Vector3F[] points)
        {
            Matrix4F invTrans;
            Matrix4F.Invert(ref Transformation, out invTrans);

            var containsAll = true;
            var containsAny = false;

            for (int i = 0; i < points.Length; i++)
            {
                Vector3F locPoint;
                Vector3F.TransformCoordinate(ref points[i], ref invTrans, out locPoint);

                locPoint.X = Math.Abs(locPoint.X);
                locPoint.Y = Math.Abs(locPoint.Y);
                locPoint.Z = Math.Abs(locPoint.Z);

                //Simple axes-aligned BB check
                if (MathHelper.NearEqual(locPoint.X, Extents.X) &&
                    MathHelper.NearEqual(locPoint.Y, Extents.Y) &&
                    MathHelper.NearEqual(locPoint.Z, Extents.Z))
                    containsAny = true;
                if (locPoint.X < Extents.X && locPoint.Y < Extents.Y && locPoint.Z < Extents.Z)
                    containsAny = true;
                else
                    containsAll = false;
            }

            if (containsAll)
                return ContainmentType.Contains;
            else if (containsAny)
                return ContainmentType.Intersects;
            else
                return ContainmentType.Disjoint;
        }

        /// <summary>
        /// Determines whether a <see cref="OrientedBoundingBox"/> contains a <see cref="BoundingSphere"/>.
        /// </summary>
        /// <param name="sphere">The sphere to test.</param>
        /// <param name="IgnoreScale">Optimize the check operation by assuming that <see cref="OrientedBoundingBox"/> has no scaling applied</param>
        /// <returns>The type of containment the two objects have.</returns>
        /// <remarks>
        /// This method is not designed for <see cref="OrientedBoundingBox"/> which has a non-uniform scaling applied to its transformation matrix.
        /// But any type of scaling applied using Scale method will keep this method accurate.
        /// </remarks>
        public ContainmentType Contains(BoundingSphere sphere, bool IgnoreScale = false)
        {
            Matrix4F invTrans;
            Matrix4F.Invert(ref Transformation, out invTrans);

            // Transform sphere center into the obb coordinates
            Vector3F locCenter;
            Vector3F.TransformCoordinate(ref sphere.Center, ref invTrans, out locCenter);

            float locRadius;
            if (IgnoreScale)
                locRadius = sphere.Radius;
            else
            {
                // Transform sphere radius into the obb coordinates
                Vector3F vRadius = Vector3F.UnitX * sphere.Radius;
                Vector3F.TransformNormal(ref vRadius, ref invTrans, out vRadius);
                locRadius = vRadius.Length();
            }

            //Perform regular BoundingBox to BoundingSphere containment check
            Vector3F minusExtens = -Extents;
            Vector3F vector;
            Vector3F.Clamp(ref locCenter, ref minusExtens, ref Extents, out vector);
            float distance = Vector3F.DistanceSquared(locCenter, vector);

            if (distance > locRadius * locRadius)
                return ContainmentType.Disjoint;

            if ((((minusExtens.X + locRadius <= locCenter.X) && (locCenter.X <= Extents.X - locRadius)) && ((Extents.X - minusExtens.X > locRadius) &&
                (minusExtens.Y + locRadius <= locCenter.Y))) && (((locCenter.Y <= Extents.Y - locRadius) && (Extents.Y - minusExtens.Y > locRadius)) &&
                (((minusExtens.Z + locRadius <= locCenter.Z) && (locCenter.Z <= Extents.Z - locRadius)) && (Extents.X - minusExtens.X > locRadius))))
            {
                return ContainmentType.Contains;
            }

            return ContainmentType.Intersects;
        }

        private static Vector3F[] GetRows(ref Matrix4F mat)
        {
            return new Vector3F[] {
                new Vector3F(mat.M11,mat.M12,mat.M13),
                new Vector3F(mat.M21,mat.M22,mat.M23),
                new Vector3F(mat.M31,mat.M32,mat.M33)
            };
        }

        /// <summary>
        /// Check the intersection between two <see cref="OrientedBoundingBox"/>
        /// </summary>
        /// <param name="obb">The OrientedBoundingBoxs to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        /// <remarks>
        /// For accuracy, The transformation matrix for both <see cref="OrientedBoundingBox"/> must not have any scaling applied to it.
        /// Anyway, scaling using Scale method will keep this method accurate.
        /// </remarks>
        public ContainmentType Contains(ref OrientedBoundingBox obb)
        {
            var cornersCheck = Contains(obb.GetCorners());
            if (cornersCheck != ContainmentType.Disjoint)
                return cornersCheck;

            //http://www.3dkingdoms.com/weekly/bbox.cpp
            var SizeA = Extents;
            var SizeB = obb.Extents;
            var RotA = GetRows(ref Transformation);
            var RotB = GetRows(ref obb.Transformation);

            var R = new Matrix4F();       // Rotation from B to A
            var AR = new Matrix4F();      // absolute values of R matrix, to use with box extents

            float ExtentA, ExtentB, Separation;
            int i, k;

            // Calculate B to A rotation matrix
            for (i = 0; i < 3; i++)
                for (k = 0; k < 3; k++)
                {
                    R[i, k] = Vector3F.Dot(RotA[i], RotB[k]);
                    AR[i, k] = Math.Abs(R[i, k]);
                }


            // Vector separating the centers of Box B and of Box A	
            var vSepWS = obb.Center - Center;
            // Rotated into Box A's coordinates
            var vSepA = new Vector3F(Vector3F.Dot(vSepWS, RotA[0]), Vector3F.Dot(vSepWS, RotA[1]), Vector3F.Dot(vSepWS, RotA[2]));

            // Test if any of A's basis vectors separate the box
            for (i = 0; i < 3; i++)
            {
                ExtentA = SizeA[i];
                ExtentB = Vector3F.Dot(SizeB, new Vector3F(AR[i, 0], AR[i, 1], AR[i, 2]));
                Separation = Math.Abs(vSepA[i]);

                if (Separation > ExtentA + ExtentB)
                    return ContainmentType.Disjoint;
            }

            // Test if any of B's basis vectors separate the box
            for (k = 0; k < 3; k++)
            {
                ExtentA = Vector3F.Dot(SizeA, new Vector3F(AR[0, k], AR[1, k], AR[2, k]));
                ExtentB = SizeB[k];
                Separation = Math.Abs(Vector3F.Dot(vSepA, new Vector3F(R[0, k], R[1, k], R[2, k])));

                if (Separation > ExtentA + ExtentB)
                    return ContainmentType.Disjoint;
            }

            // Now test Cross Products of each basis vector combination ( A[i], B[k] )
            for (i = 0; i < 3; i++)
                for (k = 0; k < 3; k++)
                {
                    int i1 = (i + 1) % 3, i2 = (i + 2) % 3;
                    int k1 = (k + 1) % 3, k2 = (k + 2) % 3;
                    ExtentA = SizeA[i1] * AR[i2, k] + SizeA[i2] * AR[i1, k];
                    ExtentB = SizeB[k1] * AR[i, k2] + SizeB[k2] * AR[i, k1];
                    Separation = Math.Abs(vSepA[i2] * R[i1, k] - vSepA[i1] * R[i2, k]);
                    if (Separation > ExtentA + ExtentB)
                        return ContainmentType.Disjoint;
                }

            // No separating axis found, the boxes overlap	
            return ContainmentType.Intersects;
        }

        /// <summary>
        /// Check the intersection between an <see cref="OrientedBoundingBox"/> and a line defined by two points
        /// </summary>
        /// <param name="L1">The first point in the line.</param>
        /// <param name="L2">The second point in the line.</param>
        /// <returns>The type of containment the two objects have.</returns>
        /// <remarks>
        /// For accuracy, The transformation matrix for the <see cref="OrientedBoundingBox"/> must not have any scaling applied to it.
        /// Anyway, scaling using Scale method will keep this method accurate.
        /// </remarks>
        public ContainmentType ContainsLine(ref Vector3F L1, ref Vector3F L2)
        {
            var cornersCheck = Contains(new Vector3F[] { L1, L2 });
            if (cornersCheck != ContainmentType.Disjoint)
                return cornersCheck;

            //http://www.3dkingdoms.com/weekly/bbox.cpp
            // Put line in box space
            Matrix4F invTrans;
            Matrix4F.Invert(ref Transformation, out invTrans);

            Vector3F LB1;
            Vector3F.TransformCoordinate(ref L1, ref invTrans, out LB1);
            Vector3F LB2;
            Vector3F.TransformCoordinate(ref L1, ref invTrans, out LB2);

            // Get line midpoint and extent
            var LMid = (LB1 + LB2) * 0.5f;
            var L = (LB1 - LMid);
            var LExt = new Vector3F(Math.Abs(L.X), Math.Abs(L.Y), Math.Abs(L.Z));

            // Use Separating Axis Test
            // Separation vector from box center to line center is LMid, since the line is in box space
            if (Math.Abs(LMid.X) > Extents.X + LExt.X) return ContainmentType.Disjoint;
            if (Math.Abs(LMid.Y) > Extents.Y + LExt.Y) return ContainmentType.Disjoint;
            if (Math.Abs(LMid.Z) > Extents.Z + LExt.Z) return ContainmentType.Disjoint;
            // Cross products of line and each axis
            if (Math.Abs(LMid.Y * L.Z - LMid.Z * L.Y) > (Extents.Y * LExt.Z + Extents.Z * LExt.Y)) return ContainmentType.Disjoint;
            if (Math.Abs(LMid.X * L.Z - LMid.Z * L.X) > (Extents.X * LExt.Z + Extents.Z * LExt.X)) return ContainmentType.Disjoint;
            if (Math.Abs(LMid.X * L.Y - LMid.Y * L.X) > (Extents.X * LExt.Y + Extents.Y * LExt.X)) return ContainmentType.Disjoint;
            // No separating axis, the line intersects
            return ContainmentType.Intersects;
        }

        /// <summary>
        /// Check the intersection between an <see cref="OrientedBoundingBox"/> and <see cref="BoundingBox"/>
        /// </summary>
        /// <param name="box">The BoundingBox to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        /// <remarks>
        /// For accuracy, The transformation matrix for the <see cref="OrientedBoundingBox"/> must not have any scaling applied to it.
        /// Anyway, scaling using Scale method will keep this method accurate.
        /// </remarks>
        public ContainmentType Contains(ref BoundingBox box)
        {
            var cornersCheck = Contains(box.GetCorners());
            if (cornersCheck != ContainmentType.Disjoint)
                return cornersCheck;

            var boxCenter = box.Min + (box.Max - box.Min) / 2f;
            var boxExtents = box.Max - boxCenter;

            var SizeA = Extents;
            var SizeB = boxExtents;
            var RotA = GetRows(ref Transformation);

            float ExtentA, ExtentB, Separation;
            int i, k;

            Matrix4F R;                   // Rotation from B to A
            Matrix4F.Invert(ref Transformation, out R);
            var AR = new Matrix4F();      // absolute values of R matrix, to use with box extents

            for (i = 0; i < 3; i++)
                for (k = 0; k < 3; k++)
                {
                    AR[i, k] = Math.Abs(R[i, k]);
                }


            // Vector separating the centers of Box B and of Box A	
            var vSepWS = boxCenter - Center;
            // Rotated into Box A's coordinates
            var vSepA = new Vector3F(Vector3F.Dot(vSepWS, RotA[0]), Vector3F.Dot(vSepWS, RotA[1]), Vector3F.Dot(vSepWS, RotA[2]));

            // Test if any of A's basis vectors separate the box
            for (i = 0; i < 3; i++)
            {
                ExtentA = SizeA[i];
                ExtentB = Vector3F.Dot(SizeB, new Vector3F(AR[i, 0], AR[i, 1], AR[i, 2]));
                Separation = Math.Abs(vSepA[i]);

                if (Separation > ExtentA + ExtentB)
                    return ContainmentType.Disjoint;
            }

            // Test if any of B's basis vectors separate the box
            for (k = 0; k < 3; k++)
            {
                ExtentA = Vector3F.Dot(SizeA, new Vector3F(AR[0, k], AR[1, k], AR[2, k]));
                ExtentB = SizeB[k];
                Separation = Math.Abs(Vector3F.Dot(vSepA, new Vector3F(R[0, k], R[1, k], R[2, k])));

                if (Separation > ExtentA + ExtentB)
                    return ContainmentType.Disjoint;
            }

            // Now test Cross Products of each basis vector combination ( A[i], B[k] )
            for (i = 0; i < 3; i++)
                for (k = 0; k < 3; k++)
                {
                    int i1 = (i + 1) % 3, i2 = (i + 2) % 3;
                    int k1 = (k + 1) % 3, k2 = (k + 2) % 3;
                    ExtentA = SizeA[i1] * AR[i2, k] + SizeA[i2] * AR[i1, k];
                    ExtentB = SizeB[k1] * AR[i, k2] + SizeB[k2] * AR[i, k1];
                    Separation = Math.Abs(vSepA[i2] * R[i1, k] - vSepA[i1] * R[i2, k]);
                    if (Separation > ExtentA + ExtentB)
                        return ContainmentType.Disjoint;
                }

            // No separating axis found, the boxes overlap	
            return ContainmentType.Intersects;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="Ray"/> and a <see cref="OrientedBoundingBox"/>.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="point">When the method completes, contains the point of intersection,
        /// or <see cref="Vector3F.Zero"/> if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(ref Ray ray, out Vector3F point)
        {
            // Put ray in box space
            Matrix4F invTrans;
            Matrix4F.Invert(ref Transformation, out invTrans);

            Ray bRay;
            Vector3F.TransformNormal(ref ray.Direction, ref invTrans, out bRay.Direction);
            Vector3F.TransformCoordinate(ref ray.Position, ref invTrans, out bRay.Position);

            //Perform a regular ray to BoundingBox check
            var bb = new BoundingBox(-Extents, Extents);
            var intersects = CollisionHelper.RayIntersectsBox(ref bRay, ref bb, out point);

            //Put the result intersection back to world
            if (intersects)
                Vector3F.TransformCoordinate(ref point, ref Transformation, out point);

            return intersects;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="Ray"/> and a <see cref="OrientedBoundingBox"/>.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public bool Intersects(ref Ray ray)
        {
            Vector3F point;
            return Intersects(ref ray, out point);
        }

        private Vector3F[] GetLocalCorners()
        {
            var xv = new Vector3F(Extents.X, 0, 0);
            var yv = new Vector3F(0, Extents.Y, 0);
            var zv = new Vector3F(0, 0, Extents.Z);

            var corners = new Vector3F[8];
            corners[0] = +xv + yv + zv;
            corners[1] = +xv + yv - zv;
            corners[2] = -xv + yv - zv;
            corners[3] = -xv + yv + zv;
            corners[4] = +xv - yv + zv;
            corners[5] = +xv - yv - zv;
            corners[6] = -xv - yv - zv;
            corners[7] = -xv - yv + zv;

            return corners;
        }

        /// <summary>
        /// Get the axis-aligned <see cref="BoundingBox"/> which contains all <see cref="OrientedBoundingBox"/> corners.
        /// </summary>
        /// <returns>The axis-aligned BoundingBox of this OrientedBoundingBox.</returns>
        public BoundingBox GetBoundingBox()
        {
            return BoundingBox.FromPoints(GetCorners());
        }

        /// <summary>
        /// Calculates the matrix required to transfer any point from one <see cref="OrientedBoundingBox"/> local coordinates to another.
        /// </summary>
        /// <param name="A">The source OrientedBoundingBox.</param>
        /// <param name="B">The target OrientedBoundingBox.</param>
        /// <param name="NoMatrixScaleApplied">
        /// If true, the method will use a fast algorithm which is inapplicable if a scale is applied to the transformation matrix of the OrientedBoundingBox.
        /// </param>
        /// <returns></returns>
        public static Matrix4F GetBoxToBoxMatrix(ref OrientedBoundingBox A, ref OrientedBoundingBox B, bool NoMatrixScaleApplied = false)
        {
            Matrix4F AtoB_Matrix;

            // Calculate B to A transformation matrix
            if (NoMatrixScaleApplied)
            {
                var RotA = GetRows(ref A.Transformation);
                var RotB = GetRows(ref B.Transformation);
                AtoB_Matrix = new Matrix4F();
                int i, k;
                for (i = 0; i < 3; i++)
                    for (k = 0; k < 3; k++)
                        AtoB_Matrix[i, k] = Vector3F.Dot(RotB[i], RotA[k]);
                var v = B.Center - A.Center;
                AtoB_Matrix.M41 = Vector3F.Dot(v, RotA[0]);
                AtoB_Matrix.M42 = Vector3F.Dot(v, RotA[1]);
                AtoB_Matrix.M43 = Vector3F.Dot(v, RotA[2]);
                AtoB_Matrix.M44 = 1;
            }
            else
            {
                Matrix4F AInvMat;
                Matrix4F.Invert(ref A.Transformation, out AInvMat);
                AtoB_Matrix = B.Transformation * AInvMat;
            }

            return AtoB_Matrix;
        }

        /// <summary>
        /// Merge an OrientedBoundingBox B into another OrientedBoundingBox A, by expanding A to contain B and keeping A orientation.
        /// </summary>
        /// <param name="A">The <see cref="OrientedBoundingBox"/> to merge into it.</param>
        /// <param name="B">The <see cref="OrientedBoundingBox"/> to be merged</param>
        /// <param name="NoMatrixScaleApplied">
        /// If true, the method will use a fast algorithm which is inapplicable if a scale is applied to the transformation matrix of the OrientedBoundingBox.
        /// </param>
        /// <remarks>
        /// Unlike merging axis aligned boxes, The operation is not interchangeable, because it keeps A orientation and merge B into it.
        /// </remarks>
        public static void Merge(ref OrientedBoundingBox A, ref OrientedBoundingBox B, bool NoMatrixScaleApplied = false)
        {
            Matrix4F AtoB_Matrix = GetBoxToBoxMatrix(ref A, ref B, NoMatrixScaleApplied);

            //Get B corners in A Space
            var bCorners = B.GetLocalCorners();
            Vector3F.TransformCoordinate(bCorners, ref AtoB_Matrix, bCorners);

            //Get A local Bounding Box
            var A_LocalBB = new BoundingBox(-A.Extents, A.Extents);

            //Find B BoundingBox in A Space
            var B_LocalBB = BoundingBox.FromPoints(bCorners);

            //Merger A and B local Bounding Boxes
            BoundingBox mergedBB;
            BoundingBox.Encapsulate(ref B_LocalBB, ref A_LocalBB, out mergedBB);

            //Find the new Extents and Center, Transform Center back to world
            var newCenter = mergedBB.Min + (mergedBB.Max - mergedBB.Min) / 2f;
            A.Extents = mergedBB.Max - newCenter;
            Vector3F.TransformCoordinate(ref newCenter, ref A.Transformation, out newCenter);
            A.Transformation.Translation = newCenter;
        }

        /// <summary>
        /// Merge this OrientedBoundingBox into another OrientedBoundingBox, keeping the other OrientedBoundingBox orientation.
        /// </summary>
        /// <param name="OBB">The other <see cref="OrientedBoundingBox"/> to merge into.</param>
        /// <param name="NoMatrixScaleApplied">
        /// If true, the method will use a fast algorithm which is inapplicable if a scale is applied to the transformation matrix of the OrientedBoundingBox.
        /// </param>
        public void MergeInto(ref OrientedBoundingBox OBB, bool NoMatrixScaleApplied = false)
        {
            Merge(ref OBB, ref this, NoMatrixScaleApplied);
        }

        /// <summary>
        /// Merge another OrientedBoundingBox into this OrientedBoundingBox.
        /// </summary>
        /// <param name="OBB">The other <see cref="OrientedBoundingBox"/> to merge into this OrientedBoundingBox.</param>
        /// <param name="NoMatrixScaleApplied">
        /// If true, the method will use a fast algorithm which is inapplicable if a scale is applied to the transformation matrix of the OrientedBoundingBox.
        /// </param>
        public void Add(ref OrientedBoundingBox OBB, bool NoMatrixScaleApplied = false)
        {
            Merge(ref this, ref OBB, NoMatrixScaleApplied);
        }

        /// <summary>
        /// Determines whether the specified <see cref="Vector4F"/> is equal to this instance.
        /// </summary>
        /// <param name="value">The <see cref="Vector4F"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="Vector4F"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(ref OrientedBoundingBox value)
        {
            return Extents == value.Extents && Transformation == value.Transformation;
        }

        /// <summary>
        /// Determines whether the specified <see cref="Vector4F"/> is equal to this instance.
        /// </summary>
        /// <param name="value">The <see cref="Vector4F"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="Vector4F"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(OrientedBoundingBox value)
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
            if (!(value is OrientedBoundingBox))
                return false;

            var strongValue = (OrientedBoundingBox)value;
            return Equals(ref strongValue);
        }

        /// <summary>
        /// Tests for equality between two objects.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> has the same value as <paramref name="right"/>; otherwise, <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(OrientedBoundingBox left, OrientedBoundingBox right)
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
        public static bool operator !=(OrientedBoundingBox left, OrientedBoundingBox right)
        {
            return !left.Equals(ref right);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return Extents.GetHashCode() + Transformation.GetHashCode();
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return String.Format(CultureInfo.CurrentCulture, "Center: {0}, Extents: {1}", Center, Extents);
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

            return string.Format(CultureInfo.CurrentCulture, "Center: {0}, Extents: {1}", Center.ToString(format, CultureInfo.CurrentCulture),
                Extents.ToString(format, CultureInfo.CurrentCulture));
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
            return string.Format(formatProvider, "Center: {0}, Extents: {1}", Center.ToString(), Extents.ToString());
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

            return string.Format(formatProvider, "Center: {0}, Extents: {1}", Center.ToString(format, formatProvider),
                Extents.ToString(format, formatProvider));
        }
    }
}

