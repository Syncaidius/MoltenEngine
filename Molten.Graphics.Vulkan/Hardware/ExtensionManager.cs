using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Silk.NET.Core.Attributes;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;

namespace Molten.Graphics
{
    internal unsafe abstract class ExtensionManager<D> : EngineObject
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

        D* _ptr;
        ExtensionBinding _bind;
        RendererVK _renderer;

        internal ExtensionManager(RendererVK renderer)
        {
            _renderer = renderer;
            _bind = new ExtensionBinding();
        }

        protected abstract bool LoadExtension(RendererVK renderer, VulkanExtension ext, D* obj);

        protected unsafe abstract Result OnBuild(RendererVK renderer, VersionVK apiVersion, TempData tmp, ExtensionBinding binding, D* obj);

        protected unsafe abstract void DestroyObject(RendererVK renderer, D* obj);

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

        internal void AddExtension<E>(Action<E> loadCallback = null, Action<E> destroyCallback = null)
            where E : NativeExtension<Vk>
        {
            if (_bind == null)
                throw new Exception("Cannot add extensions before BeginNewInstance() has been called");

            VulkanExtension<E> ext = new VulkanExtension<E>(loadCallback, destroyCallback);
            string extName = GetNativeExtensionName<E>();
            if(!_bind.Extensions.ContainsKey(extName))
                _bind.Extensions.Add(extName, ext);
        }

        internal bool HasExtension(string extName)
        {
            return _bind.Extensions.ContainsKey(extName);
        }

        internal void AddLayer(string layerName)
        {
            if (_bind == null)
                throw new Exception("Cannot add extensions before BeginNewInstance() has been called");

            if (!_bind.Layers.Contains(layerName))
                _bind.Layers.Add(layerName);
        }

        internal unsafe bool Build(VersionVK apiVersion)
        {
            if (_ptr != null)
                throw new Exception("Cannot call Build() more than once on the same ExtensionManager");

            EnableLayers();
            EnableExtensions();

            TempData tmp = new TempData()
            {
                LayerNames = (byte**)SilkMarshal.StringArrayToPtr(_bind.Layers.AsReadOnly(), NativeStringEncoding.UTF8),
                ExtensionNames = (byte**)SilkMarshal.StringArrayToPtr(_bind.Extensions.Keys.ToList().AsReadOnly(), NativeStringEncoding.UTF8)
            };

            _ptr = EngineUtil.Alloc<D>();
            Result r = OnBuild(_renderer, apiVersion, tmp, _bind, _ptr);
            bool success = _renderer.LogResult(r); 
            if (success)
            {

                // Load load all requested extension modules that were supported/available.
                foreach (VulkanExtension ext in _bind.Extensions.Values)
                {
                    bool extSuccess = LoadExtension(_renderer, ext, _ptr);
                }
            }

            SilkMarshal.Free((nint)tmp.LayerNames);
            SilkMarshal.Free((nint)tmp.ExtensionNames);

            _bind = null;
            return success;
        }

        private unsafe void EnableLayers()
        {
            string typeName = typeof(D).Name.ToLower();
            LayerProperties[] properties = _renderer.Enumerate<LayerProperties>(_renderer.VK.EnumerateInstanceLayerProperties, $"{typeName} layers");
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
                    _renderer.Log.WriteLine($"Loaded validation layer '{name}' -- Version: {specVersion} -- Implementation: {implVersion} -- Desc: {desc}");
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
                    _bind.Layers.Remove(names[i]);
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
            List<string> names = _bind.Extensions.Keys.ToList();

            foreach(ExtensionProperties p in properties)
            {
                string name = SilkMarshal.PtrToString((nint)p.ExtensionName, NativeStringEncoding.UTF8);
                if (_bind.Extensions.ContainsKey(name))
                {
                    loaded.Add(name);
                    VersionVK specVersion = p.SpecVersion;
                    _renderer.Log.WriteLine($"Loaded {typeName} extension {name} -- Version: {specVersion}");
                }
            }

            bool failWarned = false;
            for (int i = 0; i < names.Count; i++)
            {
                if (!loaded.Contains(names[i]))
                {
                    if (!failWarned)
                    {
                        _renderer.Log.Warning($"Failed to enable the following {typeName} extensions:");
                        failWarned = true;
                    }
                    _renderer.Log.Warning($"   {i + 1}. {names[i]}");
                    _bind.Extensions.Remove(names[i]);
                }
            }
        }

        protected override void OnDispose()
        {
            if (_ptr != null)
            {
                foreach (VulkanExtension ext in _bind.Extensions.Values)
                    ext.Unload(_renderer);

                _bind.Extensions.Clear();
                _bind.Layers.Clear();

                DestroyObject(_renderer, _ptr);
                EngineUtil.Free(ref _ptr);
            }
        }

        public static implicit operator D*(ExtensionManager<D> manager)
        {
            return manager._ptr;
        }

        /// <summary>
        /// Gets the underlying pointer of the object that has extensions attached to it. e.g. a <see cref="Instance"/> or <see cref="Device"/>.
        /// </summary>
        internal D* Ptr => _ptr;
    }
}
