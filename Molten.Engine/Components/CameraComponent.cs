using Molten.Graphics;
using Molten.Input;

namespace Molten
{
    /// <summary>Provides a managed 'eye' through which to render and interact with a <see cref="Scene"/>.</summary>
    public class CameraComponent : SceneComponent, IInputReceiver<KeyboardDevice>
    {
        public delegate void InputCameraSurfaceHandler(CameraComponent camera, IRenderSurface2D surface);

        public event SceneInputEventHandler<PointerButton> OnObjectFocused;

        public event SceneInputEventHandler<PointerButton> OnObjectUnfocused;

        public event ObjectHandler<IPickable<Vector2F>> FocusedChanged;

        static PointerButton[] _pButtons;

        RenderCamera _camera;
        bool _inScene = false; 
        IPickable<Vector2F> _focused;
        KeyboardDevice _keyboard;

        Dictionary<ulong, List<CameraInputTracker>> _trackers;

        /// <summary>
        /// Invoked when the output surface has been changed.
        /// </summary>
        public event InputCameraSurfaceHandler OnSurfaceChanged;

        /// <summary>
        /// Invoked when the bound <see cref="Surface"/> has been resized.
        /// </summary>
        public event InputCameraSurfaceHandler OnSurfaceResized;

        static CameraComponent()
        {
            _pButtons = ReflectionHelper.GetEnumValues<PointerButton>(); 
        }

        /// <summary>
        /// Creates a new instance of <see cref="CameraComponent"/>
        /// </summary>
        public CameraComponent()
        {
            _trackers = new Dictionary<ulong, List<CameraInputTracker>>();
            _camera = new RenderCamera(RenderCameraMode.Perspective);
            _camera.OnOutputSurfaceChanged += _camera_OnOutputSurfaceChanged;
            _camera.OnSurfaceResized += _camera_OnSurfaceResized;
        }

        #region Keyboard input handling

        /// <inheritdoc></inheritdoc>/>
        public void InitializeInput(KeyboardDevice device, Timing timing)
        {
            if (_keyboard == null)
            {
                _keyboard = device;
                device.OnConnected += OnKeyboardInitialized;
                device.OnDisconnected += OnKeyboardDeinitialized;
            }

            OnKeyboardInitialized(device);
        }

        private void OnKeyboardDeinitialized(InputDevice o)
        {
            KeyboardDevice kb = o as KeyboardDevice;
            kb.OnCharacterKey -= Device_OnCharacterKey;
            kb.OnKeyDown -= Kb_OnKeyDown;
            kb.OnKeyUp -= Kb_OnKeyUp;

            // TODO release any current keyboard-reliant state.
        }

        private void OnKeyboardInitialized(InputDevice o)
        {
            KeyboardDevice kb = o as KeyboardDevice;
            kb.OnCharacterKey += Device_OnCharacterKey;
            kb.OnKeyDown += Kb_OnKeyDown;
            kb.OnKeyUp += Kb_OnKeyUp;
        }

        private void Kb_OnKeyUp(KeyboardDevice device, KeyboardKeyState state)
        {
            FocusedPickable?.OnKeyUp(device, ref state);
        }

        private void Kb_OnKeyDown(KeyboardDevice device, KeyboardKeyState state)
        {
            FocusedPickable?.OnKeyDown(device, ref state);
        }

        private void Device_OnCharacterKey(KeyboardDevice device, KeyboardKeyState state)
        {
            FocusedPickable?.OnKeyboardChar(device, ref state);
        }

        /// <inheritdoc></inheritdoc>/>
        public void DeinitializeInput(KeyboardDevice device, Timing timing)
        {
            device.OnDisconnected += OnKeyboardDeinitialized;
        }

        /// <inheritdoc/>
        public void HandleInput(KeyboardDevice device, Timing time)
        {
            if (FocusedPickable != null)
                FocusedPickable.OnKeyboardInput(device, time);
        }
        #endregion

        private void _camera_OnSurfaceResized(RenderCamera camera, IRenderSurface2D surface)
        {
            OnSurfaceResized?.Invoke(this, surface);
        }

        protected override void OnDispose() 
        {
            if (_keyboard == null)
                return;

            _keyboard.OnConnected -= OnKeyboardInitialized;
            _keyboard.OnDisconnected -= OnKeyboardDeinitialized;
            OnKeyboardDeinitialized(_keyboard);
            _keyboard = null;
        }

        private void _camera_OnOutputSurfaceChanged(RenderCamera camera, IRenderSurface2D oldSurface, IRenderSurface2D newSurface)
        {
            OnSurfaceChanged?.Invoke(this, newSurface);
        }

        protected override void OnInitialize(SceneObject obj)
        {
            AddToScene(obj);
            obj.OnRemovedFromScene += Obj_OnRemovedFromScene;
            obj.OnAddedToScene += Obj_OnAddedToScene;

            base.OnInitialize(obj);
        }

