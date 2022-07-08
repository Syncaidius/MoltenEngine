using Molten.Graphics;

namespace Molten.Samples
{
    public class SpriteBatchArraySample : SampleGame
    {
        public override string Description => "An example of using a SpriteRenderComponent to draw sprites with a texture array.";

        List<Sprite> _sprites;

        public SpriteBatchArraySample() : base("Sprite Batch Texture Array") { }

        protected override void OnInitialize(Engine engine)
        {
            base.OnInitialize(engine);

            ContentRequest cr = engine.Content.BeginRequest("assets/");
            cr.Load<IMaterial>("BasicTexture.mfx");
            cr.Load<ITexture2D>("png_test.png");
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

            ITexture2D tex = cr.Get<ITexture2D>("png_test.png");
            mat.SetDefaultResource(tex, 0);
            TestMesh.Material = mat;

            ITexture2D texSprites = cr.Get<ITexture2D>("128.dds");
            SetupSprites(texSprites);
        }

        private void SetupSprites(ITexture2D tex)
        {
            _sprites = new List<Sprite>();

            for(int i = 0; i < 300; i++)
            {
                Sprite spr = new Sprite()
                {
                    Position = new Vector2F()
                    {
                        X = Rng.Next(0, 1920),
                        Y = Rng.Next(0, 1080),
                    },

                    Data = new SpriteData()
                    {
                        Texture = tex,
                        Source = new Rectangle(0, 0, 128, 128),
                        ArraySlice = Rng.Next(0, 3),
                    },
                    Scale = new Vector2F(Rng.Next(25, 101), Rng.Next(25, 101)) / 100,
                    Origin = new Vector2F(0.5f),
                };

                spr.Data.Style = new Color()
                {
                    R = (byte)Rng.Next(0, 255),
                    G = (byte)Rng.Next(0, 255),
                    B = (byte)Rng.Next(0, 255),
                    A = 255,
                };

                _sprites.Add(spr);
            }

            SampleSpriteRenderComponent com = SpriteLayer.AddObjectWithComponent<SampleSpriteRenderComponent>();
            com.RenderCallback = (sb) =>
            {
                for (int i = 0; i < _sprites.Count; i++)
                    sb.Draw(_sprites[i]);
            };
        }
    }
}
