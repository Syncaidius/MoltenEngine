using Silk.NET.Core.Native;
using Silk.NET.GLFW;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using Image = Silk.NET.Vulkan.Image;
using Semaphore = Silk.NET.Vulkan.Semaphore;

namespace Molten.Graphics.Vulkan
{
    internal unsafe class WindowSurfaceVK : RenderSurface2DVK, INativeSurface
    {
        struct BackBuffer
        {
            internal Image Image;

            internal ImageView View;

            internal BackBuffer(Image texture, ImageView view)
            {
                Image = texture;
                View = view;
            }
        }

        public event WindowSurfaceHandler OnHandleChanged;
        public event WindowSurfaceHandler OnParentChanged;
        public event WindowSurfaceHandler OnClose;
        public event WindowSurfaceHandler OnMinimize;
        public event WindowSurfaceHandler OnRestore;
        public event WindowSurfaceHandler OnFocusGained;
        public event WindowSurfaceHandler OnFocusLost;

        GraphicsQueueVK _presentQueue;
        WindowHandle* _window;
        PresentModeKHR _mode;
        SurfaceCapabilitiesKHR _cap;
        SwapchainKHR _swapChain;
        BackBuffer[] _backBuffer;

        string _title;
        uint _curChainSize;

        ColorSpaceKHR _colorSpace = ColorSpaceKHR.SpaceSrgbNonlinearKhr;

        KhrSwapchain _extSwapChain;
        KhrSurface _extSurface;
        FenceVK _frameFence;

        internal unsafe WindowSurfaceVK(DeviceVK device, string title, TextureDimensions dimensions,
            GraphicsResourceFlags flags = GraphicsResourceFlags.None,
            GraphicsFormat format = GraphicsFormat.B8G8R8A8_UNorm,
            PresentModeKHR presentMode = PresentModeKHR.MailboxKhr,
            string name = null) : 
            base(device, dimensions, AntiAliasLevel.None, MSAAQuality.Default, format, flags, false, name)
        {
            _title = title;
            _mode = presentMode;
            _frameFence = device.GetFence();
        }

        protected override unsafe void OnCreateImage(DeviceVK device, ResourceHandleVK* handle, ref ImageCreateInfo imgInfo, ref ImageViewCreateInfo viewInfo)
        {
            RendererVK renderer = Device.Renderer as RendererVK;

            _extSurface = _extSurface ?? renderer.GetInstanceExtension<KhrSurface>();
            if (_extSurface == null)
            {
                renderer.Log.Error($"VK_KHR_surface extension is unsupported. Unable to initialize WindowSurfaceVK");
                return;
            }

            _extSwapChain = _extSwapChain ?? device.GetExtension<KhrSwapchain>();
            if (_extSwapChain == null)
            {
                renderer.Log.Error($"VK_KHR_swapchain extension is unsupported. Unable to initialize WindowSurfaceVK");
                return;
            }

            Result r = CreateSurface(device, renderer, (int)Width, (int)Height);
            if (r != Result.Success)
                return;

            if (!IsFormatSupported(_extSurface, ResourceFormat, _colorSpace))
            {
                renderer.Log.Error($"Surface format '{ResourceFormat}' with a '{_colorSpace}' color-space is not supported");
                return;
            }

            _presentQueue = renderer.NativeDevice.FindPresentQueue(this);
            if (_presentQueue == null)
            {
                renderer.Log.Error($"No command queue found to present window surface");
                return;
            }

            _mode = ValidatePresentMode(_extSurface, _mode);
            ValidateBackBufferSize();

            r = CreateSwapChain();
            if (!r.Check(renderer, () => "Failed to create swapchain"))
                return;

            Image[] images = renderer.Enumerate<Image>((count, items) =>
            {
                return _extSwapChain.GetSwapchainImages(device, _swapChain, count, items);
            }, "Swapchain image");

            _backBuffer = new BackBuffer[images.Length];

            for (int i = 0; i < images.Length; i++)
            {
                _backBuffer[i].Image = images[i];
                viewInfo.Image = images[i];
                r = renderer.VK.CreateImageView(device, viewInfo, null, out _backBuffer[i].View);
                if (!r.Check(device, () => $"Failed to create image view for back-buffer image {i}"))
                    break;
            }
        }

