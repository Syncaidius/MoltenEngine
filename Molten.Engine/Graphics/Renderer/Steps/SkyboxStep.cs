namespace Molten.Graphics
{
    /// <summary>
    /// The skybox step.
    /// </summary>
    internal class SkyboxStep : RenderStep
    {
        Material _matSky;
        Mesh<Vertex> _sphereMesh;
        ObjectRenderData _skyboxData;

        internal override void Initialize(RenderService renderer)
        {
            _skyboxData = new ObjectRenderData();

            ShaderCompileResult result = renderer.Resources.LoadEmbeddedShader("Molten.Assets", "skybox.mfx");
            _matSky = result[ShaderClassType.Material, "skybox-default"] as Material;

            Vertex[] vertices;
            uint[] indices;
            MakeSphere(4, 4, out vertices, out indices);
            _sphereMesh = renderer.Resources.CreateMesh<Vertex>((uint)vertices.Length);
            _sphereMesh.SetVertices(vertices);
            _sphereMesh.SetIndexParameters((uint)indices.Length);
            _sphereMesh.SetIndices(indices);
            _sphereMesh.Material = _matSky;
        }

        public override void Dispose()
        {
            _matSky.Dispose();
            _sphereMesh.Dispose();
        }

        internal override void Render(RenderService renderer, RenderCamera camera, RenderChainContext context, Timing time)
        {
            // No skybox texture or we're not on the first layer.
            if (context.Scene.SkyboxTexture == null || context.Scene.Layers.First() != context.Layer)
                return;

            Rectangle bounds = (Rectangle)camera.Surface.Viewport.Bounds;
            GraphicsCommandQueue cmd = renderer.Device.Cmd;

            _sphereMesh.SetResource(context.Scene.SkyboxTexture, 0);

            // We want to add to the previous composition, rather than completely overwrite it.
            IRenderSurface2D destSurface = context.HasComposed ? context.PreviousComposition : renderer.Surfaces[MainSurfaceType.Scene];

            cmd.ResetRenderSurfaces();
            cmd.SetRenderSurface(destSurface, 0);
            cmd.DepthSurface.Value = renderer.Surfaces.GetDepth();
            cmd.SetViewports(camera.Surface.Viewport);
            cmd.SetScissorRectangle(bounds);

            cmd.BeginDraw(context.BaseStateConditions);
            _skyboxData.RenderTransform = Matrix4F.Scaling(camera.MaxDrawDistance) * Matrix4F.CreateTranslation(camera.Position);
            _sphereMesh.Render(cmd, renderer, camera, _skyboxData);
            cmd.EndDraw();
        }

        private void MakeSphere(uint latLines, uint longLines, out Vertex[] vertices, out uint[] indices)
        {
            uint NumSphereVertices = ((latLines - 2U) * longLines) + 2U;
            uint NumSphereFaces = ((latLines - 3U) * (longLines) * 2U) + (longLines * 2U);

            vertices = new Vertex[NumSphereVertices];
            indices = new uint[NumSphereFaces * 3];
            float sphereYaw = 0.0f;
            float spherePitch = 0.0f;
            Matrix4F Rotationy = Matrix4F.Identity;
            Matrix4F Rotationx = Matrix4F.Identity;
            Vector3F currVertPos = new Vector3F(0.0f, 0.0f, 1.0f);

            vertices[0] = new Vertex(0, 0, 1.0f);

            for (int i = 0; i < latLines - 2; i++)
            {
                spherePitch = (i + 1) * (3.14f / (latLines - 1));
                Rotationx = Matrix4F.RotationX(spherePitch);
                for (int j = 0; j < longLines; j++)
                {
                    sphereYaw = j * (6.28f / (longLines));
                    Rotationy = Matrix4F.RotationZ(sphereYaw);
                    currVertPos = Vector3F.TransformNormal(new Vector3F(0.0f, 0.0f, 1.0f), (Rotationx * Rotationy));
                    currVertPos.Normalize();
                    vertices[i * longLines + j + 1] = new Vertex(currVertPos);
                }
            }

            vertices[NumSphereVertices - 1] = new Vertex(0, 0, -1f);

            int k = 0;
            for (uint l = 0; l < longLines - 1; l++)
            {
                indices[k] = 0;
                indices[k + 1] = l + 1U;
                indices[k + 2] = l + 2U;
                k += 3;
            }

            indices[k] = 0;
            indices[k + 1] = longLines;
            indices[k + 2] = 1;
            k += 3;

            for (uint i = 0; i < latLines - 3; i++)
            {
                for (uint j = 0; j < longLines - 1; j++)
                {
                    indices[k] = i * longLines + j + 1;
                    indices[k + 1] = i * longLines + j + 2;
                    indices[k + 2] = (i + 1) * longLines + j + 1;

                    indices[k + 3] = (i + 1) * longLines + j + 1;
                    indices[k + 4] = i * longLines + j + 2;
                    indices[k + 5] = (i + 1) * longLines + j + 2;

                    k += 6; // next quad
                }

                indices[k] = (i * longLines) + longLines;
                indices[k + 1] = (i * longLines) + 1;
                indices[k + 2] = ((i + 1) * longLines) + longLines;

                indices[k + 3] = ((i + 1) * longLines) + longLines;
                indices[k + 4] = (i * longLines) + 1;
                indices[k + 5] = ((i + 1) * longLines) + 1;

                k += 6;
            }

            for (uint l = 0; l < longLines - 1; l++)
            {
                indices[k] = NumSphereVertices - 1;
                indices[k + 1] = (NumSphereVertices - 1) - (l + 1);
                indices[k + 2] = (NumSphereVertices - 1) - (l + 2);
                k += 3;
            }

            //store indices for triangle.
            indices[k] = NumSphereVertices - 1;
            indices[k + 1] = (NumSphereVertices - 1) - longLines;
            indices[k + 2] = NumSphereVertices - 2;
        }
    }
}
