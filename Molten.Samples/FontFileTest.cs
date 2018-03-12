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
        List<Vector2F> _contourHits;
        List<Vector2F> _intersectionLines;
        List<Color> _intersectColors;
        Stopwatch _testTimer;
        public FontFileTest(EngineSettings settings = null) : base("FontFile Test", settings) { }

        protected override void OnInitialize(Engine engine)
        {
            base.OnInitialize(engine);

            _sprites = new List<ISprite>();
            _rng = new Random();
            _positions = new List<Matrix4F>();
            _font = engine.Renderer.Resources.CreateFont("arial", 36);
            _testTimer = new Stopwatch();

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
            NewFontSystemTest('l');

            Keyboard.OnCharacterKey += Keyboard_OnCharacterKey;
        }

        private void Keyboard_OnCharacterKey(IO.CharacterEventArgs e)
        {
            NewFontSystemTest(e.Character);
        }

        private void LoadFontFile()
        {
            // Hi. I'm just a piece of test code for the new WIP font system. Please ignore me.
            //string fontPath = "assets/euphorigenic.ttf";
            //string fontPath = "assets/BroshK.ttf";
            //string fontPath = "assets/Digitalt.ttf";
            //string fontPath = "assets/STOREB.ttf"; // For testing 'cmap' (format 4 and 6).
            string fontPath = "assets/UECHIGOT.TTF"; // For testing 'PCLT', 'cmap' (format 0 and 4).

            Stopwatch fontTimer = new Stopwatch();
            using (FileStream stream = new FileStream(fontPath, FileMode.Open, FileAccess.Read))
            {
                using (FontReader reader = new FontReader(stream, Log, fontPath))
                {
                    fontTimer.Start();
                    _fontFile = reader.ReadFont();
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

            Log.WriteDebugLine($"FontFile test using glyph at index {_fontFile.GetGlyphIndex(glyphChar)}");
            Glyph glyph = _fontFile.GetGlyph(glyphChar);
            _contourHits = new List<Vector2F>();
            _intersectionLines = new List<Vector2F>();
            _intersectColors = new List<Color>();
            _intersectColors.Add(Color.Lime);

             _shapes = glyph.CreateShapes(2, true);
            ushort[] endPoints = glyph.ContourEndPoints;

            // Add 5 colors. The last color will be used when we have more points than colors.
            List<Color> colors = new List<Color>();
            colors.Add(Color.Orange);
            colors.Add(Color.Yellow);

            // Draw outline
            Vector2F offset = new Vector2F(350, 100);
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

                foreach(Shape h in shape.Holes)
                {
                    List<Vector2F> hPoints = new List<Vector2F>();
                    holePoints.Add(hPoints);

                    for (int j = 0; j < h.Points.Count; j++)
                        hPoints.Add((Vector2F)h.Points[j]);
                }
            }

            RectangleF glyphBounds = glyph.Bounds;
            glyphBounds.Top *= scale;
            glyphBounds.Left *= scale;
            glyphBounds.X += offset.X;
            glyphBounds.Y += offset.Y;
            glyphBounds.Width *= scale;
            glyphBounds.Height *= scale;

            List<Vector2F> glyphTriPoints = new List<Vector2F>();
            //glyphShapes[0].Triangulate(glyphTriPoints, offset, scale);

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

                    sb.DrawTriangleList(glyphTriPoints, colors);
                    for (int i = 0; i < linePoints.Count; i++)
                        sb.DrawLinePath(linePoints[i], Color.Red, 2);

                    for (int i = 0; i < holePoints.Count; i++)
                        sb.DrawLinePath(holePoints[i], Color.SkyBlue, 2);

                    Rectangle clickRect;
                    if (_shapes != null)
                    {
                        sb.DrawLineList(_intersectionLines, _intersectColors, 1);
                        sb.DrawLine(_clickPoint, _clickPoint + new Vector2F(3000, 0), Color.Orange, 1);

                        for(int i = 0; i < _contourHits.Count; i++)
                        {
                            Vector2F p = _contourHits[i];
                            clickRect = new Rectangle((int)p.X, (int)p.Y, 0, 0);
                            clickRect.Inflate(5);
                            sb.DrawRect(clickRect, _clickColor);
                        }

                        clickRect = new Rectangle((int)_clickPoint.X, (int)_clickPoint.Y, 0, 0);
                        clickRect.Inflate(8);
                        sb.DrawRect(clickRect, _clickColor);
                        sb.DrawString(TestFont, $"Contains Time: {_testTimer.Elapsed.TotalMilliseconds.ToString("N2")}ms", new Vector2F(5, 440), Color.White);
                        sb.DrawString(TestFont, $"Mouse: {Mouse.Position}", new Vector2F(5, 460), Color.White);
                        sb.DrawString(TestFont, $"Click point: {_clickPoint}", new Vector2F(5, 480), Color.White);
                        sb.DrawString(TestFont, $"Line points: {_intersectionLines.Count}", new Vector2F(5, 500), Color.White);
                        sb.DrawString(TestFont, $"Hit points: {_contourHits.Count}", new Vector2F(5,520), Color.White);
                        for(int i = 0; i < _contourHits.Count; i++)
                            sb.DrawString(TestFont, $"  {_contourHits[i]}", new Vector2F(5, 540 + (20 * i)), Color.White);

                        float hitX = 900;
                        float hitY = 40;
                        for (int i = 0; i < _intersectionLines.Count; i++)
                        {
                            sb.DrawString(TestFont, $"  {_intersectionLines[i]}", new Vector2F(hitX, hitY), Color.White);
                            hitY += 20;
                            if(hitY >= Window.Height)
                            {
                                hitY = 5;
                                hitX += 130;
                            }
                        }
                    }
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

                if(_shapes != null)
                {
                    _contourHits.Clear();
                    _intersectionLines.Clear();

                    _testTimer.Reset();
                    _testTimer.Start();
                    foreach (Shape shape in _shapes)
                    {
                        if (ContainsTest(shape, _clickPoint))
                        {
                            _clickColor = Color.Green;
                            break;
                        }
                    }
                    _testTimer.Stop();
                }
            }

            base.OnUpdate(time);
        }

        private bool ContainsTest(Shape shape, Vector2F point)
        {
            const float X_THRESHOLD = 0.5f; // Half a pixel
            List<Shape> holes = shape.Holes;    

            // Test holes first
            for (int i = 0; i < holes.Count; i++)
            {
                if (holes[i].Contains(point))
                    return false;
            }

            Vector2F p1;
            Vector2F p2;
            int hitCount = 0;

            Ray originRay = new Ray(new Vector3F(point, 0), Vector3F.Right);
            Vector3F rayHit;
            int end = shape.Points.Count - 1;
            RectangleF inverseRect = new RectangleF()
            {
                Left = int.MaxValue,
                Right = int.MinValue,
                Top = int.MaxValue,
                Bottom = int.MinValue,
            };

            float prevX = float.NaN;
            float prevIntersectX = prevX;
            bool prevRunsLeft = true;

            for (int i = 0; i < end; i++)
            {
                p1 = (Vector2F)shape.Points[i];
                p2 = (Vector2F)shape.Points[i + 1];

                RectangleF contourBounds = inverseRect;
                contourBounds.Encapsulate(p1);
                contourBounds.Encapsulate(p2);
                Vector2F contourDir = -Vector2F.Normalize(p2 - p1);
                Ray contourRay = new Ray(new Vector3F(p1, 0), new Vector3F(contourDir, 0));

                if (CollisionHelper.RayIntersectsRay(ref originRay, ref contourRay, out rayHit))
                {
                    // check if intersection is on the left of the point.
                    if (rayHit.X < point.X)
                        continue;

                    Vector2F intersect = (Vector2F)rayHit;
                    if (contourBounds.Contains(intersect))
                    {
                        //_intersectionLines.Add(contourBounds.TopLeft);
                        //_intersectionLines.Add(contourBounds.TopRight);
                        //_intersectionLines.Add(contourBounds.TopRight);
                        //_intersectionLines.Add(contourBounds.BottomRight);
                        //_intersectionLines.Add(contourBounds.BottomRight);
                        //_intersectionLines.Add(contourBounds.BottomLeft);
                        //_intersectionLines.Add(contourBounds.BottomLeft);
                        //_intersectionLines.Add(contourBounds.TopLeft);

                        // Check if intersect point is the same as the previous one.
                        if (prevX != intersect.X)
                        {
                            float xDist = 0;
                            bool runsLeft = (contourDir.X < 0);

                            if (!float.IsNaN(prevX))
                            {
                                if (prevX == p1.X)
                                {
                                    xDist = (prevIntersectX - intersect.X) / X_THRESHOLD;
                                    if (prevRunsLeft != runsLeft)
                                        xDist = 0;
                                    else if (xDist > -1f && xDist < 1f)
                                        xDist = 1;
                                    else
                                        xDist = 0;
                                }
                            }

                            if (xDist == 0)
                            {
                                _contourHits.Add(intersect);
                                _intersectionLines.Add(p1);
                                _intersectionLines.Add(p2);
                                hitCount++;
                                prevIntersectX = intersect.X;
                                prevX = p2.X;
                                prevRunsLeft = runsLeft;
                            }
                        }
                    }
                }
            }

            Console.WriteLine($"Hit count: {hitCount}");
            return hitCount % 2 != 0;
        }
    }
}
