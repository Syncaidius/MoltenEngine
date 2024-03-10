using Molten.Graphics;
using Molten.Input;

namespace Molten.Examples;

[Example("Save Texture", "Demonstrates saving a texture to file ")]
public class SaveTexture : MoltenExample
{
    ContentLoadHandle _hShader;
    ContentLoadHandle _hTexture;
    ContentLoadHandle _hTexData;

    protected override void OnLoadContent(ContentLoadBatch loader)
    {
        base.OnLoadContent(loader);

        _hShader = loader.Load<Shader>("assets/BasicTexture.json");
        _hTexture = loader.Load<ITexture2D>("assets/dds_dxt5.dds");
        _hTexData = loader.Load<TextureData>("assets/dds_dxt5.dds");
        loader.OnCompleted += Loader_OnCompleted;
    }

    private void Loader_OnCompleted(ContentLoadBatch loader)
    {
        if (!_hShader.HasAsset())
        {
            Close();
            return;
        }

        Shader mat = _hShader.Get<Shader>();

        // Manually construct a 2D texture array from the 3 textures we requested earlier
        ITexture2D tex = _hTexture.Get<ITexture2D>();
        mat[ShaderBindType.Resource, 0] = tex;
        TestMesh.Shader = mat;

        GraphicsTexture texStaging = Engine.Renderer.Device.Resources.CreateStagingTexture(tex);
        TextureData loadedData = _hTexData.Get<TextureData>();
        loadedData.Decompress(Log);

        TextureParameters texParams = new TextureParameters()
        {
            BlockCompressionFormat = DDSFormat.DXT5,
        };

        Engine.Content.SaveToFile("assets/saved_recompressed_texture_raw.dds", loadedData, parameters: texParams);
        tex.CopyTo(GpuPriority.EndOfFrame, texStaging, (task, successful) =>
        {
            texStaging.GetData(GpuPriority.EndOfFrame, (data) =>
            {
                ContentSaveHandle saveHandle = Engine.Content.SaveToFile("assets/saved_texture.dds", data, parameters: texParams);
            });
        });
    }

    protected override Mesh GetTestCubeMesh()
    {
        Mesh<CubeArrayVertex> cube = Engine.Renderer.Device.Resources.CreateMesh(SampleVertexData.TextureArrayCubeVertices);
        return cube;
    }

    protected override void OnUpdate(Timing time)
    {
        base.OnUpdate(time);

        // Save a screenhot of the window surface when space is pressed!
        if (Keyboard.IsTapped(KeyCode.Space))
        {
            ContentSaveHandle saveHandle = Engine.Content.SaveToFile("assets/screenshot.png", Surface);
            // TODO Add some loading/saving UI
        }
    }
}
