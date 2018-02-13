using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Font
{
    class DSIG : FontTable
    {
        public uint Version { get; internal set; }

        public ushort NumSignatures { get; internal set; }

        public ushort Flags { get; internal set; }

        public List<Signature> Signatures { get; internal set; } = new List<Signature>();

        public class Signature
        {
            public SignatureRecord Record { get; internal set; }

            public ushort Reserved1 { get; internal set; }

            public ushort Reserved2 { get; internal set; }
            
            [Obsolete("This is PKCS#7 data which needs processing into an actual signature. " +
                "See: https://www.microsoft.com/typography/otspec/dsig.htm and http://fileformats.archiveteam.org/wiki/PKCS7_certificate")]
            public byte[] Data { get; set; }
        }

        public class SignatureRecord
        {
            public uint Format { get; internal set; }
            
            public uint Length { get; internal set; }

            /// <summary>The byte offset of the signature from the start of the table.</summary>
            public uint Offset { get; internal set; }
        }

        public class Parser : FontTableParser
        {
            public override string TableTag => "DSIG";

            public override FontTable Parse(BinaryEndianAgnosticReader reader, TableHeader header, Logger log)
            {
                DSIG table = new DSIG()
                {
                    Version = reader.ReadUInt32(),
                    NumSignatures = reader.ReadUInt16(),
                    Flags = reader.ReadUInt16(),
                };

                log.WriteDebugLine($"[DSIG] Version {table.Version} -- {table.NumSignatures} signatures found");

                // Read signature record list.
                List<SignatureRecord> sigs = new List<SignatureRecord>();
                for(int i = 0; i < table.NumSignatures; i++)
                {
                    sigs.Add(new SignatureRecord()
                    {
                        Format = reader.ReadUInt32(),
                        Length = reader.ReadUInt32(),
                        Offset = reader.ReadUInt32(),
                    });
                }

                // Now read the signatures based on the provided records
                for(int i = 0; i < table.NumSignatures; i++)
                {
                    SignatureRecord record = sigs[i];
                    // Jump to signature position in stream
                    uint sigPos = header.Offset + record.Offset;
                    reader.Position = sigPos;

                    Signature sig = new Signature()
                    {
                        Reserved1 = reader.ReadUInt16(),
                        Reserved2 = reader.ReadUInt16(),
                        Record = record,
                    };

                    uint sigDataLength = reader.ReadUInt32();
                    sig.Data = reader.ReadBytes((int)sigDataLength);
                    table.Signatures.Add(sig);
                }

                return table;
            }
        }
    }

}
