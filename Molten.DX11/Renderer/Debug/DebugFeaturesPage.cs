using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class DebugFeaturesPage : DebugOverlayPage
    {
        public override void Render(SpriteFont font, RendererDX11 renderer, SpriteBatch batch, SceneRenderData scene, IRenderSurface surface)
        {
            Vector2F pos = new Vector2F(3, 3);
            GraphicsDX11Features features = renderer.Device.Features;
            IDisplayAdapter adapter = renderer.Device.DisplayManager.SelectedAdapter;

            batch.DrawString(font, $"Device: {adapter.Name}", pos, Color.Yellow);
            pos.Y += 20; batch.DrawString(font, $"Total VRAM: {adapter.DedicatedVideoMemory}MB", pos, Color.Yellow);
            pos.Y += 20; batch.DrawString(font, $"Shared RAM: {adapter.SharedSystemMemory}MB", pos, Color.Yellow);
            pos.Y += 20; batch.DrawString(font, $"Vendor: {adapter.Vendor}", pos, Color.Yellow);
            pos.Y += 20; batch.DrawString(font, "Supported features: ", pos, Color.Yellow);
            pos.Y += 20; batch.DrawString(font, $"   Feature level: {features.Level}", pos, Color.Yellow);
            pos.Y += 20; batch.DrawString(font, $"   Command lists: {features.CommandLists}", pos, Color.Yellow);
            pos.Y += 20; batch.DrawString(font, $"   Cube-map arrays: {features.CubeMapArrays}", pos, Color.Yellow);
            pos.Y += 20; batch.DrawString(font, $"   Concurrent resources: {features.ConcurrentResources}", pos, Color.Yellow);
            pos.Y += 20; batch.DrawString(font, $"   Hardware instancing: {features.HardwareInstancing}", pos, Color.Yellow);
            pos.Y += 20; batch.DrawString(font, $"   Max anisotropy: {features.MaxAnisotropy}", pos, Color.Yellow);
            pos.Y += 20; batch.DrawString(font, $"   Max constant buffer slots: {features.MaxConstantBufferSlots}", pos, Color.Yellow);
            pos.Y += 20; batch.DrawString(font, $"   Max cube-map dimension: {features.MaxCubeMapDimension}", pos, Color.Yellow);
            pos.Y += 20; batch.DrawString(font, $"   Max shader model: {features.MaximumShaderModel}", pos, Color.Yellow);
            pos.Y += 20; batch.DrawString(font, $"   Max index buffer slots: {features.MaxIndexBufferSlots}", pos, Color.Yellow);
            pos.Y += 20; batch.DrawString(font, $"   Max input resource slots: {features.MaxInputResourceSlots}", pos, Color.Yellow);
            pos.Y += 20; batch.DrawString(font, $"   Max input sampler slots: {features.MaxSamplerSlots}", pos, Color.Yellow);
            pos.Y += 20; batch.DrawString(font, $"   Max primitive count: {features.MaxPrimitiveCount}", pos, Color.Yellow);
            pos.Y += 20; batch.DrawString(font, $"   Max texture dimension: {features.MaxTextureDimension}", pos, Color.Yellow);
            pos.Y += 20; batch.DrawString(font, $"   Max texture repeat: {features.MaxTextureRepeat}", pos, Color.Yellow);
            pos.Y += 20; batch.DrawString(font, $"   Max UAVs: {features.MaxUnorderedAccessViews}", pos, Color.Yellow);
            pos.Y += 20; batch.DrawString(font, $"   Max vertex buffer slots: {features.MaxVertexBufferSlots}", pos, Color.Yellow);
            pos.Y += 20; batch.DrawString(font, $"   Max volume extent: {features.MaxVolumeExtent}", pos, Color.Yellow);
            pos.Y += 20; batch.DrawString(font, $"   Non-power-of-two textures: {features.NonPowerOfTwoTextures}", pos, Color.Yellow);
            pos.Y += 20; batch.DrawString(font, $"   Occlusion queries: {features.OcclusionQueries}", pos, Color.Yellow);
            pos.Y += 20; batch.DrawString(font, $"   Simultaneous render surfaces: {features.SimultaneousRenderSurfaces}", pos, Color.Yellow);
            pos.Y += 20; batch.DrawString(font, $"   Texture arrays: {features.TextureArrays}", pos, Color.Yellow);

        }
    }
}
