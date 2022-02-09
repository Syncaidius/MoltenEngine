using System;

namespace Molten.Graphics
{
    public interface ISpriteRenderer : IRenderable
    {
        Action<SpriteBatcher> Callback { get; set; }
    }
}