        private void AddToScene(SceneObject obj)
        {
            if (_inScene)
                return;

            // Add mesh to render data if possible.
            if (obj.Scene != null)
            {
                obj.Scene.RenderData.AddObject(_camera);
                _inScene = true;

                if (IsFocused)
                    Focus();
            }
        }

        private void RemoveFromScene(SceneObject obj)
        {
            if (!_inScene)
                return;

            if (obj.Scene != null)
            {
                obj.Scene.RenderData.RemoveObject(_camera);
                Unfocus();
                _inScene = false;
            }
        }

        protected internal override bool OnRemove(SceneObject obj)
        {
            obj.OnRemovedFromScene -= Obj_OnRemovedFromScene;
            obj.OnAddedToScene -= Obj_OnAddedToScene;
            RemoveFromScene(obj);

            return base.OnRemove(obj);
        }

        private void Obj_OnAddedToScene(SceneObject obj, Scene scene, SceneLayer layer)
        {
            AddToScene(obj);
        }

        private void Obj_OnRemovedFromScene(SceneObject obj, Scene scene, SceneLayer layer)
        {
            RemoveFromScene(obj);
        }

        /// <inheritdoc/>
        public override void OnUpdate(Timing time)
        {
            base.OnUpdate(time);

            _camera.Transform = Object.Transform.Global;
            Rectangle constraintBounds = InputConstraintBounds.HasValue ? InputConstraintBounds.Value : Rectangle.Empty;

            // Update all pointer trackers
            foreach (KeyValuePair<ulong, List<CameraInputTracker>> kv in _trackers)
            {
                for (int j = 0; j < kv.Value.Count; j++)
                    kv.Value[j].Update(time, ref constraintBounds);
            }
        }

        /// <summary>
        /// Focuses input towards the current <see cref="CameraComponent"/>.
        /// </summary>
        public void Focus()
        {
            if (_inScene)
            {
                if(Object.Scene.FocusedCamera != this)
                {
                    if(Object.Scene.FocusedCamera != null)
                        Object.Scene.FocusedCamera.Unfocus();

                    TrackPointingDevices();
                    Object.Scene.FocusedCamera = this;
                }
            }

            IsFocused = true;
        }

        /// <summary>
        /// Removes input focus from the current <see cref="CameraComponent"/>.
        /// </summary>
        public void Unfocus()
        {
            if (_inScene)
            {
                if (Object.Scene.FocusedCamera == this)
                    Object.Scene.FocusedCamera = null;
            }

            if (IsFocused)
            {
                foreach (ulong deviceID in _trackers.Keys)
                {
                    if (_trackers.TryGetValue(deviceID, out List<CameraInputTracker> trackers))
                    {
                        foreach (CameraInputTracker tracker in trackers)
                            tracker.Release();
                    }
                }

                _trackers.Clear();
                IsFocused = false;
            }
        }

        private void TrackPointingDevices()
        {
            MouseDevice mouse = Object.Engine.Input.GetMouse();
            TouchDevice touch = Object.Engine.Input.GetTouch();

            if (mouse != null)
                TrackDevice(mouse);

            if (touch != null)
                TrackDevice(touch);
        }

        private void TrackDevice(PointingDevice device)
        {
            if (_trackers.ContainsKey(device.EOID) || device.IsDisposed)
                return;

            List<CameraInputTracker> trackers = new List<CameraInputTracker>();
            _trackers.Add(device.EOID, trackers);

            device.OnDisposing += Device_OnDisposing;

            Rectangle constraintBounds = InputConstraintBounds.HasValue ? InputConstraintBounds.Value : Rectangle.Empty;

            for (int setID = 0; setID < device.StateSetCount; setID++)
            {
                foreach (PointerButton button in _pButtons)
                {
                    if (button == PointerButton.None)
                        continue;
                    
                    trackers.Add(new CameraInputTracker(this, device, setID, button, ref constraintBounds));
                }
            }
        }

        private void UntrackDevice(PointingDevice device)
        {
            if (_trackers.TryGetValue(device.EOID, out List<CameraInputTracker> trackers))
            {
                foreach (CameraInputTracker tracker in trackers)
                    tracker.Release();
            }

            _trackers.Remove(device.EOID);
        }

        private void Device_OnDisposing(EngineObject o)
        {
            UntrackDevice(o as PointingDevice);
        }

        /// <summary>Converts the provided screen position to a globalized 3D world position.</summary>
        /// <param name="location">The screen position.</param>
        /// <returns></returns>
        public Vector3F ConvertScreenToWorld(Vector2F location)
        {
            Vector4F result = Vector2F.Transform(location, Object.Transform.Global);
            return new Vector3F(result.X, result.Y, result.Z);
        }

        public Vector2F ConvertWorldToScreen(Vector3F position)
        {
            Vector4F result = Vector3F.Transform(position, _camera.View);
            return new Vector2F(result.X, result.Y);
        }

