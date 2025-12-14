using System.Diagnostics;
using GameCore.Core;

namespace GameCore.Network.Packets;

// handle all serialization on one place.
// message buffer to read/write packets

public class NetworkMessage
{
    private readonly MemoryStream _stream;
    private readonly BinaryWriter _writer;
    private readonly BinaryReader _reader;

    private readonly int _maxSize;

    public int Length => (int)_stream.Length;
    public int Position => (int)_stream.Position;

    //create new message to write
    public NetworkMessage(int maxSize = Consts.NETWORKMESSAGE_MAXSIZE)
    {
        _maxSize = maxSize;
        _stream = new MemoryStream(maxSize);
        _writer = new BinaryWriter(_stream);
        _reader = new BinaryReader(_stream);
    }

    // create from existing bytes for read
    public NetworkMessage(byte[]data)
    {
        _maxSize = data.Length;
        _stream = new MemoryStream(data);
        _writer = new BinaryWriter(_stream);
        _reader = new BinaryReader(_stream);
    }

    // WRITE METHODS

    public void AddByte(byte value)
    {
        CheckSize(1);
        _writer.Write(value);
    }

    public void AddUInt16(ushort value)
    {
        CheckSize(2);
        _writer.Write(value);
    }

    public void AddUInt32(uint value)
    {
        CheckSize(4);
        _writer.Write(value);
    }

    public void AddInt32(int value)
    {
        CheckSize(4);
        _writer.Write(value);
    }

    public void AddString(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            AddUInt16(0);
            return;
        }

        // lenght (2bytes) + data
        AddUInt16((ushort)value.Length);
        CheckSize(value.Length * 2);
        _writer.Write(value.ToCharArray());
    }

    public void AddBool(bool value)
    {
        AddByte((byte)(value ? 1 : 0));
    }    

    // READ METHODS
    public byte GetByte()
    {
        return _reader.ReadByte();
    }

    public ushort GetUInt16()
    {
        return _reader.ReadUInt16();
    }

    public uint GetUInt32()
    {
        return _reader.ReadUInt32();
    }

    public int GetInt32()
    {
        return _reader.ReadInt32();
    }
    public string GetString()
    {
        var length = GetUInt16();

        if (length == 0)
            return string.Empty;

        var chars = _reader.ReadChars(length);
        return new string(chars);
    }

    public bool GetBool()
    {
        return GetByte() == 1;
    }


    // UTILS

    // rawbytes
    public byte[] GetBytes()
    {
        return _stream.ToArray();
    }

    // reset pos to beginning
    public void Reset()
    {
        _stream.Position = 0;
    }

    // skip N bytes forward
    public void Skip(int bytes)
    {
        _stream.Position += bytes;
    }

    // Check to not go over max size
    private void CheckSize(int additionalBytes)
    {
        if (_stream.Position + additionalBytes > _maxSize)
        {
            throw new Exception($"NetworkMessage size limit exceeded. Max: {_maxSize}, Attempted: {_stream.Position + additionalBytes}");
        }
    }

    // debug: show load as hex
    public string ToHexString()
    {
        var bytes = GetBytes();
        return BitConverter.ToString(bytes).Replace("-", " ");
    }

}