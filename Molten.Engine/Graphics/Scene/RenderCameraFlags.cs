namespace Molten.Graphics
{
    public enum RenderCameraFlags
    {
        /// <summary>
        /// The current camera does not have any special flags.
        /// </summary>
        None = 0,

        /// <summary>
        /// Lighting is skipped. This generally results in the scene being fully-lit (also known as full-bright). 
        /// This is useful for scenes displayed in smaller render surfaces (e.g. in-game CCTV) or scene previews.
        /// </summary>
        SkipLighting = 1 << 0,

        /// <summary>
        /// Shadow-casting is skipped. This is useful for scenes displayed in smaller render surfaces (e.g. in-game CCTV) or scene previews.
        /// </summary>
        SkipShadows = 1 << 1,

        /// <summary>
        /// Global-illumination is skipped. This is useful for scenes displayed in smaller render surfaces (e.g. in-game CCTV) or scene previews.
        /// </summary>
        SkipGlobalIllumination = 1 << 2,

        /// <summary>
        /// Post-processing is skipped. This is useful for scenes displayed in smaller render surfaces (e.g. in-game CCTV) or scene previews.
        /// </summary>
        SkipPostProcessing = 1 << 3,

        /// <summary>
        /// The culling stage should be skipped for the current camera. This is useful if everything you intend to render is already in view of the camera.
        /// </summary>
        SkipCulling = 1 << 4,

        /// <summary>
        /// When rendering a scene with the current camera, the destination surface will not be cleared before outputting the result.
        /// </summary>
        DoNotClear = 1 << 5,

        /// <summary>
        /// Instructs the renderer not to clear it's depth buffer before rendering the current scene over previous scenes, which may share the same output surface.
        /// By default, the depth buffer will be cleared every time a scene needs to be rendered to avoid depth conflicts between different scenes.
        /// </summary>
        DoNotClearDepth = 1 << 6,

        /// <summary>
        /// [TEMPORARY] Instructs the renderer to use deferred rendering when when rendering with the current camera.
        /// </summary>
        Deferred = 1 << 7,

        /// <summary>
        /// Tells the renderer to draw the overlay on top of the rendered scene.
        /// </summary>
        ShowOverlay = 1 << 8,
    }
}
