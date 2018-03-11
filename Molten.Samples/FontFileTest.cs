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
        SceneCameraComponent _cam;
        Camera2D _cam2D;
        ISpriteFont _font;
        List<ISprite> _sprites;
        IMesh<VertexTexture> _mesh;
        SpriteBatchContainer _container;
        FontFile _fontFile;

        public FontFileTest(EngineSettings settings = null) : base("FontFile Test", settings)
        {

        }

        protected override void OnInitialize(Engine engine)
        {
            base.OnInitialize(engine);
            _cam = new SceneCameraComponent()
            {
                OutputSurface = Window,
                OutputDepthSurface = WindowDepthSurface,
            };

            _cam2D = new Camera2D()
            {
                OutputSurface = Window,
                OutputDepthSurface = WindowDepthSurface,
            };

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

            LoadFontFile();
            NewFontSystemTest('8');

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
            List<Shape> glyphShapes = glyph.CreateShapes(16, true);
            ushort[] endPoints = glyph.ContourEndPoints;
            int start = 0;

            // Add 5 colors. The last color will be used when we have more points than colors.
            List<Color> colors = new List<Color>();
            colors.Add(Color.Orange);
            colors.Add(Color.Yellow);

            // Draw outline
            Vector2F offset = new Vector2F(300);
            float scale = 0.25f;

            List<List<Vector2F>> linePoints = new List<List<Vector2F>>();
            List<List<Vector2F>> holePoints = new List<List<Vector2F>>();
            for (int i = 0; i < glyphShapes.Count; i++)
            {
                Shape shape = glyphShapes[i];
                List<Vector2F> points = new List<Vector2F>();
                linePoints.Add(points);

                for (int j = 0; j < shape.Points.Count; j++)
                {
                    Vector2F point = (Vector2F)shape.Points[j];
                    points.Add((point * scale) + offset);
                }

                foreach(Shape h in shape.Holes)
                {
                    List<Vector2F> hPoints = new List<Vector2F>();
                    holePoints.Add(hPoints);

                    for (int j = 0; j < h.Points.Count; j++)
                    {
                        Vector2F point = (Vector2F)h.Points[j];
                        hPoints.Add((point * scale) + offset);
                    }
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
            glyphShapes[0].Triangulate(glyphTriPoints, offset, scale);

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
                        sb.DrawLines(linePoints[i], Color.Red, 2);

                    for (int i = 0; i < holePoints.Count; i++)
                        sb.DrawLines(holePoints[i], Color.SkyBlue, 2);
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
            base.OnUpdate(time);
        }
    }
}
