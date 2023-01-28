using Molten.Graphics;

namespace Molten.Examples
{
    [Example("Texture Arrays - SpriteBatch", "Combines the use of texture arrays with SpriteBatcher")]
    public class SpriteBatchArray : MoltenExample
    {
        List<Sprite> _sprites;
        ContentLoadHandle _hMaterial;
        ContentLoadHandle _hTexture;
        ContentLoadHandle _hSpriteTexture;

        protected override void OnLoadContent(ContentLoadBatch loader)
        {
            base.OnLoadContent(loader);

            _hMaterial = loader.Load<Material>("assets/BasicTexture.mfx");
            _hTexture = loader.Load<ITexture2D>("assets/png_test.png");
            _hSpriteTexture = loader.Load<ITexture2D>("assets/128.dds", parameters: new TextureParameters()
            {
                PartCount = 3,
            });
            loader.OnCompleted += Loader_OnCompleted;
        }

        private void Loader_OnCompleted(ContentLoadBatch loader)
        {
            if (!_hMaterial.HasAsset())
            {
                Close();
                return;
            }

            Material mat = _hMaterial.Get<Material>();
            ITexture2D tex = _hTexture.Get<ITexture2D>();
            mat.SetDefaultResource(tex, 0);
            TestMesh.Material = mat;

            ITexture2D texSprites = _hSpriteTexture.Get<ITexture2D>();
            SetupSprites(texSprites);
        }

        private void SetupSprites(ITexture2D tex)
        {
            _sprites = new List<Sprite>();

            for (int i = 0; i < 300; i++)
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
                        Source = new RectangleF(0, 0, 128, 128),
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
