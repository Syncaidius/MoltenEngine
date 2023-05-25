namespace Molten.Graphics
{
    public class ShaderReflection : IDisposable
    {
        public unsafe class ReflectionPtr : IDisposable
        {
            internal void* Ptr;

            public void Dispose()
            {
                EngineUtil.Free(ref Ptr);
            }

            public static implicit operator void*(ReflectionPtr ptr) => ptr != null ? ptr.Ptr : null;
        }

        List<ReflectionPtr> _disposablePointers = new List<ReflectionPtr>();

        /// <summary>
        /// Gets the primitive topology that is expected as input for the geometry shader stage.
        /// </summary>
        public GeometryHullTopology GSInputPrimitive;

        /// <summary>
        /// Gets a list of required API-specific extensions, if any.
        /// </summary>
        public List<string> RequiredExtensions { get; } = new List<string>();

        public List<ShaderResourceInfo> BoundResources { get; } = new List<ShaderResourceInfo>();

        public List<ShaderParameterInfo> InputParameters { get; } = new List<ShaderParameterInfo>();

        public List<ShaderParameterInfo> OutputParameters { get; } = new List<ShaderParameterInfo>();

        public Dictionary<string, ConstantBufferInfo> ConstantBuffers { get; } = new Dictionary<string, ConstantBufferInfo>();

        /// <summary>
        /// A helper method for tracking reflection-related calls to <see cref="EngineUtil.Alloc(nuint)"/>, which ensures the pointers are freed after
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public unsafe ReflectionPtr NewPtr(nuint size)
        {
            ReflectionPtr ptr = new ReflectionPtr()
            {
                Ptr = EngineUtil.Alloc(size),
            };
            _disposablePointers.Add(ptr);
            return ptr;
        }

        public void Dispose()
        {
            foreach (ReflectionPtr ptr in _disposablePointers)
                ptr.Dispose();

            _disposablePointers.Clear();
        }
    }
}
