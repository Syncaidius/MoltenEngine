using Molten.Font;
using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Samples
{
    public class FontFileTest : SampleSceneGame
    {
        public override string Description => "A test area for the WIP FontFile system.";

        SceneObject _parent;
        SceneObject _child;
        Random _rng;
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

        public FontFileTest() : base("FontFile Test") { }

        protected override void OnInitialize(Engine engine)
        {
            base.OnInitialize(engine);
            _rng = new Random();

            ContentRequest cr = engine.Content.BeginRequest("assets/");
            cr.Load<ITexture2D>("dds_test.dds;mipmaps=true");
            cr.Load<IMaterial>("Basictexture.mfx");
            cr.OnCompleted += Cr_OnCompleted;
            cr.Commit();

            _mesh = Engine.Renderer.Resources.CreateMesh<VertexTexture>(36);
            _mesh.SetVertices(SampleVertexData.TexturedCube);
            SpawnParentChild(_mesh, Vector3F.Zero, out _parent, out _child);
            AcceptPlayerInput = false;
            Player.Transform.LocalPosition = new Vector3F(0, 0, -8);

            //LoadFontFile();
            LoadSystemFontFile("Arial");
            InitializeFontDebug();
            GenerateChar('Å');

            Keyboard.OnCharacterKey += Keyboard_OnCharacterKey;
        }

        private void Keyboard_OnCharacterKey(char character, long paramVal)
        {
            GenerateChar(character);
            _font2Test.GetCharGlyph(character);
        }

        private void LoadFontFile()
        {
            string fontPath = "assets/euphorigenic.ttf";
            //string fontPath = "assets/BroshK.ttf";
            //string fontPath = "assets/Digitalt.ttf";
            //string fontPath = "assets/NotoSansCJKkr-Regular.otf";

            Logger fontLog = Logger.Get();
            fontLog.AddOutput(new LogFileWriter("font{0}.txt"));
            Stopwatch fontTimer = new Stopwatch();
            using (FileStream stream = new FileStream(fontPath, FileMode.Open, FileAccess.Read))
            {
                using (FontReader reader = new FontReader(stream, fontLog, fontPath))
                {
                    fontTimer.Start();
                    _fontFile = reader.ReadFont(true);
                    fontTimer.Stop();
                    fontLog.WriteLine($"Took {fontTimer.Elapsed.TotalMilliseconds}ms to read font");

                    _font2Test = new SpriteFont(Engine.Renderer, _fontFile, 20);
                }
            }
        }

        private void LoadSystemFontFile(string fontName)
        {
            Stopwatch fontTimer = new Stopwatch();
            Logger fontLog = Logger.Get();
            fontLog.AddOutput(new LogFileWriter("font{0}.txt"));

            using (FontReader reader = new FontReader(fontName, fontLog))
            {
                fontTimer.Start();
                _fontFile = reader.ReadFont(true);
                fontTimer.Stop();
                fontLog.WriteLine($"Took {fontTimer.Elapsed.TotalMilliseconds}ms to read system font");

                _font2Test = new SpriteFont(Engine.Renderer, _fontFile, 20);
            }
            fontLog.Dispose();
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
                if (_font2Test != null && _font2Test.UnderlyingTexture != null)
                {
                    Vector2I pos = new Vector2I(800, 65);
                    Rectangle texBounds = new Rectangle(pos.X, pos.Y, 512, 512);
                    sb.Draw(_font2Test.UnderlyingTexture, texBounds, Color.White);
                    sb.DrawRectOutline(texBounds, Color.Red, 1);
                    pos.Y += 517;
                    sb.DrawString(_font2Test, $"Testing 1-2-3! This is a test string using the new SpriteFont class.", pos, Color.White);
                    pos.Y += _font2Test.LineSpace;
                    sb.DrawString(_font2Test, $"Font Name: {_font2Test.Font.Info.FullName}", pos, Color.White);
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

        private void Window_OnClose(IWindowSurface surface)
        {
            Exit();
        }

        protected override void OnUpdate(Timing time)
        {
            RotateParentChild(_parent, _child, time);

            if (Mouse.IsTapped(Input.MouseButton.Left))
            {
                _clickPoint = Mouse.Position;
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

            base.OnUpdate(time);
        }
    }
}
