using Molten.Data;
using Molten.Graphics;
using Molten.UI;

namespace Molten.Samples
{
    public class UIExample : SampleGame
    {
        public override string Description => "Demonstrates Molten's UI system.";
        public UIExample() : base("UI Example") { }

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

            ITexture2D tex = cr.Get<ITexture2D>(1);
            mat.SetDefaultResource(tex, 0);
            TestMesh.Material = mat;

            UIStateTheme pressTheme = Settings.UI.Theme.Value.DefaultElementTheme[UIElementState.Pressed];
            pressTheme.TextColor = Color.White;
            pressTheme.BackgroundColor = Color.SkyBlue * 0.5f;
            pressTheme.BorderColor = Color.SkyBlue;
            pressTheme.TextColor = Color.Yellow;

            UIWindow window1 = new UIWindow()
            {
                LocalBounds = new Rectangle(100, 150, 600, 440),
                Title = "This is a Window"
            };

            UIWindow window2 = new UIWindow()
            {
                LocalBounds = new Rectangle(500, 250, 640, 550),
                Title = "This is another Window"
            };

            UILineGraph lineGraph = new UILineGraph()
            {
                LocalBounds = new Rectangle(1150, 200, 600, 420)
            };

            PlotGraphData(lineGraph);

            UI.Children.Add(window1);
            UI.Children.Add(window2);
            UI.Children.Add(lineGraph);
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
