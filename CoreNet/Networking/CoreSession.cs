using CoreNet.Protocols;
using CoreNet.Sockets;
using CoreNet.Utils;
using CoreNet.Utils.Loggers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CoreNet.Networking
{
    public class CoreSession : IDisposable
    {
        public long SessionId { get; protected set; } = 0;
        public CoreSock Sock { get; private set; } = null;
        public DateTime HeartBeat { get; private set; } = DateTime.UtcNow;

        private CoreLogger logger = new ConsoleLogger();
#if DEBUG
        public static int hbDelayMilliSec { get; private set; } = 20 * 1000;
#else 
        public static int hbDelayMilliSec { get; private set; } = 3 * 60 * 1000;
#endif
        protected bool IsDisposed = false;

        public CoreSession(long _sid, CoreSock _sock)
        {
            SessionId = _sid;
            Sock = _sock;
            UpdateHeartBeat();
        }
        public byte[] xorKeyBytes = Encoding.UTF8.GetBytes("testKey");
        private bool IsXorAble
        {
            //get { return false; }
            get { return xorKeyBytes != null; }
        }

        //private byte[] dh_key = Convert.FromBase64String("A63qL9xZKzDExsJxGMffZCtQV44Lt9+3jsDrkMirbZc=");
        //private byte[] dh_iv = Convert.FromBase64String("Cj6mFNB/hde+PaZqM22W3g==");
        private byte[] dh_key = null;
        private byte[] dh_iv = null;
        private bool IsDhAble
        {
            get { return dh_key != null && dh_iv != null; }
        }



        public void SetSessionId(long _sid)
        {
            SessionId = _sid;
        }

        public void SetDhInfo(byte[] _key, byte[] _iv)
        {
            dh_key = _key;
            dh_iv = _iv;
        }

        public void UpdateHeartBeat()
        {
            HeartBeat = DateTime.UtcNow
                .AddMilliseconds(CoreSession.hbDelayMilliSec);
        }

        public bool IsExpireHeartBeat()
        {
            if (HeartBeat > DateTime.UtcNow)
                return false;
            return true;
        }

        public void DoDispose(bool _flag)
        {
            if (IsDisposed)
                return;
            IsDisposed = _flag;
            {
            }
        }

        public void Dispose()
        {
            DoDispose(true);
        }

        public void SetSocket(CoreSock _sock)
        {
            if (Sock.Sock.Connected)
                Sock.Sock.Close();
            Sock = _sock;
        }

        public void SetSyncSetting()
        {
            //socket 속성 설정.
            if (Sock is CoreTCP)
            {
                var curSocket = Sock.Sock;
                //Nagle algorithm 미적용.
                curSocket.NoDelay = true;
                //recv timeout 1초로 지정.
                var sec = 1 * 1000;
                curSocket.ReceiveTimeout = sec * 1;
            }
            else if (Sock is CoreUDP)
            {

            }
        }

        public void OnSendSync(Packet _p)
        {
            var remainCnt = Packet.GetHeaderSize();
            try
            {
                while (remainCnt > 0)
                {
                    remainCnt -= Sock.Sock.Send(_p.header.bytes
                        , Packet.GetHeaderSize() - remainCnt, remainCnt, SocketFlags.None);
                }

                remainCnt = _p.GetHeader();
                while (remainCnt > 0)
                {
                    remainCnt -= Sock.Sock.Send(_p.data.bytes
                        , _p.GetHeader() - remainCnt, remainCnt, SocketFlags.None);
                }
            }
            catch (SocketException e)
            {
                //시간 초과 시?
            }
            catch (ObjectDisposedException e)
            {   //send 대기중에 client socket 이 끊긴경우.
                logger.Error(e.ToString());
            }
        }

        //ForceClose가 불필요한지에 대한 여부 반환
        public bool OnRecvSync(out Package pkg)
        {
            pkg = default(Package);
            Packet p = new Packet();
            int headerLen = Packet.GetHeaderSize();
            int remainCnt = headerLen;
            try
            {
                pkg = new Package(this, p);
                while (remainCnt > 0)
                {
                    int recvCnt = 0;
                    recvCnt = Sock.Sock.Receive(p.header.bytes
                        , headerLen - remainCnt, remainCnt, SocketFlags.None);
                    remainCnt -= recvCnt;
                    if (recvCnt == 0)
                    {
                        //해당 소켓 연결 해제된것.?
                        pkg = new Package(this, null);
                        logger.WriteDebug($"session[{SessionId}] is disconnected");
                        return true;
                    }
                }

                int bytesLen = p.GetHeader();
                remainCnt = bytesLen;

                while (remainCnt > 0)
                {
                    int recvCnt = 0;
                    recvCnt = Sock.Sock.Receive(p.data.bytes
                        , bytesLen - remainCnt, remainCnt, SocketFlags.None);
                    remainCnt -= recvCnt;
                    logger.WriteDebug($"{recvCnt} bytes recved");
                    if (recvCnt == 0)
                    {
                        //해당 소켓 연결 해제된것.
                        pkg = new Package(this, null);
                        logger.WriteDebug($"session[{SessionId}] is disconnected");
                        return true;
                    }
                }
                return true;
            }
            catch (SocketException)
            {
                //시간 초과 시
                pkg = null;
                return true;
            }
            catch (ObjectDisposedException e)
            {   //해당 소켓이 닫힌 경우.
                logger.WriteDebugWarn(e.Message.ToString());
                return false;
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                return false;
            }
        }

        //packet recv 후 사용할 callback을 지정받자.
        public void OnSendAsync(Packet _p, Action<Packet> _callback = null)
        {
            var sock = Sock.Sock;
            if (sock.Connected == false)
            {
                logger.Error($"socket is closed");
                return;
            }
            var acb = new AsyncCallback((_ar) => { SendAsyncHeader(_ar, _callback); });
            var ret = sock.BeginSend(_p.header.bytes, 0, Packet.GetHeaderSize()
                , SocketFlags.None, acb, _p);
        }

        private void SendAsyncHeader(IAsyncResult _ar, Action<Packet> _callback)
        {
            try
            {
                Packet p = (Packet)_ar.AsyncState;
                var sock = Sock.Sock;
                if (sock.Connected == false)
                {
                    logger.Error($"socket is closed");
                    return;
                }
                int sendCnt = sock.EndSend(_ar);
                var acb = new AsyncCallback((x) => { SendAsyncData(x, _callback); });
                var ret = sock.BeginSend(p.data.bytes, 0, p.GetHeader()
                    , SocketFlags.None, acb, p);
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                _callback?.Invoke(null);
            }
        }

        private void SendAsyncData(IAsyncResult _ar, Action<Packet> _callback)
        {
            try
            {
                var p = (Packet)_ar.AsyncState;
                var sock = Sock.Sock;
                int sendCnt = sock.EndSend(_ar);
                if (sock.Connected == false)
                {
                    logger.Error($"socket is closed");
                    return;
                }
                _callback?.Invoke(p);
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                _callback?.Invoke(null);
            }
        }

        public void OnRecvAsync(Socket _s, Action<Packet> _callback = null)
        {
            try
            {
                if (_s.Connected == false)
                {
                    logger.Error($"socket is closed");
                    return;
                }
                byte[] header = new byte[sizeof(Int16)];
                int remainCnt = header.Length;
                var acb = new AsyncCallback((x) => { RecvAsyncHeader(x, _callback); });
                var ret = _s.BeginReceive(header, 0, remainCnt, SocketFlags.None, acb, header);
            }
            catch (Exception)
            {
                _callback?.Invoke(null);
            }
        }

        private void RecvAsyncHeader(IAsyncResult _ar, Action<Packet> _callback)
        {
            try
            {
                byte[] header = (byte[])_ar.AsyncState;
                var sock = Sock.Sock;
                if (sock.Connected == false)
                {
                    logger.Error($"socket is closed");
                    return;
                }
                int recvCnt = sock.EndReceive(_ar);
                if (recvCnt > 0)
                {
                    int headerVal = BitConverter.ToInt32(header, 0);
                    Packet p = new Packet(headerVal);
                    //todo: header type 수정할 것.
                    p.SetHeader(headerVal);
                    var acb = new AsyncCallback((x) => { RecvAsyncData(x, _callback); });
                    var ret = sock.BeginReceive(p.data.bytes, 0, p.GetHeader(), SocketFlags.None, acb, p);
                }
                else
                {
                    //소켓 연결 종료 시 packet을 null로 지정한다.
                    _callback?.Invoke(null);
                }
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                _callback?.Invoke(null);
            }
        }

        private void RecvAsyncData(IAsyncResult _ar, Action<Packet> _callback)
        {
            try
            {
                Packet p = (Packet)_ar.AsyncState;
                var sock = Sock.Sock;
                if (sock.Connected == false)
                {
                    logger.Error($"socket is closed");
                    return;
                }
                int recvCnt = sock.EndReceive(_ar);
                if (recvCnt > 0)
                {
                    _callback?.Invoke(p);
                    OnRecvAsync(sock, _callback);
                }
                else
                {
                    //소켓 연결 종료 시 packet을 null로 지정한다.
                    _callback?.Invoke(null);
                }
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                _callback?.Invoke(null);
            }
        }

        public async Task<Packet> OnRecvTAP()
        {
            try
            {
                var packet = await RecvPacketTAP();
                return packet;

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
            }
            return null;
        }

        private async Task<Packet> RecvPacketTAP()
        {
            try
            {
                byte[] header = new byte[Packet.GetHeaderSize()];
                var s = Sock.Sock;
                int recvCnt = 0;
                int remainCnt = header.Length;

                while (remainCnt > 0)
                {
                    recvCnt = await Task.Factory.FromAsync(s
                        .BeginReceive(header, header.Length - remainCnt, remainCnt, SocketFlags.None, null, s), s.EndReceive);
                    if (recvCnt <= 0)
                        return null;
                    remainCnt -= recvCnt;
                }
                var hStream = new NetStream(header);
                var headerVal = BitConverter.ToInt32(hStream.bytes, 0);
                if (headerVal != 0 && IsXorAble)
                {
                    hStream.DoXorCrypt(xorKeyBytes);
                    headerVal = BitConverter.ToInt32(hStream.bytes, 0);
                }


                if (headerVal == 0)
                {
                    var hbPacket = new HeartbeatNoti();
                    return hbPacket;
                }
                else
                {
                    var data = new byte[headerVal];
                    recvCnt = 0;
                    remainCnt = headerVal;
                    while (remainCnt > 0)
                    {
                        recvCnt = await Task.Factory.FromAsync(s
                        .BeginReceive(data, headerVal - remainCnt, remainCnt, SocketFlags.None, null, s), s.EndReceive);
                        if (recvCnt <= 0)
                            return null;
                        remainCnt -= recvCnt;
                    }

                    var dStream = new NetStream(data);
                    if (IsDhAble)
                    {
                        dStream.DoDhDeCrypt(dh_key, dh_iv);
                    }
                    Packet newPacket = new Packet(hStream, dStream);
                    //newPacket.UpdateHeader();
                    return newPacket;
                }

            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                return null;
            }
        }

        public async Task<bool> OnSendTAP(Packet _p)
        {
            try
            {
                return await SendPacket(_p);
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
            }
            return false;
        }

        private async Task<bool> SendPacket(Packet _p)
        {
            var s = Sock.Sock;
            {
//                int dataLen = _p.data.offset;
                //hb패킷이 아닌경우 
                if (_p.GetHeader() != 0 && IsDhAble)
                {
                    if (_p.header.IsXored == false)
                    {
                        _p.data.DoDhEncrypt(dh_key, dh_iv);
                        _p.UpdateHeader();
                    }
                }

                int headerRemainCnt = Packet.GetHeaderSize();
                int headerVal = _p.GetHeader();
                try
                {
                    byte[] headerBytes = _p.header.bytes;
                    int sendCnt = 0;
                    //if(dataLen == 0)
                    if (headerVal == 0)
                    {
                        while (headerRemainCnt > 0)
                        {
                            sendCnt = await Task<int>.Factory.FromAsync(s
                            .BeginSend(headerBytes, Packet.GetHeaderSize() - headerRemainCnt, headerRemainCnt, SocketFlags.None, null, s), s.EndSend);
                            if (sendCnt <= 0)
                                return false;
                            headerRemainCnt -= sendCnt;
                        }
                        return true;
                    }
                    else
                    {
                        if (IsXorAble)
                            _p.header.DoXorCrypt(xorKeyBytes);
                        //일반 packet은 stream을 한번에 통째로 전송한다.
                        byte[] data = new byte[Packet.GetHeaderSize() + _p.data.offset];
                        Array.Copy(_p.header.bytes, data, Packet.GetHeaderSize());
                        Array.Copy(_p.data.bytes, 0, data, Packet.GetHeaderSize(), _p.data.offset);
                        int remainCnt = data.Length;
                        while (remainCnt > 0)
                        {
                            sendCnt = await Task<int>.Factory
                                .FromAsync(s.BeginSend(data, data.Length - remainCnt, remainCnt, SocketFlags.None, null, s), s.EndSend);
                            if (sendCnt <= 0)
                                return false;
                            remainCnt -= sendCnt;
                        }
                        return true;
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e.ToString());
                    return false;
                }
            }
        }

        [Conditional("DEBUG")]
        private void RenderBytes(string _tag, byte[] _bytes, int _offset = -1)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"[{_tag}]");
            sb.Append("|");
            if (_offset == -1)
                _offset = _bytes.Length;
            int idx = 0;
            foreach (var b in _bytes)
            {
                sb.Append($"{b}|");
                idx++;
                if (idx >= _offset)
                    break;
            }
            Console.WriteLine(sb.ToString());
        }
    }
}
