using Molten.Graphics;
using Molten.UI;

namespace Molten.Samples
{
    public class UIExample : SampleSceneGame
    {
        public override string Description => "Demonstrates Molten's UI system.";

        SceneObject _parent;
        SceneObject _child;
        IMesh<VertexTexture> _mesh;
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

            _mesh = Engine.Renderer.Resources.CreateMesh<VertexTexture>(36);
            _mesh.SetVertices(SampleVertexData.TexturedCube);

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

            _ui = SpriteLayer.AddObjectWithComponent<UIManagerComponent>();
            _ui.Root = new UIPanel()
            {
                LocalBounds = new Rectangle(100, 150, 600, 450),
            };

            UIPanel childPanel = new UIPanel()
            {
                LocalBounds = new Rectangle(100, 50, 220, 200),
                Parent = _ui.Root,
                Properties = new UIPanelData()
                {
                    BackgroundColor = new Color(0,128, 0, 200),
                    BorderColor = Color.LimeGreen
                },
            };

            UILabel label = new UILabel()
            {
                LocalBounds = new Rectangle(300, 150, 200, 20),
                Parent = _ui.Root
            };
        }

        protected override void OnUpdate(Timing time)
        {
            RotateParentChild(_parent, _child, time);

            base.OnUpdate(time);
        }

        protected override void OnHudDraw(SpriteBatcher sb)
        {
            base.OnHudDraw(sb);

            string text = $"Hovered UI Element: {(_ui.HoverElement != null ? _ui.HoverElement.Name : "None")}";
            Vector2F tSize = SampleFont.MeasureString(text);
            Vector2F pos = new Vector2F()
            {
                X = Window.Width / 2 + (-tSize.X / 2),
                Y = 25,
            };

            sb.DrawString(SampleFont, text, pos, Color.White);
        }
    }
}
