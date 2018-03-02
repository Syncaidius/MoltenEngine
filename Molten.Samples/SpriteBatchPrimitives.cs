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
            float radius = 50;

            Vector2 origin = new Vector2(500);
            int outlineSegments = 32;
            float angleIncrement = 360.0f / outlineSegments;
            float angle = 0;

            for(int i = 0; i <= outlineSegments; i++)
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
            List<Color> colors = new List<Color>();
            colors.Add(Color.Orange);
            colors.Add(Color.Red);
            colors.Add(Color.Lime);
            colors.Add(Color.Blue);
            colors.Add(Color.Yellow);

            List<Vector2> triPoints = new List<Vector2>();
            triPoints.Add(new Vector2(600, 220)); // First triangle
            triPoints.Add(new Vector2(630, 320));
            triPoints.Add(new Vector2(700, 260));
            triPoints.Add(new Vector2(710, 220)); // Second triangle
            triPoints.Add(new Vector2(730, 360));
            triPoints.Add(new Vector2(770, 280));

            // Define a test shape
            Shape testShape = new Shape(new List<Vector2>()
            {
                new Vector2(2158.9981f,2350.2286f),
                new Vector2(2158.9981f,3245.4557f),
                new Vector2(-1042.9463f,3245.4557f),
                new Vector2(-1042.9463f,2496.1895f),
                new Vector2(91.149593f,800.20639f),
                new Vector2(441.75649f,251.73749f),
                new Vector2(648.06929f,-97.04991f),
                new Vector2(765.46219f,-332.30851f),
                new Vector2(849.31479f,-540.20071f),
                new Vector2(899.62689f,-720.72671f),
                new Vector2(916.39869f,-873.88651f),
                new Vector2(896.13819f,-1060.7944f),
                new Vector2(835.35969f,-1193.3788f),
                new Vector2(789.54889f,-1239.4959f),
                new Vector2(733.15879f,-1272.4376f),
                new Vector2(666.18939f,-1292.204f),
                new Vector2(588.64059f,-1298.7951f),
                new Vector2(511.08979f,-1291.4964f),
                new Vector2(444.11959f,-1269.6012f),
                new Vector2(387.73029f,-1233.1107f),
                new Vector2(341.92169f,-1182.0263f),
                new Vector2(306.46619f,-1109.2461f),
                new Vector2(281.14119f,-1007.6808f),
                new Vector2(260.88259f,-718.19491f),
                new Vector2(260.88259f,-218.68401f),
                new Vector2(-1042.9463f,-218.68401f),
                new Vector2(-1042.9463f,-410.05511f),
                new Vector2(-1030.3404f,-804.55201f),
                new Vector2(-992.52205f,-1105.8022f),
                new Vector2(-958.08057f,-1232.6032f),
                new Vector2(-905.18018f,-1358.3923f),
                new Vector2(-833.82067f,-1483.1695f),
                new Vector2(-744.00213f,-1606.9348f),
                new Vector2(-637.5262f,-1722.6871f),
                new Vector2(-516.1928f,-1823.4397f),
                new Vector2(-380.00205f,-1909.1927f),
                new Vector2(-228.95374f,-1979.9461f),
                new Vector2(-62.599167f,-2035.2866f),
                new Vector2(119.51329f,-2074.8167f),
                new Vector2(317.38399f,-2098.5364f),
                new Vector2(531.01279f,-2106.4456f),
                new Vector2(938.57049f,-2082.2155f),
                new Vector2(1122.512f,-2051.9328f),
                new Vector2(1293.2285f,-2009.5383f),
                new Vector2(1450.7202f,-1955.0316f),
                new Vector2(1594.987f,-1888.4129f),
                new Vector2(1726.0289f,-1809.6817f),
                new Vector2(1843.846f,-1718.8382f),
                new Vector2(2038.4505f,-1512.159f),
                new Vector2(2177.4543f,-1279.7356f),
                new Vector2(2260.8578f,-1021.5681f),
                new Vector2(2288.6606f,-737.65631f),
                new Vector2(2273.0151f,-508.98211f),
                new Vector2(2226.0792f,-273.82221f),
                new Vector2(2147.8538f,-32.17651f),
                new Vector2(2038.3398f,215.95519f),
                new Vector2(1852.2859f,537.88159f),
                new Vector2(1544.4495f,1000.9025f),
                new Vector2(1114.8304f,1605.018f),
                new Vector2(563.42839f,2350.2286f),
            });

            List<Vector2> shapeTriList = new List<Vector2>();
            testShape.Triangulate(shapeTriList, new Vector2(100,400), 0.0625f);

            // Use a container for doing some testing.
            SpriteBatchContainer sbContainer = new SpriteBatchContainer()
            {
                OnDraw = (sb) =>
                {
                    sb.DrawLine(new Vector2(0), new Vector2(400), Color.White, 1);
                    sb.DrawLines(linePoints, colors, 2);
                    sb.DrawLines(circleLinePoints, colors, 4);
                    sb.DrawTriangle(new Vector2(400, 220), new Vector2(350, 320), new Vector2(500, 260), Color.SkyBlue);
                    sb.DrawTriangle(new Vector2(500, 220), new Vector2(590, 350), new Vector2(650, 280), Color.Violet);
                    sb.DrawTriangleList(triPoints, colors);
                    sb.DrawTriangleList(shapeTriList, colors);

                    // Draw a few circles with a decreasing number of sides.
                    origin.X = 500;
                    origin.Y = 500;
                    int circleSides = 80;
                    for (int i = 0; i < colors.Count; i++)
                    {
                        origin.X += 100;
                        sb.DrawCircle(origin, 50, colors[i], circleSides);
                        circleSides /= 2;
                    }
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
