using Molten.Collections;
using Molten.Graphics;
using Molten.Input;
using Molten.Threading;
using Molten.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    /// <summary>Stone Bolt engine. You only need one instance.</summary>
    public class Engine : IDisposable
    {
        EngineSettings _settings;
        Logger _log;
        IRenderer _renderer;
        ThreadManager _threadManager;
        EngineThread _threadRenderer;
        ContentManager _content;
        IInputManager _input;
        UISystem _ui;

        internal List<Scene> Scenes;

        ThreadedQueue<EngineTask> _taskQueue;

        /// <summary>Gets the current instance of the engine. There can only be one active per application.</summary>
        public static Engine Current { get; private set; }

        /// <summary>
        /// Occurs right after the display manager has detected active display <see cref="IDisplayOutput"/>. Here you can change the output configuration before it is passed
        /// down to the graphics and rendering chain.
        /// </summary>
        event DisplayManagerEvent OnAdapterInitialized;
        
        public Engine(EngineSettings settings = null)
        {
            if (Current != null)
                throw new Exception("Cannot create more than one active instance of Engine. Dispose of the previous one first.");

            Current = this;
            _settings = settings;

            if (_settings == null)
            {
                _settings = new EngineSettings();
                _settings.Load();
            }

            _log = Logger.Get();
            _log.AddOutput(new LogFileWriter("engine_log{0}.txt"));
            _log.WriteDebugLine("Engine Instantiated");
            _threadManager = new ThreadManager(this, _log);
            _taskQueue = new ThreadedQueue<EngineTask>();
            _content = new ContentManager(_log, this, null, settings.ContentWorkerThreads);

            InputLoader inputLoader = new InputLoader();
            _input = inputLoader.GetManager(_log, _settings.Input);
            Scenes = new List<Scene>();

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _log.WriteError(e.ExceptionObject as Exception);
            Logger.DisposeAll();
        }

        public void LoadRenderer()
        {
            if (_renderer != null)
            {
                _log.WriteLine("Attempted to load renderer when one is already loaded!");
                return;
            }

            // Load renderer library
            RenderLoader renderLoader = new RenderLoader();
            _renderer = renderLoader.GetRenderer(_log, _settings.Graphics);
            OnAdapterInitialized?.Invoke(_renderer.DisplayManager);
            _renderer.Initialize(_settings.Graphics);
        }

        /// <summary>Starts the renderer thread.</summary>
        public void StartRenderer()
        {
            if (_renderer == null)
            {
                _log.WriteLine("A renderer has not be loaded. Unable to start renderer");
                _log.WriteDebugLine("Please ensure Engine.LoadRenderer() was called and a valid renderer library was provided.");
                return;
            }

            if (_threadRenderer != null)
            {
                _log.WriteLine("Ignored attempt to start renderer thread while already running");
                return;
            }

            _threadRenderer = _threadManager.SpawnThread("Renderer_main", true, false, (time) =>
            {
                _renderer.Present(time);
            });

            _log.WriteLine("Renderer thread started");
        }

        /// <summary>Stops the renderer thread.</summary>
        public void StopRenderer()
        {
            if (_renderer == null || _threadRenderer == null)
            {
                _log.WriteLine("Ignored attempt to stop renderer while not running");
                return;
            }

            _threadRenderer.Dispose();
            _threadRenderer = null;
        }

        internal void AddScene(Scene scene)
        {
            EngineAddScene task = EngineAddScene.Get();
            task.Scene = scene;
            _taskQueue.Enqueue(task);
        }

        internal void RemoveScene(Scene scene)
        {
            EngineRemoveScene task = EngineRemoveScene.Get();
            task.Scene = scene;
            _taskQueue.Enqueue(task);
        }

        internal void Update(Timing time)
        {
            EngineTask task = null;
            while (_taskQueue.TryDequeue(out task))
                task.Process(this, time);

            _input.Update(time);

            // Run through all the scenes and update if enabled.
            foreach(Scene scene in Scenes)
            { 
                if(scene.IsEnabled)
                    scene.Update(time);
            }
        }

        public void Dispose()
        {
            _log.WriteDebugLine("Shutting down engine");
            _threadManager.Dispose();
            _renderer?.Dispose();

            // Dispose of scenes
            for (int i = 0; i < Scenes.Count; i++)
                Scenes[i].Dispose();

            _content.Dispose();
            Logger.DisposeAll();
            _settings.Save();
            Current = null;
        }

        /// <summary>Gets the renderer attached to the current <see cref="Engine"/> instance.</summary>>
        public IRenderer Renderer => _renderer;

        /// <summary>Gets the log attached to the current <see cref="Engine"/> instance.</summary>
        internal Logger Log => _log;

        /// <summary>Gets the engine settings.</summary>
        public EngineSettings Settings => _settings;

        /// <summary>Gets the thread manager bound to the engine.</summary>
        public ThreadManager Threading => _threadManager;

        /// <summary>
        /// Gets the main content manager bound to the current engine instance. <para/>
        /// It is recommended that you use this to load assets that are unlikely to be unloaded throughout the lifetime of the current session. 
        /// You may wish to use separate <see cref="ContentManager"/> instances for level-specific content or content which is likely to have a short lifespan, 
        /// since disposing of a <see cref="ContentManager"/> instance will unload all of the content that was loaded by it.
        /// </summary>
        public ContentManager Content => _content;

        /// <summary>Gets the input manager attached to the current <see cref="Engine"/> instance.</summary>
        public IInputManager Input => _input;
    }
}
