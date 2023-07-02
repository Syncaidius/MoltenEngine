using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Molten.UI;
using Silk.NET.Core.Native;
using Silk.NET.GLFW;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using Image = Silk.NET.Vulkan.Image;

namespace Molten.Graphics.Vulkan
{
    public abstract class SwapChainSurfaceVK : RenderSurface2DVK, INativeSurface
    {
        public event WindowSurfaceHandler OnHandleChanged;
        public event WindowSurfaceHandler OnParentChanged;
        public event WindowSurfaceHandler OnClose;
        public event WindowSurfaceHandler OnMaximize;
        public event WindowSurfaceHandler OnMinimize;
        public event WindowSurfaceHandler OnRestore;
        public event WindowSurfaceHandler OnFocusGained;
        public event WindowSurfaceHandler OnFocusLost;

        SwapchainKHR _swapChain;
        SurfaceCapabilitiesKHR _cap;
        PresentModeKHR _mode;

        GraphicsQueueVK _presentQueue;
        KhrSwapchain _extSwapChain;
        KhrSurface _extSurface;
        SurfaceFormatKHR _surfaceFormat;
        uint _curChainSize;

        protected SwapChainSurfaceVK(DeviceVK device, string title, uint width, uint height, uint mipCount,
            GraphicsResourceFlags flags = GraphicsResourceFlags.None,
            GraphicsFormat format = GraphicsFormat.B8G8R8A8_UNorm,
            PresentModeKHR presentMode = PresentModeKHR.ImmediateKhr,
            string name = null) :
            base(device, width, height, mipCount, 1, AntiAliasLevel.None, MSAAQuality.Default, format, flags, false, name)
        {
            _mode = presentMode;
            FrameFence = device.GetFence();
        }

        private unsafe SurfaceFormatKHR GetPreferredFormat(KhrSurface extSurface, GraphicsFormat preferredFormat, ColorSpaceKHR preferredColorSpace)
        {
            DeviceVK device = Device as DeviceVK;
            Format pFormat = preferredFormat.ToApi();

            // Retrieve list of all formats supported by the current DeviceVK.
            SurfaceFormatKHR[] supportedFormats = (Device.Renderer as RendererVK).Enumerate<SurfaceFormatKHR>((count, items) =>
            {
                return extSurface.GetPhysicalDeviceSurfaceFormats(device, SurfaceHandle, count, items);
            }, "surface format");


            SurfaceFormatKHR bestFormat = supportedFormats[0];
            foreach (SurfaceFormatKHR sf in supportedFormats)
            {
                if (pFormat == sf.Format)
                {
                    // Take the first format that matches our preferred one.
                    // If the format already matches, we'll only accept an updated one if it also has our preferred colorspace.
                    if (bestFormat.Format != sf.Format)
                        bestFormat = sf;
                    else if (sf.ColorSpace == preferredColorSpace)
                        bestFormat = sf;
                }
            }

            return bestFormat;
        }

        private unsafe PresentModeKHR ValidatePresentMode(KhrSurface extSurface, PresentModeKHR requested)
        {
            DeviceVK device = Device as DeviceVK;
            PresentModeKHR[] supportedModes = (Device.Renderer as RendererVK).Enumerate<PresentModeKHR>((count, items) =>
            {
                return extSurface.GetPhysicalDeviceSurfacePresentModes(device.Adapter, SurfaceHandle, count, items);
            }, "present mode");

            for (int i = 0; i < supportedModes.Length; i++)
            {
                if (supportedModes[i] == requested)
                    return requested;
            }

            return PresentModeKHR.FifoKhr;
        }

        protected unsafe override void CreateImages(DeviceVK device, ResourceHandleVK<Image, ImageHandleVK>[] handles, MemoryPropertyFlags memFlags, ref ImageCreateInfo imgInfo, ref ImageViewCreateInfo viewInfo)
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

            _surfaceFormat = GetPreferredFormat(_extSurface, ResourceFormat, ColorSpaceKHR.SpaceSrgbNonlinearKhr);
            _presentQueue = renderer.NativeDevice.FindPresentQueue(this);
            imgInfo.Format = _surfaceFormat.Format;
            ResourceFormat = imgInfo.Format.FromApi();

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

            Image[] scImages = renderer.Enumerate<Image>((count, items) =>
            {
                return _extSwapChain.GetSwapchainImages(device, _swapChain, count, items);
            }, "Swapchain image");

