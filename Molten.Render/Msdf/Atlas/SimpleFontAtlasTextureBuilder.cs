//MIT, 2016-2017, WinterDev
//MIT, 2018, James Yarwood (Adapted for Molten Engine)

using System.Linq;
using System.Collections.Generic;
using System.Xml;
using Molten;
using Rectangle = Typography.Contours.Rectangle;

namespace Typography.Rendering
{
    public class SimpleFontAtlasTextureBuilder
    {
        GlyphData latestGenGlyphData;

        Dictionary<int, CacheGlyphData> glyphs = new Dictionary<int, CacheGlyphData>();
        public TextureKind TextureKind { get; private set; }

        public float FontSizeInPoints { get; private set; }

        public void AddGlyph(int codePoint, GlyphData img)
        {
            var glyphCache = new CacheGlyphData();
            glyphCache.codePoint = codePoint;
            glyphCache.img = img;

            glyphs[codePoint] = glyphCache;
        }

        public void SetAtlasInfo(TextureKind textureKind, float fontSizeInPts)
        {
            this.TextureKind = textureKind;
            this.FontSizeInPoints = fontSizeInPts;
        }

        public GlyphData BuildSingleData()
        {
            //1. add to list 
            var glyphList = new List<CacheGlyphData>(glyphs.Count);
            foreach (CacheGlyphData glyphImg in glyphs.Values)
            {
                //sort data
                glyphList.Add(glyphImg);
            }

            // TODO JY - switch to once previous changes confirmed to work: var glyphList = glyphs.Values.ToList();


            //2. sort
            glyphList.Sort((a, b) =>
            {
                return a.img.Width.CompareTo(b.img.Width);
            });

            //3. layout
            int totalMaxLim = 800;
            int maxRowHeight = 0;
            int currentY = 0;
            int currentX = 0;

            for (int i = glyphList.Count - 1; i >= 0; --i)
            {
                CacheGlyphData g = glyphList[i];
                if (g.img.Height > maxRowHeight)
                {
                    maxRowHeight = g.img.Height;
                }
                if (currentX + g.img.Width > totalMaxLim)
                {
                    //start new row
                    currentY += maxRowHeight;
                    currentX = 0;
                }
                //-------------------
                g.area = new Rectangle(currentX, currentY, g.img.Width, g.img.Height);
                currentX += g.img.Width;
            }

            currentY += maxRowHeight;
            int imgH = currentY;

            //compact image location
            //TODO: review performance here again***
            BinPacker binPacker = new BinPacker(totalMaxLim, currentY);
            for (int i = glyphList.Count - 1; i >= 0; --i)
            {
                CacheGlyphData g = glyphList[i];
                BinPackRect newRect = binPacker.Insert(g.img.Width, g.img.Height);
                g.area = new Rectangle(newRect.X, newRect.Y,
                    g.img.Width, g.img.Height);
            }

            //4. create array that can hold data
            Color[] totalBuffer = new Color[totalMaxLim * imgH];
            for (int i = glyphList.Count - 1; i >= 0; --i)
            {
                CacheGlyphData g = glyphList[i];
                //copy data to totalBuffer
                GlyphData img = g.img;
                CopyToDest(img.GetImageBuffer(), img.Width, img.Height, totalBuffer, g.area.Left, g.area.Top, totalMaxLim);
            }

            GlyphData glyphImage = new GlyphData(totalMaxLim, imgH);
            glyphImage.SetImageBuffer(totalBuffer, true);
            latestGenGlyphData = glyphImage;
            return glyphImage;

        }

