namespace Molten.Graphics
{
    public class SpriteRendererDX11 : Renderable, ISpriteRenderer
    {
        internal SpriteRendererDX11(DeviceDX11 device, Action<SpriteBatcher> callback) : base(device)
        {
            Callback = callback;
        }

        public Action<SpriteBatcher> Callback { get; set; }

        private protected override void OnRender(CommandQueueDX11 pipe, RendererDX11 renderer, RenderCamera camera, ObjectRenderData data)
        {
            Callback?.Invoke(renderer.SpriteBatcher);
            renderer.SpriteBatcher.Flush(pipe, camera, data);
        }
    }
}