        /// <summary>
        /// Returns whether or not the current <see cref="CameraComponent"/> has the specified <see cref="RenderCameraFlags"/>.
        /// </summary>
        /// <param name="flags">The flags.</param>
        /// <returns></returns>
        public bool HasFlags(RenderCameraFlags flags)
        {
            return _camera.HasFlags(flags);
        }

        public bool IsFirstInput(KeyboardDevice device)
        {
            return _keyboard == null;
        }

        /// <summary>
        /// Gets the view matrix of the current <see cref="CameraComponent"/>.
        /// </summary>
        public Matrix4F View => _camera.View;

        /// <summary>
        /// Gets the projection matrix of the current <see cref="CameraComponent"/>.
        /// </summary>
        public Matrix4F Projection => _camera.Projection;

        /// <summary>
        /// Gets the combined view and projection matrix of the current <see cref="CameraComponent"/>. This is the result of multiplying <see cref="View"/> and <see cref="Projection"/> together.
        /// </summary>
        public Matrix4F ViewProjection => _camera.ViewProjection;

        /// <summary>Gets or sets the <see cref="IRenderSurface2D"/> that the camera's view should be rendered out to.</summary>
        public IRenderSurface2D Surface
        {
            get => _camera.Surface;
            set => _camera.Surface = value;
        }

        /// <summary>Gets or sets the minimum draw dinstance. Also known as the near-clip plane. 
        /// Anything closer this value will not be drawn.</summary>
        public float MinDrawDistance
        {
            get => _camera.MinDrawDistance;
            set => _camera.MinDrawDistance = value;
        }

        /// <summary>Gets or sets the maximum draw distance. Also known as the far-clip plane. 
        /// Anything further away than this value will not be drawn.</summary>
        public float MaxDrawDistance
        {
            get => _camera.MaxDrawDistance;
            set => _camera.MaxDrawDistance = value;
        }

        /// <summary>
        /// Gets or sets the camera's field-of-view (FoV), in radians.
        /// </summary>
        public float FieldOfView
        {
            get => _camera.FieldOfView;
            set => _camera.FieldOfView = value;
        }

        /// <summary>
        /// Gets the position of the camera based on it's parent object's transform.
        /// </summary>
        public Vector3F Position => Object.Transform.GlobalPosition;

        /// <summary>
        /// Gets or sets the <see cref="RenderCameraFlags"/> for the current <see cref="CameraComponent"/>.
        /// </summary>
        public RenderCameraFlags Flags
        {
            get => _camera.Flags;
            set => _camera.Flags = value;
        }

        /// <summary>
        /// Gets or sets the camera's layer render mask. Each enabled bit ignores a layer with the same ID as the bit's position. 
        /// For example, setting bit 0 will skip rendering of layer 0 (the default layer).
        /// </summary>
        public SceneLayerMask LayerMask
        {
            get => _camera.LayerMask;
            set => _camera.LayerMask = value;
        }

        /// <summary>
        /// Gets or sets the ordering depth of the current <see cref="RenderCamera"/>. The default value is 0.
        /// Cameras which share the same output surface and order-depth will be rendered in the other they were added to the scene.
        /// If you intend to output multiple cameras to the same <see cref="IRenderSurface2D"/>, it is recommended you change the order depth accordingly.
        /// </summary>
        public int OrderDepth
        {
            get => _camera.OrderDepth;
            set => _camera.OrderDepth = value;
        }

        /// <summary>
        /// Gets or sets the camera's mode.
        /// </summary>
        public RenderCameraMode Mode
        {
            get => _camera.Mode;
            set => _camera.Mode = value;
        }

        /// <summary>
        /// Gets or sets the multi-sampling level of the current <see cref="CameraComponent"/>.
        /// </summary>
        public AntiAliasLevel MultiSampleLevel
        {
            get => _camera.MultiSampleLevel;
            set => _camera.MultiSampleLevel = value;
        }

        /// <summary>
        /// Gets whether the current <see cref="CameraComponent"/> is focused.
        /// </summary>
        public bool IsFocused { get; private set; }

        /// <summary>
        /// Gets or sets the constraint bounds for input. Only accepts input if it is within these bounds. 
        /// 
        /// <para>Any positional input data will be relative to the top-left of these bounds.</para>
        /// </summary>
        public Rectangle? InputConstraintBounds { get; set; }

        /// <summary>
        /// Gets the currently-focused <see cref="IPickable<Vector2F>"/>.
        /// </summary>
        public IPickable<Vector2F> FocusedPickable
        {
            get => _focused;
            set
            {
                if (_focused != value)
                {
                    if (_focused != null)
                        _focused.IsFocused = false;

                    _focused = value;

                    if (_focused != null)
                        _focused.IsFocused = true;

                    FocusedChanged?.Invoke(_focused);
                }
            }
        }
    }
}
