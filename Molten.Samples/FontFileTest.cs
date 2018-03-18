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
        List<Matrix4F> _positions;
        Random _rng;
        List<ISprite> _sprites;
        IMesh<VertexTexture> _mesh;
        SpriteBatchContainer _container;
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

        public FontFileTest(EngineSettings settings = null) : base("FontFile Test", settings) { }

        protected override void OnInitialize(Engine engine)
        {
            base.OnInitialize(engine);

            _sprites = new List<ISprite>();
            _rng = new Random();
            _positions = new List<Matrix4F>();

            ContentRequest cr = engine.Content.StartRequest();
            cr.Load<ITexture2D>("dds_test.dds;mipmaps=true");
            cr.Load<IMaterial>("Basictexture.sbm");
            cr.OnCompleted += Cr_OnCompleted;
            cr.Commit();

            _mesh = Engine.Renderer.Resources.CreateMesh<VertexTexture>(36);

            VertexTexture[] verts = new VertexTexture[]{
               new VertexTexture(new Vector3F(-1,-1,-1), new Vector2F(0,1)), //front
               new VertexTexture(new Vector3F(-1,1,-1), new Vector2F(0,0)),
               new VertexTexture(new Vector3F(1,1,-1), new Vector2F(1,0)),
               new VertexTexture(new Vector3F(-1,-1,-1), new Vector2F(0,1)),
               new VertexTexture(new Vector3F(1,1,-1), new Vector2F(1, 0)),
               new VertexTexture(new Vector3F(1,-1,-1), new Vector2F(1,1)),

               new VertexTexture(new Vector3F(-1,-1,1), new Vector2F(1,0)), //back
               new VertexTexture(new Vector3F(1,1,1), new Vector2F(0,1)),
               new VertexTexture(new Vector3F(-1,1,1), new Vector2F(1,1)),
               new VertexTexture(new Vector3F(-1,-1,1), new Vector2F(1,0)),
               new VertexTexture(new Vector3F(1,-1,1), new Vector2F(0, 0)),
               new VertexTexture(new Vector3F(1,1,1), new Vector2F(0,1)),

               new VertexTexture(new Vector3F(-1,1,-1), new Vector2F(0,1)), //top
               new VertexTexture(new Vector3F(-1,1,1), new Vector2F(0,0)),
               new VertexTexture(new Vector3F(1,1,1), new Vector2F(1,0)),
               new VertexTexture(new Vector3F(-1,1,-1), new Vector2F(0,1)),
               new VertexTexture(new Vector3F(1,1,1), new Vector2F(1, 0)),
               new VertexTexture(new Vector3F(1,1,-1), new Vector2F(1,1)),

               new VertexTexture(new Vector3F(-1,-1,-1), new Vector2F(1,0)), //bottom
               new VertexTexture(new Vector3F(1,-1,1), new Vector2F(0,1)),
               new VertexTexture(new Vector3F(-1,-1,1), new Vector2F(1,1)),
               new VertexTexture(new Vector3F(-1,-1,-1), new Vector2F(1,0)),
               new VertexTexture(new Vector3F(1,-1,-1), new Vector2F(0, 0)),
               new VertexTexture(new Vector3F(1,-1,1), new Vector2F(0,1)),

               new VertexTexture(new Vector3F(-1,-1,-1), new Vector2F(0,1)), //left
               new VertexTexture(new Vector3F(-1,-1,1), new Vector2F(0,0)),
               new VertexTexture(new Vector3F(-1,1,1), new Vector2F(1,0)),
               new VertexTexture(new Vector3F(-1,-1,-1), new Vector2F(0,1)),
               new VertexTexture(new Vector3F(-1,1,1), new Vector2F(1, 0)),
               new VertexTexture(new Vector3F(-1,1,-1), new Vector2F(1,1)),

               new VertexTexture(new Vector3F(1,-1,-1), new Vector2F(1,0)), //right
               new VertexTexture(new Vector3F(1,1,1), new Vector2F(0,1)),
               new VertexTexture(new Vector3F(1,-1,1), new Vector2F(1,1)),
               new VertexTexture(new Vector3F(1,-1,-1), new Vector2F(1,0)),
               new VertexTexture(new Vector3F(1,1,-1), new Vector2F(0, 0)),
               new VertexTexture(new Vector3F(1,1,1), new Vector2F(0,1)),
            };
            _mesh.SetVertices(verts);
            SpawnParentChild(_mesh, Vector3F.Zero, out _parent, out _child);
            AcceptPlayerInput = false;

            //LoadFontFile();
            LoadSystemFontFile("Arial");
            InitializeFontDebug();
            GenerateChar('{');

            Keyboard.OnCharacterKey += Keyboard_OnCharacterKey;
        }

        private void Keyboard_OnCharacterKey(IO.CharacterEventArgs e)
        {
            GenerateChar(e.Character);
            _font2Test.GetCharGlyph(e.Character);
        }

        private void LoadFontFile()
        {
            string fontPath = "assets/euphorigenic.ttf";
            //string fontPath = "assets/BroshK.ttf";
            //string fontPath = "assets/Digitalt.ttf";

            Stopwatch fontTimer = new Stopwatch();
            using (FileStream stream = new FileStream(fontPath, FileMode.Open, FileAccess.Read))
            {
                using (FontReader reader = new FontReader(stream, Log, fontPath))
                {
                    fontTimer.Start();
                    _fontFile = reader.ReadFont(true);
                    fontTimer.Stop();
                    Log.WriteLine($"Took {fontTimer.Elapsed.TotalMilliseconds}ms to read font");

                    _font2Test = new SpriteFont(Engine.Renderer, _fontFile, 20);
                }
            }
        }

        private void LoadSystemFontFile(string fontName)
        {
            Stopwatch fontTimer = new Stopwatch();
            using (FontReader reader = new FontReader(fontName, Log))
            {
                fontTimer.Start();
                _fontFile = reader.ReadFont(true);
                fontTimer.Stop();
                Log.WriteLine($"Took {fontTimer.Elapsed.TotalMilliseconds}ms to read system font");

                _font2Test = new SpriteFont(Engine.Renderer, _fontFile, 20);
            }
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

            // Use a container for doing some testing.
            _container = new SpriteBatchContainer()
            {
                OnDraw = (sb) =>
                {
                    sb.DrawRectOutline(_glyphBounds, Color.Grey, 1);
                    sb.DrawRectOutline(_fontBounds, Color.Pink, 1);

                    // Top Difference marker
                    float dif = _glyphBounds.Top - _fontBounds.Top;
                    if (dif != 0)
                    {
                        sb.DrawLine(new Vector2F(_glyphBounds.Right, _fontBounds.Top), new Vector2F(_glyphBounds.Right, _fontBounds.Top + dif), Color.Red, 1);
                        sb.DrawString(TestFont, $"Dif: {dif}", new Vector2F(_glyphBounds.Right, _fontBounds.Top + (dif / 2)), Color.White);
                    }

                    // Bottom difference marker
                    dif = _fontBounds.Bottom - _glyphBounds.Bottom;
                    if (dif != 0)
                    {
                        sb.DrawLine(new Vector2F(_glyphBounds.Right, _fontBounds.Bottom), new Vector2F(_glyphBounds.Right, _fontBounds.Bottom - dif), Color.Red, 1);
                        sb.DrawString(TestFont, $"Dif: {dif}", new Vector2F(_glyphBounds.Right, _fontBounds.Bottom - (dif / 2)), Color.White);
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

                    sb.DrawString(TestFont, $"Mouse: { Mouse.Position}", new Vector2F(5, 300), Color.Yellow);

                    sb.DrawString(TestFont, $"Font atlas: ", new Vector2F(700, 45), Color.White);
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
                }
            };
            SampleScene.AddSprite(_container);
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

        private void Cr_OnCompleted(ContentManager content, ContentRequest cr)
        {
            if (cr.RequestedFiles.Count == 0)
                return;

            ITexture2D tex = content.Get<ITexture2D>(cr.RequestedFiles[0]);
            IMaterial mat = content.Get<IMaterial>(cr.RequestedFiles[1]);

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

            if (Mouse.IsTapped(IO.MouseButton.Left))
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