            if (scImages.Length != handles.Length)
                throw new InvalidOperationException("The number of swap-chain images did not match the current buffering mode.");

            for (int i = 0; i < handles.Length; i++)
            {
                handles[i].NativePtr[0] = scImages[i];
                viewInfo.Image = scImages[i];
                r = renderer.VK.CreateImageView(device, viewInfo, null, handles[i].SubHandle.ViewPtr);
                if (!r.Check(device, () => $"Failed to create image view for back-buffer image {i}"))
                    break;
            }
        }

        private unsafe Result CreateSurface(DeviceVK device, RendererVK renderer, int width, int height)
        {
            // Dispose of old surface
            if (SurfaceHandle.Handle != 0)
            {
                OnDestroySurface(renderer);
                _extSurface.DestroySurface(*renderer.Instance, SurfaceHandle, null);
                SurfaceHandle = new SurfaceKHR();
            }

            Result r = OnCreateSurface(renderer, width, height, out VkNonDispatchableHandle resultHandle);
            if (!r.Check(renderer, () => "Failed to create surface"))
                return r;

            if (resultHandle.Handle == 0)
            {
                renderer.Log.Error("Surface handle is invalid or null.");
                return Result.ErrorInvalidExternalHandle;
            }

            SurfaceHandle = new SurfaceKHR(resultHandle.Handle);

            // Retrieve/update surface capabilities
            r = _extSurface.GetPhysicalDeviceSurfaceCapabilities(device.Adapter, SurfaceHandle, out _cap);
            r.Check(renderer);

            return r;
        }

        protected abstract Result OnCreateSurface(RendererVK renderer, int width, int height, out VkNonDispatchableHandle result);

        protected abstract void OnDestroySurface(RendererVK renderer);

        private unsafe Result CreateSwapChain()
        {
            DeviceVK device = Device as DeviceVK;
            SwapchainCreateInfoKHR createInfo = new SwapchainCreateInfoKHR()
            {
                SType = StructureType.SwapchainCreateInfoKhr,
                Surface = SurfaceHandle,
                MinImageCount = _curChainSize,
                ImageFormat = _surfaceFormat.Format,
                ImageColorSpace = _surfaceFormat.ColorSpace,
                ImageExtent = new Extent2D(Width, Height),
                ImageArrayLayers = ArraySize,
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

        protected override sealed void OnNextFrame(GraphicsQueue queue, uint imageIndex, ulong frameID)
        {
            // We don't want to do anything here as handle-switching happens as part of presentation.
        }

        internal void Prepare(GraphicsQueueVK queue, uint imageIndex)
        {
            SetHandle(imageIndex);
            Transition(queue, ImageLayout.Undefined, ImageLayout.PresentSrcKhr);
        }

        private void ValidateBackBufferSize()
        {
            FrameBufferMode mode = Device.Renderer.Settings.Graphics.FrameBufferMode;
            if (mode == FrameBufferMode.Default)
                _curChainSize = _cap.MinImageCount + 1;
            else
                _curChainSize = (uint)mode;

            if (_cap.MaxImageCount > 0)
                _curChainSize = uint.Clamp(_curChainSize, _cap.MinImageCount, _cap.MaxImageCount);
            else
                _curChainSize = uint.Max(_cap.MinImageCount, _curChainSize);
        }

        protected unsafe override void OnGraphicsRelease()
        {
            DeviceVK device = Device as DeviceVK;
            device.FreeFence(FrameFence);
            FrameFence = null;

            base.OnGraphicsRelease();

            if (_swapChain.Handle != 0)
                _extSwapChain.DestroySwapchain(device, _swapChain, null);

            _extSurface.DestroySurface(*(Device.Renderer as RendererVK).Instance, SurfaceHandle, null);
        }


        public abstract void Close();

        public abstract void Dispatch(Action callback);

        internal SwapchainKHR SwapchainHandle => _swapChain;

        internal FenceVK FrameFence { get; private set; }

        public bool IsEnabled { get; set; }

        internal SurfaceKHR SurfaceHandle { get; private set; }

        public Rectangle RenderBounds => throw new NotImplementedException();

        public nint? ParentHandle { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public WindowMode Mode { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public abstract nint? WindowHandle { get; }

        public abstract string Title { get; set; }

        public abstract bool IsVisible { get; set; }

        public abstract bool IsFocused { get; }
    }
}
