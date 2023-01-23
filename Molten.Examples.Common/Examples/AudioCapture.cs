using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Audio;
using Molten.Data;
using Molten.Graphics;
using Molten.UI;

namespace Molten.Examples
{
    [Example("Audio Capture", "Showcases Molten's audio capture capabilities.")]
    public class AudioCapture : MoltenExample
    {
        const int READ_SAMPLES_PER_FRAME = 1000;
        const int FREQUENCY = 6635;

        ContentLoadHandle _hMaterial;
        ContentLoadHandle _hTexture;

        UILabel _lblCapDevice;
        UILabel _lblSamples;
        UILabel _lblPlayState;

        UIWindow _window1;
        UIWindow _window2;
        UILineGraph _lineGraph;
        UIButton _btnStartCap;
        UIButton _btnStopCap;
        UIButton _btnPlay;
        UICheckBox _chkLoopSound;

        GraphDataSet _graphSet;

        AudioBuffer _buffer;
        ISoundSource _source;
        ISoundInstance _instance;

        protected override void OnLoadContent(ContentLoadBatch loader)
        {
            base.OnLoadContent(loader);

            _hMaterial = loader.Load<IMaterial>("assets/BasicTexture.mfx");
            _hTexture = loader.Load<ITexture2D>("assets/logo_512_bc7.dds", parameters: new TextureParameters()
            {
                GenerateMipmaps = true,
            });
                
            loader.Deserialize<UITheme>("assets/test_theme.json", (theme, isReload, handle) =>
            {
                UI.Root.Theme = theme;
            });

            loader.OnCompleted += Loader_OnCompleted;
        }

        private void Loader_OnCompleted(ContentLoadBatch loader)
        {
            if (!_hMaterial.HasAsset())
            {
                Close();
                return;
            }

            IMaterial mat = _hMaterial.Get<IMaterial>();
            ITexture2D texture = _hTexture.Get<ITexture2D>();

            mat.SetDefaultResource(texture, 0);
            TestMesh.Material = mat;

            _buffer = Engine.Audio.CreateBuffer(FREQUENCY, AudioFormat.Mono8, FREQUENCY);
            _source = Engine.Audio.Output.CreateSoundSource(null);
            _instance = _source.CreateInstance();

            _lblCapDevice = UI.Children.Add<UILabel>(new Rectangle(300, 5, 500, 25));
            _lblCapDevice.Text = $"Audio Capture Device: {Engine.Audio.Input?.Name}";

            _lblSamples = UI.Children.Add<UILabel>(new Rectangle(300, 30, 500, 25));
            _lblSamples.Text = $"Captured Samples Available: 0";

            _lblPlayState = UI.Children.Add<UILabel>(new Rectangle(300, 50, 500, 25));
            _lblPlayState.Text = $"Play State: 0";

            _window1 = new UIWindow()
            {
                LocalBounds = new Rectangle(50, 150, 900, 470),
                Title = "Capture Waveform",
            };
            {
                _lineGraph = _window1.Children.Add<UILineGraph>(new Rectangle(0, 0, 900, 430));
                _graphSet = new GraphDataSet(200000);
                _graphSet.KeyColor = Color.Lime;

                _lineGraph.AddDataSet(_graphSet);
            }

            _window2 = new UIWindow()
            {
                LocalBounds = new Rectangle(960, 250, 440, 300),
                Title = "Audio Capture Controls",
            };
            {
                _btnStartCap = _window2.Children.Add<UIButton>(new Rectangle(100, 100, 130, 30));
                _btnStartCap.Text = "Start Capture";

                _btnStopCap = _window2.Children.Add<UIButton>(new Rectangle(100, 140, 130, 30));
                _btnStopCap.Text = "Stop Capture";

                _btnPlay = _window2.Children.Add<UIButton>(new Rectangle(100, 180, 180, 30));
                _btnPlay.Text = "Play Captured Audio";

                _btnStartCap.Pressed += btnStart_Pressed;
                _btnStopCap.Pressed += btnStop_Pressed;
                _btnPlay.Pressed += _btnPlay_Pressed;

                _chkLoopSound = _window2.Children.Add<UICheckBox>(new Rectangle(100, 220, 180, 30));
                _chkLoopSound.Text = "Loop Playback";
                _chkLoopSound.Toggled += _chkLoopSound_Toggled;
            }

            UI.Children.Add(_window1);
            UI.Children.Add(_window2);
        }

        private void _chkLoopSound_Toggled(UICheckBox element)
        {
            _instance.IsLooping = _chkLoopSound.IsChecked;
        }

        private void _btnPlay_Pressed(UIElement element, CameraInputTracker tracker)
        {
            _source.CommitBuffer(_buffer);
            _instance.Stop();
            _instance.Play();
        }

        private void btnStart_Pressed(UIElement element, CameraInputTracker tracker)
        {
            Engine.Audio.Input?.StartCapture();
        }

        private void btnStop_Pressed(UIElement element, CameraInputTracker tracker)
        {
            Engine.Audio.Input?.StopCapture();
        }

        protected override IMesh GetTestCubeMesh()
        {
            IMesh<CubeArrayVertex> cube = Engine.Renderer.Resources.CreateMesh<CubeArrayVertex>(36);
            cube.SetVertices(SampleVertexData.TextureArrayCubeVertices);
            return cube;
        }

        protected override void OnUpdate(Timing time)
        {
            base.OnUpdate(time);

            if (Engine.Audio.Input != null && _buffer != null)
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

        protected override void OnDrawSprites(SpriteBatcher sb)
        {
            base.OnDrawSprites(sb);

            if (!Engine.Audio.IsDisposed && _lblSamples != null)
            {
                int samples = Engine.Audio.Input.GetAvailableSamples();
                _lblSamples.Text = $"Captured Samples Available: {samples}";

                if (_instance != null)
                    _lblPlayState.Text = $"Play State: {_instance.State}";
            }
        }
    }
}
