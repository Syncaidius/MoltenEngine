using Molten.Graphics;

namespace Molten.Examples;

/// <summary>
/// A helper component which provides a render callback for custom rendering in samples.
/// </summary>
public class SampleSpriteRenderComponent : SpriteRenderComponent
{
    public SampleSpriteRenderComponent() { }

    protected override void OnDispose(bool immediate) { }

    protected override void OnRender(SpriteBatcher sb)
    {
        RenderCallback?.Invoke(sb);
    }

    public Action<SpriteBatcher> RenderCallback { get; set; }
}
