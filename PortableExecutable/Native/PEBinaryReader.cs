using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AncientLunar.PortableExecutable.Native
{
    public readonly struct PEBinaryReader
    {
        private readonly long _startOffset;
        private readonly long _maxOffset;
        private readonly BinaryReader _reader;

        public PEBinaryReader(Stream stream, int size)
        {
            _startOffset = stream.Position;
            _maxOffset = _startOffset + size;
            _reader = new BinaryReader(stream, Encoding.UTF8);
        }

        public int CurrentOffset => (int)(_reader.BaseStream.Position - _startOffset);
        
        public void Seek(int offset)
        {
            CheckBounds(_startOffset, offset);
            _reader.BaseStream.Seek((long)offset, SeekOrigin.Begin);
        }

        public byte[] ReadBytes(int count)
        {
            CheckBounds(_reader.BaseStream.Position, count);
            return _reader.ReadBytes(count);
        }

        public byte ReadByte()
        {
            CheckBounds(1);
            return _reader.ReadByte();
        }

        public short ReadInt16()
        {
            CheckBounds(2);
            return _reader.ReadInt16();
        }

        public ushort ReadUInt16()
        {
            CheckBounds(2);
            return _reader.ReadUInt16();
        }

        public int ReadInt32()
        {
            CheckBounds(4);
            return _reader.ReadInt32();
        }

        public uint ReadUInt32()
        {
            CheckBounds(4);
            return _reader.ReadUInt32();
        }

        public long ReadInt64()
        {
            CheckBounds(8);
            return _reader.ReadInt64();
        }

        public ulong ReadUInt64()
        {
            CheckBounds(8);
            return _reader.ReadUInt64();
        }

        public string ReadNullPaddedUTF8(int byteCount)
        {
            var array = ReadBytes(byteCount);
            int num = 0;

            for (var i = array.Length; i > 0; --i)
            {
                if (array[i - 1] != 0)
                {
                    num = i;
                    break;
                }
            }

            return Encoding.UTF8.GetString(array, 0, num);
        }

        public string ReadUtf8NullTerminated()
        {
            // gross

            var bytes = new List<byte>();
            byte current = 0;

            while ((current = ReadByte()) != 0)
                bytes.Add(current);

            return Encoding.UTF8.GetString(bytes.ToArray());
        }

        public unsafe Guid ReadGuid()
        {
            var bytes = ReadBytes(16);

            fixed (byte* raw = bytes)
            {
                if (BitConverter.IsLittleEndian)
                    return *(Guid*)raw;

                return new Guid((int)(*raw) | ((int)raw[1] << 8) | ((int)raw[2] << 16) | ((int)raw[3] << 24), (short)((int)raw[4] | ((int)raw[5] << 8)), (short)((int)raw[6] | ((int)raw[7] << 8)), raw[8], raw[9], raw[10], raw[11], raw[12], raw[13], raw[14], raw[15]);
            }
        }

        private void CheckBounds(uint count)
        {
            if (_reader.BaseStream.Position + (long)((ulong)count) > _maxOffset)
                throw new Exception("Image too small");
        }

        private void CheckBounds(long startPosition, int count)
        {
            if (startPosition + (long)((ulong)count) > _maxOffset)
                throw new Exception("Image too small or contains invalid offset or count");
        }
    }
}
