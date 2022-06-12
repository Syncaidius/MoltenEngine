using Molten.Graphics;

namespace Molten.UI
{
    public interface IUIRenderData
    {
        void Render(SpriteBatcher sb, UIRenderData data);

        void ApplyTheme(UITheme theme, UIElementTheme eTheme, UIStateTheme stateTheme);
    }
}
