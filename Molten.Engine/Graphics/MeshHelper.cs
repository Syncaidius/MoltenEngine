namespace Molten.Graphics
{
    /// <summary>A collection of helper methods for creating indexed primitive meshes.</summary>
    public static class MeshHelper
    {
        /// <summary>Calculates the normal of a triangular area from 3 points.</summary>
        /// <param name="p1">The first point.</param>
        /// <param name="p2">The second point.</param>
        /// <param name="p3">The third point.</param>
        /// <returns>The resultant normal.</returns>
        public static Vector3F CalculateNormal(Vector3F p1, Vector3F p2, Vector3F p3)
        {
            //calculate normals
            Vector3F vec1 = p3 - p1;
            Vector3F vec2 = p2 - p1;
            Vector3F normal = Vector3F.Cross(vec1, vec2);

            return normal;
        }

        public static IIndexedMesh<GBufferVertex> Cube(RenderService renderer)
        {
            Vector3F nFront = new Vector3F(0, 0, -1);
            Vector3F nBack = new Vector3F(0, 0, 1);
            Vector3F nTop = new Vector3F(0, 1, 0);
            Vector3F nBottom = new Vector3F(0, -1, 0);
            Vector3F nLeft = new Vector3F(-1, 0, 0);
            Vector3F nRight = new Vector3F(1, 0, 0);

            GBufferVertex[] vertices = new GBufferVertex[]{
               new GBufferVertex(new Vector3F(-0.5f,-0.5f,-0.5f),nFront, new Vector2F(0,1)), //front
               new GBufferVertex(new Vector3F(-0.5f,0.5f,-0.5f),nFront, new Vector2F(0,0)),
               new GBufferVertex(new Vector3F(0.5f,0.5f,-0.5f),nFront, new Vector2F(1,0)),
               new GBufferVertex(new Vector3F(0.5f,-0.5f,-0.5f),nFront, new Vector2F(1,1)),

               new GBufferVertex(new Vector3F(-0.5f,-0.5f,0.5f),nBack, new Vector2F(1,0)), //back
               new GBufferVertex(new Vector3F(0.5f,0.5f,0.5f),nBack, new Vector2F(0,1)),
               new GBufferVertex(new Vector3F(-0.5f,0.5f,0.5f),nBack, new Vector2F(1,1)),
               new GBufferVertex(new Vector3F(0.5f,-0.5f,0.5f),nBack,  new Vector2F(0, 0)),

               new GBufferVertex(new Vector3F(-0.5f,0.5f,-0.5f), nTop, new Vector2F(0,1)), //top
               new GBufferVertex(new Vector3F(-0.5f,0.5f,0.5f),nTop,new Vector2F(0,0)),
               new GBufferVertex(new Vector3F(0.5f,0.5f,0.5f),nTop,new Vector2F(1,0)),
               new GBufferVertex(new Vector3F(0.5f,0.5f,-0.5f),nTop,new Vector2F(1,1)),

               new GBufferVertex(new Vector3F(-0.5f,-0.5f,-0.5f),nBottom, new Vector2F(1,0)), //bottom
               new GBufferVertex(new Vector3F(0.5f,-0.5f,0.5f),nBottom, new Vector2F(0,1)),
               new GBufferVertex(new Vector3F(-0.5f,-0.5f,0.5f),nBottom, new Vector2F(1,1)),
               new GBufferVertex(new Vector3F(0.5f,-0.5f,-0.5f),nBottom, new Vector2F(0, 0)),

               new GBufferVertex(new Vector3F(-0.5f,-0.5f,-0.5f),nLeft, new Vector2F(0,1)), //left
               new GBufferVertex(new Vector3F(-0.5f,-0.5f,0.5f),nLeft, new Vector2F(0,0)),
               new GBufferVertex(new Vector3F(-0.5f,0.5f,0.5f),nLeft, new Vector2F(1,0)),
               new GBufferVertex(new Vector3F(-0.5f,0.5f,-0.5f),nLeft, new Vector2F(1,1)),

               new GBufferVertex(new Vector3F(0.5f,-0.5f,-0.5f),nRight, new Vector2F(1,0)), //right
               new GBufferVertex(new Vector3F(0.5f,0.5f,0.5f),nRight, new Vector2F(0,1)),
               new GBufferVertex(new Vector3F(0.5f,-0.5f,0.5f),nRight, new Vector2F(1,1)),
               new GBufferVertex(new Vector3F(0.5f,0.5f,-0.5f),nRight,  new Vector2F(0, 0)),
            };

            uint[] indices = new uint[]{
                0, 1, 2, 0, 2, 3,
                4, 5, 6, 4, 7, 5,
                8, 9, 10, 8, 10, 11,
                12, 13, 14, 12, 15, 13,
                16,17,18, 16, 18, 19,
                20, 21, 22, 20, 23, 21,
            };

            CalculateTangents(vertices, indices);

            IIndexedMesh<GBufferVertex> mesh = renderer.Resources.CreateIndexedMesh((uint)vertices.Length, (uint)indices.Length);
            mesh.SetVertices(vertices);
            mesh.SetIndices(indices);
            return mesh;
        }

        public static IIndexedMesh<GBufferVertex> PlainCentered(RenderService renderer, float uvTiling = 1.0f)
        {
            GBufferVertex[] vertices = new GBufferVertex[4];
            Vector3F normal = new Vector3F(0, 1, 0);

            vertices[0] = new GBufferVertex(new Vector3F(-0.5f, 0, -0.5f), normal, new Vector2F(0, uvTiling));
            vertices[1] = new GBufferVertex(new Vector3F(-0.5f, 0, 0.5f), normal, new Vector2F(0, 0));
            vertices[2] = new GBufferVertex(new Vector3F(0.5f, 0, 0.5f), normal, new Vector2F(uvTiling, 0));
            vertices[3] = new GBufferVertex(new Vector3F(0.5f, 0, -0.5f), normal, new Vector2F(uvTiling, uvTiling));

            uint[] indices = new uint[6];
            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;
            indices[3] = 0;
            indices[4] = 2;
            indices[5] = 3;

            CalculateTangents(vertices, indices);

            IIndexedMesh<GBufferVertex> mesh = renderer.Resources.CreateIndexedMesh((uint)vertices.Length, (uint)indices.Length);
            mesh.SetVertices(vertices);
            mesh.SetIndices(indices);
            return mesh;
        }

        public static IIndexedMesh<GBufferVertex> Plain(RenderService renderer, float uvTiling = 1.0f)
        {
            GBufferVertex[] vertices = new GBufferVertex[4];
            Vector3F normal = new Vector3F(0, 1, 0);

            vertices[0] = new GBufferVertex(new Vector3F(0f, 0, 0), normal, new Vector2F(0, uvTiling));
            vertices[1] = new GBufferVertex(new Vector3F(0f, 0, 1f), normal, new Vector2F(0, 0));
            vertices[2] = new GBufferVertex(new Vector3F(1f, 0, 1f), normal, new Vector2F(uvTiling, 0));
            vertices[3] = new GBufferVertex(new Vector3F(1f, 0, 0f), normal, new Vector2F(uvTiling, uvTiling));

            uint[] indices = new uint[6];
            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;
            indices[3] = 0;
            indices[4] = 2;
            indices[5] = 3;

            CalculateTangents(vertices, indices);

            IIndexedMesh<GBufferVertex> mesh = renderer.Resources.CreateIndexedMesh<GBufferVertex>((uint)vertices.Length, (uint)indices.Length);
            mesh.SetVertices(vertices);
            mesh.SetIndices(indices);
            return mesh;
        }

        /// <summary>Calculates the normals for a list of vertices and the provided index list.</summary>
        /// <param name="vertices"></param>
        /// <param name="indices"></param>
        public static void CalculateNormals(GBufferVertex[] vertices, uint[] indices)
        {
            CalculateNormals(vertices, indices, (uint)vertices.Length, (uint)indices.Length);
        }

        /// <summary>Calculates the normals for a list of vertices and the provided index list.</summary>
        /// <param name="vertices"></param>
        /// <param name="indices"></param>
        /// <param name="vertexCount"></param>
        /// <param name="indexCount"></param>
        public static void CalculateNormals(GBufferVertex[] vertices, uint[] indices, uint vertexCount, uint indexCount)
        {
            for (int index = 0; index < indexCount; index += 3)
            {
                // Get triangle indices
                uint i1 = indices[index + 0];
                uint i2 = indices[index + 1];
                uint i3 = indices[index + 2];

                Vector3F vec1 = vertices[i2].Position - vertices[i1].Position;
                Vector3F vec2 = vertices[i1].Position - vertices[i1].Position;
                Vector3F normal = new Vector3F()
                {
                    X = (vec1.Y * vec2.Z) - (vec1.Z * vec2.Y),
                    Y = (vec1.Z * vec2.X) - (vec1.X * vec2.Z),
                    Z = (vec1.X * vec2.Y) - (vec1.Y * vec2.X),
                };

                vertices[i1].Normal += normal;
                vertices[i2].Normal += normal;
                vertices[i3].Normal += normal;

                vertices[i1].Normal.Normalize();
                vertices[i2].Normal.Normalize();
                vertices[i3].Normal.Normalize();
            }
        }

        /// <summary>Calculates the bi-normals and tangents for a list of vertices and indices.</summary>
        /// <param name="vertices"></param>
        /// <param name="indices"></param>
        public static void CalculateTangents(GBufferVertex[] vertices, uint[] indices)
        {
            CalculateTangents(vertices, indices, (uint)vertices.Length, (uint)indices.Length);
        }

        /// <summary>Calculates the bi-normals and tangents for a list of vertices and indices.</summary>
        /// <param name="vertices"></param>
        /// <param name="indices"></param>
        /// <param name="vertexCount"></param>
        /// <param name="indexCount"></param>
        public static void CalculateTangents(GBufferVertex[] vertices, uint[] indices, uint vertexCount, uint indexCount)
        {
            // Lengyel, Eric. “Computing Tangent Space Basis Vectors for an Arbitrary Mesh”.
            // Terathon Software 3D Graphics Library, 2001.
            // http://www.terathon.com/code/tangent.html
            // Hegde, Siddharth. "Messing with Tangent Space". Gamasutra, 2007.
            // http://www.gamasutra.com/view/feature/129939/messing_with_tangent_space.php
            Vector3F[] tan1 = new Vector3F[vertexCount];
            Vector3F[] tan2 = new Vector3F[vertexCount];

            for (var index = 0; index < indexCount; index += 3)
            {
                // Get triangle indices
                uint i1 = indices[index + 0];
                uint i2 = indices[index + 1];
                uint i3 = indices[index + 2];

                // Get UVs for calculating the denominator value
                Vector2F w1 = vertices[i1].UV;
                Vector2F w2 = vertices[i2].UV;
                Vector2F w3 = vertices[i3].UV;

                // Calculate denomination value
                float s1 = w2.X - w1.X;
                float s2 = w3.X - w1.X;
                float t1 = w2.Y - w1.Y;
                float t2 = w3.Y - w1.Y;
                float denom = s1 * t2 - s2 * t1;

                if (Math.Abs(denom) < float.Epsilon)
                {
                    // The triangle UVs are zero sized one dimension.
                    //
                    // So we cannot calculate the vertex tangents for this
                    // one trangle, but maybe it can with other trangles.
                    continue;
                }

                var r = 1.0f / denom;

                Vector3F v1 = vertices[i1].Position;
                Vector3F v2 = vertices[i2].Position;
                Vector3F v3 = vertices[i3].Position;

                float x1 = v2.X - v1.X;
                float x2 = v3.X - v1.X;
                float y1 = v2.Y - v1.Y;
                float y2 = v3.Y - v1.Y;
                float z1 = v2.Z - v1.Z;
                float z2 = v3.Z - v1.Z;

                Vector3F sdir = new Vector3F()
                {
                    X = (t2 * x1 - t1 * x2) * r,
                    Y = (t2 * y1 - t1 * y2) * r,
                    Z = (t2 * z1 - t1 * z2) * r,
                };

                Vector3F tdir = new Vector3F()
                {
                    X = (s1 * x2 - s2 * x1) * r,
                    Y = (s1 * y2 - s2 * y1) * r,
                    Z = (s1 * z2 - s2 * z1) * r,
                };
                tan1[i1] += sdir;
                tan1[i2] += sdir;
                tan1[i3] += sdir;
                tan2[i1] += tdir;
                tan2[i2] += tdir;
                tan2[i3] += tdir;
            }

            // At this pouint we have all the vectors accumulated, but we need to average
            // them all out. So we loop through all the final verts and do a Gram-Schmidt
            // orthonormalize, then make sure they're all unit length.
            for (var i = 0; i < vertexCount; i++)
            {
                Vector3F n = vertices[i].Normal;
                Vector3F t = tan1[i];

                if (t.LengthSquared() < float.Epsilon)
                {
                    t = Vector3F.Cross(n, Vector3F.UnitX);
                    if (t.LengthSquared() < float.Epsilon)
                        t = Vector3F.Cross(n, Vector3F.UnitY);
                    vertices[i].Tangent = Vector3F.Normalize(t);
                    vertices[i].BiNormal = Vector3F.Cross(n, vertices[i].Tangent);
                    continue;
                }

                // Gram-Schmidt orthogonalize
                // TODO: this can can cause NaN when normalized. Fix, if possible.
                var tangent = t - n * Vector3F.Dot(n, t);
                tangent = Vector3F.Normalize(tangent);
                vertices[i].Tangent = tangent;

                // Calculate handedness
                var w = (Vector3F.Dot(Vector3F.Cross(n, t), tan2[i]) < 0.0F) ? -1.0F : 1.0F;

                // Calculate the bitangent
                var bitangent = Vector3F.Cross(n, tangent) * w;
                vertices[i].BiNormal = bitangent;
            }
        }
    }
}
