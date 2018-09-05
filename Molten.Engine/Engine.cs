using Molten.Collections;
using Molten.Font;
using Molten.Graphics;
using Molten.Input;
using Molten.Threading;
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
        ThreadedQueue<EngineTask> _taskQueue;

        /// <summary>Gets the current instance of the engine. There can only be one active per application.</summary>
        public static Engine Current { get; private set; }

        /// <summary>
        /// Occurs right after the display manager has detected active display <see cref="IDisplayOutput"/>. Here you can change the output configuration before it is passed
        /// down to the graphics and rendering chain.
        /// </summary>
        event DisplayManagerEvent OnAdapterInitialized;
        
        internal Engine(EngineSettings initialSettings = null)
        {
            if (Current != null)
                throw new Exception("Cannot create more than one active instance of Engine. Dispose of the previous one first.");

            if (IntPtr.Size != 8)
                throw new NotSupportedException("Molten engine only supports 64-bit. Please build accordingly.");

            Current = this;
            Settings = initialSettings;

            if (Settings == null)
            {
                Settings = new EngineSettings();
                Settings.Load();
            }

            Log = Logger.Get();
            Log.AddOutput(new LogFileWriter("engine_log{0}.txt"));
            Log.WriteDebugLine("Engine Instantiated");
            Threading = new ThreadManager(this, Log);
            _taskQueue = new ThreadedQueue<EngineTask>();
            Content = new ContentManager(Log, this, null, null, Settings.ContentWorkerThreads);
            Scenes = new SceneManager(Settings.UI);

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Log.WriteError(e.ExceptionObject as Exception);
            Logger.DisposeAll();
        }

        internal void LoadInput()
        {
            Assembly inputAssembly;
            Input = LibraryDetection.LoadInstance<IInputManager>(Log, "input", "input manager", 
                Settings.Input.InputLibrary, 
                Settings.Input, 
                InputSettings.DEFAULT_LIBRARY, 
                out inputAssembly);

            // Initialize
            try
            {
                Input.Initialize(Settings.Input, Log);
                Log.WriteLine($"Initialized input manager");
            }
            catch (Exception e)
            {
                Log.WriteLine("Failed to initialize input manager");
                Log.WriteError(e);
                Input = null;
            }
        }

        internal bool LoadRenderer()
        {
            if (Renderer != null)
            {
                Log.WriteLine("Attempted to load renderer when one is already loaded!");
                return false;
            }

            // TODO default to OpenGL if on a non-windows platform.
            Assembly renderAssembly;
            Renderer = LibraryDetection.LoadInstance<RenderEngine>(Log, "render", "renderer", 
                Settings.Graphics.RendererLibrary, 
                Settings.Graphics, 
                GraphicsSettings.RENDERER_DX11, 
                out renderAssembly);

            try
            {
                Renderer.InitializeAdapter(Settings.Graphics);
                Log.WriteLine($"Initialized renderer");
            }
            catch (Exception e)
            {
                Log.WriteLine("Failed to initialize renderer");
                Log.WriteError(e);
                Renderer = null;
                return false;
            }

            OnAdapterInitialized?.Invoke(Renderer.DisplayManager);
            Renderer.Initialize(Settings.Graphics);
            LoadDefaultFont(Settings);

            return true;
        }

        private void LoadDefaultFont(EngineSettings settings)
        {
            try
            {
                using (FontReader reader = new FontReader(settings.DefaultFontName, Log))
                {
                    FontFile fontFile = reader.ReadFont(true);

                    DefaultFont = new SpriteFont(Renderer, fontFile, settings.DefaultFontSize);
                }
            }
            catch (Exception e)
            {
                // TODO Use the fallback font provided with the engine.
                Log.WriteError("Failed to load default font.");
                Log.WriteError(e);
                throw e;
            }
        }

        /// <summary>Starts the renderer thread.</summary>
        public void StartRenderer()
        {
            if (Renderer == null)
            {
                Log.WriteLine("A renderer has not be loaded. Unable to start renderer");
                Log.WriteDebugLine("Please ensure Engine.LoadRenderer() was called and a valid renderer library was provided.");
                return;
            }

            if (RenderThread != null)
            {
                Log.WriteLine("Ignored attempt to start renderer thread while already running");
                return;
            }

            RenderThread = Threading.SpawnThread("Renderer_main", true, false, (time) =>
            {
                Renderer.Present(time);
            });

            Log.WriteLine("Renderer thread started");
        }

        /// <summary>Stops the renderer thread.</summary>
        public void StopRenderer()
        {
            if (Renderer == null || RenderThread == null)
            {
                Log.WriteLine("Ignored attempt to stop renderer while not running");
                return;
            }

            RenderThread.Dispose();
            RenderThread = null;
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

            Input.Update(time);
            Scenes.Update(time);
        }

        public void Dispose()
        {
            Log.WriteDebugLine("Shutting down engine");
            Threading.Dispose();
            Renderer?.Dispose();
            Scenes.Dispose();
            Content.Dispose();
            Logger.DisposeAll();
            Settings.Save();
            Current = null;
        }

        /// <summary>
        /// [Internal] Gets the <see cref="Renderer"/> thread. Null if the renderer was not started.
        /// </summary>
        internal EngineThread RenderThread { get; private set; }

        /// <summary>Gets the renderer attached to the current <see cref="Engine"/> instance.</summary>>
        public RenderEngine Renderer { get; private set; }

        /// <summary>Gets the log attached to the current <see cref="Engine"/> instance.</summary>
        internal Logger Log { get; }

        /// <summary>Gets the engine settings.</summary>
        public EngineSettings Settings { get; }

        /// <summary>Gets the thread manager bound to the engine.</summary>
        public ThreadManager Threading { get; }

        /// <summary>
        /// Gets the main content manager bound to the current engine instance. <para/>
        /// It is recommended that you use this to load assets that are unlikely to be unloaded throughout the lifetime of the current session. 
        /// You should use separate <see cref="ContentManager"/> instances for level-specific or short-lifespan content. 
        /// Disposing of a <see cref="ContentManager"/> instance will unload all of the content that was loaded by it.<para />
        /// </summary>
        public ContentManager Content { get; }

        /// <summary>
        /// Gets the default font as defined in <see cref="EngineSettings"/>.
        /// </summary>
        public SpriteFont DefaultFont { get; private set; }

        /// <summary>Gets the input manager attached to the current <see cref="Engine"/> instance.</summary>
        public IInputManager Input { get; private set; }

        /// <summary>
        /// Gets the internal scene manager for the current <see cref="Engine"/> instance.
        /// </summary>
        internal SceneManager Scenes { get; }
    }
}
