using System.Diagnostics;
using Molten.Font;
using Molten.Graphics;
using Molten.Graphics.MSDF;
using Molten.Input;

namespace Molten.Samples
{
    public class SignedDistanceField : SampleSceneGame
    {
        const int CHAR_CURVE_RESOLUTION = 3;

        public override string Description => "An example of using signed-distance-field (SDF), multi-channel signed-distance-field (MSDF) and multi-channel true signed-distance-field (MTSDF) rendering.";

        FontFile _fontFile;
        SpriteFont _font2Test;

        Vector2F _clickPoint;
        Color _clickColor = Color.Red;
        Shape _shape;
        RectangleF _glyphBounds;
        RectangleF _fontBounds;
        float _scale = 0.3f;
        List<Vector2F> _glyphTriPoints;
        List<List<Vector2F>> _linePoints;
        List<List<Vector2F>> _holePoints;
        List<Color> _colors;
        Vector2F _charOffset = new Vector2F(300, 300);
        SdfGenerator _sdf;
        Dictionary<string, ITexture2D> _msdfTextures;
        Dictionary<string, ITexture2D> _msdfResultTextures;
        bool _loaded;

        public SignedDistanceField() : base("Signed Distance Field (SDF)") { }

        protected override void OnInitialize(Engine engine)
        {
            base.OnInitialize(engine);

            _msdfTextures = new Dictionary<string, ITexture2D>();
            _msdfResultTextures = new Dictionary<string, ITexture2D>();
            _sdf = new SdfGenerator();

            ContentRequest cr = engine.Content.BeginRequest("assets/");
            cr.Load<ITexture2D>("dds_test.dds", new TextureParameters()
            {
                GenerateMipmaps = true
            });

            cr.Load<IMaterial>("Basictexture.mfx");
            cr.OnCompleted += Cr_OnCompleted;
            cr.Commit();

            CameraController.AcceptInput = false;
            Player.Transform.LocalPosition = new Vector3F(0, 0, -8);

            LoadFontFile("Ananda Namaste Regular.ttf", 24);
            //LoadFontFile("BroshK.ttf", 24);
            //LoadFontFile("Arial", 16);

            Keyboard.OnCharacterKey += Keyboard_OnCharacterKey;
        }

        private void Keyboard_OnCharacterKey(KeyboardKeyState state)
        {
            if(state.Action == InputAction.Pressed && _font2Test != null)
                GenerateChar(state.Character);
        }

        private void LoadFontFile(string loadString, int size)
        {
            ContentRequest cr = Engine.Content.BeginRequest("assets/");
            cr.Load<SpriteFont>(loadString, new SpriteFontParameters()
            {
                FontSize = size,
            });
            OnContentRequested(cr);
            cr.OnCompleted += FontLoad_OnCompleted;
            cr.Commit();
        }

        private void FontLoad_OnCompleted(ContentRequest cr)
        {
            _font2Test = cr.Get<SpriteFont>(0);
            _fontFile = _font2Test.Font;
            InitializeFontDebug();
            GenerateChar('j');

            _loaded = true;
        }

        private unsafe void GenerateSDF(string label, SdfMode mode)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            uint pWidth = 64;
            uint pHeight = 64;
            double pxRange = 8;

            uint testWidth = 128;
            uint testHeight = 128;
            FillRule fl = FillRule.NonZero;

            MsdfProjection projection = new MsdfProjection()
            {
                Scale = new Vector2D(0.08), // new Vector2D(0.2),
                Translate = new Vector2D(190, 60)//new Vector2D(-240, -280)
            };

            TextureSliceRef<float> sdf = _sdf.Generate(pWidth, pHeight, _shape, projection, pxRange, mode, fl);
            TextureSliceRef<float> renderRef  = _sdf.Rasterize(testWidth, testHeight, sdf, projection, pxRange);

            ITexture2D texSdf = ConvertToTexture(sdf);
            _msdfTextures.Add(label, texSdf);

            ITexture2D tex = ConvertToTexture(renderRef);
            _msdfResultTextures.Add($"{label} Render", tex);

            sdf.Slice.Dispose();
            renderRef.Slice.Dispose();
            timer.Stop();
            Log.WriteLine($"Generated {pWidth}x{pHeight} {label} texture, rendered to {testWidth}x{testHeight} texture in {timer.Elapsed.TotalMilliseconds:N2}ms");
        }

