using System.Diagnostics;
using System.Runtime.InteropServices;
using Molten.Collections;
using Molten.Font;
using Molten.Graphics;
using Molten.Graphics.MSDF;
using Molten.Input;

namespace Molten.Samples
{
    public class MsdfTest : SampleSceneGame
    {
        const int CHAR_CURVE_RESOLUTION = 3;

        public override string Description => "An example of using signed-distance-field (SDF), multi-channel signed-distance-field (MSDF) and multi-channel true signed-distance-field (MTSDF) rendering.";

        SceneObject _parent;
        SceneObject _child;
        IMesh<VertexTexture> _mesh;
        FontFile _fontFile;
        SpriteFont _font2Test;

        Vector2F _clickPoint;
        Color _clickColor = Color.Red;
        ContourShape _shape;
        RectangleF _glyphBounds;
        RectangleF _fontBounds;
        float _scale = 0.3f;
        List<Vector2F> _glyphTriPoints;
        List<List<Vector2F>> _linePoints;
        List<List<Vector2F>> _holePoints;
        List<Color> _colors;
        Vector2F _charOffset = new Vector2F(300, 300);
        MsdfGenerator _msdf;
        Dictionary<string, ITexture2D> _msdfTextures;
        Dictionary<string, ITexture2D> _msdfResultTextures;
        bool _loaded;

        public MsdfTest() : base("Signed Distance Field (SDF)") { }

        protected override void OnInitialize(Engine engine)
        {
            base.OnInitialize(engine);

            _msdfTextures = new Dictionary<string, ITexture2D>();
            _msdfResultTextures = new Dictionary<string, ITexture2D>();
            _msdf = new MsdfGenerator();

            ContentRequest cr = engine.Content.BeginRequest("assets/");
            cr.Load<ITexture2D>("dds_test.dds", new TextureParameters()
            {
                GenerateMipmaps = true
            });

            cr.Load<IMaterial>("Basictexture.mfx");
            cr.OnCompleted += Cr_OnCompleted;
            cr.Commit();

            _mesh = Engine.Renderer.Resources.CreateMesh<VertexTexture>(36);
            _mesh.SetVertices(SampleVertexData.TexturedCube);
            SpawnParentChild(_mesh, Vector3F.Zero, out _parent, out _child);
            CameraController.AcceptInput = false;
            Player.Transform.LocalPosition = new Vector3F(0, 0, -8);

            LoadFontFile("Ananda Namaste Regular.ttf", 24);
            //LoadFontFile("BroshK.ttf", 24);

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

            GenerateSDF("SDF", 1, SdfMode.Sdf, false, ConvertSdfToRgb);
            GenerateSDF("SDF Legacy", 1, SdfMode.Sdf, true, ConvertSdfToRgb);
            GenerateSDF("PSDF", 1, SdfMode.Psdf, false, ConvertSdfToRgb);
            GenerateSDF("PSDF Legacy", 1, SdfMode.Psdf, true, ConvertSdfToRgb);
            GenerateSDF("MSDF", 3, SdfMode.Msdf, false, ConvertMsdfToRgb);
            GenerateSDF("MSDF Legacy", 3, SdfMode.Msdf, true, ConvertMsdfToRgb);
            GenerateSDF("MTSDF", 4, SdfMode.Mtsdf, false, ConvertMtsdfToRgb);
            GenerateSDF("MTSDF Legacy", 4, SdfMode.Mtsdf, true, ConvertMtsdfToRgb);

            _loaded = true;
        }

        private void ConvertSdfToRgb(TextureSliceRef<float> outRef, Color[] finalData)
        {
            for (int i = 0; i < finalData.Length; i++)
            {
                finalData[i] = new Color()
                {
                    R = (byte)(255 * outRef[i]),
                    G = (byte)(255 * outRef[i]),
                    B = (byte)(255 * outRef[i]),
                    A = (byte)(255 * outRef[i] > 0 ? 255 : 0),
                };
            }
        }

