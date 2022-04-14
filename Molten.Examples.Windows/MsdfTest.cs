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
        public override string Description => "An example of using signed-distance-field (SDF), multi-channel signed-distance-field (MSDF) and multi-channel true signed-distance-field (MTSDF) rendering.";

        SceneObject _parent;
        SceneObject _child;
        IMesh<VertexTexture> _mesh;
        FontFile _fontFile;
        SpriteFont _font2Test;

        Vector2F _clickPoint;
        Color _clickColor = Color.Red;
        List<Shape> _shapes;
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
        bool _loaded;

        public MsdfTest() : base("Signed Distance Field (SDF)") { }

        protected override void OnInitialize(Engine engine)
        {
            base.OnInitialize(engine);

            _msdfTextures = new Dictionary<string, ITexture2D>();
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
            GenerateChar('Å');

            GenerateSDF("SDF", false);
            GenerateSDF("SDF Legacy", true);
            GenerateMSDF("MSDF", false);
            GenerateMSDF("MSDF Legacy", true);

            _loaded = true;
        }

        private unsafe void GenerateSDF(string label, bool legacy)
        {
            int shapeSize = 50;
            int pWidth = 64;
            int pHeight = 64;
            int nPerPixel = 1;
            double pxRange = 4;

            int testWidth = 256;
            int testHeight = 256;
            int testNPerPixel = 1;
            Vector2D scale = new Vector2D(1);
            Vector2D pOffset = new Vector2D(0, 8);
            double avgScale = .5 * (scale.X + scale.Y);
            double range = pxRange / MsdfMath.Min(scale.X, scale.Y);
            FillRule fl = FillRule.NonZero;

            float* pixels = EngineUtil.AllocArray<float>((nuint)(pWidth * pHeight * nPerPixel));
            BitmapRef<float> sdf = new BitmapRef<float>(pixels, nPerPixel, pWidth, pHeight);
            MsdfShape shape = CreateShape(new Vector2D(shapeSize));
            shape.Normalize();

            MsdfProjection projection = new MsdfProjection(scale, pOffset);
            if (legacy)
            {
                _msdf.GeneratePseudoSDF_Legacy(sdf, shape, range, scale, pOffset);
            }
            else
            {
                _msdf.GeneratePseudoSDF(sdf, shape, projection, range, new MSDFGeneratorConfig(true, new ErrorCorrectionConfig()
                {
                    DistanceCheckMode = ErrorCorrectionConfig.DistanceErrorCheckMode.DO_NOT_CHECK_DISTANCE,
                    Mode = ErrorCorrectionConfig.ErrorCorrectMode.DISABLED
                }));
            }
            MsdfRasterization.distanceSignCorrection(sdf, shape, projection, fl);

            // Output render test texture
            float* oPixels = EngineUtil.AllocArray<float>((nuint)(testWidth * testHeight * testNPerPixel));
            BitmapRef<float> output = new BitmapRef<float>(oPixels, testNPerPixel, testWidth, testHeight);
            MsdfRasterization.simulate8bit(sdf);
            MsdfRasterization.renderSDF(output, sdf, avgScale * range, 0.5f);
            ITexture2D tex = Engine.Renderer.Resources.CreateTexture2D(new Texture2DProperties()
            {
                Width = (uint)testWidth,
                Height = (uint)testHeight,
                Format = GraphicsFormat.R8G8B8A8_UNorm
            });

            float[] pData = new float[testWidth * testHeight * testNPerPixel];
            fixed (float* ptrData = pData)
                Buffer.MemoryCopy(oPixels, ptrData, pData.Length * sizeof(float), pData.Length * sizeof(float));

            Color[] pData2 = new Color[testWidth * testHeight];
            for (int i = 0; i < pData2.Length; i++)
            {
                pData2[i] = new Color()
                {
                    R = (byte)(255 * oPixels[i]),
                    G = (byte)(255 * oPixels[i]),
                    B = (byte)(255 * oPixels[i]),
                    A = (byte)(255 * oPixels[i] > 0 ? 255 : 0),
                };

            }

            uint rowPitch = (uint)((testWidth * testNPerPixel * sizeof(Color)));
            tex.SetData(0, pData2, 0, (uint)pData2.Length, rowPitch);

            _msdfTextures.Add(label, tex);
        }

        private unsafe void GenerateMSDF(string label, bool legacy)
        {
            int shapeSize = 50;
            int pWidth = 64;
            int pHeight = 64;
            int nPerPixel = 3;
            double pxRange = 4;

            int testWidth = 256;
            int testHeight = 256;
            int testNPerPixel = 3;
            Vector2D scale = new Vector2D(1);
            Vector2D pOffset = new Vector2D(0, 8);
            double avgScale = .5 * (scale.X + scale.Y);
            double range = pxRange / MsdfMath.Min(scale.X, scale.Y);
            FillRule fl = FillRule.NonZero;

            float* pixels = EngineUtil.AllocArray<float>((nuint)(pWidth * pHeight * nPerPixel));
            BitmapRef<float> sdf = new BitmapRef<float>(pixels, nPerPixel, pWidth, pHeight);
            MsdfShape shape = CreateShape(new Vector2D(shapeSize));
            shape.Normalize();

            MsdfProjection projection = new MsdfProjection(scale, pOffset);
            MSDFGeneratorConfig config = new MSDFGeneratorConfig(true, new ErrorCorrectionConfig()
            {
                DistanceCheckMode = ErrorCorrectionConfig.DistanceErrorCheckMode.DO_NOT_CHECK_DISTANCE,
                Mode = ErrorCorrectionConfig.ErrorCorrectMode.DISABLED
            });

            if (legacy)
                _msdf.GenerateMSDF_Legacy(sdf, shape, range, scale, pOffset, config.ErrorCorrection);
            else
                _msdf.GenerateMSDF(sdf, shape, projection, range, config);

            MsdfRasterization.multiDistanceSignCorrection(sdf, shape, projection, fl);
            ErrorCorrection.MsdfErrorCorrection(new OverlappingContourCombiner<MultiDistanceSelector, MultiDistance>(shape), sdf, shape, projection, range, config);

            // Output render test texture
            int numElements = testWidth * testHeight * testNPerPixel;
            float* oPixels = EngineUtil.AllocArray<float>((nuint)numElements);
            BitmapRef<float> output = new BitmapRef<float>(oPixels, testNPerPixel, testWidth, testHeight);
            MsdfRasterization.simulate8bit(sdf);
            MsdfRasterization.renderMSDF(output, sdf, avgScale * range, 0.5f);
            ITexture2D tex = Engine.Renderer.Resources.CreateTexture2D(new Texture2DProperties()
            {
                Width = (uint)testWidth,
                Height = (uint)testHeight,
                Format = GraphicsFormat.R8G8B8A8_UNorm
            });

            Color[] pData = new Color[testWidth * testHeight];
            for (int i = 0; i < pData.Length; i++)
            {
                int pi = i * nPerPixel;
                pData[i] = new Color()
                {
                    R = (byte)(255 * oPixels[pi]),
                    G = (byte)(255 * oPixels[pi+1]),
                    B = (byte)(255 * oPixels[pi+2]),
                    A = (byte)(255 * oPixels[pi+3] > 0 ? 255 : 0),
                };
            }

            uint rowPitch = (uint)((testWidth * sizeof(Color)));
            tex.SetData(0, pData, 0, (uint)pData.Length, rowPitch);

            _msdfTextures.Add(label, tex);
        }

        private MsdfShape CreateShape(Vector2D size)
        {
            MsdfShape shape = new MsdfShape();
            Contour c = new Contour();

            c.AddEdge(new QuadraticSegment(new Vector2D(0), new Vector2D(size.X / 2f, -(size.Y / 4)), new Vector2D(size.X, 0)));

            c.AddEdge(new LinearSegment(new Vector2D(size.X, 0), size));

            c.AddEdge(new LinearSegment(size, new Vector2D(0, size.Y)));

            c.AddEdge(new LinearSegment(new Vector2D(0, size.Y), new Vector2D(0)));

            shape.Contours.Add(c);

            Vector2D innerPos = size / 6;
            Vector2D innerSize = size / 2;
            double peakSize = 5;
            Contour cInner = new Contour();
            cInner.AddEdge(new LinearSegment(innerPos + new Vector2D(innerSize.X, 0), innerPos));
            cInner.AddEdge(new LinearSegment(innerPos, innerPos + new Vector2D(0, innerSize.Y)));
            cInner.AddEdge(new LinearSegment(innerPos + new Vector2D(0, innerSize.Y), innerPos + new Vector2D(innerSize.X / 2, innerSize.Y + peakSize)));
            cInner.AddEdge(new LinearSegment(innerPos + new Vector2D(innerSize.X / 2, innerSize.Y + peakSize), innerPos + innerSize));
            cInner.AddEdge(new LinearSegment(innerPos + innerSize, innerPos + new Vector2D(innerSize.X, 0)));

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
                if (_shapes != null)
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

                    foreach (string label in _msdfTextures.Keys)
                    {
                        ITexture2D tex = _msdfTextures[label];
                        Rectangle texBounds = new Rectangle((int)pos.X, (int)pos.Y, (int)tex.Width, (int)tex.Height);
                        sb.Draw(tex, texBounds, Color.White);
                        sb.DrawRectOutline(texBounds, Color.Yellow, 1);

                        Vector2F tPos = new Vector2F(texBounds.X, texBounds.Bottom + 5);
                        sb.DrawString(SampleFont, label, tPos, Color.White);

                        pos.X += (float)tex.Width + 15;
                    }
                }
            };
        }

        /// <summary>
        /// A test for a new WIP sprite font system.
        /// </summary>
        private void GenerateChar(char glyphChar)
        {
            Glyph glyph = _fontFile.GetGlyph(glyphChar);
             _shapes = glyph.CreateShapes(16);

            // Add 5 colors. The last color will be used when we have more points than colors.
            _linePoints = new List<List<Vector2F>>();
            _holePoints = new List<List<Vector2F>>();

            // Draw outline
            foreach (Shape s in _shapes)
                s.ScaleAndOffset(_charOffset, _scale);

            for (int i = 0; i < _shapes.Count; i++)
            {
                Shape shape = _shapes[i];
                List<Vector2F> points = new List<Vector2F>();
                _linePoints.Add(points);

                for (int j = 0; j < shape.Points.Count; j++)
                    points.Add((Vector2F)shape.Points[j]);

                foreach (Shape h in shape.Holes)
                {
                    List<Vector2F> hPoints = new List<Vector2F>();
                    _holePoints.Add(hPoints);

                    for (int j = 0; j < h.Points.Count; j++)
                        hPoints.Add((Vector2F)h.Points[j]);
                }
            }

            _glyphBounds = glyph.Bounds;
            _glyphBounds.X *= _scale;
            _glyphBounds.Y *= _scale;
            _glyphBounds.Width *= _scale;
            _glyphBounds.Height *= _scale;
            _glyphBounds.X += _charOffset.X;
            _glyphBounds.Y += _charOffset.Y;
            _glyphTriPoints = new List<Vector2F>();

            foreach (Shape s in _shapes)
                s.Triangulate(_glyphTriPoints, Vector2F.Zero, 1);
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

                if (_shapes != null)
                {
                    foreach (Shape s in _shapes)
                    {
                        if (s.Contains(_clickPoint))
                            _clickColor = Color.Green;
                    }
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
