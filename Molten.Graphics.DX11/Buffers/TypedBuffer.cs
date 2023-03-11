using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace Molten.Graphics
{
    /// <summary>A typed, structured buffer. This is the application-equivilent of a typed Buffer and RWBuffer in HLSL. </summary>
    /// <typeparam name="T"></typeparam>
    internal unsafe class TypedBuffer<T> : GraphicsBuffer where T : struct
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
        /// <param name="numElements"></param>
        /// <param name="shaderResource"></param>
        /// <param name="flags"></param>
        /// <param name="unorderedAccess">If true, the buffer is given Read-Write access and a UAV is created for it. This is known as an RWStructuredBuffer in HLSL.</param>
        public TypedBuffer(
            DeviceDX11 device, 
            BufferMode flags, 
            uint stride,
            uint numElements,
            bool unorderedAccess = false, 
            bool shaderResource = true)
            : base(device, 
                  flags,
                  (shaderResource ? BindFlag.ShaderResource : 0) | (unorderedAccess ? BindFlag.UnorderedAccess : 0), 
                  stride, numElements, 
                  ResourceMiscFlag.BufferStructured)
        {
            _bufferType = typeof(T);
            ValidateType();
        }

        /// <summary>Creates a new instance of <see cref="StructuredBuffer"/>.</summary>
        /// <param name="device">The graphics device to bind the buffer to.</param>
        /// <param name="stride">The expected size of 1 element, in bytes.</param>
        /// <param name="numElements"></param>
        /// <param name="mode"></param>
        public TypedBuffer(DeviceDX11 device, BufferMode mode, uint stride, uint numElements)
            : base(device, 
                  mode, 
                  BindFlag.ShaderResource, 
                  stride,
                  numElements, 
                  ResourceMiscFlag.BufferStructured)
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

        protected override void CreateResources()
        {
            Type allocatedType = typeof(T);
            if (allocatedType != _bufferType)
                throw new InvalidOperationException("Typed buffers can only accept the data type they were initialized with.");

            if (HasFlags(BindFlag.ShaderResource))
            {
                SRV.Desc = new ShaderResourceViewDesc1()
                {
                    BufferEx = new BufferexSrv()
                    {
                        NumElements = ElementCount,
                        FirstElement = 0,
                        Flags = 0 // See: https://docs.microsoft.com/en-us/windows/win32/api/d3d11/ne-d3d11-d3d11_bufferex_srv_flag
                    },
                    ViewDimension = D3DSrvDimension.D3D11SrvDimensionBuffer,
                    Format = Format.FormatUnknown,
                };

                SRV.Create(this);
            }

            // See UAV notes: https://docs.microsoft.com/en-us/windows/win32/direct3d11/overviews-direct3d-11-resources-intro#raw-views-of-buffers
            if (HasFlags(BindFlag.UnorderedAccess))
            {
                UnorderedAccessViewDesc uavDesc = new UnorderedAccessViewDesc()
                {
                    Format = Format.FormatUnknown,
                    ViewDimension = UavDimension.Buffer,
                    Buffer = new BufferUav()
                    {
                        NumElements = ElementCount,
                        FirstElement = 0,
                        Flags = (uint)BufferUavFlag.Raw,
                    }
                };
                UAV.Create(this);
            }
        }
    }
}
