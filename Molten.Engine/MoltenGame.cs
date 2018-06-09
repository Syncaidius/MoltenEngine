using Molten.Graphics;
using Molten.Input;
using Molten.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public delegate void MoltenGameHandler(MoltenGame game);

    public abstract class MoltenGame
    {
        Engine _engine;
        EngineThread _gameThread;
        IWindowSurface _gameWindow;
        IKeyboardDevice _keyboard;
        IMouseDevice _mouse;

        /// <summary>
        /// Occurs when the game is in the process of exiting.
        /// </summary>
        public event MoltenGameHandler OnGameExiting;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="title"></param>
        /// <param name="settings">The settings for the game. If this is null, the default settings will be used.</param>
        /// <param name="useGuiControl">If true, <see cref="MoltenGame.Window"/> will be a GUI control surface instead.</param>
        public MoltenGame(string title, EngineSettings settings = null)
        {
            Title = title;
            Settings = settings ?? new EngineSettings();
        }

        /// <summary>Starts the game. This will trigger initialization, then start the renderer and game threads.</summary>
        public void Start()
        {
            if (_gameThread != null)
                return;

            _engine = new Engine(Settings);
            _engine.LoadRenderer();
            _engine.LoadInput();

            if (_engine.Input == null)
            {
                _engine.Log.WriteError("Input library failed to initialize. Forcing game exit.");
                ForceExit();
                return;
            }

            if (_engine.Renderer == null)
            {
                _engine.Log.WriteError("Renderer failed to initialize. Forcing game exit.");
                ForceExit();
                return;
            }

            if (Settings.UseGuiControl)
                _gameWindow = _engine.Renderer.Resources.CreateControlSurface(Title);
            else
                _gameWindow = _engine.Renderer.Resources.CreateFormSurface(Title);

            _engine.Renderer.OutputSurfaces.Add(_gameWindow);
            _gameWindow.Visible = true;

            _keyboard = _engine.Input.GetKeyboard(_gameWindow);
            _mouse = _engine.Input.GetMouse(_gameWindow);
            _engine.Input.SetActiveWindow(_gameWindow);

            _engine.Renderer.DefaultSurface = _gameWindow;
            _engine.StartRenderer();

            _gameThread = _engine.Threading.SpawnThread("game", false, true, (timing) =>
            {
                if (RunState != GameRunState.Exiting)
                {
                    OnUpdate(timing);
                    _engine.Update(timing);
                }
                else
                {
                    _engine.Log.WriteLine("Game exiting");
                    OnGameExiting?.Invoke(this);
                    OnExiting();
                    ForceExit();
                }
            });

            OnInitialize(Engine);
            OnFirstLoad(Engine);
            _gameThread.Start();
        }

        /// <summary>
        /// Dispatches a callback to the renderer thread, to be executed on it's next update tick.
        /// </summary>
        /// <param name="callback"></param>
        public void DispatchToRenderThread(Action callback)
        {
            Engine.RenderThread.Dispatch(callback);
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
        public Scene CreateScene(string name, 
            SceneRenderFlags flags = SceneRenderFlags.Render2D | SceneRenderFlags.Render3D)
        {
            Scene scene = new Scene(name, Engine, flags);
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
        /// <param name="updateFlags">The update flags to set on the newly spawned object.</param>
        /// <param name="scene">The scene to automatically add the object to. Default value is null.</param>
        /// <returns></returns>
        public SceneObject CreateObject(Vector3F position, Scene scene = null, ObjectUpdateFlags flags = ObjectUpdateFlags.All)
        {
            SceneObject obj = new SceneObject(Engine);
            obj.Transform.LocalPosition = position;
            scene?.AddObject(obj);
            return obj;
        }

        public void Pause()
        {
            if (RunState == GameRunState.Paused || RunState == GameRunState.Exited || RunState == GameRunState.Exiting)
                return;

            OnPause();
        }

        public void Resume()
        {
            if (RunState == GameRunState.Running || RunState == GameRunState.Exited || RunState == GameRunState.Exiting)
                return;

            OnResume();
            RunState = GameRunState.Running;
        }

        /// <summary>Signals the game/engine to start the exit procedure. This will allow the game and engine to shutdown cleanly (i.e whenever they are ready to exit).</summary>
        public void Exit()
        {
            RunState = GameRunState.Exiting;           
        }

        /// <summary>Forces the game to exit. This will not give the game a chance to save (to avoid unintended side-effects from forcefully shutting the game/engine down).</summary>
        public void ForceExit()
        {
            _engine.Log.WriteLine("Game closed");
            _engine.Dispose();
            RunState = GameRunState.Exited;
        }

        protected virtual void OnInitialize(Engine engine) { }

        protected virtual void OnFirstLoad(Engine engine) { }

        /// <summary>Occurs when the game is in the process of exiting. This gives the game logic a chance to correctly handle the exit, such as saving the player's progress.</summary>
        protected virtual void OnExiting() { }

        /// <summary>Occurs when the game is paused (and not already paused).</summary>
        protected virtual void OnPause() { }

        /// <summary>Occurs when the game is resumed (and not already running).</summary>
        protected virtual void OnResume() { }

        protected abstract void OnUpdate(Timing time);

        /// <summary>Gets or sets title of the game.</summary>
        public string Title { get; set; }

        /// <summary>Gets the game's engine settings.</summary>
        public EngineSettings Settings { get; private set; }

        /// <summary>Gets the game's current run state.</summary>
        public GameRunState RunState { get; private set; }

        /// <summary>Gets the engine instance used by the game.</summary>
        public Engine Engine => _engine;

        /// <summary>Gets the game and engine <see cref="Logger"/>. It is used to write information into the game's log file.</summary>
        public Logger Log => _engine.Log;

        public Timing Time => _gameThread.Timing;

        /// <summary>Gets the <see cref="IKeyboardDevice"/> attached to the game's main window.</summary>
        public IKeyboardDevice Keyboard => _keyboard;

        /// <summary>Gets the <see cref="IMouseDevice"/> attached to the game's main window.</summary>
        public IMouseDevice Mouse => _mouse;

        /// <summary>Gets the <see cref="IWindowSurface"/> that the game renders in to.</summary>
        public IWindowSurface Window => _gameWindow;
    }
}
