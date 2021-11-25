using Molten.Graphics;
using Molten.Input;
using Molten.Threading;
using Molten.Utility;
using System;
using System.Threading;

namespace Molten
{
    /// <summary>
    /// Provides a headless (no input or rendering capability) on which to build a game or other type 
    /// of application with Molten engine.
    /// </summary>
    public abstract class Foundation : IDisposable
    {
        EngineThread _gameThread;

        /// <summary>
        /// Occurs when the game is in the process of closing.
        /// </summary>
        public event MoltenEventHandler<Foundation> OnClosing;

        /// <summary>Creates a new instance of <see cref="Foundation"/>.</summary>
        /// <param name="title"></param>

        public Foundation(string title)
        {
            Title = title;
        }

        /// <summary>Starts the game. This will trigger initialization, then start the renderer and game threads.</summary>
        /// <param name="settings">The settings for the game. If this is null, the default settings will be used.</param>
        /// <param name="ignoreSavedSettings">If true, the previously-saved settings will be ignored and 
        /// replaced with the provided (or default) settings.</param>
        /// <param name="gameThreadApartment">The thread apartment state for the main game thread.</param>
        /// <param name="renderThreadApartment">The thread apartment state for the renderer thread</param>
        public void Start(EngineSettings settings = null,
            bool ignoreSavedSettings = false,
            ApartmentState gameThreadApartment = ApartmentState.MTA, ApartmentState renderThreadApartment = ApartmentState.MTA)
        {
            if (_gameThread != null)
                return;

            Engine = new Engine(settings, ignoreSavedSettings);

            _gameThread = Engine.Threading.SpawnThread("game", false, true, (timing) =>
            {
                if (RunState != GameRunState.Exiting)
                {
                    OnUpdate(timing);
                    Engine.Update(timing);
                }
                else
                {
                    Engine.Log.WriteLine("Game exiting");
                    OnClosing?.Invoke(this);
                    OnClose();
                    ForceExit();
                }
            }, gameThreadApartment);

            OnInitialize(Engine);
            OnFirstLoad(Engine);

            _gameThread.Start();
        }

        /// <summary>
        /// Disposes of the foundation and it's underlying <see cref="Engine"/> instance.
        /// </summary>
        public void Dispose()
        {
            if (!IsDisposed)
            {
                IsDisposed = true;
                ForceExit();
                Engine.Dispose();
                OnDispose();
            }
        }

        /// <summary>
        /// Dispatches a callback to the game thread, to be executed on its next update tick.
        /// </summary>
        /// <param name="callback"></param>
        public void DispatchToGameThread(Action callback)
        {
            _gameThread.Dispatch(callback);
        }

        /// <summary>Creates a new instance of <see cref="Scene"/> and automatically binds it to the game engine.</summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Scene CreateScene(string name)
        {
            Scene scene = new Scene(name, Engine);
            return scene;
        }

        /// <summary>Creates a new <see cref="SceneObject"/> at position 0,0,0.</summary>
        /// <param name="updateFlags">The update flags to set on the newly spawned object.</param>
        /// <param name="scene">The scene to automatically add the object to. Default value is null.</param>
        /// <returns></returns>
        public SceneObject CreateObject(Scene scene = null, ObjectUpdateFlags updateFlags = ObjectUpdateFlags.All)
        {
            return CreateObject(Vector3F.Zero, scene, updateFlags);
        }

        /// <summary>Creates a new <see cref="SceneObject"/> at specified position.</summary>
        /// <param name="position">The world position at which to create a new <see cref="SceneObject"/>.</param>
        /// <param name="flags">The update flags to set on the newly spawned object.</param>
        /// <param name="scene">The scene to automatically add the object to. Default value is null.</param>
        /// <returns></returns>
        public SceneObject CreateObject(Vector3F position, Scene scene = null, ObjectUpdateFlags flags = ObjectUpdateFlags.All)
        {
            SceneObject obj = new SceneObject(Engine);
            obj.Transform.LocalPosition = position;
            scene?.AddObject(obj);
            return obj;
        }

        /// <summary>
        /// Puases the game.
        /// </summary>
        public void Pause()
        {
            if (RunState == GameRunState.Paused || RunState == GameRunState.Exited || RunState == GameRunState.Exiting)
                return;

            OnPause();
        }

        /// <summary>
        /// Resumes the game. Has no effect if the gamge is already running, exiting or exited.
        /// </summary>
        public void Resume()
        {
            if (RunState == GameRunState.Running || RunState == GameRunState.Exited || RunState == GameRunState.Exiting)
                return;

            OnResume();
            RunState = GameRunState.Running;
        }

        /// <summary>Signals the game/engine to start the exit procedure. 
        /// This will allow the game and engine to shutdown cleanly (i.e whenever they are ready to exit).
        /// Does not call <see cref="Dispose"/></summary>
        public void Exit()
        {
            RunState = GameRunState.Exiting;
        }

