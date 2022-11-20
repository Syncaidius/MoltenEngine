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

namespace Molten
{
    /*
     * This class is organized so that the least complex objects come first so that the least
     * complex objects will have the most methods in most cases. Note that not all shapes exist
     * at this time and not all shapes have a corresponding struct. Only the objects that have
     * a corresponding struct should come first in naming and in parameter order. The order of
     * complexity is as follows:
     * 
     * 1. Point
     * 2. Ray
     * 3. Segment
     * 4. Plane
     * 5. Triangle
     * 6. Polygon
     * 7. Box
     * 8. Sphere
     * 9. Ellipsoid
     * 10. Cylinder
     * 11. Cone
     * 12. Capsule
     * 13. Torus
     * 14. Polyhedron
     * 15. Frustum
    */

    /// <summary>
    /// Contains static methods to help in determining intersections, containment, etc.
    /// </summary>
    public static class CollisionHelper
    {
        /// <summary>
        /// Determines the closest point between a point and a triangle.
        /// </summary>
        /// <param name="point">The point to test.</param>
        /// <param name="vertex1">The first vertex to test.</param>
        /// <param name="vertex2">The second vertex to test.</param>
        /// <param name="vertex3">The third vertex to test.</param>
        /// <param name="result">When the method completes, contains the closest point between the two objects.</param>
        public static void ClosestPointPointTriangle(ref Vector3F point, ref Vector3F vertex1, ref Vector3F vertex2, ref Vector3F vertex3, out Vector3F result)
        {
            //Source: Real-Time Collision Detection by Christer Ericson
            //Reference: Page 136

            //Check if P in vertex region outside A
            Vector3F ab = vertex2 - vertex1;
            Vector3F ac = vertex3 - vertex1;
            Vector3F ap = point - vertex1;

            float d1 = Vector3F.Dot(ab, ap);
            float d2 = Vector3F.Dot(ac, ap);
            if (d1 <= 0.0f && d2 <= 0.0f)
                result = vertex1; //Barycentric coordinates (1,0,0)

            //Check if P in vertex region outside B
            Vector3F bp = point - vertex2;
            float d3 = Vector3F.Dot(ab, bp);
            float d4 = Vector3F.Dot(ac, bp);
            if (d3 >= 0.0f && d4 <= d3)
                result = vertex2; // Barycentric coordinates (0,1,0)

            //Check if P in edge region of AB, if so return projection of P onto AB
            float vc = d1 * d4 - d3 * d2;
            if (vc <= 0.0f && d1 >= 0.0f && d3 <= 0.0f)
            {
                float v = d1 / (d1 - d3);
                result = vertex1 + v * ab; //Barycentric coordinates (1-v,v,0)
            }

            //Check if P in vertex region outside C
            Vector3F cp = point - vertex3;
            float d5 = Vector3F.Dot(ab, cp);
            float d6 = Vector3F.Dot(ac, cp);
            if (d6 >= 0.0f && d5 <= d6)
                result = vertex3; //Barycentric coordinates (0,0,1)

            //Check if P in edge region of AC, if so return projection of P onto AC
            float vb = d5 * d2 - d1 * d6;
            if (vb <= 0.0f && d2 >= 0.0f && d6 <= 0.0f)
            {
                float w = d2 / (d2 - d6);
                result = vertex1 + w * ac; //Barycentric coordinates (1-w,0,w)
            }

            //Check if P in edge region of BC, if so return projection of P onto BC
            float va = d3 * d6 - d5 * d4;
            if (va <= 0.0f && (d4 - d3) >= 0.0f && (d5 - d6) >= 0.0f)
            {
                float w = (d4 - d3) / ((d4 - d3) + (d5 - d6));
                result = vertex2 + w * (vertex3 - vertex2); //Barycentric coordinates (0,1-w,w)
            }

            //P inside face region. Compute Q through its Barycentric coordinates (u,v,w)
            float denom = 1.0f / (va + vb + vc);
            float v2 = vb * denom;
            float w2 = vc * denom;
            result = vertex1 + ab * v2 + ac * w2; //= u*vertex1 + v*vertex2 + w*vertex3, u = va * denom = 1.0f - v - w
        }

        /// <summary>
        /// Determines the closest point between a <see cref="Plane"/> and a point.
        /// </summary>
        /// <param name="plane">The plane to test.</param>
        /// <param name="point">The point to test.</param>
        /// <param name="result">When the method completes, contains the closest point between the two objects.</param>
        public static void ClosestPointPlanePoint(ref Plane plane, ref Vector3F point, out Vector3F result)
        {
            //Source: Real-Time Collision Detection by Christer Ericson
            //Reference: Page 126

            float dot = Vector3F.Dot(ref plane.Normal, ref point);
            float t = dot - plane.D;

            result = point - (t * plane.Normal);
        }

        /// <summary>
        /// Determines the closest point between a <see cref="BoundingBox"/> and a point.
        /// </summary>
        /// <param name="box">The box to test.</param>
        /// <param name="point">The point to test.</param>
        /// <param name="result">When the method completes, contains the closest point between the two objects.</param>
        public static void ClosestPointBoxPoint(ref BoundingBox box, ref Vector3F point, out Vector3F result)
        {
            //Source: Real-Time Collision Detection by Christer Ericson
            //Reference: Page 130

            Vector3F temp;
            Vector3F.Max(ref point, ref box.Min, out temp);
            Vector3F.Min(ref temp, ref box.Max, out result);
        }

        /// <summary>
        /// Determines the closest point between a <see cref="BoundingSphere"/> and a point.
        /// </summary>
        /// <param name="sphere"></param>
        /// <param name="point">The point to test.</param>
        /// <param name="result">When the method completes, contains the closest point between the two objects;
        /// or, if the point is directly in the center of the sphere, contains <see cref="Vector3F.Zero"/>.</param>
        public static void ClosestPointSpherePoint(ref BoundingSphere sphere, ref Vector3F point, out Vector3F result)
        {
            //Source: Jorgy343
            //Reference: None

            //Get the unit direction from the sphere's center to the point.
            Vector3F.Subtract(ref point, ref sphere.Center, out result);
            result.Normalize();

            //Multiply the unit direction by the sphere's radius to get a vector
            //the length of the sphere.
            result *= sphere.Radius;

            //Add the sphere's center to the direction to get a point on the sphere.
            result += sphere.Center;
        }

        /// <summary>
        /// Determines the closest point between a <see cref="BoundingSphere"/> and a <see cref="BoundingSphere"/>.
        /// </summary>
        /// <param name="sphere1">The first sphere to test.</param>
        /// <param name="sphere2">The second sphere to test.</param>
        /// <param name="result">When the method completes, contains the closest point between the two objects;
        /// or, if the point is directly in the center of the sphere, contains <see cref="Vector3F.Zero"/>.</param>
        /// <remarks>
        /// If the two spheres are overlapping, but not directly on top of each other, the closest point
        /// is the 'closest' point of intersection. This can also be considered is the deepest point of
        /// intersection.
        /// </remarks>
        public static void ClosestPointSphereSphere(ref BoundingSphere sphere1, ref BoundingSphere sphere2, out Vector3F result)
        {
            //Source: Jorgy343
            //Reference: None

            //Get the unit direction from the first sphere's center to the second sphere's center.
            Vector3F.Subtract(ref sphere2.Center, ref sphere1.Center, out result);
            result.Normalize();

            //Multiply the unit direction by the first sphere's radius to get a vector
            //the length of the first sphere.
            result *= sphere1.Radius;

            //Add the first sphere's center to the direction to get a point on the first sphere.
            result += sphere1.Center;
        }

