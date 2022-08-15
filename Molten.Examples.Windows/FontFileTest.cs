using System.Runtime.InteropServices;
using Molten.Font;
using Molten.Graphics;
using Molten.Graphics.SDF;
using Molten.Input;

namespace Molten.Samples
{
    public class FontFileTest : SampleGame
    {
        const int CHAR_CURVE_RESOLUTION = 8;

        public override string Description => "A test area for the WIP FontFile system.";

        FontFile _fontFile;
        SpriteFont _font2Test;

        Vector2F _clickPoint;
        Color _clickColor = Color.Red;
        Shape _shape;
        RectangleF _glyphBounds;
        RectangleF _fontBounds;
        float _scale = 0.3f;
        List<Vector2F> _glyphTriPoints;
        List<Color> _colors;
        Vector2F _charOffset = new Vector2F(300, 300);

        ContentLoadHandle _hMaterial;
        ContentLoadHandle _hTexture;

        public FontFileTest() : base("Fonts") { }


        protected override void OnLoadContent(ContentLoadBatch loader)
        {
            _hMaterial = loader.Load<IMaterial>("assets/BasicTexture.mfx");
            _hTexture = loader.Load<ITexture2D>("assets/dds_test.dds", parameters: new TextureParameters()
            {
                GenerateMipmaps = true
            });
            loader.OnCompleted += Loader_OnCompleted;
        }

        protected override void OnInitialize(Engine engine)
        {
            base.OnInitialize(engine);

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
            ContentLoadHandle handle = Engine.Content.LoadFont($"assets/{loadString}", (font, isReload) =>
            {
                _font2Test = font;
                if (_font2Test == null)
                    return;

                _fontFile = _font2Test.File;
                InitializeFontDebug();
                GenerateChar('j', CHAR_CURVE_RESOLUTION);
                _font2Test.MeasureString("abcdefghijklmnopqrstuvwxyz1234567890", 16);
            });
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

                RectStyle style = new RectStyle(Color.Transparent, Color.Grey, 1);
                sb.DrawRect(_glyphBounds, ref style);

                style.FillColor = Color.Transparent;
                style.BorderColor = Color.Pink;
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
                sb.DrawShapeOutline(_shape, Color.Red, Color.SkyBlue, 2);

                Rectangle clickRect;
                style = new RectStyle(_clickColor);

                if (_shape != null)
                {
                    clickRect = new Rectangle((int)_clickPoint.X, (int)_clickPoint.Y, 0, 0);
                    clickRect.Inflate(8);
                    sb.DrawRect(clickRect, ref style);
                }

                sb.DrawString(SampleFont, $"Mouse: { Mouse.Position}", new Vector2F(5, 300), Color.Yellow);

                sb.DrawString(SampleFont, $"Font atlas: ", new Vector2F(700, 45), Color.White);

                // Only draw test font if it's loaded

                if (_font2Test != null && _font2Test.Manager.UnderlyingTexture != null)
                {
                    RectStyle boundsStyle = new RectStyle(Color.Transparent, Color.Grey, 2);
                    Vector2F pos = new Vector2F(800, 65);
                    Rectangle texBounds = new Rectangle((int)pos.X, (int)pos.Y, 512, 512);
                    sb.Draw(texBounds, Color.White, _font2Test.Manager.UnderlyingTexture);

                    boundsStyle.BorderColor = Color.Red;
                    boundsStyle.FillColor = Color.Transparent;
                    boundsStyle.BorderThickness = new RectBorderThickness(1);

                    sb.DrawRect(texBounds, ref boundsStyle);

                    pos.Y += 517;
                    sb.DrawString(_font2Test, $"Testing 1-2-3! This is a test string using the new SpriteFont class.", pos, Color.White);
                    pos.Y += _font2Test.LineSpacing;
                    sb.DrawString(_font2Test, $"Font Name: {_font2Test.File.Info.FullName}", pos, Color.White);
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

            _glyphBounds = glyph.Bounds;
            _glyphBounds.X *= _scale;
            _glyphBounds.Y *= _scale;
            _glyphBounds.Width *= _scale;
            _glyphBounds.Height *= _scale;
            _glyphBounds.X += _charOffset.X;
            _glyphBounds.Y += _charOffset.Y;
            _glyphTriPoints = new List<Vector2F>();

            _shape.Triangulate(_glyphTriPoints, CHAR_CURVE_RESOLUTION);
        }

        private void Loader_OnCompleted(ContentLoadBatch loader)
        {
            if (!_hMaterial.HasAsset())
            {
                Exit();
                return;
            }

            IMaterial mat = _hMaterial.Get<IMaterial>();
            ITexture2D tex = _hTexture.Get<ITexture2D>();
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