        private Result CreateSurface(DeviceVK device, RendererVK renderer, int width, int height)
        {
            // Dispose of old surface
            if (Native.Handle != 0)
            {
                renderer.GLFW.DestroyWindow(_window);
                _extSurface.DestroySurface(*renderer.Instance, Native, null);

                _window = null;
                Native = new SurfaceKHR();
            }

            renderer.GLFW.WindowHint(WindowHintClientApi.ClientApi, ClientApi.NoApi);
            renderer.GLFW.WindowHint(WindowHintBool.Resizable, true);
            _window = renderer.GLFW.CreateWindow(width, height, _title, null, null);

            if (_window == null)
            {
                renderer.Log.Error($"Failed to create {width} x {height} window for WindowSurfaceVK");
                return Result.ErrorInitializationFailed;
            }

            VkHandle instanceHandle = new VkHandle(renderer.Instance->Handle);
            VkNonDispatchableHandle surfaceHandle = new VkNonDispatchableHandle();

            Result r = (Result)renderer.GLFW.CreateWindowSurface(instanceHandle, _window, null, &surfaceHandle);
            if (!r.Check(renderer))
                return r;

            Native = new SurfaceKHR(surfaceHandle.Handle);

            // Retrieve/update surface capabilities
            r = _extSurface.GetPhysicalDeviceSurfaceCapabilities(device.Adapter, Native, out _cap);
            r.Check(renderer);

            return r;
        }

        private Result CreateSwapChain()
        {
            DeviceVK device = Device as DeviceVK;
            SwapchainCreateInfoKHR createInfo = new SwapchainCreateInfoKHR()
            {
                SType = StructureType.SwapchainCreateInfoKhr,
                Surface = Native,
                MinImageCount = _curChainSize,
                ImageFormat = ResourceFormat.ToApi(),
                ImageColorSpace = _colorSpace,
                ImageExtent = new Extent2D(Width, Height),
                ImageArrayLayers = 1,
                ImageUsage = ImageUsageFlags.ColorAttachmentBit,
            };

            // Detect swap-chain sharing mode.
            (createInfo.ImageSharingMode, GraphicsQueueVK[] sharingWith) = device.GetSharingMode(device.Queue, _presentQueue);
            uint* familyIndices = stackalloc uint[sharingWith.Length];

            for (int i = 0; i < sharingWith.Length; i++)
                familyIndices[i] = sharingWith[i].FamilyIndex;

            createInfo.QueueFamilyIndexCount = (uint)sharingWith.Length;
            createInfo.PQueueFamilyIndices = familyIndices;
            createInfo.PreTransform = _cap.CurrentTransform;
            createInfo.CompositeAlpha = CompositeAlphaFlagsKHR.OpaqueBitKhr;
            createInfo.PresentMode = _mode;
            createInfo.Clipped = true;
            createInfo.OldSwapchain = _swapChain;

            return _extSwapChain.CreateSwapchain(device, &createInfo, null, out _swapChain);
        }

        private bool IsFormatSupported(KhrSurface extSurface, GraphicsFormat format, ColorSpaceKHR colorSpace)
        {
            DeviceVK device = Device as DeviceVK;
            SurfaceFormatKHR[] supportedFormats = (Device.Renderer as RendererVK).Enumerate<SurfaceFormatKHR>((count, items) =>
            {
                return extSurface.GetPhysicalDeviceSurfaceFormats(device, Native, count, items);
            }, "surface format");

            Format vkFormat = format.ToApi();

            for(int i = 0; i < supportedFormats.Length; i++)
            {
                if (supportedFormats[i].Format == vkFormat && supportedFormats[i].ColorSpace == colorSpace)
                    return true;
            }

            return false;
        }

