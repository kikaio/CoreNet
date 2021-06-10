using CoreNet.Cryptor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreNet.Networking
{
    public class NetStream
    {
        public byte[] bytes { get; private set; }
        public int offset { get; private set; } = 0;

        public NetStream(long _capacity = 512)
        {
            bytes = new byte[_capacity];
        }
        public NetStream(byte[] _data)
        {
            bytes = _data;
        }
        public void ResetOffset()
        {
            offset = 0;
        }
        private void CheckNeedMoreBytes(int _len)
        {
            if (offset + _len > bytes.Length)
                ExtendBytes(_len);
        }

        private void ExtendBytes(int _len)
        {
            byte[] newBytes = null;
            int extendCapacity = bytes.Length * 2 / 3;
            if (_len > extendCapacity)
                extendCapacity = _len;
            newBytes = new byte[bytes.Length + extendCapacity];
            Array.Copy(bytes, newBytes, bytes.Length);
            bytes = newBytes;
        }

        private void CopyToBytes(byte[] _bytes, int _len)
        {
            Array.Copy(_bytes, 0, bytes, offset, _len);
            offset += _len;
        }

        public void WriteBytes(byte[] _bytes, int _len)
        {
            CheckNeedMoreBytes(_len);
            CopyToBytes(_bytes, _len);
        }
        public void WriteByte(byte _val)
        {
            int len = sizeof(byte);
            CheckNeedMoreBytes(len);
            CopyToBytes(BitConverter.GetBytes(_val), len);
        }

        public void WriteChar(char _val)
        {
            int len = sizeof(char);
            CheckNeedMoreBytes(len);
            CopyToBytes(BitConverter.GetBytes(_val), len);
        }

        public void WriteBool(bool _val)
        {
            if (_val)
                WriteByte(1);
            else
                WriteByte(0);
        }

        public void WriteInt16(Int16 _val)
        {
            int len = sizeof(Int16);
            CheckNeedMoreBytes(len);
            CopyToBytes(BitConverter.GetBytes(_val), len);
        }
        public void WriteInt32(Int32 _val)
        {
            int len = sizeof(Int32);
            CheckNeedMoreBytes(len);
            CopyToBytes(BitConverter.GetBytes(_val), len);
        }
        public void WriteInt64(Int64 _val)
        {
            int len = sizeof(Int64);
            CheckNeedMoreBytes(len);
            CopyToBytes(BitConverter.GetBytes(_val), len);
        }
        public void WriteUInt16(UInt16 _val)
        {
            int len = sizeof(UInt16);
            CheckNeedMoreBytes(len);
            CopyToBytes(BitConverter.GetBytes(_val), len);
        }
        public void WriteUInt32(UInt32 _val)
        {
            int len = sizeof(UInt32);
            CheckNeedMoreBytes(len);
            CopyToBytes(BitConverter.GetBytes(_val), len);
        }
        public void WriteUInt64(UInt64 _val)
        {
            int len = sizeof(UInt64);
            CheckNeedMoreBytes(len);
            CopyToBytes(BitConverter.GetBytes(_val), len);
        }

        public void WriteFixed(float _val, int _precision)
        {
            Int16 val = (short)(_val * _precision);
            WriteInt16(val);
        }

        public void WriteStr(string _str)
        {
            WriteInt16((short)_str.Length);
            byte[] tmp = Encoding.UTF8.GetBytes(_str);
            WriteBytes(tmp, tmp.Length);
        }
        private byte[] CopyFromBytes(int _offset, int _len)
        {
            try
            {
                byte[] ret = new byte[_len];
                Array.Copy(bytes, _offset, ret, 0, _len);
                return ret;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public byte[] ReadBytes(int _len)
        {
            CheckNeedMoreBytes(_len);
            byte[] ret = CopyFromBytes(offset, _len);
            offset += _len;
            return ret;
        }

        public bool ReadBool()
        {
            var tmp = ReadBytes(sizeof(bool));
            return BitConverter.ToBoolean(tmp, 0);
        }

        public byte ReadByte()
        {
            return bytes[offset++];
        }

        public char ReadChar()
        {
            var tmp = ReadBytes(sizeof(char));
            return BitConverter.ToChar(tmp, 0);
        }


        public Int16 ReadInt16()
        {
            var tmp = ReadBytes(sizeof(Int16));
            return BitConverter.ToInt16(tmp, 0);
        }

        public Int32 ReadInt32()
        {
            var tmp = ReadBytes(sizeof(Int32));
            return BitConverter.ToInt32(tmp, 0);
        }

        public Int64 ReadInt64()
        {
            var tmp = ReadBytes(sizeof(Int64));
            return BitConverter.ToInt64(tmp, 0);
        }

        public UInt16 ReadUInt16()
        {
            var tmp = ReadBytes(sizeof(short));
            return BitConverter.ToUInt16(tmp, 0);
        }

        public UInt32 ReadUInt32()
        {
            var tmp = ReadBytes(sizeof(Int32));
            return BitConverter.ToUInt32(tmp, 0);
        }

        public UInt64 ReadUInt64()
        {
            var tmp = ReadBytes(sizeof(Int64));
            return BitConverter.ToUInt64(tmp, 0);
        }

        public string ReadStr()
        {
            var len = ReadInt16();
            int byteCnt = len;
            var tmp = ReadBytes(byteCnt);
            return Encoding.UTF8.GetString(tmp, 0, byteCnt);
        }

        public void DoXorCrypt(byte[] _key)
        {
            bytes = CryptHelper.XorEncrypt(bytes, _key);
        }

        public void DoDhEncrypt(byte[] _key, byte[] _iv)
        {
            string base64Str = Convert.ToBase64String(bytes);
            bytes = CryptHelper.DhEncrypt(base64Str, _key, _iv);
            offset = bytes.Length;
        }
        public void DoDhDeCrypt(byte[] _key, byte[] _iv)
        {
            bytes = CryptHelper.DhDecrypt(bytes, _key, _iv);
            //Recv시에만 Decrypt하므로 offset은 0으로 지정해준다.
            offset = 0;
        }

        [Conditional("DEBUG")]
        public void RenderStr()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"RenderStr [");
            sb.Append(BitConverter.ToString(bytes, 0));
            sb.Append("]");
            Console.WriteLine(sb.ToString());
        }

        [Conditional("DEBUG")]
        public void RenderInt16()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"RenderInt32 [");
            Console.Write($"RenderInt16 [");
            for (int idx = offset; idx < bytes.Length; idx += sizeof(Int16))
            {
                Int16 val = BitConverter.ToInt16(bytes, idx);
                sb.Append($"{val},");
            }
            sb.Append("]");
            Console.WriteLine(sb.ToString());
        }

        [Conditional("DEBUG")]
        public void RenderInt32()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"RenderInt32 [");
            for (int idx = 0; idx < bytes.Length; idx += sizeof(Int32))
            {
                Int32 val = BitConverter.ToInt32(bytes, idx);
                sb.Append($"{val},");
            }
            sb.Append("]");
            Console.WriteLine(sb.ToString());
        }

        [Conditional("DEBUG")]
        public void RenderBytes()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"RenderBytes [");
            for (int idx = 0; idx < offset; idx += sizeof(byte))
            {
                byte val = bytes[idx];
                sb.Append($"{val},");
                if (idx == offset)
                    break;
            }
            sb.Append("]");
            Console.WriteLine(sb.ToString());
        }
    }
}
