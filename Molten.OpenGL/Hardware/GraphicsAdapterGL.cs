using OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Khronos;
using KhronosApi = Khronos.KhronosApi;

namespace Molten.Graphics
{
    public class GraphicsAdapterGL : IDisplayAdapter
    {
        const int GPU_MEMORY_INFO_DEDICATED_VIDMEM_NVX = 0x9047;
        const int TEXTURE_FREE_MEMORY_ATI = 0x87FC;

        public event DisplayOutputChanged OnOutputActivated;
        public event DisplayOutputChanged OnOutputDeactivated;

        List<DisplayOutputGL> _connectedOutputs;
        List<DisplayOutputGL> _activeOutputs;

        DisplayManagerGL _manager;
        List<string> _extensions;

        internal unsafe GraphicsAdapterGL(DisplayManagerGL manager, int id)
        {
            ID = id;
            _manager = manager;
            _connectedOutputs = new List<DisplayOutputGL>();
            _activeOutputs = new List<DisplayOutputGL>();
            _extensions = new List<string>();
            PopulateInfo();

            _connectedOutputs.Add(new DisplayOutputGL(this, "Default", new Rectangle(0, 0, 1920, 1080))); // TODO detect display size(s)
        }

        private void PopulateInfo()
        {

            Name = Gl.GetString(StringName.Renderer);
            string strVendor = Gl.GetString(StringName.Vendor);

            if (strVendor != null)
            {
                strVendor = strVendor.ToLower();
                if (strVendor.Contains("amd") || strVendor.Contains("ati"))
                {
                    Vendor = GraphicsAdapterVendor.AMD;
                    // NOTE: https://www.khronos.org/registry/OpenGL/extensions/ATI/ATI_meminfo.txt
                    //      "In any case, it is highly reccommended that the information be returned in kilobytes."

                    /*  memInfo[0] - total memory free in the pool
                        memInfo[1] - largest available free block in the pool
                        memInfo[2] - total auxiliary memory free
                        memInfo[3] - largest auxiliary free block */

                    int[] memInfo = new int[48];
                    Gl.Get((GetPName)TEXTURE_FREE_MEMORY_ATI, memInfo);
                    DedicatedVideoMemory = ByteMath.ToMegabytes(ByteMath.FromKilobytes(memInfo[0]));
                    SharedSystemMemory = ByteMath.ToMegabytes(ByteMath.FromKilobytes(memInfo[2]));
                }
                else if (strVendor.Contains("intel"))
                {
                    Vendor = GraphicsAdapterVendor.Intel;
                }
                else if (strVendor.Contains("nvidia"))
                {
                    Vendor = GraphicsAdapterVendor.Nvidia;

                    // NOTE: https://www.khronos.org/registry/OpenGL/extensions/NVX/NVX_gpu_memory_info.txt
                    /* GPU_MEMORY_INFO_DEDICATED_VIDMEM_NVX 
                        - dedicated video memory, total size (in kb) of the GPU memory */

                    int nvDedicated = 0;
                    Gl.Get((GetPName)GPU_MEMORY_INFO_DEDICATED_VIDMEM_NVX, out nvDedicated);
                    DedicatedSystemMemory = ByteMath.ToMegabytes(ByteMath.FromKilobytes(nvDedicated));
                }
                else
                {
                    Vendor = GraphicsAdapterVendor.Unknown;
                }
            }

            string extensions = Gl.GetString(StringName.Extensions);
            if (extensions != null)
            {
                _extensions.AddRange(extensions.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                for (int i = 0; i < _extensions.Count; i++)
                    _extensions[i] = _extensions[i].ToLower();
            }
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

        public double DedicatedVideoMemory { get; private set; }

        public double DedicatedSystemMemory { get; private set; }

        public double SharedSystemMemory { get; private set; }

        public int ID { get; private set; }

        public GraphicsAdapterVendor Vendor { get; private set; }

        public int OutputCount => _connectedOutputs.Count;

        public IDisplayManager Manager => _manager;
    }
}
 