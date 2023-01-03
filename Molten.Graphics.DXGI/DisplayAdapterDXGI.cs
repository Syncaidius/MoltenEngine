using Silk.NET.Core.Native;
using Silk.NET.DXGI;

namespace Molten.Graphics.Dxgi
{
    public unsafe class DisplayAdapterDXGI : EngineObject, IDisplayAdapter
    {
        /// <summary>Gets the native DXGI adapter that this instance represents.</summary>
        public IDXGIAdapter4* Native;
        AdapterDesc3* _desc;
        List<DisplayOutputDXGI> _outputs;
        List<DisplayOutputDXGI> _activeOutputs;
        DisplayManagerDXGI _manager;

        /// <summary> Occurs when an <see cref="T:Molten.IDisplayOutput" /> is connected to the current <see cref="T:Molten.IDisplayAdapter" />.</summary>
        public event DisplayOutputChanged OnOutputActivated;

        /// <summary>Occurs when an <see cref="T:Molten.IDisplayOutput" /> is disconnected from the current <see cref="T:Molten.IDisplayAdapter" />. </summary>
        public event DisplayOutputChanged OnOutputDeactivated;

        public DisplayAdapterDXGI(DisplayManagerDXGI manager, GraphicsCapabilities cap, IDXGIAdapter4* adapter)
        {
            _manager = manager;
            Native = adapter;
            Capabilities = cap;

            _desc = EngineUtil.Alloc<AdapterDesc3>();
            adapter->GetDesc3(_desc);
            ID = (DeviceID)_desc->AdapterLuid;

            Name = SilkMarshal.PtrToString((nint)_desc->Description, NativeStringEncoding.LPWStr);
            Vendor = EngineUtil.VendorFromPCI(_desc->VendorId);
            
            cap.DedicatedSystemMemory = ByteMath.ToMegabytes(_desc->DedicatedSystemMemory);
            cap.DedicatedVideoMemory = ByteMath.ToMegabytes(_desc->DedicatedVideoMemory);

            nuint sharedMemory = _desc->SharedSystemMemory;
            sharedMemory = sharedMemory < 0 ? 0 : sharedMemory;
            cap.SharedSystemMemory = ByteMath.ToMegabytes(sharedMemory);
            Type = GetAdapterType(cap, _desc->Flags);

            IDXGIOutput1*[] dxgiOutputs = DXGIHelper.EnumArray<IDXGIOutput1, IDXGIOutput>((uint index, ref IDXGIOutput* ptrOutput) =>
            {
                return adapter->EnumOutputs(index, ref ptrOutput);
            });

            _activeOutputs = new List<DisplayOutputDXGI>();
            ActiveOutputs = _activeOutputs.AsReadOnly();
            _outputs = new List<DisplayOutputDXGI>();
            Outputs = _outputs.AsReadOnly();
            Guid o6Guid = IDXGIOutput6.Guid;

            for (int i = 0; i < dxgiOutputs.Length; i++)
            {
                void* ptr6 = null;
                int r = dxgiOutputs[i]->QueryInterface(&o6Guid, &ptr6);

                if (DXGIHelper.ErrorFromResult(r) != DxgiError.Ok)
                    _manager.Log.Error($"Error while querying adapter '{Name}' output IDXGIOutput1 for IDXGIOutput6 interface");

                _outputs.Add(new DisplayOutputDXGI(this, (IDXGIOutput6*)ptr6));
            }
        }

        private DisplayAdapterType GetAdapterType(GraphicsCapabilities cap, AdapterFlag3 flags)
        {
            if ((flags & AdapterFlag3.Software) == AdapterFlag3.Software)
                return DisplayAdapterType.Cpu;

            if (cap.DedicatedVideoMemory > 0)
                return DisplayAdapterType.DiscreteGpu;

            if (cap.DedicatedSystemMemory > 0 || cap.SharedSystemMemory > 0)
                return DisplayAdapterType.IntegratedGpu;

            return DisplayAdapterType.Other;
        }

        protected override void OnDispose()
        {
            EngineUtil.Free(ref _desc);
            SilkUtil.ReleasePtr(ref Native);
        }

        public void AddActiveOutput(IDisplayOutput output)
        {
            if (output.Adapter != this)
                throw new AdapterOutputException(output, "Cannot add active output: Bound to another adapter.");

            if (!_activeOutputs.Contains(output))
            {
                _activeOutputs.Add(output as DisplayOutputDXGI);
                OnOutputActivated?.Invoke(output);
            }
        }

        public void RemoveActiveOutput(IDisplayOutput output)
        {
            if (output.Adapter != this)
                throw new AdapterOutputException(output, "Cannot remove active output: Bound to another adapter.");

            if (_activeOutputs.Remove(output as DisplayOutputDXGI))
                OnOutputDeactivated?.Invoke(output);
        }

        public void RemoveAllActiveOutputs()
        {
            if (OnOutputDeactivated != null)
            {
                for (int i = 0; i < _activeOutputs.Count; i++)
                    OnOutputDeactivated.Invoke(_activeOutputs[i]);
            }

            _activeOutputs.Clear();
        }

        /// <<inheritdoc/>
        public DeviceID ID { get; private set; }

        /// <inheritdoc/>
        public DeviceVendor Vendor { get; private set; }

        /// <inheritdoc/>
        public DisplayAdapterType Type { get; private set; }

        /// <inheritdoc/>
        public IDisplayManager Manager => _manager;

        internal AdapterDesc3* Description => _desc;

        /// <inheritdoc/>
        public GraphicsCapabilities Capabilities { get; }

        /// <inheritdoc/>
        public IReadOnlyList<IDisplayOutput> Outputs { get; }

        /// <inheritdoc/>
        public IReadOnlyList<IDisplayOutput> ActiveOutputs { get; }
    }
}
