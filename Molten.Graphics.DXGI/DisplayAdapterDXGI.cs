using Silk.NET.Core.Native;
using Silk.NET.DXGI;

namespace Molten.Graphics.Dxgi
{
    public unsafe class DisplayAdapterDXGI : EngineObject, IDisplayAdapter
    {
        /// <summary>Gets the native DXGI adapter that this instance represents.</summary>
        public IDXGIAdapter1* Native;
        AdapterDesc1* _desc;
        DisplayOutputDXGI[] _connectedOutputs;
        List<DisplayOutputDXGI> _activeOutputs;
        IDisplayManager _manager;


        /// <summary> Occurs when an <see cref="T:Molten.IDisplayOutput" /> is connected to the current <see cref="T:Molten.IDisplayAdapter" />.</summary>
        public event DisplayOutputChanged OnOutputActivated;

        /// <summary>Occurs when an <see cref="T:Molten.IDisplayOutput" /> is disconnected from the current <see cref="T:Molten.IDisplayAdapter" />. </summary>
        public event DisplayOutputChanged OnOutputDeactivated;

        public DisplayAdapterDXGI(IDisplayManager manager, IDXGIAdapter1* adapter, int id)
        {
            _manager = manager;
            Native = adapter;
            _activeOutputs = new List<DisplayOutputDXGI>();
            ID = id;
            _desc = EngineUtil.Alloc<AdapterDesc1>();
            adapter->GetDesc1(_desc);

            Name = SilkMarshal.PtrToString((nint)_desc->Description, NativeStringEncoding.LPWStr);

            PopulateVendor();

            DedicatedSystemMemory = ByteMath.ToMegabytes(_desc->DedicatedSystemMemory);
            DedicatedVideoMemory = ByteMath.ToMegabytes(_desc->DedicatedVideoMemory);

            nuint sharedMemory = _desc->SharedSystemMemory;
            sharedMemory = sharedMemory < 0 ? 0 : sharedMemory;
            SharedSystemMemory = ByteMath.ToMegabytes(sharedMemory);

            IDXGIOutput1*[] outputs = DXGIHelper.EnumArray<IDXGIOutput1, IDXGIOutput>((uint index, ref IDXGIOutput* ptrOutput) =>
            {
                return adapter->EnumOutputs(index, ref ptrOutput);
            });

            _connectedOutputs = new DisplayOutputDXGI[outputs.Length];

            for (int i = 0; i < _connectedOutputs.Length; i++)
                _connectedOutputs[i] = new DisplayOutputDXGI(this, outputs[i]);
        }

        protected override void OnDispose()
        {
            EngineUtil.Free(ref _desc);
            SilkUtil.ReleasePtr(ref Native);
        }

        private void PopulateVendor()
        {
            // See: https://pcisig.com/membership/member-companies
            // See: https://gamedev.stackexchange.com/a/31626/116135
            switch (_desc->VendorId)
            {
                case 0x1002:
                case 0x1022:
                    Vendor = GraphicsAdapterVendor.AMD;
                    break;

                case 0x163C:
                case 0x8086:
                case 0x8087:
                    Vendor = GraphicsAdapterVendor.Intel;
                    break;

                case 0x10DE:
                    Vendor = GraphicsAdapterVendor.Nvidia;
                    break;

                default:
                    Vendor = GraphicsAdapterVendor.Unknown;
                    break;
            }
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

        /// <summary>Gets the amount of dedicated video memory, in megabytes.</summary>
        public double DedicatedVideoMemory { get; }

        /// <summary>Gets the amount of system memory dedicated to the adapter.</summary>
        public double DedicatedSystemMemory { get; }

        /// <summary>Gets the amount of system memory that is being shared with the adapter.</summary>
        public double SharedSystemMemory { get; }

        /// <summary>Gets the ID of the adapter.</summary>
        public int ID { get; private set; }

        /// <summary>The hardware vendor.</summary>
        public GraphicsAdapterVendor Vendor { get; private set; }

        /// <summary>Gets the number of <see cref="GraphicsOutput"/> connected to the adapter.</summary>
        public int OutputCount => _connectedOutputs.Length;

        /// <summary>
        /// Gets the <see cref="IDisplayManager" /> that spawned the adapter.
        /// </summary>
        public IDisplayManager Manager => _manager;
    }
}
