namespace Molten.Graphics
{
    public class SpriteRenderer : Renderable
    {
        internal SpriteRenderer(RenderService renderer, Action<SpriteBatcher> callback) : base(renderer)
        {
            Callback = callback;
        }

        public Action<SpriteBatcher> Callback { get; set; }

        protected override void OnRender(GraphicsCommandQueue cmd, RenderService renderer, RenderCamera camera, ObjectRenderData data)
        {
            Callback?.Invoke(renderer.SpriteBatch);
            renderer.SpriteBatch.Flush(cmd, camera, data);
        }
    }
}
