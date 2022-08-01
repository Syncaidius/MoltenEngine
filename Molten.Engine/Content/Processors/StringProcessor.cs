using System.Text;

namespace Molten.Content
{
    public class StringProcessor : ContentProcessor<StringParameters>
    {
        public override Type[] AcceptedTypes { get; } = new Type[] { typeof(string) };

        public override Type[] RequiredServices => null;

        public override Type PartType { get; } = typeof(string);

        protected override bool OnReadPart(ContentLoadHandle handle, Stream stream, StringParameters parameters, object existingPart, out object partAsset)
        {
            partAsset = null;
            Encoding encoding = parameters.Encoding ?? Encoding.UTF8;

            if (parameters.IsBinary)
            {
                // TODO properly handle encoding?
                using (BinaryReader reader = new BinaryReader(stream, encoding, true))
                    partAsset = (reader.ReadString());
            }
            else
            {
                using (StreamReader reader = new StreamReader(stream, encoding, true))
                    partAsset = reader.ReadToEnd();
            }

            return true;
        }

        protected override bool OnBuildAsset(ContentLoadHandle handle, ContentLoadHandle[] parts, StringParameters parameters, object existingAsset, out object asset)
        {
            string result = "";
            for(int i = 0; i < parts.Length; i++)
                result += $"{(i > 0 ? parameters.MultipartDelimiter : "")}{parts[i].Asset}";

            asset = result;
            return true;
        }

        protected override bool OnWrite(ContentHandle handle, Stream stream, StringParameters parameters, object asset)
        {
            string str = (string)asset;
            Encoding encoding = parameters.Encoding ?? Encoding.UTF8;

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

            return true;
        }
    }
}
