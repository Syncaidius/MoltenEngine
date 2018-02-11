using Molten.Graphics;
using Msdfgen;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Typography.Contours;
using Typography.OpenFont;
using Typography.Rendering;

namespace Molten.Samples
{
    public class MsdfSample : SampleSceneGame
    {
        public override string Description => "multi-channel signed distance field sample.";

        Scene _scene;
        SceneObject _parent;
        SceneObject _child;
        List<Matrix> _positions;
        Random _rng;
        SceneCameraComponent _cam;
        Camera2D _cam2D;
        ISpriteFont _font;
        IMesh<VertexTexture> _mesh;
        ITexture2D _msdfTexture;

        public MsdfSample(EngineSettings settings = null) : base("MSDF", settings)
        {

        }

        protected override void OnInitialize(Engine engine)
        {
            base.OnInitialize(engine);
            _cam = new SceneCameraComponent()
            {
                OutputSurface = Window,
                OutputDepthSurface = WindowDepthSurface,
            };

            _cam2D = new Camera2D()
            {
                OutputSurface = Window,
                OutputDepthSurface = WindowDepthSurface,
            };

            _rng = new Random();
            _positions = new List<Matrix>();
            _scene = CreateScene("Test");
            _scene.OutputCamera = _cam;
            _font = engine.Renderer.Resources.CreateFont("arial", 36);

            ContentRequest cr = engine.Content.StartRequest();
            cr.Load<ITexture2D>("png_test.png;mipmaps=true");
            cr.Load<IMaterial>("Basictexture.sbm");
            cr.OnCompleted += Cr_OnCompleted;
            cr.Commit();

            _mesh = Engine.Renderer.Resources.CreateMesh<VertexTexture>(36);

            VertexTexture[] verts = new VertexTexture[]{
               new VertexTexture(new Vector3(-1,-1,-1), new Vector2(0,1)), //front
               new VertexTexture(new Vector3(-1,1,-1), new Vector2(0,0)),
               new VertexTexture(new Vector3(1,1,-1), new Vector2(1,0)),
               new VertexTexture(new Vector3(-1,-1,-1), new Vector2(0,1)),
               new VertexTexture(new Vector3(1,1,-1), new Vector2(1, 0)),
               new VertexTexture(new Vector3(1,-1,-1), new Vector2(1,1)),

               new VertexTexture(new Vector3(-1,-1,1), new Vector2(1,0)), //back
               new VertexTexture(new Vector3(1,1,1), new Vector2(0,1)),
               new VertexTexture(new Vector3(-1,1,1), new Vector2(1,1)),
               new VertexTexture(new Vector3(-1,-1,1), new Vector2(1,0)),
               new VertexTexture(new Vector3(1,-1,1), new Vector2(0, 0)),
               new VertexTexture(new Vector3(1,1,1), new Vector2(0,1)),

               new VertexTexture(new Vector3(-1,1,-1), new Vector2(0,1)), //top
               new VertexTexture(new Vector3(-1,1,1), new Vector2(0,0)),
               new VertexTexture(new Vector3(1,1,1), new Vector2(1,0)),
               new VertexTexture(new Vector3(-1,1,-1), new Vector2(0,1)),
               new VertexTexture(new Vector3(1,1,1), new Vector2(1, 0)),
               new VertexTexture(new Vector3(1,1,-1), new Vector2(1,1)),

               new VertexTexture(new Vector3(-1,-1,-1), new Vector2(1,0)), //bottom
               new VertexTexture(new Vector3(1,-1,1), new Vector2(0,1)),
               new VertexTexture(new Vector3(-1,-1,1), new Vector2(1,1)),
               new VertexTexture(new Vector3(-1,-1,-1), new Vector2(1,0)),
               new VertexTexture(new Vector3(1,-1,-1), new Vector2(0, 0)),
               new VertexTexture(new Vector3(1,-1,1), new Vector2(0,1)),

               new VertexTexture(new Vector3(-1,-1,-1), new Vector2(0,1)), //left
               new VertexTexture(new Vector3(-1,-1,1), new Vector2(0,0)),
               new VertexTexture(new Vector3(-1,1,1), new Vector2(1,0)),
               new VertexTexture(new Vector3(-1,-1,-1), new Vector2(0,1)),
               new VertexTexture(new Vector3(-1,1,1), new Vector2(1, 0)),
               new VertexTexture(new Vector3(-1,1,-1), new Vector2(1,1)),

               new VertexTexture(new Vector3(1,-1,-1), new Vector2(1,0)), //right
               new VertexTexture(new Vector3(1,1,1), new Vector2(0,1)),
               new VertexTexture(new Vector3(1,-1,1), new Vector2(1,1)),
               new VertexTexture(new Vector3(1,-1,-1), new Vector2(1,0)),
               new VertexTexture(new Vector3(1,1,-1), new Vector2(0, 0)),
               new VertexTexture(new Vector3(1,1,1), new Vector2(0,1)),
            };
            _mesh.SetVertices(verts);
            SpawnParentChild(_mesh, Vector3.Zero, out _parent, out _child);

            if (File.Exists("assets/BroshK.ttf"))
                CreateSampleMsdfTextureFont("assets/BroshK.ttf", 18, new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'a', 'b', 'c', 'd', 'e', 'f', 'g' }, "msdf");
            else
                Log.WriteError("Cannot run MSDF test. Font file does not exist.");
        }

