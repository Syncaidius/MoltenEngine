using Molten.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>A class for managing the </summary>
    public class MaterialManager : IMaterialManager
    {
        ThreadedDictionary<string, Material> _materialsByName = new ThreadedDictionary<string, Material>();
        ThreadedList<Material> _materials = new ThreadedList<Material>();

        internal void AddMaterial(Material material)
        {
            _materials.Add(material);
            _materialsByName.Add(material.Name.ToLower(), material);
        }

        public IMaterial this[string materialName]
        {
            get { return _materialsByName[materialName.ToLower()]; }
        }
    }
}
