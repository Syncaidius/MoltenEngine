using Molten.Graphics;
using Molten.Input;

namespace Molten.Samples
{
    public class SaveTextureSample : SampleGame
    {
        ContentLoadHandle _hMaterial;
        ContentLoadHandle _hTexture;
        ContentLoadHandle _hTexData;

        public override string Description => "A demonstration of saving a texture to file.";

        public SaveTextureSample() : base("Save Texture") { }

        protected override void OnLoadContent(ContentLoadBatch loader)
        {
            _hMaterial = loader.Load<IMaterial>("assets/BasicTexture.mfx");
            _hTexture = loader.Load<ITexture2D>("assets/dds_dxt5.dds");
            _hTexData = loader.Load<TextureData>("assets/dds_dxt5.dds");
            loader.OnCompleted += Loader_OnCompleted;
        }

        private void Loader_OnCompleted(ContentLoadBatch loader)
        {
            if (!_hMaterial.HasAsset())
            {
                Exit();
                return;
            }

            IMaterial mat = _hMaterial.Get<IMaterial>();

            // Manually construct a 2D texture array from the 3 textures we requested earlier
            ITexture2D texture = _hTexture.Get<ITexture2D>();
            mat.SetDefaultResource(texture, 0);
            TestMesh.Material = mat;

            Texture2DProperties p = texture.Get2DProperties();
            p.Flags = TextureFlags.Staging;
            ITexture2D staging = Engine.Renderer.Resources.CreateTexture2D(p);

            TextureData loadedData = _hTexData.Get<TextureData>();
            loadedData.Decompress(Log);

            TextureParameters texParams = new TextureParameters()
            {
                BlockCompressionFormat = DDSFormat.DXT5,
            };

            Engine.Content.SaveToFile("assets/saved_recompressed_texture_raw.dds", loadedData, parameters: texParams);

            texture.GetData(staging, (data) =>
            {
                ContentSaveHandle saveHandle = Engine.Content.SaveToFile("assets/saved_texture.dds", data, parameters: texParams);
            });
        }

        protected override IMesh GetTestCubeMesh()
        {
            IMesh<CubeArrayVertex>  cube = Engine.Renderer.Resources.CreateMesh<CubeArrayVertex>(36);
            cube.SetVertices(SampleVertexData.TextureArrayCubeVertices);
            return cube;
        }

        protected override void OnUpdate(Timing time)
        {
            // Save a screenhot of the window surface when space is pressed!
            if (Keyboard.IsTapped(KeyCode.Space))
            {
                ContentSaveHandle saveHandle = Engine.Content.SaveToFile("assets/screenshot.png", Window);
                // TODO Add some loading/saving UI
            }

            base.OnUpdate(time);
        }
    }
}
