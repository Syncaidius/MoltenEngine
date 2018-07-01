using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public delegate void SceneRenderDataHandler(IRenderer renderer, SceneRenderData data);

    /// <summary>
    /// A class for storing renderer-specific information about a scene.
    /// </summary>
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

        public LightList PointLights = new LightList(100, 100);

        public LightList CapsuleLights = new LightList(50, 100);

        /// <summary>
        /// If true, the scene will be rendered.
        /// </summary>
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

        /// <summary>
        /// The ambient light color.
        /// </summary>
        public Color AmbientLightColor = Color.Black;

        public abstract void AddObject(IRenderable3D obj, ObjectRenderData renderData);

        public abstract void RemoveObject(IRenderable3D obj, ObjectRenderData renderData);

        public abstract void AddObject(IRenderable2D sprite);

        public abstract void RemoveObject(IRenderable2D sprite);

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

        /// <summary>
        /// Gets the debug overlay which displays information for the current scene.
        /// </summary>
        public abstract ISceneDebugOverlay DebugOverlay { get; }

        /* TODO:
        *  - Edit PointLights and CapsuleLights.Data directly in light scene components (e.g. PointLightComponent).
        *  - Renderer will upload the latest data to the GPU 
        */
    }
}
