﻿using Molten.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>
    /// A base class that custom renderer implementations must inherit in order to be compatible with Molten engine, as it provides basic functionality for interacting with the rest of the engine.
    /// </summary>
    public abstract class RenderEngine : IDisposable
    {
        public static readonly Matrix4F DefaultView3D = Matrix4F.LookAtLH(new Vector3F(0, 0, -5), new Vector3F(0, 0, 0), Vector3F.UnitY);

        bool _surfaceResizeRequired;
        AntiAliasMode _requestedMultiSampleLevel = AntiAliasMode.None;
        internal AntiAliasMode MsaaLevel = AntiAliasMode.None;

        /// <summary>
        /// Creates a new instance of a <see cref="RenderEngine"/> sub-type.
        /// </summary>
        public RenderEngine()
        {
            Log = Logger.Get();
            Log.AddOutput(new LogFileWriter($"renderer_{Name.Replace(' ', '_')}" + "{0}.txt"));
        }

        public void InitializeAdapter(GraphicsSettings settings)
        {
            OnInitializeAdapter(settings);
        }

        public void Initialize(GraphicsSettings settings)
        {
            settings.Log(Log, "Graphics");
            MsaaLevel = _requestedMultiSampleLevel = MsaaLevel;
            settings.MSAA.OnChanged += MSAA_OnChanged;

            OnInitialize(settings);
        }

        /// <summary>
        /// Occurs when a graphics adapter needs to be acquired, before the renderer is initialized.
        /// </summary>
        /// <param name="settings"></param>
        protected abstract void OnInitializeAdapter(GraphicsSettings settings);

        /// <summary>
        /// Occurs when the renderer is being initialized.
        /// </summary>
        /// <param name="settings"></param>
        protected abstract void OnInitialize(GraphicsSettings settings);

        private void MSAA_OnChanged(AntiAliasMode oldValue, AntiAliasMode newValue)
        {
            _requestedMultiSampleLevel = newValue;
        }

        public SceneRenderData CreateRenderData()
        {
            SceneRenderData rd = OnCreateRenderData();
            RendererAddScene task = RendererAddScene.Get();
            task.Data = rd;
            PushTask(task);
            return rd;
        }

        protected abstract SceneRenderData OnCreateRenderData();

        public void DestroyRenderData(SceneRenderData data)
        {
            RendererRemoveScene task = RendererRemoveScene.Get();
            task.Data = data as SceneRenderData;
            PushTask(task);
        }

        public void PushTask(RendererTask task)
        {
            Tasks.Enqueue(task);
        }

        public void Present(Timing time)
        {
            Profiler.StartCapture();
            OnPrePresent(time);

            if (_requestedMultiSampleLevel != MsaaLevel)
            {
                // TODO re-create all internal surfaces/textures to match the new sample level.
                // TODO adjust rasterizer mode accordingly (multisample enabled/disabled).
                MsaaLevel = _requestedMultiSampleLevel;
                _surfaceResizeRequired = true;
            }

            // Perform all queued tasks before proceeding
            RendererTask task = null;
            while (Tasks.TryDequeue(out task))
                task.Process(this);

            // Perform preliminary checks on active scene data.
            // Also ensure the backbuffer is always big enough for the largest scene render surface.
            foreach (SceneRenderData data in Scenes)
            {
                data.ProcessChanges();

                foreach (RenderCamera camera in data.Cameras)
                {
                    camera.Skip = false;

                    if (camera.OutputSurface == null)
                    {
                        camera.Skip = true;
                        continue;
                    }

                    if (camera.OutputSurface.Width > BiggestWidth)
                    {
                        _surfaceResizeRequired = true;
                        BiggestWidth = camera.OutputSurface.Width;
                    }

                    if (camera.OutputSurface.Height > BiggestHeight)
                    {
                        _surfaceResizeRequired = true;
                        BiggestHeight = camera.OutputSurface.Height;
                    }
                }
            }

            // Update surfaces if dirty. This may involve resizing or changing their format.
            if (_surfaceResizeRequired)
            {
                OnRebuildSurfaces(BiggestWidth, BiggestHeight);
                _surfaceResizeRequired = false;
            }

            OnPresent(time);

            // Present all output surfaces
            OutputSurfaces.ForInterlock(0, 1, (index, surface) =>
            {
                surface.Present();
                return false;
            });

            Profiler.EndCapture(time);
            OnPostPresent(time);

            /* TODO: 
             *  Procedure:
             *      1) Calculate:
             *          a) Capture the current transform matrix of each object in the render tree
             *          b) Calculate the distance from the scene camera. Store on RenderData
             *          
             *      2) Sort objects by distance from camera:
             *          a) Sort objects into buckets inside RenderTree, front-to-back (reduce overdraw by drawing closest first).
             *          b) Only re-sort a bucket when objects are added or the camera moves
             *          c) While sorting, build up separate bucket list of objects with a transparent material sorted back-to-front (for alpha to work)
             *          
             *  Extras:
             *      3) Reduce z-sorting needed in (2) by adding scene-graph culling (quad-tree, octree or BSP) later down the line.
             *      4) Reduce (3) further by frustum culling the graph-culling results
             * 
             *  NOTES:
                    - when SceneObject.IsVisible is changed, queue an Add or Remove operation on the RenderTree depending on visibility. This will remove it from culling/sorting.
             */
        }

        /// <summary>
        /// Occurs when the render engine detects changes which usually require render surfaces to be rebuilt, such as the game window being resized, or certain graphics settings being changed.
        /// </summary>
        /// <param name="requiredWidth">The new width required by the render engine.</param>
        /// <param name="requiredHeight">The new height required by the render engine.</param>
        protected abstract void OnRebuildSurfaces(int requiredWidth, int requiredHeight);
        
        /// <summary>
        /// Occurs before the render engine begins rendering all of the active scenes to be output to the user.
        /// </summary>
        /// <param name="time">A timing instance.</param>
        protected abstract void OnPrePresent(Timing time);

        /// <summary>
        /// Occurs when the render engine is supposed to produces a result to be displayed to the user. This usually involves rendering one or more scenes to various render surfaces.
        /// </summary>
        /// <param name="time">A timing instance.</param>
        protected abstract void OnPresent(Timing time);

        /// <summary>
        /// Occurs after render presentation is completed and profiler timing has been finalized for the current frame. Useful if you need to do some per-frame cleanup/resetting.
        /// </summary>
        /// <param name="time">A timing instance.</param>
        protected abstract void OnPostPresent(Timing time);

        public void Dispose()
        {
            OutputSurfaces.ForInterlock(0, 1, (index, surface) =>
            {
                surface.Dispose();
                return false;
            });

            OnDispose();
            Log.Dispose();
        }

        /// <summary>
        /// Occurs when the current <see cref="RenderEngine"/> instance/implementation is being disposed.
        /// </summary>
        protected abstract void OnDispose();

        /// <summary>
        /// Gets profiling data attached to the renderer.
        /// </summary>
        public RenderProfiler Profiler { get; } = new RenderProfiler();

        /// <summary>
        /// Gets the display manager bound to the renderer.
        /// </summary>
        public abstract IDisplayManager DisplayManager { get; }

        /// <summary>
        /// Gets the resource manager bound to the renderer.
        /// This is responsible for creating and destroying graphics resources, such as buffers, textures and surfaces.
        /// </summary>
        public abstract IResourceManager Resources { get; }

        public abstract IComputeManager Compute { get; }

        /// <summary>
        /// Gets the name of the renderer implementation.
        /// </summary>
        public abstract string Name { get; }

        public ThreadedList<ISwapChainSurface> OutputSurfaces { get; } = new ThreadedList<ISwapChainSurface>();

        protected internal List<SceneRenderData> Scenes { get; }  = new List<SceneRenderData>();

        private ThreadedQueue<RendererTask> Tasks { get; } = new ThreadedQueue<RendererTask>();

        protected int BiggestWidth { get; private set; } = 1;

        protected int BiggestHeight { get; private set; } = 1;

        protected internal Logger Log { get; }
    }
}
