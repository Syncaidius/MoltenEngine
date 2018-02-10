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
        List<ISprite> _sprites;
        IMesh<VertexTexture> _mesh;

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

            _sprites = new List<ISprite>();
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

            if (File.Exists("assets/tahoma.ttf"))
                CreateSampleMsdfTextureFont("assets/tahoma.ttf", 18, new char[] { 'A', 'B', 'C', 'D', 'E', 'F', '测', '试' }, "msdf");
            else
                Log.WriteError("Cannot run MSDF test. Font file does not exist.");
        }

        static void CreateSampleMsdfTextureFont(string fontfile, float sizeInPoint, char[] chars, string outputDir)
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
                var atlasBuilder = new SimpleFontAtlasBuilder();

                MsdfGenParams msdfGenParams = new MsdfGenParams();

                int j = chars.Length;
                for (int i = 0; i < j; ++i)
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
                    //glyphToContour.Read(builder.GetOutputPoints(), builder.GetOutputContours());
                    builder.ReadShapes(glyphToContour);
                    float scale = 1f / 64;
                    msdfGenParams.shapeScale = scale;
                    float s_xmin = bounds.XMin * scale;
                    float s_xmax = bounds.XMax * scale;
                    float s_ymin = bounds.YMin * scale;
                    float s_ymax = bounds.YMax * scale;


                    //-----------------------------------
                    GlyphImage glyphImg = MsdfGlyphGen.CreateMsdfImage(glyphToContour, msdfGenParams);
                    atlasBuilder.AddGlyph(gindex, glyphImg);
                    int w = glyphImg.Width;
                    int h = glyphImg.Height;
                    using (Bitmap bmp = new Bitmap(glyphImg.Width, glyphImg.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                    {
                        var bmpdata = bmp.LockBits(new System.Drawing.Rectangle(0, 0, w, h), System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);
                        int[] imgBuffer = glyphImg.GetImageBuffer();
                        System.Runtime.InteropServices.Marshal.Copy(imgBuffer, 0, bmpdata.Scan0, imgBuffer.Length);
                        bmp.UnlockBits(bmpdata);
                        string path = $"{outputDir}/char_{i}.png";
                        bmp.Save(path);
                    }
                }

                var glyphImg2 = atlasBuilder.BuildSingleImage();
                using (Bitmap bmp = new Bitmap(glyphImg2.Width, glyphImg2.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                {
                    var bmpdata = bmp.LockBits(new System.Drawing.Rectangle(0, 0, glyphImg2.Width, glyphImg2.Height),
                        System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);
                    int[] intBuffer = glyphImg2.GetImageBuffer();

                    System.Runtime.InteropServices.Marshal.Copy(intBuffer, 0, bmpdata.Scan0, intBuffer.Length);
                    bmp.UnlockBits(bmpdata);
                    bmp.Save($"{outputDir}/sheet.png");
                }
                atlasBuilder.SaveFontInfo($"{outputDir}/sheet_info.xml");
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
