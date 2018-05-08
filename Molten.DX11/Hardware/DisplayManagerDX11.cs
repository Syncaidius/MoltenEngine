using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public class DisplayManagerDX11 : GraphicsDisplayManager<Factory1, Adapter1, AdapterDescription1, Output1>
    {
        protected override AdapterDescription1 GetAdapterDescription(Adapter1 adapter)
        {
            return adapter.Description1;
        }

        protected override Adapter1[] GetDxgiAdapters(Factory1 factory)
        {
            return factory.Adapters1;
        }

        protected override GraphicsAdapterDX<Adapter1, AdapterDescription1, Output1> InstanciateAdapter(Adapter1 adapter, AdapterDescription1 desc, int id)
        {
            // Get the adapter's outputs and convert them to the correct type.
            Output[] outputs = adapter.Outputs;
            Output1[] result = new Output1[outputs.Length];
            for (int i = 0; i < result.Length; i++)
                result[i] = new Output1(outputs[i].NativePointer);

            return new GraphicsAdapterDX<Adapter1, AdapterDescription1, Output1>(this, adapter, desc, result, id);
        }
    }
}
