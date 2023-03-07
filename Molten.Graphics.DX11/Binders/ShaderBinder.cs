namespace Molten.Graphics
{
    internal unsafe class ShaderBinder : GraphicsSlotBinder<HlslShader>
    {
        public override void Bind(GraphicsSlot<HlslShader> slot, HlslShader value) { }

        public override void Unbind(GraphicsSlot<HlslShader> slot, HlslShader value) { }
    }
}
