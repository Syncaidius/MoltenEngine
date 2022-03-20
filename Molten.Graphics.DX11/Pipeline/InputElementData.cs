using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal unsafe class InputElementData : IDisposable
    {
        internal InputElementDesc[] Elements { get; }

        /// <summary>
        /// Contains extra/helper information about input elements
        /// </summary>
        internal string[] Names { get; }

        internal InputElementData(uint elementCount)
        {
            Elements = new InputElementDesc[elementCount];
            Names = new string[elementCount];
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
                    return false;
                }
                else
                {
                    uint oi = 0;
                    for (uint i = startIndex; i < endIndex; i++)
                    {
                        if (other.Names[oi] != Names[i] ||
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
