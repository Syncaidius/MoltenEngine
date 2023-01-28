namespace Molten.Graphics
{
    public interface IRenderable
    {
        void Render(GraphicsCommandQueue cmd, RenderService renderer, RenderCamera camera, ObjectRenderData data);

        bool IsVisible { get; set; }
    }
}