        private void ConvertMsdfToRgb(TextureSliceRef<float> outRef, Color[] finalData)
        {
            for (uint i = 0; i < finalData.Length; i++)
            {
                uint pi = i * outRef.ElementsPerPixel;
                finalData[i] = new Color()
                {
                    R = (byte)(255 * outRef[pi]),
                    G = (byte)(255 * outRef[pi + 1]),
                    B = (byte)(255 * outRef[pi + 2]),
                    A = (byte)(255 * outRef[pi + 3] > 0 ? 255 : 0),
                };
            }
        }

        private void ConvertMtsdfToRgb(TextureSliceRef<float> outRef, Color[] finalData)
        {
            for (uint i = 0; i < finalData.Length; i++)
            {
                uint pi = i * outRef.ElementsPerPixel;
                finalData[i] = new Color()
                {
                    R = (byte)(255 * outRef[pi]),
                    G = (byte)(255 * outRef[pi + 1]),
                    B = (byte)(255 * outRef[pi + 2]),
                    A = (byte)(255 * outRef[pi + 3]),
                };
            }
        }

        private unsafe void GenerateSDF(string label, uint elementsPerPixel, SdfMode mode, bool legacy, 
            Action<TextureSliceRef<float>, Color[]> convertCallback)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            uint pWidth = 64;
            uint pHeight = 64;
            int shapeSize = 50;
            double pxRange = 4;

            uint testWidth = 256;
            uint testHeight = 256;
            Vector2D scale = new Vector2D(1);
            Vector2D pOffset = new Vector2D(0, 8);
            double avgScale = .5 * (scale.X + scale.Y);
            double range = pxRange / Math.Min(scale.X, scale.Y);
            FillRule fl = FillRule.NonZero;

            uint sliceNumBytes = pWidth * pHeight * elementsPerPixel * sizeof(float);
            TextureSlice slice = new TextureSlice(pWidth, pHeight, sliceNumBytes)
            {
                ElementsPerPixel = elementsPerPixel,
            };

            TextureSliceRef<float> sliceRef = slice.GetReference<float>();
            ContourShape shape = CreateShape(new Vector2D(shapeSize));
            MsdfShapeProcessing.Normalize(shape);

            MsdfProjection projection = new MsdfProjection(scale, pOffset);

            uint numBytes = testWidth * testHeight * elementsPerPixel * sizeof(float);
            TextureSlice outSlice = new TextureSlice(testWidth, testHeight, numBytes)
            {
                ElementsPerPixel = elementsPerPixel,
            };

            TextureSliceRef<float> outRef = outSlice.GetReference<float>();
            uint rowPitch = (uint)((testWidth * sizeof(Color)));
            Color[] finalData = new Color[testWidth * testHeight];

            ITexture2D tex = Engine.Renderer.Resources.CreateTexture2D(new Texture2DProperties()
            {
                Width = testWidth,
                Height = testHeight,
                Format = GraphicsFormat.R8G8B8A8_UNorm
            });

            MsdfConfig config = new MsdfConfig()
            {
                DistanceCheckMode = MsdfConfig.DistanceErrorCheckMode.ALWAYS_CHECK_DISTANCE,
                Mode = MsdfConfig.ErrorCorrectMode.INDISCRIMINATE
            };

            _msdf.Generate(sliceRef, shape, projection, range, config, mode, fl, legacy);

            MsdfRasterization.RenderSDF(outRef, sliceRef, avgScale * range, 0.5f);

            convertCallback(outRef, finalData);            
            tex.SetData(0, finalData, 0, (uint)finalData.Length, rowPitch);

            _msdfResultTextures.Add(label, tex);

            slice.Dispose();
            outSlice.Dispose();
            timer.Stop();
            Log.WriteLine($"Generated {pWidth}x{pHeight} {label} texture, rendered to {testWidth}x{testHeight} texture in {timer.Elapsed.TotalMilliseconds:N2}ms");
        }

