using System;

namespace Core
{
    public class RecvBuffer
    {
        private ArraySegment<byte> _buffer;

        public RecvBuffer(int size)
        {
            _buffer = new ArraySegment<byte>(new byte[size], 0, size);
        }

        private int _readPos;
        private int _writePos;

        public int AllocSize
        {
            get { return _writePos - _readPos; }
        }

        public int FreeSize
        {
            get { return _buffer.Count - _writePos; }
        }

        public ArraySegment<byte> ReadSegment
        {
            get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _readPos, AllocSize); }
        }

        public ArraySegment<byte> WriteSegment
        {
            get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _writePos, FreeSize); }
        }

        public void Clear()
        {
            int allocSize = AllocSize;
            if (allocSize == 0)
            {
                _readPos = _writePos = 0;
            }
            else
            {
                Array.Copy(_buffer.Array, 0, _buffer.Array, _buffer.Offset + _readPos, allocSize);
                _readPos = 0;
                _writePos = allocSize;
            }
        }

        public bool OnRead(int numOfBytes)
        {
            if (numOfBytes > AllocSize)
                return false;

            _readPos += numOfBytes;
            return true;
        }

        public bool OnWrite(int numOfBytes)
        {
            if (numOfBytes > FreeSize)
                return false;

            _writePos += numOfBytes;
            return true;
        }
    }
}