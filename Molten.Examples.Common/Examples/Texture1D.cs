using Molten.Graphics;

namespace Molten.Examples;

[Example("Texture 1D", "Demonstrates how 1D textures are used")]
public class Texture1DCube : MoltenExample
{
    ContentLoadHandle _hShader;
    ContentLoadHandle _hTexture;

    protected override void OnLoadContent(ContentLoadBatch loader)
    {
        base.OnLoadContent(loader);

        _hShader = loader.Load<HlslShader>("assets/BasicTexture1D.mfx");
        _hTexture = loader.Load<ITexture1D>("assets/1d_1.png");
        loader.OnCompleted += Loader_OnCompleted;
    }

    private void Loader_OnCompleted(ContentLoadBatch loader)
    {
        if (!_hShader.HasAsset())
        {
            Close();
            return;
        }

        HlslShader shader = _hShader.Get<HlslShader>();
        ITexture1D texture = _hTexture.Get<ITexture1D>();

        shader.SetDefaultResource(texture, 0);
        TestMesh.Shader = shader;
    }

    protected override Mesh GetTestCubeMesh()
    {
        return Engine.Renderer.Device.CreateMesh(SampleVertexData.TextureArrayCubeVertices);
    }
}
