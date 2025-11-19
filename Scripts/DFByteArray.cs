using System;
using System.IO;
using System.Text;

namespace DataFabricEntry.Runtime
{
    public sealed class DFByteArray : IDisposable
    {
        private byte[] _buffer;
        private int _position;
        private int _length;
        private bool _isLittleEndian = true;
        private readonly Encoding _encoding = Encoding.UTF8;

        public int Length => _length;

        public int Position
        {
            get => _position;
            set => _position = value < 0 ? 0 : (value > _length ? _length : value);
        }

        public int BytesAvailable => _length - _position;

        public DFByteArray(int capacity = 128)
        {
            _buffer = new byte[capacity];
            _position = 0;
            _length = 0;
        }

        public DFByteArray(byte[] bytes)
        {
            _buffer = bytes ?? throw new ArgumentNullException(nameof(bytes));
            _position = 0;
            _length = bytes.Length;
        }

        #region 写入方法

        public void WriteByte(byte value)
        {
            EnsureCapacity(1);
            _buffer[_position++] = value;
            if (_position > _length) _length = _position;
        }

        public void WriteSByte(sbyte value) => WriteByte((byte) value);

        public void WriteBool(bool value) => WriteByte(value ? (byte) 1 : (byte) 0);

        public void WriteShort(short value)
        {
            EnsureCapacity(2);
            if (_isLittleEndian)
            {
                _buffer[_position++] = (byte) value;
                _buffer[_position++] = (byte) (value >> 8);
            }
            else
            {
                _buffer[_position++] = (byte) (value >> 8);
                _buffer[_position++] = (byte) value;
            }

            if (_position > _length) _length = _position;
        }

        public void WriteUShort(ushort value) => WriteShort((short) value);

        public void WriteInt(int value)
        {
            EnsureCapacity(4);
            if (_isLittleEndian)
            {
                _buffer[_position++] = (byte) value;
                _buffer[_position++] = (byte) (value >> 8);
                _buffer[_position++] = (byte) (value >> 16);
                _buffer[_position++] = (byte) (value >> 24);
            }
            else
            {
                _buffer[_position++] = (byte) (value >> 24);
                _buffer[_position++] = (byte) (value >> 16);
                _buffer[_position++] = (byte) (value >> 8);
                _buffer[_position++] = (byte) value;
            }

            if (_position > _length) _length = _position;
        }

        public void WriteUInt(uint value) => WriteInt((int) value);

        public void WriteLong(long value)
        {
            EnsureCapacity(8);
            if (_isLittleEndian)
            {
                _buffer[_position++] = (byte) value;
                _buffer[_position++] = (byte) (value >> 8);
                _buffer[_position++] = (byte) (value >> 16);
                _buffer[_position++] = (byte) (value >> 24);
                _buffer[_position++] = (byte) (value >> 32);
                _buffer[_position++] = (byte) (value >> 40);
                _buffer[_position++] = (byte) (value >> 48);
                _buffer[_position++] = (byte) (value >> 56);
            }
            else
            {
                _buffer[_position++] = (byte) (value >> 56);
                _buffer[_position++] = (byte) (value >> 48);
                _buffer[_position++] = (byte) (value >> 40);
                _buffer[_position++] = (byte) (value >> 32);
                _buffer[_position++] = (byte) (value >> 24);
                _buffer[_position++] = (byte) (value >> 16);
                _buffer[_position++] = (byte) (value >> 8);
                _buffer[_position++] = (byte) value;
            }

            if (_position > _length) _length = _position;
        }

        public void WriteULong(ulong value) => WriteLong((long) value);

