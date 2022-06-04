namespace Svelto.ECS
{
    public readonly struct BT<BufferT1, BufferT2, BufferT3, BufferT4>
    {
        public readonly BufferT1 buffer1;
        public readonly BufferT2 buffer2;
        public readonly BufferT3 buffer3;
        public readonly BufferT4 buffer4;
        public readonly int      count;

        BT(in (BufferT1 bufferT1, BufferT2 bufferT2, BufferT3 bufferT3, BufferT4 bufferT4, int count) buffer) :
            this()
        {
            buffer1 = buffer.bufferT1;
            buffer2 = buffer.bufferT2;
            buffer3 = buffer.bufferT3;
            buffer4 = buffer.bufferT4;
            count   = buffer.count;
        }

        public static implicit operator BT<BufferT1, BufferT2, BufferT3, BufferT4>(
            in (BufferT1 bufferT1, BufferT2 bufferT2, BufferT3 bufferT3, BufferT4 bufferT4, int count) buffer)
        {
            return new BT<BufferT1, BufferT2, BufferT3, BufferT4>(buffer);
        }
    }

    public readonly struct BT<BufferT1, BufferT2, BufferT3>
    {
        public readonly BufferT1 buffer1;
        public readonly BufferT2 buffer2;
        public readonly BufferT3 buffer3;
        public readonly int      count;

        BT(in (BufferT1 bufferT1, BufferT2 bufferT2, BufferT3 bufferT3, int count) buffer) : this()
        {
            buffer1 = buffer.bufferT1;
            buffer2 = buffer.bufferT2;
            buffer3 = buffer.bufferT3;
            count   = buffer.count;
        }

        public static implicit operator BT<BufferT1, BufferT2, BufferT3>(
            in (BufferT1 bufferT1, BufferT2 bufferT2, BufferT3 bufferT3, int count) buffer)
        {
            return new BT<BufferT1, BufferT2, BufferT3>(buffer);
        }
    }

    public readonly struct BT<BufferT1>
    {
        public readonly BufferT1 buffer;
        public readonly int      count;

        BT(in (BufferT1 bufferT1, int count) buffer) : this()
        {
            this.buffer = buffer.bufferT1;
            count  = buffer.count;
        }

        public static implicit operator BT<BufferT1>(in (BufferT1 bufferT1, int count) buffer)
        {
            return new BT<BufferT1>(buffer);
        }

        public static implicit operator BufferT1(BT<BufferT1> t) => t.buffer;
    }

    public readonly struct BT<BufferT1, BufferT2>
    {
        public readonly BufferT1 buffer1;
        public readonly BufferT2 buffer2;
        public readonly int      count;

        BT(in (BufferT1 bufferT1, BufferT2 bufferT2, int count) buffer) : this()
        {
            buffer1 = buffer.bufferT1;
            buffer2 = buffer.bufferT2;
            count   = buffer.count;
        }

        public static implicit operator BT<BufferT1, BufferT2>(
            in (BufferT1 bufferT1, BufferT2 bufferT2, int count) buffer)
        {
            return new BT<BufferT1, BufferT2>(buffer);
        }
    }
}