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
        List<Matrix> _positions;
        Random _rng;
        SceneCameraComponent _cam;
        Camera2D _cam2D;
        ISpriteFont _font;
        List<ISprite> _sprites;
        IMesh<VertexTexture> _mesh;

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
            _positions = new List<Matrix>();
            _font = engine.Renderer.Resources.CreateFont("arial", 36);

            ContentRequest cr = engine.Content.StartRequest();
            cr.Load<ITexture2D>("png_test.png;mipmaps=true");
            cr.Load<IMaterial>("Basictexture.sbm");
            cr.OnCompleted += Cr_OnCompleted;
            cr.Commit();

            _mesh = Engine.Renderer.Resources.CreateMesh<VertexTexture>(36);

            VertexTexture[] verts = new VertexTexture[]{
               new VertexTexture(new Vector3(-1,-1,-1), new Vector2(0,1)), //front
               new VertexTexture(new Vector3(-1,1,-1), new Vector2(0,0)),
               new VertexTexture(new Vector3(1,1,-1), new Vector2(1,0)),
               new VertexTexture(new Vector3(-1,-1,-1), new Vector2(0,1)),
               new VertexTexture(new Vector3(1,1,-1), new Vector2(1, 0)),
               new VertexTexture(new Vector3(1,-1,-1), new Vector2(1,1)),

               new VertexTexture(new Vector3(-1,-1,1), new Vector2(1,0)), //back
               new VertexTexture(new Vector3(1,1,1), new Vector2(0,1)),
               new VertexTexture(new Vector3(-1,1,1), new Vector2(1,1)),
               new VertexTexture(new Vector3(-1,-1,1), new Vector2(1,0)),
               new VertexTexture(new Vector3(1,-1,1), new Vector2(0, 0)),
               new VertexTexture(new Vector3(1,1,1), new Vector2(0,1)),

               new VertexTexture(new Vector3(-1,1,-1), new Vector2(0,1)), //top
               new VertexTexture(new Vector3(-1,1,1), new Vector2(0,0)),
               new VertexTexture(new Vector3(1,1,1), new Vector2(1,0)),
               new VertexTexture(new Vector3(-1,1,-1), new Vector2(0,1)),
               new VertexTexture(new Vector3(1,1,1), new Vector2(1, 0)),
               new VertexTexture(new Vector3(1,1,-1), new Vector2(1,1)),

               new VertexTexture(new Vector3(-1,-1,-1), new Vector2(1,0)), //bottom
               new VertexTexture(new Vector3(1,-1,1), new Vector2(0,1)),
               new VertexTexture(new Vector3(-1,-1,1), new Vector2(1,1)),
               new VertexTexture(new Vector3(-1,-1,-1), new Vector2(1,0)),
               new VertexTexture(new Vector3(1,-1,-1), new Vector2(0, 0)),
               new VertexTexture(new Vector3(1,-1,1), new Vector2(0,1)),

               new VertexTexture(new Vector3(-1,-1,-1), new Vector2(0,1)), //left
               new VertexTexture(new Vector3(-1,-1,1), new Vector2(0,0)),
               new VertexTexture(new Vector3(-1,1,1), new Vector2(1,0)),
               new VertexTexture(new Vector3(-1,-1,-1), new Vector2(0,1)),
               new VertexTexture(new Vector3(-1,1,1), new Vector2(1, 0)),
               new VertexTexture(new Vector3(-1,1,-1), new Vector2(1,1)),

               new VertexTexture(new Vector3(1,-1,-1), new Vector2(1,0)), //right
               new VertexTexture(new Vector3(1,1,1), new Vector2(0,1)),
               new VertexTexture(new Vector3(1,-1,1), new Vector2(1,1)),
               new VertexTexture(new Vector3(1,-1,-1), new Vector2(1,0)),
               new VertexTexture(new Vector3(1,1,-1), new Vector2(0, 0)),
               new VertexTexture(new Vector3(1,1,1), new Vector2(0,1)),
            };
            _mesh.SetVertices(verts);
            SpawnParentChild(_mesh, Vector3.Zero, out _parent, out _child);

            NewFontSystemTest();
        }

        /// <summary>
        /// A test for a new WIP sprite font system.
        /// </summary>
        private void NewFontSystemTest()
        {
            // Hi. I'm just a piece of test code for the new WIP font system. Please ignore me.
            //string fontPath = "assets/euphorigenic.ttf";
            //string fontPath = "assets/BroshK.ttf";
            //string fontPath = "assets/Digitalt.ttf";
            //string fontPath = "assets/STOREB.ttf"; // For testing 'cmap' (format 4 and 6).
            string fontPath = "assets/UECHIGOT.TTF"; // For testing 'PCLT', 'cmap' (format 0 and 4).

            FontFile font;
            Stopwatch fontTimer = new Stopwatch();
            using (FileStream stream = new FileStream(fontPath, FileMode.Open, FileAccess.Read))
            {
                using (FontReader reader = new FontReader(stream, Log, fontPath))
                {
                    fontTimer.Start();
                    font = reader.ReadFont();
                    fontTimer.Stop();
                    Log.WriteLine($"Took {fontTimer.Elapsed.TotalMilliseconds}ms to read font");
                }
            }

            //  Test mesh holes - anti-clockwise
            Rectangle box = new Rectangle(800, 500, 300, 200);
            List<Vector2> boxPoints = new List<Vector2>();
            boxPoints.Add(new Vector2(box.Left, box.Top));
            boxPoints.Add(new Vector2(box.Left, box.Bottom));
            boxPoints.Add(new Vector2(box.Right, box.Bottom));
            boxPoints.Add(new Vector2(box.Right, box.Top));
            List<Vector2> boxTriangleList = new List<Vector2>();
            Shape boxShape = new Shape(boxPoints);

            // Box hole - clockwise
            Rectangle boxInner = box;
            boxPoints.Clear();
            boxInner.Inflate(-20);
            boxPoints.Add(new Vector2(boxInner.Left, boxInner.Top));
            boxPoints.Add(new Vector2(boxInner.Right, boxInner.Top));
            boxPoints.Add(new Vector2(boxInner.Right, boxInner.Bottom));
            boxPoints.Add(new Vector2(boxInner.Left, boxInner.Bottom));
            Shape boxHole = new Shape(boxPoints);
            boxShape.AddHole(boxHole);

            boxShape.Triangulate(boxTriangleList);

            char glyphChar = '8';
            Log.WriteDebugLine($"FontFile test: using glyph at index {font.GetGlyphIndex(glyphChar)}");
            Glyph cGlyph = font.GetGlyph(glyphChar);
            ushort[] endPoints = cGlyph.ContourEndPoints;
            int start = 0;

            // Draw outline
            List<Vector2>[] linePoints = new List<Vector2>[endPoints.Length];
            Vector2 offset = new Vector2(300);
            float scale = 0.25f;

            for (int i = 0; i < endPoints.Length; i++)
            {
                List<Vector2> points = new List<Vector2>();
                linePoints[i] = points;
                int end = endPoints[i];

                // Offset the points so we can see them
                // Flip the Y axis because 0,0 is top-left, not bottom-left.
                for (int p = start; p <= end; p++)
                {
                    if (!cGlyph.Points[p].IsOnCurve)
                        continue;

                    Vector2 point = cGlyph.Points[p].Point;
                    point.Y = cGlyph.Bounds.Height - point.Y;
                    points.Add((point * scale) + offset);
                }

                // Add the first point again to create a loop (for rendering only)
                points.Add(points[0]);

                // Set the current end as the start of the next run.
                start = end + 1;
            }

            // Use a container for doing some testing.
            SpriteBatchContainer sbContainer = new SpriteBatchContainer()
            {
                OnDraw = (sb) =>
                {
                    //sb.DrawTriangleList(triPoints, Color.Yellow);
                    for (int i = 0; i < linePoints.Length; i++)
                        sb.DrawLines(linePoints[i], Color.Red, 1);

                    sb.DrawTriangleList(boxTriangleList, Color.Yellow);
                }
            };
            SampleScene.AddSprite(sbContainer);
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
