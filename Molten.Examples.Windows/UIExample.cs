using Molten.Data;
using Molten.Graphics;
using Molten.UI;

namespace Molten.Samples
{
    public class UIExample : SampleGame
    {
        ContentLoadHandle _hMaterial;
        ContentLoadHandle _hTexture;
        UIWindow _window1;
        UIWindow _window2;
        UILineGraph _lineGraph;

        public override string Description => "Demonstrates Molten's UI system.";
        public UIExample() : base("UI Example") { }

        protected override void OnLoadContent(ContentLoadBatch loader)
        {
            _hMaterial = loader.Load<IMaterial>("assets/BasicTexture.mfx");
            _hTexture = loader.Load<ITexture2D>("assets/dds_test.dds", parameters: new TextureParameters()
            {
                GenerateMipmaps = true,
            });

            loader.Deserialize<UITheme>("assets/test_theme.json",(theme, isReload) =>
            {
                if (_window2 != null)
                    _window2.Theme = theme;
            });

            loader.OnCompleted += Loader_OnCompleted;
        }

        private void Loader_OnCompleted(ContentLoadBatch loader)
        {
            if (!_hMaterial.HasAsset())
            {
                Exit();
                return;
            }

            IMaterial mat = _hMaterial.Get<IMaterial>();
            ITexture2D texture = _hTexture.Get<ITexture2D>();
            
            mat.SetDefaultResource(texture, 0);
            TestMesh.Material = mat;

            _window1 = new UIWindow()
            {
                LocalBounds = new Rectangle(100, 150, 600, 440),
                Title = "This is a Window"
            };

            _window2 = new UIWindow()
            {
                LocalBounds = new Rectangle(460, 250, 540, 550),
                Title = "This is another Window"
            };

            _lineGraph = new UILineGraph()
            {
                LocalBounds = new Rectangle(1050, 200, 700, 420)
            };

            /*UITheme test = new UITheme();
            test.AddStyle("Molten.UI.UIWindow/Molten.UI.UIButton/Molten.UI.UILabel");
            test.ApplyStyle(lineGraph);*/

            PlotGraphData(_lineGraph);

            UI.Children.Add(_window1);
            UI.Children.Add(_window2);
            UI.Children.Add(_lineGraph);
        }

        private void PlotGraphData(UILineGraph graph)
        {
            GraphDataSet graphSet = new GraphDataSet(200);
            graphSet.KeyColor = Color.Grey;
            for (int i = 0; i < graphSet.Capacity; i++)
                graphSet.Plot(Rng.Next(0, 500));

            GraphDataSet graphSet2 = new GraphDataSet(200);
            graphSet2.KeyColor = Color.Lime;
            float piInc = MathHelper.TwoPi / 20;
            float waveScale = 100;
            for (int i = 0; i < graphSet2.Capacity; i++)
                graphSet2.Plot(waveScale * Math.Sin(piInc * i));

            graph.AddDataSet(graphSet);
            graph.AddDataSet(graphSet2);
        }

        protected override void OnDrawSprites(SpriteBatcher sb)
        {
            if (SampleFont == null)
                return;

            base.OnDrawSprites(sb);

            string text = $"Hovered UI Element: {(UI.HoverElement != null ? UI.HoverElement.Name : "None")}";
            Vector2F tSize = SampleFont.MeasureString(text);
            Vector2F pos = new Vector2F()
            {
                X = (Window.Width / 2) - (tSize.X / 2),
                Y = 25,
            };

            sb.DrawString(SampleFont, text, pos, Color.White);
        }
    }
}
