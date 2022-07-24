using Molten.Graphics;

namespace Molten.Samples
{
    public class SpriteBatchArraySample : SampleGame
    {
        public override string Description => "An example of using a SpriteRenderComponent to draw sprites with a texture array.";

        List<Sprite> _sprites; 
        ContentLoadHandle<IMaterial> _hMaterial;
        ContentLoadHandle<ITexture2D> _hTexture;
        ContentLoadHandle<ITexture2D> _hSpriteTexture;

        public SpriteBatchArraySample() : base("Sprite Batch Texture Array") { }

        protected override void OnLoadContent(ContentLoadBatch loader)
        {
            _hMaterial = loader.Load<IMaterial>("assets/BasicTexture.mfx");
            _hTexture = loader.Load<ITexture2D>("assets/png_test.png");
            _hSpriteTexture = loader.Load<ITexture2D>("assets/128.dds", parameters: new TextureParameters()
            {
                ArraySize = 3,
            });
            loader.OnCompleted += Loader_OnCompleted;
        }

        private void Loader_OnCompleted(ContentLoadBatch loader)
        {
            if (_hMaterial.HasAsset())
            {
                Exit();
                return;
            }

            IMaterial mat = _hMaterial.Get();
            ITexture2D tex = _hTexture.Get();
            mat.SetDefaultResource(tex, 0);
            TestMesh.Material = mat;

            ITexture2D texSprites = _hSpriteTexture.Get();
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
