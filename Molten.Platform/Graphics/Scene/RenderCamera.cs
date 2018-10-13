using Molten.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public delegate void RenderCameraProjectionFunc(IRenderSurface surface, float nearClip, float farClip, float fov, ref Matrix4F projection);

    public class RenderCamera
    {
        class ClipRange
        {
            public float Near;
            public float Far;

            public ClipRange(float near, float far)
            {
                Near = near;
                Far = far;
            }
        }

        static Dictionary<RenderCameraMode, RenderCameraProjectionFunc> _projectionFuncs;
        static Dictionary<RenderCameraMode, ClipRange> _clipPreset;

        Matrix4F _view;
        Matrix4F _projection;
        Matrix4F _viewProjection;
        Matrix4F _invViewProjection;
        Matrix4F _transform;
        IRenderSurface _surface;
        float _nearClip = 0.1f;
        float _farClip = 1000f;
        float _fov;
        RenderCameraMode _mode;
        RenderCameraProjectionFunc _projFunc;

        static RenderCamera()
        {
            _projectionFuncs = new Dictionary<RenderCameraMode, RenderCameraProjectionFunc>();
            _projectionFuncs[RenderCameraMode.Perspective] = CalcPerspectiveProjection;
            _projectionFuncs[RenderCameraMode.Orthographic] = CalcOrthographicProjection;

            _clipPreset = new Dictionary<RenderCameraMode, ClipRange>();
            _clipPreset[RenderCameraMode.Perspective] = new ClipRange(0.1f, 1000f);
            _clipPreset[RenderCameraMode.Orthographic] = new ClipRange(0.0f, 1.0f);
        }

        /// <summary>
        /// Creates a new instance of <see cref="RenderCamera"/> with the specified projection calculation preset.
        /// </summary>
        /// <param name="mode">The projection calculation preset to be used upon instantiation.</param>
        public RenderCamera(RenderCameraMode mode)
        {
            View = Matrix4F.Identity;
            ClipRange clip = _clipPreset[mode];
            _nearClip = clip.Near;
            _farClip = clip.Far;
            _fov = (float)Math.PI / 4.0f;
            _projFunc = _projectionFuncs[mode];
            _projection = Matrix4F.Identity;
        }

        private static void CalcOrthographicProjection(IRenderSurface surface, float nearClip, float farClip, float fov, ref Matrix4F projection)
        {
            int width = 10;
            int height = 10;
            if (surface != null)
            {
                width = surface.Width;
                height = surface.Height;
            }

            projection = Matrix4F.OrthoOffCenterLH(0, width, -height, 0, nearClip, farClip);
        }

        private static void CalcPerspectiveProjection(IRenderSurface surface, float nearClip, float farClip, float fov, ref Matrix4F projection)
        {
            int width = 10;
            int height = 10;
            if (surface != null)
            {
                width = surface.Width;
                height = surface.Height;
            }

            projection = Matrix4F.PerspectiveFovLH(fov, (float)width / height, nearClip, farClip);
        }

        /// <summary>
        /// Returns whether or not the current <see cref="Rendercamera"/> has the specified <see cref="RenderCameraFlags"/>.
        /// </summary>
        /// <param name="flags">The flags.</param>
        /// <returns></returns>
        public bool HasFlags(RenderCameraFlags flags)
        {
            return (Flags & flags) == flags;
        }

        protected void CalculateProjection()
        {
            _projFunc(_surface, _nearClip, _farClip, _fov, ref _projection);
            _viewProjection = Matrix4F.Multiply(_view, _projection);
            _invViewProjection = Matrix4F.Invert(_viewProjection);
        }

        private void _surface_OnPostResize(ITexture texture)
        {
            CalculateProjection();
        }

        /// <summary>Gets or sets the camera's view matrix.</summary>
        public virtual Matrix4F View
        {
            get => _view;
            set
            {
                _view = value;
                _viewProjection = Matrix4F.Multiply(_view, _projection);
                _transform = Matrix4F.Invert(_view);
                _invViewProjection = Matrix4F.Invert(_viewProjection);
            }
        }

        public virtual Matrix4F Transform
        {
            get => _transform;
            set
            {
                _transform = value;
                _view = Matrix4F.Invert(_transform);
                _viewProjection = Matrix4F.Multiply(_view, _projection);
                _invViewProjection = Matrix4F.Invert(_viewProjection);
            }
        }

        /// <summary>Gets the camera's projection matrix. The projection is ignored during rendering if <see cref="OutputSurface"/> is not set.</summary>
        public Matrix4F Projection => _projection;

        /// <summary>Gets the camera's combined view-projection matrix. This is the result of the view matrix multiplied by the projection matrix.</summary>
        public Matrix4F ViewProjection => _viewProjection;

        /// <summary>
        /// Gets the inverse view-projection matrix. This is equal to passing <see cref="ViewProjection"/> through <see cref="Matrix4F.Invert(Matrix4F)"/>.
        /// </summary>
        public Matrix4F InvViewProjection => _invViewProjection;

        /// <summary>Gets or sets the <see cref="IRenderSurface"/> that the camera's view should be rendered out to.</summary>
        public IRenderSurface OutputSurface
        {
            get => _surface;
            set
            {
                if (_surface != value)
                {
                    if (_surface != null)
                        _surface.OnPostResize -= _surface_OnPostResize;

                    if (value != null)
                        value.OnPostResize += _surface_OnPostResize;

                    _surface = value;
                    CalculateProjection();
                }
            }
        }

        /// <summary>Gets or sets the minimum draw dinstance. Also known as the near-clip plane. 
        /// Anything closer this value will not be drawn.</summary>
        public float MinDrawDistance
        {
            get => _nearClip;
            set
            {
                _nearClip = value;
                CalculateProjection();
            }
        }

        /// <summary>Gets or sets the maximum draw distance. Also known as the far-clip plane. 
        /// Anything further away than this value will not be drawn.</summary>
        public float MaxDrawDistance
        {
            get => _farClip;
            set
            {
                _farClip = value;
                CalculateProjection();
            }
        }

        /// <summary>
        /// Gets or sets the field-of-view. Has no effect on a 2D camera.
        /// </summary>
        public float FieldOfView
        {
            get => _fov;
            set
            {
                _fov = value;
                CalculateProjection();
            }
        }

        public Vector3F Position => Matrix4F.Invert(_view).Translation;

        /// <summary>
        /// Gets whether the camera will be skipped during rendering.
        /// </summary>
        public bool Skip { get; internal set; }

        /// <summary>
        /// Gets or sets the render flags for the current <see cref="RenderCamera"/>.
        /// </summary>
        public RenderCameraFlags Flags { get; set; }

        /// <summary>
        /// Gets or sets the camera's layer render mask. Each enabled bit ignores a layer with the same ID as the bit's position. 
        /// For example, setting bit 0 will skip rendering of layer 0 (the default layer).
        /// </summary>
        public int LayerMask { get; set; }

        /// <summary>
        /// Gets the <see cref="RenderProfiler"/> instance bound to the current <see cref="RenderCamera"/>, which tracks render performance and statistics of the scene rendered by the camera.
        /// </summary>
        public RenderProfiler Profiler { get; } = new RenderProfiler();

        /// <summary>
        /// Gets or sets the ordering depth of the current <see cref="RenderCamera"/>. The default value is 0.
        /// Cameras which share the same output surface and order-depth will be rendered in the other they were added to the scene.
        /// If you intend to output multiple cameras to the same <see cref="IRenderSurface"/>, it is recommended you change the order depth accordingly.
        /// </summary>
        public int OrderDepth { get; set; }

        /// <summary>
        /// Gets or sets the camera's mode.
        /// </summary>
        public RenderCameraMode Mode
        {
            get => _mode;
            set
            {
                if (_mode != value)
                {
                    _mode = value;
                    _projFunc = _projectionFuncs[_mode];
                    ClipRange clip = _clipPreset[_mode];
                    _nearClip = clip.Near;
                    _farClip = clip.Far;
                    CalculateProjection();
                }
            }
        }
    }

    public enum RenderCameraMode
    {
        /// <summary>
        /// Configures a camera for left-handed perspective mode.
        /// </summary>
        Perspective = 0,

        /// <summary>
        /// Configures a camera for left-handed orthographic mode.
        /// </summary>
        Orthographic = 1,
    }

    [Flags]
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
