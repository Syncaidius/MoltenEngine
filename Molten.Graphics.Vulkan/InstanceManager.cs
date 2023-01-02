using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Silk.NET.Core.Attributes;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;

namespace Molten.Graphics
{
    internal class InstanceManager : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="info"></param>
        /// <returns>True if the instance layer/extension is valid.</returns>
        private unsafe delegate PropertyInfo EnumerateInstanceRetrieveCallback<T>(T* info) where T : unmanaged;

        internal class InstanceBinding
        {
            internal Dictionary<string, VulkanExtension> Extensions = new Dictionary<string, VulkanExtension>();

            internal List<string> Layers = new List<string>();
        }

        internal struct PropertyInfo
        {
            internal VersionVK ImplementationVersion;

            internal VersionVK SpecVersion;

            internal string Description;

            internal bool Loaded;
        }

        InstanceBinding _newInstance;
        Dictionary<Instance, InstanceBinding> _instances = new Dictionary<Instance, InstanceBinding>();

        RendererVK _renderer;

        internal InstanceManager(RendererVK renderer)
        {
            _renderer = renderer;
        }

        public unsafe void Dispose()
        {
            foreach (Instance instance in _instances.Keys)
            {
                if (instance.Handle != 0)
                {
                    InstanceBinding bind = _instances[instance];
                    foreach (VulkanExtension ext in bind.Extensions.Values)
                        ext.Unload(_renderer, instance);

                    bind.Extensions.Clear();
                    bind.Layers.Clear();

                    _renderer.VK.DestroyInstance(instance, null);
                }
            }

            _instances.Clear();
        }

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

        internal void BeginNew()
        {
            if (_newInstance != null)
                throw new Exception("Cannot begin creation of a new instance when one has already began");

            _newInstance = new InstanceBinding();
        }

        internal void AddExtension<E>(Action<E> loadCallback = null, Action<E> destroyCallback = null)
            where E : NativeExtension<Vk>
        {
            if (_newInstance == null)
                throw new Exception("Cannot add extensions before BeginNewInstance() has been called");

            VulkanExtension<E> ext = new VulkanExtension<E>(loadCallback, destroyCallback);
            string extName = GetNativeExtensionName<E>();
            if(!_newInstance.Extensions.ContainsKey(extName))
                _newInstance.Extensions.Add(extName, ext);
        }

        internal void AddLayer(string layerName)
        {
            if (_newInstance == null)
                throw new Exception("Cannot add extensions before BeginNewInstance() has been called");

            if (!_newInstance.Layers.Contains(layerName))
                _newInstance.Layers.Add(layerName);
        }

        internal unsafe bool Build(out Instance* instance)
        {
            if (_newInstance == null)
                throw new Exception("Cannot call build a new instance before BeginNewInstance() is called");

            ApplicationInfo appInfo = new ApplicationInfo()
            {
                SType = StructureType.ApplicationInfo,
                EngineVersion = 1,
                ApiVersion = new VersionVK(0, 1, 3, 0),
            };

            EnableLayers();
            EnableExtensions();

            byte** layerNames = (byte**)SilkMarshal.StringArrayToPtr(_newInstance.Layers.AsReadOnly(), NativeStringEncoding.UTF8);
            byte** extNames = (byte**)SilkMarshal.StringArrayToPtr(_newInstance.Extensions.Keys.ToList().AsReadOnly(), NativeStringEncoding.UTF8);

            InstanceCreateInfo createInfo = new InstanceCreateInfo()
            {
                SType = StructureType.InstanceCreateInfo,
                PApplicationInfo = &appInfo,
                EnabledLayerCount = (uint)_newInstance.Layers.Count,
                PpEnabledLayerNames = layerNames,
                EnabledExtensionCount = (uint)_newInstance.Extensions.Count,
                PpEnabledExtensionNames = extNames
            };

            // Create the instance
            instance = EngineUtil.Alloc<Instance>();
            Result r = _renderer.VK.CreateInstance(&createInfo, null, instance);
            bool success = _renderer.LogResult(r);
            if (success)
            {
                _instances.Add(*instance, _newInstance);

                // Load load all requested extension modules that were supported/available.
                foreach (VulkanExtension ext in _newInstance.Extensions.Values)
                    ext.Load(_renderer, *instance);
            }

            SilkMarshal.Free((nint)layerNames);
            SilkMarshal.Free((nint)extNames);

            _newInstance = null;
            return success;
        }

        private unsafe void EnableLayers()
        {
            LayerProperties[] properties = _renderer.Enumerate<LayerProperties>(_renderer.VK.EnumerateInstanceLayerProperties, "instance layers");
            if (properties.Length == 0)
                return;

            List<string> loaded = new List<string>();
            List<string> names = _newInstance.Extensions.Keys.ToList();

            _renderer.Log.WriteLine($"Enabled the following layers:");
            for (int i = 0; i < properties.Length; i++)
            {
                LayerProperties p = properties[i];
                string name = SilkMarshal.PtrToString((nint)p.LayerName, NativeStringEncoding.UTF8);

                if (_newInstance.Extensions.ContainsKey(name))
                {
                    loaded.Add(name);
                    VersionVK specVersion = p.SpecVersion;
                    VersionVK implVersion = p.ImplementationVersion;
                    string desc = SilkMarshal.PtrToString((nint)p.Description, NativeStringEncoding.UTF8);
                    _renderer.Log.WriteLine($"   {i + 1}. {name} -- Version: {specVersion} -- Implementation: {implVersion} -- Desc: {desc}");
                }
            }

            _renderer.Log.Warning($"Failed to enable the following layers:");
            for (int i = 0; i < names.Count; i++)
            {
                if (!loaded.Contains(names[i]))
                {
                    _renderer.Log.Warning($"   {i + 1}. {names[i]}");
                    _newInstance.Layers.Remove(names[i]);
                }
            }
        }

        private unsafe void EnableExtensions()
        {
            ExtensionProperties[] properties = _renderer.Enumerate<ExtensionProperties>((count, items) =>
            {
                byte* nullptr = null;
                return _renderer.VK.EnumerateInstanceExtensionProperties(nullptr, count, items);
            }, "instance extensions");

            if (properties.Length == 0)
                return;

            List<string> loaded = new List<string>();
            List<string> names = _newInstance.Extensions.Keys.ToList();

            _renderer.Log.WriteLine($"Enabled the following extensions:");
            for (int i = 0; i < properties.Length; i++)
            {
                ExtensionProperties p = properties[i];
                string name = SilkMarshal.PtrToString((nint)p.ExtensionName, NativeStringEncoding.UTF8);
                if (_newInstance.Extensions.ContainsKey(name))
                {
                    loaded.Add(name);
                    VersionVK specVersion = p.SpecVersion;
                    _renderer.Log.WriteLine($"   {i + 1}. {name} -- Version: {specVersion}");
                }
            }  

            _renderer.Log.Warning($"Failed to enable the following extensions:");
            for (int i = 0; i < names.Count; i++)
            {
                if (!loaded.Contains(names[i]))
                {
                    _renderer.Log.Warning($"   {i + 1}. {names[i]}");
                    _newInstance.Extensions.Remove(names[i]);
                }
            }
        }
    }
}
