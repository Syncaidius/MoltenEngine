namespace Molten.Content
{
    public class StringProcessor : ContentProcessor<StringParameters>
    {
        public override Type[] AcceptedTypes { get; } = new Type[] { typeof(string) };

        public override Type[] RequiredServices => null;

        protected override void OnRead(ContentContext context, StringParameters parameters)
        {
            using (Stream stream = new FileStream(context.Filename, FileMode.Open, FileAccess.Read))
            {
                if (parameters.IsBinary)
                {
                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        while (reader.BaseStream.Position < reader.BaseStream.Length)
                            context.AddOutput(reader.ReadString());
                    }
                }
                else
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        if (parameters.IsPerLine)
                        {
                            while (!reader.EndOfStream)
                                context.AddOutput(reader.ReadLine());
                        }
                        else
                        {
                            context.AddOutput(reader.ReadToEnd());
                        }
                    }
                }
            }
        }

        protected override void OnWrite(ContentContext context, StringParameters parameters)
        {
            if (context.Input.TryGetValue(AcceptedTypes[0], out List<object> strings))
            {
                using (Stream stream = new FileStream(context.Filename, FileMode.Create, FileAccess.Write))
                {
                    if (parameters.IsBinary)
                    {
                        using (BinaryWriter writer = new BinaryWriter(stream))
                        {
                            foreach (string str in strings)
                                writer.Write(str);
                        }
                    }
                    else
                    {
                        using (StreamWriter writer = new StreamWriter(stream))
                        {
                            foreach (string str in strings)
                                writer.WriteLine(str);
                        }
                    }
                }
            }
        }
    }
}
