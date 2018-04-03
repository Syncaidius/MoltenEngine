using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public delegate void SceneRenderDataHandler(IRenderer renderer, SceneRenderData data);

    public abstract class SceneRenderData : EngineObject
    {
        /// <summary>
        /// Occurs just before the scene is about to be rendered.
        /// </summary>
        public event SceneRenderDataHandler OnPreRender;

        /// <summary>
        /// Occurs just after the scene has been rendered.
        /// </summary>
        public event SceneRenderDataHandler OnPostRender;

        public bool IsVisible = true;

        /// <summary>The camera that should be used as a view or eye when rendering 3D objects in a scene.</summary>
        public ICamera Camera;

        /// <summary>
        /// Flags which describe basic rules for rendering the scene.
        /// </summary>
        public SceneRenderFlags Flags = SceneRenderFlags.Render2D | SceneRenderFlags.Render3D;

        /// <summary>
        /// The background color of the scene.
        /// </summary>
        public Color BackgroundColor = new Color(20,20,20,255);

        public abstract void AddObject(IRenderable obj, ObjectRenderData renderData);

        public abstract void RemoveObject(IRenderable obj, ObjectRenderData renderData);

        public abstract void AddSprite(ISprite sprite);

        public abstract void RemoveSprite(ISprite sprite);

        /// <summary>
        /// Returns true if the current <see cref="SceneRenderData"/> has the specified flag(s).
        /// </summary>
        /// <param name="flags">The flags to check.</param>
        /// <returns></returns>
        public bool HasFlag(SceneRenderFlags flags)
        {
            return (Flags & flags) == flags;
        }

        /// <summary>
        /// Invokes <see cref="OnPreRender"/> event.
        /// </summary>
        protected void PreRenderInvoke(IRenderer renderer) => OnPreRender?.Invoke(renderer, this);

        /// <summary>
        /// Invokes <see cref="OnPostRender"/> event.
        /// </summary>
        protected void PostRenderInvoke(IRenderer renderer) => OnPostRender?.Invoke(renderer, this);
    }
}
