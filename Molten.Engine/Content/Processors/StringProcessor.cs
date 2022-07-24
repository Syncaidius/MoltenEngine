namespace Molten.Content
{
    public class StringProcessor : ContentProcessor<StringParameters>
    {
        public override Type[] AcceptedTypes { get; } = new Type[] { typeof(string) };

        public override Type[] RequiredServices => null;

        protected override bool OnRead(ContentHandle handle, StringParameters parameters, object existingAsset, out object asset)
        {
            asset = null;

            using (Stream stream = new FileStream(handle.RelativePath, FileMode.Open, FileAccess.Read))
            {
                if (parameters.IsBinary)
                {
                    using (BinaryReader reader = new BinaryReader(stream))
                        asset = (reader.ReadString());
                }
                else
                {
                    using (StreamReader reader = new StreamReader(stream))
                        asset = reader.ReadToEnd();
                }
            }

            return true;
        }

        protected override bool OnWrite(ContentHandle handle, StringParameters parameters, object asset)
        {
            string str = (string)asset;

            using (Stream stream = new FileStream(handle.RelativePath, FileMode.Create, FileAccess.Write))
            {
                if (parameters.IsBinary)
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                        writer.Write(str);
                }
                else
                {
                    using (StreamWriter writer = new StreamWriter(stream))
                        writer.Write(str);
                }
            }

            return true;
        }
    }
}
