using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal abstract class GraphicsPresetBank<T, E> : IDisposable
        where T : PipelineObject
        where E: struct, IConvertible
    {
        protected T[] _presets;

        internal GraphicsPresetBank()
        {
            IConvertible last = EnumHelper.GetLastValue<E>();
            int presetArraySize = (int)last + 1;
            _presets = new T[presetArraySize];
        }

        public void Dispose()
        {
            foreach (T preset in _presets)
                preset.Dispose();
        }

        protected void AddPreset(E id, T preset)
        {
            int idVal = (int)(object)id;
            _presets[idVal] = preset;
        }

        internal abstract T GetPreset(E value);
    }
}