        private PresentModeKHR ValidatePresentMode(KhrSurface extSurface, PresentModeKHR requested)
        {
            DeviceVK device = Device as DeviceVK;
            PresentModeKHR[] supportedModes = (Device.Renderer as RendererVK).Enumerate<PresentModeKHR>((count, items) =>
            {
                return extSurface.GetPhysicalDeviceSurfacePresentModes(device.Adapter, Native, count, items);
            }, "present mode");

            for (int i = 0; i < supportedModes.Length; i++)
            {
                if (supportedModes[i] == requested)
                    return requested;
            }

            return PresentModeKHR.FifoKhr;
        }

        private void ValidateBackBufferSize()
        {
            BackBufferMode mode = Device.Renderer.Settings.Graphics.BufferingMode;
            if (mode == BackBufferMode.Default)
                _curChainSize = _cap.MinImageCount + 1;
            else
                _curChainSize = (uint)mode;

            if (_cap.MaxImageCount > 0)
                _curChainSize = uint.Clamp(_curChainSize, _cap.MinImageCount, _cap.MaxImageCount);
            else
                _curChainSize = uint.Max(_cap.MinImageCount, _curChainSize);
        }

        protected override void OnGraphicsRelease()
        {
            DeviceVK device = Device as DeviceVK;
            device.FreeFence(_frameFence);

            if (_swapChain.Handle != 0)
            {
                // Clean up image view handles
                for (int i = 0; i < _backBuffer.Length; i++)
                    (Device.Renderer as RendererVK).VK.DestroyImageView(device, _backBuffer[i].View, null);

                KhrSwapchain extSwapchain = device.GetExtension<KhrSwapchain>();
                extSwapchain?.DestroySwapchain(device, _swapChain, null);
            }

            KhrSurface extSurface = (Device.Renderer as RendererVK).GetInstanceExtension<KhrSurface>();
            extSurface?.DestroySurface(*(Device.Renderer as RendererVK).Instance, Native, null);

            if (_window != null)
                (Device.Renderer as RendererVK).GLFW.DestroyWindow(_window);

            base.OnGraphicsRelease();
        }

        public void Present()
        {
            OnApply(Device.Queue);

            if (_swapChain.Handle == 0)
                return;

            DeviceVK device = Device as DeviceVK;

            uint imageIndex = 0;
            Result r = _extSwapChain.AcquireNextImage(device, _swapChain, ulong.MaxValue, new Semaphore(), _frameFence, &imageIndex);
            if (!r.Check(device, () => "Failed to acquire next swapchain image"))
                return;

            if (!_frameFence.Wait())
                return;

            ResourceHandleVK* rHandle = (ResourceHandleVK*)Handle;
            rHandle->SetValue(_backBuffer[imageIndex].Image);

            int swapChainCount = 1; // TODO expand once we support multiple swapchains. May need moving to DeviceVK/GraphicsDevice.
            SwapchainKHR* swapChains = stackalloc SwapchainKHR[] { _swapChain };
            uint* scIndices = stackalloc uint[] { imageIndex }; // Back-buffer index for each presented swap-chain.
            Result* pResults = stackalloc Result[swapChainCount];

            // Get the last semaphore of each command list branch in the current frame.
            // These semaphores will be waited on before presenting the swap-chain.
            uint semaphoreCount = Device.Renderer.Frame.BranchCount;
            Semaphore* semaphores = stackalloc Semaphore[(int)semaphoreCount];
            for (uint i = 0; i < semaphoreCount; i++)
            {
                CommandListVK vkCmd = Device.Renderer.Frame[i] as CommandListVK;
                semaphores[i] = vkCmd.Semaphore.Ptr;
            }

            PresentInfoKHR presentInfoKHR = new PresentInfoKHR()
            {
                SType = StructureType.PresentInfoKhr,
                WaitSemaphoreCount = semaphoreCount,
                PWaitSemaphores = semaphores,
                SwapchainCount = (uint)swapChainCount,
                PSwapchains = swapChains,
                PImageIndices = scIndices,
                PResults = pResults,
            };

            Transition(device.Queue, ImageLayout.Undefined, ImageLayout.PresentSrcKhr);

            Queue cmdQueue = device.Queue.Native;
            r = _extSwapChain.QueuePresent(cmdQueue, &presentInfoKHR);
            if (!r.Check(device, () => $"Failed to present {swapChainCount} swapchains."))
            {
                for (int i = 0; i < swapChainCount; i++)
                    pResults[i].Check(device, () => $"\tSwapchain {i} failed to present.");
            }
        }

