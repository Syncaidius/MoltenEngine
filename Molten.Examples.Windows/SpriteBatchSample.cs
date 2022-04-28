using Molten.Graphics;

namespace Molten.Samples
{
    public class SpriteBatchSample : SampleSceneGame
    {
        public override string Description => "An example of using a SpriteRenderComponent to draw sprites.";

        List<Sprite> _sprites;

        public SpriteBatchSample() : base("Sprite Batch") { }

        protected override void OnInitialize(Engine engine)
        {
            base.OnInitialize(engine);

            ContentRequest cr = engine.Content.BeginRequest("assets/");
            cr.Load<IMaterial>("BasicTexture.mfx");
            cr.Load<ITexture2D>("dds_test.dds", new TextureParameters()
            {
                GenerateMipmaps = true
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
            SetupSprites(tex);
            SetupSprites(null);
        }

        private void SetupSprites(ITexture2D tex)
        {
            _sprites = new List<Sprite>();
            SpriteData sData = new SpriteData()
            {
                Texture = tex,
                Source = new Rectangle(0, 0, 128, 128),
            };

            for (int i = 0; i < 300; i++)
            {
                _sprites.Add(new Sprite()
                {
                    Position = new Vector2F()
                    {
                        X = Rng.Next(0, 1920),
                        Y = Rng.Next(0, 1080),
                    },

                    Color = new Color()
                    {
                        R = (byte)Rng.Next(0, 255),
                        G = (byte)Rng.Next(0, 255),
                        B = (byte)Rng.Next(0, 255),
                        A = (byte)Rng.Next(50, 100),
                    },
                    
                    Data = sData,
                    Scale = new Vector2F(Rng.Next(25, 101), Rng.Next(25, 101)) / 100,
                    Origin = new Vector2F(0.5f),
                });
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
