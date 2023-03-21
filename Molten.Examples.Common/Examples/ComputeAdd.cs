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
            if (compute != null)
            {
                uint stride = (uint)sizeof(ComputeData);
                uint numBytes = stride * NUM_SUMS;

                // Setup arrays to hold our data
                _values0 = new ComputeData[NUM_SUMS];
                _values1 = new ComputeData[NUM_SUMS];
                _result = new ComputeData[NUM_SUMS];

                // Fill our data arrays
                for (int i = 0; i < NUM_SUMS; i++)
                {
                    _values0[i] = new ComputeData() { FValue = i, IValue = i };
                    _values1[i] = new ComputeData() { FValue = i * 2, IValue = i * 3 };
                }

                // Setup two input buffers for our numbers. By default these are immutable - cannot be changed after creation.
                IStructuredBuffer numBuffer0 = Engine.Renderer.Device.CreateStructuredBuffer(_values0);
                IStructuredBuffer numBuffer1 = Engine.Renderer.Device.CreateStructuredBuffer(_values1);

                // Setup one output buffer for results
                IStructuredBuffer outBuffer = Engine.Renderer.Device.CreateStructuredBuffer<ComputeData>(GraphicsResourceFlags.GpuRead | GraphicsResourceFlags.GpuWrite, NUM_SUMS, true, false);

                // Staging buffer for transferring our compute result off the GPU
                IStagingBuffer stagingBuffer = Engine.Renderer.Device.CreateStagingBuffer(true, false, numBytes);

                // Send our compute shader off to the renderer to be worked on.
                Engine.Renderer.PushTask(RenderTaskPriority.StartOfFrame, compute, NUM_SUMS, 1, 1, () =>
                {
                    // We can get our data immediately, since the render thread is calling the completionCallback.
                    outBuffer.CopyTo(GraphicsPriority.Immediate, stagingBuffer);
                    stagingBuffer.GetData(GraphicsPriority.Immediate, _result, 0, NUM_SUMS, 0);
                    _computeFinished = true;
                });

                compute["Buffer0"].Value = numBuffer0;
                compute["Buffer1"].Value = numBuffer1;
                compute["BufferOut"].Value = outBuffer;
            }

            shader.SetDefaultResource(texture, 0);
            TestMesh.Shader = shader;
        }

        protected override Mesh GetTestCubeMesh()
        {
            return Engine.Renderer.Device.CreateMesh(SampleVertexData.TextureArrayCubeVertices);
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
