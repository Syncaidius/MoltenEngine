using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.GLFW;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;

namespace Molten.Graphics
{
    internal unsafe class WindowSurfaceVK : NativeObjectVK<SurfaceKHR>, INativeSurface
    {
        public event WindowSurfaceHandler OnHandleChanged;
        public event WindowSurfaceHandler OnParentChanged;
        public event WindowSurfaceHandler OnClose;
        public event WindowSurfaceHandler OnMinimize;
        public event WindowSurfaceHandler OnRestore;
        public event WindowSurfaceHandler OnFocusGained;
        public event WindowSurfaceHandler OnFocusLost;
        public event TextureHandler OnResize;

        CommandQueueVK _presentQueue;
        WindowHandle* _window;

        string _title;
        uint _width;
        uint _height;

        internal unsafe WindowSurfaceVK(RendererVK renderer, string title, uint width, uint height)
        {
            Renderer = renderer;
            _title = title;

            renderer.GLFW.WindowHint(WindowHintClientApi.ClientApi, ClientApi.NoApi);
            renderer.GLFW.WindowHint(WindowHintBool.Resizable, true);
            _window = renderer.GLFW.CreateWindow((int)width, (int)height, _title, null, null);

            VkHandle instanceHandle = new VkHandle(renderer.Instance.Ptr->Handle);
            VkNonDispatchableHandle surfaceHandle = new VkNonDispatchableHandle();

            Result r = (Result)renderer.GLFW.CreateWindowSurface(instanceHandle, _window, null, &surfaceHandle);
            if (!renderer.CheckResult(r))
                return;

            Native = new SurfaceKHR(surfaceHandle.Handle);
            _presentQueue = renderer.Device.FindPresentQueue(this);

            if (_presentQueue == null)
                renderer.Log.Error($"No command queue found to present window surface");
        }

        protected override void OnDispose()
        {
            KhrSurface extSurface = Renderer.Device.GetExtension<KhrSurface>();
            extSurface?.DestroySurface(*Renderer.Instance.Ptr, Native, null);

            if (_window != null)
                Renderer.GLFW.DestroyWindow(_window);
        }

        public void Present()
        {
            throw new NotImplementedException();
        }

        public void Dispatch(Action callback)
        {
            throw new NotImplementedException();
        }

        public Texture2DProperties Get2DProperties()
        {
            throw new NotImplementedException();
        }

        public void Resize(uint newWidth, uint newHeight, uint newMipMapCount = 0, uint newArraySize = 0, GraphicsFormat newFormat = GraphicsFormat.Unknown)
        {
            throw new NotImplementedException();
        }

        public Texture1DProperties Get1DProperties()
        {
            throw new NotImplementedException();
        }

        public void Resize(uint newWidth, uint newMipMapCount, GraphicsFormat format)
        {
            throw new NotImplementedException();
        }

        public void Resize(uint newWidth)
        {
            throw new NotImplementedException();
        }

        public void GenerateMipMaps()
        {
            throw new NotImplementedException();
        }

        public void CopyTo(ITexture destination)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(uint sourceLevel, uint sourceSlice, ITexture destination, uint destLevel, uint destSlice)
        {
            throw new NotImplementedException();
        }

        public void SetData(TextureData data, uint srcMipIndex, uint srcArraySlice, uint mipCount, uint arrayCount, uint destMipIndex = 0, uint destArraySlice = 0)
        {
            throw new NotImplementedException();
        }

        public void SetData<T>(uint level, T[] data, uint startIndex, uint count, uint pitch, uint arraySlice = 0) where T : unmanaged
        {
            throw new NotImplementedException();
        }

        public void SetData(TextureSlice data, uint mipLevel, uint arraySlice)
        {
            throw new NotImplementedException();
        }

        public void SetData<T>(RectangleUI area, T[] data, uint bytesPerPixel, uint level, uint arrayIndex = 0) where T : unmanaged
        {
            throw new NotImplementedException();
        }

        public void GetData(ITexture stagingTexture, Action<TextureData> callback)
        {
            throw new NotImplementedException();
        }

        public void GetData(ITexture stagingTexture, uint level, uint arrayIndex, Action<TextureSlice> callback)
        {
            throw new NotImplementedException();
        }

        public bool HasFlags(TextureFlags flags)
        {
            throw new NotImplementedException();
        }

        public void Clear(Color color)
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            if(_window != null)
                Renderer.GLFW.SetWindowShouldClose(_window, true);
        }

        /// <summary>
        /// Gets the <see cref="RendererVK"/> instance that the current <see cref="WindowSurfaceVK"/> is bound to.
        /// </summary>
        public RendererVK Renderer { get; }

        public bool IsFocused => throw new NotImplementedException();

        public WindowMode Mode { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public nint Handle => (nint)_window;

        public nint? ParentHandle { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public nint? WindowHandle => throw new NotImplementedException();

        public ViewportF Viewport => throw new NotImplementedException();

        public TextureFlags Flags => throw new NotImplementedException();

        public GraphicsFormat DataFormat => throw new NotImplementedException();

        public bool IsBlockCompressed => throw new NotImplementedException();

        public uint Width => _width;

        public uint Height => _height;

        public uint MipMapCount => throw new NotImplementedException();

        public uint ArraySize => throw new NotImplementedException();

        public AntiAliasLevel MultiSampleLevel => throw new NotImplementedException();

        public bool IsMultisampled => throw new NotImplementedException();

        RenderService ITexture.Renderer => throw new NotImplementedException();

        public Rectangle RenderBounds => throw new NotImplementedException();

        /// <inheritdoc/>
        public string Title
        {
            get => _title;
            set
            {
                if (_window != null)
                    Renderer.GLFW.SetWindowTitle(_window, _title);
            }
        }
        public bool IsVisible { get; set; }
    }
}