        private unsafe ITexture2D ConvertToTexture(TextureSliceRef<float> src)
        {
            uint rowPitch = (src.Width * (uint)sizeof(Color));
            Color[] finalData = new Color[src.Width * src.Height];
            ITexture2D tex = Engine.Renderer.Resources.CreateTexture2D(new Texture2DProperties()
            {
                Width = src.Width,
                Height = src.Height,
                Format = GraphicsFormat.R8G8B8A8_UNorm
            });

            switch (src.ElementsPerPixel)
            {
                case 1: // SDF or PSDF is one-channel. The render result of all SDF modes are also generally greyscale/white/black.
                    for (int i = 0; i < finalData.Length; i++)
                    {
                        byte c = (byte)(255 * src[i]);
                        finalData[i] = new Color()
                        {
                            R = c,
                            G = c,
                            B = c,
                            A = c,
                        };
                    }
                    break;

                case 3: // MSDF - 3 RGB 32-bit
                    for (uint i = 0; i < finalData.Length; i++)
                    {
                        uint p = i * src.ElementsPerPixel;

                        finalData[i] = new Color()
                        {
                            R = (byte)(255 * src[p]),
                            G = (byte)(255 * src[p + 1]),
                            B = (byte)(255 * src[p + 2]),
                            A = 255,
                        };
                    }

                    break;

                case 4: // MTSDF - 3 RGB 32-bit
                    for (uint i = 0; i < finalData.Length; i++)
                    {
                        uint p = i * src.ElementsPerPixel;

                        finalData[i] = new Color()
                        {
                            R = (byte)(255 * src[p]),
                            G = (byte)(255 * src[p + 1]),
                            B = (byte)(255 * src[p + 2]),
                            A = (byte)(255 * src[p + 3]),
                        };
                    }

                    break;
            }

            tex.SetData(0, finalData, 0, (uint)finalData.Length, rowPitch);
            return tex;
        }

        private void InitializeFontDebug()
        {
            _fontBounds = _fontFile.ContainerBounds;
            _fontBounds.X *= _scale;
            _fontBounds.Y *= _scale;
            _fontBounds.Width *= _scale;
            _fontBounds.Height *= _scale;
            _fontBounds.X += _charOffset.X;
            _fontBounds.Y += _charOffset.Y;

            _colors = new List<Color>();
            _colors.Add(Color.Wheat);
            _colors.Add(Color.Yellow);

            SampleSpriteRenderComponent sCom = SpriteLayer.AddObjectWithComponent<SampleSpriteRenderComponent>();
            sCom.RenderCallback = (sb) =>
            {
                if (SampleFont == null || _glyphTriPoints == null || _colors == null)
                    return;

                sb.DrawRectOutline(_glyphBounds, Color.Grey, 1);
                sb.DrawRectOutline(_fontBounds, Color.Pink, 1);

                // Top Difference marker
                float dif = _glyphBounds.Top - _fontBounds.Top;
                if (dif != 0)
                {
                    sb.DrawLine(new Vector2F(_glyphBounds.Right, _fontBounds.Top), new Vector2F(_glyphBounds.Right, _fontBounds.Top + dif), Color.Red, 1);
                    sb.DrawString(SampleFont, $"Dif: {dif}", new Vector2F(_glyphBounds.Right, _fontBounds.Top + (dif / 2)), Color.White);
                }

                // Bottom difference marker
                dif = _fontBounds.Bottom - _glyphBounds.Bottom;
                if (dif != 0)
                {
                    sb.DrawLine(new Vector2F(_glyphBounds.Right, _fontBounds.Bottom), new Vector2F(_glyphBounds.Right, _fontBounds.Bottom - dif), Color.Red, 1);
                    sb.DrawString(SampleFont, $"Dif: {dif}", new Vector2F(_glyphBounds.Right, _fontBounds.Bottom - (dif / 2)), Color.White);
                }

                sb.DrawTriangleList(_glyphTriPoints, _colors);

                if (_linePoints != null)
                {
                    for (int i = 0; i < _linePoints.Count; i++)
                        sb.DrawLinePath(_linePoints[i], Color.SkyBlue, 2);
                }

                if (_holePoints != null)
                {
                    for (int i = 0; i < _holePoints.Count; i++)
                        sb.DrawLinePath(_holePoints[i], Color.Red, 2);
                }

                Rectangle clickRect;
                if (_shape != null)
                {
                    clickRect = new Rectangle((int)_clickPoint.X, (int)_clickPoint.Y, 0, 0);
                    clickRect.Inflate(8);
                    sb.DrawRect(clickRect, _clickColor);
                }

                sb.DrawString(SampleFont, $"Mouse: { Mouse.Position}", new Vector2F(5, 300), Color.Yellow);

                sb.DrawString(SampleFont, $"Font atlas: ", new Vector2F(700, 45), Color.White);

                if (_loaded)
                {
                    DrawTextureList(sb, _msdfResultTextures, new Vector2F(700, 65));
                    DrawTextureList(sb, _msdfTextures, new Vector2F(700, 665));
                }
            };
        }

