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
using static CoreNet.Protocols.Packet;

namespace TestClient
{
    public class Client : CoreNetwork
    {
        public static Client Inst { get;  } = new Client();

        private CoreSession mSession;
        private Dictionary<string, Worker> wDict = new Dictionary<string, Worker>();

        private void Logging(string _msg)
        {
            Console.WriteLine(_msg);
        }

        public override void ReadyToStart()
        {
            wDict["hb"] = new Worker();
            wDict["hb"].PushJob(new JobInfinity(async () => {
                if (isDown)
                    return;
                var p = new HeartbeatNoti();
                p.UpdateHeader();
                Logging("send HB");
                await mSession.OnSendTAP(p);
                await Task.Delay(CoreSession.hbDelayMilliSec);
            }));
            //wDict["pkg"] = new Worker();
            wDict["test"] = new Worker();
            wDict["test"].PushJob(new JobInfinity(async () =>
            {
                if (isDown)
                    return;
                var p = new Packet();
                Translate.Write(p.data, PACKET_TYPE.REQ);
                p.UpdateHeader();
                Logging("send packet");
                await mSession.OnSendTAP(p);
                await Task.Delay(1000);
            }));
        }

        public override void Start()
        {
            Socket s = new Socket(SocketType.Stream, ProtocolType.Tcp);
            s.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 30000));
            mSession = new CoreSession(-1, new CoreTCP(s));

            foreach (var ele in wDict)
            {
                Logging($"{ele.Key} is started");
                ele.Value.WorkStart();
            }
        }

        protected override void Analizer_Ans(CoreSession _s, Packet _p)
        {
        }

        protected override void Analizer_Noti(CoreSession _s, Packet _p)
        {
        }

        protected override void Analizer_Req(CoreSession _s, Packet _p)
        {
        }

        protected override void Analizer_Test(CoreSession _s, Packet _p)
        {
        }
    }
}
