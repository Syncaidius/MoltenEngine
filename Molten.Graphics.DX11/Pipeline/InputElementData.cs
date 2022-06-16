using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal unsafe class InputElementData : IDisposable
    {
        internal struct InputElementMetadata
        {
            public string Name;

            public D3DName SystemValueType;
        }

        internal InputElementDesc[] Elements { get; }

        /// <summary>
        /// Contains extra/helper information about input elements
        /// </summary>
        internal InputElementMetadata[] Metadata { get; }

        internal InputElementData(uint elementCount)
        {
            Elements = new InputElementDesc[elementCount];
            Metadata = new InputElementMetadata[elementCount];
        }

        public bool IsCompatible(InputElementData other)
        {
            return IsCompatible(other, 0);
        }

        public bool IsCompatible(InputElementData other, uint startIndex)
        {
            if (startIndex >= Elements.Length)
            {
                return false;
            }
            else
            {
                uint endIndex = startIndex + (uint)other.Elements.Length;
                if (endIndex > Elements.Length)
                {
                    // If the remaining elements are system values (SV_ prefix), allow them.
                    for(int i = Elements.Length; i < endIndex; i++)
                    {
                        if (other.Metadata[i].SystemValueType == D3DName.D3DNameUndefined)
                            return false;
                    }
                }
                else
                {
                    uint oi = 0;
                    for (uint i = startIndex; i < endIndex; i++)
                    {
                        if (other.Metadata[oi].Name != Metadata[i].Name ||
                            other.Elements[oi].SemanticIndex != Elements[i].SemanticIndex)
                            return false;

                        oi++;
                    }
                }
            }

            return true;
        }

        public void Dispose()
        {
            // Dispose of element string pointers, since they were statically-allocated by Silk.NET
            for (uint i = 0; i < Elements.Length; i++)
                SilkMarshal.Free((nint)Elements[i].SemanticName);
        }
    }
}
