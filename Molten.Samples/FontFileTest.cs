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
        ISpriteFont _font;
        List<ISprite> _sprites;
        IMesh<VertexTexture> _mesh;
        SpriteBatchContainer _container;
        FontFile _fontFile;

        Vector2F _clickPoint;
        Color _clickColor = Color.Red;
        List<Shape> _shapes;
        public FontFileTest(EngineSettings settings = null) : base("FontFile Test", settings) { }

        protected override void OnInitialize(Engine engine)
        {
            base.OnInitialize(engine);

            _sprites = new List<ISprite>();
            _rng = new Random();
            _positions = new List<Matrix4F>();
            _font = engine.Renderer.Resources.CreateFont("arial", 36);

            ContentRequest cr = engine.Content.StartRequest();
            cr.Load<ITexture2D>("png_test.png;mipmaps=true");
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

            LoadFontFile();
            NewFontSystemTest('{');

            Keyboard.OnCharacterKey += Keyboard_OnCharacterKey;
        }

        private void Keyboard_OnCharacterKey(IO.CharacterEventArgs e)
        {
            NewFontSystemTest(e.Character);
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
                }
            }
        }

        /// <summary>
        /// A test for a new WIP sprite font system.
        /// </summary>
        private void NewFontSystemTest(char glyphChar)
        {
            if (_container != null)
                SampleScene.RemoveSprite(_container);

            Glyph glyph = _fontFile.GetGlyph(glyphChar);
             _shapes = glyph.CreateShapes(16);

            // Add 5 colors. The last color will be used when we have more points than colors.
            List<Color> colors = new List<Color>();
            colors.Add(Color.Wheat);
            colors.Add(Color.Yellow);

            // Draw outline
            Vector2F offset = new Vector2F(300,300);
            float scale = 0.3f;
            foreach (Shape s in _shapes)
                s.ScaleAndOffset(offset, scale);

            List<List<Vector2F>> linePoints = new List<List<Vector2F>>();
            List<List<Vector2F>> holePoints = new List<List<Vector2F>>();
            for (int i = 0; i < _shapes.Count; i++)
            {
                Shape shape = _shapes[i];
                List<Vector2F> points = new List<Vector2F>();
                linePoints.Add(points);

                for (int j = 0; j < shape.Points.Count; j++)
                    points.Add((Vector2F)shape.Points[j]);

                foreach (Shape h in shape.Holes)
                {
                    List<Vector2F> hPoints = new List<Vector2F>();
                    holePoints.Add(hPoints);

                    for (int j = 0; j < h.Points.Count; j++)
                        hPoints.Add((Vector2F)h.Points[j]);
                }
            }

            RectangleF glyphBounds = glyph.Bounds;
            glyphBounds.X *= scale;
            glyphBounds.Y *= scale;
            glyphBounds.Width *= scale;
            glyphBounds.Height *= scale;
            glyphBounds.X += offset.X;
            glyphBounds.Y += offset.Y;


            RectangleF containerBounds = _fontFile.ContainerBounds;
            containerBounds.X *= scale;
            containerBounds.Y *= scale;
            containerBounds.Width *= scale;
            containerBounds.Height *= scale;
            containerBounds.X += offset.X;
            containerBounds.Y += offset.Y;

            List<Vector2F> glyphTriPoints = new List<Vector2F>();

            foreach (Shape s in _shapes)
                s.Triangulate(glyphTriPoints, Vector2F.Zero, 1);

            // Use a container for doing some testing.
            _container = new SpriteBatchContainer()
            {
                OnDraw = (sb) =>
                {
                    // Draw glyph bounds
                    sb.DrawLine(glyphBounds.TopLeft, glyphBounds.TopRight, Color.Grey, 1);
                    sb.DrawLine(glyphBounds.TopRight, glyphBounds.BottomRight, Color.Grey, 1);
                    sb.DrawLine(glyphBounds.BottomRight, glyphBounds.BottomLeft, Color.Grey, 1);                    
                    sb.DrawLine(glyphBounds.BottomLeft, glyphBounds.TopLeft, Color.Grey, 1);

                    // Draw font bounds
                    sb.DrawLine(containerBounds.TopLeft, containerBounds.TopRight, Color.Pink, 1);
                    sb.DrawLine(containerBounds.TopRight, containerBounds.BottomRight, Color.Pink, 1);
                    sb.DrawLine(containerBounds.BottomRight, containerBounds.BottomLeft, Color.Pink, 1);
                    sb.DrawLine(containerBounds.BottomLeft, containerBounds.TopLeft, Color.Pink, 1);

                    // Top Difference marker
                    float dif = glyphBounds.Top - containerBounds.Top;
                    if (dif != 0)
                    {
                        sb.DrawLine(new Vector2F(glyphBounds.Right, containerBounds.Top), new Vector2F(glyphBounds.Right, containerBounds.Top + dif), Color.Red, 1);
                        sb.DrawString(TestFont, $"Dif: {dif}", new Vector2F(glyphBounds.Right, containerBounds.Top + (dif / 2)), Color.White);
                    }

                    // Bottom difference marker
                    dif = containerBounds.Bottom - glyphBounds.Bottom;
                    if (dif != 0)
                    {
                        sb.DrawLine(new Vector2F(glyphBounds.Right, containerBounds.Bottom), new Vector2F(glyphBounds.Right, containerBounds.Bottom - dif), Color.Red, 1);
                        sb.DrawString(TestFont, $"Dif: {dif}", new Vector2F(glyphBounds.Right, containerBounds.Bottom - (dif / 2)), Color.White);
                    }

                    sb.DrawTriangleList(glyphTriPoints, colors);

                    for (int i = 0; i < linePoints.Count; i++)
                        sb.DrawLinePath(linePoints[i], Color.Red, 2);

                    for (int i = 0; i < holePoints.Count; i++)
                        sb.DrawLinePath(holePoints[i], Color.SkyBlue, 2);

                    Rectangle clickRect;
                    if (_shapes != null)
                    {
                        clickRect = new Rectangle((int)_clickPoint.X, (int)_clickPoint.Y, 0, 0);
                        clickRect.Inflate(8);
                        sb.DrawRect(clickRect, _clickColor);
                    }

                    sb.DrawString(TestFont, $"Mouse: { Mouse.Position}", new Vector2F(5, 300), Color.Yellow);
                }
            };
            SampleScene.AddSprite(_container);
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
                        if (ContainsTest(s, _clickPoint))
                            _clickColor = Color.Green;
                    }
                }
            }

            base.OnUpdate(time);
        }

        public bool ContainsTest(Shape shape, Vector2F point)
        {
            for(int i = 0; i < shape.Holes.Count; i++)
            {
                if (shape.Holes[i].Contains(point))
                    return false;
            }

            int polygonLength = shape.Points.Count;
            int j = 0;
            bool inside = false;
            float pointX = point.X, pointY = point.Y; // x, y for tested point.

            // start / end point for the current polygon segment.
            float startX, startY, endX, endY;
            Vector2F endPoint = (Vector2F)shape.Points[polygonLength - 1];
            endX = endPoint.X;
            endY = endPoint.Y;

            while (j < polygonLength)
            {
                startX = endX; startY = endY;
                endPoint = (Vector2F)shape.Points[j++];
                endX = endPoint.X; endY = endPoint.Y;
                //
                inside ^= (endY > pointY ^ startY > pointY) /* ? pointY inside [startY;endY] segment ? */
                          && /* if so, test if it is under the segment */
                          ((pointX - endX) < (pointY - endY) * (startX - endX) / (startY - endY));
            }

            return inside;
        }
    }
}
