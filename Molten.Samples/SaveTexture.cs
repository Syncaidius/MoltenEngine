using Molten.Graphics;
using Molten.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Samples
{
    public class SaveTextureSample : SampleSceneGame
    {
        public override string Description => "A demonstration of saving a texture to file.";

        SceneObject _parent;
        SceneObject _child;
        IMesh<CubeArrayVertex> _mesh;

        public SaveTextureSample() : base("Save Texture") { }

        protected override void OnInitialize(Engine engine)
        {
            base.OnInitialize(engine);    

            _mesh = Engine.Renderer.Resources.CreateMesh<CubeArrayVertex>(36);
            _mesh.SetVertices(SampleVertexData.TextureArrayCubeVertices);

            ContentRequest cr = engine.Content.BeginRequest("assets/");
            cr.Load<IMaterial>("BasicTexture.mfx");
            cr.Load<ITexture2D>("dds_dxt5.dds;compress=false");
            cr.Load<TextureData>("dds_dxt5.dds");
            cr.OnCompleted += Cr_OnCompleted;
            cr.Commit();

            SpawnParentChild(_mesh, Vector3F.Zero, out _parent, out _child);
        }

        private void Cr_OnCompleted(ContentRequest cr)
        {
            IMaterial mat = cr.Get<IMaterial>(0);

            if (mat == null)
            {
                Exit();
                return;
            }

            // Manually construct a 2D texture array from the 3 textures we requested earlier
            ITexture2D texture = cr.Get<ITexture2D>("dds_dxt5.dds");
            mat.SetDefaultResource(texture, 0);
            _mesh.Material = mat;

            Texture2DProperties p = texture.Get2DProperties();
            p.Flags = TextureFlags.Staging;
            ITexture2D staging = Engine.Renderer.Resources.CreateTexture2D(p);

            TextureData loadedData = cr.Get<TextureData>("dds_dxt5.dds");
            loadedData.Decompress(Log);
            cr = Engine.Content.BeginRequest("assets/");
            cr.Save("saved_recompressed_texture_raw.dds;compress=dxt5", loadedData);
            cr.Commit();


            texture.GetData(staging, (data) =>
            {
                cr = Engine.Content.BeginRequest("assets/");
                cr.Save("saved_texture.dds;compress=dxt5", data);
                cr.Commit();
            });

            //ITexture2D decompressedTexture = Engine.Renderer.Resources.CreateTexture2D(loadedData);
            //mat.SetDefaultResource(decompressedTexture, 0);
        }

        protected override void OnUpdate(Timing time)
        {
            RotateParentChild(_parent, _child, time);

            // Save a screenhot of the window surface when space is pressed!
            if (Keyboard.IsTapped(Key.Space))
            {
                ContentRequest cr = Engine.Content.BeginRequest("assets/");
                cr.Save<ITexture2D>("screenshot.png", Window);
                cr.Commit();
            }

            base.OnUpdate(time);
        }
    }
}