        public void WriteFloat(float value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (_isLittleEndian != BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            WriteBytes(bytes);
        }

        public void WriteDouble(double value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (_isLittleEndian != BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            WriteBytes(bytes);
        }

        public void WriteString(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                WriteUShort(0);
                return;
            }

            var bytes = _encoding.GetBytes(value);
            WriteUShort((ushort) bytes.Length);
            WriteBytes(bytes);
        }

        public void WriteBytes(byte[] value)
        {
            if (value == null || value.Length == 0)
            {
                return;
            }

            EnsureCapacity(value.Length);
            Buffer.BlockCopy(value, 0, _buffer, _position, value.Length);
            _position += value.Length;
            if (_position > _length) _length = _position;
        }

        public void WriteDateTime(DateTime value)
        {
            WriteLong(value.ToUniversalTime().Ticks);
        }

        public void WriteObj<T>(T obj)
        {
            var data = MsgPackHelper.Serialize<T>(obj);
            WriteInt(data.Length);
            WriteBytes(data);
        }

        #endregion

        #region 统一 Write 方法

        public void Write(byte value) => WriteByte(value);
        public void Write(sbyte value) => WriteSByte(value);
        public void Write(bool value) => WriteBool(value);
        public void Write(short value) => WriteShort(value);
        public void Write(ushort value) => WriteUShort(value);
        public void Write(int value) => WriteInt(value);
        public void Write(uint value) => WriteUInt(value);
        public void Write(long value) => WriteLong(value);
        public void Write(ulong value) => WriteULong(value);
        public void Write(float value) => WriteFloat(value);
        public void Write(double value) => WriteDouble(value);
        public void Write(string value) => WriteString(value);
        public void Write(byte[] value) => WriteBytes(value);
        public void Write(DateTime value) => WriteDateTime(value);

        #endregion

        #region 读取方法

        public byte ReadByte()
        {
            if (_position >= _length) throw new EndOfStreamException();
            return _buffer[_position++];
        }

        public sbyte ReadSByte() => (sbyte) ReadByte();

        public bool ReadBool() => ReadByte() != 0;

        public short ReadShort()
        {
            if (_position + 2 > _length) throw new EndOfStreamException();
            short value;
            if (_isLittleEndian)
            {
                value = (short) (_buffer[_position] | (_buffer[_position + 1] << 8));
            }
            else
            {
                value = (short) ((_buffer[_position] << 8) | _buffer[_position + 1]);
            }

            _position += 2;
            return value;
        }

        public ushort ReadUShort() => (ushort) ReadShort();

        public int ReadInt()
        {
            if (_position + 4 > _length) throw new EndOfStreamException();
            int value;
            if (_isLittleEndian)
            {
                value = _buffer[_position] | (_buffer[_position + 1] << 8) |
                        (_buffer[_position + 2] << 16) | (_buffer[_position + 3] << 24);
            }
            else
            {
                value = (_buffer[_position] << 24) | (_buffer[_position + 1] << 16) |
                        (_buffer[_position + 2] << 8) | _buffer[_position + 3];
            }

            _position += 4;
            return value;
        }

        public uint ReadUInt() => (uint) ReadInt();

        public long ReadLong()
        {
            if (_position + 8 > _length) throw new EndOfStreamException();
            long value;
            if (_isLittleEndian)
            {
                value = _buffer[_position] | ((long) _buffer[_position + 1] << 8) |
                        ((long) _buffer[_position + 2] << 16) | ((long) _buffer[_position + 3] << 24) |
                        ((long) _buffer[_position + 4] << 32) | ((long) _buffer[_position + 5] << 40) |
                        ((long) _buffer[_position + 6] << 48) | ((long) _buffer[_position + 7] << 56);
            }
            else
            {
                value = ((long) _buffer[_position] << 56) | ((long) _buffer[_position + 1] << 48) |
                        ((long) _buffer[_position + 2] << 40) | ((long) _buffer[_position + 3] << 32) |
                        ((long) _buffer[_position + 4] << 24) | ((long) _buffer[_position + 5] << 16) |
                        ((long) _buffer[_position + 6] << 8) | _buffer[_position + 7];
            }

            _position += 8;
            return value;
        }

        public ulong ReadULong() => (ulong) ReadLong();

        public float ReadFloat()
        {
            var bytes = ReadBytes(4);
            if (_isLittleEndian != BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            return BitConverter.ToSingle(bytes, 0);
        }

        public double ReadDouble()
        {
            var bytes = ReadBytes(8);
            if (_isLittleEndian != BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            return BitConverter.ToDouble(bytes, 0);
        }

        public string ReadString()
        {
            ushort len = ReadUShort();
            if (len == 0) return string.Empty;

            if (_position + len > _length) throw new EndOfStreamException();
            var result = _encoding.GetString(_buffer, _position, len);
            _position += len;
            return result;
        }

        public byte[] ReadBytes(int len)
        {
            if (_position + len > _length) throw new EndOfStreamException();
            var result = new byte[len];
            Buffer.BlockCopy(_buffer, _position, result, 0, len);
            _position += len;
            return result;
        }

        public DateTime ReadDateTime()
        {
            long ticks = ReadLong();
            return new DateTime(ticks, DateTimeKind.Utc).ToLocalTime();
        }

        public T ReadObj<T>()
        {
            int length = ReadInt();
            var data = ReadBytes(length);
            return MsgPackHelper.Deserialize<T>(data);
        }

        #endregion

        #region 其他方法

        public void Clear()
        {
            _position = 0;
            _length = 0;
        }

        public byte[] ToArray()
        {
            var result = new byte[_length];
            Buffer.BlockCopy(_buffer, 0, result, 0, _length);
            return result;
        }

        public void Dispose()
        {
            _buffer = null;
            _position = 0;
            _length = 0;
        }

        private void EnsureCapacity(int needed)
        {
            if (_position + needed <= _buffer.Length) return;

            int newCapacity = Math.Max(_buffer.Length * 2, _position + needed);
            Array.Resize(ref _buffer, newCapacity);
        }

        public void SetEndianness(bool isLittleEndian)
        {
            _isLittleEndian = isLittleEndian;
        }

        #endregion
    }
}
