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

            UIStateTheme pressTheme = Settings.UI.Theme.Value.DefaultElementTheme[UIElementState.Pressed];
            pressTheme.TextColor = Color.White;
            pressTheme.BackgroundColor = Color.SkyBlue * 0.5f;
            pressTheme.BorderColor = Color.SkyBlue;
            pressTheme.TextColor = Color.Yellow;

            _ui.Root = new UIWindow()
            {
                LocalBounds = new Rectangle(100, 150, 600, 450)
            };
        }

        protected override void OnHudDraw(SpriteBatcher sb)
        {
            if (SampleFont == null)
                return;

            base.OnHudDraw(sb);

            string text = $"Hovered UI Element: {(_ui.HoverElement != null ? _ui.HoverElement.Name : "None")}";
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
