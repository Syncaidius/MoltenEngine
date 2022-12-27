namespace Molten.Graphics
{
    /// <summary>
    /// The skybox step.
    /// </summary>
    internal class SkyboxStep : RenderStepBase
    {
        Material _matSky;
        IndexedMesh<Vertex> _sphereMesh;
        ObjectRenderData _skyboxData;

        internal override void Initialize(RendererDX11 renderer)
        {
            _skyboxData = new ObjectRenderData();

            ShaderCompileResult result = renderer.Resources.LoadEmbeddedShader("Molten.Graphics.Assets", "skybox.mfx");
            _matSky = result[ShaderClassType.Material, "skybox-default"] as Material;

            Vertex[] vertices;
            int[] indices;
            MakeSphere(4, 4, out vertices, out indices);
            _sphereMesh = new IndexedMesh<Vertex>(renderer, (uint)vertices.Length, (uint)indices.Length, 
                VertexTopology.TriangleList, IndexBufferFormat.Unsigned32Bit, false);
            _sphereMesh.SetVertices(vertices);
            _sphereMesh.SetIndices(indices);
            _sphereMesh.Material = _matSky;
        }

        public override void Dispose()
        {
            _matSky.Dispose();
            _sphereMesh.Dispose();
        }

        internal override void Render(RendererDX11 renderer, RenderCamera camera, RenderChainContext context, Timing time)
        {
            // No skybox texture or we're not on the first layer.
            if (context.Scene.SkyboxTexture == null || context.Scene.Layers.First() != context.Layer)
                return;

            Rectangle bounds = (Rectangle)camera.Surface.Viewport.Bounds;
            DeviceDX11 device = renderer.Device;

            _sphereMesh.SetResource(context.Scene.SkyboxTexture, 0);

            // We want to add to the previous composition, rather than completely overwrite it.
            RenderSurface2D destSurface = context.HasComposed ? context.PreviousComposition : renderer.Surfaces[MainSurfaceType.Scene];

            device.State.ResetRenderSurfaces();
            device.State.SetRenderSurface(destSurface, 0);
            device.State.DepthSurface.Value = renderer.Surfaces.GetDepth();
            device.State.DepthWriteOverride = GraphicsDepthWritePermission.Enabled;
            device.State.SetViewports(camera.Surface.Viewport);
            device.State.SetScissorRectangle(bounds);

            renderer.Device.BeginDraw(context.BaseStateConditions);
            _skyboxData.RenderTransform = Matrix4F.Scaling(camera.MaxDrawDistance) * Matrix4F.CreateTranslation(camera.Position);
            _sphereMesh.Render(device, renderer, camera, _skyboxData);
            renderer.Device.EndDraw();
        }

        private void MakeSphere(int LatLines, int LongLines, out Vertex[] vertices, out int[] indices)
        {
            int NumSphereVertices = ((LatLines - 2) * LongLines) + 2;
            int NumSphereFaces = ((LatLines - 3) * (LongLines) * 2) + (LongLines * 2);

            vertices = new Vertex[NumSphereVertices];
            indices = new int[NumSphereFaces * 3];
            float sphereYaw = 0.0f;
            float spherePitch = 0.0f;
            Matrix4F Rotationy = Matrix4F.Identity;
            Matrix4F Rotationx = Matrix4F.Identity;

            Vector3F currVertPos = new Vector3F(0.0f, 0.0f, 1.0f);

            vertices[0] = new Vertex(0, 0, 1.0f);

            for (int i = 0; i < LatLines - 2; i++)
            {
                spherePitch = (i + 1) * (3.14f / (LatLines - 1));
                Rotationx = Matrix4F.RotationX(spherePitch);
                for (int j = 0; j < LongLines; j++)
                {
                    sphereYaw = j * (6.28f / (LongLines));
                    Rotationy = Matrix4F.RotationZ(sphereYaw);
                    currVertPos = Vector3F.TransformNormal(new Vector3F(0.0f, 0.0f, 1.0f), (Rotationx * Rotationy));
                    currVertPos.Normalize();
                    vertices[i * LongLines + j + 1] = new Vertex(currVertPos);
                }
            }

            vertices[NumSphereVertices - 1] = new Vertex(0, 0, -1f);

            int k = 0;
            for (int l = 0; l < LongLines - 1; l++)
            {
                indices[k] = 0;
                indices[k + 1] = l + 1;
                indices[k + 2] = l + 2;
                k += 3;
            }

            indices[k] = 0;
            indices[k + 1] = LongLines;
            indices[k + 2] = 1;
            k += 3;

            for (int i = 0; i < LatLines - 3; i++)
            {
                for (int j = 0; j < LongLines - 1; j++)
                {
                    indices[k] = i * LongLines + j + 1;
                    indices[k + 1] = i * LongLines + j + 2;
                    indices[k + 2] = (i + 1) * LongLines + j + 1;

                    indices[k + 3] = (i + 1) * LongLines + j + 1;
                    indices[k + 4] = i * LongLines + j + 2;
                    indices[k + 5] = (i + 1) * LongLines + j + 2;

                    k += 6; // next quad
                }

                indices[k] = (i * LongLines) + LongLines;
                indices[k + 1] = (i * LongLines) + 1;
                indices[k + 2] = ((i + 1) * LongLines) + LongLines;

                indices[k + 3] = ((i + 1) * LongLines) + LongLines;
                indices[k + 4] = (i * LongLines) + 1;
                indices[k + 5] = ((i + 1) * LongLines) + 1;

                k += 6;
            }

            for (int l = 0; l < LongLines - 1; l++)
            {
                indices[k] = NumSphereVertices - 1;
                indices[k + 1] = (NumSphereVertices - 1) - (l + 1);
                indices[k + 2] = (NumSphereVertices - 1) - (l + 2);
                k += 3;
            }

            //store indices for triangle.
            indices[k] = NumSphereVertices - 1;
            indices[k + 1] = (NumSphereVertices - 1) - LongLines;
            indices[k + 2] = NumSphereVertices - 2;
        }
    }
}
