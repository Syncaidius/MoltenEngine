using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>
    /// The skybox step.
    /// </summary>
    internal class SkyboxStep : RenderStepBase
    {
        DepthSurface _surfaceDepth;
        RenderSurface _surfaceScene;
        Material _matSky;
        IndexedMesh<Vertex> _sphereMesh;
        ObjectRenderData _skyboxData;
        IShaderValue _skyTextureValue;

        internal override void Initialize(RendererDX11 renderer)
        {
            _surfaceScene = renderer.GetSurface<RenderSurface>(MainSurfaceType.Scene);
            _surfaceDepth = renderer.GetDepthSurface();
            _skyboxData = new ObjectRenderData();

            ShaderCompileResult result = renderer.ShaderCompiler.CompileEmbedded("Molten.Graphics.Assets.skybox.mfx");
            _matSky = result["material", "skybox-default"] as Material;

            Vertex[] vertices;
            int[] indices;
            MakeSphere(4, 4, out vertices, out indices);
            _sphereMesh = new IndexedMesh<Vertex>(renderer, vertices.Length, indices.Length, VertexTopology.TriangleList, IndexBufferFormat.Signed32Bit, false);
            _sphereMesh.SetVertices(vertices);
            _sphereMesh.SetIndices(indices);
            _sphereMesh.Material = _matSky;
        }

        public override void Dispose()
        {
            _matSky.Dispose();
            _sphereMesh.Dispose();
        }

        internal override void Render(RendererDX11 renderer, RenderCamera camera, RenderChain.Context context, Timing time)
        {
            // No skybox texture or we're not on the first layer.
            if (context.Scene.SkyboxTexture == null || context.Scene.Layers.First() != context.Layer)
                return;

            Rectangle bounds = camera.OutputSurface.Viewport.Bounds;
            GraphicsDeviceDX11 device = renderer.Device;

            _sphereMesh.SetResource(context.Scene.SkyboxTexture, 0);

            context.CompositionSurface.Clear(context.Scene.BackgroundColor);
            device.UnsetRenderSurfaces();
            device.SetRenderSurface(context.CompositionSurface, 0);
            device.DepthSurface = _surfaceDepth;
            device.DepthWriteOverride = GraphicsDepthWritePermission.Enabled;
            device.Rasterizer.SetViewports(camera.OutputSurface.Viewport);
            device.Rasterizer.SetScissorRectangle(bounds);

            ITexture2D sourceSurface = context.HasComposed ? context.PreviousComposition : _surfaceScene;

            renderer.Device.BeginDraw(StateConditions.None); // TODO correctly use pipe + conditions here.
            _skyboxData.RenderTransform = Matrix4F.Scaling(camera.MaxDrawDistance) * Matrix4F.CreateTranslation(camera.Position);
            _sphereMesh.Render(device, renderer, camera, _skyboxData);
            renderer.Device.EndDraw();

            context.SwapComposition();
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
