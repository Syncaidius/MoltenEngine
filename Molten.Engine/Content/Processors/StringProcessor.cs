using System.Text;

namespace Molten.Content
{
    public class StringProcessor : ContentProcessor<StringParameters>
    {
        public override Type[] AcceptedTypes { get; } = new Type[] { typeof(string) };

        public override Type[] RequiredServices => null;

        protected override bool OnRead(ContentHandle handle, StringParameters parameters, object existingAsset, out object asset)
        {
            asset = null;
            Encoding encoding = parameters.Encoding ?? Encoding.UTF8;

            using (Stream stream = new FileStream(handle.RelativePath, FileMode.Open, FileAccess.Read))
            {
                if (parameters.IsBinary)
                {
                    // TODO properly handle encoding?
                    using (BinaryReader reader = new BinaryReader(stream, encoding, true))
                        asset = (reader.ReadString());
                }
                else
                {
                    using (StreamReader reader = new StreamReader(stream, encoding, true))
                        asset = reader.ReadToEnd();
                }
            }

            return true;
        }

        protected override bool OnWrite(ContentHandle handle, StringParameters parameters, object asset)
        {
            string str = (string)asset;
            Encoding encoding = parameters.Encoding ?? Encoding.UTF8;

            using (Stream stream = new FileStream(handle.RelativePath, FileMode.Create, FileAccess.Write))
            {
                if (parameters.IsBinary)
                {
                    using (BinaryWriter writer = new BinaryWriter(stream, encoding))
                        writer.Write(str);
                }
                else
                {
                    using (StreamWriter writer = new StreamWriter(stream, encoding))
                        writer.Write(str);
                }
            }

            return true;
        }
    }
}
