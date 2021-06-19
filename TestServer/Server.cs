using CoreNet.Jobs;
using CoreNet.Networking;
using CoreNet.Protocols;
using CoreNet.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TestServer
{
    public class Server : CoreNetwork
    {
        public static Server Inst => new Server();

        private Dictionary<string, Worker> wDict = new Dictionary<string, Worker>();

        private List<CoreSession> expiredList = new List<CoreSession>();

        private CoreSession mListener;

        private Server()
            : base( _name :"TEST", _port:30000)
        {
            CoreTCP tcp = new CoreTCP(new Socket(SocketType.Stream, ProtocolType.Tcp));
        }

        private void Logging(string _msg)
        {
            Console.WriteLine(_msg);
        }

        private void CheckSessionHB()
        {
            expiredList.Clear();
            foreach (var ele in SessionMgr.Inst.ToSessonList())
            {
                if (ele.IsExpireHeartBeat())
                    expiredList.Add(ele);
            }

            foreach (var ele in expiredList)
            {
                var del = default(CoreSession);
                SessionMgr.Inst.CloseSession(ele.SessionId, out del);
                if (del != default(CoreSession))
                    Logging($"{del.SessionId} is expired");
            }

        }
        public override void ReadyToStart()
        {
            wDict["pkg"] = new Worker("pkg");
            wDict["pkg"].PushJob(new JobInfinity(() => {
                if (this.isDown)
                {
                    return;
                }
                packageQ.Swap();
                while (true)
                {
                    var pkg = packageQ.pop();
                    if (pkg == default(Package))
                        break;
                    if (pkg.Packet.GetHeader() == 0)
                    {
                        pkg.session.UpdateHeartBeat();
                    }
                    else
                        PackageDispatcher(pkg);
                }
                CheckSessionHB();
            }));

            mListener.Sock.Sock.Bind(new IPEndPoint(IPAddress.Any, port));
            mListener.Sock.Sock.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
            mListener.Sock.Sock.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.ReuseAddress, true);
            mListener.Sock.Sock.Listen(100);
        }

        public override void Start()
        {
            Task.Factory.StartNew(async () => {
                Logging("Accept is start");
                while (isDown == false)
                {
                    Socket s = mListener.Sock.Sock.Accept();
                    var newSId = SessionMgr.Inst.GetNextSessionId();
                    var newClient = new CoreSession(newSId, new CoreTCP(s));
                    SessionMgr.Inst.AddSession(newClient);

                    Task.Factory.StartNew(async () => {
                        var client = default(CoreSession);
                        SessionMgr.Inst.GetSession(newSId, out client);
                        if (client != default(CoreSession))
                        {
                            while (isDown == false)
                            {
                                if (client.Sock.Sock.Connected == false)
                                    break;
                                var p = await client.OnRecvTAP();
                                if (p.GetHeader() == 0)
                                    client.UpdateHeartBeat();
                                else
                                    packageQ.Push(new Package(client, p));
                            }
                        }
                    });
                }
            });

            foreach (var pair in wDict)
            {
                Logging($"{pair.Key} worker is started");
                pair.Value.WorkStart();
            }
        }

        protected override void Analizer_Ans(CoreSession _s, Packet _p)
        {
            Logging($"{_p.pType} recved from {_s.SessionId}");
        }

        protected override void Analizer_Noti(CoreSession _s, Packet _p)
        {
            Logging($"{_p.pType} recved from {_s.SessionId}");
        }

        protected override void Analizer_Req(CoreSession _s, Packet _p)
        {
            Logging($"{_p.pType} recved from {_s.SessionId}");
        }

        protected override void Analizer_Test(CoreSession _s, Packet _p)
        {
            Logging($"{_p.pType} recved from {_s.SessionId}");
        }
    }
}
