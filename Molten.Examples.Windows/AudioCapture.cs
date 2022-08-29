using Molten.Audio;
using Molten.Data;
using Molten.Graphics;
using Molten.UI;

namespace Molten.Samples
{
    public class AudioCaptureExample : SampleGame
    {
        const int READ_SAMPLES_PER_FRAME = 1000;

        ContentLoadHandle _hMaterial;
        ContentLoadHandle _hTexture;

        UILabel _lblCapDevice;
        UILabel _lblSamples;

        UIWindow _window1;
        UIWindow _window2;
        UILineGraph _lineGraph;
        UIButton _btnStart;
        UIButton _btnStop;

        GraphDataSet _graphSet;

        IAudioBuffer _buffer;

        public override string Description => "Demonstrates audio recording/capture";

        public AudioCaptureExample() : base("Audio Capture") { }

        protected override void OnLoadContent(ContentLoadBatch loader)
        {
            _hMaterial = loader.Load<IMaterial>("assets/BasicTexture.mfx");
            _hTexture = loader.Load<ITexture2D>("assets/dds_test.dds", parameters: new TextureParameters()
            {
                GenerateMipmaps = true,
            });

            loader.Deserialize<UITheme>("assets/test_theme.json",(theme, isReload) =>
            {
                UI.Root.Theme = theme;
            });

            loader.OnCompleted += Loader_OnCompleted;
        }

        private void Loader_OnCompleted(ContentLoadBatch loader)
        {
            if (!_hMaterial.HasAsset())
            {
                Exit();
                return;
            }

            IMaterial mat = _hMaterial.Get<IMaterial>();
            ITexture2D texture = _hTexture.Get<ITexture2D>();

            mat.SetDefaultResource(texture, 0);
            TestMesh.Material = mat;

            _buffer = Engine.Audio.CreateBuffer(6635);

            _lblCapDevice = UI.Children.Add<UILabel>(new Rectangle(300, 5, 500, 25));
            _lblCapDevice.Text = $"Audio Capture Device: {Engine.Audio.Input?.Name}";

            _lblSamples = UI.Children.Add<UILabel>(new Rectangle(300, 30, 500, 25));
            _lblSamples.Text = $"Captured Samples Available: 0";

            _window1 = new UIWindow()
            {
                LocalBounds = new Rectangle(50, 150, 900, 470),
                Title = "Capture Waveform",
            };
            {
                _lineGraph = _window1.Children.Add<UILineGraph>(new Rectangle(0, 0, 900, 430));
                SetupGraphData(_lineGraph);
            }

            _window2 = new UIWindow()
            {
                LocalBounds = new Rectangle(960, 250, 440, 250),
                Title = "Audio Capture Controls",
            };
            {
                _btnStart = _window2.Children.Add<UIButton>(new Rectangle(100, 100, 120, 30));
                _btnStart.Text = "Start Capture";

                _btnStop = _window2.Children.Add<UIButton>(new Rectangle(100, 140, 120, 30));
                _btnStop.Text = "Stop Capture";

                _btnStart.Pressed += btnStart_Pressed;
                _btnStop.Pressed += btnStop_Pressed;
            }

            UI.Children.Add(_window1);
            UI.Children.Add(_window2);
        }

        protected override void OnUpdate(Timing time)
        {
            base.OnUpdate(time);


            if (Engine.Audio.Input != null && Engine.Audio.Input.IsCapturing)
            {
                int numSamples = Engine.Audio.Input.ReadSamples(_buffer, READ_SAMPLES_PER_FRAME);

                //TODO implement way to copy data to a dataset in 1 call
                for (int i = 0; i < numSamples; i++)
                {
                    _graphSet.Plot(_buffer[_buffer.ReadPosition++]);
                    if (_buffer.ReadPosition == _buffer.Size)
                        _buffer.ReadPosition = 0;
                }
            }
        }

        private void btnStart_Pressed(UIElement element, ScenePointerTracker tracker)
        {
            Engine.Audio.Input?.StartCapture();
        }

        private void btnStop_Pressed(UIElement element, ScenePointerTracker tracker)
        {
            Engine.Audio.Input?.StopCapture();
        }

        private void SetupGraphData(UILineGraph graph)
        {
            _graphSet = new GraphDataSet(6000);
            _graphSet.KeyColor = Color.Lime;
            for (int i = 0; i < _graphSet.Capacity; i++)
                _graphSet.Plot(Rng.Next(0, 500));

            graph.AddDataSet(_graphSet);
        }

        protected override void OnDrawSprites(SpriteBatcher sb)
        {
            if (SampleFont == null)
                return;

            base.OnDrawSprites(sb);


            int samples = Engine.Audio.Input.GetAvailableSamples();
            _lblSamples.Text = $"Captured Samples Available: {samples}";
        }
    }
}
