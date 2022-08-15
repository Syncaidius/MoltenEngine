using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Graphics;

namespace Molten
{
    public class StringParameters : ContentParameters
    {
        public bool IsBinary { get; set; } = false;

        public Encoding Encoding { get; set; } = Encoding.UTF8;

        public string MultipartDelimiter { get; set; } = " ";

        public override object Clone()
        {
            return new StringParameters()
            {
                MultipartDelimiter = MultipartDelimiter,
                Encoding = Encoding,
                PartCount = PartCount
            };
        }
    }
}
