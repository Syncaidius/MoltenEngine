using Molten.Audio;
using Molten.Collections;
using Molten.Graphics;
using Molten.Input;
using Molten.Net;
using Molten.Threading;
using Molten.UI;

namespace Molten
{
    /// <summary>Stone Bolt engine. You only need one instance.</summary>
    public class Engine : IDisposable
    {
        ThreadedQueue<EngineTask> _taskQueue;
        List<EngineService> _services;
        EngineThread _mainThread;

        /// <summary>Gets the current instance of the engine. There can only be one active per application.</summary>
        public static Engine Current { get; private set; }

        /// <summary>
        /// Creates a new instance of <see cref="Engine"/>.
        /// </summary>
        /// <param name="initialSettings">The initial engine settings. If null, the default settings will be used instead.</param>
        /// <param name="ignoreSavedSettings">If true, the previously-saved settings will be ignored and replaced with the provided (or default) settings.</param>
        internal Engine(EngineSettings initialSettings, bool ignoreSavedSettings)
        {
            if (Current != null)
                throw new Exception("Cannot create more than one active instance of Engine. Dispose of the previous one first.");

            if (IntPtr.Size != 8)
                throw new NotSupportedException("Molten engine only supports 64-bit. Please build accordingly.");

            Current = this;
            Settings = initialSettings ?? new EngineSettings();

            if (!ignoreSavedSettings)
                Settings.Load();

            Settings.Apply();

            Log = Logger.Get();
            Log.AddOutput(new LogFileWriter("Logs/engine_log.log"));
            Log.Debug("Engine Instantiated");
            Threading = new ThreadManager(Log);
            _taskQueue = new ThreadedQueue<EngineTask>();
            _services = new List<EngineService>(Settings.StartupServices);
            Content = new ContentManager(Log, this, Settings.ContentWorkerThreads, Settings.AllowContentHotReload);
            Fonts = new SpriteFontManager(Log, this);
            Scenes = new SceneManager();

            Renderer = GetService<RenderService>();
            Input = GetService<InputService>();
            Audio = GetService<AudioService>();
            Net = GetService<NetworkService>();

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            foreach (EngineService service in _services)
                service.Initialize(Settings, Log);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Log.Error(e.ExceptionObject as Exception);
            Logger.DisposeAll();
        }

        /// <summary>
        /// Retrieves an <see cref="EngineService"/> of the specified type. 
        /// Services are defined before an <see cref="Engine"/> instance is started.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetService<T>() where T: EngineService
        {
            Type t = typeof(T);
            foreach(EngineService service in Settings.StartupServices)
            {
                Type serviceType = service.GetType();
                if (t.IsAssignableFrom(serviceType))
                    return service as T;
            }

            return null;
        }

        /// <summary>
        /// Gets whether a <see cref="EngineService"/> of the specified type is available. 
        /// </summary>
        /// <typeparam name="T">The type of engine service to check for availability.</typeparam>
        /// <returns></returns>
        public bool IsServiceAvailable<T>() where T : EngineService
        {
            return IsServiceAvailable(typeof(T));
        }

        /// <summary>
        /// Gets whether a <see cref="EngineService"/> of the specified type is available. 
        /// If a service state is not equal to <see cref="EngineServiceState.Running"/>, it is not considered as available.
        /// </summary>
        ///<param name="type">The <see cref="Type"/> of engine service to check for availability</param>
        /// <returns></returns>
        public bool IsServiceAvailable(Type type)
        {
            foreach (EngineService service in Settings.StartupServices)
            {
                Type serviceType = service.GetType();
                if (type.IsAssignableFrom(serviceType))
                    return service.State == EngineServiceState.Running || 
                        service.State == EngineServiceState.Ready;
            }

            return false;
        }

        /// <summary>
        /// Starts the engine and it's services.
        /// </summary>
        public void Start(Action<Timing> updateCallback)
        {
            foreach (EngineService service in _services)
                service.Start(Threading, Log);

            Fonts.Initialize();

            Content.Workers.IsPaused = false;

            _mainThread = Threading.CreateThread("engine", true, true, (timing) =>
            {
                Update(timing);
                updateCallback(timing);
            });
        }

        /// <summary>
        /// Stops the engine and it's services.
        /// </summary>
        public void Stop()
        {
            _mainThread?.Dispose();

            foreach (EngineService service in _services)
            {
                service.Stop();

                // Wait for the service thread to stop.
                while (service.State != EngineServiceState.Ready)
                {
                    if (service.Thread != null)
                    {
                        if (service.Thread.IsDisposed)
                            break;
                        Thread.Sleep(10);
                    }
                }
            }
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

        private void Update(Timing time)
        {
            while (_taskQueue.TryDequeue(out EngineTask task))
                task.Process(this, time);

            // Update services that are set to run on the main engine thread.
            foreach(EngineService service in _services)
            {
                if (service.ThreadMode == ThreadingMode.MainThread)
                    service.Update(time);
            }

            Scenes.Update(time);
        }

        /// <summary>
        /// Attempts to safely shutdown the engine before disposing of the current instance.
        /// </summary>
        public void Dispose()
        {
            if (IsDisposed)
                return;

            Log.Debug("Disposing of engine");

            Stop();

            foreach (EngineService service in _services)
                service.Dispose();

            Threading.Dispose();
            Renderer?.Dispose();
            Scenes.Dispose();
            Content.Dispose();
            Logger.DisposeAll();
            Settings.Save();
            Current = null;
            IsDisposed = true;
        }

        /// <summary>Gets the main <see cref="RenderService"/> attached to the current <see cref="Engine"/> instance.</summary>>
        public RenderService Renderer { get; private set; }

        /// <summary>Gets the main <see cref="NetworkService"/> attached to the current <see cref="Engine"/> instance.</summary>>
        public NetworkService Net { get; private set; }

        /// <summary>
        /// Gets the main <see cref="AudioService"/> attached to the current <see cref="Engine"/> instance.
        /// </summary>
        public AudioService Audio { get; private set; }

        /// <summary>Gets the log attached to the current <see cref="Engine"/> instance.</summary>
        internal Logger Log { get; }

        /// <summary>Gets the engine settings.</summary>
        public EngineSettings Settings { get; }

        /// <summary>Gets the thread manager bound to the engine.</summary>
        public ThreadManager Threading { get; }

        /// <summary>
        /// Gets the main <see cref="EngineThread"/> of the current <see cref="Engine"/> instance.
        /// Core/game update logic is usually done on this thread.
        /// </summary>
        public EngineThread MainThread => _mainThread;

        /// <summary>
        /// Gets the main content manager bound to the current engine instance. <para/>
        /// It is recommended that you use this to load assets that are unlikely to be unloaded throughout the lifetime of the current session. 
        /// You should use separate <see cref="ContentManager"/> instances for level-specific or short-lifespan content. 
        /// Disposing of a <see cref="ContentManager"/> instance will unload all of the content that was loaded by it.<para />
        /// </summary>
        public ContentManager Content { get; }

        /// <summary>Gets the <see cref="InputService"/> attached to the current <see cref="Engine"/> instance.</summary>
        public InputService Input { get; private set; }

        /// <summary>
        /// Gets the internal scene manager for the current <see cref="Engine"/> instance.
        /// </summary>
        internal SceneManager Scenes { get; }

        /// <summary>
        /// Gets the internal <see cref="SpriteFontManager"/> bound to the current <see cref="Engine"/> instance.
        /// </summary>
        internal SpriteFontManager Fonts { get; }

        /// <summary>
        /// Gets whether or not the current <see cref="Engine"/> instance has been disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }
    }
}
