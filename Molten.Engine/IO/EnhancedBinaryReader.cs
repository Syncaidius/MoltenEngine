using System.Runtime.InteropServices;
using System.Text;

namespace Molten.IO;

/// <summary>A reader which automatically flips the bytes of read types if the current system's endian architecture does not match the stream's endian format.</summary>
public class EnhancedBinaryReader : BinaryReader
{
    protected readonly byte[] _arrayBuffer;

    public EnhancedBinaryReader(Stream input) : base(input)
    {
        _arrayBuffer = new byte[1024];
    }

    public EnhancedBinaryReader(Stream input, Encoding encoding) : this(input, new UTF8Encoding(), false) { }

    public EnhancedBinaryReader(Stream input, bool leaveOpen) : this(input, new UTF8Encoding(), leaveOpen) { }

    public EnhancedBinaryReader(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen)
    {
        _arrayBuffer = new byte[1024];
    }

    /// <summary>Reads a pascal string from the underlying stream. Pascal streams store their length in the first byte, followed by the string data, up to a maximum length of 255 bytes.</summary>
    /// <returns></returns>
    public string ReadPascalString()
    {
        byte size = base.ReadByte();
        byte[] body = base.ReadBytes(size);
        return Encoding.ASCII.GetString(body);
    }

    /// <summary>Reads a string of a fixed length.</summary>
    /// <returns></returns>
    public string ReadString(int length)
    {
        byte[] body = base.ReadBytes(length);
        return Encoding.ASCII.GetString(body);
    }

    public virtual uint ReadUInt24()
    {
        byte[] bytes = new byte[4];
        base.Read(bytes, 0, 3);
        return BitConverter.ToUInt32(bytes, 0);
    }

    public virtual T[] ReadArray<T>(int count) where T : struct
    {
        if (count == 0)
            return new T[0];

        if (count < 0)
            throw new IndexOutOfRangeException("Count cannot be less than 0");

        if (!typeof(T).IsPrimitive)
            throw new NotSupportedException("EnhancedBinaryReader: Non-primitive types are not supported.");

        int bytesToRead = count * Marshal.SizeOf<T>();
        T[] result = new T[count];
        int blockOffset = 0;

        while (bytesToRead > 0)
        {
            int toRead = Math.Min(_arrayBuffer.Length, bytesToRead);
            BaseStream.Read(_arrayBuffer, 0, bytesToRead);
            Buffer.BlockCopy(_arrayBuffer, 0, result, blockOffset, toRead);
            blockOffset += toRead;
            bytesToRead -= toRead;
        }

        return result;
    }

    /// <summary>Gets or sets the position of the underlying <see cref="Stream"/>.</summary>
    public long Position
    {
        get => BaseStream.Position;
        set => BaseStream.Position = value;
    }
}
