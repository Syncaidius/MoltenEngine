using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Samples
{
    public class UnfairSpriteCacheTestObject : IRenderable2D, ISceneObject
    {
        SpriteBatchCache _cache;

        Scene ISceneObject.Scene { get; set; }

        public UnfairSpriteCacheTestObject(SampleGame game, ITexture2D tex)
        {
            _cache = new SpriteBatchCache();
            FillCache(game, tex);
        }

        public void Render(SpriteBatch batch)
        {
            batch.Draw(_cache);
        }

        /// <summary>Deliberately generate mixed up sprites to stress sprite-batch.</summary>
        /// <param name="tex"></param>
        private void FillCache(SampleGame game, ITexture2D tex)
        {
            Random rng = game.Rng;
            Rectangle texSource = new Rectangle(0, 0, 128, 128);
            Vector2F origin = new Vector2F(0.5f);

            for (int i = 0; i < 50000; i++)
            {
                IRenderable2D s;
                bool useTextured = rng.Next(0, 5001) < 2500;
                Color col = new Color()
                {
                    R = (byte)rng.Next(0, 255),
                    G = (byte)rng.Next(0, 255),
                    B = (byte)rng.Next(0, 255),
                    A = (byte)rng.Next(0, 255),
                };


                if (useTextured)
                {
                    Vector2F pos = new Vector2F()
                    {
                        X = rng.Next(0, 1920),
                        Y = rng.Next(0, 1080),
                    };
                    _cache.Draw(tex, pos, texSource, col, 0, Vector2F.One, origin);
                }
                else
                {
                    Rectangle dest = new Rectangle()
                    {
                        X = rng.Next(0, 1920),
                        Y = rng.Next(0, 1080),
                        Width = rng.Next(16, 129),
                        Height = rng.Next(16, 129)
                    };

                    _cache.DrawRect(dest, col, 0, origin);
                }
            }
        }
    }
}
