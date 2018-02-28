using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Samples
{
    public class SpriteBatchPrimitives : SampleSceneGame
    {
        public override string Description => "Draws various primitives using sprite batch.";

        SceneObject _parent;
        SceneObject _child;
        Camera2D _cam2D;
        IMesh<VertexTexture> _mesh;

        public SpriteBatchPrimitives(EngineSettings settings = null) : base("Sprite Batch Primitives", settings) { }

        protected override void OnInitialize(Engine engine)
        {
            base.OnInitialize(engine);

            _cam2D = new Camera2D()
            {
                OutputSurface = Window,
                OutputDepthSurface = WindowDepthSurface,
            };

            ContentRequest cr = engine.Content.StartRequest();
            cr.Load<IMaterial>("BasicTexture.sbm");
            cr.Load<ITexture2D>("png_test.png;mipmaps=true");
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
        }

        private void SetupRectangles()
        {
            for (int i = 0; i < 50; i++)
            {
                RectangleSprite s = new RectangleSprite()
                {
                    Destination = new Rectangle()
                    {
                        X = Rng.Next(0, 1920),
                        Y = Rng.Next(0, 1080),
                        Width = Rng.Next(16, 129),
                        Height = Rng.Next(16, 129)
                    },

                    Color = new Color()
                    {
                        R = (byte)Rng.Next(0, 255),
                        G = (byte)Rng.Next(0, 255),
                        B = (byte)Rng.Next(0, 255),
                        A = 40,
                    },

                    Origin = new Vector2(0.5f),
                };

                SampleScene.RenderData.AddSprite(s);
            }
        }

        private void Cr_OnCompleted(ContentManager content, ContentRequest cr)
        {
            if (cr.RequestedFiles.Count == 0)
                return;

            IMaterial mat = content.Get<IMaterial>(cr.RequestedFiles[0]);
            if (mat == null)
            {
                Exit();
                return;
            }

            ITexture2D tex = content.Get<ITexture2D>(cr.RequestedFiles[1]);
            mat.SetDefaultResource(tex, 0);
            _mesh.Material = mat;
            SetupRectangles();

            // Create points for zig-zagging lines.
            List<Vector2> linePoints = new List<Vector2>();
            float y = 100;
            Vector2 lineOffset = new Vector2(300, 100);
            for (int i = 0; i < 20; i++)
            {
                linePoints.Add(lineOffset + new Vector2(i * 50, y));
                y = 100 - y;
            }

            // Create a second batch of lines for a circle outline
            List<Vector2> circleLinePoints = new List<Vector2>();
            float radius = 100;
            Vector2 origin = new Vector2(500);
            int segments = 32;
            float angleIncrement = 360.0f / segments;
            float angle = 0;

            for(int i = 0; i <= segments; i++)
            {
                float rad = angle * MathHelper.DegToRad;
                angle += angleIncrement;

                circleLinePoints.Add(new Vector2()
                {
                    X = origin.X + (float)Math.Sin(rad) * radius,
                    Y = origin.Y + (float)Math.Cos(rad) * radius,
                });
            }

            // Add 5 colors. The last color will be used when we have more points than colors.
            List<Color> lineColors = new List<Color>();
            lineColors.Add(Color.White);
            lineColors.Add(Color.Red);
            lineColors.Add(Color.Lime);
            lineColors.Add(Color.Blue);
            lineColors.Add(Color.Yellow);

            // Use a container for doing some testing.
            SpriteBatchContainer sbContainer = new SpriteBatchContainer()
            {
                OnDraw = (sb) =>
                {
                    sb.DrawLine(new Vector2(0), new Vector2(400), Color.White, 1);
                    sb.DrawLines(linePoints, lineColors, 2);
                    sb.DrawLines(circleLinePoints, lineColors, 4);
                }
            };
            SampleScene.AddSprite(sbContainer);
        }

        protected override void OnUpdate(Timing time)
        {
            RotateParentChild(_parent, _child, time);

            base.OnUpdate(time);
        }
    }
}
