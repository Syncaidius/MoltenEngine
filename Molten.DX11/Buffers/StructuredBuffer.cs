using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.D3DCompiler;
using System.Runtime.InteropServices;

namespace Molten.Graphics
{
    using Buffer = SharpDX.Direct3D11.Buffer;

    internal class StructuredBuffer : GraphicsBuffer
    {
        static Type[] AcceptedTypes = new Type[]
        {
                    typeof(float),
                    typeof(int),
                    typeof(uint),
                    typeof(short),
                    typeof(ushort),
                    typeof(byte),
                    typeof(sbyte),
                    typeof(Matrix2F),
                    typeof(Vector2F),
                    typeof(Vector3F),
                    typeof(Vector4F),
                    typeof(Vector2I),
                    typeof(Vector3I),
                    typeof(Vector4I),
                    typeof(Vector2UI),
                    typeof(Vector3UI),
                    typeof(Vector4UI),
        };

        /// <summary>Creates a new instance of <see cref="StructuredBuffer"/>.</summary>
        /// <param name="device">The graphics device to bind the buffer to.</param>
        /// <param name="stride">The expected size of 1 element, in bytes.</param>
        /// <param name="elementCapacity">The number of elements the buffer should be able to hold.</param>
        internal StructuredBuffer(GraphicsDevice device, BufferMode mode, int stride, int elementCapacity, bool unorderedAccess = false, bool shaderResource = true, Array initialData = null)
            : base(device,
                  mode,
                  (shaderResource ? BindFlags.ShaderResource : BindFlags.None) | (unorderedAccess ? BindFlags.UnorderedAccess : BindFlags.None),
                  stride * elementCapacity,
                  ResourceOptionFlags.BufferStructured, StagingBufferFlags.None, stride, initialData)
        { }

        //protected override void OnSegmentCreated(Buffer buffer)
        //{
        //    // No SRV if the shader resource flag isn't present.
        //    if ((Description.BindFlags & BindFlags.ShaderResource) != BindFlags.ShaderResource)
        //        return;

        //    // Create a new shader resource view
        //    //SRV = new ShaderResourceView(Device.D3d, buffer, new ShaderResourceViewDescription()
        //    //{
        //    //    Buffer = new ShaderResourceViewDescription.BufferResource()
        //    //    {
        //    //        ElementCount = _capacity,
        //    //        FirstElement = 0,
        //    //    },
        //    //    Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Buffer,
        //    //    Format = SharpDX.DXGI.Format.Unknown,
        //    //});
        //}
    }
}
