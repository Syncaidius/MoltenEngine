using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Samples
{
    public class SpriteCacheTestObject : ISprite
    {
        SpriteBatchCache _cache;

        public SpriteCacheTestObject(SampleGame game, ITexture2D tex)
        {
            _cache = new SpriteBatchCache();
            FillCache(game, tex);
        }

        public void Render(ISpriteBatch batch)
        {
            batch.Draw(_cache);
        }

        /// <summary>Deliberately generate mixed up sprites to stress sprite-batch.</summary>
        /// <param name="tex"></param>
        private void FillCache(SampleGame game, ITexture2D tex)
        {
            Random rng = game.Rng;
            Rectangle texSource = new Rectangle(0, 0, 128, 128);
            Vector2 origin = new Vector2(0.5f);

            // Textured sprites
            for (int i = 0; i < 10000; i++)
            { 
                Color col = new Color()
                {
                    R = (byte)rng.Next(0, 255),
                    G = (byte)rng.Next(0, 255),
                    B = (byte)rng.Next(0, 255),
                    A = 40,
                };

                    Vector2 pos = new Vector2()
                    {
                        X = rng.Next(0, 1920),
                        Y = rng.Next(0, 1080),
                    };
                    _cache.Draw(tex, pos, texSource, col, 0, Vector2.One, origin);
            }

            // Colored sprites
            for (int i = 0; i < 100000; i++)
            {
                Color col = new Color()
                {
                    R = (byte)rng.Next(0, 255),
                    G = (byte)rng.Next(0, 255),
                    B = (byte)rng.Next(0, 255),
                    A = 40,
                };

                Rectangle dest = new Rectangle()
                {
                    X = rng.Next(0, 1920),
                    Y = rng.Next(0, 1080),
                    Width = rng.Next(16, 129),
                    Height = rng.Next(16, 129)
                };

                _cache.Draw(dest, col, 0, origin);
            }
        }
    }
}
