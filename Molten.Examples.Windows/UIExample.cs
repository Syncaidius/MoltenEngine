using Molten.Graphics;
using Molten.UI;

namespace Molten.Samples
{
    public class UIExample : SampleSceneGame
    {
        public override string Description => "Demonstrates Molten's UI system.";

        UIManagerComponent _ui;

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

            _ui = SpriteLayer.AddObjectWithComponent<UIManagerComponent>();
            Settings.UI.Theme.Value.PressColors = new UITheme.StateTheme()
            {
                Text = Color.White,
                Background = Color.SkyBlue * 0.5f,
                Border = Color.SkyBlue,
            };
            _ui.Root = new UIPanel()
            {
                LocalBounds = new Rectangle(100, 150, 600, 450),
            };

            UIPanel childPanel = new UIPanel()
            {
                LocalBounds = new Rectangle(100, 10, 220, 200),
                BackgroundColor = new Color(0, 128, 0, 200),
                BorderColor = Color.LimeGreen
            };

            UIText label = new UIText()
            {
                LocalBounds = new Rectangle(0, 10, _ui.Root.LocalBounds.Width, 20),
                HorizontalAlign = UIHorizonalAlignment.Center
            };

            UIButton button = new UIButton()
            {
                LocalBounds = new Rectangle(10, 250, 150, 20),
                Text = "Click Me!"
            };

            _ui.Root.Children.Add(childPanel);
            _ui.Root.Children.Add(label);
            _ui.Root.Children.Add(button);
        }

        protected override void OnHudDraw(SpriteBatcher sb)
        {
            if (SampleFont == null)
                return;

            base.OnHudDraw(sb);

            string text = $"Hovered UI Element: {(_ui.HoverElement != null ? _ui.HoverElement.Name : "None")}";
            Vector2F tSize = SampleFont.MeasureString(text, 16);
            Vector2F pos = new Vector2F()
            {
                X = Window.Width / 2 + (-tSize.X / 2),
                Y = 25,
            };

            sb.DrawString(SampleFont, text, pos, Color.White);
        }
    }
}
