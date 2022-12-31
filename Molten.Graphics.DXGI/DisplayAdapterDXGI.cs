using Silk.NET.Core.Native;
using Silk.NET.DXGI;

namespace Molten.Graphics.Dxgi
{
    public unsafe class DisplayAdapterDXGI : EngineObject, IDisplayAdapter
    {
        /// <summary>Gets the native DXGI adapter that this instance represents.</summary>
        public IDXGIAdapter4* Native;
        AdapterDesc3* _desc;
        DisplayOutputDXGI[] _connectedOutputs;
        List<DisplayOutputDXGI> _activeOutputs;
        DisplayManagerDXGI _manager;

        /// <summary> Occurs when an <see cref="T:Molten.IDisplayOutput" /> is connected to the current <see cref="T:Molten.IDisplayAdapter" />.</summary>
        public event DisplayOutputChanged OnOutputActivated;

        /// <summary>Occurs when an <see cref="T:Molten.IDisplayOutput" /> is disconnected from the current <see cref="T:Molten.IDisplayAdapter" />. </summary>
        public event DisplayOutputChanged OnOutputDeactivated;

        public DisplayAdapterDXGI(DisplayManagerDXGI manager, IDXGIAdapter4* adapter, int id)
        {
            _manager = manager;
            Native = adapter;
            _activeOutputs = new List<DisplayOutputDXGI>();
            _desc = EngineUtil.Alloc<AdapterDesc3>();
            adapter->GetDesc3(_desc);
            ID = (DeviceID)_desc->AdapterLuid;

            Name = SilkMarshal.PtrToString((nint)_desc->Description, NativeStringEncoding.LPWStr);
            Vendor = EngineUtil.VendorFromPCI(_desc->VendorId);
            DedicatedSystemMemory = ByteMath.ToMegabytes(_desc->DedicatedSystemMemory);
            DedicatedVideoMemory = ByteMath.ToMegabytes(_desc->DedicatedVideoMemory);

            nuint sharedMemory = _desc->SharedSystemMemory;
            sharedMemory = sharedMemory < 0 ? 0 : sharedMemory;
            SharedSystemMemory = ByteMath.ToMegabytes(sharedMemory);
            Type = GetAdapterType(_desc->Flags);

            IDXGIOutput1*[] outputs = DXGIHelper.EnumArray<IDXGIOutput1, IDXGIOutput>((uint index, ref IDXGIOutput* ptrOutput) =>
            {
                return adapter->EnumOutputs(index, ref ptrOutput);
            });

            _connectedOutputs = new DisplayOutputDXGI[outputs.Length];
            Guid o6Guid = IDXGIOutput6.Guid;

            for (int i = 0; i < _connectedOutputs.Length; i++)
            {
                void* ptr6 = null;
                int r = outputs[i]->QueryInterface(&o6Guid, &ptr6);
                DxgiError err = DXGIHelper.ErrorFromResult(r);
                if (err != DxgiError.Ok)
                    _manager.Log.Error($"Error while querying adapter '{Name}' output IDXGIOutput1 for IDXGIOutput6 interface");

                _connectedOutputs[i] = new DisplayOutputDXGI(this, (IDXGIOutput6*)ptr6);
            }
        }

        private DisplayAdapterType GetAdapterType(AdapterFlag3 flags)
        {
            if ((flags & AdapterFlag3.Software) == AdapterFlag3.Software)
                return DisplayAdapterType.Cpu;

            if (DedicatedVideoMemory > 0)
                return DisplayAdapterType.DiscreteGpu;

            if (DedicatedSystemMemory > 0 || SharedSystemMemory > 0)
                return DisplayAdapterType.IntegratedGpu;

            return DisplayAdapterType.Other;
        }

        protected override void OnDispose()
        {
            EngineUtil.Free(ref _desc);
            SilkUtil.ReleasePtr(ref Native);
        }

        public IDisplayOutput GetOutput(int id)
        {
            if (id >= _connectedOutputs.Length)
                throw new IndexOutOfRangeException($"ID was {id} while there are only {_connectedOutputs.Length} connected display outputs.");

            if (id < 0)
                throw new IndexOutOfRangeException("ID cannot be less than 0");

            return _connectedOutputs[id];
        }

        public void GetActiveOutputs(List<IDisplayOutput> outputList)
        {
            outputList.AddRange(_activeOutputs);
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

        /// <summary>Gets all <see cref="T:Molten.IDisplayOutput" /> devices attached to the current <see cref="T:Molten.IDisplayAdapter" />.</summary>
        /// <param name="outputList">The output list.</param>
        public void GetAttachedOutputs(List<IDisplayOutput> outputList)
        {
            outputList.AddRange(_connectedOutputs);
        }

        /// <inheritdoc/>
        public double DedicatedVideoMemory { get; }

        /// <inheritdoc/>
        public double DedicatedSystemMemory { get; }

        /// <inheritdoc/>
        public double SharedSystemMemory { get; }

        /// <<inheritdoc/>
        public DeviceID ID { get; private set; }

        /// <inheritdoc/>
        public DeviceVendor Vendor { get; private set; }

        /// <inheritdoc/>
        public DisplayAdapterType Type { get; private set; }

        /// <inheritdoc/>
        public int OutputCount => _connectedOutputs.Length;

        /// <inheritdoc/>
        public IDisplayManager Manager => _manager;

        /// <inheritdoc/>
        public GraphicsCapabilities Capabilities { get; internal set; }
    }
}
