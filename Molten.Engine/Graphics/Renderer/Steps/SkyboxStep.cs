namespace Molten.Graphics
{
    /// <summary>
    /// The skybox step.
    /// </summary>
    internal class SkyboxStep : RenderStep
    {
        HlslShader _fxSky;
        Mesh<Vertex> _sphereMesh;
        ObjectRenderData _skyboxData;

        internal override void Initialize(RenderService renderer)
        {
            _skyboxData = new ObjectRenderData();

            ShaderCompileResult result = renderer.Device.LoadEmbeddedShader("Molten.Assets", "skybox.mfx");
            _fxSky = result["skybox-default"];

            MakeSphere(4, 4, out Vertex[] vertices, out uint[] indices);
            _sphereMesh = renderer.Device.CreateMesh(vertices, indices);
            _sphereMesh.Shader = _fxSky;
        }

        public override void Dispose()
        {
            _fxSky.Dispose();
            _sphereMesh.Dispose();
        }

        internal override void Render(RenderService renderer, RenderCamera camera, RenderChainContext context, Timing time)
        {
            // No skybox texture or we're not on the first layer.
            if (context.Scene.SkyboxTexture == null || context.Scene.Layers.First() != context.Layer)
                return;

            Rectangle bounds = (Rectangle)camera.Surface.Viewport.Bounds;
            GraphicsQueue queue = renderer.Device.Queue;

            _sphereMesh.SetResource(context.Scene.SkyboxTexture, 0);

            // We want to add to the previous composition, rather than completely overwrite it.
            IRenderSurface2D destSurface = context.HasComposed ? context.PreviousComposition : renderer.Surfaces[MainSurfaceType.Scene];

            queue.State.Surfaces.Reset();
            queue.State.Surfaces[0] = destSurface;
            queue.State.DepthSurface.Value = renderer.Surfaces.GetDepth();
            queue.State.Viewports.Reset(camera.Surface.Viewport);
            queue.State.ScissorRects.Reset(bounds);

            queue.Begin();
            _skyboxData.RenderTransform = Matrix4F.Scaling(camera.MaxDrawDistance) * Matrix4F.CreateTranslation(camera.Position);
            _sphereMesh.Render(queue, renderer, camera, _skyboxData);
            queue.End();
        }

        private void MakeSphere(uint latLines, uint longLines, out Vertex[] vertices, out uint[] indices)
        {
            uint NumSphereVertices = ((latLines - 2U) * longLines) + 2U;
            uint NumSphereFaces = ((latLines - 3U) * (longLines) * 2U) + (longLines * 2U);

            vertices = new Vertex[NumSphereVertices];
            indices = new uint[NumSphereFaces * 3];
            float sphereYaw;
            float spherePitch;
            Matrix4F rotY;
            Matrix4F rotX;
            Vector3F vertexPos = new Vector3F(0.0f, 0.0f, 1.0f);

            vertices[0] = new Vertex(0, 0, 1.0f);

            for (int i = 0; i < latLines - 2; i++)
            {
                spherePitch = (i + 1) * (3.14f / (latLines - 1));
                rotX = Matrix4F.RotationX(spherePitch);
                for (int j = 0; j < longLines; j++)
                {
                    sphereYaw = j * (6.28f / (longLines));
                    rotY = Matrix4F.RotationZ(sphereYaw);
                    vertexPos = Vector3F.TransformNormal(new Vector3F(0.0f, 0.0f, 1.0f), (rotX * rotY));
                    vertexPos.Normalize();
                    vertices[i * longLines + j + 1] = new Vertex(vertexPos);
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
