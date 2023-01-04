using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Silk.NET.Core.Attributes;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;

namespace Molten.Graphics
{
    internal abstract class ExtensionManager<D> : IDisposable
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

        ExtensionBinding _newBinding;
        Dictionary<D, ExtensionBinding> _instances = new Dictionary<D, ExtensionBinding>();

        RendererVK _renderer;

        internal ExtensionManager(RendererVK renderer)
        {
            _renderer = renderer;
        }

        protected abstract nint GetObjectHandle(D obj);

        protected abstract bool LoadExtension(RendererVK renderer, VulkanExtension ext, D obj);

        protected unsafe abstract Result OnBuild(RendererVK renderer, VersionVK apiVersion, TempData tmp, ExtensionBinding binding, D* obj);

        protected unsafe abstract void DestroyObject(RendererVK renderer, D obj);

        public unsafe void Dispose()
        {
            foreach (D obj in _instances.Keys)
            {
                if (GetObjectHandle(obj) != 0)
                {
                    ExtensionBinding bind = _instances[obj];
                    foreach (VulkanExtension ext in bind.Extensions.Values)
                        ext.Unload(_renderer);

                    bind.Extensions.Clear();
                    bind.Layers.Clear();

                    DestroyObject(_renderer, obj);
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
            if (_newBinding != null)
                throw new Exception("Cannot begin creation of a new instance when one has already began");

            _newBinding = new ExtensionBinding();
        }

        internal void AddExtension<E>(Action<E> loadCallback = null, Action<E> destroyCallback = null)
            where E : NativeExtension<Vk>
        {
            if (_newBinding == null)
                throw new Exception("Cannot add extensions before BeginNewInstance() has been called");

            VulkanExtension<E> ext = new VulkanExtension<E>(loadCallback, destroyCallback);
            string extName = GetNativeExtensionName<E>();
            if(!_newBinding.Extensions.ContainsKey(extName))
                _newBinding.Extensions.Add(extName, ext);
        }

        internal void AddLayer(string layerName)
        {
            if (_newBinding == null)
                throw new Exception("Cannot add extensions before BeginNewInstance() has been called");

            if (!_newBinding.Layers.Contains(layerName))
                _newBinding.Layers.Add(layerName);
        }

        internal unsafe bool Build(VersionVK apiVersion, out D* obj)
        {
            if (_newBinding == null)
                throw new Exception("Cannot call build a new instance before BeginNewInstance() is called");

            EnableLayers();
            EnableExtensions();

            TempData tmp = new TempData()
            {
                LayerNames = (byte**)SilkMarshal.StringArrayToPtr(_newBinding.Layers.AsReadOnly(), NativeStringEncoding.UTF8),
                ExtensionNames = (byte**)SilkMarshal.StringArrayToPtr(_newBinding.Extensions.Keys.ToList().AsReadOnly(), NativeStringEncoding.UTF8)
            };

            obj = EngineUtil.Alloc<D>();
            Result r = OnBuild(_renderer, apiVersion, tmp, _newBinding, obj);
            bool success = _renderer.LogResult(r); 
            if (success)
            {
                _instances.Add(*obj, _newBinding);

                // Load load all requested extension modules that were supported/available.
                foreach (VulkanExtension ext in _newBinding.Extensions.Values)
                {
                    bool extSuccess = LoadExtension(_renderer, ext, *obj);
                }
            }

            SilkMarshal.Free((nint)tmp.LayerNames);
            SilkMarshal.Free((nint)tmp.ExtensionNames);

            _newBinding = null;
            return success;
        }

        private unsafe void EnableLayers()
        {
            string typeName = typeof(D).Name.ToLower();
            LayerProperties[] properties = _renderer.Enumerate<LayerProperties>(_renderer.VK.EnumerateInstanceLayerProperties, $"{typeName} layers");
            if (properties.Length == 0)
                return;

            List<string> loaded = new List<string>();
            List<string> names = _newBinding.Layers.ToList();

            _renderer.Log.WriteLine($"Enabled the following {typeName} layers:");
            int loadIndex = 1;

            foreach(LayerProperties p in properties)
            {
                string name = SilkMarshal.PtrToString((nint)p.LayerName, NativeStringEncoding.UTF8);

                if (_newBinding.Layers.Contains(name))
                {
                    loaded.Add(name);
                    VersionVK specVersion = p.SpecVersion;
                    VersionVK implVersion = p.ImplementationVersion;
                    string desc = SilkMarshal.PtrToString((nint)p.Description, NativeStringEncoding.UTF8);
                    _renderer.Log.WriteLine($"   {loadIndex++}. {name} -- Version: {specVersion} -- Implementation: {implVersion} -- Desc: {desc}");
                }
            }


            bool failWarned = false;
            for (int i = 0; i < names.Count; i++)
            {
                if (!loaded.Contains(names[i]))
                {
                    if (!failWarned)
                    {
                        _renderer.Log.Warning($"Failed to enable the following {typeName} layers:");
                        failWarned = true;
                    }
                    _renderer.Log.Warning($"   {i + 1}. {names[i]}");
                    _newBinding.Layers.Remove(names[i]);
                }
            }
        }

        private unsafe void EnableExtensions()
        {
            string typeName = typeof(D).Name.ToLower();
            ExtensionProperties[] properties = _renderer.Enumerate<ExtensionProperties>((count, items) =>
            {
                byte* nullptr = null;
                return _renderer.VK.EnumerateInstanceExtensionProperties(nullptr, count, items);
            }, $"{typeName} extensions");

            if (properties.Length == 0)
                return;

            List<string> loaded = new List<string>();
            List<string> names = _newBinding.Extensions.Keys.ToList();
            int loadIndex = 1;

            _renderer.Log.WriteLine($"Enabled the following {typeName} extensions:");
            foreach(ExtensionProperties p in properties)
            {
                string name = SilkMarshal.PtrToString((nint)p.ExtensionName, NativeStringEncoding.UTF8);
                if (_newBinding.Extensions.ContainsKey(name))
                {
                    loaded.Add(name);
                    VersionVK specVersion = p.SpecVersion;
                    _renderer.Log.WriteLine($"   {loadIndex++}. {name} -- Version: {specVersion}");
                }
            }

            bool failWarned = false;
            for (int i = 0; i < names.Count; i++)
            {
                if (!loaded.Contains(names[i]))
                {
                    if (!failWarned)
                    {
                        _renderer.Log.Warning($"Failed to enable the following {typeName}extensions:");
                        failWarned = true;
                    }
                    _renderer.Log.Warning($"   {i + 1}. {names[i]}");
                    _newBinding.Extensions.Remove(names[i]);
                }
            }
        }
    }
}
