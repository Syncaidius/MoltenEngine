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

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Molten
{
    /// <summary>
    /// Defines a frustum which can be used in frustum culling, zoom to Extents (zoom to fit) operations, 
    /// (matrix, frustum, camera) interchange, and many kind of intersection testing.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct BoundingFrustum : IEquatable<BoundingFrustum>
    {
        Matrix4F _pMatrix;
        Plane  _pNear;
        Plane  _pFar;
        Plane  _pLeft;
        Plane  _pRight;
        Plane  _pTop;
        Plane  _pBottom;

        /// <summary>
        /// Gets or sets the Matrix that describes this bounding frustum.
        /// </summary>
        public Matrix4F Matrix
        {
            get => _pMatrix;
            set
            {
                _pMatrix = value;
                GetPlanesFromMatrix(ref _pMatrix, out _pNear, out _pFar, out _pLeft, out _pRight, out _pTop, out _pBottom);
            }
        }
        /// <summary>
        /// Gets the near plane of the BoundingFrustum.
        /// </summary>
        public Plane Near => _pNear;

        /// <summary>
        /// Gets the far plane of the BoundingFrustum.
        /// </summary>
        public Plane Far => _pFar;

        /// <summary>
        /// Gets the left plane of the BoundingFrustum.
        /// </summary>
        public Plane Left => _pLeft;

        /// <summary>
        /// Gets the right plane of the BoundingFrustum.
        /// </summary>
        public Plane Right => _pRight;

        /// <summary>
        /// Gets the top plane of the BoundingFrustum.
        /// </summary>
        public Plane Top => _pTop;

        /// <summary>
        /// Gets the bottom plane of the BoundingFrustum.
        /// </summary>
        public Plane Bottom => _pBottom;

        /// <summary>
        /// Creates a new instance of BoundingFrustum.
        /// </summary>
        /// <param name="matrix">Combined matrix that usually takes view × projection matrix.</param>
        public BoundingFrustum(Matrix4F matrix)
        {
            _pMatrix = matrix;
            GetPlanesFromMatrix(ref _pMatrix, out _pNear, out _pFar, out _pLeft, out _pRight, out _pTop, out _pBottom);
        }

        public override int GetHashCode()
        {
            return _pMatrix.GetHashCode();
        }

        /// <summary>
        /// Determines whether the specified <see cref="BoundingFrustum"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="BoundingFrustum"/> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="BoundingFrustum"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(ref BoundingFrustum other)
        {
            return this._pMatrix == other._pMatrix;
        }

        /// <summary>
        /// Determines whether the specified <see cref="BoundingFrustum"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="BoundingFrustum"/> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="BoundingFrustum"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(BoundingFrustum other)
        {
            return Equals(ref other);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if(!(obj is BoundingFrustum))
                return false;

            var strongValue = (BoundingFrustum)obj;
            return Equals(ref strongValue);
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(BoundingFrustum left, BoundingFrustum right)
        {
            return left.Equals(ref right);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(BoundingFrustum left, BoundingFrustum right)
        {
            return !left.Equals(ref right);
        }

        /// <summary>
        /// Returns one of the 6 planes related to this frustum.
        /// </summary>
        /// <param name="index">Plane index where 0 fro Left, 1 for Right, 2 for Top, 3 for Bottom, 4 for Near, 5 for Far</param>
        /// <returns></returns>
        public Plane GetPlane(int index)
        {
            switch (index)
            {
                case 0: return _pLeft;
                case 1: return _pRight;
                case 2: return _pTop;
                case 3: return _pBottom;
                case 4: return _pNear;
                case 5: return _pFar;
                default:
                    return new Plane();
            }
        }

        private static void GetPlanesFromMatrix(ref Matrix4F matrix, out Plane near, out Plane far, out Plane left, out Plane right, out Plane top, out Plane bottom)
        {
            //http://www.chadvernon.com/blog/resources/directx9/frustum-culling/

            // Left plane
            left.Normal.X = matrix.M14 + matrix.M11;
            left.Normal.Y = matrix.M24 + matrix.M21;
            left.Normal.Z = matrix.M34 + matrix.M31;
            left.D = matrix.M44 + matrix.M41;
            left.Normalize();

            // Right plane
            right.Normal.X = matrix.M14 - matrix.M11;
            right.Normal.Y = matrix.M24 - matrix.M21;
            right.Normal.Z = matrix.M34 - matrix.M31;
            right.D = matrix.M44 - matrix.M41;
            right.Normalize();

            // Top plane
            top.Normal.X = matrix.M14 - matrix.M12;
            top.Normal.Y = matrix.M24 - matrix.M22;
            top.Normal.Z = matrix.M34 - matrix.M32;
            top.D = matrix.M44 - matrix.M42;
            top.Normalize();

            // Bottom plane
            bottom.Normal.X = matrix.M14 + matrix.M12;
            bottom.Normal.Y = matrix.M24 + matrix.M22;
            bottom.Normal.Z = matrix.M34 + matrix.M32;
            bottom.D = matrix.M44 + matrix.M42;
            bottom.Normalize();

            // Near plane
            near.Normal.X = matrix.M13;
            near.Normal.Y = matrix.M23;
            near.Normal.Z = matrix.M33;
            near.D = matrix.M43;
            near.Normalize();

            // Far plane
            far.Normal.X = matrix.M14 - matrix.M13;
            far.Normal.Y = matrix.M24 - matrix.M23;
            far.Normal.Z = matrix.M34 - matrix.M33;
            far.D = matrix.M44 - matrix.M43;
            far.Normalize();
        }

        private static Vector3F Get3PlanesInterPoint(ref Plane p1, ref Plane p2, ref Plane p3)
        {
            //P = -d1 * N2xN3 / N1.N2xN3 - d2 * N3xN1 / N2.N3xN1 - d3 * N1xN2 / N3.N1xN2 
            Vector3F v =
                -p1.D * Vector3F.Cross(p2.Normal, p3.Normal) / Vector3F.Dot(p1.Normal, Vector3F.Cross(p2.Normal, p3.Normal))
                - p2.D * Vector3F.Cross(p3.Normal, p1.Normal) / Vector3F.Dot(p2.Normal, Vector3F.Cross(p3.Normal, p1.Normal))
                - p3.D * Vector3F.Cross(p1.Normal, p2.Normal) / Vector3F.Dot(p3.Normal, Vector3F.Cross(p1.Normal, p2.Normal));

            return v;
        }

        /// <summary>
        /// Creates a new frustum relaying on perspective camera parameters
        /// </summary>
        /// <param name="cameraPos">The camera pos.</param>
        /// <param name="lookDir">The look dir.</param>
        /// <param name="upDir">Up dir.</param>
        /// <param name="fov">The fov.</param>
        /// <param name="znear">The znear.</param>
        /// <param name="zfar">The zfar.</param>
        /// <param name="aspect">The aspect.</param>
        /// <returns>The bounding frustum calculated from perspective camera</returns>
        public static BoundingFrustum FromCamera(Vector3F cameraPos, Vector3F lookDir, Vector3F upDir, float fov, float znear, float zfar, float aspect)
        {
            //http://knol.google.com/k/view-frustum

            lookDir = Vector3F.Normalize(lookDir);
            upDir = Vector3F.Normalize(upDir);

            Vector3F nearCenter = cameraPos + lookDir * znear;
            Vector3F farCenter = cameraPos + lookDir * zfar;
            float nearHalfHeight = znear * float.Tan(fov / 2f);
            float farHalfHeight = zfar * float.Tan(fov / 2f);
            float nearHalfWidth = nearHalfHeight * aspect;
            float farHalfWidth = farHalfHeight * aspect;

            Vector3F rightDir = Vector3F.Normalize(Vector3F.Cross(upDir, lookDir));
            Vector3F Near1 = nearCenter - nearHalfHeight * upDir + nearHalfWidth * rightDir;
            Vector3F Near2 = nearCenter + nearHalfHeight * upDir + nearHalfWidth * rightDir;
            Vector3F Near3 = nearCenter + nearHalfHeight * upDir - nearHalfWidth * rightDir;
            Vector3F Near4 = nearCenter - nearHalfHeight * upDir - nearHalfWidth * rightDir;
            Vector3F Far1 = farCenter - farHalfHeight * upDir + farHalfWidth * rightDir;
            Vector3F Far2 = farCenter + farHalfHeight * upDir + farHalfWidth * rightDir;
            Vector3F Far3 = farCenter + farHalfHeight * upDir - farHalfWidth * rightDir;
            Vector3F Far4 = farCenter - farHalfHeight * upDir - farHalfWidth * rightDir;

            var result = new BoundingFrustum();
            result._pNear = new Plane(Near1, Near2, Near3);
            result._pFar = new Plane(Far3, Far2, Far1);
            result._pLeft = new Plane(Near4, Near3, Far3);
            result._pRight = new Plane(Far1, Far2, Near2);
            result._pTop = new Plane(Near2, Far2, Far3);
            result._pBottom = new Plane(Far4, Far1, Near1);

            result._pNear.Normalize();
            result._pFar.Normalize();
            result._pLeft.Normalize();
            result._pRight.Normalize();
            result._pTop.Normalize();
            result._pBottom.Normalize();

            result._pMatrix = Matrix4F.LookAtLH(cameraPos, cameraPos + lookDir * 10, upDir) * Matrix4F.PerspectiveFovLH(fov, aspect, znear, zfar);

            return result;
        }

        /// <summary>
        /// Creates a new frustum relaying on perspective camera parameters
        /// </summary>
        /// <param name="cameraParams">The camera params.</param>
        /// <returns>The bounding frustum from camera params</returns>
        public static BoundingFrustum FromCamera(FrustumCameraParams cameraParams)
        {
            return FromCamera(cameraParams.Position, cameraParams.LookAtDir, cameraParams.UpDir, cameraParams.FOV, cameraParams.ZNear, cameraParams.ZFar, cameraParams.AspectRatio);
        }

        /// <summary>
        /// Returns the 8 corners of the frustum, element0 is Near1 (near right down corner)
        /// , element1 is Near2 (near right top corner)
        /// , element2 is Near3 (near Left top corner)
        /// , element3 is Near4 (near Left down corner)
        /// , element4 is Far1 (far right down corner)
        /// , element5 is Far2 (far right top corner)
        /// , element6 is Far3 (far left top corner)
        /// , element7 is Far4 (far left down corner)
        /// </summary>
        /// <returns>The 8 corners of the frustum</returns>
        public Vector3F[] GetCorners()
        {
            var corners = new Vector3F[8];
            GetCorners(corners);
            return corners;
        }

        /// <summary>
        /// Returns the 8 corners of the frustum, element0 is Near1 (near right down corner)
        /// , element1 is Near2 (near right top corner)
        /// , element2 is Near3 (near Left top corner)
        /// , element3 is Near4 (near Left down corner)
        /// , element4 is Far1 (far right down corner)
        /// , element5 is Far2 (far right top corner)
        /// , element6 is Far3 (far left top corner)
        /// , element7 is Far4 (far left down corner)
        /// </summary>
        /// <returns>The 8 corners of the frustum</returns>
        public void GetCorners(Vector3F[] corners)
        {
            corners[0] = Get3PlanesInterPoint(ref _pNear, ref  _pBottom, ref  _pRight);    //Near1
            corners[1] = Get3PlanesInterPoint(ref _pNear, ref  _pTop, ref  _pRight);       //Near2
            corners[2] = Get3PlanesInterPoint(ref _pNear, ref  _pTop, ref  _pLeft);        //Near3
            corners[3] = Get3PlanesInterPoint(ref _pNear, ref  _pBottom, ref  _pLeft);     //Near3
            corners[4] = Get3PlanesInterPoint(ref _pFar, ref  _pBottom, ref  _pRight);    //Far1
            corners[5] = Get3PlanesInterPoint(ref _pFar, ref  _pTop, ref  _pRight);       //Far2
            corners[6] = Get3PlanesInterPoint(ref _pFar, ref  _pTop, ref  _pLeft);        //Far3
            corners[7] = Get3PlanesInterPoint(ref _pFar, ref  _pBottom, ref  _pLeft);     //Far3
        }

        /// <summary>
        /// Extracts perspective camera parameters from the frustum, doesn't work with orthographic frustums.
        /// </summary>
        /// <returns>Perspective camera parameters from the frustum</returns>
        public FrustumCameraParams GetCameraParams()
        {
            var corners = GetCorners();
            var cameraParam = new FrustumCameraParams();
            cameraParam.Position = Get3PlanesInterPoint(ref _pRight, ref _pTop, ref _pLeft);
            cameraParam.LookAtDir = _pNear.Normal;
            cameraParam.UpDir = Vector3F.Normalize(Vector3F.Cross(_pRight.Normal, _pNear.Normal));
            cameraParam.FOV = (float.Pi / 2.0f - MathF.Acos(Vector3F.Dot(_pNear.Normal, _pTop.Normal))) * 2f;
            cameraParam.AspectRatio = (corners[6] - corners[5]).Length() / (corners[4] - corners[5]).Length();
            cameraParam.ZNear = (cameraParam.Position + (_pNear.Normal * _pNear.D)).Length();
            cameraParam.ZFar = (cameraParam.Position + (_pFar.Normal * _pFar.D)).Length();
            return cameraParam;
        }

        /// <summary>
        /// Checks whether a point lay inside, intersects or lay outside the frustum.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns>Type of the containment</returns>
        public ContainmentType Contains(ref Vector3F point)
        {
            var result = PlaneIntersectionType.Front;
            var planeResult = PlaneIntersectionType.Front;
            for (int i = 0; i < 6; i++)
            {
                switch (i)
                {
                    case 0: planeResult = _pNear.Intersects(ref point); break;
                    case 1: planeResult = _pFar.Intersects(ref point); break;
                    case 2: planeResult = _pLeft.Intersects(ref point); break;
                    case 3: planeResult = _pRight.Intersects(ref point); break;
                    case 4: planeResult = _pTop.Intersects(ref point); break;
                    case 5: planeResult = _pBottom.Intersects(ref point); break;
                }
                switch (planeResult)
                {
                    case PlaneIntersectionType.Back:
                        return ContainmentType.Disjoint;
                    case PlaneIntersectionType.Intersecting:
                        result = PlaneIntersectionType.Intersecting;
                        break;
                }
            }
            switch (result)
            {
                case PlaneIntersectionType.Intersecting: return ContainmentType.Intersects;
                default: return ContainmentType.Contains;
            }
        }

        /// <summary>
        /// Checks whether a point lay inside, intersects or lay outside the frustum.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns>Type of the containment</returns>
        public ContainmentType Contains(Vector3F point)
        {
            return Contains(ref point);
        }

        /// <summary>
        /// Checks whether a group of points lay totally inside the frustum (Contains), or lay partially inside the frustum (Intersects), or lay outside the frustum (Disjoint).
        /// </summary>
        /// <param name="points">The points.</param>
        /// <returns>Type of the containment</returns>
        public ContainmentType Contains(Vector3F[] points)
        {
            throw new NotImplementedException();
            /* TODO: (PMin) This method is wrong, does not calculate case where only plane from points is intersected
            var containsAny = false;
            var containsAll = true;
            for (int i = 0; i < points.Length; i++)
            {
                switch (Contains(ref points[i]))
                {
                    case ContainmentType.Contains:
                    case ContainmentType.Intersects:
                        containsAny = true;
                        break;
                    case ContainmentType.Disjoint:
                        containsAll = false;
                        break;
                }
            }
            if (containsAny)
            {
                if (containsAll)
                    return ContainmentType.Contains;
                else
                    return ContainmentType.Intersects;
            }
            else
                return ContainmentType.Disjoint;  */
        }
        /// <summary>
        /// Checks whether a group of points lay totally inside the frustum (Contains), or lay partially inside the frustum (Intersects), or lay outside the frustum (Disjoint).
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="result">Type of the containment.</param>
        public void Contains(Vector3F[] points, out ContainmentType result)
        {
            result = Contains(points);
        }

        private void GetBoxToPlanePVertexNVertex(ref BoundingBox box, ref Vector3F planeNormal, out Vector3F p, out Vector3F n)
        {
            p = box.Min;
            if (planeNormal.X >= 0)
                p.X = box.Max.X;
            if (planeNormal.Y >= 0)
                p.Y = box.Max.Y;
            if (planeNormal.Z >= 0)
                p.Z = box.Max.Z;

            n = box.Max;
            if (planeNormal.X >= 0)
                n.X = box.Min.X;
            if (planeNormal.Y >= 0)
                n.Y = box.Min.Y;
            if (planeNormal.Z >= 0)
                n.Z = box.Min.Z;
        }

        /// <summary>
        /// Determines the intersection relationship between the frustum and a bounding box.
        /// </summary>
        /// <param name="box">The box.</param>
        /// <returns>Type of the containment</returns>
        public ContainmentType Contains(ref BoundingBox box)
        {
            Vector3F p, n;
            Plane plane;
            var result = ContainmentType.Contains;
            for (int i = 0; i < 6; i++)
            {
                plane = GetPlane(i);
                GetBoxToPlanePVertexNVertex(ref box, ref plane.Normal, out p, out n);
                if (CollisionHelper.PlaneIntersectsPoint(ref plane, ref p) == PlaneIntersectionType.Back)
                    return ContainmentType.Disjoint;

                if (CollisionHelper.PlaneIntersectsPoint(ref plane, ref n) == PlaneIntersectionType.Back)
                    result = ContainmentType.Intersects;
            }
            return result;
        }

        /// <summary>
        /// Determines the intersection relationship between the frustum and a bounding box.
        /// </summary>
        /// <param name="box">The box.</param>
        /// <returns>Type of the containment</returns>
        public ContainmentType Contains(BoundingBox box)
        {
            return Contains(ref box);
        }

        /// <summary>
        /// Determines the intersection relationship between the frustum and a bounding box.
        /// </summary>
        /// <param name="box">The box.</param>
        /// <param name="result">Type of the containment.</param>
        public void Contains(ref BoundingBox box, out ContainmentType result)
        {
            result = Contains(ref box);
        }

        /// <summary>
        /// Determines the intersection relationship between the frustum and a bounding sphere.
        /// </summary>
        /// <param name="sphere">The sphere.</param>
        /// <returns>Type of the containment</returns>
        public ContainmentType Contains(ref BoundingSphere sphere)
        {
            var result = PlaneIntersectionType.Front;
            var planeResult = PlaneIntersectionType.Front;
            for (int i = 0; i < 6; i++)
            {
                switch (i)
                {
                    case 0: planeResult = _pNear.Intersects(ref sphere); break;
                    case 1: planeResult = _pFar.Intersects(ref sphere); break;
                    case 2: planeResult = _pLeft.Intersects(ref sphere); break;
                    case 3: planeResult = _pRight.Intersects(ref sphere); break;
                    case 4: planeResult = _pTop.Intersects(ref sphere); break;
                    case 5: planeResult = _pBottom.Intersects(ref sphere); break;
                }
                switch (planeResult)
                {
                    case PlaneIntersectionType.Back:
                        return ContainmentType.Disjoint;
                    case PlaneIntersectionType.Intersecting:
                        result = PlaneIntersectionType.Intersecting;
                        break;
                }
            }
            switch (result)
            {
                case PlaneIntersectionType.Intersecting: return ContainmentType.Intersects;
                default: return ContainmentType.Contains;
            }
        }

        /// <summary>
        /// Determines the intersection relationship between the frustum and a bounding sphere.
        /// </summary>
        /// <param name="sphere">The sphere.</param>
        /// <returns>Type of the containment</returns>
        public ContainmentType Contains(BoundingSphere sphere)
        {
            return Contains(ref sphere);
        }

        /// <summary>
        /// Determines the intersection relationship between the frustum and a bounding sphere.
        /// </summary>
        /// <param name="sphere">The sphere.</param>
        /// <param name="result">Type of the containment.</param>
        public void Contains(ref BoundingSphere sphere, out ContainmentType result)
        {
            result = Contains(ref sphere);
        }

        /// <summary>
        /// Determines the intersection relationship between the frustum and another bounding frustum.
        /// </summary>
        /// <param name="frustum">The frustum.</param>
        /// <returns>Type of the containment</returns>
        public bool Contains(ref BoundingFrustum frustum)
        {
            return Contains(frustum.GetCorners()) != ContainmentType.Disjoint;
        }

        /// <summary>
        /// Determines the intersection relationship between the frustum and another bounding frustum.
        /// </summary>
        /// <param name="frustum">The frustum.</param>
        /// <returns>Type of the containment</returns>
        public bool Contains(BoundingFrustum frustum)
        {
            return Contains(ref frustum);
        }

        /// <summary>
        /// Determines the intersection relationship between the frustum and another bounding frustum.
        /// </summary>
        /// <param name="frustum">The frustum.</param>
        /// <param name="result">Type of the containment.</param>
        public void Contains(ref BoundingFrustum frustum, out bool result)
        {
            result = Contains(frustum.GetCorners()) != ContainmentType.Disjoint;
        }

        /// <summary>
        /// Checks whether the current BoundingFrustum intersects a BoundingSphere.
        /// </summary>
        /// <param name="sphere">The sphere.</param>
        /// <returns>Type of the containment</returns>
        public bool Intersects(ref BoundingSphere sphere)
        {
            return Contains(ref sphere) != ContainmentType.Disjoint;
        }

        /// <summary>
        /// Checks whether the current BoundingFrustum intersects a BoundingSphere.
        /// </summary>
        /// <param name="sphere">The sphere.</param>
        /// <param name="result">Set to <c>true</c> if the current BoundingFrustum intersects a BoundingSphere.</param>
        public void Intersects(ref BoundingSphere sphere, out bool result)
        {
            result = Contains(ref sphere) != ContainmentType.Disjoint;
        }
        /// <summary>
        /// Checks whether the current BoundingFrustum intersects a BoundingBox.
        /// </summary>
        /// <param name="box">The box.</param>
        /// <returns><c>true</c> if the current BoundingFrustum intersects a BoundingSphere.</returns>
        public bool Intersects(ref BoundingBox box)
        {
            return Contains(ref box) != ContainmentType.Disjoint;
        }

        /// <summary>
        /// Checks whether the current BoundingFrustum intersects a BoundingBox.
        /// </summary>
        /// <param name="box">The box.</param>
        /// <param name="result"><c>true</c> if the current BoundingFrustum intersects a BoundingSphere.</param>
        public void Intersects(ref BoundingBox box, out bool result)
        {
            result = Contains(ref box) != ContainmentType.Disjoint;
        }

        private PlaneIntersectionType PlaneIntersectsPoints(ref Plane plane, Vector3F[] points)
        {
            var result = CollisionHelper.PlaneIntersectsPoint(ref plane, ref points[0]);
            for (int i = 1; i < points.Length; i++)
                if (CollisionHelper.PlaneIntersectsPoint(ref plane, ref points[i]) != result)
                    return PlaneIntersectionType.Intersecting;
            return result;
        }

        /// <summary>
        /// Checks whether the current BoundingFrustum intersects the specified Plane.
        /// </summary>
        /// <param name="plane">The plane.</param>
        /// <returns>Plane intersection type.</returns>
        public PlaneIntersectionType Intersects(ref Plane plane)
        {
            return PlaneIntersectsPoints(ref plane, GetCorners());
        }

        /// <summary>
        /// Checks whether the current BoundingFrustum intersects the specified Plane.
        /// </summary>
        /// <param name="plane">The plane.</param>
        /// <param name="result">Plane intersection type.</param>
        public void Intersects(ref Plane plane, out PlaneIntersectionType result)
        {
            result = PlaneIntersectsPoints(ref plane, GetCorners());
        }

        /// <summary>
        /// Get the width of the frustum at specified depth.
        /// </summary>
        /// <param name="depth">the depth at which to calculate frustum width.</param>
        /// <returns>With of the frustum at the specified depth</returns>
        public float GetWidthAtDepth(float depth)
        {
            float hAngle = float.Pi / 2.0f - MathF.Acos(Vector3F.Dot(_pNear.Normal, _pLeft.Normal));
            return MathF.Tan(hAngle) * depth * 2f;
        }

        /// <summary>
        /// Get the height of the frustum at specified depth.
        /// </summary>
        /// <param name="depth">the depth at which to calculate frustum height.</param>
        /// <returns>Height of the frustum at the specified depth</returns>
        public float GetHeightAtDepth(float depth)
        {
            float vAngle = float.Pi / 2.0f - MathF.Acos(Vector3F.Dot(_pNear.Normal, _pTop.Normal));
            return MathF.Tan(vAngle) * depth * 2f;
        }

        private BoundingFrustum GetInsideOutClone()
        {
            var frustum = this;
            frustum._pNear.Normal = -frustum._pNear.Normal;
            frustum._pFar.Normal = -frustum._pFar.Normal;
            frustum._pLeft.Normal = -frustum._pLeft.Normal;
            frustum._pRight.Normal = -frustum._pRight.Normal;
            frustum._pTop.Normal = -frustum._pTop.Normal;
            frustum._pBottom.Normal = -frustum._pBottom.Normal;
            return frustum;
        }

        /// <summary>
        /// Checks whether the current BoundingFrustum intersects the specified Ray.
        /// </summary>
        /// <param name="ray">The ray.</param>
        /// <returns><c>true</c> if the current BoundingFrustum intersects the specified Ray.</returns>
        public bool Intersects(ref Ray ray)
        {
            float? inDist, outDist;
            return Intersects(ref ray, out inDist, out outDist);
        }

        /// <summary>
        /// Checks whether the current BoundingFrustum intersects the specified Ray.
        /// </summary>
        /// <param name="ray">The Ray to check for intersection with.</param>
        /// <param name="inDistance">The distance at which the ray enters the frustum if there is an intersection and the ray starts outside the frustum.</param>
        /// <param name="outDistance">The distance at which the ray exits the frustum if there is an intersection.</param>
        /// <returns><c>true</c> if the current BoundingFrustum intersects the specified Ray.</returns>
        public bool Intersects(ref Ray ray, out float? inDistance, out float? outDistance)
        {
            if (Contains(ray.Position) != ContainmentType.Disjoint)
            {
                float nearstPlaneDistance = float.MaxValue;
                for (int i = 0; i < 6; i++)
                {
                    var plane = GetPlane(i);
                    float distance;
                    if (CollisionHelper.RayIntersectsPlane(ref ray, ref plane, out distance) && distance < nearstPlaneDistance)
                    {
                        nearstPlaneDistance = distance;
                    }
                }

                inDistance = nearstPlaneDistance;
                outDistance = null;
                return true;
            }
            else
            {
                //We will find the two points at which the ray enters and exists the frustum
                //These two points make a line which center inside the frustum if the ray intersects it
                //Or outside the frustum if the ray intersects frustum planes outside it.
                float minDist = float.MaxValue;
                float maxDist = float.MinValue;
                for (int i = 0; i < 6; i++)
                {
                    var plane = GetPlane(i);
                    float distance;
                    if (CollisionHelper.RayIntersectsPlane(ref ray, ref plane, out distance))
                    {
                        minDist = float.Min(minDist, distance);
                        maxDist = float.Max(maxDist, distance);
                    }
                }

                Vector3F minPoint = ray.Position + ray.Direction * minDist;
                Vector3F maxPoint = ray.Position + ray.Direction * maxDist;
                Vector3F center = (minPoint + maxPoint) / 2f;
                if (Contains(ref center) != ContainmentType.Disjoint)
                {
                    inDistance = minDist;
                    outDistance = maxDist;
                    return true;
                }
                else
                {
                    inDistance = null;
                    outDistance = null;
                    return false;
                }
            }
        }

        /// <summary>
        /// Get the distance which when added to camera position along the lookat direction will do the effect of zoom to extents (zoom to fit) operation,
        /// so all the passed points will fit in the current view.
        /// if the returned value is positive, the camera will move toward the lookat direction (ZoomIn).
        /// if the returned value is negative, the camera will move in the reverse direction of the lookat direction (ZoomOut).
        /// </summary>
        /// <param name="points">The points.</param>
        /// <returns>The zoom to fit distance</returns>
        public float GetZoomToExtentsShiftDistance(Vector3F[] points)
        {
            float vAngle = float.Pi / 2.0f - MathF.Acos(Vector3F.Dot(_pNear.Normal, _pTop.Normal));
            float vSin = MathF.Sin(vAngle);
            float hAngle = float.Pi / 2.0f - MathF.Acos(Vector3F.Dot(_pNear.Normal, _pLeft.Normal));
            float hSin = MathF.Sin(hAngle);
            float horizontalToVerticalMapping = vSin / hSin;

            var ioFrustrum = GetInsideOutClone();

            float maxPointDist = float.MinValue;
            for (int i = 0; i < points.Length; i++)
            {
                float pointDist = CollisionHelper.DistancePlanePoint(ref ioFrustrum._pTop, ref points[i]);
                pointDist = Math.Max(pointDist, CollisionHelper.DistancePlanePoint(ref ioFrustrum._pBottom, ref points[i]));
                pointDist = Math.Max(pointDist, CollisionHelper.DistancePlanePoint(ref ioFrustrum._pLeft, ref points[i]) * horizontalToVerticalMapping);
                pointDist = Math.Max(pointDist, CollisionHelper.DistancePlanePoint(ref ioFrustrum._pRight, ref points[i]) * horizontalToVerticalMapping);

                maxPointDist = Math.Max(maxPointDist, pointDist);
            }
            return -maxPointDist / vSin;
        }

        /// <summary>
        /// Get the distance which when added to camera position along the lookat direction will do the effect of zoom to extents (zoom to fit) operation,
        /// so all the passed points will fit in the current view.
        /// if the returned value is positive, the camera will move toward the lookat direction (ZoomIn).
        /// if the returned value is negative, the camera will move in the reverse direction of the lookat direction (ZoomOut).
        /// </summary>
        /// <param name="boundingBox">The bounding box.</param>
        /// <returns>The zoom to fit distance</returns>
        public float GetZoomToExtentsShiftDistance(ref BoundingBox boundingBox)
        {
            return GetZoomToExtentsShiftDistance(boundingBox.GetCorners());
        }

        /// <summary>
        /// Get the vector shift which when added to camera position will do the effect of zoom to extents (zoom to fit) operation,
        /// so all the passed points will fit in the current view.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <returns>The zoom to fit vector</returns>
        public Vector3F GetZoomToExtentsShiftVector(Vector3F[] points)
        {
            return GetZoomToExtentsShiftDistance(points) * _pNear.Normal;
        }
        /// <summary>
        /// Get the vector shift which when added to camera position will do the effect of zoom to extents (zoom to fit) operation,
        /// so all the passed points will fit in the current view.
        /// </summary>
        /// <param name="boundingBox">The bounding box.</param>
        /// <returns>The zoom to fit vector</returns>
        public Vector3F GetZoomToExtentsShiftVector(ref BoundingBox boundingBox)
        {
            return GetZoomToExtentsShiftDistance(boundingBox.GetCorners()) * _pNear.Normal;
        }

        /// <summary>
        /// Indicate whether the current BoundingFrustrum is Orthographic.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the current BoundingFrustrum is Orthographic; otherwise, <c>false</c>.
        /// </value>
        public bool IsOrthographic => (_pLeft.Normal == -_pRight.Normal) && (_pTop.Normal == -_pBottom.Normal);
    }
}
