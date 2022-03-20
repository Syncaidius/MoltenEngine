using System.Runtime.Serialization;

namespace Molten.UI
{
    public class UIRenderData
    {
        internal UIRenderData() { }

        internal List<UIRenderData> Children { get; } = new List<UIRenderData>();

        /// <summary>
        /// Global position of the UI component, where 0,0 is it's origin.
        /// </summary>
        public Rectangle GlobalBounds;

        [DataMember]
        public Rectangle LocalBounds;

        public Rectangle RenderBounds;

        public Rectangle BorderBounds;

        [DataMember]
        public UISpacing Margin = new UISpacing();

        [DataMember]
        public UISpacing Padding = new UISpacing();

        [DataMember]
        public UIAnchorFlags Anchor;

        [DataMember]
        public bool IsClipEnabled = true;
    }
}
