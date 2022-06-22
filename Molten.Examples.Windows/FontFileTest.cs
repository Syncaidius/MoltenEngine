using System.Runtime.InteropServices;
using Molten.Font;
using Molten.Graphics;
using Molten.Graphics.SDF;
using Molten.Input;

namespace Molten.Samples
{
    public class FontFileTest : SampleSceneGame
    {
        const int CHAR_CURVE_RESOLUTION = 8;

        public override string Description => "A test area for the WIP FontFile system.";

        FontFile _fontFile;
        TextFont _font2Test;
        char _c;

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

        public FontFileTest() : base("Fonts") { }

        protected override void OnInitialize(Engine engine)
        {
            base.OnInitialize(engine);

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

            //LoadFontFile("Ananda Namaste Regular.ttf");
            LoadFontFile("BroshK.ttf");
            //LoadFontFile("MICROSS");
            //LoadFontFile("CASTELAR");

            Keyboard.OnCharacterKey += Keyboard_OnCharacterKey;
        }

        private void Keyboard_OnCharacterKey(KeyboardKeyState state)
        {
            if(state.Action == InputAction.Pressed && _font2Test != null)
                GenerateChar(state.Character, CHAR_CURVE_RESOLUTION);
        }

        private void LoadFontFile(string loadString)
        {
            ContentRequest cr = Engine.Content.BeginRequest("assets/");
            cr.Load<TextFont>(loadString);
            OnContentRequested(cr);
            cr.OnCompleted += FontLoad_OnCompleted;
            cr.Commit();
        }

        private unsafe void FontLoad_OnCompleted(ContentRequest cr)
        {
            _font2Test = cr.Get<TextFont>(0);
            if (_font2Test == null)
                return;

            _fontFile = _font2Test.Source.Font;
            InitializeFontDebug();
            GenerateChar('j', CHAR_CURVE_RESOLUTION);
            _font2Test.MeasureString("abcdefghijklmnopqrstuvwxyz1234567890", 16);
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

                SpriteStyle style = new SpriteStyle(Color.Transparent, Color.Grey, 1);
                sb.DrawRect(_glyphBounds, ref style);

                style.Color = Color.Pink;
                sb.DrawRect(_fontBounds, ref style);

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
                style = new SpriteStyle(_clickColor);

                if (_shape != null)
                {
                    clickRect = new Rectangle((int)_clickPoint.X, (int)_clickPoint.Y, 0, 0);
                    clickRect.Inflate(8);
                    sb.DrawRect(clickRect, ref style);
                }

                sb.DrawString(SampleFont, $"Mouse: { Mouse.Position}", new Vector2F(5, 300), Color.Yellow);

                sb.DrawString(SampleFont, $"Font atlas: ", new Vector2F(700, 45), Color.White);

                // Only draw test font if it's loaded
                style = SpriteStyle.Default;

                if (_font2Test != null && _font2Test.Source.UnderlyingTexture != null)
                {
                    Vector2F pos = new Vector2F(800, 65);
                    Rectangle texBounds = new Rectangle((int)pos.X, (int)pos.Y, 512, 512);
                    sb.Draw(texBounds, ref style, _font2Test.Source.UnderlyingTexture);

                    style.Color = Color.Red;
                    style.Thickness = 1;

                    sb.DrawRect(texBounds, ref style);
                    pos.Y += 517;
                    sb.DrawString(_font2Test, $"Testing 1-2-3! This is a test string using the new SpriteFont class.", pos, Color.White);
                    pos.Y += _font2Test.LineSpacing;
                    sb.DrawString(_font2Test, $"Font Name: {_font2Test.Source.Font.Info.FullName}", pos, Color.White);
                    pos.Y += 30;
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
            _shape.ScaleAndOffset(_charOffset, _scale);

            // Add 5 colors. The last color will be used when we have more points than colors.
            _linePoints = new List<List<Vector2F>>();
            _holePoints = new List<List<Vector2F>>();

            // Draw outline
            foreach(Shape.Contour c in _shape.Contours)
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
            _c = glyphChar;

            _shape.Triangulate(_glyphTriPoints, CHAR_CURVE_RESOLUTION);
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
            if (Mouse.IsTapped(PointerButton.Left))
            {
                _clickPoint = (Vector2F)Mouse.Position;
                _clickColor = Color.Red;

                if (_shape != null)
                {
                    if (_shape.Contains(_clickPoint, CHAR_CURVE_RESOLUTION))
                        _clickColor = Color.Green;
                }
            }

            base.OnUpdate(time);
        }

        protected override void OnGamepadInput(Timing time)
        {
            // React to gamepad ABXY buttons
            if (Gamepad.IsTapped(GamepadButtons.A))
                GenerateChar('A');

            if (Gamepad.IsTapped(GamepadButtons.B))
                GenerateChar('B');

            if (Gamepad.IsTapped(GamepadButtons.X))
                GenerateChar('X');

            if (Gamepad.IsTapped(GamepadButtons.Y))
                GenerateChar('Y');

            // React to left and right shoulder buttons (or equivilents)
            if (Gamepad.IsTapped(GamepadButtons.LeftShoulder) ||
                Gamepad.IsTapped(GamepadButtons.RightShoulder))
                GenerateChar('S');
        }
    }
}
