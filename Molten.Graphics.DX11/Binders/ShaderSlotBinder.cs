namespace Molten.Graphics
{
    internal unsafe class ShaderSlotBinder<T> : GraphicsSlotBinder<ShaderComposition<T>>
        where T : unmanaged
    {
        ContextShaderStage<T> _stage;

        internal ShaderSlotBinder(ContextShaderStage<T> stage)
        {
            _stage = stage;
        }

        public override void Bind(GraphicsSlot<ShaderComposition<T>> slot, ShaderComposition<T> value)
        {
            _stage.SetShader(value.PtrShader, null, 0);
        }

        public override void Unbind(GraphicsSlot<ShaderComposition<T>> slot, ShaderComposition<T> value)
        {
            _stage.SetShader(null, null, 0);
        }
    }
}