        private ContourShape CreateShape(Vector2D size)
        {
            ContourShape shape = new ContourShape();
            ContourShape.Contour c = new ContourShape.Contour();

            c.AddEdge(new ContourShape.QuadraticEdge(new Vector2D(0), new Vector2D(size.X / 2f, -(size.Y / 4)), new Vector2D(size.X, 0)));

            c.AddEdge(new ContourShape.LinearEdge(new Vector2D(size.X, 0), size));

            c.AddEdge(new ContourShape.LinearEdge(size, new Vector2D(0, size.Y)));

            c.AddEdge(new ContourShape.LinearEdge(new Vector2D(0, size.Y), new Vector2D(0)));

            shape.Contours.Add(c);

            Vector2D innerPos = size / 6;
            Vector2D innerSize = size / 2;
            double peakSize = 15;
            ContourShape.Contour cInner = new ContourShape.Contour();
            cInner.AddEdge(new ContourShape.LinearEdge(innerPos + new Vector2D(innerSize.X, 0), innerPos));
            cInner.AddEdge(new ContourShape.LinearEdge(innerPos, innerPos + new Vector2D(0, innerSize.Y)));
            cInner.AddEdge(new ContourShape.LinearEdge(innerPos + new Vector2D(0, innerSize.Y), innerPos + new Vector2D(innerSize.X / 2, innerSize.Y + peakSize)));
            cInner.AddEdge(new ContourShape.LinearEdge(innerPos + new Vector2D(innerSize.X / 2, innerSize.Y + peakSize), innerPos + innerSize));
            cInner.AddEdge(new ContourShape.LinearEdge(innerPos + innerSize, innerPos + new Vector2D(innerSize.X, 0)));

            shape.Contours.Add(cInner);
            return shape;
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
                        sb.DrawLinePath(_linePoints[i], Color.Red, 2);
                }

                if (_holePoints != null)
                {
                    for (int i = 0; i < _holePoints.Count; i++)
                        sb.DrawLinePath(_holePoints[i], Color.SkyBlue, 2);
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
                    Vector2F pos = new Vector2F(700, 65);
                    Color bgColor = new Color(255, 20, 0, 200);

                    foreach (string label in _msdfResultTextures.Keys)
                    {
                        ITexture2D tex = _msdfResultTextures[label];
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
            };
        }

        /// <summary>
        /// A test for a new WIP sprite font system.
        /// </summary>
        private void GenerateChar(char glyphChar, int curveResolution = 16)
        {
            Glyph glyph = _fontFile.GetGlyph(glyphChar);
            _shape = glyph.CreateShape();
            List<Shape> shapes = glyph.CreateShapes(curveResolution);

            _shape.ScaleAndOffset(_charOffset, _scale);

            // Add 5 colors. The last color will be used when we have more points than colors.
            _linePoints = new List<List<Vector2F>>();
            _holePoints = new List<List<Vector2F>>();

            // Draw outline
            foreach (ContourShape.Contour c in _shape.Contours)
            {
                List<TriPoint> edgePoints = c.GetEdgePoints(curveResolution);
                List<Vector2F> points = new List<Vector2F>();

                for (int j = 0; j < edgePoints.Count; j++)
                    points.Add((Vector2F)edgePoints[j]);

                if (c.GetWinding() < 1)
                    _holePoints.Add(points);
                else
                    _linePoints.Add(points);
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
            _mesh.Material = mat;
        }

        private void Window_OnClose(INativeSurface surface)
        {
            Exit();
        }

        protected override void OnUpdate(Timing time)
        {
            RotateParentChild(_parent, _child, time);

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

            // Apply left and right vibration equal to left and right trigger values 
            Gamepad.VibrationLeft.Value = Gamepad.LeftTrigger.Value;
            Gamepad.VibrationRight.Value = Gamepad.RightTrigger.Value;

            base.OnUpdate(time);
        }
    }
}
