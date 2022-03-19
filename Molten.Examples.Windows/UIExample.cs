using Molten.Graphics;
using Molten.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Samples
{
    public class UIExample : SampleSceneGame
    {
        public override string Description => "Demonstrates Molten's UI system.";

        SceneObject _parent;
        SceneObject _child;
        IMesh<VertexTexture> _mesh;
        Color[] _rectangleColors;
        UIRenderComponent _ui;

        public UIExample() : base("UI Example") { }

        protected override void OnInitialize(Engine engine)
        {
            base.OnInitialize(engine);

            ContentRequest cr = engine.Content.BeginRequest("assets/");
            cr.Load<IMaterial>("BasicTexture.mfx");
            cr.Load<ITexture2D>("dds_test.dds;mipmaps=true");
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
        }

        private void Cr_OnCompleted(ContentRequest cr)
        {
            if (cr.RequestedFileCount == 0)
                return;

            IMaterial mat = cr.Get<IMaterial>(0);
            if (mat == null)
            {
                Exit();
                return;
            }

            ITexture2D tex = cr.Get<ITexture2D>(1);
            mat.SetDefaultResource(tex, 0);
            _mesh.Material = mat;

            // Create points for zig-zagging lines.
            List<Vector2F> linePoints = new List<Vector2F>();
            float y = 100;
            Vector2F lineOffset = new Vector2F(300, 100);
            for (int i = 0; i < 20; i++)
            {
                linePoints.Add(lineOffset + new Vector2F(i * 50, y));
                y = 100 - y;
            }

            // Create a second batch of lines for a circle outline
            List<Vector2F> circleLinePoints = new List<Vector2F>();
            float radius = 50;

            Vector2F origin = new Vector2F(500);
            int outlineSegments = 32;
            float angleIncrement = 360.0f / outlineSegments;
            float angle = 0;

            for(int i = 0; i <= outlineSegments; i++)
            {
                float rad = angle * MathHelper.DegToRad;
                angle += angleIncrement;

                circleLinePoints.Add(new Vector2F()
                {
                    X = origin.X + (float)Math.Sin(rad) * radius,
                    Y = origin.Y + (float)Math.Cos(rad) * radius,
                });
            }

            // Add 5 colors. The last color will be used when we have more points than colors.
            List<Color> colors = new List<Color>();
            colors.Add(Color.Orange);
            colors.Add(Color.Red);
            colors.Add(Color.Lime);
            colors.Add(Color.Blue);
            colors.Add(Color.Yellow);

            List<Vector2F> triPoints = new List<Vector2F>();
            triPoints.Add(new Vector2F(600, 220)); // First triangle
            triPoints.Add(new Vector2F(630, 320));
            triPoints.Add(new Vector2F(700, 260));
            triPoints.Add(new Vector2F(710, 220)); // Second triangle
            triPoints.Add(new Vector2F(730, 360));
            triPoints.Add(new Vector2F(770, 280));

            _ui = SpriteLayer.AddObjectWithComponent<UIRenderComponent>();
            _ui.Root = new UIPanel()
            {
                LocalBounds = new Rectangle(100, 150, 600, 450),
            };

            UIPanel childPanel = new UIPanel()
            {
                LocalBounds = new Rectangle(100, 50, 220, 200),
                Parent = _ui.Root,
                Properties = new UIPanel.RenderData()
                {
                    BackgroundColor = new Color(0,128, 0, 200),
                    BorderColor = Color.LimeGreen
                },
            };

            UILabel label = new UILabel()
            {
                LocalBounds = new Rectangle(300, 150, 200, 50),
                Parent = _ui.Root
            };
        }

        protected override void OnUpdate(Timing time)
        {
            RotateParentChild(_parent, _child, time);

            base.OnUpdate(time);
        }
    }
}
