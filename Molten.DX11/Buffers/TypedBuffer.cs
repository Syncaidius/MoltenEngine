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
    /// <summary>A typed, structured buffer. This is the application-equivilent of a typed Buffer and RWBuffer in HLSL. </summary>
    /// <typeparam name="T"></typeparam>
    internal class TypedBuffer<T> : GraphicsBuffer where T : struct
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

        Type _bufferType;

        /// <summary>Creates a new instance of <see cref="StructuredBuffer"/>.</summary>
        /// <param name="device">The graphics device to bind the buffer to.</param>
        /// <param name="stride">The expected size of 1 element, in bytes.</param>
        /// <param name="capacity">The number of elements the buffer should be able to hold.</param>
        /// <param name="binding">Custom bind flags.</param>
        /// <param name="flags"></param>
        /// <param name="unorderedAccess">If true, the buffer is given Read-Write access and a UAV is created for it. This is known as an RWStructuredBuffer in HLSL.</param>
        public TypedBuffer(
            GraphicsDeviceDX11 device, 
            BufferMode flags, 
            int capacity, 
            bool unorderedAccess = false, 
            bool shaderResource = true)
            : base(device, 
                  flags,
                  (shaderResource ? BindFlags.ShaderResource : BindFlags.None) | (unorderedAccess ? BindFlags.UnorderedAccess : BindFlags.None), 
                  capacity, 
                  ResourceOptionFlags.BufferStructured)
        {
            _bufferType = typeof(T);
            ValidateType();
        }

        /// <summary>Creates a new instance of <see cref="StructuredBuffer"/>.</summary>
        /// <param name="device">The graphics device to bind the buffer to.</param>
        /// <param name="stride">The expected size of 1 element, in bytes.</param>
        /// <param name="capacity">The number of elements the buffer should be able to hold.</param>
        public TypedBuffer(GraphicsDeviceDX11 device, BufferMode mode, int stride, int capacity)
            : base(device, 
                  mode, 
                  BindFlags.ShaderResource, 
                  capacity, 
                  ResourceOptionFlags.BufferStructured)
        {
            ValidateType();
        }

        private void ValidateType()
        {
            for (int i = 0; i < AcceptedTypes.Length; i++)
            {
                if (_bufferType == AcceptedTypes[i])
                    return;
            }

            // If we reach here, the type was not valid.
            throw new InvalidOperationException("Typed buffers only accept scalar, vector and Matrix2x2 value types.");
        }

        internal override void CreateResources(int stride, int byteoffset, int elementCount)
        {
            Type allocatedType = typeof(T);
            if (allocatedType != _bufferType)
                throw new InvalidOperationException("Typed buffers can only accept the data type they were initialized with.");
            
            // No SRV if the shader resource flag isn't present.
            if ((Description.BindFlags & BindFlags.ShaderResource) != BindFlags.ShaderResource)
                return;

            // Create a new shader resource view
            SRV = new ShaderResourceView(Device.D3d, _buffer, new ShaderResourceViewDescription()
            {
                BufferEx = new ShaderResourceViewDescription.ExtendedBufferResource()
                {
                    ElementCount = elementCount,
                    FirstElement = 0,
                    Flags = ShaderResourceViewExtendedBufferFlags.None,
                },
                Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Buffer,
                Format = SharpDX.DXGI.Format.Unknown,
            });
        }
    }
}
