namespace Molten.Graphics
{
    internal unsafe class ShaderSlotBinder : GraphicsSlotBinder<ShaderComposition>
    {
        ShaderStageDX11 _stage;

        internal ShaderSlotBinder(ShaderStageDX11 stage)
        {
            _stage = stage;
        }

        public override void Bind(GraphicsSlot<ShaderComposition> slot, ShaderComposition value)
        {
            _stage.SetShader(value.PtrShader, null, 0);
        }

        public override void Unbind(GraphicsSlot<ShaderComposition> slot, ShaderComposition value)
        {
            _stage.SetShader(null, null, 0);
        }
    }
}
