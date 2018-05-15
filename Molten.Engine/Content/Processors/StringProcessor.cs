using Molten.Graphics;
using Molten.Graphics.Textures.DDS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Content
{
    public class StringProcessor : ContentProcessor
    {
        public override Type[] AcceptedTypes { get; protected set; } = new Type[] { typeof(string)};

        public override void OnRead(ContentContext context)
        {
            bool isBinary = false;

            if (context.Metadata.TryGetValue("binary", out string binaryStr))
                bool.TryParse(binaryStr, out isBinary);

            if (isBinary)
            {
                using (BinaryReader reader = new BinaryReader(context.Stream))
                {
                    while (reader.BaseStream.Position < reader.BaseStream.Length)
                        context.AddOutput<string>(reader.ReadString());
                }
            }
            else
            {
                bool perLine = false;
                if (context.Metadata.TryGetValue("perline", out string perLineStr))
                    bool.TryParse(binaryStr, out perLine);

                using (StreamReader reader = new StreamReader(context.Stream))
                {
                    if (perLine)
                    {
                        context.AddOutput<string>(reader.ReadToEnd());
                    }
                    else
                    {
                        while (!reader.EndOfStream)
                            context.AddOutput<string>(reader.ReadLine());
                    }
                }
            }
        }

        public override void OnWrite(ContentContext context)
        {
            if (context.Input.TryGetValue(AcceptedTypes[0], out List<object> strings))
            {
                bool isBinary = false;

                if (context.Metadata.TryGetValue("binary", out string binaryStr))
                    bool.TryParse(binaryStr, out isBinary);

                if (isBinary)
                {
                    using (BinaryWriter writer = new BinaryWriter(context.Stream))
                    {
                        foreach (string str in strings)
                            writer.Write(str);
                    }
                }
                else
                {
                    using (StreamWriter writer = new StreamWriter(context.Stream))
                    {
                        foreach (string str in strings)
                            writer.WriteLine(str);
                    }
                }
            }
        }
    }
}
