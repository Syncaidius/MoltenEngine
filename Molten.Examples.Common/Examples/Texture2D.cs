using Molten.Graphics;

namespace Molten.Examples;

[Example("Texture 2D", "Demonstrates how 2D textures are used")]
public class Texture2DCube : MoltenExample
{
    ContentLoadHandle _hShader;
    ContentLoadHandle _hTexture;

    protected override void OnLoadContent(ContentLoadBatch loader)
    {
        base.OnLoadContent(loader);

        _hShader = loader.Load<Shader>("assets/BasicTexture.json");
        _hTexture = loader.Load<ITexture2D>("assets/png_test.png");
        loader.OnCompleted += Loader_OnCompleted;
    }

    private void Loader_OnCompleted(ContentLoadBatch loader)
    {
        if (!_hShader.HasAsset())
        {
            Close();
            return;
        }

        Shader shader = _hShader.Get<Shader>();
        ITexture2D texture = _hTexture.Get<ITexture2D>();
        shader[ShaderBindType.Resource, 0] = texture;
        TestMesh.Shader = shader;
    }

    protected override Mesh GetTestCubeMesh()
    {
        return Engine.Renderer.Device.Resources.CreateMesh<CubeArrayVertex>(SampleVertexData.TextureArrayCubeVertices);
    }
}
