using SharpDX;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten.Graphics
{
    public class GraphicsAdapterDX<A, D, O> : IDisplayAdapter
        where A : Adapter
        where D : struct
        where O : Output
    {
        A _adapter;
        AdapterDescription _desc;

        DisplayOutputDX<A,D, O>[] _connectedOutputs;
        List<GraphicsOutput> _activeOutputs;

        IDisplayManager _manager;
        string _name;
        

        /// <summary> Occurs when an <see cref="T:Molten.IDisplayOutput" /> is connected to the current <see cref="T:Molten.IDisplayAdapter" />.</summary>
        public event DisplayOutputChanged OnOutputActivated;

        /// <summary>Occurs when an <see cref="T:Molten.IDisplayOutput" /> is disconnected from the current <see cref="T:Molten.IDisplayAdapter" />. </summary>
        public event DisplayOutputChanged OnOutputDeactivated;

        public GraphicsAdapterDX(IDisplayManager manager, A adapter, D desc, O[] outputs, int id)
        {
            _manager = manager;
            _adapter = adapter;
            _activeOutputs = new List<GraphicsOutput>();
            ID = id;
            _desc = _adapter.Description;
            _name = _desc.Description.Replace("\0", string.Empty);
            PopulateVendor();

            long shared = _desc.SharedSystemMemory;
            if (shared < 0)
                _desc.SharedSystemMemory = 0;

            _connectedOutputs = new DisplayOutputDX<A, D, O>[outputs.Length];

            for (int i = 0; i < _connectedOutputs.Length; i++)
                _connectedOutputs[i] = new DisplayOutputDX<A, D, O>(this, outputs[i]);
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
            return _desc.Description;
        }

        /// <summary>Gets the DXGI adapter this instance represents.</summary>
        public A Adapter => _adapter;

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

        /// <summary>The PCI ID of the hardware adapter.</summary>
        public int DeviceID => _desc.DeviceId;

        /// <summary>Gets a unique value that identifies the adapter hardware.</summary>
        public long UniqueID => _desc.Luid;

        /// <summary>Gets the PCI ID of the revision number of the adapter.</summary>
        public int Revision => _desc.Revision;

        /// <summary>Gets the PCI ID of the sub-system.</summary>
        public int SubsystemID => _desc.SubsystemId;

        /// <summary>Gets the number of <see cref="GraphicsOutput"/> connected to the adapter.</summary>
        public int OutputCount => _connectedOutputs.Length;

        /// <summary>
        /// Gets the <see cref="T:Molten.Graphics.IDisplayManager" /> that spawned the adapter.
        /// </summary>
        public IDisplayManager Manager => _manager;
    }
}