        void CreateSampleMsdfTextureFont(string fontfile, float sizeInPoint, char[] chars, string outputDir)
        {
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            //sample
            var reader = new OpenFontReader();
            using (var fs = new FileStream(fontfile, FileMode.Open))
            {
                //1. read typeface from font file
                Typeface typeface = reader.Read(fs);
                //sample: create sample msdf texture 
                //-------------------------------------------------------------
                var builder = new GlyphPathBuilder(typeface);
                //builder.UseTrueTypeInterpreter = this.chkTrueTypeHint.Checked;
                //builder.UseVerticalHinting = this.chkVerticalHinting.Checked;
                //-------------------------------------------------------------
                var atlasBuilder = new SimpleFontAtlasTextureBuilder();

                MsdfGenParams msdfGenParams = new MsdfGenParams();
                for (int i = 0; i < chars.Length; ++i)
                {
                    //build glyph
                    ushort gindex = typeface.LookupIndex(chars[i]);
                    //-----------------------------------
                    //get exact bounds of glyphs
                    Glyph glyph = typeface.GetGlyphByIndex(gindex);
                    Bounds bounds = glyph.Bounds;  //exact bounds

                    //-----------------------------------
                    builder.BuildFromGlyphIndex(gindex, -1);
                    var glyphToContour = new GlyphContourBuilder();
                    builder.ReadShapes(glyphToContour);
                    float scale = 1f / 64;
                    msdfGenParams.shapeScale = scale;
                    float s_xmin = bounds.XMin * scale;
                    float s_xmax = bounds.XMax * scale;
                    float s_ymin = bounds.YMin * scale;
                    float s_ymax = bounds.YMax * scale;


                    //-----------------------------------
                    GlyphData glyphImg = MsdfGlyphGen.CreateMsdfImage(Engine.Renderer, glyphToContour, msdfGenParams);
                    atlasBuilder.AddGlyph(gindex, glyphImg);
                    //int w = glyphImg.Width;
                    //int h = glyphImg.Height;
                    //using (Bitmap bmp = new Bitmap(glyphImg.Width, glyphImg.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                    //{
                    //    var bmpdata = bmp.LockBits(new System.Drawing.Rectangle(0, 0, w, h), System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);
                    //    int[] imgBuffer = glyphImg.GetImageBuffer();
                    //    System.Runtime.InteropServices.Marshal.Copy(imgBuffer, 0, bmpdata.Scan0, imgBuffer.Length);
                    //    bmp.UnlockBits(bmpdata);
                    //    string path = $"{outputDir}/char_{i}.png";
                    //    bmp.Save(path);
                    //}
                }

                var glyphImg2 = atlasBuilder.BuildSingleData();
                _msdfTexture = Engine.Renderer.Resources.CreateTexture2D(new Texture2DProperties()
                {
                    Width = glyphImg2.Width,
                    Height = glyphImg2.Height,
                    Format = GraphicsFormat.R8G8B8A8_UNorm,
                    ArraySize = 1,
                    MipMapLevels = 1,
                });

                Color[] intBuffer = glyphImg2.GetImageBuffer(); // Each integer is ARGB
                int pitch = glyphImg2.Width * sizeof(int);
                _msdfTexture.SetData(0, intBuffer, 0, intBuffer.Length, pitch);
                Sprite msdfSprite = new Sprite()
                {
                    Texture = _msdfTexture,
                    Color = Color.White,
                    Source = new Rectangle(0,0, glyphImg2.Width, glyphImg2.Height),
                    Position = new Vector2(300,200),
                    Origin = new Vector2(),
                    Rotation = 0,
                    Scale = new Vector2(5)
                };
                SampleScene.AddSprite(msdfSprite);

                msdfSprite = new Sprite()
                {
                    Texture = _msdfTexture,
                    Color = Color.White,
                    Source = new Rectangle(0, 0, glyphImg2.Width, glyphImg2.Height),
                    Position = new Vector2(300, 200 - glyphImg2.Height - 5),
                    Origin = new Vector2(),
                    Rotation = 0,
                    Scale = new Vector2(1)
                };
                SampleScene.AddSprite(msdfSprite);


                //using (Bitmap bmp = new Bitmap(glyphImg2.Width, glyphImg2.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                //{
                //    var bmpdata = bmp.LockBits(new System.Drawing.Rectangle(0, 0, glyphImg2.Width, glyphImg2.Height),
                //        System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);
                //    int[] intBuffer = glyphImg2.GetImageBuffer();

                //    System.Runtime.InteropServices.Marshal.Copy(intBuffer, 0, bmpdata.Scan0, intBuffer.Length);
                //    bmp.UnlockBits(bmpdata);
                //    bmp.Save($"{outputDir}/sheet.png");
                //}
                //atlasBuilder.SaveFontInfo($"{outputDir}/sheet_info.xml");
            }
        }

        private void Cr_OnCompleted(ContentManager content, ContentRequest cr)
        {
            if (cr.RequestedFiles.Count == 0)
                return;

            ITexture2D tex = content.Get<ITexture2D>(cr.RequestedFiles[0]);
            IMaterial mat = content.Get<IMaterial>(cr.RequestedFiles[1]);

            if (mat == null)
            {
                Exit();
                return;
            }

            mat.SetDefaultResource(tex, 0);
            _mesh.Material = mat;
        }

        private void Window_OnClose(IWindowSurface surface)
        {
            Exit();
        }

        protected override void OnUpdate(Timing time)
        {
            RotateParentChild(_parent, _child, time);
            base.OnUpdate(time);
        }
    }
}
