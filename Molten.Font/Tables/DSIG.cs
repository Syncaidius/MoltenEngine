using System;
using System.Collections.Generic;
using Molten.IO;

namespace Molten.Font
{
    /// <summary>Digital signature table. See: https://www.microsoft.com/typography/otspec/dsig.htm</summary>
    [FontTableTag("DSIG")]
    public class DSIG : FontTable
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

        internal override void Read(EnhancedBinaryReader reader, TableHeader header, Logger log, FontTableList dependencies)
        {
            Version = reader.ReadUInt32();
            NumSignatures = reader.ReadUInt16();
            Flags = reader.ReadUInt16();

            log.Debug($"[DSIG] Version {Version} -- {NumSignatures} signatures found");

            // Read signature record list.
            List<SignatureRecord> sigs = new List<SignatureRecord>();
            for (int i = 0; i < NumSignatures; i++)
            {
                sigs.Add(new SignatureRecord()
                {
                    Format = reader.ReadUInt32(),
                    Length = reader.ReadUInt32(),
                    Offset = reader.ReadUInt32(),
                });
            }

            // Now read the signatures based on the provided records
            for (int i = 0; i < NumSignatures; i++)
            {
                SignatureRecord record = sigs[i];
                // Jump to signature position in stream
                long sigPos = header.StreamOffset + record.Offset;
                reader.Position = sigPos;

                Signature sig = new Signature()
                {
                    Reserved1 = reader.ReadUInt16(),
                    Reserved2 = reader.ReadUInt16(),
                    Record = record,
                };

                uint sigDataLength = reader.ReadUInt32();
                sig.Data = reader.ReadBytes((int)sigDataLength);
                Signatures.Add(sig);
            }
        }
    }

}
