using Molten.Graphics;
using Molten.Input;
using Molten.Shapes;

namespace Molten.Examples;

[Example("Sprite Batching", "Demonstrates the use of a sprite batcher to easily render large amounts of sprites.")]
public class SpriteBatching : MoltenExample
{
    const int SPRITE_RECT_INCREMENT = 100;

    ContentLoadHandle _hShader;
    ContentLoadHandle _hTexMesh;
    ContentLoadHandle _hTexPrimitive;

    RectangleF[] _rects;
    RectStyle[] _rectStyles;
    float _rotAngle;
    int _numRects = 100;

    protected override void OnLoadContent(ContentLoadBatch loader)
    {
        base.OnLoadContent(loader);

        _hShader = loader.Load<Shader>("assets/BasicTexture.json");
        _hTexMesh = loader.Load<ITexture2D>("assets/logo_512_bc7.dds", parameters: new TextureParameters()
        {
            GenerateMipmaps = true,
        });
        _hTexPrimitive = loader.Load<ITexture2D>("assets/128.dds", parameters: new TextureParameters()
        {
            PartCount = 3,
        });

        loader.OnCompleted += Loader_OnCompleted;
    }

    private void RefreshRects()
    {
        int curCount = _rects != null ? _rects.Length : 0;

        // Setup sprite rectangles and styles.
        if (_rects == null)
        {
            _rects = new RectangleF[_numRects];
            _rectStyles = new RectStyle[_rects.Length];
        }
        else if (curCount < _numRects) // We need to expand arrays
        {
            Array.Resize(ref _rects, Math.Max(_rects.Length * 2, _numRects));
            Array.Resize(ref _rectStyles, _rects.Length);
        }
        else // We have more than required.
        {
            return;
        }

        for (int i = curCount; i < _rects.Length; i++)
        {
            _rects[i] = new RectangleF()
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

            _rectStyles[i] = new RectStyle()
            {
                FillColor = rCol,
                BorderColor = rOutlineCol,
                BorderThickness = new Thickness(Rng.Next(1, 7))
            };
        }
    }