        /// <summary>
        /// Determines the distance between a <see cref="Plane"/> and a point.
        /// </summary>
        /// <param name="plane">The plane to test.</param>
        /// <param name="point">The point to test.</param>
        /// <returns>The distance between the two objects.</returns>
        public static float DistancePlanePoint(ref Plane plane, ref Vector3F point)
        {
            //Source: Real-Time Collision Detection by Christer Ericson
            //Reference: Page 127

            float dot = Vector3F.Dot(ref plane.Normal, ref point);
            return dot - plane.D;
        }

        /// <summary>
        /// Determines the distance between a <see cref="BoundingBox"/> and a point.
        /// </summary>
        /// <param name="box">The box to test.</param>
        /// <param name="point">The point to test.</param>
        /// <returns>The distance between the two objects.</returns>
        public static float DistanceBoxPoint(ref BoundingBox box, ref Vector3F point)
        {
            //Source: Real-Time Collision Detection by Christer Ericson
            //Reference: Page 131

            float distance = 0f;

            if (point.X < box.Min.X)
                distance += (box.Min.X - point.X) * (box.Min.X - point.X);
            if (point.X > box.Max.X)
                distance += (point.X - box.Max.X) * (point.X - box.Max.X);

            if (point.Y < box.Min.Y)
                distance += (box.Min.Y - point.Y) * (box.Min.Y - point.Y);
            if (point.Y > box.Max.Y)
                distance += (point.Y - box.Max.Y) * (point.Y - box.Max.Y);

            if (point.Z < box.Min.Z)
                distance += (box.Min.Z - point.Z) * (box.Min.Z - point.Z);
            if (point.Z > box.Max.Z)
                distance += (point.Z - box.Max.Z) * (point.Z - box.Max.Z);

            return (float)Math.Sqrt(distance);
        }

        /// <summary>
        /// Determines the distance between a <see cref="BoundingBox"/> and a <see cref="BoundingBox"/>.
        /// </summary>
        /// <param name="box1">The first box to test.</param>
        /// <param name="box2">The second box to test.</param>
        /// <returns>The distance between the two objects.</returns>
        public static float DistanceBoxBox(ref BoundingBox box1, ref BoundingBox box2)
        {
            //Source:
            //Reference:

            float distance = 0f;

            //Distance for X.
            if (box1.Min.X > box2.Max.X)
            {
                float delta = box2.Max.X - box1.Min.X;
                distance += delta * delta;
            }
            else if (box2.Min.X > box1.Max.X)
            {
                float delta = box1.Max.X - box2.Min.X;
                distance += delta * delta;
            }

            //Distance for Y.
            if (box1.Min.Y > box2.Max.Y)
            {
                float delta = box2.Max.Y - box1.Min.Y;
                distance += delta * delta;
            }
            else if (box2.Min.Y > box1.Max.Y)
            {
                float delta = box1.Max.Y - box2.Min.Y;
                distance += delta * delta;
            }

            //Distance for Z.
            if (box1.Min.Z > box2.Max.Z)
            {
                float delta = box2.Max.Z - box1.Min.Z;
                distance += delta * delta;
            }
            else if (box2.Min.Z > box1.Max.Z)
            {
                float delta = box1.Max.Z - box2.Min.Z;
                distance += delta * delta;
            }

            return (float)Math.Sqrt(distance);
        }

        /// <summary>
        /// Determines the distance between a <see cref="BoundingSphere"/> and a point.
        /// </summary>
        /// <param name="sphere">The sphere to test.</param>
        /// <param name="point">The point to test.</param>
        /// <returns>The distance between the two objects.</returns>
        public static float DistanceSpherePoint(ref BoundingSphere sphere, ref Vector3F point)
        {
            //Source: Jorgy343
            //Reference: None

            float distance = Vector3F.Distance(ref sphere.Center, ref point);
            distance -= sphere.Radius;

            return Math.Max(distance, 0f);
        }

