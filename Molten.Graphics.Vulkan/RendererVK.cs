using Molten.Graphics.Dxc;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.GLFW;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Molten.Graphics.Vulkan;

public unsafe class RendererVK : RenderService
{
    internal unsafe delegate Result EnumerateCallback<T>(uint* count, T* info) where T : unmanaged;

    DisplayManagerVK _displayManager;
    InstanceLoaderVK _instanceLoader;
    Instance* _instance;
    DebugUtilsMessengerEXT* _debugMessengerHandle;
    SpirvCompiler _shaderCompiler;

    public RendererVK()
    {
        VK = Vk.GetApi();
        GLFW = Glfw.GetApi();
        GLFW.Init();
        ApiVersion = new VersionVK(0, 1, 1, 0);
        _instanceLoader = new InstanceLoaderVK(this);
        _displayManager = new DisplayManagerVK(this);
    }

    /// <summary>
    /// An extended version of <see cref="Vk.MakeVersion(uint, uint, uint)"/> which includes the variant bits.
    /// </summary>
    /// <param name="variant"></param>
    /// <param name="major"></param>
    /// <param name="minor"></param>
    /// <param name="patch"></param>
    /// <returns></returns>
    internal static uint MakeVersion(uint variant, uint major, uint minor, uint patch)
    {
        return (((variant) << 29) | ((major) << 22) | ((minor) << 12) | (patch));
    }

    internal bool HasExtension(string extName)
    {
        return _instanceLoader.HasExtension(extName);
    }

    protected override GraphicsManager OnInitializeDisplayManager(GraphicsSettings settings)
    {
        // TODO Store baseline profiles for each OS/platform where possible, or default to Moltens own.
        // For android see: https://developer.android.com/ndk/guides/graphics/android-baseline-profile

        if (settings.EnableDebugLayer.Value == true)
        {
            _instanceLoader.AddLayer("VK_LAYER_KHRONOS_validation");
            DebugLayer = _instanceLoader.AddExtension<ExtDebugUtils>(SetupDebugMessenger, (ext) =>
            {
                // Dispose of debug messenger handle.
                if (_debugMessengerHandle != null)
                {
                    ext.DestroyDebugUtilsMessenger(*_instance, *_debugMessengerHandle, null);
                    _debugMessengerHandle = null;
                }
            });
        }

        _instanceLoader.AddExtension<KhrSurface>();
        _instanceLoader.AddGlfwExtensions();

        _instance = EngineUtil.Alloc<Instance>();
        if (!_instanceLoader.Build(ApiVersion, _instance))
            Log.Error($"Failed to build new instance");

        return _displayManager;
    }

    protected override List<GraphicsDevice> OnInitializeDevices(GraphicsSettings settings, GraphicsManager manager)
    {
        // Initialize the primary device
        List<GraphicsDevice> result = new List<GraphicsDevice>();
        NativeDevice = _displayManager.PrimaryDevice as DeviceVK;
        NativeDevice.PreInitialize(CommandSetCapabilityFlags.Graphics);
        NativeDevice.AddExtension<KhrSwapchain>();

        if (NativeDevice.Initialize())
            result.Add(NativeDevice);

        // Initialize all secondary devices.
        foreach(GraphicsDevice device in _displayManager.Devices)
        {
            if (device == NativeDevice)
                continue;

            DeviceVK Device = device as DeviceVK;
            Device.PreInitialize(CommandSetCapabilityFlags.Graphics);
            Device.AddExtension<KhrSwapchain>();

            if (Device.Initialize())
                result.Add(Device);
        }

        return result;
    }

