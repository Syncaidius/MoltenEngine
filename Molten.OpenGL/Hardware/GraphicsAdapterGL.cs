using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace Molten.Graphics
{
    public class GraphicsAdapterGL : IDisplayAdapter
    {
        public event DisplayOutputChanged OnOutputActivated;
        public event DisplayOutputChanged OnOutputDeactivated;

        List<DisplayOutputGL> _connectedOutputs;
        List<DisplayOutputGL> _activeOutputs;

        DisplayManagerGL _manager;
        List<string> _extensions;

        internal GraphicsAdapterGL(DisplayManagerGL manager, int id)
        {
            ID = id;
            _manager = manager;
            _connectedOutputs = new List<DisplayOutputGL>();
            _activeOutputs = new List<DisplayOutputGL>();
            _extensions = new List<string>();
            PopulateInfo();

            HashSet<DisplayDevice> displays = new HashSet<DisplayDevice>();

            int last = (int)DisplayIndex.Sixth + 1;
            for (int i = -1; i < last; i++)
            {
                DisplayIndex index = (DisplayIndex)i;
                DisplayDevice display = DisplayDevice.GetDisplay(index);

                if (display != null)
                    _connectedOutputs.Add(new DisplayOutputGL(this, display, index));
            }
        }

        private void PopulateInfo()
        {
            Name = GL.GetString(StringName.Renderer);

            string strVendor = GL.GetString(StringName.Vendor);
            if(strVendor != null)
            {
                strVendor = strVendor.ToLower();
                if (strVendor.Contains("amd") || strVendor.Contains("ati"))
                    Vendor = GraphicsAdapterVendor.AMD;
                else if (strVendor.Contains("intel"))
                    Vendor = GraphicsAdapterVendor.Intel;
                else if (strVendor.Contains("nvidia"))
                    Vendor = GraphicsAdapterVendor.Nvidia;
                else
                    Vendor = GraphicsAdapterVendor.Unknown;
            }

            string etensions = GL.GetString(StringName.Extensions);
            _extensions.AddRange(etensions.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
            for (int i = 0; i < _extensions.Count; i++)
                _extensions[i] = _extensions[i].ToLower();
        }

        internal bool HasExtension(string extensionName)
        {
            return _extensions.Contains(extensionName.ToLower());
        }

        /// <summary>Gets all <see cref="T:Molten.IDisplayOutput" /> devices attached to the current <see cref="T:Molten.IDisplayAdapter" />.</summary>
        /// <param name="outputList">The output list.</param>
        public void GetAttachedOutputs(List<IDisplayOutput> outputList)
        {
            outputList.AddRange(_connectedOutputs);
        }

        public IDisplayOutput GetOutput(int id)
        {
            if (id >= _connectedOutputs.Count)
                throw new IndexOutOfRangeException($"ID was {id} while there are only {_connectedOutputs.Count} connected display outputs.");

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
                _activeOutputs.Add(output as DisplayOutputGL);
                OnOutputActivated?.Invoke(output);
            }
        }

        public void RemoveActiveOutput(IDisplayOutput output)
        {
            if (output.Adapter != this)
                throw new AdapterOutputException(output, "Cannot remove active output: Bound to another adapter.");

            if (_activeOutputs.Remove(output as DisplayOutputGL))
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

        public string Name { get; private set; }

        public double DedicatedVideoMemory => 0;

        public double DedicatedSystemMemory => 0;

        public double SharedSystemMemory => 0;

        public int ID { get; private set; }

        public GraphicsAdapterVendor Vendor { get; private set; }

        public int OutputCount => _connectedOutputs.Count;

        public IDisplayManager Manager => _manager;
    }
}
