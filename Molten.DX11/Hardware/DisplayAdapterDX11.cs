using Silk.NET.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public unsafe class DisplayAdapterDX11 : IDisplayAdapter
    {
        /// <summary>Gets the native DXGI adapter that this instance represents.</summary>
        public IDXGIAdapter1* Native;

        AdapterDesc1 _desc;

        DisplayOutputDX11[] _connectedOutputs;
        List<GraphicsOutput> _activeOutputs;
        IDisplayManager _manager;
        string _name;


        /// <summary> Occurs when an <see cref="T:Molten.IDisplayOutput" /> is connected to the current <see cref="T:Molten.IDisplayAdapter" />.</summary>
        public event DisplayOutputChanged OnOutputActivated;

        /// <summary>Occurs when an <see cref="T:Molten.IDisplayOutput" /> is disconnected from the current <see cref="T:Molten.IDisplayAdapter" />. </summary>
        public event DisplayOutputChanged OnOutputDeactivated;

        public DisplayAdapterDX11(IDisplayManager manager, IDXGIAdapter1* adapter, 
            ref AdapterDesc1 desc, IDXGIOutput1*[] outputs, 
            int id)
        {
            _manager = manager;
            Native = adapter;
            _activeOutputs = new List<GraphicsOutput>();
            ID = id;
            Native->GetDesc1(ref _desc);

            fixed (char* d = _desc.Description)
                _name = new string(d);

            _name.Replace("\0", string.Empty);

            PopulateVendor();

            nuint shared = _desc.SharedSystemMemory;
            if (shared < 0)
                _desc.SharedSystemMemory = 0;

            _connectedOutputs = new DisplayOutputDX11[outputs.Length];

            for (int i = 0; i < _connectedOutputs.Length; i++)
                _connectedOutputs[i] = new DisplayOutputDX11(this, outputs[i]);
        }

        ~DisplayAdapterDX11()
        {
            Native->Release();
        }

        private void PopulateVendor()
        {
            // See: https://pcisig.com/membership/member-companies
            // See: https://gamedev.stackexchange.com/a/31626/116135
            switch (_desc.VendorId)
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
                _activeOutputs.Add(output as GraphicsOutput);
                OnOutputActivated?.Invoke(output);
            }
        }

        public void RemoveActiveOutput(IDisplayOutput output)
        {
            if (output.Adapter != this)
                throw new AdapterOutputException(output, "Cannot remove active output: Bound to another adapter.");

            if (_activeOutputs.Remove(output as GraphicsOutput))
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

        public override string ToString()
        {
            return _name;
        }

        /// <summary>Gets the vendor's name of the adapter.</summary>
        public string Name => _name;

        /// <summary>Gets the amount of dedicated video memory, in megabytes.</summary>
        public double DedicatedVideoMemory => ByteMath.ToMegabytes((long)_desc.DedicatedVideoMemory);

        /// <summary>Gets the amount of system memory dedicated to the adapter.</summary>
        public double DedicatedSystemMemory => ByteMath.ToMegabytes((long)_desc.DedicatedSystemMemory);

        /// <summary>Gets the amount of system memory that is being shared with the adapter.</summary>
        public double SharedSystemMemory => ByteMath.ToMegabytes((long)_desc.SharedSystemMemory);

        /// <summary>Gets the ID of the adapter.</summary>
        public int ID { get; private set; }

        /// <summary>The hardware vendor.</summary>
        public GraphicsAdapterVendor Vendor { get; private set; }

        /// <summary>Gets the number of <see cref="GraphicsOutput"/> connected to the adapter.</summary>
        public int OutputCount => _connectedOutputs.Length;

        /// <summary>
        /// Gets the <see cref="T:Molten.Graphics.IDisplayManager" /> that spawned the adapter.
        /// </summary>
        public IDisplayManager Manager => _manager;
    }
}