        public void Dispatch(Action callback)
        {
            throw new NotImplementedException();
        }

        public void Resize(GraphicsPriority priority, uint newWidth, uint newHeight)
        {
            throw new NotImplementedException();
        }

        public void Resize(GraphicsPriority priority, uint newWidth, uint newHeight, uint newMipMapCount, uint newArraySize, GraphicsFormat newFormat)
        {
            throw new NotImplementedException();
        }

        public void GenerateMipMaps(GraphicsPriority priority, Action<GraphicsResource> completionCallback = null)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(GraphicsPriority priority, GraphicsTexture destination, Action<GraphicsResource> completeCallback = null)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(GraphicsPriority priority, uint sourceLevel, uint sourceSlice,
            GraphicsTexture destination, uint destLevel, uint destSlice, 
            Action<GraphicsResource> completeCallback = null)
        {
            throw new NotImplementedException();
        }

        public void SetData(GraphicsPriority priority, TextureData data, uint srcMipIndex, uint srcArraySlice, uint mipCount, uint arrayCount, uint destMipIndex = 0, uint destArraySlice = 0, Action<GraphicsResource> completeCallback = null)
        {
            throw new NotImplementedException();
        }

        public void SetData<T>(GraphicsPriority priority, uint level, T[] data, uint startIndex, uint count, uint pitch, uint arraySlice = 0, Action<GraphicsResource> completeCallback = null) where T : unmanaged
        {
            throw new NotImplementedException();
        }

        public void SetData(GraphicsPriority priority, TextureSlice data, uint mipLevel, uint arraySlice, Action<GraphicsResource> completeCallback = null)
        {
            throw new NotImplementedException();
        }

        public void SetData<T>(GraphicsPriority priority, RectangleUI area, T[] data, uint bytesPerPixel, uint level, uint arrayIndex = 0, Action<GraphicsResource> completeCallback = null) where T : unmanaged
        {
            throw new NotImplementedException();
        }

        public void GetData(GraphicsPriority priority, GraphicsTexture stagingTexture, Action<TextureData> completeCallback = null)
        {
            throw new NotImplementedException();
        }

        public void GetData(GraphicsPriority priority, GraphicsTexture stagingTexture, uint level, uint arrayIndex, Action<TextureSlice> completeCallback = null)
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            if(_window != null)
                (Device.Renderer as RendererVK).GLFW.SetWindowShouldClose(_window, true);
        }

        public bool IsFocused => throw new NotImplementedException();

        public WindowMode Mode { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public nint? ParentHandle { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public nint? WindowHandle => (nint)_window;

        public Rectangle RenderBounds => throw new NotImplementedException();

        /// <inheritdoc/>
        public string Title
        {
            get => _title;
            set
            {
                if (_window != null)
                    (Device.Renderer as RendererVK).GLFW.SetWindowTitle(_window, _title);
            }
        }
        public bool IsVisible { get; set; }

        internal SurfaceKHR Native { get; private set; }
    }
}
