using CoreNet.Networking;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreNet.Protocols
{
    //heartbeats packet은 packet type 없이 바로 처리됨.
    public class HeartbeatNoti : Packet
    {
        public HeartbeatNoti()
        {
            SetHeader(0);
        }
    }

    public class Packet
    {
        public static int GetHeaderSize()
        {
            return sizeof(Int32);
        }

        public NetStream header { get; protected set; } = new NetStream(Packet.GetHeaderSize());
        public NetStream data { get; protected set; }
        public PACKET_TYPE pType { get; protected set; } = PACKET_TYPE.NONE;

        public enum PACKET_TYPE : ushort
        {
            NONE,
            NOTI,
            REQ,
            ANS,
            TEST,
            END,
        }

       

        public static Packet CopyPacket(Packet _orig)
        {
            var ret = new Packet(_orig.GetHeader());
            Array.Copy(_orig.data.bytes, ret.data.bytes, _orig.GetHeader());
            Array.Copy(_orig.header.bytes, ret.header.bytes, GetHeaderSize());
            ret.SetHeader(_orig.GetHeader());
            return ret;
        }
        public Packet(long _capacity = 512)
        {
            if (_capacity <= 0)
                data = null;
            else
                data = new NetStream(_capacity);
        }
        //자식 Packet들이 사용할 것.
        protected Packet(Packet _p)
        {
            header = _p.header;
            data = _p.data;
            //content, packet type은 factory에서 읽는다.
            pType = _p.pType;
        }

        public Packet(NetStream _h, NetStream _d)
        {
            header = _h;
            data = _d;
        }


        public void SetHeader(Int32 _val)
        {
            header.ResetOffset();
            header.WriteInt32(_val);
        }

        public void UpdateHeader()
        {
            if (data == null)
                SetHeader(0);
            else
                SetHeader(data.offset);
        }

        public Int32 GetHeader()
        {
            header.ResetOffset();
            return header.ReadInt32();
        }

        public void ReadPacketType()
        {
            pType = Translate.Read<PACKET_TYPE>(data);
        }

        protected void ClearData()
        {
            if (data == null)
                return;
            Array.Clear(data.bytes, 0, data.bytes.Length);
            data.ResetOffset();
        }

        [Conditional("DEBUG")]
        public void RenderPacket(string _header = "")
        {
            Console.WriteLine($"Pacekt========{_header}");
            header?.RenderBytes();
            data?.RenderBytes();
            Console.WriteLine($"================");
        }
        [Conditional("DEBUG")]
        public void RenderProperties()
        {
            StringBuilder sb = new StringBuilder("[RenderProperties]");
            var pList = GetType().GetProperties();
            foreach (var pInfo in pList)
            {
                if (pInfo.CanWrite == false)
                    continue;
                sb.AppendLine($"{pInfo.Name}:{pInfo.GetValue(this).ToString()}");
            }
            Console.WriteLine(sb.ToString());
        }
    }
}
