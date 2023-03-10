using System.Runtime.InteropServices;
using Molten.Graphics;

namespace Molten.Examples
{
    [Example("Compute Add", "Takes two input buffers full of numbers and outputs the sums into an output buffer")]
    public class ComputeAdd : MoltenExample
    {
        const int NUM_SUMS = 100;
        ComputeData[] _values0;
        ComputeData[] _values1;
        ComputeData[] _result;
        bool _computeFinished;

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
                IGraphicsBuffer numBuffer = Engine.Renderer.Device.CreateBuffer(
                    GraphicsBufferFlags.Structured | GraphicsBufferFlags.ShaderResource, BufferMode.DynamicRing, numBytes * 2, stride);

                IGraphicsBufferSegment numSeg0 = numBuffer.Allocate<ComputeData>(NUM_SUMS);
                IGraphicsBufferSegment numSeg1 = numBuffer.Allocate<ComputeData>(NUM_SUMS);

                // A buffer to store our output data.
                IGraphicsBuffer outBuffer = Engine.Renderer.Device.CreateBuffer(
                    GraphicsBufferFlags.Structured | GraphicsBufferFlags.UnorderedAccess, BufferMode.Default, numBytes, stride);

                // Staging buffer for transferring our compute result off the GPU
                IStagingBuffer stagingBuffer = Engine.Renderer.Device.CreateStagingBuffer(StagingBufferFlags.Read, numBytes);

                // Setup arrays to hold our data
                _values0 = new ComputeData[NUM_SUMS];
                _values1 = new ComputeData[NUM_SUMS];
                _result = new ComputeData[NUM_SUMS];

                // Fill our data arrays
                for(int i = 0; i < NUM_SUMS; i++)
                {
                    _values0[i] = new ComputeData() { FValue = i, IValue = i };
                    _values1[i] = new ComputeData() { FValue = i*2, IValue = i*3 };
                }

                numSeg0.SetData(GraphicsPriority.Apply, _values0);
                numSeg1.SetData(GraphicsPriority.Apply, _values1);

                compute["Buffer0"].Value = numSeg0;
                compute["Buffer1"].Value = numSeg1;
                compute["BufferOut"].Value = outBuffer;

                Engine.Renderer.PushComputeTask(compute, NUM_SUMS, 1, 1, () =>
                {
                    // We can get our data immediately, since the render thread is calling the completionCallback.
                    outBuffer.CopyTo(GraphicsPriority.Immediate, stagingBuffer);
                    stagingBuffer.GetData(GraphicsPriority.Immediate, _result, 0, NUM_SUMS, 0);
                    _computeFinished = true;
                });
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

        protected override void OnDrawSprites(SpriteBatcher sb)
        {
            base.OnDrawSprites(sb);

            // Draw compute results
            Vector2F pos = new Vector2F(25, 30);
            if (_computeFinished)
            {
                for (int i = 0; i < NUM_SUMS; i++)
                {
                    sb.DrawString(Font, $"I: {_values0[i].IValue} + {_values1[i].IValue} = {_result[i].IValue}", pos, Color.White);
                    pos.Y += 20;
                    sb.DrawString(Font, $"F: {_values0[i].FValue:N1} + {_values1[i].FValue:N1} = {_result[i].FValue:N1}", pos, Color.White);
                    pos.Y += 20;

                    if (pos.Y >= Window.RenderBounds.Height - 40)
                    {
                        pos.X += 200;
                        pos.Y = 30;
                    }
                }
            }
            else
            {
                sb.DrawString(Font, $"Waiting for compute task...", pos, Color.White);
            }
        }
    }
}
