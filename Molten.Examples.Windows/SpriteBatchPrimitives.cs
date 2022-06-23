using Molten.Graphics;

namespace Molten.Samples
{
    public class SpriteBatchPrimitives : SampleSceneGame
    {
        const int BACKGROUND_RECT_COUNT = 1000;
        const float BACKGROUND_OUTLINE_THICKNESS = 2;

        public override string Description => "Draws various primitives using sprite batch.";

        Rectangle[] _rects;
        SpriteStyle[] _rectStyles;
        ITexture2D _texMesh;
        ITexture2D _texPrimitives;
        float _rotAngle;

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
            cr.Load<ITexture2D>("128.dds", new TextureParameters()
            {
                ArraySize = 3,
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

            _texMesh = cr.Get<ITexture2D>(1);
            _texPrimitives = cr.Get<ITexture2D>("128.dds");
            mat.SetDefaultResource(_texMesh, 0);
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

            Vector2F center = new Vector2F(500);
            int outlineSegments = 32;
            float angleIncrement = 360.0f / outlineSegments;
            float angle = 0;

            for (int i = 0; i <= outlineSegments; i++)
            {
                float rad = angle * MathHelper.DegToRad;
                angle += angleIncrement;

                circleLinePoints.Add(new Vector2F()
                {
                    X = center.X + (float)Math.Sin(rad) * radius,
                    Y = center.Y + (float)Math.Cos(rad) * radius,
                });
            }

            // Add 5 colors. The last color will be used when we have more points than colors.
            SpriteStyle[] styles = new SpriteStyle[]
            {
                new SpriteStyle(Color.Orange, Color.Lavender, 3),
                new SpriteStyle(Color.SkyBlue, Color.Red, 5),
                new SpriteStyle(Color.Lime, Color.Yellow, 7),
                new SpriteStyle(Color.Blue, Color.Orange, 10),
                new SpriteStyle(Color.Red, Color.Lime, 15),
            };

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

            // Setup sprite rectangles and styles.
            _rects = new Rectangle[BACKGROUND_RECT_COUNT];
            _rectStyles = new SpriteStyle[_rects.Length];

            for (int i = 0; i < _rects.Length; i++)
            {
                _rects[i] = new Rectangle()
                {
                    X = Rng.Next(0, 1920),
                    Y = Rng.Next(0, 1080),
                    Width = Rng.Next(16, 129),
                    Height = Rng.Next(16, 129)
                };


                Color rCol = new Color()
                {
                    R = (byte)Rng.Next(10, 255),
                    G = (byte)Rng.Next(10, 255),
                    B = (byte)Rng.Next(10, 255),
                    A = 65,
                };
                Color rOutlineCol = rCol * 1.5f;
                rOutlineCol.A = rCol.A;

                _rectStyles[i] = new SpriteStyle()
                {
                    Color = rCol,
                    Color2 = rOutlineCol,
                    Thickness = Rng.Next(0, 6)
                };
            }

            SampleSpriteRenderComponent com = SpriteLayer.AddObjectWithComponent<SampleSpriteRenderComponent>();
            com.RenderCallback = (sb) =>
            {
                for (int i = 0; i < _rects.Length; i++)
                    sb.DrawRect(_rects[i], ref _rectStyles[i], 0, Vector2F.Zero);

                sb.DrawLine(new Vector2F(0), new Vector2F(400), Color.Red, 2);
                sb.DrawLine(new Vector2F(400), new Vector2F(650, 250), Color.Red, Color.Yellow, 2);

                SpriteStyle gridStyle = new SpriteStyle()
                {
                    Color = new Color(200,100,0,150),
                    Color2 = Color.Yellow,
                    Thickness = 3,
                };

                sb.DrawGrid(new Rectangle(1450, 400, 400, 400), ref gridStyle, new Vector2F(20, 20), _rotAngle, new Vector2F(0.5f));

                /*sb.DrawLinePath(linePoints, colors, 2);
                sb.DrawLinePath(circleLinePoints, colors, 4);

                sb.DrawTriangle(new Vector2F(400, 220), new Vector2F(350, 320), new Vector2F(500, 260), Color.SkyBlue);
                sb.DrawTriangle(new Vector2F(500, 220), new Vector2F(590, 350), new Vector2F(650, 280), Color.Violet);

                sb.DrawTriangleList(triPoints, colors);
                sb.DrawTriangleList(shapeTriList, colors);*/

                // Draw circles with a decreasing number of sides.
                center.X = 305;
                center.Y = 200;
                int pSize = 50;
                Ellipse el = new Ellipse(center, pSize, pSize * 0.8f);

                center.Y += (pSize * 3);
                Circle cl = new Circle(center, pSize);

                center.Y += (pSize * 3);
                RectangleF rect = new RectangleF(center.X - pSize, center.Y - pSize, pSize, pSize);
                RectangleF rectTextured = rect;
                rectTextured.Y += (pSize * 3);

                for (int i = 0; i < styles.Length; i++)
                {
                    float angle = MathHelper.TwoPi * (0.15f * (i + 1));
                    uint texArrayID = (uint)i % 3;
                    cl.StartAngle = angle;
                    el.EndAngle = angle;

                    sb.DrawCircle(ref cl, ref styles[i], _rotAngle);
                    sb.DrawEllipse(ref el, ref styles[i], _rotAngle, _texPrimitives, null, texArrayID);
                    sb.DrawRect(rect, ref styles[i], _rotAngle, new Vector2F(0.5f));
                    sb.Draw(rectTextured, ref styles[i], _rotAngle, new Vector2F(0.5f), _texPrimitives, null, texArrayID);

                    cl.Center.X += (pSize * 2) + 5;
                    el.Center.X = cl.Center.X;
                    rect.X = cl.Center.X - pSize;
                    rectTextured.X = rect.X;
                }
            };
        }

        protected override void OnUpdate(Timing time)
        {
            base.OnUpdate(time);

            _rotAngle += 0.03f * time.Delta;
        }
    }
}