        private void DrawTextureList(SpriteBatcher sb, Dictionary<string, ITexture2D> textures, Vector2F pos)
        {
            Color bgColor = new Color(255, 20, 0, 200);

            foreach (string label in textures.Keys)
            {
                ITexture2D tex = textures[label];
                Rectangle texBounds = new Rectangle((int)pos.X, (int)pos.Y, (int)tex.Width, (int)tex.Height);
                sb.DrawRect(texBounds, bgColor);
                sb.Draw(tex, texBounds, Color.White);
                sb.DrawRectOutline(texBounds, Color.Yellow, 1);

                Vector2F tPos = new Vector2F(texBounds.X, texBounds.Bottom + 5);
                sb.DrawString(SampleFont, label, tPos, Color.White);

                if (pos.X + (tex.Width + 15 + tex.Width) > Window.Width)
                {
                    pos.X = 700;
                    pos.Y += tex.Height + 30;
                }
                else
                {
                    pos.X += (float)tex.Width + 15;
                }
            }
        }

        /// <summary>
        /// A test for a new WIP sprite font system.
        /// </summary>
        private void GenerateChar(char glyphChar, int curveResolution = 16)
        {
            Glyph glyph = _fontFile.GetGlyph(glyphChar);
            _shape = glyph.CreateShape();
            
            _msdfResultTextures.Clear();
            _msdfTextures.Clear();
            MsdfShapeProcessing.Normalize(_shape);

            GenerateSDF("SDF", SdfMode.Sdf);
            GenerateSDF("PSDF", SdfMode.Psdf);
            GenerateSDF("MSDF", SdfMode.Msdf);
            GenerateSDF("MTSDF", SdfMode.Mtsdf);

            _shape.ScaleAndOffset(_charOffset, _scale);

            // Add 5 colors. The last color will be used when we have more points than colors.
            _linePoints = new List<List<Vector2F>>();
            _holePoints = new List<List<Vector2F>>();

            // Draw outline
            foreach (Shape.Contour c in _shape.Contours)
            {
                List<TriPoint> edgePoints = c.GetEdgePoints(curveResolution);
                List<Vector2F> points = new List<Vector2F>();

                for (int j = 0; j < edgePoints.Count; j++)
                    points.Add((Vector2F)edgePoints[j]);

                if (c.GetWinding() < 1)
                    _linePoints.Add(points);
                else
                    _holePoints.Add(points);
            }

            _glyphBounds = glyph.Bounds;
            _glyphBounds.X *= _scale;
            _glyphBounds.Y *= _scale;
            _glyphBounds.Width *= _scale;
            _glyphBounds.Height *= _scale;
            _glyphBounds.X += _charOffset.X;
            _glyphBounds.Y += _charOffset.Y;
            _glyphTriPoints = new List<Vector2F>();

            _shape.Triangulate(_glyphTriPoints, Vector2F.Zero, 1, CHAR_CURVE_RESOLUTION);
        }

        private void Cr_OnCompleted(ContentRequest cr)
        {
            if (cr.RequestedFileCount == 0)
                return;

            ITexture2D tex = cr.Get<ITexture2D>(0);
            IMaterial mat = cr.Get<IMaterial>(1);

            if (mat == null)
            {
                Exit();
                return;
            }

            mat.SetDefaultResource(tex, 0);
            TestMesh.Material = mat;
        }

        protected override void OnUpdate(Timing time)
        {
            // Perform a collision test against the rendered font character
            // when left mouse button is clicked.
            if (Mouse.IsTapped(MouseButton.Left))
            {
                _clickPoint = (Vector2F)Mouse.Position;
                _clickColor = Color.Red;

                if (_shape != null)
                {
                    if (_shape.Contains(_clickPoint))
                        _clickColor = Color.Green;
                }
            }

            base.OnUpdate(time);
        }

        protected override void OnGamepadInput(Timing time)
        {
            // React to gamepad ABXY buttons
            if (Gamepad.IsTapped(GamepadButton.A))
                GenerateChar('A');

            if (Gamepad.IsTapped(GamepadButton.B))
                GenerateChar('B');

            if (Gamepad.IsTapped(GamepadButton.X))
                GenerateChar('X');

            if (Gamepad.IsTapped(GamepadButton.Y))
                GenerateChar('Y');

            // React to left and right shoulder buttons (or equivilents)
            if (Gamepad.IsTapped(GamepadButton.LeftShoulder) ||
                Gamepad.IsTapped(GamepadButton.RightShoulder))
                GenerateChar('S');
        }
    }
}