        /// <summary>Forces the game to exit. This will not give the game a chance to 
        /// save (to avoid unintended side-effects from forcefully shutting the game/engine down).
        /// Does not call <see cref="Dispose"/></summary>
        public void ForceExit()
        {
            Engine.Log.WriteLine("Game closed");
            Engine.Dispose();
            _gameThread.Dispose();
            RunState = GameRunState.Exited;
        }

        /// <summary>
        /// Invoked when the current <see cref="Foundation{R, I}"/> needs to be initialized.
        /// </summary>
        /// <param name="engine"></param>
        protected virtual void OnInitialize(Engine engine) { }

        /// <summary>
        /// Invoked when the current <see cref="Foundation{R, I}"/> is done initializing and 
        /// can begin loading for the first time.
        /// </summary>
        /// <param name="engine"></param>
        protected virtual void OnFirstLoad(Engine engine) { }

        /// <summary>
        /// Invoked when the <see cref="Foundation"/> is disposing.
        /// </summary>
        protected abstract void OnDispose();

        /// <summary>Occurs when the game is in the process of exiting. This gives the game logic a
        /// chance to correctly handle the exit, such as saving the player's progress.</summary>
        protected virtual void OnClose() { }

        /// <summary>Occurs when the game is paused (and not already paused).</summary>
        protected virtual void OnPause() { }

        /// <summary>Occurs when the game is resumed (and not already running).</summary>
        protected virtual void OnResume() { }

        /// <summary>
        /// Occurs when the game is being updated, making it a good place to put your game logic.
        /// </summary>
        /// <param name="time">A <see cref="Timing"/> instance for tracking game time, frame rate and other statistics.</param>
        protected abstract void OnUpdate(Timing time);

        /// <summary>Gets or sets title of the game.</summary>
        public string Title { get; set; }

        /// <summary>Gets the game's engine settings.</summary>
        public EngineSettings Settings => Engine.Settings;

        /// <summary>Gets the game's current run state.</summary>
        public GameRunState RunState { get; private set; }

        /// <summary>Gets the engine instance used by the game.</summary>
        public Engine Engine { get; private set; }

        /// <summary>Gets the game and engine <see cref="Logger"/>. It is used to write information into the game's log file.</summary>
        public Logger Log => Engine.Log;

        /// <summary>
        /// Gets the <see cref="Timing"/> instance of the game's main thread.
        /// </summary>
        public Timing Time => _gameThread.Timing;

        /// <summary>
        /// Gets whether or not the current <see cref="Foundation{R, I}"/> instance has been disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }
    }

    /// <summary>
    /// Provides a foundation on which to build a game or other type 
    /// of application with Molten engine.
    /// </summary>
    public abstract class Foundation<R, I> : Foundation
        where R : MoltenRenderer, new()
        where I : InputManager, new()
    {
        /// <summary>Creates a new instance of <see cref="Foundation{R, I}"/>.</summary>
        /// <param name="title"></param>

        public Foundation(string title) : 
            base(title)
        {
            Title = title;
        }

        protected override void OnDispose()
        {
            Window.Dispose();
        }

        protected override void OnInitialize(Engine engine)
        {
            Engine.LoadRenderer<R>();
            Engine.LoadInput<I>();

            if (Engine.Input == null)
            {
                Engine.Log.WriteError("Input library failed to initialize. Forcing game exit.");
                ForceExit();
                return;
            }

            if (Engine.Renderer == null)
            {
                Engine.Log.WriteError("Renderer failed to initialize. Forcing game exit.");
                ForceExit();
                return;
            }

            if (Settings.UseGuiControl)
                Window = Engine.Renderer.Resources.CreateControlSurface(Title, "MainForm");
            else
                Window = Engine.Renderer.Resources.CreateFormSurface(Title, "MainControl");

            Window.OnClose += _gameWindow_OnClose;
            Engine.Renderer.OutputSurfaces.Add(Window);
            Window.Visible = true;

            Keyboard = Engine.Input.GetKeyboard();
            Mouse = Engine.Input.GetMouse();
            Gamepad = Engine.Input.GetGamepad<GamepadDevice>(0, GamepadSubType.Gamepad);
            Engine.StartRenderer(ApartmentState.MTA);

            base.OnInitialize(engine);
        }

        private void _gameWindow_OnClose(INativeSurface surface)
        {
            Exit();
        }

        /// <summary>
        /// Dispatches a callback to the renderer thread, to be executed on it's next update tick.
        /// </summary>
        /// <param name="callback"></param>
        public void DispatchToRenderThread(Action callback)
        {
            Engine.RenderThread.Dispatch(callback);
        }

        /// <summary>Gets the <see cref="KeyboardDevice"/> attached to the game's main window.</summary>
        public KeyboardDevice Keyboard { get; private set; }

        /// <summary>Gets the <see cref="MouseDevice"/> attached to the game's main window.</summary>
        public MouseDevice Mouse { get; private set; }

        /// <summary>
        /// Gets the <see cref="GamepadDevice"/> attached to the game's main window.
        /// </summary>
        public GamepadDevice Gamepad { get; private set; }

        /// <summary>Gets the <see cref="INativeSurface"/> that the game renders in to.</summary>
        public INativeSurface Window { get; private set; }
    }
}
