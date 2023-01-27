namespace Molten.Graphics
{
    public class SpriteRenderer : Renderable
    {
        internal SpriteRenderer(GraphicsDevice device, Action<SpriteBatcher> callback) : base(device)
        {
            Callback = callback;
        }

        public Action<SpriteBatcher> Callback { get; set; }

        private protected override void OnRender(GraphicsCommandQueue cmd, RenderService renderer, RenderCamera camera, ObjectRenderData data)
        {
            Callback?.Invoke(renderer.SpriteBatch);
            renderer.SpriteBatch.Flush(cmd, camera, data);
        }
    }
}
