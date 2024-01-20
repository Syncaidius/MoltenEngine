using Silk.NET.Core.Attributes;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using System.Reflection;

namespace Molten.Graphics.Vulkan;

internal unsafe abstract class ExtensionLoaderVK<D> : EngineObject
    where D : unmanaged
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="info"></param>
    /// <returns>True if the instance layer/extension is valid.</returns>
    private unsafe delegate PropertyInfo EnumerateInstanceRetrieveCallback<T>(T* info) where T : unmanaged;

    internal struct PropertyInfo
    {
        internal VersionVK ImplementationVersion;

        internal VersionVK SpecVersion;

        internal string Description;

        internal bool Loaded;
    }

    protected unsafe class TempData
    {
        internal byte** LayerNames;

        internal byte** ExtensionNames;
    }

    ExtensionBinding _bind;

    internal ExtensionLoaderVK(RendererVK renderer)
    {
        Renderer = renderer;
        _bind = new ExtensionBinding();
    }

    protected abstract bool LoadExtensionModule(RendererVK renderer, VulkanExtension ext, D* obj);

    protected unsafe abstract Result OnBuild(RendererVK renderer, VersionVK apiVersion, TempData tmp, ExtensionBinding binding, D* obj);

    protected abstract Result GetLayers(uint* count, LayerProperties* items);

    protected abstract Result GetExtensions(uint* count, ExtensionProperties* items);

    private string GetNativeExtensionName<E>()
        where E : NativeExtension<Vk>
    {
        Type t = typeof(E);
        ExtensionAttribute attExtension = t.GetCustomAttribute<ExtensionAttribute>();

        if (attExtension != null)
            return attExtension.Name;
        else
            return t.Name;
    }

    /// <summary>
    /// Loads a module-based Vulkan extension, which provides application-level access to extended functionality.
    /// </summary>
    /// <typeparam name="E">The type of extension module to load.</typeparam>
    /// <param name="loadCallback">A callback to invoke when the module is loaded.</param>
    /// <param name="destroyCallback">A callback to invoke when the module is disposed and destroyed.</param>
    internal VulkanExtension<E> AddExtension<E>(Action<E> loadCallback = null, Action<E> destroyCallback = null)
        where E : NativeExtension<Vk>
    {
        VulkanExtension<E> ext = new VulkanExtension<E>(loadCallback, destroyCallback);
        string extName = GetNativeExtensionName<E>();
        if(!_bind.Extensions.ContainsKey(extName))
            _bind.Extensions.Add(extName, ext);

        return ext;
    }

    /// <summary>
    /// Loads a non-module based Vulkan extension. That is, an extension that only requires a name to be provided for it to be loaded.
    /// </summary>
    /// <param name="name"></param>
    internal void AddExtension(string name)
    {
        VulkanBasicExtension ext = new VulkanBasicExtension();
        if (!_bind.Extensions.ContainsKey(name))
            _bind.Extensions.Add(name, ext);
    }

    internal bool HasExtension(string extName)
    {
        return _bind.Extensions.ContainsKey(extName);
    }
    
    internal E GetExtension<E>()
        where E : NativeExtension<Vk>
    {
        string extName = GetNativeExtensionName<E>();
        if (_bind.Extensions.TryGetValue(extName, out VulkanExtension ext))
        {
            VulkanExtension<E> extension = ext as VulkanExtension<E>;
            return extension.Module;
        }
        else
        {
            throw new Exception("Attempt to retrieve invalid extension '{extName}'. Check extension support or add during instantiation.");
        }
    }

    internal void AddLayer(string layerName)
    {
        if (_bind == null)
            throw new Exception("Cannot add extensions before BeginNewInstance() has been called");

        if (!_bind.Layers.Contains(layerName))
            _bind.Layers.Add(layerName);
    }

    internal unsafe bool Build(VersionVK apiVersion, D* ptr)
    {
        if (IsBuilt)
            throw new Exception("Cannot call Build() more than once on the same ExtensionManager");

        PrepareLayers();
        PrepareExtensions();

        TempData tmp = new TempData()
        {
            LayerNames = (byte**)SilkMarshal.StringArrayToPtr(_bind.Layers.AsReadOnly(), NativeStringEncoding.UTF8),
            ExtensionNames = (byte**)SilkMarshal.StringArrayToPtr(_bind.Extensions.Keys.ToList().AsReadOnly(), NativeStringEncoding.UTF8)
        };

        Result r = OnBuild(Renderer, apiVersion, tmp, _bind, ptr);
        bool success = r.Check(Renderer);
        if (success)
        {

            // Load load all requested extension modules that were supported/available.
            foreach (VulkanExtension ext in _bind.Extensions.Values)
            {
                bool extSuccess = LoadExtensionModule(Renderer, ext, ptr);
            }
        }

        SilkMarshal.Free((nint)tmp.LayerNames);
        SilkMarshal.Free((nint)tmp.ExtensionNames);

        IsBuilt = success;
        return success;
    }

    private unsafe void PrepareLayers()
    {
        string typeName = typeof(D).Name.ToLower();

        LayerProperties[] properties = Renderer.Enumerate<LayerProperties>(GetLayers, $"{typeName} layers");
        if (properties.Length == 0)
            return;

        List<string> loaded = new List<string>();
        List<string> names = _bind.Layers.ToList();

        foreach(LayerProperties p in properties)
        {
            string name = SilkMarshal.PtrToString((nint)p.LayerName, NativeStringEncoding.UTF8);

            if (_bind.Layers.Contains(name))
            {
                loaded.Add(name);
                VersionVK specVersion = p.SpecVersion;
                VersionVK implVersion = p.ImplementationVersion;
                string desc = SilkMarshal.PtrToString((nint)p.Description, NativeStringEncoding.UTF8);
                Renderer.Log.WriteLine($"Prepared validation layer '{name}' -- Version: {specVersion} -- Implementation: {implVersion} -- Desc: {desc}");
            }
        }

        bool failWarned = false;
        for (int i = 0; i < names.Count; i++)
        {
            if (!loaded.Contains(names[i]))
            {
                if (!failWarned)
                {
                    Renderer.Log.Warning($"Failed to prepare the following {typeName} layers:");
                    failWarned = true;
                }
                Renderer.Log.Warning($"   {i + 1}. {names[i]}");
                _bind.Layers.Remove(names[i]);
            }
        }
    }

    private unsafe void PrepareExtensions()
    {
        string typeName = typeof(D).Name.ToLower();
        ExtensionProperties[] properties = Renderer.Enumerate<ExtensionProperties>(GetExtensions, $"{typeName} extensions");

        if (properties.Length == 0)
            return;

        List<string> supported = new List<string>();
        List<string> names = _bind.Extensions.Keys.ToList();

        foreach(ExtensionProperties p in properties)
        {
            string name = SilkMarshal.PtrToString((nint)p.ExtensionName, NativeStringEncoding.UTF8);
            if (_bind.Extensions.ContainsKey(name))
            {
                supported.Add(name);
                VersionVK specVersion = p.SpecVersion;
                Renderer.Log.WriteLine($"Prepared {typeName} extension {name} -- Version: {specVersion}");
            }
        }

        bool failWarned = false;
        for (int i = 0; i < names.Count; i++)
        {
            string name = names[i];

            if (!supported.Contains(name))
            {
                if (!failWarned)
                {
                    Renderer.Log.Warning($"Failed to prepare the following {typeName} extensions:");
                    failWarned = true;
                }
                Renderer.Log.Warning($"   {i + 1}. {name}");
                _bind.Extensions.Remove(name);
            }
        }
    }

    protected override void OnDispose(bool immediate)
    {

        foreach (VulkanExtension ext in _bind.Extensions.Values)
            ext.Unload(Renderer);

        _bind.Extensions.Clear();
        _bind.Layers.Clear();
    }

    /// <summary>
    /// Gets the <see cref="RendererVK"/> instance that the current <see cref="ExtensionLoaderVK{D}"/> is bound to.
    /// </summary>
    internal RendererVK Renderer { get; }

    /// <summary>
    /// Gets whether or not the current <see cref="ExtensionLoaderVK{D}"/> object has been built via <see cref="Build(VersionVK)"/>.
    /// </summary>
    internal bool IsBuilt { get; private protected set; }
}
