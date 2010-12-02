﻿using System;
using System.IO;

public class Unpacker
{
    private const int DefaultBufferSize = 32*1024;
    private readonly BufferedUnpackerImpl impl;
    private readonly Stream stream;

    protected int bufferReserveSize;
    protected int parsed;

    public Unpacker() : this(DefaultBufferSize)
    {
    }

    public Unpacker(int bufferReserveSize) : this(null, bufferReserveSize)
    {
    }

    public Unpacker(Stream stream)
        : this(stream, DefaultBufferSize)
    {
    }

    public Unpacker(Stream stream, int bufferReserveSize)
    {
        parsed = 0;
        this.bufferReserveSize = bufferReserveSize/2;
        this.stream = stream;
        impl = new BufferedUnpackerImpl(
            () =>
                {
                    if (stream == null)
                    {
                        return false;
                    }
                    ReserveBuffer(bufferReserveSize);
                    int rl = this.stream.Read(impl.buffer, impl.filled, impl.buffer.Length - impl.filled);
                    if (rl <= 0)
                    {
                        return false;
                    }
                    BufferConsumed(rl);
                    return true;
                });
    }

    public bool UnpackBool()
    {
        return impl.UnpackBool();
    }

    public object UnpackNull()
    {
        return impl.UnpackNull();
    }

    public float UnpackFloat()
    {
        return impl.UnpackFloat();
    }

    public double UnpackDouble()
    {
        return impl.UnpackDouble();
    }

    public string UnpackString()
    {
        return impl.UnpackString();
    }

    public ulong UnpackULong()
    {
        return impl.UnpackULong();
    }

    public long UnpackLong()
    {
        return impl.UnpackLong();
    }

    public uint UnpackUInt()
    {
        return impl.UnpackUInt();
    }

    public int UnpackInt()
    {
        return impl.UnpackInt();
    }

    public ushort UnpackUShort()
    {
        return impl.UnpackUShort();
    }

    public short UnpackShort()
    {
        return impl.UnpackShort();
    }

    public byte UnpackByte()
    {
        return impl.UnpackByte();
    }

    public sbyte UnpackSByte()
    {
        return impl.UnpackSByte();
    }

    public char UnpackChar()
    {
        return (char)impl.UnpackInt();
    }

    public T UnpackEnum<T>()
    {
        return (T)Enum.ToObject(typeof(T), impl.UnpackInt());
    }

    public TValue Unpack<TValue>() where TValue : class, IMessagePackable, new()
    {
        if (impl.TryUnpackNull())
        {
            return null;
        }
        var val = new TValue();
        val.FromMsgPack(this);
        return val;
    }

    public object UnpackObject()
    {
        object result;
        if (!TryUnpackObject(out result))
        {
            throw new UnpackException("Not enough data in stream.");
        }
        return result;
    }

    public bool TryUnpackObject(out object result)
    {
        return impl.TryUnpackObject(out result);
    }

    public void BufferConsumed(int size)
    {
        impl.filled += size;
    }

    public void ReserveBuffer(int require)
    {
        if (impl.buffer == null)
        {
            int nextSize1 = (bufferReserveSize < require) ? require : bufferReserveSize;
            impl.buffer = new byte[nextSize1];
            return;
        }

        if (impl.filled <= impl.offset)
        {
            // rewind the buffer
            impl.filled = 0;
            impl.offset = 0;
        }

        if (impl.buffer.Length - impl.filled >= require)
        {
            return;
        }

        int nextSize = impl.buffer.Length*2;
        int notParsed = impl.filled - impl.offset;
        while (nextSize < require + notParsed)
        {
            nextSize *= 2;
        }

        var tmp = new byte[nextSize];
        Array.Copy(impl.buffer, impl.offset, tmp, 0, impl.filled - impl.offset);

        impl.buffer = tmp;
        impl.filled = notParsed;
        impl.offset = 0;
    }
}