    protected override void OnInitializeRenderer(EngineSettings settings)
    {
        Assembly includeAssembly = GetType().Assembly;
        _shaderCompiler = new SpirvCompiler(VK, this, "\\Assets\\HLSL\\include\\", includeAssembly, SpirvCompileTarget.Vulkan1_1);
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate Bool32 DebugMessengerCallback(DebugUtilsMessageSeverityFlagsEXT messageSeverity,
        DebugUtilsMessageTypeFlagsEXT messageTypes,
        DebugUtilsMessengerCallbackDataEXT* pCallbackData, void* pUserData);

    private void SetupDebugMessenger(ExtDebugUtils ext)
    {
        // See: https://registry.khronos.org/vulkan/specs/1.3-extensions/man/html/PFN_vkDebugUtilsMessengerCallbackEXT.html
        DebugMessengerCallback debugMsgCallback = new DebugMessengerCallback((messageSeverity, messageTypes, pCallbackData, pUserData) =>
        {
            StructureType pCallbackType = pCallbackData->SType;
            string msg = SilkMarshal.PtrToString((nint)pCallbackData->PMessage, NativeStringEncoding.UTF8);
            string msgIDName = SilkMarshal.PtrToString((nint)pCallbackData->PMessageIdName, NativeStringEncoding.UTF8);
            Log.WriteLine($"[Validation:{messageSeverity}] ID: {msgIDName} - MSG: {msg}");

            // From Vulkan docs: The application should always return VK_FALSE. The VK_TRUE value is reserved for use in layer development.
            return false;
        });

        PfnVoidFunction pFunc = new PfnVoidFunction(debugMsgCallback);

        DebugUtilsMessengerCreateInfoEXT debugCreateInfo = new DebugUtilsMessengerCreateInfoEXT()
        {
            SType = StructureType.DebugUtilsMessengerCreateInfoExt,
            MessageSeverity = DebugUtilsMessageSeverityFlagsEXT.VerboseBitExt |
                            DebugUtilsMessageSeverityFlagsEXT.WarningBitExt |
                            DebugUtilsMessageSeverityFlagsEXT.ErrorBitExt |
                            DebugUtilsMessageSeverityFlagsEXT.InfoBitExt,
            MessageType = DebugUtilsMessageTypeFlagsEXT.GeneralBitExt |
                            DebugUtilsMessageTypeFlagsEXT.ValidationBitExt |
                DebugUtilsMessageTypeFlagsEXT.PerformanceBitExt,
            PfnUserCallback = *(PfnDebugUtilsMessengerCallbackEXT*)&pFunc,
            PUserData = null,
        };

        _debugMessengerHandle = EngineUtil.Alloc<DebugUtilsMessengerEXT>();
        Result r = ext.CreateDebugUtilsMessenger(*_instance, &debugCreateInfo, null, _debugMessengerHandle);
        if (!r.Check(this))
            EngineUtil.Free(ref _debugMessengerHandle);
    }

    internal unsafe T[] Enumerate<T>(EnumerateCallback<T> callback, string callbackName = "")
        where T : unmanaged
    {
        string GetCallbackName() => string.IsNullOrWhiteSpace(callbackName) ? callback.Method.Name : callbackName;

        uint count = 0;
        Result r = callback(&count, null);

        if (r.Check(this, () =>$"Enumerate: Failed to get {GetCallbackName} count"))
        {
            T* items = EngineUtil.AllocArray<T>(count);
            r = callback(&count, items);

            if (r.Check(this, () => $"Enumerate: Failed get {GetCallbackName} items"))
            {
                T[] result = new T[count];
                fixed (T* ptrResult = result)
                    System.Buffer.MemoryCopy(items, ptrResult, sizeof(T) * count, sizeof(T) * count);

                return result;
            }

            EngineUtil.Free(ref items);
        }

        return new T[0];
    }

    /// <summary>
    /// Gets an extension that was loaded for the <see cref="Instance"/>.
    /// </summary>
    /// <typeparam name="E">The type of extension to retrieve.</typeparam>
    /// <returns></returns>
    internal E GetInstanceExtension<E>()
        where E : NativeExtension<Vk>
    {
        return _instanceLoader.GetExtension<E>();
    }

    protected override void OnDisposeBeforeRender()
    {
        _shaderCompiler.Dispose();
        _displayManager.Dispose();
        _instanceLoader.Dispose();

        if(_instance != null)
        {
            VK.DestroyInstance(*_instance, null);
            EngineUtil.Free(ref _instance);
        }

        GLFW.Dispose();
        VK.Dispose();
    }

    /// <summary>
    /// Gets the underlying <see cref="Vk"/> API instance.
    /// </summary>
    internal Vk VK { get; }

    internal VulkanExtension<ExtDebugUtils> DebugLayer { get; private set; }

    /// <summary>
    /// Gets the underlying <see cref="Glfw"/> API instance.
    /// </summary>
    internal Glfw GLFW { get; }

    /// <summary>
    /// Gets the underlying <see cref="Silk.NET.Vulkan.Instance"/>.
    /// </summary>
    internal Instance* Instance => _instance;

    /// <summary>
    /// Gets the main <see cref="DeviceVK"/>.
    /// </summary>
    internal DeviceVK NativeDevice { get; private set; }

    /// <inheritdoc/>
    protected override DxcCompiler Compiler => _shaderCompiler;

    internal VersionVK ApiVersion { get; }
}
