using Molten.Graphics;

namespace Molten.Samples
{
    public class SpriteBatchPrimitives : SampleSceneGame
    {
        public override string Description => "Draws various primitives using sprite batch.";

        Rectangle[] _rectangles;
        Color[] _rectangleColors;
        ITexture2D _meshTex;

        public SpriteBatchPrimitives() : base("Sprite Batch Primitives") { }

        protected override void OnInitialize(Engine engine)
        {
            base.OnInitialize(engine);

            ContentRequest cr = engine.Content.BeginRequest("assets/");
            cr.Load<IMaterial>("BasicTexture.mfx");
            cr.Load<ITexture2D>("dds_test.dds", new TextureParameters()
            {
                GenerateMipmaps = true,
            });
            cr.OnCompleted += Cr_OnCompleted;
            cr.Commit();
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

            _meshTex = cr.Get<ITexture2D>(1);
            mat.SetDefaultResource(_meshTex, 0);
            TestMesh.Material = mat;

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

            for (int i = 0; i <= outlineSegments; i++)
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

            // Define a test shape
            List<Vector2F> pList = new List<Vector2F>()
            {
                new Vector2F(2158.9981f,2350.2286f),
                new Vector2F(2158.9981f,3245.4557f),
                new Vector2F(-1042.9463f,3245.4557f),
                new Vector2F(-1042.9463f,2496.1895f),
                new Vector2F(91.149593f,800.20639f),
                new Vector2F(441.75649f,251.73749f),
                new Vector2F(648.06929f,-97.04991f),
                new Vector2F(765.46219f,-332.30851f),
                new Vector2F(849.31479f,-540.20071f),
                new Vector2F(899.62689f,-720.72671f),
                new Vector2F(916.39869f,-873.88651f),
                new Vector2F(896.13819f,-1060.7944f),
                new Vector2F(835.35969f,-1193.3788f),
                new Vector2F(789.54889f,-1239.4959f),
                new Vector2F(733.15879f,-1272.4376f),
                new Vector2F(666.18939f,-1292.204f),
                new Vector2F(588.64059f,-1298.7951f),
                new Vector2F(511.08979f,-1291.4964f),
                new Vector2F(444.11959f,-1269.6012f),
                new Vector2F(387.73029f,-1233.1107f),
                new Vector2F(341.92169f,-1182.0263f),
                new Vector2F(306.46619f,-1109.2461f),
                new Vector2F(281.14119f,-1007.6808f),
                new Vector2F(260.88259f,-718.19491f),
                new Vector2F(260.88259f,-218.68401f),
                new Vector2F(-1042.9463f,-218.68401f),
                new Vector2F(-1042.9463f,-410.05511f),
                new Vector2F(-1030.3404f,-804.55201f),
                new Vector2F(-992.52205f,-1105.8022f),
                new Vector2F(-958.08057f,-1232.6032f),
                new Vector2F(-905.18018f,-1358.3923f),
                new Vector2F(-833.82067f,-1483.1695f),
                new Vector2F(-744.00213f,-1606.9348f),
                new Vector2F(-637.5262f,-1722.6871f),
                new Vector2F(-516.1928f,-1823.4397f),
                new Vector2F(-380.00205f,-1909.1927f),
                new Vector2F(-228.95374f,-1979.9461f),
                new Vector2F(-62.599167f,-2035.2866f),
                new Vector2F(119.51329f,-2074.8167f),
                new Vector2F(317.38399f,-2098.5364f),
                new Vector2F(531.01279f,-2106.4456f),
                new Vector2F(938.57049f,-2082.2155f),
                new Vector2F(1122.512f,-2051.9328f),
                new Vector2F(1293.2285f,-2009.5383f),
                new Vector2F(1450.7202f,-1955.0316f),
                new Vector2F(1594.987f,-1888.4129f),
                new Vector2F(1726.0289f,-1809.6817f),
                new Vector2F(1843.846f,-1718.8382f),
                new Vector2F(2038.4505f,-1512.159f),
                new Vector2F(2177.4543f,-1279.7356f),
                new Vector2F(2260.8578f,-1021.5681f),
                new Vector2F(2288.6606f,-737.65631f),
                new Vector2F(2273.0151f,-508.98211f),
                new Vector2F(2226.0792f,-273.82221f),
                new Vector2F(2147.8538f,-32.17651f),
                new Vector2F(2038.3398f,215.95519f),
                new Vector2F(1852.2859f,537.88159f),
                new Vector2F(1544.4495f,1000.9025f),
                new Vector2F(1114.8304f,1605.018f),
                new Vector2F(563.42839f,2350.2286f),
            };

            Shape testShape = new Shape(pList, new Vector2F(100, 400), 0.0325f);

            List<Vector2F> shapeTriList = new List<Vector2F>();
            testShape.Triangulate(shapeTriList);

            _rectangles = new Rectangle[50];
            _rectangleColors = new Color[_rectangles.Length];

            for (int i = 0; i < 50; i++)
            {
                _rectangles[i] = new Rectangle()
                {
                    X = Rng.Next(0, 1920),
                    Y = Rng.Next(0, 1080),
                    Width = Rng.Next(16, 129),
                    Height = Rng.Next(16, 129)
                };

                _rectangleColors[i] = new Color()
                {
                    R = (byte)Rng.Next(0, 255),
                    G = (byte)Rng.Next(0, 255),
                    B = (byte)Rng.Next(0, 255),
                    A = 40,
                };
            }

            SampleSpriteRenderComponent com = SpriteLayer.AddObjectWithComponent<SampleSpriteRenderComponent>();
            com.RenderCallback = (sb) =>
            {
                Vector2F rectOrigin = new Vector2F(0.5f);
                for (int i = 0; i < _rectangles.Length; i++)
                    sb.DrawRect(_rectangles[i], _rectangleColors[i], 0, rectOrigin);

                sb.DrawLine(new Vector2F(0), new Vector2F(400), Color.White, 1);
                sb.DrawLinePath(linePoints, colors, 2);
                sb.DrawLinePath(circleLinePoints, colors, 4);
                sb.DrawTriangle(new Vector2F(400, 220), new Vector2F(350, 320), new Vector2F(500, 260), Color.SkyBlue);
                sb.DrawTriangle(new Vector2F(500, 220), new Vector2F(590, 350), new Vector2F(650, 280), Color.Violet);
                sb.DrawTriangleList(triPoints, colors);
                sb.DrawTriangleList(shapeTriList, colors);

                // Draw circles with a decreasing number of sides.
                origin.X = 705;
                origin.Y = 500;
                int circleSides = 80;
                int circleRadius = 100;
                for (int i = 0; i < colors.Count; i++)
                {
                    //sb.DrawCircle(origin, circleRadius, colors[i], circleSides);
                    sb.DrawEllipse(_meshTex, origin, circleRadius, circleRadius, 0, 0, colors[i]);
                    circleSides /= 2;
                    origin.X += (circleRadius * 2) + 5;
                }

                // Draw ellipse outlines with a decreasing number of sides.
                origin.X = 705;
                origin.Y = 700;
                int ellipseSides = 90;
                for (int i = 0; i < colors.Count; i++)
                {
                    sb.DrawEllipseOutline(origin, circleRadius, circleRadius, colors[i], 2, ellipseSides);
                    ellipseSides /= 2;
                    origin.X += (circleRadius * 2) + 5;
                }
            };
        }
    }
}