        /// <summary>
        /// save font info into xml document
        /// </summary>
        /// <param name="filename"></param>
        public void SaveFontInfo(string filename)
        {
            //save font info as xml 
            //save position of each font
            XmlDocument xmldoc = new XmlDocument();
            XmlElement root = xmldoc.CreateElement("font");
            xmldoc.AppendChild(root);

            if (latestGenGlyphData == null)
            {
                BuildSingleData();
            }

            {
                //total img element
                XmlElement totalImgElem = xmldoc.CreateElement("total_img");
                totalImgElem.SetAttribute("w", latestGenGlyphData.Width.ToString());
                totalImgElem.SetAttribute("h", latestGenGlyphData.Height.ToString());
                totalImgElem.SetAttribute("compo", "4");
                root.AppendChild(totalImgElem);
            }

            foreach (CacheGlyphData g in glyphs.Values)
            {
                XmlElement gElem = xmldoc.CreateElement("glyph");
                //convert char to hex
                string unicode = ("0x" + ((int)g.character).ToString("X"));//code point
                Rectangle area = g.area;
                gElem.SetAttribute("c", g.codePoint.ToString());
                gElem.SetAttribute("uc", unicode);//unicode char
                gElem.SetAttribute("ltwh",
                    area.Left + " " + area.Top + " " + area.Width + " " + area.Height
                    );
                gElem.SetAttribute("borderXY",
                    g.borderX + " " + g.borderY
                    );
                var mat = g.glyphMatrix;
                gElem.SetAttribute("mat",
                    mat.advanceX + " " + mat.advanceY + " " +
                    mat.bboxXmin + " " + mat.bboxXmax + " " +
                    mat.bboxYmin + " " + mat.bboxYmax + " " +
                    mat.img_width + " " + mat.img_height + " " +
                    mat.img_horiAdvance + " " + mat.img_horiBearingX + " " +
                    mat.img_horiBearingY + " " +
                    //-----------------------------
                    mat.img_vertAdvance + " " +
                    mat.img_vertBearingX + " " + mat.img_vertBearingY);

                if (g.character > 50)
                {
                    gElem.SetAttribute("example", g.character.ToString());
                }
                root.AppendChild(gElem);
            }
            //if (embededGlyphsImage)
            //{
            //    XmlElement glyphImgElem = xmldoc.CreateElement("msdf_img");
            //    glyphImgElem.SetAttribute("w", latestGenGlyphImage.Width.ToString());
            //    glyphImgElem.SetAttribute("h", latestGenGlyphImage.Height.ToString());
            //    int[] imgBuffer = latestGenGlyphImage.GetImageBuffer();
            //    glyphImgElem.SetAttribute("buff_len", (imgBuffer.Length * 4).ToString());
            //    //----------------------------------------------------------------------
            //    glyphImgElem.AppendChild(
            //        xmldoc.CreateTextNode(ConvertToBase64(imgBuffer)));
            //    //----------------------------------------------------------------------
            //    root.AppendChild(glyphImgElem);
            //    latestGenGlyphImage.GetImageBuffer();
            //}
            xmldoc.Save(filename);
        }

        static float[] ParseFloatArray(string str)
        {
            string[] str_values = str.Split(' ');
            int j = str_values.Length;
            float[] f_values = new float[j];
            for (int i = 0; i < j; ++i)
            {
                f_values[i] = float.Parse(str_values[i]);
            }
            return f_values;
        }
        static Rectangle ParseRect(string str)
        {
            string[] ltwh = str.Split(' ');
            return new Rectangle(
                int.Parse(ltwh[0]),
                int.Parse(ltwh[1]),
                int.Parse(ltwh[2]),
                int.Parse(ltwh[3]));
        }


        static void CopyToDest(Color[] srcPixels, int srcW, int srcH, Color[] targetPixels, int targetX, int targetY, int totalTargetWidth)
        {
            int srcIndex = 0;
            unsafe
            {

                for (int r = 0; r < srcH; ++r)
                {
                    //for each row 
                    int targetP = ((targetY + r) * totalTargetWidth) + targetX;
                    for (int c = 0; c < srcW; ++c)
                    {
                        targetPixels[targetP] = srcPixels[srcIndex];
                        srcIndex++;
                        targetP++;
                    }
                }
            }
        }

    }


}