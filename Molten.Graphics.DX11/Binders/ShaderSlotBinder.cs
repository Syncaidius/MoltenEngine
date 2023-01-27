namespace Molten.Graphics
{
    internal unsafe class ShaderSlotBinder<T> : GraphicsSlotBinder<ShaderCompositionDX11<T>>
        where T : unmanaged
    {
        ContextShaderStage<T> _stage;

        internal ShaderSlotBinder(ContextShaderStage<T> stage)
        {
            _stage = stage;
        }

        public override void Bind(GraphicsSlot<ShaderCompositionDX11<T>> slot, ShaderCompositionDX11<T> value)
        {
            _stage.SetShader(value.PtrShader, null, 0);
        }

        public override void Unbind(GraphicsSlot<ShaderCompositionDX11<T>> slot, ShaderCompositionDX11<T> value)
        {
            _stage.SetShader(null, null, 0);
        }
    }
}
