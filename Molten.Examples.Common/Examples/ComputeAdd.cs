using System.Runtime.InteropServices;
using Molten.Graphics;

namespace Molten.Examples
{
    [Example("Compute Add", "Takes two input buffers full of numbers and outputs the sums into an output buffer")]
    public class ComputeAdd : MoltenExample
    {
        const int NUM_SUMS = 100;

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        struct ComputeData
        {
            public int IValue;
            public float FValue;
        }

        ContentLoadHandle _hShader;
        ContentLoadHandle _hComputeShader;
        ContentLoadHandle _hTexture;

        protected override void OnLoadContent(ContentLoadBatch loader)
        {
            base.OnLoadContent(loader);

            _hShader = loader.Load<HlslShader>("assets/BasicTexture.mfx");
            _hComputeShader = loader.Load<HlslShader>("assets/ComputeAdd.mfx");
            _hTexture = loader.Load<ITexture2D>("assets/png_test.png");
            loader.OnCompleted += Loader_OnCompleted;
        }

        private unsafe void Loader_OnCompleted(ContentLoadBatch loader)
        {
            if (!_hShader.HasAsset())
            {
                Close();
                return;
            }

            HlslShader shader = _hShader.Get<HlslShader>();
            ITexture2D texture = _hTexture.Get<ITexture2D>();


            // Populate our compute shader with the needed buffers and data
            HlslShader compute = _hComputeShader.Get<HlslShader>();
            if(compute != null)
            {
                uint stride = (uint)sizeof(ComputeData);
                uint numBytes = stride * NUM_SUMS;

                // We want 2 segments, so double the size of the buffer.
                numBytes *= 2;

                IGraphicsBuffer numBuffer = Engine.Renderer.Device.CreateBuffer(GraphicsBufferFlags.Structured, BufferMode.DynamicRing, numBytes, stride);
                IGraphicsBufferSegment numSeg0 = numBuffer.Allocate<ComputeData>(NUM_SUMS);
                IGraphicsBufferSegment numSeg1 = numBuffer.Allocate<ComputeData>(NUM_SUMS);

                // A buffer to store our output data.
                IGraphicsBuffer outBuffer = Engine.Renderer.Device.CreateBuffer(
                    GraphicsBufferFlags.Structured | GraphicsBufferFlags.UnorderedAccess, BufferMode.DynamicDiscard, numBytes, stride);
                IGraphicsBufferSegment outSeg = outBuffer.Allocate<ComputeData>(NUM_SUMS);

                compute["Buffer0"].Value = numSeg0;
                compute["Buffer1"].Value = numSeg1;
                compute["BufferOut"].Value = outSeg;
            }

            shader.SetDefaultResource(texture, 0);
            TestMesh.Shader = shader;
        }

        protected override Mesh GetTestCubeMesh()
        {
            Mesh<CubeArrayVertex> cube = Engine.Renderer.Resources.CreateMesh<CubeArrayVertex>(36);
            cube.SetVertices(SampleVertexData.TextureArrayCubeVertices);
            return cube;
        }
    }
}