    private void Loader_OnCompleted(ContentLoadBatch loader)
    {
        if (!_hShader.HasAsset())
        {
            Close();
            return;
        }

        Shader shader = _hShader.Get<Shader>();
        ITexture2D texMesh = _hTexMesh.Get<ITexture2D>();
        shader[ShaderBindType.Resource, 0] = texMesh;
        TestMesh.Shader = shader;

        // Create points for zig-zagging lines.
        List<Vector2F> linePoints = new List<Vector2F>();
        float y = 100;
        Vector2F lineOffset = new Vector2F(400, 700);
        for (int i = 0; i < 20; i++)
        {
            linePoints.Add(lineOffset + new Vector2F(i * 50, y));
            y = 100 - y;
        }

        // Create a second batch of lines for a circle outline
        List<Vector2F> circleLinePoints = new List<Vector2F>();
        float radius = 50;

        Vector2F center = new Vector2F(500, 900);
        int outlineSegments = 32;
        float angleIncrement = 360.0f / outlineSegments;
        float angle = 0;

        for (int i = 0; i <= outlineSegments; i++)
        {
            float rad = angle * MathHelper.Constants<float>.DegToRad;
            angle += angleIncrement;

            circleLinePoints.Add(new Vector2F()
            {
                X = center.X + MathF.Sin(rad) * radius,
                Y = center.Y + MathF.Cos(rad) * radius,
            });
        }

        // Add 5 colors. The last color will be used when we have more points than colors.
        EllipseStyle[] styles = new EllipseStyle[]
        {
            new EllipseStyle(Color.Orange, Color.Lavender, 3),
            new EllipseStyle(Color.SkyBlue, Color.Red, 5),
            new EllipseStyle(Color.Lime, Color.Yellow, 7),
            new EllipseStyle(Color.Blue, Color.Orange, 10),
            new EllipseStyle(Color.Red, Color.Lime, 15),
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

        RefreshRects();

        SampleSpriteRenderComponent com = SpriteLayer.AddObjectWithComponent<SampleSpriteRenderComponent>();
        com.RenderCallback = (sb) =>
        {
            int maxRects = Math.Min(_numRects, _rects.Length);
            for (int i = 0; i < maxRects; i++)
                sb.DrawRect(_rects[i], 0, Vector2F.Zero, ref _rectStyles[i]);

            sb.DrawLine(new Vector2F(0), new Vector2F(400), Color.Lime, 2, 1);
            sb.DrawLine(new Vector2F(400), new Vector2F(650, 250), Color.SkyBlue, 75, 1f);

            GridStyle gridStyle = new GridStyle()
            {
                CellColor = new Color(200, 100, 0, 150),
                LineColor = Color.Yellow,
                LineThickness = new Vector2F(3),
            };

            sb.DrawGrid(new RectangleF(1450, 400, 400, 400), new Vector2F(20, 20), _rotAngle, new Vector2F(0.5f), ref gridStyle);

            LineStyle linePathStyle = new LineStyle()
            {
                Color1 = Color.Red,
                Color2 = Color.Yellow,
                Thickness = 3
            };

            sb.DrawLinePath(linePoints, ref linePathStyle);
            sb.DrawLinePath(circleLinePoints, ref linePathStyle);

            /*sb.DrawTriangle(new Vector2F(400, 220), new Vector2F(350, 320), new Vector2F(500, 260), Color.SkyBlue);
            sb.DrawTriangle(new Vector2F(500, 220), new Vector2F(590, 350), new Vector2F(650, 280), Color.Violet);

            sb.DrawTriangleList(triPoints, colors);
            sb.DrawTriangleList(shapeTriList, colors);*/

            // Draw circles with a decreasing number of sides.
            center.X = 305;
            center.Y = 200;
            int pSize = 50;
            Ellipse el = new Ellipse(center, pSize, pSize * 0.8f);

            center.Y += (pSize * 3);
            Ellipse cl = new Ellipse(center, pSize);

            center.Y += (pSize * 3);
            RectangleF rect = new RectangleF(center.X - pSize, center.Y - pSize, pSize, pSize);
            RectangleF rectTextured = rect;
            rectTextured.Y += (pSize * 3);
            Vector2F rectOrigin = new Vector2F(0.5f);

            ITexture2D texPrimitives = _hTexPrimitive.Get<ITexture2D>();

            for (int i = 0; i < styles.Length; i++)
            {
                float angle = float.Tau * (0.15f * (i + 1));
                uint texArrayID = (uint)i % 3;
                cl.StartAngle = angle;
                el.EndAngle = angle;

                sb.DrawEllipse(ref cl, ref styles[i], _rotAngle);
                sb.DrawEllipse(ref el, ref styles[i], _rotAngle, texPrimitives, null, texArrayID);
                sb.DrawRect(rect, _rotAngle, rectOrigin, ref _rectStyles[i]);
                sb.Draw(rectTextured, _rotAngle, new Vector2F(0.5f), ref _rectStyles[i], texPrimitives, null, texArrayID);

                cl.Center.X += (pSize * 2) + 5;
                el.Center.X = cl.Center.X;
                rect.X = cl.Center.X - pSize;
                rectTextured.X = rect.X;
            }

            string strCounter = $"Rectangle Count: {_numRects}";
            string strInstructions = "[UP KEY] Increase -- [DOWN KEY] Decrease";

            Vector2F counterSize = Font.MeasureString(strCounter);
            Vector2F counterPos = new Vector2F()
            {
                X = (SceneCamera.Surface.Width / 2) - (counterSize.X / 2),
                Y = SceneCamera.Surface.Height - 120,
            };

            sb.DrawString(Font, strCounter, counterPos, Color.White);
            counterPos.Y += counterSize.Y + 5;
            counterPos.X = (SceneCamera.Surface.Width / 2) - (Font.MeasureString(strInstructions).X / 2);
            sb.DrawString(Font, strInstructions, counterPos, Color.White);
        };
    }

    protected override void OnUpdate(Timing time)
    {
        base.OnUpdate(time);

        _rotAngle += 0.01f * time.Delta;

        if (Keyboard.IsDown(KeyCode.Up))
        {
            _numRects += SPRITE_RECT_INCREMENT;
            RefreshRects();
        }
        else if (Keyboard.IsDown(KeyCode.Down))
        {
            _numRects = Math.Max(0, _numRects - SPRITE_RECT_INCREMENT);
            RefreshRects();
        }
    }
}