        /// <summary>
        /// Determines the distance between a <see cref="BoundingSphere"/> and a <see cref="BoundingSphere"/>.
        /// </summary>
        /// <param name="sphere1">The first sphere to test.</param>
        /// <param name="sphere2">The second sphere to test.</param>
        /// <returns>The distance between the two objects.</returns>
        public static float DistanceSphereSphere(ref BoundingSphere sphere1, ref BoundingSphere sphere2)
        {
            //Source: Jorgy343
            //Reference: None

            float distance = Vector3F.Distance(ref sphere1.Center, ref sphere2.Center);
            distance -= sphere1.Radius + sphere2.Radius;

            return Math.Max(distance, 0f);
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="Ray"/> and a point.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="point">The point to test.</param>
        /// <returns>Whether the two objects intersect.</returns>
        public static bool RayIntersectsPoint(ref Ray ray, ref Vector3F point)
        {
            //Source: RayIntersectsSphere
            //Reference: None

            Vector3F m;
            Vector3F.Subtract(ref ray.Position, ref point, out m);

            //Same thing as RayIntersectsSphere except that the radius of the sphere (point)
            //is the epsilon for zero.
            float b = Vector3F.Dot(m, ray.Direction);
            float c = Vector3F.Dot(m, m) - MathHelper.ZeroTolerance;

            if (c > 0f && b > 0f)
                return false;

            float discriminant = b * b - c;

            if (discriminant < 0f)
                return false;

            return true;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="Ray"/> and a <see cref="Ray"/>.
        /// </summary>
        /// <param name="ray1">The first ray to test.</param>
        /// <param name="ray2">The second ray to test.</param>
        /// <param name="point">When the method completes, contains the point of intersection,
        /// or <see cref="Vector3F.Zero"/> if there was no intersection.</param>
        /// <returns>Whether the two objects intersect.</returns>
        /// <remarks>
        /// This method performs a ray vs ray intersection test based on the following formula
        /// from Goldman.
        /// <code>s = det([o_2 - o_1, d_2, d_1 x d_2]) / ||d_1 x d_2||^2</code>
        /// <code>t = det([o_2 - o_1, d_1, d_1 x d_2]) / ||d_1 x d_2||^2</code>
        /// Where o_1 is the position of the first ray, o_2 is the position of the second ray,
        /// d_1 is the normalized direction of the first ray, d_2 is the normalized direction
        /// of the second ray, det denotes the determinant of a matrix, x denotes the cross
        /// product, [ ] denotes a matrix, and || || denotes the length or magnitude of a vector.
        /// </remarks>
        public static bool RayIntersectsRay(ref Ray ray1, ref Ray ray2, out Vector3F point)
        {
            //Source: Real-Time Rendering, Third Edition
            //Reference: Page 780

            Vector3F.Cross(ref ray1.Direction, ref ray2.Direction, out Vector3F cross);
            float denominator = cross.Length();

            //Lines are parallel.
            if (MathHelper.IsZero(denominator))
            {
                //Lines are parallel and on top of each other.
                if (MathHelper.NearEqual(ray2.Position.X, ray1.Position.X) &&
                    MathHelper.NearEqual(ray2.Position.Y, ray1.Position.Y) &&
                    MathHelper.NearEqual(ray2.Position.Z, ray1.Position.Z))
                {
                    point = Vector3F.Zero;
                    return true;
                }
            }

            denominator = denominator * denominator;

            //3x3 matrix for the first ray.
            float m11 = ray2.Position.X - ray1.Position.X;
            float m12 = ray2.Position.Y - ray1.Position.Y;
            float m13 = ray2.Position.Z - ray1.Position.Z;
            float m21 = ray2.Direction.X;
            float m22 = ray2.Direction.Y;
            float m23 = ray2.Direction.Z;
            float m31 = cross.X;
            float m32 = cross.Y;
            float m33 = cross.Z;

            //Determinant of first matrix.
            float dets =
                m11 * m22 * m33 +
                m12 * m23 * m31 +
                m13 * m21 * m32 -
                m11 * m23 * m32 -
                m12 * m21 * m33 -
                m13 * m22 * m31;

            //3x3 matrix for the second ray.
            m21 = ray1.Direction.X;
            m22 = ray1.Direction.Y;
            m23 = ray1.Direction.Z;

            //Determinant of the second matrix.
            float dett =
                m11 * m22 * m33 +
                m12 * m23 * m31 +
                m13 * m21 * m32 -
                m11 * m23 * m32 -
                m12 * m21 * m33 -
                m13 * m22 * m31;

            //t values of the point of intersection.
            float s = dets / denominator;
            float t = dett / denominator;

            //The points of intersection.
            Vector3F point1 = ray1.Position + (s * ray1.Direction);
            Vector3F point2 = ray2.Position + (t * ray2.Direction);

            //If the points are not equal, no intersection has occurred.
            if (!MathHelper.NearEqual(point2.X, point1.X) ||
                !MathHelper.NearEqual(point2.Y, point1.Y) ||
                !MathHelper.NearEqual(point2.Z, point1.Z))
            {
                point = Vector3F.Zero;
                return false;
            }

            point = point1;
            return true;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="Ray"/> and a <see cref="Plane"/>.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="plane">The plane to test.</param>
        /// <param name="distance">When the method completes, contains the distance of the intersection,
        /// or 0 if there was no intersection.</param>
        /// <returns>Whether the two objects intersect.</returns>
        public static bool RayIntersectsPlane(ref Ray ray, ref Plane plane, out float distance)
        {
            //Source: Real-Time Collision Detection by Christer Ericson
            //Reference: Page 175

            float direction = Vector3F.Dot(ref plane.Normal, ref ray.Direction);

            if (MathHelper.IsZero(direction))
            {
                distance = 0f;
                return false;
            }

            float position = Vector3F.Dot(ref plane.Normal, ref ray.Position);
            distance = (-plane.D - position) / direction;

            if (distance < 0f)
            {
                distance = 0f;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="Ray"/> and a <see cref="Plane"/>.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="plane">The plane to test</param>
        /// <param name="point">When the method completes, contains the point of intersection,
        /// or <see cref="Vector3F.Zero"/> if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool RayIntersectsPlane(ref Ray ray, ref Plane plane, out Vector3F point)
        {
            //Source: Real-Time Collision Detection by Christer Ericson
            //Reference: Page 175

            float distance;
            if (!RayIntersectsPlane(ref ray, ref plane, out distance))
            {
                point = Vector3F.Zero;
                return false;
            }

            point = ray.Position + (ray.Direction * distance);
            return true;
        }

        /// <summary>
        /// Finds the intersection between the given ray and the given plane.
        /// </summary>
        /// <param name="ray">Ray to test against the plane.</param>
        /// <param name="p">Plane for comparison.</param>
        /// <param name="t">Interval along line to intersection (A + t * AB).</param>
        /// <param name="q">Intersection point.</param>
        /// <returns>Whether or not the line intersects the plane.  If false, the line is parallel to the plane's surface.</returns>
        public static bool RayIntersectsPlane(ref Ray ray, ref Plane p, out float t, out Vector3F q)
        {
            float denominator = Vector3F.Dot(ref p.Normal, ref ray.Direction);
            if (denominator < MathHelper.Epsilon && denominator > -MathHelper.Epsilon)
            {
                //Surface of plane and line are parallel (or very close to it).
                q = new Vector3F();
                t = float.MaxValue;
                return false;
            }
            float numerator = Vector3F.Dot(ref p.Normal, ref ray.Position);
            t = (p.D - numerator) / denominator;
            //Compute the intersection position.
            Vector3F.Multiply(ref ray.Direction, t, out q);
            Vector3F.Add(ref ray.Position, ref q, out q);
            return t >= 0;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="Ray"/> and a triangle.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="vertex1">The first vertex of the triangle to test.</param>
        /// <param name="vertex2">The second vertex of the triangle to test.</param>
        /// <param name="vertex3">The third vertex of the triangle to test.</param>
        /// <param name="distance">When the method completes, contains the distance of the intersection,
        /// or 0 if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        /// <remarks>
        /// This method tests if the ray intersects either the front or back of the triangle.
        /// If the ray is parallel to the triangle's plane, no intersection is assumed to have
        /// happened. If the intersection of the ray and the triangle is behind the origin of
        /// the ray, no intersection is assumed to have happened. In both cases of assumptions,
        /// this method returns false.
        /// </remarks>
        public static bool RayIntersectsTriangle(ref Ray ray, ref Vector3F vertex1, ref Vector3F vertex2, ref Vector3F vertex3, out float distance)
        {
            //Source: Fast Minimum Storage Ray / Triangle Intersection
            //Reference: http://www.cs.virginia.edu/~gfx/Courses/2003/ImageSynthesis/papers/Acceleration/Fast%20MinimumStorage%20RayTriangle%20Intersection.pdf

            //Compute vectors along two edges of the triangle.
            Vector3F edge1, edge2;

            //Edge 1
            edge1.X = vertex2.X - vertex1.X;
            edge1.Y = vertex2.Y - vertex1.Y;
            edge1.Z = vertex2.Z - vertex1.Z;

            //Edge2
            edge2.X = vertex3.X - vertex1.X;
            edge2.Y = vertex3.Y - vertex1.Y;
            edge2.Z = vertex3.Z - vertex1.Z;

            //Cross product of ray direction and edge2 - first part of determinant.
            Vector3F directioncrossedge2;
            directioncrossedge2.X = (ray.Direction.Y * edge2.Z) - (ray.Direction.Z * edge2.Y);
            directioncrossedge2.Y = (ray.Direction.Z * edge2.X) - (ray.Direction.X * edge2.Z);
            directioncrossedge2.Z = (ray.Direction.X * edge2.Y) - (ray.Direction.Y * edge2.X);

            //Compute the determinant.
            float determinant;
            //Dot product of edge1 and the first part of determinant.
            determinant = (edge1.X * directioncrossedge2.X) + (edge1.Y * directioncrossedge2.Y) + (edge1.Z * directioncrossedge2.Z);

            //If the ray is parallel to the triangle plane, there is no collision.
            //This also means that we are not culling, the ray may hit both the
            //back and the front of the triangle.
            if (MathHelper.IsZero(determinant))
            {
                distance = 0f;
                return false;
            }

            float inversedeterminant = 1.0f / determinant;

            //Calculate the U parameter of the intersection point.
            Vector3F distanceVector;
            distanceVector.X = ray.Position.X - vertex1.X;
            distanceVector.Y = ray.Position.Y - vertex1.Y;
            distanceVector.Z = ray.Position.Z - vertex1.Z;

            float triangleU;
            triangleU = (distanceVector.X * directioncrossedge2.X) + (distanceVector.Y * directioncrossedge2.Y) + (distanceVector.Z * directioncrossedge2.Z);
            triangleU *= inversedeterminant;

            //Make sure it is inside the triangle.
            if (triangleU < 0f || triangleU > 1f)
            {
                distance = 0f;
                return false;
            }

            //Calculate the V parameter of the intersection point.
            Vector3F distancecrossedge1;
            distancecrossedge1.X = (distanceVector.Y * edge1.Z) - (distanceVector.Z * edge1.Y);
            distancecrossedge1.Y = (distanceVector.Z * edge1.X) - (distanceVector.X * edge1.Z);
            distancecrossedge1.Z = (distanceVector.X * edge1.Y) - (distanceVector.Y * edge1.X);

            float triangleV;
            triangleV = ((ray.Direction.X * distancecrossedge1.X) + (ray.Direction.Y * distancecrossedge1.Y)) + (ray.Direction.Z * distancecrossedge1.Z);
            triangleV *= inversedeterminant;

            //Make sure it is inside the triangle.
            if (triangleV < 0f || triangleU + triangleV > 1f)
            {
                distance = 0f;
                return false;
            }

            //Compute the distance along the ray to the triangle.
            float raydistance;
            raydistance = (edge2.X * distancecrossedge1.X) + (edge2.Y * distancecrossedge1.Y) + (edge2.Z * distancecrossedge1.Z);
            raydistance *= inversedeterminant;

            //Is the triangle behind the ray origin?
            if (raydistance < 0f)
            {
                distance = 0f;
                return false;
            }

            distance = raydistance;
            return true;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="Ray"/> and a triangle.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="vertex1">The first vertex of the triangle to test.</param>
        /// <param name="vertex2">The second vertex of the triangle to test.</param>
        /// <param name="vertex3">The third vertex of the triangle to test.</param>
        /// <param name="point">When the method completes, contains the point of intersection,
        /// or <see cref="Vector3F.Zero"/> if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool RayIntersectsTriangle(ref Ray ray, ref Vector3F vertex1, ref Vector3F vertex2, ref Vector3F vertex3, out Vector3F point)
        {
            float distance;
            if (!RayIntersectsTriangle(ref ray, ref vertex1, ref vertex2, ref vertex3, out distance))
            {
                point = Vector3F.Zero;
                return false;
            }

            point = ray.Position + (ray.Direction * distance);
            return true;
        }

        /// <summary>
        /// Determines the intersection between a ray and a triangle.
        /// </summary>
        /// <param name="ray">Ray to test.</param>
        /// <param name="maximumLength">Maximum length to travel in units of the direction's length.</param>
        /// <param name="a">First vertex of the triangle.</param>
        /// <param name="b">Second vertex of the triangle.</param>
        /// <param name="c">Third vertex of the triangle.</param>
        /// <param name="hitClockwise">True if the the triangle was hit on the clockwise face, false otherwise.</param>
        /// <param name="hit">Hit data of the ray, if any</param>
        /// <returns>Whether or not the ray and triangle intersect.</returns>
        public static bool RayIntersectsTriangle(ref Ray ray, float maximumLength, ref Vector3F a, ref Vector3F b, ref Vector3F c, out bool hitClockwise, out RayHit hit)
        {
            hitClockwise = false;
            hit = new RayHit();
            Vector3F ab, ac;
            Vector3F.Subtract(ref b, ref a, out ab);
            Vector3F.Subtract(ref c, ref a, out ac);

            Vector3F.Cross(ref ab, ref ac, out hit.Normal);
            if (hit.Normal.LengthSquared() < MathHelper.Epsilon)
                return false; //Degenerate triangle!

            float d = Vector3F.Dot(ref ray.Direction, ref hit.Normal);
            d = -d;

            hitClockwise = d >= 0;

            Vector3F ap;
            Vector3F.Subtract(ref ray.Position, ref a, out ap);

            hit.T = Vector3F.Dot(ref ap, ref hit.Normal) / d;
            if (hit.T < 0 || hit.T > maximumLength)
                return false;//Hit is behind origin, or too far away.

            Vector3F.Multiply(ref ray.Direction, hit.T, out hit.Location);
            Vector3F.Add(ref ray.Position, ref hit.Location, out hit.Location);

            // Compute barycentric coordinates
            Vector3F.Subtract(ref hit.Location, ref a, out ap);
            float ABdotAB = Vector3F.Dot(ref ab, ref ab);
            float ABdotAC = Vector3F.Dot(ref ab, ref ac);
            float ABdotAP = Vector3F.Dot(ref ab, ref ap);
            float ACdotAC = Vector3F.Dot(ref ac, ref ac);
            float ACdotAP = Vector3F.Dot(ref ac, ref ap);

            float denom = 1 / (ABdotAB * ACdotAC - ABdotAC * ABdotAC);
            float u = (ACdotAC * ABdotAP - ABdotAC * ACdotAP) * denom;
            float v = (ABdotAB * ACdotAP - ABdotAC * ABdotAP) * denom;

            return (u >= -MathHelper.BigEpsilon) && (v >= -MathHelper.BigEpsilon) && (u + v <= 1 + MathHelper.BigEpsilon);

        }

        /// <summary>
        /// Determines the intersection between a ray and a triangle.
        /// </summary>
        /// <param name="ray">Ray to test.</param>
        /// <param name="maximumLength">Maximum length to travel in units of the direction's length.</param>
        /// <param name="sidedness">Sidedness of the triangle to test.</param>
        /// <param name="a">First vertex of the triangle.</param>
        /// <param name="b">Second vertex of the triangle.</param>
        /// <param name="c">Third vertex of the triangle.</param>
        /// <param name="hit">Hit data of the ray, if any</param>
        /// <returns>Whether or not the ray and triangle intersect.</returns>
        public static bool RayIntersectsTriangle(ref Ray ray, float maximumLength, TriangleSidedness sidedness, ref Vector3F a, ref Vector3F b, ref Vector3F c, out RayHit hit)
        {
            hit = new RayHit();
            Vector3F ab, ac;
            Vector3F.Subtract(ref b, ref a, out ab);
            Vector3F.Subtract(ref c, ref a, out ac);

            Vector3F.Cross(ref ab, ref ac, out hit.Normal);
            if (hit.Normal.LengthSquared() < MathHelper.Epsilon)
                return false; //Degenerate triangle!

            float d = -Vector3F.Dot(ref ray.Direction, ref hit.Normal);
            switch (sidedness)
            {
                case TriangleSidedness.DoubleSided:
                    if (d <= 0) //Pointing the wrong way.  Flip the normal.
                    {
                        Vector3F.Negate(ref hit.Normal, out hit.Normal);
                        d = -d;
                    }
                    break;
                case TriangleSidedness.Clockwise:
                    if (d <= 0) //Pointing the wrong way.  Can't hit.
                        return false;

                    break;
                case TriangleSidedness.Counterclockwise:
                    if (d >= 0) //Pointing the wrong way.  Can't hit.
                        return false;

                    Vector3F.Negate(ref hit.Normal, out hit.Normal);
                    d = -d;
                    break;
            }

            Vector3F ap;
            Vector3F.Subtract(ref ray.Position, ref a, out ap);

            hit.T = Vector3F.Dot(ref ap, ref hit.Normal) / d;
            if (hit.T < 0 || hit.T > maximumLength)
                return false;//Hit is behind origin, or too far away.

            Vector3F.Multiply(ref ray.Direction, hit.T, out hit.Location);
            Vector3F.Add(ref ray.Position, ref hit.Location, out hit.Location);

            // Compute barycentric coordinates
            Vector3F.Subtract(ref hit.Location, ref a, out ap);
            float ABdotAB = Vector3F.Dot(ref ab, ref ab);
            float ABdotAC = Vector3F.Dot(ref ab, ref ac);
            float ABdotAP = Vector3F.Dot(ref ab, ref ap);
            float ACdotAC = Vector3F.Dot(ref ac, ref ac);
            float ACdotAP = Vector3F.Dot(ref ac, ref ap);

            float denom = 1 / (ABdotAB * ACdotAC - ABdotAC * ABdotAC);
            float u = (ACdotAC * ABdotAP - ABdotAC * ACdotAP) * denom;
            float v = (ABdotAB * ACdotAP - ABdotAC * ABdotAP) * denom;

            return (u >= -MathHelper.BigEpsilon) && (v >= -MathHelper.BigEpsilon) && (u + v <= 1 + MathHelper.BigEpsilon);

        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="Ray"/> and a <see cref="BoundingBox"/>.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="box">The box to test.</param>
        /// <param name="distance">When the method completes, contains the distance of the intersection,
        /// or 0 if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool RayIntersectsBox(ref Ray ray, ref BoundingBox box, out float distance)
        {
            //Source: Real-Time Collision Detection by Christer Ericson
            //Reference: Page 179

            distance = 0f;
            float tmax = float.MaxValue;

            if (MathHelper.IsZero(ray.Direction.X))
            {
                if (ray.Position.X < box.Min.X || ray.Position.X > box.Max.X)
                {
                    distance = 0f;
                    return false;
                }
            }
            else
            {
                float inverse = 1.0f / ray.Direction.X;
                float t1 = (box.Min.X - ray.Position.X) * inverse;
                float t2 = (box.Max.X - ray.Position.X) * inverse;

                if (t1 > t2)
                {
                    float temp = t1;
                    t1 = t2;
                    t2 = temp;
                }

                distance = Math.Max(t1, distance);
                tmax = Math.Min(t2, tmax);

                if (distance > tmax)
                {
                    distance = 0f;
                    return false;
                }
            }

            if (MathHelper.IsZero(ray.Direction.Y))
            {
                if (ray.Position.Y < box.Min.Y || ray.Position.Y > box.Max.Y)
                {
                    distance = 0f;
                    return false;
                }
            }
            else
            {
                float inverse = 1.0f / ray.Direction.Y;
                float t1 = (box.Min.Y - ray.Position.Y) * inverse;
                float t2 = (box.Max.Y - ray.Position.Y) * inverse;

                if (t1 > t2)
                {
                    float temp = t1;
                    t1 = t2;
                    t2 = temp;
                }

                distance = Math.Max(t1, distance);
                tmax = Math.Min(t2, tmax);

                if (distance > tmax)
                {
                    distance = 0f;
                    return false;
                }
            }

            if (MathHelper.IsZero(ray.Direction.Z))
            {
                if (ray.Position.Z < box.Min.Z || ray.Position.Z > box.Max.Z)
                {
                    distance = 0f;
                    return false;
                }
            }
            else
            {
                float inverse = 1.0f / ray.Direction.Z;
                float t1 = (box.Min.Z - ray.Position.Z) * inverse;
                float t2 = (box.Max.Z - ray.Position.Z) * inverse;

                if (t1 > t2)
                {
                    float temp = t1;
                    t1 = t2;
                    t2 = temp;
                }

                distance = Math.Max(t1, distance);
                tmax = Math.Min(t2, tmax);

                if (distance > tmax)
                {
                    distance = 0f;
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="Ray"/> and a <see cref="Plane"/>.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="box">The box to test.</param>
        /// <param name="point">When the method completes, contains the point of intersection,
        /// or <see cref="Vector3F.Zero"/> if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool RayIntersectsBox(ref Ray ray, ref BoundingBox box, out Vector3F point)
        {
            float distance;
            if (!RayIntersectsBox(ref ray, ref box, out distance))
            {
                point = Vector3F.Zero;
                return false;
            }

            point = ray.Position + (ray.Direction * distance);
            return true;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="Ray"/> and a <see cref="BoundingSphere"/>.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="sphere">The sphere to test.</param>
        /// <param name="distance">When the method completes, contains the distance of the intersection,
        /// or 0 if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool RayIntersectsSphere(ref Ray ray, ref BoundingSphere sphere, out float distance)
        {
            //Source: Real-Time Collision Detection by Christer Ericson
            //Reference: Page 177

            Vector3F m;
            Vector3F.Subtract(ref ray.Position, ref sphere.Center, out m);

            float b = Vector3F.Dot(m, ray.Direction);
            float c = Vector3F.Dot(m, m) - (sphere.Radius * sphere.Radius);

            if (c > 0f && b > 0f)
            {
                distance = 0f;
                return false;
            }

            float discriminant = b * b - c;

            if (discriminant < 0f)
            {
                distance = 0f;
                return false;
            }

            distance = -b - (float)Math.Sqrt(discriminant);

            if (distance < 0f)
                distance = 0f;

            return true;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="Ray"/> and a <see cref="BoundingSphere"/>. 
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="sphere">The sphere to test.</param>
        /// <param name="point">When the method completes, contains the point of intersection,
        /// or <see cref="Vector3F.Zero"/> if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool RayIntersectsSphere(ref Ray ray, ref BoundingSphere sphere, out Vector3F point)
        {
            float distance;
            if (!RayIntersectsSphere(ref ray, ref sphere, out distance))
            {
                point = Vector3F.Zero;
                return false;
            }

            point = ray.Position + (ray.Direction * distance);
            return true;
        }

        ///<summary>
        /// Tests a ray against a sphere.
        ///</summary>
        ///<param name="ray">Ray to test.</param>
        ///<param name="spherePosition">Position of the sphere.</param>
        ///<param name="radius">Radius of the sphere.</param>
        ///<param name="maximumLength">Maximum length of the ray in units of the ray direction's length.</param>
        ///<param name="hit">Hit data of the ray, if any.</param>
        ///<returns>Whether or not the ray hits the sphere.</returns>
        public static bool RayIntersectsSphere(ref Ray ray, ref Vector3F spherePosition, float radius, float maximumLength, out RayHit hit)
        {
            Vector3F normalizedDirection;
            float length = ray.Direction.Length();
            Vector3F.Divide(ref ray.Direction, length, out normalizedDirection);
            maximumLength *= length;
            hit = new RayHit();
            Vector3F m;
            Vector3F.Subtract(ref ray.Position, ref spherePosition, out m);
            float b = Vector3F.Dot(m, normalizedDirection);
            float c = m.LengthSquared() - radius * radius;

            if (c > 0 && b > 0)
                return false;
            float discriminant = b * b - c;
            if (discriminant < 0)
                return false;

            hit.T = -b - (float)Math.Sqrt(discriminant);
            if (hit.T < 0)
                hit.T = 0;
            if (hit.T > maximumLength)
                return false;
            hit.T /= length;
            Vector3F.Multiply(ref normalizedDirection, hit.T, out hit.Location);
            Vector3F.Add(ref hit.Location, ref ray.Position, out hit.Location);
            Vector3F.Subtract(ref hit.Location, ref spherePosition, out hit.Normal);
            hit.Normal.Normalize();
            return true;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="Plane"/> and a point.
        /// </summary>
        /// <param name="plane">The plane to test.</param>
        /// <param name="point">The point to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static PlaneIntersectionType PlaneIntersectsPoint(ref Plane plane, ref Vector3F point)
        {
            float distance = Vector3F.Dot(ref plane.Normal, ref point);
            distance += plane.D;

            if (distance > 0f)
                return PlaneIntersectionType.Front;

            if (distance < 0f)
                return PlaneIntersectionType.Back;

            return PlaneIntersectionType.Intersecting;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="Plane"/> and a <see cref="Plane"/>.
        /// </summary>
        /// <param name="plane1">The first plane to test.</param>
        /// <param name="plane2">The second plane to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool PlaneIntersectsPlane(ref Plane plane1, ref Plane plane2)
        {
            Vector3F direction;
            Vector3F.Cross(ref plane1.Normal, ref plane2.Normal, out direction);

            //If direction is the zero vector, the planes are parallel and possibly
            //coincident. It is not an intersection. The dot product will tell us.
            float denominator = Vector3F.Dot(ref direction, ref direction);

            if (MathHelper.IsZero(denominator))
                return false;

            return true;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="Plane"/> and a <see cref="Plane"/>.
        /// </summary>
        /// <param name="plane1">The first plane to test.</param>
        /// <param name="plane2">The second plane to test.</param>
        /// <param name="line">When the method completes, contains the line of intersection
        /// as a <see cref="Ray"/>, or a zero ray if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        /// <remarks>
        /// Although a ray is set to have an origin, the ray returned by this method is really
        /// a line in three dimensions which has no real origin. The ray is considered valid when
        /// both the positive direction is used and when the negative direction is used.
        /// </remarks>
        public static bool PlaneIntersectsPlane(ref Plane plane1, ref Plane plane2, out Ray line)
        {
            //Source: Real-Time Collision Detection by Christer Ericson
            //Reference: Page 207

            Vector3F direction;
            Vector3F.Cross(ref plane1.Normal, ref plane2.Normal, out direction);

            //If direction is the zero vector, the planes are parallel and possibly
            //coincident. It is not an intersection. The dot product will tell us.
            float denominator = Vector3F.Dot(ref direction, ref direction);

            //We assume the planes are normalized, therefore the denominator
            //only serves as a parallel and coincident check. Otherwise we need
            //to divide the point by the denominator.
            if (MathHelper.IsZero(denominator))
            {
                line = new Ray();
                return false;
            }

            Vector3F point;
            Vector3F temp = plane1.D * plane2.Normal - plane2.D * plane1.Normal;
            Vector3F.Cross(ref temp, ref direction, out point);

            line.Position = point;
            line.Direction = direction;
            line.Direction.Normalize();

            return true;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="Plane"/> and a triangle.
        /// </summary>
        /// <param name="plane">The plane to test.</param>
        /// <param name="vertex1">The first vertex of the triangle to test.</param>
        /// <param name="vertex2">The second vertex of the triangle to test.</param>
        /// <param name="vertex3">The third vertex of the triangle to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static PlaneIntersectionType PlaneIntersectsTriangle(ref Plane plane, ref Vector3F vertex1, ref Vector3F vertex2, ref Vector3F vertex3)
        {
            //Source: Real-Time Collision Detection by Christer Ericson
            //Reference: Page 207

            PlaneIntersectionType test1 = PlaneIntersectsPoint(ref plane, ref vertex1);
            PlaneIntersectionType test2 = PlaneIntersectsPoint(ref plane, ref vertex2);
            PlaneIntersectionType test3 = PlaneIntersectsPoint(ref plane, ref vertex3);

            if (test1 == PlaneIntersectionType.Front && test2 == PlaneIntersectionType.Front && test3 == PlaneIntersectionType.Front)
                return PlaneIntersectionType.Front;

            if (test1 == PlaneIntersectionType.Back && test2 == PlaneIntersectionType.Back && test3 == PlaneIntersectionType.Back)
                return PlaneIntersectionType.Back;

            return PlaneIntersectionType.Intersecting;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="Plane"/> and a <see cref="BoundingBox"/>.
        /// </summary>
        /// <param name="plane">The plane to test.</param>
        /// <param name="box">The box to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static PlaneIntersectionType PlaneIntersectsBox(ref Plane plane, ref BoundingBox box)
        {
            //Source: Real-Time Collision Detection by Christer Ericson
            //Reference: Page 161

            Vector3F min;
            Vector3F max;

            max.X = (plane.Normal.X >= 0.0f) ? box.Min.X : box.Max.X;
            max.Y = (plane.Normal.Y >= 0.0f) ? box.Min.Y : box.Max.Y;
            max.Z = (plane.Normal.Z >= 0.0f) ? box.Min.Z : box.Max.Z;
            min.X = (plane.Normal.X >= 0.0f) ? box.Max.X : box.Min.X;
            min.Y = (plane.Normal.Y >= 0.0f) ? box.Max.Y : box.Min.Y;
            min.Z = (plane.Normal.Z >= 0.0f) ? box.Max.Z : box.Min.Z;

            float distance = Vector3F.Dot(ref plane.Normal, ref max);

            if (distance + plane.D > 0.0f)
                return PlaneIntersectionType.Front;

            distance = Vector3F.Dot(plane.Normal, min);

            if (distance + plane.D < 0.0f)
                return PlaneIntersectionType.Back;

            return PlaneIntersectionType.Intersecting;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="Plane"/> and a <see cref="BoundingSphere"/>.
        /// </summary>
        /// <param name="plane">The plane to test.</param>
        /// <param name="sphere">The sphere to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static PlaneIntersectionType PlaneIntersectsSphere(ref Plane plane, ref BoundingSphere sphere)
        {
            //Source: Real-Time Collision Detection by Christer Ericson
            //Reference: Page 160

            float distance = Vector3F.Dot(ref plane.Normal, ref sphere.Center);
            distance += plane.D;

            if (distance > sphere.Radius)
                return PlaneIntersectionType.Front;

            if (distance < -sphere.Radius)
                return PlaneIntersectionType.Back;

            return PlaneIntersectionType.Intersecting;
        }

        /* This implementation is wrong
        /// <summary>
        /// Determines whether there is an intersection between a <see cref="BoundingBox"/> and a triangle.
        /// </summary>
        /// <param name="box">The box to test.</param>
        /// <param name="vertex1">The first vertex of the triangle to test.</param>
        /// <param name="vertex2">The second vertex of the triangle to test.</param>
        /// <param name="vertex3">The third vertex of the triangle to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool BoxIntersectsTriangle(ref BoundingBox box, ref Vector3 vertex1, ref Vector3 vertex2, ref Vector3 vertex3)
        {
            if (BoxContainsPoint(ref box, ref vertex1) == ContainmentType.Contains)
                return true;

            if (BoxContainsPoint(ref box, ref vertex2) == ContainmentType.Contains)
                return true;

            if (BoxContainsPoint(ref box, ref vertex3) == ContainmentType.Contains)
                return true;

            return false;
        }
        */

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="BoundingBox"/> and a <see cref="BoundingBox"/>.
        /// </summary>
        /// <param name="box1">The first box to test.</param>
        /// <param name="box2">The second box to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool BoxIntersectsBox(ref BoundingBox box1, ref BoundingBox box2)
        {
            if (box1.Min.X > box2.Max.X || box2.Min.X > box1.Max.X)
                return false;

            if (box1.Min.Y > box2.Max.Y || box2.Min.Y > box1.Max.Y)
                return false;

            if (box1.Min.Z > box2.Max.Z || box2.Min.Z > box1.Max.Z)
                return false;

            return true;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="BoundingBox"/> and a <see cref="BoundingSphere"/>.
        /// </summary>
        /// <param name="box">The box to test.</param>
        /// <param name="sphere">The sphere to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool BoxIntersectsSphere(ref BoundingBox box, ref BoundingSphere sphere)
        {
            //Source: Real-Time Collision Detection by Christer Ericson
            //Reference: Page 166

            Vector3F.Clamp(ref sphere.Center, ref box.Min, ref box.Max, out Vector3F vector);
            float distance = Vector3F.DistanceSquared(sphere.Center, vector);

            return distance <= sphere.Radius * sphere.Radius;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="BoundingSphere"/> and a triangle.
        /// </summary>
        /// <param name="sphere">The sphere to test.</param>
        /// <param name="vertex1">The first vertex of the triangle to test.</param>
        /// <param name="vertex2">The second vertex of the triangle to test.</param>
        /// <param name="vertex3">The third vertex of the triangle to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool SphereIntersectsTriangle(ref BoundingSphere sphere, ref Vector3F vertex1, ref Vector3F vertex2, ref Vector3F vertex3)
        {
            //Source: Real-Time Collision Detection by Christer Ericson
            //Reference: Page 167

            Vector3F point;
            ClosestPointPointTriangle(ref sphere.Center, ref vertex1, ref vertex2, ref vertex3, out point);
            Vector3F v = point - sphere.Center;

            float dot = Vector3F.Dot(ref v, ref v);

            return dot <= sphere.Radius * sphere.Radius;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="BoundingSphere"/> and a <see cref="BoundingSphere"/>.
        /// </summary>
        /// <param name="sphere1">First sphere to test.</param>
        /// <param name="sphere2">Second sphere to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool SphereIntersectsSphere(ref BoundingSphere sphere1, ref BoundingSphere sphere2)
        {
            float radiisum = sphere1.Radius + sphere2.Radius;
            return Vector3F.DistanceSquared(sphere1.Center, sphere2.Center) <= radiisum * radiisum;
        }

        /// <summary>
        /// Finds the intersection between the given line and the given plane.
        /// </summary>
        /// <param name="a">First endpoint of segment defining the line.</param>
        /// <param name="b">Second endpoint of segment defining the line.</param>
        /// <param name="p">Plane for comparison.</param>
        /// <param name="t">Interval along line to intersection (A + t * AB).</param>
        /// <param name="q">Intersection point.</param>
        /// <returns>Whether or not the line intersects the plane.  If false, the line is parallel to the plane's surface.</returns>
        public static bool LineIntersectsPlane(ref Vector3F a, ref Vector3F b, ref Plane p, out float t, out Vector3F q)
        {
            Vector3F ab;
            Vector3F.Subtract(ref b, ref a, out ab);
            float denominator = Vector3F.Dot(ref p.Normal, ref ab);
            if (denominator < MathHelper.Epsilon && denominator > -MathHelper.Epsilon)
            {
                //Surface of plane and line are parallel (or very close to it).
                q = new Vector3F();
                t = float.MaxValue;
                return false;
            }

            float numerator = Vector3F.Dot(ref p.Normal, ref a);
            t = (p.D - numerator) / denominator;

            //Compute the intersection position.
            Vector3F.Multiply(ref ab, t, out q);
            Vector3F.Add(ref a, ref q, out q);
            return true;
        }


        /// <summary>
        /// Determines whether a <see cref="BoundingBox"/> contains a point.
        /// </summary>
        /// <param name="box">The box to test.</param>
        /// <param name="point">The point to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        public static ContainmentType BoxContainsPoint(ref BoundingBox box, ref Vector3F point)
        {
            if (box.Min.X <= point.X && box.Max.X >= point.X &&
                box.Min.Y <= point.Y && box.Max.Y >= point.Y &&
                box.Min.Z <= point.Z && box.Max.Z >= point.Z)
            {
                return ContainmentType.Contains;
            }

            return ContainmentType.Disjoint;
        }

        /* This implementation is wrong
        /// <summary>
        /// Determines whether a <see cref="BoundingBox"/> contains a triangle.
        /// </summary>
        /// <param name="box">The box to test.</param>
        /// <param name="vertex1">The first vertex of the triangle to test.</param>
        /// <param name="vertex2">The second vertex of the triangle to test.</param>
        /// <param name="vertex3">The third vertex of the triangle to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        public static ContainmentType BoxContainsTriangle(ref BoundingBox box, ref Vector3 vertex1, ref Vector3 vertex2, ref Vector3 vertex3)
        {
            ContainmentType test1 = BoxContainsPoint(ref box, ref vertex1);
            ContainmentType test2 = BoxContainsPoint(ref box, ref vertex2);
            ContainmentType test3 = BoxContainsPoint(ref box, ref vertex3);

            if (test1 == ContainmentType.Contains && test2 == ContainmentType.Contains && test3 == ContainmentType.Contains)
                return ContainmentType.Contains;

            if (test1 == ContainmentType.Contains || test2 == ContainmentType.Contains || test3 == ContainmentType.Contains)
                return ContainmentType.Intersects;

            return ContainmentType.Disjoint;
        }
        */

        /// <summary>
        /// Determines whether a <see cref="BoundingBox"/> contains a <see cref="BoundingBox"/>.
        /// </summary>
        /// <param name="box1">The first box to test.</param>
        /// <param name="box2">The second box to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        public static ContainmentType BoxContainsBox(ref BoundingBox box1, ref BoundingBox box2)
        {
            if (box1.Max.X < box2.Min.X || box1.Min.X > box2.Max.X)
                return ContainmentType.Disjoint;

            if (box1.Max.Y < box2.Min.Y || box1.Min.Y > box2.Max.Y)
                return ContainmentType.Disjoint;

            if (box1.Max.Z < box2.Min.Z || box1.Min.Z > box2.Max.Z)
                return ContainmentType.Disjoint;

            if (box1.Min.X <= box2.Min.X && (box2.Max.X <= box1.Max.X &&
                box1.Min.Y <= box2.Min.Y && box2.Max.Y <= box1.Max.Y) &&
                box1.Min.Z <= box2.Min.Z && box2.Max.Z <= box1.Max.Z)
            {
                return ContainmentType.Contains;
            }

            return ContainmentType.Intersects;
        }

        /// <summary>
        /// Determines whether a <see cref="BoundingBox"/> contains a <see cref="BoundingSphere"/>.
        /// </summary>
        /// <param name="box">The box to test.</param>
        /// <param name="sphere">The sphere to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        public static ContainmentType BoxContainsSphere(ref BoundingBox box, ref BoundingSphere sphere)
        {
            Vector3F vector;
            Vector3F.Clamp(ref sphere.Center, ref box.Min, ref box.Max, out vector);
            float distance = Vector3F.DistanceSquared(sphere.Center, vector);

            if (distance > sphere.Radius * sphere.Radius)
                return ContainmentType.Disjoint;

            if ((((box.Min.X + sphere.Radius <= sphere.Center.X) && (sphere.Center.X <= box.Max.X - sphere.Radius)) && ((box.Max.X - box.Min.X > sphere.Radius) &&
                (box.Min.Y + sphere.Radius <= sphere.Center.Y))) && (((sphere.Center.Y <= box.Max.Y - sphere.Radius) && (box.Max.Y - box.Min.Y > sphere.Radius)) &&
                (((box.Min.Z + sphere.Radius <= sphere.Center.Z) && (sphere.Center.Z <= box.Max.Z - sphere.Radius)) && (box.Max.X - box.Min.X > sphere.Radius))))
            {
                return ContainmentType.Contains;
            }

            return ContainmentType.Intersects;
        }

        /// <summary>
        /// Determines whether a <see cref="BoundingSphere"/> contains a point.
        /// </summary>
        /// <param name="sphere">The sphere to test.</param>
        /// <param name="point">The point to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        public static ContainmentType SphereContainsPoint(ref BoundingSphere sphere, ref Vector3F point)
        {
            if (Vector3F.DistanceSquared(point, sphere.Center) <= sphere.Radius * sphere.Radius)
                return ContainmentType.Contains;

            return ContainmentType.Disjoint;
        }

        /// <summary>
        /// Determines whether a <see cref="BoundingSphere"/> contains a triangle.
        /// </summary>
        /// <param name="sphere">The sphere to test.</param>
        /// <param name="vertex1">The first vertex of the triangle to test.</param>
        /// <param name="vertex2">The second vertex of the triangle to test.</param>
        /// <param name="vertex3">The third vertex of the triangle to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        public static ContainmentType SphereContainsTriangle(ref BoundingSphere sphere, ref Vector3F vertex1, ref Vector3F vertex2, ref Vector3F vertex3)
        {
            //Source: Jorgy343
            //Reference: None

            ContainmentType test1 = SphereContainsPoint(ref sphere, ref vertex1);
            ContainmentType test2 = SphereContainsPoint(ref sphere, ref vertex2);
            ContainmentType test3 = SphereContainsPoint(ref sphere, ref vertex3);

            if (test1 == ContainmentType.Contains && test2 == ContainmentType.Contains && test3 == ContainmentType.Contains)
                return ContainmentType.Contains;

            if (SphereIntersectsTriangle(ref sphere, ref vertex1, ref vertex2, ref vertex3))
                return ContainmentType.Intersects;

            return ContainmentType.Disjoint;
        }

        /// <summary>
        /// Determines whether a <see cref="BoundingSphere"/> contains a <see cref="BoundingBox"/>.
        /// </summary>
        /// <param name="sphere">The sphere to test.</param>
        /// <param name="box">The box to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        public static ContainmentType SphereContainsBox(ref BoundingSphere sphere, ref BoundingBox box)
        {
            Vector3F vector;

            if (!BoxIntersectsSphere(ref box, ref sphere))
                return ContainmentType.Disjoint;

            float radiussquared = sphere.Radius * sphere.Radius;
            vector.X = sphere.Center.X - box.Min.X;
            vector.Y = sphere.Center.Y - box.Max.Y;
            vector.Z = sphere.Center.Z - box.Max.Z;

            if (vector.LengthSquared() > radiussquared)
                return ContainmentType.Intersects;

            vector.X = sphere.Center.X - box.Max.X;
            vector.Y = sphere.Center.Y - box.Max.Y;
            vector.Z = sphere.Center.Z - box.Max.Z;

            if (vector.LengthSquared() > radiussquared)
                return ContainmentType.Intersects;

            vector.X = sphere.Center.X - box.Max.X;
            vector.Y = sphere.Center.Y - box.Min.Y;
            vector.Z = sphere.Center.Z - box.Max.Z;

            if (vector.LengthSquared() > radiussquared)
                return ContainmentType.Intersects;

            vector.X = sphere.Center.X - box.Min.X;
            vector.Y = sphere.Center.Y - box.Min.Y;
            vector.Z = sphere.Center.Z - box.Max.Z;

            if (vector.LengthSquared() > radiussquared)
                return ContainmentType.Intersects;

            vector.X = sphere.Center.X - box.Min.X;
            vector.Y = sphere.Center.Y - box.Max.Y;
            vector.Z = sphere.Center.Z - box.Min.Z;

            if (vector.LengthSquared() > radiussquared)
                return ContainmentType.Intersects;

            vector.X = sphere.Center.X - box.Max.X;
            vector.Y = sphere.Center.Y - box.Max.Y;
            vector.Z = sphere.Center.Z - box.Min.Z;

            if (vector.LengthSquared() > radiussquared)
                return ContainmentType.Intersects;

            vector.X = sphere.Center.X - box.Max.X;
            vector.Y = sphere.Center.Y - box.Min.Y;
            vector.Z = sphere.Center.Z - box.Min.Z;

            if (vector.LengthSquared() > radiussquared)
                return ContainmentType.Intersects;

            vector.X = sphere.Center.X - box.Min.X;
            vector.Y = sphere.Center.Y - box.Min.Y;
            vector.Z = sphere.Center.Z - box.Min.Z;

            if (vector.LengthSquared() > radiussquared)
                return ContainmentType.Intersects;

            return ContainmentType.Contains;
        }

        /// <summary>
        /// Determines whether a <see cref="BoundingSphere"/> contains a <see cref="BoundingSphere"/>.
        /// </summary>
        /// <param name="sphere1">The first sphere to test.</param>
        /// <param name="sphere2">The second sphere to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        public static ContainmentType SphereContainsSphere(ref BoundingSphere sphere1, ref BoundingSphere sphere2)
        {
            float distance = Vector3F.Distance(sphere1.Center, sphere2.Center);

            if (sphere1.Radius + sphere2.Radius < distance)
                return ContainmentType.Disjoint;

            if (sphere1.Radius - sphere2.Radius < distance)
                return ContainmentType.Intersects;

            return ContainmentType.Contains;
        }

        /// <summary>
        /// Determines if supplied point is within the triangle as defined by the provided vertices.
        /// </summary>
        /// <param name="vA">A vertex of the triangle.</param>
        /// <param name="vB">A vertex of the triangle.</param>
        /// <param name="vC">A vertex of the triangle.</param>
        /// <param name="p">The point for comparison against the triangle.</param>
        /// <returns>Whether or not the point is within the triangle.</returns>
        public static bool IsPointInsideTriangle(ref Vector3F vA, ref Vector3F vB, ref Vector3F vC, ref Vector3F p)
        {
            float u, v, w;
            MathHelper.BarycentricCoordinates(ref p, ref vA, ref vB, ref vC, out u, out v, out w);
            //Are the barycoords valid?
            return (u > -MathHelper.Epsilon) && (v > -MathHelper.Epsilon) && (w > -MathHelper.Epsilon);
        }

        /// <summary>
        /// Determines if supplied point is within the triangle as defined by the provided vertices.
        /// </summary>
        /// <param name="vA">A vertex of the triangle.</param>
        /// <param name="vB">A vertex of the triangle.</param>
        /// <param name="vC">A vertex of the triangle.</param>
        /// <param name="p">The point for comparison against the triangle.</param>
        /// <param name="margin">Extra area on the edges of the triangle to include.  Can be negative.</param>
        /// <returns>Whether or not the point is within the triangle.</returns>
        public static bool IsPointInsideTriangle(ref Vector3F vA, ref Vector3F vB, ref Vector3F vC, ref Vector3F p, float margin)
        {
            float u, v, w;
            MathHelper.BarycentricCoordinates(ref p, ref vA, ref vB, ref vC, out u, out v, out w);
            //Are the barycoords valid?
            return (u > -margin) && (v > -margin) && (w > -margin);
        }
    }
}
